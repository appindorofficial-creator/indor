using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class WaterHeaterFlushReminderIntroViewModel
{
    public int PropiedadId { get; set; }

    public string HeaterTypeLabel { get; set; } = "Tank water heaters";
}

public class WaterHeaterFlushReminderSetupViewModel
{
    public int PropiedadId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime? NextFlushDate { get; set; }

    [MaxLength(80)]
    public string FlushLocation { get; set; } = "Basement";

    public bool RemindOneWeekBefore { get; set; } = true;

    public bool RemindOneDayBefore { get; set; } = true;

    public bool AutoRepeatEnabled { get; set; } = true;

    public bool FlushNotificationsConsent { get; set; }

    public List<string> LocationOptions { get; set; } = new();
}

public class WaterHeaterFlushReminderSavedViewModel
{
    public int PropiedadId { get; set; }

    public string LocationLabel { get; set; } = "Basement";

    public string NextFlushLabel { get; set; } = string.Empty;

    public string RemindersLabel { get; set; } = string.Empty;

    public bool AutoRepeatEnabled { get; set; } = true;
}
