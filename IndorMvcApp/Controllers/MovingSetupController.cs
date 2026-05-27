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

    public MovingSetupController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var config = await _db.MovingSetupConfig.FirstOrDefaultAsync(c => c.Activo);
        var servicios = await _db.MovingSetupServicios.Where(s => s.Activo).OrderBy(s => s.Orden).ToListAsync();
        var enlaces = await _db.MovingSetupEnlacesRapidos.Where(e => e.Activo).OrderBy(e => e.Orden).ToListAsync();

        var model = MovingSetupDisplayService.Build(config, servicios, enlaces, Url);
        if (model == null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(model);
    }
}
