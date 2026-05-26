using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class ScheduleMicroservicioViewModel
{
    public int MicroservicioId { get; set; }

    public string NombreMicroservicio { get; set; } = string.Empty;

    public string? SubtituloMicroservicio { get; set; }

    [Required(ErrorMessage = "Please select a date.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Scheduled date")]
    public DateTime FechaProgramada { get; set; } = DateTime.Today.AddDays(7);

    public bool HasExistingSchedule { get; set; }

    [StringLength(500)]
    [Display(Name = "Notes (optional)")]
    public string? Notas { get; set; }
}
