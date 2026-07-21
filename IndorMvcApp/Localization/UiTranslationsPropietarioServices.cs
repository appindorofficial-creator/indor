namespace IndorMvcApp.Localization;

/// <summary>Homeowner service flows: Safe Air, Lawn, Trash, Cleaning Pro, Home Care Guide.</summary>
public static class UiTranslationsPropietarioServices
{
    public static IEnumerable<KeyValuePair<string, string>> Entries =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Bug 7-8 — Home
            ["Feed"] = "Actividad",
            ["Feed, neighbors, local help"] = "Actividad, vecinos, ayuda local",
            ["Certified Providers"] = "Proveedores certificados",
            ["Home Care Essentials"] = "Esenciales del cuidado del hogar",
            ["Homes for sale"] = "Hogares en venta",
            ["View all nearby"] = "Ver todo cerca",
            ["Nearby promotions"] = "Promociones cercanas",
            ["Request a Service"] = "Solicitar un servicio",
            ["My Requests"] = "Mis solicitudes",

            // Safe Air 365 — landing
            ["Change it yourself or let INDOR handle it."] = "Cámbialo tú mismo o deja que INDOR lo haga.",
            ["Never forget a filter change again. We help you track sizes, send reminders, and book replacements when you need them."] =
                "Nunca olvides cambiar el filtro. Te ayudamos a registrar tamaños, enviar recordatorios y reservar reemplazos cuando los necesites.",
            ["Filter reminders"] = "Recordatorios de filtro",
            ["Size tracking"] = "Seguimiento de tamaño",
            ["DIY or pro service"] = "Hazlo tú o servicio profesional",
            ["Basic airflow check"] = "Revisión básica de flujo de aire",
            ["From $49 for provider replacement"] = "Desde $49 por reemplazo con proveedor",
            ["Don't know your filter size? Add a photo or choose \"I don't know\" and your provider can verify it."] =
                "¿No conoces el tamaño del filtro? Agrega una foto o elige \"No lo sé\" y tu proveedor puede verificarlo.",
            ["Schedule with INDOR"] = "Programar con INDOR",
            ["I changed it myself"] = "Lo cambié yo mismo",

