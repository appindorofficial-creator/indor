using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class RiskScoreDisplayService
{
    private static readonly RiskScoreTabViewModel[] TabDefs =
    [
        new() { Key = "overview", Label = "Overview" },
        new() { Key = "categories", Label = "Categories" },
        new() { Key = "actions", Label = "Actions" },
        new() { Key = "history", Label = "History" }
    ];

    private static readonly RiskChecklistFilterViewModel[] ChecklistFilterDefs =
    [
        new() { Key = "all", Label = "All" },
        new() { Key = "high-impact", Label = "High impact", DotTone = "red" },
        new() { Key = "documents", Label = "Documents", Icon = "fa-file-lines" },
        new() { Key = "systems", Label = "Systems", Icon = "fa-fan" }
    ];

    public static RiskScoreIndexViewModel BuildIndex(Propiedad propiedad, PropertyInfoViewModel? info, string? tab = null)
    {
        var ctx = BuildContext(propiedad, info);
        var activeTab = NormalizeTab(tab);

        var model = new RiskScoreIndexViewModel
        {
            PropiedadId = propiedad.Id,
            Address = ctx.Address,
            ActiveTab = activeTab,
            HasData = ctx.HasData,
            FieldCount = ctx.FieldCount,
            OverallScore = ctx.OverallScore,
            OverallLevel = ctx.OverallLevel,
            OverallLevelTone = ctx.OverallLevelTone,
            OverallSummary = ctx.OverallSummary,
            Tabs = TabDefs.ToList(),
            Categories = ctx.Categories,
            Factors = ctx.Factors,
            Findings = ctx.Findings,
            HistoryItems = ctx.HistoryItems,
            PageTitle = TitleForTab(activeTab),
            PageSubtitle = SubtitleForTab(activeTab),
            InfoBanner = BannerForTab(activeTab),
            CategoriesAlert = ctx.CategoriesAlert,
            PrimaryActionLabel = PrimaryLabelForTab(activeTab),
            SecondaryActionLabel = SecondaryLabelForTab(activeTab),
            PrimaryActionTab = PrimaryTabForTab(activeTab),
            SecondaryActionTab = SecondaryTabForTab(activeTab)
        };

        return model;
    }

    public static RiskScoreChecklistViewModel BuildChecklist(Propiedad propiedad, PropertyInfoViewModel? info, string? filter = null)
    {
        var ctx = BuildContext(propiedad, info);
        var activeFilter = NormalizeChecklistFilter(filter);
        var items = activeFilter == "all"
            ? ctx.ChecklistItems
            : ctx.ChecklistItems.Where(i => i.FilterGroup == activeFilter).ToList();

        return new RiskScoreChecklistViewModel
        {
            PropiedadId = propiedad.Id,
            Address = ctx.Address,
            ActiveFilter = activeFilter,
            HasData = ctx.HasData,
            FieldCount = ctx.FieldCount,
            CompletionPercent = ctx.CompletionPercent,
            ConfidenceLabel = ctx.ConfidenceLabel,
            ConfidenceTone = ctx.ConfidenceTone,
            Filters = ChecklistFilterDefs.ToList(),
            Items = items,
            InfoBanner = "Your risk score updates as verified information is added."
        };
    }

    private sealed class RiskContext
    {
        public string Address { get; set; } = string.Empty;
        public bool HasData { get; set; }
        public int FieldCount { get; set; }
        public int OverallScore { get; set; }
        public string OverallLevel { get; set; } = "Medium";
        public string OverallLevelTone { get; set; } = "orange";
        public string OverallSummary { get; set; } = string.Empty;
        public int CompletionPercent { get; set; }
        public string ConfidenceLabel { get; set; } = "Estimated";
        public string ConfidenceTone { get; set; } = "green";
        public string CategoriesAlert { get; set; } = string.Empty;
        public List<RiskCategoryViewModel> Categories { get; set; } = new();
        public List<RiskFactorViewModel> Factors { get; set; } = new();
        public List<RiskFindingViewModel> Findings { get; set; } = new();
        public List<RiskHistoryItemViewModel> HistoryItems { get; set; } = new();
        public List<RiskChecklistItemViewModel> ChecklistItems { get; set; } = new();
    }

    private static RiskContext BuildContext(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var profile = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated",
            propiedad.Direccion ?? info?.FormattedAddress);

        var systems = SystemsProfileDisplayService.BuildIndex(propiedad, info);
        var permits = PermitsImprovementsDisplayService.BuildIndex(propiedad, info);
        var utilities = UtilitiesDisplayService.BuildIndex(propiedad, info);
        var hoa = HoaCommunityDisplayService.Build(propiedad, info);

        var ctx = new RiskContext
        {
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? profile.FormattedAddress ?? "Property address",
            HasData = profile.HasData,
            FieldCount = profile.FieldCount,
            ConfidenceLabel = ResolveConfidenceLabel(profile.Confidence),
            ConfidenceTone = "green"
        };

        ctx.Categories =
        [
            BuildMechanicalCategory(systems),
            BuildRoofCategory(profile),
            BuildPermitsCategory(permits),
            BuildHoaCategory(hoa),
            BuildStructuralCategory(profile),
            BuildExteriorCategory(profile),
            BuildUtilityCategory(utilities)
        ];

        ctx.Categories = ctx.Categories.OrderBy(c => CategoryOrder(c.Key)).ToList();
        ctx.OverallScore = ComputeOverallScore(ctx.Categories);
        ctx.OverallLevel = ScoreToLevel(ctx.OverallScore);
        ctx.OverallLevelTone = LevelTone(ctx.OverallLevel);
        ctx.OverallSummary = SummaryForLevel(ctx.OverallLevel);
        ctx.Factors = BuildFactors(ctx.Categories, systems, permits);
        ctx.Findings = BuildFindings(ctx.Categories, systems, permits);
        ctx.ChecklistItems = BuildChecklistItems(profile, systems, permits);
        ctx.CompletionPercent = ComputeCompletion(ctx.ChecklistItems, ctx.FieldCount);
        ctx.CategoriesAlert = BuildCategoriesAlert(ctx.Categories);
        ctx.HistoryItems = BuildHistory(propiedad, profile, ctx.OverallScore);

        return ctx;
    }

    private static RiskCategoryViewModel BuildMechanicalCategory(SystemsProfileIndexViewModel systems)
    {
        var needsVerification = systems.NeedsVerificationCount;
        if (!systems.HasData || systems.SystemCount == 0)
        {
            return Category("mechanical", "Mechanical", "HVAC, plumbing, electrical & appliances", "fa-gears",
                "Unknown", "gray", "Confidence: Needs verification", null);
        }

        if (needsVerification >= 3)
        {
            return Category("mechanical", "Mechanical", "HVAC, plumbing, electrical & appliances", "fa-gears",
                "Unknown", "gray", "Confidence: Needs verification", null);
        }

        var score = Math.Max(35, 88 - needsVerification * 12);
        return Category("mechanical", "Mechanical", "HVAC, plumbing, electrical & appliances", "fa-gears",
            needsVerification > 0 ? "Medium" : "Low", needsVerification > 0 ? "orange" : "green",
            needsVerification > 0 ? "Confidence: Needs verification" : "Confidence: Estimated", score);
    }

    private static RiskCategoryViewModel BuildRoofCategory(HouseFactProfileViewModel profile)
    {
        var roof = FindField(profile, "roof", "roof type", "roof material", "roof age");
        if (IsMissing(roof))
        {
            return Category("roof", "Roof", "Roof age, materials & condition", "fa-house-chimney",
                "Unknown", "gray", "Confidence: Needs verification", null);
        }

        return Category("roof", "Roof", "Roof age, materials & condition", "fa-house-chimney",
            "Low", "green", "Confidence: Estimated", 76);
    }

    private static RiskCategoryViewModel BuildPermitsCategory(PermitsImprovementsIndexViewModel permits)
    {
        var hasConfirmed = permits.HasData && permits.Items.Count > 0;
        if (!hasConfirmed)
        {
            return Category("permits", "Permit / Documentation", "Permit history & required documentation", "fa-file-lines",
                "High", "red", "Verification: Not publicly confirmed", 25);
        }

        return Category("permits", "Permit / Documentation", "Permit history & required documentation", "fa-file-lines",
            "Medium", "orange", "Confidence: Estimated", 58);
    }

    private static RiskCategoryViewModel BuildHoaCategory(HoaCommunityViewModel hoa)
    {
        var level = hoa.HoaStatusTone == "green"
            ? "Low"
            : (hoa.HoaStatus?.Contains("Active", StringComparison.OrdinalIgnoreCase) ?? false)
                ? "Medium"
                : "Unknown";
        int? score = level switch
        {
            "Low" => 85,
            "Medium" => 60,
            _ => null
        };
        return Category("hoa", "HOA", "HOA details, fees & community info", "fa-people-group",
            level, LevelTone(level), "Confidence: Estimated", score);
    }

    private static RiskCategoryViewModel BuildStructuralCategory(HouseFactProfileViewModel profile)
    {
        var foundation = FindField(profile, "foundation", "structure", "structural");
        var score = IsMissing(foundation) ? 72 : 80;
        return Category("structural", "Structural", "Foundation, framing & structural integrity", "fa-border-all",
            score >= 78 ? "Low" : "Medium", score >= 78 ? "green" : "orange", "Confidence: Estimated", score);
    }

    private static RiskCategoryViewModel BuildExteriorCategory(HouseFactProfileViewModel profile)
    {
        var exterior = FindField(profile, "exterior", "siding", "lot", "drainage");
        var score = IsMissing(exterior) ? 70 : 78;
        return Category("exterior", "Exterior / Site", "Exterior condition, lot & surroundings", "fa-tree",
            "Low", "green", "Confidence: Estimated", score);
    }

    private static RiskCategoryViewModel BuildUtilityCategory(UtilitiesIndexViewModel utilities)
    {
        var score = utilities.ProviderCount >= 4 ? 82 : utilities.ProviderCount >= 2 ? 70 : 55;
        var level = score >= 75 ? "Low" : score >= 60 ? "Medium" : "High";
        return Category("utility", "Utility", "Utility providers & service reliability", "fa-droplet",
            level, LevelTone(level), "Confidence: Estimated", score);
    }

    private static RiskCategoryViewModel Category(
        string key, string title, string description, string icon,
        string level, string levelTone, string confidenceNote, int? score) =>
        new()
        {
            Key = key,
            Title = title,
            Description = description,
            Icon = icon,
            Level = level,
            LevelTone = levelTone,
            ConfidenceNote = confidenceNote,
            Score = score,
            ScoreDisplay = score.HasValue ? $"{score} / 100" : "-- / 100",
            ScoreTone = score.HasValue ? LevelTone(ScoreToLevel(score.Value)) : "gray"
        };

    private static int ComputeOverallScore(IReadOnlyList<RiskCategoryViewModel> categories)
    {
        var scored = categories.Where(c => c.Score.HasValue).Select(c => c.Score!.Value).ToList();
        if (scored.Count == 0) return 68;

        var avg = (int)Math.Round(scored.Average());
        var highPenalty = categories.Count(c => c.Level is "High") * 8;
        var unknownPenalty = categories.Count(c => c.Level is "Unknown") * 4;
        return Math.Clamp(avg - highPenalty - unknownPenalty, 25, 95);
    }

    private static List<RiskFactorViewModel> BuildFactors(
        IReadOnlyList<RiskCategoryViewModel> categories,
        SystemsProfileIndexViewModel systems,
        PermitsImprovementsIndexViewModel permits)
    {
        var factors = new List<RiskFactorViewModel>();

        if (systems.NeedsVerificationCount > 0 || categories.First(c => c.Key == "mechanical").Level == "Unknown")
        {
            factors.Add(new RiskFactorViewModel
            {
                Text = "HVAC system verification is missing or not publicly confirmed.",
                Icon = "fa-triangle-exclamation",
                Tone = "orange"
            });
        }

        if (categories.First(c => c.Key == "roof").Level == "Unknown")
        {
            factors.Add(new RiskFactorViewModel
            {
                Text = "Roof age and condition are not confirmed.",
                Icon = "fa-circle-question",
                Tone = "gray"
            });
        }

        if (categories.First(c => c.Key == "permits").Level == "High")
        {
            factors.Add(new RiskFactorViewModel
            {
                Text = "Permit history is not publicly confirmed.",
                Icon = "fa-file-lines",
                Tone = "red"
            });
        }

        if (factors.Count == 0)
        {
            factors.Add(new RiskFactorViewModel
            {
                Text = "Most categories are estimated from public data. Add documents to improve confidence.",
                Icon = "fa-circle-info",
                Tone = "blue"
            });
        }

        return factors;
    }

    private static List<RiskFindingViewModel> BuildFindings(
        IReadOnlyList<RiskCategoryViewModel> categories,
        SystemsProfileIndexViewModel systems,
        PermitsImprovementsIndexViewModel permits)
    {
        var findings = new List<RiskFindingViewModel>();
        var order = 1;

        if (systems.Systems.Any(s => s.Id == "hvac" && s.StatusTone is "orange" or "yellow"))
        {
            findings.Add(Finding(order++, "HVAC age / serial / warranty", "System details are missing or unverified.",
                "fa-fan", "Needs verification", "orange", "Resolve", null, "systems"));
        }

        if (categories.First(c => c.Key == "roof").Level == "Unknown")
        {
            findings.Add(Finding(order++, "Roof age / warranty / inspection status", "Roof details are missing or unverified.",
                "fa-house-chimney", "Needs verification", "orange", "Resolve", null, "systems"));
        }

        if (categories.First(c => c.Key == "permits").Level is "High" or "Medium")
        {
            findings.Add(Finding(order++, "Permit history", "Permit history is not publicly confirmed.",
                "fa-file-lines", "High impact", "red", "View source", "categories", "documents"));
        }

        findings.Add(Finding(order++, "Seller disclosure & inspection report", "Seller disclosures or inspection report not found.",
            "fa-clipboard-check", "Needed", "orange", "Resolve", null, "documents"));

        if (systems.Systems.Any(s => s.Id == "water-heater"))
        {
            findings.Add(Finding(order, "Water heater install date / serial", "Water heater details are missing.",
                "fa-fire-burner", "Needed", "orange", "Resolve", null, "systems"));
        }

        return findings;
    }

    private static RiskFindingViewModel Finding(
        int order, string title, string description, string icon,
        string badge, string badgeTone, string actionLabel, string? actionTab, string checklistFilter) =>
        new()
        {
            Order = order,
            Title = title,
            Description = description,
            Icon = icon,
            Badge = badge,
            BadgeTone = badgeTone,
            ActionLabel = actionLabel,
            ActionTab = actionTab,
            ChecklistFilter = checklistFilter
        };

    private static List<RiskChecklistItemViewModel> BuildChecklistItems(
        HouseFactProfileViewModel profile,
        SystemsProfileIndexViewModel systems,
        PermitsImprovementsIndexViewModel permits)
    {
        var items = profile.Sections
            .Where(s => s.CategoryKey is "missing" or "risk" || s.SectionKind is "checklist" or "questions")
            .SelectMany(s => s.ChecklistItems)
            .Where(i => !string.IsNullOrWhiteSpace(i.Item))
            .Select(i => MapChecklistItem(i.Item, i.Status))
            .ToList();

        if (items.Count == 0)
        {
            items =
            [
                Checklist("Seller disclosure", "High impact", "red", "Needed", "documents", "fa-file-lines"),
                Checklist("Inspection report", "High impact", "red", "Needed", "documents", "fa-clipboard-check"),
                Checklist("HVAC serial numbers / install dates / warranty", "Medium impact", "orange", "Needed", "systems", "fa-fan"),
                Checklist("Water heater serial number / install date", "Medium impact", "orange", "Needed", "systems", "fa-droplet"),
                Checklist("Roof age / warranty", "Medium impact", "orange", "Needed", "systems", "fa-house-chimney"),
                Checklist("Permit history", "Helps confidence", "green", "Needed", "documents", "fa-clipboard-list"),
                Checklist("Electrical panel details", "Medium impact", "orange", "Needed", "systems", "fa-bolt"),
                Checklist("Plumbing updates", "Helps confidence", "blue", "Needed", "systems", "fa-faucet"),
                Checklist("Foundation / crawl space condition", "High impact", "red", "Needed", "high-impact", "fa-layer-group"),
                Checklist("Drainage or moisture history", "Medium impact", "orange", "Needed", "systems", "fa-water")
            ];
        }

        return items;
    }

    private static RiskChecklistItemViewModel MapChecklistItem(string title, string? status)
    {
        var lower = title.ToLowerInvariant();
        var filter = lower.Contains("disclosure") || lower.Contains("inspection") || lower.Contains("permit")
            ? "documents"
            : lower.Contains("hvac") || lower.Contains("roof") || lower.Contains("water") || lower.Contains("electrical") || lower.Contains("plumbing")
                ? "systems"
                : lower.Contains("foundation") || lower.Contains("moisture")
                    ? "high-impact"
                    : "all";

        var impact = filter == "high-impact" ? "High impact" : filter == "documents" ? "High impact" : "Medium impact";
        return Checklist(title, impact, impact.Contains("High") ? "red" : "orange",
            string.IsNullOrWhiteSpace(status) ? "Needed" : status, filter, IconForChecklist(title));
    }

    private static RiskChecklistItemViewModel Checklist(
        string title, string impact, string impactTone, string status, string filterGroup, string icon) =>
        new()
        {
            Title = title,
            Impact = impact,
            ImpactTone = impactTone,
            Status = status,
            StatusTone = status.Equals("Needed", StringComparison.OrdinalIgnoreCase) ? "orange" : "gray",
            FilterGroup = filterGroup,
            Icon = icon
        };

    private static List<RiskHistoryItemViewModel> BuildHistory(Propiedad propiedad, HouseFactProfileViewModel profile, int score)
    {
        var items = new List<RiskHistoryItemViewModel>
        {
            new()
            {
                Title = "Risk score calculated",
                Detail = $"Overall score set to {score}/100 based on available House Fact data.",
                When = propiedad.AttomLastSyncUtc?.ToLocalTime().ToString("MMM dd, yyyy") ?? "Recently",
                Icon = "fa-shield-halved"
            }
        };

        if (profile.FieldCount > 0)
        {
            items.Add(new RiskHistoryItemViewModel
            {
                Title = "Property data enriched",
                Detail = $"{profile.FieldCount} House Fact fields saved from public and estimated sources.",
                When = propiedad.AttomLastSyncUtc?.ToLocalTime().ToString("MMM dd, yyyy") ?? "Recently",
                Icon = "fa-database"
            });
        }

        items.Add(new RiskHistoryItemViewModel
        {
            Title = "Verification pending",
            Detail = "Score may improve as documents and system details are added.",
            When = "Ongoing",
            Icon = "fa-clock"
        });

        return items;
    }

    private static string BuildCategoriesAlert(IReadOnlyList<RiskCategoryViewModel> categories)
    {
        var highImpact = categories
            .Where(c => c.Level is "High" or "Unknown" && c.Key is "permits" or "mechanical")
            .Select(c => c.Title)
            .ToList();

        if (highImpact.Count >= 2)
        {
            return $"Two areas have the highest impact on your score: {highImpact[0]} and {highImpact[1]}. Addressing these could significantly improve your score.";
        }

        if (highImpact.Count == 1)
        {
            return $"{highImpact[0]} has the highest impact on your score. Adding verified information could improve your score.";
        }

        return "Review each category to see how public data, missing documentation, and verification status affect your score.";
    }

    private static int ComputeCompletion(IReadOnlyList<RiskChecklistItemViewModel> items, int fieldCount)
    {
        if (items.Count == 0) return 72;
        var resolved = items.Count(i => !i.Status.Equals("Needed", StringComparison.OrdinalIgnoreCase));
        var checklistPct = (int)Math.Round(resolved / (double)items.Count * 100);
        return Math.Clamp((checklistPct + Math.Min(fieldCount, 120)) / 2, 35, 95);
    }

    private static string? FindField(HouseFactProfileViewModel profile, params string[] keys)
    {
        foreach (var section in profile.Sections)
        {
            foreach (var field in section.Fields)
            {
                if (keys.Any(k => field.Label.Contains(k, StringComparison.OrdinalIgnoreCase)))
                {
                    return field.Value;
                }
            }
        }

        return null;
    }

    private static bool IsMissing(string? value) =>
        string.IsNullOrWhiteSpace(value)
        || value == "—"
        || value.Contains("not confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("not publicly", StringComparison.OrdinalIgnoreCase)
        || value.Contains("unknown", StringComparison.OrdinalIgnoreCase);

    private static string ScoreToLevel(int score) =>
        score >= 75 ? "Low" : score >= 55 ? "Medium" : "High";

    private static string LevelTone(string level) => level switch
    {
        "Low" => "green",
        "High" => "red",
        _ => "orange"
    };

    private static string SummaryForLevel(string level) => level switch
    {
        "Low" => "Your score reflects mostly confirmed public data with limited verification gaps.",
        "High" => "Your score is driven by missing verification and documentation gaps.",
        _ => "Your score is mostly driven by missing verification and permit data."
    };

    private static string ResolveConfidenceLabel(string? confidence)
    {
        if (string.IsNullOrWhiteSpace(confidence)) return "Estimated";
        if (confidence.Contains("confirm", StringComparison.OrdinalIgnoreCase)) return "Confirmed";
        return "Estimated";
    }

    private static int CategoryOrder(string key) => key switch
    {
        "mechanical" => 1,
        "roof" => 2,
        "permits" => 3,
        "hoa" => 4,
        "structural" => 5,
        "exterior" => 6,
        "utility" => 7,
        _ => 99
    };

    private static string IconForChecklist(string title)
    {
        var t = title.ToLowerInvariant();
        if (t.Contains("disclosure")) return "fa-file-lines";
        if (t.Contains("inspection")) return "fa-clipboard-check";
        if (t.Contains("hvac")) return "fa-fan";
        if (t.Contains("water")) return "fa-droplet";
        if (t.Contains("roof")) return "fa-house-chimney";
        if (t.Contains("permit")) return "fa-clipboard-list";
        if (t.Contains("electrical")) return "fa-bolt";
        if (t.Contains("plumbing")) return "fa-faucet";
        if (t.Contains("foundation") || t.Contains("crawl")) return "fa-layer-group";
        if (t.Contains("drainage") || t.Contains("moisture")) return "fa-water";
        return "fa-circle-info";
    }

    private static string NormalizeTab(string? tab) => tab?.ToLowerInvariant() switch
    {
        "categories" => "categories",
        "actions" => "actions",
        "history" => "history",
        _ => "overview"
    };

    private static string NormalizeChecklistFilter(string? filter) => filter?.ToLowerInvariant() switch
    {
        "high-impact" => "high-impact",
        "documents" => "documents",
        "systems" => "systems",
        _ => "all"
    };

    private static string TitleForTab(string tab) => tab switch
    {
        "categories" => "Risk Category Breakdown",
        "actions" => "Priority Findings",
        "history" => "Risk History",
        _ => "Risk Score"
    };

    private static string SubtitleForTab(string tab) => tab switch
    {
        "categories" => "See how each category affects your overall score.",
        "actions" => "The main issues increasing this property's risk score.",
        "history" => "How your risk score has changed over time.",
        _ => "Overall risk assessment for this property."
    };

    private static string BannerForTab(string tab) => tab switch
    {
        "categories" => "Review category scores and confidence levels to understand what affects your overall risk score.",
        "actions" => "Adding documents can improve confidence and may lower risk.",
        "history" => "Your score updates when new verified information is added to House Facts.",
        _ => "Based on public data, missing documentation, and system verification status."
    };

    private static string PrimaryLabelForTab(string tab) => tab switch
    {
        "overview" => "View category breakdown",
        "categories" => "View priority findings",
        "actions" => "See missing info checklist",
        _ => "Back to overview"
    };

    private static string SecondaryLabelForTab(string tab) => tab switch
    {
        "overview" => "See missing info",
        "categories" => "Back to overview",
        "actions" => "Request review",
        _ => "See missing info"
    };

    private static string? PrimaryTabForTab(string tab) => tab switch
    {
        "overview" => "categories",
        "categories" => "actions",
        "actions" => null,
        _ => "overview"
    };

    private static string? SecondaryTabForTab(string tab) => tab switch
    {
        "overview" => null,
        "categories" => "overview",
        "actions" => null,
        _ => null
    };
}
