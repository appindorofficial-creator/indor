using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class PropertyInfoViewModel
{
    // Dirección completa
    public string FormattedAddress { get; set; } = string.Empty;

    // Coordenadas geográficas
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    // Componentes de la dirección
    public string? Street { get; set; }
    public string? HouseNumber { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    // Información adicional del lugar
    public string? PlaceType { get; set; }
    public double Importance { get; set; }
    public int PlaceRank { get; set; }

    // Bounding box
    public BoundingBoxInfo? BoundingBox { get; set; }

    // Información de la propiedad (expandida)
    public PropertyDetailsInfo? PropertyDetails { get; set; }

    // Proveedores de servicios públicos
    public UtilityProvidersInfo? UtilityProviders { get; set; }

    // Seguros y garantías de sistemas de la casa
    public HomeWarrantiesInfo? HomeWarranties { get; set; }

    // Devices added by the user
    public List<DeviceInfoViewModel> Devices { get; set; } = new();

    // ATTOM metadata (also persisted on Propiedad columns)
    public long? AttomPropertyId { get; set; }
    public string? AttomRawJson { get; set; }
    public string? DataSource { get; set; }
}

public class DeviceInfoViewModel
{
    [Display(Name = "Device type")]
    public string Type { get; set; } = string.Empty;

    [Display(Name = "Serial")]
    public string Serial { get; set; } = string.Empty;

    [Display(Name = "Warranty date")]
    [DataType(DataType.Date)]
    public DateTime? WarrantyDate { get; set; }
}

public class BoundingBoxInfo
{
    public decimal MinLat { get; set; }
    public decimal MaxLat { get; set; }
    public decimal MinLon { get; set; }
    public decimal MaxLon { get; set; }
}

public class PropertyDetailsInfo
{
    // Información general
    public string PropertyType { get; set; } = "Single-family home";
    public int? YearBuilt { get; set; }
    public int? YearRenovated { get; set; }

    // Dimensiones
    public int? LivingArea { get; set; } // Área construida en pies cuadrados
    public decimal? LotSize { get; set; } // Tamaño del terreno en acres
    public int? LotSizeSqFt { get; set; } // Tamaño del terreno en pies cuadrados

    // Habitaciones
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public int? Floors { get; set; }

    // Estilo y características
    public string? ArchitecturalStyle { get; set; }
    public List<string> Features { get; set; } = new();

    // Valor y ventas
    public decimal? LastSalePrice { get; set; }
    public DateTime? LastSaleDate { get; set; }
    public decimal? EstimatedValue { get; set; }
    public int? EstimatedValueYear { get; set; }

    // Impuestos
    public decimal? AnnualTaxAmount { get; set; }
    public int? TaxYear { get; set; }

    // Additional information
    public string? ParcelNumber { get; set; }
    public string? LegalDescription { get; set; }
    public string? Zoning { get; set; }
    public string? AssignedSchool { get; set; }

    // ATTOM extended fields (also available in AttomRawJson on Propiedad)
    public string? Fips { get; set; }
    public string? Subdivision { get; set; }
    public string? Municipality { get; set; }
    public string? CountyName { get; set; }
    public string? Occupancy { get; set; }
    public int? YearBuiltEffective { get; set; }
    public int? RoomsTotal { get; set; }
    public int? BathsFull { get; set; }
    public string? HeatingType { get; set; }
    public string? HeatingFuel { get; set; }
    public string? CoolingType { get; set; }
    public string? BuildingCondition { get; set; }
    public string? WallType { get; set; }
    public string? ParkingType { get; set; }
    public string? GarageType { get; set; }
    public int? BasementSqFt { get; set; }
    public int? Fireplaces { get; set; }
    public string? LocationAccuracy { get; set; }
}

public class UtilityProvidersInfo
{
    public UtilityProvider? Electric { get; set; }
    public UtilityProvider? Water { get; set; }
    public UtilityProvider? Gas { get; set; }
    public UtilityProvider? Sewer { get; set; }
    public List<UtilityProvider> Internet { get; set; } = new();
    public List<UtilityProvider> CableTV { get; set; } = new();
}

public class UtilityProvider
{
    public string Name { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Coverage { get; set; }
    public string? Notes { get; set; }
}

public class HomeWarrantiesInfo
{
    public HomeWarranty? HVACSystem { get; set; }
    public HomeWarranty? WaterHeater { get; set; }
    public HomeWarranty? Roof { get; set; }
    public HomeWarranty? Appliances { get; set; }
    public HomeWarranty? Plumbing { get; set; }
    public HomeWarranty? Electrical { get; set; }
    public HomeWarranty? HomeWarrantyPolicy { get; set; }
}

public class HomeWarranty
{
    public string SystemName { get; set; } = string.Empty;
    public string? WarrantyProvider { get; set; }
    public string? PolicyNumber { get; set; }
    public DateTime? CoverageStartDate { get; set; }
    public DateTime? CoverageEndDate { get; set; }
    public DateTime? InstallationDate { get; set; }
    public string? ManufacturerWarranty { get; set; }
    public int? WarrantyYears { get; set; }
    public string? Status { get; set; } // "Activa", "Expirada", "Por renovar"
    public string? ContactPhone { get; set; }
    public string? ContactWebsite { get; set; }
    public string? CoverageDetails { get; set; }
}
