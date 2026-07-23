using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Localization;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.Validation;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _db;
    private readonly IProviderRegistrationService _registration;
    private readonly IRealtorRegistrationService _realtorRegistration;
    private readonly IPropertyAdministratorRegistrationService _propertyAdministratorRegistration;
    private readonly IPasswordResetEmailSender _passwordResetEmail;
    private readonly AccountDeletionService _accountDeletion;
    private readonly IUiCultureCookieService _cultureCookie;
    private readonly IIndorLocalizer _localizer;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext db,
        IProviderRegistrationService registration,
        IRealtorRegistrationService realtorRegistration,
        IPropertyAdministratorRegistrationService propertyAdministratorRegistration,
        IPasswordResetEmailSender passwordResetEmail,
        AccountDeletionService accountDeletion,
        IUiCultureCookieService cultureCookie,
        IIndorLocalizer localizer,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _db = db;
        _registration = registration;
        _realtorRegistration = realtorRegistration;
        _propertyAdministratorRegistration = propertyAdministratorRegistration;
        _passwordResetEmail = passwordResetEmail;
        _accountDeletion = accountDeletion;
        _cultureCookie = cultureCookie;
        _localizer = localizer;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Email) && !ValidEmailAttribute.IsValidAddress(model.Email, out var emailError))
        {
            ModelState.AddModelError(nameof(model.Email),
                _localizer[emailError ?? "Enter a valid email address."]);
        }

        if (!UsPhoneOptionalAttribute.IsValidOptional(model.Telefono))
        {
            ModelState.AddModelError(nameof(model.Telefono),
                _localizer["Enter a valid 10-digit US phone number (e.g. 555 123 4567)."]);
        }

        if (ModelState.IsValid)
        {
            SplitFullName(model);
            var phone = UsPhoneOptionalAttribute.NormalizeToStorage(model.Telefono);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre,
                Apellidos = model.Apellidos,
                Telefono = phone ?? string.Empty,
                PhoneNumber = phone,
                PreferredUiCulture = ResolveRegistrationCulture(model.UiCulture),
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: true);
                _cultureCookie.SetCulture(Response, user.PreferredUiCulture ?? UiCulture.English);
                return RedirectToAction(nameof(SelectRole), new { userId = user.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ModelState.LocalizeModelState(_localizer);
        ViewBag.OnboardingStep = 1;
        ViewBag.OnboardingTitle = _localizer["Create Account"];
        ViewBag.OnboardingBackUrl = Url.Action(nameof(Welcome));
        ViewBag.OnboardingShowBack = true;
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.OnboardingStep = 1;
        ViewBag.OnboardingTitle = _localizer["Create Account"];
        ViewBag.OnboardingBackUrl = Url.Action(nameof(Welcome));
        ViewBag.OnboardingShowBack = true;
        return View(new RegisterViewModel
        {
            UiCulture = _cultureCookie.GetCulture(Request) ?? UiCulture.English
        });
    }

    private static void SplitFullName(RegisterViewModel model)
    {
        var parts = model.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        model.Nombre = parts.Length > 0 ? parts[0] : model.FullName.Trim();
        model.Apellidos = parts.Length > 1 ? parts[1] : string.Empty;
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Welcome()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            // Paint branded splash immediately; resolve role/dashboard on ContinueEntry
            // so cold entry never blocks first HTML on DB lookups.
            return EntryLoadingResult();
        }

        return View();
    }

    /// <summary>
    /// Lightweight post-auth handoff: shows INDOR loading shell, then routes to the
    /// correct home. Keeps first paint fast when opening the app or after login.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ContinueEntry(string? returnUrl = null)
    {
        return await RedirectAuthenticatedUserAsync(returnUrl);
    }

    private IActionResult EntryLoadingResult(string? returnUrl = null)
    {
        ViewData["ContinueUrl"] = Url.Action(nameof(ContinueEntry), new { returnUrl });
        return View("EntryLoading");
    }

    [HttpGet]
    public IActionResult Terms(string? from = null, int? propiedadId = null)
    {
        ViewBag.OnboardingTitle = _localizer["Terms & Conditions"];
        ViewBag.OnboardingBackUrl = ResolveLegalBackUrl(from, propiedadId);
        ViewBag.OnboardingShowBack = true;
        ViewBag.LegalFrom = from;
        ViewBag.LegalPropiedadId = propiedadId;
        return View();
    }

    [HttpGet]
    public IActionResult Privacy(string? from = null, int? propiedadId = null)
    {
        ViewBag.OnboardingTitle = _localizer["Privacy Policy"];
        ViewBag.OnboardingBackUrl = ResolveLegalBackUrl(from, propiedadId);
        ViewBag.OnboardingShowBack = true;
        ViewBag.LegalFrom = from;
        ViewBag.LegalPropiedadId = propiedadId;
        return View();
    }

    private string? ResolveLegalBackUrl(string? from, int? propiedadId)
    {
        if (from == "register")
        {
            return Url.Action(nameof(Register));
        }

        if (from == "hvac-setup" && propiedadId is > 0)
        {
            return Url.Action("Review", "HvacSetup", new { propiedadId });
        }

        if (from == "provider-entry")
        {
            return Url.Action("Entry", "ProviderRegistration");
        }

        if (from == "provider-company-info")
        {
            return Url.Action("CompanyInfo", "ProviderRegistration");
        }

        if (from == "provider-activation")
        {
            return Url.Action("ActivationCall", "ProviderRegistration");
        }

        if (from == "provider-help")
        {
            return Url.Action("HelpCenter", "Proveedor");
        }

        if (from == "realtor-support")
        {
            return Url.Action("SupportAccount", "Realtor");
        }

        if (from == "homeowner-profile")
        {
            return Url.Action("Opciones", "Perfil");
        }

        return Url.Action(nameof(Welcome));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return EntryLoadingResult(returnUrl);
        }

        return RedirectToAction(nameof(Welcome));
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult LoginForm(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return EntryLoadingResult(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            // Always persist the auth cookie on mobile/WebView — a session cookie is
            // dropped when Android kills the process, which dumps users back at
            // Welcome/SelectRole/Entry after briefly leaving the app.
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                isPersistent: true,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await ApplyUserCultureAfterLoginAsync(user);
                }

                // Splash HTML (with auth cookie) instead of a bare 302 so the WebView
                // shows INDOR loading while role/dashboard routing runs.
                return EntryLoadingResult(returnUrl);
            }

            ModelState.AddModelError(string.Empty, _localizer["Invalid login attempt."]);
        }

        ModelState.LocalizeModelState(_localizer);
        return View("LoginForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Welcome));
    }

    [HttpGet]
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(LoginForm));
        }

        ViewBag.OnboardingTitle = _localizer["Delete account"];
        ViewBag.OnboardingShowBack = true;
        ViewBag.Email = user.Email;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    [ActionName("DeleteAccount")]
    public async Task<IActionResult> DeleteAccountConfirmed(string? confirmEmail)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(LoginForm));
        }

        if (!string.Equals(confirmEmail?.Trim(), user.Email, StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.OnboardingTitle = _localizer["Delete account"];
            ViewBag.OnboardingShowBack = true;
            ViewBag.Email = user.Email;
            ModelState.AddModelError(string.Empty, _localizer["Enter your account email exactly to confirm account deletion."]);
            return View("DeleteAccount");
        }

        var deleted = await _accountDeletion.DeleteAccountAsync(user);
        if (!deleted)
        {
            ViewBag.OnboardingTitle = _localizer["Delete account"];
            ViewBag.OnboardingShowBack = true;
            ViewBag.Email = user.Email;
            ModelState.AddModelError(string.Empty, _localizer["We could not delete your account right now. Please contact support."]);
            return View("DeleteAccount");
        }

        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        TempData["AccountDeleted"] = _localizer["Your account and associated data have been permanently deleted."];
        return RedirectToAction(nameof(Welcome));
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult ForgotPassword()
    {
        ViewBag.OnboardingTitle = _localizer["Reset password"];
        ViewBag.OnboardingBackUrl = Url.Action(nameof(LoginForm));
        ViewBag.OnboardingShowBack = true;
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.LocalizeModelState(_localizer);
            ViewBag.OnboardingTitle = _localizer["Reset password"];
            ViewBag.OnboardingBackUrl = Url.Action(nameof(LoginForm));
            ViewBag.OnboardingShowBack = true;
            return View(model);
        }

        var email = model.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        // Only generate/send when the account exists, but always respond the same
        // way so we don't reveal which emails are registered.
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            _logger.LogInformation("ForgotPassword: matching account found for {Email}; sending reset email.", email);
            var now = DateTime.UtcNow;

            // Invalidate any previous codes still pending for this user.
            var pending = await _db.IndorPasswordResetCodes
                .Where(c => c.UserId == user.Id && !c.Used)
                .ToListAsync();
            foreach (var old in pending)
            {
                old.Used = true;
                old.UsedUtc = now;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var code = GenerateSixDigitCode();

            _db.IndorPasswordResetCodes.Add(new IndorPasswordResetCode
            {
                UserId = user.Id,
                Email = user.Email,
                Code = code,
                ResetToken = token,
                ExpiresUtc = now.AddHours(24),
                FechaCreacion = now
            });
            await _db.SaveChangesAsync();

            var resetUrl = Url.Action(nameof(ResetPassword), "Account",
                new { email = user.Email, code }, Request.Scheme) ?? string.Empty;
            var displayName = BuildDisplayName(user);

            await _passwordResetEmail.SendPasswordResetEmailAsync(
                new PasswordResetEmailModel(user.Email, displayName, code, resetUrl, 24));
        }
        else
        {
            // No account matched the address entered. By design we still show the same
            // confirmation screen (anti-enumeration), but log it so support can tell
            // "wrong/unregistered email" apart from a real delivery failure.
            _logger.LogInformation("ForgotPassword: no account matched {Email}; no email sent (anti-enumeration).", email);
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation), new { email });
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation(string? email = null)
    {
        ViewBag.OnboardingTitle = _localizer["Check your email"];
        ViewBag.OnboardingBackUrl = Url.Action(nameof(LoginForm));
        ViewBag.OnboardingShowBack = true;
        ViewBag.Email = email;
        return View();
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult ResetPassword(string? email = null, string? code = null)
    {
        ViewBag.OnboardingTitle = _localizer["Set a new password"];
        ViewBag.OnboardingBackUrl = Url.Action(nameof(LoginForm));
        ViewBag.OnboardingShowBack = true;
        return View(new ResetPasswordViewModel
        {
            Email = email ?? string.Empty,
            Code = code ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        ViewBag.OnboardingTitle = _localizer["Set a new password"];
        ViewBag.OnboardingBackUrl = Url.Action(nameof(LoginForm));
        ViewBag.OnboardingShowBack = true;

        if (!ModelState.IsValid)
        {
            ModelState.LocalizeModelState(_localizer);
            return View(model);
        }

        var email = model.Email.Trim();
        var code = model.Code.Trim();
        var now = DateTime.UtcNow;

        var record = await _db.IndorPasswordResetCodes
            .Where(c => c.Email == email && c.Code == code && !c.Used)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        if (record == null || record.ExpiresUtc < now)
        {
            if (record != null)
            {
                record.Attempts += 1;
                await _db.SaveChangesAsync();
            }

            ModelState.AddModelError(string.Empty,
                _localizer["This code is invalid or has expired. Please request a new one."]);
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(record.UserId)
                   ?? await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, _localizer["We couldn't find an account for this email."]);
            return View(model);
        }

        var result = await _userManager.ResetPasswordAsync(user, record.ResetToken, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            record.Attempts += 1;
            await _db.SaveChangesAsync();
            ModelState.LocalizeModelState(_localizer);
            return View(model);
        }

        record.Used = true;
        record.UsedUtc = now;

        // Burn any other pending codes for this user too.
        var others = await _db.IndorPasswordResetCodes
            .Where(c => c.UserId == record.UserId && !c.Used && c.Id != record.Id)
            .ToListAsync();
        foreach (var o in others)
        {
            o.Used = true;
            o.UsedUtc = now;
        }
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        ViewBag.OnboardingTitle = _localizer["Password updated"];
        ViewBag.OnboardingBackUrl = Url.Action(nameof(LoginForm));
        ViewBag.OnboardingShowBack = true;
        return View();
    }

    private static string GenerateSixDigitCode()
    {
        // Cryptographically strong 6-digit code (000000-999999).
        var value = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6");
    }

    private static string BuildDisplayName(ApplicationUser user)
    {
        var name = $"{user.Nombre} {user.Apellidos}".Trim();
        return string.IsNullOrWhiteSpace(name) ? (user.Email ?? string.Empty) : name;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> SelectRole(string? userId = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Challenge();
        }

        // Missing/mismatched userId used to Challenge() → LoginForm?ReturnUrl=/Account/SelectRole
        // → authenticated LoginForm redirects back to SelectRole → ERR_TOO_MANY_REDIRECTS.
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction(nameof(SelectRole), new { userId = currentUser.Id });
        }

        if (!string.Equals(currentUser.Id, userId, StringComparison.Ordinal))
        {
            return RedirectToAction(nameof(SelectRole), new { userId = currentUser.Id });
        }

        if (!string.IsNullOrEmpty(currentUser.RolUsuario)
            && !await CanRevisitRoleSelectionAsync(currentUser))
        {
            return await RedirectAuthenticatedUserAsync(user: currentUser);
        }

        ViewBag.OnboardingStep = 2;
        ViewBag.OnboardingTitle = _localizer["How will you use INDOR?"];
        ViewBag.OnboardingBackUrl = Url.Action(nameof(Welcome));
        ViewBag.OnboardingShowBack = true;

        return View(new SelectRoleViewModel
        {
            UserId = currentUser.Id,
            SelectedRole = string.IsNullOrWhiteSpace(currentUser.RolUsuario)
                ? "Propietario"
                : currentUser.RolUsuario
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> SelectRole(SelectRoleViewModel model, string? skipLater)
    {
        if (string.Equals(skipLater, "true", StringComparison.OrdinalIgnoreCase))
        {
            model.SelectedRole = "Propietario";
            ModelState.Remove(nameof(model.SelectedRole));
        }

        if (ModelState.IsValid && !string.IsNullOrEmpty(model.SelectedRole))
        {
            var currentUserId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null || currentUserId == null || user.Id != currentUserId)
            {
                return RedirectToAction(nameof(SelectRole), new { userId = currentUserId });
            }

            user.RolUsuario = model.SelectedRole;
            await _userManager.UpdateAsync(user);

            if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.SelectedRole));
            }

            if (!await _userManager.IsInRoleAsync(user, model.SelectedRole))
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            // Re-issue a persistent auth cookie so role claims and mobile sessions stay valid.
            await _signInManager.SignInAsync(user, isPersistent: true);

            if (string.Equals(model.SelectedRole, "Propietario", StringComparison.OrdinalIgnoreCase))
            {
                TempData["OnboardingComplete"] = true;
            }

            return model.SelectedRole switch
            {
                "Propietario" => RedirectToAction("HomeReady", "Propietario", new { id = 0 }),
                "ProveedorServicios" => RedirectToAction("Entry", "ProviderRegistration"),
                "Realtor" => RedirectToAction("Profile", "RealtorRegistration"),
                "AdministradorPropiedades" => RedirectToAction("Profile", "PropertyAdministratorRegistration"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        ViewBag.OnboardingStep = 2;
        ViewBag.OnboardingTitle = _localizer["How will you use INDOR?"];
        ViewBag.OnboardingBackUrl = Url.Action(nameof(Welcome));
        ViewBag.OnboardingShowBack = true;
        return View(model);
    }

    private async Task<IActionResult> RedirectAuthenticatedUserAsync(string? returnUrl = null, ApplicationUser? user = null)
    {
        user ??= await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(LoginForm));
        }

        if (Url.IsLocalUrl(returnUrl))
        {
            // Never bounce authenticated users through LoginForm↔SelectRole with a bare ReturnUrl.
            if (IsSelectRolePath(returnUrl))
            {
                return RedirectToAction(nameof(SelectRole), new { userId = user.Id });
            }

            return Redirect(returnUrl);
        }

        if (!string.IsNullOrEmpty(user.RolUsuario))
        {
            if (string.Equals(user.RolUsuario, "ProveedorServicios", StringComparison.OrdinalIgnoreCase))
            {
                return await RedirectProveedorAsync(user);
            }

            if (string.Equals(user.RolUsuario, "Realtor", StringComparison.OrdinalIgnoreCase))
            {
                return await RedirectRealtorAsync(user);
            }

            if (string.Equals(user.RolUsuario, "AdministradorPropiedades", StringComparison.OrdinalIgnoreCase))
            {
                return await RedirectAdministradorAsync(user);
            }

            if (await _db.Propiedades.AnyAsync(p => p.UserId == user.Id && p.Activo))
            {
                return RedirectToAction("Index", "Home");
            }

            return user.RolUsuario switch
            {
                "Propietario" => RedirectToAction("Index", "Home"),
                "Realtor" => RedirectToAction("Profile", "RealtorRegistration"),
                "AdministradorPropiedades" => RedirectToAction("Profile", "PropertyAdministratorRegistration"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        if (await _db.Propiedades.AnyAsync(p => p.UserId == user.Id && p.Activo))
        {
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction(nameof(SelectRole), new { userId = user.Id });
    }

    private static bool IsSelectRolePath(string returnUrl)
    {
        var path = returnUrl.Split('?', 2)[0];
        return string.Equals(path, "/Account/SelectRole", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Providers (and other pros) mid-onboarding must be able to step Back to SelectRole
    /// without being immediately redirected forward again.
    /// </summary>
    private async Task<bool> CanRevisitRoleSelectionAsync(ApplicationUser user)
    {
        if (string.Equals(user.RolUsuario, "ProveedorServicios", StringComparison.OrdinalIgnoreCase))
        {
            var proveedor = await _db.IndorProveedores
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            return proveedor == null
                || string.Equals(proveedor.RegistrationStatus, ProviderRegistrationStatuses.Draft, StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(user.RolUsuario, "Realtor", StringComparison.OrdinalIgnoreCase))
        {
            var realtor = await _db.IndorRealtors
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == user.Id);

            return realtor == null
                || string.Equals(realtor.RegistrationStatus, RealtorRegistrationStatuses.Draft, StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(user.RolUsuario, "AdministradorPropiedades", StringComparison.OrdinalIgnoreCase))
        {
            var admin = await _db.IndorPropertyAdministrators
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == user.Id);

            return admin == null
                || string.Equals(admin.RegistrationStatus, PropertyAdministratorRegistrationStatuses.Draft, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private async Task<IActionResult> RedirectProveedorAsync(ApplicationUser user)
    {
        var proveedor = await _db.IndorProveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (proveedor == null)
        {
            return RedirectToAction("Entry", "ProviderRegistration");
        }

        if (string.Equals(proveedor.RegistrationStatus, ProviderRegistrationStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            var action = _registration.ResolveWizardResumeAction(proveedor);
            return RedirectToAction(action, "ProviderRegistration");
        }

        return RedirectToAction("Dashboard", "Proveedor");
    }

    private async Task<IActionResult> RedirectRealtorAsync(ApplicationUser user)
    {
        var realtor = await _db.IndorRealtors
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == user.Id);

        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        if (realtor.RegistrationStatus == RealtorRegistrationStatuses.Draft)
        {
            if (!string.IsNullOrWhiteSpace(realtor.BrokerageName))
            {
                await _realtorRegistration.CompleteVerificationAsync(skipped: true);
                return RedirectToAction("Ready", "RealtorRegistration");
            }

            return RedirectToAction("Profile", "RealtorRegistration");
        }

        return RedirectToAction("Dashboard", "Realtor");
    }

    private async Task<IActionResult> RedirectAdministradorAsync(ApplicationUser user)
    {
        var admin = await _db.IndorPropertyAdministrators
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == user.Id);

        if (admin == null)
        {
            return RedirectToAction("Profile", "PropertyAdministratorRegistration");
        }

        if (admin.RegistrationStatus == PropertyAdministratorRegistrationStatuses.Draft)
        {
            // Finish quickly if terms were already accepted; otherwise resume Profile only.
            if (admin.TermsAccepted)
            {
                await _propertyAdministratorRegistration.CompleteRegistrationAsync(platformTermsAccepted: true);
                return RedirectToAction("Index", "Administrador");
            }

            return RedirectToAction(
                _propertyAdministratorRegistration.ResolveWizardResumeAction(Math.Max(1, admin.CurrentStep)),
                "PropertyAdministratorRegistration");
        }

        return RedirectToAction("Index", "Administrador");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetLanguage(string culture, string? returnUrl = null)
    {
        var normalized = UiCulture.Normalize(culture);
        _cultureCookie.SetCulture(Response, normalized);

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.PreferredUiCulture = normalized;
                await _userManager.UpdateAsync(user);
            }
        }

        TempData["LanguageUpdated"] = true;

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Welcome));
    }

    private string ResolveRegistrationCulture(string? requestedCulture)
    {
        if (UiCulture.IsSupported(requestedCulture))
        {
            return UiCulture.Normalize(requestedCulture);
        }

        var fromCookie = _cultureCookie.GetCulture(Request);
        return UiCulture.IsSupported(fromCookie)
            ? UiCulture.Normalize(fromCookie)
            : UiCulture.English;
    }

    private async Task ApplyUserCultureAfterLoginAsync(ApplicationUser user)
    {
        var cookieCulture = _cultureCookie.GetCulture(Request);
        if (UiCulture.IsSupported(cookieCulture))
        {
            var normalized = UiCulture.Normalize(cookieCulture!);
            _cultureCookie.SetCulture(Response, normalized);

            if (!string.Equals(user.PreferredUiCulture, normalized, StringComparison.OrdinalIgnoreCase))
            {
                user.PreferredUiCulture = normalized;
                await _userManager.UpdateAsync(user);
            }

            return;
        }

        if (UiCulture.IsSupported(user.PreferredUiCulture))
        {
            _cultureCookie.SetCulture(Response, user.PreferredUiCulture!);
        }
    }
}
