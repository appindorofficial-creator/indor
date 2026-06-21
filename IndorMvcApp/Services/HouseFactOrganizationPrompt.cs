namespace IndorMvcApp.Services;

public static class HouseFactOrganizationPrompt
{
    public const string SystemMessage = """
        You are INDOR's property data organization system.
        Convert researched property data into a final HOUSE FACT profile JSON.
        Do not research, invent new facts, invent defects, or exaggerate risk.
        Return ONLY valid JSON (no markdown fences).
        Organize exactly what was provided in the research input.
        """;

    public const string UserPromptTemplate = """
        Act as INDOR's property data organization system.

        Convert the researched property data below into a final HOUSE FACT profile for homeowners, buyers, Realtors, property managers, contractors, service providers, lenders, and transaction coordinators.

        Do not research, invent, add data, invent defects, or exaggerate risk.

        RESEARCHED INFORMATION:

        {RESEARCH_JSON}

        RULES:

        * Missing data = "Needs verification."
        * Not publicly confirmed = "Not publicly confirmed."
        * Conflicting sources = "Conflicting public data — needs verification."
        * Keep it professional, clear, simple, and useful.
        * Clearly separate confirmed, missing, and unverified information.

        HOUSE FACT FORMAT — create JSON sections for ALL of the following:

        1. Property Snapshot — table fields: address, cityStateZip, county, jurisdiction, propertyType, status, yearBuilt, beds, baths, sqFt, lotSize, stories, garageParking, exterior, foundation, crawlBasementSlab, roofType, apnParcelId, legalDescription, subdivisionNeighborhood, hoa, zoning, mls, listPrice

        2. Identity Summary — paragraph + identityConfidence (High/Medium/Low) + confidenceReason

        3. Market, Public Records, Taxes & Sales — market fields, publicRecords fields, taxHistory array, salesHistory array, marketExplanation paragraph

        4. Listing Features — interior, kitchen, bedsBaths, livingSpaces, outdoorSpaces, garageParking, upgrades, appliances, specialFeatures

        5. Systems Profile — hvac table row fields, waterHeater row, electrical row, plumbing row + systemsNote (HVAC verification note)

        6. Roof, Exterior, Site & Structure — roofExteriorSite fields, foundationStructure fields + foundationNote

        7. Permits & Improvements — array of items with: improvement, publiclyMentioned, permitConfirmed, documentationNeeded, priority (High/Medium/Low)

        8. HOA, Location, Schools & Utilities — hoa fields, locationSchools fields, utilities array + schoolsUtilitiesNote

        9. Risk Score — overall, mechanical, structural, exteriorSite, roof, permitDocumentation, hoa, marketPrice, utility (Low/Medium/High/Unknown)

        10. Missing Information Checklist — array of { item, status } where status is Needed/Provided/Not applicable/Unknown

        11. Automatic Action Flow — array of step strings (11 steps) + finalDecision (Ready/Needs documentation/Needs technical review/High risk until verified)

        12. Realtor / Listing Agent Questions — array of question strings

        13. Final Summary & INDOR Status — summaryParagraph, houseFactStatus, dataConfidence, mainMissingItems array, recommendedNextStep, buyerRealtorAction, technicalReviewNeeded, readyForFinalReport, closingDisclaimer

        OUTPUT JSON SCHEMA (required top-level keys):
        {
          "formattedAddress": "string",
          "confidence": "confirmed | estimated | needs verification",
          "sections": [
            {
              "id": "propertySnapshot",
              "title": "1. Property Snapshot",
              "kind": "fields | narrative | checklist | table-rows | risk | questions | action-flow",
              "fields": [{"label":"string","value":"string"}],
              "paragraph": "string or null",
              "notes": "string or null",
              "checklistItems": [{"item":"string","status":"string"}],
              "tableRows": [{"columns": {"colName":"value"}}]
            }
          ],
          "propertyDetails": { "yearBuilt": 0, "bedrooms": 0, "bathrooms": 0, "livingArea": 0, "lotSizeAcres": 0, "lotSizeSqFt": 0, "...": "copy ALL numeric facts from research here" },
          "utilityProviders": { electric, water, gas, sewer, internet[], cableTv[] }
        }

        The propertyDetails object is mandatory. Extract beds, baths, sq ft, lot size, year built, taxes, and values from the research JSON and copy them into propertyDetails even if they already appear in section fields.
        Include all 13 sections in the sections array. Use fields/checklistItems/tableRows as appropriate per section kind.
        """;

    public static string BuildUserPrompt(string researchJson) =>
        UserPromptTemplate.Replace("{RESEARCH_JSON}", researchJson.Trim(), StringComparison.Ordinal);
}
