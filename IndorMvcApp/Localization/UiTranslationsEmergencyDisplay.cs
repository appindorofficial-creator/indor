namespace IndorMvcApp.Localization;

/// <summary>
/// Presentation translations shared by emergency review and confirmation flows.
/// Internal emergency codes and persisted catalog values remain in English.
/// </summary>
public static class UiTranslationsEmergencyDisplay
{
    public static IEnumerable<KeyValuePair<string, string>> Entries =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Flood Emergency"] = "Emergencia por inundación",
            ["Water Heater Emergency"] = "Emergencia de calentador de agua",
            ["HVAC Emergency"] = "Emergencia de HVAC",
            ["Plumbing Emergency"] = "Emergencia de plomería",
            ["Electrical Emergency"] = "Emergencia eléctrica",
            ["Roof Leak Emergency"] = "Emergencia por fuga de techo",
            ["Tree Damage Emergency"] = "Emergencia por daños de árbol",
            ["Smoke Detector Emergency"] = "Emergencia de detectores de humo",
            ["Smoke Detector & CO Alert"] = "Alerta de detector de humo y CO",
            ["Plumbing emergency"] = "Emergencia de plomería",
            ["HVAC issue"] = "Problema de HVAC",
            ["Water heater issue"] = "Problema del calentador de agua",
            ["Electrical issue"] = "Problema eléctrico",
            ["Tree damage"] = "Daño de árbol",
            ["Roof leak"] = "Fuga de techo",

            ["Unknown / being investigated"] = "Desconocido / en investigación",
            ["Searching for provider"] = "Buscando proveedor",
            ["Happening now"] = "Ocurriendo ahora",
            ["Not happening now"] = "No está ocurriendo ahora",
            ["Status unknown"] = "Estado desconocido",
            ["No photos attached"] = "No hay fotos adjuntas",
            ["No files uploaded"] = "No se subieron archivos",
            ["None uploaded"] = "Ninguna subida",
            ["None provided"] = "No se proporcionó información",
            ["1 attached"] = "1 archivo adjunto",
            ["{0} attached"] = "{0} archivos adjuntos",
            ["1 uploaded"] = "1 archivo subido",
            ["{0} uploaded"] = "{0} archivos subidos",
            ["1 photo"] = "1 foto",
            ["{0} photos"] = "{0} fotos",

            ["Water actively flowing"] = "El agua fluye activamente",
            ["Water not actively flowing"] = "El agua no fluye activamente",
            ["Water flow unknown"] = "Flujo de agua desconocido",
            ["Water status unknown"] = "Estado del agua desconocido",
            ["Can shut off water"] = "Puede cerrar el agua",
            ["Unable to shut off water"] = "No puede cerrar el agua",
            ["Needs help shutting off water"] = "Necesita ayuda para cerrar el agua",
            ["Shutoff status unknown"] = "Estado del cierre desconocido",
            ["Yes, enter if not home"] = "Sí, entrar si no hay nadie",
            ["No, do not enter"] = "No, no entrar",
            ["Call first before entering"] = "Llamar antes de entrar",
            ["active leak"] = "fuga activa",

            ["Review pending"] = "Revisión pendiente",
            ["Photos pending"] = "Fotos pendientes",
            ["Callback pending"] = "Llamada pendiente",
            ["Location pending"] = "Ubicación pendiente",
            ["Next step pending"] = "Siguiente paso pendiente",
            ["Submit pending"] = "Envío pendiente",
            ["Details pending"] = "Detalles pendientes",
            ["Dispatching"] = "Despachando proveedor",
            ["Provider search in progress"] = "Búsqueda de proveedor en curso",
            ["Confirmed"] = "Confirmado",

            ["Unknown area"] = "Área desconocida",
            ["Water still active"] = "El agua sigue activa",
            ["Water not active"] = "El agua ya no está activa",
            ["Active status unknown"] = "Estado de actividad desconocido",
            ["Inside home"] = "Dentro de la vivienda",
            ["Can turn off power"] = "Puede apagar la electricidad",
            ["Cannot turn off power"] = "No puede apagar la electricidad",
            ["Needs help with power"] = "Necesita ayuda con la electricidad",
            ["panel"] = "panel",

            ["Full"] = "Completo",
            ["Partial"] = "Parcial",
            ["No access"] = "Sin acceso",
            ["blocked driveway"] = "entrada bloqueada",
            ["driveway"] = "entrada vehicular",

            ["Yes, it is safe to stay inside"] = "Sí, es seguro permanecer adentro",
            ["No, it is not safe to stay inside"] = "No, no es seguro permanecer adentro",
            ["Not sure it is safe to stay inside"] = "No sabe si es seguro permanecer adentro",
            ["Adult home now"] = "Hay un adulto en casa",
            ["Children home now"] = "Hay niños en casa",
            ["No one home"] = "No hay nadie en casa",
            ["Smoke alarm / CO safety concern"] = "Riesgo de seguridad por alarma de humo / CO",
            ["Smoke alarm safety concern"] = "Riesgo de seguridad por alarma de humo",

            ["Heater front label, leak area, pipes, error code, and surrounding water damage."] =
                "Etiqueta frontal del calentador, zona de fuga, tuberías, código de error y agua alrededor."
        };
}
