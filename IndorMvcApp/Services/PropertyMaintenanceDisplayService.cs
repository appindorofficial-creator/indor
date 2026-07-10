using System.Text.Json;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

public static class PropertyMaintenanceDisplayService
{
    private static string Ml(string english) => DisplayLabelsLocalization.L(english);
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
        int? maxItems = null,
        IReadOnlyList<HomeCarePriority>? homeCarePriorities = null,
        IUrlHelper? url = null,
        int? propiedadId = null)
    {
        if (!IsRealAiPlan(plan))
        {
            var unavailableMessage = plan?.Summary;
            if (string.IsNullOrWhiteSpace(unavailableMessage))
            {
                unavailableMessage = plan == null
                    ? Ml("Maintenance recommendations will appear after OpenAI analyzes your property.")
                    : Ml("We couldn't generate AI maintenance suggestions for this address yet.");
            }

            return new PropertyMaintenanceSectionViewModel
            {
                Title = Ml("AI Maintenance Plan"),
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
            Title = Ml("AI Maintenance Plan"),
            Subtitle = Ml("Personalized upkeep recommendations from OpenAI based on your home profile."),
            Summary = plan.Summary,
            DataSource = plan.DataSource,
            IsAiGenerated = true,
            IsUnavailable = false,
            Items = items.Select(i => EnrichItem(i, homeCarePriorities, url, propiedadId)).ToList(),
            UrgentCount = plan.Items.Count(i => i.Priority == "Urgent"),
            HighCount = plan.Items.Count(i => i.Priority == "High"),
            ShowAlerts = plan.Items.Any(i => i.Priority is "Urgent" or "High")
        };
    }

    public static List<HomeTodayTaskViewModel> BuildAlertTasks(
        PropertyMaintenancePlanViewModel? plan,
        int propiedadId,
        IUrlHelper url,
        IReadOnlyList<HomeCarePriority>? homeCarePriorities = null)
    {
        if (!IsRealAiPlan(plan)) return [];

        return plan!.Items
            .Where(i => i.Priority is "Urgent" or "High")
            .OrderBy(i => PriorityWeight(i.Priority))
            .ThenBy(i => i.SortOrder)
            .Take(3)
            .Select(i =>
            {
                var taskUrl = $"{url.Action("Index", "Home")}#hd-maintenance-alerts";
                if (homeCarePriorities is { Count: > 0 })
                {
                    var (scheduleUrl, _) = PropertyMaintenanceBookingLinkResolver.Resolve(
                        i.Title,
                        i.Category,
                        homeCarePriorities,
                        url,
                        propiedadId);
                    if (!string.IsNullOrWhiteSpace(scheduleUrl))
                    {
                        taskUrl = scheduleUrl;
                    }
                }

                return new HomeTodayTaskViewModel
                {
                    Icon = PropertyMaintenanceIconResolver.ToCssClass(i.Icon, i.Category, i.Title),
                    Category = i.Category,
                    Title = i.Title,
                    Subtitle = string.IsNullOrWhiteSpace(i.Frequency) ? i.Description : $"{i.Frequency} · {i.Category}",
                    Badge = i.Priority == "Urgent" ? Ml("AI · Urgent") : Ml("AI · Recommended"),
                    Url = taskUrl
                };
            })
            .ToList();
    }

    private static int PriorityWeight(string priority) => priority switch
    {
        "Urgent" => 0,
        "High" => 1,
        _ => 2
    };

    private static PropertyMaintenanceItemViewModel EnrichItem(
        PropertyMaintenanceItemViewModel item,
        IReadOnlyList<HomeCarePriority>? homeCarePriorities,
        IUrlHelper? url,
        int? propiedadId)
    {
        var enriched = NormalizeItemIcon(item);
        if (homeCarePriorities is { Count: > 0 } && url != null)
        {
            var (scheduleUrl, actionLabel) = PropertyMaintenanceBookingLinkResolver.Resolve(
                item.Title,
                item.Category,
                homeCarePriorities,
                url,
                propiedadId);
            enriched.ScheduleUrl = scheduleUrl;
            enriched.ScheduleActionLabel = actionLabel;
        }

        return enriched;
    }

    private static PropertyMaintenanceItemViewModel NormalizeItemIcon(PropertyMaintenanceItemViewModel item) =>
        new()
        {
            Title = item.Title,
            Description = item.Description,
            Category = item.Category,
            Priority = item.Priority,
            Frequency = item.Frequency,
            Icon = PropertyMaintenanceIconResolver.ToCssClass(item.Icon, item.Category, item.Title),
            Reason = item.Reason,
            SortOrder = item.SortOrder
        };
}
