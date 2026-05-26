using System.Text.Json;
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
public class PropietarioController : Controller
{
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
        var userId = _userManager.GetUserId(User);
        if (!string.IsNullOrEmpty(userId))
        {
            var hasProperty = await _db.Propiedades
                .AnyAsync(p => p.UserId == userId && p.Activo);
            if (hasProperty)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProperty(AddPropertyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            _logger.LogInformation("Buscando información para la dirección: {Address}", model.Address);

            // Obtener TODA la información de la propiedad: dirección + utilities + warranties + detalles
            var propertyInfo = await _addressLookupService.GetPropertyInfoAsync(model.Address);

            if (propertyInfo == null)
            {
                _logger.LogWarning("No se encontró información para: {Address}", model.Address);
                ModelState.AddModelError("", "No information was found for this address. Try a more specific address.");
                return View(model);
            }

            // Guardar el resultado en TempData para pasarlo a la página de edición
            TempData["PropertyInfoJson"] = JsonSerializer.Serialize(propertyInfo);
            return RedirectToAction("EditAddress");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar/guardar la dirección");
            ModelState.AddModelError("", "An error occurred while processing the address. Please try again.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult EditAddress()
    {
        if (TempData["PropertyInfoJson"] is string json && !string.IsNullOrEmpty(json))
        {
            var propertyInfo = JsonSerializer.Deserialize<PropertyInfoViewModel>(json);
            // Keep TempData for POST
            TempData.Keep("PropertyInfoJson");
            return View(propertyInfo);
        }
        return RedirectToAction("AddProperty");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(PropertyInfoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Recuperar el JSON original
            var originalJson = Request.Form["OriginalJson"].ToString();
            PropertyInfoViewModel fullModel = null;
            if (!string.IsNullOrWhiteSpace(originalJson))
            {
                fullModel = JsonSerializer.Deserialize<PropertyInfoViewModel>(originalJson) ?? new PropertyInfoViewModel();
            }
            else
            {
                fullModel = model;
            }

            // Actualizar campos editados
            fullModel.FormattedAddress = model.FormattedAddress;
            fullModel.Street = model.Street;
            fullModel.HouseNumber = model.HouseNumber;
            fullModel.City = model.City;
            fullModel.State = model.State;
            fullModel.PostalCode = model.PostalCode;
            fullModel.Country = model.Country;
            if (fullModel.PropertyDetails == null)
                fullModel.PropertyDetails = new PropertyDetailsInfo();
            if (model.PropertyDetails != null)
            {
                fullModel.PropertyDetails.YearBuilt = model.PropertyDetails.YearBuilt;
                fullModel.PropertyDetails.LivingArea = model.PropertyDetails.LivingArea;
                fullModel.PropertyDetails.LotSize = model.PropertyDetails.LotSize;
                fullModel.PropertyDetails.LotSizeSqFt = model.PropertyDetails.LotSizeSqFt;
            }

            // Procesar dispositivos del formulario (recorrer todos los índices presentes)
            var devices = new List<DeviceInfoViewModel>();
            var deviceTypes = Request.Form["Devices[*].Type"];
            if (deviceTypes.Count == 0)
            {
                // Si no funciona el wildcard, usar el método tradicional
                int i = 0;
                while (true)
                {
                    var type = Request.Form[$"Devices[{i}].Type"];
                    var serial = Request.Form[$"Devices[{i}].Serial"];
                    var warrantyDateStr = Request.Form[$"Devices[{i}].WarrantyDate"];
                    if (string.IsNullOrWhiteSpace(type) && string.IsNullOrWhiteSpace(serial) && string.IsNullOrWhiteSpace(warrantyDateStr))
                        break;
                    if (!string.IsNullOrWhiteSpace(type) || !string.IsNullOrWhiteSpace(serial) || !string.IsNullOrWhiteSpace(warrantyDateStr))
                    {
                        var device = new DeviceInfoViewModel
                        {
                            Type = type,
                            Serial = serial,
                            WarrantyDate = DateTime.TryParse(warrantyDateStr, out var dt) ? dt : (DateTime?)null
                        };
                        devices.Add(device);
                    }
                    i++;
                }
            }
            else
            {
                // Si el wildcard funciona, recorre todos los valores
                for (int i = 0; i < deviceTypes.Count; i++)
                {
                    var type = Request.Form[$"Devices[{i}].Type"];
                    var serial = Request.Form[$"Devices[{i}].Serial"];
                    var warrantyDateStr = Request.Form[$"Devices[{i}].WarrantyDate"];
                    if (!string.IsNullOrWhiteSpace(type) || !string.IsNullOrWhiteSpace(serial) || !string.IsNullOrWhiteSpace(warrantyDateStr))
                    {
                        var device = new DeviceInfoViewModel
                        {
                            Type = type,
                            Serial = serial,
                            WarrantyDate = DateTime.TryParse(warrantyDateStr, out var dt) ? dt : (DateTime?)null
                        };
                        devices.Add(device);
                    }
                }
            }
            fullModel.Devices = devices;

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var jsonRaw = JsonSerializer.Serialize(fullModel, jsonOptions);

            var propiedad = new Propiedad
            {
                Direccion = fullModel.FormattedAddress,
                DatosJson = jsonRaw,
                UserId = userId,
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            _db.Propiedades.Add(propiedad);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Propiedad guardada (Id={Id}) para usuario {UserId}", propiedad.Id, userId);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al guardar la dirección editada");
            ModelState.AddModelError("", "An error occurred while saving the address. Please try again.");
            return View(model);
        }
    }
}
