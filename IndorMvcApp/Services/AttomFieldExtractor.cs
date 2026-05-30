using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class AttomFieldExtractor
{
    private static readonly Dictionary<string, string> LabelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["attomId"] = "ATTOM ID",
        ["Id"] = "ATTOM ID",
        ["fips"] = "FIPS code",
        ["apn"] = "Parcel ID (APN)",
        ["apnOrig"] = "Original APN",
        ["legalDesc"] = "Legal description",
        ["legal1"] = "Legal description",
        ["lotsize1"] = "Lot size (acres)",
        ["lotsize2"] = "Lot size (sq ft)",
        ["lotnum"] = "Lot number",
        ["yearbuilt"] = "Year built",
        ["yearbuilteffective"] = "Effective year built",
        ["yearrenovated"] = "Year renovated",
        ["livingsize"] = "Living area (sq ft)",
        ["universalsize"] = "Universal size (sq ft)",
        ["bldgsize"] = "Building size (sq ft)",
        ["grosssize"] = "Gross size (sq ft)",
        ["beds"] = "Bedrooms",
        ["bathstotal"] = "Bathrooms",
        ["bathsfull"] = "Full bathrooms",
        ["roomsTotal"] = "Total rooms",
        ["propclass"] = "Property class",
        ["propertyType"] = "Property type",
        ["proptype"] = "Property type code",
        ["propsubtype"] = "Property subtype",
        ["propLandUse"] = "Land use",
        ["absenteeInd"] = "Occupancy",
        ["countyuse1"] = "County use",
        ["zonetype"] = "Zoning",
        ["subdname"] = "Subdivision",
        ["munname"] = "Municipality",
        ["countrysecsubd"] = "County",
        ["taxcodearea"] = "Tax code area",
        ["heatingtype"] = "Heating type",
        ["heatingfuel"] = "Heating fuel",
        ["coolingtype"] = "Cooling type",
        ["wallType"] = "Wall type",
        ["walltype"] = "Wall type",
        ["condition"] = "Building condition",
        ["quality"] = "Building quality",
        ["prkgType"] = "Parking type",
        ["garagetype"] = "Garage type",
        ["prkgSpaces"] = "Parking spaces",
        ["prkgSize"] = "Parking size (sq ft)",
        ["bsmtsize"] = "Basement size (sq ft)",
        ["fplccount"] = "Fireplaces",
        ["levels"] = "Floors / levels",
        ["matchCode"] = "Address match code",
        ["accuracy"] = "Location accuracy",
        ["latitude"] = "Latitude",
        ["longitude"] = "Longitude",
        ["oneLine"] = "Full address",
        ["line1"] = "Address line 1",
        ["line2"] = "Address line 2",
        ["locality"] = "City",
        ["postal1"] = "ZIP code",
        ["postal2"] = "ZIP+4",
        ["taxamt"] = "Annual tax amount",
        ["taxyear"] = "Tax year",
        ["mktttlvalue"] = "Market value",
        ["assdttlvalue"] = "Assessed value",
        ["saleamt"] = "Last sale price",
        ["saleTransDate"] = "Last sale date",
        ["schoolname"] = "Assigned school",
        ["lastModified"] = "Last modified",
        ["pubDate"] = "Published date",
        ["transactionID"] = "Transaction ID",
        ["responseDateTime"] = "Response date",
        ["confidence"] = "Confidence",
        ["formattedAddress"] = "Formatted address",
        ["yearBuilt"] = "Year built",
        ["yearRenovated"] = "Year renovated",
        ["livingArea"] = "Living area (sq ft)",
        ["lotSizeAcres"] = "Lot size (acres)",
        ["lotSizeSqFt"] = "Lot size (sq ft)",
        ["bedrooms"] = "Bedrooms",
        ["bathrooms"] = "Bathrooms",
        ["estimatedValue"] = "Estimated value",
        ["estimatedValueYear"] = "Estimated value year",
        ["annualTaxAmount"] = "Annual tax amount",
        ["parcelNumber"] = "Parcel number",
        ["legalDescription"] = "Legal description",
        ["assignedSchool"] = "Assigned school",
        ["countyName"] = "County",
        ["architecturalStyle"] = "Architectural style",
        ["buildingCondition"] = "Building condition",
        ["heatingType"] = "Heating type",
        ["heatingFuel"] = "Heating fuel",
        ["coolingType"] = "Cooling type",
        ["parkingType"] = "Parking type",
        ["garageType"] = "Garage type",
        ["basementSqFt"] = "Basement (sq ft)",
        ["serviceType"] = "Service type",
        ["website"] = "Website",
        ["coverage"] = "Coverage",
        ["notes"] = "Notes",
        ["name"] = "Provider name",
        ["phone"] = "Phone"
    };

    private static readonly (string Key, string Title)[] HouseFactSections =
    [
        ("propertyIdentity", "1. Property Identity"),
        ("basicPropertyFacts", "2. Basic Property Facts"),
        ("listingMarketData", "3. Listing / Market Data"),
        ("publicRecordsTaxes", "4. Public Records / Taxes"),
        ("salesHistory", "5. Sales History"),
        ("mechanicalUtilitySystems", "6. Mechanical / Utility Systems"),
        ("roofExteriorSite", "7. Roof / Exterior / Site"),
        ("foundationStructure", "8. Foundation / Structure"),
        ("permitsImprovements", "9. Permits / Improvements"),
        ("hoaCommunity", "10. HOA / Community"),
        ("schoolsLocationUtilities", "11. Schools / Location / Utilities"),
        ("itemsNeedingVerification", "12. Key Items Needing Verification"),
        ("sources", "13. Sources")
    ];

    // Legacy section list retained for ATTOM fallback paths.
    public static List<AttomFieldGroupViewModel> ExtractGroups(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return new List<AttomFieldGroupViewModel>();
        }

        try
        {
            var root = JsonNode.Parse(rawJson);
            if (root == null) return new List<AttomFieldGroupViewModel>();

            if (root["propertyIdentity"] != null)
            {
                return HouseFactDisplayService.BuildSections(root);
            }

            var groups = new List<AttomFieldGroupViewModel>();
            var propertyDetails = root["propertyDetails"] ?? root["PropertyDetails"];
            if (propertyDetails != null)
            {
                groups.Add(BuildGroup("Property details", propertyDetails));

                var utilities = root["utilityProviders"] ?? root["UtilityProviders"];
                if (utilities != null)
                {
                    groups.Add(BuildGroup("Utility providers", utilities));
                }

                var formatted = root["formattedAddress"] ?? root["FormattedAddress"];
                if (formatted != null)
                {
                    groups.Add(BuildGroup("Address", formatted));
                }

                var confidence = root["confidence"] ?? root["Confidence"];
                if (confidence != null)
                {
                    groups.Add(BuildGroup("Confidence", confidence));
                }

                return groups.Where(g => g.Fields.Count > 0).ToList();
            }

            var status = root["status"];
            if (status != null)
            {
                groups.Add(BuildGroup("API status", status));
            }

            var property = root["property"]?.AsArray()?.FirstOrDefault()
                           ?? root["Property"]?.AsArray()?.FirstOrDefault();

            if (property == null)
            {
                return groups;
            }

            foreach (var kv in property.AsObject())
            {
                if (kv.Value == null) continue;
                groups.Add(BuildGroup(HumanizeGroup(kv.Key), kv.Value));
            }

            return groups.Where(g => g.Fields.Count > 0).ToList();
        }
        catch
        {
            return new List<AttomFieldGroupViewModel>();
        }
    }

    public static string FormatPrettyJson(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return rawJson;
        }
    }

    private static AttomFieldGroupViewModel BuildGroup(string title, JsonNode node)
    {
        var fields = new List<AttomFieldItemViewModel>();
        Flatten(node, string.Empty, fields);
        return new AttomFieldGroupViewModel
        {
            Title = title,
            Fields = fields
                .GroupBy(f => f.Label, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(f => f.Label)
                .ToList()
        };
    }

    public static void FlattenFields(JsonNode? node, string path, List<AttomFieldItemViewModel> fields)
    {
        if (node == null) return;

        switch (node)
        {
            case JsonObject obj:
                foreach (var kv in obj)
                {
                    var segment = kv.Key;
                    var childPath = string.IsNullOrEmpty(path) ? segment : $"{path}.{segment}";
                    FlattenFields(kv.Value, childPath, fields);
                }
                break;

            case JsonArray arr when arr.Count == 0:
                fields.Add(CreateField(path, "—"));
                break;

            case JsonArray arr when arr.All(x => x is JsonValue or null):
                fields.Add(CreateField(path, string.Join(", ", arr.Select(FormatScalar))));
                break;

            case JsonArray arr:
                var index = 0;
                foreach (var item in arr)
                {
                    var childPath = string.IsNullOrEmpty(path) ? $"[{index}]" : $"{path}[{index}]";
                    FlattenFields(item, childPath, fields);
                    index++;
                }
                break;

            default:
                fields.Add(CreateField(path, FormatScalar(node)));
                break;
        }
    }

    private static AttomFieldItemViewModel CreateField(string path, string value)
    {
        var display = string.IsNullOrWhiteSpace(value) ? "—" : value;
        return new AttomFieldItemViewModel
        {
            Label = LabelFor(path),
            Value = display,
            NeedsVerification = display.Contains("needs verification", StringComparison.OrdinalIgnoreCase)
                || display.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static void Flatten(JsonNode? node, string path, List<AttomFieldItemViewModel> fields) =>
        FlattenFields(node, path, fields);

    private static string LabelFor(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "Value";

        var leaf = path.Split('.').Last();
        if (LabelMap.TryGetValue(leaf, out var mapped))
        {
            return mapped;
        }

        return HumanizeGroup(leaf);
    }

    private static string HumanizeGroup(string key)
    {
        if (LabelMap.TryGetValue(key, out var mapped))
        {
            return mapped;
        }

        var sb = new StringBuilder();
        for (var i = 0; i < key.Length; i++)
        {
            var c = key[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(key[i - 1]))
            {
                sb.Append(' ');
            }
            sb.Append(i == 0 ? char.ToUpper(c) : c);
        }

        return sb.ToString().Replace('_', ' ');
    }

    private static string FormatScalar(JsonNode? node)
    {
        if (node == null) return "—";

        return node.GetValueKind() switch
        {
            JsonValueKind.String => node.GetValue<string>(),
            JsonValueKind.Number => node.ToJsonString(),
            JsonValueKind.True => "Yes",
            JsonValueKind.False => "No",
            JsonValueKind.Null => "—",
            _ => node.ToJsonString()
        };
    }
}