            // Safe Air 365 — form & schedule
            ["Filter details"] = "Detalles del filtro",
            ["Help us identify the right filter for your home."] = "Ayúdanos a identificar el filtro correcto para tu hogar.",
            ["What do you need today?"] = "¿Qué necesitas hoy?",
            ["INDOR replaces it"] = "INDOR lo reemplaza",
            ["Just remind me"] = "Solo recuérdamelo",
            ["How many filters?"] = "¿Cuántos filtros?",
            ["Do you know the filter size?"] = "¿Conoces el tamaño del filtro?",
            ["Width (in)"] = "Ancho (pulg.)",
            ["Height (in)"] = "Alto (pulg.)",
            ["Depth (in)"] = "Profundidad (pulg.)",
            ["x"] = "x",
            ["I don't know"] = "No lo sé",
            ["Enter size manually"] = "Ingresar tamaño manualmente",
            ["Where is the filter located?"] = "¿Dónde está ubicado el filtro?",
            ["Ceiling"] = "Techo",
            ["Wall return"] = "Retorno en pared",
            ["HVAC unit"] = "Unidad HVAC",
            ["Attic"] = "Ático",
            ["Not sure"] = "No estoy seguro",
            ["Who provides the filter?"] = "¿Quién proporciona el filtro?",
            ["INDOR brings it"] = "INDOR lo trae",
            ["I have the filter"] = "Yo tengo el filtro",
            ["Remind me every 3 months"] = "Recuérdame cada 3 meses",
            ["We'll send reminders when it's time."] = "Te enviaremos recordatorios cuando sea el momento.",
            ["Photos & schedule"] = "Fotos y programación",
            ["Add context so the provider arrives prepared."] = "Agrega contexto para que el proveedor llegue preparado.",
            ["Add a photo of the filter or vent"] = "Agrega una foto del filtro o de la rejilla",
            ["Upload photos from gallery"] = "Subir fotos de la galería",
            ["Upload photo"] = "Subir foto",
            ["Tap to add (JPG, PNG, HEIC)"] = "Toca para agregar (JPG, PNG, HEIC)",
            ["Take photo"] = "Tomar foto",
            ["Good example"] = "Buen ejemplo",
            ["If you don't know the filter size, a photo helps us verify it."] = "Si no conoces el tamaño del filtro, una foto nos ayuda a verificarlo.",
            ["Preferred time"] = "Horario preferido",
            ["Next available"] = "Próximo disponible",
            ["Morning"] = "Mañana",
            ["Afternoon"] = "Tarde",
            ["Midday"] = "Mediodía",
            ["Evening"] = "Noche",
            ["Flexible"] = "Flexible",
            ["Access details"] = "Detalles de acceso",
            ["House"] = "Casa",
            ["Apartment"] = "Apartamento",
            ["Attic access"] = "Acceso al ático",
            ["Gate code"] = "Código de portón",
            ["Add any access notes (optional)"] = "Agrega notas de acceso (opcional)",
            ["Service:"] = "Servicio:",
            ["Filters:"] = "Filtros:",
            ["Size:"] = "Tamaño:",
            ["Provider brings filter:"] = "Proveedor trae el filtro:",
            ["Reminder:"] = "Recordatorio:",
            ["Confirm request"] = "Confirmar solicitud",
            ["You're all set"] = "Todo listo",
            ["Your {0} request has been saved."] = "Tu solicitud de {0} ha sido guardada.",
            ["Visit"] = "Visita",
            ["Filter size"] = "Tamaño del filtro",
            ["Provider"] = "Proveedor",
            ["Status"] = "Estado",
            ["Confirmed"] = "Confirmado",
            ["Next reminder scheduled in 3 months"] = "Próximo recordatorio programado en 3 meses",
            ["We'll remind you again around {0}."] = "Te recordaremos de nuevo alrededor del {0}.",
            ["Reminder active"] = "Recordatorio activo",
            ["Photo received"] = "Foto recibida",
            ["Filter tracked"] = "Filtro registrado",
            ["If you selected \"I changed it myself,\" your reminder has been updated."] = "Si seleccionaste \"Lo cambié yo mismo\", tu recordatorio ha sido actualizado.",
            ["View appointment"] = "Ver cita",
            ["Mark as changed"] = "Marcar como cambiado",
            ["Go to My Home"] = "Ir a Mi hogar",
            ["Request Confirmed - INDOR"] = "Solicitud confirmada - INDOR",
            ["Booking Confirmed"] = "Reserva confirmada",
            ["Please enter a number."] = "Ingresa un número.",
            ["Enter a realistic size in inches (e.g. 20 x 25 x 1). Each side must be between 0.25 and 99.99."] =
                "Ingresa un tamaño realista en pulgadas (ej. 20 x 25 x 1). Cada lado debe estar entre 0.25 y 99.99.",

            // Safe Air display labels
            ["No visit scheduled"] = "Sin visita programada",
            ["Tomorrow, 10:00–12:00"] = "Mañana, 10:00–12:00",
            ["Tomorrow, 8:00–12:00"] = "Mañana, 8:00–12:00",
            ["Tomorrow, 12:00–4:00 PM"] = "Mañana, 12:00–4:00 p. m.",
            ["Flexible window"] = "Ventana flexible",
            ["To be confirmed"] = "Por confirmar",
            ["To be verified from photo"] = "Por verificar con foto",
            ["Every 3 months"] = "Cada 3 meses",
            ["Off"] = "Desactivado",
            ["INDOR partner"] = "Socio INDOR",
            ["Self-service"] = "Autoservicio",

