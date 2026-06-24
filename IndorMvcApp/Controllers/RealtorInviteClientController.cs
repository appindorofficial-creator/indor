using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Controllers;

[Authorize]
public class RealtorInviteClientController(
    IRealtorInviteClientService inviteService,
    IRealtorRegistrationService registration,
    UserManager<ApplicationUser> userManager) : Controller
{
    private static readonly Regex UsZipRegex = new(@"^\d{5}(-\d{4})?$", RegexOptions.Compiled);
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
    public IActionResult Index() => RedirectToAction(nameof(New));

    [HttpGet]
    public async Task<IActionResult> New()
    {
        await inviteService.CancelDraftAsync();
        return RedirectToAction(nameof(ClientInfo), new { edit = true });
    }

    [HttpGet]
    public async Task<IActionResult> ClientInfo(bool edit = false)
    {
        var draft = await inviteService.GetDraftAsync();
        if (!edit && draft != null && draft.CurrentStep > 1)
        {
            return RedirectToAction(inviteService.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await inviteService.BuildClientInfoAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClientInfo(
        string fullName, string email, string? phone, string clientRole, string? quickNote)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            ModelState.AddModelError(nameof(fullName), "Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(nameof(email), "Email address is required.");
        }
        else if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email.Trim()))
        {
            ModelState.AddModelError(nameof(email), "Please enter a valid email address.");
        }

        if (!RealtorClientRoles.All.Contains(clientRole ?? "", StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(clientRole), "Please select a client role.");
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
    public async Task<IActionResult> CreateProperty()
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

        return View(await inviteService.BuildCreatePropertyAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProperty(RealtorInviteCreatePropertyViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Address))
        {
            ModelState.AddModelError(nameof(model.Address), "Property address is required.");
        }

        if (string.IsNullOrWhiteSpace(model.City))
        {
            ModelState.AddModelError(nameof(model.City), "City is required.");
        }

        if (string.IsNullOrWhiteSpace(model.StateCode))
        {
            ModelState.AddModelError(nameof(model.StateCode), "State is required.");
        }

        if (string.IsNullOrWhiteSpace(model.PostalCode))
        {
            ModelState.AddModelError(nameof(model.PostalCode), "ZIP code is required.");
        }
        else if (!UsZipRegex.IsMatch(model.PostalCode.Trim()))
        {
            ModelState.AddModelError(nameof(model.PostalCode), "Enter a valid ZIP code.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                await inviteService.CreatePropertyAsync(model);
                return RedirectToAction(nameof(Property));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"We couldn't save the property. {ex.GetBaseException().Message}");
            }
        }

        var invalidVm = await inviteService.BuildCreatePropertyAsync();
        invalidVm.Address = model.Address ?? "";
        invalidVm.Unit = model.Unit ?? "";
        invalidVm.City = model.City ?? "";
        invalidVm.StateCode = model.StateCode ?? "";
        invalidVm.PostalCode = model.PostalCode ?? "";
        invalidVm.Nickname = model.Nickname ?? "";
        invalidVm.PropertyType = model.PropertyType ?? RealtorPropertyTypes.SingleFamily;
        invalidVm.SelectForClient = model.SelectForClient;
        return View(invalidVm);
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
