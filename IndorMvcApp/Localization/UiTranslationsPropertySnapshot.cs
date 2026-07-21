namespace IndorMvcApp.Localization;

/// <summary>
/// Additive Spanish strings for Propietario Property Snapshot (Resumen) screen.
/// </summary>
public static class UiTranslationsPropertySnapshot
{
    public static IEnumerable<KeyValuePair<string, string>> Entries =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Page chrome
            ["Your property at a glance."] = "Tu propiedad de un vistazo.",
            ["Core identity details"] = "Detalles principales de identidad",
            ["Lot, site & exterior basics"] = "Lote, terreno y exterior básico",
            ["Confidence, notes & follow-up"] = "Confianza, notas y seguimiento",
            ["Mostly confirmed"] = "Mayormente confirmado",
            ["Mostly estimated"] = "Mayormente estimado",
            ["Quick sections"] = "Secciones rápidas",
            ["Snapshot highlights"] = "Destacados del resumen",
            ["Next: Details"] = "Siguiente: Detalles",
            ["Next: Lot details"] = "Siguiente: Detalles del lote",
            ["Next: Notes"] = "Siguiente: Notas",

            // Quick section cards
            ["Identity"] = "Identidad",
            ["Ownership & legal info"] = "Información de propiedad y legal",
            ["Home Facts"] = "Datos del hogar",
            ["Key property details"] = "Detalles clave de la propiedad",
            ["Location"] = "Ubicación",
            ["Address & neighborhood"] = "Dirección y vecindario",
            ["Verification"] = "Verificación",
            ["Sources & confidence"] = "Fuentes y confianza",

            // Core stat / field labels
            ["Interior Living Area"] = "Área habitable",
            ["Lot Size"] = "Tamaño del lote",
            ["Year Built"] = "Año de construcción",
            ["City / State / ZIP"] = "Ciudad / Estado / ZIP",
            ["Parcel ID / APN"] = "ID de parcela / APN",
            ["Neighborhood"] = "Vecindario",
            ["Current Status"] = "Estado actual",
            ["Land Use"] = "Uso del suelo",
            ["Jurisdiction"] = "Jurisdicción",
            ["Garage / Parking"] = "Garaje / estacionamiento",
            ["Roof Type"] = "Tipo de techo",
            ["Exterior Material"] = "Material exterior",
            ["Porch / Deck"] = "Porche / terraza",
            ["Flood Zone"] = "Zona de inundación",
            ["Driveway / Drainage"] = "Entrada / drenaje",
            ["Nearby Roads"] = "Calles cercanas",
            ["Not confirmed"] = "No confirmado",
            ["Stories"] = "Pisos",
            ["Foundation"] = "Cimentación",
            ["Address"] = "Dirección",

            // Pattern templates for formatted values
            ["{0} Beds"] = "{0} habitaciones",
            ["{0} Baths"] = "{0} baños",
            ["{0} beds"] = "{0} habitaciones",
            ["{0} baths"] = "{0} baños",
            ["Built {0}"] = "Construido en {0}",
            ["{0} sq ft"] = "{0} pies²",
            ["{0} acres"] = "{0} acres",

            // Notes tab copy / defaults
            ["Estimated"] = "Estimado",
            ["Public listings"] = "Listados públicos",
            ["Web search"] = "Búsqueda web",
            ["Seller disclosure"] = "Declaración del vendedor",
            ["Inspection report"] = "Informe de inspección",
            ["Roof age"] = "Antigüedad del techo",
            ["Permit history"] = "Historial de permisos",
            ["Not recorded"] = "No registrado",
            ["Some details are confirmed from public records. Others are estimated from saved web search data and should be verified."] =
                "Algunos detalles están confirmados por registros públicos. Otros se estiman a partir de datos de búsqueda web guardados y deben verificarse.",
            ["Most snapshot details appear confirmed from public records in your saved House Fact profile."] =
                "La mayoría de los detalles del resumen parecen confirmados por registros públicos en tu perfil House Fact guardado.",
            ["Some details are confirmed from public records. Others are estimated from web sources and should be verified."] =
                "Algunos detalles están confirmados por registros públicos. Otros se estiman a partir de fuentes web y deben verificarse.",
        };
}
