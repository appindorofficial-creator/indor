using System.Globalization;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class PropertySnapshotDisplayService
{
    private static readonly (string Label, string[] Keys, string Icon)[] CoreStatDefs =
    [
        ("Property Type", ["property type", "propertytype", "type"], "fa-house"),
        ("Year Built", ["year built", "yearbuilt", "effective year"], "fa-calendar"),
        ("Bedrooms", ["beds", "bedrooms", "bed"], "fa-bed"),
        ("Bathrooms", ["baths", "bathrooms", "bath"], "fa-bath"),
        ("Interior Living Area", ["sq ft", "sqft", "living area", "square feet", "living sq"], "fa-ruler-combined"),
        ("Lot Size", ["lot size", "lot", "lot acres", "lot size acres"], "fa-tree")
    ];

    private static readonly (string Label, string[] Keys, string Icon)[] HighlightDefs =
    [
        ("County", ["county"], "fa-landmark"),
        ("Jurisdiction", ["jurisdiction", "municipality", "city"], "fa-building-columns"),
        ("Parcel ID / APN", ["parcel", "apn", "parcel id"], "fa-barcode"),
        ("Neighborhood", ["neighborhood", "subdivision", "subdivisionneighborhood"], "fa-people-group"),
        ("Current Status", ["status", "listing status", "current status"], "fa-circle-notch"),
        ("Land Use", ["land use", "landuse", "use"], "fa-vector-square")
    ];

    private static readonly (string Label, string[] Keys, string Icon, bool FullWidth)[] DetailFieldDefs =
    [
        ("Address", ["address", "formattedaddress", "full address"], "fa-location-dot", true),
        ("City / State / ZIP", ["citystatezip", "city / state", "city state zip", "city, state"], "fa-map-location-dot", false),
        ("County", ["county"], "fa-landmark", false),
        ("Jurisdiction", ["jurisdiction", "municipality"], "fa-building-columns", false),
        ("Parcel ID / APN", ["parcel", "apn"], "fa-barcode", false),
        ("Neighborhood", ["neighborhood", "subdivisionneighborhood"], "fa-people-group", false),
        ("Property Type", ["property type", "propertytype"], "fa-house", false),
        ("Land Use", ["land use", "landuse"], "fa-vector-square", false),
        ("Current Status", ["status", "current status", "listing status"], "fa-circle-notch", false),
        ("Zoning", ["zoning"], "fa-map", false),
        ("Subdivision", ["subdivision"], "fa-signs-post", false),
        ("Year Built", ["year built", "yearbuilt"], "fa-calendar", false)
    ];

    private static readonly (string Label, string[] Keys, string Icon)[] LotFieldDefs =
    [
        ("Lot Size", ["lot size", "lot", "lot acres"], "fa-ruler-combined"),
        ("Stories", ["stories", "floors", "floor"], "fa-layer-group"),
        ("Garage / Parking", ["garage", "parking", "garage parking"], "fa-warehouse"),
        ("Foundation", ["foundation", "crawl", "basement", "slab"], "fa-layer-group"),
        ("Roof Type", ["roof", "roof type"], "fa-house-chimney"),
        ("Exterior Material", ["exterior", "wall", "exterior material"], "fa-border-all"),
        ("Porch / Deck", ["porch", "deck", "patio"], "fa-chair"),
        ("Flood Zone", ["flood"], "fa-water"),
        ("Driveway / Drainage", ["driveway", "drainage"], "fa-road"),
        ("Nearby Roads", ["road", "access"], "fa-road")
    ];

    public static PropertySnapshotViewModel Build(Propiedad propiedad, PropertyInfoViewModel? info, string? tab = null)
    {
        var profile = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated",
            propiedad.Direccion ?? info?.FormattedAddress);

        var details = info?.PropertyDetails;
        var fieldMap = BuildFieldMap(profile, info, propiedad);
        var address = propiedad.Direccion ?? info?.FormattedAddress ?? profile.FormattedAddress ?? "Property address";
        var confidence = ResolveConfidence(profile.Confidence, fieldMap);
        var activeTab = NormalizeTab(tab);

        var model = new PropertySnapshotViewModel
        {
            PropiedadId = propiedad.Id,
            Address = address,
            ActiveTab = activeTab,
            PageSubtitle = SubtitleForTab(activeTab),
            ConfidenceBadge = confidence.Badge,
            ConfidenceLevel = confidence.Level,
            FieldCount = CountPopulatedFields(fieldMap),
            HasData = profile.HasData || details != null,
            Latitude = info?.Latitude,
            Longitude = info?.Longitude,
            CoreStats = BuildCoreStats(fieldMap, details),
            QuickSections = BuildQuickSections(),
            Highlights = BuildHighlights(fieldMap, details),
            DetailFields = BuildDetailFields(fieldMap, address),
            LotFields = BuildLotFields(fieldMap, profile, details),
            Notes = BuildNotes(profile, fieldMap)
        };

        SetNextTab(model);
        return model;
    }

    private static void SetNextTab(PropertySnapshotViewModel model)
    {
        switch (model.ActiveTab)
        {
            case "overview":
                model.NextTab = "details";
                model.NextTabLabel = "Next: Details";
                break;
            case "details":
                model.NextTab = "lot";
                model.NextTabLabel = "Next: Lot details";
                break;
            case "lot":
                model.NextTab = "notes";
                model.NextTabLabel = "Next: Notes";
                break;
            default:
                model.NextTab = "overview";
                model.NextTabLabel = "Done";
                break;
        }
    }

    private static Dictionary<string, string> BuildFieldMap(
        HouseFactProfileViewModel profile,
        PropertyInfoViewModel? info,
        Propiedad propiedad)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (info != null)
        {
            UpsertField(map, "Address", info.FormattedAddress);
            UpsertField(map, "City / State / ZIP", BuildCityStateZip(info));
            UpsertField(map, "County", info.County ?? info.PropertyDetails?.CountyName);
            UpsertField(map, "Latitude", info.Latitude != 0 ? info.Latitude.ToString(CultureInfo.InvariantCulture) : null);
            UpsertField(map, "Longitude", info.Longitude != 0 ? info.Longitude.ToString(CultureInfo.InvariantCulture) : null);
        }

        var d = info?.PropertyDetails;
        if (d != null)
        {
            UpsertField(map, "Property Type", d.PropertyType);
            UpsertField(map, "Year Built", d.YearBuilt?.ToString());
            UpsertField(map, "Bedrooms", d.Bedrooms?.ToString());
            UpsertField(map, "Bathrooms", d.Bathrooms?.ToString("0.#"));
            UpsertField(map, "Interior Living Area", d.LivingArea.HasValue ? $"{d.LivingArea:N0} sq ft" : null);
            UpsertField(map, "Lot Size", FormatLot(d.LotSize, d.LotSizeSqFt));
            UpsertField(map, "Stories", d.Floors?.ToString());
            UpsertField(map, "Parcel ID / APN", d.ParcelNumber);
            UpsertField(map, "Zoning", d.Zoning);
            UpsertField(map, "Subdivision", d.Subdivision);
            UpsertField(map, "Neighborhood", d.Subdivision);
            UpsertField(map, "Garage / Parking", d.GarageType ?? d.ParkingType);
            UpsertField(map, "Exterior Material", d.WallType);
            UpsertField(map, "Foundation", d.BasementSqFt.HasValue ? $"Basement {d.BasementSqFt:N0} sq ft" : null);
        }

        foreach (var section in profile.Sections.Where(s => s.CategoryKey == "snapshot" || IsSnapshotSection(s)))
        {
            foreach (var field in section.Fields)
            {
                MergeSectionField(map, field.Label, field.Value);
            }
        }

        UpsertField(map, "Address", propiedad.Direccion);
        return map;
    }

    private static List<SnapshotStatViewModel> BuildCoreStats(Dictionary<string, string> map, PropertyDetailsInfo? details)
    {
        var stats = new List<SnapshotStatViewModel>();
        foreach (var (label, keys, icon) in CoreStatDefs)
        {
            var value = ResolveValue(map, keys) ?? FallbackCoreStat(label, details);
            stats.Add(new SnapshotStatViewModel
            {
                Label = label,
                Value = FormatDisplayValue(value, label),
                Icon = icon
            });
        }

        return stats;
    }

    private static List<SnapshotHighlightViewModel> BuildHighlights(Dictionary<string, string> map, PropertyDetailsInfo? details)
    {
        var items = new List<SnapshotHighlightViewModel>();
        foreach (var (label, keys, icon) in HighlightDefs)
        {
            var raw = ResolveValue(map, keys) ?? FallbackHighlight(label, details);
            items.Add(new SnapshotHighlightViewModel
            {
                Label = label,
                Value = FormatDisplayValue(raw, label),
                Icon = icon,
                IsEstimated = IsEstimatedValue(raw)
            });
        }

        return items;
    }

    private static List<SnapshotFieldViewModel> BuildDetailFields(Dictionary<string, string> map, string modelAddress)
    {
        var fields = new List<SnapshotFieldViewModel>();
        foreach (var (label, keys, icon, fullWidth) in DetailFieldDefs)
        {
            var raw = label == "Address"
                ? (ResolveValue(map, keys) ?? modelAddress)
                : ResolveValue(map, keys);

            fields.Add(new SnapshotFieldViewModel
            {
                Label = label,
                Value = FormatDisplayValue(raw, label),
                Icon = icon,
                FullWidth = fullWidth,
                IsEstimated = IsEstimatedValue(raw)
            });
        }

        return fields;
    }

    private static List<SnapshotFieldViewModel> BuildLotFields(
        Dictionary<string, string> map,
        HouseFactProfileViewModel profile,
        PropertyDetailsInfo? details)
    {
        var fields = new List<SnapshotFieldViewModel>();
        foreach (var (label, keys, icon) in LotFieldDefs)
        {
            var raw = ResolveValue(map, keys) ?? FallbackLot(label, profile, details);
            fields.Add(new SnapshotFieldViewModel
            {
                Label = label,
                Value = FormatDisplayValue(raw, label),
                Icon = icon,
                IsEstimated = IsEstimatedValue(raw)
            });
        }

        return fields;
    }

    private static SnapshotNotesViewModel BuildNotes(HouseFactProfileViewModel profile, Dictionary<string, string> map)
    {
        var sources = profile.Sections
            .Where(s => s.SectionKind == "sources")
            .SelectMany(s => s.Sources)
            .Select(s => new SnapshotSourceViewModel
            {
                Name = s.SourceName,
                Icon = IconForSource(s.SourceName)
            })
            .Take(5)
            .ToList();

        if (sources.Count == 0)
        {
            sources =
            [
                new SnapshotSourceViewModel { Name = "County records", Icon = "fa-landmark" },
                new SnapshotSourceViewModel { Name = "Public listings", Icon = "fa-house" },
                new SnapshotSourceViewModel { Name = "Web search", Icon = "fa-globe" }
            ];
        }

        var missing = profile.Sections
            .Where(s => s.CategoryKey == "missing" || s.SectionKind is "checklist" or "questions")
            .SelectMany(s => s.ChecklistItems)
            .Take(6)
            .Select(i => new SnapshotMissingItemViewModel
            {
                Title = i.Item,
                Status = string.IsNullOrWhiteSpace(i.Status) ? "Needs verification" : i.Status,
                Icon = IconForMissing(i.Item)
            })
            .ToList();

        if (missing.Count == 0)
        {
            missing =
            [
                new SnapshotMissingItemViewModel { Title = "Seller disclosure", Status = "Unknown", Icon = "fa-file-lines" },
                new SnapshotMissingItemViewModel { Title = "Inspection report", Status = "Not recorded", Icon = "fa-clipboard-check" },
                new SnapshotMissingItemViewModel { Title = "Roof age", Status = "Unknown", Icon = "fa-house-chimney" },
                new SnapshotMissingItemViewModel { Title = "Permit history", Status = "Needs verification", Icon = "fa-file-contract" }
            ];
        }

        return new SnapshotNotesViewModel
        {
            ConfidenceSummary = BuildConfidenceSummary(profile.Confidence),
            Sources = sources,
            MissingItems = missing
        };
    }

    private static List<SnapshotQuickLinkViewModel> BuildQuickSections() =>
    [
        new() { Title = "Identity", Subtitle = "Ownership & legal info", Icon = "fa-id-card", Tab = "details" },
        new() { Title = "Home Facts", Subtitle = "Key property details", Icon = "fa-house", Tab = "overview" },
        new() { Title = "Location", Subtitle = "Address & neighborhood", Icon = "fa-location-dot", Tab = "lot" },
        new() { Title = "Verification", Subtitle = "Sources & confidence", Icon = "fa-shield-check", Tab = "notes" }
    ];

    private static string? ResolveValue(Dictionary<string, string> map, string[] keys)
    {
        foreach (var key in keys)
        {
            var match = map.FirstOrDefault(kv => kv.Key.Contains(key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Value) && !IsPlaceholderValue(match.Value))
            {
                return match.Value.Trim();
            }
        }

        return null;
    }

    private static void AddField(Dictionary<string, string> map, string label, string? value) =>
        UpsertField(map, label, value);

    private static void UpsertField(Dictionary<string, string> map, string label, string? value)
    {
        if (IsPlaceholderValue(value)) return;
        map[label] = value!.Trim();
    }

    private static void MergeSectionField(Dictionary<string, string> map, string label, string? value)
    {
        if (IsPlaceholderValue(value)) return;
        if (map.TryGetValue(label, out var existing) && !IsPlaceholderValue(existing))
        {
            return;
        }

        map[label] = value!.Trim();
    }

    private static bool IsSnapshotSection(AttomFieldGroupViewModel section)
    {
        var key = $"{section.SectionId} {section.Title} {section.DisplayTitle}".ToLowerInvariant();
        return key.Contains("snapshot") || key.Contains("identity") || key.Contains("basic property");
    }

    private static string NormalizeTab(string? tab) => tab?.ToLowerInvariant() switch
    {
        "details" => "details",
        "lot" => "lot",
        "notes" => "notes",
        _ => "overview"
    };

    private static string SubtitleForTab(string tab) => tab switch
    {
        "details" => "Core identity details",
        "lot" => "Lot, site & exterior basics",
        "notes" => "Confidence, notes & follow-up",
        _ => "Your property at a glance."
    };

    private static (string Badge, string Level) ResolveConfidence(string? confidence, Dictionary<string, string> map)
    {
        var text = confidence ?? ResolveValue(map, ["confidence", "status"]) ?? "estimated";
        if (text.Contains("confirm", StringComparison.OrdinalIgnoreCase))
        {
            return ("Mostly confirmed", "confirmed");
        }

        if (text.Contains("verif", StringComparison.OrdinalIgnoreCase))
        {
            return ("Needs verification", "verify");
        }

        return ("Mostly estimated", "estimated");
    }

    private static string BuildConfidenceSummary(string? confidence)
    {
        if (string.IsNullOrWhiteSpace(confidence))
        {
            return "Some details are confirmed from public records. Others are estimated from saved web search data and should be verified.";
        }

        return confidence.Contains("confirm", StringComparison.OrdinalIgnoreCase)
            ? "Most snapshot details appear confirmed from public records in your saved House Fact profile."
            : "Some details are confirmed from public records. Others are estimated from web sources and should be verified.";
    }

    private static int CountPopulatedFields(Dictionary<string, string> map) =>
        map.Count(kv => !IsEstimatedValue(kv.Value) && !IsPlaceholderValue(kv.Value));

    private static string FormatDisplayValue(string? raw, string label)
    {
        if (IsPlaceholderValue(raw))
        {
            return label.Contains("Year", StringComparison.OrdinalIgnoreCase) ? "Not confirmed" : "—";
        }

        if (label == "Year Built" && int.TryParse(raw, out var year))
        {
            return $"Built {year}";
        }

        if (label == "Bedrooms" && int.TryParse(raw, out var beds))
        {
            return $"{beds} Beds";
        }

        if (label == "Bathrooms" && decimal.TryParse(raw, out var baths))
        {
            return $"{baths:0.#} Baths";
        }

        return raw;
    }

    private static string? FallbackCoreStat(string label, PropertyDetailsInfo? d)
    {
        if (d == null) return null;
        return label switch
        {
            "Property Type" => d.PropertyType,
            "Year Built" => d.YearBuilt?.ToString(),
            "Bedrooms" => d.Bedrooms?.ToString(),
            "Bathrooms" => d.Bathrooms?.ToString("0.#"),
            "Interior Living Area" => d.LivingArea.HasValue ? $"{d.LivingArea:N0} sq ft" : null,
            "Lot Size" => FormatLot(d.LotSize, d.LotSizeSqFt),
            _ => null
        };
    }

    private static string? FallbackHighlight(string label, PropertyDetailsInfo? d)
    {
        if (d == null) return null;
        return label switch
        {
            "County" => d.CountyName,
            "Parcel ID / APN" => d.ParcelNumber,
            "Neighborhood" => d.Subdivision,
            "Subdivision" => d.Subdivision,
            "Current Status" => "Estimated",
            _ => null
        };
    }

    private static string? FallbackLot(string label, HouseFactProfileViewModel profile, PropertyDetailsInfo? d)
    {
        if (d == null) return null;
        return label switch
        {
            "Lot Size" => FormatLot(d.LotSize, d.LotSizeSqFt),
            "Stories" => d.Floors?.ToString(),
            "Garage / Parking" => d.GarageType ?? d.ParkingType,
            "Exterior Material" => d.WallType,
            "Roof Type" => null,
            _ => null
        };
    }

    private static string? FormatLot(decimal? acres, int? sqft)
    {
        if (acres.HasValue) return $"{acres.Value:0.##} acres";
        if (sqft.HasValue) return $"{sqft.Value:N0} sq ft";
        return null;
    }

    private static string BuildCityStateZip(PropertyInfoViewModel info)
    {
        var parts = new[] { info.City, info.State, info.PostalCode }.Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    private static bool IsEstimatedValue(string? value) =>
        !string.IsNullOrWhiteSpace(value) && (
            value.Contains("estimated", StringComparison.OrdinalIgnoreCase)
            || value.Contains("verification", StringComparison.OrdinalIgnoreCase)
            || value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
            || value.Contains("not confirmed", StringComparison.OrdinalIgnoreCase));

    private static bool IsPlaceholderValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || IsEmptyValue(value))
        {
            return true;
        }

        return value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
            || value.Contains("needs verification", StringComparison.OrdinalIgnoreCase)
            || value.Contains("not confirmed", StringComparison.OrdinalIgnoreCase)
            || value.Contains("unknown", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEmptyValue(string value) =>
        value.Trim() is "—" or "-" or "N/A" or "n/a";

    private static string IconForSource(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.Contains("county") || n.Contains("gis") || n.Contains("assessor")) return "fa-landmark";
        if (n.Contains("tax")) return "fa-file-invoice-dollar";
        if (n.Contains("zillow")) return "fa-z";
        if (n.Contains("realtor")) return "fa-r";
        if (n.Contains("redfin")) return "fa-house";
        return "fa-globe";
    }

    private static string IconForMissing(string title)
    {
        var t = title.ToLowerInvariant();
        if (t.Contains("roof")) return "fa-house-chimney";
        if (t.Contains("hvac")) return "fa-fan";
        if (t.Contains("water")) return "fa-droplet";
        if (t.Contains("permit")) return "fa-file-contract";
        if (t.Contains("inspection")) return "fa-clipboard-check";
        return "fa-circle-question";
    }
}
