namespace IndorMvcApp.Localization;

/// <summary>Homeowner ↔ Provider service request marketplace (ServiceRequest views,
/// provider available-requests views, toasts and notifications).</summary>
public static class UiTranslationsServiceRequest
{
    public static IEnumerable<KeyValuePair<string, string>> Entries =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // ----- Layout / titles -----
            ["Service Requests"] = "Solicitudes de servicio",
            ["Request a Service"] = "Solicitar un servicio",
            ["My Requests"] = "Mis solicitudes",
            ["Request Details"] = "Detalles de la solicitud",
            ["Available Requests - INDOR PRO"] = "Solicitudes disponibles - INDOR PRO",
            ["Request Details - INDOR PRO"] = "Detalles de la solicitud - INDOR PRO",

            // ----- Homeowner: Create -----
            ["Request a service"] = "Solicitar un servicio",
            ["Tell us what you need. We'll instantly notify verified INDOR providers in that trade — the first one to accept will contact you."] =
                "Cuéntanos qué necesitas. Notificaremos al instante a proveedores INDOR verificados de ese oficio — el primero en aceptar te contactará.",
            ["What do you need?"] = "¿Qué necesitas?",
            ["Pick a category"] = "Elige una categoría",
            ["e.g. Leaking kitchen faucet"] = "ej. Fuga en el grifo de la cocina",
            ["Describe the job"] = "Describe el trabajo",
            ["Add any details that help the provider prepare..."] = "Agrega detalles que ayuden al proveedor a prepararse...",
            ["Property"] = "Propiedad",
            ["No specific property"] = "Sin propiedad específica",
            ["Service address"] = "Dirección del servicio",
            ["Where should the provider go?"] = "¿A dónde debe ir el proveedor?",
            ["Preferred date"] = "Fecha preferida",
            ["Preferred time"] = "Hora preferida",
            ["e.g. Morning"] = "ej. Mañana",
            ["Budget (USD)"] = "Presupuesto (USD)",
            ["e.g. 150"] = "ej. 150",
            ["Your phone number"] = "Tu número de teléfono",
            ["Urgency"] = "Urgencia",
            ["Standard"] = "Estándar",
            ["Urgent"] = "Urgente",
            ["Emergency"] = "Emergencia",
            ["Post request"] = "Publicar solicitud",

            // ----- Homeowner: Mine -----
            ["My service requests"] = "Mis solicitudes de servicio",
            ["Track requests you've posted and see which provider accepted them."] =
                "Sigue las solicitudes que publicaste y ve qué proveedor las aceptó.",
            ["New request"] = "Nueva solicitud",
            ["No requests yet"] = "Aún no hay solicitudes",
            ["Post your first service request and matching providers will be notified right away."] =
                "Publica tu primera solicitud y los proveedores compatibles serán notificados de inmediato.",
            ["Waiting for a provider"] = "Esperando un proveedor",
            ["Accepted"] = "Aceptada",
            ["Closed"] = "Cerradas",
            ["Waiting"] = "Esperando",
            ["Cancelled"] = "Cancelada",
            ["Completed"] = "Completada",

            // ----- Homeowner: Detail -----
            ["Licensed"] = "Con licencia",
            ["Insured"] = "Asegurado",
            ["Waiting for a provider to accept. We'll notify you the moment one takes it."] =
                "Esperando que un proveedor acepte. Te avisaremos en cuanto alguno la tome.",
            ["Request details"] = "Detalles de la solicitud",
            ["When"] = "Cuándo",
            ["Cancel this request?"] = "¿Cancelar esta solicitud?",
            ["Cancel request"] = "Cancelar solicitud",

            // ----- Provider: Available Requests -----
            ["Available Requests"] = "Solicitudes disponibles",
            ["{0} open request(s) in your trades"] = "{0} solicitud(es) abierta(s) en tus oficios",
            ["Add your trades first"] = "Agrega tus oficios primero",
            ["Select the service categories you offer in your profile so we can match you with homeowner requests."] =
                "Selecciona en tu perfil las categorías de servicio que ofreces para poder emparejarte con solicitudes de propietarios.",
            ["Go to profile"] = "Ir al perfil",
            ["No open requests right now"] = "No hay solicitudes abiertas ahora",
            ["When a homeowner requests a service in one of your trades, it will appear here and we'll notify you by email."] =
                "Cuando un propietario solicite un servicio en uno de tus oficios, aparecerá aquí y te avisaremos por correo.",

            // ----- Provider: Request Details -----
            ["Job details"] = "Detalles del trabajo",
            ["You took this request. Contact the homeowner to coordinate."] =
                "Tomaste esta solicitud. Contacta al propietario para coordinar.",
            ["Another provider already took this request."] = "Otro proveedor ya tomó esta solicitud.",
            ["Homeowner contact"] = "Contacto del propietario",
            ["Contact details were not provided."] = "No se proporcionaron datos de contacto.",
            ["Take this request"] = "Tomar esta solicitud",
            ["First provider to take it wins the job."] = "El primer proveedor en tomarla se queda con el trabajo.",

            // ----- Entry points -----
            ["My requests"] = "Mis solicitudes",

            // ----- Toasts (controllers) -----
            ["Please add a short title for your request."] = "Agrega un título breve para tu solicitud.",
            ["Please choose a service category."] = "Elige una categoría de servicio.",
            ["Your request was posted. We notified matching providers."] =
                "Tu solicitud fue publicada. Notificamos a los proveedores compatibles.",
            ["Request cancelled."] = "Solicitud cancelada.",
            ["That request can no longer be cancelled."] = "Esa solicitud ya no se puede cancelar.",
            ["That request is no longer available."] = "Esa solicitud ya no está disponible.",
            ["You took this request. The homeowner has been notified with your details."] =
                "Tomaste esta solicitud. El propietario fue notificado con tus datos.",
            ["This request is outside your service categories."] = "Esta solicitud está fuera de tus categorías de servicio.",

            // ----- Persisted notification titles/bodies (bell) -----
            ["New service request"] = "Nueva solicitud de servicio",
            ["Your request was accepted"] = "Tu solicitud fue aceptada",
        };
}