            // Lawn — landing & setup
            ["What's included"] = "Qué incluye",
            ["Starting at"] = "Desde",
            ["Automatic reminder"] = "Recordatorio automático",
            ["Remind me every 15 days to mow the lawn. We will send you a notification to schedule or repeat the service."] =
                "Recuérdame cada 15 días cortar el césped. Te enviaremos una notificación para programar o repetir el servicio.",
            ["Only remind me"] = "Solo recuérdamelo",
            ["Customize service"] = "Personalizar servicio",
            ["Reminder-only mode. You can still book the service later from your notification."] =
                "Modo solo recordatorio. Aún puedes reservar el servicio más adelante desde tu notificación.",
            ["How do you want it?"] = "¿Cómo lo quieres?",
            ["You can activate it as a recurring service or just as a reminder."] = "Puedes activarlo como servicio recurrente o solo como recordatorio.",
            ["Area to service"] = "Área a atender",
            ["Optional extras"] = "Extras opcionales",
            ["Estimated total"] = "Total estimado",
            ["Once"] = "Una vez",
            ["Every 15 days"] = "Cada 15 días",
            ["Monthly"] = "Mensual",
            ["Front"] = "Frente",
            ["Backyard"] = "Patio trasero",
            ["Front + Backyard"] = "Frente + patio trasero",
            ["Side yard"] = "Patio lateral",
            ["Full property"] = "Propiedad completa",
            ["Edging / borders"] = "Bordes / contornos",
            ["Bush trimming"] = "Poda de arbustos",
            ["No thanks"] = "No, gracias",
            ["One-time service"] = "Servicio único",
            ["None"] = "Ninguno",
            ["8–11 AM"] = "8–11 a. m.",
            ["11 AM–2 PM"] = "11 a. m.–2 p. m.",
            ["2–5 PM"] = "2–5 p. m.",
            ["5–8 PM"] = "5–8 p. m.",
            ["1 day before"] = "1 día antes",
            ["2 days before"] = "2 días antes",
            ["3 days before"] = "3 días antes",
            ["{0} days before"] = "{0} días antes",
            ["Reminder channel"] = "Canal de recordatorio",
            ["FullService"] = "Servicio completo",
            ["ReminderOnly"] = "Solo recordatorio",
            ["Morning 8–11"] = "Mañana 8–11",
            ["Midday 11–2"] = "Mediodía 11–2",
            ["Afternoon 2–5"] = "Tarde 2–5",
            ["Push"] = "Notificación",
            ["SMS"] = "SMS",
            ["Email"] = "Correo electrónico",

            // Trash — landing & setup
            ["Trash"] = "Basura",
            ["Recycle"] = "Reciclaje",
            ["Yard waste"] = "Residuos de jardín",
            ["Trash Day Assistant"] = "Asistente de día de basura",
            ["Never forget collection day again."] = "Nunca olvides el día de recolección.",
            ["Forget fines, bad odors, or missed pickups. We make sure your trash is ready on the right day and return bins to their place."] =
                "Olvídate de multas, malos olores o recolecciones perdidas. Nos aseguramos de que tu basura esté lista el día correcto y devolvemos los contenedores a su lugar.",
            ["Place bins out on pickup day"] = "Sacar contenedores el día de recolección",
            ["Return bins to place"] = "Devolver contenedores a su lugar",
            ["Punctual, reliable service"] = "Servicio puntual y confiable",
            ["Activate service"] = "Activar servicio",
            ["From ${0} /mo"] = "Desde ${0} /mes",

            // Cleaning Pro — common
            ["Customize your cleaning"] = "Personaliza tu limpieza",
            ["How many hours?"] = "¿Cuántas horas?",
            ["Select add-ons"] = "Selecciona extras",
            ["Review your cleaning"] = "Revisa tu limpieza",

            // Home Care Guide — HVAC & shared landing
            ["Includes"] = "Incluye",
            ["What we'll ask"] = "Lo que preguntaremos",
            ["HVAC Tune-Up"] = "Mantenimiento HVAC",
            ["Yearly preventive maintenance to keep your air conditioning system running efficiently and reliably."] =
                "Mantenimiento preventivo anual para mantener tu sistema de aire acondicionado funcionando de forma eficiente y confiable.",
            ["System inspection"] = "Inspección del sistema",
            ["Filter check"] = "Revisión del filtro",
            ["Performance test"] = "Prueba de rendimiento",
            ["Basic tune-up"] = "Ajuste básico",
            ["AC serial number"] = "Número de serie del AC",
            ["Last maintenance date (if known)"] = "Última fecha de mantenimiento (si se conoce)",
            ["Preferred visit time"] = "Horario de visita preferido",
            ["Start tune-up request"] = "Iniciar solicitud de mantenimiento",
            ["Go to Home Care Guide"] = "Ir a la guía de cuidado del hogar",
            ["Back to Home Care Guide"] = "Volver a la guía de cuidado del hogar",

            // Water heater flush
            ["Water Heater Flush"] = "Lavado de calentador de agua",
            ["Annual flush to reduce sediment buildup and extend the life of your water heater."] =
                "Lavado anual para reducir la acumulación de sedimentos y prolongar la vida de tu calentador de agua.",

            // Crawlspace
            ["Crawlspace Check"] = "Revisión de espacio bajo el piso",
            ["Moisture, insulation, and structural checks in your crawlspace."] =
                "Revisión de humedad, aislamiento y estructura en tu espacio bajo el piso.",

            // Roof inspection
            ["Roof Inspection"] = "Inspección de techo",
            ["Professional roof inspection to catch issues before they become costly repairs."] =
                "Inspección profesional de techo para detectar problemas antes de que se vuelvan reparaciones costosas.",

