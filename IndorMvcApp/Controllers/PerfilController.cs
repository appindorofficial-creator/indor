using System.Security.Claims;
using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.Validation;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Controllers;

[Authorize]
public class PerfilController : Controller
{
    private const string MembershipSessionKey = "MembershipSignup";
    private static readonly string[] ProfilePhotoExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxProfilePhotoBytes = 10_000_000;
    private static readonly TimeSpan AddressLookupTimeout = TimeSpan.FromMinutes(6);
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _env;
    private readonly IHomeownerPropertyService _homeownerPropertyService;
    private readonly ILogger<PerfilController> _logger;

    public PerfilController(AppDbContext db,
                            UserManager<ApplicationUser> userManager,
                            SignInManager<ApplicationUser> signInManager,
                            IWebHostEnvironment env,
                            IHomeownerPropertyService homeownerPropertyService,
                            ILogger<PerfilController> logger)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
        _homeownerPropertyService = homeownerPropertyService;
        _logger = logger;
    }

    private async Task<MoreProfileViewModel> BuildMoreProfileAsync()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.GetUserAsync(User);
        var membresia = await _db.MembresiasUsuario
            .Include(m => m.Plan)
            .Where(m => m.UserId == userId && m.Activa)
            .OrderByDescending(m => m.FechaInicio)
            .FirstOrDefaultAsync();

        var homeCount = await _db.Propiedades.CountAsync(p => p.UserId == userId && p.Activo);
        var propIds = await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .Select(p => p.Id)
            .ToListAsync();
        var docCount = propIds.Count == 0
            ? 0
            : await _db.PropiedadDocumentos.CountAsync(d => propIds.Contains(d.PropiedadId));
        var serviceCount = await _db.HistorialServicios.CountAsync(h => h.UserId == userId)
            + await _db.ProgramacionesMicroservicio.CountAsync(p => p.UserId == userId);

        var primaryPropId = propIds.Count > 0 ? propIds.Max() : 0;
        ViewBag.PropiedadId = primaryPropId > 0 ? (int?)primaryPropId : null;

        return ProfileDisplayService.Build(user, membresia, homeCount, docCount, serviceCount, Url);
    }

    private async Task CargarUsuarioYMembresiaAsync()
    {
        var userId = _userManager.GetUserId(User);
        ViewBag.UsuarioActual = await _userManager.GetUserAsync(User);
        ViewBag.MembresiaActual = await _db.MembresiasUsuario
            .Include(m => m.Plan)
            .Where(m => m.UserId == userId && m.Activa)
            .OrderByDescending(m => m.FechaInicio)
            .FirstOrDefaultAsync();
        ViewBag.MoreProfile = await BuildMoreProfileAsync();
    }

    private MembershipSignupState? GetMembershipSignup()
    {
        var json = HttpContext.Session.GetString(MembershipSessionKey);
        return string.IsNullOrEmpty(json)
            ? null
            : JsonSerializer.Deserialize<MembershipSignupState>(json);
    }

    private void SaveMembershipSignup(MembershipSignupState state)
    {
        HttpContext.Session.SetString(MembershipSessionKey,
            JsonSerializer.Serialize(state));
    }

    private void ClearMembershipSignup() =>
        HttpContext.Session.Remove(MembershipSessionKey);

    private async Task<PlanMembresia?> GetSignupPlanAsync()
    {
        var state = GetMembershipSignup();
        if (state == null || state.PlanId <= 0) return null;
        return await _db.PlanesMembresia.FirstOrDefaultAsync(p => p.Id == state.PlanId && p.Activo);
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Opciones));

    [HttpGet]
    public async Task<IActionResult> Opciones()
    {
        await CargarUsuarioYMembresiaAsync();
        ViewData["Title"] = "Profile Options";
        ViewData["Subtitulo"] = "Manage your account details and preferences.";
        ViewBag.BottomNavActive = "more";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> EditarPerfil()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await CargarUsuarioYMembresiaAsync();
        ViewData["Title"] = "Edit Profile";
        ViewData["Subtitulo"] = "Update your details and connect your home with AI.";
        ViewBag.BottomNavActive = "more";
        return View(await MapEditProfileViewModelAsync(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestTimeout("OnboardingAddressLookup")]
    public async Task<IActionResult> EnriquecerPropiedad([Bind(Prefix = "AddressForm")] AddPropertyViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            TempData["PerfilError"] = "Please complete your street address, city, state, and ZIP code.";
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#home");
        }

        var userId = user.Id;
        var existing = await _homeownerPropertyService.GetPrimaryPropertyAsync(userId);

        try
        {
            var propertyInfo = await _homeownerPropertyService
                .EnrichAddressAsync(model, requestFullHouseFactResearch: existing?.Id > 0)
                .WaitAsync(AddressLookupTimeout, HttpContext.RequestAborted);

            if (propertyInfo == null)
            {
                TempData["PerfilError"] = "No information was found for this address. Try a more specific address.";
                return Redirect(Url.Action(nameof(EditarPerfil)) + "#home");
            }

            var propiedadId = await _homeownerPropertyService.SaveOrUpdatePropertyAsync(
                propertyInfo,
                userId,
                existing?.Id,
                HttpContext.RequestAborted);

            var hasAiData = !string.IsNullOrWhiteSpace(propertyInfo.AttomRawJson)
                && (PropertyEnrichmentMapper.HasMeaningfulDetails(propertyInfo.PropertyDetails ?? new PropertyDetailsInfo())
                    || HouseFactDisplayService.BuildProfile(propertyInfo.AttomRawJson).FieldCount > 0);

            if (hasAiData)
            {
                TempData["PerfilOk"] = "Your home profile is ready — House Facts and maintenance insights are now available.";
                TempData["HomeEnriched"] = true;
            }
            else if (!string.IsNullOrWhiteSpace(propertyInfo.AttomRawJson))
            {
                TempData["PerfilOk"] =
                    "Address saved with basic property details. Full House Facts are loading — refresh in about 1 minute.";
            }
            else
            {
                _logger.LogWarning(
                    "Home enrichment incomplete for {Address}. RawJson={HasJson}, Error={Error}",
                    model.BuildLookupAddress(),
                    !string.IsNullOrWhiteSpace(propertyInfo.AttomRawJson),
                    propertyInfo.EnrichmentError ?? "none");

                TempData["PerfilOk"] =
                    "Address saved! AI is researching your home now — refresh this page in about 1 minute to see House Facts.";
            }

            return Redirect(Url.Action(nameof(EditarPerfil), new { id = propiedadId }) + "#home");
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Home enrichment timed out for {Address}", model.BuildLookupAddress());
            TempData["PerfilError"] = "Address lookup is taking longer than expected. Please try again in a moment.";
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#home");
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            TempData["PerfilError"] = "Address lookup was interrupted. Please try again.";
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error enriching homeowner property");
            TempData["PerfilError"] = "We couldn't research this address right now. Please try again.";
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> EditarPerfil(
        string nombre, string apellidos, string telefono, IFormFile? foto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (!string.IsNullOrWhiteSpace(nombre)) user.Nombre = nombre.Trim();
        if (!string.IsNullOrWhiteSpace(apellidos)) user.Apellidos = apellidos.Trim();
        if (!string.IsNullOrWhiteSpace(telefono))
        {
            user.Telefono = telefono.Trim();
            user.PhoneNumber = telefono.Trim();
        }

        var photoError = await TrySaveHomeownerPhotoAsync(user, foto);
        if (!string.IsNullOrWhiteSpace(photoError))
        {
            TempData["PerfilError"] = photoError;
            ViewData["Title"] = "Edit Profile";
            ViewData["Subtitulo"] = "Update your name, phone, and profile photo.";
            return View(await MapEditProfileViewModelAsync(user));
        }

        await _userManager.UpdateAsync(user);
        TempData["PerfilOk"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Opciones));
    }

    [HttpGet]
    public async Task<IActionResult> Pagos()
    {
        var userId = _userManager.GetUserId(User);
        await CargarUsuarioYMembresiaAsync();
        ViewBag.MetodosPago = await _db.MetodosPago
            .Where(m => m.UserId == userId && m.Activo)
            .OrderByDescending(m => m.EsPredeterminado)
            .ThenByDescending(m => m.FechaCreacion)
            .ToListAsync();
        ViewBag.Pagos = await _db.Pagos
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();
        ViewData["Title"] = "Payments & History";
        ViewData["Subtitulo"] = "Track services, billing, and financing in one place.";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Suscripciones(int? planId)
    {
        await CargarUsuarioYMembresiaAsync();
        ViewBag.Planes = await _db.PlanesMembresia
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync();
        ViewBag.SelectedPlanId = planId ?? GetMembershipSignup()?.PlanId;
        ViewData["Title"] = "Choose your membership";
        ViewData["Subtitulo"] = "Pick the plan that fits your home care needs.";
        ViewData["MembershipStep"] = 1;
        ViewData["MembershipTotalSteps"] = 6;
        ViewData["MembershipBackUrl"] = Url.Action("Index", "Home", new { section = "more" });
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeleccionarPlan(int planId)
    {
        var plan = await _db.PlanesMembresia.FirstOrDefaultAsync(p => p.Id == planId && p.Activo);
        if (plan == null)
        {
            TempData["PerfilError"] = "Plan not found.";
            return RedirectToAction(nameof(Suscripciones));
        }

        SaveMembershipSignup(new MembershipSignupState { PlanId = planId });

        if (plan.PrecioMensual <= 0)
        {
            return await ActivarPlanInternal(planId);
        }

        return RedirectToAction(nameof(PlanDetalle), new { id = planId });
    }

    [HttpGet]
    public async Task<IActionResult> PlanDetalle(int id)
    {
        var plan = await _db.PlanesMembresia.FirstOrDefaultAsync(p => p.Id == id && p.Activo);
        if (plan == null) return RedirectToAction(nameof(Suscripciones));

        var state = GetMembershipSignup() ?? new MembershipSignupState { PlanId = id };
        state.PlanId = id;
        SaveMembershipSignup(state);

        await CargarUsuarioYMembresiaAsync();
        ViewBag.Plan = plan;
        var kind = ProfileDisplayService.GetPlanKind(plan);
        SetPlanDetalleViewData(plan, kind);
        ViewData["MembershipBackUrl"] = Url.Action(nameof(Suscripciones));
        ViewBag.PlanKind = kind;
        return View();
    }

    private void SetPlanDetalleViewData(PlanMembresia plan, MembershipPlanKind kind)
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
        var price = plan.PrecioMensual.ToString("C0", culture);
        switch (kind)
        {
            case MembershipPlanKind.Filter:
                ViewData["Title"] = "Filter Plan details";
                ViewData["Subtitulo"] = $"See how your {price}/month plan works.";
                ViewData["MembershipStep"] = 2;
                break;
            case MembershipPlanKind.HomeCare:
                ViewData["Title"] = "Home Care Plan";
                ViewData["Subtitulo"] = "Everything you need for ongoing home care.";
                ViewData["MembershipStep"] = 2;
                break;
            case MembershipPlanKind.Premium:
                ViewData["Title"] = "Premium Care Plan";
                ViewData["Subtitulo"] = "Best for proactive homeowners.";
                ViewData["MembershipStep"] = 2;
                break;
            default:
                ViewData["Title"] = plan.Nombre;
                ViewData["Subtitulo"] = $"See how your {price}/month plan works.";
                ViewData["MembershipStep"] = 2;
                break;
        }
        ViewData["MembershipTotalSteps"] = 6;
    }

    private async Task PrefillAddressFromPropertyAsync(MembershipSignupState state)
    {
        var userId = _userManager.GetUserId(User);
        var prop = await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();
        if (prop == null) return;
        if (string.IsNullOrWhiteSpace(state.PropertyAddress))
            state.PropertyAddress = prop.Direccion;
        if (string.IsNullOrWhiteSpace(state.ShippingAddress))
            state.ShippingAddress = prop.Direccion;
    }

    [HttpGet]
    public async Task<IActionResult> MembresiaFiltro()
    {
        var plan = await GetSignupPlanAsync();
        if (plan == null) return RedirectToAction(nameof(Suscripciones));
        if (!ProfileDisplayService.IsFilterPlan(plan.Nombre))
            return RedirectToAction(nameof(MembresiaEntregaHogar));

        var state = GetMembershipSignup() ?? new MembershipSignupState { PlanId = plan.Id };
        await PrefillAddressFromPropertyAsync(state);
        SaveMembershipSignup(state);

        await CargarUsuarioYMembresiaAsync();
        ViewBag.Plan = plan;
        ViewBag.Signup = state;
        ViewData["Title"] = "Tell us about your filter";
        ViewData["Subtitulo"] = "We'll send the right filter to your home.";
        ViewData["MembershipStep"] = 3;
        ViewData["MembershipBackUrl"] = Url.Action(nameof(PlanDetalle), new { id = plan.Id });
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> MembresiaFiltro(
        string? propertyAddress, string? hvacNickname, string? filterSize,
        string? filterType, bool petsAtHome = false, IFormFile? filterPhoto = null)
    {
        var state = GetMembershipSignup();
        if (state == null) return RedirectToAction(nameof(Suscripciones));

        state.PropertyAddress = propertyAddress?.Trim();
        state.HvacNickname = hvacNickname?.Trim();
        state.FilterSize = filterSize?.Trim();
        state.FilterType = filterType?.Trim();
        state.PetsAtHome = petsAtHome;

        if (filterPhoto != null && filterPhoto.Length > 0)
        {
            var ext = Path.GetExtension(filterPhoto.FileName).ToLowerInvariant();
            if (new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
            {
                var userId = _userManager.GetUserId(User) ?? "anon";
                var carpeta = Path.Combine(_env.WebRootPath, "uploads", "membership", userId);
                Directory.CreateDirectory(carpeta);
                var nombre = $"filter_{DateTime.Now.Ticks}{ext}";
                var ruta = Path.Combine(carpeta, nombre);
                await using (var fs = new FileStream(ruta, FileMode.Create))
                    await filterPhoto.CopyToAsync(fs);
                state.FilterPhotoUrl = $"/uploads/membership/{userId}/{nombre}";
            }
        }

        SaveMembershipSignup(state);
        return RedirectToAction(nameof(MembresiaEntrega));
    }

    [HttpGet]
    public async Task<IActionResult> MembresiaEntregaHogar()
    {
        var plan = await GetSignupPlanAsync();
        if (plan == null) return RedirectToAction(nameof(Suscripciones));
        var kind = ProfileDisplayService.GetPlanKind(plan);
        if (kind is not MembershipPlanKind.HomeCare and not MembershipPlanKind.Premium)
            return RedirectToAction(nameof(MembresiaFiltro));

        var state = GetMembershipSignup() ?? new MembershipSignupState { PlanId = plan.Id };
        await PrefillAddressFromPropertyAsync(state);
        if (state.FirstDeliveryDate == null)
            state.FirstDeliveryDate = DateTime.Today.AddDays(30);
        if (string.IsNullOrWhiteSpace(state.FilterSize))
            state.FilterSize = "16x20x1";
        SaveMembershipSignup(state);

        await CargarUsuarioYMembresiaAsync();
        ViewBag.Plan = plan;
        ViewBag.Signup = state;
        ViewBag.PlanKind = kind;
        ViewData["Title"] = "Set up your filter delivery";
        ViewData["Subtitulo"] = "Confirm where and what we should send every 3 months.";
        ViewData["MembershipStep"] = 3;
        ViewData["MembershipTotalSteps"] = 6;
        ViewData["MembershipBackUrl"] = Url.Action(nameof(PlanDetalle), new { id = plan.Id });
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MembresiaEntregaHogar(
        string? propertyAddress, string? filterSize,
        string? deliveryCycle, DateTime? firstDeliveryDate,
        int filterQuantity, bool petsAtHome = false)
    {
        var state = GetMembershipSignup();
        if (state == null) return RedirectToAction(nameof(Suscripciones));

        state.PropertyAddress = propertyAddress?.Trim();
        state.ShippingAddress = propertyAddress?.Trim();
        state.FilterSize = filterSize?.Trim();
        state.FilterQuantity = filterQuantity <= 0 ? 1 : Math.Clamp(filterQuantity, 1, 10);
        state.DeliveryCycle = string.IsNullOrWhiteSpace(deliveryCycle) ? "Every 3 months" : deliveryCycle.Trim();
        state.FirstDeliveryDate = firstDeliveryDate;
        state.PetsAtHome = petsAtHome;
        SaveMembershipSignup(state);
        return RedirectToAction(nameof(MembresiaBeneficios));
    }

    [HttpGet]
    public async Task<IActionResult> MembresiaBeneficios()
    {
        var plan = await GetSignupPlanAsync();
        if (plan == null) return RedirectToAction(nameof(Suscripciones));
        var kind = ProfileDisplayService.GetPlanKind(plan);
        if (kind is not MembershipPlanKind.HomeCare and not MembershipPlanKind.Premium)
            return RedirectToAction(nameof(MembresiaEntrega));

        var state = GetMembershipSignup() ?? new MembershipSignupState { PlanId = plan.Id };
        await CargarUsuarioYMembresiaAsync();
        ViewBag.Plan = plan;
        ViewBag.Signup = state;
        ViewBag.PlanKind = kind;
        ViewData["Title"] = "Reminders & member benefits";
        ViewData["Subtitulo"] = "Choose the maintenance alerts you want and see how your savings work.";
        ViewData["MembershipStep"] = 4;
        ViewData["MembershipTotalSteps"] = 6;
        ViewData["MembershipBackUrl"] = Url.Action(nameof(MembresiaEntregaHogar));
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MembresiaBeneficios(
        bool reminderAirFilter = true, bool reminderHvac = true,
        bool reminderSmokeDetector = true, bool reminderSeasonal = true)
    {
        var state = GetMembershipSignup();
        if (state == null) return RedirectToAction(nameof(Suscripciones));

        state.ReminderAirFilter = reminderAirFilter;
        state.ReminderHvac = reminderHvac;
        state.ReminderSmokeDetector = reminderSmokeDetector;
        state.ReminderSeasonal = reminderSeasonal;
        SaveMembershipSignup(state);
        return RedirectToAction(nameof(MembresiaRevision));
    }

    [HttpGet]
    public async Task<IActionResult> MembresiaEntrega()
    {
        var plan = await GetSignupPlanAsync();
        if (plan == null) return RedirectToAction(nameof(Suscripciones));
        if (!ProfileDisplayService.IsFilterPlan(plan.Nombre))
            return RedirectToAction(nameof(MembresiaEntregaHogar));

        var state = GetMembershipSignup()!;
        if (string.IsNullOrWhiteSpace(state.ShippingAddress))
            state.ShippingAddress = state.PropertyAddress;
        if (state.FirstDeliveryDate == null)
            state.FirstDeliveryDate = DateTime.Today.AddDays(30);
        SaveMembershipSignup(state);

        await CargarUsuarioYMembresiaAsync();
        ViewBag.Plan = plan;
        ViewBag.Signup = state;
        ViewData["Title"] = "Delivery setup";
        ViewData["Subtitulo"] = "Choose where and when your filter should arrive.";
        ViewData["MembershipStep"] = 4;
        ViewData["MembershipTotalSteps"] = 6;
        ViewData["MembershipBackUrl"] = Url.Action(nameof(MembresiaFiltro));
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MembresiaEntrega(
        string? shippingAddress, string? deliveryCycle, DateTime? firstDeliveryDate,
        bool shipmentReminder = true, bool replaceFilterReminder = true, bool lowInventoryReminder = true)
    {
        var state = GetMembershipSignup();
        if (state == null) return RedirectToAction(nameof(Suscripciones));

        state.ShippingAddress = shippingAddress?.Trim();
        state.DeliveryCycle = string.IsNullOrWhiteSpace(deliveryCycle) ? "Every 3 months" : deliveryCycle.Trim();
        state.FirstDeliveryDate = firstDeliveryDate;
        state.ShipmentReminder = shipmentReminder;
        state.ReplaceFilterReminder = replaceFilterReminder;
        state.LowInventoryReminder = lowInventoryReminder;
        SaveMembershipSignup(state);
        return RedirectToAction(nameof(MembresiaRevision));
    }

    [HttpGet]
    public async Task<IActionResult> MembresiaRevision()
    {
        var plan = await GetSignupPlanAsync();
        if (plan == null) return RedirectToAction(nameof(Suscripciones));

        var userId = _userManager.GetUserId(User);
        await CargarUsuarioYMembresiaAsync();
        ViewBag.Plan = plan;
        ViewBag.Signup = GetMembershipSignup();
        ViewBag.MetodosPago = await _db.MetodosPago
            .Where(m => m.UserId == userId && m.Activo)
            .OrderByDescending(m => m.EsPredeterminado)
            .FirstOrDefaultAsync();
        var kind = ProfileDisplayService.GetPlanKind(plan);
        ViewBag.PlanKind = kind;
        switch (kind)
        {
            case MembershipPlanKind.Filter:
                ViewData["Title"] = "Review & payment";
                ViewData["Subtitulo"] = "Confirm your Filter Plan before subscribing.";
                ViewData["MembershipBackUrl"] = Url.Action(nameof(MembresiaEntrega));
                break;
            case MembershipPlanKind.HomeCare:
            case MembershipPlanKind.Premium:
                ViewData["Title"] = "Review & payment";
                ViewData["Subtitulo"] = $"Confirm your {plan.Nombre} before activating your membership.";
                ViewData["MembershipBackUrl"] = Url.Action(nameof(MembresiaBeneficios));
                break;
            default:
                ViewData["Title"] = "Review & confirm your plan";
                ViewData["Subtitulo"] = "Almost there! Complete your subscription setup.";
                ViewData["MembershipBackUrl"] = Url.Action(nameof(PlanDetalle), new { id = plan.Id });
                break;
        }
        ViewData["MembershipStep"] = 5;
        ViewData["MembershipTotalSteps"] = 6;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarMembresia(bool agreeBilling = false)
    {
        var state = GetMembershipSignup();
        if (state == null) return RedirectToAction(nameof(Suscripciones));

        var plan = await _db.PlanesMembresia.FirstOrDefaultAsync(p => p.Id == state.PlanId && p.Activo);
        if (plan == null) return RedirectToAction(nameof(Suscripciones));

        var kind = ProfileDisplayService.GetPlanKind(plan);
        var skipBillingCheckbox = kind is MembershipPlanKind.Filter or MembershipPlanKind.HomeCare or MembershipPlanKind.Premium;
        if (plan.PrecioMensual > 0 && !skipBillingCheckbox && !agreeBilling)
        {
            TempData["PerfilError"] = "Please agree to recurring billing to continue.";
            return RedirectToAction(nameof(MembresiaRevision));
        }

        var result = await ActivarPlanInternal(state.PlanId);
        if (result is RedirectToActionResult r && r.ActionName == nameof(MembresiaExito))
            return result;

        return RedirectToAction(nameof(MembresiaExito));
    }

    [HttpGet]
    public async Task<IActionResult> MembresiaExito()
    {
        var plan = await GetSignupPlanAsync();
        if (plan == null)
        {
            var membresia = await _db.MembresiasUsuario
                .Include(m => m.Plan)
                .Where(m => m.UserId == _userManager.GetUserId(User) && m.Activa)
                .OrderByDescending(m => m.FechaInicio)
                .FirstOrDefaultAsync();
            plan = membresia?.Plan;
        }

        var signup = GetMembershipSignup();
        await CargarUsuarioYMembresiaAsync();
        ViewBag.Plan = plan;
        ViewBag.Signup = signup;
        if (signup?.FirstDeliveryDate != null)
            ViewBag.NextReminderDate = signup.FirstDeliveryDate.Value.AddDays(-7);

        var kind = plan != null ? ProfileDisplayService.GetPlanKind(plan) : MembershipPlanKind.Other;
        ViewBag.PlanKind = kind;
        switch (kind)
        {
            case MembershipPlanKind.Filter:
                ViewData["Title"] = "You're all set!";
                ViewData["Subtitulo"] = "Your Filter Plan is now active.";
                break;
            case MembershipPlanKind.HomeCare:
                ViewData["Title"] = "Welcome to Home Care Plan";
                ViewData["Subtitulo"] = "Your membership is active.";
                break;
            case MembershipPlanKind.Premium:
                ViewData["Title"] = "Welcome to Premium Care Plan";
                ViewData["Subtitulo"] = "Your membership is active.";
                break;
            default:
                ViewData["Title"] = "You're enrolled";
                ViewData["Subtitulo"] = plan != null ? $"Your membership in {plan.Nombre} is active." : "Your plan is now active.";
                break;
        }
        ViewData["MembershipStep"] = 6;
        ViewData["MembershipTotalSteps"] = 6;
        ViewData["MembershipBackUrl"] = Url.Action(nameof(Suscripciones));
        ClearMembershipSignup();
        return View();
    }

    private async Task<HomeownerEditProfileViewModel> MapEditProfileViewModelAsync(ApplicationUser user)
    {
        var fullName = UserDisplayName.Format(user);
        var initial = "?";
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            initial = fullName.Trim()[..1].ToUpperInvariant();
        }
        else if (!string.IsNullOrWhiteSpace(user.Email))
        {
            initial = user.Email.Trim()[..1].ToUpperInvariant();
        }

        var model = new HomeownerEditProfileViewModel
        {
            Nombre = user.Nombre,
            Apellidos = user.Apellidos,
            Telefono = user.Telefono,
            Email = user.Email ?? string.Empty,
            PhotoUrl = user.FotoUrl,
            DisplayInitial = initial
        };

        var propiedad = await _homeownerPropertyService.GetPrimaryPropertyAsync(user.Id);
        if (propiedad == null)
        {
            return model;
        }

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        model.HasHome = true;
        model.PropiedadId = propiedad.Id;
        model.HomeAddress = propiedad.Direccion ?? info?.FormattedAddress;
        model.DataSource = info?.DataSource ?? propiedad.AttomSyncStatus;

        if (info?.PropertyDetails != null)
        {
            model.YearBuilt = info.PropertyDetails.YearBuilt;
            model.LivingArea = info.PropertyDetails.LivingArea;
            model.Bedrooms = info.PropertyDetails.Bedrooms;
            model.Bathrooms = info.PropertyDetails.Bathrooms;
        }

        model.HouseFactPreview = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            model.DataSource,
            model.HomeAddress);
        model.HouseFactFieldCount = model.HouseFactPreview.FieldCount;
        model.HasEnrichedData = !string.IsNullOrWhiteSpace(propiedad.AttomRawJson)
            && (PropertyEnrichmentMapper.HasMeaningfulDetails(info?.PropertyDetails ?? new PropertyDetailsInfo())
                || model.HouseFactFieldCount > 3);

        model.AddressForm = new AddPropertyViewModel
        {
            StreetAddress = info?.Street ?? string.Empty,
            City = info?.City ?? string.Empty,
            State = info?.State ?? string.Empty,
            ZipCode = info?.PostalCode ?? string.Empty,
            Unit = info?.Unit
        };

        if (string.IsNullOrWhiteSpace(model.AddressForm.StreetAddress)
            && !string.IsNullOrWhiteSpace(model.HomeAddress))
        {
            model.AddressForm.Address = model.HomeAddress;
        }

        return model;
    }

    private async Task<string?> TrySaveHomeownerPhotoAsync(ApplicationUser user, IFormFile? foto)
    {
        if (foto == null || foto.Length == 0)
        {
            return null;
        }

        var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) && foto.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            ext = foto.ContentType.Contains("png", StringComparison.OrdinalIgnoreCase) ? ".png"
                : foto.ContentType.Contains("webp", StringComparison.OrdinalIgnoreCase) ? ".webp"
                : ".jpg";
        }

        if (!ProfilePhotoExtensions.Contains(ext))
        {
            return "Photo must be JPG, PNG, or WEBP.";
        }

        if (foto.Length > MaxProfilePhotoBytes)
        {
            return "Photo must be 10 MB or less.";
        }

        var carpeta = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(carpeta);
        var nombreArchivo = $"{user.Id}_{Guid.NewGuid():N}{ext}";
        var ruta = Path.Combine(carpeta, nombreArchivo);
        await using (var fs = System.IO.File.Create(ruta))
        {
            await foto.CopyToAsync(fs);
        }

        user.FotoUrl = $"/uploads/avatars/{nombreArchivo}";
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Historial()
    {
        var userId = _userManager.GetUserId(User);
        await CargarUsuarioYMembresiaAsync();
        ViewBag.Historial = await _db.HistorialServicios
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync();
        ViewData["Title"] = "History";
        ViewBag.BottomNavActive = "more";
        ViewData["Subtitulo"] = "Microservices, inspections, and past services";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Internet()
    {
        await CargarUsuarioYMembresiaAsync();
        ViewBag.PlanesInternet = await _db.PlanesInternet
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync();
        ViewData["Title"] = "Internet comparison";
        ViewData["Subtitulo"] = "Compare internet plans and providers";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Soporte()
    {
        var userId = _userManager.GetUserId(User);
        await CargarUsuarioYMembresiaAsync();
        ViewBag.MensajesSoporte = await _db.MensajesSoporte
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Fecha)
            .ToListAsync();
        ViewData["Title"] = "Support";
        ViewData["Subtitulo"] = "Chat with our team";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarDatos(string nombre, string apellidos, string telefono)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (!string.IsNullOrWhiteSpace(nombre)) user.Nombre = nombre.Trim();
        if (!string.IsNullOrWhiteSpace(apellidos)) user.Apellidos = apellidos.Trim();
        if (!string.IsNullOrWhiteSpace(telefono))
        {
            user.Telefono = telefono.Trim();
            user.PhoneNumber = telefono.Trim();
        }

        await _userManager.UpdateAsync(user);
        TempData["PerfilOk"] = "Profile updated successfully.";
        return RedirectToAction("Index", "Home", new { section = "more" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> SubirFoto(IFormFile foto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (foto == null || foto.Length == 0)
        {
            TempData["PerfilError"] = "Please choose a photo to upload.";
            return RedirectToAction(nameof(EditarPerfil));
        }

        var photoError = await TrySaveHomeownerPhotoAsync(user, foto);
        if (!string.IsNullOrWhiteSpace(photoError))
        {
            TempData["PerfilError"] = photoError;
            return RedirectToAction(nameof(EditarPerfil));
        }

        await _userManager.UpdateAsync(user);
        TempData["PerfilOk"] = "Photo updated.";
        return RedirectToAction("Index", "Home", new { section = "more" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPassword(string actual, string nueva, string confirmar)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (string.IsNullOrEmpty(nueva) || nueva != confirmar)
        {
            TempData["PerfilError"] = "Passwords do not match.";
            return RedirectToAction(nameof(Opciones));
        }

        var result = await _userManager.ChangePasswordAsync(user, actual ?? string.Empty, nueva);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
            TempData["PerfilOk"] = "Password updated.";
        }
        else
        {
            TempData["PerfilError"] = string.Join(" ", result.Errors.Select(e => e.Description));
        }
        return RedirectToAction(nameof(Opciones));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarMetodoPago(string tipo, string marca, string ultimos4, string titular, string expiracion, bool predeterminado)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

        if (!PaymentMethodValidation.TryValidate(tipo, marca, ultimos4, titular, expiracion, out var errors))
        {
            TempData["PerfilError"] = string.Join(" ", errors.Values);
            return RedirectToAction(nameof(Opciones));
        }

        if (predeterminado)
        {
            var existentes = await _db.MetodosPago.Where(m => m.UserId == userId && m.EsPredeterminado).ToListAsync();
            foreach (var m in existentes) m.EsPredeterminado = false;
        }

        _db.MetodosPago.Add(new MetodoPago
        {
            UserId = userId,
            Tipo = string.IsNullOrWhiteSpace(tipo) ? "Card" : tipo.Trim(),
            Marca = (marca ?? string.Empty).Trim(),
            Ultimos4 = PaymentMethodValidation.NormalizeLastFour(ultimos4),
            Titular = PaymentMethodValidation.NormalizeCardholder(titular),
            Expiracion = PaymentMethodValidation.NormalizeExpiry(expiracion),
            EsPredeterminado = predeterminado
        });
        await _db.SaveChangesAsync();
        TempData["PerfilOk"] = "Payment method added.";
        return RedirectToAction(nameof(Pagos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarMensajeSoporte(string contenido)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
        if (string.IsNullOrWhiteSpace(contenido))
        {
            return RedirectToAction(nameof(Soporte));
        }

        _db.MensajesSoporte.Add(new MensajeSoporte
        {
            UserId = userId,
            Remitente = "User",
            Contenido = contenido.Trim()
        });
        _db.MensajesSoporte.Add(new MensajeSoporte
        {
            UserId = userId,
            Remitente = "Support",
            Contenido = "Hello! We received your message. An agent will reply soon."
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Soporte));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ActivarPlan(int planId) => ActivarPlanInternal(planId);

    private async Task<IActionResult> ActivarPlanInternal(int planId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

        var plan = await _db.PlanesMembresia.FirstOrDefaultAsync(p => p.Id == planId && p.Activo);
        if (plan == null)
        {
            TempData["PerfilError"] = "Plan not found.";
            return RedirectToAction(nameof(Suscripciones));
        }

        var actuales = await _db.MembresiasUsuario.Where(m => m.UserId == userId && m.Activa).ToListAsync();
        foreach (var m in actuales)
        {
            m.Activa = false;
            m.FechaFin = DateTime.Now;
        }

        _db.MembresiasUsuario.Add(new MembresiaUsuario
        {
            UserId = userId,
            PlanMembresiaId = planId,
            FechaInicio = DateTime.Now,
            Activa = true
        });
        await _db.SaveChangesAsync();
        TempData["PerfilOk"] = $"You're enrolled in {plan.Nombre}.";
        var signupState = GetMembershipSignup() ?? new MembershipSignupState();
        signupState.PlanId = planId;
        SaveMembershipSignup(signupState);
        return RedirectToAction(nameof(MembresiaExito));
    }
}
