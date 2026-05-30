using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class PermitsImprovementsDisplayService
{
    private sealed record PermitDef(
        string Id,
        string Name,
        string Icon,
        string[] FieldKeys,
        string DefaultSubtitle,
        string MissingDocLabel);

    private sealed record ImprovementDef(
        string Id,
        string Name,
        string Icon,
        string[] FieldKeys,
        string DefaultSubtitle);

    private static readonly PermitDef[] PermitDefs =
    [
        new("hvac", "HVAC Permits", "fa-fan", ["hvac permit", "hvac"], "Permit history not publicly confirmed", "HVAC permit or install invoice"),
        new("roof", "Roof Permits", "fa-house-chimney", ["roof permit", "roof"], "Permit history not publicly confirmed", "Roof permit / warranty"),
        new("water-heater", "Water Heater Permits", "fa-fire-burner", ["water heater permit", "water heater"], "Permit history not publicly confirmed", "Water heater install date"),
        new("electrical", "Electrical Permits", "fa-bolt", ["electrical permit", "electrical"], "Permit history not publicly confirmed", "Electrical permit history"),
        new("plumbing", "Plumbing Permits", "fa-faucet", ["plumbing permit", "plumbing"], "Permit history not publicly confirmed", "Plumbing update records"),
        new("deck-porch", "Deck / Porch Addition", "fa-chair", ["deck", "porch", "addition"], "Permit history not publicly confirmed", "Deck / porch addition permit"),
        new("remodel", "Remodel / Structural", "fa-hammer", ["remodel", "structural"], "Permit history not publicly confirmed", "Remodel / structural permits")
    ];

    private static readonly ImprovementDef[] ImprovementDefs =
    [
        new("hvac", "HVAC Replacement", "fa-fan", ["hvac replacement", "hvac install"], "Install date not publicly confirmed"),
        new("water-heater", "Water Heater Install", "fa-fire-burner", ["water heater install"], "Age and invoice not publicly confirmed"),
        new("deck-porch", "Deck / Porch Update", "fa-chair", ["deck", "porch update"], "Public mention not confirmed"),
        new("roof", "Roof Replacement", "fa-house-chimney", ["roof replacement", "roof install"], "Warranty documents not uploaded")
    ];

    public static PermitsImprovementsIndexViewModel BuildIndex(Propiedad propiedad, PropertyInfoViewModel? info, string? tab = null)
    {
        var ctx = BuildContext(propiedad, info);
        var activeTab = NormalizeTab(tab);
        var permitItems = PermitDefs.Select(d => BuildPermitItem(d, ctx)).ToList();
        var verified = permitItems.Count(i => i.StatusTone == "green");
        var needsReview = permitItems.Count(i => i.StatusTone is "orange" or "red");

        var model = new PermitsImprovementsIndexViewModel
        {
            PropiedadId = propiedad.Id,
            Address = ctx.Address,
            ActiveTab = activeTab,
            HasData = ctx.HasData,
            PageSubtitle = SubtitleForTab(activeTab),
            InfoBanner = BannerForTab(activeTab)
        };

        switch (activeTab)
        {
            case "improvements":
                model.Items = ImprovementDefs.Select(d => BuildImprovementItem(d, ctx)).ToList();
                model.Stats =
                [
                    Stat("Improvement categories", model.Items.Count.ToString(), "fa-wrench"),
                    Stat("Verified", model.Items.Count(i => i.StatusTone == "green").ToString(), "fa-shield-check"),
                    Stat("Estimated", model.Items.Count(i => i.StatusTone == "orange").ToString(), "fa-triangle-exclamation"),
                    Stat("Documents", "8", "fa-folder-open")
                ];
                model.RecordedNotes =
                [
                    Note("Last public record check", "No recent permit match", "fa-clock"),
                    Note("Seller-provided updates", "Not uploaded", "fa-users")
                ];
                break;

            case "missing-docs":
                model.NeededItems = PermitDefs.Select(d => new PermitsListItemViewModel
                {
                    Id = d.Id,
                    Title = d.MissingDocLabel,
                    Subtitle = string.Empty,
                    Icon = "fa-triangle-exclamation",
                    Status = "Needed",
                    StatusTone = "orange"
                }).ToList();
                model.HelpfulItems =
                [
                    new() { Id = "inspection", Title = "Inspection report", Icon = "fa-file-lines", Status = "Recommended", StatusTone = "blue" },
                    new() { Id = "disclosure", Title = "Seller disclosure", Icon = "fa-file-lines", Status = "Recommended", StatusTone = "blue" }
                ];
                model.MissingDocsBadge = $"{needsReview} items need review";
                model.Stats = [];
                break;

            default:
                model.Items = permitItems;
                model.Stats =
                [
                    Stat("Permit types", permitItems.Count.ToString(), "fa-file-lines"),
                    Stat("Verified", verified.ToString(), "fa-shield-check"),
                    Stat("Needs review", needsReview.ToString(), "fa-triangle-exclamation"),
                    Stat("Documents saved", "8", "fa-folder-open")
                ];
                break;
        }

        return model;
    }

    public static PermitsDetailViewModel? BuildDetail(Propiedad propiedad, PropertyInfoViewModel? info, string permitId, string? tab = null)
    {
        var def = PermitDefs.FirstOrDefault(d => string.Equals(d.Id, permitId, StringComparison.OrdinalIgnoreCase));
        if (def == null) return null;

        var ctx = BuildContext(propiedad, info);
        var fields = ExtractFields(ctx, def.FieldKeys);
        var activeTab = NormalizeDetailTab(tab);
        var permitStatus = ResolvePermitStatus(fields, def);
        var roofType = Find(ctx, "roof type", "roof", "shingle") ?? "Asphalt shingles";
        var lastPermit = Find(ctx, "last permit", "permit date", "install") ?? "Not publicly confirmed";
        var warranty = info?.HomeWarranties?.Roof?.Status ?? Find(ctx, "warranty") ?? "Unknown";

        return new PermitsDetailViewModel
        {
            PropiedadId = propiedad.Id,
            PermitId = def.Id,
            Name = def.Id == "roof" ? "Roof Permits" : def.Name,
            PageSubtitle = def.Id == "roof"
                ? "Permit status, roof work history, and verification details."
                : $"Permit status and verification details for {def.Name.ToLowerInvariant()}.",
            Address = ctx.Address,
            ActiveTab = activeTab,
            SummaryCards = def.Id == "roof"
                ?
                [
                    Summary("Permit status", permitStatus.Label, "fa-triangle-exclamation", permitStatus.Tone),
                    Summary("Roof type", roofType, "fa-house-chimney", "default"),
                    Summary("Last permit", lastPermit, "fa-calendar", ToneForValue(lastPermit)),
                    Summary("Warranty", warranty, "fa-shield-halved", ToneForValue(warranty))
                ]
                : BuildGenericSummary(def, fields, permitStatus),
            VerificationItems = BuildVerificationChecklist(ctx, def),
            RecordItems = BuildPublicRecords(ctx),
            InfoBanner = def.Id == "roof"
                ? "Roof permit history may be incomplete. Upload documents to improve confidence."
                : "Permit information may be estimated. Upload documents to improve accuracy.",
            HistoryNote = activeTab == "history"
                ? "No permit history records saved yet in your House Fact profile."
                : null,
            DocumentsNote = activeTab == "docs"
                ? "Upload permits, invoices, or inspection reports to improve verification status."
                : null
        };
    }

    private sealed class PermitsContext
    {
        public string Address { get; set; } = string.Empty;
        public bool HasData { get; set; }
        public Dictionary<string, string> FieldMap { get; set; } = new();
    }

    private static PermitsContext BuildContext(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var profile = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated",
            propiedad.Direccion ?? info?.FormattedAddress);

        return new PermitsContext
        {
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? profile.FormattedAddress ?? "Property address",
            HasData = profile.HasData || info?.PropertyDetails != null,
            FieldMap = BuildFieldMap(profile, info)
        };
    }

    private static Dictionary<string, string> BuildFieldMap(HouseFactProfileViewModel profile, PropertyInfoViewModel? info)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var section in profile.Sections.Where(s => IsPermitsSection(s)))
        {
            foreach (var field in section.Fields)
            {
                AddField(map, field.Label, field.Value);
            }

            foreach (var item in section.ChecklistItems)
            {
                AddField(map, item.Item, item.Status);
            }

            if (!string.IsNullOrWhiteSpace(section.Paragraph))
            {
                AddField(map, section.DisplayTitle, section.Paragraph);
            }
        }

        var roofWarranty = info?.HomeWarranties?.Roof;
        if (roofWarranty != null)
        {
            AddField(map, "Roof warranty", roofWarranty.Status ?? roofWarranty.WarrantyProvider);
            AddField(map, "Roof install", roofWarranty.InstallationDate?.ToString("yyyy"));
        }

        return map;
    }

    private static PermitsListItemViewModel BuildPermitItem(PermitDef def, PermitsContext ctx)
    {
        var fields = ExtractFields(ctx, def.FieldKeys);
        var subtitle = fields.FirstOrDefault().Value ?? def.DefaultSubtitle;
        var status = ResolvePermitStatus(fields, def);

        return new PermitsListItemViewModel
        {
            Id = def.Id,
            Title = def.Name,
            Subtitle = subtitle,
            Icon = def.Icon,
            Status = status.Label,
            StatusTone = status.Tone,
            LinkToDetail = true
        };
    }

    private static PermitsListItemViewModel BuildImprovementItem(ImprovementDef def, PermitsContext ctx)
    {
        var value = Find(ctx, def.FieldKeys) ?? def.DefaultSubtitle;
        var tone = def.Id == "roof" && value.Contains("not uploaded", StringComparison.OrdinalIgnoreCase)
            ? "red"
            : "orange";
        var status = tone == "red" ? "Needs docs" : "Estimated";

        return new PermitsListItemViewModel
        {
            Id = def.Id,
            Title = def.Name,
            Subtitle = value,
            Icon = def.Icon,
            Status = status,
            StatusTone = tone,
            LinkToDetail = def.Id == "roof"
        };
    }

    private static List<(string Key, string Value)> ExtractFields(PermitsContext ctx, string[] keys)
    {
        var results = new List<(string, string)>();
        foreach (var kv in ctx.FieldMap)
        {
            if (keys.Any(k => kv.Key.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                results.Add((kv.Key, kv.Value));
            }
        }

        return results;
    }

    private static (string Label, string Tone) ResolvePermitStatus(List<(string Key, string Value)> fields, PermitDef def)
    {
        if (fields.Any(f => !IsUnconfirmed(f.Value) && !NeedsVerification(f.Value)))
        {
            return ("Confirmed", "green");
        }

        return ("Needs verification", "orange");
    }

    private static List<PermitsSummaryCardViewModel> BuildGenericSummary(
        PermitDef def,
        List<(string Key, string Value)> fields,
        (string Label, string Tone) status) =>
    [
        Summary("Permit status", status.Label, "fa-triangle-exclamation", status.Tone),
        Summary("Public record", fields.FirstOrDefault().Value ?? "Not publicly confirmed", "fa-file-lines", "gray"),
        Summary("Last update", FindField(fields, "date", "year") ?? "Unknown", "fa-calendar", "gray"),
        Summary("Documents", "Not uploaded", "fa-folder-open", "gray")
    ];

    private static List<PermitsDetailRowViewModel> BuildVerificationChecklist(PermitsContext ctx, PermitDef def)
    {
        var hasPermit = HasValue(ctx, "permit number", "permit #");
        var hasYear = HasValue(ctx, "install year", "year", "install date");
        var hasContractor = HasValue(ctx, "contractor");
        var hasInspection = HasValue(ctx, "final inspection", "inspection");
        var hasWarranty = HasValue(ctx, "warranty");

        return
        [
            Row("Permit number", "fa-hashtag", hasPermit ? "Added" : "Needed", hasPermit ? "green" : "orange"),
            Row("Install year", "fa-calendar", hasYear ? "Added" : "Needed", hasYear ? "green" : "orange"),
            Row("Contractor", "fa-user-hard-hat", hasContractor ? "Added" : "Needed", hasContractor ? "green" : "orange"),
            Row("Final inspection", "fa-clipboard-check", hasInspection ? "Added" : "Not added", hasInspection ? "green" : "orange"),
            Row("Warranty details", "fa-shield-halved", hasWarranty ? "Added" : "Not added", hasWarranty ? "green" : "orange")
        ];
    }

    private static List<PermitsDetailRowViewModel> BuildPublicRecords(PermitsContext ctx)
    {
        var countyMatch = HasValue(ctx, "county permit", "permit search", "public match");
        var sellerDocs = HasValue(ctx, "seller document", "seller disclosure");
        var inspection = HasValue(ctx, "inspection report");

        return
        [
            Row("County permit search", "fa-building-columns", countyMatch ? "Match found" : "No public match found", countyMatch ? "green" : "gray"),
            Row("Seller documents", "fa-file-contract", sellerDocs ? "Uploaded" : "Not uploaded", sellerDocs ? "green" : "gray"),
            Row("Inspection report", "fa-file-lines", inspection ? "Uploaded" : "Not uploaded", inspection ? "green" : "gray")
        ];
    }

    private static bool IsPermitsSection(AttomFieldGroupViewModel section)
    {
        var key = $"{section.SectionId} {section.Title} {section.CategoryKey}".ToLowerInvariant();
        return key.Contains("permit") || key.Contains("improvement");
    }

    private static void AddField(Dictionary<string, string> map, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "—") return;
        map[label] = value.Trim();
    }

    private static string? Find(PermitsContext ctx, params string[] keys)
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

    private static string? FindField(List<(string Key, string Value)> fields, params string[] keys)
    {
        foreach (var key in keys)
        {
            var match = fields.FirstOrDefault(f => f.Key.Contains(key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Value))
            {
                return match.Value;
            }
        }

        return null;
    }

    private static bool HasValue(PermitsContext ctx, params string[] keys) =>
        keys.Any(k => ctx.FieldMap.Any(f =>
            f.Key.Contains(k, StringComparison.OrdinalIgnoreCase) && !IsUnconfirmed(f.Value)));

    private static string NormalizeTab(string? tab) => tab?.ToLowerInvariant() switch
    {
        "improvements" => "improvements",
        "missing-docs" or "missing" => "missing-docs",
        _ => "permits"
    };

    private static string NormalizeDetailTab(string? tab)
    {
        var t = tab?.ToLowerInvariant() ?? "overview";
        return t is "overview" or "history" or "docs" ? t : "overview";
    }

    private static string SubtitleForTab(string tab) => tab switch
    {
        "improvements" => "Recorded upgrades and work history for this home.",
        "missing-docs" => "Documents needed to improve permit and upgrade accuracy.",
        _ => "Permit history, upgrades, and verification status for this home."
    };

    private static string BannerForTab(string tab) => tab switch
    {
        "improvements" => "Improvement history gets stronger as invoices, permits, warranties, and inspection reports are added.",
        "missing-docs" => "Adding documents helps confirm dates, contractors, warranties, and recorded improvements.",
        _ => "Permit information may be estimated. You can upload documents to improve accuracy."
    };

    private static bool IsUnconfirmed(string? value) =>
        string.IsNullOrWhiteSpace(value) || value == "—"
        || value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("not confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("unknown", StringComparison.OrdinalIgnoreCase);

    private static bool NeedsVerification(string value) =>
        value.Contains("needs verification", StringComparison.OrdinalIgnoreCase)
        || value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase);

    private static string ToneForValue(string? value) =>
        IsUnconfirmed(value) ? "orange" : "default";

    private static PermitsStatViewModel Stat(string label, string value, string icon) =>
        new() { Label = label, Value = value, Icon = icon };

    private static PermitsNoteCardViewModel Note(string title, string subtitle, string icon) =>
        new() { Title = title, Subtitle = subtitle, Icon = icon };

    private static PermitsSummaryCardViewModel Summary(string label, string value, string icon, string tone) =>
        new() { Label = label, Value = value, Icon = icon, StatusTone = tone };

    private static PermitsDetailRowViewModel Row(string title, string icon, string status, string tone) =>
        new() { Title = title, Icon = icon, Status = status, StatusTone = tone };
}
