using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Controllers;

[Authorize]
[ResponseCache(NoStore = true, Duration = 0, Location = ResponseCacheLocation.None)]
public class NeighborhoodController(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    NeighborhoodFeedService feedService,
    IWebHostEnvironment webHostEnvironment) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? tab, string? filter, string? q, string? zip, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("LoginForm", "Account");
        }

        var (propiedad, info) = await ResolvePrimaryPropertyAsync(userId, cancellationToken);
        var model = await feedService.BuildFeedAsync(
            userId, propiedad, info, 0, tab, filter, q, zip, Url, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("LoginForm", "Account");
        }

        var model = await feedService.BuildDetailAsync(userId, id, Url, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("LoginForm", "Account");
        }

        var (propiedad, info) = await ResolvePrimaryPropertyAsync(userId, cancellationToken);
        if (propiedad == null || string.IsNullOrWhiteSpace(NeighborhoodFeedService.NormalizeZip(info?.PostalCode)))
        {
            return RedirectToAction(nameof(Index));
        }

        return View(feedService.BuildCreate(propiedad, info, Url));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NeighborhoodCreatePostViewModel model, List<IFormFile>? media, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        var user = await userManager.GetUserAsync(User);
        if (userId == null || user == null)
        {
            return RedirectToAction("LoginForm", "Account");
        }

        var (propiedad, info) = await ResolvePrimaryPropertyAsync(userId, cancellationToken);
        var zip = NeighborhoodFeedService.NormalizeZip(info?.PostalCode);
        if (propiedad == null || string.IsNullOrWhiteSpace(zip))
        {
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(model.Body))
        {
            ModelState.AddModelError(nameof(model.Body), "Required");
            return View(feedService.BuildCreate(propiedad, info, Url));
        }

        var savedMedia = await SaveMediaAsync(media, cancellationToken);
        var postId = await feedService.CreatePostAsync(userId, user, propiedad, zip, model, savedMedia, cancellationToken);
        if (postId == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Published), new { id = postId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Published(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("LoginForm", "Account");
        }

        var detail = await feedService.BuildDetailAsync(userId, id, Url, cancellationToken);
        if (detail == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(feedService.BuildPublished(id, Url));
    }

    [HttpGet]
    public async Task<IActionResult> Saved(string? tab, string? category, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("LoginForm", "Account");
        }

        var model = await feedService.BuildSavedAsync(userId, tab, category, Url, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comment(int id, string body, int? parentId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        var user = await userManager.GetUserAsync(User);
        if (userId == null || user == null)
        {
            return RedirectToAction("LoginForm", "Account");
        }

        var (_, info) = await ResolvePrimaryPropertyAsync(userId, cancellationToken);
        await feedService.AddCommentAsync(userId, user, id, body ?? string.Empty, parentId, info?.PostalCode, cancellationToken);
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var deleted = await feedService.DeletePostAsync(userId, id, cancellationToken);
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Json(new { ok = deleted });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Report(int id)
    {
        // Reports are acknowledged; moderation handling is out of scope here.
        return Json(new { ok = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var saved = await feedService.ToggleSaveAsync(userId, id, cancellationToken);
        return Json(new { ok = true, saved });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveComment(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var saved = await feedService.ToggleCommentSaveAsync(userId, id, cancellationToken);
        return Json(new { ok = true, saved });
    }

    private async Task<(Propiedad? propiedad, PropertyInfoViewModel? info)> ResolvePrimaryPropertyAsync(
        string userId, CancellationToken cancellationToken)
    {
        var propiedad = await db.Propiedades
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync(cancellationToken);

        var info = propiedad != null ? MyHomeDisplayService.DeserializeProperty(propiedad) : null;
        return (propiedad, info);
    }

    private static readonly HashSet<string> VideoExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".mov", ".webm", ".m4v", ".ogg" };

    private async Task<List<(string Path, bool IsVideo)>> SaveMediaAsync(
        List<IFormFile>? files, CancellationToken cancellationToken)
    {
        var result = new List<(string, bool)>();
        if (files == null || files.Count == 0)
        {
            return result;
        }

        var uploadsDir = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "neighborhood");
        Directory.CreateDirectory(uploadsDir);

        // Cap at 6 attachments per post.
        foreach (var file in files.Where(f => f is { Length: > 0 }).Take(6))
        {
            var ext = Path.GetExtension(file.FileName);
            var isVideo = VideoExtensions.Contains(ext) ||
                          (file.ContentType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ?? false);
            if (string.IsNullOrWhiteSpace(ext) || ext.Length > 6)
            {
                ext = isVideo ? ".mp4" : ".jpg";
            }

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            result.Add(($"/uploads/neighborhood/{fileName}", isVideo));
        }

        return result;
    }
}
