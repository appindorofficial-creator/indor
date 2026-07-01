using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Helpers;

public static class HomeNavigationUrls
{
    public const string ServicesTabSection = "section-services";
    public const string HouseFactsTabSection = "section-myhome";
    public const string MoreTabSection = "section-more";

    private static readonly Regex FragmentQueryRegex = new(
        @"[?&]fragment=([^&#]+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string ServicesTab(IUrlHelper url, string? scrollTargetId = null)
    {
        var path = url.Action("Index", "Home") ?? "/";
        var hash = string.IsNullOrWhiteSpace(scrollTargetId) ? ServicesTabSection : scrollTargetId;
        return $"{path.TrimEnd('/')}#{hash}";
    }

    public static string HouseFactsTab(IUrlHelper url) =>
        $"{(url.Action("Index", "Home") ?? "/").TrimEnd('/')}#{HouseFactsTabSection}";

    public static string MoreTab(IUrlHelper url) =>
        $"{(url.Action("Index", "Home") ?? "/").TrimEnd('/')}#{MoreTabSection}";

    /// <summary>
    /// Resolves the back URL for House Facts / MyHome layout pages.
    /// Views may override via <see cref="ViewBag.MyHomeBackUrl"/>.
    /// </summary>
    public static string ResolveHouseFactBackUrl(
        IUrlHelper url,
        ISession? session,
        string? myHomeBackUrl,
        string? houseFactReturnUrl,
        bool hfPreview,
        int propiedadId)
    {
        if (!string.IsNullOrWhiteSpace(myHomeBackUrl))
        {
            return myHomeBackUrl;
        }

        if (!string.IsNullOrWhiteSpace(houseFactReturnUrl))
        {
            return houseFactReturnUrl;
        }

        var sessionReturn = session != null ? HouseFactPreviewContext.GetReturnUrl(session) : null;
        if (!string.IsNullOrWhiteSpace(sessionReturn))
        {
            return sessionReturn;
        }

        if (hfPreview)
        {
            return url.Action("PropertyDetails", "Propietario") ?? HouseFactsTab(url);
        }

        if (propiedadId > 0)
        {
            return url.Action("Details", "MyHome", new { id = propiedadId, tab = "attom" }) ?? HouseFactsTab(url);
        }

        return HouseFactsTab(url);
    }

    /// <summary>
    /// Resolves back navigation for MyHome sub-pages (History, Documents, Providers, Maintenance).
    /// </summary>
    public static string ResolveMyHomeBackUrl(IUrlHelper url, string? from, int propiedadId)
    {
        if (string.Equals(from, "housefacts", StringComparison.OrdinalIgnoreCase)
            || string.Equals(from, "home", StringComparison.OrdinalIgnoreCase))
        {
            return HouseFactsTab(url);
        }

        return url.Action("Index", "MyHome", new { id = propiedadId }) ?? HouseFactsTab(url);
    }

    public static string? NormalizeMyHomeNavigationFrom(string? from) =>
        string.Equals(from, "home", StringComparison.OrdinalIgnoreCase) ? "home"
        : string.Equals(from, "housefacts", StringComparison.OrdinalIgnoreCase) ? "housefacts"
        : null;

    /// <summary>
    /// Ensures service wizard back links return to the Services tab instead of the default Nearby home.
    /// Also converts mistaken <c>?fragment=</c> query strings from <see cref="UrlHelper.Action"/> into URL hashes.
    /// </summary>
    public static string? NormalizeWizardBackUrl(string? backUrl)
    {
        if (string.IsNullOrWhiteSpace(backUrl))
        {
            return backUrl;
        }

        var trimmed = backUrl.Trim();
        var fragmentFromQuery = TryExtractFragmentQuery(trimmed, out var withoutQueryFragment);
        if (fragmentFromQuery != null)
        {
            return $"{withoutQueryFragment}#{fragmentFromQuery}";
        }

        if (trimmed.Contains('#'))
        {
            return trimmed;
        }

        if (!IsBareHomeIndex(trimmed))
        {
            return trimmed;
        }

        var pathOnly = trimmed.Split('?')[0];
        return $"{pathOnly.TrimEnd('/')}#{ServicesTabSection}";
    }

    private static string? TryExtractFragmentQuery(string url, out string baseUrl)
    {
        var match = FragmentQueryRegex.Match(url);
        if (!match.Success)
        {
            baseUrl = url;
            return null;
        }

        var fragment = Uri.UnescapeDataString(match.Groups[1].Value);
        var query = url[url.IndexOf('?')..];
        var cleanedQuery = FragmentQueryRegex.Replace(query, string.Empty)
            .TrimStart('?')
            .TrimStart('&');

        var path = url.Split('?')[0].TrimEnd('/');
        baseUrl = string.IsNullOrEmpty(cleanedQuery) ? path : $"{path}?{cleanedQuery}";
        return fragment;
    }

    private static bool IsBareHomeIndex(string url)
    {
        var path = url.Split('?')[0].Split('#')[0].TrimEnd('/').ToLowerInvariant();
        return path.Length == 0
            || path.EndsWith("/home", StringComparison.Ordinal)
            || path.EndsWith("/home/index", StringComparison.Ordinal);
    }
}
