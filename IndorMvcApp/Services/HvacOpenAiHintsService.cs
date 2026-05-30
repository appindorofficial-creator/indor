using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class HvacOpenAiHintsService
{
    private static readonly string[] FilterSizeOptions =
    [
        "16x25x1", "20x20x1", "20x25x1", "16x20x1", "14x25x1", "12x12x1"
    ];

    public static IReadOnlyList<string> CommonFilterSizes => FilterSizeOptions;

    public static HvacOpenAiHints Extract(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var hints = new HvacOpenAiHints
        {
            DataSource = string.IsNullOrWhiteSpace(info?.DataSource)
                ? (propiedad.AttomSyncStatus ?? "OpenAI House Fact")
                : info!.DataSource
        };

        ApplyPropertyDetails(hints, info);
        ApplyWarranty(hints, info?.HomeWarranties?.HVACSystem);
        ApplyRawJson(hints, propiedad.AttomRawJson ?? info?.AttomRawJson ?? propiedad.DatosJson);

        hints.SystemType ??= InferSystemType(info, hints);
        return hints;
    }

    public static string SystemTypeLabel(string? systemType) => systemType switch
    {
        "HeatPump" => "Heat Pump",
        "MiniSplit" => "Mini Split",
        _ => "Central AC"
    };

    public static List<int> BuildInstallYearOptions()
    {
        var current = DateTime.Today.Year;
        return Enumerable.Range(1970, current - 1970 + 1).Reverse().ToList();
    }

    private static void ApplyPropertyDetails(HvacOpenAiHints hints, PropertyInfoViewModel? info)
    {
        var d = info?.PropertyDetails;
        if (d == null) return;

        hints.SystemType ??= MapSystemType($"{d.CoolingType} {d.HeatingType} {d.HeatingFuel}");
        if (d.YearBuilt.HasValue && !hints.InstallYear.HasValue)
        {
            hints.InstallYear = d.YearBuilt.Value + 5 <= DateTime.Today.Year
                ? Math.Min(d.YearBuilt.Value + 8, DateTime.Today.Year)
                : d.YearBuilt.Value;
        }
    }

    private static void ApplyWarranty(HvacOpenAiHints hints, HomeWarranty? warranty)
    {
        if (warranty == null) return;

        hints.Brand ??= ExtractBrand(warranty.SystemName, warranty.WarrantyProvider, warranty.CoverageDetails);
        hints.InstallYear ??= warranty.InstallationDate?.Year;
        hints.LastServiceDate ??= warranty.InstallationDate;
    }

    private static void ApplyRawJson(HvacOpenAiHints hints, string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return;

        try
        {
            var root = JsonNode.Parse(rawJson);
            if (root == null) return;

            WalkNode(hints, root);

            if (root["sections"] is JsonArray sections)
            {
                foreach (var sectionNode in sections)
                {
                    if (sectionNode is not JsonObject section) continue;
                    var id = ReadString(section["id"]) ?? string.Empty;
                    var title = ReadString(section["title"]) ?? string.Empty;
                    if (!IsSystemsSection(id, title)) continue;

                    WalkNode(hints, section);
                    if (section["fields"] is JsonArray fields)
                    {
                        foreach (var fieldNode in fields)
                        {
                            if (fieldNode is not JsonObject field) continue;
                            ApplyLabelValue(hints, ReadString(field["label"]), ReadString(field["value"]));
                        }
                    }

                    if (section["tableRows"] is JsonArray rows)
                    {
                        foreach (var rowNode in rows)
                        {
                            if (rowNode is not JsonObject row) continue;
                            foreach (var kv in row)
                            {
                                if (kv.Value is JsonObject columns)
                                {
                                    foreach (var col in columns)
                                    {
                                        ApplyLabelValue(hints, col.Key, ReadString(col.Value));
                                    }
                                }
                                else
                                {
                                    ApplyLabelValue(hints, kv.Key, ReadString(kv.Value));
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore malformed JSON; user can still enter manually.
        }
    }

    private static void WalkNode(HvacOpenAiHints hints, JsonNode node)
    {
        if (node is JsonObject obj)
        {
            foreach (var kv in obj)
            {
                ApplyLabelValue(hints, kv.Key, ReadString(kv.Value));
                if (kv.Value != null) WalkNode(hints, kv.Value);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item != null) WalkNode(hints, item);
            }
        }
    }

    private static void ApplyLabelValue(HvacOpenAiHints hints, string? label, string? value)
    {
        if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value)) return;
        if (IsMissingValue(value)) return;

        var key = label.ToLowerInvariant();
        var text = value.Trim();

        if (key.Contains("system") && key.Contains("type") || key.Contains("hvac") && key.Contains("type"))
        {
            hints.SystemType ??= MapSystemType(text);
            return;
        }

        if (key.Contains("brand") || key.Contains("manufacturer") || key.Contains("make"))
        {
            hints.Brand ??= text;
            return;
        }

        if (key.Contains("model"))
        {
            hints.Model ??= text;
            return;
        }

        if (key.Contains("serial"))
        {
            hints.SerialNumber ??= text;
            return;
        }

        if (key.Contains("filter"))
        {
            var size = ExtractFilterSize(text);
            if (!string.IsNullOrWhiteSpace(size))
            {
                hints.FilterSize ??= size;
            }

            return;
        }

        if (key.Contains("install") && key.Contains("year") || key.Contains("year") && key.Contains("hvac"))
        {
            hints.InstallYear ??= ParseYear(text);
            return;
        }

        if (key.Contains("last service") || key.Contains("service date") || key.Contains("serviced"))
        {
            hints.LastServiceDate ??= ParseDate(text);
            return;
        }

        if (key is "coolingtype" or "cooling" or "heatingtype" or "heating")
        {
            hints.SystemType ??= MapSystemType(text);
        }
    }

    private static bool IsSystemsSection(string id, string title)
    {
        var combined = $"{id} {title}".ToLowerInvariant();
        return combined.Contains("systems") || combined.Contains("hvac") || combined.Contains("mechanical");
    }

    private static string? InferSystemType(PropertyInfoViewModel? info, HvacOpenAiHints hints)
    {
        return MapSystemType($"{info?.PropertyDetails?.CoolingType} {info?.PropertyDetails?.HeatingType}");
    }

    private static string? MapSystemType(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var lower = text.ToLowerInvariant();
        if (lower.Contains("mini split") || lower.Contains("minisplit") || lower.Contains("ductless"))
        {
            return "MiniSplit";
        }

        if (lower.Contains("heat pump"))
        {
            return "HeatPump";
        }

        if (lower.Contains("central") || lower.Contains("forced air") || lower.Contains("cooling") || lower.Contains("hvac"))
        {
            return "CentralAC";
        }

        return null;
    }

    private static string? ExtractBrand(params string?[] parts)
    {
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part)) continue;
            var match = Regex.Match(part, @"\b(Carrier|Trane|Lennox|Rheem|Goodman|York|Bryant|American Standard|Daikin|Mitsubishi|Amana|Heil|Ruud|Payne|Coleman)\b",
                RegexOptions.IgnoreCase);
            if (match.Success) return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(match.Value.ToLowerInvariant());
        }

        return null;
    }

    private static string? ExtractFilterSize(string text)
    {
        var match = Regex.Match(text, @"\b(\d{1,2}\s?[xX×]\s?\d{1,2}\s?[xX×]\s?\d)\b");
        if (!match.Success) return null;
        return match.Value.Replace(" ", string.Empty).Replace('×', 'x').ToLowerInvariant();
    }

    private static int? ParseYear(string text)
    {
        var match = Regex.Match(text, @"\b(19|20)\d{2}\b");
        return match.Success && int.TryParse(match.Value, out var year) ? year : null;
    }

    private static DateTime? ParseDate(string text) =>
        DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt)
            ? dt.Date
            : null;

    private static bool IsMissingValue(string value)
    {
        var lower = value.Trim().ToLowerInvariant();
        return lower is "—" or "-" or "n/a" or "unknown" or "needs verification." or "needs verification"
               or "not publicly confirmed." or "not publicly confirmed";
    }

    private static string? ReadString(JsonNode? node) =>
        node?.GetValueKind() == System.Text.Json.JsonValueKind.String
            ? node.GetValue<string>()
            : node?.ToString();
}
