using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.ViewComponents;

public class ProviderProTopbarViewComponent(
    IProviderRegistrationService registration,
    IProviderProDataService proData) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var proveedor = await registration.GetProveedorForCurrentUserAsync(HttpContext.RequestAborted);
        if (proveedor == null)
        {
            return View(new ProviderProTopbarViewModel());
        }

        var model = await proData.GetTopbarAsync(proveedor, HttpContext.RequestAborted);
        return View(model);
    }
}
