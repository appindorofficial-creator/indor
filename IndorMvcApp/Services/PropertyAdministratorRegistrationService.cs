using System.Globalization;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class PropertyAdministratorRegistrationService(
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment webHostEnvironment) : IPropertyAdministratorRegistrationService
{
    private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".heic", ".doc", ".docx"
    };
    private const string AdminIdSessionKey = "PropertyAdminRegistroId";

    public async Task<PropertyAdministratorRegistrationState> GetAsync(CancellationToken cancellationToken = default)
    {
        var adminId = await ResolveAdministratorIdAsync(cancellationToken);
        if (adminId is not > 0)
        {
            return new PropertyAdministratorRegistrationState();
        }

        var entity = await db.IndorPropertyAdministrators
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == adminId, cancellationToken);

        return entity == null ? new PropertyAdministratorRegistrationState() : MapFromEntity(entity);
    }

    public async Task LinkCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var adminId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorPropertyAdministrators.FirstAsync(a => a.Id == adminId, cancellationToken);
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return;
        }

        entity.UserId = userId;
        entity.Email ??= user.Email;
        entity.Phone ??= user.PhoneNumber ?? user.Telefono;
        entity.DisplayName = $"{user.Nombre} {user.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(entity.DisplayName))
        {
            entity.DisplayName = user.Email ?? "Property Administrator";
        }

        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveProfileAsync(PropertyAdministratorProfileInput input, CancellationToken cancellationToken = default)
    {
        var adminId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorPropertyAdministrators.FirstAsync(a => a.Id == adminId, cancellationToken);

        entity.PortfolioBusinessName = input.PortfolioBusinessName.Trim();
        entity.TermsAccepted = input.TermsAccepted;
        entity.MarketingOptIn = input.MarketingOptIn;
        entity.TermsAcceptedUtc = input.TermsAccepted ? DateTime.UtcNow : entity.TermsAcceptedUtc;
        entity.CurrentStep = 2;
        entity.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SavePortfolioAsync(PropertyAdministratorPortfolioInput input, CancellationToken cancellationToken = default)
    {
        var adminId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorPropertyAdministrators.FirstAsync(a => a.Id == adminId, cancellationToken);

        entity.PropertyCountRange = input.PropertyCountRange.Trim();
        entity.PortfolioType = input.PortfolioType.Trim();
        entity.OwnershipType = input.OwnershipType.Trim();
        entity.PrimaryMarket = input.PrimaryMarket.Trim();
        entity.ManagementStyle = input.ManagementStyle.Trim();
        entity.CurrentStep = 3;
        entity.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PropertyAdministratorPropertyItemViewModel>> GetPortfolioPropertiesAsync(
        CancellationToken cancellationToken = default)
    {
        var adminId = await ResolveAdministratorIdAsync(cancellationToken);
        if (adminId is not > 0)
        {
            return [];
        }

        return await db.IndorPropertyAdminPortfolioProperties
            .AsNoTracking()
            .Where(p => p.AdministratorId == adminId)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => new PropertyAdministratorPropertyItemViewModel
            {
                Id = p.Id,
                PropiedadId = p.PropiedadId,
                PropertyName = p.PropertyName,
                Location = p.Location,
                PropertyType = p.PropertyType,
                PropertyTypeLabel = PropertyAdministratorDisplayLocalization.LabelPropertyType(p.PropertyType),
                ImageUrl = p.ImageUrl,
                Status = p.Status
            })
            .ToListAsync(cancellationToken);
    }

    public async Task AddPortfolioPropertyAsync(PropertyAdministratorPropertyInput input, CancellationToken cancellationToken = default)
    {
        var adminId = await EnsureDraftAsync(cancellationToken);
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        var streetLine = !string.IsNullOrWhiteSpace(input.StreetAddress)
            ? input.StreetAddress.Trim()
            : PropertyAdministratorCatalog.BuildStreetLine(input.HouseNumber, input.StreetName);
        var location = !string.IsNullOrWhiteSpace(input.Location)
            ? input.Location.Trim()
            : PropertyAdministratorCatalog.FormatPropertyLocation(input.City, input.State, streetLine, input.ZipCode);
        var propertyName = !string.IsNullOrWhiteSpace(input.PropertyName)
            ? input.PropertyName.Trim()
            : streetLine;

        var propiedadId = (int?)null;
        if (!string.IsNullOrEmpty(userId))
        {
            var propiedad = new Propiedad
            {
                UserId = userId,
                Direccion = $"{propertyName}, {location}",
                DatosJson = BuildPropertyJson(input, location, streetLine, propertyName),
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            db.Propiedades.Add(propiedad);
            await db.SaveChangesAsync(cancellationToken);
            propiedadId = propiedad.Id;
        }

        var portfolioProperty = new IndorPropertyAdminPortfolioProperty
        {
            AdministratorId = adminId,
            PropertyName = propertyName,
            Location = location,
            PropertyType = input.PropertyType.Trim(),
            ImageUrl = PropertyAdministratorCatalog.DefaultImageForType(input.PropertyType.Trim()),
            PropiedadId = propiedadId,
            Status = "Added",
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorPropertyAdminPortfolioProperties.Add(portfolioProperty);

        var admin = await db.IndorPropertyAdministrators.FirstAsync(a => a.Id == adminId, cancellationToken);
        admin.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PropertyAdministratorPortfolioImportResult> ImportPortfolioFromCsvAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default)
    {
        var parsed = PropertyAdministratorPortfolioCsvImporter.ParseAndValidate(csvStream);
        if (parsed.Properties.Count == 0)
        {
            return parsed;
        }

        foreach (var property in parsed.Properties)
        {
            await AddPortfolioPropertyAsync(property, cancellationToken);
            parsed.ImportedCount++;
        }

        return parsed;
    }

    public async Task UploadPortfolioDocumentAsync(
        int portfolioPropertyId,
        IFormFile file,
        string? title,
        CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("Choose a document to upload.");
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            throw new InvalidOperationException("Documents must be 10 MB or smaller.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedDocumentExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Upload a PDF, image, or Word document.");
        }

        var adminId = await ResolveAdministratorIdAsync(cancellationToken);
        if (adminId is not > 0)
        {
            throw new InvalidOperationException("Portfolio not found.");
        }

        var portfolioProperty = await db.IndorPropertyAdminPortfolioProperties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == portfolioPropertyId && p.AdministratorId == adminId, cancellationToken)
            ?? throw new InvalidOperationException("Select a property from your portfolio.");

        if (portfolioProperty.PropiedadId is not > 0)
        {
            throw new InvalidOperationException("This property is not ready for document uploads yet.");
        }

        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("Sign in to upload documents.");
        }

        var ownsProperty = await db.Propiedades.AsNoTracking()
            .AnyAsync(p => p.Id == portfolioProperty.PropiedadId && p.UserId == userId, cancellationToken);
        if (!ownsProperty)
        {
            throw new InvalidOperationException("You do not have access to this property.");
        }

        var folder = Path.Combine(
            webHostEnvironment.WebRootPath,
            "uploads",
            "my-home",
            userId,
            portfolioProperty.PropiedadId.Value.ToString(CultureInfo.InvariantCulture));
        Directory.CreateDirectory(folder);

        var originalName = Path.GetFileName(file.FileName);
        var stored = $"{Guid.NewGuid():N}_{originalName}";
        var physical = Path.Combine(folder, stored);
        await using (var stream = System.IO.File.Create(physical))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var documentTitle = !string.IsNullOrWhiteSpace(title)
            ? title.Trim()
            : Path.GetFileNameWithoutExtension(originalName);

        db.PropiedadDocumentos.Add(new PropiedadDocumento
        {
            PropiedadId = portfolioProperty.PropiedadId.Value,
            Category = "Other",
            Title = documentTitle,
            FileName = originalName,
            StoragePath = $"/uploads/my-home/{userId}/{portfolioProperty.PropiedadId}/{stored}",
            ContentType = file.ContentType,
            SizeBytes = file.Length
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePortfolioPropertyAsync(int propertyId, CancellationToken cancellationToken = default)
    {
        var adminId = await ResolveAdministratorIdAsync(cancellationToken);
        if (adminId is not > 0)
        {
            return;
        }

        var row = await db.IndorPropertyAdminPortfolioProperties
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.AdministratorId == adminId, cancellationToken);

        if (row == null)
        {
            return;
        }

        db.IndorPropertyAdminPortfolioProperties.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AdvanceFromPropertiesAsync(CancellationToken cancellationToken = default)
    {
        var adminId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorPropertyAdministrators.FirstAsync(a => a.Id == adminId, cancellationToken);

        if (entity.CurrentStep < 4)
        {
            entity.CurrentStep = 4;
            entity.FechaActualizacion = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SaveToolsAsync(PropertyAdministratorToolsInput input, CancellationToken cancellationToken = default)
    {
        var adminId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorPropertyAdministrators.FirstAsync(a => a.Id == adminId, cancellationToken);

        entity.ToolMaintenanceRequests = input.ToolMaintenanceRequests;
        entity.ToolTurnoverCleaning = input.ToolTurnoverCleaning;
        entity.ToolGuestMessaging = input.ToolGuestMessaging;
        entity.ToolInvoicesPayments = input.ToolInvoicesPayments;
        entity.ToolDocumentsWarranties = input.ToolDocumentsWarranties;
        entity.ToolServiceProviders = input.ToolServiceProviders;
        entity.ToolTeamAccess = input.ToolTeamAccess;
        entity.NotifyUrgentMaintenance = input.NotifyUrgentMaintenance;
        entity.NotifyWeeklySummary = input.NotifyWeeklySummary;
        entity.NotifyBookingLeaseUpdates = input.NotifyBookingLeaseUpdates;
        entity.CurrentStep = 5;
        entity.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PropertyAdministratorReviewViewModel> GetReviewViewModelAsync(CancellationToken cancellationToken = default)
    {
        var adminId = await ResolveAdministratorIdAsync(cancellationToken);
        if (adminId is not > 0)
        {
            return new PropertyAdministratorReviewViewModel();
        }

        var entity = await db.IndorPropertyAdministrators
            .AsNoTracking()
            .FirstAsync(a => a.Id == adminId, cancellationToken);

        var properties = await GetPortfolioPropertiesAsync(cancellationToken);
        var state = MapFromEntity(entity);

        return new PropertyAdministratorReviewViewModel
        {
            DisplayStep = 5,
            TotalSteps = 5,
            Title = "Review & finish setup",
            Subtitle = "Confirm your portfolio details before opening your dashboard.",
            BackUrl = "",
            State = state,
            PropertyCount = properties.Count,
            PortfolioTypeLabel = PropertyAdministratorDisplayLocalization.LabelPortfolioType(entity.PortfolioType),
            ManagementStyleLabel = PropertyAdministratorDisplayLocalization.LabelManagementStyle(entity.ManagementStyle),
            AccountCreated = entity.TermsAccepted,
            PortfolioDetailsAdded = !string.IsNullOrWhiteSpace(entity.PortfolioType),
            PropertiesAdded = properties.Count > 0,
            ToolsSelected = entity.ToolMaintenanceRequests || entity.ToolTurnoverCleaning
        };
    }

    public async Task CompleteRegistrationAsync(bool platformTermsAccepted, CancellationToken cancellationToken = default)
    {
        var adminId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorPropertyAdministrators.FirstAsync(a => a.Id == adminId, cancellationToken);

        entity.PlatformTermsAccepted = platformTermsAccepted;
        entity.RegistrationStatus = PropertyAdministratorRegistrationStatuses.Completed;
        entity.RegistrationCompletedUtc = DateTime.UtcNow;
        entity.CurrentStep = 5;
        entity.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IndorPropertyAdministrator?> GetAdministratorForCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await db.IndorPropertyAdministrators
            .AsNoTracking()
            .Include(a => a.PortfolioProperties)
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
    }

    public string ResolveWizardResumeAction(int currentStep) => currentStep switch
    {
        <= 1 => "Profile",
        2 => "Portfolio",
        3 => "Properties",
        4 => "Tools",
        _ => "Review"
    };

    public bool IsRegistrationComplete(IndorPropertyAdministrator administrator) =>
        administrator.RegistrationStatus == PropertyAdministratorRegistrationStatuses.Completed
        && administrator.RegistrationCompletedUtc.HasValue;

    private async Task<int> EnsureDraftAsync(CancellationToken cancellationToken)
    {
        var session = httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");

        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        var id = session.GetInt32(AdminIdSessionKey);
        if (id is > 0)
        {
            var exists = await db.IndorPropertyAdministrators.AnyAsync(a => a.Id == id, cancellationToken);
            if (exists)
            {
                return id.Value;
            }
        }

        if (!string.IsNullOrEmpty(userId))
        {
            var byUser = await db.IndorPropertyAdministrators
                .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
            if (byUser != null)
            {
                session.SetInt32(AdminIdSessionKey, byUser.Id);
                return byUser.Id;
            }
        }

        var entity = new IndorPropertyAdministrator
        {
            UserId = userId,
            RegistrationStatus = PropertyAdministratorRegistrationStatuses.Draft,
            CurrentStep = 1,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorPropertyAdministrators.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        session.SetInt32(AdminIdSessionKey, entity.Id);
        return entity.Id;
    }

    private async Task<int?> ResolveAdministratorIdAsync(CancellationToken cancellationToken)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        var id = session?.GetInt32(AdminIdSessionKey);
        if (id is > 0)
        {
            return id;
        }

        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var admin = await db.IndorPropertyAdministrators.AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
        if (admin != null)
        {
            session?.SetInt32(AdminIdSessionKey, admin.Id);
            return admin.Id;
        }

        return null;
    }

    private static PropertyAdministratorRegistrationState MapFromEntity(IndorPropertyAdministrator entity) =>
        new()
        {
            DisplayName = entity.DisplayName ?? "",
            Email = entity.Email ?? "",
            Phone = entity.Phone ?? "",
            PortfolioBusinessName = entity.PortfolioBusinessName ?? "",
            TermsAccepted = entity.TermsAccepted,
            MarketingOptIn = entity.MarketingOptIn,
            PropertyCountRange = entity.PropertyCountRange ?? "",
            PortfolioType = entity.PortfolioType ?? "",
            OwnershipType = entity.OwnershipType ?? "",
            PrimaryMarket = entity.PrimaryMarket ?? "",
            ManagementStyle = entity.ManagementStyle ?? "",
            ToolMaintenanceRequests = entity.ToolMaintenanceRequests,
            ToolTurnoverCleaning = entity.ToolTurnoverCleaning,
            ToolGuestMessaging = entity.ToolGuestMessaging,
            ToolInvoicesPayments = entity.ToolInvoicesPayments,
            ToolDocumentsWarranties = entity.ToolDocumentsWarranties,
            ToolServiceProviders = entity.ToolServiceProviders,
            ToolTeamAccess = entity.ToolTeamAccess,
            NotifyUrgentMaintenance = entity.NotifyUrgentMaintenance,
            NotifyWeeklySummary = entity.NotifyWeeklySummary,
            NotifyBookingLeaseUpdates = entity.NotifyBookingLeaseUpdates
        };

    private static string BuildPropertyJson(
        PropertyAdministratorPropertyInput input,
        string location,
        string streetLine,
        string propertyName) =>
        $$"""{"propertyName":"{{propertyName}}","houseNumber":"{{input.HouseNumber?.Trim() ?? ""}}","streetName":"{{input.StreetName.Trim()}}","streetAddress":"{{streetLine}}","city":"{{input.City.Trim()}}","state":"{{input.State.Trim()}}","zipCode":"{{input.ZipCode?.Trim() ?? ""}}","location":"{{location}}","propertyType":"{{input.PropertyType.Trim()}}","source":"property_administrator_onboarding"}""";
}