using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Services;

namespace IndorMvcApp.Controllers;

[Authorize]
public class MovingSetupController : Controller
{
    private readonly AppDbContext _db;
    private readonly IIndorLocalizer _localizer;

    public MovingSetupController(AppDbContext db, IIndorLocalizer localizer)
    {
        _db = db;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var config = await _db.MovingSetupConfig.FirstOrDefaultAsync(c => c.Activo);
        var servicios = await _db.MovingSetupServicios.Where(s => s.Activo).OrderBy(s => s.Orden).ToListAsync();
        var enlaces = await _db.MovingSetupEnlacesRapidos.Where(e => e.Activo).OrderBy(e => e.Orden).ToListAsync();

        var model = MovingSetupDisplayService.Build(config, servicios, enlaces, Url, _localizer.IsSpanish);
        if (model == null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(model);
    }
}
