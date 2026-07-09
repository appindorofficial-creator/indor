using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

/// <summary>
/// Contractor Network — lets a verified provider find, review, post jobs to,
/// and hire other verified subcontractors from the INDOR network. All data is
/// read from and written to the database via <see cref="Services.IProviderNetworkService"/>.
/// </summary>
public partial class ProveedorController
{
    private static readonly string[] NetworkPhotoExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxNetworkPhotoBytes = 10_000_000;

    // ------------------------------------------------------ Screen 1: Home

    [HttpGet]
    public async Task<IActionResult> Network(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await network.GetHomeAsync(proveedor.Entity!, cancellationToken);
        return View(model);
    }

    // ------------------------------------------------------ Screen 2: Find

    [HttpGet]
    public async Task<IActionResult> FindSubcontractors(
        string? q,
        string? trade,
        string? view,
        bool nearby,
        bool insured,
        bool available,
        bool docs,
        CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await network.GetFindAsync(
            proveedor.Entity!, q, trade, view, nearby, insured, available, docs, cancellationToken);
        return View(model);
    }

    // ------------------------------------------------------ Screen 3: Profile

    [HttpGet]
    public async Task<IActionResult> SubcontractorProfile(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await network.GetProfileAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSaveSubcontractor(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        await network.ToggleSaveAsync(proveedor.Entity!.Id, id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(SubcontractorProfile), new { id });
    }

    // ------------------------------------------------------ Screen 4: Post a Job (3-step wizard)

    // Step 1 — Details
    [HttpGet]
    public async Task<IActionResult> PostNetworkJob(int? id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await network.GetDetailsAsync(proveedor.Entity!, id, cancellationToken);
        return View("PostNetworkJob", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> PostNetworkJobDetails(PostJobDetailsInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var newPhotos = new List<string>();
        if (input.Photos is { Count: > 0 })
        {
            foreach (var file in input.Photos.Where(f => f is { Length: > 0 }).Take(6))
            {
                var url = await SaveNetworkJobPhotoAsync(proveedor.Entity!.Id, file);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    newPhotos.Add(url);
                }
            }
        }

        if (string.IsNullOrWhiteSpace(input.TradeId) || string.IsNullOrWhiteSpace(input.JobTitle))
        {
            var model = await network.GetDetailsAsync(proveedor.Entity!, input.DraftId, cancellationToken);
            model.SelectedTradeId = input.TradeId;
            model.JobTitle = input.JobTitle;
            model.Description = input.Description;
            model.Urgency = input.Urgency;
            model.Photos.Clear();
            model.Photos.AddRange((input.ExistingPhotos ?? []).Where(u => !string.IsNullOrWhiteSpace(u)));
            model.Photos.AddRange(newPhotos);
            model.ErrorMessage = "Please choose a trade and add a job title.";
            return View("PostNetworkJob", model);
        }

        var draftId = await network.SaveDetailsAsync(proveedor.Entity!.Id, input, newPhotos, cancellationToken);
        return RedirectToAction(nameof(PostNetworkJobLocation), new { id = draftId });
    }

    // Step 2 — Location & Budget
    [HttpGet]
    public async Task<IActionResult> PostNetworkJobLocation(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await network.GetLocationAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostNetworkJobLocation(PostJobLocationInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var ok = await network.SaveLocationAsync(proveedor.Entity!, input, cancellationToken);
        if (!ok)
        {
            return NotFound();
        }

        if (string.Equals(input.Mode, "draft", StringComparison.OrdinalIgnoreCase))
        {
            TempData["NetworkToast"] = localizer["Draft saved. You can finish posting it anytime."];
            return RedirectToAction(nameof(Network));
        }

        return RedirectToAction(nameof(PostNetworkJobReview), new { id = input.DraftId });
    }

    // Step 3 — Review
    [HttpGet]
    public async Task<IActionResult> PostNetworkJobReview(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await network.GetReviewAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishNetworkJob(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var jobId = await network.PublishJobAsync(proveedor.Entity!, id, cancellationToken);
        if (jobId == null)
        {
            return RedirectToAction(nameof(PostNetworkJob), new { id });
        }

        return RedirectToAction(nameof(NetworkJobPosted), new { id = jobId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> NetworkJobPosted(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await network.GetJobPostedAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    // ------------------------------------------------------ Screen 5: Hire

    [HttpGet]
    public async Task<IActionResult> HireSubcontractor(int id, int? jobId, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await network.GetHireAsync(proveedor.Entity!, id, jobId, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmHire(ConfirmHireInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var hireId = await network.ConfirmHireAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (hireId == null)
        {
            return RedirectToAction(nameof(FindSubcontractors));
        }

        return RedirectToAction(nameof(NetworkHireConfirmed), new { id = hireId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> NetworkHireConfirmed(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await network.GetHireConfirmedAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    // ------------------------------------------------------ Helpers

    private async Task<string?> SaveNetworkJobPhotoAsync(int proveedorId, IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            ext = ".jpg";
        }

        if (!NetworkPhotoExtensions.Contains(ext) || file.Length > MaxNetworkPhotoBytes)
        {
            return null;
        }

        var folder = Path.Combine(env.WebRootPath, "uploads", "network-jobs", proveedorId.ToString());
        Directory.CreateDirectory(folder);

        var stored = $"{Guid.NewGuid():N}{ext}";
        var physical = Path.Combine(folder, stored);
        await using (var stream = System.IO.File.Create(physical))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/network-jobs/{proveedorId}/{stored}";
    }
}
