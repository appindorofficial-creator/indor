namespace IndorMvcApp.ViewModels;

public class ProviderProAddCustomerDraft
{
    public string CustomerType { get; set; } = "Homeowner";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string PreferredContactMethod { get; set; } = "SMS";
    public string CompanyName { get; set; } = "";
    public string StreetAddress { get; set; } = "";
    public string AptUnit { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string PropertyType { get; set; } = "Single Family";
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public bool IsBillingAddressSame { get; set; } = true;
    public string AccessNotes { get; set; } = "";
    public string EstimateDeliveryPref { get; set; } = "Email";
    public string InvoiceDeliveryPref { get; set; } = "Email";
    public string PreferredLanguage { get; set; } = "English";
    public string CustomerSource { get; set; } = "Manual Entry";
    public List<string> Tags { get; set; } = [];
    public string InternalNotes { get; set; } = "";
    public bool SendIndorInvite { get; set; } = true;
    public bool AllowServiceUpdates { get; set; } = true;
}

public class ProviderProAddCustomerInfoViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public string CustomerType { get; set; } = "Homeowner";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string PreferredContactMethod { get; set; } = "SMS";
    public string CustomerCompanyName { get; set; } = "";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProAddCustomerPropertyViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 2;
    public int TotalSteps { get; set; } = 4;
    public string StreetAddress { get; set; } = "";
    public string AptUnit { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string PropertyType { get; set; } = "Single Family";
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public bool IsBillingAddressSame { get; set; } = true;
    public string AccessNotes { get; set; } = "";
    public List<string> StateOptions { get; set; } = [];
    public List<int> BedroomOptions { get; set; } = [];
    public List<decimal> BathroomOptions { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProAddCustomerPreferencesViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 3;
    public int TotalSteps { get; set; } = 4;
    public string EstimateDeliveryPref { get; set; } = "Email";
    public string InvoiceDeliveryPref { get; set; } = "Email";
    public string PreferredLanguage { get; set; } = "English";
    public string CustomerSource { get; set; } = "Manual Entry";
    public List<string> Tags { get; set; } = [];
    public string InternalNotes { get; set; } = "";
    public bool SendIndorInvite { get; set; } = true;
    public bool AllowServiceUpdates { get; set; } = true;
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProAddCustomerReviewViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 4;
    public int TotalSteps { get; set; } = 4;
    public string CustomerType { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string PreferredContactMethod { get; set; } = "";
    public string FullAddress { get; set; } = "";
    public string PropertyType { get; set; } = "";
    public string BedsBathsLabel { get; set; } = "";
    public string BillingLabel { get; set; } = "";
    public string EstimateDeliveryPref { get; set; } = "";
    public string InvoiceDeliveryPref { get; set; } = "";
    public string PreferredLanguage { get; set; } = "";
    public string CustomerSource { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public string InternalNotes { get; set; } = "";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProAddCustomerSuccessViewModel : ProviderProPageBaseViewModel
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string CustomerType { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string StatusLabel { get; set; } = "Active";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProAddCustomerInfoInput
{
    public string CustomerType { get; set; } = "Homeowner";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string PreferredContactMethod { get; set; } = "SMS";
    public string CompanyName { get; set; } = "";
}

public class ProviderProAddCustomerPropertyInput
{
    public string StreetAddress { get; set; } = "";
    public string AptUnit { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string PropertyType { get; set; } = "Single Family";
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public bool IsBillingAddressSame { get; set; } = true;
    public string AccessNotes { get; set; } = "";
}

public class ProviderProAddCustomerPreferencesInput
{
    public string EstimateDeliveryPref { get; set; } = "Email";
    public string InvoiceDeliveryPref { get; set; } = "Email";
    public string PreferredLanguage { get; set; } = "English";
    public string CustomerSource { get; set; } = "Manual Entry";
    public List<string> Tags { get; set; } = [];
    public string InternalNotes { get; set; } = "";
    public bool SendIndorInvite { get; set; } = true;
    public bool AllowServiceUpdates { get; set; } = true;
}
