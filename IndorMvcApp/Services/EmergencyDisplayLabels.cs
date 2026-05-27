namespace IndorMvcApp.Services;

public static class EmergencyDisplayLabels
{
    public static string TipoProblemaPlomeria(string? value) => value switch
    {
        "BurstPipe" => "Burst pipe",
        "LeakDripping" => "Leak / dripping",
        "CloggedDrain" => "Clogged drain",
        "ToiletOverflow" => "Toilet overflow",
        "MainShutoff" => "Main shutoff",
        "WaterHeaterLeak" => "Water heater leak",
        "SewerBackup" => "Sewer backup",
        "Other" => "Other",
        _ => "Plumbing emergency"
    };

    public static string AguaFluyendo(string? value) => value switch
    {
        "Yes" => "Water actively flowing",
        "No" => "Water not actively flowing",
        "NotSure" => "Water flow unknown",
        _ => "Water status unknown"
    };

    public static string PuedeCerrarAgua(string? value) => value switch
    {
        "Yes" => "Can shut off water",
        "No" => "Unable to shut off water",
        "NeedHelp" => "Needs help shutting off water",
        _ => "Shutoff status unknown"
    };

    public static string UrgenciaEmergencia(string? value) => value switch
    {
        "Emergency" => "Emergency",
        "Priority" => "Priority",
        "Today" => "Today",
        "Normal" => "Normal",
        _ => value ?? "Emergency"
    };

    public static string AccesoSiAusente(string? value) => value switch
    {
        "Yes" => "Yes, enter if not home",
        "No" => "No, do not enter",
        "CallFirst" => "Call first before entering",
        _ => "Not specified"
    };

