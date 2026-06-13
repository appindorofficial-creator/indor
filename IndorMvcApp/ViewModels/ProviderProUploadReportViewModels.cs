using IndorMvcApp.Models;

namespace IndorMvcApp.ViewModels;

public class ProviderProUploadReportDraft
{
    public int? JobId { get; set; }
    public string ReportType { get; set; } = ProviderReportTypes.Completion;
    public bool AttachToHouseFacts { get; set; } = true;
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string WorkCompleted { get; set; } = "";
    public string MaterialsUsed { get; set; } = "";
    public string WarrantyInfo { get; set; } = "";
    public string Recommendations { get; set; } = "";
    public string InternalNotes { get; set; } = "";
    public bool SendToHomeowner { get; set; } = true;
    public bool RequestApproval { get; set; } = true;
    public bool CreateHouseFactsRecord { get; set; } = true;
    public List<ProviderProUploadReportFileSlot> PhotoSlots { get; set; } =
    [
        new() { Slot = "Before" },
        new() { Slot = "During" },
        new() { Slot = "After" },
        new() { Slot = "Final Result" }
    ];
    public List<ProviderProUploadReportFileSlot> DocumentSlots { get; set; } =
    [
        new() { Slot = "Invoice", Required = true },
        new() { Slot = "Warranty Document", Required = false },
        new() { Slot = "Permit / Receipt", Required = false }
    ];
    public List<ProviderProUploadReportFileSlot> GeneralFiles { get; set; } = [];
}

public class ProviderProUploadReportFileSlot
{
    public string Slot { get; set; } = "";
    public string? Url { get; set; }
    public string? FileName { get; set; }
    public bool Required { get; set; }
}

public class ProviderProUploadReportJobSummaryViewModel
{
    public int JobId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "";
    public string DateLabel { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string JobCode { get; set; } = "";
    public string ScheduledLabel { get; set; } = "";
}

public class ProviderProUploadReportSelectJobViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 1;
    public int TotalSteps { get; set; } = 5;
    public string? SearchQuery { get; set; }
    public string ActiveFilter { get; set; } = "all";
    public int TotalJobsAvailable { get; set; }
    public bool HasSearchWithNoResults { get; set; }
    public List<ProviderProUploadReportJobOptionViewModel> Jobs { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProUploadReportJobOptionViewModel
{
    public int JobId { get; set; }
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string DateLabel { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "";
    public string IconClass { get; set; } = "fa-wrench";
    public string? ImageUrl { get; set; }
    public string JobCode { get; set; } = "";
}

public class ProviderProUploadReportTypeViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 2;
    public int TotalSteps { get; set; } = 5;
    public string ReportType { get; set; } = ProviderReportTypes.Completion;
    public ProviderProUploadReportJobSummaryViewModel Job { get; set; } = new();
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProUploadReportFilesViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 3;
    public int TotalSteps { get; set; } = 5;
    public ProviderProUploadReportJobSummaryViewModel Job { get; set; } = new();
    public bool AttachToHouseFacts { get; set; } = true;
    public List<ProviderProUploadReportFileSlot> PhotoSlots { get; set; } = [];
    public List<ProviderProUploadReportFileSlot> DocumentSlots { get; set; } = [];
    public List<ProviderProUploadReportFileSlot> GeneralFiles { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProUploadReportDetailsViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 4;
    public int TotalSteps { get; set; } = 5;
    public string ReportType { get; set; } = "";
    public ProviderProUploadReportJobSummaryViewModel Job { get; set; } = new();
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string WorkCompleted { get; set; } = "";
    public string MaterialsUsed { get; set; } = "";
    public string WarrantyInfo { get; set; } = "";
    public string Recommendations { get; set; } = "";
    public string InternalNotes { get; set; } = "";
    public bool SendToHomeowner { get; set; } = true;
    public bool RequestApproval { get; set; } = true;
    public bool CreateHouseFactsRecord { get; set; } = true;
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProUploadReportSuccessViewModel : ProviderProPageBaseViewModel
{
    public int ReportId { get; set; }
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string ReportType { get; set; } = "";
    public int FilesCount { get; set; }
    public string StatusLabel { get; set; } = "Ready to send";
    public int? JobId { get; set; }
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProUploadReportSelectJobInput
{
    public int JobId { get; set; }
    public string? Search { get; set; }
    public string? Filter { get; set; }
}

public class ProviderProUploadReportTypeInput
{
    public string ReportType { get; set; } = ProviderReportTypes.Completion;
}

public class ProviderProUploadReportFilesInput
{
    public bool AttachToHouseFacts { get; set; } = true;
}

public class ProviderProUploadReportDetailsInput
{
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string WorkCompleted { get; set; } = "";
    public string MaterialsUsed { get; set; } = "";
    public string WarrantyInfo { get; set; } = "";
    public string Recommendations { get; set; } = "";
    public string InternalNotes { get; set; } = "";
    public bool SendToHomeowner { get; set; } = true;
    public bool RequestApproval { get; set; } = true;
    public bool CreateHouseFactsRecord { get; set; } = true;
}
