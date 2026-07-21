using System.Globalization;
using IndorMvcApp.Localization;

namespace IndorMvcApp.Services;

public sealed class IndorLocalizer : IIndorLocalizer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IndorLocalizer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string CurrentCulture => UiCulture.Normalize(CultureInfo.CurrentUICulture.Name);

    public bool IsSpanish => UiCulture.IsSpanish(CurrentCulture);

    public string this[string key] => T(key);

    public string T(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        key = key.Trim();

        if (!IsSpanish)
        {
            return key;
        }

        return UiTranslations.Spanish.TryGetValue(key, out var translated)
            ? translated
            : key;
    }

    public string T(string key, params object[] args)
    {
        var text = T(key);
        return args.Length == 0 ? text : string.Format(CultureInfo.CurrentCulture, text, args);
    }
}
