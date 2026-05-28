using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("HomeCarePrioritiesConfig")]
public class HomeCarePrioritiesConfig
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Titulo { get; set; } = "Home Care Guide";

    [Required, MaxLength(200)]
    public string Subtitulo { get; set; } = "Stay ahead of important home maintenance.";

    [MaxLength(50)]
    public string IconoClase { get; set; } = "fa-shield-halved";

    [MaxLength(40)]
    public string ViewAllTexto { get; set; } = "View all tasks";

    [MaxLength(80)]
    public string? ViewAllController { get; set; } = "MyHome";

    [MaxLength(80)]
    public string? ViewAllAction { get; set; } = "Maintenance";

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("HomeCarePriorities")]
public class HomeCarePriority
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Subtitulo { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [MaxLength(50)]
    public string IconoClase { get; set; } = "fa-wrench";

    [MaxLength(80)]
    public string? LinkController { get; set; } = "MyHome";

    [MaxLength(80)]
    public string? LinkAction { get; set; } = "Maintenance";

    [MaxLength(300)]
    public string? LinkUrl { get; set; }

    public int Orden { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
