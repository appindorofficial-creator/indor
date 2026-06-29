using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

[Authorize]
[ResponseCache(NoStore = true, Duration = 0, Location = ResponseCacheLocation.None)]
public class NeighborRequestController(
    NeighborRequestWizardService wizardService,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int propiedadId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var propiedad = await wizardService.ValidatePropiedadAsync(userId, propiedadId, cancellationToken);
        if (propiedad == null)
        {
            return RedirectToAction("Index", "Home");
        }

        wizardService.ClearDraft(HttpContext.Session);
        wizardService.MarkFreshStart(HttpContext.Session);
        return RedirectToAction(nameof(Category), new { propiedadId, fresh = true });
    }

    [HttpGet]
    public async Task<IActionResult> Category(int propiedadId, bool? fresh, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var propiedad = await wizardService.ValidatePropiedadAsync(userId, propiedadId, cancellationToken);
        if (propiedad == null)
        {
            return RedirectToAction("Index", "Home");
        }

        if (fresh == true)
        {
            wizardService.ClearDraft(HttpContext.Session);
            wizardService.MarkFreshStart(HttpContext.Session);
        }

        var fromWizard = IsNeighborRequestWizardReferrer();
        var isFreshStart = wizardService.IsFreshStart(HttpContext.Session);
        var draft = wizardService.LoadDraft(HttpContext.Session);

        if (draft?.EditingRequestId is int editId and > 0)
        {
            return RedirectToAction(nameof(Edit), new { id = editId });
        }

        var resumeDraft = fromWizard && !isFreshStart && draft is { CategoryId: > 0 };

        if (!resumeDraft)
        {
            await wizardService.InitializeBlankDraftAsync(HttpContext.Session, propiedad, cancellationToken);
        }

        draft = wizardService.LoadDraft(HttpContext.Session);

        return View(await wizardService.BuildCategoryStepAsync(
            propiedad,
            propiedad.Id,
            draft,
            Url,
            cancellationToken,
            useDraftFieldValues: resumeDraft));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Category(NeighborRequestCategoryStepViewModel model, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var propiedad = await wizardService.ValidatePropiedadAsync(userId, model.PropiedadId, cancellationToken);
        if (propiedad == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var categories = await wizardService.LoadCategoriesAsync(cancellationToken);
        if (model.CategoryId <= 0 || categories.All(c => c.Id != model.CategoryId))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Choose a category to continue.");
        }

        if (!ModelState.IsValid)
        {
            var invalidVm = await wizardService.BuildCategoryStepAsync(model.PropiedadId, null, Url, cancellationToken);
            invalidVm.CategoryId = model.CategoryId;
            invalidVm.SelectedCategoryId = model.CategoryId;
            invalidVm.Title = model.Title;
            invalidVm.Description = model.Description;
            invalidVm.LocationAddress = model.LocationAddress;
            invalidVm.UseHomeAddress = model.UseHomeAddress;
            invalidVm.ResumeDraft = model.ResumeDraft;
            return View(invalidVm);
        }

        var draft = wizardService.LoadDraft(HttpContext.Session);

        if (draft?.EditingRequestId is > 0)
        {
            draft.PropiedadId = model.PropiedadId;
            draft.CategoryId = model.CategoryId;
        }
        else if (model.ResumeDraft && draft is { CategoryId: > 0, EditingRequestId: null } && draft.CategoryId == model.CategoryId)
        {
            // Keep in-progress draft when returning from a later wizard step.
        }
        else
        {
            draft = await wizardService.CreateNewDraftAsync(HttpContext.Session, propiedad, model.CategoryId, cancellationToken);
        }

        draft!.CategoryId = model.CategoryId;
        draft.Title = model.Title.Trim();
        draft.Description = model.Description?.Trim() ?? string.Empty;
        draft.LocationAddress = model.LocationAddress.Trim();
        draft.UseHomeAddress = model.UseHomeAddress;
        wizardService.SaveDraft(HttpContext.Session, draft);

        return RedirectToAction(nameof(Preferences), new { propiedadId = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> Describe(int propiedadId, CancellationToken cancellationToken)
    {
        var draft = await RequireDraftAsync(propiedadId, minCategory: true, cancellationToken);
        if (draft == null)
        {
            return RestartWizard(propiedadId);
        }

        if (draft.EditingRequestId is null && string.IsNullOrWhiteSpace(draft.Title))
        {
            return RestartWizard(propiedadId);
        }

        var vm = await wizardService.BuildDescribeStepAsync(draft, cancellationToken);
        return vm == null
            ? RestartWizard(propiedadId)
            : View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Describe(NeighborRequestDescribeStepViewModel model, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var draft = await RequireDraftAsync(model.PropiedadId, minCategory: true, cancellationToken);
        if (draft == null)
        {
            return RestartWizard(model.PropiedadId);
        }

        wizardService.ApplyExtrasToDraft(draft, model);
        await wizardService.SavePhotosAsync(draft, model.PhotoFiles, cancellationToken);
        wizardService.SaveDraft(HttpContext.Session, draft);

        if (draft.EditingRequestId is > 0)
        {
            return RedirectToAction(nameof(Review), new { propiedadId = model.PropiedadId });
        }

        var requestId = await wizardService.PublishAsync(userId, draft, cancellationToken);
        if (requestId == null)
        {
            TempData["NeighborRequestError"] = "We could not publish your job. Please try again.";
            var invalidVm = await wizardService.BuildDescribeStepAsync(draft, cancellationToken);
            return View(invalidVm);
        }

        wizardService.ClearDraft(HttpContext.Session);
        return RedirectToAction(nameof(Helpers), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> Preferences(int propiedadId, CancellationToken cancellationToken)
    {
        var draft = await RequireDraftAsync(propiedadId, minCategory: true, cancellationToken);
        if (draft == null)
        {
            return RestartWizard(propiedadId);
        }

        if (draft.EditingRequestId is null && string.IsNullOrWhiteSpace(draft.Title))
        {
            return RestartWizard(propiedadId);
        }

        return View(wizardService.BuildPreferencesStep(draft));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preferences(NeighborRequestPreferencesStepViewModel model, CancellationToken cancellationToken)
    {
        var draft = await RequireDraftAsync(model.PropiedadId, minCategory: true, cancellationToken);
        if (draft == null)
        {
            return RestartWizard(model.PropiedadId);
        }

        if (!ModelState.IsValid)
        {
            var invalidVm = wizardService.BuildPreferencesStep(draft);
            MergePreferencesPostedValues(invalidVm, model);
            return View(invalidVm);
        }

        wizardService.ApplyScheduleToDraft(draft, model);
        if (!model.IsEditMode)
        {
            draft.AudienceCode = model.SelectedAudiences is { Count: > 0 }
                ? NeighborRequestWizardService.CombineAudienceCodes(model.SelectedAudiences)
                : NeighborRequestAudienceCodes.Both;
        }

        wizardService.SaveDraft(HttpContext.Session, draft);

        var userId = userManager.GetUserId(User);
        if (userId != null)
        {
            await wizardService.PersistPreferencesAsync(userId, draft, cancellationToken);
        }

        return model.IsEditMode
            ? RedirectToAction(nameof(Review), new { propiedadId = model.PropiedadId })
            : RedirectToAction(nameof(Describe), new { propiedadId = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> Review(int propiedadId, CancellationToken cancellationToken)
    {
        var draft = await RequireDraftAsync(propiedadId, minCategory: true, cancellationToken);
        if (draft == null)
        {
            return RestartWizard(propiedadId);
        }

        if (draft.EditingRequestId is null && string.IsNullOrWhiteSpace(draft.Title))
        {
            return RestartWizard(propiedadId);
        }

        if (!NeighborRequestWizardService.IsNeededByDateAllowed(draft.NeededByDate))
        {
            TempData["NeighborRequestError"] = NeighborRequestWizardService.NeededByDatePastErrorMessage;
            if (draft.EditingRequestId is int editId and > 0)
            {
                return RedirectToAction(nameof(Edit), new { id = editId });
            }

            return RedirectToAction(nameof(Preferences), new { propiedadId });
        }

        var vm = await wizardService.BuildReviewStepAsync(draft, cancellationToken);
        return vm == null
            ? RestartWizard(propiedadId)
            : View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int propiedadId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var draft = await RequireDraftAsync(propiedadId, minCategory: true, cancellationToken);
        if (draft == null || string.IsNullOrWhiteSpace(draft.Title))
        {
            return RestartWizard(propiedadId);
        }

        if (!NeighborRequestWizardService.IsNeededByDateAllowed(draft.NeededByDate))
        {
            TempData["NeighborRequestError"] = NeighborRequestWizardService.NeededByDatePastErrorMessage;
            if (draft.EditingRequestId is int editId and > 0)
            {
                return RedirectToAction(nameof(Edit), new { id = editId });
            }

            return RedirectToAction(nameof(Review), new { propiedadId });
        }

        var requestId = await wizardService.PublishAsync(userId, draft, cancellationToken);
        if (requestId == null)
        {
            TempData["NeighborRequestError"] = "We could not publish your request. Please try again.";
            return RedirectToAction(nameof(Review), new { propiedadId });
        }

        var editingRequestId = draft.EditingRequestId;
        wizardService.ClearDraft(HttpContext.Session);

        if (editingRequestId is > 0)
        {
            TempData["NeighborRequestSaved"] = "Your request was updated.";
            return RedirectToAction(nameof(Detail), new { id = editingRequestId.Value });
        }

        return RedirectToAction(nameof(Helpers), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> Helpers(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var vm = await wizardService.BuildHelpersStepAsync(userId, id, Url, cancellationToken);
        return vm == null ? RedirectToAction("Index", "Home") : View(vm);
    }

    [HttpGet]
    public IActionResult Success(int id) =>
        RedirectToAction(nameof(Helpers), new { id });

    [HttpGet]
    public async Task<IActionResult> Mine(int propiedadId, string? tab, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var propiedad = await wizardService.ValidatePropiedadAsync(userId, propiedadId, cancellationToken);
        if (propiedad == null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(await wizardService.BuildMineAsync(userId, propiedadId, tab, Url, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var vm = await wizardService.BuildDetailAsync(userId, id, Url, cancellationToken);
        return vm == null ? RedirectToAction("Index", "Home") : View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Offers(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var vm = await wizardService.BuildOffersListAsync(userId, id, Url, cancellationToken);
        return vm == null ? RedirectToAction("Index", "Home") : View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, string? step, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var propiedadId = await wizardService.LoadRequestForEditAsync(userId, id, HttpContext.Session, cancellationToken);

        if (string.Equals(step, "providers", StringComparison.OrdinalIgnoreCase) && propiedadId != null)
        {
            return RedirectToAction(nameof(Preferences), new { propiedadId });
        }

        var vm = await wizardService.BuildEditDetailsStepAsync(userId, id, Url, cancellationToken);
        return vm == null ? RedirectToAction("Index", "Home") : View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(NeighborRequestEditDetailsStepViewModel model, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid || model.RequestId is not > 0)
        {
            var invalidVm = await wizardService.BuildEditDetailsStepAsync(userId, model.RequestId ?? 0, Url, cancellationToken);
            if (invalidVm == null)
            {
                return RedirectToAction("Index", "Home");
            }

            invalidVm.CategoryId = model.CategoryId;
            invalidVm.DetailsSummary = model.DetailsSummary;
            invalidVm.Description = model.Description;
            invalidVm.NeededByDate = model.NeededByDate;
            invalidVm.TimeWindowPreset = model.TimeWindowPreset;
            invalidVm.AudienceCode = model.AudienceCode;

            return View(invalidVm);
        }

        var saved = await wizardService.SaveEditDetailsStepAsync(userId, model, HttpContext.Session, cancellationToken);
        if (!saved)
        {
            if (!NeighborRequestWizardService.IsNeededByDateAllowed(model.NeededByDate))
            {
                ModelState.AddModelError(
                    nameof(model.NeededByDate),
                    NeighborRequestWizardService.NeededByDatePastErrorMessage);
            }

            var failedVm = await wizardService.BuildEditDetailsStepAsync(userId, model.RequestId ?? 0, Url, cancellationToken);
            if (failedVm == null)
            {
                return RedirectToAction("Index", "Home");
            }

            failedVm.CategoryId = model.CategoryId;
            failedVm.DetailsSummary = model.DetailsSummary;
            failedVm.Description = model.Description;
            failedVm.NeededByDate = model.NeededByDate;
            failedVm.TimeWindowPreset = model.TimeWindowPreset;
            failedVm.AudienceCode = model.AudienceCode;

            return View(failedVm);
        }

        return RedirectToAction(nameof(Preferences), new { propiedadId = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var vm = await wizardService.BuildCancelStepAsync(userId, id, Url, cancellationToken);
        return vm == null ? RedirectToAction("Index", "Home") : View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(NeighborRequestCancelViewModel model, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var propiedadId = await wizardService.CancelRequestAsync(
            userId,
            model.Id,
            model.CancelReasonCode,
            model.CancelNote,
            cancellationToken);

        if (propiedadId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        TempData["NeighborRequestSaved"] = "Your request was cancelled.";
        return RedirectToAction(nameof(Mine), new { propiedadId });
    }

    private IActionResult RestartWizard(int propiedadId) =>
        RedirectToAction(nameof(Create), new { propiedadId });

    private bool IsNeighborRequestWizardReferrer()
    {
        var referer = Request.Headers.Referer.ToString();
        return referer.Contains("/NeighborRequest/", StringComparison.OrdinalIgnoreCase);
    }

    private static void MergePreferencesPostedValues(
        NeighborRequestPreferencesStepViewModel target,
        NeighborRequestPreferencesStepViewModel source)
    {
        target.WhenCode = source.WhenCode;
        target.PreferredTimeCode = source.PreferredTimeCode;
        target.HelperCount = source.HelperCount;
        target.DurationCode = source.DurationCode;
        target.PayTypeCode = source.PayTypeCode;
        target.TimelineCode = source.TimelineCode;
        target.AudienceCode = source.AudienceCode;
        target.SelectedAudiences = source.SelectedAudiences;
        target.BudgetAmount = source.BudgetAmount;
        target.NeededByDate = source.NeededByDate;
    }

    private async Task<NeighborRequestDraftState?> RequireDraftAsync(
        int propiedadId,
        bool minCategory,
        CancellationToken ct)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return null;
        }

        var propiedad = await wizardService.ValidatePropiedadAsync(userId, propiedadId, ct);
        if (propiedad == null)
        {
            return null;
        }

        var draft = wizardService.LoadDraft(HttpContext.Session);
        if (draft == null || draft.PropiedadId != propiedadId)
        {
            return null;
        }

        if (minCategory && draft.CategoryId <= 0)
        {
            return null;
        }

        return draft;
    }

    private static string NormalizeTimeline(string? code) =>
        code?.Trim() switch
        {
            NeighborRequestTimelineCodes.Asap => NeighborRequestTimelineCodes.Asap,
            NeighborRequestTimelineCodes.ThisMonth => NeighborRequestTimelineCodes.ThisMonth,
            NeighborRequestTimelineCodes.Flexible => NeighborRequestTimelineCodes.Flexible,
            _ => NeighborRequestTimelineCodes.ThisWeek
        };

    private static string NormalizeAudience(string? code) =>
        code?.Trim() switch
        {
            NeighborRequestAudienceCodes.CertifiedProviders => NeighborRequestAudienceCodes.CertifiedProviders,
            NeighborRequestAudienceCodes.Both => NeighborRequestAudienceCodes.Both,
            _ => NeighborRequestAudienceCodes.Neighbors
        };
}
