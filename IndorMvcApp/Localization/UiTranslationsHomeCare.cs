namespace IndorMvcApp.Localization;

/// <summary>
/// Home Care Guide (Guía de cuidado del hogar) — 9 propietario service flows (Bug 14).
/// Registered late so these keys override Flows identity / Spanglish winners.
/// </summary>
public static class UiTranslationsHomeCare
{
    public static IEnumerable<KeyValuePair<string, string>> Entries =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Shared wizard chrome
            ["Includes"] = "Incluye",
            ["What we'll ask"] = "Lo que preguntaremos",
            ["Why it matters"] = "Por qué importa",
            ["Continue"] = "Continuar",
            ["From $79"] = "Desde $79",
            ["From $89"] = "Desde $89",
            ["From $99"] = "Desde $99",
            ["From ${0}"] = "Desde ${0}",
            ["Not provided"] = "No proporcionado",
            ["Not sure"] = "No estoy seguro",
            ["I don't know"] = "No lo sé",
            ["Other"] = "Otro",
            ["Yes"] = "Sí",
            ["No"] = "No",
            ["Electric"] = "Eléctrico",
            ["Gas"] = "Gas",
            ["Garage"] = "Garaje",
            ["Basement"] = "Sótano",
            ["Closet"] = "Armario",
            ["Attic"] = "Ático",
            ["Brick"] = "Ladrillo",
            ["Stucco"] = "Estuco",
            ["Driveway"] = "Entrada vehicular",
            ["Patio"] = "Patio",
            ["Fence"] = "Cerca",
            ["Vinyl siding"] = "Revestimiento de vinilo",
            ["Moisture"] = "Humedad",
            ["Encapsulation"] = "Encapsulación",
            ["Insulation"] = "Aislamiento",
            ["Air leaks"] = "Fugas de aire",
            ["Pests"] = "Plagas",
            ["Cracks"] = "Grietas",
            ["Standing water"] = "Agua estancada",
            ["Musty odor"] = "Olor a humedad",
            ["Mold / mildew"] = "Moho / hongos",
            ["Pest signs"] = "Señales de plagas",
            ["Pipe leaks"] = "Fugas de tuberías",
            ["Damaged ducts"] = "Conductos dañados",

            // 1 — HVAC
            ["HVAC Tune-Up"] = "Mantenimiento HVAC",
            ["Recommended yearly"] = "Recomendado cada año",
            ["Yearly preventive maintenance to keep your air conditioning system running efficiently and reliably."] =
                "Mantenimiento preventivo anual para mantener tu sistema de aire acondicionado funcionando de forma eficiente y confiable.",
            ["System inspection"] = "Inspección del sistema",
            ["Filter check"] = "Revisión del filtro",
            ["Performance test"] = "Prueba de rendimiento",
            ["Basic tune-up"] = "Ajuste básico",
            ["AC serial number"] = "Número de serie del AC",
            ["Last maintenance date (if known)"] = "Última fecha de mantenimiento (si se conoce)",
            ["Preferred visit time"] = "Horario de visita preferido",
            ["We'll match you with the right HVAC pro based on your system details."] =
                "Te conectaremos con el profesional de HVAC adecuado según los detalles de tu sistema.",
            ["Start tune-up request"] = "Iniciar solicitud de mantenimiento",
            ["Tell us about your system"] = "Cuéntanos sobre tu sistema",
            ["Tell us about your system - INDOR"] = "Cuéntanos sobre tu sistema - INDOR",
            ["A few details help us prepare the right tune-up."] =
                "Unos detalles nos ayudan a preparar el ajuste adecuado.",
            ["Photo of model / serial label"] = "Foto de la etiqueta del modelo / serie",
            ["Last maintenance"] = "Último mantenimiento",
            ["Last maintenance date"] = "Fecha del último mantenimiento",
            ["I don't know the serial number"] = "No conozco el número de serie",
            ["One-time tune-up"] = "Puesta a punto única",
            ["Yearly reminder enabled"] = "Recordatorio anual activado",
            ["Morning 8–11"] = "Mañana 8–11",
            ["Midday 11–2"] = "Mediodía 11–2",
            ["Afternoon 2–5"] = "Tarde 2–5",

