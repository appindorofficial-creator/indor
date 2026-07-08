using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

/// <summary>
/// Verify Contractors — operator console to review contractor documents,
/// insurance and verification status, then approve. Reads real contractor
/// data and writes review decisions via <see cref="Services.IContractorVerificationService"/>.
/// </summary>
public partial class ProveedorController
{
    // ------------------------------------------------------ Screen 1: Queue

    [HttpGet]
    public async Task<IActionResult> VerifyContractors(string? tab, string? q, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await verification.GetQueueAsync(proveedor.Entity!, tab, q, cancellationToken);
        return View(model);
    }

    // ------------------------------------------------------ Screen 2: Detail

    [HttpGet]
    public async Task<IActionResult> ContractorVerification(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await verification.GetDetailAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveVerificationReview(int id, string? operatorNotes, string? mode, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var ok = await verification.SaveReviewAsync(proveedor.Entity!, id, operatorNotes, mode, cancellationToken);
        if (!ok)
        {
            return NotFound();
        }

        if (string.Equals(mode, "request", StringComparison.OrdinalIgnoreCase))
        {
            TempData["VerifyToast"] = "Info requested — contractor moved to Flagged.";
            return RedirectToAction(nameof(VerifyContractors), new { tab = "flagged" });
        }

        return RedirectToAction(nameof(VerificationComplete), new { id });
    }

    // ------------------------------------------------------ Screen 3: Complete

    [HttpGet]
    public async Task<IActionResult> VerificationComplete(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await verification.GetCompleteAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveVerification(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var ok = await verification.ApproveAsync(proveedor.Entity!, id, cancellationToken);
        if (!ok)
        {
            return NotFound();
        }

        TempData["VerifyToast"] = "Contractor approved and verified.";
        return RedirectToAction(nameof(VerifyContractors), new { tab = "approved" });
    }
}
