using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

/// <summary>Display projection of a persisted in-app notification, culture-resolved.</summary>
public sealed class AppNotificationView
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Body { get; init; }
    public string? CategoryTag { get; init; }
    public string IconClass { get; init; } = "fa-bell";
    public string? TargetUrl { get; init; }
    public bool IsRead { get; init; }
    public DateTime FechaCreacion { get; init; }
}

public sealed record NewAppNotification(
    string RecipientUserId,
    string Audience,
    string TitleEn,
    string TitleEs,
    string? BodyEn = null,
    string? BodyEs = null,
    string? CategoryTag = null,
    string? IconClass = null,
    string? TargetUrl = null);

public interface INotificationService
{
    Task CreateAsync(NewAppNotification notification, CancellationToken cancellationToken = default);
    Task CreateManyAsync(IEnumerable<NewAppNotification> notifications, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppNotificationView>> GetRecentAsync(string userId, bool isSpanish, int take = 12, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    Task MarkAllReadAsync(string userId, CancellationToken cancellationToken = default);
}

public class NotificationService(AppDbContext db) : INotificationService
{
    public async Task CreateAsync(NewAppNotification notification, CancellationToken cancellationToken = default)
    {
        await CreateManyAsync(new[] { notification }, cancellationToken);
    }

    public async Task CreateManyAsync(IEnumerable<NewAppNotification> notifications, CancellationToken cancellationToken = default)
    {
        var rows = notifications
            .Where(n => !string.IsNullOrWhiteSpace(n.RecipientUserId))
            .Select(n => new IndorAppNotification
            {
                RecipientUserId = n.RecipientUserId,
                Audience = n.Audience,
                TitleEn = Trim(n.TitleEn, 200),
                TitleEs = Trim(string.IsNullOrWhiteSpace(n.TitleEs) ? n.TitleEn : n.TitleEs, 200),
                BodyEn = Trim(n.BodyEn, 500),
                BodyEs = Trim(string.IsNullOrWhiteSpace(n.BodyEs) ? n.BodyEn : n.BodyEs, 500),
                CategoryTag = Trim(n.CategoryTag, 40),
                IconClass = Trim(n.IconClass, 60),
                TargetUrl = Trim(n.TargetUrl, 300),
                IsRead = false,
                FechaCreacion = DateTime.UtcNow
            })
            .ToList();

        if (rows.Count == 0)
        {
            return;
        }

        db.IndorAppNotifications.AddRange(rows);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppNotificationView>> GetRecentAsync(string userId, bool isSpanish, int take = 12, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<AppNotificationView>();
        }

        var rows = await db.IndorAppNotifications
            .AsNoTracking()
            .Where(n => n.RecipientUserId == userId)
            .OrderByDescending(n => n.FechaCreacion)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(n => new AppNotificationView
        {
            Id = n.Id,
            Title = isSpanish ? n.TitleEs : n.TitleEn,
            Body = isSpanish ? n.BodyEs : n.BodyEn,
            CategoryTag = n.CategoryTag,
            IconClass = string.IsNullOrWhiteSpace(n.IconClass) ? "fa-bell" : n.IconClass!,
            TargetUrl = n.TargetUrl,
            IsRead = n.IsRead,
            FechaCreacion = n.FechaCreacion
        }).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return 0;
        }

        return await db.IndorAppNotifications
            .CountAsync(n => n.RecipientUserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task MarkAllReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        await db.IndorAppNotifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadUtc, DateTime.UtcNow), cancellationToken);
    }

    private static string? Trim(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        value = value.Trim();
        return value.Length <= max ? value : value[..max];
    }
}
