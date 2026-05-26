using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

public class MensajeSoporte
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    [Required, StringLength(20)]
    public string Remitente { get; set; } = "Usuario"; // Usuario | Soporte

    [Required]
    public string Contenido { get; set; } = string.Empty;

    public DateTime Fecha { get; set; } = DateTime.Now;
    public bool Leido { get; set; }
}
