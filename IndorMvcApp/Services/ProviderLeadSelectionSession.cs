namespace IndorMvcApp.Services;

public static class ProviderLeadSelectionSession
{
    private const string KeyPrefix = "ProviderLeadSelectedFindings:";

    public static void Set(Microsoft.AspNetCore.Http.ISession session, int leadId, IReadOnlyList<int> findingIndices)
    {
        session.SetString($"{KeyPrefix}{leadId}", string.Join(",", findingIndices));
    }

    public static IReadOnlyList<int> Get(Microsoft.AspNetCore.Http.ISession session, int leadId)
    {
        var raw = session.GetString($"{KeyPrefix}{leadId}");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var i) ? i : -1)
            .Where(i => i >= 0)
            .Distinct()
            .ToList();
    }

    public static void Clear(Microsoft.AspNetCore.Http.ISession session, int leadId) =>
        session.Remove($"{KeyPrefix}{leadId}");
}
