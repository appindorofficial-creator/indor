namespace IndorMvcApp.Services;

public sealed record InvitationEmailModel(
    string ToEmail,
    string ClientName,
    string RealtorName,
    string PropertyDisplay,
    string AcceptUrl,
    string? WelcomeMessage);

public interface IInvitationEmailSender
{
    Task SendInvitationEmailAsync(InvitationEmailModel model, CancellationToken cancellationToken = default);
}
