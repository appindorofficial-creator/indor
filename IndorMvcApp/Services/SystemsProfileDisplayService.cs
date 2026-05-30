using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class SystemsProfileDisplayService
{
    private sealed record SystemDef(
        string Id,
        string Name,
        string Category,
        string Icon,
        string DefaultSubtitle,
        string[] FieldKeys,
        string VerificationTitle,
        string VerificationSubtitle);

    private static readonly SystemDef[] SystemDefs =
    [
        new("hvac", "HVAC", "hvac", "fa-snowflake", "Central HVAC • Electric",
            ["hvac", "heating", "cooling", "air condition", "furnace", "heat pump"],
            "HVAC serial number", "Add the model and serial number"),
        new("water-heater", "Water Heater", "water-heater", "fa-fire-burner", "Type not publicly confirmed",
            ["water heater", "waterheater", "hot water"],
            "Water heater serial number", "Add the model and serial number"),
        new("electrical", "Electrical", "electrical", "fa-bolt", "Panel & main service",
            ["electrical", "electric panel", "panel", "service"],
            "Electrical panel photo", "Upload a clear photo of your panel"),
        new("plumbing", "Plumbing", "plumbing", "fa-faucet", "Supply, drains & fixtures",
            ["plumbing", "plumb", "sewer", "water supply"],
            "Plumbing notes", "Add notes about pipes or fixtures"),
        new("appliances", "Appliances", "appliances", "fa-kitchen-set", "Major built-in appliances",
            ["appliance", "dishwasher", "range", "oven", "refrigerator"],
            "Appliance details", "Confirm major built-in appliances"),
        new("smoke-co", "Smoke / CO", "smoke-co", "fa-wifi", "Safety devices",
            ["smoke", "carbon monoxide", "co detector", "detector"],
            "Smoke / CO devices", "Confirm safety devices are present")
    ];

    public static SystemsProfileIndexViewModel BuildIndex(Propiedad propiedad, PropertyInfoViewModel? info, string? filter = null)
    {
        var ctx = BuildContext(propiedad, info);
        var activeFilter = NormalizeFilter(filter);
        var systems = activeFilter == "all"
            ? ctx.Systems
            : ctx.Systems.Where(s => s.Category == activeFilter).ToList();

        return new SystemsProfileIndexViewModel
        {
            PropiedadId = propiedad.Id,
            Address = ctx.Address,
            ActiveFilter = activeFilter,
            HasData = ctx.HasData,
            SystemCount = ctx.Systems.Count,
            NeedsVerificationCount = ctx.Systems.Count(s => s.StatusTone is "orange" or "yellow"),
            AlertCount = ctx.Systems.Count(s => s.StatusTone == "red"),
            Systems = systems,
            Filters = BuildFilters()
        };
    }

    public static SystemsVerificationViewModel BuildVerification(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var ctx = BuildContext(propiedad, info);
        var items = ctx.Systems.Select(s => new SystemVerificationItemViewModel
        {
            SystemId = s.Id,
            Title = SystemDefs.First(d => d.Id == s.Id).VerificationTitle,
            Subtitle = SystemDefs.First(d => d.Id == s.Id).VerificationSubtitle,
            Icon = s.Icon,
            Status = MapVerificationStatus(s.Status),
            StatusTone = MapVerificationTone(s.Status)
        }).ToList();

        var verified = items.Count(i => i.StatusTone is "green");
        var total = items.Count;

        return new SystemsVerificationViewModel
        {
            PropiedadId = propiedad.Id,
            Items = items,
            VerifiedCount = verified,
            TotalCount = total,
            ProgressPercent = total == 0 ? 0 : (int)Math.Round(verified * 100.0 / total)
        };
    }

    public static SystemDetailViewModel? BuildDetail(Propiedad propiedad, PropertyInfoViewModel? info, string systemId, string? tab = null)
    {
        var def = SystemDefs.FirstOrDefault(d => string.Equals(d.Id, systemId, StringComparison.OrdinalIgnoreCase));
        if (def == null) return null;

        var ctx = BuildContext(propiedad, info);
        var system = ctx.Systems.First(s => s.Id == def.Id);
        var fields = ExtractSystemFields(ctx.FieldMap, def);
        var attributes = BuildAttributes(def, fields, info);
        var known = BuildKnownFacts(def, fields, info);
        var missing = BuildMissingItems(def, attributes);
        var activeTab = NormalizeDetailTab(tab, def.Id);

        return new SystemDetailViewModel
        {
            PropiedadId = propiedad.Id,
            SystemId = def.Id,
            Name = def.Name,
            PageSubtitle = SubtitleForSystem(def.Id),
            Address = ctx.Address,
            ActiveTab = activeTab,
            StatusBadges = BuildStatusBadges(system, def, missing.Count),
            Attributes = attributes,
            KnownFacts = known,
            MissingItems = missing,
            SuggestedActions = BuildSuggestedActions(def, propiedad.Id),
            InfoBanner = BannerForSystem(def.Id),
            Tabs = TabsForSystem(def.Id),
            ServiceHistoryNote = activeTab is "service" or "maintenance"
                ? "No service records saved yet in your House Fact profile."
                : null,
            WarrantyNote = activeTab == "warranty"
                ? "Warranty details were not confirmed in saved public data."
                : null,
            DocumentsNote = activeTab == "documents"
                ? "Upload invoices, inspection reports, or photos to improve system accuracy."
                : null,
            HvacMicroservicioId = def.Id == "hvac" ? ctx.HvacMicroservicioId : null
        };
    }

    private sealed class SystemsContext
    {
        public string Address { get; set; } = string.Empty;
        public bool HasData { get; set; }
        public Dictionary<string, string> FieldMap { get; set; } = new();
        public List<SystemProfileItemViewModel> Systems { get; set; } = new();
        public int? HvacMicroservicioId { get; set; }
    }

    private static SystemsContext BuildContext(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var profile = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated",
            propiedad.Direccion ?? info?.FormattedAddress);

        var fieldMap = BuildFieldMap(profile, info);
        var systems = SystemDefs.Select(def => BuildSystemItem(def, fieldMap, info)).ToList();

        return new SystemsContext
        {
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? profile.FormattedAddress ?? "Property address",
            HasData = profile.HasData || info?.PropertyDetails != null,
            FieldMap = fieldMap,
            Systems = systems
        };
    }

    private static Dictionary<string, string> BuildFieldMap(HouseFactProfileViewModel profile, PropertyInfoViewModel? info)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var section in profile.Sections.Where(s =>
            s.CategoryKey == "systems" || IsSystemsSection(s)))
        {
            foreach (var field in section.Fields)
            {
                AddField(map, field.Label, field.Value);
            }

            if (!string.IsNullOrWhiteSpace(section.Paragraph))
            {
                AddField(map, section.DisplayTitle, section.Paragraph);
            }
        }

        var d = info?.PropertyDetails;
        if (d != null)
        {
            AddField(map, "HVAC Type", d.HeatingType);
            AddField(map, "HVAC Fuel", d.HeatingFuel);
            AddField(map, "Cooling Type", d.CoolingType);
            AddField(map, "Heating", $"{d.HeatingType} / {d.HeatingFuel}");
        }

        var warranties = info?.HomeWarranties;
        if (warranties?.HVACSystem != null)
        {
            AddField(map, "HVAC warranty", warranties.HVACSystem.Status ?? warranties.HVACSystem.WarrantyProvider);
        }

        if (warranties?.WaterHeater != null)
        {
            AddField(map, "Water heater warranty", warranties.WaterHeater.Status);
        }

        return map;
    }

    private static SystemProfileItemViewModel BuildSystemItem(SystemDef def, Dictionary<string, string> map, PropertyInfoViewModel? info)
    {
        var fields = ExtractSystemFields(map, def);
        var subtitle = BuildSubtitle(def, fields, info);
        var status = ResolveStatus(fields, def, info);

        return new SystemProfileItemViewModel
        {
            Id = def.Id,
            Name = def.Name,
            Category = def.Category,
            Icon = def.Icon,
            Subtitle = subtitle,
            Status = status.Label,
            StatusTone = status.Tone
        };
    }

    private static List<(string Label, string Value)> ExtractSystemFields(Dictionary<string, string> map, SystemDef def)
    {
        var results = new List<(string, string)>();
        foreach (var kv in map)
        {
            if (def.FieldKeys.Any(k => kv.Key.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                results.Add((kv.Key, kv.Value));
            }
        }

        return results;
    }

    private static string BuildSubtitle(SystemDef def, List<(string Label, string Value)> fields, PropertyInfoViewModel? info)
    {
        if (def.Id == "hvac")
        {
            var heating = info?.PropertyDetails?.HeatingType ?? FindValue(fields, "hvac", "heating", "type");
            var fuel = info?.PropertyDetails?.HeatingFuel ?? FindValue(fields, "fuel");
            if (!string.IsNullOrWhiteSpace(heating) && !IsUnconfirmed(heating))
            {
                return string.IsNullOrWhiteSpace(fuel) || IsUnconfirmed(fuel)
                    ? heating
                    : $"{heating} • {fuel}";
            }
        }

        var first = fields.FirstOrDefault(f => !IsUnconfirmed(f.Value));
        if (!string.IsNullOrWhiteSpace(first.Value))
        {
            return Truncate(first.Value, 42);
        }

        return def.DefaultSubtitle;
    }

    private static (string Label, string Tone) ResolveStatus(
        List<(string Label, string Value)> fields,
        SystemDef def,
        PropertyInfoViewModel? info)
    {
        if (def.Id == "smoke-co" && fields.Any(f => f.Value.Contains("confirm", StringComparison.OrdinalIgnoreCase)))
        {
            return ("Confirmed", "green");
        }

        if (fields.Count == 0)
        {
            return def.Id == "appliances" ? ("Not confirmed", "gray") : ("Needs verification", "orange");
        }

        if (fields.All(f => IsUnconfirmed(f.Value)))
        {
            return def.Id switch
            {
                "electrical" => ("Review", "yellow"),
                "plumbing" => ("Estimated", "green"),
                "appliances" => ("Not confirmed", "gray"),
                _ => ("Needs verification", "orange")
            };
        }

        if (fields.Any(f => NeedsVerification(f.Value)))
        {
            return ("Needs verification", "orange");
        }

        return ("Estimated", "green");
    }

    private static List<SystemAttributeViewModel> BuildAttributes(SystemDef def, List<(string, string)> fields, PropertyInfoViewModel? info)
    {
        if (def.Id == "hvac")
        {
            return
            [
                Attr("Type", info?.PropertyDetails?.HeatingType ?? FindValue(fields, "type", "hvac") ?? "Central HVAC"),
                Attr("Fuel", info?.PropertyDetails?.HeatingFuel ?? FindValue(fields, "fuel") ?? "Electric", ToneFor(FindValue(fields, "fuel"))),
                Attr("Zones", FindValue(fields, "zone") ?? "1"),
                Attr("Age", FindValue(fields, "age", "year") ?? "Not publicly confirmed", "orange"),
                Attr("Serial Number", FindValue(fields, "serial") ?? "Needs owner upload", "orange"),
                Attr("Last Service", FindValue(fields, "service", "last") ?? "Unknown")
            ];
        }

        if (def.Id == "water-heater")
        {
            return
            [
                Attr("Type", FindValue(fields, "type") ?? "Not publicly confirmed"),
                Attr("Fuel Source", FindValue(fields, "fuel") ?? "Not publicly confirmed"),
                Attr("Tank Size", FindValue(fields, "size", "tank") ?? "Needs verification", "orange"),
                Attr("Install Date", FindValue(fields, "install") ?? "Unknown"),
                Attr("Serial Number", FindValue(fields, "serial") ?? "Add to verify age", "blue"),
                Attr("Last Flush", FindValue(fields, "flush", "service") ?? "Unknown")
            ];
        }

        return fields.Take(6).Select(f => Attr(f.Item1, f.Item2, ToneFor(f.Item2))).ToList();
    }

    private static List<SystemFactViewModel> BuildKnownFacts(SystemDef def, List<(string, string)> fields, PropertyInfoViewModel? info)
    {
        if (def.Id == "hvac")
        {
            return
            [
                Fact("System type", info?.PropertyDetails?.HeatingType ?? FindValue(fields, "type", "hvac") ?? "Central HVAC", "fa-snowflake"),
                Fact("Fuel type", info?.PropertyDetails?.HeatingFuel ?? FindValue(fields, "fuel") ?? "Electric", "fa-bolt"),
                Fact("Number of zones", FindValue(fields, "zone") ?? "1", "fa-table-cells"),
                Fact("Location", FindValue(fields, "location") ?? "Outdoor — verify on site", "fa-location-dot")
            ];
        }

        return fields.Where(f => !IsUnconfirmed(f.Item2)).Take(4)
            .Select(f => Fact(f.Item1, f.Item2, "fa-circle-check"))
            .ToList();
    }

    private static List<SystemFactViewModel> BuildMissingItems(SystemDef def, List<SystemAttributeViewModel> attributes)
    {
        var missing = attributes
            .Where(a => a.Tone is "orange" or "blue" || IsUnconfirmed(a.Value) || a.Value is "Unknown")
            .Select(a => Fact(a.Label, a.Value, "fa-circle-exclamation"))
            .ToList();

        if (missing.Count == 0)
        {
            missing = def.Id switch
            {
                "hvac" =>
                [
                    Fact("Serial number", "Needed", "fa-circle-exclamation"),
                    Fact("Install date", "Unknown", "fa-circle-exclamation"),
                    Fact("Warranty", "Not recorded", "fa-circle-exclamation"),
                    Fact("Last tune-up", "Unknown", "fa-circle-exclamation")
                ],
                "water-heater" =>
                [
                    Fact("Serial number", "Needed", "fa-circle-exclamation"),
                    Fact("Install date", "Unknown", "fa-circle-exclamation"),
                    Fact("Warranty", "Not recorded", "fa-circle-exclamation"),
                    Fact("Last flush", "Unknown", "fa-circle-exclamation")
                ],
                _ => [Fact("Owner documents", "Add to improve accuracy", "fa-circle-exclamation")]
            };
        }

        return missing;
    }

    private static List<SystemActionViewModel> BuildSuggestedActions(SystemDef def, int propiedadId) =>
        def.Id switch
        {
            "hvac" =>
            [
                new() { Title = "Add serial number", Subtitle = "Verify age and warranty", Icon = "fa-pen" },
                new() { Title = "Schedule HVAC tune-up", Subtitle = "Book seasonal maintenance", Icon = "fa-calendar-check" }
            ],
            "water-heater" =>
            [
                new() { Title = "Add serial number", Subtitle = "Verify age and coverage", Icon = "fa-plus-circle" },
                new() { Title = "Set flush reminder", Subtitle = "Stay on track yearly", Icon = "fa-calendar" },
                new() { Title = "Book water heater service", Subtitle = "Keep it running efficiently", Icon = "fa-wrench" }
            ],
            _ =>
            [
                new() { Title = "Upload photo", Subtitle = "Add photos of equipment", Icon = "fa-camera" },
                new() { Title = "Add invoice", Subtitle = "Upload receipts or invoices", Icon = "fa-file-invoice-dollar" }
            ]
        };

    private static List<SystemStatusBadgeViewModel> BuildStatusBadges(SystemProfileItemViewModel system, SystemDef def, int missingCount)
    {
        var badges = new List<SystemStatusBadgeViewModel>
        {
            new() { Label = system.Status, Icon = "fa-shield-check", Tone = system.StatusTone }
        };

        if (missingCount > 0 && def.Id == "hvac")
        {
            badges.Add(new() { Label = "1 reminder", Icon = "fa-triangle-exclamation", Tone = "red" });
        }

        if (def.Id == "water-heater")
        {
            badges.Add(new() { Label = "Yearly flush recommended", Icon = "fa-droplet", Tone = "blue" });
        }

        return badges;
    }

    private static List<SystemFilterChipViewModel> BuildFilters() =>
    [
        new() { Key = "all", Label = "All" },
        new() { Key = "hvac", Label = "HVAC" },
        new() { Key = "water-heater", Label = "Water Heater" },
        new() { Key = "electrical", Label = "Electrical" },
        new() { Key = "plumbing", Label = "Plumbing" },
        new() { Key = "appliances", Label = "Appliances" }
    ];

    private static List<SystemTabViewModel> TabsForSystem(string systemId) => systemId switch
    {
        "hvac" =>
        [
            new() { Key = "overview", Label = "Overview" },
            new() { Key = "service", Label = "Service" },
            new() { Key = "warranty", Label = "Warranty" },
            new() { Key = "documents", Label = "Documents" }
        ],
        "water-heater" =>
        [
            new() { Key = "overview", Label = "Overview" },
            new() { Key = "maintenance", Label = "Maintenance" },
            new() { Key = "warranty", Label = "Warranty" },
            new() { Key = "documents", Label = "Documents" }
        ],
        _ =>
        [
            new() { Key = "overview", Label = "Overview" },
            new() { Key = "warranty", Label = "Warranty" },
            new() { Key = "documents", Label = "Documents" }
        ]
    };

    private static string SubtitleForSystem(string id) => id switch
    {
        "hvac" => "Equipment details, verification, warranty & service history",
        "water-heater" => "Tank details, maintenance, age & protection",
        _ => "System details from saved House Fact data"
    };

    private static string BannerForSystem(string id) => id switch
    {
        "hvac" => "System age should be verified by serial number, permit, inspection report, or invoice.",
        "water-heater" => "Annual flushing helps reduce sediment buildup, improve efficiency, and extend the life of your water heater.",
        _ => "System data may include public records and saved House Fact research. Add documents to improve accuracy."
    };

    private static string NormalizeFilter(string? filter) => filter?.ToLowerInvariant() switch
    {
        "hvac" => "hvac",
        "water-heater" => "water-heater",
        "electrical" => "electrical",
        "plumbing" => "plumbing",
        "appliances" => "appliances",
        _ => "all"
    };

    private static string NormalizeDetailTab(string? tab, string systemId)
    {
        var t = tab?.ToLowerInvariant() ?? "overview";
        if (systemId == "water-heater" && t == "service") return "maintenance";
        return t is "overview" or "service" or "maintenance" or "warranty" or "documents" ? t : "overview";
    }

    private static string MapVerificationStatus(string status) => status switch
    {
        "Confirmed" => "Confirmed",
        "Estimated" => "Estimated",
        "Review" => "Review",
        "Not confirmed" => "Not started",
        _ => "Needed"
    };

    private static string MapVerificationTone(string status) => status switch
    {
        "Confirmed" => "green",
        "Estimated" => "green",
        "Review" => "yellow",
        "Not confirmed" => "gray",
        _ => "orange"
    };

    private static bool IsSystemsSection(AttomFieldGroupViewModel section)
    {
        var key = $"{section.SectionId} {section.Title}".ToLowerInvariant();
        return key.Contains("system") || key.Contains("mechanical") || key.Contains("hvac");
    }

    private static void AddField(Dictionary<string, string> map, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "—") return;
        map[label] = value.Trim();
    }

    private static string? FindValue(List<(string Label, string Value)> fields, params string[] keys)
    {
        foreach (var key in keys)
        {
            var match = fields.FirstOrDefault(f => f.Label.Contains(key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Value) && !IsUnconfirmed(match.Value))
            {
                return match.Value;
            }
        }

        return null;
    }

    private static SystemAttributeViewModel Attr(string label, string value, string tone = "default") =>
        new() { Label = label, Value = value, Tone = tone };

    private static SystemFactViewModel Fact(string label, string value, string icon) =>
        new() { Label = label, Value = value, Icon = icon };

    private static string ToneFor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || IsUnconfirmed(value) || value is "Unknown") return "orange";
        if (value.Contains("add", StringComparison.OrdinalIgnoreCase)) return "blue";
        return "default";
    }

    private static bool NeedsVerification(string value) =>
        value.Contains("verification", StringComparison.OrdinalIgnoreCase)
        || value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("needs owner", StringComparison.OrdinalIgnoreCase);

    private static bool IsUnconfirmed(string? value) =>
        string.IsNullOrWhiteSpace(value) || value == "—"
        || value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("needs verification", StringComparison.OrdinalIgnoreCase)
        || value.Contains("not confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("unknown", StringComparison.OrdinalIgnoreCase);

    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..(max - 1)].TrimEnd() + "…";
}
