namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class InspeccionDisplayLabels
{
    public static string MotivoRevision(string? value) => value switch
    {
        "BuyingHome" => DisplayLabelsLocalization.L("Buying a home"),
        "SafetyCheck" => DisplayLabelsLocalization.L("Safety check"),
        "IssueAtHome" => DisplayLabelsLocalization.L("Issue at home"),
        "InspectionFollowUp" => DisplayLabelsLocalization.L("Inspection follow-up"),
        _ => DisplayLabelsLocalization.L("General review")
    };

    public static string PreocupacionPrincipal(string? value) => value switch
    {
        "BreakerTrips" => DisplayLabelsLocalization.L("Breaker trips"),
        "LightsFlicker" => DisplayLabelsLocalization.L("Lights flicker"),
        "OutletsNotWorking" => DisplayLabelsLocalization.L("Outlets not working"),
        "OldPanel" => DisplayLabelsLocalization.L("Old panel"),
        "BurningSmell" => DisplayLabelsLocalization.L("Burning smell"),
        "GeneralReview" => DisplayLabelsLocalization.L("General electrical review"),
        _ => DisplayLabelsLocalization.L("General electrical review")
    };

    public static string ObjetivoPrincipal(string? value) => value switch
    {
        "BuyWithConfidence" => DisplayLabelsLocalization.L("Buy with confidence"),
        "UnderstandRepairRisks" => DisplayLabelsLocalization.L("Understand repair risks"),
        "NegotiateRepairs" => DisplayLabelsLocalization.L("Negotiate repairs"),
        "SecondOpinion" => DisplayLabelsLocalization.L("Second opinion"),
        _ => DisplayLabelsLocalization.L("Home purchase review")
    };

    public static string RolComprador(string? value) => value switch
    {
        "Buyer" => DisplayLabelsLocalization.L("Buyer"),
        "Realtor" => DisplayLabelsLocalization.L("Realtor"),
        "Investor" => DisplayLabelsLocalization.L("Investor"),
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
        "BuyingHome" => DisplayLabelsLocalization.L("Buying a home"),
        "AnnualReview" => DisplayLabelsLocalization.L("Annual review"),
        "SellingHome" => DisplayLabelsLocalization.L("Selling a home"),
        "InspectionFollowUp" => DisplayLabelsLocalization.L("Inspection follow-up"),
        _ => DisplayLabelsLocalization.L("Home review")
    };

    public static string AreaEnfoque(string? value) => value switch
    {
        "Electrical" => DisplayLabelsLocalization.L("Electrical"),
        "HVAC" => DisplayLabelsLocalization.L("HVAC"),
        "GeneralStructure" => DisplayLabelsLocalization.L("General structure"),
        "Plumbing" => DisplayLabelsLocalization.L("Plumbing"),
        "Roof" => DisplayLabelsLocalization.L("Roof"),
        "Moisture" => DisplayLabelsLocalization.L("Moisture"),
        "Safety" => DisplayLabelsLocalization.L("Safety"),
        _ => value ?? string.Empty
    };

    public static string FormatAreasEnfoque(string? areasPipeSeparated)
    {
        if (string.IsNullOrWhiteSpace(areasPipeSeparated))
        {
            return DisplayLabelsLocalization.L("General structure");
        }

        return string.Join(" / ",
            areasPipeSeparated
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(AreaEnfoque));
    }

    public static string TipoProblemaPlomeria(string? value) => value switch
    {
        "BathroomIssue" => DisplayLabelsLocalization.L("Bathroom issue"),
        "KitchenIssue" => DisplayLabelsLocalization.L("Kitchen leak"),
        "Toilet" => DisplayLabelsLocalization.L("Toilet issue"),
        "FaucetSink" => DisplayLabelsLocalization.L("Faucet / sink"),
        "ShowerTub" => DisplayLabelsLocalization.L("Shower / tub"),
        "WaterSupplyLine" => DisplayLabelsLocalization.L("Water supply line"),
        "DrainLineClog" => DisplayLabelsLocalization.L("Drain line / clog"),
        "MainWaterLine" => DisplayLabelsLocalization.L("Main water line"),
        "MainShutoffValve" => DisplayLabelsLocalization.L("Main shutoff valve"),
        "LeakDetection" => DisplayLabelsLocalization.L("Leak detection"),
        "LowWaterPressure" => DisplayLabelsLocalization.L("Low water pressure"),
        "SewerMainDrain" => DisplayLabelsLocalization.L("Sewer / main drain"),
        "ExteriorHoseBib" => DisplayLabelsLocalization.L("Exterior hose bib"),
        "GeneralReview" => DisplayLabelsLocalization.L("General review"),
        _ => DisplayLabelsLocalization.L("Plumbing issue")
    };

    public static string UbicacionProblemaPlomeria(string? value) => value switch
    {
        "Kitchen" => DisplayLabelsLocalization.L("Kitchen"),
        "Bathroom" => DisplayLabelsLocalization.L("Bathroom"),
        "Laundry" => DisplayLabelsLocalization.L("Laundry"),
        "CrawlSpace" => DisplayLabelsLocalization.L("Crawl space"),
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        "Exterior" => DisplayLabelsLocalization.L("Exterior"),
        "WholeHouse" => DisplayLabelsLocalization.L("Whole house"),
        _ => value ?? "Unknown area"
    };

    public static string SituacionPlomeria(string? value) => value switch
    {
        "LeakUnderSink" => DisplayLabelsLocalization.L("Leak under sink"),
        "SlowDrain" => DisplayLabelsLocalization.L("Slow drain"),
        "ClogBackup" => DisplayLabelsLocalization.L("Clog / backup"),
        "PipeNoise" => DisplayLabelsLocalization.L("Pipe noise"),
        "WaterStain" => DisplayLabelsLocalization.L("Water stain"),
        "NoHotWater" => DisplayLabelsLocalization.L("No hot water"),
        "ToiletRunning" => DisplayLabelsLocalization.L("Toilet running"),
        "BadSmell" => DisplayLabelsLocalization.L("Bad smell"),
        "FixtureLoose" => DisplayLabelsLocalization.L("Fixture loose"),
        "LowPressure" => DisplayLabelsLocalization.L("Low pressure"),
        _ => value ?? string.Empty
    };

    public static string FormatPlumbingSummary(string? tipo, string? ubicacion)
    {
        var detail = tipo switch
        {
            "KitchenIssue" => DisplayLabelsLocalization.L("drain issue"),
            "DrainLineClog" => DisplayLabelsLocalization.L("drain issue"),
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
            "ClogBackup" => DisplayLabelsLocalization.L("drain issue"),
            null or "" => tipo switch
            {
                "KitchenIssue" => DisplayLabelsLocalization.L("drain issue"),
                _ => UbicacionProblemaPlomeria(ubicacion).ToLowerInvariant() + " issue"
            },
            _ => SituacionPlomeria(firstSituacion).ToLowerInvariant()
        };

        return $"{TipoProblemaPlomeria(tipo)} / {detail}";
    }

    public static string TipoProblemaHvac(string? value) => value switch
    {
        "NotCooling" => DisplayLabelsLocalization.L("Not cooling"),
        "NotHeating" => DisplayLabelsLocalization.L("Not heating"),
        "NoAirflow" => DisplayLabelsLocalization.L("No airflow"),
        "WeakAirflow" => DisplayLabelsLocalization.L("Weak airflow"),
        "StrangeNoise" => DisplayLabelsLocalization.L("Strange noise"),
        "BadSmell" => DisplayLabelsLocalization.L("Bad smell"),
        "WaterLeak" => DisplayLabelsLocalization.L("Water leak"),
        "FrozenCoil" => DisplayLabelsLocalization.L("Frozen coil"),
        "ThermostatIssue" => DisplayLabelsLocalization.L("Thermostat issue"),
        "FilterIssue" => DisplayLabelsLocalization.L("Filter issue"),
        "HighEnergyBill" => DisplayLabelsLocalization.L("High energy bill"),
        "AnnualMaintenance" => DisplayLabelsLocalization.L("Annual maintenance"),
        "GeneralReview" => DisplayLabelsLocalization.L("General review"),
        _ => DisplayLabelsLocalization.L("HVAC issue")
    };

    public static string ParteAtencionHvac(string? value) => value switch
    {
        "OutdoorUnit" => DisplayLabelsLocalization.L("Outdoor unit"),
        "IndoorUnit" => DisplayLabelsLocalization.L("Indoor unit"),
        "FurnaceAirHandler" => DisplayLabelsLocalization.L("Furnace / air handler"),
        "Thermostat" => DisplayLabelsLocalization.L("Thermostat"),
        "AirFilter" => DisplayLabelsLocalization.L("Air filter"),
        "DuctVents" => DisplayLabelsLocalization.L("Duct / vents"),
        "WholeSystem" => DisplayLabelsLocalization.L("Whole system"),
        _ => value ?? "Whole system"
    };

    public static string ComponenteHvac(string? value) => value switch
    {
        "OutdoorCondenser" => DisplayLabelsLocalization.L("Outdoor unit"),
        "IndoorCoil" => DisplayLabelsLocalization.L("Indoor unit"),
        "Furnace" => DisplayLabelsLocalization.L("Furnace"),
        "Thermostat" => DisplayLabelsLocalization.L("Thermostat"),
        "Filters" => DisplayLabelsLocalization.L("Filter"),
        "Ductwork" => DisplayLabelsLocalization.L("Ductwork"),
        "DrainLine" => DisplayLabelsLocalization.L("Drain line"),
        _ => value ?? string.Empty
    };

    public static string FormatHvacConcern(string? tipo, string? parte)
    {
        var detail = tipo switch
        {
            "NotCooling" => DisplayLabelsLocalization.L("weak airflow"),
            "WeakAirflow" => ParteAtencionHvac(parte).ToLowerInvariant(),
            "NoAirflow" => DisplayLabelsLocalization.L("no airflow"),
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
            return DisplayLabelsLocalization.L("Outdoor unit, indoor unit, thermostat, filter");
        }

        return string.Join(", ",
            componentesPipe
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ComponenteHvac)
                .Where(v => !string.IsNullOrWhiteSpace(v)));
    }

    public static string MotivoRevisionStructural(string? value) => value switch
    {
        "BeforePurchase" or "BuyingHome" => DisplayLabelsLocalization.L("Before purchase"),
        "AfterDamage" => DisplayLabelsLocalization.L("After seeing damage"),
        "Remodeling" or "RemodelPlanning" => DisplayLabelsLocalization.L("Remodeling"),
        "AnnualReview" => DisplayLabelsLocalization.L("Annual review"),
        "InsuranceClaim" => DisplayLabelsLocalization.L("Insurance / claim"),
        "SecondOpinion" => DisplayLabelsLocalization.L("Second opinion"),
        "SafetyCheck" => DisplayLabelsLocalization.L("Safety check"),
        "VisibleIssue" => DisplayLabelsLocalization.L("Visible issue"),
        "InspectionFollowUp" => DisplayLabelsLocalization.L("Inspection follow-up"),
        _ => DisplayLabelsLocalization.L("Structural review")
    };

    public static string TipoPreocupacionStructural(string? value) => value switch
    {
        "FoundationCrack" => DisplayLabelsLocalization.L("Foundation crack"),
        "WallCrack" => DisplayLabelsLocalization.L("Wall crack"),
        "FloorSloping" or "FloorUneven" => DisplayLabelsLocalization.L("Floor sloping"),
        "SettlementSigns" => DisplayLabelsLocalization.L("Settlement signs"),
        "SaggingFloor" => DisplayLabelsLocalization.L("Sagging floor"),
        "SaggingRoof" or "RoofSagging" => DisplayLabelsLocalization.L("Sagging roof"),
        "DoorWindowSticking" => DisplayLabelsLocalization.L("Door / window sticking"),
        "SupportColumn" => DisplayLabelsLocalization.L("Support column"),
        "BeamConcern" => DisplayLabelsLocalization.L("Beam concern"),
        "CrawlSpaceIssue" => DisplayLabelsLocalization.L("Crawl space issue"),
        "ChimneyCrack" => DisplayLabelsLocalization.L("Chimney crack"),
        "MoistureDamage" => DisplayLabelsLocalization.L("Moisture damage"),
        "GeneralReview" => DisplayLabelsLocalization.L("General review"),
        _ => DisplayLabelsLocalization.L("Structural concern")
    };

    public static string AreaPreocupacionStructural(string? value) => value switch
    {
        "Foundation" => DisplayLabelsLocalization.L("Foundation"),
        "InteriorWall" => DisplayLabelsLocalization.L("Interior wall"),
        "ExteriorWall" => DisplayLabelsLocalization.L("Exterior wall"),
        "Walls" => DisplayLabelsLocalization.L("Walls"),
        "Floor" or "Floors" => DisplayLabelsLocalization.L("Floor"),
        "CeilingRoof" or "Roof" => DisplayLabelsLocalization.L("Ceiling / roof"),
        "CrawlSpace" => DisplayLabelsLocalization.L("Crawl space"),
        "GarageSlab" => DisplayLabelsLocalization.L("Garage slab"),
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        "Exterior" => DisplayLabelsLocalization.L("Exterior"),
        "WholeHouse" or "WholeStructure" => DisplayLabelsLocalization.L("Whole house"),
        _ => value ?? "Structure"
    };

    public static string AreaEnfoqueStructural(string? value) => value switch
    {
        "Foundation" => DisplayLabelsLocalization.L("Foundation"),
        "Walls" => DisplayLabelsLocalization.L("Walls"),
        "Floors" or "Floor" => DisplayLabelsLocalization.L("Floors"),
        "Roof" => DisplayLabelsLocalization.L("Roof"),
        "CrawlSpace" => DisplayLabelsLocalization.L("Crawl space"),
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        "Exterior" => DisplayLabelsLocalization.L("Exterior"),
        _ => value ?? string.Empty
    };

    public static string SignoVisibleStructural(string? value) => value switch
    {
        "HairlineCrack" => DisplayLabelsLocalization.L("Hairline crack"),
        "StairStepCrack" => DisplayLabelsLocalization.L("Stair-step crack"),
        "HorizontalCrack" => DisplayLabelsLocalization.L("Horizontal crack"),
        "VerticalCrack" => DisplayLabelsLocalization.L("Vertical crack"),
        "WideCrack" => DisplayLabelsLocalization.L("Wide crack"),
        "SeparationGap" => DisplayLabelsLocalization.L("Separation gap"),
        "UnevenFloor" => DisplayLabelsLocalization.L("Uneven floor"),
        "BouncyFloor" => DisplayLabelsLocalization.L("Bouncy floor"),
        "WallBulging" => DisplayLabelsLocalization.L("Wall bulging"),
        "CeilingCrack" => DisplayLabelsLocalization.L("Ceiling crack"),
        "DoorSticking" => DisplayLabelsLocalization.L("Door sticking"),
        "WindowNotClosing" => DisplayLabelsLocalization.L("Window not closing"),
        "WaterIntrusion" => DisplayLabelsLocalization.L("Water intrusion"),
        "CrawlSpaceMoisture" => DisplayLabelsLocalization.L("Crawl space moisture"),
        "RotDecay" => DisplayLabelsLocalization.L("Rot or decay"),
        "RustedSupport" => DisplayLabelsLocalization.L("Rusted support"),
        "FoundationSettlement" => DisplayLabelsLocalization.L("Foundation settlement"),
        "RoofSaggingSign" => DisplayLabelsLocalization.L("Roof sagging"),
        _ => value ?? string.Empty
    };

    public static string SeveridadStructural(string? value) => value switch
    {
        "Mild" => DisplayLabelsLocalization.L("Mild"),
        "Moderate" => DisplayLabelsLocalization.L("Moderate"),
        "Severe" => DisplayLabelsLocalization.L("Severe"),
        _ => value ?? "Moderate"
    };

    public static string DuracionProblemaStructural(string? value) => value switch
    {
        "LessThanWeek" => DisplayLabelsLocalization.L("Less than a week"),
        "OneToThreeMonths" => DisplayLabelsLocalization.L("1–3 months"),
        "ThreeToTwelveMonths" => DisplayLabelsLocalization.L("3–12 months"),
        "MoreThanYear" => DisplayLabelsLocalization.L("More than a year"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Not sure"
    };

    public static string UrgenciaStructural(string? value) => value switch
    {
        "Normal" => DisplayLabelsLocalization.L("Within 30 days"),
        "Priority" => DisplayLabelsLocalization.L("Within 2 weeks"),
        "Emergency" => DisplayLabelsLocalization.L("ASAP"),
        _ => value ?? "Within 30 days"
    };

    public static string TipoPropiedadStructural(string? value) => value switch
    {
        "SingleFamily" => DisplayLabelsLocalization.L("Single-family home"),
        "Townhome" => DisplayLabelsLocalization.L("Townhome"),
        "Duplex" => DisplayLabelsLocalization.L("Duplex"),
        "Condo" => DisplayLabelsLocalization.L("Condo"),
        _ => value ?? "Single-family home"
    };

    public static string TipoFundacionStructural(string? value) => value switch
    {
        "CrawlSpace" => DisplayLabelsLocalization.L("Crawl space"),
        "Slab" => DisplayLabelsLocalization.L("Slab-on-grade"),
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Not sure"
    };

    public static string AccesoPreferidoStructural(string? value) => value switch
    {
        "SomeoneHome" => DisplayLabelsLocalization.L("Someone home"),
        "Lockbox" => DisplayLabelsLocalization.L("Lockbox"),
        "RealtorAccess" => DisplayLabelsLocalization.L("Realtor access"),
        "Vacant" => DisplayLabelsLocalization.L("Vacant"),
        _ => value ?? "Someone home"
    };

    public static string MejorHorarioVisitaStructural(string? value) => value switch
    {
        "Morning" => DisplayLabelsLocalization.L("Morning"),
        "Afternoon" => DisplayLabelsLocalization.L("Afternoon"),
        "FirstAvailable" => DisplayLabelsLocalization.L("First available"),
        _ => value ?? "First available"
    };

    public static string ReparacionesPreviasStructural(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "No"
    };

    public static string EdadPropiedadStructural(string? value) => value switch
    {
        "LessThan10" => DisplayLabelsLocalization.L("Less than 10 years"),
        "TenTo30" => DisplayLabelsLocalization.L("10–30 years"),
        "ThirtyPlus" => DisplayLabelsLocalization.L("30+ years"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
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
            "FoundationCrack" => DisplayLabelsLocalization.L("settlement signs"),
            "SettlementSigns" => DisplayLabelsLocalization.L("settlement signs"),
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
            return DisplayLabelsLocalization.L("Foundation, walls, floors, crawl space");
        }

        return string.Join(", ",
            ParsePipeValues(areasPipe)
                .Select(v => AreaEnfoqueStructural(v) is { Length: > 0 } label ? label : AreaPreocupacionStructural(v))
                .Where(v => !string.IsNullOrWhiteSpace(v)));
    }

    public static string TipoProblemaTecho(string? value) => value switch
    {
        "ActiveLeak" => DisplayLabelsLocalization.L("Active leak"),
        "MissingShingles" => DisplayLabelsLocalization.L("Missing shingles"),
        "StormDamage" => DisplayLabelsLocalization.L("Storm damage"),
        "CeilingStain" => DisplayLabelsLocalization.L("Ceiling stain"),
        "GutterIssue" => DisplayLabelsLocalization.L("Gutter issue"),
        "GeneralReview" => DisplayLabelsLocalization.L("General review"),
        _ => value ?? "General review"
    };

    public static string UbicacionProblemaTecho(string? value) => value switch
    {
        "MainRoof" => DisplayLabelsLocalization.L("Main roof"),
        "AroundChimney" => DisplayLabelsLocalization.L("Around chimney"),
        "AroundVent" => DisplayLabelsLocalization.L("Around vent"),
        "GutterEdge" => DisplayLabelsLocalization.L("Gutter / edge"),
        "AtticCeiling" => DisplayLabelsLocalization.L("Attic / ceiling"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Main roof"
    };

    public static string MotivoRevisionTecho(string? value) => value switch
    {
        "LeakConcern" => DisplayLabelsLocalization.L("Leak concern"),
        "AfterStorm" => DisplayLabelsLocalization.L("After storm"),
        "AnnualReview" => DisplayLabelsLocalization.L("Annual review"),
        "BeforePurchase" => DisplayLabelsLocalization.L("Before purchase"),
        "InsuranceClaim" => DisplayLabelsLocalization.L("Insurance claim"),
        _ => value ?? "Leak concern"
    };

    public static string NumeroPisosTecho(string? value) => value switch
    {
        "One" => DisplayLabelsLocalization.L("1 story"),
        "Two" => DisplayLabelsLocalization.L("2 story"),
        "ThreePlus" => DisplayLabelsLocalization.L("3+ story"),
        _ => value ?? "2 story"
    };

    public static string MaterialTecho(string? value) => value switch
    {
        "AsphaltShingles" => DisplayLabelsLocalization.L("Asphalt shingles"),
        "Metal" => DisplayLabelsLocalization.L("Metal"),
        "Tile" => DisplayLabelsLocalization.L("Tile"),
        "Flat" => DisplayLabelsLocalization.L("Flat"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
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
            "AroundChimney" => DisplayLabelsLocalization.L("flashing concern"),
            "AroundVent" => DisplayLabelsLocalization.L("vent flashing concern"),
            "GutterEdge" => DisplayLabelsLocalization.L("edge / gutter concern"),
            "AtticCeiling" => DisplayLabelsLocalization.L("attic stain concern"),
            "StormDamage" or _ when string.Equals(tipo, "StormDamage", StringComparison.OrdinalIgnoreCase) => "storm damage concern",
            _ => DisplayLabelsLocalization.L("roof concern")
        };

        return $"{primary.ToLowerInvariant()} / {detail}";
    }

    public static string FormatRoofPropertySummary(string? tipoPropiedad, string? numeroPisos, string? material)
    {
        return string.Join(" â€¢ ",
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
            return DisplayLabelsLocalization.L("chimney, valley, attic stain, gutters");
        }

        return string.Join(", ", parts.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    public static string TipoProblemaMoldMoisture(string? value) => value switch
    {
        "MustySmell" => DisplayLabelsLocalization.L("Musty smell"),
        "VisibleMold" => DisplayLabelsLocalization.L("Visible mold"),
        "WaterStain" => DisplayLabelsLocalization.L("Water stain"),
        "ActiveLeak" => DisplayLabelsLocalization.L("Active leak"),
        "Condensation" => DisplayLabelsLocalization.L("Condensation"),
        "CrawlSpaceHumidity" => DisplayLabelsLocalization.L("Crawl space humidity"),
        "BathroomMoisture" => DisplayLabelsLocalization.L("Bathroom moisture"),
        "AtticMoisture" => DisplayLabelsLocalization.L("Attic moisture"),
        "HvacMoisture" => DisplayLabelsLocalization.L("HVAC moisture"),
        "GeneralReview" => DisplayLabelsLocalization.L("General review"),
        _ => value ?? "General review"
    };

    public static string UbicacionMoldMoisture(string? value) => value switch
    {
        "Bathroom" => DisplayLabelsLocalization.L("Bathroom"),
        "Kitchen" => DisplayLabelsLocalization.L("Kitchen"),
        "Ceiling" => DisplayLabelsLocalization.L("Ceiling"),
        "Wall" => DisplayLabelsLocalization.L("Wall"),
        "CrawlSpace" => DisplayLabelsLocalization.L("Crawl space"),
        "Attic" => DisplayLabelsLocalization.L("Attic"),
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        "HvacCloset" => DisplayLabelsLocalization.L("HVAC closet"),
        "AroundWindow" => DisplayLabelsLocalization.L("Around window"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Bathroom"
    };

    public static string MotivoRevisionMoldMoisture(string? value) => value switch
    {
        "MustySmellConcern" => DisplayLabelsLocalization.L("Musty smell concern"),
        "AfterWaterLeak" => DisplayLabelsLocalization.L("After water leak"),
        "BeforePurchase" => DisplayLabelsLocalization.L("Before purchase"),
        "HealthConcern" => DisplayLabelsLocalization.L("Health concern"),
        "AnnualReview" => DisplayLabelsLocalization.L("Annual review"),
        "SecondOpinion" => DisplayLabelsLocalization.L("Second opinion"),
        _ => value ?? "Musty smell concern"
    };

    public static string TipoPropiedadMoldMoisture(string? value) => value switch
    {
        "SingleFamily" => DisplayLabelsLocalization.L("Single-family"),
        "Townhome" => DisplayLabelsLocalization.L("Townhome"),
        "Condo" => DisplayLabelsLocalization.L("Condo"),
        "Apartment" => DisplayLabelsLocalization.L("Apartment"),
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
            "Ceiling" => DisplayLabelsLocalization.L("ceiling moisture"),
            "Bathroom" => DisplayLabelsLocalization.L("bathroom moisture"),
            "Attic" => DisplayLabelsLocalization.L("attic moisture"),
            "CrawlSpace" => DisplayLabelsLocalization.L("crawl space moisture"),
            "HvacCloset" => DisplayLabelsLocalization.L("HVAC moisture"),
            "Wall" => DisplayLabelsLocalization.L("wall moisture"),
            _ => DisplayLabelsLocalization.L("moisture concern")
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
            return DisplayLabelsLocalization.L("bathroom, ceiling, vent area");
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
        "DraftAir" => DisplayLabelsLocalization.L("Draft / air coming in"),
        "HighEnergyBill" => DisplayLabelsLocalization.L("High energy bill"),
        "ColdRoom" => DisplayLabelsLocalization.L("Cold room"),
        "HotRoom" => DisplayLabelsLocalization.L("Hot room"),
        "WindowCondensation" => DisplayLabelsLocalization.L("Window condensation"),
        "WindowSealing" => DisplayLabelsLocalization.L("Window sealing issue"),
        "DamagedInsulation" => DisplayLabelsLocalization.L("Damaged insulation"),
        "AtticInsulation" => DisplayLabelsLocalization.L("Attic insulation"),
        "WallInsulation" => DisplayLabelsLocalization.L("Wall insulation"),
        "MoistureAroundWindow" => DisplayLabelsLocalization.L("Moisture around window"),
        "WholeHouseReview" => DisplayLabelsLocalization.L("Whole-house review"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Draft / air coming in"
    };

    public static string AreaAtencionWindowsInsulation(string? value) => value switch
    {
        "LivingRoom" => DisplayLabelsLocalization.L("Living room"),
        "Bedroom" => DisplayLabelsLocalization.L("Bedroom"),
        "Attic" => DisplayLabelsLocalization.L("Attic"),
        "CrawlSpace" => DisplayLabelsLocalization.L("Crawl space"),
        "AroundWindows" => DisplayLabelsLocalization.L("Around windows"),
        "ExteriorWall" => DisplayLabelsLocalization.L("Exterior wall"),
        "WholeHouse" => DisplayLabelsLocalization.L("Whole house"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Living room"
    };

    public static string MotivoRevisionWindowsInsulation(string? value) => value switch
    {
        "HighUtilityBill" => DisplayLabelsLocalization.L("High utility bill"),
        "ComfortIssue" => DisplayLabelsLocalization.L("Comfort issue"),
        "BeforePurchase" => DisplayLabelsLocalization.L("Before purchase"),
        "AfterRemodel" => DisplayLabelsLocalization.L("After remodel"),
        "AnnualReview" => DisplayLabelsLocalization.L("Annual review"),
        "MoistureConcern" => DisplayLabelsLocalization.L("Moisture concern"),
        "SecondOpinion" => DisplayLabelsLocalization.L("Second opinion"),
        _ => value ?? "High utility bill"
    };

    public static string NumeroPisosWindowsInsulation(string? value) => value switch
    {
        "OneStory" => DisplayLabelsLocalization.L("1 story"),
        "TwoStory" => DisplayLabelsLocalization.L("2 story"),
        "ThreePlus" => DisplayLabelsLocalization.L("3+ story"),
        _ => value ?? "2 story"
    };

    public static string AreaEnfoqueWindowsInsulation(string? value) => value switch
    {
        "Windows" => DisplayLabelsLocalization.L("Windows"),
        "AtticInsulation" => DisplayLabelsLocalization.L("Attic insulation"),
        "Doors" => DisplayLabelsLocalization.L("Doors"),
        "CrawlSpaceInsulation" => DisplayLabelsLocalization.L("Crawl space insulation"),
        "ExteriorWalls" => DisplayLabelsLocalization.L("Exterior walls"),
        "WholeHouse" => DisplayLabelsLocalization.L("Whole house"),
        _ => value ?? "Windows"
    };

    public static string TipoVentanaWindowsInsulation(string? value) => value switch
    {
        "DoublePane" => DisplayLabelsLocalization.L("Double-pane windows"),
        "SinglePane" => DisplayLabelsLocalization.L("Single-pane windows"),
        "NotSure" => DisplayLabelsLocalization.L("Window type not sure"),
        _ => value ?? "Double-pane windows"
    };

    public static string DanoHumedadVisibleWindowsInsulation(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "No"
    };

    public static string UrgenciaWindowsInsulation(string? value) => value switch
    {
        "Normal" => DisplayLabelsLocalization.L("Normal"),
        "Priority" => DisplayLabelsLocalization.L("Priority"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
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
            return DisplayLabelsLocalization.L("No files uploaded");
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
        "SmokeDetectorConcern" => DisplayLabelsLocalization.L("Smoke detector concern"),
        "CoDetectorConcern" => DisplayLabelsLocalization.L("CO detector concern"),
        "NoDetectors" => DisplayLabelsLocalization.L("No detectors"),
        "ChirpingAlarm" => DisplayLabelsLocalization.L("Chirping alarm"),
        "FireExtinguisherConcern" => DisplayLabelsLocalization.L("Fire extinguisher concern"),
        "TripFallHazard" => DisplayLabelsLocalization.L("Trip / fall hazard"),
        "StairRailingConcern" => DisplayLabelsLocalization.L("Stair / railing concern"),
        "DoorLockSafety" => DisplayLabelsLocalization.L("Door / lock safety"),
        "OutletBasicHazard" => DisplayLabelsLocalization.L("Outlet / basic hazard"),
        "ChildSafetyConcern" => DisplayLabelsLocalization.L("Child safety concern"),
        "GeneralSafetyReview" => DisplayLabelsLocalization.L("General safety review"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Smoke detector concern"
    };

    public static string AreaAtencionHomeSafety(string? value) => value switch
    {
        "Hallway" => DisplayLabelsLocalization.L("Hallway"),
        "Bedroom" => DisplayLabelsLocalization.L("Bedroom"),
        "Kitchen" => DisplayLabelsLocalization.L("Kitchen"),
        "Garage" => DisplayLabelsLocalization.L("Garage"),
        "Stairway" => DisplayLabelsLocalization.L("Stairway"),
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        "Exterior" => DisplayLabelsLocalization.L("Exterior"),
        "WholeHouse" => DisplayLabelsLocalization.L("Whole house"),
        "Laundry" => DisplayLabelsLocalization.L("Laundry"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Hallway"
    };

    public static string MotivoRevisionHomeSafety(string? value) => value switch
    {
        "AnnualReview" => DisplayLabelsLocalization.L("Annual review"),
        "ChildrenAtHome" => DisplayLabelsLocalization.L("Children at home"),
        "BeforePurchase" => DisplayLabelsLocalization.L("Before purchase"),
        "ElderlySafety" => DisplayLabelsLocalization.L("Elderly safety"),
        "SecondOpinion" => DisplayLabelsLocalization.L("Second opinion"),
        _ => value ?? "Annual review"
    };

    public static string AreaEnfoqueHomeSafety(string? value) => value switch
    {
        "SmokeCoDetectors" => DisplayLabelsLocalization.L("Smoke / CO detectors"),
        "WholeHouse" => DisplayLabelsLocalization.L("Whole house"),
        "StairsRailings" => DisplayLabelsLocalization.L("Stairs & railings"),
        "DoorsExits" => DisplayLabelsLocalization.L("Doors & exits"),
        "GarageSafety" => DisplayLabelsLocalization.L("Garage safety"),
        "KitchenSafety" => DisplayLabelsLocalization.L("Kitchen safety"),
        _ => value ?? "Smoke / CO detectors"
    };

    public static string OcupanteHomeSafety(string? value) => value switch
    {
        "Children" => DisplayLabelsLocalization.L("Children"),
        "Adults" => DisplayLabelsLocalization.L("Adults"),
        "Seniors" => DisplayLabelsLocalization.L("Seniors"),
        "Pets" => DisplayLabelsLocalization.L("Pets"),
        _ => value ?? "Children"
    };

    public static string RiesgoActivoHomeSafety(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
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
            return DisplayLabelsLocalization.L("No files uploaded");
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
        "Flip" => DisplayLabelsLocalization.L("Flip"),
        "RentalProperty" => DisplayLabelsLocalization.L("Rental property"),
        "BuyAndHold" => DisplayLabelsLocalization.L("Buy and hold"),
        "BRRRR" => DisplayLabelsLocalization.L("BRRRR"),
        "BeforeOffer" => DisplayLabelsLocalization.L("Before offer"),
        "BeforeClosing" => DisplayLabelsLocalization.L("Before closing"),
        _ => value ?? "Flip"
    };

    public static string EnfoqueInversionInvestor(string? value) => value switch
    {
        "GeneralAssessment" => DisplayLabelsLocalization.L("General assessment"),
        "RepairRisk" => DisplayLabelsLocalization.L("Repair risk"),
        "RehabBudget" => DisplayLabelsLocalization.L("Rehab evaluation"),
        "RentReadiness" => DisplayLabelsLocalization.L("Rent readiness"),
        "ResalePotential" => DisplayLabelsLocalization.L("Resale potential"),
        "NegotiationSupport" => DisplayLabelsLocalization.L("Negotiation support"),
        _ => value ?? "Rehab evaluation"
    };

    public static string TipoPropiedadInvestor(string? value) => value switch
    {
        "SingleFamily" => DisplayLabelsLocalization.L("Single-family"),
        "Duplex" => DisplayLabelsLocalization.L("Duplex"),
        "Triplex" => DisplayLabelsLocalization.L("Triplex"),
        "Condo" => DisplayLabelsLocalization.L("Condo"),
        "SmallMultifamily" => DisplayLabelsLocalization.L("Small multifamily"),
        _ => value ?? "Single-family"
    };

    public static string OcupacionInvestor(string? value) => value switch
    {
        "TenantOccupied" => DisplayLabelsLocalization.L("Tenant occupied"),
        "Vacant" => DisplayLabelsLocalization.L("Vacant"),
        "OwnerOccupied" => DisplayLabelsLocalization.L("Owner occupied"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Tenant occupied"
    };

    public static string NivelRehabInvestor(string? value) => value switch
    {
        "Light" => DisplayLabelsLocalization.L("Light"),
        "Medium" => DisplayLabelsLocalization.L("Medium"),
        "Heavy" => DisplayLabelsLocalization.L("Heavy"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Light"
    };

    public static string AreaRevisionInvestor(string? value) => value switch
    {
        "Roof" => DisplayLabelsLocalization.L("Roof"),
        "Hvac" => DisplayLabelsLocalization.L("HVAC"),
        "Plumbing" => DisplayLabelsLocalization.L("Plumbing"),
        "Electrical" => DisplayLabelsLocalization.L("Electrical"),
        "Foundation" => DisplayLabelsLocalization.L("Foundation"),
        "Moisture" => DisplayLabelsLocalization.L("Moisture"),
        "Safety" => DisplayLabelsLocalization.L("Safety"),
        "Cosmetic" => DisplayLabelsLocalization.L("Cosmetic"),
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
            return DisplayLabelsLocalization.L("Single-family home");
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
            return DisplayLabelsLocalization.L("No files uploaded");
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
