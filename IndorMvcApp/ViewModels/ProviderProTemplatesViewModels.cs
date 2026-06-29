namespace IndorMvcApp.ViewModels;

public class ReportTemplateSectionView
{
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "fa-circle";
}

public class ReportTemplateView
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "fa-clipboard";
    public string Color { get; set; } = "blue";
    public string? Badge { get; set; }
    public string Category { get; set; } = "Reports";
    public bool IsCustom { get; set; }
    public List<ReportTemplateSectionView> Sections { get; set; } = [];

    public string SavedTo => IsCustom ? "My Templates" : "Most Used";
}

public class ProviderProTemplatesPageViewModel
{
    public List<ReportTemplateView> MostUsed { get; set; } = [];
    public List<ReportTemplateView> MyTemplates { get; set; } = [];
}