            // 2 — Water heater flush
            ["Water Heater Flush"] = "Lavado de calentador de agua",
            ["Recommended yearly to keep your system clean and efficient."] =
                "Recomendado cada año para mantener tu sistema limpio y eficiente.",
            ["Remove sediment buildup"] = "Eliminar acumulación de sedimento",
            ["Improve efficiency"] = "Mejorar la eficiencia",
            ["Extend tank life"] = "Prolongar la vida del tanque",
            ["Serial number"] = "Número de serie",
            ["Any symptoms"] = "Cualquier síntoma",
            ["Preferred date"] = "Fecha preferida",
            ["Over time, sediment builds up at the bottom of your water heater tank. This can reduce efficiency, cause rumbling noises, and shorten the life of your system."] =
                "Con el tiempo, el sedimento se acumula en el fondo del tanque del calentador. Esto puede reducir la eficiencia, causar ruidos y acortar la vida del sistema.",
            ["Annual flush + basic visual check"] = "Lavado anual + revisión visual básica",
            ["Tell us about your water heater"] = "Cuéntanos sobre tu calentador de agua",
            ["Tell us about your water heater - INDOR"] = "Cuéntanos sobre tu calentador de agua - INDOR",
            ["A few details help us bring the right maintenance plan."] =
                "Unos detalles nos ayudan a preparar el plan de mantenimiento adecuado.",
            ["Heater type"] = "Tipo de calentador",
            ["Tank"] = "Tanque",
            ["Tankless"] = "Sin tanque",
            ["Power source"] = "Fuente de energía",
            ["Location"] = "Ubicación",
            ["Enter serial number"] = "Ingresa el número de serie",
            ["Brand / model (optional)"] = "Marca / modelo (opcional)",
            ["Enter brand or model"] = "Ingresa marca o modelo",
            ["Add a photo of the label"] = "Agrega una foto de la etiqueta",
            ["Tap to take a photo or choose from gallery"] = "Toca para tomar una foto o elegir de la galería",
            ["Within 1 year"] = "En el último año",
            ["1–2 years ago"] = "Hace 1–2 años",
            ["1-2 years ago"] = "Hace 1–2 años",
            ["More than 2 years"] = "Más de 2 años",
            ["Rumbling noise"] = "Ruido de retumbo",
            ["Rusty / cloudy water"] = "Agua oxidada / turbia",
            ["Slow hot water"] = "Agua caliente lenta",
            ["Temperature changes"] = "Cambios de temperatura",
            ["No issues — just maintenance"] = "Sin problemas — solo mantenimiento",
            ["Yearly reminder set"] = "Recordatorio anual configurado",
            ["One-time flush"] = "Lavado único",
            ["This week"] = "Esta semana",
            ["Choose date"] = "Elegir fecha",
            ["Next available"] = "Próxima disponible",
            ["from ${0}"] = "desde ${0}",

            // 3 — Crawlspace
            ["Crawlspace Check"] = "Revisión de espacio bajo el piso",
            ["Inspect moisture, insulation, structure, and air quality before small issues become expensive repairs."] =
                "Revisa humedad, aislamiento, estructura y calidad del aire antes de que problemas pequeños se vuelvan costosos.",
            ["Check yearly and after heavy rain or moisture events."] =
                "Revisa cada año y después de lluvias fuertes o eventos de humedad.",
            ["We'll help inspect moisture, air leaks, insulation, pests, and structural warning signs."] =
                "Te ayudaremos a inspeccionar humedad, fugas de aire, aislamiento, plagas y señales estructurales.",
            ["Start crawlspace check"] = "Iniciar revisión del espacio bajo el piso",

