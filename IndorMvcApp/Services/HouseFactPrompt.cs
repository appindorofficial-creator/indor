namespace IndorMvcApp.Services;

public static class HouseFactPrompt
{
    public const string AddressPlaceholder = "[PASTE FULL PROPERTY ADDRESS HERE]";

    public const string SystemMessage = """
        You are a real estate and due diligence analyst for INDOR House Fact profiles.
        Return ONLY valid JSON (no markdown fences).

        Fill as many fields as possible. Label estimates with "Estimated:" when not parcel-specific.
        Reserve "Not publicly confirmed — needs verification." ONLY for parcel-specific facts you truly cannot determine.

        In propertyDetails, use NUMBERS for yearBuilt, bedrooms, bathrooms, livingArea, lotSizeAcres, estimatedValue, annualTaxAmount when known or reasonably estimated.
        """;

    public const string WebSearchSystemMessage = """
        You are INDOR's property research analyst with live web search.
        Search Zillow, Redfin, Realtor.com, Homes.com, county assessor/GIS, and local public sources for the exact address provided.
        Collect confirmed public listing and property record data with source names.
        Return ONLY valid JSON (no markdown fences) using the same schema as the research output format below.
        Include source names in mainSourcesUsed and in each section where data was found.
        Do NOT fabricate parcel IDs, MLS numbers, or sale dates — only include values found in search results.
        """;

    public const string WebSearchUserPrefix = """
        Use web search NOW for this exact property address before answering.

        Search these sources (at minimum): Zillow, Redfin, Realtor.com, Mecklenburg County assessor/GIS, Canopy MLS references.

        PROPERTY ADDRESS:

        {ADDRESS}

        Collect ALL available public data: parcel/APN, subdivision, year built, beds, baths, sq ft, lot, list price, MLS status, taxes, sale history, schools, utilities, zoning, HOA, and listing features.

        """;

