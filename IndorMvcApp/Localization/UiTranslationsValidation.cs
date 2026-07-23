namespace IndorMvcApp.Localization;

/// <summary>DataAnnotations and manual validation error messages (English keys).</summary>
public static class UiTranslationsValidation
{
    public static IEnumerable<KeyValuePair<string, string>> Entries =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Full name is required."] = "El nombre completo es obligatorio.",
            ["Full name must be at least 2 characters."] = "El nombre completo debe tener al menos 2 caracteres.",
            ["Enter a valid full name using letters (e.g. John Smith)."] = "Ingresa un nombre válido con letras (ej. Juan Pérez).",
            ["Full name cannot contain only numbers."] = "El nombre completo no puede contener solo números.",
            ["Enter the client's first and last name."] = "Ingresa el nombre y apellido del cliente.",
            ["First name and last name are required."] = "El nombre y el apellido son obligatorios.",
            ["Email address is required."] = "El correo electrónico es obligatorio.",
            ["Enter a valid email address."] = "Ingresa un correo electrónico válido.",
            ["Enter a valid email address with a complete domain (e.g. name@email.com)."] = "Ingresa un correo válido con un dominio completo (ej. nombre@correo.com).",
            ["Enter a valid guest arrival time (for example, 4:00 PM)."] =
                "Ingresa una hora válida de llegada del huésped (por ejemplo, 4:00 p. m.).",
            ["Phone number is required."] = "El número de teléfono es obligatorio.",
            ["Enter a valid 10-digit US phone number (e.g. 555 123 4567)."] = "Ingresa un número válido de 10 dígitos (EE. UU.) (ej. 555 123 4567).",
            ["Please select a client role."] = "Selecciona un rol de cliente.",
            ["Property address is required."] = "La dirección de la propiedad es obligatoria.",
            ["City is required."] = "La ciudad es obligatoria.",
            ["State is required."] = "El estado es obligatorio.",
            ["ZIP code is required."] = "El código postal es obligatorio.",
            ["Enter a valid 5-digit ZIP code (e.g. 77002)."] = "Ingresa un código postal válido de 5 dígitos (ej. 77002).",
            ["Enter a complete street address."] = "Ingresa una dirección completa.",
            ["Enter a valid street address with a street name."] = "Ingresa una dirección válida con nombre de calle.",
            ["Address cannot contain only numbers."] = "La dirección no puede contener solo números.",
            ["Enter a complete street address (e.g. 123 Main St, Charlotte, NC)."] = "Ingresa una dirección completa (ej. 123 Main St, Charlotte, NC).",
            ["Enter a street number (e.g. 123 Main St)."] = "Ingresa un número de calle (ej. 123 Main St).",
            ["Please enter a valid street address."] = "Ingresa una dirección válida.",
            ["Please enter a valid street address (e.g. 123 Main St, Charlotte, NC)."] =
                "Ingresa una dirección completa (ej. 123 Main St, Charlotte, NC).",
            ["Enter a complete address with city and state (e.g. 123 Main St, Charlotte, NC)."] =
                "Ingresa una dirección completa con ciudad y estado (ej. 123 Main St, Charlotte, NC).",
            ["Please enter the pick-up address."] = "Ingresa la dirección de recogida.",
            ["Please enter the drop-off address."] = "Ingresa la dirección de entrega.",
            ["Please select a move date."] = "Selecciona una fecha de mudanza.",
            ["Please enter a job title."] = "El título del trabajo es obligatorio.",
            ["Please enter a location."] = "La ubicación es obligatoria.",
            ["The Title field is required."] = "El título del trabajo es obligatorio.",
            ["The LocationAddress field is required."] = "La ubicación es obligatoria.",
            ["License number is required."] = "El número de licencia es obligatorio.",
            ["License number must be at least 4 characters."] = "El número de licencia debe tener al menos 4 caracteres.",
            ["License number cannot exceed 20 characters."] = "El número de licencia no puede superar 20 caracteres.",
            ["License number can only contain letters and numbers (no spaces or symbols)."] = "El número de licencia solo puede contener letras y números (sin espacios ni símbolos).",
            ["License number must include at least one letter (cannot be only numbers)."] = "El número de licencia debe incluir al menos una letra (no puede ser solo números).",
            ["Select at least one language."] = "Selecciona al menos un idioma.",
            ["Only English and Spanish are supported."] = "Solo se admiten inglés y español.",
            ["Use only letters, numbers, and hyphens."] = "Usa solo letras, números y guiones.",
            ["Serial number must include at least one letter and one number."] = "El número de serie debe incluir al menos una letra y un número.",
            ["Brokerage Name is required."] = "El nombre de la correduría es obligatorio.",
            ["Brokerage / Company Name is required."] = "El nombre de la correduría / empresa es obligatorio.",
            ["Brokerage / Company Name must be at least 2 characters."] = "El nombre de la correduría / empresa debe tener al menos 2 caracteres.",
            ["Brokerage / Company Name cannot exceed 200 characters."] = "El nombre de la correduría / empresa no puede superar 200 caracteres.",
            ["Brokerage / Company Name must include letters (e.g. Keller Williams, RE/MAX)."] =
                "El nombre de la correduría / empresa debe incluir letras (ej. Keller Williams, RE/MAX).",
            ["Brokerage / Company Name cannot contain only numbers."] =
                "El nombre de la correduría / empresa no puede contener solo números.",
            ["City / market area is required."] = "La ciudad / área de mercado es obligatoria.",
            ["Please select your license state."] = "Selecciona el estado de tu licencia.",
            ["Office address is required."] = "La dirección de oficina es obligatoria.",
            ["Please check the authorization box to verify your license before continuing."] =
                "Marca la casilla de autorización para verificar tu licencia antes de continuar.",
            // ASP.NET binder defaults for omitted radio posts (Urgent Quote Property step).
            ["The requestCategory field is required."] = "Selecciona qué necesitas.",
            ["The serviceType field is required."] = "Selecciona un tipo de servicio.",
            ["The urgencyLevel field is required."] = "Selecciona un nivel de urgencia.",
            ["Select a property and what you need to continue."] =
                "Selecciona una propiedad y lo que necesitas para continuar.",
            ["Select a property or enter the property address to continue."] =
                "Selecciona una propiedad o ingresa la dirección para continuar.",
            ["Select what you need."] = "Selecciona qué necesitas.",
            ["Select a service type."] = "Selecciona un tipo de servicio.",
            ["Select an urgency level."] = "Selecciona un nivel de urgencia.",

