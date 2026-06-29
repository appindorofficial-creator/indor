namespace IndorMvcApp.Services;

/// <summary>
/// Maps AI-suggested Font Awesome tokens to icons that exist in FA 6.4 Free.
/// </summary>
public static class PropertyMaintenanceIconResolver
{
    /// <summary>Icons verified in Font Awesome 6.4.0 free metadata.</summary>
    private static readonly HashSet<string> FreeIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        "fan", "faucet-drip", "bolt", "house-chimney", "paint-roller", "shield-halved", "leaf",
        "screwdriver-wrench", "droplet", "fire-flame-simple", "fire", "fire-extinguisher", "bell",
        "grip-lines", "water", "filter", "broom", "bug", "tree", "seedling",
        "window-maximize", "plug", "lightbulb", "wrench", "hammer", "cloud-bolt", "snowflake",
        "sun", "wind", "circle-check", "triangle-exclamation", "house", "building", "warehouse",
        "stairs", "sink", "shower", "toilet", "smog", "lungs", "eye", "clipboard-check",
        "house-crack", "person-shelter", "helmet-safety", "mask-face", "bolt-lightning",
        "temperature-arrow-up", "temperature-half", "house-fire", "fire-burner", "file-circle-plus"
    };

    private static readonly Dictionary<string, string> IconAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["thermometer-half"] = "temperature-half",
        ["thermometer"] = "temperature-half",
        ["air-conditioner"] = "fan",
        ["airconditioner"] = "fan",
        ["hvac"] = "fan",
        ["extinguisher"] = "fire-extinguisher",
        ["smoke-detector"] = "bell",
        ["smoke-alarm"] = "bell",
        ["shield"] = "shield-halved",
        ["house-damage"] = "house-crack",
        ["exclamation-triangle"] = "triangle-exclamation",
        ["alert-triangle"] = "triangle-exclamation",
        ["tools"] = "screwdriver-wrench",
        ["toolbox"] = "screwdriver-wrench",
        ["home"] = "house"
    };

    public static string Resolve(string? icon, string? category = null, string? title = null)
    {
        var token = ApplyAlias(NormalizeToken(icon));
        if (IsUsableFreeIcon(token))
        {
            return $"fa-{token}";
        }

        var fromTitle = ResolveFromTitle(title);
        if (fromTitle != null) return fromTitle;

        return ResolveFromCategory(category);
    }

    /// <summary>Full Font Awesome class list for views, e.g. "fas fa-fan".</summary>
    public static string ToCssClass(string? icon, string? category = null, string? title = null)
    {
        var token = NormalizeToken(Resolve(icon, category, title));
        if (string.IsNullOrEmpty(token))
        {
            token = NormalizeToken(ResolveFromCategory(category)) ?? "screwdriver-wrench";
        }

        return $"fas fa-{token}";
    }

    /// <summary>Always re-resolves to a valid FA 6 Free icon class list.</summary>
    public static string EnsureCssClass(string? icon, string? category = null, string? title = null) =>
        ToCssClass(icon, category, title);

    private static string? ApplyAlias(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        return IconAliases.TryGetValue(token, out var alias) ? alias : token;
    }

    private static bool IsUsableFreeIcon(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        if (token.Length == 1 && token[0] is >= 'a' and <= 'z') return false;
        return FreeIcons.Contains(token);
    }

    private static string? NormalizeToken(string? icon)
    {
        if (string.IsNullOrWhiteSpace(icon)) return null;

        var value = icon.Trim().ToLowerInvariant();
        value = value.Replace("fas ", "", StringComparison.Ordinal)
            .Replace("far ", "", StringComparison.Ordinal)
            .Replace("fab ", "", StringComparison.Ordinal)
            .Replace("fa-solid ", "", StringComparison.Ordinal)
            .Replace("fa ", "", StringComparison.Ordinal);

        while (value.StartsWith("fa-", StringComparison.Ordinal))
        {
            value = value[3..];
        }

        value = value.Trim('-', ' ');
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? ResolveFromTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;

        var t = title.ToLowerInvariant();
        if (t.Contains("earthquake", StringComparison.Ordinal) || t.Contains("seismic", StringComparison.Ordinal))
            return "fa-house-crack";
        if (t.Contains("fire safety", StringComparison.Ordinal) || t.Contains("fire check", StringComparison.Ordinal)
            || t.Contains("fire extinguisher", StringComparison.Ordinal)
            || (t.Contains("fire", StringComparison.Ordinal) && t.Contains("safety", StringComparison.Ordinal)))
            return "fa-fire-extinguisher";
        if (t.Contains("carbon monoxide", StringComparison.Ordinal) || t.Contains("co alarm", StringComparison.Ordinal))
            return "fa-bell";
        if (t.Contains("smoke", StringComparison.Ordinal) || t.Contains("detector", StringComparison.Ordinal))
            return "fa-bell";
        if (t.Contains("water heater", StringComparison.Ordinal)) return "fa-fire-flame-simple";
        if (t.Contains("gutter", StringComparison.Ordinal)) return "fa-grip-lines";
        if (t.Contains("hvac", StringComparison.Ordinal) || t.Contains("furnace", StringComparison.Ordinal)
            || t.Contains("air condition", StringComparison.Ordinal) || t.Contains("a/c", StringComparison.Ordinal)
            || (t.Contains("system check", StringComparison.Ordinal) && t.Contains("heat", StringComparison.Ordinal)))
            return "fa-fan";
        if (t.Contains("roof", StringComparison.Ordinal)) return "fa-house-chimney";
        if (t.Contains("paint", StringComparison.Ordinal) || t.Contains("exterior", StringComparison.Ordinal))
            return "fa-paint-roller";
        if (t.Contains("lawn", StringComparison.Ordinal) || t.Contains("landscap", StringComparison.Ordinal)
            || t.Contains("yard", StringComparison.Ordinal))
            return "fa-seedling";
        if (t.Contains("pest", StringComparison.Ordinal) || t.Contains("termite", StringComparison.Ordinal))
            return "fa-bug";
        if (t.Contains("plumb", StringComparison.Ordinal) || t.Contains("drain", StringComparison.Ordinal)
            || t.Contains("pipe", StringComparison.Ordinal))
            return "fa-faucet-drip";
        if (t.Contains("electr", StringComparison.Ordinal) || t.Contains("outlet", StringComparison.Ordinal))
            return "fa-bolt";
        if (t.Contains("filter", StringComparison.Ordinal) || t.Contains("duct", StringComparison.Ordinal))
            return "fa-filter";
        if (t.Contains("chimney", StringComparison.Ordinal) || t.Contains("fireplace", StringComparison.Ordinal))
            return "fa-fire";
        if (t.Contains("window", StringComparison.Ordinal)) return "fa-window-maximize";
        if (t.Contains("clean", StringComparison.Ordinal)) return "fa-broom";
        if (t.Contains("foundation", StringComparison.Ordinal)) return "fa-house";
        if (t.Contains("inspection", StringComparison.Ordinal) || t.Contains("inspection report", StringComparison.Ordinal))
            return "fa-file-circle-plus";
        return null;
    }

    private static string ResolveFromCategory(string? category) =>
        (category ?? "General").Trim().ToLowerInvariant() switch
        {
            var c when c.Contains("hvac", StringComparison.Ordinal) => "fa-fan",
            var c when c.Contains("plumb", StringComparison.Ordinal) => "fa-faucet-drip",
            var c when c.Contains("electr", StringComparison.Ordinal) => "fa-bolt",
            var c when c.Contains("roof", StringComparison.Ordinal) => "fa-house-chimney",
            var c when c.Contains("exterior", StringComparison.Ordinal) => "fa-paint-roller",
            var c when c.Contains("safe", StringComparison.Ordinal) => "fa-shield-halved",
            var c when c.Contains("land", StringComparison.Ordinal) => "fa-seedling",
            _ => "fa-screwdriver-wrench"
        };
}
