using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class AddHvacSystemViewModel
{
    public int PropiedadId { get; set; }

    public string Address { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = "/welcome-house.png";

    public string? OpenAiHintNote { get; set; }

    [Required(ErrorMessage = "Select a system type.")]
    public string SystemType { get; set; } = "CentralAC";

    [MaxLength(80)]
    public string? Brand { get; set; }

    [MaxLength(80)]
    public string? Model { get; set; }

    [MaxLength(80)]
    public string? SerialNumber { get; set; }

    public int? InstallYear { get; set; }

    [MaxLength(40)]
    public string? FilterSize { get; set; }

    [DataType(DataType.Date)]
    public DateTime? LastServiceDate { get; set; }

    public bool FilterRemindersEnabled { get; set; } = true;

    public List<int> InstallYearOptions { get; set; } = new();

    public List<string> FilterSizeOptions { get; set; } = new();
}

public class HvacSavedViewModel
{
    public int PropiedadId { get; set; }

    public string SystemTypeLabel { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? InstallYearLabel { get; set; }

    public string? FilterSize { get; set; }

    public string? LastServiceLabel { get; set; }

    public bool FilterRemindersEnabled { get; set; }

    public int FilterReminderDays { get; set; }

    public string NextReminderTitle { get; set; } = "HVAC filter replacement";

    public string NextReminderDueLabel { get; set; } = string.Empty;

    public string HouseFactsUrl { get; set; } = "#";
}

public class HvacOpenAiHints
{
    public string? SystemType { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public int? InstallYear { get; set; }

    public string? FilterSize { get; set; }

    public DateTime? LastServiceDate { get; set; }

    public string? DataSource { get; set; }

    public bool HasAny =>
        !string.IsNullOrWhiteSpace(SystemType)
        || !string.IsNullOrWhiteSpace(Brand)
        || !string.IsNullOrWhiteSpace(Model)
        || InstallYear.HasValue
        || !string.IsNullOrWhiteSpace(FilterSize);
}
