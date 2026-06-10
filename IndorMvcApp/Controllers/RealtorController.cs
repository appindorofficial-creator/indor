using IndorMvcApp.Models;
using IndorMvcApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndorMvcApp.Controllers;

[Authorize]
public class RealtorController(
    IRealtorRegistrationService registration,
    RealtorPortalService portalService,
    UserManager<ApplicationUser> userManager) : Controller
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            context.Result = Challenge();
            return;
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null ||
            !string.Equals(user.RolUsuario, "Realtor", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = RedirectToAction("Index", "Home");
            return;
        }

        await next();
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildHomeAsync(r, cancellationToken), cancellationToken);

    [HttpGet]
    public async Task<IActionResult> Clients(string? q, string? filter, CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildClientsAsync(r, q, filter, cancellationToken), cancellationToken);

    [HttpGet]
    public async Task<IActionResult> Files(string? q, string? filter, CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildFilesAsync(r, q, filter, cancellationToken), cancellationToken);

    [HttpGet]
    public async Task<IActionResult> Quotes(string? q, string? filter, CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildQuotesAsync(r, q, filter, cancellationToken), cancellationToken);

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildProfileAsync(r, cancellationToken), cancellationToken);

    private async Task<IActionResult> PortalPageAsync<T>(
        Func<IndorRealtor, Task<T>> build,
        CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        if (realtor.RegistrationStatus == RealtorRegistrationStatuses.Draft)
        {
            var action = registration.ResolveWizardResumeAction(Math.Max(1, realtor.CurrentStep));
            if (action == "Dashboard")
            {
                return RedirectToAction("Profile", "RealtorRegistration");
            }

            return RedirectToAction(action, "RealtorRegistration");
        }

        var model = await build(realtor);
        return View(model);
    }
}