    public const string UserPromptTemplate = """
        Act as a real estate, public records, and technical due diligence analyst for an INDOR “House Fact” profile.

        Research this property and collect the key public information needed to create a House Fact.

        PROPERTY ADDRESS:

        {ADDRESS}

        Use reliable sources such as Zillow, Realtor.com, Redfin, Homes.com, Compass, public MLS data, county property records, tax records, GIS/parcel records, assessor records, permits, flood maps, school sources, and utility/provider lookup tools when available.

        RULES:

        * Do not fabricate exact parcel IDs, MLS numbers, or precise transaction dates.
        * DO populate fields using address-level and market-level knowledge (this is required — empty profiles are not acceptable).
        * For each section, prefer realistic estimates over blank or "needs verification" defaults.
        * Prefix estimated narrative values with "Estimated:" when not parcel-specific.
        * Use "Not publicly confirmed — needs verification." only for exact records you cannot infer (APN, MLS #, exact sale date/price, permit numbers).
        * If sources conflict, explain the conflict.
        * List sources that informed your estimates (market knowledge, county, ZIP, regional norms).
        * Separate confirmed data from estimated data in wording, not by leaving fields empty.
        * Do not invent defects or exaggerate risk.

        RESEARCH STRATEGY (required):

        1. Parse city, county, state, and ZIP from the address.
        2. Infer property type, size, year built, beds/baths, and value range typical for that street/submarket.
        3. Assign likely utility providers for that county/municipality (electric, water, sewer, gas, internet).
        4. Assign likely school district and schools serving that ZIP.
        5. Note typical NC/ regional construction: crawl space or slab, brick/vinyl exterior, central HVAC, etc.
        6. For Mecklenburg County / Mint Hill / Charlotte area: consider Duke Energy Carolinas (electric), Piedmont Natural Gas, Charlotte-Mecklenburg Schools, Mint Hill or Charlotte Water service areas.
        7. Provide a useful itemsNeedingVerification checklist even when estimates are present.

        DELIVER THE INFORMATION IN THIS ORDER:

        1. Property Identity

        Provide:

        * Full address
        * City, state, ZIP
        * County
        * Parcel ID / APN
        * Subdivision / neighborhood
        * Property type
        * Land use
        * Zoning
        * Current status
        * Main sources used

        2. Basic Property Facts

        Provide:

        * Year built
        * Bedrooms
        * Bathrooms
        * Heated square footage
        * Lot size
        * Stories
        * Garage / parking
        * Exterior material
        * Foundation type
        * Crawl space / basement / slab
        * Roof type, if available
        * Porch, deck, patio, fence, pool, fireplace, or other key features

        Note any conflicts between sources.

        3. Listing / Market Data

        If listed or recently listed, provide:

        * List price
        * MLS number
        * Status
        * Listing date
        * Days on market
        * Price per square foot
        * Listing agent
        * Brokerage
        * Listing summary
        * Important features / upgrades
        * Price changes, if available

        4. Public Records / Taxes

        Provide:

        * Assessed value
        * Land value
        * Improvement value
        * Annual taxes
        * Tax year
        * Last assessment year
        * Major assessment changes, if any

        5. Sales History

        Provide:

        * Sale date
        * Sale price
        * Transaction/listing event
        * Source

        If possible, calculate current price vs. last sale:

        * Dollar difference
        * Percentage difference

        6. Mechanical / Utility Systems

        Find public information about:

        * HVAC heating/cooling type
        * Number of HVAC systems, if available
        * Fuel source
        * HVAC age, replacement, permit, or warranty mention
        * Water heater type, fuel source, age, permit, or warranty mention
        * Electrical panel or upgrades, if available
        * Plumbing, water source, sewer/septic, gas service, and plumbing updates

        If ages are missing, write:

        “System age not publicly confirmed — needs verification by serial number, permit, inspection report, or seller documentation.”

        7. Roof / Exterior / Site

        Provide:

        * Roof type and age, if available
        * Exterior material
        * Gutters, drainage, grading, driveway
        * Porch, deck, patio, fence
        * Trees near structure or lot slope, if mentioned
        * Flood zone or flood risk, if available
        * Moisture or drainage concerns, if publicly mentioned

        8. Foundation / Structure

        Provide:

        * Foundation type
        * Crawl space / slab / basement
        * Public mention of structural repairs
        * Public mention of moisture, settlement, termite history, or drainage concerns

        Do not invent defects. Only state what is found or what needs verification.

        9. Permits / Improvements

        Search for:

        * HVAC permits
        * Roof permits
        * Water heater permits
        * Electrical permits
        * Plumbing permits
        * Deck / porch / addition permits
        * Remodel or structural permits

        If not found, write:

        “Permit history not publicly confirmed — should be verified with county/city permit records and seller documentation.”

        10. HOA / Community

        If applicable, provide:

        * HOA status
        * HOA name
        * HOA fee
        * Fee frequency
        * Amenities
        * Restrictions / architectural review requirements
        * HOA documents needed

        11. Schools / Location / Utilities

        Provide:

        * Elementary, middle, and high school
        * School district
        * Neighborhood / area name
        * Nearby major roads or location notes
        * Water source/provider
        * Sewer/septic
        * Gas service
        * Electric provider, if available
        * Trash/recycling provider, if available
        * Internet/cable providers, if available

        If school data is not official, write:

        “School assignments should be verified directly with the school district.”

        12. Key Items Needing Verification

        Create a checklist for:

        * Seller disclosure
        * Inspection report
        * HVAC serial numbers / install dates / warranty
        * Water heater serial number / install date
        * Roof age / warranty
        * Permit history
        * Electrical panel details
        * Plumbing updates
        * Foundation / crawl space / slab condition
        * Drainage or moisture history
        * Termite bond or treatment history
        * Insurance claims
        * HOA documents
        * Utility providers
        * Renovation invoices
        * Contractor receipts
        * Product warranties

        13. Sources

        List all sources used with:

        * Source name
        * Link, if available
        * Information found
        * Any conflicts with another source

        OUTPUT FORMAT (required):

        Return ONLY one JSON object with these top-level keys:
        - propertyIdentity (object for section 1)
        - basicPropertyFacts (object for section 2)
        - listingMarketData (object for section 3)
        - publicRecordsTaxes (object for section 4)
        - salesHistory (object or array for section 5)
        - mechanicalUtilitySystems (object for section 6)
        - roofExteriorSite (object for section 7)
        - foundationStructure (object for section 8)
        - permitsImprovements (object for section 9)
        - hoaCommunity (object for section 10)
        - schoolsLocationUtilities (object for section 11)
        - itemsNeedingVerification (array of strings for section 12)
        - sources (array of objects with sourceName, link, informationFound, conflicts for section 13)
        - formattedAddress (string)
        - confidence ("confirmed", "estimated", or "needs verification" — use "estimated" when most fields are inferred from address/market knowledge)
        - propertyDetails (object — numeric fields MUST be numbers when estimated: yearBuilt, livingArea, lotSizeAcres, lotSizeSqFt, bedrooms, bathrooms, floors, lastSalePrice, estimatedValue, estimatedValueYear, annualTaxAmount, taxYear, basementSqFt, fireplaces, roomsTotal, bathsFull; strings for propertyType, parcelNumber only when unknown use verification text)
        - utilityProviders (object with electric, water, gas, sewer, internet[], cableTv[] — each provider: name, serviceType, phone, website, coverage, notes)
        """;

    public static string BuildUserPrompt(string address) =>
        UserPromptTemplate.Replace("{ADDRESS}", address.Trim(), StringComparison.Ordinal);

    public static string BuildWebResearchPrompt(string address) =>
        WebSearchUserPrefix.Replace("{ADDRESS}", address.Trim(), StringComparison.Ordinal) + UserPromptTemplate.Replace("{ADDRESS}", address.Trim(), StringComparison.Ordinal);
}
