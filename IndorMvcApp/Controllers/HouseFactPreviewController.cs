using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class HouseFactPreviewController : Controller
{
    private readonly AppDbContext _db;

    public HouseFactPreviewController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Schools(string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();

        if (string.Equals(tab, "compare", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(SchoolsCompare));
        }

        if (string.Equals(tab, "district", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(SchoolsMap));
        }

        ConfigurePreview();
        return View("~/Views/Schools/Index.cshtml", SchoolsDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public IActionResult SchoolsMap()
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/Schools/Map.cshtml", SchoolsDisplayService.BuildMap(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public IActionResult SchoolsProfile(string schoolId)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        var model = SchoolsDisplayService.BuildProfile(bundle.Value.Propiedad, bundle.Value.Info, schoolId);
        if (model == null) return NotFound();
        ConfigurePreview();
        return View("~/Views/Schools/Profile.cshtml", model);
    }

    [HttpGet]
    public IActionResult SchoolsCompare()
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/Schools/Compare.cshtml", SchoolsDisplayService.BuildCompare(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public IActionResult PropertySnapshot(string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/PropertySnapshot/Index.cshtml", PropertySnapshotDisplayService.Build(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public IActionResult SystemsProfile(string? filter)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/SystemsProfile/Index.cshtml", SystemsProfileDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, filter));
    }

    [HttpGet]
    public IActionResult SystemsVerification()
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/SystemsProfile/Verification.cshtml", SystemsProfileDisplayService.BuildVerification(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public IActionResult SystemsDetail(string systemId, string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        var model = SystemsProfileDisplayService.BuildDetail(bundle.Value.Propiedad, bundle.Value.Info, systemId, tab);
        if (model == null) return NotFound();
        ConfigurePreview();
        return View("~/Views/SystemsProfile/Detail.cshtml", model);
    }

    [HttpGet]
    public IActionResult RoofExterior(string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/RoofExterior/Index.cshtml", RoofExteriorDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public async Task<IActionResult> RoofExteriorSection(string sectionId, string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        var priorities = await LoadPriorityIdsAsync();
        var model = RoofExteriorDisplayService.BuildSection(bundle.Value.Propiedad, bundle.Value.Info, sectionId, tab, priorities);
        if (model == null) return NotFound();
        ConfigurePreview();
        return View("~/Views/RoofExterior/Section.cshtml", model);
    }

    [HttpGet]
    public async Task<IActionResult> RoofExteriorCarePlan()
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        var priorities = await LoadPriorityIdsAsync();
        ConfigurePreview();
        return View("~/Views/RoofExterior/CarePlan.cshtml", RoofExteriorDisplayService.BuildCarePlan(bundle.Value.Propiedad, bundle.Value.Info, priorities));
    }

    [HttpGet]
    public IActionResult PermitsImprovements(string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/PermitsImprovements/Index.cshtml", PermitsImprovementsDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public IActionResult PermitsDetail(string permitId, string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        var model = PermitsImprovementsDisplayService.BuildDetail(bundle.Value.Propiedad, bundle.Value.Info, permitId, tab);
        if (model == null) return NotFound();
        ConfigurePreview();
        return View("~/Views/PermitsImprovements/Detail.cshtml", model);
    }

    [HttpGet]
    public IActionResult HoaCommunity(string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/HoaCommunity/Index.cshtml", HoaCommunityDisplayService.Build(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public IActionResult Utilities(string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/Utilities/Index.cshtml", UtilitiesDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public IActionResult UtilitiesDetail(string utilityId)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        var model = UtilitiesDisplayService.BuildDetail(bundle.Value.Propiedad, bundle.Value.Info, utilityId);
        if (model == null) return NotFound();
        ConfigurePreview();
        return View("~/Views/Utilities/Detail.cshtml", model);
    }

    [HttpGet]
    public IActionResult RiskScore(string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/RiskScore/Index.cshtml", RiskScoreDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public IActionResult RiskScoreChecklist(string? filter)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/RiskScore/Checklist.cshtml", RiskScoreDisplayService.BuildChecklist(bundle.Value.Propiedad, bundle.Value.Info, filter));
    }

    [HttpGet]
    public IActionResult MissingInformation()
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/MissingInformation/Index.cshtml", MissingInformationDisplayService.BuildHub(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public IActionResult MissingInformationCategories()
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/MissingInformation/Categories.cshtml", MissingInformationDisplayService.BuildCategories(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public IActionResult MissingInformationCategory(string categoryId)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        var model = MissingInformationDisplayService.BuildCategory(bundle.Value.Propiedad, bundle.Value.Info, categoryId);
        if (model == null) return NotFound();
        ConfigurePreview();
        return View("~/Views/MissingInformation/Category.cshtml", model);
    }

    [HttpGet]
    public IActionResult MissingInformationVerify(string itemId)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        var model = MissingInformationDisplayService.BuildVerify(bundle.Value.Propiedad, bundle.Value.Info, itemId);
        if (model == null) return NotFound();
        ConfigurePreview();
        return View("~/Views/MissingInformation/Verify.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MissingInformationComplete(string itemId)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        return RedirectToAction(nameof(MissingInformationUpdated), new { itemId });
    }

    [HttpGet]
    public IActionResult MissingInformationUpdated(string? itemId)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/MissingInformation/Updated.cshtml", MissingInformationDisplayService.BuildUpdated(bundle.Value.Propiedad, bundle.Value.Info, itemId));
    }

    [HttpGet]
    public IActionResult Documents(string? tab)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/Documents/Index.cshtml", DocumentsDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public IActionResult DocumentsDetail(string documentId)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        var model = DocumentsDisplayService.BuildDetail(bundle.Value.Propiedad, bundle.Value.Info, documentId);
        if (model == null) return NotFound();
        ConfigurePreview();
        return View("~/Views/Documents/Detail.cshtml", model);
    }

    [HttpGet]
    public IActionResult DocumentsAdd(string? category)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/Documents/Add.cshtml", DocumentsDisplayService.BuildAdd(bundle.Value.Propiedad, category));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DocumentsAdd(DocumentAddViewModel model)
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        return RedirectToAction(nameof(Documents));
    }

    [HttpGet]
    public IActionResult DocumentsRequests()
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/Documents/Requests.cshtml", DocumentsDisplayService.BuildRequests(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public IActionResult Parks()
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/NearbyPlaces/Parks.cshtml", NearbyPlacesDisplayService.BuildParks(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public IActionResult Airports()
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/NearbyPlaces/Airports.cshtml", NearbyPlacesDisplayService.BuildAirports(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public IActionResult Hospitals()
    {
        var bundle = LoadPreview();
        if (bundle == null) return MissingPreview();
        ConfigurePreview();
        return View("~/Views/NearbyPlaces/Hospitals.cshtml", NearbyPlacesDisplayService.BuildHospitals(bundle.Value.Propiedad, bundle.Value.Info));
    }

    private (Models.Propiedad Propiedad, ViewModels.PropertyInfoViewModel Info)? LoadPreview() =>
        HouseFactPreviewContext.LoadBundle(HttpContext.Session);

    private void ConfigurePreview() => HouseFactPreviewContext.ConfigurePreviewView(this);

    private IActionResult MissingPreview() => RedirectToAction("AddProperty", "Propietario");

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
