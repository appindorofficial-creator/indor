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
    public IActionResult Index() => RedirectToAction(nameof(Profile));

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        await registration.LinkCurrentUserAsync();
        var realtor = await registration.GetRealtorForCurrentUserAsync();
        if (realtor != null && registration.IsRegistrationComplete(realtor))
        {
            return RedirectToAction("Dashboard", "Realtor");
        }

        var state = await registration.GetAsync();
        var user = await userManager.GetUserAsync(User);
        state.Email ??= user?.Email ?? "";
        state.DisplayName ??= $"{user?.Nombre} {user?.Apellidos}".Trim();

        return View(StepVm(2, "Realtor Verification",
            "Enter your license information so we can verify your realtor profile.",
            state, Url.Action("SelectRole", "Account"),
            registration.GetLicenseStates()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(
        string brokerageName,
        string licenseNumber,
        string licenseState,
        string serviceAreas,
        bool professionalTermsAccepted)
    {
        var state = await registration.GetAsync();

        if (!BrokerageNameAttribute.IsValidBrokerageName(brokerageName, out var brokerageError))
        {
            ModelState.AddModelError(nameof(brokerageName), brokerageError!);
        }

        if (!RealtorLicenseNumberAttribute.IsValidLicenseNumber(licenseNumber, out var licenseError))
        {
            ModelState.AddModelError(nameof(licenseNumber), licenseError!);
        }

        if (string.IsNullOrWhiteSpace(licenseState))
        {
            ModelState.AddModelError(nameof(licenseState), "License state is required.");
        }

        if (!professionalTermsAccepted)
        {
            ModelState.AddModelError(nameof(professionalTermsAccepted), "Please authorize Home Indor to verify your license to continue.");
        }

        if (!ModelState.IsValid)
        {
            state.BrokerageName = brokerageName?.Trim() ?? "";
            state.LicenseNumber = licenseNumber?.Trim() ?? "";
            state.LicenseState = licenseState?.Trim() ?? "";
            state.ServiceAreas = serviceAreas?.Trim() ?? "";
            state.ProfessionalTermsAccepted = professionalTermsAccepted;

            return View(StepVm(2, "Realtor Verification",
                "Enter your license information so we can verify your realtor profile.",
                state, Url.Action("SelectRole", "Account"),
                registration.GetLicenseStates()));
        }

        state.BrokerageName = brokerageName?.Trim() ?? "";
        state.LicenseNumber = licenseNumber.Trim();
        state.LicenseState = licenseState.Trim();
        state.ServiceAreas = serviceAreas?.Trim() ?? "";
        state.ProfessionalTermsAccepted = professionalTermsAccepted;

        await registration.SaveProfileAsync(state);
        return RedirectToAction(nameof(Verification));
    }

    [HttpGet]
    public async Task<IActionResult> Verification()
    {
        var state = await registration.GetAsync();
        if (string.IsNullOrWhiteSpace(state.LicenseNumber))
        {
            return RedirectToAction(nameof(Profile));
        }

        await registration.EnsureDocumentSlotsAsync();
        ViewBag.DocumentSlots = await registration.GetDocumentSlotsAsync();
        ViewBag.LicenseNumber = state.LicenseNumber;

        return View(StepVm(3, "Optional Verification",
            "You can do this now or update it later from your profile.",
            state, Url.Action(nameof(Profile)),
            registration.GetLicenseStates()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
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
            await SaveDocumentFileAsync(licensePhotoFile, RealtorDocumentTypes.LicensePhoto);
            await SaveDocumentFileAsync(governmentIdFile, RealtorDocumentTypes.GovernmentId);
            await SaveDocumentFileAsync(businessCardFile, RealtorDocumentTypes.BusinessCard);
            return RedirectToAction(nameof(Verification));
        }

        var skipped = string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase);
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

    private static RealtorRegistrationStepViewModel StepVm(
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
            TotalSteps = 4,
            Title = title,
            Subtitle = subtitle,
            BackUrl = backUrl,
            State = state,
            LicenseStates = licenseStates
        };

    private async Task SaveDocumentFileAsync(IFormFile? file, string documentType)
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
        if (!AllowedDocExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase) || file.Length > MaxDocumentBytes)
        {
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
