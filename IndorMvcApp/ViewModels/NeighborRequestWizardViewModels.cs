using System.ComponentModel.DataAnnotations;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
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
    public string MinNeededByDateIso => DateTime.Today.ToString("yyyy-MM-dd");
}

public class NeighborRequestCategoryStepViewModel : NeighborRequestWizardShellViewModel
{
    public int? SelectedCategoryId { get; set; }
    public bool ResumeDraft { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Please enter a job title."), MaxLength(60)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please enter a location."), MaxLength(500)]
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

public class NeighborRequestDescribeStepViewModel : NeighborRequestWizardShellViewModel, IValidatableObject
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Unchecked chip groups often bind as null — treat as empty, never NRE.
        SelectedTools ??= [];
        if (SelectedTools.Count == 0)
        {
            yield return new ValidationResult(
                "Select what the helper should bring.",
                [nameof(SelectedTools)]);
        }

        if (string.IsNullOrWhiteSpace(PetsOnProperty))
        {
            yield return new ValidationResult(
                "Select whether there are pets on the property.",
                [nameof(PetsOnProperty)]);
        }

        if (string.IsNullOrWhiteSpace(HasStairs))
        {
            yield return new ValidationResult(
                "Select whether there are stairs.",
                [nameof(HasStairs)]);
        }

        if (string.IsNullOrWhiteSpace(ParkingAvailable))
        {
            yield return new ValidationResult(
                "Select whether parking is available.",
                [nameof(ParkingAvailable)]);
        }
    }
}

public class NeighborRequestPreferencesStepViewModel : NeighborRequestWizardShellViewModel, IValidatableObject
{
    public string WhenCode { get; set; } = string.Empty;
    public string PreferredTimeCode { get; set; } = string.Empty;
    public int HelperCount { get; set; }
    public string DurationCode { get; set; } = string.Empty;
    public string PayTypeCode { get; set; } = string.Empty;
    public string TimelineCode { get; set; } = string.Empty;
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsEditMode)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(WhenCode))
        {
            yield return new ValidationResult("Choose when you need help.", [nameof(WhenCode)]);
        }

        if (string.IsNullOrWhiteSpace(PreferredTimeCode))
        {
            yield return new ValidationResult("Choose a preferred time.", [nameof(PreferredTimeCode)]);
        }

        if (HelperCount <= 0)
        {
            yield return new ValidationResult("Choose how many helpers you need.", [nameof(HelperCount)]);
        }

        if (string.IsNullOrWhiteSpace(DurationCode))
        {
            yield return new ValidationResult("Choose an estimated duration.", [nameof(DurationCode)]);
        }

        if (string.IsNullOrWhiteSpace(PayTypeCode))
        {
            yield return new ValidationResult("Choose how you want to pay.", [nameof(PayTypeCode)]);
        }

        if (BudgetAmount is null or <= 0)
        {
            yield return new ValidationResult("Enter your budget.", [nameof(BudgetAmount)]);
        }

        if (WhenCode != NeighborRequestTimelineCodes.PickDate)
        {
            yield break;
        }

        if (NeededByDate == null)
        {
            yield return new ValidationResult("Pick a date to continue.", [nameof(NeededByDate)]);
            yield break;
        }

        if (!NeighborRequestWizardService.IsNeededByDateAllowed(NeededByDate))
        {
            yield return new ValidationResult(
                NeighborRequestWizardService.NeededByDatePastErrorMessage,
                [nameof(NeededByDate)]);
        }
    }
}

public class NeighborRequestEditDetailsStepViewModel : NeighborRequestWizardShellViewModel, IValidatableObject
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NeededByDate is not null
            && !NeighborRequestWizardService.IsNeededByDateAllowed(NeededByDate))
        {
            yield return new ValidationResult(
                NeighborRequestWizardService.NeededByDatePastErrorMessage,
                [nameof(NeededByDate)]);
        }
    }
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
    public string InviteUrl { get; set; } = "#";
    public double? MapCenterLatitude { get; set; }
    public double? MapCenterLongitude { get; set; }
    public bool HasMapLocation => MapCenterLatitude is not null && MapCenterLongitude is not null;
}

public class NeighborRequestBrowseHelpersViewModel
{
    public int PropiedadId { get; set; }
    public string HomeUrl { get; set; } = "/";
    public string LocationAddress { get; set; } = string.Empty;
    public string RadiusLabel { get; set; } = "3 miles around your home";
    public string PostQuickJobUrl { get; set; } = "#";
    public List<NeighborRequestHelperCardViewModel> Helpers { get; set; } = [];
}

public class NeighborRequestHelperCardViewModel
{
    public string SelectionKey { get; set; } = string.Empty;
    public int ProviderId { get; set; }
    public bool IsSelected { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string AvatarIconClass { get; set; } = "fa-user";
    public string? RatingLabel { get; set; }
    public int ReviewCount { get; set; }
    public string DistanceLabel { get; set; } = string.Empty;
    public string PriceLabel { get; set; } = string.Empty;
    public string MinHoursLabel { get; set; } = string.Empty;
    public List<string> SkillTags { get; set; } = [];
    public bool IsVerified { get; set; }
    public bool IsOnline { get; set; } = true;
    public string MessageUrl { get; set; } = "#";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class NeighborRequestListViewModel
{
    public int PropiedadId { get; set; }
    public string HomeUrl { get; set; } = "/";
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
