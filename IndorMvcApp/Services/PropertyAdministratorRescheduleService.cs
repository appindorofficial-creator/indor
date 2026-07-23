using System.Globalization;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorRescheduleService
{
    Task<PropertyAdministratorRescheduleFormViewModel?> GetFormAsync(
        IUrlHelper url, int requestId, string? fromAction, CancellationToken cancellationToken = default);

    Task<string?> SubmitAsync(
        PropertyAdministratorRescheduleSubmitInput input, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorRescheduleService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorRescheduleService
{
    private static readonly string[] TimeOptions =
    [
        "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM",
        "1:00 PM", "2:00 PM", "3:00 PM", "4:00 PM", "5:00 PM", "6:00 PM"
    ];

    private static readonly HashSet<string> AllowedFromActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "TurnoverCleaningConfirmed",
        "StandardCleaningConfirmed",
        "PetDeepCleanConfirmed",
        "LinenRestockConfirmed",
        "MovingHelpConfirmed",
        "AirFilterConfirmed",
        "SmokeDetectorConfirmed",
        "PressureWashingConfirmed",
        "Tasks"
    };

    public async Task<PropertyAdministratorRescheduleFormViewModel?> GetFormAsync(
        IUrlHelper url, int requestId, string? fromAction, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken);
        if (admin == null)
        {
            return null;
        }

        var request = admin.ServiceRequests.FirstOrDefault(r => r.Id == requestId);
        if (request == null)
        {
            return null;
        }

        var shell = PropertyAdministratorFlowServiceSupport.BuildShell(admin);
        await PropertyAdministratorFlowServiceSupport.ApplyProfilePhotoAsync(shell, userManager, httpContextAccessor);

        var property = request.PortfolioPropertyId.HasValue
            ? admin.PortfolioProperties.FirstOrDefault(p => p.Id == request.PortfolioPropertyId.Value)
            : null;

        var scheduledLocal = (request.ScheduledUtc ?? DateTime.UtcNow.AddHours(1)).ToLocalTime();
        var (startLabel, endLabel) = ParseWindowLabels(request.EtaLabel, scheduledLocal);

        return new PropertyAdministratorRescheduleFormViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            FromAction = NormalizeFromAction(fromAction, request.Title),
            ServiceTitle = request.Title,
            PropertyName = request.PropertyName,
            Location = request.Location,
            CurrentScheduleLabel = request.EtaLabel
                ?? PropertyAdministratorDisplayLocalization.T(
                    "Today • {0} – {1}", startLabel, endLabel),
            VisitDate = scheduledLocal.ToString("yyyy-MM-dd"),
            StartTimeLabel = startLabel,
            EndTimeLabel = endLabel,
            TimeOptions = TimeOptions,
            ViewingProperty = PropertyAdministratorFlowServiceSupport.MapProperty(property)
        };
    }

    public async Task<string?> SubmitAsync(
        PropertyAdministratorRescheduleSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken);
        if (admin == null)
        {
            return null;
        }

        var request = admin.ServiceRequests.FirstOrDefault(r => r.Id == input.RequestId);
        if (request == null)
        {
            return null;
        }

        if (!DateTime.TryParseExact(
                input.VisitDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var visitDate))
        {
            return null;
        }

        var startLocal = CombineDateAndTime(visitDate, input.StartTimeLabel) ?? visitDate.Date.AddHours(11);
        var endLocal = CombineDateAndTime(visitDate, input.EndTimeLabel) ?? startLocal.AddHours(3);
        if (endLocal <= startLocal)
        {
            endLocal = startLocal.AddHours(3);
        }

        var etaLabel = BuildEtaLabel(startLocal, endLocal);
        request.ScheduledUtc = startLocal.ToUniversalTime();
        request.EtaLabel = etaLabel;

        var visit = await db.IndorPropertyAdminScheduledVisits
            .Where(v => v.AdministratorId == admin.Id && v.PropertyName == request.PropertyName)
            .OrderByDescending(v => v.VisitDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (visit != null)
        {
            visit.VisitDate = startLocal.Date;
            visit.TimeWindow = $"{FormatTime(startLocal)} – {FormatTime(endLocal)}";
        }

        await db.SaveChangesAsync(cancellationToken);
        return NormalizeFromAction(input.FromAction, request.Title);
    }

    private async Task<IndorPropertyAdministrator?> LoadAdminAsync(
        bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var query = db.IndorPropertyAdministrators
            .Include(a => a.PortfolioProperties)
            .Include(a => a.ServiceRequests)
            .Where(a => a.UserId == userId && a.RegistrationStatus == PropertyAdministratorRegistrationStatuses.Completed);

        return trackChanges
            ? await query.FirstOrDefaultAsync(cancellationToken)
            : await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    private static string NormalizeFromAction(string? fromAction, string? title)
    {
        if (!string.IsNullOrWhiteSpace(fromAction) && AllowedFromActions.Contains(fromAction))
        {
            return fromAction;
        }

        return ResolveFromTitle(title) ?? "Tasks";
    }

    private static string? ResolveFromTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var t = title;
        if (ContainsAny(t, "Turnover Cleaning", "Limpieza de rotación", "Turnover cleaning"))
        {
            return "TurnoverCleaningConfirmed";
        }

        if (ContainsAny(t, "Standard Cleaning", "Limpieza estándar", "Standard cleaning"))
        {
            return "StandardCleaningConfirmed";
        }

        if (ContainsAny(t, "Pet Deep Clean", "Limpieza profunda para mascotas", "Pet deep clean"))
        {
            return "PetDeepCleanConfirmed";
        }

        if (ContainsAny(t, "Linen", "Sábanas", "supply restock", "Restock"))
        {
            return "LinenRestockConfirmed";
        }

        if (ContainsAny(t, "Moving Help", "Ayuda de mudanza", "Moving help"))
        {
            return "MovingHelpConfirmed";
        }

        if (ContainsAny(t, "Air Filter", "filtro de aire", "Air filter"))
        {
            return "AirFilterConfirmed";
        }

        if (ContainsAny(t, "Smoke Detector", "detectores de humo", "Smoke detector"))
        {
            return "SmokeDetectorConfirmed";
        }

        if (ContainsAny(t, "Pressure Washing", "Lavado a presión", "Pressure washing"))
        {
            return "PressureWashingConfirmed";
        }

        return null;
    }

    private static bool ContainsAny(string haystack, params string[] needles) =>
        needles.Any(n => haystack.Contains(n, StringComparison.OrdinalIgnoreCase));

    private static (string Start, string End) ParseWindowLabels(string? etaLabel, DateTime fallbackStart)
    {
        var start = FormatTime(fallbackStart);
        var end = FormatTime(fallbackStart.AddHours(3));
        if (string.IsNullOrWhiteSpace(etaLabel))
        {
            return (start, end);
        }

        // Examples: "Today • 11:00 AM – 2:00 PM", "Hoy • 11:00 a. m. - 2:00 p. m."
        var separators = new[] { " – ", " - ", "–", "-" };
        foreach (var sep in separators)
        {
            var idx = etaLabel.LastIndexOf(sep, StringComparison.Ordinal);
            if (idx <= 0)
            {
                continue;
            }

            var before = etaLabel[..idx].Trim();
            var after = etaLabel[(idx + sep.Length)..].Trim();
            var bulletIdx = before.LastIndexOf('•');
            if (bulletIdx >= 0)
            {
                before = before[(bulletIdx + 1)..].Trim();
            }

            if (!string.IsNullOrWhiteSpace(before) && !string.IsNullOrWhiteSpace(after))
            {
                return (NormalizeTimeLabel(before) ?? start, NormalizeTimeLabel(after) ?? end);
            }
        }

        return (start, end);
    }

    private static string? NormalizeTimeLabel(string raw)
    {
        var cleaned = raw
            .Replace("a. m.", "AM", StringComparison.OrdinalIgnoreCase)
            .Replace("p. m.", "PM", StringComparison.OrdinalIgnoreCase)
            .Replace("a.m.", "AM", StringComparison.OrdinalIgnoreCase)
            .Replace("p.m.", "PM", StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (DateTime.TryParse(cleaned, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            || DateTime.TryParse(cleaned, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
        {
            return FormatTime(parsed);
        }

        return TimeOptions.FirstOrDefault(t =>
            string.Equals(t, cleaned, StringComparison.OrdinalIgnoreCase));
    }

    private static DateTime? CombineDateAndTime(DateTime date, string? timeLabel)
    {
        var normalized = NormalizeTimeLabel(timeLabel ?? "");
        if (normalized == null)
        {
            return null;
        }

        if (!DateTime.TryParseExact(
                normalized,
                ["h:mm tt", "h:mmtt", "hh:mm tt"],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var time))
        {
            return null;
        }

        return date.Date.Add(time.TimeOfDay);
    }

    private static string BuildEtaLabel(DateTime startLocal, DateTime endLocal)
    {
        var window = $"{FormatTime(startLocal)} – {FormatTime(endLocal)}";
        var today = DateTime.Today;
        if (startLocal.Date == today)
        {
            return PropertyAdministratorDisplayLocalization.T("Today • {0}", window);
        }

        if (startLocal.Date == today.AddDays(1))
        {
            return PropertyAdministratorDisplayLocalization.T("Tomorrow • {0}", window);
        }

        return PropertyAdministratorDisplayLocalization.T(
            "{0} • {1}",
            startLocal.ToString("MMM d", CultureInfo.CurrentCulture),
            window);
    }

    private static string FormatTime(DateTime value) =>
        value.ToString("h:mm tt", CultureInfo.InvariantCulture);
}
