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
public class UtilitiesSetupController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAddressLookupService _addressLookup;

    public UtilitiesSetupController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IAddressLookupService addressLookup)
    {
        _db = db;
        _userManager = userManager;
        _addressLookup = addressLookup;
    }

    [HttpGet]
    public async Task<IActionResult> UtilitiesSetupAddress(int id)
    {
        var servicio = await LoadServicioAsync(id);
        if (servicio == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var solicitud = await GetOrCreateSolicitudAsync(userId, id, null);

        if (propiedad != null)
        {
            solicitud.PropiedadId = propiedad.Id;
            solicitud.DireccionPropiedad ??= propiedad.Direccion;
            await _db.SaveChangesAsync();
        }

        // Only echo prior choices after the user has submitted this step (Bug 12/20).
        var addressEntered = !string.Equals(solicitud.Estado, "InProgress", StringComparison.OrdinalIgnoreCase);

        return View(new UtilitiesSetupAddressViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = id,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty,
            ServiciosConectar = addressEntered ? (solicitud.ServiciosConectar ?? string.Empty) : string.Empty,
            FechaServicio = addressEntered ? solicitud.FechaServicio : null,
            PreferenciaContacto = addressEntered ? (solicitud.PreferenciaContacto ?? string.Empty) : string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UtilitiesSetupAddress(UtilitiesSetupAddressViewModel model, string? action)
    {
        var servicio = await LoadServicioAsync(model.MovingSetupServicioId);
        if (servicio == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home");
        }

        await EnsureAddressFromPropertyAsync(userId!, model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
            if (solicitud == null) return NotFound();

            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.ServiciosConectar = model.ServiciosConectar?.Trim();
            solicitud.FechaServicio = model.FechaServicio;
            solicitud.PreferenciaContacto = model.PreferenciaContacto;
            solicitud.Estado = "AddressCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (NeedsInternetStep(solicitud.ServiciosConectar))
            {
                return RedirectToAction(nameof(UtilitiesSetupInternet), new { id = solicitud.Id });
            }

            await PopulateUtilityContactsAsync(solicitud);
            return RedirectToAction(nameof(UtilitiesSetupUtilities), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your utilities setup. Please ensure the utilities setup flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> UtilitiesSetupInternet(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var proveedores = await _db.UtilitiesSetupProveedorInternet
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync();

        // Don't pre-select an ISP or cable option on first visit (Bug 20 / Bug 12 pattern).
        var defaultId = solicitud.ProveedorInternetId;
        var cableOption = !string.IsNullOrWhiteSpace(solicitud.OpcionCable)
            ? solicitud.OpcionCable
            : string.Empty;

        return View(new UtilitiesSetupInternetViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty,
            ProveedorInternetId = defaultId,
            OpcionCable = cableOption,
            Proveedores = proveedores.Select(p => new UtilitiesSetupInternetProviderViewModel
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Etiqueta = p.Etiqueta,
                Velocidad = p.Velocidad,
                DetalleExtra = p.DetalleExtra,
                PrecioDesde = p.PrecioDesde,
                Selected = defaultId.HasValue && p.Id == defaultId.Value
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UtilitiesSetupInternet(UtilitiesSetupInternetViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(UtilitiesSetupAddress), new { id = solicitud.MovingSetupServicioId });
        }

        try
        {
            if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
            {
                solicitud.OmitirInternet = true;
                solicitud.ProveedorInternetId = null;
            }
            else
            {
                if (!model.ProveedorInternetId.HasValue)
                {
                    ModelState.AddModelError(nameof(model.ProveedorInternetId), "Please select an internet provider.");
                    return View(await RebuildInternetViewModelAsync(solicitud, model));
                }

                solicitud.OmitirInternet = false;
                solicitud.ProveedorInternetId = model.ProveedorInternetId;
                solicitud.OpcionCable = model.OpcionCable;
            }

            solicitud.Estado = "InternetCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await PopulateUtilityContactsAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(UtilitiesSetupUtilities), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save internet selection. Please ensure the utilities setup flow tables exist in the database and try again.");
            return View(await RebuildInternetViewModelAsync(solicitud, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> UtilitiesSetupUtilities(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeContactos: true);
        if (solicitud == null) return NotFound();

        if (solicitud.Contactos.Count == 0)
        {
            await PopulateUtilityContactsAsync(solicitud);
            await _db.SaveChangesAsync();
        }

        return View(BuildUtilitiesViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UtilitiesSetupUtilities(int id, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeContactos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            if (NeedsInternetStep(solicitud.ServiciosConectar))
            {
                return RedirectToAction(nameof(UtilitiesSetupInternet), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(UtilitiesSetupAddress), new { id = solicitud.MovingSetupServicioId });
        }

        try
        {
            solicitud.Estado = "UtilitiesCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(UtilitiesSetupReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not continue. Please ensure the utilities setup flow tables exist in the database and try again.");
            return View(BuildUtilitiesViewModel(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> UtilitiesSetupReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeContactos: true, includeInternet: true);
        if (solicitud == null) return NotFound();

        return View(BuildReviewViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UtilitiesSetupReview(int id, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeContactos: true, includeInternet: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(UtilitiesSetupUtilities), new { id = solicitud.Id });
        }

        try
        {
            solicitud.Estado = "Confirmed";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            if (string.Equals(solicitud.PreferenciaContacto, "SaveForLater", StringComparison.OrdinalIgnoreCase)
                && solicitud.PropiedadId.HasValue)
            {
                await SyncToMyHomeAsync(solicitud);
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(UtilitiesSetupCompleted), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your setup. Please ensure the utilities setup flow tables exist in the database and try again.");
            return View(BuildReviewViewModel(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> UtilitiesSetupCompleted(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeContactos: true, includeInternet: true);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(UtilitiesSetupReview), new { id = solicitud.Id });
        }

        return View(BuildCompletedViewModel(solicitud));
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<MovingSetupServicio?> LoadServicioAsync(int id) =>
        await _db.MovingSetupServicios.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

    private async Task<Propiedad?> GetLatestPropertyAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<int?> GetLatestPropertyIdAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    private async Task<SolicitudUtilitiesSetup?> GetActiveSolicitudAsync(string userId, int movingSetupServicioId) =>
        await _db.SolicitudesUtilitiesSetup
            .Where(s => s.UserId == userId
                        && s.MovingSetupServicioId == movingSetupServicioId
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudUtilitiesSetup> GetOrCreateSolicitudAsync(
        string userId,
        int movingSetupServicioId,
        int? solicitudId)
    {
        SolicitudUtilitiesSetup? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesUtilitiesSetup
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, movingSetupServicioId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudUtilitiesSetup
            {
                UserId = userId,
                MovingSetupServicioId = movingSetupServicioId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                ServiciosConectar = string.Empty,
                PreferenciaContacto = string.Empty,
                OpcionCable = string.Empty
            };
            _db.SolicitudesUtilitiesSetup.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudUtilitiesSetup?> LoadSolicitudForUserAsync(
        int id,
        bool includeContactos = false,
        bool includeInternet = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        IQueryable<SolicitudUtilitiesSetup> query = _db.SolicitudesUtilitiesSetup
            .Include(s => s.MovingSetupServicio);

        if (includeContactos)
        {
            query = query.Include(s => s.Contactos);
        }

        if (includeInternet)
        {
            query = query.Include(s => s.ProveedorInternet);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task EnsureAddressFromPropertyAsync(string userId, UtilitiesSetupAddressViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.DireccionPropiedad))
        {
            return;
        }

        var propiedad = await GetLatestPropertyAsync(userId);
        if (string.IsNullOrWhiteSpace(propiedad?.Direccion))
        {
            return;
        }

        model.DireccionPropiedad = propiedad.Direccion;
        ModelState.Remove(nameof(model.DireccionPropiedad));
    }

    private static bool NeedsInternetStep(string? servicios) =>
        !string.IsNullOrWhiteSpace(servicios)
        && (servicios.Contains("Internet", StringComparison.OrdinalIgnoreCase)
            || servicios.Contains("Cable", StringComparison.OrdinalIgnoreCase));

    private async Task PopulateUtilityContactsAsync(SolicitudUtilitiesSetup solicitud)
    {
        var selected = SplitPipe(solicitud.ServiciosConectar);
        var address = solicitud.DireccionPropiedad ?? string.Empty;

        var existing = await _db.UtilitiesSetupContactos
            .Where(c => c.SolicitudUtilitiesSetupId == solicitud.Id)
            .ToListAsync();
        if (existing.Count > 0)
        {
            _db.UtilitiesSetupContactos.RemoveRange(existing);
        }

        var defaults = await GetDefaultUtilityContactsAsync(address);
        var orden = 1;

        if (selected.Contains("Electricity", StringComparer.OrdinalIgnoreCase) && defaults.Electric != null)
        {
            solicitud.Contactos.Add(MapContact("Electricity", defaults.Electric, orden++));
        }

        if (selected.Contains("Water", StringComparer.OrdinalIgnoreCase) && defaults.Water != null)
        {
            solicitud.Contactos.Add(MapContact("Water", defaults.Water, orden++));
        }

        if (selected.Contains("Gas", StringComparer.OrdinalIgnoreCase) && defaults.Gas != null)
        {
            solicitud.Contactos.Add(MapContact("Gas", defaults.Gas, orden++));
        }
    }

    private async Task<UtilityProvidersInfo> GetDefaultUtilityContactsAsync(string address)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                var info = await _addressLookup.GetPropertyInfoAsync(address);
                if (info?.UtilityProviders != null)
                {
                    return info.UtilityProviders;
                }
            }
        }
        catch
        {
            // Fall back to Charlotte defaults below.
        }

        return GetCharlotteDefaults();
    }

    private static UtilityProvidersInfo GetCharlotteDefaults() => new()
    {
        Electric = new UtilityProvider
        {
            Name = "Duke Energy",
            Phone = "800-777-9898",
            Website = "https://www.duke-energy.com"
        },
        Water = new UtilityProvider
        {
            Name = "Charlotte Water",
            Phone = "704-336-5499",
            Website = "https://www.charlottewater.org"
        },
        Gas = new UtilityProvider
        {
            Name = "Piedmont Natural Gas",
            Phone = "800-752-7504",
            Website = "https://www.piedmontng.com"
        }
    };

    private static UtilitiesSetupContacto MapContact(string tipo, UtilityProvider provider, int orden) =>
        new()
        {
            TipoUtilidad = tipo,
            Nombre = provider.Name,
            Telefono = provider.Phone,
            Website = provider.Website,
            IconoClase = UtilitiesSetupDisplayLabels.UtilityIcon(tipo),
            Orden = orden
        };

    private async Task SyncToMyHomeAsync(SolicitudUtilitiesSetup solicitud)
    {
        var propiedadId = solicitud.PropiedadId;
        if (!propiedadId.HasValue) return;

        foreach (var contacto in solicitud.Contactos.OrderBy(c => c.Orden))
        {
            var category = UtilitiesSetupDisplayLabels.FormatUtilityType(contacto.TipoUtilidad);
            var exists = await _db.PropiedadProveedores.AnyAsync(p =>
                p.PropiedadId == propiedadId.Value
                && p.Activo
                && p.ServiceCategory == category
                && p.Name == contacto.Nombre);

            if (exists) continue;

            _db.PropiedadProveedores.Add(new PropiedadProveedor
            {
                PropiedadId = propiedadId.Value,
                Name = contacto.Nombre,
                ServiceCategory = category,
                Phone = contacto.Telefono,
                Website = contacto.Website,
                Source = "UtilitiesSetup",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        if (!solicitud.OmitirInternet && solicitud.ProveedorInternet != null)
        {
            var internetName = solicitud.ProveedorInternet.Nombre;
            var exists = await _db.PropiedadProveedores.AnyAsync(p =>
                p.PropiedadId == propiedadId.Value
                && p.Activo
                && p.ServiceCategory == "Internet"
                && p.Name == internetName);

            if (!exists)
            {
                _db.PropiedadProveedores.Add(new PropiedadProveedor
                {
                    PropiedadId = propiedadId.Value,
                    Name = internetName,
                    ServiceCategory = "Internet",
                    Notes = UtilitiesSetupDisplayLabels.FormatCableOption(solicitud.OpcionCable),
                    Source = "UtilitiesSetup",
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                });
            }
        }
    }

    private async Task<UtilitiesSetupInternetViewModel> RebuildInternetViewModelAsync(
        SolicitudUtilitiesSetup solicitud,
        UtilitiesSetupInternetViewModel model)
    {
        var proveedores = await _db.UtilitiesSetupProveedorInternet
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync();

        model.DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty;
        model.Proveedores = proveedores.Select(p => new UtilitiesSetupInternetProviderViewModel
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Nombre = p.Nombre,
            Etiqueta = p.Etiqueta,
            Velocidad = p.Velocidad,
            DetalleExtra = p.DetalleExtra,
            PrecioDesde = p.PrecioDesde,
            Selected = p.Id == model.ProveedorInternetId
        }).ToList();

        return model;
    }

    private static UtilitiesSetupUtilitiesViewModel BuildUtilitiesViewModel(SolicitudUtilitiesSetup solicitud) =>
        new()
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty,
            FechaServicioLabel = UtilitiesSetupDisplayLabels.FormatDate(solicitud.FechaServicio),
            Contactos = solicitud.Contactos
                .OrderBy(c => c.Orden)
                .Select(c => new UtilitiesSetupUtilityContactViewModel
                {
                    TipoUtilidad = c.TipoUtilidad,
                    TipoLabel = UtilitiesSetupDisplayLabels.FormatUtilityType(c.TipoUtilidad),
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Website = c.Website,
                    Icon = c.IconoClase ?? UtilitiesSetupDisplayLabels.UtilityIcon(c.TipoUtilidad)
                })
                .ToList()
        };

    private static UtilitiesSetupReviewViewModel BuildReviewViewModel(SolicitudUtilitiesSetup solicitud)
    {
        var internetSelected = !solicitud.OmitirInternet && solicitud.ProveedorInternet != null;
        string? internetResumen = null;

        if (internetSelected && solicitud.ProveedorInternet != null)
        {
            internetResumen = UtilitiesSetupDisplayLabels.FormatInternetSummary(
                solicitud.ProveedorInternet.Nombre,
                solicitud.ProveedorInternet.Velocidad,
                solicitud.ProveedorInternet.PrecioDesde);
        }
        else if (NeedsInternetStep(solicitud.ServiciosConectar))
        {
            internetResumen = DisplayLabelsLocalization.L("Skipped");
        }

        return new UtilitiesSetupReviewViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            FechaServicioLabel = UtilitiesSetupDisplayLabels.FormatDate(solicitud.FechaServicio),
            ServiciosConectarLabel = UtilitiesSetupDisplayLabels.FormatServicesList(solicitud.ServiciosConectar),
            PreferenciaContactoLabel = UtilitiesSetupDisplayLabels.FormatContactPreference(solicitud.PreferenciaContacto),
            InternetResumen = internetResumen,
            InternetSelected = internetSelected,
            Contactos = solicitud.Contactos
                .OrderBy(c => c.Orden)
                .Select(c => new UtilitiesSetupUtilityContactViewModel
                {
                    TipoUtilidad = c.TipoUtilidad,
                    TipoLabel = UtilitiesSetupDisplayLabels.FormatUtilityType(c.TipoUtilidad),
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Website = c.Website,
                    Icon = c.IconoClase ?? UtilitiesSetupDisplayLabels.UtilityIcon(c.TipoUtilidad)
                })
                .ToList()
        };
    }

    private static UtilitiesSetupCompletedViewModel BuildCompletedViewModel(SolicitudUtilitiesSetup solicitud)
    {
        var cards = solicitud.Contactos
            .OrderBy(c => c.Orden)
            .Select(c => new UtilitiesSetupServiceCardViewModel
            {
                Label = UtilitiesSetupDisplayLabels.FormatUtilityType(c.TipoUtilidad),
                ProviderName = c.Nombre,
                Phone = c.Telefono,
                Website = c.Website,
                Icon = c.IconoClase ?? UtilitiesSetupDisplayLabels.UtilityIcon(c.TipoUtilidad)
            })
            .ToList();

        string? internetResumen = null;
        string? internetName = null;

        if (!solicitud.OmitirInternet && solicitud.ProveedorInternet != null)
        {
            internetName = solicitud.ProveedorInternet.Nombre;
            internetResumen = UtilitiesSetupDisplayLabels.FormatInternetSummary(
                solicitud.ProveedorInternet.Nombre,
                solicitud.ProveedorInternet.Velocidad,
                solicitud.ProveedorInternet.PrecioDesde);

            cards.Add(new UtilitiesSetupServiceCardViewModel
            {
                Label = DisplayLabelsLocalization.L("Internet & Cable"),
                ProviderName = solicitud.ProveedorInternet.Nombre,
                ExtraNote = DisplayLabelsLocalization.L("Other options available"),
                Icon = "fa-wifi"
            });
        }

        return new UtilitiesSetupCompletedViewModel
        {
            SolicitudId = solicitud.Id,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            FechaServicioLabel = UtilitiesSetupDisplayLabels.FormatDate(solicitud.FechaServicio),
            InternetResumen = internetResumen,
            InternetProviderName = internetName,
            Contactos = solicitud.Contactos
                .OrderBy(c => c.Orden)
                .Select(c => new UtilitiesSetupUtilityContactViewModel
                {
                    TipoUtilidad = c.TipoUtilidad,
                    TipoLabel = UtilitiesSetupDisplayLabels.FormatUtilityType(c.TipoUtilidad),
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Website = c.Website,
                    Icon = c.IconoClase ?? UtilitiesSetupDisplayLabels.UtilityIcon(c.TipoUtilidad)
                })
                .ToList(),
            ServiceCards = cards
        };
    }

    private static string[] SplitPipe(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
