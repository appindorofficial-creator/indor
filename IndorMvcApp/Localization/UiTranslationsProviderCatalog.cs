namespace IndorMvcApp.Localization;

/// <summary>
/// Additive Spanish labels for provider onboarding / profile service catalog
/// (<see cref="Models.OnboardingCatalog"/> English keys stored in DB).
/// Display via <c>ProviderProDisplayLocalization.CatalogLabel</c> →
/// <c>CatalogText.PickWithUiFallback</c>. Registered last so keys win over
/// incomplete merges (e.g. Drywall → Drywall).
/// </summary>
public static class UiTranslationsProviderCatalog
{
    public static IEnumerable<KeyValuePair<string, string>> Entries =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Categories — match SeedCatalogSpanishTranslations / catalog LabelEs tone
            ["Electrical"] = "Eléctrico",
            ["Plumbing"] = "Plomería",
            ["HVAC"] = "HVAC",
            ["Handyman"] = "Manitas",
            ["Construction Company"] = "Empresa de construcción",
            ["Bathroom Remodeling"] = "Remodelación de baño",
            ["Kitchen Remodeling"] = "Remodelación de cocina",
            ["Roofing"] = "Techado",
            ["Painting"] = "Pintura",
            ["Flooring"] = "Pisos",
            ["Cleaning"] = "Limpieza",
            ["Landscaping"] = "Paisajismo",
            ["Pest Control"] = "Control de plagas",
            ["Appliance Repair"] = "Reparación de electrodomésticos",

            // Generic offerings
            ["Installations"] = "Instalaciones",
            ["Repairs"] = "Reparaciones",
            ["Maintenance"] = "Mantenimiento",
            ["Upgrades"] = "Mejoras",
            ["Inspections"] = "Inspecciones",
            ["Emergency Services"] = "Servicios de emergencia",

            // Plumbing
            ["Leak repair"] = "Reparación de fugas",
            ["Drain cleaning"] = "Limpieza de drenajes",
            ["Water heater service"] = "Servicio de calentador de agua",
            ["Fixture installation"] = "Instalación de accesorios",

            // Appliance repair
            ["Refrigerator"] = "Refrigerador",
            ["Freezer"] = "Congelador",
            ["Dishwasher"] = "Lavavajillas",
            ["Oven / Range"] = "Horno / estufa",
            ["Cooktop"] = "Parrilla / cubierta",
            ["Microwave"] = "Microondas",
            ["Washer"] = "Lavadora",
            ["Dryer"] = "Secadora",
            ["Garbage Disposal"] = "Triturador de basura",
            ["Ice Maker"] = "Máquina de hielo",
            ["Trash Compactor"] = "Compactador de basura",
            ["Other Small Appliances"] = "Otros electrodomésticos pequeños",

            // Pest control
            ["General Pest Control"] = "Control general de plagas",
            ["Ant Control"] = "Control de hormigas",
            ["Roach Control"] = "Control de cucarachas",
            ["Rodent Control"] = "Control de roedores",
            ["Termite Inspection"] = "Inspección de termitas",
            ["Termite Treatment"] = "Tratamiento de termitas",
            ["Mosquito Treatment"] = "Tratamiento de mosquitos",
            ["Bed Bug Service"] = "Servicio de chinches",
            ["Wasp / Bee Removal"] = "Remoción de avispas / abejas",
            ["Preventive Maintenance"] = "Mantenimiento preventivo",

            // Landscaping
            ["Lawn mowing"] = "Corte de césped",
            ["Lawn maintenance"] = "Mantenimiento de césped",
            ["Mulching"] = "Acolchado",
            ["Planting"] = "Plantación",
            ["Tree trimming"] = "Poda de árboles",
            ["Hedge trimming"] = "Poda de setos",
            ["Sod installation"] = "Instalación de césped en rollo",
            ["Irrigation"] = "Riego",
            ["Seasonal cleanup"] = "Limpieza de temporada",
            ["Hardscape maintenance"] = "Mantenimiento de superficies duras",

            // Cleaning
            ["Standard Cleaning"] = "Limpieza estándar",
            ["Deep Cleaning"] = "Limpieza profunda",
            ["Move-In / Move-Out"] = "Mudanza de entrada / salida",
            ["Post-Construction Cleaning"] = "Limpieza postconstrucción",
            ["Office Cleaning"] = "Limpieza de oficinas",
            ["Airbnb Turnover"] = "Rotación Airbnb",
            ["Recurring Cleaning"] = "Limpieza recurrente",
            ["Window Cleaning"] = "Limpieza de ventanas",

            // Flooring
            ["Hardwood Installation"] = "Instalación de madera",
            ["Laminate"] = "Laminado",
            ["Vinyl / LVP"] = "Vinilo / LVP",
            ["Tile Flooring"] = "Piso de losa",
            ["Carpet Installation"] = "Instalación de alfombra",
            ["Floor Repair"] = "Reparación de pisos",
            ["Subfloor Repair"] = "Reparación de subpiso",
            ["Refinishing"] = "Acabado de pisos",

