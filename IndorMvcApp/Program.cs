using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddRequestTimeouts(options =>
{
    options.AddPolicy("OnboardingAddressLookup", TimeSpan.FromMinutes(3));
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
});

// Configurar Entity Framework con SQL Server LocalDB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

// Register HttpClient and property services
builder.Services.Configure<OpenAiPropertyOptions>(builder.Configuration.GetSection(OpenAiPropertyOptions.SectionName));
builder.Services.Configure<GoogleMapsOptions>(builder.Configuration.GetSection(GoogleMapsOptions.SectionName));
builder.Services.Configure<AttomOptions>(builder.Configuration.GetSection(AttomOptions.SectionName));
builder.Services.AddHttpClient<OpenAiPropertyEnrichmentService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAiPropertyOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(180);
});
builder.Services.AddHttpClient<IAttomPropertyService, AttomPropertyService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AttomOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IPropertyEnrichmentService, CompositePropertyEnrichmentService>();
builder.Services.AddHttpClient<IAddressLookupService, AddressLookupService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IProviderRegistrationService, ProviderRegistrationService>();
builder.Services.AddScoped<IProviderProDataService, ProviderProDataService>();
builder.Services.AddScoped<IProviderProJobWorkflowService, ProviderProJobWorkflowService>();
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
builder.Services.AddScoped<RealtorNearbyNetworkService>();
builder.Services.AddScoped<HomeownerNearbyNetworkService>();
builder.Services.AddScoped<LawnCatalogService>();
builder.Services.AddScoped<NeighborRequestWizardService>();
builder.Services.AddScoped<RealtorSharedQuoteService>();
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
        if (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase)
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
app.UseMiddleware<PreventWebViewCacheMiddleware>();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Welcome}/{id?}");


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
