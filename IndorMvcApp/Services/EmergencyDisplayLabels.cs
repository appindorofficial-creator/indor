namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class EmergencyDisplayLabels
{
    public static string TipoProblemaPlomeria(string? value) => value switch
    {
        "BurstPipe" => DisplayLabelsLocalization.L("Burst pipe"),
        "LeakDripping" => DisplayLabelsLocalization.L("Leak / dripping"),
        "CloggedDrain" => DisplayLabelsLocalization.L("Clogged drain"),
        "ToiletOverflow" => DisplayLabelsLocalization.L("Toilet overflow"),
        "MainShutoff" => DisplayLabelsLocalization.L("Main shutoff"),
        "WaterHeaterLeak" => DisplayLabelsLocalization.L("Water heater leak"),
        "SewerBackup" => DisplayLabelsLocalization.L("Sewer backup"),
        "Other" => DisplayLabelsLocalization.L("Other"),
        _ => DisplayLabelsLocalization.L("Plumbing emergency")
    };

    public static string AguaFluyendo(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Water actively flowing"),
        "No" => DisplayLabelsLocalization.L("Water not actively flowing"),
        "NotSure" => DisplayLabelsLocalization.L("Water flow unknown"),
        _ => DisplayLabelsLocalization.L("Water status unknown")
    };

    public static string PuedeCerrarAgua(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Can shut off water"),
        "No" => DisplayLabelsLocalization.L("Unable to shut off water"),
        "NeedHelp" => DisplayLabelsLocalization.L("Needs help shutting off water"),
        _ => DisplayLabelsLocalization.L("Shutoff status unknown")
    };

    public static string UrgenciaEmergencia(string? value) => value switch
    {
        "Emergency" => DisplayLabelsLocalization.L("Emergency"),
        "Priority" => DisplayLabelsLocalization.L("Priority"),
        "Today" => DisplayLabelsLocalization.L("Today"),
        "Normal" => DisplayLabelsLocalization.L("Normal"),
        _ => value ?? "Emergency"
    };

    public static string AccesoSiAusente(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes, enter if not home"),
        "No" => DisplayLabelsLocalization.L("No, do not enter"),
        "CallFirst" => DisplayLabelsLocalization.L("Call first before entering"),
        _ => DisplayLabelsLocalization.L("Not specified")
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
        "NotCooling" => DisplayLabelsLocalization.L("Not cooling"),
        "NoHeat" => DisplayLabelsLocalization.L("No heat"),
        "WontTurnOn" => DisplayLabelsLocalization.L("Won't turn on"),
        "WaterLeak" => DisplayLabelsLocalization.L("Water leak"),
        "ThermostatIssue" => DisplayLabelsLocalization.L("Thermostat issue"),
        "BurningSmell" => DisplayLabelsLocalization.L("Burning smell / smoke"),
        "WeakAirflow" => DisplayLabelsLocalization.L("Weak airflow"),
        "StrangeNoise" => DisplayLabelsLocalization.L("Strange noise"),
        _ => DisplayLabelsLocalization.L("HVAC issue")
    };

    public static string SucedeAhora(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Happening now"),
        "No" => DisplayLabelsLocalization.L("Not happening now"),
        "Intermittent" => DisplayLabelsLocalization.L("Intermittent"),
        _ => DisplayLabelsLocalization.L("Status unknown")
    };

    public static string PuedeLlamarYa(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes, call right away"),
        "No" => DisplayLabelsLocalization.L("Do not call yet"),
        "CallFirst" => DisplayLabelsLocalization.L("Call first"),
        _ => DisplayLabelsLocalization.L("Not specified")
    };

    public static string EnCasaAhora(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Home now"),
        "No" => DisplayLabelsLocalization.L("Not home"),
        "OnMyWay" => DisplayLabelsLocalization.L("On my way"),
        _ => DisplayLabelsLocalization.L("Not specified")
    };

    public static string UrgenciaEmergenciaCss(string? value) =>
        string.Equals(value, "Emergency", StringComparison.OrdinalIgnoreCase) ? "urgency-emergency" : string.Empty;

    public static string TipoProblemaWaterHeater(string? value) => value switch
    {
        "NoHotWater" => DisplayLabelsLocalization.L("No hot water"),
        "LeakingTank" => DisplayLabelsLocalization.L("Leaking tank"),
        "WaterAroundUnit" => DisplayLabelsLocalization.L("Water around unit"),
        "PilotLight" => DisplayLabelsLocalization.L("Pilot light / ignition"),
        "ErrorCode" => DisplayLabelsLocalization.L("Error code"),
        "StrangeNoise" => DisplayLabelsLocalization.L("Strange noise"),
        "SmellOfGas" => DisplayLabelsLocalization.L("Smell of gas"),
        "PressureReliefValve" => DisplayLabelsLocalization.L("Pressure relief valve"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => DisplayLabelsLocalization.L("Water heater issue")
    };

    public static string UnidadFuncionando(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string UbicacionWaterHeater(string? value) => value switch
    {
        "Garage" => DisplayLabelsLocalization.L("Garage"),
        "Closet" => DisplayLabelsLocalization.L("Closet"),
        "Attic" => DisplayLabelsLocalization.L("Attic"),
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        "Outside" => DisplayLabelsLocalization.L("Outside"),
        "UtilityRoom" => DisplayLabelsLocalization.L("Utility room"),
        _ => value ?? "Garage"
    };

    public static string TipoUnidadWaterHeater(string? value) => value switch
    {
        "Gas" => DisplayLabelsLocalization.L("Gas"),
        "Electric" => DisplayLabelsLocalization.L("Electric"),
        "Tankless" => DisplayLabelsLocalization.L("Tankless"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Not sure"
    };

    public static string SintomaWaterHeater(string? value) => value switch
    {
        "WaterLeaking" => DisplayLabelsLocalization.L("Water leaking"),
        "RustyWater" => DisplayLabelsLocalization.L("Rusty water"),
        "NoHotWater" => DisplayLabelsLocalization.L("No hot water"),
        "WarmOnly" => DisplayLabelsLocalization.L("Warm only"),
        "LowPressureNearby" => DisplayLabelsLocalization.L("Low pressure nearby"),
        "ErrorCode" => DisplayLabelsLocalization.L("Error code"),
        "BurnerWontStayOn" => DisplayLabelsLocalization.L("Burner won't stay on"),
        "PoppingNoise" => DisplayLabelsLocalization.L("Popping noise"),
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
        "Submitted" => DisplayLabelsLocalization.L("Confirmed"),
        _ => EstadoSolicitud(estado)
    };

    public static string EstadoSolicitud(string? estado) => estado switch
    {
        "Submitted" => DisplayLabelsLocalization.L("Searching for provider"),
        "PhotosCompleted" => DisplayLabelsLocalization.L("Review pending"),
        "SafetyCompleted" => DisplayLabelsLocalization.L("Photos pending"),
        "LocationCompleted" => DisplayLabelsLocalization.L("Callback pending"),
        "ProblemCompleted" => DisplayLabelsLocalization.L("Location pending"),
        "DescribeCompleted" => DisplayLabelsLocalization.L("Next step pending"),
        "DetailsCompleted" => DisplayLabelsLocalization.L("Submit pending"),
        "YourInfoCompleted" => DisplayLabelsLocalization.L("Review pending"),
        "ContactCompleted" => DisplayLabelsLocalization.L("Review pending"),
        "IssueCompleted" => DisplayLabelsLocalization.L("Details pending"),
        "InProgress" => DisplayLabelsLocalization.L("In progress"),
        _ => estado ?? "In progress"
    };

    public static string CausaAguaFlood(string? value) => value switch
    {
        "BurstPipe" => DisplayLabelsLocalization.L("Burst pipe"),
        "WaterHeater" => DisplayLabelsLocalization.L("Water heater"),
        "ApplianceLeak" => DisplayLabelsLocalization.L("Appliance leak"),
        "ToiletOverflow" => DisplayLabelsLocalization.L("Toilet overflow"),
        "RoofCeilingLeak" => DisplayLabelsLocalization.L("Roof / ceiling leak"),
        "UnknownSource" => DisplayLabelsLocalization.L("Unknown / being investigated"),
        _ => DisplayLabelsLocalization.L("Unknown / being investigated")
    };

    public static string UbicacionAguaFlood(string? value) => value switch
    {
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        "FirstFloor" => DisplayLabelsLocalization.L("1st floor"),
        "SecondFloor" => DisplayLabelsLocalization.L("2nd floor"),
        "Bathroom" => DisplayLabelsLocalization.L("Bathroom"),
        "Kitchen" => DisplayLabelsLocalization.L("Kitchen"),
        "Laundry" => DisplayLabelsLocalization.L("Laundry"),
        "Garage" => DisplayLabelsLocalization.L("Garage"),
        "CrawlSpace" => DisplayLabelsLocalization.L("Crawl space"),
        _ => value ?? "Unknown area"
    };

    public static string AguaActivaFlood(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Water still active"),
        "No" => DisplayLabelsLocalization.L("Water not active"),
        "NotSure" => DisplayLabelsLocalization.L("Active status unknown"),
        _ => DisplayLabelsLocalization.L("Active status unknown")
    };

    public static string UbicacionCierreAguaFlood(string? value) => value switch
    {
        "InsideHome" => DisplayLabelsLocalization.L("Inside home"),
        "Outside" => DisplayLabelsLocalization.L("Outside"),
        "DontKnow" => DisplayLabelsLocalization.L("Don't know"),
        _ => DisplayLabelsLocalization.L("Don't know")
    };

    public static string PuedeApagarElectricidadFlood(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Can turn off power"),
        "No" => DisplayLabelsLocalization.L("Cannot turn off power"),
        "NeedHelp" => DisplayLabelsLocalization.L("Needs help with power"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string CantidadAguaFlood(string? value) => value switch
    {
        "SmallArea" => DisplayLabelsLocalization.L("Small area"),
        "OneRoom" => DisplayLabelsLocalization.L("One room"),
        "SeveralRooms" => DisplayLabelsLocalization.L("Several rooms"),
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

        return $"{location} â€” {amount.ToLowerInvariant()}";
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
        return $"{low}â€“{high} min";
    }

    public static string TipoProblemaElectrical(string? value) => value switch
    {
        "NoPower" => DisplayLabelsLocalization.L("No power"),
        "PartialOutage" => DisplayLabelsLocalization.L("Partial outage"),
        "BreakerTripping" => DisplayLabelsLocalization.L("Breaker keeps tripping"),
        "OutletSwitch" => DisplayLabelsLocalization.L("Outlet / switch issue"),
        "SparksBurning" => DisplayLabelsLocalization.L("Sparks or burning smell"),
        "PanelIssue" => DisplayLabelsLocalization.L("Panel issue"),
        "ExposedWire" => DisplayLabelsLocalization.L("Exposed wire"),
        "Other" => DisplayLabelsLocalization.L("Other"),
        _ => DisplayLabelsLocalization.L("Electrical issue")
    };

    public static string UbicacionElectrical(string? value) => value switch
    {
        "WholeHouse" => DisplayLabelsLocalization.L("Whole house"),
        "Kitchen" => DisplayLabelsLocalization.L("Kitchen"),
        "LivingRoom" => DisplayLabelsLocalization.L("Living room"),
        "Bedroom" => DisplayLabelsLocalization.L("Bedroom"),
        "Bathroom" => DisplayLabelsLocalization.L("Bathroom"),
        "Garage" => DisplayLabelsLocalization.L("Garage"),
        "Outside" => DisplayLabelsLocalization.L("Outside"),
        "ElectricalPanel" => DisplayLabelsLocalization.L("Electrical panel"),
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
        "NoPower" => DisplayLabelsLocalization.L("No power"),
        "Sparks" => DisplayLabelsLocalization.L("Sparks"),
        "BurningSmell" => DisplayLabelsLocalization.L("Burning smell"),
        "WarmOutlet" => DisplayLabelsLocalization.L("Warm outlet"),
        "Buzzing" => DisplayLabelsLocalization.L("Buzzing"),
        "LightsFlickering" => DisplayLabelsLocalization.L("Lights flickering"),
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
        "Yes" => DisplayLabelsLocalization.L("Still on"),
        "No" => DisplayLabelsLocalization.L("Off"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string PuedeApagarBreakerElectrical(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string PuedeAlejarseElectrical(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "NeedHelp" => DisplayLabelsLocalization.L("Need help"),
        _ => DisplayLabelsLocalization.L("Not specified")
    };

    public static string UrgenciaElectrical(string? value) => value switch
    {
        "Emergency" => DisplayLabelsLocalization.L("Emergency"),
        "Urgent" => DisplayLabelsLocalization.L("Urgent"),
        "Priority" => DisplayLabelsLocalization.L("Priority"),
        _ => value ?? "Emergency"
    };

    public static string EstadoElectricalConfirmado(string? estado) => estado switch
    {
        "Submitted" => DisplayLabelsLocalization.L("Dispatching"),
        _ => EstadoSolicitud(estado)
    };

    public static string TipoProblemaTreeDamage(string? value) => value switch
    {
        "TreeOnRoof" => DisplayLabelsLocalization.L("Tree on roof"),
        "FallenBranch" => DisplayLabelsLocalization.L("Fallen branch"),
        "BlockedDriveway" => DisplayLabelsLocalization.L("Blocked driveway"),
        "FenceDamage" => DisplayLabelsLocalization.L("Fence damage"),
        "TreeLeaning" => DisplayLabelsLocalization.L("Tree leaning"),
        "DebrisCleanup" => DisplayLabelsLocalization.L("Debris cleanup"),
        "Other" => DisplayLabelsLocalization.L("Other"),
        _ => DisplayLabelsLocalization.L("Tree damage")
    };

    public static string UbicacionTreeDamage(string? value) => value switch
    {
        "FrontYard" => DisplayLabelsLocalization.L("Front yard"),
        "BackYard" => DisplayLabelsLocalization.L("Back yard"),
        "Roof" => DisplayLabelsLocalization.L("Roof"),
        "Driveway" => DisplayLabelsLocalization.L("Driveway"),
        "Street" => DisplayLabelsLocalization.L("Street"),
        "SideOfHouse" => DisplayLabelsLocalization.L("Side of house"),
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
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string RiesgoUtilidadTreeDamage(string? value) => value switch
    {
        "NearPowerLine" => DisplayLabelsLocalization.L("Near power line"),
        "NearGasMeter" => DisplayLabelsLocalization.L("Near gas meter"),
        "NoUtilityRisk" => DisplayLabelsLocalization.L("No utility risk"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string AccesoCasaTreeDamage(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Full"),
        "Partially" => DisplayLabelsLocalization.L("Partial"),
        "No" => DisplayLabelsLocalization.L("No access"),
        _ => value ?? "Not specified"
    };

    public static string EntradaBloqueadaTreeDamage(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string PuedeAlejarseTreeDamage(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "NeedHelp" => DisplayLabelsLocalization.L("Need help"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string TiempoLlegadaRangoTreeDamage(int minutos)
    {
        var low = Math.Max(30, minutos);
        var high = minutos + 45;
        return $"{low}â€“{high} min";
    }

    public static string EstadoTreeDamageConfirmado(string? estado) => estado switch
    {
        "Submitted" => DisplayLabelsLocalization.L("Dispatching"),
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
        "ActiveDripping" => DisplayLabelsLocalization.L("Active dripping"),
        "CeilingStain" => DisplayLabelsLocalization.L("Ceiling stain"),
        "WaterNearChimney" => DisplayLabelsLocalization.L("Water near chimney"),
        "WaterNearSkylight" => DisplayLabelsLocalization.L("Water near skylight"),
        "MissingShingles" => DisplayLabelsLocalization.L("Missing shingles"),
        "StormDamage" => DisplayLabelsLocalization.L("Storm damage"),
        _ => DisplayLabelsLocalization.L("Roof leak")
    };

    public static string UbicacionRoofLeak(string? value) => value switch
    {
        "Attic" => DisplayLabelsLocalization.L("Attic"),
        "Ceiling" => DisplayLabelsLocalization.L("Ceiling"),
        "Wall" => DisplayLabelsLocalization.L("Wall"),
        "NearWindow" => DisplayLabelsLocalization.L("Near window"),
        "GutterEdge" => DisplayLabelsLocalization.L("Gutter / edge"),
        "Unknown" => DisplayLabelsLocalization.L("Unknown"),
        _ => value ?? "Unknown area"
    };

    public static string FormatProblemaRoofLeak(string? tipo, string? ubicacion)
    {
        var problem = TipoProblemaRoofLeak(tipo);
        if (string.Equals(tipo, "ActiveDripping", StringComparison.OrdinalIgnoreCase)
            && string.Equals(ubicacion, "Ceiling", StringComparison.OrdinalIgnoreCase))
        {
            return DisplayLabelsLocalization.L("Active dripping / ceiling leak");
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
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "AlreadyDone" => DisplayLabelsLocalization.L("Already done"),
        _ => DisplayLabelsLocalization.L("Not specified")
    };

    public static string TiempoLlegadaRangoRoofLeak(int minutos)
    {
        var low = Math.Max(30, minutos);
        var high = minutos + 15;
        return $"{low}â€“{high} min";
    }

    public static string EstadoRoofLeakConfirmado(string? estado) => estado switch
    {
        "Submitted" => DisplayLabelsLocalization.L("Dispatching"),
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
        "SmokeDetectorBeeping" => DisplayLabelsLocalization.L("Smoke detector beeping"),
        "SmokeDetectorNotWorking" => DisplayLabelsLocalization.L("Smoke detector not working"),
        "CoDetectorAlert" => DisplayLabelsLocalization.L("CO detector alert"),
        "LowBatteryChirp" => DisplayLabelsLocalization.L("Low battery chirp"),
        "SmellOfGas" => DisplayLabelsLocalization.L("Smell of gas"),
        "NeedDetectorCheck" => DisplayLabelsLocalization.L("Need detector check"),
        _ => value ?? "Smoke detector concern"
    };

    public static string UbicacionSmokeDetector(string? value) => value switch
    {
        "Bedroom" => DisplayLabelsLocalization.L("Bedroom"),
        "LivingRoom" => DisplayLabelsLocalization.L("Living room"),
        "Hallway" => DisplayLabelsLocalization.L("Hallway"),
        "Kitchen" => DisplayLabelsLocalization.L("Kitchen"),
        "CommonArea" => DisplayLabelsLocalization.L("Common area"),
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        _ => value ?? "Unknown"
    };

    public static string SituacionActualSmokeDetector(string? value) => value switch
    {
        "AlarmSounding" => DisplayLabelsLocalization.L("Alarm sounding"),
        "NoSound" => DisplayLabelsLocalization.L("No sound"),
        "IntermittentChirp" => DisplayLabelsLocalization.L("Intermittent chirp"),
        "GasSmell" => DisplayLabelsLocalization.L("Gas smell"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Not sure"
    };

    public static string PuedePermanecerAdentroSmokeDetector(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes, it is safe to stay inside"),
        "No" => DisplayLabelsLocalization.L("No, it is not safe to stay inside"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure it is safe to stay inside"),
        _ => DisplayLabelsLocalization.L("Not sure it is safe to stay inside")
    };

    public static string AccesoPropiedadSmokeDetector(string? value) => value switch
    {
        "AdultHomeNow" => DisplayLabelsLocalization.L("Adult home now"),
        "ChildrenHome" => DisplayLabelsLocalization.L("Children home now"),
        "NoOneHome" => DisplayLabelsLocalization.L("No one home"),
        "SomeoneArriving" => DisplayLabelsLocalization.L("Someone arriving soon"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "Adult home now"
    };

    public static string FormatTiposProblemaSmokeDetector(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return DisplayLabelsLocalization.L("Smoke detector concern");
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
            return DisplayLabelsLocalization.L("Unknown");
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
            return DisplayLabelsLocalization.L("Smoke alarm / CO safety concern");
        }

        return DisplayLabelsLocalization.L("Smoke alarm safety concern");
    }

    public static string TiempoCallbackRangoSmokeDetector(int minutos)
    {
        var low = Math.Max(10, minutos / 4);
        var high = Math.Max(15, minutos / 3);
        return $"{low}â€“{high} min";
    }

    public static string EstadoSmokeDetectorConfirmado(string? estado) => estado switch
    {
        "Submitted" => DisplayLabelsLocalization.L("Provider search in progress"),
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
