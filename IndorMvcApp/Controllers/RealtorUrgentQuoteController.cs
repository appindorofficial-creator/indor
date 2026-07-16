using IndorMvcApp.Localization;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndorMvcApp.Controllers;

[Authorize]
public class RealtorUrgentQuoteController(
    IRealtorUrgentQuoteWizardService wizard,
    IRealtorRegistrationService registration,
    UserManager<ApplicationUser> userManager,
    IIndorLocalizer localizer) : Controller
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
        var draft = await wizard.GetDraftAsync();
        if (draft != null && draft.CurrentStep > 1)
        {
            return RedirectToAction(wizard.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await wizard.BuildPropertyAsync(q));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Property(
        int propertyFileId,
        string? requestCategory,
        string? serviceType,
        string? urgencyLevel,
        string? newPropertyAddress)
    {
        // Clear binder "The X field is required" messages for optional radio posts.
        ModelState.Remove(nameof(requestCategory));
        ModelState.Remove(nameof(serviceType));
        ModelState.Remove(nameof(urgencyLevel));

        try
        {
            await wizard.SavePropertyAsync(
                propertyFileId,
                requestCategory ?? string.Empty,
                serviceType ?? string.Empty,
                urgencyLevel ?? string.Empty,
                newPropertyAddress);
            return RedirectToAction(nameof(Issue));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, localizer[ex.Message]);
        }
        catch
        {
            ModelState.AddModelError(string.Empty, localizer["Select a property and what you need to continue."]);
        }

        var vm = await wizard.BuildPropertyAsync(newPropertyAddress);
        vm.SelectedPropertyFileId = propertyFileId > 0 ? propertyFileId : null;
        vm.RequestCategory = requestCategory ?? "";
        vm.ServiceType = serviceType ?? "";
        vm.UrgencyLevel = urgencyLevel ?? "";
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> BackToProperty()
    {
        await wizard.RewindToPropertyAsync();
        return RedirectToAction(nameof(Property));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickAddProperty(
        string? quickAddAddress, string? quickAddCity, string? quickAddState, string? quickAddZip, bool quickAddUseForQuote = true)
    {
        if (string.IsNullOrWhiteSpace(quickAddAddress) ||
            string.IsNullOrWhiteSpace(quickAddCity) ||
            string.IsNullOrWhiteSpace(quickAddState) ||
            string.IsNullOrWhiteSpace(quickAddZip))
        {
            ModelState.AddModelError(string.Empty, localizer["Enter street, city, state and ZIP to add a property quickly."]);
            var vm = await wizard.BuildPropertyAsync(null);
            vm.QuickAddOpen = true;
            vm.QuickAddAddress = quickAddAddress ?? "";
            vm.QuickAddCity = quickAddCity ?? "";
            vm.QuickAddState = quickAddState ?? "";
            vm.QuickAddZip = quickAddZip ?? "";
            vm.QuickAddUseForQuote = quickAddUseForQuote;
            return View("Property", vm);
        }

        await wizard.QuickAddPropertyAsync(quickAddAddress!, quickAddCity!, quickAddState!, quickAddZip!, quickAddUseForQuote);
        return RedirectToAction(nameof(Property));
    }

    [HttpGet]
    public async Task<IActionResult> Issue()
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 2)
        {
            return RedirectToAction(nameof(Property));
        }

        if (draft.CurrentStep > 2)
        {
            return RedirectToAction(wizard.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await wizard.BuildIssueAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Issue(string? serviceType, string? urgencyLevel, string? quickDescription, string? requestTypeTag)
    {
        await wizard.SaveIssueAsync(
            serviceType ?? string.Empty,
            urgencyLevel ?? string.Empty,
            quickDescription ?? string.Empty,
            requestTypeTag ?? string.Empty);
        return RedirectToAction(nameof(Photos));
    }

    [HttpGet]
    public async Task<IActionResult> Photos()
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 3)
        {
            return RedirectToAction(nameof(Issue));
        }

        if (draft.CurrentStep > 3)
        {
            return RedirectToAction(wizard.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await wizard.BuildPhotosAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Photos(string? optionalNote, bool skipPhotos, List<IFormFile>? photos)
    {
        await wizard.SavePhotosAsync(optionalNote, skipPhotos, photos);
        return RedirectToAction(nameof(Send));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePhoto(int photoId)
    {
        await wizard.RemovePhotoAsync(photoId);
        return RedirectToAction(nameof(Photos));
    }

    [HttpGet]
    public async Task<IActionResult> Send()
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 4)
        {
            return RedirectToAction(nameof(Photos));
        }

        return View(await wizard.BuildSendAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendRequest(string? providerSelectionMode, string? sendPayload, bool notifyClient)
    {
        try
        {
            var result = await wizard.SendAsync(
                providerSelectionMode ?? string.Empty,
                sendPayload ?? string.Empty,
                notifyClient);
            return View("Success", result);
        }
        catch
        {
            ModelState.AddModelError(string.Empty, localizer["Unable to send urgent quote request."]);
            return View("Send", await wizard.BuildSendAsync());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel()
    {
        await wizard.CancelDraftAsync();
        return RedirectToAction("Dashboard", "Realtor");
    }
}
