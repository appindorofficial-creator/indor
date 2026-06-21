using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

[Authorize]
public class NeighborRequestController(
    NeighborRequestWizardService wizardService,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public Task<IActionResult> Create(int propiedadId, CancellationToken ct) =>
        RedirectToCategoryAsync(propiedadId, ct);

    [HttpGet]
    public async Task<IActionResult> Category(int propiedadId, CancellationToken cancellationToken)
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

        await wizardService.EnsureDraftAsync(HttpContext.Session, propiedad, cancellationToken);
        var draft = wizardService.LoadDraft(HttpContext.Session);
        var model = await wizardService.BuildCategoryStepAsync(propiedadId, Url, cancellationToken);
        if (draft?.CategoryId > 0)
        {
            model.SelectedCategoryId = draft.CategoryId;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Category(int propiedadId, int categoryId, CancellationToken cancellationToken)
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

        var categories = await wizardService.LoadCategoriesAsync(cancellationToken);
        if (categories.All(c => c.Id != categoryId))
        {
            return RedirectToAction(nameof(Category), new { propiedadId });
        }

        var draft = wizardService.LoadDraft(HttpContext.Session) ?? new NeighborRequestDraftState { PropiedadId = propiedadId };
        draft.PropiedadId = propiedadId;
        draft.CategoryId = categoryId;
        wizardService.SaveDraft(HttpContext.Session, draft);

        return RedirectToAction(nameof(Describe), new { propiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> Describe(int propiedadId, CancellationToken cancellationToken)
    {
        var draft = await RequireDraftAsync(propiedadId, minCategory: true, cancellationToken);
        if (draft == null)
        {
            return RedirectToAction(nameof(Category), new { propiedadId });
        }

        var vm = await wizardService.BuildDescribeStepAsync(draft, cancellationToken);
        return vm == null
            ? RedirectToAction(nameof(Category), new { propiedadId })
            : View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Describe(NeighborRequestDescribeStepViewModel model, CancellationToken cancellationToken)
    {
        var draft = await RequireDraftAsync(model.PropiedadId, minCategory: true, cancellationToken);
        if (draft == null)
        {
            return RedirectToAction(nameof(Category), new { propiedadId = model.PropiedadId });
        }

        if (!ModelState.IsValid)
        {
            var invalidVm = await wizardService.BuildDescribeStepAsync(draft, cancellationToken);
            if (invalidVm != null)
            {
                invalidVm.Title = model.Title;
                invalidVm.Description = model.Description;
                invalidVm.LocationAddress = model.LocationAddress;
                invalidVm.NeededByDate = model.NeededByDate;
            }

            return View(invalidVm);
        }

        draft.Title = model.Title.Trim();
        draft.Description = model.Description?.Trim() ?? string.Empty;
        draft.LocationAddress = model.LocationAddress.Trim();
        draft.NeededByDate = model.NeededByDate?.Date;

        await wizardService.SavePhotosAsync(draft, model.PhotoFiles, cancellationToken);
        wizardService.SaveDraft(HttpContext.Session, draft);

        return RedirectToAction(nameof(Preferences), new { propiedadId = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> Preferences(int propiedadId, CancellationToken cancellationToken)
    {
        var draft = await RequireDraftAsync(propiedadId, minCategory: true, cancellationToken);
        if (draft == null)
        {
            return RedirectToAction(nameof(Category), new { propiedadId });
        }

        if (draft.EditingRequestId is null && string.IsNullOrWhiteSpace(draft.Title))
        {
            return RedirectToAction(nameof(Describe), new { propiedadId });
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
            return RedirectToAction(nameof(Category), new { propiedadId = model.PropiedadId });
        }

        draft.TimelineCode = NormalizeTimeline(model.TimelineCode);
        if (!model.IsEditMode)
        {
            draft.AudienceCode = NormalizeAudience(model.AudienceCode);
        }

        draft.BudgetAmount = model.BudgetAmount is > 0 ? model.BudgetAmount : null;
        wizardService.SaveDraft(HttpContext.Session, draft);

        var userId = userManager.GetUserId(User);
        if (userId != null)
        {
            await wizardService.PersistPreferencesAsync(userId, draft, cancellationToken);
        }

        return RedirectToAction(nameof(Review), new { propiedadId = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> Review(int propiedadId, CancellationToken cancellationToken)
    {
        var draft = await RequireDraftAsync(propiedadId, minCategory: true, cancellationToken);
        if (draft == null)
        {
            return RedirectToAction(nameof(Category), new { propiedadId });
        }

        if (draft.EditingRequestId is null && string.IsNullOrWhiteSpace(draft.Title))
        {
            return RedirectToAction(nameof(Describe), new { propiedadId });
        }

        var vm = await wizardService.BuildReviewStepAsync(draft, cancellationToken);
        return vm == null
            ? RedirectToAction(nameof(Category), new { propiedadId })
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
            return RedirectToAction(nameof(Category), new { propiedadId });
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

        return RedirectToAction(nameof(Success), new { id = requestId });
    }

    [HttpGet]
    public async Task<IActionResult> Success(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var detail = await wizardService.BuildDetailAsync(userId, id, Url, cancellationToken);
        if (detail == null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new NeighborRequestSuccessViewModel
        {
            RequestId = id,
            PropiedadId = detail.PropiedadId
        });
    }

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
            if (invalidVm != null)
            {
                invalidVm.CategoryId = model.CategoryId;
                invalidVm.DetailsSummary = model.DetailsSummary;
                invalidVm.Description = model.Description;
                invalidVm.NeededByDate = model.NeededByDate;
                invalidVm.TimeWindowPreset = model.TimeWindowPreset;
                invalidVm.AudienceCode = model.AudienceCode;
            }

            return View(invalidVm);
        }

        var saved = await wizardService.SaveEditDetailsStepAsync(userId, model, HttpContext.Session, cancellationToken);
        if (!saved)
        {
            return RedirectToAction("Index", "Home");
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

    private Task<IActionResult> RedirectToCategoryAsync(int propiedadId, CancellationToken ct) =>
        Task.FromResult<IActionResult>(RedirectToAction(nameof(Category), new { propiedadId }));

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
