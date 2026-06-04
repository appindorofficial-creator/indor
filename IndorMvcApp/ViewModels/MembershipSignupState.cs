namespace IndorMvcApp.ViewModels;

public class MembershipSignupState
{
    public int PlanId { get; set; }
    public string? FilterSize { get; set; }
    public string? FilterType { get; set; }
    public string? HvacNickname { get; set; }
    public string? PropertyAddress { get; set; }
    public bool PetsAtHome { get; set; }
    public string? ShippingAddress { get; set; }
    public string? DeliveryCycle { get; set; } = "Every 3 months";
    public DateTime? FirstDeliveryDate { get; set; }
    public int FilterQuantity { get; set; } = 1;
    public bool ShipmentReminder { get; set; } = true;
    public bool ReplaceFilterReminder { get; set; } = true;
    public bool LowInventoryReminder { get; set; } = true;
    public bool ReminderAirFilter { get; set; } = true;
    public bool ReminderHvac { get; set; } = true;
    public bool ReminderSmokeDetector { get; set; } = true;
    public bool ReminderSeasonal { get; set; } = true;
    public string? FilterPhotoUrl { get; set; }
}
