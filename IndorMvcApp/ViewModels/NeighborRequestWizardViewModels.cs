using System.ComponentModel.DataAnnotations;
using IndorMvcApp.Models;
using Microsoft.AspNetCore.Http;

namespace IndorMvcApp.ViewModels;

public class NeighborRequestWizardShellViewModel
{
    public int PropiedadId { get; set; }
    public int? RequestId { get; set; }
    public bool IsEditMode { get; set; }
    public string PageTitle { get; set; } = "Post Quick Job";
    public int DisplayStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public string? BackUrl { get; set; }
    public string CloseUrl { get; set; } = "/Home/Index";
    public IReadOnlyList<string> StepLabels { get; set; } = ["Details", "Schedule", "Extras", "Helpers"];
}

public class NeighborRequestCategoryStepViewModel : NeighborRequestWizardShellViewModel
{
    public int? SelectedCategoryId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required, MaxLength(60)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    [Required, MaxLength(500)]
    public string LocationAddress { get; set; } = string.Empty;

    public bool UseHomeAddress { get; set; } = true;

    public List<NeighborRequestCategoryOptionViewModel> Categories { get; set; } = [];
}

public class NeighborRequestCategoryOptionViewModel
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string IconClass { get; set; } = "fa-circle";
    public string IllustrationClass { get; set; } = "nr-cat-ill--other";
    public string? ImageUrl { get; set; }
}

public class NeighborRequestDescribeStepViewModel : NeighborRequestWizardShellViewModel
{
    public string CategoryLabel { get; set; } = string.Empty;

    public List<string> ExistingPhotoUrls { get; set; } = [];

    public List<IFormFile>? PhotoFiles { get; set; }

    public List<string> SelectedTools { get; set; } = [];

    [MaxLength(250)]
    public string? SpecialNotes { get; set; }

    public string? PetsOnProperty { get; set; }
    public string? HasStairs { get; set; }
    public string? GateCode { get; set; }
    public string? ParkingAvailable { get; set; }

    public IReadOnlyList<(string Value, string Label, string IconClass)> ToolOptions { get; set; } = [];
}

public class NeighborRequestPreferencesStepViewModel : NeighborRequestWizardShellViewModel
{
    public string WhenCode { get; set; } = NeighborRequestTimelineCodes.Today;
    public string PreferredTimeCode { get; set; } = NeighborRequestPreferredTimeCodes.Flexible;
    public int HelperCount { get; set; } = 1;
    public string DurationCode { get; set; } = NeighborRequestDurationCodes.TwoHours;
    public string PayTypeCode { get; set; } = NeighborRequestPayTypeCodes.Hourly;
    public string TimelineCode { get; set; } = NeighborRequestTimelineCodes.Today;
    public string AudienceCode { get; set; } = NeighborRequestAudienceCodes.Neighbors;
    public List<string> SelectedAudiences { get; set; } = [];
    public decimal? BudgetAmount { get; set; }

    [DataType(DataType.Date)]
    public DateTime? NeededByDate { get; set; }

    public IReadOnlyList<(string Value, string Label, string IconClass)> WhenOptions { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string IconClass)> PreferredTimeOptions { get; set; } = [];
    public IReadOnlyList<(int Value, string Label, string IconClass)> HelperCountOptions { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string IconClass)> DurationOptions { get; set; } = [];
    public IReadOnlyList<(string Value, string Label)> TimelineOptions { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string Subtitle, string IconClass)> AudienceOptions { get; set; } = [];
}

public class NeighborRequestEditDetailsStepViewModel : NeighborRequestWizardShellViewModel
{
    public int CategoryId { get; set; }
    public string CategoryLabel { get; set; } = string.Empty;
    public string CategoryIconClass { get; set; } = "fa-house";

