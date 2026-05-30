namespace IndorMvcApp.ViewModels;

public class SchoolsFlowViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? DistrictName { get; set; }
    public string? DistrictNote { get; set; }
    public string ActiveTab { get; set; } = "assigned";
    public bool HasData { get; set; }
    public List<SchoolItemViewModel> AssignedSchools { get; set; } = new();
    public List<SchoolItemViewModel> NearbySchools { get; set; } = new();
    public List<SchoolItemViewModel> AllSchools { get; set; } = new();
    public List<SchoolProgramViewModel> DistrictPrograms { get; set; } = new();
}

public class SchoolItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = "School";
    public string Grades { get; set; } = "—";
    public string SchoolType { get; set; } = "School";
    public string? Distance { get; set; }
    public string? CommuteTime { get; set; }
    public string? RatingValue { get; set; }
    public string? RatingLabel { get; set; }
    public bool IsAssigned { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? District { get; set; }
    public string? Enrollment { get; set; }
    public string Icon { get; set; } = "fa-school";
    public string Tone { get; set; } = "blue";
    public List<SchoolProgramViewModel> Programs { get; set; } = new();
    public List<SchoolGlanceItemViewModel> AtAGlance { get; set; } = new();
    public string Subtitle => BuildSubtitle();

    private string BuildSubtitle()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(Grades) && Grades != "—") parts.Add(Grades);
        if (!string.IsNullOrWhiteSpace(SchoolType)) parts.Add(SchoolType);
        if (!string.IsNullOrWhiteSpace(Distance)) parts.Add(Distance);
        return parts.Count > 0 ? string.Join(" • ", parts) : "Details from saved property data";
    }
}

public class SchoolProgramViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-star";
    public string Tone { get; set; } = "blue";
}

public class SchoolGlanceItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
}

public class SchoolProfileViewModel
{
    public int PropiedadId { get; set; }
    public SchoolItemViewModel School { get; set; } = new();
    public string Address { get; set; } = string.Empty;
}

public class SchoolsCompareViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? DistrictName { get; set; }
    public string QuickTakeaway { get; set; } = string.Empty;
    public List<SchoolItemViewModel> AssignedSchools { get; set; } = new();
}

public class SchoolsMapViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? DistrictName { get; set; }
    public string? DistrictNote { get; set; }
    public List<SchoolItemViewModel> AssignedSchools { get; set; } = new();
    public List<SchoolItemViewModel> NearbySchools { get; set; } = new();
}