            // Painting
            ["Interior Painting"] = "Pintura interior",
            ["Exterior Painting"] = "Pintura exterior",
            ["Cabinet Painting"] = "Pintura de gabinetes",
            ["Drywall Prep & Patching"] = "Preparación y parches de tablaroca",
            ["Trim & Doors"] = "Molduras y puertas",
            ["Deck & Fence Staining"] = "Tinción de decks y cercas",
            ["Wallpaper Removal"] = "Remoción de papel tapiz",
            ["Pressure Washing Prep"] = "Preparación con hidrolavado",

            // Roofing
            ["Shingle roof replacement"] = "Reemplazo de techo de tejas",
            ["Roof repairs"] = "Reparaciones de techo",
            ["Metal roofing"] = "Techo metálico",
            ["Flat roofing"] = "Techo plano",
            ["Leak detection"] = "Detección de fugas",
            ["Flashing & ventilation"] = "Tapajuntas y ventilación",
            ["Gutter installation"] = "Instalación de canaletas",
            ["Emergency tarp service"] = "Servicio de lona de emergencia",

            // Kitchen
            ["Full kitchen remodel"] = "Remodelación completa de cocina",
            ["Cabinet installation"] = "Instalación de gabinetes",
            ["Cabinet replacement"] = "Reemplazo de gabinetes",
            ["Countertop installation"] = "Instalación de mesadas",
            ["Backsplash installation"] = "Instalación de salpicadero",
            ["Painting & finish work"] = "Pintura y acabados",
            ["Appliance installation"] = "Instalación de electrodomésticos",
            ["Sink & faucet replacement"] = "Reemplazo de fregadero y grifo",
            ["Lighting & fixture coordination"] = "Coordinación de iluminación y accesorios",
            ["Demolition"] = "Demolición",
            ["Trim & finish carpentry"] = "Carpintería de molduras y acabados",

            // Bathroom (screenshot examples)
            ["Full Bathroom Renovation"] = "Renovación completa de baño",
            ["Shower / Tub Installation"] = "Instalación de ducha / bañera",
            ["Vanity Installation"] = "Instalación de tocador",
            ["Tile & Flooring"] = "Losa y pisos",
            ["Toilet Installation"] = "Instalación de inodoro",
            ["Fixture Replacement"] = "Reemplazo de accesorios",
            ["Waterproofing"] = "Impermeabilización",
            ["Drywall & Paint"] = "Tablaroca y pintura",
            ["Accessibility Upgrades"] = "Mejoras de accesibilidad",
            ["Glass Door Installation"] = "Instalación de puerta de vidrio",

            // Construction (screenshot examples)
            ["Home additions"] = "Ampliaciones de vivienda",
            ["Full home remodels"] = "Remodelaciones completas de vivienda",
            ["Structural framing"] = "Armado estructural",
            ["Drywall"] = "Tablaroca",
            ["Concrete work"] = "Trabajo de concreto",
            ["Finish carpentry"] = "Carpintería de acabados",
            ["Decks & porches"] = "Decks y porches",
            ["Exterior renovations"] = "Renovaciones exteriores",
            ["Project management"] = "Gestión de proyectos",

            // Handyman (screenshot examples)
            ["Drywall patch & repair"] = "Parche y reparación de tablaroca",
            ["Door adjustments"] = "Ajustes de puertas",
            ["TV / picture mounting"] = "Montaje de TV / cuadros",
            ["Shelving installation"] = "Instalación de estanterías",
            ["Furniture assembly"] = "Ensamblaje de muebles",
            ["Furniture Assembly"] = "Ensamblaje de muebles",
            ["Hardware replacement"] = "Reemplazo de herrajes",
            ["Caulking & sealing"] = "Calafateo y sellado",
            ["Minor punch-list repairs"] = "Reparaciones menores de lista de pendientes",

            // HVAC
            ["AC Repair"] = "Reparación de A/C",
            ["AC Installation"] = "Instalación de A/C",
            ["Heating Repair"] = "Reparación de calefacción",
            ["Heat Pump Service"] = "Servicio de bomba de calor",
            ["Ductwork"] = "Ductos",
            ["Thermostat"] = "Termostato",
            ["Indoor Air Quality"] = "Calidad del aire interior",
            ["Mini-Split"] = "Mini-split",
            ["Commercial HVAC"] = "HVAC comercial",

            // Electrical disallowed / allowed chip labels (registration)
            ["Electrical repairs"] = "Reparaciones eléctricas",
            ["Panel work"] = "Trabajo de panel",
            ["Outlets & switches"] = "Tomacorrientes e interruptores",
            ["Lighting installation"] = "Instalación de iluminación",
            ["Plumbing jobs"] = "Trabajos de plomería",
            ["HVAC jobs"] = "Trabajos de HVAC",
            ["Roofing jobs"] = "Trabajos de techado",
            ["General handyman jobs"] = "Trabajos generales de manitas",
        };
}
