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
    IPropertyAdministratorEmergencyFloodService emergencyFlood,
    IPropertyAdministratorPreventiveMaintenanceService preventiveMaintenance,
    IPropertyAdministratorAirFilterService airFilter,
    IPropertyAdministratorSmokeDetectorService smokeDetector,
    IPropertyAdministratorTurnoverCleaningService turnoverCleaning,
    IPropertyAdministratorStandardCleaningService standardCleaning,
    IPropertyAdministratorPetDeepCleanService petDeepClean,
    UserManager<ApplicationUser> userManager) : Controller
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
    public async Task<IActionResult> Index(int? propertyId)
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.NavActive = "home";
        var model = await portal.GetHomeAsync(Url, propertyId);
        var activePropertyId = propertyId ?? model.ViewingProperty?.Id;
        model.FeaturedPetDeepClean = petDeepClean.BuildFeaturedCta(Url, activePropertyId);
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
    public async Task<IActionResult> Properties()
    {
        if (await EnsureRegisteredAsync() is { } redirect)
        {
            return redirect;
        }

        ViewBag.NavActive = "properties";
        return View(await portal.GetPropertiesAsync());
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

        ViewBag.NavActive = "services";
        ViewBag.ServiceSlug = service;
        return View();
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
}
