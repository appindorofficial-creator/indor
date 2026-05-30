using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _db;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            SplitFullName(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre,
                Apellidos = model.Apellidos,
                Telefono = model.Telefono ?? string.Empty,
                PhoneNumber = model.Telefono,
                RolUsuario = "Propietario"
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("AddProperty", "Propietario");
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
                var user = await _userManager.FindByEmailAsync(model.Email);
                return await RedirectAuthenticatedUserAsync(returnUrl, user);
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
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new SelectRoleViewModel
        {
            UserId = userId
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> SelectRole(SelectRoleViewModel model)
    {
        if (ModelState.IsValid && !string.IsNullOrEmpty(model.SelectedRole))
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user != null)
            {
                user.RolUsuario = model.SelectedRole;
                await _userManager.UpdateAsync(user);

                // Tras seleccionar el rol, llevar siempre a la captura de dirección de propiedad
                return RedirectToAction("AddProperty", "Propietario");
            }
        }

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

        if (await _db.Propiedades.AnyAsync(p => p.UserId == user.Id && p.Activo))
        {
            return RedirectToAction("Index", "Home");
        }

        if (!string.IsNullOrEmpty(user.RolUsuario))
        {
            return user.RolUsuario switch
            {
                "Propietario" => RedirectToAction("AddProperty", "Propietario"),
                "Realtor" => RedirectToAction("Dashboard", "Realtor"),
                "AdministradorPropiedades" => RedirectToAction("Dashboard", "Administrador"),
                "ProveedorServicios" => RedirectToAction("Dashboard", "Proveedor"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        return RedirectToAction(nameof(SelectRole), new { userId = user.Id });
    }
}
