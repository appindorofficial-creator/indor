using IndorMvcApp.Models;

namespace IndorMvcApp.ViewModels;

public class ProviderRegistrationStepViewModel
{
    public int Step { get; init; }
    public int TotalSteps { get; init; } = ProviderRegistrationState.TotalSteps;
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string? BackUrl { get; init; }
    public ProviderRegistrationState State { get; init; } = new();
    public bool ShowSaveLater { get; set; } = true;
}
