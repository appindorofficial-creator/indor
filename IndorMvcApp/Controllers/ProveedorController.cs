using IndorMvcApp.Models;
using IndorMvcApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

[Authorize]
public class ProveedorController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IProviderRegistrationService registration) : Controller
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

        var proveedor = await registration.GetProveedorForCurrentUserAsync();
        if (proveedor == null ||
            proveedor.RegistrationStatus == ProviderRegistrationStatuses.Draft)
        {
            return RedirectToAction("Categories", "ProviderRegistration");
        }

        ViewBag.CompanyName = !string.IsNullOrWhiteSpace(proveedor.DbaName)
            ? proveedor.DbaName
            : proveedor.BusinessName ?? proveedor.PrimaryContact;
        ViewBag.ContactEmail = proveedor.Email ?? user.Email;
        ViewBag.Status = proveedor.RegistrationStatus;
        ViewBag.ExamScore = proveedor.ExamScorePercent;
        ViewBag.ExamPassed = proveedor.ExamPassed;

        ViewBag.Trades = proveedor.Categorias
            .Select(c => c.Categoria?.LabelEn ?? c.CategoriaId)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        ViewBag.Services = proveedor.Ofertas
            .Select(o => o.Oferta?.LabelEn ?? o.OfertaId)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        ViewBag.Documents = proveedor.Documentos
            .Where(d => !string.IsNullOrWhiteSpace(d.FileUrl))
            .Select(d =>
            {
                var trade = proveedor.Categorias.FirstOrDefault()?.CategoriaId;
                var slot = ProviderDocumentTypes.GetSlotsForTrade(trade).FirstOrDefault(s =>
                    s.Type.Equals(d.DocumentType, StringComparison.OrdinalIgnoreCase));
                return string.IsNullOrEmpty(slot.Type) ? d.DocumentType : slot.Label;
            })
            .ToList();

        return View();
    }
}
