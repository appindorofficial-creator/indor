using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class MyHomeSummaryViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string HeroImageUrl { get; set; } = "/inspeccion2.jpeg";
    public int? YearBuilt { get; set; }
    public int? LivingArea { get; set; }
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public decimal? LotSizeAcres { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string DataSource { get; set; } = "Estimated";
    public DateTime? AttomLastSyncUtc { get; set; }
}

public class MyHomePropertyDetailsViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? ParcelId { get; set; }
    public string? Zoning { get; set; }
    public string? AssignedSchool { get; set; }
    public DateTime? LastSaleDate { get; set; }
    public decimal? LastSalePrice { get; set; }
    public decimal? AnnualTaxAmount { get; set; }
    public int? TaxYear { get; set; }
    public int? YearBuilt { get; set; }
    public int? LivingArea { get; set; }
    public decimal? LotSizeAcres { get; set; }
    public string? PropertyType { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string DataSource { get; set; } = "Estimated";
    public string ActiveTab { get; set; } = "information";
}

public class MyHomeEditViewModel
{
    public int PropiedadId { get; set; }

    [Required, MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? PropertyType { get; set; }

    public int? YearBuilt { get; set; }
    public int? LivingArea { get; set; }
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public decimal? LotSizeAcres { get; set; }
    public decimal? EstimatedValue { get; set; }
    public decimal? AnnualTaxAmount { get; set; }
    public int? TaxYear { get; set; }
    public string? ParcelId { get; set; }
    public string? Zoning { get; set; }
    public string? AssignedSchool { get; set; }
    public decimal? LastSalePrice { get; set; }
    public DateTime? LastSaleDate { get; set; }
}

public class MyHomeHistoryListViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Filter { get; set; } = "All";
    public List<MyHomeHistoryItemViewModel> Items { get; set; } = new();
}

public class MyHomeHistoryItemViewModel
{
    public int Id { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
    public decimal? TotalCost { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
}

public class MyHomeHistoryFormViewModel
{
    public int? Id { get; set; }
    public int PropiedadId { get; set; }

    [Required, MaxLength(30)]
    public string RecordType { get; set; } = "Improvement";

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? ProviderName { get; set; }

    public DateTime? CompletionDate { get; set; }

    public decimal? TotalCost { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(30)]
    public string? WarrantyStatus { get; set; }
}

public class MyHomeHistoryDetailViewModel
{
    public int Id { get; set; }
    public int PropiedadId { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
    public int? ProviderId { get; set; }
    public DateTime? CompletionDate { get; set; }
    public decimal? TotalCost { get; set; }
    public string? Description { get; set; }
    public string? WarrantyStatus { get; set; }
    public List<MyHomeDocumentItemViewModel> RelatedDocuments { get; set; } = new();
}

public class MyHomeDocumentItemViewModel
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? StoragePath { get; set; }
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
}

public class MyHomeDocumentListViewModel
{
    public int PropiedadId { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<MyHomeDocumentItemViewModel> Items { get; set; } = new();
}

public class MyHomeMaintenanceDetailViewModel
{
    public int Id { get; set; }
    public int PropiedadId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = "Upcoming";
    public string? Notes { get; set; }
    public string? ProviderName { get; set; }
}

public class MyHomeProvidersViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Search { get; set; }
    public List<MyHomeProviderItemViewModel> Items { get; set; } = new();
}

public class MyHomeProviderItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ServiceCategory { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class MyHomeProviderFormViewModel
{
    public int? Id { get; set; }
    public int PropiedadId { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string ServiceCategory { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(300)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class MyHomeMaintenanceViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Filter { get; set; } = "Upcoming";
    public List<MyHomeMaintenanceItemViewModel> Items { get; set; } = new();
}

public class MyHomeMaintenanceItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Upcoming";
    public string MonthLabel { get; set; } = string.Empty;
}

public class MyHomeMaintenanceFormViewModel
{
    public int? Id { get; set; }
    public int PropiedadId { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Upcoming";

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? PropiedadProveedorId { get; set; }
}

public class MyHomeDocumentsViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Search { get; set; }
    public List<MyHomeDocumentCategoryViewModel> Categories { get; set; } = new();
}

public class MyHomeDocumentCategoryViewModel
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MyHomeDocumentFormViewModel
{
    public int PropiedadId { get; set; }

    [Required, MaxLength(40)]
    public string Category { get; set; } = "Other";

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}
