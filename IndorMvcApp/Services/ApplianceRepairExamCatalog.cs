using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class ApplianceRepairExamCatalog
{
    public const string TradeCode = "appliance";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "What should be done first before servicing any appliance?", ["Disconnect power", "Replace parts", "Remove panels", "Test the motor"], 0),
        new(2, "What is the purpose of a multimeter in appliance repair?", ["Measure voltage and continuity", "Lift heavy appliances", "Clean internal parts", "Seal refrigerant lines"], 0),
        new(3, "If a dryer does not heat, which component is commonly checked first?", ["Heating element", "Water valve", "Door handle", "Ice maker arm"], 0),
        new(4, "Why is it important to verify the model number before ordering parts?", ["To match the correct replacement part", "To increase water pressure", "To reduce cleaning time", "To bypass diagnostics"], 0),
        new(5, "A refrigerator is running but not cooling well. What should be checked first?", ["Condenser coils", "Cabinet color", "Door logo", "Control knob shape"], 0),
        new(6, "What does continuity testing help confirm?", ["Whether a circuit path is complete", "Whether the appliance is level", "Whether the paint is dry", "Whether the customer approved the repair"], 0),
        new(7, "If a washer does not drain, which part is commonly inspected?", ["Drain pump", "Bake element", "Water filter pitcher", "Light bulb socket"], 0),
        new(8, "Why should the correct replacement part number be used?", ["To ensure proper fit and function", "To reduce floor noise", "To improve packaging", "To speed up cleaning only"], 0),
        new(9, "What is a common sign of a faulty refrigerator door gasket?", ["Warm air leaking around the door", "The shelves are full", "The cord is too long", "The ice tray is blue"], 0),
        new(10, "When diagnosing an electric oven that will not heat, what is commonly checked?", ["Bake element", "Door paint", "Handle screws only", "Rack color"], 0),
        new(11, "What is the function of a thermal fuse in many appliances?", ["Protect against overheating", "Increase water pressure", "Change motor speed automatically", "Improve exterior appearance"], 0),
        new(12, "If a dishwasher is not filling with water, which item is often checked first?", ["Water inlet valve", "Dryer vent", "Cabinet feet", "Door sticker"], 0),
        new(13, "Why is it important to inspect appliance wiring for damage?", ["To prevent shorts and unsafe operation", "To improve shelf appearance", "To increase detergent flow", "To reduce packaging waste"], 0),
        new(14, "A microwave is completely dead. What is one basic item to verify first?", ["Power supply to the unit", "Color of the control panel", "Shape of the glass tray", "Position of the kitchen table"], 0),
        new(15, "What does a technician use a wiring diagram for?", ["To trace circuits and components", "To estimate appliance weight", "To clean the appliance exterior", "To advertise services"], 0),
        new(16, "If a dryer tumbles but has weak airflow, what is a common cause?", ["Blocked venting", "Loose cabinet paint", "Wrong timer font", "Cold water valve leak"], 0),
        new(17, "Why should moving parts be inspected for wear?", ["To prevent failure and noise", "To speed up packaging", "To brighten the appliance finish", "To increase room lighting"], 0),
        new(18, "What is a common first step when troubleshooting an appliance complaint?", ["Verify the reported symptom", "Order parts without testing", "Replace the control board immediately", "Ignore the customer description"], 0),
        new(19, "What customer service practice is most important after completing a repair?", ["Explain the repair and test results", "Hide replaced parts", "Leave without speaking", "Skip cleanup"], 0),
        new(20, "What should be done before closing the job?", ["Confirm the appliance operates correctly", "Remove the serial tag", "Turn off the house water main", "Reset unrelated appliances"], 0),
    ];

    public static IReadOnlyList<ExamQuestion> PageQuestions(int page)
    {
        page = Math.Max(1, page);
        return Questions
            .Skip((page - 1) * QuestionsPerPage)
            .Take(QuestionsPerPage)
            .ToList();
    }
}
