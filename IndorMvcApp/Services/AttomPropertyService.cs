using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public class AttomPropertyService : IAttomPropertyService
{
    private readonly HttpClient _httpClient;
    private readonly AttomOptions _options;
    private readonly ILogger<AttomPropertyService> _logger;

    public AttomPropertyService(
        HttpClient httpClient,
        IOptions<AttomOptions> options,
        ILogger<AttomPropertyService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AttomEnrichmentResult> EnrichPropertyAsync(PropertyInfoViewModel propertyInfo)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new AttomEnrichmentResult
            {
                Success = false,
                ErrorMessage = "ATTOM API key is not configured."
            };
        }

        var (address1, address2) = BuildAttomAddressLines(propertyInfo);
        if (string.IsNullOrWhiteSpace(address1))
        {
            return new AttomEnrichmentResult
            {
                Success = false,
                ErrorMessage = "Insufficient address data for ATTOM lookup."
            };
        }

        try
        {
            var query = $"propertyapi/v1.0.0/property/detail?address1={Uri.EscapeDataString(address1)}";
            if (!string.IsNullOrWhiteSpace(address2))
            {
                query += $"&address2={Uri.EscapeDataString(address2)}";
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, query);
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("apikey", _options.ApiKey);

            using var response = await _httpClient.SendAsync(request);
            var rawJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "ATTOM lookup failed ({StatusCode}) for {Address1}, {Address2}",
                    (int)response.StatusCode,
                    address1,
                    address2);

                return new AttomEnrichmentResult
                {
                    Success = false,
                    ErrorMessage = $"ATTOM returned {(int)response.StatusCode}.",
                    RawJson = rawJson
                };
            }

            var attomId = ApplyAttomPayload(propertyInfo, rawJson);
            propertyInfo.DataSource = "ATTOM";
            propertyInfo.AttomPropertyId = attomId;

            return new AttomEnrichmentResult
            {
                Success = attomId.HasValue,
                AttomPropertyId = attomId,
                RawJson = rawJson,
                ErrorMessage = attomId.HasValue ? null : "ATTOM response did not include a property match."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ATTOM enrichment failed for {Address}", propertyInfo.FormattedAddress);
            return new AttomEnrichmentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    internal static (string Address1, string? Address2) BuildAttomAddressLines(PropertyInfoViewModel info)
    {
        var address1 = string.Join(" ",
                new[] { info.HouseNumber, info.Street }.Where(x => !string.IsNullOrWhiteSpace(x)))
            .Trim();

        if (string.IsNullOrWhiteSpace(address1) && !string.IsNullOrWhiteSpace(info.FormattedAddress))
        {
            var parts = info.FormattedAddress.Split(',', StringSplitOptions.TrimEntries);
            address1 = parts.Length > 0 ? parts[0] : info.FormattedAddress;
        }

        string? address2 = null;
        if (!string.IsNullOrWhiteSpace(info.City) || !string.IsNullOrWhiteSpace(info.State))
        {
            var cityState = string.Join(", ",
                new[] { info.City, info.State }.Where(x => !string.IsNullOrWhiteSpace(x)));
            address2 = string.IsNullOrWhiteSpace(info.PostalCode)
                ? cityState
                : $"{cityState} {info.PostalCode}".Trim();
        }
        else if (!string.IsNullOrWhiteSpace(info.FormattedAddress))
        {
            var parts = info.FormattedAddress.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length > 1)
            {
                address2 = string.Join(", ", parts.Skip(1));
            }
        }

        return (address1, address2);
    }

    internal static long? ApplyAttomPayload(PropertyInfoViewModel info, string rawJson)
    {
        var root = JsonNode.Parse(rawJson);
        var propertyNode = root?["property"]?.AsArray()?.FirstOrDefault()
                           ?? root?["Property"]?.AsArray()?.FirstOrDefault();

        if (propertyNode == null)
        {
            return null;
        }

        info.PropertyDetails ??= new PropertyDetailsInfo();

        var attomId = ReadLong(propertyNode["identifier"]?["attomId"])
                      ?? ReadLong(propertyNode["identifier"]?["Id"]);

        info.PropertyDetails.ParcelNumber = ReadString(propertyNode["identifier"]?["apn"])
                                            ?? info.PropertyDetails.ParcelNumber;
        info.PropertyDetails.LegalDescription = ReadString(propertyNode["identifier"]?["legalDesc"])
                                                ?? info.PropertyDetails.LegalDescription;

        var summary = propertyNode["summary"];
        info.PropertyDetails.PropertyType = ReadString(summary?["propclass"])
                                            ?? ReadString(summary?["proptype"])
                                            ?? info.PropertyDetails.PropertyType;
        info.PropertyDetails.ArchitecturalStyle = ReadString(summary?["archStyle"])
                                                    ?? info.PropertyDetails.ArchitecturalStyle;

        var building = propertyNode["building"];
        info.PropertyDetails.YearBuilt = ReadInt(building?["construction"]?["yearbuilt"])
                                         ?? info.PropertyDetails.YearBuilt;
        info.PropertyDetails.YearRenovated = ReadInt(building?["construction"]?["yearrenovated"])
                                             ?? info.PropertyDetails.YearRenovated;
        info.PropertyDetails.LivingArea = ReadInt(building?["size"]?["livingsize"])
                                          ?? ReadInt(building?["size"]?["universalsize"])
                                          ?? info.PropertyDetails.LivingArea;
        info.PropertyDetails.Bedrooms = ReadInt(building?["rooms"]?["beds"])
                                        ?? info.PropertyDetails.Bedrooms;
        info.PropertyDetails.Bathrooms = ReadDecimal(building?["rooms"]?["bathstotal"])
                                         ?? info.PropertyDetails.Bathrooms;
        info.PropertyDetails.Floors = ReadInt(building?["summary"]?["levels"])
                                      ?? info.PropertyDetails.Floors;

        var lot = propertyNode["lot"];
        info.PropertyDetails.LotSize = ReadDecimal(lot?["lotsize1"]) ?? info.PropertyDetails.LotSize;
        info.PropertyDetails.LotSizeSqFt = ReadInt(lot?["lotsize2"]) ?? info.PropertyDetails.LotSizeSqFt;

        var area = propertyNode["area"];
        info.PropertyDetails.Zoning = ReadString(area?["zonetype"])
                                      ?? ReadString(area?["countyuse1"])
                                      ?? info.PropertyDetails.Zoning;
        info.PropertyDetails.AssignedSchool = ReadString(propertyNode["school"]?["schoolname"])
                                              ?? info.PropertyDetails.AssignedSchool;

        var assessment = propertyNode["assessment"];
        info.PropertyDetails.AnnualTaxAmount = ReadDecimal(assessment?["tax"]?["taxamt"])
                                               ?? info.PropertyDetails.AnnualTaxAmount;
        info.PropertyDetails.TaxYear = ReadInt(assessment?["tax"]?["taxyear"])
                                       ?? info.PropertyDetails.TaxYear;
        info.PropertyDetails.EstimatedValue = ReadDecimal(assessment?["market"]?["mktttlvalue"])
                                              ?? ReadDecimal(assessment?["assessed"]?["assdttlvalue"])
                                              ?? info.PropertyDetails.EstimatedValue;
        info.PropertyDetails.EstimatedValueYear = ReadInt(assessment?["tax"]?["taxyear"])
                                                    ?? info.PropertyDetails.EstimatedValueYear;

        var sale = propertyNode["sale"];
        info.PropertyDetails.LastSalePrice = ReadDecimal(sale?["amount"]?["saleamt"])
                                             ?? info.PropertyDetails.LastSalePrice;
        info.PropertyDetails.LastSaleDate = ReadAttomDate(sale?["saleTransDate"])
                                            ?? info.PropertyDetails.LastSaleDate;

        var features = new List<string>();
        AddFeature(features, ReadString(building?["construction"]?["roofcover"]));
        AddFeature(features, ReadString(building?["construction"]?["walltype"]));
        AddFeature(features, ReadString(building?["parking"]?["prkgType"]));
        AddFeature(features, ReadString(building?["interior"]?["fplctype"]));
        if (features.Count > 0)
        {
            info.PropertyDetails.Features = features;
        }

        return attomId;
    }

    private static void AddFeature(List<string> features, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && !features.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            features.Add(value);
        }
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

    private static long? ReadLong(JsonNode? node)
    {
        if (node == null) return null;
        if (node.GetValueKind() == JsonValueKind.Number)
        {
            try { return node.GetValue<long>(); }
            catch { try { return Convert.ToInt64(node.GetValue<double>()); } catch { } }
        }

        if (node.GetValueKind() == JsonValueKind.String
            && long.TryParse(node.GetValue<string>(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
        {
            return l;
        }

        return null;
    }

    private static DateTime? ReadAttomDate(JsonNode? node)
    {
        var value = ReadString(node);
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
        {
            return dt;
        }

        if (value.Length == 8
            && DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
        {
            return dt;
        }

        return null;
    }
}
