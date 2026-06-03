using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndorMvcApp.Controllers;

[Authorize]
public class ProviderRegistrationController(
    IProviderRegistrationService registration,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Controller
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
            !string.Equals(user.RolUsuario, "ProveedorServicios", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = RedirectToAction("Index", "Home");
            return;
        }

        if (!await userManager.IsInRoleAsync(user, "ProveedorServicios"))
        {
            await userManager.AddToRoleAsync(user, "ProveedorServicios");
            await signInManager.RefreshSignInAsync(user);
        }

        await next();
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Categories));

    [HttpGet]
    public async Task<IActionResult> Categories()
    {
        await registration.LinkCurrentUserAsync();
        var state = await registration.GetAsync();
        ViewBag.Categories = await registration.GetCategoriesAsync();
        ViewBag.SelectedIds = state.SelectedCategoryIds;
        return View(StepVm(1, "What type of provider are you?",
            "Select all trades that apply. Electricians must pass the INDOR electrical exam.",
            state, Url.Action("Welcome", "Account")));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Categories(string[]? categoryIds)
    {
        var state = await registration.GetAsync();
        state.SelectedCategoryIds = categoryIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
        if (state.SelectedCategoryIds.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Select at least one category.");
            ViewBag.Categories = await registration.GetCategoriesAsync();
            ViewBag.SelectedIds = state.SelectedCategoryIds;
            return View(StepVm(1, "What type of provider are you?",
                "Select all trades that apply.", state, Url.Action(nameof(Categories))));
        }

        await registration.SaveAsync(state, 1);
        return RedirectToAction(nameof(Services));
    }

    [HttpGet]
    public async Task<IActionResult> Services()
    {
        var (state, redirect) = await RequireCategoriesAsync();
        if (redirect != null)
        {
            return redirect;
        }
        ViewBag.Offerings = await registration.GetServiceOfferingsAsync();
        ViewBag.SelectedIds = state.SelectedServiceIds;
        return View(StepVm(2, "What services do you offer?",
            "Select the types of work you want to receive through INDOR.",
            state, Url.Action(nameof(Categories))));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Services(string[]? serviceIds)
    {
        var state = await registration.GetAsync();
        state.SelectedServiceIds = serviceIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
        if (state.SelectedServiceIds.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Select at least one service.");
            ViewBag.Offerings = await registration.GetServiceOfferingsAsync();
            ViewBag.SelectedIds = state.SelectedServiceIds;
            return View(StepVm(2, "What services do you offer?", "Select at least one.", state, Url.Action(nameof(Categories))));
        }

        await registration.SaveAsync(state, 2);
        return RedirectToAction(nameof(Business));
    }

    [HttpGet]
    public async Task<IActionResult> Business()
    {
        var (state, redirect) = await RequireServicesAsync();
        if (redirect != null)
        {
            return redirect;
        }

        var user = await userManager.GetUserAsync(User);
        if (user != null && string.IsNullOrWhiteSpace(state!.Email))
        {
            state.Email = user.Email ?? "";
            state.PrimaryContact = string.IsNullOrWhiteSpace(state.PrimaryContact)
                ? UserDisplayName.Format(user)
                : state.PrimaryContact;
        }

        return View(StepVm(3, "Tell us about your business",
            "This information appears on your provider profile.",
            state!, Url.Action(nameof(Services))));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Business(ProviderRegistrationState posted)
    {
        var state = await registration.GetAsync();
        state.ProviderType = posted.ProviderType;
        state.BusinessName = posted.BusinessName?.Trim() ?? "";
        state.DbaName = posted.DbaName?.Trim() ?? "";
        state.PrimaryContact = posted.PrimaryContact?.Trim() ?? "";
        state.Phone = posted.Phone?.Trim() ?? "";
        state.Email = posted.Email?.Trim() ?? "";
        state.YearsExperience = posted.YearsExperience?.Trim() ?? "";
        state.LicenseNumber = posted.LicenseNumber?.Trim();

        if (string.IsNullOrWhiteSpace(state.PrimaryContact) || string.IsNullOrWhiteSpace(state.Email))
        {
            ModelState.AddModelError(string.Empty, "Contact name and email are required.");
            return View(StepVm(3, "Tell us about your business", "", state, Url.Action(nameof(Services))));
        }

        await registration.SaveAsync(state, 3);
        await registration.LinkCurrentUserAsync();

        return state.IsElectricianOnly
            ? RedirectToAction(nameof(Exam))
            : RedirectToAction(nameof(ServiceArea));
    }

    [HttpGet]
    public async Task<IActionResult> Exam(int page = 1)
    {
        var (state, redirect) = await RequireBusinessAsync();
        if (redirect != null)
        {
            return redirect;
        }

        if (!state!.IsElectricianOnly)
        {
            return RedirectToAction(nameof(ServiceArea));
        }

        page = Math.Max(1, page);
        var totalPages = await registration.GetExamTotalPagesAsync();
        if (page > totalPages)
        {
            page = totalPages;
        }

        ViewBag.ExamPage = page;
        ViewBag.ExamTotalPages = totalPages;
        ViewBag.Questions = await registration.GetExamPageQuestionsAsync(page);
        ViewBag.Answers = state.ExamAnswers;
        ViewBag.PassingPercent = await registration.GetExamPassingPercentAsync();
        ViewBag.ExamFailedScore = TempData["ExamFailedScore"];

        var backUrl = page > 1
            ? Url.Action(nameof(Exam), new { page = page - 1 })
            : Url.Action(nameof(Business));

        return View(StepVm(4, "INDOR electrical exam",
            $"Answer all questions. You need {ViewBag.PassingPercent}% to pass.",
            state, backUrl));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Exam(int page, Dictionary<int, string>? answers)
    {
        var state = await registration.GetAsync();
        if (!state.IsElectricianOnly)
        {
            return RedirectToAction(nameof(ServiceArea));
        }

        var pageAnswers = answers ?? new Dictionary<int, string>();
        await registration.SaveExamPageAnswersAsync(page, pageAnswers);

        var totalPages = await registration.GetExamTotalPagesAsync();
        if (page < totalPages)
        {
            return RedirectToAction(nameof(Exam), new { page = page + 1 });
        }

        var (passed, score) = await registration.FinalizeExamAsync();
        state = await registration.GetAsync();
        state.ExamScorePercent = score;
        state.ExamPassed = passed;
        await registration.SaveAsync(state, 4);

        if (!passed)
        {
            TempData["ExamFailedScore"] = score;
            return RedirectToAction(nameof(Exam), new { page = 1 });
        }

        return RedirectToAction(nameof(Scope));
    }

    [HttpGet]
    public async Task<IActionResult> Scope()
    {
        var (state, redirect) = await RequireBusinessAsync();
        if (redirect != null)
        {
            return redirect;
        }

        if (!state!.IsElectricianOnly || state.ExamPassed != true)
        {
            return RedirectToAction(nameof(Exam));
        }

        ViewBag.Allowed = await registration.GetScopeAllowedAsync();
        ViewBag.Disallowed = await registration.GetScopeDisallowedAsync();
        return View(StepVm(5, "Review your provider scope",
            "Confirm the work you may accept through INDOR.",
            state, Url.Action(nameof(Exam), new { page = await registration.GetExamTotalPagesAsync() })));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Scope(bool scopeTradeUnderstood, bool scopeStandardsAgreed)
    {
        var state = await registration.GetAsync();
        if (!scopeTradeUnderstood || !scopeStandardsAgreed)
        {
            ModelState.AddModelError(string.Empty, "You must agree to continue.");
            ViewBag.Allowed = await registration.GetScopeAllowedAsync();
            ViewBag.Disallowed = await registration.GetScopeDisallowedAsync();
            return View(StepVm(5, "Review your provider scope", "", state, Url.Action(nameof(Exam))));
        }

        state.ScopeTradeUnderstood = true;
        state.ScopeStandardsAgreed = true;
        state.ProfileSubmitted = true;
        await registration.SaveAsync(state, 5);
        await registration.CompleteRegistrationAsync(state);
        return RedirectToAction(nameof(Submitted));
    }

    [HttpGet]
    public async Task<IActionResult> ServiceArea()
    {
        var (state, redirect) = await RequireBusinessAsync();
        if (redirect != null)
        {
            return redirect;
        }

        if (state!.IsElectricianOnly)
        {
            return RedirectToAction(nameof(Exam));
        }

        return View(StepVm(4, "Service area",
            "Where do you want to receive jobs?",
            state, Url.Action(nameof(Business))));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ServiceArea(ProviderRegistrationState posted)
    {
        var state = await registration.GetAsync();
        state.PrimaryCity = posted.PrimaryCity?.Trim() ?? state.PrimaryCity;
        state.TravelRadiusMiles = posted.TravelRadiusMiles > 0 ? posted.TravelRadiusMiles : 25;
        state.EmergencyService = posted.EmergencyService;
        state.SameDayJobs = posted.SameDayJobs;
        await registration.SaveAsync(state, 4);
        return RedirectToAction(nameof(Documents));
    }

    [HttpGet]
    public async Task<IActionResult> Documents()
    {
        var (state, redirect) = await RequireBusinessAsync();
        if (redirect != null)
        {
            return redirect;
        }

        if (state!.IsElectricianOnly)
        {
            return RedirectToAction(nameof(Exam));
        }

        return View(StepVm(5, "Documents",
            "Upload required documents (coming soon — mark when ready).",
            state, Url.Action(nameof(ServiceArea))));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Documents(bool logoUploaded)
    {
        var state = await registration.GetAsync();
        state.LogoUploaded = logoUploaded;
        await registration.SaveAsync(state, 5);
        return RedirectToAction(nameof(Review));
    }

    [HttpGet]
    public async Task<IActionResult> Review()
    {
        var (state, redirect) = await RequireBusinessAsync();
        if (redirect != null)
        {
            return redirect;
        }

        if (state!.IsElectricianOnly)
        {
            return RedirectToAction(nameof(Scope));
        }

        return View(StepVm(6, "Review your profile",
            "Confirm your information before submitting.",
            state, Url.Action(nameof(Documents))));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Review")]
    public async Task<IActionResult> SubmitReview()
    {
        var state = await registration.GetAsync();
        state.ProfileSubmitted = true;
        await registration.CompleteRegistrationAsync(state);
        return RedirectToAction(nameof(Submitted));
    }

    [HttpGet]
    public async Task<IActionResult> Submitted()
    {
        var state = await registration.GetAsync();
        if (!state.ProfileSubmitted && state.ExamPassed != true)
        {
            return RedirectToAction(nameof(Categories));
        }

        var vm = StepVm(6, "Qualification submitted",
            "Your profile is being reviewed by INDOR.",
            state, null);
        vm.ShowSaveLater = false;
        return View(vm);
    }

    private async Task<(ProviderRegistrationState? State, IActionResult? Redirect)> RequireCategoriesAsync()
    {
        var state = await registration.GetAsync();
        if (state.SelectedCategoryIds.Count == 0)
        {
            return (null, RedirectToAction(nameof(Categories)));
        }

        return (state, null);
    }

    private async Task<(ProviderRegistrationState? State, IActionResult? Redirect)> RequireServicesAsync()
    {
        var state = await registration.GetAsync();
        if (state.SelectedCategoryIds.Count == 0)
        {
            return (null, RedirectToAction(nameof(Categories)));
        }

        if (state.SelectedServiceIds.Count == 0)
        {
            return (null, RedirectToAction(nameof(Services)));
        }

        return (state, null);
    }

    private async Task<(ProviderRegistrationState? State, IActionResult? Redirect)> RequireBusinessAsync()
    {
        var (state, redirect) = await RequireServicesAsync();
        return redirect != null ? (null, redirect) : (state, null);
    }

    private ProviderRegistrationStepViewModel StepVm(
        int step,
        string title,
        string subtitle,
        ProviderRegistrationState state,
        string? backUrl) =>
        new()
        {
            Step = step,
            Title = title,
            Subtitle = subtitle,
            State = state,
            BackUrl = backUrl,
        };
}
