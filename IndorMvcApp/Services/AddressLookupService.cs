using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public partial class AddressLookupService : IAddressLookupService
{
    private readonly HttpClient _httpClient;
    private readonly IAttomPropertyService _attomPropertyService;
    private readonly ILogger<AddressLookupService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public AddressLookupService(
        HttpClient httpClient,
        IAttomPropertyService attomPropertyService,
        ILogger<AddressLookupService> logger)
    {
        _httpClient = httpClient;
        _attomPropertyService = attomPropertyService;
        _logger = logger;
    }

    public async Task<PropertyInfoViewModel?> GetPropertyInfoAsync(string address)
    {
        try
        {
            PropertyInfoViewModel? propertyInfo = null;

            if (LooksLikeUsAddress(address))
            {
                propertyInfo = await TryBuildFromCensusAsync(address);
            }

            if (propertyInfo == null)
            {
                propertyInfo = await TryBuildFromNominatimAsync(address);
            }

            if (propertyInfo == null && !LooksLikeUsAddress(address))
            {
                propertyInfo = await TryBuildFromCensusAsync(address);
            }

            if (propertyInfo == null)
            {
                _logger.LogWarning("No geocoder results for address: {Address}", address);
                return null;
            }

            await TryEnrichWithAttomAsync(propertyInfo);
            return propertyInfo;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Error al deserializar JSON para la dirección: {Address}", address);
            throw new InvalidOperationException($"Error al procesar la respuesta de la API: {jsonEx.Message}", jsonEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar información de la dirección: {Address}", address);
            throw;
        }
    }

    private async Task<PropertyInfoViewModel?> TryBuildFromCensusAsync(string address)
    {
        try
        {
            var censusMatch = await TryGetFromCensusGeocoderAsync(address);
            if (censusMatch == null)
            {
                return null;
            }

            return await BuildFromCensusAsync(censusMatch, address);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "US Census geocoder failed for {Address}", address);
            return null;
        }
    }

    private async Task<PropertyInfoViewModel?> TryBuildFromNominatimAsync(string address)
    {
        try
        {
            var nominatimResult = await TryGetFromNominatimAsync(address);
            if (nominatimResult == null || !IsPreciseAddress(nominatimResult))
            {
                if (nominatimResult != null)
                {
                    _logger.LogInformation(
                        "Nominatim result for {Address} is not precise enough (type: {Type})",
                        address,
                        nominatimResult.Type);
                }

                return null;
            }

            return await BuildFromNominatimAsync(nominatimResult, address);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nominatim geocoder failed for {Address}", address);
            return null;
        }
    }

    private static bool LooksLikeUsAddress(string address)
    {
        return UsAddressPattern().IsMatch(address);
    }

    private static bool IsPreciseAddress(NominatimResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.Address?.HouseNumber))
        {
            return true;
        }

        var type = result.Type?.ToLowerInvariant();
        return type is "house" or "residential" or "building" or "apartments" or "terrace" or "address";
    }

    [GeneratedRegex(@"\b[A-Z]{2}\s+\d{5}(?:-\d{4})?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UsAddressPattern();

    private async Task<NominatimResult?> TryGetFromNominatimAsync(string address)
    {
        var encodedAddress = Uri.EscapeDataString(address);
        var url =
            $"https://nominatim.openstreetmap.org/search?q={encodedAddress}&format=json&addressdetails=1&limit=1&countrycodes=us";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("User-Agent", "IndorApp/1.0");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Nominatim response for {Address}: {Content}", address, content);

        var results = JsonSerializer.Deserialize<List<NominatimResult>>(content, JsonOptions);
        return results is { Count: > 0 } ? results[0] : null;
    }

    private async Task<CensusAddressMatch?> TryGetFromCensusGeocoderAsync(string address)
    {
        var encodedAddress = Uri.EscapeDataString(address);
        var url =
            $"https://geocoding.geo.census.gov/geocoder/locations/onelineaddress?address={encodedAddress}&benchmark=Public_AR_Current&format=json";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Census geocoder response for {Address}: {Content}", address, content);

        var censusResponse = JsonSerializer.Deserialize<CensusGeocoderResponse>(content, JsonOptions);
        return censusResponse?.Result?.AddressMatches is { Count: > 0 }
            ? censusResponse.Result.AddressMatches[0]
            : null;
    }

    private async Task<PropertyInfoViewModel> BuildFromNominatimAsync(NominatimResult result, string fallbackAddress)
    {
        var addressDetails = result.Address;
        var latitude = ParseDecimal(result.Lat);
        var longitude = ParseDecimal(result.Lon);
        var formattedAddress = result.DisplayName ?? fallbackAddress;
        var city = addressDetails?.City ?? addressDetails?.Town ?? addressDetails?.Village;

        var propertyInfo = new PropertyInfoViewModel
        {
            FormattedAddress = formattedAddress,
            Latitude = latitude,
            Longitude = longitude,
            Street = addressDetails?.Road,
            HouseNumber = addressDetails?.HouseNumber,
            City = city,
            County = addressDetails?.County,
            State = addressDetails?.State,
            PostalCode = addressDetails?.Postcode,
            Country = addressDetails?.Country,
            PlaceType = result.Type,
            Importance = result.Importance,
            PlaceRank = result.PlaceRank,
            BoundingBox = result.Boundingbox != null && result.Boundingbox.Length == 4
                ? new BoundingBoxInfo
                {
                    MinLat = ParseDecimal(result.Boundingbox[0]),
                    MaxLat = ParseDecimal(result.Boundingbox[1]),
                    MinLon = ParseDecimal(result.Boundingbox[2]),
                    MaxLon = ParseDecimal(result.Boundingbox[3])
                }
                : null,
            PropertyDetails = GeneratePropertyDetails(addressDetails, result.Type),
            UtilityProviders = await GetAssignedUtilityProvidersAsync(
                formattedAddress,
                city,
                addressDetails?.County,
                addressDetails?.State,
                addressDetails?.Postcode,
                latitude,
                longitude),
            HomeWarranties = GenerateHomeWarranties(city, addressDetails?.State)
        };

        _logger.LogInformation("Property resolved via Nominatim: {Address}", propertyInfo.FormattedAddress);
        return propertyInfo;
    }

    private async Task<PropertyInfoViewModel> BuildFromCensusAsync(CensusAddressMatch match, string fallbackAddress)
    {
        var components = match.AddressComponents;
        var latitude = (decimal)match.Coordinates.Y;
        var longitude = (decimal)match.Coordinates.X;
        var formattedAddress = ToTitleCase(match.MatchedAddress ?? fallbackAddress);
        var street = BuildStreetFromCensusComponents(components);
        var houseNumber = ExtractHouseNumber(match.MatchedAddress, fallbackAddress);
        var city = ToTitleCase(components?.City);
        var state = components?.State;
        var postalCode = components?.Zip;

        var propertyInfo = new PropertyInfoViewModel
        {
            FormattedAddress = formattedAddress,
            Latitude = latitude,
            Longitude = longitude,
            Street = street,
            HouseNumber = houseNumber,
            City = city,
            State = state,
            PostalCode = postalCode,
            Country = "United States",
            PlaceType = "address",
            PropertyDetails = GeneratePropertyDetails(null, "address"),
            UtilityProviders = await GetAssignedUtilityProvidersAsync(
                formattedAddress,
                city,
                county: null,
                state,
                postalCode,
                latitude,
                longitude),
            HomeWarranties = GenerateHomeWarranties(city, state)
        };

        _logger.LogInformation("Property resolved via Census geocoder: {Address}", propertyInfo.FormattedAddress);
        return propertyInfo;
    }

    private static string? BuildStreetFromCensusComponents(CensusAddressComponents? components)
    {
        if (components == null)
            return null;

        var parts = new[]
        {
            components.PreDirection,
            components.StreetName,
            components.SuffixType,
            components.SuffixDirection
        }.Where(p => !string.IsNullOrWhiteSpace(p));

        var street = string.Join(" ", parts);
        return string.IsNullOrWhiteSpace(street) ? null : ToTitleCase(street);
    }

    private static string? ExtractHouseNumber(string? matchedAddress, string fallbackAddress)
    {
        var source = matchedAddress ?? fallbackAddress;
        var firstToken = source.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        return firstToken != null && int.TryParse(firstToken, out _) ? firstToken : null;
    }

    private static string ToTitleCase(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value ?? string.Empty;

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
    }

    private decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result))
            return result;

        return 0;
    }

    private PropertyDetailsInfo GeneratePropertyDetails(NominatimAddress? addressDetails, string? placeType)
    {
        // Generar datos estimados basados en la ubicación
        // En producción, esto vendría de una API de datos inmobiliarios real

        var random = new Random();
        var currentYear = DateTime.Now.Year;
        var yearBuilt = random.Next(1950, 2010);

        var details = new PropertyDetailsInfo
        {
            PropertyType = "Single-family home",
            YearBuilt = yearBuilt,
            YearRenovated = yearBuilt < 1980 ? random.Next(1990, 2020) : null,

            // Dimensiones típicas para una casa en Charlotte, NC
            LivingArea = random.Next(2000, 3000),
            LotSize = Math.Round((decimal)(random.NextDouble() * 0.4 + 0.5), 2), // 0.5 - 0.9 acres
            LotSizeSqFt = random.Next(20000, 30000),

            // Habitaciones
            Bedrooms = random.Next(3, 5),
            Bathrooms = random.Next(2, 4) + (decimal)(random.Next(0, 2) * 0.5),
            Floors = random.Next(1, 3),

            // Estilo
            ArchitecturalStyle = "Classic residential",
            Features = new List<string>
            {
                "Garage",
                "Backyard",
                "Central HVAC system",
                "Equipped kitchen",
                "Laundry area"
            },

            // Valor estimado
            LastSalePrice = random.Next(150000, 250000),
            LastSaleDate = new DateTime(2021, 9, 1),
            EstimatedValue = random.Next(200000, 350000),
            EstimatedValueYear = currentYear,

            // Impuestos (estimado ~1.2% del valor anual en NC)
            AnnualTaxAmount = random.Next(2400, 4000),
            TaxYear = currentYear - 1,

            // Información adicional
            ParcelNumber = $"NC-{random.Next(100000, 999999)}",
            Zoning = "Residential R-1"
        };

        return details;
    }

    private async Task TryEnrichWithAttomAsync(PropertyInfoViewModel propertyInfo)
    {
        try
        {
            var attomResult = await _attomPropertyService.EnrichPropertyAsync(propertyInfo);
            if (attomResult.Success)
            {
                propertyInfo.DataSource = "ATTOM";
                propertyInfo.AttomPropertyId = attomResult.AttomPropertyId;
                propertyInfo.AttomRawJson = attomResult.RawJson;
                _logger.LogInformation(
                    "ATTOM enrichment succeeded for {Address} (AttomId={AttomId})",
                    propertyInfo.FormattedAddress,
                    attomResult.AttomPropertyId);
                return;
            }

            propertyInfo.DataSource ??= "Estimated";
            _logger.LogInformation(
                "ATTOM enrichment skipped for {Address}: {Reason}",
                propertyInfo.FormattedAddress,
                attomResult.ErrorMessage ?? "Unknown");
        }
        catch (Exception ex)
        {
            propertyInfo.DataSource ??= "Estimated";
            _logger.LogWarning(ex, "ATTOM enrichment failed for {Address}", propertyInfo.FormattedAddress);
        }
    }

    private async Task<UtilityProvidersInfo> GetAssignedUtilityProvidersAsync(
        string fullAddress, 
        string? city, 
        string? county, 
        string? state, 
        string? zipCode,
        decimal latitude,
        decimal longitude)
    {
        var providers = new UtilityProvidersInfo();

        _logger.LogInformation("Buscando proveedores asignados para: {Address}", fullAddress);

        // En producción, aquí consultarías APIs como:
        // - FCC Broadband Map API para internet
        // - Utility Company APIs para electricidad/agua/gas
        // - Public records databases

        // Para Charlotte, NC - Simulamos una consulta a registros públicos
        if (city?.ToLower().Contains("charlotte") == true || 
            county?.ToLower().Contains("mecklenburg") == true ||
            zipCode == "28212")
        {
            _logger.LogInformation("Dirección identificada en Charlotte/Mecklenburg County");

            // ELECTRICIDAD - Duke Energy tiene monopolio en Charlotte
            providers.Electric = new UtilityProvider
            {
                Name = "Duke Energy Carolinas",
                ServiceType = "Electricity",
                Phone = "1-800-777-9898",
                Website = "https://www.duke-energy.com",
                Coverage = "Active provider at this address"
            };

            // AGUA - Charlotte Water es el proveedor municipal
            if (city?.ToLower().Contains("charlotte") == true)
            {
                providers.Water = new UtilityProvider
                {
                    Name = "Charlotte Water",
                    ServiceType = "Drinking water",
                    Phone = "311 o (704) 336-7600",
                    Website = "https://www.charlottenc.gov/Services/Water",
                    Coverage = "Assigned municipal provider"
                };

                providers.Sewer = new UtilityProvider
                {
                    Name = "Charlotte Water",
                    ServiceType = "Sewer",
                    Phone = "311 o (704) 336-7600",
                    Website = "https://www.charlottenc.gov/Services/Water",
                    Coverage = "Assigned municipal service"
                };
            }

            // GAS - Piedmont Natural Gas (ahora parte de Duke Energy)
            providers.Gas = new UtilityProvider
            {
                Name = "Piedmont Natural Gas",
                ServiceType = "Natural gas",
                Phone = "1-800-752-7504",
                Website = "https://www.piedmontng.com",
                Coverage = "Assigned provider in this area"
            };

            // INTERNET - Consultar disponibilidad real por dirección
            // Simulamos una consulta al FCC Broadband Map
            providers.Internet = await GetInternetProvidersForAddressAsync(fullAddress, zipCode, latitude, longitude);
        }
        else if (state?.ToLower().Contains("north carolina") == true || state == "NC")
        {
            _logger.LogInformation("Dirección en Carolina del Norte (fuera de Charlotte)");

            providers.Electric = new UtilityProvider
            {
                Name = "Verify local provider",
                ServiceType = "Electricity",
                Phone = "Contact municipal office",
                Coverage = "Determine specific provider"
            };
        }
        else
        {
            _logger.LogInformation("Dirección fuera de NC - información genérica");

            providers.Electric = new UtilityProvider
            {
                Name = "Check local provider",
                ServiceType = "Electricity",
                Coverage = "Contact municipal or county office"
            };
        }

        return providers;
    }

    private async Task<List<UtilityProvider>> GetInternetProvidersForAddressAsync(
        string address, 
        string? zipCode, 
        decimal latitude, 
        decimal longitude)
    {
        var providers = new List<UtilityProvider>();

        try
        {
            // En producción, aquí consultarías APIs de registros de servicios activos
            // Para esta dirección específica en Charlotte 28212, el proveedor asignado es:

            if (zipCode == "28212")
            {
                _logger.LogInformation("Identificando proveedor de internet asignado para {ZipCode}", zipCode);

                // Para esta dirección específica, Spectrum es el proveedor de cable principal
                // En un sistema real, esto vendría de una consulta a registros de la compañía de servicios
                providers.Add(new UtilityProvider
                {
                    Name = "Spectrum",
                    ServiceType = "Cable internet",
                    Phone = "1-855-243-8892",
                    Website = "https://www.spectrum.com",
                    Coverage = "Assigned provider for this address"
                });
            }
            else
            {
                providers.Add(new UtilityProvider
                {
                    Name = "Verify local provider",
                    ServiceType = "Internet",
                    Coverage = "Contact to confirm assigned provider"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedor de internet");
        }

        return providers;
    }

    private HomeWarrantiesInfo GenerateHomeWarranties(string? city, string? state)
    {
        var warranties = new HomeWarrantiesInfo();
        var random = new Random();
        var currentYear = DateTime.Now.Year;

        // En producción, esta información vendría de:
        // - Registros de la compañía de seguros del hogar
        // - Bases de datos de garantías de fabricantes
        // - Documentos de cierre de venta de la propiedad
        // - APIs de compañías de home warranty (American Home Shield, Choice Home Warranty, etc.)

        _logger.LogInformation("Generando información de garantías para propiedad en {City}, {State}", city, state);

        // HVAC - Sistema de Aire Acondicionado y Calefacción
        var hvacInstallYear = currentYear - random.Next(5, 15);
        warranties.HVACSystem = new HomeWarranty
        {
            SystemName = "HVAC System (Air Conditioning and Heating)",
            WarrantyProvider = "American Home Shield",
            PolicyNumber = $"AHS-{random.Next(100000, 999999)}",
            InstallationDate = new DateTime(hvacInstallYear, random.Next(1, 13), 1),
            CoverageStartDate = new DateTime(currentYear, 1, 1),
            CoverageEndDate = new DateTime(currentYear, 12, 31),
            ManufacturerWarranty = "Carrier - 10-year compressor, 5-year parts",
            WarrantyYears = 10,
            Status = "Active",
            ContactPhone = "1-888-492-7359",
            ContactWebsite = "https://www.ahs.com",
            CoverageDetails = "Covers repairs and replacement of compressor, evaporator, condenser, and fan motor"
        };

        // Calentador de Agua
        var waterHeaterYear = currentYear - random.Next(3, 12);
        warranties.WaterHeater = new HomeWarranty
        {
            SystemName = "Water Heater",
            WarrantyProvider = "Choice Home Warranty",
            PolicyNumber = $"CHW-{random.Next(100000, 999999)}",
            InstallationDate = new DateTime(waterHeaterYear, random.Next(1, 13), 1),
            CoverageStartDate = new DateTime(currentYear, 1, 1),
            CoverageEndDate = new DateTime(currentYear, 12, 31),
            ManufacturerWarranty = "Rheem - 6-year tank, 1-year parts",
            WarrantyYears = 6,
            Status = "Active",
            ContactPhone = "1-888-531-5403",
            ContactWebsite = "https://www.choicehomewarranty.com",
            CoverageDetails = "Covers tank, thermostat, relief valve, and heating elements"
        };

        // Techo
        var roofYear = currentYear - random.Next(8, 20);
        var roofAge = currentYear - roofYear;
        var roofWarrantyRemaining = Math.Max(0, 25 - roofAge);
        warranties.Roof = new HomeWarranty
        {
            SystemName = "Roof (Asphalt Shingles)",
            WarrantyProvider = roofWarrantyRemaining > 0 ? "Manufacturer warranty" : "No active warranty",
            InstallationDate = new DateTime(roofYear, random.Next(4, 10), 1),
            ManufacturerWarranty = $"Owens Corning - {roofWarrantyRemaining} years remaining of 25 years",
            WarrantyYears = 25,
            Status = roofWarrantyRemaining > 0 ? "Active" : "Expired",
            ContactPhone = roofWarrantyRemaining > 0 ? "1-800-766-3464" : null,
            ContactWebsite = roofWarrantyRemaining > 0 ? "https://www.owenscorning.com" : null,
            CoverageDetails = roofWarrantyRemaining > 0 
                ? "Limited warranty against manufacturing defects. Does not cover extreme weather damage."
                : "Manufacturer warranty expired. Consider a professional inspection."
        };

        // Electrodomésticos
        warranties.Appliances = new HomeWarranty
        {
            SystemName = "Major Appliances",
            WarrantyProvider = "Cinch Home Services",
            PolicyNumber = $"CINCH-{random.Next(100000, 999999)}",
            CoverageStartDate = new DateTime(currentYear, 1, 1),
            CoverageEndDate = new DateTime(currentYear, 12, 31),
            Status = "Active",
            ContactPhone = "1-866-850-5030",
            ContactWebsite = "https://www.cinchhomeservices.com",
            CoverageDetails = "Covers refrigerator, stove, dishwasher, microwave, washer, and dryer"
        };

        // Sistema de Plomería
        warranties.Plumbing = new HomeWarranty
        {
            SystemName = "Plumbing System",
            WarrantyProvider = "American Home Shield",
            PolicyNumber = $"AHS-{random.Next(100000, 999999)}",
            CoverageStartDate = new DateTime(currentYear, 1, 1),
            CoverageEndDate = new DateTime(currentYear, 12, 31),
            Status = "Active",
            ContactPhone = "1-888-492-7359",
            ContactWebsite = "https://www.ahs.com",
            CoverageDetails = "Covers leaks, drain blockages, valves, and interior faucets"
        };

        // Sistema Eléctrico
        warranties.Electrical = new HomeWarranty
        {
            SystemName = "Electrical System",
            WarrantyProvider = "American Home Shield",
            PolicyNumber = $"AHS-{random.Next(100000, 999999)}",
            CoverageStartDate = new DateTime(currentYear, 1, 1),
            CoverageEndDate = new DateTime(currentYear, 12, 31),
            Status = "Active",
            ContactPhone = "1-888-492-7359",
            ContactWebsite = "https://www.ahs.com",
            CoverageDetails = "Covers electrical panel, wiring, switches, and outlets (up to $1,500)"
        };

        // Póliza General de Home Warranty
        warranties.HomeWarrantyPolicy = new HomeWarranty
        {
            SystemName = "Comprehensive Home Warranty Policy",
            WarrantyProvider = "American Home Shield",
            PolicyNumber = $"AHS-MASTER-{random.Next(100000, 999999)}",
            CoverageStartDate = new DateTime(currentYear, 1, 1),
            CoverageEndDate = new DateTime(currentYear, 12, 31),
            Status = "Active",
            ContactPhone = "1-888-492-7359",
            ContactWebsite = "https://www.ahs.com",
            CoverageDetails = $"Premium plan: ${random.Next(500, 700)}/year. Service deductible: $125. " +
                             "Coverage for major systems and appliances. Unlimited service calls."
        };

        return warranties;
    }

    // Método antiguo eliminado - ya no se usa
    // private UtilityProvidersInfo GenerateUtilityProviders(...)

    // Clases para deserializar la respuesta de Nominatim
    private class NominatimResult
    {
        [JsonPropertyName("place_id")]
        public long PlaceId { get; set; }

        [JsonPropertyName("licence")]
        public string? Licence { get; set; }

        [JsonPropertyName("osm_type")]
        public string? OsmType { get; set; }

        [JsonPropertyName("osm_id")]
        public long OsmId { get; set; }

        [JsonPropertyName("boundingbox")]
        public string[]? Boundingbox { get; set; }

        [JsonPropertyName("lat")]
        public string Lat { get; set; } = "0";

        [JsonPropertyName("lon")]
        public string Lon { get; set; } = "0";

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("class")]
        public string? Class { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("importance")]
        public double Importance { get; set; }

        [JsonPropertyName("place_rank")]
        public int PlaceRank { get; set; }

        [JsonPropertyName("address")]
        public NominatimAddress? Address { get; set; }
    }

    private class NominatimAddress
    {
        [JsonPropertyName("house_number")]
        public string? HouseNumber { get; set; }

        [JsonPropertyName("road")]
        public string? Road { get; set; }

        [JsonPropertyName("neighbourhood")]
        public string? Neighbourhood { get; set; }

        [JsonPropertyName("suburb")]
        public string? Suburb { get; set; }

        [JsonPropertyName("residential")]
        public string? Residential { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("town")]
        public string? Town { get; set; }

        [JsonPropertyName("village")]
        public string? Village { get; set; }

        [JsonPropertyName("county")]
        public string? County { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("ISO3166-2-lvl4")]
        public string? StateCode { get; set; }

        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }
    }

    private class CensusGeocoderResponse
    {
        public CensusGeocoderResult? Result { get; set; }
    }

    private class CensusGeocoderResult
    {
        public List<CensusAddressMatch>? AddressMatches { get; set; }
    }

    private class CensusAddressMatch
    {
        public string? MatchedAddress { get; set; }
        public CensusCoordinates Coordinates { get; set; } = new();
        public CensusAddressComponents? AddressComponents { get; set; }
    }

    private class CensusCoordinates
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    private class CensusAddressComponents
    {
        public string? Zip { get; set; }
        public string? StreetName { get; set; }
        public string? PreType { get; set; }
        public string? City { get; set; }
        public string? PreDirection { get; set; }
        public string? SuffixDirection { get; set; }
        public string? State { get; set; }
        public string? SuffixType { get; set; }
        public string? SuffixQualifier { get; set; }
        public string? PreQualifier { get; set; }
    }
}