            // 4 — Roof
            ["Roof Inspection"] = "Inspección de techo",
            ["Regular roof inspections help catch loose shingles, failing sealant, damaged flashing, clogged drainage, and leak risks before they become major repairs."] =
                "Las inspecciones regulares de techo ayudan a detectar tejas sueltas, sellador dañado, flashing defectuoso, drenaje obstruido y riesgos de fuga antes de que se vuelvan reparaciones mayores.",
            ["Visual roof check: spring & fall"] = "Revisión visual del techo: primavera y otoño",
            ["Professional inspection: every 1–2 years"] = "Inspección profesional: cada 1–2 años",
            ["After major storms: inspect again"] = "Después de tormentas fuertes: inspecciona de nuevo",
            ["Older roof or active issues: inspect sooner"] = "Techo antiguo o problemas activos: inspecciona antes",
            ["Shingles"] = "Tejas",
            ["Flashing & sealant"] = "Tapajuntas y sellador",
            ["Vents / skylights"] = "Ventilaciones / tragaluces",
            ["Gutters & valleys"] = "Canaletas y valles",
            ["Attic moisture signs"] = "Señales de humedad en el ático",
            ["Debris / branches"] = "Escombros / ramas",
            ["Vetted professionals. Clear reports. Peace of mind."] =
                "Profesionales verificados. Informes claros. Tranquilidad.",
            ["Set roof check"] = "Configurar revisión de techo",
            ["Roofers typically recommend a visual roof check in spring and fall, and a professional inspection every 1–2 years or after major storms."] =
                "Los techadores suelen recomendar una revisión visual en primavera y otoño, y una inspección profesional cada 1–2 años o después de tormentas fuertes.",

            // 5 — Power wash
            ["Power Wash Exterior"] = "Lavado a presión exterior",
            ["Recommended every 1–2 years"] = "Recomendado cada 1–2 años",
            ["This service helps remove dirt, algae, mildew, pollen, and surface buildup from the exterior of your home."] =
                "Este servicio ayuda a quitar suciedad, algas, moho, polen y acumulación de la superficie exterior de tu hogar.",
            ["We'll use your answers to understand your surface type, condition, and access so we can recommend the right approach."] =
                "Usaremos tus respuestas para entender el tipo de superficie, el estado y el acceso y recomendar el enfoque correcto.",
            ["Power washing is commonly recommended every 1–2 years, or sooner if you notice mildew, pollen, or staining."] =
                "El lavado a presión suele recomendarse cada 1–2 años, o antes si notas moho, polen o manchas.",
            ["We use this to choose the safest wash pressure for your home."] =
                "Usamos esto para elegir la presión de lavado más segura para tu hogar.",
            ["Start exterior check"] = "Iniciar revisión exterior",
            ["Full exterior"] = "Exterior completo",

            // 6 — Exterior paint
            ["Exterior Paint Review"] = "Revisión de pintura exterior",
            ["Recommended every 5 years"] = "Recomendado cada 5 años",
            ["Help us understand your exterior so we can schedule the right paint review."] =
                "Ayúdanos a entender tu exterior para programar la revisión de pintura adecuada.",
            ["Paint sooner if you see peeling, fading, or damaged caulk."] =
                "Pinta antes si ves desprendimiento, decoloración o calafateo dañado.",
            ["Fresh exterior paint protects siding and trim"] =
                "La pintura exterior nueva protege el revestimiento y los acabados",
            ["Annual visual checks help catch peeling and bad caulk early"] =
                "Revisiones visuales anuales ayudan a detectar desprendimiento y calafateo dañado a tiempo",
            ["A full repaint is often needed about every 5 years, depending on material and weather"] =
                "Un repintado completo suele necesitarse cada ~5 años, según material y clima",
            ["We'll review your photos"] = "Revisaremos tus fotos",
            ["We'll confirm scope and surface type"] = "Confirmaremos el alcance y el tipo de superficie",
            ["We'll help you plan timing and color options"] =
                "Te ayudaremos a planear plazos y opciones de color",
            ["Check paint condition every year"] = "Revisa el estado de la pintura cada año",
            ["Exterior paint review and planning"] = "Revisión y planificación de pintura exterior",

