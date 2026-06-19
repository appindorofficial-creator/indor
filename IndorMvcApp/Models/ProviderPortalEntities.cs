using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("IndorProveedorCategoriasCatalogo")]
public class IndorProveedorCategoriaCatalogo
{
    [Key, MaxLength(40)]
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string LabelEn { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string IconClass { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DescriptionEn { get; set; }

    public int SortOrder { get; set; }

    public bool RequiresTradeExam { get; set; }

    public bool Activo { get; set; } = true;
}

[Table("IndorProveedorOfertasCatalogo")]
public class IndorProveedorOfertaCatalogo
{
    [Key, MaxLength(40)]
    public string Id { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string LabelEn { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string IconClass { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool Activo { get; set; } = true;
}

[Table("IndorProveedores")]
public class IndorProveedor
{
    public int Id { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    public Guid RegistrationToken { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)]
    public string RegistrationStatus { get; set; } = ProviderRegistrationStatuses.Draft;

    public int CurrentStep { get; set; } = 1;

    [Required, MaxLength(20)]
    public string ProviderType { get; set; } = "Company";

    [MaxLength(200)]
    public string? BusinessName { get; set; }

    [MaxLength(200)]
    public string? DbaName { get; set; }

    [MaxLength(120)]
    public string? PrimaryContact { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(40)]
    public string? YearsExperience { get; set; }

    [MaxLength(200)]
    public string? LanguagesJson { get; set; }

    [MaxLength(80)]
    public string? LicenseNumber { get; set; }

    [MaxLength(80)]
    public string? EpaCertificationNumber { get; set; }

    public bool BackgroundCheckConsent { get; set; }

    [MaxLength(200)]
    public string? ServiceDescription { get; set; }

    public bool IsInsured { get; set; }

    public bool IsLicensed { get; set; }

    [MaxLength(40)]
    public string? TeamSize { get; set; }

    [MaxLength(300)]
    public string? BusinessAddress { get; set; }

    [MaxLength(120)]
    public string? PrimaryCity { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Longitude { get; set; }

    public int TravelRadiusMiles { get; set; } = 25;

    [MaxLength(500)]
    public string? ZipNeighborhoodsJson { get; set; }

    public bool EmergencyService { get; set; } = true;

    public bool SameDayJobs { get; set; } = true;

    [MaxLength(80)]
    public string? AvailableDaysJson { get; set; }

    [MaxLength(60)]
    public string? PreferredHours { get; set; }

    [MaxLength(120)]
    public string? JobSizesJson { get; set; }

    public bool LogoUploaded { get; set; }

    public bool ScopeTradeUnderstood { get; set; }

    public bool ScopeStandardsAgreed { get; set; }

    public int? ExamScorePercent { get; set; }

    public bool? ExamPassed { get; set; }

    public DateTime? ExamSubmittedUtc { get; set; }

    public DateTime? ProfileSubmittedUtc { get; set; }

    [MaxLength(2000)]
    public string? OnboardingMetaJson { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<IndorProveedorCategoriaSel> Categorias { get; set; } = [];

    public ICollection<IndorProveedorOfertaSel> Ofertas { get; set; } = [];

    public ICollection<IndorProveedorExamRespuesta> ExamRespuestas { get; set; } = [];

    public ICollection<IndorProveedorDocumento> Documentos { get; set; } = [];
}

public static class ProviderRegistrationStatuses
{
    public const string Draft = "Draft";
    public const string IndorProActive = "IndorProActive";
    public const string Submitted = "Submitted";
    public const string PendingReview = "PendingReview";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

[Table("IndorProveedorCategoriasSel")]
public class IndorProveedorCategoriaSel
{
    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    [MaxLength(40)]
    public string CategoriaId { get; set; } = string.Empty;

    [ForeignKey(nameof(CategoriaId))]
    public IndorProveedorCategoriaCatalogo? Categoria { get; set; }
}

[Table("IndorProveedorOfertasSel")]
public class IndorProveedorOfertaSel
{
    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    [MaxLength(40)]
    public string OfertaId { get; set; } = string.Empty;

    [ForeignKey(nameof(OfertaId))]
    public IndorProveedorOfertaCatalogo? Oferta { get; set; }
}

[Table("IndorProveedorExamPreguntas")]
public class IndorProveedorExamPregunta
{
    [MaxLength(30)]
    public string TradeCode { get; set; } = "electrical";

    public int QuestionNumber { get; set; }

    public int PageNumber { get; set; }

    [Required, MaxLength(500)]
    public string TextEn { get; set; } = string.Empty;

    [Required]
    public string OptionsJson { get; set; } = "[]";

    public int CorrectIndex { get; set; }

    public bool Activo { get; set; } = true;
}

[Table("IndorProveedorExamRespuestas")]
public class IndorProveedorExamRespuesta
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    [MaxLength(30)]
    public string TradeCode { get; set; } = "electrical";

    public int QuestionNumber { get; set; }

    public int SelectedIndex { get; set; }

    public bool IsCorrect { get; set; }

    public DateTime AnsweredUtc { get; set; } = DateTime.UtcNow;
}

[Table("IndorProveedorDocumentos")]
public class IndorProveedorDocumento
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    [Required, MaxLength(40)]
    public string DocumentType { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Required";

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    public DateTime? UploadedUtc { get; set; }
}

[Table("IndorProveedorAlcanceReglas")]
public class IndorProveedorAlcanceRegla
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string TradeCode { get; set; } = "electrical";

    [Required, MaxLength(120)]
    public string LabelEn { get; set; } = string.Empty;

    public bool IsAllowed { get; set; }

    public int SortOrder { get; set; }

    public bool Activo { get; set; } = true;
}
