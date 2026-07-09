namespace IndorMvcApp.Localization;

public static class UiTranslationBuilder
{
    public static IReadOnlyDictionary<string, string> Merge(params IEnumerable<KeyValuePair<string, string>>[] sources)
    {
        var merged = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            foreach (var pair in source)
            {
                merged[pair.Key] = pair.Value;
            }
        }

        return merged;
    }
}
