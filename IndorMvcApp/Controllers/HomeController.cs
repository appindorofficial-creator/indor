using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Models;

namespace IndorMvcApp.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var usuario = await _userManager.GetUserAsync(User);
        ViewBag.UsuarioActual = usuario;

        var propiedades = await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();

        var microservicios = await _db.Microservicios
            .Where(m => m.Activo)
            .OrderBy(m => m.Id)
            .ToListAsync();
        ViewBag.Microservicios = microservicios;

        var servicios = await _db.Servicios
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Id)
            .ToListAsync();
        ViewBag.Servicios = servicios;

        var inspecciones = await _db.Inspecciones
            .Where(i => i.Activo)
            .OrderBy(i => i.Orden)
            .ThenBy(i => i.Id)
            .ToListAsync();
        ViewBag.Inspecciones = inspecciones;

        // === Datos para sección "More" ===
        ViewBag.Planes = await _db.PlanesMembresia
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync();

        ViewBag.MembresiaActual = await _db.MembresiasUsuario
            .Include(m => m.Plan)
            .Where(m => m.UserId == userId && m.Activa)
            .OrderByDescending(m => m.FechaInicio)
            .FirstOrDefaultAsync();

        ViewBag.MetodosPago = await _db.MetodosPago
            .Where(m => m.UserId == userId && m.Activo)
            .OrderByDescending(m => m.EsPredeterminado)
            .ThenByDescending(m => m.FechaCreacion)
            .ToListAsync();

        ViewBag.Pagos = await _db.Pagos
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();

        ViewBag.PlanesInternet = await _db.PlanesInternet
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync();

        ViewBag.Historial = await _db.HistorialServicios
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync();

        ViewBag.MensajesSoporte = await _db.MensajesSoporte
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Fecha)
            .ToListAsync();

        ViewBag.ProgramacionesMicroservicio = await _db.ProgramacionesMicroservicio
            .Include(p => p.Microservicio)
            .Include(p => p.Propiedad)
            .Where(p => p.UserId == userId && p.Estado == "Scheduled")
            .OrderBy(p => p.FechaProgramada)
            .ToListAsync();

        return View(propiedades);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