            // 7 — Gutter cleaning
            ["Gutter Cleaning"] = "Limpieza de canaletas",
            ["Recommended twice a year"] = "Recomendado dos veces al año",
            ["Gutters should be cleaned in the spring and fall to help prevent clogs, overflow, fascia damage, foundation issues, and water intrusion."] =
                "Las canaletas deben limpiarse en primavera y otoño para evitar obstrucciones, desbordes, daño en la fascia, problemas de cimentación e ingreso de agua.",
            ["Prevents overflow"] = "Evita desbordamientos",
            ["Helps protect roof edges"] = "Ayuda a proteger los bordes del techo",
            ["Keeps downspouts clear"] = "Mantiene las bajantes despejadas",
            ["Reduces water around foundation"] = "Reduce el agua alrededor de la cimentación",
            ["We saved your reminder schedule"] = "Guardamos tu calendario de recordatorios",
            ["A pro can review your request"] = "Un profesional puede revisar tu solicitud",
            ["You can update this anytime in My Home"] = "Puedes actualizar esto en cualquier momento en Mi hogar",
            ["Spring cleaning: March – May"] = "Limpieza de primavera: marzo – mayo",
            ["Fall cleaning: September – November"] = "Limpieza de otoño: septiembre – noviembre",
            ["Routine gutter cleaning helps prevent overflow, roof damage, and foundation issues."] =
                "La limpieza rutinaria de canaletas ayuda a prevenir desbordes, daño al techo y problemas de cimentación.",

            // 8 — Pest control
            ["Pest Control Check"] = "Revisión de control de plagas",
            ["Pest Control"] = "Control de plagas",
            ["Recommended yearly to help catch problems early and protect your home."] =
                "Recomendado cada año para detectar problemas a tiempo y proteger tu hogar.",
            ["Spot termites and other pests early"] = "Detecta termitas y otras plagas a tiempo",
            ["Check for moisture, nests, droppings, and entry points"] =
                "Revisa humedad, nidos, excrementos y puntos de entrada",
            ["Help protect wood, insulation, and indoor air quality"] =
                "Ayuda a proteger madera, aislamiento y la calidad del aire interior",
            ["Best for: annual inspections, prevention plans, and homes with past pest activity."] =
                "Ideal para: inspecciones anuales, planes de prevención y hogares con plagas previas.",
            ["Annual checks are most helpful for homes with past pest activity, moisture issues, wood-to-soil contact, or cracks around the home."] =
                "Las revisiones anuales son más útiles en hogares con plagas previas, humedad, contacto madera-suelo o grietas.",
            ["Helps catch termite or rodent issues early"] = "Ayuda a detectar termitas o roedores a tiempo",
            ["Checks for moisture, nests, and entry points"] = "Revisa humedad, nidos y puntos de entrada",
            ["Supports ongoing home protection"] = "Apoya la protección continua del hogar",

            // 9 — Smoke detector
            ["Smoke / CO Check"] = "Revisión de humo / CO",
            ["Smoke Detector Check"] = "Revisión de detector de humo",
            ["Protect your home and the people in it."] = "Protege tu hogar y a quienes viven en él.",
            ["Smoke and carbon monoxide alarms are your first line of defense. Regular checks keep them ready when it matters most."] =
                "Las alarmas de humo y monóxido de carbono son tu primera línea de defensa. Las revisiones regulares las mantienen listas cuando más importa.",
            ["Test monthly"] = "Probar mensualmente",
            ["Battery check yearly"] = "Revisión anual de batería",
            ["Replace alarm every 10 years"] = "Reemplazar alarma cada 10 años",
            ["Press the test button to make sure your alarm is working."] =
                "Presiona el botón de prueba para asegurar que tu alarma funciona.",
            ["Check and replace batteries at least once a year."] =
                "Revisa y cambia las baterías al menos una vez al año.",
            ["Alarms should be replaced 10 years from the install date."] =
                "Las alarmas deben reemplazarse a los 10 años desde la fecha de instalación.",
            ["Bedroom alarms"] = "Alarmas de dormitorio",
            ["Hallway alarms"] = "Alarmas de pasillo",
            ["Living area alarms"] = "Alarmas de áreas comunes",
            ["CO combo units"] = "Unidades combo de CO",
            ["INDOR will remind you when it's time to test, change batteries, or replace older alarms."] =
                "INDOR te recordará cuándo probar, cambiar baterías o reemplazar alarmas antiguas.",
            ["Start reminder setup"] = "Iniciar configuración de recordatorio",
            ["Replace by {0}"] = "Reemplazar antes del {0}",
            ["{0} alarms"] = "{0} alarmas",
            ["Off"] = "Desactivado",
            ["What you'll track"] = "Lo que vas a seguir",
        };
}
