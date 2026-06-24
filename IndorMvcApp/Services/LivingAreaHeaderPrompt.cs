namespace IndorMvcApp.Services;

/// <summary>
/// Focused listing-site lookup for living/interior sq ft when general enrichment looks wrong.
/// </summary>
public static class LivingAreaHeaderPrompt
{
    public const string SystemMessage = """
        You are a property data assistant with live web search.
        Find the exact address on Redfin or Zillow (whichever has the property first).
        Return ONLY valid JSON (no markdown):

        {
          "livingArea": number,
          "livingAreaSource": "Redfin or Zillow — whichever site you used"
        }

        Rules:
        - livingArea = interior / living / habitable sq ft from the listing (header or facts section).
        - Do NOT use total built area, gross building area, assessor heated sq ft, tax records, or year built.
        - livingArea is square footage (usually 800–8000). It must NOT equal yearBuilt.
        - Set livingAreaSource to the actual site name (e.g. "Redfin", "Zillow").
        """;

    public static string BuildUserPrompt(string address) =>
        $"""
        Search Redfin and Zillow for this exact property. Copy the interior/living sq ft (NOT total built area).

        ADDRESS: {address.Trim()}

        Return JSON with livingArea and livingAreaSource from the listing site you found.
        """;
}
