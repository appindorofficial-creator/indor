using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class RoofExteriorDisplayService
{
    private sealed record ItemDef(
        string Id,
        string Title,
        string Icon,
        string[] FieldKeys,
        string DefaultSubtitle,
        string? SectionId);

    private static readonly ItemDef[] AllItems =
    [
        new("roof-type", "Roof Type & Age", "fa-house-chimney", ["roof", "shingle", "roof type"], "Asphalt shingles, age not confirmed", "roof"),
        new("exterior-material", "Exterior Material", "fa-border-all", ["exterior", "wall", "brick", "siding"], "Brick", "exterior"),
        new("gutters", "Gutters & Drainage", "fa-water", ["gutter", "drainage", "downspout"], "Standard residential setup", "drainage"),
        new("porch-deck", "Porch / Deck / Patio", "fa-chair", ["porch", "deck", "patio"], "Porch, deck", "porch-deck"),
        new("flood-risk", "Flood Risk", "fa-water", ["flood", "flood zone"], "Not in a known flood zone", "foundation"),
        new("moisture", "Moisture Concerns", "fa-droplet", ["moisture", "mold", "water damage"], "Not publicly mentioned", "exterior"),
        new("foundation-type", "Foundation Type", "fa-layer-group", ["foundation", "crawl", "basement", "slab"], "Crawl space", "foundation"),
        new("crawl-space", "Crawl Space / Slab / Basement", "fa-warehouse", ["crawl", "basement", "slab"], "Crawl space", "foundation"),
        new("paint-finish", "Paint / Finish Condition", "fa-paint-roller", ["paint", "finish", "exterior paint"], "Not publicly confirmed", "exterior"),
        new("drainage-grading", "Drainage / Grading", "fa-road", ["drainage", "grading", "drain"], "No public concern noted", "drainage"),
        new("trees", "Trees Near Structure", "fa-tree", ["tree", "vegetation"], "Not publicly confirmed", "exterior"),
        new("power-wash", "Power Wash Exterior", "fa-spray-can-sparkles", ["power wash", "wash exterior"], "Recommended every 1–2 years", "exterior"),
        new("gutter-cleaning", "Gutter Cleaning", "fa-water", ["gutter clean"], "Recommended twice a year", "drainage")
    ];

    public static RoofExteriorIndexViewModel BuildIndex(Propiedad propiedad, PropertyInfoViewModel? info, string? tab = null)
    {
        var ctx = BuildContext(propiedad, info);
        var activeTab = NormalizeTab(tab);

        return new RoofExteriorIndexViewModel
        {
            PropiedadId = propiedad.Id,
            Address = ctx.Address,
            ActiveTab = activeTab,
            HasData = ctx.HasData,
            Tabs = BuildTabs(),
            SummaryCards = BuildSummaryCards(ctx, activeTab),
            Items = BuildIndexItems(ctx, activeTab),
            InfoBanner = "Some details are estimated or not publicly confirmed. Add documents to improve accuracy."
        };
    }

    public static RoofExteriorSectionViewModel? BuildSection(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        string sectionId,
        string? tab = null,
        IReadOnlyDictionary<string, int>? priorityIds = null)
    {
        var normalized = NormalizeSectionId(sectionId);
        if (normalized == null) return null;

        var ctx = BuildContext(propiedad, info);
        var activeTab = NormalizeTab(tab ?? normalized);

        return normalized switch
        {
            "roof" => BuildRoofSection(ctx, propiedad.Id, priorityIds),
            "exterior" or "drainage" => BuildExteriorSection(ctx, propiedad.Id, activeTab, priorityIds),
            _ => BuildGenericSection(ctx, propiedad.Id, normalized, activeTab)
        };
    }

    public static RoofExteriorCarePlanViewModel BuildCarePlan(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        IReadOnlyDictionary<string, int>? priorityIds = null)
    {
        var ctx = BuildContext(propiedad, info);
        var verification = BuildVerificationItems(ctx);
        var verified = verification.Count(v => v.StatusTone == "green");

        return new RoofExteriorCarePlanViewModel
        {
            PropiedadId = propiedad.Id,
            Address = ctx.Address,
            RecommendedActions = BuildCareActions(priorityIds),
            VerificationItems = verification,
            VerifiedCount = verified,
            TotalCount = verification.Count,
            ProgressPercent = verification.Count == 0 ? 0 : (int)Math.Round(verified * 100.0 / verification.Count),
            ServiceOptions = BuildServiceOptions(priorityIds),
            WhatHappensNext =
            [
                "We remind you before due dates",
                "You can upload new records anytime",
                "House Facts stays updated as you verify details"
            ]
        };
    }

    private sealed class RoofExteriorContext
    {
        public string Address { get; set; } = string.Empty;
        public bool HasData { get; set; }
        public Dictionary<string, string> FieldMap { get; set; } = new();
    }

    private static RoofExteriorContext BuildContext(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var profile = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated",
            propiedad.Direccion ?? info?.FormattedAddress);

        var fieldMap = BuildFieldMap(profile, info);

        return new RoofExteriorContext
        {
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? profile.FormattedAddress ?? "Property address",
            HasData = profile.HasData || info?.PropertyDetails != null,
            FieldMap = fieldMap
        };
    }

    private static Dictionary<string, string> BuildFieldMap(HouseFactProfileViewModel profile, PropertyInfoViewModel? info)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var section in profile.Sections.Where(s => IsRoofExteriorSection(s)))
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
            AddField(map, "Exterior Material", d.WallType);
            AddField(map, "Property Type", d.PropertyType);
        }

        var roofWarranty = info?.HomeWarranties?.Roof;
        if (roofWarranty != null)
        {
            AddField(map, "Roof warranty", roofWarranty.Status ?? roofWarranty.WarrantyProvider);
            AddField(map, "Roof install", roofWarranty.InstallationDate?.ToString("yyyy"));
        }

        return map;
    }

    private static List<RoofExteriorSummaryCardViewModel> BuildSummaryCards(RoofExteriorContext ctx, string activeTab)
    {
        var cards = activeTab switch
        {
            "exterior" => new[]
            {
                Card("Exterior Material", Find(ctx, "exterior", "wall", "brick") ?? "Brick", "fa-border-all"),
                Card("Gutter Cleaning", "Twice a year", "fa-water", "Estimated", "blue"),
                Card("Exterior Paint Review", "Every 5 years", "fa-paint-roller", "Suggested", "blue"),
                Card("Power Wash", "Every 1–2 years", "fa-spray-can-sparkles", "Suggested", "blue")
            },
            "drainage" => new[]
            {
                Card("Gutters & Downspouts", Find(ctx, "gutter") ?? "Standard residential setup", "fa-water", "Estimated", "blue"),
                Card("Drainage / Grading", Find(ctx, "drainage", "grading") ?? "No public concern noted", "fa-road"),
                Card("Gutter Cleaning", "Twice a year", "fa-water", "Estimated", "blue")
            },
            "porch-deck" => new[]
            {
                Card("Porch / Deck", Find(ctx, "porch", "deck", "patio") ?? "Porch, deck", "fa-chair"),
                Card("Exterior Material", Find(ctx, "exterior", "wall") ?? "Brick", "fa-border-all")
            },
            "foundation" => new[]
            {
                Card("Foundation", Find(ctx, "foundation", "crawl") ?? "Crawl space", "fa-layer-group"),
                Card("Flood Risk", Find(ctx, "flood") ?? "Not in a flood zone", "fa-water"),
                Card("Crawl / Slab / Basement", Find(ctx, "crawl", "basement", "slab") ?? "Crawl space", "fa-warehouse")
            },
            _ => new[]
            {
                Card("Roof Type", Find(ctx, "roof", "shingle") ?? "Asphalt shingles", "fa-house-chimney"),
                Card("Exterior Material", Find(ctx, "exterior", "wall", "brick") ?? "Brick", "fa-border-all"),
                Card("Foundation", Find(ctx, "foundation", "crawl") ?? "Crawl space", "fa-layer-group"),
                Card("Flood Risk", Find(ctx, "flood") ?? "Not in a flood zone", "fa-water")
            }
        };

        return cards.Select(c => ApplyCardStatus(c, ctx)).ToList();
    }

    private static List<RoofExteriorListItemViewModel> BuildIndexItems(RoofExteriorContext ctx, string activeTab)
    {
        var items = AllItems
            .Where(i => TabMatchesItem(activeTab, i))
            .Select(i => BuildListItem(i, ctx))
            .ToList();

        if (items.Count == 0)
        {
            items = AllItems.Select(i => BuildListItem(i, ctx)).Take(8).ToList();
        }

        return items;
    }

    private static RoofExteriorSectionViewModel BuildRoofSection(
        RoofExteriorContext ctx,
        int propiedadId,
        IReadOnlyDictionary<string, int>? priorityIds)
    {
        var roofType = Find(ctx, "roof", "shingle", "roof type") ?? "Asphalt shingles";
        var age = Find(ctx, "roof age", "age") ?? "Not confirmed";

        return new RoofExteriorSectionViewModel
        {
            PropiedadId = propiedadId,
            SectionId = "roof",
            Name = "Roof Details",
            PageSubtitle = "Track roof condition, documents, and recommended care.",
            Address = ctx.Address,
            ActiveTab = "roof",
            Tabs = BuildTabs(),
            HealthBadges =
            [
                Badge("Every 1–2 years", "fa-calendar", "blue"),
                Badge("Documents needed", "fa-file-lines", "orange"),
                Badge("Last check not confirmed", "fa-clock", "gray")
            ],
            InfoBanner = "Routine roof inspections help catch damaged shingles, worn flashing, and sealant issues before they become leaks.",
            Rows =
            [
                Row("Roof Type", roofType, "fa-house-chimney", ResolveFieldStatus(roofType)),
                Row("Approximate Age", age, "fa-calendar", ResolveFieldStatus(age)),
                Row("Last Roof Inspection", Find(ctx, "inspection", "last roof") ?? "Not recorded", "fa-magnifying-glass", "Needs verification", "orange"),
                Row("Flashing & Sealants", "Review around vents, valleys, chimneys", "fa-shield-halved", "Confirmed", "green"),
                Row("Visible Leak History", Find(ctx, "leak") ?? "Not publicly mentioned", "fa-droplet", "Confirmed", "green"),
                Row("Warranty / Install Records", Find(ctx, "warranty", "install") ?? "Missing documents", "fa-file-contract", "Needs attention", "red"),
                Row("Roof Permits", Find(ctx, "permit", "roof permit") ?? "Permit history not publicly confirmed", "fa-file-lines", "Needs verification", "orange")
            ],
            Reminders =
            [
                "Inspect after major storms",
                "Check attic for stains or moisture",
                "Schedule routine inspection every 1–2 years"
            ],
            SuggestedActions =
            [
                Action("Set roof reminder", "Stay on track yearly", "fa-bell"),
                Action("Schedule roof check", "Book seasonal inspection", "fa-calendar-check",
                    PriorityUrl(priorityIds, "Roof inspection", "RoofInspection", "RoofInspectionService"))
            ]
        };
    }

    private static RoofExteriorSectionViewModel BuildExteriorSection(
        RoofExteriorContext ctx,
        int propiedadId,
        string activeTab,
        IReadOnlyDictionary<string, int>? priorityIds)
    {
        return new RoofExteriorSectionViewModel
        {
            PropiedadId = propiedadId,
            SectionId = activeTab,
            Name = activeTab == "drainage" ? "Drainage" : "Exterior & Drainage",
            PageSubtitle = "Understand siding, moisture, gutters, and site drainage.",
            Address = ctx.Address,
            ActiveTab = activeTab,
            Tabs = BuildTabs(),
            SummaryCards = BuildSummaryCards(ctx, activeTab == "drainage" ? "drainage" : "exterior"),
            Rows = BuildExteriorRows(ctx, activeTab),
            WhyItMattersTitle = "Why this matters",
            WhyItMattersText = "Regular exterior reviews and proper drainage help prevent water damage, staining, mold growth, and costly foundation issues.",
            SuggestedActions =
            [
                Action("Set gutter reminder", "Stay on top of cleanings", "fa-calendar",
                    PriorityUrl(priorityIds, "Gutter cleaning", "GutterCleaning", "GutterCleaningService")),
                Action("Set exterior wash reminder", "Keep it clean & protected", "fa-droplet",
                    PriorityUrl(priorityIds, "Power wash exterior", "PowerWash", "PowerWashService")),
                Action("Add exterior photos", "Track condition over time", "fa-camera")
            ]
        };
    }

    private static RoofExteriorSectionViewModel BuildGenericSection(
        RoofExteriorContext ctx,
        int propiedadId,
        string sectionId,
        string activeTab)
    {
        var name = sectionId switch
        {
            "porch-deck" => "Porch / Deck / Patio",
            "foundation" => "Foundation & Structure",
            _ => "Roof & Exterior"
        };

        return new RoofExteriorSectionViewModel
        {
            PropiedadId = propiedadId,
            SectionId = sectionId,
            Name = name,
            PageSubtitle = "See key exterior and structural information in one place.",
            Address = ctx.Address,
            ActiveTab = activeTab,
            Tabs = BuildTabs(),
            SummaryCards = BuildSummaryCards(ctx, sectionId),
            Rows = AllItems
                .Where(i => TabMatchesItem(sectionId, i))
                .Select(i =>
                {
                    var value = FindForItem(ctx, i);
                    var status = ResolveFieldStatus(value);
                    return Row(i.Title, value, i.Icon, status.Label, status.Tone);
                })
                .ToList(),
            InfoBanner = "Some details are estimated or not publicly confirmed. Add documents to improve accuracy."
        };
    }

    private static List<RoofExteriorDetailRowViewModel> BuildExteriorRows(RoofExteriorContext ctx, string activeTab)
    {
        var ids = activeTab == "drainage"
            ? new[] { "gutters", "drainage-grading", "gutter-cleaning", "flood-risk" }
            : new[] { "exterior-material", "paint-finish", "gutters", "moisture", "trees", "power-wash", "gutter-cleaning" };

        return AllItems
            .Where(i => ids.Contains(i.Id))
            .Select(i =>
            {
                var value = FindForItem(ctx, i);
                var status = ResolveFieldStatus(value);
                if (i.Id == "gutters" || i.Id == "gutter-cleaning")
                {
                    status = ("Estimated", "blue");
                }

                return Row(i.Title, value, i.Icon, status.Label, status.Tone);
            })
            .ToList();
    }

    private static List<RoofExteriorCareActionViewModel> BuildCareActions(IReadOnlyDictionary<string, int>? priorityIds) =>
    [
        CareAction("Roof Inspection", "Every 1–2 years", "fa-house-chimney", "Reminder set", "green",
            PriorityUrl(priorityIds, "Roof inspection", "RoofInspection", "RoofInspectionService")),
        CareAction("Gutter Cleaning", "Twice a year", "fa-water", "Needs setup", "orange",
            PriorityUrl(priorityIds, "Gutter cleaning", "GutterCleaning", "GutterCleaningService")),
        CareAction("Exterior Power Wash", "Every 1–2 years", "fa-spray-can-sparkles", "Suggested", "blue",
            PriorityUrl(priorityIds, "Power wash exterior", "PowerWash", "PowerWashService")),
        CareAction("Exterior Paint Review", "Every 5 years", "fa-paint-roller", "Suggested", "blue",
            PriorityUrl(priorityIds, "Exterior paint", "ExteriorPaint", "ExteriorPaintReview"))
    ];

    private static List<RoofExteriorVerificationItemViewModel> BuildVerificationItems(RoofExteriorContext ctx)
    {
        var hasRoofReport = HasConfirmedValue(ctx, "inspection", "roof report");
        var hasPermits = HasConfirmedValue(ctx, "permit");
        var hasPhotos = false;

        return
        [
            Verify("Upload roof report", "Add inspection or condition reports", "fa-file-arrow-up",
                hasRoofReport ? "Completed" : "Needs upload", hasRoofReport ? "green" : "orange"),
            Verify("Add permit records", "Include permits & approvals", "fa-file-contract",
                hasPermits ? "Completed" : "Needs upload", hasPermits ? "green" : "orange"),
            Verify("Add exterior photos", "Help support and update your facts", "fa-camera",
                hasPhotos ? "Completed" : "Suggested", hasPhotos ? "green" : "blue")
        ];
    }

    private static List<RoofExteriorServiceOptionViewModel> BuildServiceOptions(IReadOnlyDictionary<string, int>? priorityIds) =>
    [
        Service("Request roof inspection", "Find local pros", "fa-hard-hat",
            PriorityUrl(priorityIds, "Roof inspection", "RoofInspection", "RoofInspectionService")),
        Service("Schedule gutter cleaning", "Trusted local service", "fa-filter",
            PriorityUrl(priorityIds, "Gutter cleaning", "GutterCleaning", "GutterCleaningService")),
        Service("Book exterior review", "Inspection + advice", "fa-house-circle-check",
            PriorityUrl(priorityIds, "Exterior paint", "ExteriorPaint", "ExteriorPaintReview"))
    ];

    private static List<RoofExteriorTabViewModel> BuildTabs() =>
    [
        new() { Key = "roof", Label = "Roof", Icon = "fa-house-chimney" },
        new() { Key = "exterior", Label = "Exterior", Icon = "fa-border-all" },
        new() { Key = "drainage", Label = "Drainage", Icon = "fa-water" },
        new() { Key = "porch-deck", Label = "Porch/Deck", Icon = "fa-chair" },
        new() { Key = "foundation", Label = "Foundation", Icon = "fa-layer-group" }
    ];

    private static RoofExteriorListItemViewModel BuildListItem(ItemDef def, RoofExteriorContext ctx)
    {
        var value = FindForItem(ctx, def);
        var status = ResolveFieldStatus(value);
        if (def.Id is "gutters" or "gutter-cleaning")
        {
            status = ("Estimated", "blue");
        }

        return new RoofExteriorListItemViewModel
        {
            Id = def.Id,
            Title = def.Title,
            Subtitle = value,
            Icon = def.Icon,
            Status = status.Label,
            StatusTone = status.Tone,
            SectionId = def.SectionId
        };
    }

    private static bool TabMatchesItem(string tab, ItemDef item) => tab switch
    {
        "roof" => item.SectionId == "roof",
        "exterior" => item.SectionId is "exterior" or "drainage",
        "drainage" => item.SectionId == "drainage" || item.Id.Contains("gutter") || item.Id.Contains("drainage"),
        "porch-deck" => item.SectionId == "porch-deck",
        "foundation" => item.SectionId == "foundation",
        _ => true
    };

    private static RoofExteriorSummaryCardViewModel ApplyCardStatus(RoofExteriorSummaryCardViewModel card, RoofExteriorContext ctx)
    {
        if (card.StatusTone is "blue" or "gray") return card;
        var status = ResolveFieldStatus(card.Value);
        card.Status = status.Label;
        card.StatusTone = status.Tone;
        return card;
    }

    private static string FindForItem(RoofExteriorContext ctx, ItemDef def)
    {
        var value = Find(ctx, def.FieldKeys);
        return string.IsNullOrWhiteSpace(value) ? def.DefaultSubtitle : value;
    }

    private static string? Find(RoofExteriorContext ctx, params string[] keys)
    {
        foreach (var key in keys)
        {
            var match = ctx.FieldMap.FirstOrDefault(kv => kv.Key.Contains(key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Value) && !IsUnconfirmed(match.Value))
            {
                return match.Value;
            }
        }

        return null;
    }

    private static bool HasConfirmedValue(RoofExteriorContext ctx, params string[] keys) =>
        keys.Any(k => ctx.FieldMap.Any(f =>
            f.Key.Contains(k, StringComparison.OrdinalIgnoreCase) && !IsUnconfirmed(f.Value)));

    private static (string Label, string Tone) ResolveFieldStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || IsUnconfirmed(value))
        {
            if (value?.Contains("missing", StringComparison.OrdinalIgnoreCase) == true)
            {
                return ("Needs attention", "red");
            }

            return ("Needs verification", "orange");
        }

        if (value.Contains("not publicly mentioned", StringComparison.OrdinalIgnoreCase))
        {
            return ("Confirmed", "green");
        }

        return ("Confirmed", "green");
    }

    private static bool IsRoofExteriorSection(AttomFieldGroupViewModel section)
    {
        var key = $"{section.SectionId} {section.Title} {section.CategoryKey}".ToLowerInvariant();
        return key.Contains("roof") || key.Contains("exterior") || key.Contains("site")
            || key.Contains("foundation") || key.Contains("structure");
    }

    private static void AddField(Dictionary<string, string> map, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "—") return;
        map[label] = value.Trim();
    }

    private static string NormalizeTab(string? tab) => tab?.ToLowerInvariant() switch
    {
        "exterior" => "exterior",
        "drainage" => "drainage",
        "porch-deck" => "porch-deck",
        "foundation" => "foundation",
        _ => "roof"
    };

    private static string? NormalizeSectionId(string sectionId) => sectionId.ToLowerInvariant() switch
    {
        "roof" => "roof",
        "exterior" => "exterior",
        "drainage" => "drainage",
        "porch-deck" => "porch-deck",
        "foundation" => "foundation",
        _ => null
    };

    private static bool IsUnconfirmed(string? value) =>
        string.IsNullOrWhiteSpace(value) || value == "—"
        || value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("not confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("needs verification", StringComparison.OrdinalIgnoreCase)
        || value.Contains("not recorded", StringComparison.OrdinalIgnoreCase)
        || value.Contains("unknown", StringComparison.OrdinalIgnoreCase)
        || value.Contains("not publicly mentioned", StringComparison.OrdinalIgnoreCase);

    private static RoofExteriorSummaryCardViewModel Card(
        string label,
        string value,
        string icon,
        string status = "Confirmed",
        string tone = "green") =>
        new() { Label = label, Value = value, Icon = icon, Status = status, StatusTone = tone };

    private static RoofExteriorSummaryCardViewModel Badge(string label, string icon, string tone) =>
        new() { Label = label, Value = string.Empty, Icon = icon, Status = label, StatusTone = tone };

    private static RoofExteriorDetailRowViewModel Row(
        string title,
        string subtitle,
        string icon,
        (string Label, string Tone) status) =>
        Row(title, subtitle, icon, status.Label, status.Tone);

    private static RoofExteriorDetailRowViewModel Row(
        string title,
        string subtitle,
        string icon,
        string status,
        string tone) =>
        new() { Title = title, Subtitle = subtitle, Icon = icon, Status = status, StatusTone = tone };

    private static RoofExteriorActionViewModel Action(
        string title,
        string subtitle,
        string icon,
        string? url = null) =>
        new() { Title = title, Subtitle = subtitle, Icon = icon, Url = url };

    private static RoofExteriorCareActionViewModel CareAction(
        string title,
        string frequency,
        string icon,
        string status,
        string tone,
        string? url = null) =>
        new() { Title = title, Frequency = frequency, Icon = icon, Status = status, StatusTone = tone, Url = url };

    private static RoofExteriorVerificationItemViewModel Verify(
        string title,
        string subtitle,
        string icon,
        string status,
        string tone) =>
        new() { Title = title, Subtitle = subtitle, Icon = icon, Status = status, StatusTone = tone };

    private static RoofExteriorServiceOptionViewModel Service(
        string title,
        string subtitle,
        string icon,
        string? url = null) =>
        new() { Title = title, Subtitle = subtitle, Icon = icon, Url = url };

    private static string? PriorityUrl(
        IReadOnlyDictionary<string, int>? priorityIds,
        string name,
        string controller,
        string action)
    {
        if (priorityIds != null && priorityIds.TryGetValue(name, out var id))
        {
            return $"/{controller}/{action}/{id}";
        }

        return null;
    }
}
