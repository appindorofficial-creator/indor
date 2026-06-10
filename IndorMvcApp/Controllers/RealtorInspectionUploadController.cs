using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndorMvcApp.Controllers;

[Authorize]
public class RealtorInspectionUploadController(
    IRealtorInspectionUploadWizardService wizard,
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
    public IActionResult Index() => RedirectToAction(nameof(Upload));

    [HttpGet]
    public async Task<IActionResult> Upload(string? q)
    {
        var draft = await wizard.GetDraftAsync();
        if (draft != null && draft.CurrentStep > 1)
        {
            return RedirectToAction(wizard.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await wizard.BuildUploadAsync(q));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int propertyFileId, string uploadMethod, IFormFile? reportFile)
    {
        try
        {
            await wizard.SaveUploadAsync(propertyFileId, uploadMethod, reportFile);
            return RedirectToAction(nameof(Analyze));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Select a property and add a report to continue.");
            var vm = await wizard.BuildUploadAsync(null);
            vm.SelectedPropertyFileId = propertyFileId;
            vm.UploadMethod = uploadMethod;
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Analyze()
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 2)
        {
            return RedirectToAction(nameof(Upload));
        }

        if (draft.CurrentStep > 2 && draft.AnalysisStatus == RealtorInspectionAnalysisStatuses.Complete)
        {
            return RedirectToAction(wizard.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await wizard.BuildAnalyzeAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Analyze(string action)
    {
        if (string.Equals(action, "background", StringComparison.OrdinalIgnoreCase))
        {
            await wizard.AdvanceAnalysisAsync();
            return RedirectToAction("Dashboard", "Realtor");
        }

        await wizard.CompleteAnalysisAsync();
        return RedirectToAction(nameof(Priorities));
    }

    [HttpGet]
    public async Task<IActionResult> Priorities(string? filter, string? sort)
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 3)
        {
            return RedirectToAction(nameof(Analyze));
        }

        if (draft.CurrentStep > 3)
        {
            return RedirectToAction(wizard.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await wizard.BuildPrioritiesAsync(filter, sort));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Priorities(int[]? selectedFindingIds)
    {
        await wizard.SavePrioritiesAsync(selectedFindingIds);
        return RedirectToAction(nameof(Providers));
    }

    [HttpGet]
    public async Task<IActionResult> Providers(string? trade)
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 4)
        {
            return RedirectToAction(nameof(Priorities));
        }

        if (draft.CurrentStep > 4)
        {
            return RedirectToAction(wizard.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await wizard.BuildProvidersAsync(trade));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProviders(string? trade)
    {
        var providersByTrade = ParseTradeProviders(Request.Form);
        await wizard.SaveProvidersAsync(providersByTrade);
        return RedirectToAction(nameof(Review));
    }

    [HttpGet]
    public async Task<IActionResult> Review()
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 5)
        {
            return RedirectToAction(nameof(Providers));
        }

        return View(await wizard.BuildReviewAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRequests()
    {
        try
        {
            var result = await wizard.CreateQuoteRequestsAsync();
            return View("Success", result);
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Unable to create quote requests.");
            return View("Review", await wizard.BuildReviewAsync());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel()
    {
        await wizard.CancelDraftAsync();
        return RedirectToAction("Dashboard", "Realtor");
    }

    private static Dictionary<string, int[]> ParseTradeProviders(IFormCollection form)
    {
        var result = new Dictionary<string, int[]>();
        foreach (var key in form.Keys)
        {
            if (!key.StartsWith("provider_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var trade = key["provider_".Length..];
            var ids = form[key].Select(v => int.TryParse(v, out var id) ? id : 0).Where(id => id > 0).ToArray();
            if (ids.Length > 0)
            {
                result[trade] = ids;
            }
        }

        return result;
    }
}
