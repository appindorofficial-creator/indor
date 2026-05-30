using System.Text.Json;
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
public class PropietarioController : Controller
{
    private const string OnboardingPropertySessionKey = "OnboardingPropertyInfo";

    private readonly IAddressLookupService _addressLookupService;
    private readonly ILogger<PropietarioController> _logger;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PropietarioController(
        IAddressLookupService addressLookupService,
        ILogger<PropietarioController> logger,
        AppDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _addressLookupService = addressLookupService;
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

        ViewBag.OnboardingStep = 2;
        ViewBag.OnboardingTitle = "Create Account";
        ViewBag.OnboardingBackUrl = Url.Action("Register", "Account");
        ViewBag.OnboardingShowBack = false;
        return View(new AddPropertyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
            _logger.LogInformation("Looking up address: {Address}", model.Address);
            var propertyInfo = await _addressLookupService.GetPropertyInfoAsync(model.Address);

            if (propertyInfo == null)
            {
                ModelState.AddModelError("", "No information was found for this address. Try a more specific address.");
                SetPropertyStepViewBag(showBack: false);
                return View(model);
            }

            SaveOnboardingProperty(propertyInfo);
            return RedirectToAction(nameof(ConfirmProperty));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error looking up address");
            ModelState.AddModelError("", "An error occurred while processing the address. Please try again.");
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

        var fullName = $"{user.Nombre} {user.Apellidos}".Trim();
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
    public IActionResult PropertyDetails()
    {
        var propertyInfo = LoadOnboardingProperty();
        if (propertyInfo == null)
        {
            return RedirectToAction(nameof(AddProperty));
        }

        HouseFactPreviewContext.Save(HttpContext.Session, propertyInfo);
        ViewBag.HouseFactProfile = HouseFactDisplayService.BuildProfile(
            propertyInfo.AttomRawJson,
            propertyInfo.DataSource,
            propertyInfo.FormattedAddress);

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
        if (!string.IsNullOrWhiteSpace(sessionJson))
        {
            return JsonSerializer.Deserialize<PropertyInfoViewModel>(sessionJson);
        }

        if (TempData["PropertyInfoJson"] is string json && !string.IsNullOrEmpty(json))
        {
            TempData.Keep("PropertyInfoJson");
            return JsonSerializer.Deserialize<PropertyInfoViewModel>(json);
        }

        return null;
    }

    private void SaveOnboardingProperty(PropertyInfoViewModel info)
    {
        var json = JsonSerializer.Serialize(info);
        HttpContext.Session.SetString(OnboardingPropertySessionKey, json);
        TempData["PropertyInfoJson"] = json;
    }

    private void ClearOnboardingProperty()
    {
        HttpContext.Session.Remove(OnboardingPropertySessionKey);
    }

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
        ViewBag.OnboardingStep = 2;
        ViewBag.OnboardingTitle = "Create Account";
        ViewBag.OnboardingBackUrl = showBack ? Url.Action(nameof(AddProperty)) : Url.Action("Register", "Account");
        ViewBag.OnboardingShowBack = showBack;
    }
}
