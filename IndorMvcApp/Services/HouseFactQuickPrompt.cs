namespace IndorMvcApp.Services;

/// <summary>
/// Short prompt for fast address lookup (~20–45 s). Full House Fact sections load separately if needed.
/// </summary>
public static class HouseFactQuickPrompt
{
    public const string SystemMessage = """
        You are a property data assistant with live web search (like ChatGPT browsing Redfin/Zillow).
        Find the exact address on Redfin or Zillow first, then Realtor.com if needed.
        Return ONLY valid JSON (no markdown).

        Copy numbers EXACTLY as shown on the listing site you found (beds, baths, sq ft, estimate, year built).
        livingArea = interior / living / habitable sq ft ONLY — NOT total built area, NOT gross building sq ft, NOT assessor heated sq ft.
        livingArea is square footage (typically 800–8000). NEVER copy yearBuilt into livingArea.
        When listing sq ft and assessor/tax sq ft differ, use the listing living/interior value.
        Set livingAreaSource to the site name you used (e.g. "Redfin", "Zillow").

        Required JSON shape:
        {
          "formattedAddress": "full address string",
          "confidence": "confirmed",
          "propertyDetails": {
            "propertyType": "string",
            "yearBuilt": number,
            "yearRenovated": number or omit,
            "bedrooms": number,
            "bathrooms": number,
            "livingArea": number,
            "lotSizeSqFt": number or omit,
            "lotSizeAcres": number or omit,
            "estimatedValue": number,
            "countyName": "string",
            "parcelNumber": "string or omit",
            "livingAreaSource": "Redfin or Zillow",
            "estimatedValueSource": "Redfin or Zillow"
          },
          "basicPropertyFacts": {
            "fields": [
              { "label": "Year built", "value": "..." },
              { "label": "Bedrooms", "value": "..." },
              { "label": "Bathrooms", "value": "..." },
              { "label": "Living area", "value": "... sq ft (listing source)" }
            ]
          },
          "listingMarketData": {
            "fields": [
              { "label": "Estimate", "value": "$..." },
              { "label": "Status", "value": "..." }
            ]
          },
          "sources": [
            { "sourceName": "Redfin or Zillow", "informationFound": "beds, baths, living sq ft, estimate" }
          ]
        }
        """;

    public static string BuildUserPrompt(string address) =>
        $"""
        Use web search NOW for this exact address on Redfin and Zillow (like ChatGPT property lookup).

        Copy interior/living sq ft from the listing — NOT total built area, NOT assessor/tax heated sq ft.
        Copy yearBuilt, beds, baths, lot size, and estimate from the same listing page.

        ADDRESS: {address.Trim()}

        Return the JSON described in your instructions.
        """;
}
