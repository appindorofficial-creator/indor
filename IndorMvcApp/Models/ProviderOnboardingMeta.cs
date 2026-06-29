namespace IndorMvcApp.Models;

public class ProviderOnboardingMeta
{
    public string OnboardingPath { get; set; } = "";

    public bool AssessmentSkipped { get; set; }

    public bool AssessmentStarted { get; set; }

    public bool TermsAccepted { get; set; }

    public string? Website { get; set; }

    public string? EinNumber { get; set; }

    public string? ActivationCallSlot { get; set; }

    public bool ActivationCallScheduled { get; set; }

    public bool IndorProActive { get; set; }

    public bool UsesNewWizard { get; set; } = true;

    public bool NotifyJobAlerts { get; set; } = true;

    public bool NotifyLeadUpdates { get; set; } = true;

    public bool NotifyPaymentAlerts { get; set; } = true;

    public bool NotifyReportReminders { get; set; } = true;
}
