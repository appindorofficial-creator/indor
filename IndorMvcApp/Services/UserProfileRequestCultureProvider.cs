using IndorMvcApp.Localization;
using IndorMvcApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace IndorMvcApp.Services;

public sealed class UserProfileRequestCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var cookieService = httpContext.RequestServices.GetRequiredService<IUiCultureCookieService>();
        var fromCookie = cookieService.GetCulture(httpContext.Request);
        if (UiCulture.IsSupported(fromCookie))
        {
            return new ProviderCultureResult(fromCookie!);
        }

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(httpContext.User);
            if (user != null && UiCulture.IsSupported(user.PreferredUiCulture))
            {
                return new ProviderCultureResult(user.PreferredUiCulture!);
            }
        }

        var options = httpContext.RequestServices
            .GetRequiredService<IOptions<RequestLocalizationOptions>>()
            .Value;

        return new ProviderCultureResult(options.DefaultRequestCulture.Culture.Name);
    }
}
