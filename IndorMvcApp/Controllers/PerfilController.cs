using System.Security.Claims;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Controllers;

[Authorize]
public class PerfilController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _env;

    public PerfilController(AppDbContext db,
                            UserManager<ApplicationUser> userManager,
                            SignInManager<ApplicationUser> signInManager,
                            IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
    }

    // ====== Vistas (GET) ======
    private async Task CargarUsuarioYMembresiaAsync()
    {
        var userId = _userManager.GetUserId(User);
        ViewBag.UsuarioActual = await _userManager.GetUserAsync(User);
        ViewBag.MembresiaActual = await _db.MembresiasUsuario
            .Include(m => m.Plan)
            .Where(m => m.UserId == userId && m.Activa)
            .OrderByDescending(m => m.FechaInicio)
            .FirstOrDefaultAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Opciones()
    {
        await CargarUsuarioYMembresiaAsync();
        return View();
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
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Suscripciones()
    {
        await CargarUsuarioYMembresiaAsync();
        ViewBag.Planes = await _db.PlanesMembresia
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync();
        return View();
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
        return RedirectToAction(nameof(Opciones));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> SubirFoto(IFormFile foto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (foto != null && foto.Length > 0)
        {
            var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
            var permitidos = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (Array.IndexOf(permitidos, ext) >= 0)
            {
                var carpeta = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(carpeta);
                var nombreArchivo = $"{user.Id}_{DateTime.Now.Ticks}{ext}";
                var ruta = Path.Combine(carpeta, nombreArchivo);
                using (var fs = new FileStream(ruta, FileMode.Create))
                {
                    await foto.CopyToAsync(fs);
                }
                user.FotoUrl = $"/uploads/avatars/{nombreArchivo}";
                await _userManager.UpdateAsync(user);
                TempData["PerfilOk"] = "Photo updated.";
            }
            else
            {
                TempData["PerfilError"] = "Image format not allowed.";
            }
        }
        return RedirectToAction(nameof(Opciones));
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
            await _signInManager.RefreshSignInAsync(user);
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

        if (predeterminado)
        {
            var existentes = await _db.MetodosPago.Where(m => m.UserId == userId && m.EsPredeterminado).ToListAsync();
            foreach (var m in existentes) m.EsPredeterminado = false;
        }

        _db.MetodosPago.Add(new MetodoPago
        {
            UserId = userId,
            Tipo = string.IsNullOrWhiteSpace(tipo) ? "Card" : tipo,
            Marca = marca,
            Ultimos4 = ultimos4,
            Titular = titular,
            Expiracion = expiracion,
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
        // Auto-respuesta del bot de soporte
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
    public async Task<IActionResult> ActivarPlan(int planId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

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
        TempData["PerfilOk"] = "Plan activated.";
        return RedirectToAction(nameof(Suscripciones));
    }
}
