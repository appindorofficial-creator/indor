namespace IndorMvcApp.ViewModels;

/// <summary>
/// Shared wizard chrome (header, stepper, footer) for service flows and portals.
/// </summary>
public class IndorWizardShellViewModel
{
    public string PageTitle { get; set; } = string.Empty;
    public int DisplayStep { get; set; }
    public int TotalSteps { get; set; }
    public string? BackUrl { get; set; }
    public string? CloseUrl { get; set; }
    public bool ShowClose { get; set; } = true;
    public bool HideStepper { get; set; }
    public string? ContinueLabel { get; set; }
    public string? ContinueFormId { get; set; }
}
