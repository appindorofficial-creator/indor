using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndorMvcApp.Controllers;

[Authorize]
public class RealtorController(
    IRealtorRegistrationService registration,
    RealtorPortalService portalService,
    RealtorNearbyNetworkService nearbyNetworkService,
    RealtorSharedQuoteService sharedQuoteService,
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
    public async Task<IActionResult> QuoteDetail(int id, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await portalService.BuildQuoteDetailAsync(realtor, id, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        if (model.ProviderQuotesReceived > 0 || model.QuoteStatus == "Accepted")
        {
            return Redirect(portalService.ResolveQuoteFlowUrl(new IndorRealtorQuote
            {
                Id = model.QuoteId,
                Status = model.QuoteStatus,
                ProviderQuotesReceived = model.ProviderQuotesReceived
            }) ?? $"/Realtor/ViewQuote/{id}");
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ViewQuote(int id, int? bidId, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await portalService.BuildViewQuoteAsync(realtor, id, bidId, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(CompareQuotes), new { id });
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CompareQuotes(int id, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await portalService.BuildCompareQuotesPageAsync(realtor, id, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(ViewQuote), new { id });
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> QuoteSelected(int id, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await portalService.BuildQuoteSelectedAsync(realtor, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptQuote(int quoteId, int bidId, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var accepted = await portalService.AcceptQuoteAsync(realtor, quoteId, bidId, cancellationToken);
        return accepted
            ? RedirectToAction(nameof(QuoteSelected), new { id = quoteId })
            : RedirectToAction(nameof(Quotes));
    }

    [HttpGet]
    public async Task<IActionResult> EditSharedQuote(int quoteId, int bidId, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await sharedQuoteService.BuildEditAsync(realtor, quoteId, bidId, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSharedQuote(RealtorEditSharedQuoteViewModel model, string? action, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var sharedQuoteId = await sharedQuoteService.SaveEditAsync(realtor, model, cancellationToken);
        if (sharedQuoteId is not > 0)
        {
            return NotFound();
        }

        return string.Equals(action, "draft", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction(nameof(EditSharedQuote), new { quoteId = model.QuoteId, bidId = model.BidId })
            : RedirectToAction(nameof(PreviewSharedQuote), new { id = sharedQuoteId });
    }

    [HttpGet]
    public async Task<IActionResult> PreviewSharedQuote(int id, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await sharedQuoteService.BuildPreviewAsync(realtor, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendSharedQuote(int id, string deliveryMethod, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var sent = await sharedQuoteService.SendAsync(realtor, id, deliveryMethod, cancellationToken);
        return sent
            ? RedirectToAction(nameof(SharedQuote), new { id })
            : RedirectToAction(nameof(Quotes));
    }

    [HttpGet]
    public async Task<IActionResult> SharedQuote(int id, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await sharedQuoteService.BuildTrackingAsync(realtor, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Network(string? view, string? filter, string? q, string? scope, CancellationToken cancellationToken) =>
        await PortalPageAsync(r => nearbyNetworkService.BuildAsync(r, view, filter, q, scope, cancellationToken), cancellationToken);

    [HttpGet]
    public async Task<IActionResult> CreateNetworkListing(CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await nearbyNetworkService.BuildListingFormAsync(realtor, null, cancellationToken);
        return View("NetworkListingForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNetworkListing(RealtorNetworkListingFormViewModel model, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        if (!ModelState.IsValid)
        {
            var shell = await portalService.BuildShellAsync(realtor, cancellationToken);
            model.DisplayName = shell.DisplayName;
            model.FullDisplayName = shell.FullDisplayName;
            model.ProfilePhotoUrl = shell.ProfilePhotoUrl;
            model.BadgeLabel = shell.BadgeLabel;
            model.IsVerified = shell.IsVerified;
            model.HasNotifications = shell.HasNotifications;
            return View("NetworkListingForm", model);
        }

        await nearbyNetworkService.SaveListingAsync(realtor, model, cancellationToken);
        return RedirectToAction(nameof(Network), new { filter = "Homes", scope = "mine" });
    }

    [HttpGet]
    public async Task<IActionResult> EditNetworkListing(int id, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await nearbyNetworkService.BuildListingFormAsync(realtor, id, cancellationToken);
        return model == null ? NotFound() : View("NetworkListingForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditNetworkListing(RealtorNetworkListingFormViewModel model, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        if (!ModelState.IsValid || model.ItemId is not > 0)
        {
            var shell = await portalService.BuildShellAsync(realtor, cancellationToken);
            model.DisplayName = shell.DisplayName;
            model.FullDisplayName = shell.FullDisplayName;
            model.ProfilePhotoUrl = shell.ProfilePhotoUrl;
            model.BadgeLabel = shell.BadgeLabel;
            model.IsVerified = shell.IsVerified;
            model.HasNotifications = shell.HasNotifications;
            return View("NetworkListingForm", model);
        }

        var savedId = await nearbyNetworkService.SaveListingAsync(realtor, model, cancellationToken);
        return savedId == null
            ? NotFound()
            : RedirectToAction(nameof(Network), new { filter = "Homes", scope = "mine" });
    }

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildProfileAsync(r, cancellationToken), cancellationToken);

    [HttpGet]
    public async Task<IActionResult> ProviderNetwork(string? q, string? filter, CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildNetworkAsync(r, q, filter, cancellationToken), cancellationToken);

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
