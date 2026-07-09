namespace IndorMvcApp.Services;

public interface IIndorLocalizer
{
    string this[string key] { get; }

    string T(string key);

    string T(string key, params object[] args);

    bool IsSpanish { get; }

    string CurrentCulture { get; }
}
