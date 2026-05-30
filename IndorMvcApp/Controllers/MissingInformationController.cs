using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class MissingInformationController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MissingInformationController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        ConfigureView(id);
        return View(MissingInformationDisplayService.BuildHub(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public async Task<IActionResult> Categories(int id)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        ConfigureView(id);
        return View(MissingInformationDisplayService.BuildCategories(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public async Task<IActionResult> Category(int id, string categoryId)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        var model = MissingInformationDisplayService.BuildCategory(bundle.Value.Propiedad, bundle.Value.Info, categoryId);
        if (model == null) return NotFound();
        ConfigureView(id);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Verify(int id, string itemId)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        var model = MissingInformationDisplayService.BuildVerify(bundle.Value.Propiedad, bundle.Value.Info, itemId);
        if (model == null) return NotFound();
        ConfigureView(id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, string itemId)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        return RedirectToAction(nameof(Updated), new { id, itemId });
    }

    [HttpGet]
    public async Task<IActionResult> Updated(int id, string? itemId)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        ConfigureView(id);
        return View(MissingInformationDisplayService.BuildUpdated(bundle.Value.Propiedad, bundle.Value.Info, itemId));
    }

    private void ConfigureView(int id)
    {
        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        HouseFactPreviewContext.ApplyReturnUrlToView(this);
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