    [MaxLength(200)]
    public string? DetailsSummary { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime? NeededByDate { get; set; }

    public string? TimeWindowPreset { get; set; }

    [Required]
    public string AudienceCode { get; set; } = NeighborRequestAudienceCodes.Neighbors;

    public List<NeighborRequestCategoryOptionViewModel> Categories { get; set; } = [];
    public IReadOnlyList<(string Value, string Label)> TimeWindowOptions { get; set; } = [];
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
    public string? TimeWindowLabel { get; set; }
    public string? DetailsSummary { get; set; }
    public string? BudgetLabel { get; set; }
    public string PublishButtonLabel { get; set; } = "Post request";
    public string PublishSuccessUrl { get; set; } = string.Empty;
}

public class NeighborRequestSuccessViewModel
{
    public int RequestId { get; set; }
    public int PropiedadId { get; set; }
}

public class NeighborRequestHelpersStepViewModel : NeighborRequestWizardShellViewModel
{
    public string JobTitle { get; set; } = string.Empty;
    public string WhenLabel { get; set; } = string.Empty;
    public string TimeLabel { get; set; } = string.Empty;
    public string HelpersLabel { get; set; } = string.Empty;
    public string PayLabel { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public string CategoryIllustrationClass { get; set; } = "nr-cat-ill--other";
    public List<NeighborRequestHelperCardViewModel> Helpers { get; set; } = [];
    public string DetailUrl { get; set; } = "#";
}

public class NeighborRequestHelperCardViewModel
{
    public int ProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string AvatarIconClass { get; set; } = "fa-user";
    public string RatingLabel { get; set; } = "4.9";
    public int ReviewCount { get; set; }
    public string DistanceLabel { get; set; } = string.Empty;
    public string PriceLabel { get; set; } = string.Empty;
    public string MinHoursLabel { get; set; } = string.Empty;
    public List<string> SkillTags { get; set; } = [];
    public bool IsVerified { get; set; }
    public bool IsOnline { get; set; } = true;
    public string MessageUrl { get; set; } = "#";
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
    public string? DescriptionBody { get; set; }
    public bool IsDescriptionLong { get; set; }
    public string? HeroImageUrl { get; set; }
    public List<string> PhotoUrls { get; set; } = [];
    public List<NeighborRequestDetailStatViewModel> DetailStats { get; set; } = [];
    public List<NeighborRequestOfferItemViewModel> Offers { get; set; } = [];
    public List<NeighborRequestOfferItemViewModel> NeighborOffers { get; set; } = [];
    public List<NeighborRequestOfferItemViewModel> ProviderOffers { get; set; } = [];
    public int TotalNeighborOfferCount { get; set; }
    public string OfferCountLabel { get; set; } = string.Empty;
    public bool HasNeighborOffers { get; set; }
    public bool ShowProvidersPanel { get; set; }
    public bool CanManage { get; set; }
    public string BackUrl { get; set; } = "/";
    public string EditUrl { get; set; } = "#";
    public string CancelUrl { get; set; } = "#";
    public string ProvidersUrl { get; set; } = "#";
    public string SeeAllOffersUrl { get; set; } = "#";
    public string ViewProvidersUrl { get; set; } = "#";
}

public class NeighborRequestCancelViewModel
{
    public int Id { get; set; }
    public int PropiedadId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string IconClass { get; set; } = "fa-house";
    public string StatusLabel { get; set; } = "ACTIVE";
    public string StatusCss { get; set; } = "active";
    public string LocationAddress { get; set; } = string.Empty;
    public List<NeighborRequestDetailStatViewModel> DetailStats { get; set; } = [];
    public string BackUrl { get; set; } = "/";
    public string KeepUrl { get; set; } = "/";
    public string CreateUrl { get; set; } = "/";

    [MaxLength(40)]
    public string? CancelReasonCode { get; set; }

    [MaxLength(500)]
    public string? CancelNote { get; set; }

    public IReadOnlyList<(string Value, string Label)> ReasonOptions { get; set; } = [];
}

public class NeighborRequestDetailStatViewModel
{
    public string IconClass { get; set; } = "fa-circle";
    public string Label { get; set; } = string.Empty;
}

public class NeighborRequestOfferItemViewModel
{
    public int Id { get; set; }
    public int? ProviderId { get; set; }
    public string OffererName { get; set; } = string.Empty;
    public string? OffererPhotoUrl { get; set; }
    public string AvatarIconClass { get; set; } = "fa-user";
    public string? Message { get; set; }
    public string? PriceLabel { get; set; }
    public string? ScheduleLabel { get; set; }
    public string? RatingLabel { get; set; }
    public string MetaLabel { get; set; } = string.Empty;
    public string RoleLabel { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsProviderOffer { get; set; }
    public string DetailUrl { get; set; } = "#";
    public string ViewUrl { get; set; } = "#";
    public string MessageUrl { get; set; } = "#";
}

public class NeighborRequestOffersListViewModel
{
    public int RequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string BackUrl { get; set; } = "/";
    public List<NeighborRequestOfferItemViewModel> Offers { get; set; } = [];
}
