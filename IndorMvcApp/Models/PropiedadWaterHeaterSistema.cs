using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("PropiedadWaterHeaterSistemas")]
public class PropiedadWaterHeaterSistema
{
    public int Id { get; set; }

    public int PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(20)]
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

    public DateTime? LastServiceDate { get; set; }

    public bool FlushRemindersEnabled { get; set; } = true;

    public int FlushReminderDays { get; set; } = 365;

    public DateTime? NextFlushDate { get; set; }

    [MaxLength(80)]
    public string? FlushLocation { get; set; }

    public bool RemindOneWeekBefore { get; set; } = true;

    public bool RemindOneDayBefore { get; set; } = true;

    public bool AutoRepeatEnabled { get; set; } = true;

    public bool FlushReminderSetupComplete { get; set; }

    public bool FlushNotificationsConsent { get; set; }

    [MaxLength(120)]
    public string? OpenAiDataSource { get; set; }

    [MaxLength(300)]
    public string? LabelImagePath { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}
