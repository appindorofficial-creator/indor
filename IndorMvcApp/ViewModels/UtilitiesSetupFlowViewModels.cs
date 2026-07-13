using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class UtilitiesSetupServiceOptionViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-check";
}

public class UtilitiesSetupAddressViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string PageTitle { get; set; } = "Utilities Setup";
    public string Subtitulo { get; set; } = "Connect internet, cable, electricity, water, and gas for your new home.";

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    public string ServiciosConectar { get; set; } = string.Empty;

    public DateTime? FechaServicio { get; set; }

    [Required]
    public string PreferenciaContacto { get; set; } = string.Empty;
}

public class UtilitiesSetupInternetProviderViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Etiqueta { get; set; }
    public string? Velocidad { get; set; }
    public string? DetalleExtra { get; set; }
    public decimal PrecioDesde { get; set; }
    public bool Selected { get; set; }
}

public class UtilitiesSetupInternetViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string PageTitle { get; set; } = "Utilities Setup";
    public string DireccionPropiedad { get; set; } = string.Empty;
    public List<UtilitiesSetupInternetProviderViewModel> Proveedores { get; set; } = new();

    public int? ProveedorInternetId { get; set; }

    [Required]
    public string OpcionCable { get; set; } = "InternetOnly";
}

public class UtilitiesSetupUtilityContactViewModel
{
    public string TipoUtilidad { get; set; } = string.Empty;
    public string TipoLabel { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Website { get; set; }
    public string Icon { get; set; } = "fa-bolt";
    public bool Included { get; set; } = true;
}

public class UtilitiesSetupUtilitiesViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string PageTitle { get; set; } = "Utilities Setup";
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string FechaServicioLabel { get; set; } = string.Empty;
    public List<UtilitiesSetupUtilityContactViewModel> Contactos { get; set; } = new();
}

public class UtilitiesSetupReviewViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string PageTitle { get; set; } = "Review & Save";
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string FechaServicioLabel { get; set; } = string.Empty;
    public string ServiciosConectarLabel { get; set; } = string.Empty;
    public string PreferenciaContactoLabel { get; set; } = string.Empty;
    public string? InternetResumen { get; set; }
    public bool InternetSelected { get; set; }
    public List<UtilitiesSetupUtilityContactViewModel> Contactos { get; set; } = new();
}

public class UtilitiesSetupCompletedViewModel
{
    public int SolicitudId { get; set; }
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string FechaServicioLabel { get; set; } = string.Empty;
    public string? InternetResumen { get; set; }
    public string? InternetProviderName { get; set; }
    public List<UtilitiesSetupUtilityContactViewModel> Contactos { get; set; } = new();
    public List<UtilitiesSetupServiceCardViewModel> ServiceCards { get; set; } = new();
}

public class UtilitiesSetupServiceCardViewModel
{
    public string Label { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string Icon { get; set; } = "fa-bolt";
    public string? ExtraNote { get; set; }
}
