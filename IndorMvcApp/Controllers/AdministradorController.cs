using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndorMvcApp.Controllers;

[Authorize]
public class AdministradorController(
    IPropertyAdministratorRegistrationService registration,
    IPropertyAdministratorPortalService portal,
    IPropertyAdministratorEmergencyAcService emergencyAc,
    IPropertyAdministratorEmergencyElectricalService emergencyElectrical,
    IPropertyAdministratorEmergencyPlumbingService emergencyPlumbing,
    IPropertyAdministratorEmergencyRoofLeakService emergencyRoofLeak,
    IPropertyAdministratorEmergencyTreeBranchService emergencyTreeBranch,
    IPropertyAdministratorLockoutAccessService lockoutAccess,
    IPropertyAdministratorBrokenWindowBoardUpService brokenWindowBoardUp,
    IPropertyAdministratorEmergencySewerBackupService emergencySewerBackup,
    IPropertyAdministratorEmergencyWaterHeaterService emergencyWaterHeater,
    IPropertyAdministratorEmergencyFloodService emergencyFlood,
    IPropertyAdministratorPreventiveMaintenanceService preventiveMaintenance,
    IPropertyAdministratorAirFilterService airFilter,
    IPropertyAdministratorSmokeDetectorService smokeDetector,
    IPropertyAdministratorTurnoverCleaningService turnoverCleaning,
    IPropertyAdministratorStandardCleaningService standardCleaning,
    IPropertyAdministratorPetDeepCleanService petDeepClean,
    IPropertyAdministratorMovingHelpService movingHelp,
    IPropertyAdministratorJunkRemovalService junkRemoval,
    IPropertyAdministratorFurnitureHaulAwayService furnitureHaulAway,
    IPropertyAdministratorTrashOutService trashOut,
    IPropertyAdministratorLawnCareService lawnCare,
    IPropertyAdministratorLandscapingService landscaping,
    IPropertyAdministratorPressureWashingService pressureWashing,
    IPropertyAdministratorPestControlService pestControl,
    IPropertyAdministratorPoolHotTubService poolHotTub,
    HomeownerNearbyNetworkService nearbyNetwork,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Controller
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            context.Result = Challenge();
            return;
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null ||
            !string.Equals(user.RolUsuario, "AdministradorPropiedades", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = RedirectToAction("Index", "Home");
            return;
        }

        await next();
    }

    private async Task<IActionResult?> EnsureRegisteredAsync()
    {
        var admin = await registration.GetAdministratorForCurrentUserAsync();
        if (admin == null || !registration.IsRegistrationComplete(admin))
        {
            return RedirectToAction("Profile", "PropertyAdministratorRegistration");
        }

        return null;
    }

    [HttpGet]
    public IActionResult Dashboard() => RedirectToAction(nameof(Index));

    [HttpGet]
    public async Task<IActionResult> Index(int? propertyId, string? view, string? filter, string? q)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.NavActive = "home";
        var model = await portal.GetHomeAsync(Url, propertyId);
        var activePropertyId = propertyId ?? model.ViewingProperty?.Id;

        model.NetworkView = string.Equals(view, "map", StringComparison.OrdinalIgnoreCase) ? "map" : "feed";
        model.NetworkFilter = string.IsNullOrWhiteSpace(filter) ? "All" : filter.Trim();
        model.NetworkSearch = q;
        if (model.NetworkView == "map")
        {
            var centerAddress = model.ViewingProperty?.Location;
            var servicesUrl = Url.Action(nameof(Services), "Administrador") ?? "#";
            var messageUrl = Url.Action(nameof(Services), "Administrador") ?? "#";
            model.NearbyMap = await nearbyNetwork.BuildMapForAddressAsync(
                centerAddress,
                "Your portfolio",
                filter,
                Url,
                servicesUrl,
                messageUrl);
        }

        model.FeaturedPoolHotTub = poolHotTub.BuildFeaturedCta(Url, activePropertyId);
        model.FeaturedPestControl = null;
        model.FeaturedPressureWashing = null;
        model.FeaturedLandscaping = null;
        model.FeaturedLawnCare = null;
        model.FeaturedTrashOut = null;
        model.FeaturedFurnitureHaulAway = null;
        model.FeaturedJunkRemoval = null;
        model.FeaturedMovingHelp = null;
        model.FeaturedPetDeepClean = null;
        model.FeaturedStandardCleaning = null;
        model.FeaturedTurnoverCleaning = null;
        model.FeaturedSmokeDetector = null;
        model.FeaturedAirFilter = null;
        model.FeaturedPreventive = null;
        model.FeaturedEmergency = null;
        model.NearestPro = null;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyElectricalDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await emergencyElectrical.GetFormAsync(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyElectricalDetails(PropertyAdministratorEmergencyElectricalSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await emergencyElectrical.SubmitAsync(input);
        return RedirectToAction(nameof(EmergencyElectricalConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyElectricalConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await emergencyElectrical.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyAcDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await emergencyAc.GetFormAsync(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyAcDetails(PropertyAdministratorEmergencyAcSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await emergencyAc.SubmitAsync(input);
        return RedirectToAction(nameof(EmergencyAcConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyAcConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await emergencyAc.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyPlumbingDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await emergencyPlumbing.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyPlumbingDetails(PropertyAdministratorEmergencyPlumbingStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["PlumbingStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(EmergencyPlumbingAccess));
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyPlumbingAccess()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["PlumbingStep1"] is not string json)
        {
            return RedirectToAction(nameof(EmergencyPlumbingDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorEmergencyPlumbingStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(EmergencyPlumbingDetails));
        }

        var model = await emergencyPlumbing.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(EmergencyPlumbingDetails));
        }

        ViewBag.HideBottomNav = true;
        ViewBag.BadgeLabel = model.GuestsOnSiteLabel;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyPlumbingAccess(PropertyAdministratorEmergencyPlumbingSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await emergencyPlumbing.SubmitAsync(input);
        return RedirectToAction(nameof(EmergencyPlumbingConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyPlumbingConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await emergencyPlumbing.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyRoofLeakDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await emergencyRoofLeak.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyRoofLeakDetails(PropertyAdministratorEmergencyRoofLeakStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["RoofLeakStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(EmergencyRoofLeakAccess));
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyRoofLeakAccess()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["RoofLeakStep1"] is not string json)
        {
            return RedirectToAction(nameof(EmergencyRoofLeakDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorEmergencyRoofLeakStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(EmergencyRoofLeakDetails));
        }

        var model = await emergencyRoofLeak.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(EmergencyRoofLeakDetails));
        }

        ViewBag.HideBottomNav = true;
        ViewBag.BadgeLabel = model.GuestsOnSiteLabel;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyRoofLeakAccess(PropertyAdministratorEmergencyRoofLeakSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await emergencyRoofLeak.SubmitAsync(input);
        return RedirectToAction(nameof(EmergencyRoofLeakConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyRoofLeakConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await emergencyRoofLeak.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyTreeBranchDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await emergencyTreeBranch.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyTreeBranchDetails(PropertyAdministratorEmergencyTreeBranchStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["TreeBranchStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(EmergencyTreeBranchReview));
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyTreeBranchReview()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["TreeBranchStep1"] is not string json)
        {
            return RedirectToAction(nameof(EmergencyTreeBranchDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorEmergencyTreeBranchStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(EmergencyTreeBranchDetails));
        }

        var model = await emergencyTreeBranch.GetReviewAsync(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(EmergencyTreeBranchDetails));
        }

        ViewBag.HideBottomNav = true;
        ViewBag.BadgeLabel = model.GuestsOnSiteLabel;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyTreeBranchReview(PropertyAdministratorEmergencyTreeBranchSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await emergencyTreeBranch.SubmitAsync(input);
        return RedirectToAction(nameof(EmergencyTreeBranchConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyTreeBranchConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await emergencyTreeBranch.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> LockoutAccessDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await lockoutAccess.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockoutAccessDetails(PropertyAdministratorLockoutAccessStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["LockoutAccessStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(LockoutAccessEntry));
    }

    [HttpGet]
    public async Task<IActionResult> LockoutAccessEntry()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["LockoutAccessStep1"] is not string json)
        {
            return RedirectToAction(nameof(LockoutAccessDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorLockoutAccessStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(LockoutAccessDetails));
        }

        var model = await lockoutAccess.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(LockoutAccessDetails));
        }

        ViewBag.HideBottomNav = true;
        ViewBag.BadgeLabel = model.GuestsWaitingLabel;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockoutAccessEntry(PropertyAdministratorLockoutAccessSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await lockoutAccess.SubmitAsync(input);
        return RedirectToAction(nameof(LockoutAccessConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> LockoutAccessConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await lockoutAccess.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> BrokenWindowDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await brokenWindowBoardUp.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BrokenWindowDetails(PropertyAdministratorBrokenWindowBoardUpStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["BrokenWindowStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(BrokenWindowAccess));
    }

    [HttpGet]
    public async Task<IActionResult> BrokenWindowAccess()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["BrokenWindowStep1"] is not string json)
        {
            return RedirectToAction(nameof(BrokenWindowDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorBrokenWindowBoardUpStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(BrokenWindowDetails));
        }

        var model = await brokenWindowBoardUp.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(BrokenWindowDetails));
        }

        ViewBag.HideBottomNav = true;
        ViewBag.BadgeLabel = model.GuestsOnSiteLabel;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BrokenWindowAccess(PropertyAdministratorBrokenWindowBoardUpSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await brokenWindowBoardUp.SubmitAsync(input);
        return RedirectToAction(nameof(BrokenWindowConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> BrokenWindowConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await brokenWindowBoardUp.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> SewerBackupDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await emergencySewerBackup.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SewerBackupDetails(PropertyAdministratorEmergencySewerBackupStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["SewerBackupStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(SewerBackupReview));
    }

    [HttpGet]
    public async Task<IActionResult> SewerBackupReview()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["SewerBackupStep1"] is not string json)
        {
            return RedirectToAction(nameof(SewerBackupDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorEmergencySewerBackupStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(SewerBackupDetails));
        }

        var model = await emergencySewerBackup.GetReviewAsync(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(SewerBackupDetails));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SewerBackupReview(PropertyAdministratorEmergencySewerBackupSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await emergencySewerBackup.SubmitAsync(input);
        return RedirectToAction(nameof(SewerBackupConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> SewerBackupConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await emergencySewerBackup.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> WaterHeaterDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await emergencyWaterHeater.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WaterHeaterDetails(PropertyAdministratorEmergencyWaterHeaterStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["WaterHeaterStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(WaterHeaterReview));
    }

    [HttpGet]
    public async Task<IActionResult> WaterHeaterReview()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["WaterHeaterStep1"] is not string json)
        {
            return RedirectToAction(nameof(WaterHeaterDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorEmergencyWaterHeaterStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(WaterHeaterDetails));
        }

        var model = await emergencyWaterHeater.GetReviewAsync(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(WaterHeaterDetails));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WaterHeaterReview(PropertyAdministratorEmergencyWaterHeaterSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await emergencyWaterHeater.SubmitAsync(input);
        return RedirectToAction(nameof(WaterHeaterConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> WaterHeaterConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await emergencyWaterHeater.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyFloodDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await emergencyFlood.GetFormAsync(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyFloodDetails(PropertyAdministratorEmergencyFloodSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await emergencyFlood.SubmitAsync(input);
        return RedirectToAction(nameof(EmergencyFloodConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyFloodConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await emergencyFlood.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> PoolHotTubDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await poolHotTub.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PoolHotTubDetails(PropertyAdministratorPoolHotTubStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["PoolHotTubStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(PoolHotTubAccess));
    }

    [HttpGet]
    public async Task<IActionResult> PoolHotTubAccess()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["PoolHotTubStep1"] is not string json)
        {
            return RedirectToAction(nameof(PoolHotTubDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorPoolHotTubStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(PoolHotTubDetails));
        }

        var model = await poolHotTub.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(PoolHotTubDetails));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PoolHotTubAccess(PropertyAdministratorPoolHotTubSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (input.UpdateRecipientsList.Count == 0)
        {
            input.UpdateRecipientsList = ["Me"];
        }

        var requestId = await poolHotTub.SubmitAsync(input);
        return RedirectToAction(nameof(PoolHotTubConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> PoolHotTubConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await poolHotTub.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> PestControlDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await pestControl.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PestControlDetails(PropertyAdministratorPestControlStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["PestControlStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(PestControlSchedule));
    }

    [HttpGet]
    public async Task<IActionResult> PestControlSchedule()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["PestControlStep1"] is not string json)
        {
            return RedirectToAction(nameof(PestControlDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorPestControlStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(PestControlDetails));
        }

        var model = await pestControl.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(PestControlDetails));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PestControlSchedule(PropertyAdministratorPestControlSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (input.TreatAreasList.Count == 0)
        {
            input.TreatAreasList = ["Kitchen"];
        }

        if (input.UpdateRecipientsList.Count == 0)
        {
            input.UpdateRecipientsList = ["Me"];
        }

        var requestId = await pestControl.SubmitAsync(input);
        return RedirectToAction(nameof(PestControlConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> PestControlConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await pestControl.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> PressureWashingDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await pressureWashing.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PressureWashingDetails(PropertyAdministratorPressureWashingStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (input.WashAreasList.Count == 0)
        {
            input.WashAreasList = ["Walkway"];
        }

        TempData["PressureWashingStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(PressureWashingSchedule));
    }

    [HttpGet]
    public async Task<IActionResult> PressureWashingSchedule()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["PressureWashingStep1"] is not string json)
        {
            return RedirectToAction(nameof(PressureWashingDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorPressureWashingStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(PressureWashingDetails));
        }

        var model = await pressureWashing.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(PressureWashingDetails));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PressureWashingSchedule(PropertyAdministratorPressureWashingSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (input.WashAreasList.Count == 0)
        {
            input.WashAreasList = ["Walkway"];
        }

        if (input.UpdateRecipientsList.Count == 0)
        {
            input.UpdateRecipientsList = ["Me"];
        }

        var requestId = await pressureWashing.SubmitAsync(input);
        return RedirectToAction(nameof(PressureWashingConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> PressureWashingConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await pressureWashing.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> LandscapingDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await landscaping.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LandscapingDetails(PropertyAdministratorLandscapingStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["LandscapingStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(LandscapingScope));
    }

    [HttpGet]
    public async Task<IActionResult> LandscapingScope()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["LandscapingStep1"] is not string json)
        {
            return RedirectToAction(nameof(LandscapingDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorLandscapingStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(LandscapingDetails));
        }

        var model = await landscaping.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(LandscapingDetails));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LandscapingScope(PropertyAdministratorLandscapingSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (input.UpdateRecipientsList.Count == 0)
        {
            input.UpdateRecipientsList = ["Me"];
        }

        var requestId = await landscaping.SubmitAsync(input);
        return RedirectToAction(nameof(LandscapingConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> LandscapingConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await landscaping.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> LawnCareDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await lawnCare.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LawnCareDetails(PropertyAdministratorLawnCareStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["LawnCareStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(LawnCareSchedule));
    }

    [HttpGet]
    public async Task<IActionResult> LawnCareSchedule()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["LawnCareStep1"] is not string json)
        {
            return RedirectToAction(nameof(LawnCareDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorLawnCareStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(LawnCareDetails));
        }

        var model = await lawnCare.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(LawnCareDetails));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LawnCareSchedule(PropertyAdministratorLawnCareSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (input.UpdateRecipientsList.Count == 0)
        {
            input.UpdateRecipientsList = ["Me"];
        }

        var requestId = await lawnCare.SubmitAsync(input);
        return RedirectToAction(nameof(LawnCareConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> LawnCareConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await lawnCare.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> TrashOutDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await trashOut.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrashOutDetails(PropertyAdministratorTrashOutStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["TrashOutStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(TrashOutAccess));
    }

    [HttpGet]
    public async Task<IActionResult> TrashOutAccess()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["TrashOutStep1"] is not string json)
        {
            return RedirectToAction(nameof(TrashOutDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorTrashOutStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(TrashOutDetails));
        }

        var model = await trashOut.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(TrashOutDetails));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrashOutAccess(PropertyAdministratorTrashOutSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (input.UpdateRecipientsList.Count == 0)
        {
            input.UpdateRecipientsList = ["Me"];
        }

        var requestId = await trashOut.SubmitAsync(input);
        return RedirectToAction(nameof(TrashOutConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> TrashOutConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await trashOut.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> FurnitureHaulAwayDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await furnitureHaulAway.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FurnitureHaulAwayDetails(PropertyAdministratorFurnitureHaulAwayStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["FurnitureHaulAwayStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(FurnitureHaulAwayAccess));
    }

    [HttpGet]
    public async Task<IActionResult> FurnitureHaulAwayAccess()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["FurnitureHaulAwayStep1"] is not string json)
        {
            return RedirectToAction(nameof(FurnitureHaulAwayDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorFurnitureHaulAwayStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(FurnitureHaulAwayDetails));
        }

        var model = await furnitureHaulAway.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(FurnitureHaulAwayDetails));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FurnitureHaulAwayAccess(PropertyAdministratorFurnitureHaulAwaySubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (input.UpdateRecipientsList.Count == 0)
        {
            input.UpdateRecipientsList = ["Me"];
        }

        var requestId = await furnitureHaulAway.SubmitAsync(input);
        return RedirectToAction(nameof(FurnitureHaulAwayConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> FurnitureHaulAwayConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await furnitureHaulAway.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> JunkRemovalDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await junkRemoval.GetStep1Async(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JunkRemovalDetails(PropertyAdministratorJunkRemovalStep1Input input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        TempData["JunkRemovalStep1"] = System.Text.Json.JsonSerializer.Serialize(input);
        return RedirectToAction(nameof(JunkRemovalAccess));
    }

    [HttpGet]
    public async Task<IActionResult> JunkRemovalAccess()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (TempData["JunkRemovalStep1"] is not string json)
        {
            return RedirectToAction(nameof(JunkRemovalDetails));
        }

        var step1 = System.Text.Json.JsonSerializer.Deserialize<PropertyAdministratorJunkRemovalStep1Input>(json);
        if (step1 == null)
        {
            return RedirectToAction(nameof(JunkRemovalDetails));
        }

        var model = await junkRemoval.GetStep2Async(Url, step1);
        if (model == null)
        {
            return RedirectToAction(nameof(JunkRemovalDetails));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JunkRemovalAccess(PropertyAdministratorJunkRemovalSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (input.UpdateRecipientsList.Count == 0)
        {
            input.UpdateRecipientsList = ["Me"];
        }

        var requestId = await junkRemoval.SubmitAsync(input);
        return RedirectToAction(nameof(JunkRemovalConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> JunkRemovalConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await junkRemoval.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> MovingHelpDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await movingHelp.GetFormAsync(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MovingHelpDetails(PropertyAdministratorMovingHelpSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (input.UpdateRecipientsList.Count == 0)
        {
            input.UpdateRecipientsList = ["Me", "CoHost"];
        }

        ViewBag.HideBottomNav = true;
        return View("MovingHelpReview", await movingHelp.GetReviewAsync(Url, input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MovingHelpReview(PropertyAdministratorMovingHelpSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await movingHelp.SubmitAsync(input);
        return RedirectToAction(nameof(MovingHelpConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> MovingHelpConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await movingHelp.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> PetDeepCleanDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await petDeepClean.GetFormAsync(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PetDeepCleanDetails(PropertyAdministratorPetDeepCleanSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await petDeepClean.SubmitAsync(input);
        return RedirectToAction(nameof(PetDeepCleanConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> PetDeepCleanConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await petDeepClean.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> StandardCleaningDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await standardCleaning.GetFormAsync(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StandardCleaningDetails(PropertyAdministratorStandardCleaningSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await standardCleaning.SubmitAsync(input);
        return RedirectToAction(nameof(StandardCleaningConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> StandardCleaningConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await standardCleaning.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> TurnoverCleaningDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await turnoverCleaning.GetFormAsync(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TurnoverCleaningDetails(PropertyAdministratorTurnoverCleaningSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await turnoverCleaning.SubmitAsync(input);
        return RedirectToAction(nameof(TurnoverCleaningConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> TurnoverCleaningConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await turnoverCleaning.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> SmokeDetectorDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await smokeDetector.GetFormAsync(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SmokeDetectorDetails(PropertyAdministratorSmokeDetectorSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await smokeDetector.SubmitAsync(input);
        return RedirectToAction(nameof(SmokeDetectorConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> SmokeDetectorConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await smokeDetector.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AirFilterDetails(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        return View(await airFilter.GetFormAsync(Url, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AirFilterDetails(PropertyAdministratorAirFilterSubmitInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var requestId = await airFilter.SubmitAsync(input);
        return RedirectToAction(nameof(AirFilterConfirmed), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> AirFilterConfirmed(int id)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await airFilter.GetConfirmedAsync(Url, id);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.HideBottomNav = true;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> PreventiveMaintenanceServices(int? propertyId, int? planId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        ViewBag.FlowStep = 2;
        ViewBag.FlowTotalSteps = 4;
        ViewBag.FlowBackUrl = Url.Action(nameof(Index), new { propertyId });
        var model = await preventiveMaintenance.GetServicesStepAsync(Url, propertyId, planId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreventiveMaintenanceServices(PropertyAdministratorPreventiveServicesStepInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var planId = await preventiveMaintenance.SaveServicesStepAsync(input);
        return RedirectToAction(nameof(PreventiveMaintenanceSchedule), new { planId });
    }

    [HttpGet]
    public async Task<IActionResult> PreventiveMaintenanceSchedule(int planId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.FlowStep = 3;
        ViewBag.FlowTotalSteps = 4;
        var model = await preventiveMaintenance.GetScheduleStepAsync(planId);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.FlowBackUrl = Url.Action(nameof(PreventiveMaintenanceServices), new { planId, propertyId = model.ViewingProperty?.Id });
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreventiveMaintenanceSchedule(PropertyAdministratorPreventiveScheduleStepInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        await preventiveMaintenance.SaveScheduleStepAsync(input);
        return RedirectToAction(nameof(PreventiveMaintenanceReview), new { planId = input.PlanId });
    }

    [HttpGet]
    public async Task<IActionResult> PreventiveMaintenanceReview(int planId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.HideBottomNav = true;
        ViewBag.FlowStep = 4;
        ViewBag.FlowTotalSteps = 4;
        ViewBag.FlowBackUrl = Url.Action(nameof(PreventiveMaintenanceSchedule), new { planId });
        var model = await preventiveMaintenance.GetReviewStepAsync(planId);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivatePreventivePlan(int planId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        await preventiveMaintenance.ActivatePlanAsync(planId);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Calendar()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.NavActive = "calendar";
        return View(await portal.GetCalendarAsync(Url));
    }

    [HttpGet]
    public async Task<IActionResult> Properties(string? from)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.NavActive = "properties";
        return View(await portal.GetPropertiesAsync(Url, from));
    }

    [HttpGet]
    public async Task<IActionResult> PropertyDetail(int id, string? tab)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        var model = await portal.GetPropertyDetailAsync(Url, id, tab);
        if (model == null)
        {
            return NotFound();
        }

        ViewBag.NavActive = "properties";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Services(string? filter)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.NavActive = "services";
        return View(await portal.GetServicesAsync(Url, filter));
    }

    [HttpGet]
    public async Task<IActionResult> Tasks(string? filter)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.NavActive = "tasks";
        return View(await portal.GetTasksAsync(Url, filter));
    }

    [HttpGet]
    public async Task<IActionResult> RequestService(string? service)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        return RedirectToAction(nameof(Services));
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        return View(await portal.GetProfileAsync(Url));
    }

    [HttpGet]
    public async Task<IActionResult> PersonalInformation()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        return View(await portal.GetPersonalInformationAsync(Url));
    }

    [HttpGet]
    public async Task<IActionResult> NotificationPreferences(bool? saved)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        return View(await portal.GetNotificationPreferencesAsync(Url, saved == true));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NotificationPreferences(PropertyAdministratorNotificationPreferencesInput input)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (!await portal.SaveNotificationPreferencesAsync(input))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(NotificationPreferences), new { saved = true });
    }

    [HttpGet]
    public async Task<IActionResult> Security(bool? saved, string? error)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        return View(await portal.GetSecurityAsync(Url, saved == true, error));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Security(string currentPassword, string newPassword, string confirmPassword)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
        {
            return View(await portal.GetSecurityAsync(
                Url,
                errorMessage: "Passwords do not match.",
                currentPassword: currentPassword,
                newPassword: newPassword,
                confirmPassword: confirmPassword));
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword ?? string.Empty, newPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return View(await portal.GetSecurityAsync(
                Url,
                errorMessage: message,
                currentPassword: currentPassword,
                newPassword: newPassword,
                confirmPassword: confirmPassword));
        }

        await signInManager.SignInAsync(user, isPersistent: true);
        return RedirectToAction(nameof(Security), new { saved = true });
    }

    [HttpGet]
    public async Task<IActionResult> PaymentsBilling()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        return View(await portal.GetPaymentsBillingAsync(Url));
    }

    [HttpGet]
    public async Task<IActionResult> SavedProviders()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        return View(await portal.GetSavedProvidersAsync(Url));
    }

    [HttpGet]
    public async Task<IActionResult> HelpSupport()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        return View(await portal.GetHelpSupportAsync(Url));
    }
}
