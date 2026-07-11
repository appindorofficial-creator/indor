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
    private readonly AccountDeletionService _accountDeletion;
    private readonly ILogger<PerfilController> _logger;
    private readonly IIndorLocalizer _localizer;

    public PerfilController(AppDbContext db,
                            UserManager<ApplicationUser> userManager,
                            SignInManager<ApplicationUser> signInManager,
                            IWebHostEnvironment env,
                            IHomeownerPropertyService homeownerPropertyService,
                            AccountDeletionService accountDeletion,
                            ILogger<PerfilController> logger,
                            IIndorLocalizer localizer)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
        _homeownerPropertyService = homeownerPropertyService;
        _accountDeletion = accountDeletion;
        _logger = logger;
        _localizer = localizer;
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

    private string ResolveProfileBackUrl(string? from) =>
        string.Equals(from, "opciones", StringComparison.OrdinalIgnoreCase)
            ? Url.Action(nameof(Opciones)) ?? HomeNavigationUrls.MoreTab(Url)
            : HomeNavigationUrls.MoreTab(Url);

    private void SetMoreSectionBackUrl(string? from = null)
    {
        ViewData["MembershipBackUrl"] = ResolveProfileBackUrl(from);
        ViewBag.BottomNavActive ??= "more";
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

    private IActionResult? RedirectIfPaidMembershipDisabled()
    {
        if (ProfileDisplayService.PaidMembershipEnabled)
        {
            return null;
        }

        return RedirectToAction("Index", "Home", new { section = "more" });
    }

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
        ViewData["Title"] = _localizer.T("Profile Options");
        ViewData["Subtitulo"] = _localizer.T("Manage your account details and preferences.");
        SetMoreSectionBackUrl();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> EditarPerfil(string? from)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await CargarUsuarioYMembresiaAsync();
        ViewData["Title"] = _localizer.T("Edit Profile");
        ViewData["Subtitulo"] = _localizer.T("Update your details and connect your home with AI.");
        SetMoreSectionBackUrl(from);
        return View(await MapEditProfileViewModelAsync(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestTimeout("OnboardingAddressLookup")]
    public async Task<IActionResult> EnriquecerPropiedad([Bind(Prefix = "AddressForm")] AddPropertyViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await RestoreSavedAddressWhenFormIncompleteAsync(user.Id, model);
        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid)
        {
            TempData["PerfilError"] = _localizer.T("Please complete your street address, city, state, and ZIP code.");
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#home");
        }

        var userId = user.Id;
        var existing = await _homeownerPropertyService.GetPrimaryPropertyAsync(userId);

        try
        {
            // Never run the full multi-minute House Fact research inline: Azure App
            // Service kills any HTTP request that runs past ~230s, which would abort
            // the request before SaveOrUpdatePropertyAsync and leave nothing saved.
            // Do the fast enrichment synchronously and let SaveOrUpdatePropertyAsync
            // queue the full research in the background (BackgroundEnrichmentTimeout).
            var propertyInfo = await _homeownerPropertyService
                .EnrichAddressAsync(model, requestFullHouseFactResearch: false)
                .WaitAsync(AddressLookupTimeout, HttpContext.RequestAborted);

            if (propertyInfo == null)
            {
                TempData["PerfilError"] = _localizer.T("No information was found for this address. Try a more specific address.");
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
                TempData["PerfilOk"] = _localizer.T("Your home profile is ready — House Facts and maintenance insights are now available.");
                TempData["HomeEnriched"] = true;
            }
            else if (!string.IsNullOrWhiteSpace(propertyInfo.AttomRawJson))
            {
                TempData["PerfilOk"] =
                    _localizer.T("Address saved with basic property details. Full House Facts are loading — refresh in about 1 minute.");
            }
            else
            {
                _logger.LogWarning(
                    "Home enrichment incomplete for {Address}. RawJson={HasJson}, Error={Error}",
                    model.BuildLookupAddress(),
                    !string.IsNullOrWhiteSpace(propertyInfo.AttomRawJson),
                    propertyInfo.EnrichmentError ?? "none");

                if (!string.IsNullOrWhiteSpace(propertyInfo.EnrichmentError))
                {
                    // Surface the real reason (e.g. "OpenAI property enrichment is not
                    // configured") so a misconfigured/timed-out server can be diagnosed
                    // instead of showing a misleading "AI is researching" message.
                    TempData["PerfilError"] =
                        _localizer.T("Address saved, but AI research could not complete: {0} Please try again in a moment.", propertyInfo.EnrichmentError);
                }
                else
                {
                    TempData["PerfilOk"] =
                        _localizer.T("Address saved! AI is researching your home now — refresh this page in about 1 minute to see House Facts.");
                }
            }

            return Redirect(Url.Action(nameof(EditarPerfil), new { id = propiedadId }) + "#home");
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Home enrichment timed out for {Address}", model.BuildLookupAddress());
            TempData["PerfilError"] = _localizer.T("Address lookup is taking longer than expected. Please try again in a moment.");
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#home");
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            TempData["PerfilError"] = _localizer.T("Address lookup was interrupted. Please try again.");
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error enriching homeowner property");
            TempData["PerfilError"] = _localizer.T("We couldn't research this address right now. Please try again.");
            return Redirect(Url.Action(nameof(EditarPerfil)) + "#home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> EditarPerfil(
        string nombre,
        string apellidos,
        string telefono,
        IFormFile? foto,
        [Bind(Prefix = "AddressForm")] AddPropertyViewModel? addressForm)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        nombre = nombre?.Trim() ?? string.Empty;
        apellidos = apellidos?.Trim() ?? string.Empty;
        telefono = telefono?.Trim() ?? string.Empty;
        addressForm ??= new AddPropertyViewModel();

        if (string.IsNullOrWhiteSpace(nombre)
            || string.IsNullOrWhiteSpace(apellidos)
            || string.IsNullOrWhiteSpace(telefono))
        {
            return await ReturnEditProfileViewAsync(
                user,
                _localizer.T("First name, last name, and phone are required."),
                nombre,
                apellidos,
                telefono,
                addressForm);
        }

        if (!UsPhoneOptionalAttribute.IsValidOptional(telefono))
        {
            return await ReturnEditProfileViewAsync(
                user,
                _localizer.T("Enter a valid 10-digit US phone number (e.g. 555 123 4567)."),
                nombre,
                apellidos,
                telefono,
                addressForm);
        }

        if (HasAnyAddressField(addressForm) && !IsAddressFormComplete(addressForm))
        {
            return await ReturnEditProfileViewAsync(
                user,
                _localizer.T("Complete street address, city, state, and ZIP code to save your home."),
                nombre,
                apellidos,
                telefono,
                addressForm);
        }

        if (IsAddressFormComplete(addressForm))
        {
            if (!TryValidateModel(addressForm, prefix: "AddressForm"))
            {
                var addressError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(e => !string.IsNullOrWhiteSpace(e))
                    ?? _localizer.T("Enter a valid home address.");
                return await ReturnEditProfileViewAsync(
                    user,
                    addressError,
                    nombre,
                    apellidos,
                    telefono,
                    addressForm);
            }
        }

        user.Nombre = nombre;
        user.Apellidos = apellidos;
        var phone = UsPhoneOptionalAttribute.NormalizeToStorage(telefono) ?? telefono;
        user.Telefono = phone;
        user.PhoneNumber = phone;

        var photoError = await TrySaveHomeownerPhotoAsync(user, foto);
        if (!string.IsNullOrWhiteSpace(photoError))
        {
            return await ReturnEditProfileViewAsync(user, photoError, nombre, apellidos, telefono, addressForm);
        }

        await _userManager.UpdateAsync(user);

        var savedHome = false;
        if (IsAddressFormComplete(addressForm))
        {
            savedHome = await _homeownerPropertyService.SaveHomeAddressAsync(
                addressForm,
                user.Id,
                HttpContext.RequestAborted) is > 0;
        }

        TempData["PerfilOk"] = savedHome
            ? _localizer.T("Profile and home address saved successfully.")
            : _localizer.T("Profile updated successfully.");
        return RedirectToAction("Index", "Home");
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
        ViewData["Title"] = _localizer.T("Payments & History");
        ViewData["Subtitulo"] = _localizer.T("Track services, billing, and financing in one place.");
        SetMoreSectionBackUrl();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Suscripciones(int? planId)
    {
        if (RedirectIfPaidMembershipDisabled() is { } disabled)
        {
            return disabled;
        }

        await CargarUsuarioYMembresiaAsync();
        ViewBag.Planes = await _db.PlanesMembresia
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync();
        ViewBag.SelectedPlanId = planId ?? GetMembershipSignup()?.PlanId;
        ViewData["Title"] = _localizer.T("Choose your membership");
        ViewData["Subtitulo"] = _localizer.T("Pick the plan that fits your home care needs.");
        ViewData["MembershipStep"] = 1;
        ViewData["MembershipTotalSteps"] = 6;
        ViewData["MembershipBackUrl"] = HomeNavigationUrls.MoreTab(Url);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeleccionarPlan(int planId)
    {
        if (RedirectIfPaidMembershipDisabled() is { } disabled)
        {
            return disabled;
        }

        var plan = await _db.PlanesMembresia.FirstOrDefaultAsync(p => p.Id == planId && p.Activo);
        if (plan == null)
        {
            TempData["PerfilError"] = _localizer.T("Plan not found.");
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
        if (RedirectIfPaidMembershipDisabled() is { } disabled)
        {
            return disabled;
        }

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
                ViewData["Title"] = _localizer.T("Filter Plan details");
                ViewData["Subtitulo"] = _localizer.T("See how your {0}/month plan works.", price);
                ViewData["MembershipStep"] = 2;
                break;
            case MembershipPlanKind.HomeCare:
                ViewData["Title"] = _localizer.T("Home Care Plan");
                ViewData["Subtitulo"] = _localizer.T("Everything you need for ongoing home care.");
                ViewData["MembershipStep"] = 2;
                break;
            case MembershipPlanKind.Premium:
                ViewData["Title"] = _localizer.T("Premium Care Plan");
                ViewData["Subtitulo"] = _localizer.T("Best for proactive homeowners.");
                ViewData["MembershipStep"] = 2;
                break;
            default:
                ViewData["Title"] = plan.LocalizedNombre(_localizer.IsSpanish);
                ViewData["Subtitulo"] = _localizer.T("See how your {0}/month plan works.", price);
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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

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
        ViewData["Title"] = _localizer.T("Tell us about your filter");
        ViewData["Subtitulo"] = _localizer.T("We'll send the right filter to your home.");
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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

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
        ViewData["Title"] = _localizer.T("Set up your filter delivery");
        ViewData["Subtitulo"] = _localizer.T("Confirm where and what we should send every 3 months.");
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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

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
        ViewData["Title"] = _localizer.T("Reminders & member benefits");
        ViewData["Subtitulo"] = _localizer.T("Choose the maintenance alerts you want and see how your savings work.");
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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

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
        ViewData["Title"] = _localizer.T("Delivery setup");
        ViewData["Subtitulo"] = _localizer.T("Choose where and when your filter should arrive.");
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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

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
                ViewData["Title"] = _localizer.T("Review & payment");
                ViewData["Subtitulo"] = _localizer.T("Confirm your Filter Plan before subscribing.");
                ViewData["MembershipBackUrl"] = Url.Action(nameof(MembresiaEntrega));
                break;
            case MembershipPlanKind.HomeCare:
            case MembershipPlanKind.Premium:
                ViewData["Title"] = _localizer.T("Review & payment");
                ViewData["Subtitulo"] = _localizer.T("Confirm your {0} before activating your membership.", plan.LocalizedNombre(_localizer.IsSpanish));
                ViewData["MembershipBackUrl"] = Url.Action(nameof(MembresiaBeneficios));
                break;
            default:
                ViewData["Title"] = _localizer.T("Review & confirm your plan");
                ViewData["Subtitulo"] = _localizer.T("Almost there! Complete your subscription setup.");
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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

        var state = GetMembershipSignup();
        if (state == null) return RedirectToAction(nameof(Suscripciones));

        var plan = await _db.PlanesMembresia.FirstOrDefaultAsync(p => p.Id == state.PlanId && p.Activo);
        if (plan == null) return RedirectToAction(nameof(Suscripciones));

        var kind = ProfileDisplayService.GetPlanKind(plan);
        var skipBillingCheckbox = kind is MembershipPlanKind.Filter or MembershipPlanKind.HomeCare or MembershipPlanKind.Premium;
        if (plan.PrecioMensual > 0 && !skipBillingCheckbox && !agreeBilling)
        {
            TempData["PerfilError"] = _localizer.T("Please agree to recurring billing to continue.");
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
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

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
                ViewData["Title"] = _localizer.T("You're all set!");
                ViewData["Subtitulo"] = _localizer.T("Your Filter Plan is now active.");
                break;
            case MembershipPlanKind.HomeCare:
                ViewData["Title"] = _localizer.T("Welcome to Home Care Plan");
                ViewData["Subtitulo"] = _localizer.T("Your membership is active.");
                break;
            case MembershipPlanKind.Premium:
                ViewData["Title"] = _localizer.T("Welcome to Premium Care Plan");
                ViewData["Subtitulo"] = _localizer.T("Your membership is active.");
                break;
            default:
                ViewData["Title"] = _localizer.T("You're enrolled");
                ViewData["Subtitulo"] = plan != null
                    ? _localizer.T("Your membership in {0} is active.", plan.LocalizedNombre(_localizer.IsSpanish))
                    : _localizer.T("Your plan is now active.");
                break;
        }
        ViewData["MembershipStep"] = 6;
        ViewData["MembershipTotalSteps"] = 6;
        ViewData["MembershipBackUrl"] = Url.Action(nameof(Suscripciones));
        ClearMembershipSignup();
        return View();
    }

    private async Task<IActionResult> ReturnEditProfileViewAsync(
        ApplicationUser user,
        string error,
        string nombre,
        string apellidos,
        string telefono,
        AddPropertyViewModel? addressForm = null)
    {
        TempData["PerfilError"] = error;
        ViewData["Title"] = _localizer.T("Edit Profile");
        ViewData["Subtitulo"] = _localizer.T("Update your name, phone, and profile photo.");
        SetMoreSectionBackUrl(Request.Query["from"].ToString());
        var model = await MapEditProfileViewModelAsync(user);
        model.Nombre = nombre;
        model.Apellidos = apellidos;
        model.Telefono = telefono;
        if (addressForm != null)
        {
            model.AddressForm = addressForm;
        }
        return View(model);
    }

    private static bool HasAnyAddressField(AddPropertyViewModel model) =>
        !string.IsNullOrWhiteSpace(model.StreetAddress)
        || !string.IsNullOrWhiteSpace(model.City)
        || !string.IsNullOrWhiteSpace(model.State)
        || !string.IsNullOrWhiteSpace(model.ZipCode);

    private static bool IsAddressFormComplete(AddPropertyViewModel model) =>
        !string.IsNullOrWhiteSpace(model.StreetAddress)
        && !string.IsNullOrWhiteSpace(model.City)
        && !string.IsNullOrWhiteSpace(model.State)
        && !string.IsNullOrWhiteSpace(model.ZipCode);

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

        model.AddressForm = MyHomeDisplayService.BuildAddressFormForEdit(propiedad, info);

        return model;
    }

    private async Task RestoreSavedAddressWhenFormIncompleteAsync(string userId, AddPropertyViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.StreetAddress)
            && !string.IsNullOrWhiteSpace(model.City)
            && !string.IsNullOrWhiteSpace(model.State)
            && !string.IsNullOrWhiteSpace(model.ZipCode))
        {
            return;
        }

        var propiedad = await _homeownerPropertyService.GetPrimaryPropertyAsync(userId);
        if (propiedad == null)
        {
            return;
        }

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        var saved = MyHomeDisplayService.BuildAddressFormForEdit(propiedad, info);

        if (string.IsNullOrWhiteSpace(model.StreetAddress) && !string.IsNullOrWhiteSpace(saved.StreetAddress))
        {
            model.StreetAddress = saved.StreetAddress;
        }

        if (string.IsNullOrWhiteSpace(model.City) && !string.IsNullOrWhiteSpace(saved.City))
        {
            model.City = saved.City;
        }

        if (string.IsNullOrWhiteSpace(model.State) && !string.IsNullOrWhiteSpace(saved.State))
        {
            model.State = saved.State;
        }

        if (string.IsNullOrWhiteSpace(model.ZipCode) && !string.IsNullOrWhiteSpace(saved.ZipCode))
        {
            model.ZipCode = saved.ZipCode;
        }

        if (string.IsNullOrWhiteSpace(model.Unit) && !string.IsNullOrWhiteSpace(saved.Unit))
        {
            model.Unit = saved.Unit;
        }
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
            return _localizer.T("Photo must be JPG, PNG, or WEBP.");
        }

        if (foto.Length > MaxProfilePhotoBytes)
        {
            return _localizer.T("Photo must be 10 MB or less.");
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
        ViewData["Title"] = _localizer.T("History");
        ViewData["Subtitulo"] = _localizer.T("Microservices, inspections, and past services");
        SetMoreSectionBackUrl();
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
        ViewData["Title"] = _localizer.T("Internet comparison");
        ViewData["Subtitulo"] = _localizer.T("Compare internet plans and providers");
        SetMoreSectionBackUrl();
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
        ViewData["Title"] = _localizer.T("Support");
        ViewData["Subtitulo"] = _localizer.T("Chat with our team");
        SetMoreSectionBackUrl();
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
            telefono = telefono.Trim();
            if (!UsPhoneOptionalAttribute.IsValidOptional(telefono))
            {
                TempData["PerfilError"] = _localizer.T("Enter a valid 10-digit US phone number (e.g. 555 123 4567).");
                return RedirectToAction(nameof(Opciones));
            }

            var phone = UsPhoneOptionalAttribute.NormalizeToStorage(telefono) ?? telefono;
            user.Telefono = phone;
            user.PhoneNumber = phone;
        }

        await _userManager.UpdateAsync(user);
        TempData["PerfilOk"] = _localizer.T("Profile updated successfully.");
        return RedirectToAction("Index", "Home", new { section = "more" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> SubirFoto(IFormFile? foto, IFormFile? photo)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return IsAjaxPhotoUploadRequest()
                ? Unauthorized()
                : RedirectToAction("Login", "Account");
        }

        var file = foto ?? photo;
        if (file == null || file.Length == 0)
        {
            return HomeownerPhotoUploadResult(_localizer.T("Please choose a photo to upload."));
        }

        var photoError = await TrySaveHomeownerPhotoAsync(user, file);
        if (!string.IsNullOrWhiteSpace(photoError))
        {
            return HomeownerPhotoUploadResult(photoError);
        }

        await _userManager.UpdateAsync(user);
        return HomeownerPhotoUploadResult(null, user.FotoUrl);
    }

    private IActionResult HomeownerPhotoUploadResult(string? error, string? photoUrl = null)
    {
        if (IsAjaxPhotoUploadRequest())
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                return BadRequest(new { ok = false, message = error });
            }

            return Json(new { ok = true, message = _localizer.T("Profile photo updated."), photoUrl });
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            TempData["PerfilError"] = error;
        }
        else
        {
            TempData["PerfilOk"] = _localizer.T("Photo updated.");
        }

        var returnTo = Request.Form["returnTo"].ToString();
        if (string.Equals(returnTo, "editar", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EditarPerfil));
        }

        return RedirectToAction("Index", "Home", new { section = "more" });
    }

    private bool IsAjaxPhotoUploadRequest() =>
        string.Equals(Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPassword(string actual, string nueva, string confirmar)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (string.IsNullOrEmpty(nueva) || nueva != confirmar)
        {
            TempData["PerfilError"] = _localizer.T("Passwords do not match.");
            return RedirectToAction(nameof(Opciones));
        }

        var result = await _userManager.ChangePasswordAsync(user, actual ?? string.Empty, nueva);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
            TempData["PerfilOk"] = _localizer.T("Password updated.");
        }
        else
        {
            TempData["PerfilError"] = string.Join(" ", result.Errors.Select(e => e.Description));
        }
        return RedirectToAction(nameof(Opciones));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCuenta(string? confirmEmail)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!string.Equals(confirmEmail?.Trim(), user.Email, StringComparison.OrdinalIgnoreCase))
        {
            TempData["PerfilError"] = _localizer.T("Enter your account email exactly to confirm account deletion.");
            return RedirectToAction(nameof(Opciones));
        }

        var deleted = await _accountDeletion.DeleteAccountAsync(user);
        if (!deleted)
        {
            TempData["PerfilError"] = _localizer.T("We could not delete your account right now. Please contact support.");
            return RedirectToAction(nameof(Opciones));
        }

        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        TempData["AccountDeleted"] = _localizer.T("Your account and associated data have been permanently deleted.");
        return RedirectToAction("Welcome", "Account");
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
        TempData["PerfilOk"] = _localizer.T("Payment method added.");
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
            Contenido = _localizer.T("Hello! We received your message. An agent will reply soon.")
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Soporte));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ActivarPlan(int planId) => ActivarPlanInternal(planId);

    private async Task<IActionResult> ActivarPlanInternal(int planId)
    {
        if (RedirectIfPaidMembershipDisabled() is { } disabled) return disabled;

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

        var plan = await _db.PlanesMembresia.FirstOrDefaultAsync(p => p.Id == planId && p.Activo);
        if (plan == null)
        {
            TempData["PerfilError"] = _localizer.T("Plan not found.");
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
        TempData["PerfilOk"] = _localizer.T("You're enrolled in {0}.", plan.LocalizedNombre(_localizer.IsSpanish));
        var signupState = GetMembershipSignup() ?? new MembershipSignupState();
        signupState.PlanId = planId;
        SaveMembershipSignup(signupState);
        return RedirectToAction(nameof(MembresiaExito));
    }
}
