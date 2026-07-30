namespace IndorMvcApp.ViewModels;

public class HomeownerMessageProviderViewModel
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? TradeLabel { get; set; }
    public string? PhotoUrl { get; set; }
    public string IconClass { get; set; } = "fa-screwdriver-wrench";
    public string Body { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string BackUrl { get; set; } = "/";
}

public class HomeownerMessageProviderInput
{
    public int ProviderId { get; set; }
    public string? Body { get; set; }
}

public class HomeownerMessageSentViewModel
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string BackUrl { get; set; } = "/";
    public string ServicesUrl { get; set; } = "/";
}