            // Bug 21 — homeowner offered-service confirm/status chips (shared across remodeling + siblings)
            ["Pending quote"] = "Cotización pendiente",
            ["Pending confirmation"] = "Confirmación pendiente",
            ["Text"] = "Mensaje de texto",
            ["Reminder and service saved"] = "Recordatorio y servicio guardados",
            ["Set reminder"] = "Configurar recordatorio",
            ["Exterior paint reminder saved"] = "Recordatorio de pintura exterior guardado",
            ["Request received"] = "Solicitud recibida",
            ["Provider matching"] = "Asignando proveedor",
            ["Assigning provider"] = "Asignando proveedor",
            ["Next reminder active"] = "Próximo recordatorio activo",
            ["Reminder active"] = "Recordatorio activo",

            // Auth / login DataAnnotations (also in Shared — kept here so validation catalog is complete)
            ["Email is required"] = "El correo electrónico es obligatorio",
            ["Invalid email address"] = "Correo electrónico inválido",
            ["Password is required"] = "La contraseña es obligatoria",
            ["Full name is required"] = "El nombre completo es obligatorio",
            ["Enter the 6-digit code"] = "Ingresa el código de 6 dígitos",
            ["The code must be 6 digits"] = "El código debe tener 6 dígitos",
            ["Confirm your password"] = "Confirma tu contraseña",
            ["Passwords do not match"] = "Las contraseñas no coinciden",
            ["Password must be at least 8 characters"] = "La contraseña debe tener al menos 8 caracteres",
            ["Password must be at least {2} characters."] = "La contraseña debe tener al menos {2} caracteres.",
        };
}
