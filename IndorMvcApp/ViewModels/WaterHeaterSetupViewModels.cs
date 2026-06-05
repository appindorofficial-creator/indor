using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class WaterHeaterSetupDraft
{
    public int PropiedadId { get; set; }

    public string EntryMode { get; set; } = "manual";

    public string HeaterType { get; set; } = "Tank";

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public int? InstallYear { get; set; }

    public string? TankSize { get; set; }

    public DateTime? LastServiceDate { get; set; }

    public bool FlushRemindersEnabled { get; set; } = true;

    public string? LabelImagePath { get; set; }
}

public class WaterHeaterSetupStepViewModel
{
    public int PropiedadId { get; set; }

    public int CurrentStep { get; set; } = 1;

    public string Address { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = "/welcome-house.png";

    public string PageTitle { get; set; } = "Scan Water Heater Label";

    public string BackUrl { get; set; } = "/";
}

public class WaterHeaterSetupStartViewModel : WaterHeaterSetupStepViewModel
{
}

public class WaterHeaterScanLabelViewModel : WaterHeaterSetupStepViewModel
{
    public string? ScanHint { get; set; }

    public string? PreviewImageUrl { get; set; }
}

public class AddWaterHeaterViewModel : WaterHeaterSetupStepViewModel
{
    public string? OpenAiHintNote { get; set; }

    [Required(ErrorMessage = "Select a water heater type.")]
    public string HeaterType { get; set; } = "Tank";

    [MaxLength(80)]
    public string? Brand { get; set; }

    [MaxLength(80)]
    public string? Model { get; set; }

    [MaxLength(80)]
    public string? SerialNumber { get; set; }

    public int? InstallYear { get; set; }

    [MaxLength(40)]
    public string? TankSize { get; set; }

    [DataType(DataType.Date)]
    public DateTime? LastServiceDate { get; set; }

    public bool FlushRemindersEnabled { get; set; } = true;

    public List<int> InstallYearOptions { get; set; } = new();

    public List<string> TankSizeOptions { get; set; } = new();
}

public class WaterHeaterDetailsFoundViewModel : WaterHeaterSetupStepViewModel
{
    public string HeaterTypeLabel { get; set; } = "Tank";

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string InstallYearLabel { get; set; } = "—";

    public string TankSizeLabel { get; set; } = "—";

    public string? EstimatedAgeLabel { get; set; }

    public string? LabelImageUrl { get; set; }
}

public class WaterHeaterReviewViewModel : WaterHeaterSetupStepViewModel
{
    public string HeaterType { get; set; } = "Tank";

    public string HeaterTypeLabel { get; set; } = "Tank";

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public int? InstallYear { get; set; }

    public string InstallYearLabel { get; set; } = "—";

    public string? TankSize { get; set; }

    public string TankSizeLabel { get; set; } = "—";

    public string? LabelImageUrl { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Please confirm before saving.")]
    public bool ConfirmSave { get; set; }
}

public class WaterHeaterSavedViewModel : WaterHeaterSetupStepViewModel
{
    public string HeaterTypeLabel { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? InstallYearLabel { get; set; }

    public string? TankSizeLabel { get; set; }

    public string? EstimatedAgeLabel { get; set; }

    public string? EquipmentImageUrl { get; set; }

    public string? LastServiceLabel { get; set; }

    public bool FlushRemindersEnabled { get; set; }

    public bool FlushReminderSetupComplete { get; set; }

    public int FlushReminderMonths { get; set; } = 12;

    public string NextReminderTitle { get; set; } = "Annual water heater flush";

    public string NextReminderDueLabel { get; set; } = string.Empty;

    public string HouseFactsUrl { get; set; } = "#";

    public string MyHomeUrl { get; set; } = "/";
}

public class WaterHeaterOpenAiHints
{
    public string? HeaterType { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public int? InstallYear { get; set; }

    public string? TankSize { get; set; }

    public DateTime? LastServiceDate { get; set; }

    public string? DataSource { get; set; }

    public bool HasAny =>
        !string.IsNullOrWhiteSpace(HeaterType)
        || !string.IsNullOrWhiteSpace(Brand)
        || !string.IsNullOrWhiteSpace(Model)
        || !string.IsNullOrWhiteSpace(SerialNumber)
        || InstallYear.HasValue
        || !string.IsNullOrWhiteSpace(TankSize);
}
