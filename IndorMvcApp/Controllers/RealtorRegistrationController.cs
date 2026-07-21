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
public class RealtorRegistrationController(
    IRealtorRegistrationService registration,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IWebHostEnvironment env) : Controller
{
    private static readonly string[] AllowedDocExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxDocumentBytes = 25_000_000;

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

        if (!await userManager.IsInRoleAsync(user, "Realtor"))
        {
            await userManager.AddToRoleAsync(user, "Realtor");
            await signInManager.SignInAsync(user, isPersistent: true);
        }

        await next();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await registration.LinkCurrentUserAsync();
        var realtor = await registration.GetRealtorForCurrentUserAsync();
        if (realtor != null && registration.IsRegistrationComplete(realtor))
        {
            return RedirectToAction("Dashboard", "Realtor");
        }

        // Quick entry: brokerage already saved → finish as Basic and show Ready.
        if (realtor != null && !string.IsNullOrWhiteSpace(realtor.BrokerageName))
        {
            await registration.CompleteVerificationAsync(skipped: true);
            return RedirectToAction(nameof(Ready));
        }

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        await registration.LinkCurrentUserAsync();
        var realtor = await registration.GetRealtorForCurrentUserAsync();
        if (realtor != null && registration.IsRegistrationComplete(realtor))
        {
            return RedirectToAction("Dashboard", "Realtor");
        }

        if (realtor != null && !string.IsNullOrWhiteSpace(realtor.BrokerageName))
        {
            await registration.CompleteVerificationAsync(skipped: true);
            return RedirectToAction(nameof(Ready));
        }

        var state = await registration.GetAsync();
        var user = await userManager.GetUserAsync(User);
        state.Email ??= user?.Email ?? "";
        state.DisplayName ??= $"{user?.Nombre} {user?.Apellidos}".Trim();

        return View(StepVm(1, "Welcome to INDOR for Realtors",
            "This is your professional space for clients, files, and quotes. No license or documents required to start.",
            state, Url.Action("SelectRole", "Account", new { userId = user?.Id })!,
            registration.GetLicenseStates()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(string? brokerageName)
    {
        var state = await registration.GetAsync();
        var user = await userManager.GetUserAsync(User);

        if (!BrokerageNameAttribute.IsValidBrokerageName(brokerageName, out var brokerageError, "Brokerage / Company Name"))
        {
            ModelState.AddModelError(nameof(brokerageName), brokerageError!);
        }

        if (!ModelState.IsValid)
        {
            state.BrokerageName = brokerageName?.Trim() ?? "";
            return View(StepVm(1, "Welcome to INDOR for Realtors",
                "This is your professional space for clients, files, and quotes. No license or documents required to start.",
                state, Url.Action("SelectRole", "Account", new { userId = user?.Id })!,
                registration.GetLicenseStates()));
        }

        state.BrokerageName = brokerageName!.Trim();
        // License, office, languages, and documents are completed later in Profile.
        state.LicenseNumber = state.LicenseNumber ?? "";
        state.LicenseState = state.LicenseState ?? "";
        state.ServiceAreas = state.ServiceAreas ?? "";
        state.OfficeAddress = state.OfficeAddress ?? "";
        state.OfficeCity = state.OfficeCity ?? "";
        state.OfficeState = state.OfficeState ?? "";
        state.OfficeZip = state.OfficeZip ?? "";
        state.Languages = string.IsNullOrWhiteSpace(state.Languages) ? "English" : state.Languages;
        state.ProfessionalTermsAccepted = true;

        await registration.SaveProfileAsync(state);
        await registration.CompleteVerificationAsync(skipped: true);
        return RedirectToAction(nameof(Ready));
    }

    [HttpGet]
    public async Task<IActionResult> Verification()
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync();
        if (realtor == null || !registration.IsRegistrationComplete(realtor))
        {
            // Quick-entry users finish registration first; verification is optional later.
            return RedirectToAction(nameof(Profile));
        }

        var state = await registration.GetAsync();
        await registration.EnsureDocumentSlotsAsync();
        ViewBag.DocumentSlots = await registration.GetDocumentSlotsAsync();
        ViewBag.LicenseNumber = state.LicenseNumber;

        return View(StepVm(2, "Optional Verification",
            "You can do this now or update it later from your profile.",
            state, Url.Action("Profile", "Realtor")!,
            registration.GetLicenseStates()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(80_000_000)]
    public async Task<IActionResult> Verification(
        IFormFile? licensePhotoFile,
        IFormFile? governmentIdFile,
        IFormFile? businessCardFile,
        string? documentType,
        string? action)
    {
        if (string.Equals(action, "remove", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(documentType))
        {
            var removedUrl = await registration.ClearDocumentAsync(documentType);
            if (!string.IsNullOrWhiteSpace(removedUrl))
            {
                DeleteUploadedFile(removedUrl);
            }

            return RedirectToAction(nameof(Verification));
        }

        if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
        {
            var uploadErrors = await SaveVerificationDocumentsAsync(licensePhotoFile, governmentIdFile, businessCardFile);
            if (uploadErrors.Count > 0)
            {
                TempData["VerificationError"] = string.Join(" ", uploadErrors);
            }

            return RedirectToAction(nameof(Verification));
        }

        var skipped = string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase);
        if (!skipped)
        {
            var continueErrors = await SaveVerificationDocumentsAsync(licensePhotoFile, governmentIdFile, businessCardFile);
            if (continueErrors.Count > 0)
            {
                TempData["VerificationError"] = string.Join(" ", continueErrors);
                return RedirectToAction(nameof(Verification));
            }

            var slots = await registration.GetDocumentSlotsAsync();
            var missingRequired = slots
                .Where(s => s.Required && !s.Uploaded)
                .Select(s => s.Label)
                .ToList();
            if (missingRequired.Count > 0)
            {
                TempData["VerificationError"] = missingRequired.Count == 1
                    ? $"Please attach your {missingRequired[0].ToLower()} before continuing, or choose Skip for now."
                    : "No required documents attached. Please upload your license photo and government ID, or choose Skip for now.";
                return RedirectToAction(nameof(Verification));
            }
        }

        await registration.CompleteVerificationAsync(skipped);
        return RedirectToAction(nameof(Ready));
    }

    [HttpGet]
    public async Task<IActionResult> Ready()
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync();
        if (realtor == null || !registration.IsRegistrationComplete(realtor))
        {
            return RedirectToAction(nameof(Profile));
        }

        var model = await registration.GetReadyViewModelAsync();
        return View(model);
    }

    private RealtorRegistrationStepViewModel StepVm(
        int displayStep,
        string title,
        string subtitle,
        RealtorRegistrationState state,
        string backUrl,
        IReadOnlyList<string> licenseStates) =>
        new()
        {
            Step = displayStep - 1,
            DisplayStep = displayStep,
            TotalSteps = 2,
            Title = title,
            Subtitle = subtitle,
            BackUrl = backUrl,
            State = state,
            LicenseStates = licenseStates,
            SupportedLanguages = registration.GetSupportedLanguages()
        };

    private async Task<List<string>> SaveVerificationDocumentsAsync(
        IFormFile? licensePhotoFile,
        IFormFile? governmentIdFile,
        IFormFile? businessCardFile)
    {
        var errors = new List<string>();
        await SaveDocumentFileAsync(licensePhotoFile, RealtorDocumentTypes.LicensePhoto, "License photo", errors);
        await SaveDocumentFileAsync(governmentIdFile, RealtorDocumentTypes.GovernmentId, "Government ID", errors);
        await SaveDocumentFileAsync(businessCardFile, RealtorDocumentTypes.BusinessCard, "Business card", errors);
        return errors;
    }

    private async Task SaveDocumentFileAsync(IFormFile? file, string documentType, string label, List<string> errors)
    {
        if (file == null || file.Length == 0)
        {
            return;
        }

        var realtor = await registration.GetRealtorForCurrentUserAsync();
        if (realtor == null)
        {
            return;
        }

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedDocExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"{label}: unsupported file type. Use PDF, JPG, PNG or WEBP.");
            return;
        }

        if (file.Length > MaxDocumentBytes)
        {
            errors.Add($"{label}: file is too large (max {MaxDocumentBytes / 1_000_000} MB).");
            return;
        }

        var folder = Path.Combine(env.WebRootPath, "uploads", "realtor-docs", realtor.Id.ToString());
        Directory.CreateDirectory(folder);
        var fileName = $"{documentType}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/realtor-docs/{realtor.Id}/{fileName}";
        await registration.RegisterDocumentUploadAsync(documentType, relativeUrl);
    }

    private bool IsCheckboxChecked(string fieldName)
    {
        if (!Request.Form.TryGetValue(fieldName, out var values) || values.Count == 0)
        {
            return false;
        }

        return values.Any(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase));
    }

    private void DeleteUploadedFile(string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
        {
            return;
        }

        var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(env.WebRootPath, relativePath);
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
