using IndorMvcApp.Localization;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.Validation;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace IndorMvcApp.Controllers;

[Authorize]
public class PropertyAdministratorRegistrationController(
    IPropertyAdministratorRegistrationService registration,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IWebHostEnvironment environment,
    IIndorLocalizer L) : Controller
{
    private const int MaxPortfolioImportBytes = 5_000_000;
    private const int MaxPropertyDocumentBytes = 10_000_000;
    private static readonly string[] AllowedDocumentExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".webp", ".heic"];
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            context.Result = Challenge();
            return;
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null ||
            !string.Equals(user.RolUsuario, "AdministradorPropiedades", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = RedirectToAction("Index", "Home");
            return;
        }

        if (!await userManager.IsInRoleAsync(user, "AdministradorPropiedades"))
        {
            await userManager.AddToRoleAsync(user, "AdministradorPropiedades");
            await signInManager.SignInAsync(user, isPersistent: true);
        }

        await next();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var admin = await registration.GetAdministratorForCurrentUserAsync();
        if (admin != null && registration.IsRegistrationComplete(admin))
        {
            return RedirectToAction("Dashboard", "Administrador");
        }

        // Draft users who already accepted terms can enter the app;
        // portfolio/properties are completed later inside the portal.
        if (admin is { TermsAccepted: true })
        {
            await registration.CompleteRegistrationAsync(platformTermsAccepted: true);
            return RedirectToAction("Dashboard", "Administrador");
        }

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        await registration.LinkCurrentUserAsync();
        var admin = await registration.GetAdministratorForCurrentUserAsync();
        if (admin != null && registration.IsRegistrationComplete(admin))
        {
            return RedirectToAction("Dashboard", "Administrador");
        }

        if (admin is { TermsAccepted: true })
        {
            await registration.CompleteRegistrationAsync(platformTermsAccepted: true);
            return RedirectToAction("Dashboard", "Administrador");
        }

        var state = await registration.GetAsync();
        var userId = userManager.GetUserId(User);
        return View(StepVm(1, "Create your Multi-Property Owner profile",
            "Accept the terms to enter INDOR. You can add portfolio and properties later inside the app.",
            state, Url.Action("SelectRole", "Account", new { userId })!, totalSteps: 1));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(
        string portfolioBusinessName,
        bool termsAccepted,
        bool marketingOptIn)
    {
        await registration.LinkCurrentUserAsync();
        termsAccepted = IsCheckboxChecked("termsAccepted");
        marketingOptIn = IsCheckboxChecked("marketingOptIn");

        if (!termsAccepted)
        {
            ModelState.AddModelError(nameof(termsAccepted), "Please agree to the Terms to continue.");
        }

        if (!ModelState.IsValid)
        {
            var state = await registration.GetAsync();
            state.PortfolioBusinessName = portfolioBusinessName?.Trim() ?? "";
            state.TermsAccepted = termsAccepted;
            state.MarketingOptIn = marketingOptIn;
            var userId = userManager.GetUserId(User);
            return View(StepVm(1, "Create your Multi-Property Owner profile",
                "Accept the terms to enter INDOR. You can add portfolio and properties later inside the app.",
                state, Url.Action("SelectRole", "Account", new { userId })!, totalSteps: 1));
        }

        await registration.SaveProfileAsync(new PropertyAdministratorProfileInput
        {
            PortfolioBusinessName = portfolioBusinessName ?? "",
            TermsAccepted = termsAccepted,
            MarketingOptIn = marketingOptIn
        });

        await registration.CompleteRegistrationAsync(platformTermsAccepted: true);
        return RedirectToAction("Dashboard", "Administrador");
    }

    [HttpGet]
    public async Task<IActionResult> Portfolio()
    {
        var state = await registration.GetAsync();
        if (!state.TermsAccepted)
        {
            return RedirectToAction(nameof(Profile));
        }

        return View(StepVm(2, "Tell us about your portfolio",
            "Help us tailor INDOR to your properties and rental operations.",
            state, Url.Action(nameof(Profile), "PropertyAdministratorRegistration")!));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Portfolio(
        string propertyCountRange,
        string portfolioType,
        string ownershipType,
        string primaryMarket,
        string managementStyle)
    {
        await registration.LinkCurrentUserAsync();

        if (string.IsNullOrWhiteSpace(propertyCountRange))
        {
            ModelState.AddModelError(nameof(propertyCountRange), "Please select how many properties you manage.");
        }

        if (string.IsNullOrWhiteSpace(portfolioType))
        {
            ModelState.AddModelError(nameof(portfolioType), "Please select a portfolio type.");
        }

        if (string.IsNullOrWhiteSpace(ownershipType))
        {
            ModelState.AddModelError(nameof(ownershipType), "Please select an ownership type.");
        }

        if (string.IsNullOrWhiteSpace(primaryMarket))
        {
            ModelState.AddModelError(nameof(primaryMarket), "Please select your primary market.");
        }

        if (string.IsNullOrWhiteSpace(managementStyle))
        {
            ModelState.AddModelError(nameof(managementStyle), "Please select how you manage your properties.");
        }

        if (!ModelState.IsValid)
        {
            var state = await registration.GetAsync();
            state.PropertyCountRange = propertyCountRange ?? "";
            state.PortfolioType = portfolioType ?? "";
            state.OwnershipType = ownershipType ?? "";
            state.PrimaryMarket = primaryMarket ?? "";
            state.ManagementStyle = managementStyle ?? "";
            return View(StepVm(2, "Tell us about your portfolio",
                "Help us tailor INDOR to your properties and rental operations.",
                state, Url.Action(nameof(Profile))!));
        }

        await registration.SavePortfolioAsync(new PropertyAdministratorPortfolioInput
        {
            PropertyCountRange = propertyCountRange,
            PortfolioType = portfolioType,
            OwnershipType = ownershipType,
            PrimaryMarket = primaryMarket,
            ManagementStyle = managementStyle
        });

        return RedirectToAction(nameof(Properties));
    }

    [HttpGet]
    public async Task<IActionResult> Properties()
    {
        var state = await registration.GetAsync();
        if (string.IsNullOrWhiteSpace(state.PortfolioType))
        {
            return RedirectToAction(nameof(Portfolio));
        }

        var model = await BuildPropertiesViewModelAsync();
        if (TempData["PropertyAdminSuccess"] is string success)
        {
            model.FormSuccess = success;
        }

        if (TempData["PropertyAdminImportErrors"] is string importErrorsRaw
            && !string.IsNullOrWhiteSpace(importErrorsRaw))
        {
            model.ImportErrors = importErrorsRaw
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProperty(
        string? houseNumber,
        string streetName,
        string city,
        string state,
        string? zipCode,
        string propertyType,
        string? propertyNickname)
    {
        var draft = new PropertyAdministratorPropertyInput
        {
            HouseNumber = houseNumber,
            StreetName = streetName ?? "",
            City = city ?? "",
            State = state ?? "",
            ZipCode = zipCode,
            PropertyType = propertyType ?? "",
            PropertyName = propertyNickname ?? ""
        };

        var missingFields = PropertyAdministratorPortfolioCsvImporter.ValidatePropertyInput(draft);
        if (missingFields.Count > 0)
        {
            var model = await BuildPropertiesViewModelAsync(draft);
            model.FormError = "Please enter " + string.Join(", ", missingFields) + ".";
            return View(nameof(Properties), model);
        }

        await SavePortfolioPropertyAsync(draft);

        return RedirectAfterPropertyChange(L["Property saved successfully."]);
    }

    [HttpGet]
    public async Task<IActionResult> ImportPortfolio(string? returnUrl = null)
    {
        if (!await CanAccessPropertiesStepAsync())
        {
            return RedirectToAction(nameof(Portfolio));
        }

        return View(await BuildImportPortfolioViewModelAsync(returnUrl));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportPortfolio(
        IFormFile? csvFile,
        IFormFile? portfolioFile,
        string? returnUrl = null)
    {
        var file = csvFile ?? portfolioFile;
        if (file == null || file.Length == 0)
        {
            var emptyModel = await BuildImportPortfolioViewModelAsync(returnUrl);
            emptyModel.FormError = "Choose a CSV file to import.";
            return View(emptyModel);
        }

        if (file.Length > 1024 * 1024)
        {
            var largeModel = await BuildImportPortfolioViewModelAsync(returnUrl);
            largeModel.FormError = "CSV files must be 1 MB or smaller.";
            return View(largeModel);
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(file.ContentType, "text/csv", StringComparison.OrdinalIgnoreCase))
        {
            var typeModel = await BuildImportPortfolioViewModelAsync(returnUrl);
            typeModel.FormError = "Upload a .csv file.";
            return View(typeModel);
        }

        PropertyAdministratorPortfolioImportResult result;
        await using (var stream = file.OpenReadStream())
        {
            result = await registration.ImportPortfolioFromCsvAsync(stream);
        }

        if (result.ImportedCount == 0)
        {
            var failedModel = await BuildImportPortfolioViewModelAsync(returnUrl);
            failedModel.FormError = result.Errors.FirstOrDefault() ?? "No properties were imported.";
            return View(failedModel);
        }

        var successMessage = result.ImportedCount == 1
            ? "1 property imported from CSV."
            : $"{result.ImportedCount} properties imported from CSV.";
        if (result.Errors.Count > 0)
        {
            successMessage += $" {result.Errors.Count} row(s) were skipped.";
        }

        TempData["PropertyAdminImportErrors"] = result.Errors.Count > 0
            ? string.Join('\n', result.Errors)
            : null;

        var safeReturn = ResolveLocalReturnUrl(returnUrl);
        if (!string.IsNullOrWhiteSpace(safeReturn))
        {
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                TempData["PropertyAdminSuccess"] = successMessage;
            }

            return LocalRedirect(safeReturn);
        }

        return RedirectAfterPropertyChange(successMessage);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPropertyDocument(
        int portfolioPropertyId,
        IFormFile? documentFile,
        string? documentTitle)
    {
        var model = await BuildPropertiesViewModelAsync();
        if (!model.CanUploadDocuments)
        {
            model.FormError = "Add at least one property before uploading documents.";
            return View(nameof(Properties), model);
        }

        if (documentFile == null || documentFile.Length == 0)
        {
            model.FormError = "Choose a document to upload.";
            return View(nameof(Properties), model);
        }

        try
        {
            await registration.UploadPortfolioDocumentAsync(portfolioPropertyId, documentFile, documentTitle);
            model = await BuildPropertiesViewModelAsync();
            model.FormSuccess = "Document uploaded successfully.";
            return View(nameof(Properties), model);
        }
        catch (InvalidOperationException ex)
        {
            model.FormError = ex.Message;
            return View(nameof(Properties), model);
        }
    }

    [HttpGet]
    public IActionResult DownloadPortfolioTemplate()
    {
        var csv = L.IsSpanish
            ? """
              Número,Calle,Ciudad,Estado,ZIP,Tipo,Apodo
              1615,Redcliff,Los Angeles,CA,90001,Casa unifamiliar,Casa de playa
              """
            : """
              HouseNumber,StreetName,City,State,ZipCode,PropertyType,Nickname
              1615,Redcliff,Los Angeles,CA,90001,SingleFamily,Beach house
              """;
        var fileName = L.IsSpanish ? "indor-plantilla-portafolio.csv" : "indor-portfolio-template.csv";
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> UploadPropertyDocuments()
    {
        if (!await CanAccessPropertiesStepAsync())
        {
            return RedirectToAction(nameof(Portfolio));
        }

        return View(await BuildUploadDocumentsViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadPropertyDocuments(List<IFormFile>? documentFiles)
    {
        if (!await CanAccessPropertiesStepAsync())
        {
            return RedirectToAction(nameof(Portfolio));
        }

        var files = documentFiles?.Where(f => f.Length > 0).ToList() ?? [];
        if (files.Count == 0)
        {
            var empty = await BuildUploadDocumentsViewModelAsync();
            empty.FormError = "Please choose at least one document to upload.";
            return View(empty);
        }

        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var webRoot = ResolveWebRootPath();
        var folder = Path.Combine(webRoot, "uploads", "property-admin-registration", userId);
        Directory.CreateDirectory(folder);

        var saved = GetUploadedPropertyDocumentEntries();
        var errors = new List<string>();
        foreach (var file in files)
        {
            if (file.Length > MaxPropertyDocumentBytes)
            {
                errors.Add($"{file.FileName} is too large (max 10 MB).");
                continue;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedDocumentExtensions.Contains(extension))
            {
                errors.Add($"{file.FileName} is not a supported file type.");
                continue;
            }

            var stored = $"{Guid.NewGuid():N}{extension}";
            var physical = Path.Combine(folder, stored);
            await using (var stream = System.IO.File.Create(physical))
            {
                await file.CopyToAsync(stream);
            }

            var label = $"{file.FileName} ({FormatFileSize(file.Length)})";
            if (!saved.Any(s => s.Label.Equals(label, StringComparison.OrdinalIgnoreCase)))
            {
                saved.Add(new UploadedDocumentEntry
                {
                    StoredFileName = stored,
                    Label = label
                });
            }
        }

        SaveUploadedPropertyDocumentEntries(saved);

        var uploadedCount = files.Count - errors.Count;
        if (uploadedCount == 0)
        {
            var failed = await BuildUploadDocumentsViewModelAsync();
            failed.FormError = errors.Count > 0 ? string.Join(" ", errors.Take(3)) : "Upload failed. Please try again.";
            return View(failed);
        }

        var model = await BuildUploadDocumentsViewModelAsync();
        model.FormSuccess = uploadedCount == 1
            ? "1 document uploaded."
            : $"{uploadedCount} documents uploaded.";
        if (errors.Count > 0)
        {
            model.FormError = string.Join(" ", errors.Take(3));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUploadedPropertyDocument(string? fileId, string? label)
    {
        if (!await CanAccessPropertiesStepAsync())
        {
            return RedirectToAction(nameof(Portfolio));
        }

        var entries = GetUploadedPropertyDocumentEntries();
        UploadedDocumentEntry? match = null;
        if (!string.IsNullOrWhiteSpace(fileId))
        {
            match = entries.FirstOrDefault(e =>
                e.StoredFileName.Equals(fileId, StringComparison.OrdinalIgnoreCase));
        }

        if (match == null && !string.IsNullOrWhiteSpace(label))
        {
            match = entries.FirstOrDefault(e =>
                e.Label.Equals(label, StringComparison.OrdinalIgnoreCase));
        }

        if (match != null)
        {
            entries.Remove(match);
            SaveUploadedPropertyDocumentEntries(entries);

            var userId = userManager.GetUserId(User);
            var safeName = Path.GetFileName(match.StoredFileName);
            if (!string.IsNullOrWhiteSpace(userId)
                && !string.IsNullOrWhiteSpace(safeName)
                && string.Equals(safeName, match.StoredFileName, StringComparison.Ordinal))
            {
                var physical = Path.Combine(
                    ResolveWebRootPath(),
                    "uploads",
                    "property-admin-registration",
                    userId,
                    safeName);
                if (System.IO.File.Exists(physical))
                {
                    System.IO.File.Delete(physical);
                }
            }
        }

        var model = await BuildUploadDocumentsViewModelAsync();
        model.FormSuccess = L["Document removed."].ToString();
        return View(nameof(UploadPropertyDocuments), model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveProperty(int id)
    {
        await registration.RemovePortfolioPropertyAsync(id);
        return RedirectAfterPropertyChange(L["Property removed."]);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ContinueFromProperties(
        string? houseNumber,
        string? streetName,
        string? city,
        string? state,
        string? zipCode,
        string? propertyType,
        string? propertyNickname)
    {
        if (await IsRegistrationCompleteAsync())
        {
            return RedirectToAction("Properties", "Administrador");
        }

        var regState = await registration.GetAsync();
        if (IsPropertyDraftStarted(
                regState.PrimaryMarket,
                houseNumber,
                streetName,
                city,
                state,
                zipCode,
                propertyType,
                propertyNickname))
        {
            var draft = new PropertyAdministratorPropertyInput
            {
                HouseNumber = houseNumber,
                StreetName = streetName ?? "",
                City = city ?? "",
                State = state ?? "",
                ZipCode = zipCode,
                PropertyType = propertyType ?? "",
                PropertyName = propertyNickname ?? ""
            };

            var missingFields = GetMissingPropertyFields(draft);
            if (missingFields.Count > 0)
            {
                var model = await BuildPropertiesViewModelAsync(draft);
                model.FormError = "Please complete the property form before continuing. Missing: "
                    + string.Join(", ", missingFields) + ".";
                return View(nameof(Properties), model);
            }

            await SavePortfolioPropertyAsync(draft);
        }

        await registration.AdvanceFromPropertiesAsync();
        return RedirectToAction(nameof(Tools));
    }

    [HttpGet]
    public async Task<IActionResult> Tools()
    {
        var state = await registration.GetAsync();
        if (string.IsNullOrWhiteSpace(state.PortfolioType))
        {
            return RedirectToAction(nameof(Portfolio));
        }

        return View(StepVm(4, "Choose your operations tools",
            "Select what you want to manage across your properties.",
            state, Url.Action(nameof(Properties))!));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Tools(
        bool toolMaintenanceRequests,
        bool toolTurnoverCleaning,
        bool toolGuestMessaging,
        bool toolInvoicesPayments,
        bool toolDocumentsWarranties,
        bool toolServiceProviders,
        bool toolTeamAccess,
        bool notifyUrgentMaintenance,
        bool notifyWeeklySummary,
        bool notifyBookingLeaseUpdates)
    {
        await registration.SaveToolsAsync(new PropertyAdministratorToolsInput
        {
            ToolMaintenanceRequests = toolMaintenanceRequests,
            ToolTurnoverCleaning = toolTurnoverCleaning,
            ToolGuestMessaging = toolGuestMessaging,
            ToolInvoicesPayments = toolInvoicesPayments,
            ToolDocumentsWarranties = toolDocumentsWarranties,
            ToolServiceProviders = toolServiceProviders,
            ToolTeamAccess = toolTeamAccess,
            NotifyUrgentMaintenance = notifyUrgentMaintenance,
            NotifyWeeklySummary = notifyWeeklySummary,
            NotifyBookingLeaseUpdates = notifyBookingLeaseUpdates
        });

        return RedirectToAction(nameof(Review));
    }

    [HttpGet]
    public async Task<IActionResult> InviteTeam()
    {
        var state = await registration.GetAsync();
        if (string.IsNullOrWhiteSpace(state.PortfolioType))
        {
            return RedirectToAction(nameof(Portfolio));
        }

        return View(await BuildInviteTeamViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InviteTeam(string inviteEmail, string? inviteName)
    {
        var state = await registration.GetAsync();
        if (string.IsNullOrWhiteSpace(state.PortfolioType))
        {
            return RedirectToAction(nameof(Portfolio));
        }

        var email = inviteEmail?.Trim() ?? "";
        var name = inviteName?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(email))
        {
            return await InviteTeamInvalidAsync(name, email, "Please enter an email address.");
        }

        if (!ValidEmailAttribute.IsValidAddress(email, out var emailError))
        {
            return await InviteTeamInvalidAsync(name, email, emailError ?? "Please enter a valid email address.");
        }

        var display = string.IsNullOrWhiteSpace(name) ? email : $"{name} <{email}>";
        var invites = GetPendingTeamInvites();
        if (InviteListContainsEmail(invites, email))
        {
            return await InviteTeamInvalidAsync(name, email, "This email is already on your invite list.");
        }

        invites.Add(display);
        SavePendingTeamInvites(invites);

        var model = await BuildInviteTeamViewModelAsync();
        model.FormSuccess = $"{display} was added to your invite list. Add another teammate below or tap Back to review when you are finished.";
        return View(model);
    }

    private async Task<IActionResult> InviteTeamInvalidAsync(string? name, string? email, string message)
    {
        var invalid = await BuildInviteTeamViewModelAsync();
        invalid.InviteName = name ?? "";
        invalid.InviteEmail = email ?? "";
        invalid.FormError = message;
        return View(invalid);
    }

    private static bool InviteListContainsEmail(IEnumerable<string> invites, string email) =>
        invites.Any(invite =>
            invite.Equals(email, StringComparison.OrdinalIgnoreCase)
            || invite.EndsWith($"<{email}>", StringComparison.OrdinalIgnoreCase));

    [HttpGet]
    public async Task<IActionResult> Review()
    {
        var state = await registration.GetAsync();
        if (string.IsNullOrWhiteSpace(state.PortfolioType))
        {
            return RedirectToAction(nameof(Portfolio));
        }

        var model = await registration.GetReviewViewModelAsync();
        model.BackUrl = Url.Action(nameof(Tools))!;
        model.PendingTeamInviteCount = GetPendingTeamInvites().Count;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(bool platformTermsAccepted)
    {
        await registration.LinkCurrentUserAsync();
        platformTermsAccepted = IsCheckboxChecked("platformTermsAccepted");

        if (!platformTermsAccepted)
        {
            ModelState.AddModelError(nameof(platformTermsAccepted), "Please agree to INDOR's platform terms.");
            var model = await registration.GetReviewViewModelAsync();
            model.BackUrl = Url.Action(nameof(Tools))!;
            model.PendingTeamInviteCount = GetPendingTeamInvites().Count;
            return View(model);
        }

        await registration.CompleteRegistrationAsync(platformTermsAccepted);
        return RedirectToAction("Dashboard", "Administrador");
    }


    private const string PendingTeamInvitesSessionKey = "PropertyAdminPendingTeamInvites";
    private const string UploadedPropertyDocumentsSessionKey = "PropertyAdminUploadedDocuments";

    private async Task<bool> CanAccessPropertiesStepAsync()
    {
        var state = await registration.GetAsync();
        return !string.IsNullOrWhiteSpace(state.PortfolioType);
    }

    private async Task<PropertyAdministratorImportPortfolioViewModel> BuildImportPortfolioViewModelAsync(
        string? returnUrl = null)
    {
        var state = await registration.GetAsync();
        var isComplete = await IsRegistrationCompleteAsync();
        var defaultBack = isComplete
            ? (Url.Action("Properties", "Administrador") ?? "#")
            : (Url.Action(nameof(Properties)) ?? "#");
        return new PropertyAdministratorImportPortfolioViewModel
        {
            DisplayStep = 3,
            TotalSteps = 5,
            Title = "Import portfolio",
            Subtitle = "Upload a CSV file to add multiple properties at once.",
            BackUrl = ResolveLocalReturnUrl(returnUrl) ?? defaultBack,
            State = state,
            IsRegistrationComplete = isComplete
        };
    }

    private string? ResolveLocalReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : null;

    private string ResolveWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(environment.WebRootPath)
            && Directory.Exists(environment.WebRootPath))
        {
            return environment.WebRootPath;
        }

        var contentRoot = !string.IsNullOrWhiteSpace(environment.ContentRootPath)
            ? environment.ContentRootPath
            : AppContext.BaseDirectory;

        var candidates = new[]
        {
            Path.Combine(contentRoot, "wwwroot"),
            Path.GetFullPath(Path.Combine(contentRoot, "..", "..", "..", "wwwroot")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "wwwroot")),
            Path.Combine(AppContext.BaseDirectory, "wwwroot")
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        var fallback = Path.Combine(contentRoot, "wwwroot");
        Directory.CreateDirectory(fallback);
        return fallback;
    }

    private async Task<PropertyAdministratorUploadDocumentsViewModel> BuildUploadDocumentsViewModelAsync()
    {
        var state = await registration.GetAsync();
        return new PropertyAdministratorUploadDocumentsViewModel
        {
            Step = 2,
            DisplayStep = 3,
            TotalSteps = 5,
            Title = "Scan or upload documents",
            Subtitle = "Add leases, deeds, or spreadsheets to help INDOR understand your portfolio.",
            BackUrl = Url.Action(nameof(Properties))!,
            State = state,
            UploadedFiles = GetUploadedPropertyDocumentEntries()
                .Select(e => new PropertyAdministratorUploadedDocumentItem
                {
                    Id = e.StoredFileName,
                    Label = e.Label
                })
                .ToList()
        };
    }

    private sealed class UploadedDocumentEntry
    {
        public string StoredFileName { get; set; } = "";
        public string Label { get; set; } = "";
    }

    private List<UploadedDocumentEntry> GetUploadedPropertyDocumentEntries()
    {
        var raw = HttpContext.Session.GetString(UploadedPropertyDocumentsSessionKey);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line =>
            {
                var parts = line.Split('\t', 2);
                return parts.Length == 2
                    ? new UploadedDocumentEntry { StoredFileName = parts[0], Label = parts[1] }
                    : new UploadedDocumentEntry { Label = line };
            })
            .ToList();
    }

    private void SaveUploadedPropertyDocumentEntries(IReadOnlyList<UploadedDocumentEntry> files)
    {
        HttpContext.Session.SetString(
            UploadedPropertyDocumentsSessionKey,
            string.Join(
                '\n',
                files.Select(f =>
                    string.IsNullOrWhiteSpace(f.StoredFileName)
                        ? f.Label
                        : $"{f.StoredFileName}\t{f.Label}")));
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1_048_576)
        {
            return $"{bytes / 1024.0:0.#} KB";
        }

        return $"{bytes / 1_048_576.0:0.#} MB";
    }

    private async Task<PropertyAdministratorInviteTeamViewModel> BuildInviteTeamViewModelAsync()
    {
        var state = await registration.GetAsync();
        return new PropertyAdministratorInviteTeamViewModel
        {
            Step = 4,
            DisplayStep = 5,
            TotalSteps = 5,
            Title = "Invite team members",
            Subtitle = "Add colleagues who will help manage your portfolio.",
            BackUrl = Url.Action(nameof(Review))!,
            State = state,
            PendingInvites = GetPendingTeamInvites()
        };
    }

    private List<string> GetPendingTeamInvites()
    {
        var invites = HttpContext.Session.GetString(PendingTeamInvitesSessionKey);
        if (string.IsNullOrWhiteSpace(invites))
        {
            return [];
        }

        return invites
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void SavePendingTeamInvites(IReadOnlyList<string> invites)
    {
        HttpContext.Session.SetString(
            PendingTeamInvitesSessionKey,
            string.Join('\n', invites));
    }

    private async Task<bool> IsRegistrationCompleteAsync()
    {
        var admin = await registration.GetAdministratorForCurrentUserAsync();
        return admin != null && registration.IsRegistrationComplete(admin);
    }

    private IActionResult RedirectAfterPropertyChange(string? successMessage = null)
    {
        if (!string.IsNullOrWhiteSpace(successMessage))
        {
            TempData["PropertyAdminSuccess"] = successMessage;
        }

        return RedirectToAction(nameof(Properties));
    }

    private async Task<PropertyAdministratorPropertiesStepViewModel> BuildPropertiesViewModelAsync(
        PropertyAdministratorPropertyInput? draft = null)
    {
        var state = await registration.GetAsync();
        var properties = await registration.GetPortfolioPropertiesAsync();
        var isComplete = await IsRegistrationCompleteAsync();
        var doneUrl = Url.Action("Properties", "Administrador") ?? "#";
        return new PropertyAdministratorPropertiesStepViewModel
        {
            DisplayStep = 3,
            TotalSteps = 5,
            Title = isComplete ? "Add property" : "Add your properties",
            Subtitle = isComplete
                ? "Add manually, import a CSV, or upload documents for your portfolio."
                : "Start building your portfolio inside INDOR.",
            BackUrl = isComplete ? doneUrl : Url.Action(nameof(Portfolio))!,
            State = state,
            Properties = properties,
            DraftProperty = draft,
            IsRegistrationComplete = isComplete,
            DoneUrl = doneUrl
        };
    }

    private bool IsCheckboxChecked(string fieldName)
    {
        if (!Request.Form.TryGetValue(fieldName, out var values) || values.Count == 0)
        {
            return false;
        }

        return values.Any(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase));
    }

    private static PropertyAdministratorRegistrationStepViewModel StepVm(
        int displayStep,
        string title,
        string subtitle,
        PropertyAdministratorRegistrationState state,
        string backUrl,
        int totalSteps = 5) =>
        new()
        {
            Step = displayStep - 1,
            DisplayStep = displayStep,
            TotalSteps = totalSteps,
            Title = title,
            Subtitle = subtitle,
            BackUrl = backUrl,
            State = state
        };

    private static (string City, string State) GetDefaultMarketLocation(string? primaryMarket)
    {
        if (string.IsNullOrWhiteSpace(primaryMarket) ||
            string.Equals(primaryMarket, "Other", StringComparison.OrdinalIgnoreCase))
        {
            return ("", "");
        }

        var parts = primaryMarket.Split(',', 2, StringSplitOptions.TrimEntries);
        return parts.Length == 2 ? (parts[0], parts[1]) : ("", "");
    }

    private static bool IsPropertyDraftStarted(
        string? primaryMarket,
        string? houseNumber,
        string? streetName,
        string? city,
        string? state,
        string? zipCode,
        string? propertyType,
        string? propertyNickname)
    {
        if (!string.IsNullOrWhiteSpace(houseNumber)) return true;
        if (!string.IsNullOrWhiteSpace(streetName)) return true;
        if (!string.IsNullOrWhiteSpace(zipCode)) return true;
        if (!string.IsNullOrWhiteSpace(propertyType)) return true;
        if (!string.IsNullOrWhiteSpace(propertyNickname)) return true;

        var (defaultCity, defaultState) = GetDefaultMarketLocation(primaryMarket);
        if (!string.IsNullOrWhiteSpace(city) &&
            !string.Equals(city.Trim(), defaultCity, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(state) &&
            !string.Equals(state.Trim(), defaultState, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static List<string> GetMissingPropertyFields(PropertyAdministratorPropertyInput draft)
    {
        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(draft.HouseNumber))
        {
            missingFields.Add("house number");
        }

        if (string.IsNullOrWhiteSpace(draft.StreetName))
        {
            missingFields.Add("street name");
        }

        if (string.IsNullOrWhiteSpace(draft.City))
        {
            missingFields.Add("city");
        }

        if (string.IsNullOrWhiteSpace(draft.State))
        {
            missingFields.Add("state");
        }

        if (string.IsNullOrWhiteSpace(draft.ZipCode))
        {
            missingFields.Add("ZIP");
        }
        else if (!UsZipCodeAttribute.IsValidRequired(draft.ZipCode, out _))
        {
            missingFields.Add("a valid ZIP code");
        }

        if (string.IsNullOrWhiteSpace(draft.PropertyType) ||
            !PropertyAdministratorCatalog.IsValidPropertyType(draft.PropertyType))
        {
            missingFields.Add("property type");
        }

        return missingFields;
    }

    private async Task SavePortfolioPropertyAsync(PropertyAdministratorPropertyInput draft)
    {
        var streetLine = PropertyAdministratorCatalog.BuildStreetLine(draft.HouseNumber, draft.StreetName);
        var propertyName = !string.IsNullOrWhiteSpace(draft.PropertyName)
            ? draft.PropertyName.Trim()
            : streetLine;

        await registration.AddPortfolioPropertyAsync(new PropertyAdministratorPropertyInput
        {
            PropertyName = propertyName,
            HouseNumber = draft.HouseNumber,
            StreetName = draft.StreetName,
            StreetAddress = streetLine,
            City = draft.City,
            State = draft.State,
            ZipCode = UsZipCodeAttribute.NormalizeToStorage(draft.ZipCode) ?? draft.ZipCode?.Trim(),
            Location = PropertyAdministratorCatalog.FormatPropertyLocation(
                draft.City, draft.State, streetLine, draft.ZipCode),
            PropertyType = draft.PropertyType
        });
    }
}
