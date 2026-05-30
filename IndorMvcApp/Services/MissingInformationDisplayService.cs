using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class MissingInformationDisplayService
{
    private sealed record ItemDef(
        string Id,
        string Title,
        string CategoryId,
        string Icon,
        string Impact,
        string ImpactTone,
        bool HighPriority,
        string VerifyTitle,
        string VerifySubtitle,
        string[] Needs,
        string PrimaryAction);

    private static readonly ItemDef[] ItemDefs =
    [
        new("inspection-report", "Inspection report", "documents", "fa-clipboard-check", "High impact", "red", true,
            "Upload inspection report", "This helps confirm structure, systems, and overall property condition.",
            ["Full inspection report PDF", "Summary of major findings", "Inspector contact if available", "Date of inspection"],
            "Upload inspection report"),
        new("seller-disclosure", "Seller disclosure", "documents", "fa-file-lines", "High impact", "red", true,
            "Add seller disclosure", "Seller disclosures help confirm known issues and property history.",
            ["Signed seller disclosure", "Known defect list", "Repair history notes", "Date signed"],
            "Upload seller disclosure"),
        new("permits", "Permit history", "documents", "fa-clipboard-list", "Helps confidence", "green", false,
            "Add permit records", "Permit records help confirm improvements and major work.",
            ["Permit numbers if available", "Contractor name", "Work description", "Completion date"],
            "Add permit info"),
        new("warranties", "Warranty documents", "documents", "fa-shield-halved", "Helps confidence", "blue", false,
            "Add warranty documents", "Warranties help confirm coverage for systems and appliances.",
            ["Warranty provider", "Policy number", "Coverage dates", "System covered"],
            "Upload warranty"),
        new("hvac", "HVAC system", "systems", "fa-fan", "High impact", "red", true,
            "Verify HVAC System", "This helps confirm system age, warranty status, and mechanical risk.",
            ["Outdoor unit serial number", "Indoor unit serial number", "Install date", "Warranty document if available"],
            "Add HVAC info"),
        new("water-heater", "Water heater", "systems", "fa-fire-burner", "Medium impact", "orange", false,
            "Verify water heater", "Install date and serial help confirm age and replacement timing.",
            ["Serial number", "Install date", "Tank or tankless type", "Warranty document if available"],
            "Add water heater info"),
        new("electrical-panel", "Electrical panel", "systems", "fa-bolt", "Medium impact", "orange", false,
            "Verify electrical panel", "Panel details help confirm service capacity and safety.",
            ["Panel photo", "Main breaker size", "Panel brand/model", "Recent upgrade notes"],
            "Add electrical info"),
        new("plumbing", "Plumbing updates", "systems", "fa-faucet", "Helps confidence", "blue", false,
            "Verify plumbing", "Plumbing notes help confirm supply, drains, and recent updates.",
            ["Known repipe or updates", "Water supply type", "Sewer connection notes", "Recent repair history"],
            "Add plumbing info"),
        new("roof-age", "Roof age / warranty", "property", "fa-house-chimney", "Medium impact", "orange", true,
            "Verify roof details", "Roof age and warranty help confirm exterior risk and maintenance needs.",
            ["Roof install or replacement date", "Roof material", "Warranty document if available", "Recent inspection notes"],
            "Add roof info"),
        new("foundation", "Foundation / crawl space", "property", "fa-layer-group", "High impact", "red", true,
            "Verify foundation condition", "Foundation details help confirm structural confidence.",
            ["Known cracks or repairs", "Crawl space access notes", "Moisture history", "Inspection notes"],
            "Add foundation info"),
        new("drainage", "Drainage or moisture history", "property", "fa-water", "Medium impact", "orange", false,
            "Verify drainage history", "Drainage and moisture notes help confirm site risk.",
            ["Past water intrusion", "Sump pump details", "Grading concerns", "Gutter/downspout notes"],
            "Add drainage info"),
        new("hoa-docs", "HOA documents", "community", "fa-people-group", "Helps confidence", "purple", false,
            "Add HOA documents", "HOA documents help confirm fees, rules, and community details.",
            ["HOA contact", "Fee schedule", "CC&Rs or rules summary", "Management company"],
            "Add HOA info"),
        new("utilities-setup", "Utility account setup", "community", "fa-droplet", "Helps confidence", "blue", false,
            "Confirm utility providers", "Utility details help confirm service transfer and provider contacts.",
            ["Electric provider account", "Water provider account", "Gas provider if applicable", "Trash pickup schedule"],
            "Review utilities")
    ];

    private static readonly (string Id, string Title, string Subtitle, string Icon, string Tone)[] CategoryDefs =
    [
        ("documents", "Documents", "Seller disclosure, inspection report, permits", "fa-file-lines", "blue"),
        ("systems", "Systems to Verify", "HVAC, water heater, electrical panel", "fa-screwdriver-wrench", "green"),
        ("property", "Property Details", "Roof age, foundation, drainage history", "fa-house", "purple"),
        ("community", "Community & Property", "HOA, utilities, school district", "fa-people-group", "purple")
    ];

    public static MissingInformationHubViewModel BuildHub(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var ctx = BuildContext(propiedad, info);
        return new MissingInformationHubViewModel
        {
            PropiedadId = propiedad.Id,
            Address = ctx.Address,
            HasData = ctx.HasData,
            ProfileStrengthPercent = ctx.ProfileStrengthPercent,
            ItemsRemaining = ctx.PendingItems.Count,
            HighPriorityCount = ctx.PendingItems.Count(i => i.IsHighPriority),
            DocumentsNeededCount = ctx.PendingItems.Count(i => i.CategoryId == "documents"),
            RecommendedStep = ctx.RecommendedStep,
            CategorySummaries = BuildCategorySummaries(ctx.PendingItems)
        };
    }

    public static MissingInformationCategoriesViewModel BuildCategories(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var ctx = BuildContext(propiedad, info);
        return new MissingInformationCategoriesViewModel
        {
            PropiedadId = propiedad.Id,
            Address = ctx.Address,
            ProfileStrengthPercent = ctx.ProfileStrengthPercent,
            FeaturedStep = ctx.RecommendedStep,
            Categories = CategoryDefs.Select(c =>
            {
                var count = ctx.PendingItems.Count(i => i.CategoryId == c.Id);
                return new MissingCategoryCardViewModel
                {
                    CategoryId = c.Id,
                    Title = CategoryTitle(c.Id),
                    Subtitle = c.Subtitle,
                    Icon = c.Icon,
                    Tone = c.Tone,
                    ItemCount = count
                };
            }).Where(c => c.ItemCount > 0).ToList()
        };
    }

    public static MissingInformationCategoryViewModel? BuildCategory(Propiedad propiedad, PropertyInfoViewModel? info, string categoryId)
    {
        var ctx = BuildContext(propiedad, info);
        var def = CategoryDefs.FirstOrDefault(c => c.Id.Equals(categoryId, StringComparison.OrdinalIgnoreCase));
        if (def.Id == null) return null;

        var items = ctx.PendingItems.Where(i => i.CategoryId == categoryId).ToList();
        return new MissingInformationCategoryViewModel
        {
            PropiedadId = propiedad.Id,
            CategoryId = categoryId,
            Title = CategoryTitle(categoryId),
            Subtitle = def.Subtitle,
            Icon = def.Icon,
            Tone = def.Tone,
            Items = items
        };
    }

    public static MissingInformationVerifyViewModel? BuildVerify(Propiedad propiedad, PropertyInfoViewModel? info, string itemId)
    {
        var def = ItemDefs.FirstOrDefault(i => i.Id.Equals(itemId, StringComparison.OrdinalIgnoreCase));
        if (def == null) return null;

        return new MissingInformationVerifyViewModel
        {
            PropiedadId = propiedad.Id,
            ItemId = def.Id,
            Title = def.VerifyTitle,
            Subtitle = def.VerifySubtitle,
            Priority = def.HighPriority ? "High priority" : "Medium priority",
            PriorityTone = def.HighPriority ? "red" : "orange",
            Icon = def.Icon,
            Needs = def.Needs.Select(n => new MissingVerifyNeedViewModel { Label = n }).ToList(),
            Actions =
            [
                new() { Label = "Take photo", Icon = "fa-camera", Tone = "blue" },
                new() { Label = "Upload document", Icon = "fa-file-arrow-up", Tone = "blue" },
                new() { Label = "I don't have this", Icon = "fa-circle-question", Tone = "gray" },
                new() { Label = "Request from agent / seller", Icon = "fa-paper-plane", Tone = "blue" }
            ],
            InfoBanner = "You can upload an invoice, inspection report, permit, or warranty to verify this item.",
            PrimaryActionLabel = def.PrimaryAction
        };
    }

    public static MissingInformationUpdatedViewModel BuildUpdated(Propiedad propiedad, PropertyInfoViewModel? info, string? itemId)
    {
        var ctx = BuildContext(propiedad, info);
        var before = ctx.ProfileStrengthPercent;
        var improvement = itemId == "inspection-report" ? 6 : itemId == "hvac" ? 4 : 3;
        var after = Math.Min(before + improvement, 98);
        var def = ItemDefs.FirstOrDefault(i => i.Id.Equals(itemId, StringComparison.OrdinalIgnoreCase));

        var updated = new List<MissingUpdatedItemViewModel>();
        if (def != null)
        {
            updated.Add(new MissingUpdatedItemViewModel { Title = $"{def.Title} verification started", Icon = def.Icon });
            if (def.CategoryId == "systems")
            {
                updated.Add(new MissingUpdatedItemViewModel { Title = "Mechanical risk updated", Icon = "fa-shield-halved" });
            }
            if (def.CategoryId == "documents")
            {
                updated.Add(new MissingUpdatedItemViewModel { Title = "Documents section updated", Icon = "fa-file-lines" });
            }
        }
        else
        {
            updated.Add(new MissingUpdatedItemViewModel { Title = "House Fact profile updated", Icon = "fa-circle-check" });
        }

        return new MissingInformationUpdatedViewModel
        {
            PropiedadId = propiedad.Id,
            BeforePercent = before,
            AfterPercent = after,
            ImprovementPercent = after - before,
            UpdatedItems = updated,
            NextSteps =
            [
                "We saved your document.",
                "INDOR can scan related details",
                "You can complete another item anytime"
            ]
        };
    }

    private sealed class MissingContext
    {
        public string Address { get; set; } = string.Empty;
        public bool HasData { get; set; }
        public int ProfileStrengthPercent { get; set; }
        public List<MissingItemViewModel> PendingItems { get; set; } = new();
        public MissingRecommendedStepViewModel? RecommendedStep { get; set; }
    }

    private static MissingContext BuildContext(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var profile = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus,
            propiedad.Direccion ?? info?.FormattedAddress);

        var checklist = RiskScoreDisplayService.BuildChecklist(propiedad, info);

        var items = ItemDefs.Select(def =>
        {
            var match = checklist.Items.FirstOrDefault(c => MapChecklistToDef(c.Title)?.Id == def.Id);
            return ToItem(def, match);
        })
        .Where(i => i.Status.Equals("Needed", StringComparison.OrdinalIgnoreCase))
        .ToList();

        var profileStrength = Math.Clamp(checklist.CompletionPercent > 0 ? checklist.CompletionPercent : ComputeStrength(profile.FieldCount, items.Count), 55, 92);

        var recommended = items.FirstOrDefault(i => i.ItemId == "inspection-report")
            ?? items.FirstOrDefault(i => i.IsHighPriority)
            ?? items.FirstOrDefault();

        return new MissingContext
        {
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? profile.FormattedAddress ?? "Property address",
            HasData = profile.HasData || items.Count > 0,
            ProfileStrengthPercent = profileStrength,
            PendingItems = items.OrderByDescending(i => i.IsHighPriority).ThenBy(i => CategoryOrder(i.ItemId)).ToList(),
            RecommendedStep = recommended == null ? null : new MissingRecommendedStepViewModel
            {
                Title = recommended.ItemId == "inspection-report"
                    ? "Upload your inspection report first."
                    : $"Complete {recommended.Title.ToLowerInvariant()} next.",
                Description = "Recommended next step",
                Detail = recommended.ItemId == "inspection-report"
                    ? "It can help complete multiple sections at once."
                    : "This can improve accuracy and lower risk.",
                Icon = "fa-star",
                ItemId = recommended.ItemId,
                CategoryId = ItemDefs.FirstOrDefault(d => d.Id == recommended.ItemId)?.CategoryId ?? recommended.CategoryId,
            }
        };
    }

    private static List<MissingCategorySummaryViewModel> BuildCategorySummaries(IReadOnlyList<MissingItemViewModel> items) =>
        CategoryDefs.Select(c => new MissingCategorySummaryViewModel
        {
            CategoryId = c.Id,
            Title = HubTitle(c.Id),
            Subtitle = c.Subtitle,
            Icon = c.Icon,
            Tone = c.Tone,
            ItemCount = items.Count(i => i.CategoryId == c.Id)
        }).Where(c => c.ItemCount > 0).ToList();

    private static MissingItemViewModel ToItem(ItemDef def, RiskChecklistItemViewModel? checklist = null) => new()
    {
        ItemId = def.Id,
        CategoryId = def.CategoryId,
        Title = def.Title,
        Impact = checklist?.Impact ?? def.Impact,
        ImpactTone = checklist?.ImpactTone ?? def.ImpactTone,
        Status = checklist?.Status ?? "Needed",
        Icon = def.Icon,
        IsHighPriority = def.HighPriority
    };

    private static ItemDef? MapChecklistToDef(string title)
    {
        var t = title.ToLowerInvariant();
        if (t.Contains("inspection")) return ItemDefs.First(d => d.Id == "inspection-report");
        if (t.Contains("disclosure")) return ItemDefs.First(d => d.Id == "seller-disclosure");
        if (t.Contains("hvac")) return ItemDefs.First(d => d.Id == "hvac");
        if (t.Contains("water heater")) return ItemDefs.First(d => d.Id == "water-heater");
        if (t.Contains("electrical")) return ItemDefs.First(d => d.Id == "electrical-panel");
        if (t.Contains("plumbing")) return ItemDefs.First(d => d.Id == "plumbing");
        if (t.Contains("roof")) return ItemDefs.First(d => d.Id == "roof-age");
        if (t.Contains("foundation") || t.Contains("crawl")) return ItemDefs.First(d => d.Id == "foundation");
        if (t.Contains("drainage") || t.Contains("moisture")) return ItemDefs.First(d => d.Id == "drainage");
        if (t.Contains("permit")) return ItemDefs.First(d => d.Id == "permits");
        return null;
    }

    private static int ComputeStrength(int fieldCount, int pendingCount)
    {
        var basePct = Math.Min(40 + fieldCount / 2, 85);
        return Math.Clamp(basePct - pendingCount, 55, 85);
    }

    private static string HubTitle(string categoryId) => categoryId switch
    {
        "documents" => "Documents Needed",
        "systems" => "Systems to Verify",
        "property" => "Property Details",
        _ => "Community & Property"
    };

    private static string CategoryTitle(string categoryId) => categoryId switch
    {
        "documents" => "Documents",
        "systems" => "Systems",
        "property" => "Exterior & Structure",
        _ => "Community & Property"
    };

    private static int CategoryOrder(string itemId)
    {
        var idx = Array.FindIndex(ItemDefs, d => d.Id == itemId);
        return idx >= 0 ? idx : 99;
    }
}
