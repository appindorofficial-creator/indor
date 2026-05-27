using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("MovingSetupConfig")]
public class MovingSetupConfig
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Titulo { get; set; } = "Moving Setup";

    [Required, MaxLength(200)]
    public string Subtitulo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string IconoClase { get; set; } = "fa-box-open";

    [MaxLength(40)]
    public string ViewAllTexto { get; set; } = "View all";

    [MaxLength(200)]
    public string? ViewAllUrl { get; set; }

    [MaxLength(40)]
    public string FeaturedEtiqueta { get; set; } = "FEATURED";

    [Required, MaxLength(120)]
    public string FeaturedTitulo { get; set; } = "Moving Assistant";

    [Required, MaxLength(300)]
    public string FeaturedDescripcion { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? FeaturedImagenUrl { get; set; }

    [MaxLength(500)]
    public string? FeaturedCaracteristicas { get; set; }

    [MaxLength(200)]
    public string? FeaturedIconosCaracteristicas { get; set; }

    [MaxLength(80)]
    public string FeaturedCtaTexto { get; set; } = "Start moving setup";

    [MaxLength(80)]
    public string? FeaturedCtaController { get; set; }

    [MaxLength(80)]
    public string? FeaturedCtaAction { get; set; }

    public int? FeaturedCtaRouteId { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("MovingSetupServicios")]
public class MovingSetupServicio
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string IconoClase { get; set; } = "fa-house";

    [MaxLength(80)]
    public string? LinkController { get; set; }

    [MaxLength(80)]
    public string? LinkAction { get; set; }

    public int? LinkRouteId { get; set; }

    public int Orden { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("MovingSetupEnlacesRapidos")]
public class MovingSetupEnlaceRapido
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string IconoClase { get; set; } = "fa-clipboard-list";

    [MaxLength(80)]
    public string? LinkController { get; set; }

    [MaxLength(80)]
    public string? LinkAction { get; set; }

    public int? LinkRouteId { get; set; }

    [MaxLength(300)]
    public string? LinkUrl { get; set; }

    public int Orden { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
