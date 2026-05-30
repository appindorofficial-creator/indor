using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace IndorMvcApp.Helpers;

public static class HouseFactFlowLinkHelper
{
    public static string FlowAction(ViewContext viewContext, string savedAction, string previewAction) =>
        HouseFactPreviewContext.IsPreview(viewContext) ? previewAction : savedAction;

    public static string FlowController(ViewContext viewContext, string savedController) =>
        HouseFactPreviewContext.IsPreview(viewContext) ? "HouseFactPreview" : savedController;

    public static object? FlowId(ViewContext viewContext, int propiedadId) =>
        HouseFactPreviewContext.IsPreview(viewContext) ? null : propiedadId;

    public static string BackUrl(IUrlHelper url, ViewContext viewContext, int propiedadId) =>
        HouseFactPreviewContext.IsPreview(viewContext)
            ? url.Action("PropertyDetails", "Propietario") ?? "#"
            : url.Action("Details", "MyHome", new { id = propiedadId, tab = "attom" }) ?? "#";
}
