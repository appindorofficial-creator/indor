using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace IndorMvcApp.ViewComponents;

public class PropertyAdminViewingStripViewComponent(IPropertyAdministratorPortalService portal) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(
        PropertyAdministratorFlowPropertyViewModel? property,
        string? badgeLabel = null,
        bool allowSwitch = true)
    {
        if (property == null || property.Id <= 0)
        {
            return Content(string.Empty);
        }

        // Mid-flow steps (Review/Access/Schedule/…) keep a read-only strip.
        // Property switching is for entry *Details* wizards (e.g. AirFilterDetails).
        var action = (ViewContext.RouteData.Values["action"] as string) ?? "";
        var isEntryDetailsStep = action.EndsWith("Details", StringComparison.OrdinalIgnoreCase);
        allowSwitch = allowSwitch && isEntryDetailsStep;

        var portfolio = allowSwitch
            ? await portal.ListFlowPropertiesAsync(HttpContext.RequestAborted)
            : [];

        var options = portfolio
            .Select(p => new PropertyAdminViewingStripOptionViewModel
            {
                Id = p.Id,
                PropertyName = p.PropertyName,
                PropertyTypeLabel = p.PropertyTypeLabel,
                Location = p.Location,
                ImageUrl = p.ImageUrl,
                IsSelected = p.Id == property.Id,
                SwitchUrl = BuildSwitchUrl(p.Id)
            })
            .ToList();

        // Ensure the current property appears even if portfolio load failed / stale.
        if (options.Count == 0 || options.All(o => o.Id != property.Id))
        {
            options.Insert(0, new PropertyAdminViewingStripOptionViewModel
            {
                Id = property.Id,
                PropertyName = property.PropertyName,
                PropertyTypeLabel = property.PropertyTypeLabel,
                Location = property.Location,
                ImageUrl = property.ImageUrl,
                IsSelected = true,
                SwitchUrl = BuildSwitchUrl(property.Id)
            });
        }

        return View(new PropertyAdminViewingStripViewModel
        {
            Viewing = property,
            BadgeLabel = badgeLabel,
            AllowSwitch = allowSwitch && options.Count > 1,
            Options = options
        });
    }

    private string BuildSwitchUrl(int propertyId)
    {
        var request = HttpContext.Request;
        var pairs = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in request.Query.Keys)
        {
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            // Normalize to a single propertyId query key for wizard actions.
            if (string.Equals(key, "propertyId", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "PropertyId", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            pairs[key] = request.Query[key].ToString();
        }

        pairs["propertyId"] = propertyId.ToString();
        return QueryHelpers.AddQueryString(request.Path.Value ?? "/", pairs);
    }
}
