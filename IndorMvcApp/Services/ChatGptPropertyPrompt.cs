namespace IndorMvcApp.Services;

/// <summary>
/// User prompt styled like ChatGPT property lookup. System/schema comes from HouseFactQuickPrompt.
/// </summary>
public static class ChatGptPropertyPrompt
{
    public static string BuildSearchUserPrompt(string address) =>
        $"""
        Search the web (Redfin, Zillow, Realtor.com — same as ChatGPT) for this exact property:

        {address.Trim()}

        Copy year built, beds, baths, interior living sq ft, lot size, estimate, and county exactly from the listing.
        Return ONLY the JSON shape from your system instructions (propertyDetails required).
        """;
}
