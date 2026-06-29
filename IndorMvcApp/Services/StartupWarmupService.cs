using IndorMvcApp.Data;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

/// <summary>
/// Pre-builds the EF Core model and opens a pooled DB connection during host startup so the
/// first real user request doesn't pay the cold-start cost (EF model build for ~100 entities +
/// JIT + connection pool warmup), which is what makes the app sit blank for several seconds
/// after an IIS app-pool recycle. Runs in the background and never blocks/crashes startup.
/// Combine with IIS Application Initialization (preload) so this happens before any user request.
/// </summary>
public sealed class StartupWarmupService(
    IServiceScopeFactory scopeFactory,
    ILogger<StartupWarmupService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Touching a small table forces the EF model to build and a connection to open.
                await db.Microservicios
                    .AsNoTracking()
                    .Select(m => m.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                await ProviderDatabaseSchemaInitializer.EnsureEditProfileColumnsAsync(db, logger, cancellationToken);

                logger.LogInformation("Startup warmup completed: EF model built and DB connection ready.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Startup warmup failed (non-fatal). First request may be slower.");
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
