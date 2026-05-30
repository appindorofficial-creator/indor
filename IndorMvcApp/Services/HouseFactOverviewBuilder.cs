using System.Text.RegularExpressions;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class HouseFactOverviewBuilder
{
    private sealed record CategoryDef(
        string Key,
        string Title,
        string Subtitle,
        string Icon,
        string Tone,
        string BadgeSuffix,
        bool IsWarning,
        params string[] Keywords);

    private static readonly CategoryDef[] Categories =
    [
        new("schools", "Schools", "Assigned schools, district & ratings", "fa-graduation-cap", "purple", "schools", false,
            "school", "education", "assigned school"),
        new("utilities", "Utilities", "Utility providers", "fa-droplet", "blue", "providers", false,
            "utilit", "provider", "electric", "water", "gas", "sewer", "internet", "cable"),
        new("hoa", "HOA & Community", "Fees, rules, amenities & contacts", "fa-people-roof", "purple", "status", false,
            "hoa", "community", "association"),
        new("risk", "Risk Score", "Overall risk assessment", "fa-shield-halved", "orange", "level", false,
            "risk"),
        new("missing", "Missing Information", "Items needing attention", "fa-circle-exclamation", "red", "items", true,
            "missing", "checklist", "verification", "action flow", "question", "realtor"),
        new("documents", "Documents", "Reports & disclosures", "fa-folder-open", "blue", "documents", false,
            "source", "summary", "final", "document"),
        new("systems", "Systems Profile", "HVAC, plumbing, electrical, appliances & key home systems", "fa-screwdriver-wrench", "blue", "systems", false,
            "systems", "mechanical", "hvac"),
        new("roof", "Roof & Exterior", "Roof, siding, drainage & structure", "fa-house-chimney", "blue", "items", false,
            "roof", "exterior", "site", "structure", "foundation"),
        new("permits", "Permits & Improvements", "Permit history, upgrades & verification", "fa-file-lines", "green", "permit types", false,
            "permit", "improvement"),
        new("snapshot", "Property Snapshot", "Address, lot, structure & key details", "fa-table-cells-large", "blue", "facts", false,
            "snapshot", "identity", "basic", "property identity", "listing", "market", "public record", "tax", "sales", "feature")
    ];

    private static readonly (string Key, string Label, string Icon)[] QuickJumpDefs =
    [
        ("snapshot", "Snapshot", "fa-table-cells-large"),
        ("systems", "Systems", "fa-screwdriver-wrench"),
        ("roof", "Roof & Exterior", "fa-house-chimney"),
        ("permits", "Permits", "fa-file-lines"),
        ("hoa", "HOA & Community", "fa-people-roof"),
        ("more", "More", "fa-ellipsis")
    ];

    public static void Apply(HouseFactProfileViewModel profile)
    {
        if (!profile.HasData || profile.Sections.Count == 0)
        {
            EnsureNearbyPlaceCards(profile);
            return;
        }

        AssignCategories(profile.Sections);

        var location = BuildLocationSummary(profile);
        var yearBuilt = FormatStat(FindFieldValue(profile.Sections, "year built", "yearbuilt", "year built (public)"));
        var livingArea = FormatStat(FindFieldValue(profile.Sections, "sq ft", "living area", "square feet", "sqft", "living sq"));
        var confidence = FormatConfidence(profile.Confidence);

        profile.Overview = new HouseFactOverviewViewModel
        {
            LocationSummary = location,
            YearBuiltDisplay = yearBuilt,
            LivingAreaDisplay = livingArea,
            ConfidenceDisplay = confidence,
            HeroStats =
            [
                new HouseFactHeroStatViewModel { Label = "Location", Value = location ?? "— Not confirmed", Icon = "fa-location-dot" },
                new HouseFactHeroStatViewModel { Label = "Year built", Value = yearBuilt, Icon = "fa-calendar" },
                new HouseFactHeroStatViewModel { Label = "Living area", Value = livingArea, Icon = "fa-ruler-combined" },
                new HouseFactHeroStatViewModel { Label = "Confidence", Value = confidence, Icon = "fa-shield-check" }
            ],
            QuickJumps = QuickJumpDefs.Select(j => new HouseFactQuickJumpViewModel
            {
                Key = j.Key,
                Label = j.Label,
                Icon = j.Icon
            }).ToList(),
            CategoryCards = BuildCategoryCards(profile.Sections)
        };

        AppendNearbyPlaceCards(profile.Overview.CategoryCards, profile);
    }

    public static void EnsureNearbyPlaceCards(HouseFactProfileViewModel profile)
    {
        profile.Overview ??= new HouseFactOverviewViewModel();

        if (profile.Overview.QuickJumps.Count == 0)
        {
            profile.Overview.QuickJumps = QuickJumpDefs.Select(j => new HouseFactQuickJumpViewModel
            {
                Key = j.Key,
                Label = j.Label,
                Icon = j.Icon
            }).ToList();
        }

        if (profile.Overview.HeroStats.Count == 0 && profile.Sections.Count > 0)
        {
            var location = BuildLocationSummary(profile);
            var yearBuilt = FormatStat(FindFieldValue(profile.Sections, "year built", "yearbuilt", "year built (public)"));
            var livingArea = FormatStat(FindFieldValue(profile.Sections, "sq ft", "living area", "square feet", "sqft", "living sq"));
            var confidence = FormatConfidence(profile.Confidence);

            profile.Overview.LocationSummary = location;
            profile.Overview.YearBuiltDisplay = yearBuilt;
            profile.Overview.LivingAreaDisplay = livingArea;
            profile.Overview.ConfidenceDisplay = confidence;
            profile.Overview.HeroStats =
            [
                new HouseFactHeroStatViewModel { Label = "Location", Value = location ?? "— Not confirmed", Icon = "fa-location-dot" },
                new HouseFactHeroStatViewModel { Label = "Year built", Value = yearBuilt, Icon = "fa-calendar" },
                new HouseFactHeroStatViewModel { Label = "Living area", Value = livingArea, Icon = "fa-ruler-combined" },
                new HouseFactHeroStatViewModel { Label = "Confidence", Value = confidence, Icon = "fa-shield-check" }
            ];
        }
        else if (profile.Overview.HeroStats.Count == 0)
        {
            var location = ExtractCityCountyFromAddress(profile.FormattedAddress) ?? "— Not confirmed";
            profile.Overview.HeroStats =
            [
                new HouseFactHeroStatViewModel { Label = "Location", Value = location, Icon = "fa-location-dot" },
                new HouseFactHeroStatViewModel { Label = "Year built", Value = "— Not confirmed", Icon = "fa-calendar" },
                new HouseFactHeroStatViewModel { Label = "Living area", Value = "— Not confirmed", Icon = "fa-ruler-combined" },
                new HouseFactHeroStatViewModel { Label = "Confidence", Value = "Needs verification", Icon = "fa-shield-check" }
            ];
        }

        if (profile.Overview.CategoryCards.Count == 0 && profile.Sections.Count > 0)
        {
            AssignCategories(profile.Sections);
            profile.Overview.CategoryCards = BuildCategoryCards(profile.Sections);
        }

        AppendNearbyPlaceCards(profile.Overview.CategoryCards, profile);
    }

    private static void AssignCategories(List<AttomFieldGroupViewModel> sections)
    {
        foreach (var section in sections)
        {
            section.CategoryKey = ResolveCategoryKey(section);
        }
    }

    private static string ResolveCategoryKey(AttomFieldGroupViewModel section)
    {
        var haystack = $"{section.SectionId} {section.Title} {section.DisplayTitle}".ToLowerInvariant();

        foreach (var category in Categories)
        {
            if (category.Keywords.Any(k => haystack.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                return category.Key;
            }
        }

        return "more";
    }

    private static List<HouseFactCategoryCardViewModel> BuildCategoryCards(List<AttomFieldGroupViewModel> sections)
    {
        var cards = new List<HouseFactCategoryCardViewModel>();

        foreach (var def in Categories)
        {
            var matched = sections.Where(s => s.CategoryKey == def.Key).ToList();
            if (matched.Count == 0)
            {
                continue;
            }

            var itemCount = matched.Sum(s => s.ItemCount);
            cards.Add(new HouseFactCategoryCardViewModel
            {
                Key = def.Key,
                Title = def.Title,
                Subtitle = def.Subtitle,
                Icon = def.Icon,
                Tone = def.Tone,
                IsWarning = def.IsWarning,
                ItemCount = itemCount,
                Badge = BuildBadge(def, matched, itemCount),
                SectionIds = matched.Select(s => s.SectionId).ToList()
            });
        }

        var moreSections = sections.Where(s => s.CategoryKey == "more").ToList();
        if (moreSections.Count > 0)
        {
            var existingMore = cards.FirstOrDefault(c => c.Key == "more");
            if (existingMore == null)
            {
                var count = moreSections.Sum(s => s.ItemCount);
                cards.Add(new HouseFactCategoryCardViewModel
                {
                    Key = "more",
                    Title = "More Details",
                    Subtitle = "Additional property information",
                    Icon = "fa-ellipsis",
                    Tone = "blue",
                    ItemCount = count,
                    Badge = count > 0 ? $"{count} items" : "View",
                    SectionIds = moreSections.Select(s => s.SectionId).ToList()
                });
            }
            else
            {
                existingMore.SectionIds.AddRange(moreSections.Select(s => s.SectionId));
                existingMore.ItemCount += moreSections.Sum(s => s.ItemCount);
                existingMore.Badge = $"{existingMore.ItemCount} items";
            }
        }

        return cards;
    }

    private static void AppendNearbyPlaceCards(List<HouseFactCategoryCardViewModel> cards, HouseFactProfileViewModel profile)
    {
        foreach (var place in NearbyPlacesDisplayService.BuildOverviewCards(profile))
        {
            if (cards.Any(c => c.Key == place.Key))
            {
                continue;
            }

            cards.Add(new HouseFactCategoryCardViewModel
            {
                Key = place.Key,
                Title = place.Title,
                Subtitle = place.Subtitle,
                Icon = place.Icon,
                Tone = place.Tone,
                ItemCount = place.ItemCount,
                Badge = place.Badge,
                SectionIds = []
            });
        }
    }

    private static string BuildBadge(CategoryDef def, List<AttomFieldGroupViewModel> sections, int itemCount)
    {
        if (def.Key == "risk")
        {
            var risk = FindRiskLevel(sections);
            return string.IsNullOrWhiteSpace(risk) ? "Unknown" : risk;
        }

        if (def.Key == "hoa")
        {
            var hoa = FindFieldValue(sections, "hoa", "hoa fee", "hoa name", "association");
            if (string.IsNullOrWhiteSpace(hoa) || IsUnconfirmed(hoa))
            {
                return itemCount > 0 ? $"{itemCount} items" : "Unknown";
            }

            if (hoa.Contains("none", StringComparison.OrdinalIgnoreCase)
                || hoa.Contains("no hoa", StringComparison.OrdinalIgnoreCase))
            {
                return "None";
            }

            return "Active";
        }

        if (def.Key == "schools")
        {
            var schoolCount = sections.Sum(s => s.Schools.Count);
            if (schoolCount == 0)
            {
                schoolCount = sections.Sum(s => s.Fields.Count(f =>
                    f.Label.Contains("school", StringComparison.OrdinalIgnoreCase)));
            }

            return schoolCount > 0 ? $"{schoolCount} schools" : $"{itemCount} items";
        }

        if (itemCount <= 0)
        {
            return "View";
        }

        return $"{itemCount} {def.BadgeSuffix}";
    }

    private static string? FindRiskLevel(List<AttomFieldGroupViewModel> sections)
    {
        foreach (var section in sections)
        {
            foreach (var field in section.Fields)
            {
                var value = field.Value.Trim();
                if (Regex.IsMatch(value, @"\b(Low|Medium|High|Unknown)\b", RegexOptions.IgnoreCase))
                {
                    var match = Regex.Match(value, @"\b(Low|Medium|High|Unknown)\b", RegexOptions.IgnoreCase);
                    return char.ToUpper(match.Value[0]) + match.Value[1..].ToLowerInvariant();
                }

                if (field.Label.Contains("overall", StringComparison.OrdinalIgnoreCase)
                    || field.Label.Contains("risk", StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }
        }

        return null;
    }

    private static string? BuildLocationSummary(HouseFactProfileViewModel profile)
    {
        var city = FindFieldValue(profile.Sections, "city", "citystatezip", "city / state");
        var county = FindFieldValue(profile.Sections, "county", "jurisdiction");

        if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(county))
        {
            return $"{city} · {county}";
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            return city;
        }

        if (!string.IsNullOrWhiteSpace(county))
        {
            return county;
        }

        return ExtractCityCountyFromAddress(profile.FormattedAddress);
    }

    private static string? ExtractCityCountyFromAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        var parts = address.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return $"{parts[^2]}, {parts[^1]}";
        }

        return address;
    }

    private static string? FindFieldValue(IEnumerable<AttomFieldGroupViewModel> sections, params string[] labels)
    {
        foreach (var section in sections)
        {
            foreach (var field in section.Fields)
            {
                var label = field.Label.ToLowerInvariant();
                if (labels.Any(l => label.Contains(l, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!IsUnconfirmed(field.Value))
                    {
                        return field.Value.Trim();
                    }
                }
            }
        }

        return null;
    }

    private static bool IsUnconfirmed(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "—")
        {
            return true;
        }

        return value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
            || value.Contains("needs verification", StringComparison.OrdinalIgnoreCase)
            || value.Contains("not confirmed", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatStat(string? value) =>
        string.IsNullOrWhiteSpace(value) || IsUnconfirmed(value) ? "— Not confirmed" : value;

    private static string FormatConfidence(string? confidence)
    {
        if (string.IsNullOrWhiteSpace(confidence))
        {
            return "Needs verification";
        }

        if (confidence.Contains("confirm", StringComparison.OrdinalIgnoreCase))
        {
            return "Confirmed";
        }

        if (confidence.Contains("estimat", StringComparison.OrdinalIgnoreCase))
        {
            return "Estimated · Needs verification";
        }

        return confidence;
    }
}
