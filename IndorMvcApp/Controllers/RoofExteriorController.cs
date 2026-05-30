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
public class RoofExteriorController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoofExteriorController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id, string? tab)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        HouseFactPreviewContext.ApplyReturnUrlToView(this);
        return View(RoofExteriorDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public async Task<IActionResult> Section(int id, string sectionId, string? tab)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        var priorities = await LoadPriorityIdsAsync();
        var model = RoofExteriorDisplayService.BuildSection(
            bundle.Value.Propiedad,
            bundle.Value.Info,
            sectionId,
            tab,
            priorities);

        if (model == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CarePlan(int id)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        var priorities = await LoadPriorityIdsAsync();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        return View(RoofExteriorDisplayService.BuildCarePlan(bundle.Value.Propiedad, bundle.Value.Info, priorities));
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

    private async Task<Dictionary<string, int>> LoadPriorityIdsAsync()
    {
        try
        {
            return await _db.HomeCarePriorities
                .AsNoTracking()
                .Where(p => p.Activo)
                .ToDictionaryAsync(p => p.Nombre, p => p.Id);
        }
        catch
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
