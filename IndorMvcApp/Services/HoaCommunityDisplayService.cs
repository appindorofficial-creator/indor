using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class HoaCommunityDisplayService
{
    private static readonly HoaTabViewModel[] TabDefs =
    [
        new() { Key = "overview", Label = "Overview", Step = 1 },
        new() { Key = "fees", Label = "Fees", Step = 2 },
        new() { Key = "rules", Label = "Rules", Step = 3 },
        new() { Key = "amenities", Label = "Amenities", Step = 4 },
        new() { Key = "contacts", Label = "Contacts", Step = 5 }
    ];

    public static HoaCommunityViewModel Build(Propiedad propiedad, PropertyInfoViewModel? info, string? tab = null)
    {
        var ctx = BuildContext(propiedad, info);
        var activeTab = NormalizeTab(tab);
        var tabDef = TabDefs.First(t => t.Key == activeTab);

        var model = new HoaCommunityViewModel
        {
            PropiedadId = propiedad.Id,
            Address = ctx.Address,
            ActiveTab = activeTab,
            CurrentStep = tabDef.Step,
            Tabs = TabDefs.ToList(),
            HasData = ctx.HasData,
            HoaStatus = ctx.HoaStatus,
            HoaStatusTone = ctx.HoaStatusTone,
            HoaName = ctx.HoaName,
            EstimatedFee = ctx.EstimatedFee,
            Confidence = ctx.Confidence,
            ConfidenceTone = ctx.ConfidenceTone,
            PageTitle = TitleForTab(activeTab),
            PageSubtitle = SubtitleForTab(activeTab),
            InfoBanner = BannerForTab(activeTab),
            PrimaryActionLabel = PrimaryActionForTab(activeTab),
            SecondaryActionLabel = SecondaryActionForTab(activeTab),
            PrimaryActionIcon = PrimaryIconForTab(activeTab),
            SecondaryActionIcon = SecondaryIconForTab(activeTab)
        };

        switch (activeTab)
        {
            case "fees":
                model.Rows = BuildFeeRows(ctx);
                break;
            case "rules":
                model.Rows = BuildRuleRows(ctx);
                break;
            case "amenities":
                model.Amenities = BuildAmenities(ctx);
                break;
            case "contacts":
                model.Management = BuildManagement(ctx);
                model.Documents = BuildDocuments();
                break;
            default:
                model.Rows = BuildOverviewRows(ctx);
                break;
        }

        return model;
    }

    private sealed class HoaContext
    {
        public string Address { get; set; } = string.Empty;
        public bool HasData { get; set; }
        public Dictionary<string, string> FieldMap { get; set; } = new();
        public string HoaStatus { get; set; } = "Active";
        public string HoaStatusTone { get; set; } = "green";
        public string HoaName { get; set; } = "Not publicly confirmed";
        public string EstimatedFee { get; set; } = "Not publicly confirmed";
        public string Confidence { get; set; } = "Estimated";
        public string ConfidenceTone { get; set; } = "green";
    }

    private static HoaContext BuildContext(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var profile = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated",
            propiedad.Direccion ?? info?.FormattedAddress);

        var fieldMap = BuildFieldMap(profile);
        var hoaName = Find(fieldMap, "hoa name", "association", "community name") ?? "Not publicly confirmed";
        var hoaFee = Find(fieldMap, "hoa fee", "fee", "dues", "annual") ?? "Not publicly confirmed";
        var hoaStatus = ResolveHoaStatus(fieldMap, hoaName);

        return new HoaContext
        {
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? profile.FormattedAddress ?? "Property address",
            HasData = profile.HasData || fieldMap.Count > 0,
            FieldMap = fieldMap,
            HoaName = FormatHoaName(hoaName),
            EstimatedFee = FormatFee(hoaFee),
            HoaStatus = hoaStatus.Label,
            HoaStatusTone = hoaStatus.Tone,
            Confidence = "Estimated",
            ConfidenceTone = "green"
        };
    }

    private static Dictionary<string, string> BuildFieldMap(HouseFactProfileViewModel profile)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var section in profile.Sections.Where(s => IsHoaSection(s)))
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

        foreach (var section in profile.Sections)
        {
            foreach (var field in section.Fields.Where(f =>
                f.Label.Contains("hoa", StringComparison.OrdinalIgnoreCase)
                || f.Label.Contains("association", StringComparison.OrdinalIgnoreCase)))
            {
                AddField(map, field.Label, field.Value);
            }
        }

        return map;
    }

    private static List<HoaRowViewModel> BuildOverviewRows(HoaContext ctx) =>
    [
        Row("Management Company", Find(ctx.FieldMap, "management", "manager") ?? "FirstService Residential", "fa-building",
            "Estimated", "blue"),
        Row("Payment Frequency", Find(ctx.FieldMap, "frequency", "billing", "payment frequency") ?? "Annual", "fa-calendar"),
        Row("Community Type", Find(ctx.FieldMap, "community type", "property type") ?? "Single-Family Homes", "fa-people-group")
    ];

    private static List<HoaRowViewModel> BuildFeeRows(HoaContext ctx)
    {
        var fee = ctx.EstimatedFee == "Not publicly confirmed" ? "$200 / year" : ctx.EstimatedFee;
        var transfer = Find(ctx.FieldMap, "transfer") ?? "Needs verification";
        var late = Find(ctx.FieldMap, "late fee", "late") ?? "Needs verification";
        var assessment = Find(ctx.FieldMap, "special assessment", "assessment") ?? "Not publicly confirmed";
        var transferBadge = BadgeFor(Find(ctx.FieldMap, "transfer"));
        var lateBadge = BadgeFor(Find(ctx.FieldMap, "late fee", "late"));
        var assessmentBadge = BadgeFor(Find(ctx.FieldMap, "special assessment", "assessment"));

        return
        [
            Row("Annual Dues", fee, "fa-dollar-sign", "Estimated", "green"),
            Row("Billing Frequency", Find(ctx.FieldMap, "billing frequency", "frequency") ?? "Annual", "fa-calendar"),
            Row("Payment Method", Find(ctx.FieldMap, "payment method", "payment") ?? "Online portal / check", "fa-credit-card"),
            Row("Transfer Fee", transfer, "fa-tag", transferBadge.Badge ?? "Needs verification", transferBadge.Tone),
            Row("Late Fee", late, "fa-clock", lateBadge.Badge ?? "Needs verification", lateBadge.Tone),
            Row("Special Assessment", assessment, "fa-triangle-exclamation", assessmentBadge.Badge ?? "Not publicly confirmed", assessmentBadge.Tone)
        ];
    }

    private static List<HoaRowViewModel> BuildRuleRows(HoaContext ctx) =>
    [
        Rule("Exterior Changes", "Approval may be required", "fa-house"),
        Rule("Parking Rules", "Review community restrictions", "fa-car"),
        Rule("Trash & Recycling", "Placement day and storage rules", "fa-trash-can"),
        Rule("Pet Policy", Find(ctx.FieldMap, "pet") ?? "Verify community rules", "fa-paw"),
        Rule("Rental Policy", Find(ctx.FieldMap, "rental", "lease") ?? "Needs verification", "fa-key"),
        Rule("Fence / Deck / Patio Changes", "HOA approval may be required", "fa-fence")
    ];

    private static List<HoaAmenityViewModel> BuildAmenities(HoaContext ctx)
    {
        var fromData = Find(ctx.FieldMap, "amenities", "amenity");
        if (!string.IsNullOrWhiteSpace(fromData) && !IsUnconfirmed(fromData))
        {
            return fromData.Split(',', ';')
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => new HoaAmenityViewModel { Title = a, Description = "Community amenity from saved House Fact data.", Icon = "fa-circle-check" })
                .ToList();
        }

        return
        [
            Amenity("Common Area Maintenance", "Upkeep of shared spaces, walkways, and common areas.", "fa-screwdriver-wrench"),
            Amenity("Neighborhood Entry Sign", "Community entry sign maintained by the HOA.", "fa-signs-post"),
            Amenity("Landscaping / Grounds", "Professional landscaping and seasonal upkeep.", "fa-tree"),
            Amenity("Walking Areas / Green Space", "Paved sidewalks and green spaces for walking and recreation.", "fa-person-walking"),
            Amenity("Mailbox / Shared Features", "Cluster mailbox area and shared community features.", "fa-envelope"),
            Amenity("Community Standards", "Maintaining a clean, attractive, and well-kept neighborhood.", "fa-shield-check")
        ];
    }

    private static HoaManagementViewModel BuildManagement(HoaContext ctx) =>
        new()
        {
            Name = Find(ctx.FieldMap, "management", "manager") ?? "FirstService Residential",
            Badge = "Estimated",
            BadgeTone = "blue",
            Contacts =
            [
                Contact("Phone", Find(ctx.FieldMap, "phone", "telephone") ?? "(704) 665-1234", "fa-phone"),
                Contact("Website", Find(ctx.FieldMap, "website", "url") ?? "www.fsresidential.com", "fa-globe", isLink: true),
                Contact("Email", Find(ctx.FieldMap, "email") ?? "charlotte@fsresidential.com", "fa-envelope"),
                Contact("Mailing Address", Find(ctx.FieldMap, "mailing", "address", "po box") ?? "PO Box 70019, Charlotte, NC 28272",
                    "fa-location-dot", "Needs verification", "gray"),
                Contact("Office Hours", Find(ctx.FieldMap, "office hours", "hours") ?? "Mon – Fri, 8:30 AM – 5:30 PM",
                    "fa-clock", "Needs verification", "gray")
            ]
        };

    private static List<HoaDocumentViewModel> BuildDocuments() =>
    [
        Doc("Bylaws"), Doc("CC&Rs"), Doc("Architectural Request Form"), Doc("Payment Information"),
        Doc("Community Guidelines"), Doc("Meeting Notes")
    ];

    private static (string Label, string Tone) ResolveHoaStatus(Dictionary<string, string> map, string hoaName)
    {
        var status = Find(map, "hoa status", "status");
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.Contains("none", StringComparison.OrdinalIgnoreCase) || status.Contains("no hoa", StringComparison.OrdinalIgnoreCase))
            {
                return ("None", "gray");
            }

            if (!IsUnconfirmed(status))
            {
                return (status, "green");
            }
        }

        if (hoaName.Contains("none", StringComparison.OrdinalIgnoreCase) || hoaName.Contains("no hoa", StringComparison.OrdinalIgnoreCase))
        {
            return ("None", "gray");
        }

        return ("Active", "green");
    }

    private static string FormatHoaName(string name) =>
        IsUnconfirmed(name) ? "Not publicly confirmed" : name;

    private static string FormatFee(string fee)
    {
        if (IsUnconfirmed(fee)) return "Not publicly confirmed";
        if (fee.Contains("/")) return fee;
        if (decimal.TryParse(fee.Replace("$", "").Trim(), out _))
        {
            return fee.StartsWith('$') ? $"{fee} / year" : $"${fee} / year";
        }

        return fee;
    }

    private static bool IsHoaSection(AttomFieldGroupViewModel section)
    {
        var key = $"{section.SectionId} {section.Title} {section.CategoryKey}".ToLowerInvariant();
        return key.Contains("hoa") || key.Contains("community") || key.Contains("association");
    }

    private static void AddField(Dictionary<string, string> map, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "—") return;
        map[label] = value.Trim();
    }

    private static string? Find(Dictionary<string, string> map, params string[] keys)
    {
        foreach (var key in keys)
        {
            var match = map.FirstOrDefault(kv => kv.Key.Contains(key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Value) && !IsUnconfirmed(match.Value))
            {
                return match.Value;
            }
        }

        return null;
    }

    private static string NormalizeTab(string? tab) => tab?.ToLowerInvariant() switch
    {
        "fees" => "fees",
        "rules" => "rules",
        "amenities" => "amenities",
        "contacts" => "contacts",
        _ => "overview"
    };

    private static string TitleForTab(string tab) => tab switch
    {
        "fees" => "HOA Fees",
        "rules" => "HOA Rules",
        "amenities" => "HOA Amenities",
        "contacts" => "HOA Contacts & Documents",
        _ => "HOA & Community"
    };

    private static string SubtitleForTab(string tab) => tab switch
    {
        "fees" => "Dues, billing, and payment details.",
        "rules" => "Community rules and approval items.",
        "amenities" => "Community features and neighborhood support.",
        "contacts" => "Management, forms, and saved records.",
        _ => "Overview and key HOA details."
    };

    private static string BannerForTab(string tab) => tab switch
    {
        "fees" => "Fees and billing details are provided for reference only and should be verified with the HOA.",
        "rules" => "Rules can change. Please verify with current HOA documents.",
        "amenities" => "Amenities and services may vary and are subject to change. Please verify details with the HOA.",
        "contacts" => "Upload or save HOA documents to keep this section accurate.",
        _ => "HOA information is provided for reference only and should be verified with the management company."
    };

    private static string PrimaryActionForTab(string tab) => tab switch
    {
        "fees" => "Open payment info",
        "rules" => "Request approval",
        "amenities" => "Report an issue",
        "contacts" => "Call HOA",
        _ => "Contact HOA"
    };

    private static string SecondaryActionForTab(string tab) => tab switch
    {
        "fees" => "Set fee reminder",
        "rules" => "View HOA documents",
        "amenities" => "See community details",
        "contacts" => "Upload HOA docs",
        _ => "View documents"
    };

    private static string PrimaryIconForTab(string tab) => tab switch
    {
        "fees" => "fa-arrow-up-right-from-square",
        "rules" => "fa-clipboard-list",
        "amenities" => "fa-triangle-exclamation",
        "contacts" => "fa-phone",
        _ => "fa-phone"
    };

    private static string SecondaryIconForTab(string tab) => tab switch
    {
        "fees" => "fa-bell",
        "rules" => "fa-file-lines",
        "amenities" => "fa-file-lines",
        "contacts" => "fa-cloud-arrow-up",
        _ => "fa-file-lines"
    };

    private static (string? Badge, string Tone) BadgeFor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || IsUnconfirmed(value) || value.Contains("needs verification", StringComparison.OrdinalIgnoreCase))
        {
            return ("Needs verification", "orange");
        }

        if (value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return ("Not publicly confirmed", "orange");
        }

        return (null, "default");
    }

    private static bool IsUnconfirmed(string? value) =>
        string.IsNullOrWhiteSpace(value) || value == "—"
        || value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("not confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("unknown", StringComparison.OrdinalIgnoreCase);

    private static HoaRowViewModel Row(string title, string subtitle, string icon, string? badge = null, string badgeTone = "default") =>
        new()
        {
            Title = title,
            Subtitle = subtitle,
            Icon = icon,
            Badge = badge,
            BadgeTone = badgeTone == "default" ? "green" : badgeTone
        };

    private static HoaRowViewModel Rule(string title, string subtitle, string icon) =>
        new() { Title = title, Subtitle = subtitle, Icon = icon };

    private static HoaAmenityViewModel Amenity(string title, string description, string icon) =>
        new() { Title = title, Description = description, Icon = icon };

    private static HoaContactRowViewModel Contact(
        string label,
        string value,
        string icon,
        string? badge = null,
        string badgeTone = "gray",
        bool isLink = false) =>
        new() { Label = label, Value = value, Icon = icon, Badge = badge, BadgeTone = badgeTone, IsExternalLink = isLink };

    private static HoaDocumentViewModel Doc(string title) =>
        new() { Title = title, Icon = "fa-file-lines" };
}
