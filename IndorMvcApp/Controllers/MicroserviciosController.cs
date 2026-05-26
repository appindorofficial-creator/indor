using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class MicroserviciosController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MicroserviciosController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Schedule(int id)
    {
        var microservicio = await _db.Microservicios
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.Activo);
        if (microservicio == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var existing = await _db.ProgramacionesMicroservicio
            .AsNoTracking()
            .Where(p => p.UserId == userId
                        && p.MicroservicioId == id
                        && p.Estado == "Scheduled")
            .OrderByDescending(p => p.FechaActualizacion ?? p.FechaCreacion)
            .FirstOrDefaultAsync();

        var model = new ScheduleMicroservicioViewModel
        {
            MicroservicioId = microservicio.Id,
            NombreMicroservicio = microservicio.Nombre,
            SubtituloMicroservicio = microservicio.Subtitulo,
            FechaProgramada = existing?.FechaProgramada.Date ?? DateTime.Today.AddDays(7),
            Notas = existing?.Notas,
            HasExistingSchedule = existing != null
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Schedule(ScheduleMicroservicioViewModel model)
    {
        var microservicio = await _db.Microservicios
            .FirstOrDefaultAsync(m => m.Id == model.MicroservicioId && m.Activo);
        if (microservicio == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await PopulateScheduleViewModelAsync(model, microservicio, userId);
            return View(model);
        }

        if (model.FechaProgramada.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.FechaProgramada), "Please select today or a future date.");
            await PopulateScheduleViewModelAsync(model, microservicio, userId);
            return View(model);
        }

        var propiedadId = await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

        var existing = await _db.ProgramacionesMicroservicio
            .Where(p => p.UserId == userId
                        && p.MicroservicioId == model.MicroservicioId
                        && p.Estado == "Scheduled")
            .OrderByDescending(p => p.FechaActualizacion ?? p.FechaCreacion)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.FechaProgramada = model.FechaProgramada.Date;
            existing.Notas = model.Notas;
            existing.PropiedadId = propiedadId;
            existing.FechaActualizacion = DateTime.Now;
        }
        else
        {
            _db.ProgramacionesMicroservicio.Add(new ProgramacionMicroservicio
            {
                UserId = userId,
                MicroservicioId = model.MicroservicioId,
                PropiedadId = propiedadId,
                FechaProgramada = model.FechaProgramada.Date,
                Notas = model.Notas,
                Estado = "Scheduled",
                FechaCreacion = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();

        TempData["ScheduleSaved"] = $"{microservicio.Nombre} scheduled for {model.FechaProgramada:MM/dd/yyyy}.";
        return RedirectToAction("Index", "Home");
    }

    private async Task PopulateScheduleViewModelAsync(
        ScheduleMicroservicioViewModel model,
        Microservicio microservicio,
        string userId)
    {
        model.NombreMicroservicio = microservicio.Nombre;
        model.SubtituloMicroservicio = microservicio.Subtitulo;

        var hasExisting = await _db.ProgramacionesMicroservicio
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId
                           && p.MicroservicioId == model.MicroservicioId
                           && p.Estado == "Scheduled");

        model.HasExistingSchedule = hasExisting;
    }
}
