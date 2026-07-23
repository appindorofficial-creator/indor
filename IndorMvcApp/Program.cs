using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Globalization;
using IndorMvcApp;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.ApplyOpenAiFromAppsettingsJson(builder.Environment);

// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddSingleton<IAppVersionService, AppVersionService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<PropertyEnrichmentCache>();
builder.Services.AddSingleton<HomeCatalogCache>();

builder.Services.AddRequestTimeouts(options =>
{
    options.AddPolicy("OnboardingAddressLookup", TimeSpan.FromMinutes(6));
    options.AddPolicy("OnboardingPropertyDetails", TimeSpan.FromMinutes(2));
});

var dataProtectionKeysPath = ResolveDataProtectionKeysPath(builder.Environment);
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("IndorMvcApp");

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.Name = "Indor.Session";
    // Survive brief Android WebView backgrounding (session cookies die with the process).
    options.Cookie.MaxAge = TimeSpan.FromHours(8);
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var enUs = new CultureInfo("en-US");
    var esUs = new CultureInfo("es-US");
    options.DefaultRequestCulture = new RequestCulture(enUs);
    options.SupportedCultures = [enUs, esUs];
    options.SupportedUICultures = [enUs, esUs];
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new UserProfileRequestCultureProvider());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IUiCultureCookieService, UiCultureCookieService>();
builder.Services.AddScoped<IIndorLocalizer, IndorLocalizer>();

// Configurar Entity Framework con SQL Server LocalDB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContextFactory<AppDbContext>(
    options => options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped);

builder.Services.AddScoped<HomeIndexQueryService>();
builder.Services.AddScoped<AccountDeletionService>();

// Configurar Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Password reset tokens (and other data-protection tokens) are valid for 24 hours.
builder.Services.Configure<Microsoft.AspNetCore.Identity.DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(24);
});

// Configurar cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/LoginForm";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "Indor.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    // Keep auth alive across Android WebView / PWA background kills.
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.Cookie.MaxAge = TimeSpan.FromDays(30);
});

