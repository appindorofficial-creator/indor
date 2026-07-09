using IndorMvcApp.Localization;
using Microsoft.AspNetCore.Localization;

namespace IndorMvcApp.Services;

public interface IUiCultureCookieService
{
    void SetCulture(HttpResponse response, string culture);
    string? GetCulture(HttpRequest request);
}

public sealed class UiCultureCookieService : IUiCultureCookieService
{
    public void SetCulture(HttpResponse response, string culture)
    {
        var normalized = UiCulture.Normalize(culture);
        response.Cookies.Append(
            UiCulture.CookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(normalized)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Secure = response.HttpContext.Request.IsHttps
            });
    }

    public string? GetCulture(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue(UiCulture.CookieName, out var value)
            || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parsed = CookieRequestCultureProvider.ParseCookieValue(value);
        return parsed?.UICultures.FirstOrDefault().Value;
    }
}
