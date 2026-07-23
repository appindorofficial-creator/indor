namespace IndorMvcApp.ViewModels;

public class PropertyAdminViewingStripViewModel
{
    public PropertyAdministratorFlowPropertyViewModel Viewing { get; set; } = new();
    public string? BadgeLabel { get; set; }
    public bool AllowSwitch { get; set; } = true;
    public IReadOnlyList<PropertyAdminViewingStripOptionViewModel> Options { get; set; } = [];
}

public class PropertyAdminViewingStripOptionViewModel
{
    public int Id { get; set; }
    public string PropertyName { get; set; } = "";
    public string PropertyTypeLabel { get; set; } = "";
    public string Location { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string SwitchUrl { get; set; } = "#";
    public bool IsSelected { get; set; }
}