// Register HttpClient and property services
builder.Services.Configure<OpenAiPropertyOptions>(builder.Configuration.GetSection(OpenAiPropertyOptions.SectionName));
builder.Services.AddSingleton<IPostConfigureOptions<OpenAiPropertyOptions>, OpenAiPropertyOptionsPostConfigure>();
builder.Services.Configure<GoogleMapsOptions>(builder.Configuration.GetSection(GoogleMapsOptions.SectionName));
builder.Services.Configure<AttomOptions>(builder.Configuration.GetSection(AttomOptions.SectionName));
builder.Services.AddHttpClient<OpenAiPropertyEnrichmentService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAiPropertyOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddHttpClient<IAttomPropertyService, AttomPropertyService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AttomOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IPropertyEnrichmentService, CompositePropertyEnrichmentService>();
builder.Services.AddHttpClient<IAddressLookupService, AddressLookupService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IProviderRegistrationService, ProviderRegistrationService>();
builder.Services.AddScoped<IProviderProDataService, ProviderProDataService>();
builder.Services.AddScoped<IProviderProJobWorkflowService, ProviderProJobWorkflowService>();
builder.Services.AddScoped<IProviderNetworkService, ProviderNetworkService>();
builder.Services.AddScoped<IContractorVerificationService, ContractorVerificationService>();
builder.Services.AddScoped<INetworkRequestsService, NetworkRequestsService>();
builder.Services.AddScoped<ProviderProDashboardService>();
builder.Services.AddScoped<RealtorGuidanceService>();
builder.Services.AddScoped<IRealtorRegistrationService, RealtorRegistrationService>();
builder.Services.AddScoped<IPropertyAdministratorRegistrationService, PropertyAdministratorRegistrationService>();
builder.Services.AddScoped<IPropertyAdministratorPortalService, PropertyAdministratorPortalService>();
builder.Services.AddScoped<IPropertyAdministratorEmergencyAcService, PropertyAdministratorEmergencyAcService>();
builder.Services.AddScoped<IPropertyAdministratorEmergencyElectricalService, PropertyAdministratorEmergencyElectricalService>();
builder.Services.AddScoped<IPropertyAdministratorEmergencyPlumbingService, PropertyAdministratorEmergencyPlumbingService>();
builder.Services.AddScoped<IPropertyAdministratorEmergencyRoofLeakService, PropertyAdministratorEmergencyRoofLeakService>();
builder.Services.AddScoped<IPropertyAdministratorEmergencyTreeBranchService, PropertyAdministratorEmergencyTreeBranchService>();
builder.Services.AddScoped<IPropertyAdministratorLockoutAccessService, PropertyAdministratorLockoutAccessService>();
builder.Services.AddScoped<IPropertyAdministratorBrokenWindowBoardUpService, PropertyAdministratorBrokenWindowBoardUpService>();
builder.Services.AddScoped<IPropertyAdministratorEmergencySewerBackupService, PropertyAdministratorEmergencySewerBackupService>();
builder.Services.AddScoped<IPropertyAdministratorEmergencyWaterHeaterService, PropertyAdministratorEmergencyWaterHeaterService>();
builder.Services.AddScoped<IPropertyAdministratorEmergencyFloodService, PropertyAdministratorEmergencyFloodService>();
builder.Services.AddScoped<IPropertyAdministratorPreventiveMaintenanceService, PropertyAdministratorPreventiveMaintenanceService>();
builder.Services.AddScoped<IPropertyAdministratorAirFilterService, PropertyAdministratorAirFilterService>();
builder.Services.AddScoped<IPropertyAdministratorSmokeDetectorService, PropertyAdministratorSmokeDetectorService>();
builder.Services.AddScoped<IPropertyAdministratorTurnoverCleaningService, PropertyAdministratorTurnoverCleaningService>();
builder.Services.AddScoped<IPropertyAdministratorStandardCleaningService, PropertyAdministratorStandardCleaningService>();
builder.Services.AddScoped<IPropertyAdministratorLinenRestockService, PropertyAdministratorLinenRestockService>();
builder.Services.AddScoped<IPropertyAdministratorPetDeepCleanService, PropertyAdministratorPetDeepCleanService>();
builder.Services.AddScoped<IPropertyAdministratorMovingHelpService, PropertyAdministratorMovingHelpService>();
builder.Services.AddScoped<IPropertyAdministratorJunkRemovalService, PropertyAdministratorJunkRemovalService>();
builder.Services.AddScoped<IPropertyAdministratorFurnitureHaulAwayService, PropertyAdministratorFurnitureHaulAwayService>();
builder.Services.AddScoped<IPropertyAdministratorTrashOutService, PropertyAdministratorTrashOutService>();
builder.Services.AddScoped<IPropertyAdministratorLawnCareService, PropertyAdministratorLawnCareService>();
builder.Services.AddScoped<IPropertyAdministratorLandscapingService, PropertyAdministratorLandscapingService>();
builder.Services.AddScoped<IPropertyAdministratorPressureWashingService, PropertyAdministratorPressureWashingService>();
builder.Services.AddScoped<IPropertyAdministratorPestControlService, PropertyAdministratorPestControlService>();
builder.Services.AddScoped<IPropertyAdministratorPoolHotTubService, PropertyAdministratorPoolHotTubService>();
builder.Services.AddScoped<IHomeownerPropertyService, HomeownerPropertyService>();
builder.Services.AddScoped<RealtorPortalService>();
builder.Services.AddScoped<RealtorPropertyFileInspectionBackfillService>();
builder.Services.AddScoped<RealtorNearbyNetworkService>();
builder.Services.AddScoped<HomeownerNearbyNetworkService>();
builder.Services.AddScoped<NeighborhoodFeedService>();
builder.Services.AddScoped<LawnCatalogService>();
builder.Services.AddScoped<NeighborRequestWizardService>();
builder.Services.AddScoped<RealtorSharedQuoteService>();
builder.Services.Configure<IndorMvcApp.Models.SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<IndorMvcApp.Models.InsuranceSettings>(builder.Configuration.GetSection("Insurance"));
builder.Services.Configure<IndorMvcApp.Models.StripeSettings>(builder.Configuration.GetSection(IndorMvcApp.Models.StripeSettings.SectionName));
builder.Services.AddScoped<IInvitationEmailSender, SmtpInvitationEmailSender>();
builder.Services.AddScoped<IPasswordResetEmailSender, SmtpPasswordResetEmailSender>();
builder.Services.AddScoped<IInsuranceCarrierEmailSender, SmtpInsuranceCarrierEmailSender>();
builder.Services.AddScoped<ITransactionalEmailSender, SmtpTransactionalEmailSender>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IInsuranceStripeCheckoutService, InsuranceStripeCheckoutService>();
builder.Services.AddHostedService<StartupWarmupService>();
builder.Services.AddScoped<IRealtorInviteClientService, RealtorInviteClientService>();
builder.Services.AddScoped<IRealtorPropertyFileWizardService, RealtorPropertyFileWizardService>();
builder.Services.AddScoped<IRealtorQuoteRequestService, RealtorQuoteRequestService>();
builder.Services.AddHttpClient<IOpenAiInspectionAnalysisService, OpenAiInspectionAnalysisService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAiPropertyOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromMinutes(10);
});
builder.Services.AddHttpClient<IOpenAiMaintenanceRecommendationService, OpenAiMaintenanceRecommendationService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAiPropertyOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(120);
});
builder.Services.AddScoped<IRealtorProviderBridgeService, RealtorProviderBridgeService>();
builder.Services.AddScoped<IRealtorInspectionUploadWizardService, RealtorInspectionUploadWizardService>();
builder.Services.AddScoped<IRealtorUrgentQuoteWizardService, RealtorUrgentQuoteWizardService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = RealtorInspectionUploadLimits.MaxFileBytes;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = RealtorInspectionUploadLimits.MaxFileBytes;
});

