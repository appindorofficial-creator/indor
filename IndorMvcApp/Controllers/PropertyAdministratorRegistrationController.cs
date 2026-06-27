using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndorMvcApp.Controllers;

[Authorize]
public class PropertyAdministratorRegistrationController(
    IPropertyAdministratorRegistrationService registration,
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

        return View(await BuildPropertiesViewModelAsync());
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

        var streetLine = PropertyAdministratorCatalog.BuildStreetLine(houseNumber, streetName);
        var propertyName = !string.IsNullOrWhiteSpace(propertyNickname)
            ? propertyNickname.Trim()
            : streetLine;

        await registration.AddPortfolioPropertyAsync(new PropertyAdministratorPropertyInput
        {
            PropertyName = propertyName,
            HouseNumber = houseNumber,
            StreetName = streetName,
            StreetAddress = streetLine,
            City = city,
            State = state,
            ZipCode = zipCode,
            Location = PropertyAdministratorCatalog.FormatPropertyLocation(city, state, streetLine, zipCode),
            PropertyType = propertyType
        });

        return RedirectToAction(nameof(Properties));
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveProperty(int id)
    {
        await registration.RemovePortfolioPropertyAsync(id);
        return RedirectToAction(nameof(Properties));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ContinueFromProperties()
    {
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

    private async Task<PropertyAdministratorPropertiesStepViewModel> BuildPropertiesViewModelAsync(
        PropertyAdministratorPropertyInput? draft = null)
    {
        var state = await registration.GetAsync();
        var properties = await registration.GetPortfolioPropertiesAsync();
        return new PropertyAdministratorPropertiesStepViewModel
        {
            DisplayStep = 3,
            TotalSteps = 5,
            Title = "Add your properties",
            Subtitle = "Start building your portfolio inside INDOR.",
            BackUrl = Url.Action(nameof(Portfolio))!,
            State = state,
            Properties = properties,
            DraftProperty = draft
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
}
