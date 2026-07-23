using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

/// <summary>
/// Provider-facing marketplace of open homeowner service requests that match the
/// provider's trades. First-come: taking a request claims it atomically and it
/// disappears from every other provider's list.
/// </summary>
public partial class ProveedorController
{
    [HttpGet]
    public async Task<IActionResult> AvailableRequests(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await serviceRequests.GetAvailableForProviderAsync(proveedor.Entity!, localizer.IsSpanish, cancellationToken);
        ViewBag.NavActive = "jobs";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AvailableRequestDetails(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await serviceRequests.GetProviderRequestDetailAsync(proveedor.Entity!, id, localizer.IsSpanish, cancellationToken);
        if (model == null)
        {
            TempData["ServiceRequestProToast"] = localizer.T("That request is no longer available.");
            return RedirectToAction(nameof(AvailableRequests));
        }

        ViewBag.NavActive = "jobs";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TakeRequest(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var result = await serviceRequests.ClaimAsync(proveedor.Entity!, id, cancellationToken);
        switch (result)
        {
            case ClaimServiceRequestResult.Success:
                TempData["ServiceRequestProToast"] = localizer.T("You took this request. The homeowner has been notified with your details.");
                return RedirectToAction(nameof(AvailableRequestDetails), new { id });
            case ClaimServiceRequestResult.AlreadyTaken:
                TempData["ServiceRequestProToast"] = localizer.T("Another provider already took this request.");
                return RedirectToAction(nameof(AvailableRequests));
            case ClaimServiceRequestResult.NotEligible:
                TempData["ServiceRequestProToast"] = localizer.T("This request is outside your service categories.");
                return RedirectToAction(nameof(AvailableRequests));
            default:
                TempData["ServiceRequestProToast"] = localizer.T("That request is no longer available.");
                return RedirectToAction(nameof(AvailableRequests));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAppNotificationsRead(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId != null)
        {
            await notificationService.MarkAllReadAsync(userId, cancellationToken);
        }
        return Json(new { ok = true });
    }
}