var app = builder.Build();

var openAiStartup = app.Configuration.GetSection(OpenAiPropertyOptions.SectionName);
var openAiEnabled = openAiStartup.GetValue<bool>("Enabled");
var openAiKeyPresent = !string.IsNullOrWhiteSpace(openAiStartup["ApiKey"]);
app.Logger.LogInformation(
    "OpenAI property enrichment: Enabled={Enabled}, ApiKeyConfigured={KeyConfigured}, ResearchModel={Model}, QuickFirst={QuickFirst}",
    openAiEnabled,
    openAiKeyPresent,
    openAiStartup["ResearchModel"] ?? openAiStartup["Model"] ?? "default",
    openAiStartup.GetValue("UseQuickEnrichmentFirst", true));

var showDetailedErrors = builder.Configuration.GetValue("Diagnostics:ShowDetailedErrors", true);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || showDetailedErrors)
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestTimeouts();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = static ctx =>
    {
        var path = ctx.Context.Request.Path.Value ?? string.Empty;

        // Stylesheets and scripts change with every deploy. Force the browser to
        // revalidate them on each load (cheap 304 via ETag) so published CSS/JS
        // changes are picked up immediately instead of being pinned for a year.
        if (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.CacheControl = "no-cache,must-revalidate";
        }
        // Fingerprint-stable binary assets can be cached aggressively.
        else if (path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
        }
    }
});
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseMiddleware<PreventWebViewCacheMiddleware>();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Welcome}/{id?}");

// Lightweight warmup/health endpoint. IIS Application Initialization (preload) hits this
// after an app-pool recycle so the EF model + DB connection are warm before any real user,
// avoiding the multi-second blank screen on first open.
app.MapGet("/healthz", async (AppDbContext db, CancellationToken ct) =>
{
    try
    {
        await db.Microservicios.AsNoTracking().Select(m => m.Id).FirstOrDefaultAsync(ct);
        return Results.Text("ok");
    }
    catch
    {
        return Results.Text("starting");
    }
}).AllowAnonymous();

if (args.Contains("--backfill-property-inspections", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var backfill = scope.ServiceProvider.GetRequiredService<RealtorPropertyFileInspectionBackfillService>();
    var result = await backfill.BackfillAsync();
    app.Logger.LogInformation(
        "Property inspection backfill complete: drafts={Drafts}, reports={Reports}, repairItems={Repairs}, phasesFixed={Phases}",
        result.DraftsProcessed,
        result.ReportsSynced,
        result.RepairItemsSynced,
        result.PhasesFixed);
    return;
}

app.Run();

static string ResolveDataProtectionKeysPath(IWebHostEnvironment environment)
{
    var home = Environment.GetEnvironmentVariable("HOME");
    if (!string.IsNullOrWhiteSpace(home))
    {
        return Path.Combine(home, "site", "data", "ProtectionKeys");
    }

    return Path.Combine(environment.ContentRootPath, "DataProtection-Keys");
}
