using System.Globalization;
using System.Text.Json;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class MyHomeDisplayService
{
    public static PropertyInfoViewModel? DeserializeProperty(Propiedad propiedad)
    {
        if (string.IsNullOrWhiteSpace(propiedad.DatosJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyInfoViewModel>(propiedad.DatosJson);
        }
        catch
        {
            return null;
        }
    }

    public static MyHomeSummaryViewModel BuildSummary(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var details = info?.PropertyDetails;
        return new MyHomeSummaryViewModel
        {
            PropiedadId = propiedad.Id,
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? "Property",
            HeroImageUrl = "/inspeccion2.jpeg",
            YearBuilt = details?.YearBuilt,
            LivingArea = details?.LivingArea,
            Bedrooms = details?.Bedrooms,
            Bathrooms = details?.Bathrooms,
            LotSizeAcres = details?.LotSize,
            EstimatedValue = details?.EstimatedValue,
            DataSource = info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated",
            AttomLastSyncUtc = propiedad.AttomLastSyncUtc
        };
    }

    public static MyHomePropertyDetailsViewModel BuildDetails(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var details = info?.PropertyDetails ?? new PropertyDetailsInfo();
        return new MyHomePropertyDetailsViewModel
        {
            PropiedadId = propiedad.Id,
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? string.Empty,
            ParcelId = details.ParcelNumber,
            Zoning = details.Zoning,
            AssignedSchool = details.AssignedSchool,
            LastSaleDate = details.LastSaleDate,
            LastSalePrice = details.LastSalePrice,
            AnnualTaxAmount = details.AnnualTaxAmount,
            TaxYear = details.TaxYear,
            YearBuilt = details.YearBuilt,
            LivingArea = details.LivingArea,
            LotSizeAcres = details.LotSize,
            PropertyType = details.PropertyType,
            EstimatedValue = details.EstimatedValue,
            DataSource = info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated"
        };
    }

    public static string FormatCurrency(decimal? value) =>
        value.HasValue ? value.Value.ToString("C0", CultureInfo.GetCultureInfo("en-US")) : "—";

    public static string FormatDate(DateTime? value) =>
        value.HasValue ? value.Value.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture) : "—";

    public static string FormatArea(int? sqft) =>
        sqft.HasValue ? $"{sqft.Value:N0} sq ft" : "—";

    public static string FormatLot(decimal? acres) =>
        acres.HasValue ? $"{acres.Value:0.##} acres" : "—";

    public static string RecordTypeLabel(string? type) => type switch
    {
        "Improvement" => "Improvement",
        "Repair" => "Repair",
        "Maintenance" => "Maintenance",
        _ => type ?? "Record"
    };

    public static string MaintenanceStatusLabel(string? status) => status switch
    {
        "Upcoming" => "Upcoming",
        "Completed" => "Completed",
        _ => status ?? "Upcoming"
    };

    public static string RecordTypeIcon(string? type) => type switch
    {
        "Improvement" => "fa-house-chimney",
        "Repair" => "fa-house-crack",
        "Maintenance" => "fa-snowflake",
        _ => "fa-house-circle-check"
    };

    public static string FormatDueDate(DateTime? value) =>
        value.HasValue ? $"Due on {value.Value:MMM dd, yyyy}" : "No due date";

    public static string FormatFileSize(long? bytes)
    {
        if (!bytes.HasValue || bytes.Value <= 0) return "—";
        if (bytes.Value < 1024) return $"{bytes.Value} B";
        if (bytes.Value < 1024 * 1024) return $"{bytes.Value / 1024.0:0.#} KB";
        return $"{bytes.Value / (1024.0 * 1024.0):0.#} MB";
    }

    public static readonly string[] DocumentCategories =
    [
        "Warranties",
        "Permits",
        "Invoices",
        "Contracts",
        "Inspections",
        "Manuals",
        "Other"
    ];
}