    public static string FormatPlumbingProblem(string? tipoProblema, string? aguaFluyendo)
    {
        var problem = TipoProblemaPlomeria(tipoProblema);
        if (string.Equals(aguaFluyendo, "Yes", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(tipoProblema, "Other", StringComparison.OrdinalIgnoreCase))
        {
            return $"{problem} / active leak";
        }

        return problem;
    }

    public static string ArchivosResumen(int count)
    {
        return count switch
        {
            0 => "No files uploaded",
            1 => "1 photo",
            _ => $"{count} photos"
        };
    }

    public static string ArchivosAdjuntos(int count)
    {
        return count switch
        {
            0 => "No photos attached",
            1 => "1 attached",
            _ => $"{count} attached"
        };
    }

    public static string TipoProblemaHvac(string? value) => value switch
    {
        "NotCooling" => "Not cooling",
        "NoHeat" => "No heat",
        "WontTurnOn" => "Won't turn on",
        "WaterLeak" => "Water leak",
        "ThermostatIssue" => "Thermostat issue",
        "BurningSmell" => "Burning smell / smoke",
        "WeakAirflow" => "Weak airflow",
        "StrangeNoise" => "Strange noise",
        _ => "HVAC issue"
    };

    public static string SucedeAhora(string? value) => value switch
    {
        "Yes" => "Happening now",
        "No" => "Not happening now",
        "Intermittent" => "Intermittent",
        _ => "Status unknown"
    };

    public static string PuedeLlamarYa(string? value) => value switch
    {
        "Yes" => "Yes, call right away",
        "No" => "Do not call yet",
        "CallFirst" => "Call first",
        _ => "Not specified"
    };

    public static string EnCasaAhora(string? value) => value switch
    {
        "Yes" => "Home now",
        "No" => "Not home",
        "OnMyWay" => "On my way",
        _ => "Not specified"
    };

    public static string UrgenciaEmergenciaCss(string? value) =>
        string.Equals(value, "Emergency", StringComparison.OrdinalIgnoreCase) ? "urgency-emergency" : string.Empty;

    public static string TipoProblemaWaterHeater(string? value) => value switch
    {
        "NoHotWater" => "No hot water",
        "LeakingTank" => "Leaking tank",
        "WaterAroundUnit" => "Water around unit",
        "PilotLight" => "Pilot light / ignition",
        "ErrorCode" => "Error code",
        "StrangeNoise" => "Strange noise",
        "SmellOfGas" => "Smell of gas",
        "PressureReliefValve" => "Pressure relief valve",
        "NotSure" => "Not sure",
        _ => "Water heater issue"
    };

    public static string UnidadFuncionando(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => "Not sure"
    };

    public static string UbicacionWaterHeater(string? value) => value switch
    {
        "Garage" => "Garage",
        "Closet" => "Closet",
        "Attic" => "Attic",
        "Basement" => "Basement",
        "Outside" => "Outside",
        "UtilityRoom" => "Utility room",
        _ => value ?? "Garage"
    };

    public static string TipoUnidadWaterHeater(string? value) => value switch
    {
        "Gas" => "Gas",
        "Electric" => "Electric",
        "Tankless" => "Tankless",
        "NotSure" => "Not sure",
        _ => value ?? "Not sure"
    };

    public static string SintomaWaterHeater(string? value) => value switch
    {
        "WaterLeaking" => "Water leaking",
        "RustyWater" => "Rusty water",
        "NoHotWater" => "No hot water",
        "WarmOnly" => "Warm only",
        "LowPressureNearby" => "Low pressure nearby",
        "ErrorCode" => "Error code",
        "BurnerWontStayOn" => "Burner won't stay on",
        "PoppingNoise" => "Popping noise",
        _ => value ?? string.Empty
    };

    public static string FormatWaterHeaterIssues(string? tiposPipeSeparated, string? fallback)
    {
        if (string.IsNullOrWhiteSpace(tiposPipeSeparated))
        {
            return TipoProblemaWaterHeater(fallback);
        }

        var labels = tiposPipeSeparated
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(TipoProblemaWaterHeater)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return labels.Count == 0
            ? TipoProblemaWaterHeater(fallback)
            : string.Join(" + ", labels);
    }

    public static string FormatSintomasWaterHeater(string? sintomasPipeSeparated)
    {
        if (string.IsNullOrWhiteSpace(sintomasPipeSeparated))
        {
            return string.Empty;
        }

        return string.Join(", ",
            sintomasPipeSeparated
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(SintomaWaterHeater));
    }

    public static string ArchivosSubidos(int count)
    {
        return count switch
        {
            0 => "None uploaded",
            1 => "1 uploaded",
            _ => $"{count} uploaded"
        };
    }

    public static string EstadoWaterHeaterConfirmado(string? estado) => estado switch
    {
        "Submitted" => "Confirmed",
        _ => EstadoSolicitud(estado)
    };

    public static string EstadoSolicitud(string? estado) => estado switch
    {
        "Submitted" => "Searching for provider",
        "PhotosCompleted" => "Review pending",
        "SafetyCompleted" => "Photos pending",
        "LocationCompleted" => "Callback pending",
        "ProblemCompleted" => "Location pending",
        "DescribeCompleted" => "Next step pending",
        "DetailsCompleted" => "Submit pending",
        "YourInfoCompleted" => "Review pending",
        "ContactCompleted" => "Review pending",
        "IssueCompleted" => "Details pending",
        "InProgress" => "In progress",
        _ => estado ?? "In progress"
    };

    public static string CausaAguaFlood(string? value) => value switch
    {
        "BurstPipe" => "Burst pipe",
        "WaterHeater" => "Water heater",
        "ApplianceLeak" => "Appliance leak",
        "ToiletOverflow" => "Toilet overflow",
        "RoofCeilingLeak" => "Roof / ceiling leak",
        "UnknownSource" => "Unknown / being investigated",
        _ => "Unknown / being investigated"
    };

    public static string UbicacionAguaFlood(string? value) => value switch
    {
        "Basement" => "Basement",
        "FirstFloor" => "1st floor",
        "SecondFloor" => "2nd floor",
        "Bathroom" => "Bathroom",
        "Kitchen" => "Kitchen",
        "Laundry" => "Laundry",
        "Garage" => "Garage",
        "CrawlSpace" => "Crawl space",
        _ => value ?? "Unknown area"
    };

    public static string AguaActivaFlood(string? value) => value switch
    {
        "Yes" => "Water still active",
        "No" => "Water not active",
        "NotSure" => "Active status unknown",
        _ => "Active status unknown"
    };

    public static string UbicacionCierreAguaFlood(string? value) => value switch
    {
        "InsideHome" => "Inside home",
        "Outside" => "Outside",
        "DontKnow" => "Don't know",
        _ => "Don't know"
    };

    public static string PuedeApagarElectricidadFlood(string? value) => value switch
    {
        "Yes" => "Can turn off power",
        "No" => "Cannot turn off power",
        "NeedHelp" => "Needs help with power",
        "NotSure" => "Not sure",
        _ => "Not sure"
    };

    public static string CantidadAguaFlood(string? value) => value switch
    {
        "SmallArea" => "Small area",
        "OneRoom" => "One room",
        "SeveralRooms" => "Several rooms",
        _ => value ?? "One room"
    };

    public static string FormatFloodArea(string? ubicacion, string? cantidad)
    {
        var location = UbicacionAguaFlood(ubicacion);
        var amount = CantidadAguaFlood(cantidad);
        if (string.Equals(amount, "One room", StringComparison.OrdinalIgnoreCase))
        {
            return $"{location} ({amount.ToLowerInvariant()})";
        }

        return $"{location} — {amount.ToLowerInvariant()}";
    }

    public static string TiempoLlegadaRango(int minutos)
    {
        var low = Math.Max(15, minutos - 10);
        var high = minutos + 5;
        return $"{low}-{high} min";
    }

    public static string TiempoLlegadaRangoElectrical(int minutos)
    {
        var low = Math.Max(30, minutos);
        var high = minutos + 15;
        return $"{low}–{high} min";
    }

    public static string TipoProblemaElectrical(string? value) => value switch
    {
        "NoPower" => "No power",
        "PartialOutage" => "Partial outage",
        "BreakerTripping" => "Breaker keeps tripping",
        "OutletSwitch" => "Outlet / switch issue",
        "SparksBurning" => "Sparks or burning smell",
        "PanelIssue" => "Panel issue",
        "ExposedWire" => "Exposed wire",
        "Other" => "Other",
        _ => "Electrical issue"
    };

    public static string UbicacionElectrical(string? value) => value switch
    {
        "WholeHouse" => "Whole house",
        "Kitchen" => "Kitchen",
        "LivingRoom" => "Living room",
        "Bedroom" => "Bedroom",
        "Bathroom" => "Bathroom",
        "Garage" => "Garage",
        "Outside" => "Outside",
        "ElectricalPanel" => "Electrical panel",
        _ => value ?? "Unknown area"
    };

    public static string FormatAreaElectrical(string? ubicacion)
    {
        var location = UbicacionElectrical(ubicacion);
        if (string.Equals(ubicacion, "Garage", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ubicacion, "ElectricalPanel", StringComparison.OrdinalIgnoreCase))
        {
            return $"{location} / panel";
        }

        return location;
    }

    public static string SintomaElectrical(string? value) => value switch
    {
        "NoPower" => "No power",
        "Sparks" => "Sparks",
        "BurningSmell" => "Burning smell",
        "WarmOutlet" => "Warm outlet",
        "Buzzing" => "Buzzing",
        "LightsFlickering" => "Lights flickering",
        _ => value ?? string.Empty
    };

    public static string FormatSintomasElectrical(string? sintomasPipeSeparated)
    {
        if (string.IsNullOrWhiteSpace(sintomasPipeSeparated))
        {
            return string.Empty;
        }

        return string.Join(", ",
            sintomasPipeSeparated
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(SintomaElectrical));
    }

    public static string EnergiaEncendidaElectrical(string? value) => value switch
    {
        "Yes" => "Still on",
        "No" => "Off",
        "NotSure" => "Not sure",
        _ => "Not sure"
    };

    public static string PuedeApagarBreakerElectrical(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => "Not sure"
    };

    public static string PuedeAlejarseElectrical(string? value) => value switch
    {
        "Yes" => "Yes",
        "NeedHelp" => "Need help",
        _ => "Not specified"
    };

    public static string UrgenciaElectrical(string? value) => value switch
    {
        "Emergency" => "Emergency",
        "Urgent" => "Urgent",
        "Priority" => "Priority",
        _ => value ?? "Emergency"
    };

    public static string EstadoElectricalConfirmado(string? estado) => estado switch
    {
        "Submitted" => "Dispatching",
        _ => EstadoSolicitud(estado)
    };

    public static string TipoProblemaTreeDamage(string? value) => value switch
    {
        "TreeOnRoof" => "Tree on roof",
        "FallenBranch" => "Fallen branch",
        "BlockedDriveway" => "Blocked driveway",
        "FenceDamage" => "Fence damage",
        "TreeLeaning" => "Tree leaning",
        "DebrisCleanup" => "Debris cleanup",
        "Other" => "Other",
        _ => "Tree damage"
    };

    public static string UbicacionTreeDamage(string? value) => value switch
    {
        "FrontYard" => "Front yard",
        "BackYard" => "Back yard",
        "Roof" => "Roof",
        "Driveway" => "Driveway",
        "Street" => "Street",
        "SideOfHouse" => "Side of house",
        _ => value ?? "Unknown area"
    };

    public static string FormatProblemaTreeDamage(string? tipo, string? ubicacion)
    {
        var problem = TipoProblemaTreeDamage(tipo);
        if (string.Equals(tipo, "BlockedDriveway", StringComparison.OrdinalIgnoreCase)
            || string.Equals(tipo, "FallenBranch", StringComparison.OrdinalIgnoreCase))
        {
            return $"{problem} / blocked driveway";
        }

        return problem;
    }

    public static string FormatUbicacionTreeDamage(string? ubicacion, string? tipo)
    {
        var location = UbicacionTreeDamage(ubicacion);
        if (string.Equals(ubicacion, "Driveway", StringComparison.OrdinalIgnoreCase)
            || string.Equals(tipo, "BlockedDriveway", StringComparison.OrdinalIgnoreCase))
        {
            return $"{location} / driveway";
        }

        return location;
    }

    public static string PeligroInmediatoTreeDamage(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => "Not sure"
    };

    public static string RiesgoUtilidadTreeDamage(string? value) => value switch
    {
        "NearPowerLine" => "Near power line",
        "NearGasMeter" => "Near gas meter",
        "NoUtilityRisk" => "No utility risk",
        "NotSure" => "Not sure",
        _ => "Not sure"
    };

    public static string AccesoCasaTreeDamage(string? value) => value switch
    {
        "Yes" => "Full",
        "Partially" => "Partial",
        "No" => "No access",
        _ => value ?? "Not specified"
    };

    public static string EntradaBloqueadaTreeDamage(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => "Not sure"
    };

    public static string PuedeAlejarseTreeDamage(string? value) => value switch
    {
        "Yes" => "Yes",
        "NeedHelp" => "Need help",
        "NotSure" => "Not sure",
        _ => "Not sure"
    };

    public static string TiempoLlegadaRangoTreeDamage(int minutos)
    {
        var low = Math.Max(30, minutos);
        var high = minutos + 45;
        return $"{low}–{high} min";
    }

    public static string EstadoTreeDamageConfirmado(string? estado) => estado switch
    {
        "Submitted" => "Dispatching",
        _ => EstadoSolicitud(estado)
    };

    public static string ArchivosAdjuntosTreeDamage(int count)
    {
        return count switch
        {
            0 => "None attached",
            1 => "1 attached",
            _ => $"{count} attached"
        };
    }

    public static string TipoProblemaRoofLeak(string? value) => value switch
    {
        "ActiveDripping" => "Active dripping",
        "CeilingStain" => "Ceiling stain",
        "WaterNearChimney" => "Water near chimney",
        "WaterNearSkylight" => "Water near skylight",
        "MissingShingles" => "Missing shingles",
        "StormDamage" => "Storm damage",
        _ => "Roof leak"
    };

    public static string UbicacionRoofLeak(string? value) => value switch
    {
        "Attic" => "Attic",
        "Ceiling" => "Ceiling",
        "Wall" => "Wall",
        "NearWindow" => "Near window",
        "GutterEdge" => "Gutter / edge",
        "Unknown" => "Unknown",
        _ => value ?? "Unknown area"
    };

    public static string FormatProblemaRoofLeak(string? tipo, string? ubicacion)
    {
        var problem = TipoProblemaRoofLeak(tipo);
        if (string.Equals(tipo, "ActiveDripping", StringComparison.OrdinalIgnoreCase)
            && string.Equals(ubicacion, "Ceiling", StringComparison.OrdinalIgnoreCase))
        {
            return "Active dripping / ceiling leak";
        }

        return problem;
    }

    public static string FormatAreaRoofLeak(string? ubicacion, string? nota)
    {
        var area = UbicacionRoofLeak(ubicacion);
        if (!string.IsNullOrWhiteSpace(nota) && nota.Length <= 80)
        {
            return nota.Trim();
        }

        return area;
    }

    public static string PuedeColocarCubetaRoofLeak(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "AlreadyDone" => "Already done",
        _ => "Not specified"
    };

    public static string TiempoLlegadaRangoRoofLeak(int minutos)
    {
        var low = Math.Max(30, minutos);
        var high = minutos + 15;
        return $"{low}–{high} min";
    }

    public static string EstadoRoofLeakConfirmado(string? estado) => estado switch
    {
        "Submitted" => "Dispatching",
        _ => EstadoSolicitud(estado)
    };

    public static string ArchivosAdjuntosRoofLeak(int count)
    {
        return count switch
        {
            0 => "None attached",
            1 => "1 attached",
            _ => $"{count} attached"
        };
    }

    public static string TipoProblemaSmokeDetector(string? value) => value switch
    {
        "SmokeDetectorBeeping" => "Smoke detector beeping",
        "SmokeDetectorNotWorking" => "Smoke detector not working",
        "CoDetectorAlert" => "CO detector alert",
        "LowBatteryChirp" => "Low battery chirp",
        "SmellOfGas" => "Smell of gas",
        "NeedDetectorCheck" => "Need detector check",
        _ => value ?? "Smoke detector concern"
    };

    public static string UbicacionSmokeDetector(string? value) => value switch
    {
        "Bedroom" => "Bedroom",
        "LivingRoom" => "Living room",
        "Hallway" => "Hallway",
        "Kitchen" => "Kitchen",
        "CommonArea" => "Common area",
        "Basement" => "Basement",
        _ => value ?? "Unknown"
    };

    public static string SituacionActualSmokeDetector(string? value) => value switch
    {
        "AlarmSounding" => "Alarm sounding",
        "NoSound" => "No sound",
        "IntermittentChirp" => "Intermittent chirp",
        "GasSmell" => "Gas smell",
        "NotSure" => "Not sure",
        _ => value ?? "Not sure"
    };

    public static string PuedePermanecerAdentroSmokeDetector(string? value) => value switch
    {
        "Yes" => "Yes, it is safe to stay inside",
        "No" => "No, it is not safe to stay inside",
        "NotSure" => "Not sure it is safe to stay inside",
        _ => "Not sure it is safe to stay inside"
    };

    public static string AccesoPropiedadSmokeDetector(string? value) => value switch
    {
        "AdultHomeNow" => "Adult home now",
        "ChildrenHome" => "Children home now",
        "NoOneHome" => "No one home",
        "SomeoneArriving" => "Someone arriving soon",
        "NotSure" => "Not sure",
        _ => value ?? "Adult home now"
    };

    public static string FormatTiposProblemaSmokeDetector(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return "Smoke detector concern";
        }

        var labels = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(TipoProblemaSmokeDetector)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return labels.Count == 0 ? "Smoke detector concern" : string.Join(" + ", labels);
    }

