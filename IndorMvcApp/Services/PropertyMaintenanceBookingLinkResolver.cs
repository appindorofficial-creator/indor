using IndorMvcApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

/// <summary>
/// Maps AI maintenance recommendations to existing Home Care booking flows.
/// </summary>
public static class PropertyMaintenanceBookingLinkResolver
{
    private static readonly (string[] Keywords, string PriorityName)[] TitleMappings =
    [
        (["smoke", "co detector", "carbon monoxide", "detector", "alarm"], "Smoke Detector"),
        (["hvac", "furnace", "air condition", "a/c", "heat pump", "cooling system", "heating system"], "HVAC maintenance"),
        (["roof", "shingle", "asphalt roof"], "Roof inspection"),
        (["crawl space", "crawlspace"], "Crawlspace check"),
        (["water heater", "hot water"], "Water heater flush"),
        (["gutter"], "Gutter cleaning"),
        (["pest", "termite", "rodent", "insect"], "Pest control"),
        (["power wash", "pressure wash"], "Power wash exterior"),
        (["exterior paint", "paint exterior"], "Exterior paint"),
    ];

    public static (string? Url, string ActionLabel) Resolve(
        string title,
        string category,
        IReadOnlyList<HomeCarePriority> priorities,
        IUrlHelper url,
        int? propiedadId)
    {
        var priority = FindPriority(title, category, priorities);
        if (priority != null)
        {
            var bookingUrl = BuildPriorityUrl(priority, url, propiedadId);
            if (!string.IsNullOrWhiteSpace(bookingUrl))
            {
                return (bookingUrl, "Schedule now");
            }
        }

        var servicesUrl = $"{url.Action("Index", "Home")}#home-care-priorities";
        return (servicesUrl, "View services");
    }

    private static HomeCarePriority? FindPriority(
        string title,
        string category,
        IReadOnlyList<HomeCarePriority> priorities)
    {
        if (priorities.Count == 0)
        {
            return null;
        }

        var priorityName = ResolvePriorityName(title, category);
        if (string.IsNullOrWhiteSpace(priorityName))
        {
            return null;
        }

        return priorities.FirstOrDefault(p =>
            string.Equals(p.Nombre, priorityName, StringComparison.OrdinalIgnoreCase))
            ?? priorities.FirstOrDefault(p =>
                NormalizeName(p.Nombre).Contains(NormalizeName(priorityName), StringComparison.OrdinalIgnoreCase)
                || NormalizeName(priorityName).Contains(NormalizeName(p.Nombre), StringComparison.OrdinalIgnoreCase));
    }

    private static string? ResolvePriorityName(string title, string category)
    {
        var combined = $"{title} {category}".ToLowerInvariant();

        foreach (var (keywords, priorityName) in TitleMappings)
        {
            if (keywords.Any(k => combined.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                return priorityName;
            }
        }

        return category.Trim().ToLowerInvariant() switch
        {
            var c when c.Contains("hvac", StringComparison.Ordinal) => "HVAC maintenance",
            var c when c.Contains("roof", StringComparison.Ordinal) => "Roof inspection",
            var c when c.Contains("safe", StringComparison.Ordinal) => "Smoke Detector",
            var c when c.Contains("plumb", StringComparison.Ordinal) => "Water heater flush",
            var c when c.Contains("exterior", StringComparison.Ordinal) => "Power wash exterior",
            var c when c.Contains("land", StringComparison.Ordinal) => "Gutter cleaning",
            var c when c.Contains("electr", StringComparison.Ordinal) => "Smoke Detector",
            _ => null
        };
    }

    private static string? BuildPriorityUrl(HomeCarePriority priority, IUrlHelper url, int? propiedadId)
    {
        if (!string.IsNullOrWhiteSpace(priority.LinkUrl))
        {
            return priority.LinkUrl;
        }

        if (!string.IsNullOrWhiteSpace(priority.LinkController)
            && !string.Equals(priority.LinkController, "MyHome", StringComparison.OrdinalIgnoreCase))
        {
            return url.Action(priority.LinkAction ?? "Index", priority.LinkController, new { id = priority.Id });
        }

        if (propiedadId.HasValue)
        {
            return url.Action(
                priority.LinkAction ?? "Maintenance",
                priority.LinkController ?? "MyHome",
                new { id = propiedadId.Value });
        }

        return null;
    }

    private static string NormalizeName(string value) =>
        new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
