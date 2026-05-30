using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class AddWaterHeaterViewModel
{
    public int PropiedadId { get; set; }

    public string Address { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = "/welcome-house.png";

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

public class WaterHeaterSavedViewModel
{
    public int PropiedadId { get; set; }

    public string HeaterTypeLabel { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? InstallYearLabel { get; set; }

    public string? TankSizeLabel { get; set; }

    public string? LastServiceLabel { get; set; }

    public bool FlushRemindersEnabled { get; set; }

    public bool FlushReminderSetupComplete { get; set; }

    public int FlushReminderMonths { get; set; } = 12;

    public string NextReminderTitle { get; set; } = "Annual water heater flush";

    public string NextReminderDueLabel { get; set; } = string.Empty;

    public string HouseFactsUrl { get; set; } = "#";
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
        || InstallYear.HasValue
        || !string.IsNullOrWhiteSpace(TankSize);
}
