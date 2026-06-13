using System.Text.Json;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

public static class PropertyMaintenanceDisplayService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static PropertyMaintenancePlanViewModel? ParseFromPropiedad(Propiedad propiedad)
    {
        if (!string.IsNullOrWhiteSpace(propiedad.MantenimientoRecomendadoJson))
        {
            return ParseJson(propiedad.MantenimientoRecomendadoJson, propiedad.MantenimientoRecomendadoUtc);
        }

        if (string.IsNullOrWhiteSpace(propiedad.DatosJson)) return null;

        try
        {
            var info = JsonSerializer.Deserialize<PropertyInfoViewModel>(propiedad.DatosJson, JsonOptions);
            return info?.MaintenanceRecommendations;
        }
        catch
        {
            return null;
        }
    }

    public static PropertyMaintenancePlanViewModel? ParseJson(string? json, DateTime? generatedUtc = null)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var plan = JsonSerializer.Deserialize<PropertyMaintenancePlanViewModel>(json, JsonOptions);
            if (plan != null && !plan.GeneratedUtc.HasValue && generatedUtc.HasValue)
            {
                plan.GeneratedUtc = generatedUtc;
            }
            return plan;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsRealAiPlan(PropertyMaintenancePlanViewModel? plan) =>
        plan is { Items.Count: > 0 }
        && (plan.IsAiGenerated
            || string.Equals(plan.DataSource, "OpenAI", StringComparison.OrdinalIgnoreCase));

    public static PropertyMaintenanceSectionViewModel BuildSection(
        PropertyMaintenancePlanViewModel? plan,
        bool compact = false,
        int? maxItems = null)
    {
        if (!IsRealAiPlan(plan))
        {
            var unavailableMessage = plan?.Summary;
            if (string.IsNullOrWhiteSpace(unavailableMessage))
            {
                unavailableMessage = plan == null
                    ? "Maintenance recommendations will appear after OpenAI analyzes your property."
                    : "We couldn't generate AI maintenance suggestions for this address yet.";
            }

            return new PropertyMaintenanceSectionViewModel
            {
                Title = "AI Maintenance Plan",
                Subtitle = unavailableMessage,
                Summary = plan?.Summary,
                DataSource = plan?.DataSource,
                IsAiGenerated = false,
                IsUnavailable = plan != null,
                Items = [],
                ShowAlerts = false
            };
        }

        var items = plan!.Items.OrderBy(i => PriorityWeight(i.Priority)).ThenBy(i => i.SortOrder).ToList();
        if (compact)
        {
            items = items.Take(4).ToList();
        }
        else if (maxItems is > 0)
        {
            items = items.Take(maxItems.Value).ToList();
        }

        return new PropertyMaintenanceSectionViewModel
        {
            Title = "AI Maintenance Plan",
            Subtitle = "Personalized upkeep recommendations from OpenAI based on your home profile.",
            Summary = plan.Summary,
            DataSource = plan.DataSource,
            IsAiGenerated = true,
            IsUnavailable = false,
            Items = items,
            UrgentCount = plan.Items.Count(i => i.Priority == "Urgent"),
            HighCount = plan.Items.Count(i => i.Priority == "High"),
            ShowAlerts = plan.Items.Any(i => i.Priority is "Urgent" or "High")
        };
    }

    public static List<HomeTodayTaskViewModel> BuildAlertTasks(
        PropertyMaintenancePlanViewModel? plan,
        int propiedadId,
        IUrlHelper url)
    {
        if (!IsRealAiPlan(plan)) return [];

        return plan!.Items
            .Where(i => i.Priority is "Urgent" or "High")
            .OrderBy(i => PriorityWeight(i.Priority))
            .ThenBy(i => i.SortOrder)
            .Take(3)
            .Select(i => new HomeTodayTaskViewModel
            {
                Icon = i.Icon.StartsWith("fa-") ? i.Icon : $"fa-{i.Icon}",
                Title = i.Title,
                Subtitle = string.IsNullOrWhiteSpace(i.Frequency) ? i.Description : $"{i.Frequency} · {i.Category}",
                Badge = i.Priority == "Urgent" ? "AI · Urgent" : "AI · Recommended",
                Url = $"{url.Action("Index", "Home")}#hd-maintenance-alerts"
            })
            .ToList();
    }

    private static int PriorityWeight(string priority) => priority switch
    {
        "Urgent" => 0,
        "High" => 1,
        _ => 2
    };
}
