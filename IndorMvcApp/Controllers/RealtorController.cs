using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

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
    public async Task<IActionResult> NetworkMapData(
        double? lat,
        double? lng,
        string? q,
        string? filter,
        CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return Unauthorized();
        }

        var data = await nearbyNetworkService.GetMapDataAsync(realtor, lat, lng, q, filter, cancellationToken);
        return data == null
            ? BadRequest(new { error = "Address not found." })
            : Json(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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

        var model = await nearbyNetworkService.BuildListingWizardShellAsync(
            realtor,
            displayStep: 0,
            title: "Post Listing with INDOR",
            subtitle: null,
            showStepper: false,
            cancellationToken);
        return View("NetworkListing/Intro", model);
    }

    [HttpGet]
    public async Task<IActionResult> PostListingBenefits(CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await nearbyNetworkService.BuildListingWizardShellAsync(
            realtor,
            displayStep: 1,
            title: "Post Listing",
            subtitle: "Why post your listing on INDOR?",
            showStepper: true,
            cancellationToken);
        return View("NetworkListing/Benefits", model);
    }

    [HttpGet]
    public async Task<IActionResult> PostListingDetails(CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await nearbyNetworkService.BuildListingFormAsync(realtor, null, cancellationToken);
        return model == null
            ? NotFound()
            : View("NetworkListing/Details", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNetworkListing(
        RealtorNetworkListingFormViewModel model,
        IFormFile? photoPdfFile,
        CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        await ProcessListingPhotoUploadAsync(realtor, model, photoPdfFile, cancellationToken);

        if (!ModelState.IsValid)
        {
            await ApplyListingFormShellAsync(realtor, model, cancellationToken);
            return View("NetworkListing/Details", model);
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
        if (model == null)
        {
            return NotFound();
        }

        model.IsEdit = true;
        model.WizardStep = 2;
        return View("NetworkListing/Details", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditNetworkListing(
        RealtorNetworkListingFormViewModel model,
        IFormFile? photoPdfFile,
        CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        await ProcessListingPhotoUploadAsync(realtor, model, photoPdfFile, cancellationToken);

        if (!ModelState.IsValid || model.ItemId is not > 0)
        {
            await ApplyListingFormShellAsync(realtor, model, cancellationToken);
            model.IsEdit = true;
            return View("NetworkListing/Details", model);
        }

        var savedId = await nearbyNetworkService.SaveListingAsync(realtor, model, cancellationToken);
        return savedId == null
            ? NotFound()
            : RedirectToAction(nameof(Network), new { filter = "Homes", scope = "mine" });
    }

    private async Task ProcessListingPhotoUploadAsync(
        IndorRealtor realtor,
        RealtorNetworkListingFormViewModel model,
        IFormFile? photoPdfFile,
        CancellationToken cancellationToken)
    {
        if (photoPdfFile == null || photoPdfFile.Length == 0)
        {
            return;
        }

        var (url, error) = await nearbyNetworkService.SaveListingPhotoPdfAsync(
            realtor.Id,
            photoPdfFile,
            model.PhotoPdfUrl,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(error))
        {
            ModelState.AddModelError(string.Empty, error);
            return;
        }

        if (!string.IsNullOrWhiteSpace(url))
        {
            model.PhotoPdfUrl = url;
            model.PhotoPdfFileName = Path.GetFileName(photoPdfFile.FileName);
        }
    }

    private async Task ApplyListingFormShellAsync(
        IndorRealtor realtor,
        RealtorNetworkListingFormViewModel model,
        CancellationToken cancellationToken)
    {
        var shell = await portalService.BuildShellAsync(realtor, cancellationToken);
        model.DisplayName = shell.DisplayName;
        model.FullDisplayName = shell.FullDisplayName;
        model.ProfilePhotoUrl = shell.ProfilePhotoUrl;
        model.BadgeLabel = shell.BadgeLabel;
        model.IsVerified = shell.IsVerified;
        model.HasNotifications = shell.HasNotifications;
    }

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildProfileAsync(r, cancellationToken), cancellationToken);

    [HttpGet]
    public async Task<IActionResult> PublicProfile(CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildPublicProfileAsync(r, cancellationToken), cancellationToken);

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
