namespace IndorMvcApp.Services;

public static class InspeccionDisplayLabels
{
    public static string MotivoRevision(string? value) => value switch
    {
        "BuyingHome" => "Buying a home",
        "SafetyCheck" => "Safety check",
        "IssueAtHome" => "Issue at home",
        "InspectionFollowUp" => "Inspection follow-up",
        _ => "General review"
    };

    public static string PreocupacionPrincipal(string? value) => value switch
    {
        "BreakerTrips" => "Breaker trips",
        "LightsFlicker" => "Lights flicker",
        "OutletsNotWorking" => "Outlets not working",
        "OldPanel" => "Old panel",
        "BurningSmell" => "Burning smell",
        "GeneralReview" => "General electrical review",
        _ => "General electrical review"
    };

    public static string ObjetivoPrincipal(string? value) => value switch
    {
        "BuyWithConfidence" => "Buy with confidence",
        "UnderstandRepairRisks" => "Understand repair risks",
        "NegotiateRepairs" => "Negotiate repairs",
        "SecondOpinion" => "Second opinion",
        _ => "Home purchase review"
    };

    public static string RolComprador(string? value) => value switch
    {
        "Buyer" => "Buyer",
        "Realtor" => "Realtor",
        "Investor" => "Investor",
        _ => value ?? "Buyer"
    };

    public static string FormatElectricalConcern(string? preocupacion, string? motivo)
    {
        return $"{PreocupacionPrincipal(preocupacion)} / {MotivoRevision(motivo).ToLowerInvariant()}";
    }

    public static string FormatPurchaseConcern(string? objetivo, string? notas, string? rol)
    {
        var summary = ObjetivoPrincipal(objetivo);
        if (!string.IsNullOrWhiteSpace(notas))
        {
            var trimmed = notas.Trim();
            if (trimmed.Length > 60)
            {
                trimmed = trimmed[..57] + "...";
            }

            return $"{summary} — {trimmed}";
        }

        return $"{summary} ({RolComprador(rol)})";
    }

    public static string FormatTime(TimeSpan time)
    {
        var hours = time.Hours;
        var minutes = time.Minutes;
        var period = hours >= 12 ? "PM" : "AM";
        var displayHour = hours % 12;
        if (displayHour == 0)
        {
            displayHour = 12;
        }

        return minutes == 0
            ? $"{displayHour}:00 {period}"
            : $"{displayHour}:{minutes:D2} {period}";
    }

    public static string MotivoInspeccionCompleta(string? value) => value switch
    {
        "BuyingHome" => "Buying a home",
        "AnnualReview" => "Annual review",
        "SellingHome" => "Selling a home",
        "InspectionFollowUp" => "Inspection follow-up",
        _ => "Home review"
    };

    public static string AreaEnfoque(string? value) => value switch
    {
        "Electrical" => "Electrical",
        "HVAC" => "HVAC",
        "GeneralStructure" => "General structure",
        "Plumbing" => "Plumbing",
        "Roof" => "Roof",
        "Moisture" => "Moisture",
        "Safety" => "Safety",
        _ => value ?? string.Empty
    };

    public static string FormatAreasEnfoque(string? areasPipeSeparated)
    {
        if (string.IsNullOrWhiteSpace(areasPipeSeparated))
        {
            return "General structure";
        }

        return string.Join(" / ",
            areasPipeSeparated
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(AreaEnfoque));
    }

    public static string TipoProblemaPlomeria(string? value) => value switch
    {
        "BathroomIssue" => "Bathroom issue",
        "KitchenIssue" => "Kitchen leak",
        "Toilet" => "Toilet issue",
        "FaucetSink" => "Faucet / sink",
        "ShowerTub" => "Shower / tub",
        "WaterSupplyLine" => "Water supply line",
        "DrainLineClog" => "Drain line / clog",
        "MainWaterLine" => "Main water line",
        "MainShutoffValve" => "Main shutoff valve",
        "LeakDetection" => "Leak detection",
        "LowWaterPressure" => "Low water pressure",
        "SewerMainDrain" => "Sewer / main drain",
        "ExteriorHoseBib" => "Exterior hose bib",
        "GeneralReview" => "General review",
        _ => "Plumbing issue"
    };

    public static string UbicacionProblemaPlomeria(string? value) => value switch
    {
        "Kitchen" => "Kitchen",
        "Bathroom" => "Bathroom",
        "Laundry" => "Laundry",
        "CrawlSpace" => "Crawl space",
        "Basement" => "Basement",
        "Exterior" => "Exterior",
        "WholeHouse" => "Whole house",
        _ => value ?? "Unknown area"
    };

    public static string SituacionPlomeria(string? value) => value switch
    {
        "LeakUnderSink" => "Leak under sink",
        "SlowDrain" => "Slow drain",
        "ClogBackup" => "Clog / backup",
        "PipeNoise" => "Pipe noise",
        "WaterStain" => "Water stain",
        "NoHotWater" => "No hot water",
        "ToiletRunning" => "Toilet running",
        "BadSmell" => "Bad smell",
        "FixtureLoose" => "Fixture loose",
        "LowPressure" => "Low pressure",
        _ => value ?? string.Empty
    };

    public static string FormatPlumbingSummary(string? tipo, string? ubicacion)
    {
        var detail = tipo switch
        {
            "KitchenIssue" => "drain issue",
            "DrainLineClog" => "drain issue",
            _ => UbicacionProblemaPlomeria(ubicacion).ToLowerInvariant() + " issue"
        };

        return $"{TipoProblemaPlomeria(tipo)} / {detail}";
    }

