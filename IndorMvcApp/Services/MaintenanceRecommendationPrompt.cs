using System.Text.Json;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class MaintenanceRecommendationPrompt
{
    public const string SystemMessage = """
        You are an expert home maintenance advisor for INDOR.
        Given property facts (year built, size, location, systems, climate), recommend practical maintenance tasks.
        Return ONLY valid JSON matching this schema:

        {
          "summary": "1-2 sentence overview of the home's maintenance priorities",
          "items": [
            {
              "title": "short task name",
              "description": "what to do and why (1-2 sentences)",
              "category": "HVAC" | "Plumbing" | "Electrical" | "Roof" | "Exterior" | "Safety" | "Landscaping" | "General",
              "priority": "Urgent" | "High" | "Routine",
              "frequency": "e.g. Every 3 months, Annually, Before winter",
              "icon": "fontawesome icon class without fa- prefix e.g. fan, faucet-drip, bolt",
              "reason": "brief why this matters for this specific home"
            }
          ]
        }

        Rules:
        - Recommend 6-10 distinct, actionable maintenance items
        - Tailor EVERY item to the specific property: year built, climate/region, home size, systems, and enrichment data
        - Reference concrete property traits in each "reason" field (e.g. age of home, HVAC type, lot size, region)
        - Urgent = safety or prevents major damage; High = important seasonal/preventive; Routine = regular upkeep
        - Use realistic US homeowner maintenance only — never generic copy-paste lists
        - Do not invent specific dollar amounts or claim inspections were performed
        - If property data is sparse, still personalize using address region and typical risks for that area
        """;

    public static string BuildUserPrompt(PropertyInfoViewModel info)
    {
        var d = info.PropertyDetails;
        var payload = new
        {
            address = info.FormattedAddress,
            city = info.City,
            state = info.State,
            county = info.County ?? d?.CountyName,
            propertyType = d?.PropertyType,
            yearBuilt = d?.YearBuilt,
            yearRenovated = d?.YearRenovated,
            livingAreaSqFt = d?.LivingArea,
            lotSizeAcres = d?.LotSize,
            bedrooms = d?.Bedrooms,
            bathrooms = d?.Bathrooms,
            floors = d?.Floors,
            buildingCondition = d?.BuildingCondition,
            wallType = d?.WallType,
            heatingType = d?.HeatingType,
            coolingType = d?.CoolingType,
            estimatedValue = d?.EstimatedValue,
            dataSource = info.DataSource,
            enrichmentExcerpt = Truncate(info.AttomRawJson, 6000)
        };

        return "Analyze this property and recommend maintenance:\n" + JsonSerializer.Serialize(payload);
    }

    private static string? Truncate(string? text, int max)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= max) return text;
        return text[..max];
    }
}
