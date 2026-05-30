using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("PropiedadHvacSistemas")]
public class PropiedadHvacSistema
{
    public int Id { get; set; }

    public int PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(30)]
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

    public DateTime? LastServiceDate { get; set; }

    public bool FilterRemindersEnabled { get; set; } = true;

    public int FilterReminderDays { get; set; } = 90;

    [MaxLength(120)]
    public string? OpenAiDataSource { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}
