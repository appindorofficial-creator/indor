using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

/// <summary>
/// Permanently deletes a user account and its associated data (App Store Guideline 5.1.1(v)).
///
/// Cleanup runs inside a single DB transaction so a mid-flight FK failure cannot leave homes
/// deleted while the AspNetUsers row (and Multipropietario portal) still exists.
/// </summary>
public sealed class AccountDeletionService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountDeletionService> _logger;

    public AccountDeletionService(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountDeletionService> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> DeleteAccountAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var userId = user.Id;

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await CleanupDependentDataAsync(userId, cancellationToken);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Account deletion failed for {UserId}: {Errors}",
                    userId,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Account deletion threw while deleting user {UserId}. Rolling back.", userId);
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogWarning(rollbackEx, "Account deletion rollback failed for {UserId}.", userId);
            }

            return false;
        }
    }

    private async Task CleanupDependentDataAsync(string userId, CancellationToken cancellationToken)
    {
        var propertyIds = await _db.Propiedades
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        // Multipropietario: remove the admin profile entirely (cascades portfolio, requests, visits).
        // Unlinking UserId alone left an empty "Jacobo home" portal while AspNetUsers survived FK failures.
        var adminIds = await _db.IndorPropertyAdministrators
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        if (adminIds.Count > 0)
        {
            // Preventive plans reference portfolio properties; delete them before portfolio/admin rows.
            await _db.IndorPropertyAdminPreventivePlans
                .Where(p => adminIds.Contains(p.AdministratorId))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.IndorPropertyAdminServiceRequests
                .Where(r => adminIds.Contains(r.AdministratorId))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.IndorPropertyAdminScheduledVisits
                .Where(v => adminIds.Contains(v.AdministratorId))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.IndorPropertyAdminHomecarePlans
                .Where(h => adminIds.Contains(h.AdministratorId))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.IndorPropertyAdminPortfolioProperties
                .Where(p => adminIds.Contains(p.AdministratorId))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.IndorPropertyAdministrators
                .Where(a => adminIds.Contains(a.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        // Other admins may still reference this user's homes — clear before Propiedades cascade.
        if (propertyIds.Count > 0)
        {
            await _db.IndorPropertyAdminPortfolioProperties
                .Where(pp => pp.PropiedadId != null && propertyIds.Contains(pp.PropiedadId.Value))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.PropiedadId, (int?)null), cancellationToken);
        }

        // NO ACTION FKs to AspNetUsers that block Identity delete.
        await _db.SolicitudesRealtor
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.PropiedadHvacSistemas
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.PropiedadWaterHeaterSistemas
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        // Neighborhood feed (UserId string, typically no cascade from AspNetUsers).
        var postIds = await _db.IndorNeighborhoodPosts
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (postIds.Count > 0)
        {
            var commentIds = await _db.IndorNeighborhoodComments
                .AsNoTracking()
                .Where(c => postIds.Contains(c.PostId))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            await _db.IndorNeighborhoodPostLikes
                .Where(x => postIds.Contains(x.PostId))
                .ExecuteDeleteAsync(cancellationToken);
            await _db.IndorNeighborhoodPostSaves
                .Where(x => postIds.Contains(x.PostId))
                .ExecuteDeleteAsync(cancellationToken);
            if (commentIds.Count > 0)
            {
                await _db.IndorNeighborhoodCommentSaves
                    .Where(x => commentIds.Contains(x.CommentId))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            await _db.IndorNeighborhoodComments
                .Where(c => postIds.Contains(c.PostId))
                .ExecuteDeleteAsync(cancellationToken);
            await _db.IndorNeighborhoodPostMedia
                .Where(m => postIds.Contains(m.PostId))
                .ExecuteDeleteAsync(cancellationToken);
            await _db.IndorNeighborhoodPosts
                .Where(p => postIds.Contains(p.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        await _db.IndorNeighborhoodPostLikes
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        await _db.IndorNeighborhoodPostSaves
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        await _db.IndorNeighborhoodCommentSaves
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.IndorNeighborRequests
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.IndorPasswordResetCodes
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.IndorAppNotifications
            .Where(x => x.RecipientUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _db.IndorServiceRequests
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        // Optional role profiles (NO ACTION on UserId).
        await _db.IndorProveedores
            .Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UserId, (string?)null), cancellationToken);

        await _db.IndorRealtors
            .Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UserId, (string?)null), cancellationToken);
    }
}
