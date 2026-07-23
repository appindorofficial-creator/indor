using System.Globalization;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

/// <summary>Shared shell, property mapping, and timeline helpers for PA service wizards.</summary>
public static class PropertyAdministratorFlowServiceSupport
{
    /// <summary>
    /// PA Servicios catalog (emergencies list / service hub). Use for wizard "Volver al inicio"
    /// / "Back to start" — never <c>Administrador/Index</c> (portfolio home).
    /// </summary>
    public static string ServicesUrl(IUrlHelper url, int? propertyId = null) =>
        url.Action("Services", "Administrador", new { propertyId }) ?? "#";

    public static PropertyAdministratorPortalShellViewModel BuildShell(IndorPropertyAdministrator admin)
    {
        var firstName = admin.DisplayName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
            ?? PropertyAdministratorDisplayLocalization.L("there");
        var hour = DateTime.Now.Hour;

        return new PropertyAdministratorPortalShellViewModel
        {
            DisplayName = admin.DisplayName ?? PropertyAdministratorDisplayLocalization.L("Property Owner"),
            PortfolioName = PropertyAdministratorDisplayLocalization.BuildPortfolioName(
                firstName,
                admin.PortfolioBusinessName),
            ActivePropertyCount = admin.PortfolioProperties.Count,
            Greeting = PropertyAdministratorDisplayLocalization.BuildGreeting(hour, firstName),
            NotificationCount = admin.ServiceRequests.Count(r =>
                r.Status is PropertyAdministratorRequestStatuses.Open
                    or PropertyAdministratorRequestStatuses.Emergency
                    or PropertyAdministratorRequestStatuses.InProgress)
        };
    }

    public static async Task ApplyProfilePhotoAsync(
        PropertyAdministratorPortalShellViewModel shell,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var user = await userManager.FindByIdAsync(userId);
        shell.ProfilePhotoUrl = user?.FotoUrl;
    }

    public static PropertyAdministratorFlowPropertyViewModel MapProperty(IndorPropertyAdminPortfolioProperty? property)
    {
        if (property == null)
        {
            return new PropertyAdministratorFlowPropertyViewModel();
        }

        return new PropertyAdministratorFlowPropertyViewModel
        {
            Id = property.Id,
            PropertyName = property.PropertyName,
            Location = property.Location,
            PropertyTypeLabel = PropertyAdministratorDisplayLocalization.LabelPropertyType(property.PropertyType),
            ImageUrl = PropertyAdministratorCatalog.ResolvePortfolioImageUrl(property.ImageUrl, property.PropertyType),
            OccupancyLabel = PropertyAdministratorDisplayLocalization.OccupancyLabel(property.PropertyType)
        };
    }

    public static string YesNo(string? value) => value switch
    {
        "Yes" => PropertyAdministratorDisplayLocalization.L("Yes"),
        "No" => PropertyAdministratorDisplayLocalization.L("No"),
        "NotSure" => PropertyAdministratorDisplayLocalization.L("Not sure"),
        _ => value ?? "—"
    };

    public static string FormatTodayTime(DateTime utc) =>
        PropertyAdministratorDisplayLocalization.T(
            "Today, {0}",
            utc.ToLocalTime().ToString("h:mm tt", CultureInfo.CurrentCulture));

    public static string FormatTodayBulletTime(DateTime utc) =>
        PropertyAdministratorDisplayLocalization.T(
            "Today • {0}",
            utc.ToLocalTime().ToString("h:mm tt", CultureInfo.CurrentCulture));

    public static IReadOnlyList<PropertyAdministratorEmergencyAcTimelineItemViewModel> BuildEmergencyRouteTimeline(
        IndorPropertyAdminServiceRequest request,
        string finalStepLabel,
        string finalStepIcon = "fa-clipboard-list")
    {
        var submitted = FormatTodayTime(request.FechaCreacion);
        var assigned = FormatTodayTime(request.FechaCreacion.AddMinutes(5));
        var enRoute = FormatTodayTime(request.FechaCreacion.AddMinutes(8));
        var step = request.TimelineStep;
        var pending = PropertyAdministratorDisplayLocalization.L("Pending");

        return
        [
            new()
            {
                Label = PropertyAdministratorDisplayLocalization.L("Request submitted"),
                StatusLabel = submitted,
                IconClass = "fa-circle-check",
                State = "done"
            },
            new()
            {
                Label = PropertyAdministratorDisplayLocalization.L("Technician assigned"),
                StatusLabel = assigned,
                IconClass = "fa-circle-check",
                State = step >= 1 ? "done" : "pending"
            },
            new()
            {
                Label = PropertyAdministratorDisplayLocalization.L("En route"),
                StatusLabel = enRoute,
                IconClass = "fa-truck",
                State = step >= 2 ? "active" : "pending"
            },
            new()
            {
                Label = PropertyAdministratorDisplayLocalization.L("Arrived"),
                StatusLabel = pending,
                IconClass = "fa-location-dot",
                State = step >= 3 ? "done" : "pending"
            },
            new()
            {
                Label = PropertyAdministratorDisplayLocalization.L(finalStepLabel),
                StatusLabel = pending,
                IconClass = finalStepIcon,
                State = step >= 4 ? "done" : "pending"
            }
        ];
    }
}
