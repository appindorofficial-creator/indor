using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace IndorMvcApp.Helpers;

/// <summary>
/// Builds URLs for switching the active portfolio property during PA service flows.
/// </summary>
public static class PropertyAdministratorPropertySwitch
{
    private static readonly string[] RestartAtPreventiveServicesPaths =
    [
        "/Administrador/PreventiveMaintenanceSchedule",
        "/Administrador/PreventiveMaintenanceReview",
        "/Administrador/PreventiveMaintenanceConfirmed"
    ];

    public static string BuildSwitchListUrl(IUrlHelper url, PathString path, QueryString query)
    {
        var returnUrl = path + query;
        return url.Action(
            "Properties",
            "Administrador",
            new { from = "switch", returnUrl }) ?? "#";
    }

    public static string ResolveSelectionUrl(IUrlHelper url, int propertyId, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !url.IsLocalUrl(returnUrl))
        {
            return url.Action("Index", "Administrador", new { propertyId }) ?? "#";
        }

        var pathAndQuery = returnUrl.Trim();
        if (!pathAndQuery.StartsWith('/'))
        {
            pathAndQuery = "/" + pathAndQuery;
        }

        var qIndex = pathAndQuery.IndexOf('?', StringComparison.Ordinal);
        var path = qIndex >= 0 ? pathAndQuery[..qIndex] : pathAndQuery;

        if (RestartAtPreventiveServicesPaths.Any(p =>
                path.Equals(p, StringComparison.OrdinalIgnoreCase)))
        {
            return url.Action("PreventiveMaintenanceServices", "Administrador", new { propertyId }) ?? "#";
        }

        var query = qIndex >= 0
            ? QueryHelpers.ParseQuery(pathAndQuery[(qIndex + 1)..])
            : new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();

        var pairs = new List<KeyValuePair<string, string?>>();
        foreach (var kv in query)
        {
            if (kv.Key.Equals("propertyId", StringComparison.OrdinalIgnoreCase)
                || kv.Key.Equals("planId", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            pairs.Add(new KeyValuePair<string, string?>(kv.Key, kv.Value.FirstOrDefault()));
        }

        pairs.Add(new KeyValuePair<string, string?>("propertyId", propertyId.ToString()));
        var qs = QueryString.Create(pairs);
        return path + qs.ToUriComponent();
    }
}
