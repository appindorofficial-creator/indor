namespace IndorMvcApp.Services;

public class AttomOptions
{
    public const string SectionName = "Attom";

    public string BaseUrl { get; set; } = "https://api.gateway.attomdata.com/";
    public string? ApiKey { get; set; }
    public bool Enabled { get; set; } = true;
}
