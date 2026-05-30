using System.Globalization;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class RegionalPropertyHints
{
    public static void Apply(PropertyInfoViewModel info)
    {
        if (info == null) return;

        var state = NormalizeState(info.State);
        var county = (info.PropertyDetails?.CountyName ?? info.County ?? string.Empty).ToLowerInvariant();
        var city = (info.City ?? info.PropertyDetails?.Municipality ?? string.Empty).ToLowerInvariant();
        var zip = info.PostalCode ?? string.Empty;

        if (state != "NC") return;

        info.PropertyDetails ??= new PropertyDetailsInfo();
        info.UtilityProviders ??= new UtilityProvidersInfo();

        if (IsMecklenburgArea(county, city, zip))
        {
            ApplyMecklenburgHints(info, city);
        }
    }

    private static void ApplyMecklenburgHints(PropertyInfoViewModel info, string city)
    {
        var details = info.PropertyDetails!;

        details.CountyName ??= "Mecklenburg";
        details.HeatingType ??= "Estimated: Central forced air";
        details.HeatingFuel ??= "Estimated: Natural gas or electric";
        details.CoolingType ??= "Estimated: Central air conditioning";
        details.BuildingCondition ??= "Estimated: Average — verify on inspection";
        details.WallType ??= "Estimated: Brick veneer or vinyl siding (common in area)";
        details.ParkingType ??= "Estimated: Driveway / attached garage";
        details.Municipality ??= TitleCase(city) ?? info.City;

        if (ShouldReplaceProvider(info.UtilityProviders!.Electric))
        {
            info.UtilityProviders.Electric = new UtilityProvider
            {
                Name = "Duke Energy Carolinas",
                ServiceType = "Electricity",
                Phone = "1-800-777-9898",
                Website = "https://www.duke-energy.com",
                Coverage = "Estimated: Mecklenburg County service area",
                Notes = "Regional default — confirm with utility lookup"
            };
        }

        if (ShouldReplaceProvider(info.UtilityProviders.Gas))
        {
            info.UtilityProviders.Gas = new UtilityProvider
            {
                Name = "Piedmont Natural Gas",
                ServiceType = "Natural gas",
                Phone = "1-800-752-7504",
                Website = "https://www.piedmontng.com",
                Coverage = "Estimated: Mecklenburg County service area",
                Notes = "Regional default — confirm with utility lookup"
            };
        }

        if (city.Contains("charlotte", StringComparison.OrdinalIgnoreCase))
        {
            if (ShouldReplaceProvider(info.UtilityProviders.Water))
            {
                info.UtilityProviders.Water = new UtilityProvider
                {
                    Name = "Charlotte Water",
                    ServiceType = "Water",
                    Phone = "(704) 336-7600",
                    Website = "https://www.charlottenc.gov/Services/Water",
                    Coverage = "Estimated: Charlotte municipal service",
                    Notes = "Regional default — confirm for exact address"
                };
            }

            if (ShouldReplaceProvider(info.UtilityProviders.Sewer))
            {
                info.UtilityProviders.Sewer = new UtilityProvider
                {
                    Name = "Charlotte Water",
                    ServiceType = "Sewer",
                    Phone = "(704) 336-7600",
                    Website = "https://www.charlottenc.gov/Services/Water",
                    Coverage = "Estimated: Charlotte municipal service",
                    Notes = "Regional default — confirm for exact address"
                };
            }
        }
        else if (city.Contains("mint hill", StringComparison.OrdinalIgnoreCase))
        {
            if (ShouldReplaceProvider(info.UtilityProviders.Water))
            {
                info.UtilityProviders.Water = new UtilityProvider
                {
                    Name = "Estimated: Mint Hill / county water service",
                    ServiceType = "Water",
                    Phone = "(704) 545-1065",
                    Website = "https://www.minthill.com",
                    Coverage = "Estimated: Mint Hill area",
                    Notes = "Verify provider for this exact address"
                };
            }

            if (ShouldReplaceProvider(info.UtilityProviders.Sewer))
            {
                info.UtilityProviders.Sewer = new UtilityProvider
                {
                    Name = "Estimated: Municipal or septic (verify)",
                    ServiceType = "Sewer",
                    Coverage = "Estimated: Mint Hill area",
                    Notes = "Verify on-site — some areas use septic"
                };
            }
        }

        if (info.UtilityProviders.Internet.Count == 0)
        {
            info.UtilityProviders.Internet.Add(new UtilityProvider
            {
                Name = "Estimated: Spectrum or AT&T Fiber",
                ServiceType = "Internet",
                Coverage = "Estimated: Mecklenburg County",
                Notes = "Availability varies by street — verify at FCC broadband map"
            });
        }

        details.AssignedSchool ??= "Estimated: Charlotte-Mecklenburg Schools (CMS) — verify assignment";
    }

    private static bool IsMecklenburgArea(string county, string city, string zip)
    {
        if (county.Contains("mecklenburg", StringComparison.OrdinalIgnoreCase)) return true;
        if (city.Contains("charlotte", StringComparison.OrdinalIgnoreCase)) return true;
        if (city.Contains("mint hill", StringComparison.OrdinalIgnoreCase)) return true;

        return zip.StartsWith("282", StringComparison.Ordinal)
            || zip.StartsWith("28105", StringComparison.Ordinal);
    }

    private static bool ShouldReplaceProvider(UtilityProvider? provider)
    {
        if (provider == null) return true;
        return IsPlaceholderProvider(provider.Name);
    }

    private static bool IsPlaceholderProvider(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return true;

        return name.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
            || name.Contains("needs verification", StringComparison.OrdinalIgnoreCase)
            || name.Contains("verify local", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeState(string? state)
    {
        if (string.IsNullOrWhiteSpace(state)) return null;
        var s = state.Trim();
        if (s.Equals("North Carolina", StringComparison.OrdinalIgnoreCase)) return "NC";
        return s.ToUpperInvariant();
    }

    private static string? TitleCase(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
    }
}
