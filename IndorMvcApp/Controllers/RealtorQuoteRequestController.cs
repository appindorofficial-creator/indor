using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;

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
    public async Task<IActionResult> Property(string? q, string? returnTo)
    {
        if (!string.IsNullOrWhiteSpace(returnTo))
        {
            RealtorWizardReturnNavigation.CaptureReturnTo(
                HttpContext.Session,
                returnTo,
                RealtorWizardReturnNavigation.QuoteRequestSessionKey,
                RealtorWizardReturnNavigation.Quotes);
        }
        else
        {
            RealtorWizardReturnNavigation.CaptureReturnToIfMissing(
                HttpContext.Session,
                RealtorWizardReturnNavigation.QuoteRequestSessionKey,
                RealtorWizardReturnNavigation.Quotes);
        }

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
        string? scheduledSendUtc,
        string? scheduledSendDate,
        string? scheduledSendTime,
        int responseDeadlineHours,
        bool allowProviderQuestions,
        bool allowFullProjectQuote,
        bool allowItemizedQuote,
        string? optionalMessage)
    {
        var parsedScheduledSendUtc = ParseScheduledSendUtc(sendNow, scheduledSendUtc, scheduledSendDate, scheduledSendTime);

        try
        {
            var quoteId = await quoteRequest.SendAsync(
                sendNow, parsedScheduledSendUtc, responseDeadlineHours,
                allowProviderQuestions, allowFullProjectQuote, allowItemizedQuote, optionalMessage);
            return RedirectToAction(nameof(Success), new { id = quoteId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex is InvalidOperationException ? ex.Message : "Unable to send quote request. Please try again.");

            var vm = await quoteRequest.BuildReviewAsync();
            vm.SendNow = sendNow;
            vm.ScheduledSendUtc = parsedScheduledSendUtc;
            vm.ResponseDeadlineHours = responseDeadlineHours;
            vm.AllowProviderQuestions = allowProviderQuestions;
            vm.AllowFullProjectQuote = allowFullProjectQuote;
            vm.AllowItemizedQuote = allowItemizedQuote;
            vm.OptionalMessage = optionalMessage ?? "";
            return View("Review", vm);
        }
    }

    private static DateTime? ParseScheduledSendUtc(
        bool sendNow,
        string? scheduledSendUtc,
        string? scheduledSendDate,
        string? scheduledSendTime)
    {
        if (sendNow)
        {
            return null;
        }

        var combined = scheduledSendUtc?.Trim();
        if (string.IsNullOrWhiteSpace(combined))
        {
            var date = scheduledSendDate?.Trim();
            var time = scheduledSendTime?.Trim();
            if (!string.IsNullOrWhiteSpace(date) && !string.IsNullOrWhiteSpace(time))
            {
                combined = $"{date}T{time}";
            }
        }

        if (string.IsNullOrWhiteSpace(combined))
        {
            return null;
        }

        if (DateTime.TryParseExact(
                combined,
                "yyyy-MM-ddTHH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var localSchedule)
            || DateTime.TryParseExact(
                combined,
                "yyyy-MM-ddTHH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out localSchedule))
        {
            return localSchedule.ToUniversalTime();
        }

        if (DateTime.TryParse(
                combined,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out localSchedule))
        {
            return localSchedule.ToUniversalTime();
        }

        return null;
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
        var returnTo = RealtorWizardReturnNavigation.GetReturnToken(
            HttpContext.Session,
            RealtorWizardReturnNavigation.QuoteRequestSessionKey,
            RealtorWizardReturnNavigation.Quotes);
        await quoteRequest.CancelDraftAsync();
        RealtorWizardReturnNavigation.ClearReturnTo(
            HttpContext.Session,
            RealtorWizardReturnNavigation.QuoteRequestSessionKey);
        return RealtorWizardReturnNavigation.RedirectTo(this, returnTo);
    }
}
