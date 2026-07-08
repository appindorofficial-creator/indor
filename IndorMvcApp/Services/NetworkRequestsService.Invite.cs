using System.Text.Json;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public sealed partial class NetworkRequestsService
{
    public async Task<InviteToJobViewModel?> GetInviteAsync(
        IndorProveedor me, int subcontractorId, CancellationToken cancellationToken = default)
    {
        var sub = await LoadProviderAsync(subcontractorId, cancellationToken);
        if (sub == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var header = BuildSubHeader(sub, me, catalog);
        var (tradeId, tradeLabel) = PrimaryTrade(sub, catalog);

        return new InviteToJobViewModel
        {
            Sub = header,
            TradeId = tradeId,
            ServiceCategory = tradeLabel,
            PropertyAddress = ComposeAddress(me),
            ScheduleToday = true,
            TimingPreference = NetworkInvitationTimings.Urgent
        };
    }

    public async Task<int?> SaveInviteAsync(
        IndorProveedor me, InviteToJobInput input, List<string> newAttachmentUrls, CancellationToken cancellationToken = default)
    {
        var sub = await db.IndorProveedores.AsNoTracking().FirstOrDefaultAsync(p => p.Id == input.SubcontractorId, cancellationToken);
        if (sub == null)
        {
            return null;
        }

        var attachments = (input.ExistingAttachments ?? [])
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Concat(newAttachmentUrls)
            .Distinct()
            .ToList();

        DateTime? scheduleDate = input.ScheduleToday
            ? DateTime.UtcNow.Date
            : DateTime.TryParse(input.ScheduleDate, out var d) ? d.Date : null;

        var isDraft = string.Equals(input.Mode, "draft", StringComparison.OrdinalIgnoreCase);

        var invitation = new IndorProveedorNetworkInvitacion
        {
            InviterProveedorId = me.Id,
            SubcontractorProveedorId = input.SubcontractorId,
            JobTitle = Clip(input.JobTitle, 160),
            TradeId = Clip(input.TradeId, 40),
            ServiceCategory = Clip(input.ServiceCategory, 120),
            PropertyAddress = Clip(input.PropertyAddress, 300),
            ScheduleToday = input.ScheduleToday,
            ScheduleDate = scheduleDate,
            BudgetRange = Clip(input.BudgetRange, 40),
            Description = Clip(input.Description, 600),
            TimingPreference = NormalizeTiming(input.TimingPreference),
            AttachmentsJson = attachments.Count > 0 ? JsonSerializer.Serialize(attachments) : null,
            Status = isDraft ? NetworkInvitationStatuses.Draft : NetworkInvitationStatuses.Sent,
            FechaCreacion = DateTime.UtcNow
        };

        try
        {
            db.IndorProveedorNetworkInvitaciones.Add(invitation);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            logger.LogWarning(ex, "Network invitations table missing; invitation not saved.");
            return null;
        }

        return invitation.Id;
    }

    public async Task<InvitationSentViewModel?> GetInvitationSentAsync(
        IndorProveedor me, int invitationId, CancellationToken cancellationToken = default)
    {
        IndorProveedorNetworkInvitacion? invitation;
        try
        {
            invitation = await db.IndorProveedorNetworkInvitaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == invitationId && i.InviterProveedorId == me.Id, cancellationToken);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }

        if (invitation == null)
        {
            return null;
        }

        var sub = await LoadProviderAsync(invitation.SubcontractorProveedorId, cancellationToken);
        if (sub == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var header = BuildSubHeader(sub, me, catalog);

        return new InvitationSentViewModel
        {
            Id = invitation.Id,
            Sub = header,
            JobTitle = string.IsNullOrWhiteSpace(invitation.JobTitle) ? "Job request" : invitation.JobTitle!,
            ScheduleLabel = ScheduleLabel(invitation),
            BudgetRange = invitation.BudgetRange ?? "—",
            Steps = BuildInviteSteps(invitation.Status)
        };
    }

    // ---------------------------------------------------------------- Helpers

    private InviteSubHeaderViewModel BuildSubHeader(
        IndorProveedor sub, IndorProveedor me, IReadOnlyDictionary<string, IndorProveedorCategoriaCatalogo> catalog)
    {
        var (tradeId, tradeLabel) = PrimaryTrade(sub, catalog);
        return new InviteSubHeaderViewModel
        {
            Id = sub.Id,
            Name = ResolveName(sub),
            TradeLabel = tradeLabel,
            IconClass = catalog.TryGetValue(tradeId ?? "", out var cat) ? cat.IconClass : "fa-screwdriver-wrench",
            IconTone = ToneForTrade(tradeId),
            PhotoUrl = LogoUrl(sub),
            LocationLabel = ComposeLocation(sub),
            DistanceLabel = DistanceLabel(me, sub),
            IsVerified = IsVerified(sub),
            IsInsured = sub.IsInsured,
            IsDocsReady = sub.Documentos.Any(d => d.UploadedUtc != null),
            IsAvailableNow = sub.SameDayJobs,
            AvailabilityLabel = sub.SameDayJobs ? "Available now" : sub.EmergencyService ? "Responds fast" : "Available this week"
        };
    }

    private static (string? Id, string? Label) PrimaryTrade(
        IndorProveedor p, IReadOnlyDictionary<string, IndorProveedorCategoriaCatalogo> catalog)
    {
        var primary = p.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
        if (primary != null && catalog.TryGetValue(primary, out var cat))
        {
            return (cat.Id, cat.LabelEn);
        }

        return (primary, p.ServiceDescription);
    }

    private static string? ComposeAddress(IndorProveedor p)
    {
        if (!string.IsNullOrWhiteSpace(p.BusinessAddress))
        {
            return p.BusinessAddress!.Trim();
        }

        return string.IsNullOrWhiteSpace(p.PrimaryCity) ? null : p.PrimaryCity!.Trim();
    }

    private static string NormalizeTiming(string? timing) => timing?.Trim() switch
    {
        NetworkInvitationTimings.ThisWeek => NetworkInvitationTimings.ThisWeek,
        NetworkInvitationTimings.Flexible => NetworkInvitationTimings.Flexible,
        _ => NetworkInvitationTimings.Urgent
    };

    private static string ScheduleLabel(IndorProveedorNetworkInvitacion inv)
    {
        if (inv.ScheduleToday || !inv.ScheduleDate.HasValue)
        {
            return "Today";
        }

        return inv.ScheduleDate.Value.ToLocalTime().ToString("ddd, MMM d, yyyy");
    }

    private static List<RequestStepViewModel> BuildInviteSteps(string status)
    {
        var order = new[]
        {
            NetworkInvitationStatuses.Sent,
            NetworkInvitationStatuses.Viewed,
            NetworkInvitationStatuses.Responded,
            NetworkInvitationStatuses.Hired
        };
        var current = Array.IndexOf(order, status);
        if (current < 0)
        {
            current = 0;
        }

        RequestStepViewModel Step(int idx, string label, string icon) => new()
        {
            Label = label,
            IconClass = icon,
            State = idx < current ? "done" : idx == current ? "active" : "pending"
        };

        return
        [
            Step(0, "Sent", "fa-paper-plane"),
            Step(1, "Viewed", "fa-eye"),
            Step(2, "Responded", "fa-comment-dots"),
            Step(3, "Hired", "fa-user")
        ];
    }

    private static string? DistanceLabel(IndorProveedor me, IndorProveedor other)
    {
        if (!me.Latitude.HasValue || !me.Longitude.HasValue || !other.Latitude.HasValue || !other.Longitude.HasValue)
        {
            return null;
        }

        var miles = HaversineMiles(
            (double)me.Latitude.Value, (double)me.Longitude.Value,
            (double)other.Latitude.Value, (double)other.Longitude.Value);
        return $"{miles:0.0} mi";
    }

    private static double HaversineMiles(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 3958.8;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static string? Clip(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();
        return value.Length > max ? value[..max] : value;
    }
}
