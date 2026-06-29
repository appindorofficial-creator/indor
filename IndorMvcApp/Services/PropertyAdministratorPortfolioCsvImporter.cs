using System.Text;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class PropertyAdministratorPortfolioCsvImporter
{
    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["housenumber"] = nameof(PropertyAdministratorPropertyInput.HouseNumber),
        ["house_number"] = nameof(PropertyAdministratorPropertyInput.HouseNumber),
        ["number"] = nameof(PropertyAdministratorPropertyInput.HouseNumber),
        ["streetname"] = nameof(PropertyAdministratorPropertyInput.StreetName),
        ["street_name"] = nameof(PropertyAdministratorPropertyInput.StreetName),
        ["street"] = nameof(PropertyAdministratorPropertyInput.StreetName),
        ["city"] = nameof(PropertyAdministratorPropertyInput.City),
        ["state"] = nameof(PropertyAdministratorPropertyInput.State),
        ["zipcode"] = nameof(PropertyAdministratorPropertyInput.ZipCode),
        ["zip_code"] = nameof(PropertyAdministratorPropertyInput.ZipCode),
        ["zip"] = nameof(PropertyAdministratorPropertyInput.ZipCode),
        ["propertytype"] = nameof(PropertyAdministratorPropertyInput.PropertyType),
        ["property_type"] = nameof(PropertyAdministratorPropertyInput.PropertyType),
        ["type"] = nameof(PropertyAdministratorPropertyInput.PropertyType),
        ["propertynickname"] = nameof(PropertyAdministratorPropertyInput.PropertyName),
        ["property_nickname"] = nameof(PropertyAdministratorPropertyInput.PropertyName),
        ["nickname"] = nameof(PropertyAdministratorPropertyInput.PropertyName),
        ["name"] = nameof(PropertyAdministratorPropertyInput.PropertyName)
    };

    public static PropertyAdministratorPortfolioImportResult ParseAndValidate(Stream csvStream)
    {
        var result = new PropertyAdministratorPortfolioImportResult();
        using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

        var headerLine = ReadNextDataLine(reader);
        if (headerLine == null)
        {
            result.Errors.Add("The CSV file is empty.");
            return result;
        }

        var columnMap = MapHeaders(ParseLine(headerLine));
        if (!columnMap.ContainsKey(nameof(PropertyAdministratorPropertyInput.StreetName))
            || !columnMap.ContainsKey(nameof(PropertyAdministratorPropertyInput.City))
            || !columnMap.ContainsKey(nameof(PropertyAdministratorPropertyInput.State))
            || !columnMap.ContainsKey(nameof(PropertyAdministratorPropertyInput.ZipCode))
            || !columnMap.ContainsKey(nameof(PropertyAdministratorPropertyInput.PropertyType)))
        {
            result.Errors.Add("CSV must include columns: houseNumber, streetName, city, state, zipCode, propertyType.");
            return result;
        }

        var lineNumber = 1;
        string? line;
        while ((line = ReadNextDataLine(reader)) != null)
        {
            lineNumber++;
            var cells = ParseLine(line);
            if (cells.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var row = MapRow(cells, columnMap);
            var missing = ValidatePropertyInput(row);
            if (missing.Count > 0)
            {
                result.Errors.Add($"Row {lineNumber}: please enter {string.Join(", ", missing)}.");
                continue;
            }

            var streetLine = PropertyAdministratorCatalog.BuildStreetLine(row.HouseNumber, row.StreetName);
            row.StreetAddress = streetLine;
            row.Location = PropertyAdministratorCatalog.FormatPropertyLocation(row.City, row.State, streetLine, row.ZipCode);
            if (string.IsNullOrWhiteSpace(row.PropertyName))
            {
                row.PropertyName = streetLine;
            }

            result.Properties.Add(row);
        }

        if (result.Properties.Count == 0 && result.Errors.Count == 0)
        {
            result.Errors.Add("No property rows were found in the CSV file.");
        }

        return result;
    }

    private static Dictionary<string, int> MapHeaders(IReadOnlyList<string> headers)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headers.Count; i++)
        {
            var raw = headers[i].Trim();
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var normalized = raw.Replace(" ", "", StringComparison.Ordinal).Replace("_", "", StringComparison.Ordinal);
            if (HeaderAliases.TryGetValue(normalized, out var field))
            {
                map[field] = i;
            }
        }

        return map;
    }

    private static PropertyAdministratorPropertyInput MapRow(
        IReadOnlyList<string> cells,
        IReadOnlyDictionary<string, int> columnMap)
    {
        string Get(string field) =>
            columnMap.TryGetValue(field, out var index) && index < cells.Count
                ? cells[index].Trim()
                : "";

        var rawType = Get(nameof(PropertyAdministratorPropertyInput.PropertyType));
        return new PropertyAdministratorPropertyInput
        {
            HouseNumber = Get(nameof(PropertyAdministratorPropertyInput.HouseNumber)),
            StreetName = Get(nameof(PropertyAdministratorPropertyInput.StreetName)),
            City = Get(nameof(PropertyAdministratorPropertyInput.City)),
            State = Get(nameof(PropertyAdministratorPropertyInput.State)),
            ZipCode = Get(nameof(PropertyAdministratorPropertyInput.ZipCode)),
            PropertyType = PropertyAdministratorCatalog.ResolvePropertyType(rawType) ?? rawType,
            PropertyName = Get(nameof(PropertyAdministratorPropertyInput.PropertyName))
        };
    }

    public static List<string> ValidatePropertyInput(PropertyAdministratorPropertyInput input)
    {
        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(input.HouseNumber))
        {
            missingFields.Add("house number");
        }

        if (string.IsNullOrWhiteSpace(input.StreetName))
        {
            missingFields.Add("street name");
        }

        if (string.IsNullOrWhiteSpace(input.City))
        {
            missingFields.Add("city");
        }

        if (string.IsNullOrWhiteSpace(input.State))
        {
            missingFields.Add("state");
        }

        if (string.IsNullOrWhiteSpace(input.ZipCode))
        {
            missingFields.Add("ZIP");
        }

        if (string.IsNullOrWhiteSpace(input.PropertyType)
            || !PropertyAdministratorCatalog.IsValidPropertyType(input.PropertyType))
        {
            missingFields.Add("property type");
        }

        return missingFields;
    }

    private static string? ReadNextDataLine(TextReader reader)
    {
        while (true)
        {
            var line = reader.ReadLine();
            if (line == null)
            {
                return null;
            }

            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            return line;
        }
    }

    private static List<string> ParseLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        values.Add(current.ToString());
        return values;
    }
}
