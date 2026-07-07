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

    // ------------------------------------------------------ Screen 4: Post a Job

    [HttpGet]
    public async Task<IActionResult> PostNetworkJob(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await network.GetPostJobAsync(proveedor.Entity!, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> PostNetworkJob(PostNetworkJobInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (string.IsNullOrWhiteSpace(input.TradeId) || string.IsNullOrWhiteSpace(input.Description))
        {
            var model = await network.GetPostJobAsync(proveedor.Entity!, cancellationToken);
            model.SelectedTradeId = input.TradeId;
            model.Description = input.Description;
            model.Location = input.Location;
            model.DateNeeded = input.DateNeeded;
            model.BudgetRange = input.BudgetRange;
            model.ErrorMessage = "Please choose a trade and describe the project.";
            return View(model);
        }

        string? photoUrl = null;
        if (input.Photo is { Length: > 0 })
        {
            photoUrl = await SaveNetworkJobPhotoAsync(proveedor.Entity!.Id, input.Photo);
        }

        var jobId = await network.SavePostJobAsync(proveedor.Entity!.Id, input, photoUrl, cancellationToken);
        return RedirectToAction(nameof(NetworkJobPosted), new { id = jobId });
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
