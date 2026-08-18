namespace IndorMvcApp.ViewModels;

public class ProviderProSendReportsPageViewModel : ProviderProPageBaseViewModel
{
    public string? SearchQuery { get; set; }
    public int? SelectedReportId { get; set; }
    public List<ProviderProSendReportOptionViewModel> Reports { get; set; } = [];
}

public class ProviderProSendReportOptionViewModel
{
    public int ReportId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "ready";
    public string IconClass { get; set; } = "fa-file-lines";
}

public class ProviderProSendReportReviewViewModel : ProviderProPageBaseViewModel
{
    public int ReportId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string? CustomerEmail { get; set; }
    public string ReportCode { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "ready";
    public string IconClass { get; set; } = "fa-file-lines";
    public string DeliveryMethod { get; set; } = "email";
    public string CustomerMessage { get; set; } = "Your completion report is ready to review.";
    public bool RequestApproval { get; set; }
}

public class ProviderProSendReportInput
{
    public int ReportId { get; set; }
    public string? DeliveryMethod { get; set; }
    public string? CustomerMessage { get; set; }
    public bool RequestApproval { get; set; }
}
