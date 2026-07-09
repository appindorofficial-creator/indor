using IndorMvcApp.Models;
using IndorMvcApp.Validation;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

public partial class ProviderRegistrationController
{
    [HttpGet]
    public async Task<IActionResult> Entry()
    {
        await registration.LinkCurrentUserAsync();
        var state = await registration.GetAsync();
        var user = await userManager.GetUserAsync(User);
        state.Email ??= user?.Email ?? "";

        return View(StepVm(1, "Service Provider",
            "Choose how you want to get started with INDOR.",
            state, Url.Action("SelectRole", "Account")));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entry(string? path, string? action)
    {
        var state = await registration.GetAsync();
        var termsAccepted = IsCheckboxChecked("termsAccepted");

        if (string.Equals(action, "later", StringComparison.OrdinalIgnoreCase))
        {
            state.IndorProActive = true;
            state.OnboardingPath = "ProOnly";
            state.UsesNewWizard = true;
            await registration.ActivateIndorProAsync(state);
            return RedirectToAction("Dashboard", "Proveedor");
        }

        if (!termsAccepted)
        {
            ModelState.AddModelError(string.Empty, localizer["Please agree to INDOR's Terms & Conditions."]);
            return View(StepVm(1, "Service Provider",
                "Choose how you want to get started with INDOR.",
                state, Url.Action("SelectRole", "Account")));
        }

        state.TermsAccepted = true;
        state.OnboardingPath = string.Equals(path, "apply", StringComparison.OrdinalIgnoreCase) ? "Apply" : "ProOnly";
        state.UsesNewWizard = true;
        state.IndorProActive = true;
        await registration.SaveAsync(state, 1);
        return RedirectToAction(nameof(CompanyInfo));
    }

    [HttpGet]
    public async Task<IActionResult> CompanyInfo()
    {
        await registration.LinkCurrentUserAsync();
        var state = await registration.GetAsync();
        if (!state.UsesNewWizard)
        {
            return RedirectToAction(nameof(Categories));
        }

        var user = await userManager.GetUserAsync(User);
        state.Email ??= user?.Email ?? "";
        ViewBag.Categories = await registration.GetCategoriesAsync();

        return View(StepVm(2, "Your Company Information",
            "Tell us about your business to set up your provider profile.",
            state, Url.Action(nameof(Entry))));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompanyInfo(
        string businessName,
        string primaryContact,
        string? phone,
        string email,
        string? primaryCategoryId,
        string? serviceAreas,
        string? website,
        string? einNumber,
        string? action)
    {
        var state = await registration.GetAsync();
        var termsAccepted = IsCheckboxChecked("termsAccepted");

        if (string.Equals(action, "later", StringComparison.OrdinalIgnoreCase))
        {
            await registration.ActivateIndorProAsync(state);
            return RedirectToAction("Dashboard", "Proveedor");
        }

        ApplyCompanyInfoFields(
            state,
            businessName,
            primaryContact,
            phone,
            email,
            primaryCategoryId,
            serviceAreas,
            website,
            einNumber,
            termsAccepted);

        if (!termsAccepted)
        {
            ModelState.AddModelError(string.Empty, localizer["Please agree to INDOR's Terms & Conditions."]);
        }

        if (string.IsNullOrWhiteSpace(businessName))
        {
            ModelState.AddModelError(string.Empty, localizer["Company name is required."]);
        }

        if (string.IsNullOrWhiteSpace(primaryContact))
        {
            ModelState.AddModelError(string.Empty, localizer["Contact name is required."]);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(string.Empty, localizer["Email address is required."]);
        }

        if (!UsPhoneOptionalAttribute.IsValidOptional(phone))
        {
            ModelState.AddModelError(string.Empty,
                "Enter a valid 10-digit US phone number (e.g. 555 123 4567).");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await registration.GetCategoriesAsync();
            return View(StepVm(2, "Your Company Information",
                "Tell us about your business to set up your provider profile.",
                state, Url.Action(nameof(Entry))));
        }

        state.Phone = UsPhoneOptionalAttribute.NormalizeToStorage(phone) ?? "";

        await registration.SaveAsync(state, 2);
        return RedirectToAction(nameof(Verification));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCompanyInfoDraft(
        string? businessName,
        string? primaryContact,
        string? phone,
        string? email,
        string? primaryCategoryId,
        string? serviceAreas,
        string? website,
        string? einNumber,
        bool termsAccepted = false)
    {
        var state = await registration.GetAsync();
        if (!state.UsesNewWizard)
        {
            return BadRequest();
        }

        termsAccepted = IsCheckboxChecked("termsAccepted");

        ApplyCompanyInfoFields(
            state,
            businessName ?? "",
            primaryContact ?? "",
            phone,
            email ?? "",
            primaryCategoryId,
            serviceAreas,
            website,
            einNumber,
            termsAccepted);

        await registration.SaveAsync(state, 2);
        return Ok();
    }

    private static void ApplyCompanyInfoFields(
        ProviderRegistrationState state,
        string businessName,
        string primaryContact,
        string? phone,
        string email,
        string? primaryCategoryId,
        string? serviceAreas,
        string? website,
        string? einNumber,
        bool termsAccepted)
    {
        state.BusinessName = businessName?.Trim() ?? "";
        state.PrimaryContact = primaryContact?.Trim() ?? "";
        state.Phone = phone?.Trim() ?? "";
        state.Email = email?.Trim() ?? "";
        state.Website = website?.Trim();
        state.EinNumber = einNumber?.Trim();
        state.LicenseNumber = einNumber?.Trim();
        state.TermsAccepted = termsAccepted;
        state.ServiceZipCodes = serviceAreas?.Trim() ?? "";
        if (!string.IsNullOrWhiteSpace(serviceAreas))
        {
            state.ZipOrNeighborhoods = serviceAreas
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(primaryCategoryId))
        {
            state.SelectedCategoryIds = [primaryCategoryId.Trim()];
        }
    }

    [HttpGet]
    public async Task<IActionResult> Verification()
    {
        var state = await registration.GetAsync();
        if (!state.UsesNewWizard)
        {
            return RedirectToAction(nameof(Documents));
        }

        if (string.IsNullOrWhiteSpace(state.BusinessName))
        {
            return RedirectToAction(nameof(CompanyInfo));
        }

        await registration.EnsureDocumentSlotsAsync();
        ViewBag.DocumentSlots = await registration.GetDocumentSlotsAsync();
        ViewBag.HasDocuments = await registration.HasRequiredDocumentsAsync();

        return View(StepVm(3, "Access & Verification",
            "Start using INDOR Pro now, and complete verification to receive INDOR jobs.",
            state, Url.Action(nameof(CompanyInfo))));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Verification(
        IFormFile? licenseFile,
        IFormFile? insuranceFile,
        IFormFile? governmentIdFile,
        IFormFile? businessRegistrationFile,
        string? action)
    {
        var state = await registration.GetAsync();

        if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
        {
            await SaveDocumentFileAsync(licenseFile, ProviderDocumentTypes.License);
            await SaveDocumentFileAsync(insuranceFile, ProviderDocumentTypes.Insurance);
            await SaveDocumentFileAsync(governmentIdFile, ProviderDocumentTypes.GovernmentId);
            await SaveDocumentFileAsync(businessRegistrationFile, ProviderDocumentTypes.BusinessRegistration);
            return RedirectToAction(nameof(Verification));
        }

        if (string.Equals(action, "pro-only", StringComparison.OrdinalIgnoreCase))
        {
            await registration.ActivateIndorProAsync(state);
            return RedirectToAction("Dashboard", "Proveedor");
        }

        await registration.SaveAsync(state, 3);
        return RedirectToAction(nameof(CategoriesAssessment));
    }

    [HttpGet]
    public async Task<IActionResult> CategoriesAssessment()
    {
        var state = await registration.GetAsync();
        if (!state.UsesNewWizard)
        {
            return RedirectToAction(nameof(Categories));
        }

        if (string.IsNullOrWhiteSpace(state.BusinessName))
        {
            return RedirectToAction(nameof(CompanyInfo));
        }

        ViewBag.Categories = await registration.GetCategoriesAsync();
        ViewBag.SelectedIds = state.SelectedCategoryIds;

        return View(StepVm(4, "Service Categories & Assessment",
            "Select your service categories and complete a short assessment if you want to activate your INDOR Provider profile.",
            state, Url.Action(nameof(Verification))));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoriesAssessment(string[]? categoryIds, string? action)
    {
        var state = await registration.GetAsync();
        var picked = categoryIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];

        if (picked.Count == 0 && string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            picked = state.SelectedCategoryIds;
        }

        if (picked.Count == 0)
        {
            ModelState.AddModelError(string.Empty, localizer["Select at least one service category."]);
            ViewBag.Categories = await registration.GetCategoriesAsync();
            ViewBag.SelectedIds = state.SelectedCategoryIds;
            return View(StepVm(4, "Service Categories & Assessment",
                "Select your service categories and complete a short assessment if you want to activate your INDOR Provider profile.",
                state, Url.Action(nameof(Verification))));
        }

        state.SelectedCategoryIds = picked;
        state.SelectedServiceIds = [];
        state.UsesNewWizard = true;

        if (string.Equals(action, "take-assessment", StringComparison.OrdinalIgnoreCase))
        {
            state.AssessmentStarted = true;
            state.AssessmentSkipped = false;
            state.ExamAnswers.Clear();
            state.ExamPassed = null;
            state.ExamScorePercent = 0;
            await registration.SaveAsync(state, 4);

            if (state.UsesExamIntroFlow)
            {
                return RedirectToAction(nameof(ExamIntro));
            }

            return RedirectToAction(nameof(Exam));
        }

        state.AssessmentSkipped = true;
        state.AssessmentStarted = false;
        await registration.SaveAsync(state, 4);
        return RedirectToAction(nameof(ActivationCall));
    }

    [HttpGet]
    public async Task<IActionResult> ActivationCall()
    {
        var state = await registration.GetAsync();
        if (!state.UsesNewWizard)
        {
            return RedirectToAction(nameof(Review));
        }

        if (string.IsNullOrWhiteSpace(state.BusinessName))
        {
            return RedirectToAction(nameof(CompanyInfo));
        }

        ViewBag.HasDocuments = await registration.HasRequiredDocumentsAsync();
        ViewBag.AssessmentComplete = state.ExamPassed == true;
        ViewBag.AssessmentSkipped = state.AssessmentSkipped;

        return View(StepVm(5, "Activation Call",
            "Schedule a call with the INDOR team to activate your profile for INDOR job opportunities.",
            state, Url.Action(nameof(CategoriesAssessment))));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivationCall(string? callSlot, string? action, bool termsAccepted)
    {
        var state = await registration.GetAsync();

        if (string.Equals(action, "enter-pro", StringComparison.OrdinalIgnoreCase))
        {
            await registration.ActivateIndorProAsync(state);
            return RedirectToAction("Dashboard", "Proveedor");
        }

        if (!termsAccepted)
        {
            ModelState.AddModelError(string.Empty, localizer["Please agree to INDOR's Terms & Conditions."]);
            ViewBag.HasDocuments = await registration.HasRequiredDocumentsAsync();
            ViewBag.AssessmentComplete = state.ExamPassed == true;
            ViewBag.AssessmentSkipped = state.AssessmentSkipped;
            return View(StepVm(5, "Activation Call",
                "Schedule a call with the INDOR team to activate your profile for INDOR job opportunities.",
                state, Url.Action(nameof(CategoriesAssessment))));
        }

        state.TermsAccepted = termsAccepted;
        state.ActivationCallSlot = callSlot?.Trim();
        state.ActivationCallScheduled = !string.IsNullOrWhiteSpace(callSlot)
            && !string.Equals(callSlot, "later", StringComparison.OrdinalIgnoreCase);
        state.OnboardingPath = "Apply";
        await registration.SaveAsync(state, 5);

        if (state.ActivationCallScheduled)
        {
            state.ProfileSubmitted = true;
            await registration.SubmitApplicationAsync(state);
        }
        else
        {
            await registration.ActivateIndorProAsync(state);
        }

        return RedirectToAction("Dashboard", "Proveedor");
    }
}
