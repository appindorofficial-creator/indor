using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class HvacSetupDraft
{
    public int PropiedadId { get; set; }

    public string EntryMode { get; set; } = "manual";

    public string SystemType { get; set; } = "CentralAC";

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public int? InstallYear { get; set; }

    public string? FilterSize { get; set; }

    public DateTime? LastServiceDate { get; set; }

    public bool FilterRemindersEnabled { get; set; } = true;

    public string? LabelImagePath { get; set; }
}

public class HvacSetupStepViewModel
{
    public int PropiedadId { get; set; }

    public int CurrentStep { get; set; } = 1;

    public string Address { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = "/welcome-house.png";

    public string PageTitle { get; set; } = "Add HVAC System";

    public string BackUrl { get; set; } = "/";
}

public class HvacSetupStartViewModel : HvacSetupStepViewModel
{
}

public class HvacScanLabelViewModel : HvacSetupStepViewModel
{
    public string? ScanHint { get; set; }

    public string? PreviewImageUrl { get; set; }
}

public class AddHvacSystemViewModel : HvacSetupStepViewModel
{
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

public class HvacReviewViewModel : HvacSetupStepViewModel
{
    public string SystemType { get; set; } = "CentralAC";

    public string SystemTypeLabel { get; set; } = "Central AC";

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public int? InstallYear { get; set; }

    public string InstallYearLabel { get; set; } = "—";

    [Range(typeof(bool), "true", "true", ErrorMessage = "Please confirm your equipment information.")]
    public bool ConfirmInfo { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Please authorize INDOR to store this equipment data.")]
    public bool AuthorizeStorage { get; set; }

    public string? LabelImageUrl { get; set; }
}

public class HvacSavedViewModel : HvacSetupStepViewModel
{
    public string SystemTypeLabel { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? InstallYearLabel { get; set; }

    public string? EstimatedAgeLabel { get; set; }

    public string? EquipmentImageUrl { get; set; }

    public string? FilterSize { get; set; }

    public string? LastServiceLabel { get; set; }

    public bool FilterRemindersEnabled { get; set; }

    public int FilterReminderDays { get; set; }

    public bool FilterReminderSetupComplete { get; set; }

    public string? FilterFrequencyLabel { get; set; }

    public string NextReminderTitle { get; set; } = "HVAC filter replacement";

    public string NextReminderDueLabel { get; set; } = string.Empty;

    public string HouseFactsUrl { get; set; } = "#";

    public string MyHomeUrl { get; set; } = "/";

    public string AddAnotherUrl { get; set; } = "#";
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
        || !string.IsNullOrWhiteSpace(SerialNumber)
        || InstallYear.HasValue
        || !string.IsNullOrWhiteSpace(FilterSize);
}
