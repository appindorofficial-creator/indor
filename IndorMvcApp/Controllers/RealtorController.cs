using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.Validation;
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
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment env) : Controller
{
    private const long MaxDocumentBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedDocExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".webp"];
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

        var model = await nearbyNetworkService.BuildListingFormAsync(realtor, null, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        model.WizardStep = 1;
        ViewBag.ListingWizardPage = "details";
        return View("NetworkListing/Details", model);
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
            subtitle: null,
            showStepper: true,
            cancellationToken);
        ViewBag.ListingWizardPage = "benefits";
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
        model.WizardStep = 3;
        ViewBag.ListingWizardPage = "details";
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
        ViewBag.ListingWizardPage = "details";
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
        ViewBag.ListingWizardPage = "details";
    }

    [HttpGet]
    public async Task<IActionResult> Profile(bool? saved, CancellationToken cancellationToken)
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

        var model = await portalService.BuildProfileAsync(realtor, cancellationToken);
        model.NotificationsSaved = saved == true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProfileNotificationPreferences(
        RealtorNotificationPreferencesInput input,
        CancellationToken cancellationToken)
    {
        var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return isAjax ? Unauthorized() : RedirectToAction("Profile", "RealtorRegistration");
        }

        if (realtor.RegistrationStatus == RealtorRegistrationStatuses.Draft)
        {
            return isAjax ? Unauthorized() : RedirectToAction("Profile", "RealtorRegistration");
        }

        await portalService.SaveNotificationPreferencesAsync(realtor, input, cancellationToken);

        // For toggle clicks we save in the background (fetch) so the page doesn't
        // reload and jump back to the top; keep the redirect as a no-JS fallback.
        if (isAjax)
        {
            return Ok(new { saved = true });
        }

        return RedirectToAction(nameof(Profile), new { saved = true });
    }

    [HttpGet]
    public IActionResult EditProfile() =>
        RedirectToAction(nameof(EditProfileContact), new { from = "public" });

    [HttpGet]
    public async Task<IActionResult> BusinessInformation(CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        if (realtor.RegistrationStatus == RealtorRegistrationStatuses.Draft)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await portalService.BuildBusinessInformationAsync(realtor, cancellationToken);
        return View("EditProfile/BusinessInformation", model);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfileContact(string? from, CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await portalService.BuildEditProfileContactAsync(
            realtor,
            registration.GetLicenseStates(),
            cancellationToken);

        if (string.Equals(from, "public", StringComparison.OrdinalIgnoreCase))
        {
            model.BackAction = "PublicProfile";
            model.BackController = "Realtor";
        }

        return View("EditProfile/EditProfileContact", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfileContact(
        RealtorEditProfileContactViewModel model,
        CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        if (string.IsNullOrWhiteSpace(model.BusinessName))
        {
            ModelState.AddModelError(nameof(model.BusinessName), "Business Name is required.");
        }

        if (!BrokerageNameAttribute.IsValidBrokerageName(model.BrokerageName, out var brokerageError, "Brokerage Name"))
        {
            ModelState.AddModelError(nameof(model.BrokerageName), brokerageError!);
        }

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email is required.");
        }

        if (!RealtorSupportedLanguages.TryNormalize(model.LanguagesCsv, out _, out var languagesError))
        {
            ModelState.AddModelError(nameof(model.LanguagesCsv), languagesError!);
        }

        if (!ModelState.IsValid)
        {
            var messages = CollectModelStateErrors();
            if (messages.Count > 0)
            {
                TempData["EditProfileError"] = string.Join(" ", messages);
            }

            await ApplyEditProfileShellAsync(realtor, model, cancellationToken);
            model.LicenseStates = registration.GetLicenseStates();
            return View("EditProfile/EditProfileContact", model);
        }

        await portalService.SaveEditProfileContactAsync(realtor, model, cancellationToken);
        return RedirectToAction(nameof(EditProfileLicense));
    }

    [HttpGet]
    public async Task<IActionResult> EditProfileLicense(CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        var model = await portalService.BuildEditProfileLicenseAsync(
            realtor,
            registration.GetLicenseStates(),
            cancellationToken);
        return View("EditProfile/EditProfileLicense", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> EditProfileLicense(
        RealtorEditProfileLicenseViewModel model,
        List<string>? selectedSpecialties,
        IFormFile? licensePhotoFile,
        CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        model.SelectedSpecialties = selectedSpecialties?
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Take(3)
            .ToList() ?? [];

        ModelState.Remove(nameof(licensePhotoFile));
        ModelState.Remove(nameof(model.TeamName));
        ModelState.Remove(nameof(model.BrokerInCharge));

        if (!RealtorLicenseNumberAttribute.IsValidLicenseNumber(model.LicenseNumber, out var licenseError))
        {
            ModelState.AddModelError(nameof(model.LicenseNumber), licenseError!);
        }

        if (string.IsNullOrWhiteSpace(model.LicenseState))
        {
            ModelState.AddModelError(nameof(model.LicenseState), "License state is required.");
        }

        if (string.IsNullOrWhiteSpace(model.YearsOfExperience))
        {
            ModelState.AddModelError(nameof(model.YearsOfExperience), "Years of experience is required.");
        }

        if (model.SelectedSpecialties.Count == 0)
        {
            ModelState.AddModelError(nameof(model.SelectedSpecialties), "Select at least one specialty.");
        }

        if (!ModelState.IsValid)
        {
            var messages = CollectModelStateErrors();
            if (messages.Count > 0)
            {
                TempData["EditProfileError"] = string.Join(" ", messages);
            }

            var viewModel = await MergeEditProfileLicenseAsync(realtor, model, cancellationToken);
            return View("EditProfile/EditProfileLicense", viewModel);
        }

        try
        {
            await portalService.SaveEditProfileLicenseAsync(realtor, model, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            TempData["EditProfileError"] = ex.Message;
            var viewModel = await MergeEditProfileLicenseAsync(realtor, model, cancellationToken);
            return View("EditProfile/EditProfileLicense", viewModel);
        }

        await SaveLicenseDocumentAsync(realtor, licensePhotoFile, cancellationToken);
        return RedirectToAction(nameof(EditProfileReview));
    }

    [HttpGet]
    public async Task<IActionResult> EditProfileReview(CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        if (!HasValidStoredLicense(realtor))
        {
            return RedirectToEditProfileLicenseForInvalidLicense();
        }

        var model = await portalService.BuildEditProfileReviewAsync(realtor, cancellationToken);
        return View("EditProfile/EditProfileReview", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("SaveEditProfile")]
    public async Task<IActionResult> SaveEditProfileReview(CancellationToken cancellationToken)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return RedirectToAction("Profile", "RealtorRegistration");
        }

        if (!HasValidStoredLicense(realtor))
        {
            return RedirectToEditProfileLicenseForInvalidLicense();
        }

        await portalService.FinalizeEditProfileAsync(realtor, cancellationToken);
        return RedirectToAction(nameof(PublicProfile));
    }

    [HttpGet]
    public async Task<IActionResult> PublicProfile(CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildPublicProfileAsync(r, cancellationToken), cancellationToken);

    [HttpGet]
    public async Task<IActionResult> ProviderNetwork(string? q, string? filter, CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildNetworkAsync(r, q, filter, cancellationToken), cancellationToken);

    [HttpGet]
    public async Task<IActionResult> SupportAccount(CancellationToken cancellationToken) =>
        await PortalPageAsync(r => portalService.BuildShellAsync(r, cancellationToken), cancellationToken);

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

    private static bool HasValidStoredLicense(IndorRealtor realtor) =>
        !string.IsNullOrWhiteSpace(realtor.LicenseState)
        && RealtorLicenseNumberAttribute.IsValidLicenseNumber(realtor.LicenseNumber, out _);

    private List<string> CollectModelStateErrors() =>
        ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private IActionResult RedirectToEditProfileLicenseForInvalidLicense()
    {
        TempData["EditProfileError"] =
            "Enter a valid license number (4–20 characters, at least one letter) to continue.";
        return RedirectToAction(nameof(EditProfileLicense));
    }

    private async Task<RealtorEditProfileLicenseViewModel> MergeEditProfileLicenseAsync(
        IndorRealtor realtor,
        RealtorEditProfileLicenseViewModel posted,
        CancellationToken cancellationToken)
    {
        var model = await portalService.BuildEditProfileLicenseAsync(
            realtor,
            registration.GetLicenseStates(),
            cancellationToken);

        model.LicenseNumber = posted.LicenseNumber;
        model.LicenseState = posted.LicenseState;
        model.YearsOfExperience = posted.YearsOfExperience;
        model.SelectedSpecialties = posted.SelectedSpecialties;
        model.TeamName = posted.TeamName;
        model.BrokerInCharge = posted.BrokerInCharge;
        return model;
    }

    private async Task ApplyEditProfileShellAsync(
        IndorRealtor realtor,
        RealtorEditProfileWizardViewModel model,
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

    private async Task SaveLicenseDocumentAsync(
        IndorRealtor realtor,
        IFormFile? licensePhotoFile,
        CancellationToken cancellationToken)
    {
        if (licensePhotoFile == null || licensePhotoFile.Length == 0)
        {
            return;
        }

        var ext = Path.GetExtension(licensePhotoFile.FileName);
        if (!AllowedDocExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase) ||
            licensePhotoFile.Length > MaxDocumentBytes)
        {
            return;
        }

        var folder = Path.Combine(env.WebRootPath, "uploads", "realtor-docs", realtor.Id.ToString());
        Directory.CreateDirectory(folder);
        var fileName = $"{RealtorDocumentTypes.LicensePhoto}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await licensePhotoFile.CopyToAsync(stream, cancellationToken);
        }

        var relativeUrl = $"/uploads/realtor-docs/{realtor.Id}/{fileName}";
        await registration.RegisterDocumentUploadAsync(RealtorDocumentTypes.LicensePhoto, relativeUrl, cancellationToken);
    }
}
