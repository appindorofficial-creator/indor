namespace IndorMvcApp.Services;

public sealed record PasswordResetEmailModel(
    string ToEmail,
    string Name,
    string Code,
    string ResetUrl,
    int ValidHours);

public interface IPasswordResetEmailSender
{
    Task SendPasswordResetEmailAsync(PasswordResetEmailModel model, CancellationToken cancellationToken = default);
}
