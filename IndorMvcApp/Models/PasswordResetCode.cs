using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("IndorPasswordResetCodes")]
public class IndorPasswordResetCode
{
    public int Id { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    /// <summary>ASP.NET Identity password reset token used to actually change the password.</summary>
    [Required, MaxLength(2000)]
    public string ResetToken { get; set; } = string.Empty;

    public DateTime ExpiresUtc { get; set; }

    public bool Used { get; set; }

    public DateTime? UsedUtc { get; set; }

    public int Attempts { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
