using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("PropiedadHistorial")]
public class PropiedadHistorial
{
    public int Id { get; set; }

    public int PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(30)]
    public string RecordType { get; set; } = "Improvement";

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? ProviderName { get; set; }

    public int? PropiedadProveedorId { get; set; }

    [ForeignKey(nameof(PropiedadProveedorId))]
    public PropiedadProveedor? Proveedor { get; set; }

    public DateTime? CompletionDate { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? TotalCost { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(30)]
    public string? WarrantyStatus { get; set; }

    [MaxLength(20)]
    public string Source { get; set; } = "User";

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}

[Table("PropiedadProveedores")]
public class PropiedadProveedor
{
    public int Id { get; set; }

    public int PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string ServiceCategory { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(300)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(20)]
    public string Source { get; set; } = "User";

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("PropiedadMantenimiento")]
public class PropiedadMantenimiento
{
    public int Id { get; set; }

    public int PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Upcoming";

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? PropiedadProveedorId { get; set; }

    [ForeignKey(nameof(PropiedadProveedorId))]
    public PropiedadProveedor? Proveedor { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}

[Table("PropiedadDocumentos")]
public class PropiedadDocumento
{
    public int Id { get; set; }

    public int PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(40)]
    public string Category { get; set; } = "Other";

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(260)]
    public string? FileName { get; set; }

    [MaxLength(500)]
    public string? StoragePath { get; set; }

    [MaxLength(80)]
    public string? ContentType { get; set; }

    public long? SizeBytes { get; set; }

    public DateTime? InspectionDate { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
