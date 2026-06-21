using System.Text.Json;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Helpers;

public static class HouseFactPreviewContext
{
    public const string SessionKey = "HouseFactPreviewPropertyInfo";
    public const string ReturnUrlSessionKey = "HouseFactReturnUrl";

    public static void Save(ISession session, PropertyInfoViewModel info) =>
        session.SetString(SessionKey, JsonSerializer.Serialize(info));

    public static PropertyInfoViewModel? Load(ISession session)
    {
        var json = session.GetString(SessionKey);
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<PropertyInfoViewModel>(json);
    }

    public static (Propiedad Propiedad, PropertyInfoViewModel Info)? LoadBundle(ISession session)
    {
        var info = Load(session);
        if (info == null) return null;
        return (ToPropiedad(info), info);
    }

    public static Propiedad ToPropiedad(PropertyInfoViewModel info) => new()
    {
        Id = 0,
        Direccion = info.FormattedAddress,
        AttomRawJson = info.AttomRawJson,
        AttomSyncStatus = info.DataSource,
        DatosJson = JsonSerializer.Serialize(info),
        Activo = true
    };

    public static void ConfigurePreviewView(Controller controller)
    {
        var returnUrl = controller.Url.Action("PropertyDetails", "Propietario");
        controller.ViewBag.HouseFactPreview = true;
        controller.ViewBag.PropiedadId = 0;
        controller.ViewBag.MyHomeNav = "summary";
        controller.ViewBag.HouseFactReturnUrl = returnUrl;
        controller.HttpContext.Session.SetString(ReturnUrlSessionKey, returnUrl!);
        controller.HttpContext.Items["HouseFactPreview"] = true;
    }

    public static string? GetReturnUrl(ISession session) =>
        session.GetString(ReturnUrlSessionKey);

    public static void ApplyReturnUrlToView(Controller controller)
    {
        var returnUrl = GetReturnUrl(controller.HttpContext.Session);
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            controller.ViewBag.HouseFactReturnUrl = returnUrl;
        }
    }

    public static IActionResult? RedirectIfMissing(Controller controller, (Propiedad, PropertyInfoViewModel)? bundle)
    {
        if (bundle != null) return null;
        return controller.Redirect(controller.Url.Action("EditarPerfil", "Perfil") + "#home");
    }

    public static bool IsPreview(Microsoft.AspNetCore.Mvc.Rendering.ViewContext? viewContext) =>
        viewContext?.ViewBag.HouseFactPreview as bool? == true
        || viewContext?.HttpContext.Items.ContainsKey("HouseFactPreview") == true;
}
