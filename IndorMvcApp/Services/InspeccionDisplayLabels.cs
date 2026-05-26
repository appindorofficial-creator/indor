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
