using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace IndorMvcApp.ViewModels;

public class NeighborRequestWizardShellViewModel
{
    public int PropiedadId { get; set; }
    public string PageTitle { get; set; } = "Post a Request";
    public int DisplayStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 5;
    public string? BackUrl { get; set; }
    public string CloseUrl { get; set; } = "/Home/Index";
}

public class NeighborRequestCategoryStepViewModel : NeighborRequestWizardShellViewModel
{
    public int? SelectedCategoryId { get; set; }

    public List<NeighborRequestCategoryOptionViewModel> Categories { get; set; } = [];
}

public class NeighborRequestCategoryOptionViewModel
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string IconClass { get; set; } = "fa-circle";
}

public class NeighborRequestDescribeStepViewModel : NeighborRequestWizardShellViewModel
{
    public string CategoryLabel { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, MaxLength(500)]
    public string LocationAddress { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? NeededByDate { get; set; }

    public List<string> ExistingPhotoUrls { get; set; } = [];

    public List<IFormFile>? PhotoFiles { get; set; }
}

public class NeighborRequestPreferencesStepViewModel : NeighborRequestWizardShellViewModel
{
    public string TimelineCode { get; set; } = "ThisWeek";
    public string AudienceCode { get; set; } = "Neighbors";
    public decimal? BudgetAmount { get; set; }
    public IReadOnlyList<(string Value, string Label)> TimelineOptions { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string Subtitle, string IconClass)> AudienceOptions { get; set; } = [];
}

public class NeighborRequestReviewStepViewModel : NeighborRequestWizardShellViewModel
{
    public string CategoryLabel { get; set; } = string.Empty;
    public string CategoryIconClass { get; set; } = "fa-circle";
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> PhotoUrls { get; set; } = [];
    public string LocationAddress { get; set; } = string.Empty;
    public string TimelineLabel { get; set; } = string.Empty;
    public string AudienceLabel { get; set; } = string.Empty;
    public string? NeededByLabel { get; set; }
    public string? BudgetLabel { get; set; }
}

public class NeighborRequestSuccessViewModel
{
    public int RequestId { get; set; }
    public int PropiedadId { get; set; }
}

public class NeighborRequestListViewModel
{
    public int PropiedadId { get; set; }
    public string ActiveTab { get; set; } = "Active";
    public IReadOnlyList<string> Tabs { get; set; } = ["Active", "InProgress", "Completed"];
    public List<NeighborRequestListItemViewModel> Items { get; set; } = [];
}

public class NeighborRequestListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public string PostedLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string IconClass { get; set; } = "fa-comment-dots";
    public int OfferCount { get; set; }
    public string OfferCountLabel { get; set; } = string.Empty;
    public string DetailUrl { get; set; } = "#";
}

public class NeighborRequestDetailViewModel
{
    public int Id { get; set; }
    public int PropiedadId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public string IconClass { get; set; } = "fa-paint-roller";
    public string StatusLabel { get; set; } = "ACTIVE";
    public string StatusCss { get; set; } = "active";
    public string PostedLabel { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> PhotoUrls { get; set; } = [];
    public List<NeighborRequestOfferItemViewModel> Offers { get; set; } = [];
    public string OfferCountLabel { get; set; } = string.Empty;
}

public class NeighborRequestOfferItemViewModel
{
    public int Id { get; set; }
    public string OffererName { get; set; } = string.Empty;
    public string? OffererPhotoUrl { get; set; }
    public string? Message { get; set; }
    public string? PriceLabel { get; set; }
    public string MetaLabel { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public string DetailUrl { get; set; } = "#";
}
