using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static partial class PropertyEnrichmentMapper
{
    private static readonly string[] ResearchSectionKeys =
    [
        "propertyIdentity",
        "basicPropertyFacts",
        "listingMarketData",
        "publicRecordsTaxes",
        "salesHistory",
        "mechanicalUtilitySystems",
        "roofExteriorSite",
        "foundationStructure",
        "hoaCommunity",
        "schoolsLocationUtilities"
    ];

    public static bool ApplyPayload(PropertyInfoViewModel info, string rawJson)
    {
        var root = JsonNode.Parse(rawJson);
        if (root == null)
        {
            return false;
        }

        info.PropertyDetails ??= new PropertyDetailsInfo();

        var detailsNode = root["propertyDetails"] ?? root["PropertyDetails"];
        if (detailsNode != null)
        {
            ApplyDetails(info.PropertyDetails, detailsNode);
        }

        foreach (var sectionKey in ResearchSectionKeys)
        {
            var section = root[sectionKey];
            if (section != null)
            {
                ApplyDetailsFromLooseObject(info.PropertyDetails, section);
            }
        }

        ExtractFromSectionsArray(info.PropertyDetails, root["sections"]?.AsArray());
        DeepScanForDetails(info.PropertyDetails, root);

        var utilitiesNode = root["utilityProviders"] ?? root["UtilityProviders"];
        if (utilitiesNode != null)
        {
            ApplyUtilities(info, utilitiesNode);
        }
        else
        {
            ExtractUtilitiesFromSchoolsSection(info, root["schoolsLocationUtilities"]);
        }

        var formatted = ReadString(root["formattedAddress"] ?? root["FormattedAddress"]);
        if (!string.IsNullOrWhiteSpace(formatted))
        {
            info.FormattedAddress = formatted!;
        }

        MergeCountyFromIdentity(info, root["propertyIdentity"]);

        return detailsNode != null
            || HasMeaningfulDetails(info.PropertyDetails)
            || root["sections"]?.AsArray()?.Count > 0
            || root["propertyIdentity"] != null
            || root["basicPropertyFacts"] != null;
    }

    private static void ApplyDetails(PropertyDetailsInfo details, JsonNode node)
    {
        if (node is JsonObject obj)
        {
            ApplyDetailsFromLooseObject(details, obj);
            return;
        }

        ApplyLabeledValue(details, "value", ReadString(node));
    }

    private static void ApplyDetailsFromLooseObject(PropertyDetailsInfo details, JsonNode node)
    {
        if (node is not JsonObject obj)
        {
            return;
        }

        foreach (var kv in obj)
        {
            ApplyFieldByKey(details, kv.Key, kv.Value);
        }

        if (node["fields"] is JsonArray fields)
        {
            ApplyLabeledFields(details, fields);
        }

        if (node["tableRows"] is JsonArray rows)
        {
            foreach (var row in rows)
            {
                if (row?["columns"] is JsonObject columns)
                {
                    foreach (var col in columns)
                    {
                        ApplyFieldByKey(details, col.Key, col.Value);
                    }
                }
            }
        }
    }

    private static void ExtractFromSectionsArray(PropertyDetailsInfo details, JsonArray? sections)
    {
        if (sections == null) return;

        foreach (var section in sections)
        {
            if (section is not JsonObject obj) continue;

            if (obj["fields"] is JsonArray fields)
            {
                ApplyLabeledFields(details, fields);
            }

            if (obj["tableRows"] is JsonArray rows)
            {
                foreach (var row in rows)
                {
                    if (row?["columns"] is JsonObject columns)
                    {
                        foreach (var col in columns)
                        {
                            ApplyFieldByKey(details, col.Key, col.Value);
                        }
                    }
                }
            }

            ApplyDetailsFromLooseObject(details, obj);
        }
    }

    private static void DeepScanForDetails(PropertyDetailsInfo details, JsonNode? node, int depth = 0)
    {
        if (node == null || depth > 12) return;

        switch (node)
        {
            case JsonObject obj:
                foreach (var kv in obj)
                {
                    if (IsDetailKey(kv.Key))
                    {
                        ApplyFieldByKey(details, kv.Key, kv.Value);
                    }

                    DeepScanForDetails(details, kv.Value, depth + 1);
                }
                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    DeepScanForDetails(details, item, depth + 1);
                }
                break;
        }
    }

    private static void ApplyLabeledFields(PropertyDetailsInfo details, JsonArray fields)
    {
        foreach (var field in fields)
        {
            if (field is not JsonObject obj) continue;
            var label = ReadString(obj["label"]) ?? ReadString(obj["name"]) ?? ReadString(obj["key"]);
            var value = ReadString(obj["value"]) ?? ReadString(obj["text"]);
            if (!string.IsNullOrWhiteSpace(label))
            {
                ApplyLabeledValue(details, label, value);
            }
        }
    }

    private static void ApplyFieldByKey(PropertyDetailsInfo details, string key, JsonNode? value)
    {
        if (value == null) return;

        var normalized = NormalizeToken(key);
        var text = ReadString(value);

        switch (normalized)
        {
            case "propertytype" or "landuse" or "propertytypecode":
                details.PropertyType = CoalesceString(details.PropertyType, text);
                break;
            case "yearbuilt" or "yearbuiltdate":
                details.YearBuilt ??= ConfirmedInt(value, text);
                break;
            case "yearrenovated" or "yearrenovateddate":
                details.YearRenovated ??= ConfirmedInt(value, text);
                break;
            case "yearbuilteffective" or "effectiveyearbuilt":
                details.YearBuiltEffective ??= ConfirmedInt(value, text);
                break;
            case "livingarea" or "heatedsquarefootage" or "heatedsqft" or "squarefootage" or "sqft" or "livingsqft" or "livingsize" or "universalsize" or "grosssize" or "bldgsize" or "interiorlivingarea":
                details.LivingArea ??= ConfirmedInt(value, text, ParseSqFtFromText(text));
                break;
            case "lotsizeacres" or "lotacres" or "lotsize1":
                details.LotSize ??= ConfirmedDecimal(value, text);
                break;
            case "lotsizesqft" or "lotsize2" or "lotsquarefeet":
                details.LotSizeSqFt ??= ConfirmedInt(value, text, ParseSqFtFromText(text));
                break;
            case "lotsize":
                if (!LooksEstimated(text))
                {
                    ApplyLotSizeValue(details, value, text);
                }
                break;
            case "bedrooms" or "beds" or "bed" or "bedroom":
                details.Bedrooms ??= ConfirmedInt(value, text);
                break;
            case "bathrooms" or "baths" or "bath" or "bathstotal":
                details.Bathrooms ??= ConfirmedDecimal(value, text);
                break;
            case "bathsfull" or "fullbathrooms" or "fullbaths":
                details.BathsFull ??= ConfirmedInt(value, text);
                break;
            case "floors" or "stories" or "levels" or "storycount":
                details.Floors ??= ConfirmedInt(value, text);
                break;
            case "roomstotal" or "totalrooms" or "rooms":
                details.RoomsTotal ??= ConfirmedInt(value, text);
                break;
            case "lastsaleprice" or "saleprice" or "saleamt":
                details.LastSalePrice ??= PositiveDecimal(ReadDecimal(value) ?? ParseMoneyFromText(text));
                break;
            case "lastsaledate" or "saledate" or "saletransdate":
                details.LastSaleDate ??= ReadDate(value) ?? ParseDateFromText(text);
                break;
            case "estimatedvalue" or "marketvalue" or "assessedvalue" or "zestimate" or "redfinestimate" or "listprice" or "mktttlvalue" or "assdttlvalue":
                details.EstimatedValue ??= PositiveDecimal(ReadDecimal(value) ?? ParseMoneyFromText(text));
                break;
            case "estimatedvalueyear" or "valuationyear":
                details.EstimatedValueYear ??= ConfirmedInt(value, text);
                break;
            case "annualtaxamount" or "taxamount" or "taxamt" or "annualtaxes":
                details.AnnualTaxAmount ??= PositiveDecimal(ReadDecimal(value) ?? ParseMoneyFromText(text));
                break;
            case "taxyear" or "assessmentyear":
                details.TaxYear ??= ConfirmedInt(value, text);
                break;
            case "parcelnumber" or "apn" or "parcelid" or "apnparcelid" or "apnorig":
                details.ParcelNumber = CoalesceString(details.ParcelNumber, text);
                break;
            case "legaldescription" or "legaldesc" or "legal1":
                details.LegalDescription = CoalesceString(details.LegalDescription, text);
                break;
            case "zoning" or "zonetype":
                details.Zoning = CoalesceString(details.Zoning, text);
                break;
            case "assignedschool" or "schoolname" or "schooldistrict":
                details.AssignedSchool = CoalesceString(details.AssignedSchool, text);
                break;
            case "fips" or "fipscode":
                details.Fips = CoalesceString(details.Fips, text);
                break;
            case "subdivision" or "subdivisionneighborhood" or "subdname" or "neighborhood":
                details.Subdivision = CoalesceString(details.Subdivision, text);
                break;
            case "municipality" or "munname" or "jurisdiction" or "city":
                details.Municipality = CoalesceString(details.Municipality, text);
                break;
            case "county" or "countyname" or "countrysecsubd":
                details.CountyName = CoalesceString(details.CountyName, text);
                break;
            case "occupancy" or "absenteeind":
                details.Occupancy = CoalesceString(details.Occupancy, text);
                break;
            case "heatingtype" or "hvacheating":
                details.HeatingType = CoalesceString(details.HeatingType, text);
                break;
            case "heatingfuel" or "fuelsource":
                details.HeatingFuel = CoalesceString(details.HeatingFuel, text);
                break;
            case "coolingtype" or "hvacooling":
                details.CoolingType = CoalesceString(details.CoolingType, text);
                break;
            case "buildingcondition" or "condition":
                details.BuildingCondition = CoalesceString(details.BuildingCondition, text);
                break;
            case "walltype" or "exterior" or "exteriormaterial" or "exteriorwall":
                details.WallType = CoalesceString(details.WallType, text);
                break;
            case "architecturalstyle" or "archstyle" or "style":
                details.ArchitecturalStyle = CoalesceString(details.ArchitecturalStyle, text);
                break;
            case "parkingtype" or "prkgtype" or "garageparking":
                details.ParkingType = CoalesceString(details.ParkingType, text);
                break;
            case "garagetype" or "garage":
                details.GarageType = CoalesceString(details.GarageType, text);
                break;
            case "basementsqft" or "bsmtsize" or "basementsize":
                details.BasementSqFt ??= ConfirmedInt(value, text, ParseSqFtFromText(text));
                break;
            case "fireplaces" or "fireplacecount" or "fplccount":
                details.Fireplaces ??= ConfirmedInt(value, text);
                break;
            case "locationaccuracy" or "accuracy":
                details.LocationAccuracy = CoalesceString(details.LocationAccuracy, text);
                break;
            default:
                if (normalized.Contains("feature", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(text))
                {
                    AddFeature(details, text!);
                }
                break;
        }
    }

    private static void ApplyLabeledValue(PropertyDetailsInfo details, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Contains("needs verification", StringComparison.OrdinalIgnoreCase)
            || value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var normalized = NormalizeToken(label);
        ApplyFieldByKey(details, normalized, JsonValue.Create(value));
    }

    private static readonly string[] EstimateMarkers =
    [
        "estimated", "estimate", "approx", "approximate", "typical", "typically",
        "average", "around", "about", "roughly", "circa", "~", "needs verification",
        "not publicly confirmed", "unknown", "varies", "guess"
    ];

    private static bool LooksEstimated(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        foreach (var marker in EstimateMarkers)
        {
            if (text.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // Returns a confirmed positive integer, or null when the value is missing,
    // zero/negative, or flagged as an estimate/placeholder.
    private static int? ConfirmedInt(JsonNode? value, string? text, int? parsedOverride = null)
    {
        if (LooksEstimated(text)) return null;
        var result = parsedOverride ?? ReadInt(value) ?? ParseIntFromText(text);
        return result is > 0 ? result : null;
    }

    // Returns a confirmed positive decimal, or null when missing,
    // zero/negative, or flagged as an estimate/placeholder.
    private static decimal? ConfirmedDecimal(JsonNode? value, string? text)
    {
        if (LooksEstimated(text)) return null;
        var result = ReadDecimal(value) ?? ParseDecimalFromText(text);
        return result is > 0 ? result : null;
    }

    // Drops zero/negative values (e.g. schema 0-fill) but allows estimates,
    // since market value / tax amounts are legitimately estimated.
    private static decimal? PositiveDecimal(decimal? value) => value is > 0 ? value : null;

    private static void ApplyLotSizeValue(PropertyDetailsInfo details, JsonNode value, string? text)
    {
        var raw = text ?? ReadString(value);
        if (string.IsNullOrWhiteSpace(raw)) return;

        if (raw.Contains("acre", StringComparison.OrdinalIgnoreCase))
        {
            var acres = ParseDecimalFromText(raw);
            if (acres is > 0) details.LotSize ??= acres;
            return;
        }

        var sqFt = ParseSqFtFromText(raw);
        if (sqFt is > 0)
        {
            details.LotSizeSqFt ??= sqFt;
            return;
        }

        var fallbackAcres = ReadDecimal(value) ?? ParseDecimalFromText(raw);
        if (fallbackAcres is > 0) details.LotSize ??= fallbackAcres;
    }

    private static void MergeCountyFromIdentity(PropertyInfoViewModel info, JsonNode? identity)
    {
        if (identity == null) return;
        info.County ??= info.PropertyDetails?.CountyName;
        if (string.IsNullOrWhiteSpace(info.PropertyDetails?.CountyName) && identity is JsonObject obj)
        {
            var county = ReadString(obj["county"]) ?? ReadString(obj["countyName"]);
            if (!string.IsNullOrWhiteSpace(county))
            {
                info.PropertyDetails!.CountyName = county;
                info.County ??= county;
            }
        }
    }

    private static void ExtractUtilitiesFromSchoolsSection(PropertyInfoViewModel info, JsonNode? schoolsSection)
    {
        if (schoolsSection is not JsonObject obj) return;

        info.UtilityProviders ??= new UtilityProvidersInfo();

        foreach (var kv in obj)
        {
            var key = NormalizeToken(kv.Key);
            var text = ReadString(kv.Value);
            if (string.IsNullOrWhiteSpace(text)) continue;

            if (key.Contains("electric", StringComparison.Ordinal))
            {
                info.UtilityProviders.Electric ??= new UtilityProvider { Name = text, ServiceType = "Electricity" };
            }
            else if (key.Contains("water", StringComparison.Ordinal) && !key.Contains("waterheater", StringComparison.Ordinal))
            {
                info.UtilityProviders.Water ??= new UtilityProvider { Name = text, ServiceType = "Water" };
            }
            else if (key.Contains("sewer", StringComparison.Ordinal) || key.Contains("septic", StringComparison.Ordinal))
            {
                info.UtilityProviders.Sewer ??= new UtilityProvider { Name = text, ServiceType = "Sewer" };
            }
            else if (key.Contains("gas", StringComparison.Ordinal))
            {
                info.UtilityProviders.Gas ??= new UtilityProvider { Name = text, ServiceType = "Gas" };
            }
        }
    }

    private static bool IsDetailKey(string key)
    {
        var normalized = NormalizeToken(key);
        return normalized is "bedrooms" or "beds" or "bathrooms" or "baths"
            or "livingarea" or "sqft" or "heatedsquarefootage" or "squarefootage"
            or "yearbuilt" or "lotsize" or "lotsizeacres" or "lotsizesqft"
            or "estimatedvalue" or "listprice" or "lastsaleprice" or "annualtaxamount"
            or "parcelnumber" or "apn" or "floors" or "stories" or "fireplaces"
            or "heatingtype" or "coolingtype" or "propertytype";
    }

    private static bool HasMeaningfulDetails(PropertyDetailsInfo details) =>
        details.YearBuilt is > 0
        || details.LivingArea is > 0
        || details.Bedrooms is > 0
        || details.Bathrooms is > 0
        || details.EstimatedValue is > 0
        || details.LotSize is > 0
        || details.LotSizeSqFt is > 0;

    private static void AddFeature(PropertyDetailsInfo details, string feature)
    {
        if (details.Features.Contains(feature, StringComparer.OrdinalIgnoreCase)) return;
        details.Features.Add(feature);
    }

    private static string? CoalesceString(string? current, string? incoming)
    {
        if (string.IsNullOrWhiteSpace(incoming)) return current;
        if (string.IsNullOrWhiteSpace(current)) return incoming.Trim();
        if (current.Contains(incoming, StringComparison.OrdinalIgnoreCase)) return current;
        return incoming.Trim();
    }

    private static string NormalizeToken(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : Regex.Replace(value, @"[^a-zA-Z0-9]", string.Empty).ToLowerInvariant();

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
        if (node is not JsonArray array) return [];

        return array
            .Select(ReadProvider)
            .Where(p => p != null)
            .Cast<UtilityProvider>()
            .ToList();
    }

    private static string? ReadString(JsonNode? node)
    {
        if (node == null) return null;
        return node.GetValueKind() switch
        {
            JsonValueKind.String => node.GetValue<string>(),
            JsonValueKind.Number => node.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => node.ToJsonString()
        };
    }

    private static int? ReadInt(JsonNode? node)
    {
        if (node == null) return null;
        if (node.GetValueKind() == JsonValueKind.Number)
        {
            try { return node.GetValue<int>(); }
            catch { try { return Convert.ToInt32(node.GetValue<double>()); } catch { } }
        }

        return ParseIntFromText(ReadString(node));
    }

    private static decimal? ReadDecimal(JsonNode? node)
    {
        if (node == null) return null;
        if (node.GetValueKind() == JsonValueKind.Number)
        {
            try { return node.GetValue<decimal>(); }
            catch { try { return Convert.ToDecimal(node.GetValue<double>()); } catch { } }
        }

        return ParseDecimalFromText(ReadString(node));
    }

    private static DateTime? ReadDate(JsonNode? node)
    {
        return ParseDateFromText(ReadString(node));
    }

    private static int? ParseIntFromText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var direct))
        {
            return direct;
        }

        var match = DigitsRegex().Match(text);
        return match.Success && int.TryParse(match.Value, out var parsed) ? parsed : null;
    }

    private static int? ParseSqFtFromText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var match = SqFtRegex().Match(text.Replace(",", string.Empty, StringComparison.Ordinal));
        return match.Success && int.TryParse(match.Groups[1].Value, out var sqft) ? sqft : ParseIntFromText(text);
    }

    private static decimal? ParseDecimalFromText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var cleaned = MoneyCleanupRegex().Replace(text, string.Empty).Replace(",", string.Empty).Trim();
        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static decimal? ParseMoneyFromText(string? text) => ParseDecimalFromText(text);

    private static DateTime? ParseDateFromText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt
            : null;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex DigitsRegex();

    [GeneratedRegex(@"([\d,]+)\s*(?:sq\.?\s*ft|sqft|square\s*feet)", RegexOptions.IgnoreCase)]
    private static partial Regex SqFtRegex();

    [GeneratedRegex(@"[$£€]")]
    private static partial Regex MoneyCleanupRegex();
}
