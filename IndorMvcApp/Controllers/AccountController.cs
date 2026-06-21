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

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext db,
        IProviderRegistrationService registration,
        IRealtorRegistrationService realtorRegistration,
        IPropertyAdministratorRegistrationService propertyAdministratorRegistration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _db = db;
        _registration = registration;
        _realtorRegistration = realtorRegistration;
        _propertyAdministratorRegistration = propertyAdministratorRegistration;
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
    public IActionResult Terms(string? from = null)
    {
        ViewBag.OnboardingTitle = "Terms & Conditions";
        ViewBag.OnboardingBackUrl = from == "register"
            ? Url.Action(nameof(Register))
            : Url.Action(nameof(Welcome));
        ViewBag.OnboardingShowBack = true;
        return View();
    }

    [HttpGet]
    public IActionResult Privacy(string? from = null)
    {
        ViewBag.OnboardingTitle = "Privacy Policy";
        ViewBag.OnboardingBackUrl = from == "register"
            ? Url.Action(nameof(Register))
            : Url.Action(nameof(Welcome));
        ViewBag.OnboardingShowBack = true;
        return View();
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
