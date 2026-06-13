using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class DocumentsDisplayService
{
    private sealed record DocDef(
        string Id,
        string Title,
        string FileName,
        string CategoryLabel,
        string TabGroup,
        string UpdatedLabel,
        string Status,
        string StatusTone,
        string Icon,
        string IconTone,
        string SizeLabel,
        string PageCountLabel,
        string Source,
        string UploadedBy,
        string RelatedSection,
        string Notes,
        string[] AiSummary);

    private static readonly DocDef[] SeedDocs =
    [
        new("inspection-report", "Inspection Report", "Inspection Report.pdf", "Report", "reports",
            "Updated May 12, 2024", "Reviewed", "green", "fa-file-pdf", "red", "1.2 MB", "14 pages",
            "Inspector", "House Facts", "Reports", "Annual general home inspection.",
            ["General home inspection completed", "Roof age not publicly confirmed", "HVAC serial verification recommended"]),
        new("seller-disclosure", "Seller Disclosure", "Seller Disclosure.pdf", "Disclosure", "disclosures",
            "Updated May 10, 2024", "Pending", "orange", "fa-file-lines", "orange", "860 KB", "6 pages",
            "Seller", "House Facts", "Disclosures", "Seller-provided disclosure package.",
            ["Disclosure received and pending review", "Known defect list included", "Repair history notes available"]),
        new("hvac-permit", "HVAC Permit", "HVAC Permit.pdf", "Permit", "permits",
            "Updated May 8, 2024", "Needs verification", "amber", "fa-clipboard-check", "green", "420 KB", "2 pages",
            "County records", "House Facts", "Permits", "HVAC replacement permit filed in 2019.",
            ["Permit number on file", "Contractor name included", "Completion date needs confirmation"]),
        new("roof-warranty", "Roof Warranty", "Roof Warranty.pdf", "Warranty", "reports",
            "Updated Apr 28, 2024", "Saved", "green", "fa-shield-halved", "blue", "540 KB", "3 pages",
            "Manufacturer", "House Facts", "Roof & Exterior", "Transferable roof warranty through 2028.",
            ["Warranty provider identified", "Coverage dates confirmed", "Transfer terms included"]),
        new("water-heater-invoice", "Water Heater Invoice", "Water Heater Invoice.pdf", "Invoice", "reports",
            "Updated Apr 15, 2024", "Uploaded", "blue", "fa-file-invoice-dollar", "blue", "320 KB", "1 page",
            "Contractor", "House Facts", "Systems", "Tankless water heater install invoice.",
            ["Install date recorded", "Serial number on invoice", "Warranty reference included"])
    ];

    private static readonly (string Key, string Label)[] TabDefs =
    [
        ("all", "All"),
        ("reports", "Reports"),
        ("permits", "Permits"),
        ("disclosures", "Disclosures"),
        ("photos", "Photos")
    ];

    public static readonly string[] AddCategories =
        ["Report", "Permit", "Disclosure", "Warranty", "Invoice", "Photo"];

    public static readonly string[] RelatedSections =
        ["Systems", "Roof & Exterior", "Permits", "HOA", "Utilities", "Schools"];

    public static DocumentsIndexViewModel BuildIndex(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        string? tab,
        IReadOnlyList<PropiedadDocumento>? dbDocs = null)
    {
        var activeTab = NormalizeTab(tab);
        var allDocs = MergeDocuments(dbDocs);
        var filtered = activeTab == "all"
            ? allDocs
            : allDocs.Where(d => d.TabGroup == activeTab).ToList();

        return new DocumentsIndexViewModel
        {
            PropiedadId = propiedad.Id,
            Address = ResolveAddress(propiedad, info),
            TotalDocuments = allDocs.Count,
            PendingCount = allDocs.Count(d => d.StatusTone is "orange" or "amber"),
            SharedCount = 2,
            ActiveTab = activeTab,
            Tabs = TabDefs.Select(t => new DocumentTabViewModel { Key = t.Key, Label = t.Label }).ToList(),
            Documents = filtered
        };
    }

    public static DocumentDetailViewModel? BuildDetail(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        string documentId,
        IReadOnlyList<PropiedadDocumento>? dbDocs = null)
    {
        var allDocs = MergeDocuments(dbDocs);
        var listItem = allDocs.FirstOrDefault(d => d.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
        if (listItem == null) return null;

        var def = SeedDocs.FirstOrDefault(d => d.Id == documentId);
        if (def != null)
        {
            return new DocumentDetailViewModel
            {
                PropiedadId = propiedad.Id,
                DocumentId = def.Id,
                Title = def.Title,
                FileName = def.FileName,
                SizeLabel = def.SizeLabel,
                PageCountLabel = def.PageCountLabel,
                CategoryLabel = def.CategoryLabel,
                UpdatedLabel = def.UpdatedLabel,
                Status = def.Status,
                StatusTone = def.StatusTone,
                Icon = def.Icon,
                IconTone = def.IconTone,
                Details =
                [
                    new() { Label = "Document type", Value = def.CategoryLabel, Icon = "fa-file-lines" },
                    new() { Label = "Source", Value = def.Source, Icon = "fa-user" },
                    new() { Label = "Uploaded by", Value = def.UploadedBy, Icon = "fa-cloud-arrow-up" },
                    new() { Label = "Related section", Value = def.RelatedSection, Icon = "fa-layer-group" },
                    new() { Label = "Notes", Value = def.Notes, Icon = "fa-note-sticky" }
                ],
                AiSummary = def.AiSummary.ToList(),
                PrimaryActionLabel = def.Status == "Reviewed" ? "Mark as reviewed" : "Mark as reviewed"
            };
        }

        var dbDoc = dbDocs?.FirstOrDefault(d => $"db-{d.Id}" == documentId);
        if (dbDoc == null) return null;

        return new DocumentDetailViewModel
        {
            PropiedadId = propiedad.Id,
            DocumentId = documentId,
            Title = dbDoc.Title,
            FileName = dbDoc.FileName ?? dbDoc.Title,
            SizeLabel = MyHomeDisplayService.FormatFileSize(dbDoc.SizeBytes),
            PageCountLabel = "—",
            CategoryLabel = MapDbCategory(dbDoc.Category),
            UpdatedLabel = $"Updated {dbDoc.FechaCreacion:MMM d, yyyy}",
            Status = "Uploaded",
            StatusTone = "blue",
            Icon = "fa-file-lines",
            IconTone = "blue",
            StoragePath = dbDoc.StoragePath,
            Details =
            [
                new() { Label = "Document type", Value = MapDbCategory(dbDoc.Category), Icon = "fa-file-lines" },
                new() { Label = "Source", Value = "Uploaded file", Icon = "fa-user" },
                new() { Label = "Uploaded by", Value = "House Facts", Icon = "fa-cloud-arrow-up" },
                new() { Label = "Related section", Value = MapDbCategory(dbDoc.Category), Icon = "fa-layer-group" },
                new() { Label = "Notes", Value = "Saved to your property file.", Icon = "fa-note-sticky" }
            ],
            AiSummary = ["Document saved to your House Facts file", "INDOR can scan related details when available"],
            PrimaryActionLabel = "Mark as reviewed"
        };
    }

    public static DocumentAddViewModel BuildAdd(Propiedad propiedad, string? category = null) => new()
    {
        PropiedadId = propiedad.Id,
        Category = string.IsNullOrWhiteSpace(category) ? "Report" : category,
        RelatedSection = "Systems",
        CategoryOptions = AddCategories.ToList(),
        SectionOptions = RelatedSections.ToList()
    };

    public static DocumentRequestsViewModel BuildRequests(Propiedad propiedad, PropertyInfoViewModel? info) =>
        new()
        {
            PropiedadId = propiedad.Id,
            PendingCount = 3,
            SharedCount = 2,
            RemindersSent = 1,
            PendingItems =
            [
                new()
                {
                    RequestId = "seller-disclosure",
                    Title = "Seller Disclosure",
                    Subtitle = "Requested May 10, 2024",
                    Status = "Pending",
                    StatusTone = "orange",
                    Icon = "fa-file-lines",
                    IconTone = "orange",
                    ActionLabel = "Send reminder",
                    ActionTone = "outline"
                },
                new()
                {
                    RequestId = "roof-warranty",
                    Title = "Roof Warranty",
                    Subtitle = "Requested Apr 28, 2024",
                    Status = "Waiting",
                    StatusTone = "orange",
                    Icon = "fa-shield-halved",
                    IconTone = "blue",
                    ActionLabel = "Upload now",
                    ActionTone = "outline"
                },
                new()
                {
                    RequestId = "hvac-permit",
                    Title = "HVAC Permit",
                    Subtitle = "Requested May 8, 2024",
                    Status = "Requested",
                    StatusTone = "orange",
                    Icon = "fa-clipboard-check",
                    IconTone = "green",
                    ActionLabel = "View request",
                    ActionTone = "outline"
                }
            ],
            SharedItems =
            [
                new()
                {
                    RequestId = "inspection-report",
                    Title = "Inspection Report",
                    Subtitle = "Shared with Realtor",
                    Status = "Shared",
                    StatusTone = "purple",
                    Icon = "fa-file-pdf",
                    IconTone = "red",
                    ActionLabel = "Manage access",
                    ActionTone = "outline"
                },
                new()
                {
                    RequestId = "hvac-permit-shared",
                    Title = "HVAC Permit",
                    Subtitle = "Shared with Buyer",
                    Status = "Shared",
                    StatusTone = "purple",
                    Icon = "fa-clipboard-check",
                    IconTone = "green",
                    ActionLabel = "Manage access",
                    ActionTone = "outline"
                }
            ]
        };

    private static List<DocumentListItemViewModel> MergeDocuments(IReadOnlyList<PropiedadDocumento>? dbDocs)
    {
        var items = SeedDocs.Select(d => new DocumentListItemViewModel
        {
            DocumentId = d.Id,
            Title = d.Title,
            CategoryLabel = d.CategoryLabel,
            UpdatedLabel = d.UpdatedLabel,
            Status = d.Status,
            StatusTone = d.StatusTone,
            Icon = d.Icon,
            IconTone = d.IconTone,
            TabGroup = d.TabGroup
        }).ToList();

        if (dbDocs == null || dbDocs.Count == 0) return items;

        foreach (var doc in dbDocs.OrderByDescending(d => d.FechaCreacion))
        {
            var tabGroup = MapDbTabGroup(doc.Category);
            items.Insert(0, new DocumentListItemViewModel
            {
                DocumentId = $"db-{doc.Id}",
                Title = doc.Title,
                CategoryLabel = MapDbCategory(doc.Category),
                UpdatedLabel = $"Updated {doc.FechaCreacion:MMM d, yyyy}",
                Status = "Uploaded",
                StatusTone = "blue",
                Icon = tabGroup == "photos" ? "fa-image" : "fa-file-lines",
                IconTone = "blue",
                TabGroup = tabGroup
            });
        }

        return items;
    }

    private static string NormalizeTab(string? tab)
    {
        if (string.IsNullOrWhiteSpace(tab)) return "all";
        var key = tab.ToLowerInvariant();
        return TabDefs.Any(t => t.Key == key) ? key : "all";
    }

    private static string ResolveAddress(Propiedad propiedad, PropertyInfoViewModel? info) =>
        propiedad.Direccion ?? info?.FormattedAddress ?? "Property address";

    private static string MapDbCategory(string category) => category switch
    {
        "Inspections" => "Report",
        "Permits" => "Permit",
        "Contracts" => "Disclosure",
        "Warranties" => "Warranty",
        "Invoices" => "Invoice",
        "Manuals" => "Manual",
        "Photo" => "Photo",
        "Photos" => "Photo",
        _ => category
    };

    private static string MapDbTabGroup(string category) => category switch
    {
        "Permits" => "permits",
        "Contracts" => "disclosures",
        "Photo" => "photos",
        "Photos" => "photos",
        _ => "reports"
    };
}
