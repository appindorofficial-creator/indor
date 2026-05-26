using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class StructuralMoreDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? UbicacionEspecifica { get; set; }

    [MaxLength(50)]
    public string? CuandoNotadoTexto { get; set; }

    [MaxLength(20)]
    public string? DuracionProblema { get; set; }

    [MaxLength(20)]
    public string? Severidad { get; set; }

    [MaxLength(20)]
    public string? ReparacionesPrevias { get; set; }

    [MaxLength(200)]
    public string? CondicionesInseguras { get; set; }

    [MaxLength(20)]
    public string? MejorHorarioVisita { get; set; }
}
