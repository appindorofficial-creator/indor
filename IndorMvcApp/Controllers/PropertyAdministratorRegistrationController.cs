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

        var missingFields = GetMissingPropertyFields(draft);
        if (missingFields.Count > 0)
        {
            var model = await BuildPropertiesViewModelAsync(draft);
            model.FormError = "Please enter " + string.Join(", ", missingFields) + ".";
            return View(nameof(Properties), model);
        }

        await SavePortfolioPropertyAsync(draft);

        return RedirectToAction(nameof(Properties));
    }

    [HttpGet]
    public async Task<IActionResult> ImportPortfolio()
    {
        if (!await CanAccessPropertiesStepAsync())
        {
            return RedirectToAction(nameof(Portfolio));
        }

        return View(await BuildImportPortfolioViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxPortfolioImportBytes)]
    public async Task<IActionResult> ImportPortfolio(IFormFile? portfolioFile)
    {
        if (!await CanAccessPropertiesStepAsync())
        {
            return RedirectToAction(nameof(Portfolio));
        }

        if (portfolioFile == null || portfolioFile.Length == 0)
        {
            var empty = await BuildImportPortfolioViewModelAsync();
            empty.FormError = "Please choose a CSV file to import.";
            return View(empty);
        }

        if (portfolioFile.Length > MaxPortfolioImportBytes)
        {
            var tooLarge = await BuildImportPortfolioViewModelAsync();
            tooLarge.FormError = "File is too large. Maximum size is 5 MB.";
            return View(tooLarge);
        }

        var extension = Path.GetExtension(portfolioFile.FileName).ToLowerInvariant();
        if (extension is not ".csv" and not ".txt")
        {
            var invalidType = await BuildImportPortfolioViewModelAsync();
            invalidType.FormError = "Please upload a CSV file (.csv).";
            return View(invalidType);
        }

        string csvText;
        using (var reader = new StreamReader(portfolioFile.OpenReadStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
        {
            csvText = await reader.ReadToEndAsync();
        }

        var rows = ParsePortfolioCsv(csvText);
        if (rows.Count == 0)
        {
            var noRows = await BuildImportPortfolioViewModelAsync();
            noRows.FormError = "No property rows were found. Use the template and add at least one property.";
            return View(noRows);
        }

        var imported = 0;
        var errors = new List<string>();
        foreach (var (rowNumber, input) in rows)
        {
            var missing = GetMissingPropertyFields(input);
            if (missing.Count > 0)
            {
                errors.Add($"Row {rowNumber}: missing {string.Join(", ", missing)}.");
                continue;
            }

            try
            {
                await SavePortfolioPropertyAsync(input);
                imported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row {rowNumber}: {ex.Message}");
            }
        }

        if (imported == 0)
        {
            var failed = await BuildImportPortfolioViewModelAsync();
            failed.FormError = errors.Count > 0
                ? string.Join(" ", errors.Take(3))
                : "No properties could be imported. Check your file and try again.";
            return View(failed);
        }

        TempData["PropertyAdminSuccess"] = imported == 1
            ? "1 property imported successfully."
            : $"{imported} properties imported successfully.";
        if (errors.Count > 0)
        {
            TempData["PropertyAdminError"] = $"{errors.Count} row(s) were skipped. {errors[0]}";
        }

        return RedirectToAction(nameof(Properties));
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
        return RedirectToAction(nameof(Properties));
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

    private async Task<PropertyAdministratorImportPortfolioViewModel> BuildImportPortfolioViewModelAsync()
    {
        var state = await registration.GetAsync();
        return new PropertyAdministratorImportPortfolioViewModel
        {
            Step = 2,
            DisplayStep = 3,
            TotalSteps = 5,
            Title = "Import portfolio",
            Subtitle = "Upload a CSV file to add multiple properties at once.",
            BackUrl = Url.Action(nameof(Properties))!,
            State = state
        };
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

    private static List<(int RowNumber, PropertyAdministratorPropertyInput Input)> ParsePortfolioCsv(string csvText)
    {
        var results = new List<(int, PropertyAdministratorPropertyInput)>();
        var lines = csvText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var rowNumber = i + 1;
            var columns = ParseCsvLine(line);
            if (columns.Count == 0)
            {
                continue;
            }

            if (rowNumber == 1 && columns[0].Equals("HouseNumber", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var input = new PropertyAdministratorPropertyInput
            {
                HouseNumber = GetCsvColumn(columns, 0),
                StreetName = GetCsvColumn(columns, 1) ?? "",
                City = GetCsvColumn(columns, 2) ?? "",
                State = GetCsvColumn(columns, 3) ?? "",
                ZipCode = GetCsvColumn(columns, 4),
                PropertyType = NormalizePropertyType(GetCsvColumn(columns, 5) ?? ""),
                PropertyName = GetCsvColumn(columns, 6) ?? ""
            };

            if (string.IsNullOrWhiteSpace(input.HouseNumber) &&
                string.IsNullOrWhiteSpace(input.StreetName) &&
                string.IsNullOrWhiteSpace(input.City))
            {
                continue;
            }

            results.Add((rowNumber, input));
        }

        return results;
    }

    private static List<string> ParseCsvLine(string line) =>
        line.Split(',').Select(part => part.Trim().Trim('"')).ToList();

    private static string? GetCsvColumn(IReadOnlyList<string> columns, int index) =>
        index < columns.Count ? columns[index].Trim() : null;

    private static string NormalizePropertyType(string value)
    {
        var trimmed = value.Trim();
        if (PropertyAdministratorCatalog.IsValidPropertyType(trimmed))
        {
            return trimmed;
        }

        var match = PropertyAdministratorCatalog.PropertyTypes
            .FirstOrDefault(type => type.Label.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        return string.IsNullOrWhiteSpace(match.Value) ? trimmed : match.Value;
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