    public static string FormatPlumbingConcern(
        string? tipo,
        string? ubicacion,
        string? situacionesPipe)
    {
        var firstSituacion = GetFirstPipeValue(situacionesPipe);
        var detail = firstSituacion switch
        {
            "ClogBackup" => "drain issue",
            null or "" => tipo switch
            {
                "KitchenIssue" => "drain issue",
                _ => UbicacionProblemaPlomeria(ubicacion).ToLowerInvariant() + " issue"
            },
            _ => SituacionPlomeria(firstSituacion).ToLowerInvariant()
        };

        return $"{TipoProblemaPlomeria(tipo)} / {detail}";
    }

    public static string TipoProblemaHvac(string? value) => value switch
    {
        "NotCooling" => "Not cooling",
        "NotHeating" => "Not heating",
        "NoAirflow" => "No airflow",
        "WeakAirflow" => "Weak airflow",
        "StrangeNoise" => "Strange noise",
        "BadSmell" => "Bad smell",
        "WaterLeak" => "Water leak",
        "FrozenCoil" => "Frozen coil",
        "ThermostatIssue" => "Thermostat issue",
        "FilterIssue" => "Filter issue",
        "HighEnergyBill" => "High energy bill",
        "AnnualMaintenance" => "Annual maintenance",
        "GeneralReview" => "General review",
        _ => "HVAC issue"
    };

    public static string ParteAtencionHvac(string? value) => value switch
    {
        "OutdoorUnit" => "Outdoor unit",
        "IndoorUnit" => "Indoor unit",
        "FurnaceAirHandler" => "Furnace / air handler",
        "Thermostat" => "Thermostat",
        "AirFilter" => "Air filter",
        "DuctVents" => "Duct / vents",
        "WholeSystem" => "Whole system",
        _ => value ?? "Whole system"
    };

    public static string ComponenteHvac(string? value) => value switch
    {
        "OutdoorCondenser" => "Outdoor unit",
        "IndoorCoil" => "Indoor unit",
        "Furnace" => "Furnace",
        "Thermostat" => "Thermostat",
        "Filters" => "Filter",
        "Ductwork" => "Ductwork",
        "DrainLine" => "Drain line",
        _ => value ?? string.Empty
    };

    public static string FormatHvacConcern(string? tipo, string? parte)
    {
        var detail = tipo switch
        {
            "NotCooling" => "weak airflow",
            "WeakAirflow" => ParteAtencionHvac(parte).ToLowerInvariant(),
            "NoAirflow" => "no airflow",
            _ => ParteAtencionHvac(parte).ToLowerInvariant()
        };

        return $"{TipoProblemaHvac(tipo)} / {detail}";
    }

    public static string FormatHvacSummary(string? tipo, string? parte)
    {
        return FormatHvacConcern(tipo, parte);
    }

