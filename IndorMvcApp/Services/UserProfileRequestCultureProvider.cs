using IndorMvcApp.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace IndorMvcApp.Services;

public sealed class UserProfileRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var cookieService = httpContext.RequestServices.GetRequiredService<IUiCultureCookieService>();
        var fromCookie = cookieService.GetCulture(httpContext.Request);
        if (UiCulture.IsSupported(fromCookie))
        {
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(fromCookie!));
        }

        var options = httpContext.RequestServices
            .GetRequiredService<IOptions<RequestLocalizationOptions>>()
            .Value;

        return Task.FromResult<ProviderCultureResult?>(
            new ProviderCultureResult(options.DefaultRequestCulture.Culture.Name));
    }
}
