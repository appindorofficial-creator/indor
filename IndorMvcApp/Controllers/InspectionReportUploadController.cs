using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class InspectionReportUploadController : Controller
{
    private const string SessionKey = "PendingInspectionUpload";

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png"
    };

    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<InspectionReportUploadController> _logger;

    public InspectionReportUploadController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env,
        ILogger<InspectionReportUploadController> logger)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Upload(int? propiedadId)
    {
        var propiedad = await LoadPropertyAsync(propiedadId);
        if (propiedad == null) return RedirectToAction("AddProperty", "Propietario");

        var pending = LoadPendingUpload();
        var info = MyHomeDisplayService.DeserializeProperty(propiedad);

        return View(new UploadInspectionReportViewModel
        {
            PropiedadId = propiedad.Id,
            Address = info?.FormattedAddress ?? propiedad.Direccion ?? "—",
            Title = pending?.Title ?? "Annual Home Inspection Report",
            InspectionDate = pending?.InspectionDate ?? DateTime.Today,
            Category = pending?.Category ?? "Inspections",
            Notes = pending?.Notes,
            SelectedFileName = pending?.OriginalFileName,
            SelectedFileSizeLabel = pending != null ? FormatFileSize(pending.SizeBytes) : null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadInspectionReportViewModel model, IFormFile? reportFile)
    {
        var propiedad = await LoadPropertyAsync(model.PropiedadId);
        if (propiedad == null) return NotFound();

        var pending = LoadPendingUpload();
        var info = MyHomeDisplayService.DeserializeProperty(propiedad);

        if (reportFile == null || reportFile.Length == 0)
        {
            if (pending == null || pending.PropiedadId != propiedad.Id)
            {
                ModelState.AddModelError(string.Empty, "Select a PDF, JPG, or PNG file to continue.");
            }
        }
        else if (!TryValidateFile(reportFile, out var fileError))
        {
            ModelState.AddModelError(string.Empty, fileError!);
        }

        if (!ModelState.IsValid)
        {
            model.Address = info?.FormattedAddress ?? propiedad.Direccion ?? "—";
            model.SelectedFileName = pending?.OriginalFileName;
            model.SelectedFileSizeLabel = pending != null ? FormatFileSize(pending.SizeBytes) : null;
            return View(model);
        }

        try
        {
            if (reportFile != null && reportFile.Length > 0)
            {
                ClearPendingUpload(deleteFile: true);
                var saved = await SaveTempFileAsync(reportFile);
                pending = new PendingInspectionUploadSession
                {
                    PropiedadId = propiedad.Id,
                    TempRelativePath = saved.RelativePath,
                    OriginalFileName = saved.OriginalFileName,
                    ContentType = saved.ContentType,
                    SizeBytes = saved.SizeBytes
                };
            }
            else
            {
                pending ??= new PendingInspectionUploadSession { PropiedadId = propiedad.Id };
            }

            pending!.PropiedadId = propiedad.Id;
            pending.Title = model.Title.Trim();
            pending.InspectionDate = model.InspectionDate?.Date;
            pending.Category = string.IsNullOrWhiteSpace(model.Category) ? "Inspections" : model.Category.Trim();
            pending.Notes = NullIfEmpty(model.Notes);
            SavePendingUpload(pending);

            return RedirectToAction(nameof(Review), new { id = propiedad.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stage inspection report upload");
            ModelState.AddModelError(string.Empty, "Could not process the file. Please try again.");
            model.Address = info?.FormattedAddress ?? propiedad.Direccion ?? "—";
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Review(int id)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var pending = LoadPendingUpload();
        if (pending == null || pending.PropiedadId != propiedad.Id || string.IsNullOrWhiteSpace(pending.TempRelativePath))
        {
            return RedirectToAction(nameof(Upload), new { propiedadId = id });
        }

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return View(new ReviewInspectionReportViewModel
        {
            PropiedadId = propiedad.Id,
            Address = info?.FormattedAddress ?? propiedad.Direccion ?? "—",
            Title = pending.Title,
            InspectionDate = pending.InspectionDate,
            Category = pending.Category,
            CategoryLabel = CategoryLabel(pending.Category),
            Notes = pending.Notes,
            OriginalFileName = pending.OriginalFileName,
            FileSizeLabel = FormatFileSize(pending.SizeBytes),
            FileTypeLabel = FileTypeLabel(pending.OriginalFileName, pending.ContentType)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, ReviewInspectionReportViewModel model)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var pending = LoadPendingUpload();
        if (pending == null || pending.PropiedadId != propiedad.Id)
        {
            return RedirectToAction(nameof(Upload), new { propiedadId = id });
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var physicalTemp = GetPhysicalPath(pending.TempRelativePath);
        if (!System.IO.File.Exists(physicalTemp))
        {
            ClearPendingUpload(deleteFile: false);
            TempData["UploadError"] = "The selected file expired. Please choose it again.";
            return RedirectToAction(nameof(Upload), new { propiedadId = id });
        }

        try
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads", "my-home", userId, propiedad.Id.ToString());
            Directory.CreateDirectory(folder);
            var stored = $"{Guid.NewGuid():N}_{Path.GetFileName(pending.OriginalFileName)}";
            var physicalFinal = Path.Combine(folder, stored);
            System.IO.File.Move(physicalTemp, physicalFinal, overwrite: true);

            var storagePath = $"/uploads/my-home/{userId}/{propiedad.Id}/{stored}";
            var document = new PropiedadDocumento
            {
                PropiedadId = propiedad.Id,
                Category = pending.Category,
                Title = pending.Title,
                FileName = pending.OriginalFileName,
                StoragePath = storagePath,
                ContentType = pending.ContentType,
                SizeBytes = pending.SizeBytes,
                InspectionDate = pending.InspectionDate,
                Notes = pending.Notes,
                FechaCreacion = DateTime.UtcNow
            };

            _db.PropiedadDocumentos.Add(document);
            _db.PropiedadHistorial.Add(new PropiedadHistorial
            {
                PropiedadId = propiedad.Id,
                RecordType = "Document",
                Title = "Inspection report uploaded",
                Description = pending.Title,
                CompletionDate = pending.InspectionDate ?? DateTime.UtcNow,
                Source = "User",
                FechaCreacion = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            ClearPendingUpload(deleteFile: false);

            TempData["UploadedDocumentId"] = document.Id.ToString();
            return RedirectToAction(nameof(Uploaded), new { id = propiedad.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save inspection report for propiedad {PropiedadId}", id);
            ModelState.AddModelError(string.Empty, "Could not upload the report. Please try again.");
            var info = MyHomeDisplayService.DeserializeProperty(propiedad);
            model.Address = info?.FormattedAddress ?? propiedad.Direccion ?? "—";
            model.OriginalFileName = pending.OriginalFileName;
            model.FileSizeLabel = FormatFileSize(pending.SizeBytes);
            model.FileTypeLabel = FileTypeLabel(pending.OriginalFileName, pending.ContentType);
            model.CategoryLabel = CategoryLabel(pending.Category);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Uploaded(int id)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        PropiedadDocumento? document = null;
        if (TempData["UploadedDocumentId"] is string docId && int.TryParse(docId, out var documentId))
        {
            document = await _db.PropiedadDocumentos
                .FirstOrDefaultAsync(d => d.Id == documentId && d.PropiedadId == propiedad.Id);
        }

        if (document == null)
        {
            document = await _db.PropiedadDocumentos
                .Where(d => d.PropiedadId == propiedad.Id
                            && (d.Category.Contains("Inspection") || d.Title.Contains("inspection", StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(d => d.FechaCreacion)
                .FirstOrDefaultAsync();
        }

        if (document == null)
        {
            return RedirectToAction(nameof(Upload), new { propiedadId = id });
        }

        var displayDate = document.InspectionDate ?? document.FechaCreacion.ToLocalTime().Date;
        return View(new InspectionReportUploadedViewModel
        {
            PropiedadId = propiedad.Id,
            Title = document.Title,
            UploadedDateLabel = displayDate.ToString("MMM d, yyyy", CultureInfo.GetCultureInfo("en-US")),
            FileTypeLabel = FileTypeLabel(document.FileName, document.ContentType)
        });
    }

    private async Task<Propiedad?> LoadPropertyAsync(int? id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        if (id.HasValue)
        {
            return await _db.Propiedades
                .FirstOrDefaultAsync(p => p.Id == id.Value && p.UserId == userId && p.Activo);
        }

        return await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private PendingInspectionUploadSession? LoadPendingUpload()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<PendingInspectionUploadSession>(json);
        }
        catch
        {
            return null;
        }
    }

    private void SavePendingUpload(PendingInspectionUploadSession pending)
    {
        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(pending));
    }

    private void ClearPendingUpload(bool deleteFile)
    {
        var pending = LoadPendingUpload();
        if (deleteFile && pending != null && !string.IsNullOrWhiteSpace(pending.TempRelativePath))
        {
            var physical = GetPhysicalPath(pending.TempRelativePath);
            if (System.IO.File.Exists(physical))
            {
                System.IO.File.Delete(physical);
            }
        }

        HttpContext.Session.Remove(SessionKey);
    }

    private async Task<(string RelativePath, string OriginalFileName, string ContentType, long SizeBytes)> SaveTempFileAsync(IFormFile file)
    {
        var userId = _userManager.GetUserId(User)!;
        var folder = Path.Combine(_env.WebRootPath, "uploads", "pending-inspection", userId);
        Directory.CreateDirectory(folder);

        var original = Path.GetFileName(file.FileName);
        var stored = $"{Guid.NewGuid():N}_{original}";
        var physical = Path.Combine(folder, stored);

        await using (var stream = System.IO.File.Create(physical))
        {
            await file.CopyToAsync(stream);
        }

        return ($"/uploads/pending-inspection/{userId}/{stored}", original, file.ContentType, file.Length);
    }

    private string GetPhysicalPath(string relativePath)
    {
        var trimmed = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_env.WebRootPath, trimmed);
    }

    private static bool TryValidateFile(IFormFile file, out string? error)
    {
        error = null;
        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
        {
            error = "Only PDF, JPG, and PNG files are supported.";
            return false;
        }

        if (file.Length > MaxFileSizeBytes)
        {
            error = "File size must be 25 MB or less.";
            return false;
        }

        return true;
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string FormatFileSize(long bytes) =>
        bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:0.#} KB",
            _ => $"{bytes / (1024.0 * 1024.0):0.#} MB"
        };

    private static string CategoryLabel(string category) =>
        category.Contains("Inspection", StringComparison.OrdinalIgnoreCase) ? "Inspection Report" : category;

    private static string FileTypeLabel(string? fileName, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) && contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            return "PDF";
        }

        var ext = Path.GetExtension(fileName ?? string.Empty).Trim('.').ToUpperInvariant();
        return ext switch
        {
            "PDF" => "PDF",
            "JPG" or "JPEG" => "JPG",
            "PNG" => "PNG",
            _ => string.IsNullOrWhiteSpace(ext) ? "FILE" : ext
        };
    }
}
