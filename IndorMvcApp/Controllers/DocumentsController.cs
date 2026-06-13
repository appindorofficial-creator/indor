using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class DocumentsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public DocumentsController(AppDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id, string? tab)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        var dbDocs = await LoadDocumentsAsync(id);
        ConfigureView(id);
        return View(DocumentsDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab, dbDocs));
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, string documentId)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        var dbDocs = await LoadDocumentsAsync(id);
        var model = DocumentsDisplayService.BuildDetail(bundle.Value.Propiedad, bundle.Value.Info, documentId, dbDocs);
        if (model == null) return NotFound();
        ConfigureView(id);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Add(int id, string? category)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        ConfigureView(id);
        return View(DocumentsDisplayService.BuildAdd(bundle.Value.Propiedad, category));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(DocumentAddViewModel model, IFormFile? file)
    {
        var bundle = await LoadBundleAsync(model.PropiedadId);
        if (bundle == null) return NotFound();

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            ModelState.AddModelError(nameof(model.Title), "Document title is required.");
        }

        if (!ModelState.IsValid)
        {
            model.CategoryOptions = DocumentsDisplayService.AddCategories.ToList();
            model.SectionOptions = DocumentsDisplayService.RelatedSections.ToList();
            ConfigureView(model.PropiedadId);
            return View(model);
        }

        string? storagePath = null;
        string? fileName = null;
        long? sizeBytes = null;
        string? contentType = null;

        if (file != null && file.Length > 0)
        {
            var userId = _userManager.GetUserId(User)!;
            var folder = Path.Combine(_env.WebRootPath, "uploads", "my-home", userId, model.PropiedadId.ToString());
            Directory.CreateDirectory(folder);
            fileName = Path.GetFileName(file.FileName);
            var stored = $"{Guid.NewGuid():N}_{fileName}";
            var physical = Path.Combine(folder, stored);
            await using (var stream = System.IO.File.Create(physical))
            {
                await file.CopyToAsync(stream);
            }

            storagePath = $"/uploads/my-home/{userId}/{model.PropiedadId}/{stored}";
            sizeBytes = file.Length;
            contentType = file.ContentType;
        }

        _db.PropiedadDocumentos.Add(new PropiedadDocumento
        {
            PropiedadId = model.PropiedadId,
            Category = MapCategoryToDb(model.Category),
            Title = model.Title.Trim(),
            FileName = fileName,
            StoragePath = storagePath,
            ContentType = contentType,
            SizeBytes = sizeBytes
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Could not save the document. Please verify database tables are created.");
            model.CategoryOptions = DocumentsDisplayService.AddCategories.ToList();
            model.SectionOptions = DocumentsDisplayService.RelatedSections.ToList();
            ConfigureView(model.PropiedadId);
            return View(model);
        }

        return RedirectToAction(nameof(Index), new { id = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> Requests(int id)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        ConfigureView(id);
        return View(DocumentsDisplayService.BuildRequests(bundle.Value.Propiedad, bundle.Value.Info));
    }

    private void ConfigureView(int id)
    {
        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "documents";
        HouseFactPreviewContext.ApplyReturnUrlToView(this);
    }

    private async Task<(Propiedad Propiedad, PropertyInfoViewModel? Info)?> LoadBundleAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        var propiedad = await _db.Propiedades
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && p.Activo);

        if (propiedad == null) return null;

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return (propiedad, info);
    }

    private async Task<List<PropiedadDocumento>> LoadDocumentsAsync(int propiedadId)
    {
        try
        {
            return await _db.PropiedadDocumentos
                .Where(d => d.PropiedadId == propiedadId)
                .OrderByDescending(d => d.FechaCreacion)
                .ToListAsync();
        }
        catch
        {
            return [];
        }
    }

    private static string MapCategoryToDb(string category) => category switch
    {
        "Report" => "Inspections",
        "Permit" => "Permits",
        "Disclosure" => "Contracts",
        "Warranty" => "Warranties",
        "Invoice" => "Invoices",
        "Photo" => "Photo",
        _ => "Other"
    };
}
