using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class HvacFilterFlowViewModel
{
    public int PropiedadId { get; set; }

    public int Step { get; set; } = 1;

    public string? FilterSize { get; set; }
}

public class HvacFilterPetsViewModel : HvacFilterFlowViewModel
{
    public bool? HasPets { get; set; }
}

public class HvacFilterScheduleViewModel : HvacFilterFlowViewModel
{
    public bool HasPets { get; set; }

    [Required]
    public string ScheduleMode { get; set; } = "Every2Months";
}

public class HvacFilterChooseDateViewModel : HvacFilterFlowViewModel
{
    public bool HasPets { get; set; }

    public string ScheduleMode { get; set; } = "Every2Months";

    public string FrequencyLabel { get; set; } = "Every 2 months";

    [Required]
    [DataType(DataType.Date)]
    public DateTime? NextChangeDate { get; set; }
}

public class HvacFilterNotificationsViewModel : HvacFilterFlowViewModel
{
    public bool HasPets { get; set; }

    public string FrequencyLabel { get; set; } = string.Empty;

    public string NextChangeLabel { get; set; } = string.Empty;

    public bool RemindOneWeekBefore { get; set; } = true;

    public bool RemindOneDayBefore { get; set; } = true;

    public bool FilterNotificationsConsent { get; set; }
}
