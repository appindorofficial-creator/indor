using IndorMvcApp.Models;

namespace IndorMvcApp.ViewModels;

public class RealtorPropertyFileStepViewModel
{
    public int DisplayStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public string Title { get; set; } = "Create Property File";
    public string Subtitle { get; set; } = "";
}

public class RealtorPropertyFileDetailsViewModel : RealtorPropertyFileStepViewModel
{
    public string? SearchQuery { get; set; }
    public int? SelectedPropertyId { get; set; }
    public string FilePhase { get; set; } = RealtorPropertyFilePhases.PreClosing;
    public List<RealtorPropertyPickerViewModel> Properties { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string Icon)> FilePhaseOptions { get; set; } = [];
}

public class RealtorPropertyPickerViewModel
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string CityRegion { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string DisplayAddress { get; set; } = "";
}

public class RealtorPropertyFileAddItemsViewModel : RealtorPropertyFileStepViewModel
{
    public string PropertyDisplay { get; set; } = "";
    public string FilePhaseLabel { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public List<RealtorPropertyFileCategoryOptionViewModel> Categories { get; set; } = [];
}

public class RealtorPropertyFileCategoryOptionViewModel
{
    public string CategoryType { get; set; } = "";
    public string Label { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public bool Selected { get; set; }
}

public class RealtorPropertyFileContentViewModel : RealtorPropertyFileStepViewModel
{
    public string PropertyDisplay { get; set; } = "";
    public string FilePhaseLabel { get; set; } = "";
    public List<RealtorPropertyFileContentSectionViewModel> Sections { get; set; } = [];
}

public class RealtorPropertyFileContentSectionViewModel
{
    public string CategoryType { get; set; } = "";
    public string Label { get; set; } = "";
    public List<RealtorPropertyFileItemCardViewModel> Items { get; set; } = [];
    public string? NoteText { get; set; }
}

public class RealtorPropertyFileItemCardViewModel
{
    public int Id { get; set; }
    public string ItemLabel { get; set; } = "";
    public string? FileUrl { get; set; }
    public string? SizeLabel { get; set; }
    public string? ExpirationLabel { get; set; }
    public bool IsImage { get; set; }
}

public class RealtorPropertyFileReviewViewModel : RealtorPropertyFileStepViewModel
{
    public string PropertyDisplay { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string FilePhaseLabel { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public List<RealtorPropertyFileReviewItemViewModel> IncludedItems { get; set; } = [];
    public bool CreateAndContinueLater { get; set; }
}

public class RealtorPropertyFileReviewItemViewModel
{
    public string Label { get; set; } = "";
    public string CountLabel { get; set; } = "";
    public string Icon { get; set; } = "";
}

public class RealtorPropertyFileSuccessViewModel
{
    public int PropertyFileId { get; set; }
    public string FilePhaseLabel { get; set; } = "";
    public string PropertyDisplay { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string AddedNowLabel { get; set; } = "";
    public string StatusLabel { get; set; } = "Active";
}
