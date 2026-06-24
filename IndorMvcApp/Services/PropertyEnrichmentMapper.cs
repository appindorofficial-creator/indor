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
                try
                {
                    ApplyDetailsFromLooseObject(info.PropertyDetails, section);
                }
                catch (ArgumentException ex) when (ex.ParamName == "propertyName")
                {
                    // Some AI payloads repeat keys like "fields" inside a section.
                }
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
        ReconcileListingSourcePriority(info.PropertyDetails, root);

        var yearBuilt = SelectBestYearBuilt(root);
        if (yearBuilt is > 0)
        {
            info.PropertyDetails!.YearBuilt = yearBuilt;
        }

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

        try
        {
            foreach (var kv in obj)
            {
                ApplyFieldByKey(details, kv.Key, kv.Value);
            }
        }
        catch (ArgumentException ex) when (ex.ParamName == "propertyName")
        {
            ApplyDetailsFromDuplicateKeyObject(details, obj);
        }

        TryApplyNestedFieldArrays(details, node);
    }

    private static void TryApplyNestedFieldArrays(PropertyDetailsInfo details, JsonNode node)
    {
        try
        {
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
        catch (ArgumentException ex) when (ex.ParamName == "propertyName")
        {
            // AI occasionally emits duplicate keys inside section payloads.
        }
    }

    private static void ApplyDetailsFromDuplicateKeyObject(PropertyDetailsInfo details, JsonObject obj)
    {
        foreach (var key in KnownDetailPropertyKeys)
        {
            try
            {
                var value = obj[key];
                if (value != null)
                {
                    ApplyFieldByKey(details, key, value);
                }
            }
            catch (ArgumentException)
            {
                // JsonObject with duplicate keys can throw even on indexer access.
            }
        }
    }

    private static readonly string[] KnownDetailPropertyKeys =
    [
        "yearBuilt", "YearBuilt", "livingArea", "LivingArea", "bedrooms", "Bedrooms",
        "bathrooms", "Bathrooms", "lotSize", "LotSize", "estimatedValue", "EstimatedValue",
        "propertyType", "PropertyType", "stories", "Stories", "garageSpaces", "GarageSpaces",
        "heating", "Heating", "cooling", "Cooling", "roofType", "RoofType", "foundation", "Foundation"
    ];

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
            case "livingarea" or "squarefootage" or "sqft" or "livingsqft" or "livingsize" or "universalsize" or "grosssize" or "bldgsize" or "interiorlivingarea":
                details.LivingArea ??= ConfirmedInt(value, text, ParseSqFtFromText(text));
                break;
            case "heatedsquarefootage" or "heatedsqft":
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

        if (IsAssessorPartialLivingAreaLabel(label))
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
        var fromNode = ReadInt(value);
        if (fromNode is > 0)
        {
            return fromNode;
        }

        if (LooksEstimated(text)) return null;
        var result = parsedOverride ?? ParseIntFromText(text);
        return result is > 0 ? result : null;
    }

    // Returns a confirmed positive decimal, or null when missing,
    // zero/negative, or flagged as an estimate/placeholder.
    private static decimal? ConfirmedDecimal(JsonNode? value, string? text)
    {
        var fromNode = ReadDecimal(value);
        if (fromNode is > 0)
        {
            return fromNode;
        }

        if (LooksEstimated(text)) return null;
        var result = ParseDecimalFromText(text);
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

    internal static bool HasMeaningfulDetails(PropertyDetailsInfo details) =>
        details.YearBuilt is > 0
        || details.LivingArea is > 0
        || details.Bedrooms is > 0
        || details.Bathrooms is > 0
        || details.EstimatedValue is > 0
        || details.LotSize is > 0
        || details.LotSizeSqFt is > 0;

    /// <summary>
    /// True when JSON contains multiple sq ft values that likely refer to the same property (partial vs total).
    /// </summary>
    internal static bool HasLivingAreaSqFtConflict(string rawJson)
    {
        try
        {
            var root = JsonNode.Parse(rawJson);
            if (root == null)
            {
                return false;
            }

            var candidates = new List<(int Value, int Score)>();
            CollectLivingAreaCandidates(root, candidates, sectionHint: null, depth: 0);
            CollectLivingAreaFromSources(root, candidates);

            var values = candidates.Select(c => c.Value).Distinct().OrderByDescending(v => v).ToList();
            return values.Count >= 2 && values[0] > values[1] * 1.12;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// True when livingArea appears sourced from assessor/heated partials rather than a listing header.
    /// </summary>
    internal static bool LooksAssessorPartialLivingArea(string rawJson)
    {
        try
        {
            var root = JsonNode.Parse(rawJson);
            return root != null && LooksAssessorPartialLivingArea(root);
        }
        catch
        {
            return false;
        }
    }

    private static bool LooksAssessorPartialLivingArea(JsonNode root)
    {
        if (root["propertyDetails"] is JsonObject details)
        {
            var source = ReadString(details["livingAreaSource"]) ?? ReadString(details["livingAreaSourceName"]);
            if (HasAssessorSourceMarker(source))
            {
                return true;
            }

            var livingArea = ReadInt(details["livingArea"]);
            if (livingArea is > 0 && MatchesAssessorPartialField(root, livingArea.Value))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// True when propertyDetails.livingArea is materially smaller than another sq ft mention in the same payload.
    /// </summary>
    internal static bool HasUnderstatedLivingAreaVsPayloadMentions(string rawJson)
    {
        try
        {
            var root = JsonNode.Parse(rawJson);
            if (root == null)
            {
                return false;
            }

            var declared = ReadInt(root["propertyDetails"]?["livingArea"]);
            if (declared is not > 0)
            {
                return false;
            }

            var candidates = new List<(int Value, int Score)>();
            CollectLivingAreaCandidates(root, candidates, sectionHint: null, depth: 0);
            CollectLivingAreaFromSources(root, candidates);

            var maxMention = candidates.Select(c => c.Value).DefaultIfEmpty(0).Max();
            return maxMention > declared * 1.12;
        }
        catch
        {
            return false;
        }
    }

    private static readonly string[] ListingSourceMarkers =
    [
        "zillow", "redfin", "realtor.com", "realtor", "homes.com", "mls", "canopy", "attom"
    ];

    private static readonly string[] LivingAreaLabelHints =
    [
        "living area", "living sq", "sq ft", "sqft", "square footage", "interior", "living space", "zillow header", "header"
    ];

    private static readonly string[] HeatedOnlyLabelHints =
    [
        "heated", "heat area", "assessor heated"
    ];

    private static readonly string[] AssessorOnlyLabelHints =
    [
        "above grade", "above-grade", "assessor", "tax record", "public record", "heated only",
        "first floor", "1st floor", "main level", "main/1st", "finished area"
    ];

    private static readonly string[] TotalBuiltLabelHints =
    [
        "total built", "gross", "building area", "bldg area", "construida total", "universal size",
        "gross size", "total area", "structure size"
    ];

    private static readonly string[] AssessorSourceMarkers =
    [
        "assessor", "tax", "county", "public record", "mecknc", "cad", "appraisal district"
    ];

    /// <summary>
    /// Prefer Zillow/Redfin listing header sq ft over county assessor partials when sources disagree.
    /// </summary>
    private static void ReconcileListingSourcePriority(PropertyDetailsInfo details, JsonNode root)
    {
        var livingArea = SelectBestLivingArea(root);
        if (livingArea is > 0)
        {
            details.LivingArea = livingArea;
        }

        var zestimate = SelectBestEstimatedValue(root);
        if (zestimate is > 0)
        {
            details.EstimatedValue = zestimate;
        }

        var bedrooms = SelectBestIntMetric(root, BedroomKeys(), minValue: 1, maxValue: 20);
        if (bedrooms is > 0)
        {
            details.Bedrooms = bedrooms;
        }

        var bathrooms = SelectBestDecimalMetric(root, BathroomKeys(), minValue: 1, maxValue: 20);
        if (bathrooms is > 0)
        {
            details.Bathrooms = bathrooms;
        }

        if (IsPlaceholderLotAcres(details.LotSize))
        {
            details.LotSize = null;
        }
    }

    private static int? SelectBestLivingArea(JsonNode root)
    {
        var candidates = new List<(int Value, int Score)>();
        AddPropertyDetailsLivingAreaCandidate(root, candidates);
        CollectLivingAreaCandidates(root, candidates, sectionHint: null, depth: 0);
        CollectLivingAreaFromSources(root, candidates);

        if (candidates.Count == 0)
        {
            return null;
        }

        candidates = FilterYearBuiltCollisions(root, candidates);

        var listingCandidates = candidates.Where(c => c.Score >= 60).ToList();
        if (listingCandidates.Count > 0)
        {
            // Prefer highest-scored source (living/habitable/header), not blindly the largest number (total built > living).
            return listingCandidates
                .OrderByDescending(c => c.Score)
                .ThenByDescending(c => c.Value)
                .First()
                .Value;
        }

        var distinctValues = candidates.Select(c => c.Value).Distinct().OrderByDescending(v => v).ToList();
        if (distinctValues.Count >= 2 && distinctValues[0] > distinctValues[1] * 1.12)
        {
            return distinctValues[0];
        }

        return candidates
            .OrderByDescending(c => c.Score)
            .ThenByDescending(c => c.Value)
            .First()
            .Value;
    }

    private static void AddPropertyDetailsLivingAreaCandidate(JsonNode root, List<(int Value, int Score)> candidates)
    {
        if (root["propertyDetails"] is not JsonObject details)
        {
            return;
        }

        var area = ReadInt(details["livingArea"]);
        if (area is not > 0)
        {
            return;
        }

        var source = ReadString(details["livingAreaSource"]) ?? ReadString(details["livingAreaSourceName"]);
        var context = $"propertyDetails livingArea {source} {area}";
        var score = ScoreLivingArea(context, "propertyDetails", "livingArea", source);
        if (HasListingSourceMarker(source))
        {
            score += 30;
        }

        if (HasAssessorSourceMarker(source) || MatchesAssessorPartialField(root, area.Value))
        {
            score -= 80;
        }

        if (CollidesWithYearBuilt(root, area.Value))
        {
            score -= 200;
        }

        candidates.Add((area.Value, score));
    }

    private static bool CollidesWithYearBuilt(JsonNode root, int livingArea) =>
        ReadInt(root["propertyDetails"]?["yearBuilt"]) is int yearBuilt
        && yearBuilt is >= 1800 and <= 2100
        && livingArea == yearBuilt;

    private static List<(int Value, int Score)> FilterYearBuiltCollisions(
        JsonNode root,
        List<(int Value, int Score)> candidates)
    {
        var yearBuilt = ReadInt(root["propertyDetails"]?["yearBuilt"]);
        if (yearBuilt is not (>= 1800 and <= 2100))
        {
            return candidates;
        }

        var filtered = candidates.Where(c => c.Value != yearBuilt).ToList();
        return filtered.Count > 0 ? filtered : candidates;
    }

    /// <summary>
    /// True when livingArea likely came from assessor or was confused with yearBuilt.
    /// </summary>
    internal static bool NeedsLivingAreaCorrection(string rawJson)
    {
        try
        {
            var root = JsonNode.Parse(rawJson);
            if (root?["propertyDetails"] is not JsonObject details)
            {
                return false;
            }

            var livingArea = ReadInt(details["livingArea"]);
            if (livingArea is not > 0)
            {
                return false;
            }

            var yearBuilt = ReadInt(details["yearBuilt"]);
            if (yearBuilt is >= 1800 and <= 2100 && livingArea == yearBuilt)
            {
                return true;
            }

            if (HasUnderstatedLivingAreaVsPayloadMentions(rawJson))
            {
                return true;
            }

            if (LooksAssessorPartialLivingArea(rawJson))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    internal static string MergeLivingAreaCorrection(string payload, string correctionJson)
    {
        try
        {
            var root = JsonNode.Parse(payload)?.AsObject();
            var fix = JsonNode.Parse(correctionJson);
            if (root == null || fix == null)
            {
                return payload;
            }

            var area = ReadInt(fix["livingArea"] ?? fix["propertyDetails"]?["livingArea"]);
            if (area is not (> 300 and < 50000))
            {
                return payload;
            }

            var yearBuilt = ReadInt(root["propertyDetails"]?["yearBuilt"]);
            if (yearBuilt is >= 1800 and <= 2100 && area == yearBuilt)
            {
                return payload;
            }

            var details = root["propertyDetails"] as JsonObject ?? new JsonObject();
            var existing = ReadInt(details["livingArea"]);
            if (existing is > 0 && area <= existing && !CollidesWithYearBuilt(root, existing.Value))
            {
                return payload;
            }

            details["livingArea"] = area;
            var source = ReadString(fix["livingAreaSource"] ?? fix["propertyDetails"]?["livingAreaSource"]);
            if (!string.IsNullOrWhiteSpace(source))
            {
                details["livingAreaSource"] = source;
            }

            root["propertyDetails"] = details;
            return root.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
        catch
        {
            return payload;
        }
    }

    private static bool MatchesAssessorPartialField(JsonNode root, int livingArea)
    {
        foreach (var sectionKey in ResearchSectionKeys)
        {
            if (root[sectionKey]?["fields"] is not JsonArray fields)
            {
                continue;
            }

            foreach (var field in fields)
            {
                if (field is not JsonObject fieldObj)
                {
                    continue;
                }

                var label = ReadString(fieldObj["label"] ?? fieldObj["name"] ?? fieldObj["key"]);
                if (!IsAssessorPartialLivingAreaLabel(label))
                {
                    continue;
                }

                var sqft = ParseSqFtFromText(ReadString(fieldObj["value"] ?? fieldObj["text"])) ?? ReadInt(fieldObj["value"]);
                if (sqft == livingArea)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsAssessorPartialLivingAreaLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        var lower = label.ToLowerInvariant();
        if (HeatedOnlyLabelHints.Any(h => lower.Contains(h, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return AssessorOnlyLabelHints.Any(h => lower.Contains(h, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasAssessorSourceMarker(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return AssessorSourceMarkers.Any(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static void CollectLivingAreaFromSources(JsonNode root, List<(int Value, int Score)> candidates)
    {
        if (root["sources"] is not JsonArray sources)
        {
            return;
        }

        foreach (var sourceNode in sources)
        {
            if (sourceNode is not JsonObject sourceObj)
            {
                continue;
            }

            var sourceName = ReadString(sourceObj["sourceName"] ?? sourceObj["name"]);
            var information = ReadString(sourceObj["informationFound"] ?? sourceObj["information"] ?? sourceObj["details"]);
            var context = $"{sourceName} {information}";
            foreach (var sqft in ExtractAllSqFtValues(information))
            {
                candidates.Add((sqft, ScoreLivingArea(context, "sources", sourceName, information) + 40));
            }
        }
    }

    private static IEnumerable<int> ExtractAllSqFtValues(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        foreach (Match match in SqFtRegex().Matches(text))
        {
            if (int.TryParse(match.Groups[1].Value.Replace(",", string.Empty), out var sqft) && sqft is > 300 and < 50000)
            {
                yield return sqft;
            }
        }
    }

    private static bool HasListingSourceMarker(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return ListingSourceMarkers.Any(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static decimal? SelectBestEstimatedValue(JsonNode root)
    {
        var candidates = new List<(decimal Value, int Score)>();
        CollectMoneyCandidates(root, candidates, ValueKeys(), sectionHint: null, depth: 0);

        return candidates
            .Where(c => c.Score >= 60)
            .OrderByDescending(c => c.Score)
            .ThenByDescending(c => c.Value)
            .Select(c => (decimal?)c.Value)
            .FirstOrDefault();
    }

    private static int? SelectBestYearBuilt(JsonNode root)
    {
        var preferred = SelectBestIntMetric(root, YearBuiltKeys(), minValue: 1800, maxValue: 2100);
        if (preferred is > 0)
        {
            return preferred;
        }

        var candidates = new List<(int Value, int Score)>();
        CollectIntCandidates(root, candidates, YearBuiltKeys(), sectionHint: null, depth: 0);
        return candidates
            .Where(c => c.Value is >= 1800 and <= 2100)
            .OrderByDescending(c => c.Score)
            .Select(c => (int?)c.Value)
            .FirstOrDefault();
    }

    private static int? SelectBestIntMetric(JsonNode root, string[] keys, int minValue, int maxValue)
    {
        var candidates = new List<(int Value, int Score)>();
        CollectIntCandidates(root, candidates, keys, sectionHint: null, depth: 0);

        var valid = candidates
            .Where(c => c.Value >= minValue && c.Value <= maxValue && c.Score >= 60)
            .ToList();
        return valid.Count == 0
            ? null
            : valid.OrderByDescending(c => c.Score).First().Value;
    }

    private static decimal? SelectBestDecimalMetric(JsonNode root, string[] keys, decimal minValue, decimal maxValue)
    {
        var candidates = new List<(decimal Value, int Score)>();
        CollectDecimalCandidates(root, candidates, keys, sectionHint: null, depth: 0);

        var valid = candidates
            .Where(c => c.Value >= minValue && c.Value <= maxValue && c.Score >= 60)
            .ToList();
        return valid.Count == 0
            ? null
            : valid.OrderByDescending(c => c.Score).First().Value;
    }

    private static void CollectLivingAreaCandidates(JsonNode? node, List<(int Value, int Score)> candidates, string? sectionHint, int depth)
    {
        if (node == null || depth > 14) return;

        switch (node)
        {
            case JsonObject obj:
                var section = sectionHint ?? InferSectionHint(obj);
                foreach (var kv in obj)
                {
                    var keyText = kv.Key;
                    var valueText = ReadString(kv.Value);
                    var context = $"{section} {keyText} {valueText}";
                    var normalizedKey = NormalizeToken(keyText);

                    if (IsHeatedOrPartialLivingAreaKey(normalizedKey))
                    {
                        continue;
                    }

                    if (IsLivingAreaKey(normalizedKey) || IsLivingAreaLabel(keyText) || IsLivingAreaLabel(valueText))
                    {
                        var sqft = ReadInt(kv.Value) ?? ParseSqFtFromText(valueText);
                        if (sqft is > 300 and < 50000)
                        {
                            candidates.Add((sqft.Value, ScoreLivingArea(context, section, keyText, valueText)));
                        }
                    }

                    if (keyText.Equals("fields", StringComparison.OrdinalIgnoreCase) && kv.Value is JsonArray fields)
                    {
                        foreach (var field in fields)
                        {
                            if (field is not JsonObject fieldObj) continue;
                            var label = ReadString(fieldObj["label"] ?? fieldObj["name"] ?? fieldObj["key"]);
                            var fieldValue = ReadString(fieldObj["value"] ?? fieldObj["text"]);
                            if (!IsLivingAreaLabel(label) && ParseSqFtFromText(fieldValue) == null) continue;

                            var sqft = ParseSqFtFromText(fieldValue) ?? ReadInt(fieldObj["value"]);
                            if (sqft is > 300 and < 50000)
                            {
                                var fieldContext = $"{section} {label} {fieldValue}";
                                candidates.Add((sqft.Value, ScoreLivingArea(fieldContext, section, label, fieldValue)));
                            }
                        }
                    }

                    var nextSection = ResearchSectionKeys.Contains(keyText, StringComparer.OrdinalIgnoreCase) ? keyText : section;
                    CollectLivingAreaCandidates(kv.Value, candidates, nextSection, depth + 1);
                }
                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    CollectLivingAreaCandidates(item, candidates, sectionHint, depth + 1);
                }
                break;
        }
    }

    private static void CollectIntCandidates(JsonNode? node, List<(int Value, int Score)> candidates, string[] keys, string? sectionHint, int depth)
    {
        if (node == null || depth > 14) return;

        if (node is JsonObject obj)
        {
            var section = sectionHint ?? InferSectionHint(obj);
            foreach (var kv in obj)
            {
                var normalizedKey = NormalizeToken(kv.Key);
                if (keys.Any(k => normalizedKey.Contains(NormalizeToken(k), StringComparison.Ordinal)))
                {
                    var value = ReadInt(kv.Value) ?? ParseIntFromText(ReadString(kv.Value));
                    if (value is > 0)
                    {
                        var context = $"{section} {kv.Key} {ReadString(kv.Value)}";
                        candidates.Add((value.Value, ScoreListingContext(context, section, preferListingSection: section is "listingMarketData" or "basicPropertyFacts")));
                    }
                }

                var nextSection = ResearchSectionKeys.Contains(kv.Key, StringComparer.OrdinalIgnoreCase) ? kv.Key : section;
                CollectIntCandidates(kv.Value, candidates, keys, nextSection, depth + 1);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                CollectIntCandidates(item, candidates, keys, sectionHint, depth + 1);
            }
        }
    }

    private static void CollectDecimalCandidates(JsonNode? node, List<(decimal Value, int Score)> candidates, string[] keys, string? sectionHint, int depth)
    {
        if (node == null || depth > 14) return;

        if (node is JsonObject obj)
        {
            var section = sectionHint ?? InferSectionHint(obj);
            foreach (var kv in obj)
            {
                var normalizedKey = NormalizeToken(kv.Key);
                if (keys.Any(k => normalizedKey.Contains(NormalizeToken(k), StringComparison.Ordinal)))
                {
                    var value = ReadDecimal(kv.Value) ?? ParseDecimalFromText(ReadString(kv.Value));
                    if (value is > 0)
                    {
                        var context = $"{section} {kv.Key} {ReadString(kv.Value)}";
                        candidates.Add((value.Value, ScoreListingContext(context, section, preferListingSection: section is "listingMarketData" or "basicPropertyFacts")));
                    }
                }

                var nextSection = ResearchSectionKeys.Contains(kv.Key, StringComparer.OrdinalIgnoreCase) ? kv.Key : section;
                CollectDecimalCandidates(kv.Value, candidates, keys, nextSection, depth + 1);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                CollectDecimalCandidates(item, candidates, keys, sectionHint, depth + 1);
            }
        }
    }

    private static void CollectMoneyCandidates(JsonNode? node, List<(decimal Value, int Score)> candidates, string[] keys, string? sectionHint, int depth)
    {
        if (node == null || depth > 14) return;

        if (node is JsonObject obj)
        {
            var section = sectionHint ?? InferSectionHint(obj);
            foreach (var kv in obj)
            {
                var normalizedKey = NormalizeToken(kv.Key);
                if (keys.Any(k => normalizedKey.Contains(NormalizeToken(k), StringComparison.Ordinal)))
                {
                    var value = ReadDecimal(kv.Value) ?? ParseMoneyFromText(ReadString(kv.Value));
                    if (value is > 10000)
                    {
                        var context = $"{section} {kv.Key} {ReadString(kv.Value)}";
                        candidates.Add((value.Value, ScoreListingContext(context, section, preferListingSection: section is "listingMarketData")));
                    }
                }

                var nextSection = ResearchSectionKeys.Contains(kv.Key, StringComparer.OrdinalIgnoreCase) ? kv.Key : section;
                CollectMoneyCandidates(kv.Value, candidates, keys, nextSection, depth + 1);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                CollectMoneyCandidates(item, candidates, keys, sectionHint, depth + 1);
            }
        }
    }

    private static int ScoreLivingArea(string context, string? section, string? label, string? value)
    {
        var score = ScoreListingContext(context, section, preferListingSection: section is "listingMarketData");

        var labelText = $"{label} {value}".ToLowerInvariant();
        if (LivingAreaLabelHints.Any(h => labelText.Contains(h, StringComparison.OrdinalIgnoreCase)))
        {
            score += 25;
        }

        if (HeatedOnlyLabelHints.Any(h => labelText.Contains(h, StringComparison.OrdinalIgnoreCase)))
        {
            score -= 40;
        }

        if (AssessorOnlyLabelHints.Any(h => labelText.Contains(h, StringComparison.OrdinalIgnoreCase)))
        {
            score -= 50;
        }

        if (TotalBuiltLabelHints.Any(h => labelText.Contains(h, StringComparison.OrdinalIgnoreCase)))
        {
            score -= 60;
        }

        if (labelText.Contains("habitable", StringComparison.OrdinalIgnoreCase))
        {
            score += 30;
        }

        if (labelText.Contains("header", StringComparison.OrdinalIgnoreCase)
            || (labelText.Contains("redfin", StringComparison.OrdinalIgnoreCase) && labelText.Contains("sq", StringComparison.OrdinalIgnoreCase))
            || (labelText.Contains("zillow", StringComparison.OrdinalIgnoreCase) && labelText.Contains("sq", StringComparison.OrdinalIgnoreCase)))
        {
            score += 35;
        }

        if (section?.Equals("publicRecordsTaxes", StringComparison.OrdinalIgnoreCase) == true)
        {
            score -= 20;
        }

        return score;
    }

    private static int ScoreListingContext(string context, string? section, bool preferListingSection)
    {
        var score = 0;
        var lower = context.ToLowerInvariant();

        if (lower.Contains("zillow", StringComparison.Ordinal)) score += 120;
        else if (lower.Contains("redfin", StringComparison.Ordinal)) score += 100;
        else if (lower.Contains("realtor", StringComparison.Ordinal)) score += 90;
        else if (ListingSourceMarkers.Any(m => lower.Contains(m, StringComparison.Ordinal))) score += 70;

        if (preferListingSection) score += 30;
        if (section?.Equals("listingMarketData", StringComparison.OrdinalIgnoreCase) == true) score += 40;
        if (section?.Equals("basicPropertyFacts", StringComparison.OrdinalIgnoreCase) == true) score += 20;
        if (section?.Equals("publicRecordsTaxes", StringComparison.OrdinalIgnoreCase) == true) score -= 15;

        if (lower.Contains("assessor", StringComparison.Ordinal) || lower.Contains("tax record", StringComparison.Ordinal))
        {
            score -= 25;
        }

        return score;
    }

    private static string? InferSectionHint(JsonObject obj)
    {
        foreach (var key in ResearchSectionKeys)
        {
            if (obj.ContainsKey(key))
            {
                return key;
            }
        }

        return null;
    }

    private static bool IsLivingAreaKey(string normalizedKey) =>
        normalizedKey is "livingarea" or "squarefootage"
            or "sqft" or "livingsqft" or "livingsize" or "interiorlivingarea";

    private static bool IsHeatedOrPartialLivingAreaKey(string normalizedKey) =>
        normalizedKey is "heatedsquarefootage" or "heatedsqft"
            or "universalsize" or "grosssize" or "bldgsize";

    private static bool IsLivingAreaLabel(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        return LivingAreaLabelHints.Any(h => text.Contains(h, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPlaceholderLotAcres(decimal? acres) =>
        acres is 0.25m or 0.5m or 0.33m or 0.34m;

    private static string[] BedroomKeys() => ["bedroom", "bedrooms", "beds", "bed"];
    private static string[] BathroomKeys() => ["bathroom", "bathrooms", "baths", "bath"];
    private static string[] YearBuiltKeys() => ["yearbuilt", "year built", "yearbuiltdate", "year built date"];
    private static string[] ValueKeys() => ["zestimate", "estimatedvalue", "marketvalue", "listprice", "redfinestimate"];

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
