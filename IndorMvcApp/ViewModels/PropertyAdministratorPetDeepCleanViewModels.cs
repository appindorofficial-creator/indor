namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorPetDeepCleanFeaturedViewModel
{
    public string Title { get; set; } = "Pet Deep Clean";
    public string Subtitle { get; set; } = "Deep clean after pet stays — hair, dander, and odors removed.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorPetDeepCleanFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string ServiceType { get; set; } = "AfterPetStayCleanup";
    public string PetCount { get; set; } = "2";
    public string PetType { get; set; } = "Dog";
    public string FocusAreas { get; set; } = "PetHair,OdorRemoval,AccidentsStains,BedsUpholstery,Floors";
    public string ScheduleWhen { get; set; } = "Tomorrow";
    public string ScheduleTimeWindow { get; set; } = "10:00 AM – 2:00 PM";
    public string EntryAccess { get; set; } = "SmartLock";
    public string Details { get; set; } = "";
}

public class PropertyAdministratorPetDeepCleanSubmitInput
{
    public int PropertyId { get; set; }
    public string ServiceType { get; set; } = "AfterPetStayCleanup";
    public string PetCount { get; set; } = "2";
    public string PetType { get; set; } = "Dog";
    public List<string> FocusAreasList { get; set; } = ["PetHair", "OdorRemoval", "AccidentsStains", "BedsUpholstery", "Floors"];
    public string ScheduleWhen { get; set; } = "Tomorrow";
    public string ScheduleTimeWindow { get; set; } = "10:00 AM – 2:00 PM";
    public string EntryAccess { get; set; } = "SmartLock";
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "CoHost"];
    public string? Details { get; set; }
}

public class PropertyAdministratorPetDeepCleanTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorPetDeepCleanBookingItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorPetDeepCleanConfirmedViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string TechnicianName { get; set; } = "";
    public decimal TechnicianRating { get; set; }
    public int TechnicianReviewCount { get; set; }
    public string TechnicianTitle { get; set; } = "";
    public string TechnicianExperience { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorPetDeepCleanBookingItemViewModel> BookingDetails { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorPetDeepCleanTimelineItemViewModel> Timeline { get; set; } = [];
    public string InfoBanner { get; set; } = "Ideal for rental homes and Airbnb properties after guest stays with pets.";
}
