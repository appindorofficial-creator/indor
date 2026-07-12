namespace IndorMvcApp.ViewModels;

public sealed class GuestEmergencyChooseViewModel
{
    public required IReadOnlyList<GuestEmergencyOptionViewModel> Options { get; init; }
    public int? SelectedId { get; init; }
}

public sealed class GuestEmergencyOptionViewModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string IconClass { get; init; }
}

public sealed class GuestEmergencyDetailsViewModel
{
    public int ServicioEmergenciaId { get; init; }
    public required string ServiceName { get; init; }
    public required string ServiceTitle { get; init; }
    public string? Description { get; init; }
    public string WhenNeeded { get; init; } = "ASAP";
}

public sealed class GuestEmergencySearchingViewModel
{
    public required string ServiceTitle { get; init; }
    public int RadiusMiles { get; init; } = 3;
}

public sealed class GuestEmergencySentViewModel
{
    public required string RequestId { get; init; }
    public required string ServiceTitle { get; init; }
    public required string ResumeUrl { get; init; }
}

public sealed class GuestEmergencyQuickCategoryViewModel
{
    public int Id { get; init; }
    public required string Label { get; init; }
    public required string Subtitle { get; init; }
    public required string IconClass { get; init; }
    public required string Url { get; init; }
}
