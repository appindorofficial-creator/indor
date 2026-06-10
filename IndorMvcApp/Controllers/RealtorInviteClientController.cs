using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndorMvcApp.Controllers;

[Authorize]
public class RealtorInviteClientController(
    IRealtorInviteClientService inviteService,
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
    public IActionResult Index() => RedirectToAction(nameof(ClientInfo));

    [HttpGet]
    public async Task<IActionResult> ClientInfo()
    {
        var draft = await inviteService.GetDraftAsync();
        if (draft != null && draft.CurrentStep > 1)
        {
            return RedirectToAction(inviteService.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await inviteService.BuildClientInfoAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClientInfo(
        string fullName, string email, string phone, string clientRole, string quickNote)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            ModelState.AddModelError(nameof(fullName), "Required");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(nameof(email), "Required");
        }

        if (!ModelState.IsValid)
        {
            var vm = await inviteService.BuildClientInfoAsync();
            vm.FullName = fullName ?? "";
            vm.Email = email ?? "";
            vm.Phone = phone ?? "";
            vm.ClientRole = clientRole ?? "Buyer";
            vm.QuickNote = quickNote ?? "";
            return View(vm);
        }

        await inviteService.SaveClientInfoAsync(fullName, email, phone, clientRole, quickNote);
        return RedirectToAction(nameof(Property));
    }

    [HttpGet]
    public async Task<IActionResult> Property(string? q)
    {
        var draft = await inviteService.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 2)
        {
            return RedirectToAction(nameof(ClientInfo));
        }

        if (draft.CurrentStep > 2)
        {
            return RedirectToAction(inviteService.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await inviteService.BuildPropertyAsync(q));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Property(int? propertyFileId)
    {
        try
        {
            await inviteService.SavePropertyAsync(propertyFileId);
            return RedirectToAction(nameof(Access));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Please select a property.");
            return View(await inviteService.BuildPropertyAsync(null));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Access()
    {
        var draft = await inviteService.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 3)
        {
            return RedirectToAction(nameof(Property));
        }

        if (draft.CurrentStep > 3)
        {
            return RedirectToAction(nameof(Review));
        }

        return View(await inviteService.BuildAccessAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Access(RealtorInviteAccessViewModel model)
    {
        model.DisplayStep = 3;
        model.Subtitle = "Choose what the client can access";
        model.CollaborationOptions = RealtorCollaborationLevels.Options;

        if (string.IsNullOrWhiteSpace(model.WelcomeMessage))
        {
            model.WelcomeMessage =
                "Hi! I'd like to invite you to view project details and stay updated in INDOR. Let me know if you have any questions.";
        }

        await inviteService.SaveAccessAsync(model);
        return RedirectToAction(nameof(Review));
    }

    [HttpGet]
    public async Task<IActionResult> Review()
    {
        var draft = await inviteService.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 4)
        {
            return RedirectToAction(nameof(Access));
        }

        return View(await inviteService.BuildReviewAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(bool sendReminder48h = true)
    {
        var invitationId = await inviteService.SendInvitationAsync(sendReminder48h);
        return RedirectToAction(nameof(Success), new { id = invitationId });
    }

    [HttpGet]
    public async Task<IActionResult> Success(int id)
    {
        return View(await inviteService.BuildSuccessAsync(id));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel()
    {
        await inviteService.CancelDraftAsync();
        return RedirectToAction("Clients", "Realtor");
    }
}
