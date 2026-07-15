using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndorMvcApp.Localization;

namespace IndorMvcApp.Models;

[Table("ServiciosEmergencia")]
public class ServicioEmergencia
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string TituloEmergencia { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? TituloEmergenciaEs { get; set; }

    [Required, MaxLength(300)]
    public string Descripcion { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? DescripcionEs { get; set; }

    public int TiempoLlegadaMinutos { get; set; } = 45;

    [MaxLength(50)]
    public string IconoClase { get; set; } = "fa-droplet";

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [MaxLength(80)]
    public string? BadgeTexto { get; set; }

    [MaxLength(80)]
    public string? BadgeTextoEs { get; set; }

    public bool EsPredeterminado { get; set; }

    [MaxLength(500)]
    public string? Caracteristicas { get; set; }

    [MaxLength(500)]
    public string? CaracteristicasEs { get; set; }

    [MaxLength(200)]
    public string? IconosCaracteristicas { get; set; }

    [MaxLength(80)]
    public string CtaTexto { get; set; } = "Request help";

    [MaxLength(80)]
    public string? CtaTextoEs { get; set; }

    public bool Activo { get; set; } = true;

    public int Orden { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public string LocalizedTitulo(bool isSpanish) => CatalogText.PickWithUiFallback(TituloEmergencia, TituloEmergenciaEs, isSpanish);
    public string LocalizedDescripcion(bool isSpanish) => CatalogText.PickWithUiFallback(Descripcion, DescripcionEs, isSpanish);
    public string LocalizedCtaTexto(bool isSpanish) => CatalogText.PickWithUiFallback(CtaTexto, CtaTextoEs, isSpanish);
    public string? LocalizedCaracteristicas(bool isSpanish) => CatalogText.PickPipeListWithUiFallback(Caracteristicas, CaracteristicasEs, isSpanish);
    public string? LocalizedBadgeTexto(bool isSpanish) => CatalogText.PickWithUiFallback(BadgeTexto, BadgeTextoEs, isSpanish);
}