    public static string FormatUbicacionesSmokeDetector(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return "Unknown";
        }

        var labels = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(UbicacionSmokeDetector)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return labels.Count == 0 ? "Unknown" : string.Join(", ", labels);
    }

    public static string OlorGasSmokeDetector(string? tiposProblema, string? situacionActual)
    {
        var hasGas = ContainsCsvValue(tiposProblema, "SmellOfGas")
                     || string.Equals(situacionActual, "GasSmell", StringComparison.OrdinalIgnoreCase);
        return hasGas ? "Yes" : "No";
    }

    public static string PreocupacionSmokeDetector(string? tiposProblema, string? situacionActual)
    {
        if (ContainsCsvValue(tiposProblema, "CoDetectorAlert")
            || string.Equals(situacionActual, "GasSmell", StringComparison.OrdinalIgnoreCase)
            || ContainsCsvValue(tiposProblema, "SmellOfGas"))
        {
            return "Smoke alarm / CO safety concern";
        }

        return "Smoke alarm safety concern";
    }

    public static string TiempoCallbackRangoSmokeDetector(int minutos)
    {
        var low = Math.Max(10, minutos / 4);
        var high = Math.Max(15, minutos / 3);
        return $"{low}–{high} min";
    }

    public static string EstadoSmokeDetectorConfirmado(string? estado) => estado switch
    {
        "Submitted" => "Provider search in progress",
        _ => EstadoSolicitud(estado)
    };

    private static bool ContainsCsvValue(string? csv, string value)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return false;
        }

        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));
    }
}
