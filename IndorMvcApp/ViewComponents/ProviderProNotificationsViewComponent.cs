using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.ViewComponents;

public class ProviderProNotificationsViewComponent(
    IProviderRegistrationService registration,
    IProviderProDataService proData) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string tone = "default")
    {
        var proveedor = await registration.GetProveedorForCurrentUserAsync(HttpContext.RequestAborted);
        if (proveedor == null)
        {
            return Content(string.Empty);
        }

        var model = await proData.GetTopbarAsync(proveedor, HttpContext.RequestAborted);
        ViewData["NotifyTone"] = tone;
        return View(model);
    }
}
