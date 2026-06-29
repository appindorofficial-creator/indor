using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.Validation;
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
        if (!PersonNameAttribute.IsValidName(fullName, out var nameError))
        {
            ModelState.AddModelError(nameof(fullName), nameError!);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(nameof(email), "Email address is required.");
        }
        else if (!ValidEmailAttribute.IsValidAddress(email, out var emailError))
        {
            ModelState.AddModelError(nameof(email), emailError!);
        }

        if (!RealtorClientRoles.All.Contains(clientRole ?? "", StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(clientRole), "Please select a client role.");
        }

        if (!UsPhoneRequiredAttribute.IsValidRequired(phone, out var phoneError))
        {
            ModelState.AddModelError(nameof(phone), phoneError!);
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

        var normalizedPhone = UsPhoneOptionalAttribute.NormalizeToStorage(phone);
        await inviteService.SaveClientInfoAsync(fullName, email, normalizedPhone, clientRole, quickNote);
        return RedirectToAction(nameof(Property));
    }

    [HttpGet]
    public async Task<IActionResult> Property(string? q, bool edit = false)
    {
        var draft = await inviteService.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 2)
        {
            return RedirectToAction(nameof(ClientInfo));
        }

        if (!edit && draft.CurrentStep > 2)
        {
            return RedirectToAction(inviteService.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await inviteService.BuildPropertyAsync(q));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Property(int? propertyFileId, string? q)
    {
        if (propertyFileId is not > 0 && !string.IsNullOrWhiteSpace(q))
        {
            return RedirectToAction(nameof(CreateProperty), new { address = q.Trim() });
        }

        try
        {
            await inviteService.SavePropertyAsync(propertyFileId);
            return RedirectToAction(nameof(Access));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Please select a property or create a new one.");
            return View(await inviteService.BuildPropertyAsync(q));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateProperty(bool edit = false, string? address = null)
    {
        var draft = await inviteService.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 2)
        {
            return RedirectToAction(nameof(ClientInfo));
        }

        if (!edit && draft.CurrentStep > 2)
        {
            return RedirectToAction(inviteService.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await inviteService.BuildCreatePropertyAsync(address));
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

        if (!UsZipCodeAttribute.IsValidRequired(model.PostalCode, out var zipError))
        {
            ModelState.AddModelError(nameof(model.PostalCode), zipError!);
        }

        if (ModelState.IsValid)
        {
            try
            {
                await inviteService.CreatePropertyAsync(model);
                if (model.SelectForClient)
                {
                    return RedirectToAction(nameof(Access));
                }

                return RedirectToAction(nameof(Property));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"We couldn't save the property. {ex.GetBaseException().Message}");
            }
        }

        var invalidVm = await inviteService.BuildCreatePropertyAsync(model.Address);
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
    public async Task<IActionResult> Access(bool edit = false)
    {
        var draft = await inviteService.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 3)
        {
            return RedirectToAction(nameof(Property));
        }

        if (!edit && draft.CurrentStep > 3)
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

        if (!RealtorInviteClientService.HasAnyAccessPermission(model))
        {
            ModelState.AddModelError(string.Empty, "Select at least one access permission.");
        }

        if (string.IsNullOrWhiteSpace(model.CollaborationLevel) ||
            !RealtorCollaborationLevels.Options.Any(o =>
                string.Equals(o.Value, model.CollaborationLevel, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.CollaborationLevel), "Select a collaboration level.");
        }

        if (!model.DeliveryEmail && !model.DeliveryText)
        {
            ModelState.AddModelError(string.Empty, "Select at least one invitation delivery method (email or text).");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
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
        var draft = await inviteService.GetDraftAsync();
        if (draft == null)
        {
            return RedirectToAction(nameof(ClientInfo));
        }

        if (!PersonNameAttribute.IsValidName(draft.FullName, out var nameError))
        {
            ModelState.AddModelError(string.Empty, nameError!);
        }

        if (string.IsNullOrWhiteSpace(draft.Email))
        {
            ModelState.AddModelError(string.Empty, "Email address is required.");
        }
        else if (!ValidEmailAttribute.IsValidAddress(draft.Email, out var emailError))
        {
            ModelState.AddModelError(string.Empty, emailError!);
        }

        if (!UsPhoneRequiredAttribute.IsValidRequired(draft.Phone, out var phoneError))
        {
            ModelState.AddModelError(string.Empty, phoneError!);
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(Review), await inviteService.BuildReviewAsync());
        }

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
