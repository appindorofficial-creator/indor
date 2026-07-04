using System.Globalization;
using System.Text.Json;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class MyHomeDisplayService
{
    private static readonly JsonSerializerOptions PropertyJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static PropertyInfoViewModel? DeserializeProperty(Propiedad propiedad)
    {
        PropertyInfoViewModel? info = null;

        if (!string.IsNullOrWhiteSpace(propiedad.DatosJson))
        {
            try
            {
                info = JsonSerializer.Deserialize<PropertyInfoViewModel>(propiedad.DatosJson, PropertyJsonOptions);
            }
            catch
            {
                // fall through — maintenance may still exist in dedicated column
            }
        }

        if (!string.IsNullOrWhiteSpace(propiedad.AttomRawJson))
        {
            info ??= new PropertyInfoViewModel
            {
                FormattedAddress = propiedad.Direccion ?? string.Empty
            };
            info.PropertyDetails ??= new PropertyDetailsInfo();
            ApplyAttomPreservingUserEdits(info, propiedad.AttomRawJson);
        }

        var maintenance = PropertyMaintenanceDisplayService.ParseFromPropiedad(propiedad);
        if (maintenance != null)
        {
            info ??= new PropertyInfoViewModel
            {
                FormattedAddress = propiedad.Direccion ?? string.Empty
            };
            info.MaintenanceRecommendations ??= maintenance;
        }

        return info;
    }

    /// <summary>
    /// Re-applies the ATTOM/enrichment payload but keeps values the homeowner typed in
    /// "Edit property". Without this guard, <see cref="PropertyEnrichmentMapper.ApplyPayload"/>
    /// overwrites manually edited fields (YearBuilt, LivingArea, Bedrooms, Bathrooms,
    /// EstimatedValue, PropertyType) on every read, so saved edits appear to be lost.
    /// For non-edited properties the enrichment still wins as before.
    /// </summary>
    private static void ApplyAttomPreservingUserEdits(PropertyInfoViewModel info, string attomRawJson)
    {
        var details = info.PropertyDetails!;
        var userEdited = info.DataSource?.Contains("Edited", StringComparison.OrdinalIgnoreCase) == true;

        if (!userEdited)
        {
            PropertyEnrichmentMapper.ApplyPayload(info, attomRawJson);
            return;
        }

        var propertyType = details.PropertyType;
        var yearBuilt = details.YearBuilt;
        var livingArea = details.LivingArea;
        var lotSize = details.LotSize;
        var bedrooms = details.Bedrooms;
        var bathrooms = details.Bathrooms;
        var estimatedValue = details.EstimatedValue;
        var annualTax = details.AnnualTaxAmount;
        var taxYear = details.TaxYear;
        var parcelNumber = details.ParcelNumber;
        var zoning = details.Zoning;
        var assignedSchool = details.AssignedSchool;
        var lastSalePrice = details.LastSalePrice;
        var lastSaleDate = details.LastSaleDate;

        PropertyEnrichmentMapper.ApplyPayload(info, attomRawJson);

        // Restore homeowner-entered values; ATTOM only keeps fields the user left blank.
        if (!string.IsNullOrWhiteSpace(propertyType)) details.PropertyType = propertyType;
        if (yearBuilt.HasValue) details.YearBuilt = yearBuilt;
        if (livingArea.HasValue) details.LivingArea = livingArea;
        if (lotSize.HasValue) details.LotSize = lotSize;
        if (bedrooms.HasValue) details.Bedrooms = bedrooms;
        if (bathrooms.HasValue) details.Bathrooms = bathrooms;
        if (estimatedValue.HasValue) details.EstimatedValue = estimatedValue;
        if (annualTax.HasValue) details.AnnualTaxAmount = annualTax;
        if (taxYear.HasValue) details.TaxYear = taxYear;
        if (!string.IsNullOrWhiteSpace(parcelNumber)) details.ParcelNumber = parcelNumber;
        if (!string.IsNullOrWhiteSpace(zoning)) details.Zoning = zoning;
        if (!string.IsNullOrWhiteSpace(assignedSchool)) details.AssignedSchool = assignedSchool;
        if (lastSalePrice.HasValue) details.LastSalePrice = lastSalePrice;
        if (lastSaleDate.HasValue) details.LastSaleDate = lastSaleDate;
    }

    public static AddPropertyViewModel BuildAddressFormForEdit(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var form = new AddPropertyViewModel
        {
            Unit = info?.Unit
        };

        var street = ResolveStreetLine(info);
        if (!string.IsNullOrWhiteSpace(street))
        {
            form.StreetAddress = street;
        }

        if (!string.IsNullOrWhiteSpace(info?.City))
        {
            form.City = info.City.Trim();
        }

        if (!string.IsNullOrWhiteSpace(info?.State))
        {
            form.State = PropertyAdministratorCatalog.NormalizeUsStateCode(info.State)
                ?? info.State.Trim().ToUpperInvariant();
        }

        if (!string.IsNullOrWhiteSpace(info?.PostalCode))
        {
            form.ZipCode = info.PostalCode.Trim();
        }

        if (string.IsNullOrWhiteSpace(form.State))
        {
            form.State = PropertyAdministratorCatalog.TryExtractStateFromAddress(info?.FormattedAddress)
                ?? PropertyAdministratorCatalog.TryExtractStateFromAddress(propiedad.Direccion);
        }

        if (IsAddressFormComplete(form))
        {
            form.Address = form.BuildLookupAddress();
            return form;
        }

        foreach (var candidate in new[]
                 {
                     info?.FormattedAddress,
                     propiedad.Direccion
                 })
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (!PropertyAdministratorCatalog.TryParsePropertyLocation(
                    candidate,
                    out var parsedStreet,
                    out var parsedCity,
                    out var parsedState,
                    out var parsedZip))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(form.StreetAddress))
            {
                form.StreetAddress = parsedStreet;
            }

            if (string.IsNullOrWhiteSpace(form.City))
            {
                form.City = parsedCity;
            }

            if (string.IsNullOrWhiteSpace(form.State))
            {
                form.State = parsedState;
            }

            if (string.IsNullOrWhiteSpace(form.ZipCode))
            {
                form.ZipCode = parsedZip;
            }

            break;
        }

        if (string.IsNullOrWhiteSpace(form.StreetAddress)
            && !string.IsNullOrWhiteSpace(propiedad.Direccion))
        {
            form.StreetAddress = propiedad.Direccion.Trim();
        }

        form.Address = IsAddressFormComplete(form)
            ? form.BuildLookupAddress()
            : propiedad.Direccion ?? info?.FormattedAddress ?? string.Empty;

        return form;
    }

    private static string? ResolveStreetLine(PropertyInfoViewModel? info)
    {
        if (info == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(info.Street))
        {
            var street = info.Street.Trim();
            if (!string.IsNullOrWhiteSpace(info.HouseNumber)
                && !street.StartsWith(info.HouseNumber.Trim(), StringComparison.Ordinal))
            {
                return PropertyAdministratorCatalog.BuildStreetLine(info.HouseNumber, street);
            }

            return street;
        }

        return string.IsNullOrWhiteSpace(info.HouseNumber) ? null : info.HouseNumber.Trim();
    }

    private static bool IsAddressFormComplete(AddPropertyViewModel form) =>
        !string.IsNullOrWhiteSpace(form.StreetAddress)
        && !string.IsNullOrWhiteSpace(form.City)
        && !string.IsNullOrWhiteSpace(form.State)
        && !string.IsNullOrWhiteSpace(form.ZipCode);

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
            AttomLastSyncUtc = propiedad.AttomLastSyncUtc,
            AttomPropertyId = propiedad.AttomPropertyId ?? info?.AttomPropertyId,
            HasAttomData = !string.IsNullOrWhiteSpace(propiedad.AttomRawJson),
            AttomFieldCount = HouseFactDisplayService.BuildProfile(propiedad.AttomRawJson).FieldCount
        };
    }

    public static MyHomePropertyDetailsViewModel BuildDetails(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var details = info?.PropertyDetails ?? new PropertyDetailsInfo();
        var rawJson = propiedad.AttomRawJson;
        var houseFact = HouseFactDisplayService.BuildProfile(
            rawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated",
            propiedad.Direccion ?? info?.FormattedAddress);

        return new MyHomePropertyDetailsViewModel
        {
            PropiedadId = propiedad.Id,
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? string.Empty,
            AttomPropertyId = propiedad.AttomPropertyId ?? info?.AttomPropertyId,
            AttomLastSyncUtc = propiedad.AttomLastSyncUtc,
            HasAttomData = !string.IsNullOrWhiteSpace(rawJson),
            ParcelId = details.ParcelNumber,
            Fips = details.Fips,
            LegalDescription = details.LegalDescription,
            Zoning = details.Zoning,
            AssignedSchool = details.AssignedSchool,
            Subdivision = details.Subdivision,
            Municipality = details.Municipality,
            County = details.CountyName,
            LastSaleDate = details.LastSaleDate,
            LastSalePrice = details.LastSalePrice,
            AnnualTaxAmount = details.AnnualTaxAmount,
            TaxYear = details.TaxYear,
            YearBuilt = details.YearBuilt,
            YearBuiltEffective = details.YearBuiltEffective,
            LivingArea = details.LivingArea,
            LotSizeSqFt = details.LotSizeSqFt,
            LotSizeAcres = details.LotSize,
            Bedrooms = details.Bedrooms,
            Bathrooms = details.Bathrooms,
            RoomsTotal = details.RoomsTotal,
            Floors = details.Floors,
            PropertyType = details.PropertyType,
            Occupancy = details.Occupancy,
            EstimatedValue = details.EstimatedValue,
            HeatingType = details.HeatingType,
            HeatingFuel = details.HeatingFuel,
            CoolingType = details.CoolingType,
            BuildingCondition = details.BuildingCondition,
            WallType = details.WallType,
            ParkingType = details.ParkingType,
            GarageType = details.GarageType,
            BasementSqFt = details.BasementSqFt,
            Fireplaces = details.Fireplaces,
            LocationAccuracy = details.LocationAccuracy,
            Latitude = info?.Latitude,
            Longitude = info?.Longitude,
            Features = details.Features,
            DataSource = info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated",
            HouseFact = houseFact
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
