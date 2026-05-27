namespace IndorMvcApp.Services;

public static class EmergencyFlowRules
{
    public const string HvacEmergencyName = "HVAC";
    public const string PlumbingEmergencyName = "Plumbing";
    public const string WaterHeaterEmergencyName = "Water Heater";
    public const string FloodEmergencyName = "Flood";
    public const string ElectricalEmergencyName = "Electrical";
    public const string TreeDamageEmergencyName = "Tree Damage";
    public const string RoofLeakEmergencyName = "Roof Leak";
    public const string SmokeDetectorEmergencyName = "Smoke Detector";

    public static bool SupportsSmokeDetectorEmergencyFlow(string? nombreServicio)
    {
        return string.Equals(
            nombreServicio?.Trim(),
            SmokeDetectorEmergencyName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsRoofLeakEmergencyFlow(string? nombreServicio)
    {
        return string.Equals(
            nombreServicio?.Trim(),
            RoofLeakEmergencyName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsTreeDamageEmergencyFlow(string? nombreServicio)
    {
        return string.Equals(
            nombreServicio?.Trim(),
            TreeDamageEmergencyName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsElectricalEmergencyFlow(string? nombreServicio)
    {
        return string.Equals(
            nombreServicio?.Trim(),
            ElectricalEmergencyName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsFloodEmergencyFlow(string? nombreServicio)
    {
        return string.Equals(
            nombreServicio?.Trim(),
            FloodEmergencyName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsWaterHeaterEmergencyFlow(string? nombreServicio)
    {
        return string.Equals(
            nombreServicio?.Trim(),
            WaterHeaterEmergencyName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsHvacEmergencyFlow(string? nombreServicio)
    {
        return string.Equals(
            nombreServicio?.Trim(),
            HvacEmergencyName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsPlumbingEmergencyFlow(string? nombreServicio)
    {
        return string.Equals(
            nombreServicio?.Trim(),
            PlumbingEmergencyName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static string? GetDetailsActionName(string? nombreServicio)
    {
        if (SupportsRoofLeakEmergencyFlow(nombreServicio))
        {
            return "EmergencyRoofLeakDescribe";
        }

        if (SupportsSmokeDetectorEmergencyFlow(nombreServicio))
        {
            return "EmergencySmokeDetectorDetails";
        }

        if (SupportsTreeDamageEmergencyFlow(nombreServicio))
        {
            return "EmergencyTreeDamageDescribe";
        }

        if (SupportsElectricalEmergencyFlow(nombreServicio))
        {
            return "EmergencyElectricalProblem";
        }

        if (SupportsFloodEmergencyFlow(nombreServicio))
        {
            return "EmergencyFloodDetails";
        }

        if (SupportsWaterHeaterEmergencyFlow(nombreServicio))
        {
            return "EmergencyWaterHeaterIssue";
        }

        if (SupportsHvacEmergencyFlow(nombreServicio))
        {
            return "EmergencyHvacDetails";
        }

        if (SupportsPlumbingEmergencyFlow(nombreServicio))
        {
            return "EmergencyPlumbingDetails";
        }

        return null;
    }
}
