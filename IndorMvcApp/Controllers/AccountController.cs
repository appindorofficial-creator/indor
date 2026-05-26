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

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre,
                Apellidos = model.Apellidos,
                Telefono = model.Telefono,
                PhoneNumber = model.Telefono
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("SelectRole", new { userId = user.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Welcome()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> LoginForm(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && await _db.Propiedades.AnyAsync(p => p.UserId == user.Id && p.Activo))
            {
                return RedirectToAction("Index", "Home");
            }
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

                if (user != null)
                {
                    var tienePropiedades = await _db.Propiedades
                        .AnyAsync(p => p.UserId == user.Id && p.Activo);

                    if (tienePropiedades)
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
                            _ => RedirectToLocal(returnUrl)
                        };
                    }
                }

                return RedirectToLocal(returnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
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

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