            // Gutter cleaning
            ["Gutter Cleaning"] = "Limpieza de canaletas",
            ["Clear debris from gutters and downspouts to protect your home from water damage."] =
                "Elimina residuos de canaletas y bajantes para proteger tu hogar de daños por agua.",

            // Pest control
            ["Pest Control"] = "Control de plagas",
            ["Protect your home from common pests with scheduled preventive treatments."] =
                "Protege tu hogar de plagas comunes con tratamientos preventivos programados.",

            // Smoke detector
            ["Smoke Detector Check"] = "Revisión de detector de humo",
            ["Test alarms, replace batteries, and keep your family safe."] =
                "Prueba alarmas, cambia baterías y mantén segura a tu familia.",

            // Power wash
            ["Power Wash Exterior"] = "Lavado a presión exterior",
            ["Restore curb appeal with professional exterior power washing."] =
                "Recupera el aspecto exterior con lavado a presión profesional.",

            // Shared form patterns
            ["Continue"] = "Continuar",
            ["Back"] = "Atrás",
            ["Review"] = "Revisar",
            ["Confirm"] = "Confirmar",
            ["Submit request"] = "Enviar solicitud",
            ["Schedule service"] = "Agendar servicio",
            ["One-time"] = "Una vez",
            ["Annual"] = "Anual",
            ["Yes"] = "Sí",
            ["No"] = "No",
            ["or"] = "o",

            // Lawn confirmed
            ["Reminder activated - INDOR"] = "Recordatorio activado - INDOR",
            ["Service scheduled - INDOR"] = "Servicio programado - INDOR",
            ["Your reminder is active!"] = "¡Tu recordatorio está activo!",
            ["Your service is scheduled!"] = "¡Tu servicio está programado!",
            ["We also activated your {0} reminder."] = "También activamos tu recordatorio {0}.",

            // Cleaning Pro confirmed
            ["Your {0} service is all set."] = "Tu servicio de {0} está listo.",

            // DisplayLabels — inspections (+ ElectricalDetails chips)
            ["Buying a home"] = "Comprar una vivienda",
            ["Safety check"] = "Revisión de seguridad",
            ["Issue at home"] = "Problema en el hogar",
            ["Inspection follow-up"] = "Seguimiento de inspección",
            ["Breaker trips"] = "Se dispara el breaker",
            ["Lights flicker"] = "Luces parpadean",
            ["Outlets not working"] = "Tomas sin funcionar",
            ["Old panel"] = "Panel antiguo",
            ["Burning smell"] = "Olor a quemado",
            ["General review"] = "Revisión general",
            ["General electrical review"] = "Revisión eléctrica general",
            ["Understand repair risks"] = "Entender riesgos de reparación",
            ["Home purchase review"] = "Revisión de compra de vivienda",
            ["Water actively flowing"] = "Agua fluyendo activamente",
            ["Water not actively flowing"] = "Agua no fluye activamente",
            ["Water flow unknown"] = "Flujo de agua desconocido",
            ["Water status unknown"] = "Estado del agua desconocido",
            ["Can shut off water"] = "Puede cerrar el agua",
            ["Unable to shut off water"] = "No puede cerrar el agua",
            ["Needs help shutting off water"] = "Necesita ayuda para cerrar el agua",
            ["Shutoff status unknown"] = "Estado de cierre desconocido",
            ["Yes, enter if not home"] = "Sí, entrar si no estoy",
            ["No, do not enter"] = "No, no entrar",
            ["Call first before entering"] = "Llamar primero antes de entrar",
            ["Not specified"] = "No especificado",
            ["1 photo"] = "1 foto",
            ["{0} photos"] = "{0} fotos",
            ["1 attached"] = "1 adjunto",
            ["{0} attached"] = "{0} adjuntos",

            // DisplayLabels — lawn/trash
            ["Reminder only"] = "Solo recordatorio",
            ["1 bin"] = "1 contenedor",
            ["2 bins"] = "2 contenedores",
            ["3 bins"] = "3 contenedores",
            ["Sunday"] = "Domingo",
            ["Monday"] = "Lunes",
            ["Tuesday"] = "Martes",
            ["Wednesday"] = "Miércoles",
            ["Thursday"] = "Jueves",
            ["Friday"] = "Viernes",
            ["Saturday"] = "Sábado",

            // Misc inspection gaps
            ["Request confirmed"] = "Solicitud confirmada",
            ["Weekly"] = "Semanal",
        };
}
