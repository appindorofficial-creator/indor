using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class PropietarioController : Controller
{
    private const string OnboardingPropertySessionKey = "OnboardingPropertyInfo";
    private const string OnboardingPropertyAttomSessionKey = "OnboardingPropertyAttomJson";
    private static readonly TimeSpan AddressLookupTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan MaintenanceTimeout = TimeSpan.FromSeconds(90);
    private static readonly JsonSerializerOptions OnboardingJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IAddressLookupService _addressLookupService;
    private readonly IOpenAiMaintenanceRecommendationService _maintenanceRecommendationService;
    private readonly ILogger<PropietarioController> _logger;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PropietarioController(
        IAddressLookupService addressLookupService,
        IOpenAiMaintenanceRecommendationService maintenanceRecommendationService,
        ILogger<PropietarioController> logger,
        AppDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _addressLookupService = addressLookupService;
        _maintenanceRecommendationService = maintenanceRecommendationService;
        _logger = logger;
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> AddProperty()
    {
        if (await UserHasPropertyAsync())
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.OnboardingStep = 3;
        ViewBag.OnboardingTitle = "Your property";
        ViewBag.OnboardingBackUrl = Url.Action("SelectRole", "Account", new { userId = _userManager.GetUserId(User) });
        ViewBag.OnboardingShowBack = false;
        return View(new AddPropertyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestTimeout("OnboardingAddressLookup")]
    public async Task<IActionResult> AddProperty(AddPropertyViewModel model)
    {
        if (await UserHasPropertyAsync())
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            SetPropertyStepViewBag(showBack: false);
            return View(model);
        }

        try
        {
            var lookupAddress = model.BuildLookupAddress();
            _logger.LogInformation("Looking up address: {Address}", lookupAddress);

            var propertyInfo = await _addressLookupService
                .GetPropertyInfoAsync(lookupAddress)
                .WaitAsync(AddressLookupTimeout, HttpContext.RequestAborted);

            if (propertyInfo == null)
            {
                ModelState.AddModelError("", "No information was found for this address. Try a more specific address.");
                SetPropertyStepViewBag(showBack: false);
                return View(model);
            }

            ApplyUserAddressFields(propertyInfo, model);
            SaveOnboardingProperty(propertyInfo);
            return RedirectToAction(nameof(PropertyDetails));
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Address lookup timed out for {Address}", model.BuildLookupAddress());
            ModelState.AddModelError("", "Address lookup is taking longer than expected. Please try again in a moment.");
            SetPropertyStepViewBag(showBack: false);
            return View(model);
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogWarning("Address lookup cancelled for {Address}", model.BuildLookupAddress());
            ModelState.AddModelError("", "Address lookup was interrupted. Please try again.");
            SetPropertyStepViewBag(showBack: false);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error looking up address");
            ModelState.AddModelError("", DescribeAddressLookupError(ex));
            SetPropertyStepViewBag(showBack: false);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult ConfirmProperty()
    {
        var propertyInfo = LoadOnboardingProperty();
        if (propertyInfo == null)
        {
            return RedirectToAction(nameof(AddProperty));
        }

        ViewBag.OnboardingStep = 2;
        ViewBag.OnboardingTitle = "Create Account";
        ViewBag.OnboardingBackUrl = Url.Action(nameof(AddProperty));
        ViewBag.OnboardingShowBack = true;
        return View(new AddPropertyViewModel
        {
            Address = propertyInfo.FormattedAddress,
            Unit = propertyInfo.Unit
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmProperty(string? unit)
    {
        var propertyInfo = LoadOnboardingProperty();
        if (propertyInfo == null)
        {
            return RedirectToAction(nameof(AddProperty));
        }

        propertyInfo.Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        SaveOnboardingProperty(propertyInfo);
        return RedirectToAction(nameof(ReviewAccept));
    }

    [HttpGet]
    public async Task<IActionResult> ReviewAccept()
    {
        var propertyInfo = LoadOnboardingProperty();
        if (propertyInfo == null)
        {
            return RedirectToAction(nameof(AddProperty));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        ViewBag.OnboardingStep = 3;
        ViewBag.OnboardingTitle = "Create Account";
        ViewBag.OnboardingBackUrl = Url.Action(nameof(ConfirmProperty));
        ViewBag.OnboardingShowBack = true;

        var fullName = UserDisplayName.Format(user);
        ViewBag.MaintenanceSection = PropertyMaintenanceDisplayService.BuildSection(
            propertyInfo.MaintenanceRecommendations, compact: true);

        return View(new OnboardingReviewViewModel
        {
            FullName = string.IsNullOrWhiteSpace(fullName) ? user.Email ?? string.Empty : fullName,
            Email = user.Email ?? string.Empty,
            Phone = string.IsNullOrWhiteSpace(user.Telefono) ? user.PhoneNumber : user.Telefono,
            Address = propertyInfo.FormattedAddress,
            Unit = propertyInfo.Unit
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("ReviewAccept")]
    public async Task<IActionResult> ReviewAcceptPost()
    {
        var propertyInfo = LoadOnboardingProperty();
        if (propertyInfo == null)
        {
            return RedirectToAction(nameof(AddProperty));
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        try
        {
            var propiedadId = await SavePropertyAsync(propertyInfo, userId);
            ClearOnboardingProperty();
            TempData["OnboardingComplete"] = true;
            return RedirectToAction(nameof(HomeReady), new { id = propiedadId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving property during onboarding");
            return RedirectToAction(nameof(ReviewAccept));
        }
    }

    [HttpGet]
    public IActionResult HomeReady(int id)
    {
        ViewBag.PropiedadId = id;
        return View();
    }

    [HttpGet]
    [RequestTimeout("OnboardingPropertyDetails")]
    public async Task<IActionResult> PropertyDetails()
    {
        var propertyInfo = LoadOnboardingProperty();
        if (propertyInfo == null)
        {
            return RedirectToAction(nameof(AddProperty));
        }

        if (propertyInfo.MaintenanceRecommendations == null)
        {
            propertyInfo.MaintenanceRecommendations = await TryGenerateMaintenanceAsync(propertyInfo);
            SaveOnboardingProperty(propertyInfo);
        }

        HouseFactPreviewContext.Save(HttpContext.Session, propertyInfo);
        ViewBag.HouseFactProfile = HouseFactDisplayService.BuildProfile(
            propertyInfo.AttomRawJson,
            propertyInfo.DataSource,
            propertyInfo.FormattedAddress);
        ViewBag.MaintenanceSection = PropertyMaintenanceDisplayService.BuildSection(
            propertyInfo.MaintenanceRecommendations);

        return View(propertyInfo);
    }

    [HttpGet]
    public IActionResult EditAddress()
    {
        var propertyInfo = LoadOnboardingProperty();
        if (propertyInfo == null)
        {
            return RedirectToAction(nameof(AddProperty));
        }

        HouseFactPreviewContext.Save(HttpContext.Session, propertyInfo);
        return View(propertyInfo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(PropertyInfoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var fullModel = MergeEditAddressModel(model);
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        try
        {
            await SavePropertyAsync(fullModel, userId);
            ClearOnboardingProperty();
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving edited address");
            ModelState.AddModelError("", "An error occurred while saving the address. Please try again.");
            return View(model);
        }
    }

    private PropertyInfoViewModel? LoadOnboardingProperty()
    {
        var sessionJson = HttpContext.Session.GetString(OnboardingPropertySessionKey);
        PropertyInfoViewModel? propertyInfo = null;

        if (!string.IsNullOrWhiteSpace(sessionJson))
        {
            propertyInfo = JsonSerializer.Deserialize<PropertyInfoViewModel>(sessionJson, OnboardingJsonOptions);
        }
        else if (TempData["PropertyInfoJson"] is string json && !string.IsNullOrEmpty(json))
        {
            TempData.Keep("PropertyInfoJson");
            propertyInfo = JsonSerializer.Deserialize<PropertyInfoViewModel>(json, OnboardingJsonOptions);
        }

        if (propertyInfo == null)
        {
            return null;
        }

        var attomJson = HttpContext.Session.GetString(OnboardingPropertyAttomSessionKey);
        if (!string.IsNullOrWhiteSpace(attomJson))
        {
            propertyInfo.AttomRawJson = attomJson;
        }

        return propertyInfo;
    }

    private void SaveOnboardingProperty(PropertyInfoViewModel info)
    {
        var attomJson = info.AttomRawJson;
        info.AttomRawJson = null;

        var json = JsonSerializer.Serialize(info, OnboardingJsonOptions);
        HttpContext.Session.SetString(OnboardingPropertySessionKey, json);

        if (!string.IsNullOrWhiteSpace(attomJson))
        {
            HttpContext.Session.SetString(OnboardingPropertyAttomSessionKey, attomJson);
        }
        else
        {
            HttpContext.Session.Remove(OnboardingPropertyAttomSessionKey);
        }

        TempData["PropertyInfoJson"] = json;
    }

    private void ClearOnboardingProperty()
    {
        HttpContext.Session.Remove(OnboardingPropertySessionKey);
        HttpContext.Session.Remove(OnboardingPropertyAttomSessionKey);
    }

    private static void ApplyUserAddressFields(PropertyInfoViewModel propertyInfo, AddPropertyViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Unit))
        {
            propertyInfo.Unit = model.Unit.Trim();
        }

        if (string.IsNullOrWhiteSpace(propertyInfo.Street))
        {
            propertyInfo.Street = model.StreetAddress.Trim();
        }

        if (string.IsNullOrWhiteSpace(propertyInfo.City))
        {
            propertyInfo.City = model.City.Trim();
        }

        if (string.IsNullOrWhiteSpace(propertyInfo.State))
        {
            propertyInfo.State = model.State.Trim().ToUpperInvariant();
        }

        if (string.IsNullOrWhiteSpace(propertyInfo.PostalCode))
        {
            propertyInfo.PostalCode = model.ZipCode.Trim();
        }

        propertyInfo.Country ??= "US";

        if (string.IsNullOrWhiteSpace(propertyInfo.FormattedAddress))
        {
            propertyInfo.FormattedAddress = model.BuildLookupAddress();
        }
    }

    private async Task<PropertyMaintenancePlanViewModel> TryGenerateMaintenanceAsync(
        PropertyInfoViewModel propertyInfo)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
            cts.CancelAfter(MaintenanceTimeout);

            var plan = await _maintenanceRecommendationService.GenerateAsync(propertyInfo, cts.Token);
            if (PropertyMaintenanceDisplayService.IsRealAiPlan(plan))
            {
                _logger.LogInformation(
                    "OpenAI maintenance plan generated for {Address} ({Count} items)",
                    propertyInfo.FormattedAddress,
                    plan.Items.Count);
                return plan;
            }

            _logger.LogWarning(
                "OpenAI maintenance unavailable for {Address}: {Reason}",
                propertyInfo.FormattedAddress,
                plan.Summary);
            return plan;
        }
        catch (Exception ex) when (ex is OperationCanceledException or TimeoutException)
        {
            _logger.LogWarning(ex, "Maintenance recommendation timed out for {Address}", propertyInfo.FormattedAddress);
            return BuildUnavailableMaintenancePlan(
                "Maintenance suggestions are still loading. You can continue and review them later.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Maintenance recommendation failed for {Address}", propertyInfo.FormattedAddress);
            return BuildUnavailableMaintenancePlan(
                "We couldn't generate maintenance suggestions right now. You can continue onboarding.");
        }
    }

    private static PropertyMaintenancePlanViewModel BuildUnavailableMaintenancePlan(string message) =>
        new()
        {
            Summary = message,
            DataSource = "Unavailable",
            IsAiGenerated = false,
            GeneratedUtc = DateTime.UtcNow,
            Items = []
        };

    private static string DescribeAddressLookupError(Exception ex) =>
        ex switch
        {
            InvalidOperationException => ex.Message,
            HttpRequestException => "We couldn't reach the address lookup service. Check your connection and try again.",
            _ => "An error occurred while processing the address. Please try again."
        };

    private async Task<bool> UserHasPropertyAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return false;
        return await _db.Propiedades.AnyAsync(p => p.UserId == userId && p.Activo);
    }

    private async Task<int> SavePropertyAsync(PropertyInfoViewModel fullModel, string userId)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var attomRawJson = fullModel.AttomRawJson;
        fullModel.AttomRawJson = null;
        var jsonRaw = JsonSerializer.Serialize(fullModel, jsonOptions);
        var hasAttomPayload = !string.IsNullOrWhiteSpace(attomRawJson);

        var propiedad = new Propiedad
        {
            Direccion = FormatAddressWithUnit(fullModel.FormattedAddress, fullModel.Unit),
            DatosJson = jsonRaw,
            UserId = userId,
            FechaCreacion = DateTime.Now,
            Activo = true,
            AttomPropertyId = fullModel.AttomPropertyId,
            AttomRawJson = attomRawJson,
            AttomLastSyncUtc = hasAttomPayload ? DateTime.UtcNow : null,
            AttomSyncStatus = IsSuccessfulEnrichment(fullModel.DataSource) || fullModel.AttomPropertyId.HasValue
                ? "Success"
                : (hasAttomPayload ? "Partial" : "Estimated"),
            AttomSyncError = hasAttomPayload || IsSuccessfulEnrichment(fullModel.DataSource)
                ? null
                : "Property enrichment not available"
        };

        if (PropertyMaintenanceDisplayService.IsRealAiPlan(fullModel.MaintenanceRecommendations))
        {
            var maintenancePlan = fullModel.MaintenanceRecommendations!;
            maintenancePlan.GeneratedUtc ??= DateTime.UtcNow;
            propiedad.MantenimientoRecomendadoJson = JsonSerializer.Serialize(maintenancePlan, jsonOptions);
            propiedad.MantenimientoRecomendadoUtc = maintenancePlan.GeneratedUtc;
        }

        _db.Propiedades.Add(propiedad);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Property saved (Id={Id}) for user {UserId}", propiedad.Id, userId);
        return propiedad.Id;
    }

    private PropertyInfoViewModel MergeEditAddressModel(PropertyInfoViewModel model)
    {
        var originalJson = Request.Form["OriginalJson"].ToString();
        PropertyInfoViewModel fullModel;
        if (!string.IsNullOrWhiteSpace(originalJson))
        {
            fullModel = JsonSerializer.Deserialize<PropertyInfoViewModel>(originalJson) ?? new PropertyInfoViewModel();
        }
        else
        {
            fullModel = model;
        }

        fullModel.FormattedAddress = model.FormattedAddress;
        fullModel.Street = model.Street;
        fullModel.HouseNumber = model.HouseNumber;
        fullModel.City = model.City;
        fullModel.State = model.State;
        fullModel.PostalCode = model.PostalCode;
        fullModel.Country = model.Country;
        fullModel.PropertyDetails ??= new PropertyDetailsInfo();
        if (model.PropertyDetails != null)
        {
            fullModel.PropertyDetails.YearBuilt = model.PropertyDetails.YearBuilt;
            fullModel.PropertyDetails.LivingArea = model.PropertyDetails.LivingArea;
            fullModel.PropertyDetails.LotSize = model.PropertyDetails.LotSize;
            fullModel.PropertyDetails.LotSizeSqFt = model.PropertyDetails.LotSizeSqFt;
        }

        fullModel.Devices = ParseDevicesFromForm();
        return fullModel;
    }

    private List<DeviceInfoViewModel> ParseDevicesFromForm()
    {
        var devices = new List<DeviceInfoViewModel>();
        var i = 0;
        while (true)
        {
            var type = Request.Form[$"Devices[{i}].Type"];
            var serial = Request.Form[$"Devices[{i}].Serial"];
            var warrantyDateStr = Request.Form[$"Devices[{i}].WarrantyDate"];
            if (string.IsNullOrWhiteSpace(type) && string.IsNullOrWhiteSpace(serial) && string.IsNullOrWhiteSpace(warrantyDateStr))
            {
                break;
            }

            if (!string.IsNullOrWhiteSpace(type) || !string.IsNullOrWhiteSpace(serial) || !string.IsNullOrWhiteSpace(warrantyDateStr))
            {
                devices.Add(new DeviceInfoViewModel
                {
                    Type = type!,
                    Serial = serial!,
                    WarrantyDate = DateTime.TryParse(warrantyDateStr, out var dt) ? dt : null
                });
            }

            i++;
        }

        return devices;
    }

    private static bool IsSuccessfulEnrichment(string? dataSource)
    {
        if (string.IsNullOrWhiteSpace(dataSource)) return false;
        return dataSource.Contains("AI", StringComparison.OrdinalIgnoreCase)
            || dataSource.Contains("ATTOM", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatAddressWithUnit(string address, string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit)) return address;
        return $"{address}, {unit.Trim()}";
    }

    private void SetPropertyStepViewBag(bool showBack)
    {
        ViewBag.OnboardingStep = 3;
        ViewBag.OnboardingTitle = "Your property";
        ViewBag.OnboardingBackUrl = showBack
            ? Url.Action(nameof(AddProperty))
            : Url.Action("SelectRole", "Account", new { userId = _userManager.GetUserId(User) });
        ViewBag.OnboardingShowBack = showBack;
    }
}
