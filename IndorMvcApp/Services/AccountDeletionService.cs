using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

/// <summary>
/// Permanently deletes a user account and its associated data (App Store Guideline 5.1.1(v)).
///
/// Most user data is removed automatically by database cascade rules: <c>Propiedad</c>, every
/// <c>Solicitud*</c> service/inspection/emergency request (and their file rows), scheduled
/// reminders, memberships, payment methods, payments, service history and support messages all
/// have a required <c>UserId</c> foreign key that cascades from <c>AspNetUsers</c>.
///
/// A few role-profile tables reference the user through an OPTIONAL foreign key that was mapped
/// with <c>NO ACTION</c> (EF Core default for optional relationships). Those would otherwise
/// block the delete, so they are unlinked (UserId set to null) first. Tables keyed only by a
/// plain <c>UserId</c> string (neighbor requests, password reset codes) are removed explicitly.
///
/// Multi-Property (property administrator) portfolio rows reference <c>Propiedad</c> with
/// <c>FK_IndorPropAdminProp_Propiedad</c> (NO ACTION), so they must be cleared before the
/// Identity cascade deletes the user's properties.
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

        // Remove data that is NOT cascade-deleted from AspNetUsers. Each step is isolated so a
        // single failure never leaves an account that cannot be deleted.
        await BestEffortAsync("neighbor requests", () =>
            _db.IndorNeighborRequests.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken));

        await BestEffortAsync("password reset codes", () =>
            _db.IndorPasswordResetCodes.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken));

        // Role profiles reference the user through an optional FK (ON DELETE NO ACTION), which
        // would block the delete. Unlink them so the account can be removed. IndorProveedor is
        // mapped with SetNull and is handled automatically, but we unlink it too for consistency.
        await BestEffortAsync("provider profile unlink", () =>
            _db.IndorProveedores.Where(x => x.UserId == userId)
               .ExecuteUpdateAsync(s => s.SetProperty(x => x.UserId, (string?)null), cancellationToken));

        await BestEffortAsync("realtor profile unlink", () =>
            _db.IndorRealtors.Where(x => x.UserId == userId)
               .ExecuteUpdateAsync(s => s.SetProperty(x => x.UserId, (string?)null), cancellationToken));

        // Multi-Property: portfolio rows hold PropiedadId with NO ACTION. Must succeed before
        // AspNetUsers cascade deletes Propiedades (FK_IndorPropAdminProp_Propiedad).
        try
        {
            await ClearPropertyAdministratorDataAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Account deletion aborted for {UserId}: property administrator cleanup failed.",
                userId);
            return false;
        }

        // Deletes the Identity user. Database cascades remove homes, service requests, files,
        // memberships, payments, history, support messages and the Identity rows (roles, logins,
        // tokens, claims).
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError(
                "Account deletion failed for {UserId}: {Errors}",
                userId,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return false;
        }

        return true;
    }

    /// <summary>
    /// Removes property-administrator rows that would block cascade-delete of the user's
    /// <c>Propiedad</c> records, then deletes the administrator profile itself.
    /// </summary>
    private async Task ClearPropertyAdministratorDataAsync(string userId, CancellationToken cancellationToken)
    {
        var adminIds = await _db.IndorPropertyAdministrators
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        // Also clear portfolio links to this user's properties (covers any orphaned/admin-mismatch rows).
        var propiedadIds = await _db.Propiedades
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (adminIds.Count > 0)
        {
            // PortfolioPropertyId is a plain int on these tables (no formal FK), but clear them
            // before removing portfolio/admin rows so account data does not linger.
            await _db.IndorPropertyAdminPreventivePlans
                .Where(x => adminIds.Contains(x.AdministratorId))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.IndorPropertyAdminServiceRequests
                .Where(x => adminIds.Contains(x.AdministratorId))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.IndorPropertyAdminHomecarePlans
                .Where(x => adminIds.Contains(x.AdministratorId))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.IndorPropertyAdminScheduledVisits
                .Where(x => adminIds.Contains(x.AdministratorId))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.IndorPropertyAdminPortfolioProperties
                .Where(x => adminIds.Contains(x.AdministratorId))
                .ExecuteDeleteAsync(cancellationToken);

            await _db.IndorPropertyAdministrators
                .Where(a => adminIds.Contains(a.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (propiedadIds.Count > 0)
        {
            await _db.IndorPropertyAdminPortfolioProperties
                .Where(x => x.PropiedadId != null && propiedadIds.Contains(x.PropiedadId.Value))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }

    private async Task BestEffortAsync(string step, Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Account deletion: step '{Step}' failed but continuing.", step);
        }
    }
}
