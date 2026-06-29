using IndorMvcApp.Models;
using IndorMvcApp.Services;
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
    IWebHostEnvironment environment) : Controller
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

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        await registration.LinkCurrentUserAsync();
        var state = await registration.GetAsync();
        var userId = userManager.GetUserId(User);
        return View(StepVm(1, "Create your Multi-Property Owner profile",
            "For owners of multiple homes, rentals, and short-term rental properties.",
            state, Url.Action("SelectRole", "Account", new { userId })!));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(
        string portfolioBusinessName,
        bool termsAccepted,
        bool marketingOptIn)
    {
        if (!termsAccepted)
        {
            ModelState.AddModelError(string.Empty, "Please agree to the Terms to continue.");
        }

        if (!ModelState.IsValid)
        {
            var state = await registration.GetAsync();
            state.PortfolioBusinessName = portfolioBusinessName?.Trim() ?? "";
            state.TermsAccepted = termsAccepted;
            state.MarketingOptIn = marketingOptIn;
            var userId = userManager.GetUserId(User);
            return View(StepVm(1, "Create your Multi-Property Owner profile",
                "For owners of multiple homes, rentals, and short-term rental properties.",
                state, Url.Action("SelectRole", "Account", new { userId })!));
        }

        await registration.SaveProfileAsync(new PropertyAdministratorProfileInput
        {
            PortfolioBusinessName = portfolioBusinessName ?? "",
            TermsAccepted = termsAccepted,
            MarketingOptIn = marketingOptIn
        });

        return RedirectToAction(nameof(Portfolio));
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

        return RedirectAfterPropertyChange("Property saved successfully.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportPortfolio(IFormFile? csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            var emptyModel = await BuildPropertiesViewModelAsync();
            emptyModel.FormError = "Choose a CSV file to import.";
            return View(nameof(Properties), emptyModel);
        }

        if (csvFile.Length > 1024 * 1024)
        {
            var largeModel = await BuildPropertiesViewModelAsync();
            largeModel.FormError = "CSV files must be 1 MB or smaller.";
            return View(nameof(Properties), largeModel);
        }

        var extension = Path.GetExtension(csvFile.FileName);
        if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(csvFile.ContentType, "text/csv", StringComparison.OrdinalIgnoreCase))
        {
            var typeModel = await BuildPropertiesViewModelAsync();
            typeModel.FormError = "Upload a .csv file.";
            return View(nameof(Properties), typeModel);
        }

        PropertyAdministratorPortfolioImportResult result;
        await using (var stream = csvFile.OpenReadStream())
        {
            result = await registration.ImportPortfolioFromCsvAsync(stream);
        }

        var model = await BuildPropertiesViewModelAsync();
        model.ImportErrors = result.Errors;
        if (result.ImportedCount > 0)
        {
            model.FormSuccess = result.ImportedCount == 1
                ? "1 property imported from CSV."
                : $"{result.ImportedCount} properties imported from CSV.";
        }

        if (result.ImportedCount == 0)
        {
            model.FormError = result.Errors.FirstOrDefault() ?? "No properties were imported.";
        }
        else if (result.Errors.Count > 0)
        {
            model.FormSuccess += $" {result.Errors.Count} row(s) were skipped.";
        }

        return View(nameof(Properties), model);
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
        const string csv = """
            HouseNumber,StreetName,City,State,ZipCode,PropertyType,Nickname
            1615,Redcliff,Los Angeles,CA,90001,SingleFamily,Beach house
            """;
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "indor-portfolio-template.csv");
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

        var folder = Path.Combine(environment.WebRootPath, "uploads", "property-admin-registration", userId);
        Directory.CreateDirectory(folder);

        var saved = GetUploadedPropertyDocuments();
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
            if (!saved.Any(s => s.Equals(label, StringComparison.OrdinalIgnoreCase)))
            {
                saved.Add(label);
            }
        }

        SaveUploadedPropertyDocuments(saved);

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
    public async Task<IActionResult> RemoveProperty(int id)
    {
        await registration.RemovePortfolioPropertyAsync(id);
        return RedirectAfterPropertyChange("Property removed.");
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
        if (string.IsNullOrWhiteSpace(email))
        {
            var invalid = await BuildInviteTeamViewModelAsync();
            invalid.FormError = "Please enter an email address.";
            return View(invalid);
        }

        if (!email.Contains('@') || !email.Contains('.'))
        {
            var invalid = await BuildInviteTeamViewModelAsync();
            invalid.FormError = "Please enter a valid email address.";
            return View(invalid);
        }

        var display = string.IsNullOrWhiteSpace(inviteName) ? email : $"{inviteName.Trim()} <{email}>";
        var invites = GetPendingTeamInvites();
        if (!invites.Any(i => i.Equals(display, StringComparison.OrdinalIgnoreCase)))
        {
            invites.Add(display);
            SavePendingTeamInvites(invites);
        }

        var model = await BuildInviteTeamViewModelAsync();
        model.FormSuccess = $"Invite added for {email}.";
        return View(model);
    }

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
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(bool platformTermsAccepted)
    {
        if (!platformTermsAccepted)
        {
            ModelState.AddModelError(string.Empty, "Please agree to INDOR's platform terms.");
            var model = await registration.GetReviewViewModelAsync();
            model.BackUrl = Url.Action(nameof(Tools))!;
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
            UploadedFiles = GetUploadedPropertyDocuments()
        };
    }

    private List<string> GetUploadedPropertyDocuments()
    {
        var raw = HttpContext.Session.GetString(UploadedPropertyDocumentsSessionKey);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private void SaveUploadedPropertyDocuments(IReadOnlyList<string> files)
    {
        HttpContext.Session.SetString(
            UploadedPropertyDocumentsSessionKey,
            string.Join('\n', files));
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

    private static PropertyAdministratorRegistrationStepViewModel StepVm(
        int displayStep,
        string title,
        string subtitle,
        PropertyAdministratorRegistrationState state,
        string backUrl) =>
        new()
        {
            Step = displayStep - 1,
            DisplayStep = displayStep,
            TotalSteps = 5,
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
            ZipCode = draft.ZipCode,
            Location = PropertyAdministratorCatalog.FormatPropertyLocation(
                draft.City, draft.State, streetLine, draft.ZipCode),
            PropertyType = draft.PropertyType
        });
    }
}
