using System.Globalization;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

/// <summary>Localized labels for Property Administrator portal and flows.</summary>
public static class PropertyAdministratorDisplayLocalization
{
    public static string L(string english) => DisplayLabelsLocalization.L(english);

    public static string T(string key, params object[] args)
    {
        var template = L(key);
        return args.Length == 0 ? template : string.Format(CultureInfo.CurrentCulture, template, args);
    }

    public static string Localize(IIndorLocalizer localizer, string? text) =>
        UiDisplayLocalization.Localize(localizer, text);

    public static string LabelPropertyType(string? value) =>
        L(PropertyAdministratorCatalog.LabelPropertyType(value));

    public static string LabelPortfolioType(string? value) =>
        L(PropertyAdministratorCatalog.LabelPortfolioType(value));

    public static string LabelManagementStyle(string? value) =>
        L(PropertyAdministratorCatalog.LabelManagementStyle(value));

    public static string LabelOwnershipType(string? value) =>
        L(PropertyAdministratorCatalog.LabelOwnershipType(value));

    public static string? OccupancyLabel(string? propertyType) =>
        propertyType == "ShortTermRental" ? L("Occupied now") : null;

    public static string BuildGreeting(int hour, string firstName)
    {
        var greeting = hour < 12 ? L("Good morning")
            : hour < 18 ? L("Good afternoon")
            : L("Good evening");
        return T("{0}, {1}", greeting, firstName);
    }

    public static string BuildPortfolioName(string firstName, string? businessName) =>
        !string.IsNullOrWhiteSpace(businessName)
            ? businessName
            : T("{0} Portfolio", firstName);

    public static string EventAtProperty(string title, string propertyName) =>
        T("{0} at {1}", title, propertyName);

    public static string MapPropertyStatusLabel(string? status) =>
        string.IsNullOrWhiteSpace(status) || status is "Added" or "Active"
            ? L("Active")
            : L(status);

    public static (string Label, string Css) MapRecentRequestStatus(string status) => status switch
    {
        PropertyAdministratorRequestStatuses.InProgress => (L("En route"), "inprogress"),
        PropertyAdministratorRequestStatuses.Emergency => (L("Emergency"), "emergency"),
        _ => (L("Open"), "open")
    };

    public static (string Label, string Css) MapRequestStatus(string status) => status switch
    {
        PropertyAdministratorRequestStatuses.Emergency => (L("EMERGENCY"), "emergency"),
        PropertyAdministratorRequestStatuses.Scheduled => (L("SCHEDULED"), "scheduled"),
        PropertyAdministratorRequestStatuses.InProgress => (L("IN PROGRESS"), "inprogress"),
        PropertyAdministratorRequestStatuses.Completed => (L("COMPLETED"), "completed"),
        _ => (L("OPEN"), "open")
    };

    public static (string Label, string Css) MapBillingStatus(string status) => status switch
    {
        PropertyAdministratorRequestStatuses.Completed => (L("Paid"), "paid"),
        PropertyAdministratorRequestStatuses.Scheduled => (L("Upcoming"), "scheduled"),
        PropertyAdministratorRequestStatuses.InProgress => (L("Pending"), "pending"),
        _ => (L("Open"), "open")
    };

    public static string FormatSquareFootage(int sqft) =>
        T("{0} sq ft", sqft.ToString("N0", CultureInfo.CurrentCulture));

    public static string FormatLotSizeSqFt(int sqft) =>
        T("{0} sq ft", sqft.ToString("N0", CultureInfo.CurrentCulture));

    public static string FormatLotSizeAcres(decimal acres) =>
        T("{0} acres", acres.ToString("N2", CultureInfo.CurrentCulture));

    /// <summary>
    /// Shared ETA line for PA emergency / trade cards:
    /// "Nearest {trade} pro available in {minutes} minutes".
    /// </summary>
    public static string NearestProAvailableInMinutes(string tradeEnglish, int minutes) =>
        T("Nearest {0} pro available in {1} minutes", L(tradeEnglish), minutes);
}
