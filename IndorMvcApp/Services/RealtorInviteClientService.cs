using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorInviteClientService(
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor,
    IRealtorRegistrationService registration) : IRealtorInviteClientService
{
    private const string InviteIdSessionKey = "RealtorInviteId";

    private const string DefaultWelcomeMessage =
        "Hi! I'd like to invite you to view project details and stay updated in INDOR. Let me know if you have any questions.";

    public async Task<IndorRealtorInvitation> EnsureDraftAsync(CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        var session = httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");

        var inviteId = session.GetInt32(InviteIdSessionKey);
        if (inviteId is > 0)
        {
            var existing = await db.IndorRealtorInvitations
                .FirstOrDefaultAsync(i => i.Id == inviteId && i.RealtorId == realtor.Id &&
                                          i.Status == RealtorInvitationStatuses.Draft, cancellationToken);
            if (existing != null)
            {
                return existing;
            }
        }

        var entity = new IndorRealtorInvitation
        {
            RealtorId = realtor.Id,
            Status = RealtorInvitationStatuses.Draft,
            CurrentStep = 1,
            WelcomeMessage = DefaultWelcomeMessage,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorRealtorInvitations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        session.SetInt32(InviteIdSessionKey, entity.Id);
        return entity;
    }

    public async Task<IndorRealtorInvitation?> GetDraftAsync(CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return null;
        }

        var session = httpContextAccessor.HttpContext?.Session;
        var inviteId = session?.GetInt32(InviteIdSessionKey);
        if (inviteId is not > 0)
        {
            return null;
        }

        return await db.IndorRealtorInvitations
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.RealtorId == realtor.Id &&
                                        i.Status == RealtorInvitationStatuses.Draft, cancellationToken);
    }

    public async Task CancelDraftAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken);
        if (draft == null)
        {
            httpContextAccessor.HttpContext?.Session.Remove(InviteIdSessionKey);
            return;
        }

        db.IndorRealtorInvitations.Remove(draft);
        await db.SaveChangesAsync(cancellationToken);
        httpContextAccessor.HttpContext?.Session.Remove(InviteIdSessionKey);
    }

    public string ResolveResumeAction(int currentStep) => currentStep switch
    {
        <= 1 => "ClientInfo",
        2 => "Property",
        3 => "Access",
        4 => "Review",
        _ => "ClientInfo"
    };

    public async Task<RealtorInviteClientInfoViewModel> BuildClientInfoAsync(CancellationToken cancellationToken = default)
    {
        var draft = await EnsureDraftAsync(cancellationToken);
        return new RealtorInviteClientInfoViewModel
        {
            DisplayStep = 1,
            Subtitle = "Invite a client to collaborate on properties and projects.",
            FullName = draft.FullName,
            Email = draft.Email,
            Phone = draft.Phone ?? "",
            ClientRole = string.IsNullOrWhiteSpace(draft.ClientRole) ? RealtorClientRoles.Buyer : draft.ClientRole,
            QuickNote = draft.QuickNote ?? "",
            ClientRoles = RealtorClientRoles.All
        };
    }

    public async Task SaveClientInfoAsync(
        string fullName, string email, string phone, string clientRole, string quickNote,
        CancellationToken cancellationToken = default)
    {
        var draft = await EnsureDraftAsync(cancellationToken);
        draft.FullName = fullName.Trim();
        draft.Email = email.Trim();
        draft.Phone = phone?.Trim();
        draft.ClientRole = string.IsNullOrWhiteSpace(clientRole) ? RealtorClientRoles.Buyer : clientRole.Trim();
        draft.QuickNote = quickNote?.Trim().Length > 250 ? quickNote.Trim()[..250] : quickNote?.Trim();
        draft.CurrentStep = 2;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorInvitePropertyViewModel> BuildPropertyAsync(string? search, CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draft = await GetDraftAsync(cancellationToken);
        if (draft == null || string.IsNullOrWhiteSpace(draft.FullName))
        {
            throw new InvalidOperationException("Complete client info first.");
        }

        var query = db.IndorRealtorPropertyFiles.AsNoTracking()
            .Where(p => p.RealtorId == realtor.Id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Address.Contains(term) ||
                p.Title.Contains(term) ||
                (p.CityRegion != null && p.CityRegion.Contains(term)));
        }

        var properties = await query
            .OrderByDescending(p => p.UpdatedUtc ?? p.FechaCreacion)
            .Take(20)
            .ToListAsync(cancellationToken);

        return new RealtorInvitePropertyViewModel
        {
            DisplayStep = 2,
            Subtitle = "Choose the property this client will collaborate on",
            SearchQuery = search,
            SelectedPropertyFileId = draft.PropertyFileId,
            Properties = properties.Select(MapPropertyOption).ToList()
        };
    }

    public async Task SavePropertyAsync(int? propertyFileId, CancellationToken cancellationToken = default)
    {
        var draft = await EnsureDraftAsync(cancellationToken);
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        if (propertyFileId is not > 0)
        {
            throw new InvalidOperationException("Select a property.");
        }

        var property = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == propertyFileId && p.RealtorId == realtor.Id, cancellationToken)
            ?? throw new InvalidOperationException("Property not found.");

        draft.PropertyFileId = property.Id;
        draft.PropertyAddress = property.Address;
        draft.PropertyCityRegion = property.CityRegion;
        draft.PropertyLabel = MapPropertyLabel(property);
        draft.PropertyStatusLabel = property.FilePhase ?? property.Status;
        draft.CurrentStep = 3;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorInviteCreatePropertyViewModel> BuildCreatePropertyAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken);
        if (draft == null || string.IsNullOrWhiteSpace(draft.FullName))
        {
            throw new InvalidOperationException("Complete client info first.");
        }

        return new RealtorInviteCreatePropertyViewModel
        {
            DisplayStep = 2,
            Title = "Create New Property",
            Subtitle = "Add the property and it will be added automatically for this client invitation.",
            States = registration.GetLicenseStates(),
            PropertyTypes = RealtorPropertyTypes.Options
        };
    }

    public async Task<int> CreatePropertyAsync(RealtorInviteCreatePropertyViewModel model, CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draft = await EnsureDraftAsync(cancellationToken);

        var address = model.Address?.Trim() ?? "";
        var city = model.City?.Trim() ?? "";
        var state = model.StateCode?.Trim() ?? "";
        var zip = model.PostalCode?.Trim() ?? "";
        var nickname = model.Nickname?.Trim() ?? "";
        var unit = model.Unit?.Trim() ?? "";
        var propertyType = RealtorPropertyTypes.IsValid(model.PropertyType)
            ? model.PropertyType.Trim()
            : RealtorPropertyTypes.SingleFamily;

        var cityRegion = string.Join(", ",
            new[] { city, string.Join(" ", new[] { state, zip }.Where(s => s.Length > 0)) }
                .Where(s => s.Length > 0));

        var property = new IndorRealtorPropertyFile
        {
            RealtorId = realtor.Id,
            Title = nickname.Length > 0 ? nickname : address,
            Address = address,
            Unit = unit.Length > 0 ? unit : null,
            CityRegion = cityRegion.Length > 0 ? cityRegion : null,
            StateCode = state.Length > 0 ? state : null,
            PostalCode = zip.Length > 0 ? zip : null,
            PropertyType = propertyType,
            ClientName = draft.FullName,
            Status = "Active",
            FilePhase = RealtorPropertyFilePhases.General,
            UpdatedUtc = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorRealtorPropertyFiles.Add(property);
        await db.SaveChangesAsync(cancellationToken);

        if (model.SelectForClient)
        {
            draft.PropertyFileId = property.Id;
            draft.PropertyAddress = property.Address;
            draft.PropertyCityRegion = property.CityRegion;
            draft.PropertyLabel = MapPropertyLabel(property);
            draft.PropertyStatusLabel = property.FilePhase ?? property.Status;
            draft.FechaActualizacion = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "file",
            Description = $"Created property {property.Address}",
            CategoryTag = "Properties",
            OccurredUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);

        return property.Id;
    }

    public async Task<RealtorInviteAccessViewModel> BuildAccessAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken);
        if (draft == null || draft.PropertyFileId is not > 0)
        {
            throw new InvalidOperationException("Select a property first.");
        }

        return new RealtorInviteAccessViewModel
        {
            DisplayStep = 3,
            Subtitle = "Choose what the client can access",
            AccessPropertyOverview = draft.AccessPropertyOverview,
            AccessFilesReports = draft.AccessFilesReports,
            AccessQuotesEstimates = draft.AccessQuotesEstimates,
            AccessMessages = draft.AccessMessages,
            AccessProjectUpdates = draft.AccessProjectUpdates,
            AccessPayments = draft.AccessPayments,
            CollaborationLevel = draft.CollaborationLevel,
            DeliveryEmail = draft.DeliveryEmail,
            DeliveryText = draft.DeliveryText,
            WelcomeMessage = string.IsNullOrWhiteSpace(draft.WelcomeMessage)
                ? DefaultWelcomeMessage
                : draft.WelcomeMessage,
            CollaborationOptions = RealtorCollaborationLevels.Options
        };
    }

    public async Task SaveAccessAsync(RealtorInviteAccessViewModel model, CancellationToken cancellationToken = default)
    {
        var draft = await EnsureDraftAsync(cancellationToken);
        draft.AccessPropertyOverview = model.AccessPropertyOverview;
        draft.AccessFilesReports = model.AccessFilesReports;
        draft.AccessQuotesEstimates = model.AccessQuotesEstimates;
        draft.AccessMessages = model.AccessMessages;
        draft.AccessProjectUpdates = model.AccessProjectUpdates;
        draft.AccessPayments = model.AccessPayments;
        draft.CollaborationLevel = string.IsNullOrWhiteSpace(model.CollaborationLevel)
            ? RealtorCollaborationLevels.CanComment
            : model.CollaborationLevel;
        draft.DeliveryEmail = model.DeliveryEmail;
        draft.DeliveryText = model.DeliveryText;

        if (!draft.DeliveryEmail && !draft.DeliveryText)
        {
            draft.DeliveryEmail = true;
        }

        draft.WelcomeMessage = model.WelcomeMessage?.Trim().Length > 250
            ? model.WelcomeMessage.Trim()[..250]
            : model.WelcomeMessage?.Trim() ?? DefaultWelcomeMessage;
        draft.CurrentStep = 4;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorInviteReviewViewModel> BuildReviewAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken);
        if (draft == null || draft.CurrentStep < 4)
        {
            throw new InvalidOperationException("Complete all steps first.");
        }

        return new RealtorInviteReviewViewModel
        {
            DisplayStep = 4,
            Subtitle = "Confirm the client details before sending.",
            FullName = draft.FullName,
            Email = draft.Email,
            Phone = draft.Phone ?? "",
            ClientRole = draft.ClientRole ?? RealtorClientRoles.Buyer,
            PropertyDisplay = FormatPropertyDisplay(draft),
            AccessSummary = BuildAccessSummary(draft),
            CollaborationLabel = FormatCollaboration(draft.CollaborationLevel),
            DeliveryLabel = FormatDelivery(draft),
            WelcomeMessage = draft.WelcomeMessage ?? "",
            SendReminder48h = draft.SendReminder48h
        };
    }

    public async Task<int> SendInvitationAsync(bool sendReminder48h, CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("No draft invitation found.");
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        draft.Status = RealtorInvitationStatuses.Sent;
        draft.CurrentStep = 5;
        draft.SendReminder48h = sendReminder48h;
        draft.SentUtc = DateTime.UtcNow;
        draft.FechaActualizacion = DateTime.UtcNow;

        if (draft.InvitationToken == Guid.Empty)
        {
            draft.InvitationToken = Guid.NewGuid();
        }

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "link",
            Description = $"Invitation sent to {draft.FullName} for {FormatPropertyDisplay(draft)}",
            CategoryTag = "Clients",
            OccurredUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        httpContextAccessor.HttpContext?.Session.Remove(InviteIdSessionKey);
        return draft.Id;
    }

    public async Task<RealtorInviteSuccessViewModel> BuildSuccessAsync(int invitationId, CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        var invitation = await db.IndorRealtorInvitations.AsNoTracking()
            .FirstAsync(i => i.Id == invitationId && i.RealtorId == realtor.Id, cancellationToken);

        var pending = await db.IndorRealtorInvitations.AsNoTracking()
            .Where(i => i.RealtorId == realtor.Id && i.Status == RealtorInvitationStatuses.Sent)
            .OrderByDescending(i => i.SentUtc)
            .Take(5)
            .ToListAsync(cancellationToken);

        var request = httpContextAccessor.HttpContext?.Request;
        var inviteLink = request == null
            ? $"/invite/{invitation.InvitationToken}"
            : $"{request.Scheme}://{request.Host}/invite/{invitation.InvitationToken}";

        return new RealtorInviteSuccessViewModel
        {
            FullName = invitation.FullName,
            Email = invitation.Email,
            Phone = invitation.Phone ?? "",
            PropertyDisplay = FormatPropertyDisplay(invitation),
            StatusLabel = "Pending Acceptance",
            InviteLink = inviteLink,
            PendingInvitations = pending.Select(MapPendingCard).ToList()
        };
    }

    private static RealtorInvitePropertyOptionViewModel MapPropertyOption(IndorRealtorPropertyFile file)
    {
        var label = MapPropertyLabel(file);
        var icon = label.Contains("Condo", StringComparison.OrdinalIgnoreCase) ? "fa-building" : "fa-house";
        return new RealtorInvitePropertyOptionViewModel
        {
            Id = file.Id,
            Address = file.Address,
            Label = label,
            CityRegion = file.CityRegion ?? "",
            StatusLabel = file.FilePhase ?? file.Status,
            Icon = icon
        };
    }

    private static string MapPropertyLabel(IndorRealtorPropertyFile file)
    {
        if (!string.IsNullOrWhiteSpace(file.FilePhase))
        {
            return file.FilePhase;
        }

        return file.ClientName != null ? $"{file.ClientName} File" : file.Title;
    }

    private static string FormatPropertyDisplay(IndorRealtorInvitation inv) =>
        string.IsNullOrWhiteSpace(inv.PropertyCityRegion)
            ? inv.PropertyAddress ?? ""
            : $"{inv.PropertyAddress}, {inv.PropertyCityRegion}";

    private static string BuildAccessSummary(IndorRealtorInvitation inv)
    {
        var parts = new List<string>();
        if (inv.AccessPropertyOverview) parts.Add("Property Overview");
        if (inv.AccessFilesReports) parts.Add("Files & Reports");
        if (inv.AccessQuotesEstimates) parts.Add("Quotes & Estimates");
        if (inv.AccessMessages) parts.Add("Messages");
        if (inv.AccessProjectUpdates) parts.Add("Project Updates");
        if (inv.AccessPayments) parts.Add("Payments");
        return string.Join(", ", parts);
    }

    private static string FormatCollaboration(string level) => level switch
    {
        RealtorCollaborationLevels.ViewOnly => "View Only",
        RealtorCollaborationLevels.CanUpload => "Can Upload / Collaborate",
        _ => "Can Comment"
    };

    private static string FormatDelivery(IndorRealtorInvitation inv) => inv.DeliveryEmail switch
    {
        true when inv.DeliveryText => "Email + Text Message",
        true => "Email",
        _ => "Text Message"
    };

    private static RealtorInvitationCardViewModel MapPendingCard(IndorRealtorInvitation inv)
    {
        var parts = inv.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[^1][0]}"
            : inv.FullName.Length > 0 ? inv.FullName[..1] : "?";

        return new RealtorInvitationCardViewModel
        {
            Id = inv.Id,
            FullName = inv.FullName,
            Email = inv.Email,
            Initials = initials.ToUpperInvariant(),
            SentLabel = inv.SentUtc.ToLocalTime().ToString("MMM d, yyyy")
        };
    }
}
