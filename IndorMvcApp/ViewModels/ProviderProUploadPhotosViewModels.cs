namespace IndorMvcApp.ViewModels;

public class ProviderProUploadPhotosDraft
{
    public int? JobId { get; set; }
    public string Notes { get; set; } = "";
    public List<ProviderProUploadReportFileSlot> Photos { get; set; } = [];
}

public class ProviderProUploadPhotosSelectJobViewModel : ProviderProPageBaseViewModel
{
    public string? SearchQuery { get; set; }
    public string ActiveFilter { get; set; } = "all";
    public int TotalJobsAvailable { get; set; }
    public bool HasSearchWithNoResults { get; set; }
    public List<ProviderProUploadPhotosJobOptionViewModel> Jobs { get; set; } = [];
}

public class ProviderProUploadPhotosJobOptionViewModel
{
    public int JobId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "";
    public string IconClass { get; set; } = "fa-wrench";
    public string? ImageUrl { get; set; }
    public int PhotosCount { get; set; }
}

public class ProviderProUploadPhotosJobSummary
{
    public int JobId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "";
    public string IconClass { get; set; } = "fa-house";
    public string? ImageUrl { get; set; }
    public int ExistingPhotosCount { get; set; }
}

public class ProviderProUploadPhotosItem
{
    public int Index { get; set; }
    public string Url { get; set; } = "";
    public string Category { get; set; } = "After";
}

public class ProviderProUploadPhotosAddViewModel : ProviderProPageBaseViewModel
{
    public ProviderProUploadPhotosJobSummary Job { get; set; } = new();
    public List<ProviderProUploadPhotosItem> NewPhotos { get; set; } = [];
}

public class ProviderProUploadPhotosReviewViewModel : ProviderProPageBaseViewModel
{
    public ProviderProUploadPhotosJobSummary Job { get; set; } = new();
    public List<ProviderProUploadPhotosItem> Photos { get; set; } = [];
    public string Notes { get; set; } = "";
    public int NewCount { get; set; }
    public int TotalCount { get; set; }
}
