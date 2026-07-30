using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

/// <summary>
/// Homeowner in-app messaging to nearby INDOR providers (Cercanas → Servicios → Mensaje en INDOR).
/// </summary>
[Authorize]
[ResponseCache(NoStore = true, Duration = 0, Location = ResponseCacheLocation.None)]
public class HomeownerMessageController(
    HomeownerProviderMessageService messages,
    UserManager<ApplicationUser> userManager,
    IIndorLocalizer localizer) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Provider(int id, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = await messages.BuildComposeAsync(user, id, Url, cancellationToken);
        if (model == null)
        {
            TempData["HomeNearbyError"] = localizer.T("Provider not found.");
            return RedirectToAction("Index", "Home", new { filter = NearbyNetworkHomeownerFilters.Providers });
        }

        ViewData["Title"] = localizer.T("Message in INDOR");
        ViewData["BackUrl"] = model.BackUrl;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Provider(HomeownerMessageProviderInput input, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await messages.SendAsync(user, input, Url, cancellationToken);
        if (result.Sent != null)
        {
            return View("Sent", result.Sent);
        }

        if (result.Retry != null)
        {
            ViewData["Title"] = localizer.T("Message in INDOR");
            ViewData["BackUrl"] = result.Retry.BackUrl;
            return View(result.Retry);
        }

        TempData["HomeNearbyError"] = localizer.T("Provider not found.");
        return RedirectToAction("Index", "Home", new { filter = NearbyNetworkHomeownerFilters.Providers });
    }
}
