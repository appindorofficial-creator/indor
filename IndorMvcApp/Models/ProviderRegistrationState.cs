namespace IndorMvcApp.Models;

public class ProviderRegistrationState
{
    public const int TotalSteps = 6;
    public const string ElectricalCategoryId = "electrical";
    public const int ExamPassingPercent = 80;

    public List<string> SelectedCategoryIds { get; set; } = [];
    public List<string> SelectedServiceIds { get; set; } = [];

    public string ProviderType { get; set; } = "Company";
    public string BusinessName { get; set; } = "";
    public string DbaName { get; set; } = "";
    public string PrimaryContact { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string YearsExperience { get; set; } = "";
    public List<string> Languages { get; set; } = ["English"];
    public string? LicenseNumber { get; set; }
    public string PrimaryCity { get; set; } = "Charlotte, NC";
    public int TravelRadiusMiles { get; set; } = 25;
    public List<string> ZipOrNeighborhoods { get; set; } = ["28202", "28203", "28205"];
    public bool EmergencyService { get; set; } = true;
    public bool SameDayJobs { get; set; } = true;
    public List<string> AvailableDays { get; set; } = ["Mon", "Tue", "Wed", "Thu", "Fri"];
    public string PreferredHours { get; set; } = "8:00 AM – 6:00 PM";
    public List<string> JobSizes { get; set; } = ["small", "standard", "large"];
    public bool LogoUploaded { get; set; }
    public bool ScopeTradeUnderstood { get; set; }
    public bool ScopeStandardsAgreed { get; set; }
    public Dictionary<int, string> ExamAnswers { get; set; } = new();
    public int ExamScorePercent { get; set; }
    public bool? ExamPassed { get; set; }
    public bool ProfileSubmitted { get; set; }

    public bool IsElectricianOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(ElectricalCategoryId, StringComparison.OrdinalIgnoreCase);

    public string DisplayCompanyName =>
        !string.IsNullOrWhiteSpace(DbaName) ? DbaName :
        !string.IsNullOrWhiteSpace(BusinessName) ? BusinessName : PrimaryContact;
}

public record OnboardingOption(string Id, string Label, string IconClass);

public record ExamQuestion(int Number, string Text, IReadOnlyList<string> Options, int CorrectIndex);
