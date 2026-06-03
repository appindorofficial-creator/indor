using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Controllers;

[Authorize]
public class ProveedorController(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null ||
            !string.Equals(user.RolUsuario, "ProveedorServicios", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home");
        }

        if (!await userManager.IsInRoleAsync(user, "ProveedorServicios"))
        {
            await userManager.AddToRoleAsync(user, "ProveedorServicios");
            await signInManager.RefreshSignInAsync(user);
        }

        var userId = user.Id;
        var proveedor = await db.IndorProveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (proveedor == null ||
            proveedor.RegistrationStatus == ProviderRegistrationStatuses.Draft)
        {
            return RedirectToAction("Categories", "ProviderRegistration");
        }

        ViewBag.CompanyName = !string.IsNullOrWhiteSpace(proveedor.DbaName)
            ? proveedor.DbaName
            : proveedor.BusinessName ?? proveedor.PrimaryContact;
        ViewBag.Status = proveedor.RegistrationStatus;

        return View();
    }
}
