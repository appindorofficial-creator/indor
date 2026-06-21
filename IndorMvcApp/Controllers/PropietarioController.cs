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
    private static readonly JsonSerializerOptions OnboardingJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHomeownerPropertyService _homeownerPropertyService;
    private readonly ILogger<PropietarioController> _logger;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PropietarioController(
        IHomeownerPropertyService homeownerPropertyService,
        ILogger<PropietarioController> logger,
        AppDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _homeownerPropertyService = homeownerPropertyService;
        _logger = logger;
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult AddProperty()
    {
        return Redirect(Url.Action("EditarPerfil", "Perfil") + "#home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddProperty(AddPropertyViewModel model)
    {
        return Redirect(Url.Action("EditarPerfil", "Perfil") + "#home");
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
            var propiedadId = await _homeownerPropertyService.SaveOrUpdatePropertyAsync(propertyInfo, userId);
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
            propertyInfo.MaintenanceRecommendations = await _homeownerPropertyService.TryGenerateMaintenanceAsync(propertyInfo);
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
            var existing = await _homeownerPropertyService.GetPrimaryPropertyAsync(userId);
            await _homeownerPropertyService.SaveOrUpdatePropertyAsync(fullModel, userId, existing?.Id);
            ClearOnboardingProperty();
            TempData["PropertySaved"] = "Property saved successfully.";
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
}
