using IndorMvcApp.Helpers;
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
public partial class ProviderRegistrationController(
    IProviderRegistrationService registration,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IWebHostEnvironment env) : Controller
{
    private static readonly string[] AllowedDocExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxDocumentBytes = 10_000_000;
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
            await signInManager.SignInAsync(user, isPersistent: true);
        }

        await next();
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Entry));

    private async Task<bool> ShouldBlockForExamAsync(ProviderRegistrationState state) =>
        state.UsesNewWizard
            ? state.ExamIsMandatory && state.ExamPassed != true
            : await registration.RequiresTradeExamAsync(state) && state.ExamPassed != true;

    private string ExamPassNextUrl(ProviderRegistrationState state) =>
        (state.UsesNewWizard
            ? Url.Action(nameof(ActivationCall))
            : state.IsElectricianOnly
                ? Url.Action(nameof(Scope))
                : Url.Action(nameof(Documents)))!;

    [HttpGet]
    public async Task<IActionResult> Categories()
    {
        await registration.LinkCurrentUserAsync();
        var state = await registration.GetAsync();
        ViewBag.Categories = await registration.GetCategoriesAsync();
        ViewBag.SelectedIds = state.SelectedCategoryIds;
        var subtitle = state.IsHvacOnly
            ? "Choose one primary trade for your company. INDOR providers can only qualify for one main licensed category."
            : "Choose one specialty only. INDOR providers can only apply for the trade they are licensed or qualified to perform.";

        return View(StepVm(1, "What type of provider are you?", subtitle,
            state, Url.Action("Welcome", "Account")));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Categories(string? categoryId, string[]? categoryIds)
    {
        var state = await registration.GetAsync();
        var picked = !string.IsNullOrWhiteSpace(categoryId)
            ? [categoryId.Trim()]
            : categoryIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Take(1).ToList() ?? [];

        state.SelectedCategoryIds = picked;
        state.SelectedServiceIds = [];
        state.ExamAnswers.Clear();
        state.ExamPassed = null;
        state.ExamScorePercent = 0;

        if (state.SelectedCategoryIds.Count != 1)
        {
            ModelState.AddModelError(string.Empty, "Choose exactly one provider category.");
            ViewBag.Categories = await registration.GetCategoriesAsync();
            ViewBag.SelectedIds = state.SelectedCategoryIds;
            return View(StepVm(1, "What type of provider are you?",
                "Choose one specialty only.", state, Url.Action(nameof(Categories))));
        }

        await registration.SaveAsync(state, 1);
        return state.UsesServicesFirstFlow
            ? RedirectToAction(nameof(Services))
            : RedirectToAction(nameof(Business));
    }

    [HttpGet]
    public async Task<IActionResult> Services()
    {
        ProviderRegistrationState? state;
        IActionResult? redirect;
        int step;
        string backUrl;
        string title;
        string subtitle;

        var flowState = await registration.GetAsync();
        if (flowState is { UsesServicesFirstFlow: true })
        {
            (state, redirect) = await RequireCategoriesAsync();
            step = 2;
            backUrl = Url.Action(nameof(Categories))!;
            if (flowState.IsBathroomOnly)
            {
                title = "What bathroom remodeling services do you provide?";
                subtitle = "Select the bathroom remodeling work your company is qualified to perform.";
            }
            else if (flowState.IsHandymanOnly)
            {
                title = "What handyman services do you offer?";
                subtitle = "Choose the work types you are qualified to perform within the handyman category.";
            }
            else
            {
                title = "What HVAC services do you provide?";
                subtitle = "Choose the HVAC services your company is licensed or trained to perform.";
            }
        }
        else
        {
            (state, redirect) = await RequireBusinessCompletedAsync();
            step = 3;
            backUrl = Url.Action(nameof(Business))!;
            if (state?.IsRoofingOnly == true)
            {
                title = "What roofing services do you provide?";
                subtitle = "Select the roofing specialties your company is qualified to perform.";
            }
            else if (state?.IsKitchenOnly == true)
            {
                title = "What kitchen remodeling services do you provide?";
                subtitle = "Select the services your company is qualified to perform within kitchen remodeling.";
            }
            else if (state?.IsPaintingOnly == true)
            {
                title = "What painting services do you provide?";
                subtitle = "Choose the services your painting business is qualified to perform.";
            }
            else if (state?.IsFlooringOnly == true)
            {
                title = "What flooring services do you provide?";
                subtitle = "Choose the flooring services your business performs.";
            }
            else if (state?.IsCleaningOnly == true)
            {
                title = "What cleaning services do you offer?";
                subtitle = "Choose the cleaning services your business is qualified to provide.";
            }
            else if (state?.IsLandscapingOnly == true)
            {
                title = "What landscaping services do you provide?";
                subtitle = "Choose the landscaping services your business offers.";
            }
            else if (state?.IsPestOnly == true)
            {
                title = "What pest control services do you offer?";
                subtitle = "Choose the services your company is qualified to provide.";
            }
            else if (state?.IsApplianceOnly == true)
            {
                title = "Which appliances do you repair?";
                subtitle = "Choose the appliance types your business is qualified to service.";
            }
            else if (state?.IsConstructionOnly == true)
            {
                title = "What construction work do you perform?";
                subtitle = "Choose the project types your company is qualified to handle.";
            }
            else
            {
                title = "What services do you offer?";
                subtitle = state?.IsPlumbingOnly == true
                    ? "Select the plumbing work you want to receive through INDOR."
                    : "Select the types of work you want to receive through INDOR.";
            }
        }

        if (redirect != null)
        {
            return redirect;
        }

        ViewBag.Offerings = await registration.GetServiceOfferingsForTradeAsync();
        ViewBag.SelectedIds = state!.SelectedServiceIds;
        ViewBag.IsHvac = state.IsHvacOnly;
        ViewBag.IsHandyman = state.IsHandymanOnly;
        ViewBag.IsConstruction = state.IsConstructionOnly;
        ViewBag.IsBathroom = state.IsBathroomOnly;
        ViewBag.IsKitchen = state.IsKitchenOnly;
        ViewBag.IsRoofing = state.IsRoofingOnly;
        ViewBag.IsPainting = state.IsPaintingOnly;
        ViewBag.IsFlooring = state.IsFlooringOnly;
        ViewBag.IsCleaning = state.IsCleaningOnly;
        ViewBag.IsLandscaping = state.IsLandscapingOnly;
        ViewBag.IsPest = state.IsPestOnly;
        ViewBag.IsAppliance = state.IsApplianceOnly;
        ViewBag.TradeLabel = await registration.GetPrimaryTradeLabelAsync() ?? "your trade";

        return View(StepVm(step, title, subtitle, state, backUrl));
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
            ViewBag.Offerings = await registration.GetServiceOfferingsForTradeAsync();
            ViewBag.SelectedIds = state.SelectedServiceIds;
            ViewBag.IsHvac = state.IsHvacOnly;
            ViewBag.IsHandyman = state.IsHandymanOnly;
            ViewBag.IsConstruction = state.IsConstructionOnly;
            ViewBag.IsBathroom = state.IsBathroomOnly;
            ViewBag.IsKitchen = state.IsKitchenOnly;
            ViewBag.IsRoofing = state.IsRoofingOnly;
            ViewBag.IsPainting = state.IsPaintingOnly;
            ViewBag.IsFlooring = state.IsFlooringOnly;
        ViewBag.IsCleaning = state.IsCleaningOnly;
        ViewBag.IsLandscaping = state.IsLandscapingOnly;
        ViewBag.IsPest = state.IsPestOnly;
        ViewBag.IsAppliance = state.IsApplianceOnly;
            var back = state.UsesServicesFirstFlow ? Url.Action(nameof(Categories)) : Url.Action(nameof(Business));
            return View(StepVm(state.UsesServicesFirstFlow ? 2 : 3, "What services do you offer?", "Select at least one.", state, back));
        }

        await registration.SaveAsync(state, state.UsesServicesFirstFlow ? 2 : 3);

        if (state.UsesServicesFirstFlow)
        {
            return RedirectToAction(nameof(Business));
        }

        if (await ShouldBlockForExamAsync(state))
        {
            return state.UsesExamIntroFlow
                ? RedirectToAction(nameof(ExamIntro))
                : RedirectToAction(nameof(Exam));
        }

        return state.UsesNewWizard
            ? RedirectToAction(nameof(ActivationCall))
            : RedirectToAction(nameof(Documents));
    }

    [HttpGet]
    public async Task<IActionResult> Business()
    {
        ProviderRegistrationState? state;
        IActionResult? redirect;
        int step;
        string backUrl;

        if (await registration.GetAsync() is { UsesServicesFirstFlow: true })
        {
            (state, redirect) = await RequireServicesSelectedAsync();
            step = 3;
            backUrl = Url.Action(nameof(Services))!;
        }
        else
        {
            (state, redirect) = await RequireCategoriesAsync();
            step = 2;
            backUrl = Url.Action(nameof(Categories))!;
        }

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

        if (state.IsPlumbingOnly &&
            (string.IsNullOrWhiteSpace(state.ProviderType) ||
             state.ProviderType is "Company" or "Individual"))
        {
            state.ProviderType = "MasterPlumber";
        }

        ViewBag.TradeLabel = await registration.GetPrimaryTradeLabelAsync() ?? "your trade";
        ViewBag.IsPlumbing = state.IsPlumbingOnly;
        ViewBag.IsHvac = state.IsHvacOnly;
        ViewBag.IsHandyman = state.IsHandymanOnly;
        ViewBag.IsConstruction = state.IsConstructionOnly;
        ViewBag.IsBathroom = state.IsBathroomOnly;
        ViewBag.IsKitchen = state.IsKitchenOnly;
        ViewBag.IsRoofing = state.IsRoofingOnly;
        ViewBag.IsPainting = state.IsPaintingOnly;
        ViewBag.IsFlooring = state.IsFlooringOnly;
        ViewBag.IsCleaning = state.IsCleaningOnly;
        ViewBag.IsLandscaping = state.IsLandscapingOnly;
        ViewBag.IsPest = state.IsPestOnly;
        ViewBag.IsAppliance = state.IsApplianceOnly;
        state.ServiceZipCodes = state.ServiceZipCodesDisplay;

        if (state.IsConstructionOnly)
        {
            state.ProviderType = "ConstructionCompany";
        }

        if (state.IsBathroomOnly)
        {
            state.ProviderType = "BathroomRemodeler";
        }

        if (state.IsKitchenOnly)
        {
            state.ProviderType = "KitchenRemodeler";
        }

        if (state.IsRoofingOnly)
        {
            state.ProviderType = "RoofingContractor";
        }

        if (state.IsPaintingOnly)
        {
            state.ProviderType = "PaintingContractor";
        }

        if (state.IsFlooringOnly)
        {
            state.ProviderType = "FlooringContractor";
        }

        if (state.IsCleaningOnly)
        {
            state.ProviderType = "CleaningCompany";
        }

        if (state.IsLandscapingOnly)
        {
            state.ProviderType = "LandscapingCompany";
        }

        if (state.IsPestOnly)
        {
            state.ProviderType = "PestControlCompany";
        }

        if (state.IsApplianceOnly)
        {
            state.ProviderType = "ApplianceRepairCompany";
        }

        string title;
        string subtitle;
        if (state.IsRoofingOnly)
        {
            title = "Tell us about your roofing business";
            subtitle = "Use your legal business information to create your provider profile.";
        }
        else if (state.IsKitchenOnly)
        {
            title = "Tell us about your kitchen remodeling business";
            subtitle = "Complete your provider profile to continue.";
        }
        else if (state.IsPaintingOnly)
        {
            title = "Tell us about your painting business";
            subtitle = "Complete your provider profile to continue.";
        }
        else if (state.IsFlooringOnly)
        {
            title = "Tell us about your flooring business";
            subtitle = "Add your company details to continue.";
        }
        else if (state.IsCleaningOnly)
        {
            title = "Tell us about your cleaning business";
            subtitle = "Enter the core details for your cleaning provider profile.";
        }
        else if (state.IsLandscapingOnly)
        {
            title = "Tell us about your landscaping business";
            subtitle = "Add your business details to continue.";
        }
        else if (state.IsPestOnly)
        {
            title = "Tell us about your pest control business";
            subtitle = "Add your company details to start the qualification process.";
        }
        else if (state.IsApplianceOnly)
        {
            title = "Tell us about your appliance repair business";
            subtitle = "Enter your company details to start your appliance repair application.";
        }
        else if (state.IsBathroomOnly)
        {
            title = "Tell us about your bathroom remodeling business";
            subtitle = "Enter your company details so INDOR can review your application.";
        }
        else if (state.IsConstructionOnly)
        {
            title = "Tell us about your company";
            subtitle = "Add your business information for construction company verification.";
        }
        else if (state.IsHandymanOnly)
        {
            title = "Tell us about your handyman business";
            subtitle = "Add your company and service details.";
        }
        else if (state.IsHvacOnly)
        {
            title = "Tell us about your HVAC business";
            subtitle = "Add your business details so INDOR can review your profile.";
        }
        else if (state.IsPlumbingOnly)
        {
            title = "Tell us about your plumbing business";
            subtitle = "Your profile will be reviewed for plumbing jobs only.";
        }
        else
        {
            var tradeLabel = ViewBag.TradeLabel as string ?? "your trade";
            title = $"Tell us about your {tradeLabel.ToString()!.ToLowerInvariant()} business";
            subtitle = "This information appears on your provider profile.";
        }

        return View(StepVm(step, title, subtitle, state, backUrl));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Business(
        ProviderRegistrationState posted,
        string[]? serviceAreas)
    {
        var state = await registration.GetAsync();
        state.ProviderType = posted.ProviderType;
        state.BusinessName = posted.BusinessName?.Trim() ?? "";
        state.DbaName = posted.DbaName?.Trim() ?? "";
        state.PrimaryContact = posted.PrimaryContact?.Trim() ?? "";
        state.Phone = posted.Phone?.Trim() ?? "";
        state.Email = posted.Email?.Trim() ?? "";
        state.BusinessAddress = posted.BusinessAddress?.Trim() ?? "";
        state.YearsExperience = posted.YearsExperience?.Trim() ?? "";
        state.LicenseNumber = posted.LicenseNumber?.Trim();
        state.EpaCertificationNumber = posted.EpaCertificationNumber?.Trim();
        state.BackgroundCheckConsent = posted.BackgroundCheckConsent;
        state.ServiceDescription = posted.ServiceDescription?.Trim() ?? "";
        state.IsInsured = posted.IsInsured;
        state.IsLicensed = posted.IsLicensed;
        state.EmergencyService = posted.EmergencyService;
        state.ServiceZipCodes = posted.ServiceZipCodes?.Trim() ?? "";
        state.TeamSize = posted.TeamSize?.Trim() ?? "";
        if (posted.TravelRadiusMiles > 0)
        {
            state.TravelRadiusMiles = posted.TravelRadiusMiles;
        }

        if (serviceAreas is { Length: > 0 })
        {
            state.ZipOrNeighborhoods = serviceAreas
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            state.ServiceZipCodes = string.Join(", ", state.ZipOrNeighborhoods);
            state.PrimaryCity = state.ZipOrNeighborhoods[0];
        }

        if (!string.IsNullOrWhiteSpace(state.ServiceZipCodes))
        {
            state.ZipOrNeighborhoods = state.ServiceZipCodes
                .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (string.IsNullOrWhiteSpace(state.BusinessName) ||
            string.IsNullOrWhiteSpace(state.PrimaryContact) ||
            string.IsNullOrWhiteSpace(state.Email))
        {
            ModelState.AddModelError(string.Empty, "Business name, contact name, and email are required.");
            ViewBag.IsPlumbing = state.IsPlumbingOnly;
            ViewBag.IsHvac = state.IsHvacOnly;
            ViewBag.IsHandyman = state.IsHandymanOnly;
            ViewBag.IsConstruction = state.IsConstructionOnly;
            ViewBag.IsBathroom = state.IsBathroomOnly;
            ViewBag.IsKitchen = state.IsKitchenOnly;
            ViewBag.IsRoofing = state.IsRoofingOnly;
            ViewBag.IsPainting = state.IsPaintingOnly;
            ViewBag.IsFlooring = state.IsFlooringOnly;
        ViewBag.IsCleaning = state.IsCleaningOnly;
        ViewBag.IsLandscaping = state.IsLandscapingOnly;
        ViewBag.IsPest = state.IsPestOnly;
        ViewBag.IsAppliance = state.IsApplianceOnly;
            return View(StepVm(state.UsesServicesFirstFlow ? 3 : 2, "Tell us about your business", "", state,
                state.UsesServicesFirstFlow ? Url.Action(nameof(Services))! : Url.Action(nameof(Categories))!));
        }

        if (!UsPhoneOptionalAttribute.IsValidOptional(posted.Phone))
        {
            ModelState.AddModelError(string.Empty,
                "Enter a valid 10-digit US phone number (e.g. 555 123 4567).");
            ViewBag.IsPlumbing = state.IsPlumbingOnly;
            ViewBag.IsHvac = state.IsHvacOnly;
            ViewBag.IsHandyman = state.IsHandymanOnly;
            ViewBag.IsConstruction = state.IsConstructionOnly;
            ViewBag.IsBathroom = state.IsBathroomOnly;
            ViewBag.IsKitchen = state.IsKitchenOnly;
            ViewBag.IsRoofing = state.IsRoofingOnly;
            ViewBag.IsPainting = state.IsPaintingOnly;
            ViewBag.IsFlooring = state.IsFlooringOnly;
            ViewBag.IsCleaning = state.IsCleaningOnly;
            ViewBag.IsLandscaping = state.IsLandscapingOnly;
            ViewBag.IsPest = state.IsPestOnly;
            ViewBag.IsAppliance = state.IsApplianceOnly;
            return View(StepVm(state.UsesServicesFirstFlow ? 3 : 2, "Tell us about your business", "", state,
                state.UsesServicesFirstFlow ? Url.Action(nameof(Services))! : Url.Action(nameof(Categories))!));
        }

        state.Phone = UsPhoneOptionalAttribute.NormalizeToStorage(posted.Phone) ?? "";

        if (state.IsHandymanOnly && string.IsNullOrWhiteSpace(state.ServiceZipCodes))
        {
            state.PrimaryCity = posted.PrimaryCity?.Trim() ?? state.PrimaryCity;
        }
        else if (!string.IsNullOrWhiteSpace(state.ServiceZipCodes))
        {
            state.PrimaryCity = state.ServiceZipCodes;
        }

        if (state.IsHvacOnly && !state.BackgroundCheckConsent)
        {
            ModelState.AddModelError(string.Empty, "Background check consent is required to continue.");
            ViewBag.IsHvac = true;
            return View(StepVm(3, "Tell us about your HVAC business", "", state, Url.Action(nameof(Services))!));
        }

        await registration.SaveAsync(state, state.UsesServicesFirstFlow ? 3 : 2);
        await registration.LinkCurrentUserAsync();

        if (state.IsElectricianOnly)
        {
            return RedirectToAction(nameof(Exam));
        }

        if (state.UsesExamIntroFlow)
        {
            return RedirectToAction(nameof(ExamIntro));
        }

        if (state.UsesBusinessBeforeServicesFlow)
        {
            return RedirectToAction(nameof(Services));
        }

        return RedirectToAction(nameof(Services));
    }

    [HttpGet]
    public async Task<IActionResult> ExamIntro()
    {
        var (readyState, redirect) = await RequireExamReadyAsync();
        if (redirect != null)
        {
            return redirect;
        }

        var state = readyState!;

        if (!state.UsesExamIntroFlow)
        {
            return RedirectToAction(nameof(Exam));
        }

        if (state.ExamPassed == true)
        {
            return state.UsesNewWizard
                ? RedirectToAction(nameof(ActivationCall))
                : RedirectToAction(nameof(Documents));
        }

        var (introTitle, introSubtitle) = GetExamIntroCopy(state);
        ViewBag.IsHandyman = state.IsHandymanOnly;
        ViewBag.IsHvac = state.IsHvacOnly;
        ViewBag.IsConstruction = state.IsConstructionOnly;
        ViewBag.IsBathroom = state.IsBathroomOnly;
        ViewBag.IsKitchen = state.IsKitchenOnly;
        ViewBag.IsRoofing = state.IsRoofingOnly;
        ViewBag.IsPainting = state.IsPaintingOnly;
        ViewBag.IsFlooring = state.IsFlooringOnly;
        ViewBag.IsCleaning = state.IsCleaningOnly;
        ViewBag.IsLandscaping = state.IsLandscapingOnly;
        ViewBag.IsPest = state.IsPestOnly;
        ViewBag.IsAppliance = state.IsApplianceOnly;

        var introBack = GetExamIntroBackUrl(state);

        return View(StepVm(4, introTitle, introSubtitle, state, introBack));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExamIntro(string? action)
    {
        var (readyState, redirect) = await RequireExamReadyAsync();
        if (redirect != null)
        {
            return redirect;
        }

        var state = readyState!;
        state.ExamIntroAcknowledged = true;
        await registration.SaveAsync(state, 4);

        var (introTitle, introSubtitle) = GetExamIntroCopy(state);

        if (string.Equals(action, "review", StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.IsHandyman = state.IsHandymanOnly;
            ViewBag.IsHvac = state.IsHvacOnly;
            ViewBag.IsConstruction = state.IsConstructionOnly;
            ViewBag.IsBathroom = state.IsBathroomOnly;
            ViewBag.IsKitchen = state.IsKitchenOnly;
            ViewBag.IsRoofing = state.IsRoofingOnly;
            ViewBag.IsPainting = state.IsPaintingOnly;
            ViewBag.IsFlooring = state.IsFlooringOnly;
        ViewBag.IsCleaning = state.IsCleaningOnly;
        ViewBag.IsLandscaping = state.IsLandscapingOnly;
        ViewBag.IsPest = state.IsPestOnly;
        ViewBag.IsAppliance = state.IsApplianceOnly;
            var introBack = GetExamIntroBackUrl(state);
            return View(StepVm(4, introTitle, introSubtitle, state, introBack));
        }

        return RedirectToAction(nameof(Exam), new { page = 1 });
    }

    [HttpGet]
    public async Task<IActionResult> Exam(int page = 1)
    {
        var (state, redirect) = await RequireExamReadyAsync();
        if (redirect != null)
        {
            return redirect;
        }

        if (!await registration.RequiresTradeExamAsync(state))
        {
            return RedirectToAction(nameof(Documents));
        }

        var tradeCode = await registration.ResolveTradeCodeAsync(state);
        page = Math.Max(1, page);
        var totalPages = await registration.GetExamTotalPagesAsync(tradeCode);
        if (page > totalPages)
        {
            page = totalPages;
        }

        var tradeLabel = await registration.GetPrimaryTradeLabelAsync() ?? "trade";
        var questionCount = await registration.GetExamQuestionCountAsync(tradeCode);

        ViewBag.ExamPage = page;
        ViewBag.ExamTotalPages = totalPages;
        ViewBag.ExamQuestionCount = questionCount;
        ViewBag.Questions = await registration.GetExamPageQuestionsAsync(page, tradeCode);
        ViewBag.Answers = state!.ExamAnswers;
        ViewBag.PassingPercent = await registration.GetExamPassingPercentAsync();
        ViewBag.ExamFailedScore = TempData["ExamFailedScore"];
        ViewBag.TradeLabel = tradeLabel;

        var questionStart = (page - 1) * 4 + 1;
        var questionEnd = Math.Min(page * 4, questionCount);
        ViewBag.QuestionRangeStart = questionStart;
        ViewBag.QuestionRangeEnd = questionEnd;

        string backUrl;
        if (page > 1)
        {
            backUrl = Url.Action(nameof(Exam), new { page = page - 1 })!;
        }
        else if (state.UsesExamIntroFlow)
        {
            backUrl = Url.Action(nameof(ExamIntro))!;
        }
        else if (state.IsElectricianOnly)
        {
            backUrl = Url.Action(nameof(Business))!;
        }
        else
        {
            backUrl = Url.Action(nameof(Services))!;
        }

        var examTitle = state.IsPlumbingOnly
            ? "INDOR plumbing exam"
            : state.IsHvacOnly
                ? "INDOR HVAC qualification exam"
                : state.IsHandymanOnly
                    ? "INDOR handyman exam"
                    : state.IsConstructionOnly
                        ? "INDOR construction qualification exam"
                        : state.IsBathroomOnly
                            ? "INDOR bathroom remodeling exam"
                            : state.IsKitchenOnly
                                ? "INDOR kitchen remodeling exam"
                                : state.IsRoofingOnly
                                    ? "INDOR roofing qualification"
                                    : state.IsPaintingOnly
                                        ? "INDOR painting qualification exam"
                                        : state.IsFlooringOnly
                                            ? "INDOR flooring qualification exam"
                                            : state.IsCleaningOnly
                                                ? "INDOR cleaning qualification"
                                                : state.IsLandscapingOnly
                                                    ? "INDOR landscaping exam"
                                                    : state.IsPestOnly
                                                        ? "INDOR pest control exam"
                                                        : state.IsApplianceOnly
                                                            ? "INDOR appliance repair qualification"
                                                            : $"INDOR {tradeLabel.ToLowerInvariant()} exam";

        var examSubtitle = state.IsApplianceOnly
            ? "Pass the trade qualification to unlock appliance repair jobs only."
            : state.IsPestOnly
            ? "Answer the qualification questions to unlock pest control jobs only."
            : state.IsLandscapingOnly
            ? "Pass the trade qualification to unlock landscaping jobs only."
            : state.IsCleaningOnly
            ? "Pass the trade qualification to unlock cleaning jobs only."
            : state.IsFlooringOnly
            ? "Answer the questions below to continue."
            : state.IsPaintingOnly
            ? "Answer the questions below to continue your painting qualification."
            : state.IsHandymanOnly || state.IsConstructionOnly
                ? "Answer the questions below to continue your qualification."
                : state.IsRoofingOnly
                    ? "Answer the roofing trade questions to continue your qualification."
                    : state.IsBathroomOnly
                        ? "Pass the trade qualification to unlock bathroom remodeling jobs only."
                        : state.IsKitchenOnly
                            ? "Pass the trade qualification to unlock kitchen remodeling jobs only."
                            : state.IsHvacOnly
                                ? "Pass the trade qualification to unlock HVAC jobs only."
                                : $"Pass the trade qualification to unlock {tradeLabel.ToLowerInvariant()} jobs only.";

        ViewBag.IsHandyman = state.IsHandymanOnly;
        ViewBag.IsConstruction = state.IsConstructionOnly;
        ViewBag.IsBathroom = state.IsBathroomOnly;
        ViewBag.IsKitchen = state.IsKitchenOnly;
        ViewBag.IsRoofing = state.IsRoofingOnly;
        ViewBag.IsPainting = state.IsPaintingOnly;
        ViewBag.IsFlooring = state.IsFlooringOnly;
        ViewBag.IsCleaning = state.IsCleaningOnly;
        ViewBag.IsLandscaping = state.IsLandscapingOnly;
        ViewBag.IsPest = state.IsPestOnly;
        ViewBag.IsAppliance = state.IsApplianceOnly;

        return View(StepVm(4, examTitle, examSubtitle, state, backUrl));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Exam(int page, Dictionary<int, string>? answers)
    {
        var state = await registration.GetAsync();
        if (!await registration.RequiresTradeExamAsync(state))
        {
            return RedirectToAction(nameof(Documents));
        }

        var tradeCode = await registration.ResolveTradeCodeAsync(state);
        var pageAnswers = answers ?? new Dictionary<int, string>();
        await registration.SaveExamPageAnswersAsync(page, pageAnswers, tradeCode);

        var totalPages = await registration.GetExamTotalPagesAsync(tradeCode);
        if (page < totalPages)
        {
            return RedirectToAction(nameof(Exam), new { page = page + 1 });
        }

        var (passed, score) = await registration.FinalizeExamAsync(tradeCode);
        state = await registration.GetAsync();
        state.ExamScorePercent = score;
        state.ExamPassed = passed;
        await registration.SaveAsync(state, 4);

        return RedirectToAction(nameof(Result));
    }

    [HttpGet]
    public async Task<IActionResult> Result()
    {
        var state = await registration.GetAsync();
        if (!await registration.RequiresTradeExamAsync(state))
        {
            return RedirectToAction(nameof(Documents));
        }

        if (state.ExamPassed is null)
        {
            return RedirectToAction(nameof(Exam), new { page = 1 });
        }

        var tradeCode = await registration.ResolveTradeCodeAsync(state);
        var result = await registration.GetExamResultAsync(tradeCode);

        ViewBag.ExamResult = result;
        ViewBag.RetryUrl = Url.Action(nameof(Exam), new { page = 1 });
        ViewBag.ContinueUrl = ExamPassNextUrl(state);

        var title = result.Passed ? "You passed the exam" : "You did not pass";
        var subtitle = result.Passed
            ? $"You scored {result.ScorePercent}%. Here is the breakdown of every question."
            : $"You scored {result.ScorePercent}%. You need {result.PassingPercent}% to pass. Review every question and try again.";

        return View(StepVm(4, title, subtitle, state, null));
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

        if (await ShouldBlockForExamAsync(state))
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

        if (await ShouldBlockForExamAsync(state))
        {
            return RedirectToAction(nameof(Exam));
        }

        await registration.EnsureDocumentSlotsAsync();
        ViewBag.DocumentSlots = await registration.GetDocumentSlotsAsync();
        ViewBag.TradeLabel = await registration.GetPrimaryTradeLabelAsync() ?? "trade";

        var docSubtitle = state.IsPlumbingOnly
            ? "Your plumbing application will be reviewed using the documents below."
            : state.IsHvacOnly
                ? "Submit your credentials so INDOR can verify your HVAC company."
                : state.IsHandymanOnly
                    ? "Help us verify your handyman business."
                    : state.IsConstructionOnly
                        ? "We need your documents to verify your construction company account."
                        : state.IsBathroomOnly
                            ? "Add the documents needed for bathroom remodeling approval."
                            : state.IsKitchenOnly
                                ? "Add the documents needed for kitchen remodeling approval."
                                : state.IsPaintingOnly
                                    ? "Submit the items needed to verify your painting provider account."
                                    : state.IsFlooringOnly
                                        ? "Required documents for flooring provider approval."
                                        : state.IsCleaningOnly
                                            ? "Submit the required files to complete your cleaning provider application."
                                            : state.IsLandscapingOnly
                                                ? "Add the required files to complete your landscaping application."
                                                : state.IsPestOnly
                                                    ? "Submit the documents needed to verify your pest control business."
                                                    : state.IsApplianceOnly
                                                        ? "Submit the required documents to verify your appliance repair business."
                                                        : state.IsRoofingOnly
                                        ? "Submit the required files so INDOR can verify your company."
                                        : "Upload required documents (PDF or image).";

        var documentsBack = state.UsesServicesFirstFlow
            ? Url.Action(nameof(Business))!
            : Url.Action(nameof(Services))!;
        if (await registration.RequiresTradeExamAsync(state) && state.ExamPassed == true)
        {
            var tradeCode = await registration.ResolveTradeCodeAsync(state);
            var lastPage = await registration.GetExamTotalPagesAsync(tradeCode);
            documentsBack = Url.Action(nameof(Exam), new { page = lastPage })!;
        }

        ViewBag.IsHvac = state.IsHvacOnly;
        ViewBag.IsHandyman = state.IsHandymanOnly;
        ViewBag.IsConstruction = state.IsConstructionOnly;
        ViewBag.IsPlumbing = state.IsPlumbingOnly;

        ViewBag.IsBathroom = state.IsBathroomOnly;
        ViewBag.IsKitchen = state.IsKitchenOnly;
        ViewBag.IsRoofing = state.IsRoofingOnly;
        ViewBag.IsPainting = state.IsPaintingOnly;
        ViewBag.IsFlooring = state.IsFlooringOnly;
        ViewBag.IsCleaning = state.IsCleaningOnly;
        ViewBag.IsLandscaping = state.IsLandscapingOnly;
        ViewBag.IsPest = state.IsPestOnly;
        ViewBag.IsAppliance = state.IsApplianceOnly;

        var docTitle = state.IsPaintingOnly || state.IsFlooringOnly || state.IsCleaningOnly || state.IsLandscapingOnly || state.IsPestOnly || state.IsApplianceOnly
            ? "Upload your documents"
            : state.IsRoofingOnly
                ? "Upload your roofing documents"
                : state.IsBathroomOnly || state.IsKitchenOnly
                    ? "Upload your supporting documents"
                    : state.IsHandymanOnly || state.IsConstructionOnly
                        ? "Upload documents"
                        : "Upload required documents";

        return View(StepVm(5, docTitle, docSubtitle, state, documentsBack));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Documents(
        IFormFile? licenseFile,
        IFormFile? insuranceFile,
        IFormFile? logoFile,
        IFormFile? plumbingLicenseFile,
        IFormFile? photoIdFile,
        IFormFile? businessRegistrationFile,
        IFormFile? w9File,
        IFormFile? tradeCertsFile,
        IFormFile? portfolioFile,
        IFormFile? hvacLicenseFile,
        IFormFile? epaCertFile,
        IFormFile? liabilityInsuranceFile,
        IFormFile? governmentIdFile,
        IFormFile? workPhotosFile,
        IFormFile? referencesFile,
        IFormFile? contractorLicenseFile,
        IFormFile? roofingLicenseFile,
        IFormFile? paintingProjectPhotosFile,
        IFormFile? flooringProjectPhotosFile,
        IFormFile? cleaningWorkPhotosFile,
        IFormFile? pestControlLicenseFile,
        IFormFile? applianceRepairCertificationFile,
        string? action)
    {
        var state = await registration.GetAsync();

        if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
        {
            await SaveDocumentFileAsync(roofingLicenseFile, ProviderDocumentTypes.RoofingLicense);
            await SaveDocumentFileAsync(plumbingLicenseFile, ProviderDocumentTypes.PlumbingLicense);
            await SaveDocumentFileAsync(photoIdFile, ProviderDocumentTypes.PhotoId);
            await SaveDocumentFileAsync(licenseFile, ProviderDocumentTypes.License);
            await SaveDocumentFileAsync(insuranceFile, ProviderDocumentTypes.Insurance);
            await SaveDocumentFileAsync(liabilityInsuranceFile, ProviderDocumentTypes.LiabilityInsurance);
            await SaveDocumentFileAsync(businessRegistrationFile, ProviderDocumentTypes.BusinessRegistration);
            await SaveDocumentFileAsync(w9File, ProviderDocumentTypes.W9);
            await SaveDocumentFileAsync(tradeCertsFile, ProviderDocumentTypes.TradeCerts);
            await SaveDocumentFileAsync(portfolioFile, ProviderDocumentTypes.Portfolio);
            await SaveDocumentFileAsync(logoFile, ProviderDocumentTypes.Logo);
            await SaveDocumentFileAsync(hvacLicenseFile, ProviderDocumentTypes.HvacLicense);
            await SaveDocumentFileAsync(epaCertFile, ProviderDocumentTypes.EpaCertification);
            await SaveDocumentFileAsync(governmentIdFile, ProviderDocumentTypes.GovernmentId);
            await SaveDocumentFileAsync(workPhotosFile, ProviderDocumentTypes.WorkPhotos);
            await SaveDocumentFileAsync(referencesFile, ProviderDocumentTypes.References);
            await SaveDocumentFileAsync(contractorLicenseFile, ProviderDocumentTypes.ContractorLicense);
            await SaveDocumentFileAsync(paintingProjectPhotosFile, ProviderDocumentTypes.PaintingProjectPhotos);
            await SaveDocumentFileAsync(flooringProjectPhotosFile, ProviderDocumentTypes.FlooringProjectPhotos);
            await SaveDocumentFileAsync(cleaningWorkPhotosFile, ProviderDocumentTypes.CleaningWorkPhotos);
            await SaveDocumentFileAsync(pestControlLicenseFile, ProviderDocumentTypes.PestControlLicense);
            await SaveDocumentFileAsync(applianceRepairCertificationFile, ProviderDocumentTypes.ApplianceRepairCertification);
            return RedirectToAction(nameof(Documents));
        }

        if (!await registration.HasRequiredDocumentsAsync())
        {
            ModelState.AddModelError(string.Empty, "Upload all required documents before continuing.");
            ViewBag.DocumentSlots = await registration.GetDocumentSlotsAsync();
            ViewBag.TradeLabel = await registration.GetPrimaryTradeLabelAsync();
            return View(StepVm(5, "Upload required documents", "", state,
                Url.Action(nameof(Services))));
        }

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

        if (state.UsesNewWizard)
        {
            return RedirectToAction(nameof(ActivationCall));
        }

        if (await ShouldBlockForExamAsync(state) &&
            !state.IsBathroomOnly &&
            !state.IsKitchenOnly &&
            !state.IsRoofingOnly &&
            !state.IsPaintingOnly &&
            !state.IsFlooringOnly &&
            !state.IsCleaningOnly &&
            !state.IsLandscapingOnly &&
            !state.IsPestOnly &&
            !state.IsApplianceOnly)
        {
            return RedirectToAction(nameof(Exam));
        }

        ViewBag.DocumentSlots = await registration.GetDocumentSlotsAsync();
        ViewBag.Categories = await registration.GetCategoriesAsync();
        ViewBag.Offerings = await registration.GetServiceOfferingsForTradeAsync();

        ViewBag.TradeLabel = await registration.GetPrimaryTradeLabelAsync() ?? "trade";
        ViewBag.ProviderTypeLabel = FormatProviderTypeLabel(state.ProviderType);

        var reviewTitle = state.IsHvacOnly
            ? "Review your HVAC application"
            : state.IsHandymanOnly
                ? "Review your handyman application"
                : state.IsRoofingOnly
                    ? "Review your roofing application"
                    : state.IsKitchenOnly
                        ? "Review your kitchen remodeling application"
                        : state.IsPaintingOnly
                            ? "Review your painting application"
                            : state.IsFlooringOnly
                                ? "Review your flooring application"
                                : state.IsCleaningOnly
                                    ? "Review your cleaning application"
                                    : state.IsLandscapingOnly
                                        ? "Review your landscaping application"
                                        : state.IsPestOnly
                                            ? "Review your pest control application"
                                            : state.IsApplianceOnly
                                                ? "Review your appliance repair application"
                                                : state.IsConstructionOnly
                                ? "Review your application"
                                : state.IsBathroomOnly
                                    ? "Review your bathroom remodeling application"
                                    : "Review & submit";
        var reviewSubtitle = state.IsHvacOnly
            ? "Confirm your information before submitting for INDOR approval."
            : state.IsApplianceOnly
                ? "Confirm your details before submitting for INDOR review."
                : state.IsBathroomOnly || state.IsKitchenOnly || state.IsRoofingOnly || state.IsPaintingOnly || state.IsFlooringOnly || state.IsCleaningOnly || state.IsLandscapingOnly || state.IsPestOnly
                ? state.IsRoofingOnly
                    ? "Check your information before sending it to INDOR for approval."
                    : "Confirm your information before submitting."
                : state.IsHandymanOnly || state.IsConstructionOnly
                    ? "Confirm everything before submission."
                    : "Confirm your information before submitting your application.";

        ViewBag.IsHandyman = state.IsHandymanOnly;
        ViewBag.IsHvac = state.IsHvacOnly;
        ViewBag.IsConstruction = state.IsConstructionOnly;
        ViewBag.IsBathroom = state.IsBathroomOnly;
        ViewBag.IsKitchen = state.IsKitchenOnly;
        ViewBag.IsRoofing = state.IsRoofingOnly;
        ViewBag.IsPainting = state.IsPaintingOnly;
        ViewBag.IsFlooring = state.IsFlooringOnly;
        ViewBag.IsCleaning = state.IsCleaningOnly;
        ViewBag.IsLandscaping = state.IsLandscapingOnly;
        ViewBag.IsPest = state.IsPestOnly;
        ViewBag.IsAppliance = state.IsApplianceOnly;

        return View(StepVm(6, reviewTitle, reviewSubtitle, state, Url.Action(nameof(Documents))));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Review")]
    public async Task<IActionResult> SubmitReview(bool confirmAccurate, bool agreeToReview = false)
    {
        var state = await registration.GetAsync();

        if (!confirmAccurate || (state.IsRoofingOnly && !agreeToReview))
        {
            ModelState.AddModelError(string.Empty, state.IsRoofingOnly && confirmAccurate
                ? "You must agree to INDOR provider review and verification."
                : "You must confirm that your information is accurate.");
            ViewBag.DocumentSlots = await registration.GetDocumentSlotsAsync();
            ViewBag.Categories = await registration.GetCategoriesAsync();
            ViewBag.Offerings = await registration.GetServiceOfferingsForTradeAsync();
            ViewBag.TradeLabel = await registration.GetPrimaryTradeLabelAsync();
            ViewBag.ProviderTypeLabel = FormatProviderTypeLabel(state.ProviderType);
            ViewBag.IsHandyman = state.IsHandymanOnly;
            ViewBag.IsHvac = state.IsHvacOnly;
            ViewBag.IsConstruction = state.IsConstructionOnly;
            ViewBag.IsBathroom = state.IsBathroomOnly;
            ViewBag.IsKitchen = state.IsKitchenOnly;
            ViewBag.IsRoofing = state.IsRoofingOnly;
            ViewBag.IsPainting = state.IsPaintingOnly;
            ViewBag.IsFlooring = state.IsFlooringOnly;
        ViewBag.IsCleaning = state.IsCleaningOnly;
        ViewBag.IsLandscaping = state.IsLandscapingOnly;
        ViewBag.IsPest = state.IsPestOnly;
        ViewBag.IsAppliance = state.IsApplianceOnly;
            var errTitle = state.IsHandymanOnly
                ? "Review your handyman application"
                : state.IsHvacOnly
                    ? "Review your HVAC application"
                    : state.IsRoofingOnly
                        ? "Review your roofing application"
                        : state.IsKitchenOnly
                            ? "Review your kitchen remodeling application"
                            : state.IsPaintingOnly
                                ? "Review your painting application"
                                : state.IsFlooringOnly
                                    ? "Review your flooring application"
                                : state.IsCleaningOnly
                                    ? "Review your cleaning application"
                                : state.IsLandscapingOnly
                                    ? "Review your landscaping application"
                                : state.IsPestOnly
                                    ? "Review your pest control application"
                                : state.IsApplianceOnly
                                    ? "Review your appliance repair application"
                                : state.IsBathroomOnly
                                    ? "Review your bathroom remodeling application"
                                    : state.IsConstructionOnly
                                        ? "Review your application"
                                        : "Review & submit";
            return View(StepVm(6, errTitle, "", state, Url.Action(nameof(Documents))));
        }

        if (await ShouldBlockForExamAsync(state))
        {
            return RedirectToAction(nameof(Exam));
        }

        if (!await registration.HasRequiredDocumentsAsync())
        {
            return RedirectToAction(nameof(Documents));
        }

        if (state.IsRoofingOnly)
        {
            state.ScopeStandardsAgreed = agreeToReview;
        }

        await registration.SubmitApplicationAsync(state);
        return RedirectToAction(nameof(Submitted));
    }

    [HttpGet]
    public async Task<IActionResult> Submitted()
    {
        var state = await registration.GetAsync();
        if (!state.ProfileSubmitted)
        {
            return RedirectToAction(nameof(Categories));
        }

        var vm = StepVm(6, "Qualification submitted",
            "Your profile is being reviewed by INDOR.",
            state, null);
        vm.ShowSaveLater = false;
        return View(vm);
    }

    private async Task<(ProviderRegistrationState? State, IActionResult? Redirect)> RequireServicesSelectedAsync()
    {
        var (state, redirect) = await RequireCategoriesAsync();
        if (redirect != null)
        {
            return (null, redirect);
        }

        if (state!.SelectedServiceIds.Count == 0)
        {
            return (null, RedirectToAction(nameof(Services)));
        }

        return (state, null);
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

    private async Task<(ProviderRegistrationState? State, IActionResult? Redirect)> RequireBusinessCompletedAsync()
    {
        var (state, redirect) = await RequireCategoriesAsync();
        if (redirect != null)
        {
            return (null, redirect);
        }

        if (string.IsNullOrWhiteSpace(state!.BusinessName) ||
            string.IsNullOrWhiteSpace(state.PrimaryContact) ||
            string.IsNullOrWhiteSpace(state.Email))
        {
            return (null, RedirectToAction(nameof(Business)));
        }

        return (state, null);
    }

    private async Task<(ProviderRegistrationState? State, IActionResult? Redirect)> RequireServicesCompletedAsync()
    {
        var (state, redirect) = await RequireBusinessCompletedAsync();
        if (redirect != null)
        {
            return (null, redirect);
        }

        if (state!.SelectedServiceIds.Count == 0 && !state.IsElectricianOnly)
        {
            return (null, RedirectToAction(nameof(Services)));
        }

        return (state, null);
    }

    private async Task<(ProviderRegistrationState? State, IActionResult? Redirect)> RequireBusinessAsync()
    {
        var (state, redirect) = await RequireServicesCompletedAsync();
        return redirect != null ? (null, redirect) : (state, null);
    }

    private async Task<(ProviderRegistrationState? State, IActionResult? Redirect)> RequireExamReadyAsync()
    {
        var state = await registration.GetAsync();
        if (state.UsesServicesFirstFlow || state.UsesServicesBeforeExamIntro)
        {
            return await RequireServicesCompletedAsync();
        }

        return await RequireBusinessCompletedAsync();
    }

    private string GetExamIntroBackUrl(ProviderRegistrationState state)
    {
        if (state.UsesNewWizard)
        {
            return Url.Action(nameof(CategoriesAssessment))!;
        }

        if (state.UsesServicesFirstFlow)
        {
            return Url.Action(nameof(Business))!;
        }

        if (state.UsesServicesBeforeExamIntro)
        {
            return Url.Action(nameof(Services))!;
        }

        return Url.Action(nameof(Business))!;
    }

    private static (string Title, string Subtitle) GetExamIntroCopy(ProviderRegistrationState state)
    {
        if (state.IsHandymanOnly)
        {
            return ("INDOR handyman qualification", "Pass the trade qualification to unlock handyman jobs only.");
        }

        if (state.IsConstructionOnly)
        {
            return ("INDOR construction qualification", "Pass the trade qualification to unlock construction company jobs only.");
        }

        if (state.IsBathroomOnly)
        {
            return ("INDOR bathroom remodeling qualification", "Pass the trade qualification to unlock bathroom remodeling jobs only.");
        }

        if (state.IsKitchenOnly)
        {
            return ("INDOR kitchen remodeling qualification", "Pass the trade qualification to unlock kitchen remodeling jobs only.");
        }

        if (state.IsPaintingOnly)
        {
            return ("INDOR painting qualification", "Pass the trade qualification to unlock painting jobs only.");
        }

        if (state.IsFlooringOnly)
        {
            return ("INDOR flooring qualification", "Pass the trade qualification to unlock flooring jobs only.");
        }

        if (state.IsCleaningOnly)
        {
            return ("INDOR cleaning qualification", "Pass the trade qualification to unlock cleaning jobs only.");
        }

        if (state.IsLandscapingOnly)
        {
            return ("INDOR landscaping qualification", "Pass the trade qualification to unlock landscaping jobs only.");
        }

        if (state.IsPestOnly)
        {
            return ("INDOR pest control qualification", "Pass the trade qualification to unlock pest control jobs only.");
        }

        if (state.IsApplianceOnly)
        {
            return ("INDOR appliance repair qualification", "Pass the trade qualification to unlock appliance repair jobs only.");
        }

        if (state.IsRoofingOnly)
        {
            return ("INDOR roofing qualification", "Pass the trade qualification to unlock roofing jobs only.");
        }

        return ("INDOR HVAC qualification", "Pass the trade qualification to unlock HVAC jobs only.");
    }

    private static string FormatProviderTypeLabel(string providerType) => providerType switch
    {
        "LicensedPlumber" => "Licensed plumber",
        "MasterPlumber" => "Master plumber",
        "PlumbingCompany" => "Plumbing company",
        "Company" => "Company",
        "Individual" => "Individual",
        "Independent" => "Independent",
        "HvacCompany" => "HVAC company",
        "ConstructionCompany" => "Construction company",
        "BathroomRemodeler" => "Bathroom remodeling",
        "KitchenRemodeler" => "Kitchen remodeling",
        "RoofingContractor" => "Roofing",
        "PaintingContractor" => "Painting",
        "FlooringContractor" => "Flooring",
        "CleaningCompany" => "Cleaning",
        "LandscapingCompany" => "Landscaping",
        "PestControlCompany" => "Pest Control",
        "ApplianceRepairCompany" => "Appliance Repair",
        _ => providerType,
    };

    private async Task SaveDocumentFileAsync(IFormFile? file, string documentType)
    {
        if (file == null || file.Length == 0)
        {
            return;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            ext = ".jpg";
        }

        if (!AllowedDocExtensions.Contains(ext))
        {
            ModelState.AddModelError(string.Empty, $"File type not allowed for {documentType}. Use PDF, JPG, or PNG.");
            return;
        }

        if (file.Length > MaxDocumentBytes)
        {
            ModelState.AddModelError(string.Empty, "Each file must be 10 MB or less.");
            return;
        }

        var proveedor = await registration.GetProveedorForCurrentUserAsync();
        if (proveedor == null)
        {
            await registration.LinkCurrentUserAsync();
            proveedor = await registration.GetProveedorForCurrentUserAsync();
        }

        if (proveedor == null)
        {
            return;
        }

        var uploadDir = Path.Combine(env.WebRootPath, "uploads", "provider", proveedor.Id.ToString());
        Directory.CreateDirectory(uploadDir);
        var storedName = $"{documentType}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadDir, storedName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/provider/{proveedor.Id}/{storedName}";
        await registration.RegisterDocumentUploadAsync(documentType, relativeUrl);
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

    private bool IsCheckboxChecked(string fieldName)
    {
        if (!Request.Form.TryGetValue(fieldName, out var values) || values.Count == 0)
        {
            return false;
        }

        return values.Any(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase));
    }
}
