using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class PropertyEnrichmentMapper
{
    public static bool ApplyPayload(PropertyInfoViewModel info, string rawJson)
    {
        var root = JsonNode.Parse(rawJson);
        if (root == null)
        {
            return false;
        }

        var detailsNode = root["propertyDetails"] ?? root["PropertyDetails"];
        if (detailsNode != null)
        {
            info.PropertyDetails ??= new PropertyDetailsInfo();
            ApplyDetails(info.PropertyDetails, detailsNode);
        }

        var utilitiesNode = root["utilityProviders"] ?? root["UtilityProviders"];
        if (utilitiesNode != null)
        {
            ApplyUtilities(info, utilitiesNode);
        }

        var formatted = ReadString(root["formattedAddress"] ?? root["FormattedAddress"]);
        if (!string.IsNullOrWhiteSpace(formatted))
        {
            info.FormattedAddress = formatted!;
        }

        return detailsNode != null;
    }

    private static void ApplyDetails(PropertyDetailsInfo details, JsonNode node)
    {
        details.PropertyType = ReadString(node["propertyType"]) ?? details.PropertyType;
        details.YearBuilt = ReadInt(node["yearBuilt"]) ?? details.YearBuilt;
        details.YearRenovated = ReadInt(node["yearRenovated"]) ?? details.YearRenovated;
        details.LivingArea = ReadInt(node["livingArea"]) ?? details.LivingArea;
        details.LotSize = ReadDecimal(node["lotSizeAcres"]) ?? details.LotSize;
        details.LotSizeSqFt = ReadInt(node["lotSizeSqFt"]) ?? details.LotSizeSqFt;
        details.Bedrooms = ReadInt(node["bedrooms"]) ?? details.Bedrooms;
        details.Bathrooms = ReadDecimal(node["bathrooms"]) ?? details.Bathrooms;
        details.Floors = ReadInt(node["floors"]) ?? details.Floors;
        details.ArchitecturalStyle = ReadString(node["architecturalStyle"]) ?? details.ArchitecturalStyle;
        details.LastSalePrice = ReadDecimal(node["lastSalePrice"]) ?? details.LastSalePrice;
        details.LastSaleDate = ReadDate(node["lastSaleDate"]) ?? details.LastSaleDate;
        details.EstimatedValue = ReadDecimal(node["estimatedValue"]) ?? details.EstimatedValue;
        details.EstimatedValueYear = ReadInt(node["estimatedValueYear"]) ?? details.EstimatedValueYear;
        details.AnnualTaxAmount = ReadDecimal(node["annualTaxAmount"]) ?? details.AnnualTaxAmount;
        details.TaxYear = ReadInt(node["taxYear"]) ?? details.TaxYear;
        details.ParcelNumber = ReadString(node["parcelNumber"]) ?? details.ParcelNumber;
        details.LegalDescription = ReadString(node["legalDescription"]) ?? details.LegalDescription;
        details.Zoning = ReadString(node["zoning"]) ?? details.Zoning;
        details.AssignedSchool = ReadString(node["assignedSchool"]) ?? details.AssignedSchool;
        details.Fips = ReadString(node["fips"]) ?? details.Fips;
        details.Subdivision = ReadString(node["subdivision"]) ?? details.Subdivision;
        details.Municipality = ReadString(node["municipality"]) ?? details.Municipality;
        details.CountyName = ReadString(node["countyName"]) ?? details.CountyName;
        details.Occupancy = ReadString(node["occupancy"]) ?? details.Occupancy;
        details.YearBuiltEffective = ReadInt(node["yearBuiltEffective"]) ?? details.YearBuiltEffective;
        details.RoomsTotal = ReadInt(node["roomsTotal"]) ?? details.RoomsTotal;
        details.BathsFull = ReadInt(node["bathsFull"]) ?? details.BathsFull;
        details.HeatingType = ReadString(node["heatingType"]) ?? details.HeatingType;
        details.HeatingFuel = ReadString(node["heatingFuel"]) ?? details.HeatingFuel;
        details.CoolingType = ReadString(node["coolingType"]) ?? details.CoolingType;
        details.BuildingCondition = ReadString(node["buildingCondition"]) ?? details.BuildingCondition;
        details.WallType = ReadString(node["wallType"]) ?? details.WallType;
        details.ParkingType = ReadString(node["parkingType"]) ?? details.ParkingType;
        details.GarageType = ReadString(node["garageType"]) ?? details.GarageType;
        details.BasementSqFt = ReadInt(node["basementSqFt"]) ?? details.BasementSqFt;
        details.Fireplaces = ReadInt(node["fireplaces"]) ?? details.Fireplaces;
        details.LocationAccuracy = ReadString(node["locationAccuracy"]) ?? details.LocationAccuracy;

        var features = ReadStringArray(node["features"]);
        if (features.Count > 0)
        {
            details.Features = features;
        }
    }

    private static void ApplyUtilities(PropertyInfoViewModel info, JsonNode node)
    {
        info.UtilityProviders ??= new UtilityProvidersInfo();
        info.UtilityProviders.Electric = ReadProvider(node["electric"]) ?? info.UtilityProviders.Electric;
        info.UtilityProviders.Water = ReadProvider(node["water"]) ?? info.UtilityProviders.Water;
        info.UtilityProviders.Gas = ReadProvider(node["gas"]) ?? info.UtilityProviders.Gas;
        info.UtilityProviders.Sewer = ReadProvider(node["sewer"]) ?? info.UtilityProviders.Sewer;

        var internet = ReadProviderList(node["internet"]);
        if (internet.Count > 0)
        {
            info.UtilityProviders.Internet = internet;
        }

        var cable = ReadProviderList(node["cableTv"] ?? node["cableTV"]);
        if (cable.Count > 0)
        {
            info.UtilityProviders.CableTV = cable;
        }
    }

    private static UtilityProvider? ReadProvider(JsonNode? node)
    {
        if (node == null) return null;
        var name = ReadString(node["name"]);
        if (string.IsNullOrWhiteSpace(name)) return null;

        return new UtilityProvider
        {
            Name = name!,
            ServiceType = ReadString(node["serviceType"]) ?? string.Empty,
            Phone = ReadString(node["phone"]),
            Website = ReadString(node["website"]),
            Coverage = ReadString(node["coverage"]),
            Notes = ReadString(node["notes"])
        };
    }

    private static List<UtilityProvider> ReadProviderList(JsonNode? node)
    {
        if (node is not JsonArray array) return new List<UtilityProvider>();

        return array
            .Select(ReadProvider)
            .Where(p => p != null)
            .Cast<UtilityProvider>()
            .ToList();
    }

    private static List<string> ReadStringArray(JsonNode? node)
    {
        if (node is not JsonArray array) return new List<string>();

        return array
            .Select(ReadString)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Cast<string>()
            .ToList();
    }

    private static string? ReadString(JsonNode? node) =>
        node?.GetValueKind() == JsonValueKind.String ? node.GetValue<string>() : node?.ToString();

    private static int? ReadInt(JsonNode? node)
    {
        if (node == null) return null;
        if (node.GetValueKind() == JsonValueKind.Number)
        {
            try { return node.GetValue<int>(); }
            catch { try { return Convert.ToInt32(node.GetValue<double>()); } catch { } }
        }

        if (node.GetValueKind() == JsonValueKind.String
            && int.TryParse(node.GetValue<string>(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
        {
            return i;
        }

        return null;
    }

    private static decimal? ReadDecimal(JsonNode? node)
    {
        if (node == null) return null;
        if (node.GetValueKind() == JsonValueKind.Number)
        {
            try { return node.GetValue<decimal>(); }
            catch { try { return Convert.ToDecimal(node.GetValue<double>()); } catch { } }
        }

        if (node.GetValueKind() == JsonValueKind.String
            && decimal.TryParse(node.GetValue<string>(), NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
        {
            return d;
        }

        return null;
    }

    private static DateTime? ReadDate(JsonNode? node)
    {
        var value = ReadString(node);
        if (string.IsNullOrWhiteSpace(value)) return null;

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt
            : null;
    }
}
