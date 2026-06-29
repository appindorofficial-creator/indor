using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
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

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext db,
        IProviderRegistrationService registration,
        IRealtorRegistrationService realtorRegistration,
        IPropertyAdministratorRegistrationService propertyAdministratorRegistration,
        IPasswordResetEmailSender passwordResetEmail)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _db = db;
        _registration = registration;
        _realtorRegistration = realtorRegistration;
        _propertyAdministratorRegistration = propertyAdministratorRegistration;
        _passwordResetEmail = passwordResetEmail;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Email) && !ValidEmailAttribute.IsValidAddress(model.Email, out var emailError))
        {
            ModelState.AddModelError(nameof(model.Email), emailError ?? "Enter a valid email address.");
        }

        if (!UsPhoneOptionalAttribute.IsValidOptional(model.Telefono))
        {
            ModelState.AddModelError(nameof(model.Telefono),
                "Enter a valid 10-digit US phone number (e.g. 555 123 4567).");
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
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: true);
                return RedirectToAction(nameof(SelectRole), new { userId = user.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewBag.OnboardingStep = 1;
        ViewBag.OnboardingTitle = "Create Account";
        ViewBag.OnboardingBackUrl = Url.Action(nameof(Welcome));
        ViewBag.OnboardingShowBack = true;
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewBag.OnboardingStep = 1;
        ViewBag.OnboardingTitle = "Create Account";
        ViewBag.OnboardingBackUrl = Url.Action(nameof(Welcome));
        ViewBag.OnboardingShowBack = true;
        return View();
    }

    private static void SplitFullName(RegisterViewModel model)
    {
        var parts = model.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        model.Nombre = parts.Length > 0 ? parts[0] : model.FullName.Trim();
        model.Apellidos = parts.Length > 1 ? parts[1] : string.Empty;
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Welcome()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return await RedirectAuthenticatedUserAsync();
        }

        return View();
    }

    [HttpGet]
    public IActionResult Terms(string? from = null, int? propiedadId = null)
    {
        ViewBag.OnboardingTitle = "Terms & Conditions";
        ViewBag.OnboardingBackUrl = ResolveLegalBackUrl(from, propiedadId);
        ViewBag.OnboardingShowBack = true;
        ViewBag.LegalFrom = from;
        ViewBag.LegalPropiedadId = propiedadId;
        return View();
    }

    [HttpGet]
    public IActionResult Privacy(string? from = null, int? propiedadId = null)
    {
        ViewBag.OnboardingTitle = "Privacy Policy";
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

        return Url.Action(nameof(Welcome));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return await RedirectAuthenticatedUserAsync(returnUrl);
        }

        return RedirectToAction(nameof(Welcome));
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> LoginForm(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return await RedirectAuthenticatedUserAsync(returnUrl);
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
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // PasswordSignInAsync uses UserName (email at registration), not Email lookup.
                // FindByEmailAsync throws if duplicate emails exist in AspNetUsers.
                return await RedirectAuthenticatedUserAsync(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View("LoginForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Welcome));
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult ForgotPassword()
    {
        ViewBag.OnboardingTitle = "Reset password";
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
            ViewBag.OnboardingTitle = "Reset password";
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

        return RedirectToAction(nameof(ForgotPasswordConfirmation), new { email });
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation(string? email = null)
    {
        ViewBag.OnboardingTitle = "Check your email";
        ViewBag.OnboardingBackUrl = Url.Action(nameof(LoginForm));
        ViewBag.OnboardingShowBack = true;
        ViewBag.Email = email;
        return View();
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult ResetPassword(string? email = null, string? code = null)
    {
        ViewBag.OnboardingTitle = "Set a new password";
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
        ViewBag.OnboardingTitle = "Set a new password";
        ViewBag.OnboardingBackUrl = Url.Action(nameof(LoginForm));
        ViewBag.OnboardingShowBack = true;

        if (!ModelState.IsValid)
        {
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
                "This code is invalid or has expired. Please request a new one.");
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(record.UserId)
                   ?? await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "We couldn't find an account for this email.");
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
        ViewBag.OnboardingTitle = "Password updated";
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
    public async Task<IActionResult> SelectRole(string userId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || currentUser.Id != userId)
        {
            return Challenge();
        }

        if (!string.IsNullOrEmpty(currentUser.RolUsuario))
        {
            return await RedirectAuthenticatedUserAsync(user: currentUser);
        }

        ViewBag.OnboardingStep = 2;
        ViewBag.OnboardingTitle = "Create your profile";
        ViewBag.OnboardingBackUrl = Url.Action(nameof(Welcome));
        ViewBag.OnboardingShowBack = true;

        return View(new SelectRoleViewModel { UserId = userId, SelectedRole = "Propietario" });
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
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null || user.Id != _userManager.GetUserId(User))
            {
                return Challenge();
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
        ViewBag.OnboardingTitle = "Create your profile";
        ViewBag.OnboardingBackUrl = Url.Action(nameof(Welcome));
        ViewBag.OnboardingShowBack = true;
        return View(model);
    }

    private async Task<IActionResult> RedirectAuthenticatedUserAsync(string? returnUrl = null, ApplicationUser? user = null)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        user ??= await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(LoginForm));
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

    private async Task<IActionResult> RedirectProveedorAsync(ApplicationUser user)
    {
        var proveedor = await _db.IndorProveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (proveedor == null)
        {
            return RedirectToAction("Entry", "ProviderRegistration");
        }

        if (proveedor.RegistrationStatus == ProviderRegistrationStatuses.Draft)
        {
            var action = _registration.ResolveWizardResumeAction(Math.Max(1, proveedor.CurrentStep));
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
            var action = _realtorRegistration.ResolveWizardResumeAction(Math.Max(1, realtor.CurrentStep));
            return action == "Dashboard"
                ? RedirectToAction("Profile", "RealtorRegistration")
                : RedirectToAction(action, "RealtorRegistration");
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
            var action = _propertyAdministratorRegistration.ResolveWizardResumeAction(Math.Max(1, admin.CurrentStep));
            return RedirectToAction(action, "PropertyAdministratorRegistration");
        }

        return RedirectToAction("Index", "Administrador");
    }
}
