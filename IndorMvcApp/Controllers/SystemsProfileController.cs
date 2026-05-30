using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class SystemsProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SystemsProfileController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id, string? filter)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        return View(SystemsProfileDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, filter));
    }

    [HttpGet]
    public async Task<IActionResult> Verification(int id)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        return View(SystemsProfileDisplayService.BuildVerification(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, string systemId, string? tab)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        var model = SystemsProfileDisplayService.BuildDetail(bundle.Value.Propiedad, bundle.Value.Info, systemId, tab);
        if (model == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        return View(model);
    }

    private async Task<(Propiedad Propiedad, PropertyInfoViewModel? Info)?> LoadBundleAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        var propiedad = await _db.Propiedades
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && p.Activo);

        if (propiedad == null) return null;

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return (propiedad, info);
    }
}
