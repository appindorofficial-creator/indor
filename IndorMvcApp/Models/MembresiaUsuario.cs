using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

public class MembresiaUsuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int PlanMembresiaId { get; set; }

    [ForeignKey(nameof(PlanMembresiaId))]
    public PlanMembresia? Plan { get; set; }

    public DateTime FechaInicio { get; set; } = DateTime.Now;
    public DateTime? FechaFin { get; set; }
    public bool Activa { get; set; } = true;
}
