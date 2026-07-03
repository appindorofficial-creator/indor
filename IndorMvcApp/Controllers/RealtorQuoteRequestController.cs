using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndorMvcApp.Controllers;

[Authorize]
public class RealtorQuoteRequestController(
    IRealtorQuoteRequestService quoteRequest,
    IRealtorRegistrationService registration,
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

        var realtor = await registration.GetRealtorForCurrentUserAsync();
        if (realtor == null || realtor.RegistrationStatus == RealtorRegistrationStatuses.Draft)
        {
            context.Result = RedirectToAction("Profile", "RealtorRegistration");
            return;
        }

        await next();
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Property));

    [HttpGet]
    public async Task<IActionResult> Property(string? q)
    {
        var draft = await quoteRequest.GetDraftAsync();
        if (draft != null && draft.CurrentStep > 1)
        {
            return RedirectToAction(quoteRequest.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await quoteRequest.BuildPropertyAsync(q));
    }

    [HttpGet]
    public async Task<IActionResult> Start(int propertyFileId)
    {
        try
        {
            await quoteRequest.CancelDraftAsync();
            await quoteRequest.SavePropertyAsync(propertyFileId);
            return RedirectToAction(nameof(RequestDetails));
        }
        catch
        {
            return RedirectToAction(nameof(Property));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Property(int propertyFileId)
    {
        try
        {
            await quoteRequest.SavePropertyAsync(propertyFileId);
            return RedirectToAction(nameof(RequestDetails));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Please select a property.");
            var vm = await quoteRequest.BuildPropertyAsync(null);
            vm.SelectedPropertyFileId = propertyFileId;
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> BackToProperty()
    {
        await quoteRequest.PrepareBackToPropertyAsync();
        return RedirectToAction(nameof(Property));
    }

    [HttpGet]
    public async Task<IActionResult> BackToRequestDetails()
    {
        await quoteRequest.PrepareBackToRequestDetailsAsync();
        return RedirectToAction(nameof(RequestDetails));
    }

    [HttpGet]
    public async Task<IActionResult> BackToProviders()
    {
        await quoteRequest.PrepareBackToProvidersAsync();
        return RedirectToAction(nameof(Providers));
    }

    [HttpGet]
    public async Task<IActionResult> RequestDetails()
    {
        var draft = await quoteRequest.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 2)
        {
            return RedirectToAction(nameof(Property));
        }

        if (draft.CurrentStep > 2)
        {
            return RedirectToAction(quoteRequest.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await quoteRequest.BuildRequestDetailsAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestDetails(
        string requestType,
        bool sharePhotosVideos,
        bool shareInspectionReport,
        bool shareRepairItems,
        bool shareNotes,
        int responseDeadlineHours)
    {
        try
        {
            await quoteRequest.SaveRequestDetailsAsync(
                requestType, sharePhotosVideos, shareInspectionReport,
                shareRepairItems, shareNotes, responseDeadlineHours);
            return RedirectToAction(nameof(Providers));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Please complete request details.");
            return View(await quoteRequest.BuildRequestDetailsAsync());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Providers(string? q, string? filter)
    {
        var draft = await quoteRequest.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 3)
        {
            return RedirectToAction(nameof(RequestDetails));
        }

        if (draft.CurrentStep > 3)
        {
            return RedirectToAction(quoteRequest.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await quoteRequest.BuildProvidersAsync(q, filter));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Providers(
        string providerSelectionMode,
        int[]? providerIds,
        string serviceType,
        int providerCountTarget,
        bool verifiedOnly,
        string priority,
        int coverageMiles,
        string? q,
        string? filter)
    {
        try
        {
            await quoteRequest.SaveProvidersAsync(
                providerSelectionMode, providerIds, serviceType,
                providerCountTarget, verifiedOnly, priority, coverageMiles);
            return RedirectToAction(nameof(Review));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Please select at least one provider.");
            return View(await quoteRequest.BuildProvidersAsync(q, filter));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Review()
    {
        var draft = await quoteRequest.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 4)
        {
            return RedirectToAction(nameof(Providers));
        }

        return View(await quoteRequest.BuildReviewAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(
        bool sendNow,
        DateTime? scheduledSendUtc,
        int responseDeadlineHours,
        bool allowProviderQuestions,
        bool allowFullProjectQuote,
        bool allowItemizedQuote,
        string? optionalMessage)
    {
        try
        {
            var quoteId = await quoteRequest.SendAsync(
                sendNow, scheduledSendUtc, responseDeadlineHours,
                allowProviderQuestions, allowFullProjectQuote, allowItemizedQuote, optionalMessage);
            return RedirectToAction(nameof(Success), new { id = quoteId });
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Unable to send quote request. Please try again.");
            return View("Review", await quoteRequest.BuildReviewAsync());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Success(int id)
    {
        return View(await quoteRequest.BuildSuccessAsync(id));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel()
    {
        await quoteRequest.CancelDraftAsync();
        return RedirectToAction("Quotes", "Realtor");
    }
}
