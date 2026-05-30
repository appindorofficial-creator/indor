using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class WaterHeaterOpenAiHintsService
{
    private static readonly string[] TankSizeOptions =
    [
        "30 gal", "40 gal", "50 gal", "75 gal", "80 gal"
    ];

    public static IReadOnlyList<string> CommonTankSizes => TankSizeOptions;

    public static WaterHeaterOpenAiHints Extract(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var hints = new WaterHeaterOpenAiHints
        {
            DataSource = string.IsNullOrWhiteSpace(info?.DataSource)
                ? (propiedad.AttomSyncStatus ?? "OpenAI House Fact")
                : info!.DataSource
        };

        ApplyWarranty(hints, info?.HomeWarranties?.WaterHeater);
        ApplyRawJson(hints, propiedad.AttomRawJson ?? info?.AttomRawJson ?? propiedad.DatosJson);
        hints.HeaterType ??= "Tank";
        return hints;
    }

    public static string HeaterTypeLabel(string? heaterType) =>
        string.Equals(heaterType, "Tankless", StringComparison.OrdinalIgnoreCase) ? "Tankless" : "Tank";

    public static List<int> BuildInstallYearOptions()
    {
        var current = DateTime.Today.Year;
        return Enumerable.Range(1970, current - 1970 + 1).Reverse().ToList();
    }

    private static void ApplyWarranty(WaterHeaterOpenAiHints hints, HomeWarranty? warranty)
    {
        if (warranty == null) return;

        hints.Brand ??= ExtractBrand(warranty.SystemName, warranty.WarrantyProvider, warranty.CoverageDetails);
        hints.InstallYear ??= warranty.InstallationDate?.Year;
        hints.LastServiceDate ??= warranty.InstallationDate;
        hints.HeaterType ??= MapHeaterType(warranty.SystemName);
    }

    private static void ApplyRawJson(WaterHeaterOpenAiHints hints, string? rawJson)
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
                    if (!IsWaterHeaterSection(id, title)) continue;

                    WalkNode(hints, section);
                    if (section["fields"] is JsonArray fields)
                    {
                        foreach (var fieldNode in fields)
                        {
                            if (fieldNode is not JsonObject field) continue;
                            ApplyLabelValue(hints, ReadString(field["label"]), ReadString(field["value"]));
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore malformed JSON hints.
        }
    }

    private static void WalkNode(WaterHeaterOpenAiHints hints, JsonNode node)
    {
        if (node is JsonObject obj)
        {
            foreach (var pair in obj)
            {
                if (pair.Value == null) continue;
                ApplyLabelValue(hints, pair.Key, ReadString(pair.Value));
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

    private static void ApplyLabelValue(WaterHeaterOpenAiHints hints, string? label, string? value)
    {
        if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(value)) return;
        if (IsMissingValue(value)) return;

        var key = label.ToLowerInvariant();
        var text = value.Trim();

        if (key.Contains("water") && key.Contains("heater") && key.Contains("type")
            || key.Contains("heater") && key.Contains("type"))
        {
            hints.HeaterType ??= MapHeaterType(text);
            return;
        }

        if (key.Contains("brand") || key.Contains("manufacturer") || key.Contains("make"))
        {
            if (IsUsableShortField(text)) hints.Brand ??= text;
            return;
        }

        if (key.Contains("model"))
        {
            if (IsUsableShortField(text)) hints.Model ??= text;
            return;
        }

        if (key.Contains("serial"))
        {
            if (IsUsableShortField(text)) hints.SerialNumber ??= text;
            return;
        }

        if (key.Contains("tank") && (key.Contains("size") || key.Contains("capacity"))
            || key.Contains("capacity"))
        {
            hints.TankSize ??= ExtractTankSize(text);
            return;
        }

        if (key.Contains("install") && key.Contains("year") || key.Contains("year") && key.Contains("heater"))
        {
            hints.InstallYear ??= ParseYear(text);
            return;
        }

        if (key.Contains("last service") || key.Contains("service date") || key.Contains("flushed"))
        {
            hints.LastServiceDate ??= ParseDate(text);
            return;
        }

        if (key.Contains("water heater") || key is "waterheater" or "hot water")
        {
            hints.HeaterType ??= MapHeaterType(text);
            hints.TankSize ??= ExtractTankSize(text);
        }
    }

    private static bool IsWaterHeaterSection(string id, string title)
    {
        var combined = $"{id} {title}".ToLowerInvariant();
        return combined.Contains("water heater") || combined.Contains("water-heater") || combined.Contains("hot water");
    }

    private static string? MapHeaterType(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var lower = text.ToLowerInvariant();
        return lower.Contains("tankless") || lower.Contains("on demand") || lower.Contains("on-demand")
            ? "Tankless"
            : lower.Contains("tank") || lower.Contains("storage") ? "Tank" : null;
    }

    private static string? ExtractTankSize(string text)
    {
        var match = Regex.Match(text, @"(\d{2,3})\s*(?:gal|gallon)", RegexOptions.IgnoreCase);
        return match.Success ? $"{match.Groups[1].Value} gal" : null;
    }

    private static string? ExtractBrand(params string?[] parts)
    {
        foreach (var part in parts)
        {
            if (IsUsableShortField(part ?? string.Empty) && !part!.Contains("water heater", StringComparison.OrdinalIgnoreCase))
            {
                return part.Trim();
            }
        }

        return null;
    }

    private static int? ParseYear(string text)
    {
        var match = Regex.Match(text, @"\b(19|20)\d{2}\b");
        return match.Success && int.TryParse(match.Value, out var year) ? year : null;
    }

    private static DateTime? ParseDate(string text) =>
        DateTime.TryParse(text, out var date) ? date : null;

    private static bool IsMissingValue(string value)
    {
        var lower = value.Trim().ToLowerInvariant();
        return lower is "—" or "-" or "n/a" or "unknown" or "needs verification." or "needs verification"
               or "not publicly confirmed." or "not publicly confirmed";
    }

    private static bool IsUsableShortField(string value, int maxLength = 80)
    {
        if (IsMissingValue(value)) return false;
        var text = value.Trim();
        if (text.Length == 0 || text.Length > maxLength) return false;
        var lower = text.ToLowerInvariant();
        return !lower.Contains("not publicly confirmed")
               && !lower.Contains("needs verification")
               && !lower.Contains("should be verified");
    }

    private static string? ReadString(JsonNode? node) =>
        node?.GetValueKind() == System.Text.Json.JsonValueKind.String
            ? node.GetValue<string>()
            : node?.ToString();
}
