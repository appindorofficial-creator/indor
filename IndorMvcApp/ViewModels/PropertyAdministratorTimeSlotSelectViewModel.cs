namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorTimeSlotSelectViewModel
{
    public string FieldName { get; set; } = "ScheduleTimeWindow";
    public string? SelectedValue { get; set; }
    public string DefaultValue { get; set; } = Helpers.PropertyAdministratorTimeSlots.Default;
}
