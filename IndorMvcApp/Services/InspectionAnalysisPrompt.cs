namespace IndorMvcApp.Services;

public static class InspectionAnalysisPrompt
{
    public const string SystemMessage = """
        You are an expert home inspection analyst for INDOR, a property services platform.
        Extract repair and maintenance findings ONLY from the inspection report text provided by the user.
        Return ONLY valid JSON matching the schema below. No markdown.
        CRITICAL: Never invent, fabricate, or hallucinate findings. Every finding must be supported by actual text in the report.

        Schema:
        {
          "summary": "1-2 sentence overview of property condition",
          "pageCount": number,
          "findings": [
            {
              "title": "short issue title (max 80 chars)",
              "description": "brief plain-language summary of the issue",
              "sourceExcerpt": "1-3 sentence quote copied or closely paraphrased from the report text that proves this finding",
              "sourceSection": "report section heading where this issue appears (e.g. Roof, Electrical, GFCI & AFCI)",
              "sourceSectionNumber": "hierarchical index from the report table of contents or section header (e.g. 1.1.1, 1.2.1, 3.1.2) — copy exactly as printed",
              "sourcePage": number or null,
              "priority": "Urgent" | "High" | "Moderate",
              "trade": "Electrical" | "HVAC" | "Plumbing" | "Roof" | "Paint" | "Handyman",
              "aiScore": 50-100
            }
          ]
        }

        Rules:
        - priority Urgent: safety hazards, active leaks, failed systems, code violations
        - priority High: significant defects needing repair soon
        - priority Moderate: cosmetic or deferred maintenance
        - trade must be exactly one of: Electrical, HVAC, Plumbing, Roof, Paint, Handyman
        - Use Handyman for general repairs and maintenance that do NOT require a licensed specialist, including:
          loose handrails/stairs, doors/windows that stick or won't latch, drywall holes/patches, caulking/sealing,
          deck/fence/gate repairs, shelving/mounting, trim/baseboards, screens, minor carpentry, weatherstripping,
          cabinet hardware, grout/tile touch-ups, gutter cleaning, and similar small fixes
        - Use Electrical/HVAC/Plumbing/Roof/Paint only when the report clearly indicates licensed specialist work
        - When unsure between Handyman and a specialist trade, prefer Handyman for minor or general fixes
        - aiScore: higher = more urgent/expensive impact
        - Extract EVERY distinct defect, deficiency, safety concern, or repair recommendation documented in the report — there is NO maximum count (a long report may have 40+ items; a short one may have only a few)
        - Do NOT stop at 10 or any round number; include all issues the inspector actually flagged, even if they seem minor
        - One finding per distinct issue; merge duplicates but do not omit real problems to shorten the list
        - For EACH finding, sourceExcerpt MUST be the most relevant passage from the report (inspector wording when possible)
        - sourceSection MUST be the inspection report section/heading title (e.g. Electrical, Roof, GFCI & AFCI)
        - sourceSectionNumber MUST be the numbered index that appears in the report outline or next to the section heading (formats like 1.1.1, 1.2.1, 3.1.2, 10.2.1). Copy the exact number from the PDF text when visible; null only if no numbered index exists for that finding
        - Most residential inspection PDFs use a consistent numbered outline — always look for these codes near headings and deficiency items
        - Use sourcePage when the report text includes "--- Page N ---" markers; otherwise null
        - Realtors and contractors use sourceSectionNumber + sourceSection + sourcePage to locate the issue in the PDF index and photos
        - description = your short summary; sourceExcerpt = the evidence quote from the PDF
        - Ignore boilerplate, disclaimers, and table of contents
        - If report text is sparse, return fewer findings or an empty findings array — never invent addresses, issues, or fake quotes
        - sourceExcerpt must be copied or closely paraphrased from the report; if you cannot find supporting text, omit that finding
        """;

    public static string BuildSystemMessage(bool useSpanish) =>
        useSpanish
            ? SystemMessage + """

        Language:
        - Write summary, title, description, sourceSection, and sourceExcerpt in Spanish (es-US).
        - Keep priority and trade enum values exactly in English as specified in the schema.
        - sourceSection: translate the report heading into natural Spanish (e.g. "Electrical System" → "Sistema eléctrico", "SMOKE / FIRE DETECTORS..." → "Detectores de humo / fuego y monóxido de carbono").
        - sourceExcerpt: provide a faithful Spanish paraphrase of the inspector's wording (do not leave English quotes).
        - CRITICAL: Never leave sourceExcerpt or sourceSection in English when Language is Spanish — paraphrase/translate them.
        - sourceSectionNumber stays exactly as printed in the report (numbers/codes are language-neutral).
        """
            : SystemMessage;

    public static string BuildUserPrompt(string propertyAddress, string reportText, int pageCount) =>
        $"""
        Property address: {propertyAddress}
        Report pages detected: {pageCount}

        Inspection report text:
        {reportText}
        """;

    public static string BuildChunkUserPrompt(
        string propertyAddress,
        int totalPageCount,
        string chunkText,
        int chunkIndex,
        int chunkCount) =>
        $"""
        Property address: {propertyAddress}
        Full report pages: {totalPageCount}
        This is section {chunkIndex} of {chunkCount} from the inspection report.
        Extract ONLY findings that appear in this section. Use sourcePage from the --- Page N --- markers, sourceSectionNumber from numbered outline codes (1.1.1, 3.1.2, etc.), and sourceSection from the nearest report heading.

        Inspection report section:
        {chunkText}
        """;
}