    public static string FormatHvacFocusAreas(string? componentesPipe)
    {
        if (string.IsNullOrWhiteSpace(componentesPipe))
        {
            return "Outdoor unit, indoor unit, thermostat, filter";
        }

        return string.Join(", ",
            componentesPipe
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ComponenteHvac)
                .Where(v => !string.IsNullOrWhiteSpace(v)));
    }

    public static string MotivoRevisionStructural(string? value) => value switch
    {
        "BeforePurchase" or "BuyingHome" => "Before purchase",
        "AfterDamage" => "After seeing damage",
        "Remodeling" or "RemodelPlanning" => "Remodeling",
        "AnnualReview" => "Annual review",
        "InsuranceClaim" => "Insurance / claim",
        "SecondOpinion" => "Second opinion",
        "SafetyCheck" => "Safety check",
        "VisibleIssue" => "Visible issue",
        "InspectionFollowUp" => "Inspection follow-up",
        _ => "Structural review"
    };

    public static string TipoPreocupacionStructural(string? value) => value switch
    {
        "FoundationCrack" => "Foundation crack",
        "WallCrack" => "Wall crack",
        "FloorSloping" or "FloorUneven" => "Floor sloping",
        "SettlementSigns" => "Settlement signs",
        "SaggingFloor" => "Sagging floor",
        "SaggingRoof" or "RoofSagging" => "Sagging roof",
        "DoorWindowSticking" => "Door / window sticking",
        "SupportColumn" => "Support column",
        "BeamConcern" => "Beam concern",
        "CrawlSpaceIssue" => "Crawl space issue",
        "ChimneyCrack" => "Chimney crack",
        "MoistureDamage" => "Moisture damage",
        "GeneralReview" => "General review",
        _ => "Structural concern"
    };

    public static string AreaPreocupacionStructural(string? value) => value switch
    {
        "Foundation" => "Foundation",
        "InteriorWall" => "Interior wall",
        "ExteriorWall" => "Exterior wall",
        "Walls" => "Walls",
        "Floor" or "Floors" => "Floor",
        "CeilingRoof" or "Roof" => "Ceiling / roof",
        "CrawlSpace" => "Crawl space",
        "GarageSlab" => "Garage slab",
        "Basement" => "Basement",
        "Exterior" => "Exterior",
        "WholeHouse" or "WholeStructure" => "Whole house",
        _ => value ?? "Structure"
    };

    public static string AreaEnfoqueStructural(string? value) => value switch
    {
        "Foundation" => "Foundation",
        "Walls" => "Walls",
        "Floors" or "Floor" => "Floors",
        "Roof" => "Roof",
        "CrawlSpace" => "Crawl space",
        "Basement" => "Basement",
        "Exterior" => "Exterior",
        _ => value ?? string.Empty
    };

    public static string SignoVisibleStructural(string? value) => value switch
    {
        "HairlineCrack" => "Hairline crack",
        "StairStepCrack" => "Stair-step crack",
        "HorizontalCrack" => "Horizontal crack",
        "VerticalCrack" => "Vertical crack",
        "WideCrack" => "Wide crack",
        "SeparationGap" => "Separation gap",
        "UnevenFloor" => "Uneven floor",
        "BouncyFloor" => "Bouncy floor",
        "WallBulging" => "Wall bulging",
        "CeilingCrack" => "Ceiling crack",
        "DoorSticking" => "Door sticking",
        "WindowNotClosing" => "Window not closing",
        "WaterIntrusion" => "Water intrusion",
        "CrawlSpaceMoisture" => "Crawl space moisture",
        "RotDecay" => "Rot or decay",
        "RustedSupport" => "Rusted support",
        "FoundationSettlement" => "Foundation settlement",
        "RoofSaggingSign" => "Roof sagging",
        _ => value ?? string.Empty
    };

    public static string SeveridadStructural(string? value) => value switch
    {
        "Mild" => "Mild",
        "Moderate" => "Moderate",
        "Severe" => "Severe",
        _ => value ?? "Moderate"
    };

    public static string DuracionProblemaStructural(string? value) => value switch
    {
        "LessThanWeek" => "Less than a week",
        "OneToThreeMonths" => "1–3 months",
        "ThreeToTwelveMonths" => "3–12 months",
        "MoreThanYear" => "More than a year",
        "NotSure" => "Not sure",
        _ => value ?? "Not sure"
    };

    public static string UrgenciaStructural(string? value) => value switch
    {
        "Normal" => "Within 30 days",
        "Priority" => "Within 2 weeks",
        "Emergency" => "ASAP",
        _ => value ?? "Within 30 days"
    };

    public static string TipoPropiedadStructural(string? value) => value switch
    {
        "SingleFamily" => "Single-family home",
        "Townhome" => "Townhome",
        "Duplex" => "Duplex",
        "Condo" => "Condo",
        _ => value ?? "Single-family home"
    };

    public static string TipoFundacionStructural(string? value) => value switch
    {
        "CrawlSpace" => "Crawl space",
        "Slab" => "Slab-on-grade",
        "Basement" => "Basement",
        "NotSure" => "Not sure",
        _ => value ?? "Not sure"
    };

    public static string AccesoPreferidoStructural(string? value) => value switch
    {
        "SomeoneHome" => "Someone home",
        "Lockbox" => "Lockbox",
        "RealtorAccess" => "Realtor access",
        "Vacant" => "Vacant",
        _ => value ?? "Someone home"
    };

    public static string MejorHorarioVisitaStructural(string? value) => value switch
    {
        "Morning" => "Morning",
        "Afternoon" => "Afternoon",
        "FirstAvailable" => "First available",
        _ => value ?? "First available"
    };

    public static string ReparacionesPreviasStructural(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => value ?? "No"
    };

    public static string EdadPropiedadStructural(string? value) => value switch
    {
        "LessThan10" => "Less than 10 years",
        "TenTo30" => "10–30 years",
        "ThirtyPlus" => "30+ years",
        "NotSure" => "Not sure",
        _ => value ?? "Not sure"
    };

    public static List<string> FormatStructuralConcernsList(string? tiposPipe, string? fallbackTipo)
    {
        var items = ParsePipeValues(tiposPipe);
        if (items.Count == 0 && !string.IsNullOrWhiteSpace(fallbackTipo))
        {
            items.Add(fallbackTipo);
        }

        return items.Select(TipoPreocupacionStructural).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
    }

    public static string FormatStructuralConcern(string? tipo, string? area, string? tiposPipe = null)
    {
        var concerns = FormatStructuralConcernsList(tiposPipe, tipo);
        if (concerns.Count == 0)
        {
            return FormatStructuralConcernLegacy(tipo, area);
        }

        var primary = concerns.First();
        var detail = primary.Contains("Foundation", StringComparison.OrdinalIgnoreCase)
            || primary.Contains("Settlement", StringComparison.OrdinalIgnoreCase)
            ? "settlement signs"
            : AreaPreocupacionStructural(area).ToLowerInvariant() + " concern";

        return $"{primary.ToLowerInvariant()} / {detail}";
    }

    private static string FormatStructuralConcernLegacy(string? tipo, string? area)
    {
        var detail = tipo switch
        {
            "FoundationCrack" => "settlement signs",
            "SettlementSigns" => "settlement signs",
            _ => AreaPreocupacionStructural(area).ToLowerInvariant() + " concern"
        };

        return $"{TipoPreocupacionStructural(tipo)} / {detail}";
    }

    public static string FormatStructuralSummary(string? tipo, string? area, string? tiposPipe = null)
    {
        return FormatStructuralConcern(tipo, area, tiposPipe);
    }

    public static string FormatStructuralFocusAreas(string? areasPipe)
    {
        if (string.IsNullOrWhiteSpace(areasPipe))
        {
            return "Foundation, walls, floors, crawl space";
        }

        return string.Join(", ",
            ParsePipeValues(areasPipe)
                .Select(v => AreaEnfoqueStructural(v) is { Length: > 0 } label ? label : AreaPreocupacionStructural(v))
                .Where(v => !string.IsNullOrWhiteSpace(v)));
    }

    public static string TipoProblemaTecho(string? value) => value switch
    {
        "ActiveLeak" => "Active leak",
        "MissingShingles" => "Missing shingles",
        "StormDamage" => "Storm damage",
        "CeilingStain" => "Ceiling stain",
        "GutterIssue" => "Gutter issue",
        "GeneralReview" => "General review",
        _ => value ?? "General review"
    };

    public static string UbicacionProblemaTecho(string? value) => value switch
    {
        "MainRoof" => "Main roof",
        "AroundChimney" => "Around chimney",
        "AroundVent" => "Around vent",
        "GutterEdge" => "Gutter / edge",
        "AtticCeiling" => "Attic / ceiling",
        "NotSure" => "Not sure",
        _ => value ?? "Main roof"
    };

    public static string MotivoRevisionTecho(string? value) => value switch
    {
        "LeakConcern" => "Leak concern",
        "AfterStorm" => "After storm",
        "AnnualReview" => "Annual review",
        "BeforePurchase" => "Before purchase",
        "InsuranceClaim" => "Insurance claim",
        _ => value ?? "Leak concern"
    };

    public static string NumeroPisosTecho(string? value) => value switch
    {
        "One" => "1 story",
        "Two" => "2 story",
        "ThreePlus" => "3+ story",
        _ => value ?? "2 story"
    };

    public static string MaterialTecho(string? value) => value switch
    {
        "AsphaltShingles" => "Asphalt shingles",
        "Metal" => "Metal",
        "Tile" => "Tile",
        "Flat" => "Flat",
        "NotSure" => "Not sure",
        _ => value ?? "Asphalt shingles"
    };

    public static List<string> FormatRoofProblemsList(string? tiposPipe, string? fallbackTipo, string? ubicacion)
    {
        var items = ParsePipeValues(tiposPipe)
            .Select(TipoProblemaTecho)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (items.Count == 0 && !string.IsNullOrWhiteSpace(fallbackTipo))
        {
            items.Add(TipoProblemaTecho(fallbackTipo));
        }

        var locationLabel = UbicacionProblemaTecho(ubicacion);
        if (!string.IsNullOrWhiteSpace(locationLabel)
            && !string.Equals(ubicacion, "NotSure", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(ubicacion, "MainRoof", StringComparison.OrdinalIgnoreCase)
            && !items.Contains(locationLabel, StringComparer.OrdinalIgnoreCase))
        {
            items.Add(locationLabel);
        }

        return items;
    }

    public static string FormatRoofConcern(string? tipo, string? ubicacion, string? tiposPipe = null)
    {
        var primary = TipoProblemaTecho(
            GetFirstPipeValue(tiposPipe) ?? tipo);

        var detail = ubicacion switch
        {
            "AroundChimney" => "flashing concern",
            "AroundVent" => "vent flashing concern",
            "GutterEdge" => "edge / gutter concern",
            "AtticCeiling" => "attic stain concern",
            "StormDamage" or _ when string.Equals(tipo, "StormDamage", StringComparison.OrdinalIgnoreCase) => "storm damage concern",
            _ => "roof concern"
        };

        return $"{primary.ToLowerInvariant()} / {detail}";
    }

    public static string FormatRoofPropertySummary(string? tipoPropiedad, string? numeroPisos, string? material)
    {
        return string.Join(" • ",
            new[]
            {
                TipoPropiedadStructural(tipoPropiedad),
                NumeroPisosTecho(numeroPisos),
                MaterialTecho(material)
            }.Where(v => !string.IsNullOrWhiteSpace(v)));
    }

    public static string FormatRoofFocusAreas(string? areasPipe, string? ubicacion, string? tiposPipe)
    {
        if (!string.IsNullOrWhiteSpace(areasPipe))
        {
            return string.Join(", ",
                ParsePipeValues(areasPipe)
                    .Select(v => UbicacionProblemaTecho(v) is { Length: > 0 } label && !string.Equals(v, "NotSure", StringComparison.OrdinalIgnoreCase)
                        ? label.ToLowerInvariant()
                        : TipoProblemaTecho(v).ToLowerInvariant())
                    .Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        var parts = new List<string>();
        var location = UbicacionProblemaTecho(ubicacion);
        if (!string.IsNullOrWhiteSpace(location) && !string.Equals(ubicacion, "NotSure", StringComparison.OrdinalIgnoreCase))
        {
            parts.Add(location.ToLowerInvariant());
        }

        foreach (var tipo in ParsePipeValues(tiposPipe))
        {
            var label = TipoProblemaTecho(tipo).ToLowerInvariant();
            if (label.Contains("gutter", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add("gutters");
            }
            else if (label.Contains("ceiling", StringComparison.OrdinalIgnoreCase)
                     || label.Contains("attic", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add("attic stain");
            }
            else if (label.Contains("leak", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add("valley");
            }
        }

        if (parts.Count == 0)
        {
            return "chimney, valley, attic stain, gutters";
        }

        return string.Join(", ", parts.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    public static string TipoProblemaMoldMoisture(string? value) => value switch
    {
        "MustySmell" => "Musty smell",
        "VisibleMold" => "Visible mold",
        "WaterStain" => "Water stain",
        "ActiveLeak" => "Active leak",
        "Condensation" => "Condensation",
        "CrawlSpaceHumidity" => "Crawl space humidity",
        "BathroomMoisture" => "Bathroom moisture",
        "AtticMoisture" => "Attic moisture",
        "HvacMoisture" => "HVAC moisture",
        "GeneralReview" => "General review",
        _ => value ?? "General review"
    };

    public static string UbicacionMoldMoisture(string? value) => value switch
    {
        "Bathroom" => "Bathroom",
        "Kitchen" => "Kitchen",
        "Ceiling" => "Ceiling",
        "Wall" => "Wall",
        "CrawlSpace" => "Crawl space",
        "Attic" => "Attic",
        "Basement" => "Basement",
        "HvacCloset" => "HVAC closet",
        "AroundWindow" => "Around window",
        "NotSure" => "Not sure",
        _ => value ?? "Bathroom"
    };

    public static string MotivoRevisionMoldMoisture(string? value) => value switch
    {
        "MustySmellConcern" => "Musty smell concern",
        "AfterWaterLeak" => "After water leak",
        "BeforePurchase" => "Before purchase",
        "HealthConcern" => "Health concern",
        "AnnualReview" => "Annual review",
        "SecondOpinion" => "Second opinion",
        _ => value ?? "Musty smell concern"
    };

    public static string TipoPropiedadMoldMoisture(string? value) => value switch
    {
        "SingleFamily" => "Single-family",
        "Townhome" => "Townhome",
        "Condo" => "Condo",
        "Apartment" => "Apartment",
        _ => value ?? "Single-family"
    };

    public static List<string> FormatMoldMoistureReviewItems(
        string? tiposPipe,
        string? fallbackTipo,
        string? ubicacion,
        string? urgencia)
    {
        var items = ParsePipeValues(tiposPipe)
            .Select(TipoProblemaMoldMoisture)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (items.Count == 0 && !string.IsNullOrWhiteSpace(fallbackTipo))
        {
            items.Add(TipoProblemaMoldMoisture(fallbackTipo));
        }

        var locationLabel = UbicacionMoldMoisture(ubicacion);
        if (!string.IsNullOrWhiteSpace(locationLabel)
            && !string.Equals(ubicacion, "NotSure", StringComparison.OrdinalIgnoreCase)
            && !items.Contains(locationLabel, StringComparer.OrdinalIgnoreCase))
        {
            items.Add(locationLabel);
        }

        var urgencyLabel = UrgenciaStructural(urgencia);
        if (!string.IsNullOrWhiteSpace(urgencyLabel)
            && !string.Equals(urgencia, "Normal", StringComparison.OrdinalIgnoreCase)
            && !items.Contains(urgencyLabel, StringComparer.OrdinalIgnoreCase))
        {
            items.Add(urgencyLabel);
        }

        return items;
    }

    public static string FormatMoldMoistureConcern(string? tipo, string? ubicacion, string? tiposPipe = null)
    {
        var primary = TipoProblemaMoldMoisture(
            GetFirstPipeValue(tiposPipe) ?? tipo);

        var detail = ubicacion switch
        {
            "Ceiling" => "ceiling moisture",
            "Bathroom" => "bathroom moisture",
            "Attic" => "attic moisture",
            "CrawlSpace" => "crawl space moisture",
            "HvacCloset" => "HVAC moisture",
            "Wall" => "wall moisture",
            _ => "moisture concern"
        };

        return $"{primary.ToLowerInvariant()} / {detail}";
    }

    public static string FormatMoldMoisturePropertySummary(string? tipoPropiedad)
    {
        return TipoPropiedadMoldMoisture(tipoPropiedad);
    }

    public static string FormatMoldMoistureFocusAreas(
        string? areasPipe,
        string? ubicacionPrincipal,
        string? ubicacionProblema)
    {
        if (!string.IsNullOrWhiteSpace(areasPipe))
        {
            return string.Join(", ",
                ParsePipeValues(areasPipe)
                    .Select(v => UbicacionMoldMoisture(v).ToLowerInvariant())
                    .Where(v => !string.IsNullOrWhiteSpace(v) && v != "not sure"));
        }

        var location = UbicacionMoldMoisture(ubicacionPrincipal ?? ubicacionProblema);
        if (string.IsNullOrWhiteSpace(location) || string.Equals(location, "Not sure", StringComparison.OrdinalIgnoreCase))
        {
            return "bathroom, ceiling, vent area";
        }

        var parts = new List<string> { location.ToLowerInvariant() };
        if (string.Equals(ubicacionPrincipal ?? ubicacionProblema, "Bathroom", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ubicacionPrincipal ?? ubicacionProblema, "Ceiling", StringComparison.OrdinalIgnoreCase))
        {
            parts.Add("ceiling");
            parts.Add("vent area");
        }

        return string.Join(", ", parts.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    public static string TipoProblemaWindowsInsulation(string? value) => value switch
    {
        "DraftAir" => "Draft / air coming in",
        "HighEnergyBill" => "High energy bill",
        "ColdRoom" => "Cold room",
        "HotRoom" => "Hot room",
        "WindowCondensation" => "Window condensation",
        "WindowSealing" => "Window sealing issue",
        "DamagedInsulation" => "Damaged insulation",
        "AtticInsulation" => "Attic insulation",
        "WallInsulation" => "Wall insulation",
        "MoistureAroundWindow" => "Moisture around window",
        "WholeHouseReview" => "Whole-house review",
        "NotSure" => "Not sure",
        _ => value ?? "Draft / air coming in"
    };

    public static string AreaAtencionWindowsInsulation(string? value) => value switch
    {
        "LivingRoom" => "Living room",
        "Bedroom" => "Bedroom",
        "Attic" => "Attic",
        "CrawlSpace" => "Crawl space",
        "AroundWindows" => "Around windows",
        "ExteriorWall" => "Exterior wall",
        "WholeHouse" => "Whole house",
        "NotSure" => "Not sure",
        _ => value ?? "Living room"
    };

    public static string MotivoRevisionWindowsInsulation(string? value) => value switch
    {
        "HighUtilityBill" => "High utility bill",
        "ComfortIssue" => "Comfort issue",
        "BeforePurchase" => "Before purchase",
        "AfterRemodel" => "After remodel",
        "AnnualReview" => "Annual review",
        "MoistureConcern" => "Moisture concern",
        "SecondOpinion" => "Second opinion",
        _ => value ?? "High utility bill"
    };

    public static string NumeroPisosWindowsInsulation(string? value) => value switch
    {
        "OneStory" => "1 story",
        "TwoStory" => "2 story",
        "ThreePlus" => "3+ story",
        _ => value ?? "2 story"
    };

    public static string AreaEnfoqueWindowsInsulation(string? value) => value switch
    {
        "Windows" => "Windows",
        "AtticInsulation" => "Attic insulation",
        "Doors" => "Doors",
        "CrawlSpaceInsulation" => "Crawl space insulation",
        "ExteriorWalls" => "Exterior walls",
        "WholeHouse" => "Whole house",
        _ => value ?? "Windows"
    };

    public static string TipoVentanaWindowsInsulation(string? value) => value switch
    {
        "DoublePane" => "Double-pane windows",
        "SinglePane" => "Single-pane windows",
        "NotSure" => "Window type not sure",
        _ => value ?? "Double-pane windows"
    };

    public static string DanoHumedadVisibleWindowsInsulation(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => value ?? "No"
    };

    public static string UrgenciaWindowsInsulation(string? value) => value switch
    {
        "Normal" => "Normal",
        "Priority" => "Priority",
        "NotSure" => "Not sure",
        _ => value ?? "Normal"
    };

    public static string FormatWindowsInsulationMainConcern(
        string? tiposPipe,
        string? fallbackTipo,
        string? danoHumedadVisible)
    {
        var issues = ParsePipeValues(tiposPipe)
            .Select(TipoProblemaWindowsInsulation)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (issues.Count == 0 && !string.IsNullOrWhiteSpace(fallbackTipo))
        {
            issues.Add(TipoProblemaWindowsInsulation(fallbackTipo));
        }

        var moisture = DanoHumedadVisibleWindowsInsulation(danoHumedadVisible);
        var baseText = issues.Count == 0 ? "Efficiency concern" : string.Join(", ", issues);
        return $"{baseText}, Visible moisture: {moisture}";
    }

    public static string FormatWindowsInsulationConcern(string? tiposPipe, string? fallbackTipo)
    {
        var first = TipoProblemaWindowsInsulation(GetFirstPipeValue(tiposPipe) ?? fallbackTipo);
        var second = ParsePipeValues(tiposPipe)
            .Skip(1)
            .Select(TipoProblemaWindowsInsulation)
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        if (string.IsNullOrWhiteSpace(second))
        {
            return first.ToLowerInvariant();
        }

        return $"{first.ToLowerInvariant()} / {second.ToLowerInvariant()}";
    }

    public static string FormatWindowsInsulationPropertySummary(
        string? tipoPropiedad,
        string? numeroPisos,
        string? tipoVentana)
    {
        var parts = new List<string>();
        var propertyType = TipoPropiedadMoldMoisture(tipoPropiedad);
        if (!string.IsNullOrWhiteSpace(propertyType))
        {
            parts.Add(propertyType);
        }

        var stories = NumeroPisosWindowsInsulation(numeroPisos);
        if (!string.IsNullOrWhiteSpace(stories))
        {
            parts.Add(stories);
        }

        var windowType = TipoVentanaWindowsInsulation(tipoVentana);
        if (!string.IsNullOrWhiteSpace(windowType))
        {
            parts.Add(windowType);
        }

        return string.Join(", ", parts);
    }

    public static string FormatWindowsInsulationFocusAreas(string? areasEnfoque, string? areasAtencion)
    {
        var labels = ParsePipeValues(areasEnfoque)
            .Select(AreaEnfoqueWindowsInsulation)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (labels.Count == 0)
        {
            labels = ParsePipeValues(areasAtencion)
                .Select(AreaAtencionWindowsInsulation)
                .Where(v => !string.IsNullOrWhiteSpace(v)
                            && !string.Equals(v, "Not sure", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return labels.Count == 0 ? "Windows, attic insulation" : string.Join(", ", labels);
    }

    public static string FormatWindowsInsulationFilesSummary(IEnumerable<(string? CategoriaArchivo, string? NombreArchivo)> archivos)
    {
        var list = archivos.ToList();
        if (list.Count == 0)
        {
            return "No files uploaded";
        }

        var photos = list.Count(a =>
            string.Equals(a.CategoriaArchivo, "photo", StringComparison.OrdinalIgnoreCase)
            || string.Equals(a.CategoriaArchivo, "video", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(a.CategoriaArchivo));
        var utilityBills = list.Count(a =>
            string.Equals(a.CategoriaArchivo, "utility", StringComparison.OrdinalIgnoreCase));
        var reports = list.Count(a =>
            string.Equals(a.CategoriaArchivo, "report", StringComparison.OrdinalIgnoreCase));

        var parts = new List<string>();
        if (photos > 0)
        {
            parts.Add($"{photos} photo{(photos == 1 ? "" : "s")}");
        }

        if (utilityBills > 0)
        {
            parts.Add($"{utilityBills} utility bill{(utilityBills == 1 ? "" : "s")}");
        }

        if (reports > 0)
        {
            parts.Add($"{reports} report{(reports == 1 ? "" : "s")}");
        }

        return parts.Count == 0 ? $"{list.Count} file{(list.Count == 1 ? "" : "s")} uploaded" : string.Join(" and ", parts);
    }

    public static string TipoProblemaHomeSafety(string? value) => value switch
    {
        "SmokeDetectorConcern" => "Smoke detector concern",
        "CoDetectorConcern" => "CO detector concern",
        "NoDetectors" => "No detectors",
        "ChirpingAlarm" => "Chirping alarm",
        "FireExtinguisherConcern" => "Fire extinguisher concern",
        "TripFallHazard" => "Trip / fall hazard",
        "StairRailingConcern" => "Stair / railing concern",
        "DoorLockSafety" => "Door / lock safety",
        "OutletBasicHazard" => "Outlet / basic hazard",
        "ChildSafetyConcern" => "Child safety concern",
        "GeneralSafetyReview" => "General safety review",
        "NotSure" => "Not sure",
        _ => value ?? "Smoke detector concern"
    };

    public static string AreaAtencionHomeSafety(string? value) => value switch
    {
        "Hallway" => "Hallway",
        "Bedroom" => "Bedroom",
        "Kitchen" => "Kitchen",
        "Garage" => "Garage",
        "Stairway" => "Stairway",
        "Basement" => "Basement",
        "Exterior" => "Exterior",
        "WholeHouse" => "Whole house",
        "Laundry" => "Laundry",
        "NotSure" => "Not sure",
        _ => value ?? "Hallway"
    };

    public static string MotivoRevisionHomeSafety(string? value) => value switch
    {
        "AnnualReview" => "Annual review",
        "ChildrenAtHome" => "Children at home",
        "BeforePurchase" => "Before purchase",
        "ElderlySafety" => "Elderly safety",
        "SecondOpinion" => "Second opinion",
        _ => value ?? "Annual review"
    };

    public static string AreaEnfoqueHomeSafety(string? value) => value switch
    {
        "SmokeCoDetectors" => "Smoke / CO detectors",
        "WholeHouse" => "Whole house",
        "StairsRailings" => "Stairs & railings",
        "DoorsExits" => "Doors & exits",
        "GarageSafety" => "Garage safety",
        "KitchenSafety" => "Kitchen safety",
        _ => value ?? "Smoke / CO detectors"
    };

    public static string OcupanteHomeSafety(string? value) => value switch
    {
        "Children" => "Children",
        "Adults" => "Adults",
        "Seniors" => "Seniors",
        "Pets" => "Pets",
        _ => value ?? "Children"
    };

    public static string RiesgoActivoHomeSafety(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => value ?? "No"
    };

    public static string FormatHomeSafetyMainConcern(
        string? tiposPipe,
        string? fallbackTipo,
        string? riesgoActivo)
    {
        var issues = ParsePipeValues(tiposPipe)
            .Select(TipoProblemaHomeSafety)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (issues.Count == 0 && !string.IsNullOrWhiteSpace(fallbackTipo))
        {
            issues.Add(TipoProblemaHomeSafety(fallbackTipo));
        }

        var risk = RiesgoActivoHomeSafety(riesgoActivo);
        var baseText = issues.Count == 0 ? "Safety concern" : string.Join(", ", issues);
        return $"{baseText}, Active risk: {risk}";
    }

    public static string FormatHomeSafetyConcern(string? tiposPipe, string? fallbackTipo)
    {
        var first = TipoProblemaHomeSafety(GetFirstPipeValue(tiposPipe) ?? fallbackTipo);
        var second = ParsePipeValues(tiposPipe)
            .Skip(1)
            .Select(TipoProblemaHomeSafety)
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        if (string.IsNullOrWhiteSpace(second))
        {
            return first.ToLowerInvariant();
        }

        return $"{first.ToLowerInvariant()} / {second.ToLowerInvariant()}";
    }

    public static string FormatHomeSafetyPropertySummary(string? tipoPropiedad, string? numeroPisos)
    {
        var parts = new List<string>();
        var propertyType = TipoPropiedadMoldMoisture(tipoPropiedad);
        if (!string.IsNullOrWhiteSpace(propertyType))
        {
            parts.Add(propertyType);
        }

        var stories = NumeroPisosWindowsInsulation(numeroPisos);
        if (!string.IsNullOrWhiteSpace(stories))
        {
            parts.Add(stories);
        }

        return string.Join(", ", parts);
    }

    public static string FormatHomeSafetyFocusAreas(string? areasEnfoque, string? areasAtencion)
    {
        var labels = ParsePipeValues(areasEnfoque)
            .Select(AreaEnfoqueHomeSafety)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        if (labels.Count == 0)
        {
            labels = ParsePipeValues(areasAtencion)
                .Select(AreaAtencionHomeSafety)
                .Where(v => !string.IsNullOrWhiteSpace(v)
                            && !string.Equals(v, "Not sure", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return labels.Count == 0 ? "Hallway, whole house" : string.Join(", ", labels);
    }

    public static string FormatHomeSafetyFilesSummary(IEnumerable<(string? CategoriaArchivo, string? NombreArchivo)> archivos)
    {
        var list = archivos.ToList();
        if (list.Count == 0)
        {
            return "No files uploaded";
        }

        var photos = list.Count(a =>
            string.Equals(a.CategoriaArchivo, "photo", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(a.CategoriaArchivo));
        var videos = list.Count(a =>
            string.Equals(a.CategoriaArchivo, "video", StringComparison.OrdinalIgnoreCase));
        var reports = list.Count(a =>
            string.Equals(a.CategoriaArchivo, "report", StringComparison.OrdinalIgnoreCase));

        var parts = new List<string>();
        if (photos > 0)
        {
            parts.Add($"{photos} photo{(photos == 1 ? "" : "s")}");
        }

        if (videos > 0)
        {
            parts.Add($"{videos} short video{(videos == 1 ? "" : "s")}");
        }

        if (reports > 0)
        {
            parts.Add($"{reports} report{(reports == 1 ? "" : "s")}");
        }

        return parts.Count == 0 ? $"{list.Count} file{(list.Count == 1 ? "" : "s")} uploaded" : string.Join(" and ", parts);
    }

    public static string TipoInversionInvestor(string? value) => value switch
    {
        "Flip" => "Flip",
        "RentalProperty" => "Rental property",
        "BuyAndHold" => "Buy and hold",
        "BRRRR" => "BRRRR",
        "BeforeOffer" => "Before offer",
        "BeforeClosing" => "Before closing",
        _ => value ?? "Flip"
    };

    public static string EnfoqueInversionInvestor(string? value) => value switch
    {
        "GeneralAssessment" => "General assessment",
        "RepairRisk" => "Repair risk",
        "RehabBudget" => "Rehab evaluation",
        "RentReadiness" => "Rent readiness",
        "ResalePotential" => "Resale potential",
        "NegotiationSupport" => "Negotiation support",
        _ => value ?? "Rehab evaluation"
    };

    public static string TipoPropiedadInvestor(string? value) => value switch
    {
        "SingleFamily" => "Single-family",
        "Duplex" => "Duplex",
        "Triplex" => "Triplex",
        "Condo" => "Condo",
        "SmallMultifamily" => "Small multifamily",
        _ => value ?? "Single-family"
    };

    public static string OcupacionInvestor(string? value) => value switch
    {
        "TenantOccupied" => "Tenant occupied",
        "Vacant" => "Vacant",
        "OwnerOccupied" => "Owner occupied",
        "NotSure" => "Not sure",
        _ => value ?? "Tenant occupied"
    };

    public static string NivelRehabInvestor(string? value) => value switch
    {
        "Light" => "Light",
        "Medium" => "Medium",
        "Heavy" => "Heavy",
        "NotSure" => "Not sure",
        _ => value ?? "Light"
    };

    public static string AreaRevisionInvestor(string? value) => value switch
    {
        "Roof" => "Roof",
        "Hvac" => "HVAC",
        "Plumbing" => "Plumbing",
        "Electrical" => "Electrical",
        "Foundation" => "Foundation",
        "Moisture" => "Moisture",
        "Safety" => "Safety",
        "Cosmetic" => "Cosmetic",
        _ => value ?? string.Empty
    };

    public static string FormatInvestorGoal(string? tipoInversion, string? enfoquesInversion)
    {
        var investment = TipoInversionInvestor(tipoInversion);
        var firstEnfoque = GetFirstPipeValue(enfoquesInversion);
        var focus = EnfoqueInversionInvestor(firstEnfoque);
        return $"{investment} / {focus.ToLowerInvariant()}";
    }

    public static string FormatInvestorFocusAreas(string? areasRevision)
    {
        var labels = ParsePipeValues(areasRevision)
            .Select(AreaRevisionInvestor)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        return labels.Count == 0 ? "Roof, HVAC, Plumbing, Electrical" : string.Join(", ", labels);
    }

    public static string FormatInvestorPropertySummary(string? tipoPropiedad, string? ocupacion)
    {
        var property = TipoPropiedadInvestor(tipoPropiedad);
        if (string.Equals(tipoPropiedad, "SingleFamily", StringComparison.OrdinalIgnoreCase))
        {
            return "Single-family home";
        }

        var occupancy = OcupacionInvestor(ocupacion);
        if (string.IsNullOrWhiteSpace(ocupacion) || string.Equals(ocupacion, "NotSure", StringComparison.OrdinalIgnoreCase))
        {
            return property;
        }

        return $"{property} · {occupancy.ToLowerInvariant()}";
    }

    public static string FormatInvestorFilesSummary(IEnumerable<(string? CategoriaArchivo, string? NombreArchivo)> archivos)
    {
        var list = archivos.ToList();
        if (list.Count == 0)
        {
            return "No files uploaded";
        }

        var photos = list.Count(a =>
            string.Equals(a.CategoriaArchivo, "photo", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(a.CategoriaArchivo));
        var videos = list.Count(a =>
            string.Equals(a.CategoriaArchivo, "video", StringComparison.OrdinalIgnoreCase));
        var reports = list.Count(a =>
            string.Equals(a.CategoriaArchivo, "report", StringComparison.OrdinalIgnoreCase));

        var parts = new List<string>();
        if (photos > 0)
        {
            parts.Add($"{photos} photo{(photos == 1 ? "" : "s")}");
        }

        if (videos > 0)
        {
            parts.Add($"{videos} video{(videos == 1 ? "" : "s")}");
        }

        if (reports > 0)
        {
            parts.Add($"{reports} report{(reports == 1 ? "" : "s")}");
        }

        return parts.Count == 0 ? $"{list.Count} file{(list.Count == 1 ? "" : "s")} uploaded" : string.Join(", ", parts);
    }

    public static string FormatPipeLabels(string? pipe, Func<string?, string> labelFn, string fallback = "")
    {
        var labels = ParsePipeValues(pipe).Select(labelFn).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
        return labels.Count == 0 ? fallback : string.Join(", ", labels);
    }

    private static List<string> ParsePipeValues(string? pipeSeparated)
    {
        if (string.IsNullOrWhiteSpace(pipeSeparated))
        {
            return new List<string>();
        }

        return pipeSeparated
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static string? GetFirstPipeValue(string? pipeSeparated)
    {
        if (string.IsNullOrWhiteSpace(pipeSeparated))
        {
            return null;
        }

        return pipeSeparated
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
    }
}
