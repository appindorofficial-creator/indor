using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

/// <summary>
/// A social post published to the INDOR Neighborhood feed, scoped to a ZIP code.
/// Neighbors from the same ZIP see each other's updates, experiences and local help.
/// </summary>
[Table("IndorNeighborhoodPosts")]
public class IndorNeighborhoodPost
{
    public int Id { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    /// <summary>5-digit ZIP the post belongs to. Feed is filtered by this.</summary>
    [Required, MaxLength(12)]
    public string ZipCode { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string AuthorName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AuthorPhotoUrl { get; set; }

    /// <summary>Category code used for the coloured tag (Construction, Home, HVAC, ...).</summary>
    [MaxLength(40)]
    public string? CategoryCode { get; set; }

    /// <summary>Post type shown as the corner badge (WorkDone, NeedHelp, Sale, Event, ...).</summary>
    [MaxLength(30)]
    public string? PostType { get; set; }

    /// <summary>Audience scope label ("Public", "Neighbors").</summary>
    [MaxLength(20)]
    public string Audience { get; set; } = "Public";

    [MaxLength(200)]
    public string? Title { get; set; }

    [Required, MaxLength(2000)]
    public string Body { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImagePath { get; set; }

    [MaxLength(200)]
    public string? LocationLabel { get; set; }

    /// <summary>Optional linked verified provider ("Ver proveedor").</summary>
    public int? ProveedorId { get; set; }

    public int LikeCount { get; set; }

    public int CommentCount { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedUtc { get; set; }

    public ICollection<IndorNeighborhoodComment> Comments { get; set; } = [];

    public ICollection<IndorNeighborhoodPostMedia> Media { get; set; } = [];
}

/// <summary>A photo or video attached to a neighborhood post.</summary>
[Table("IndorNeighborhoodPostMedia")]
public class IndorNeighborhoodPostMedia
{
    public int Id { get; set; }

    public int PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public IndorNeighborhoodPost? Post { get; set; }

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>"image" or "video".</summary>
    [Required, MaxLength(20)]
    public string MediaType { get; set; } = "image";

    public int SortOrder { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

[Table("IndorNeighborhoodComments")]
public class IndorNeighborhoodComment
{
    public int Id { get; set; }

    public int PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public IndorNeighborhoodPost? Post { get; set; }

    /// <summary>When set, this comment is a reply to another comment (one level deep).</summary>
    public int? ParentCommentId { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string AuthorName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AuthorPhotoUrl { get; set; }

    [MaxLength(12)]
    public string? ZipCode { get; set; }

    [Required, MaxLength(1000)]
    public string Body { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Tracks a unique like per user/post so likes can be toggled.</summary>
[Table("IndorNeighborhoodPostLikes")]
public class IndorNeighborhoodPostLike
{
    public int Id { get; set; }

    public int PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public IndorNeighborhoodPost? Post { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Tracks a post saved by a user ("Mis Guardados" → Posts tab).</summary>
[Table("IndorNeighborhoodPostSaves")]
public class IndorNeighborhoodPostSave
{
    public int Id { get; set; }

    public int PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public IndorNeighborhoodPost? Post { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Tracks an individual comment saved as a tip ("Mis Guardados" → Comentarios tab).</summary>
[Table("IndorNeighborhoodCommentSaves")]
public class IndorNeighborhoodCommentSave
{
    public int Id { get; set; }

    public int CommentId { get; set; }

    [ForeignKey(nameof(CommentId))]
    public IndorNeighborhoodComment? Comment { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Categories used for the coloured tags on neighborhood posts.</summary>
public static class NeighborhoodPostCategories
{
    public sealed record Category(string Code, string LabelEn, string LabelEs, string Css);

    public static readonly IReadOnlyList<Category> All =
    [
        new("Construction", "Construction", "Construcción", "construction"),
        new("Home", "Home", "Hogar", "home"),
        new("HVAC", "HVAC", "HVAC", "hvac"),
        new("Plumbing", "Plumbing", "Plomería", "plumbing"),
        new("Services", "Services", "Servicios", "services"),
        new("Recommendation", "Recommendation", "Recomendación", "recommend"),
        new("Question", "Question", "Pregunta", "question"),
        new("Other", "Other", "Otro", "other")
    ];

    // Legacy codes from earlier posts mapped to the closest current category.
    private static readonly IReadOnlyDictionary<string, string> LegacyMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["General"] = "Home",
            ["Roofing"] = "Construction",
            ["Landscaping"] = "Services",
            ["Electrical"] = "Services"
        };

    public static Category Resolve(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return All[0];
        }

        if (LegacyMap.TryGetValue(code, out var mapped))
        {
            code = mapped;
        }

        foreach (var category in All)
        {
            if (string.Equals(category.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return category;
            }
        }

        return All[0];
    }
}

/// <summary>Post types (what kind of publication) shown as the corner badge.</summary>
public static class NeighborhoodPostTypes
{
    public sealed record PostType(string Code, string LabelEn, string LabelEs, string Css, string Icon);

    public static readonly IReadOnlyList<PostType> All =
    [
        new("WorkDone", "Work completed", "Trabajo terminado", "workdone", "fa-circle-check"),
        new("NeedHelp", "Need help", "Necesito ayuda", "needhelp", "fa-hand"),
        new("Recommendation", "Recommendation", "Recomendación", "recommend", "fa-star"),
        new("Question", "Question", "Pregunta", "question", "fa-circle-question"),
        new("BeforeAfter", "Before / After", "Antes / Después", "beforeafter", "fa-images"),
        new("Sale", "Sale", "Venta", "sale", "fa-tag"),
        new("Service", "Service", "Servicio", "service", "fa-screwdriver-wrench"),
        new("Event", "Event", "Evento", "event", "fa-calendar-day")
    ];

    public static PostType? Resolve(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        foreach (var type in All)
        {
            if (string.Equals(type.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return type;
            }
        }

        return null;
    }
}

/// <summary>Audience options for a post.</summary>
public static class NeighborhoodAudiences
{
    public sealed record Audience(string Code, string LabelEn, string LabelEs, string Icon);

    public static readonly IReadOnlyList<Audience> All =
    [
        new("Public", "Public", "Público", "fa-globe"),
        new("Neighbors", "Neighbors only", "Solo vecinos", "fa-user-group")
    ];

    public static Audience Resolve(string? code)
    {
        foreach (var a in All)
        {
            if (string.Equals(a.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return a;
            }
        }

        return All[0];
    }
}
