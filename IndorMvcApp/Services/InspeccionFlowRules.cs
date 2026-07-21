namespace IndorMvcApp.Services;

public static class InspeccionFlowRules
{
    public const string PrePurchaseHomeInspectionName = "Pre-Purchase Home Inspection";
    public const string ElectricalInspectionName = "Electrical Inspection";
    public const string CompleteHomeInspectionName = "Complete Home Inspection";
    public const string PlumbingInspectionName = "Plumbing Inspection";
    public const string HvacInspectionName = "HVAC Inspection";
    public const string StructuralInspectionName = "Structural Inspection";
    public const string FoundationInspectionName = "Foundation Inspection";
    public const string RoofInspectionName = "Roof Inspection";
    public const string MoldMoistureInspectionName = "Mold and Moisture Inspection";
    public const string WindowsInsulationInspectionName = "Windows and Insulation Inspection";
    public const string HomeSafetyInspectionName = "Home Safety Inspection";
    public const string InvestorInspectionName = "Investor Inspection";

    public static bool SupportsPurchaseFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            PrePurchaseHomeInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsElectricalFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            ElectricalInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsCompleteHomeFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            CompleteHomeInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsPlumbingFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            PlumbingInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsHvacFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            HvacInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsStructuralFlow(string? nombreInspeccion)
    {
        var name = nombreInspeccion?.Trim();
        return string.Equals(name, StructuralInspectionName, StringComparison.OrdinalIgnoreCase)
               || string.Equals(name, FoundationInspectionName, StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsRoofFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            RoofInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsMoldMoistureFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            MoldMoistureInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsWindowsInsulationFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            WindowsInsulationInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsHomeSafetyFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            HomeSafetyInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsInvestorFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            InvestorInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsCustomFlow(string? nombreInspeccion)
    {
        return SupportsPurchaseFlow(nombreInspeccion)
               || SupportsElectricalFlow(nombreInspeccion)
               || SupportsCompleteHomeFlow(nombreInspeccion)
               || SupportsPlumbingFlow(nombreInspeccion)
               || SupportsHvacFlow(nombreInspeccion)
               || SupportsStructuralFlow(nombreInspeccion)
               || SupportsRoofFlow(nombreInspeccion)
               || SupportsMoldMoistureFlow(nombreInspeccion)
               || SupportsWindowsInsulationFlow(nombreInspeccion)
               || SupportsHomeSafetyFlow(nombreInspeccion)
               || SupportsInvestorFlow(nombreInspeccion);
    }

    public static string? GetFlowAction(string? nombreInspeccion)
    {
        if (SupportsPurchaseFlow(nombreInspeccion))
        {
            return "PurchaseDetails";
        }

        if (SupportsElectricalFlow(nombreInspeccion))
        {
            return "ElectricalDetails";
        }

        if (SupportsCompleteHomeFlow(nombreInspeccion))
        {
            return "HomeReviewDetails";
        }

        if (SupportsPlumbingFlow(nombreInspeccion))
        {
            return "PlumbingDetails";
        }

        if (SupportsHvacFlow(nombreInspeccion))
        {
            return "HvacDetails";
        }

        if (SupportsStructuralFlow(nombreInspeccion))
        {
            return "StructuralDetails";
        }

        if (SupportsRoofFlow(nombreInspeccion))
        {
            return "RoofDetails";
        }

        if (SupportsMoldMoistureFlow(nombreInspeccion))
        {
            return "MoldMoistureDetails";
        }

        if (SupportsWindowsInsulationFlow(nombreInspeccion))
        {
            return "WindowsInsulationDetails";
        }

        if (SupportsHomeSafetyFlow(nombreInspeccion))
        {
            return "HomeSafetyDetails";
        }

        if (SupportsInvestorFlow(nombreInspeccion))
        {
            return "InvestorDetails";
        }

        return null;
    }
}
