using IndorMvcApp.Data;
using IndorMvcApp.Localization;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class LawnCatalogService(AppDbContext db)
{
    public const decimal SubscriptionDiscount = 10m;

    public async Task<IReadOnlyList<LawnCatalogOption>> LoadGroupAsync(
        int microservicioId,
        string optionGroup,
        CancellationToken ct = default)
    {
        try
        {
            var options = await db.LawnCatalogOptions
                .AsNoTracking()
                .Where(o => o.MicroservicioId == microservicioId && o.OptionGroup == optionGroup && o.IsActive)
                .OrderBy(o => o.SortOrder)
                .ThenBy(o => o.LabelEn)
                .ToListAsync(ct);

            if (options.Count > 0)
            {
                return options;
            }
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            // fall through to static defaults
        }

        return GetFallbackGroup(optionGroup);
    }

    public async Task<LawnCatalogOption?> FindOptionAsync(
        int microservicioId,
        string optionGroup,
        string? code,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var group = await LoadGroupAsync(microservicioId, optionGroup, ct);
        return group.FirstOrDefault(o => o.Code == code);
    }

    public static string PickLabel(LawnCatalogOption option) =>
        CatalogText.PickWithUiFallback(option.LabelEn, option.LabelEs, DisplayLabelsLocalization.IsSpanishUi);

    public async Task<string> GetLabelAsync(
        int microservicioId,
        string optionGroup,
        string? code,
        CancellationToken ct = default)
    {
        var option = await FindOptionAsync(microservicioId, optionGroup, code, ct);
        return option != null ? PickLabel(option) : code ?? "—";
    }

    public async Task<decimal> GetBasePriceAsync(int microservicioId, string? areaCode, CancellationToken ct = default)
    {
        var area = await FindOptionAsync(microservicioId, LawnCatalogGroups.Area, areaCode, ct);
        return area?.Price ?? 45m;
    }

    public async Task<decimal> GetAddonsTotalAsync(
        int microservicioId,
        string? pipeValue,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pipeValue))
        {
            return 0m;
        }

        var addons = await LoadGroupAsync(microservicioId, LawnCatalogGroups.Addon, ct);
        return pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(code => !string.Equals(code, "NoThanks", StringComparison.OrdinalIgnoreCase))
            .Select(code => addons.FirstOrDefault(a => a.Code == code)?.Price ?? 0m)
            .Sum();
    }

    public Task<decimal> GetSubscriptionDiscountAsync(string? frequencyCode) =>
        Task.FromResult(frequencyCode is "Every15Days" or "Monthly" or "Flexible" or "Biweekly" or "Weekly"
            ? SubscriptionDiscount
            : 0m);

    public async Task<decimal> CalculateTotalAsync(
        int microservicioId,
        string? frequencyCode,
        string? areaCode,
        string? addonsPipe,
        CancellationToken ct = default)
    {
        var basePrice = await GetBasePriceAsync(microservicioId, areaCode, ct);
        var addons = await GetAddonsTotalAsync(microservicioId, addonsPipe, ct);
        var discount = await GetSubscriptionDiscountAsync(frequencyCode);
        return Math.Max(0m, basePrice + addons - discount);
    }

    public async Task<IReadOnlyList<LawnCatalogOption>> ParseAddonsAsync(
        int microservicioId,
        string? pipeValue,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pipeValue))
        {
            return [];
        }

        var addons = await LoadGroupAsync(microservicioId, LawnCatalogGroups.Addon, ct);
        return pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(code => !string.Equals(code, "NoThanks", StringComparison.OrdinalIgnoreCase))
            .Select(code => addons.FirstOrDefault(a => a.Code == code))
            .Where(addon => addon != null)
            .Cast<LawnCatalogOption>()
            .ToList();
    }

    public static string MapFrequencyToServiceType(string? frequencyCode) =>
        string.Equals(frequencyCode, "Once", StringComparison.OrdinalIgnoreCase)
            ? "OneTime"
            : "Subscription";

    public static int GetFrequencyIntervalDays(string? frequencyCode) => frequencyCode switch
    {
        "Every15Days" or "Biweekly" => 15,
        "Monthly" => 30,
        "Weekly" => 7,
        _ => 0
    };

    public static DateTime? ComputeNextReminderUtc(DateTime? fromDate, string? frequencyCode)
    {
        var days = GetFrequencyIntervalDays(frequencyCode);
        if (days <= 0 || fromDate == null)
        {
            return null;
        }

        return fromDate.Value.Date.AddDays(days).ToUniversalTime();
    }

    private static IReadOnlyList<LawnCatalogOption> GetFallbackGroup(string optionGroup) =>
        optionGroup switch
        {
            LawnCatalogGroups.Frequency =>
            [
                new() { Code = "Once", LabelEn = "Once", IconClass = "fa-calendar-day", SortOrder = 1 },
                new() { Code = "Every15Days", LabelEn = "Every 15 days", IconClass = "fa-rotate", SortOrder = 2 },
                new() { Code = "Monthly", LabelEn = "Monthly", IconClass = "fa-calendar", SortOrder = 3 }
            ],
            LawnCatalogGroups.Area =>
            [
                new() { Code = "FrontYard", LabelEn = "Front", Price = 45, IconClass = "fa-house", SortOrder = 1 },
                new() { Code = "BackYard", LabelEn = "Backyard", Price = 45, IconClass = "fa-fence", SortOrder = 2 },
                new() { Code = "FrontBack", LabelEn = "Front + Backyard", Price = 75, IconClass = "fa-house-chimney", SortOrder = 3 }
            ],
            LawnCatalogGroups.Addon =>
            [
                new() { Code = "EdgeBorders", LabelEn = "Edging / borders", Price = 20, IconClass = "fa-border-all", SortOrder = 1 },
                new() { Code = "BushTrimming", LabelEn = "Bush trimming", Price = 30, IconClass = "fa-leaf", SortOrder = 2 },
                new() { Code = "NoThanks", LabelEn = "No thanks", IconClass = "fa-ban", SortOrder = 3 }
            ],
            LawnCatalogGroups.TimeWindow =>
            [
                new() { Code = "Morning8_11", LabelEn = "8–11 AM", IconClass = "fa-sun", SortOrder = 1 },
                new() { Code = "Midday11_2", LabelEn = "11 AM–2 PM", IconClass = "fa-cloud-sun", SortOrder = 2 },
                new() { Code = "Afternoon2_5", LabelEn = "2–5 PM", IconClass = "fa-cloud", SortOrder = 3 },
                new() { Code = "Evening5_8", LabelEn = "5–8 PM", IconClass = "fa-moon", SortOrder = 4 }
            ],
            LawnCatalogGroups.ReminderLead =>
            [
                new() { Code = "1", LabelEn = "1 day before", IconClass = "fa-bell", SortOrder = 1 },
                new() { Code = "2", LabelEn = "2 days before", IconClass = "fa-bell", SortOrder = 2 }
            ],
            LawnCatalogGroups.ReminderChannel =>
            [
                new() { Code = "Push", LabelEn = "Push", IconClass = "fa-mobile-screen", SortOrder = 1 },
                new() { Code = "SMS", LabelEn = "SMS", IconClass = "fa-comment-sms", SortOrder = 2 },
                new() { Code = "Email", LabelEn = "Email", IconClass = "fa-envelope", SortOrder = 3 }
            ],
            _ => []
        };
}
