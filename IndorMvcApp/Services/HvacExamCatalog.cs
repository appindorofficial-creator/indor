using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class HvacExamCatalog
{
    public const string TradeCode = "hvac";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "Before servicing an HVAC unit, what should be done first?", ["Disconnect power and verify it is off", "Open the refrigerant valves", "Replace the thermostat", "Increase fan speed"], 0),
        new(2, "What is the main purpose of an air filter?", ["Protect equipment and improve airflow quality", "Raise refrigerant pressure", "Increase compressor voltage", "Lubricate the blower motor"], 0),
        new(3, "Which tool is commonly used to check voltage?", ["Multimeter", "Pipe cutter", "Manifold hose only", "Thermometer"], 0),
        new(4, "If the thermostat screen is blank, what should be checked first?", ["Power supply or batteries", "Compressor oil", "Condenser coil color", "Duct insulation"], 0),
        new(5, "A dirty evaporator coil most commonly causes?", ["Reduced cooling performance", "Higher thermostat accuracy", "Increased refrigerant recovery", "Stronger breaker protection"], 0),
        new(6, "Low airflow across the evaporator can lead to?", ["Coil icing", "Higher gas pressure only", "Larger duct size", "Automatic refrigerant refill"], 0),
        new(7, "A common cause of short cycling is?", ["Dirty filter or thermostat issue", "Fresh paint on walls", "Oversized shoes on the installer", "Low ladder height"], 0),
        new(8, "When some rooms are much warmer than others, what should be checked?", ["Airflow, ducts, and dampers", "Roof color only", "Water heater settings", "Exterior brick pattern"], 0),
        new(9, "Before adding refrigerant, what should a technician do first?", ["Verify charge and check for leaks", "Open the condensate drain", "Replace the thermostat immediately", "Shut off the furnace gas valve"], 0),
        new(10, "Symptoms such as low pressure and poor cooling may suggest?", ["Possible low refrigerant charge", "Perfect system balance", "Too much duct insulation", "An upgraded air filter only"], 0),
        new(11, "Why are superheat and subcooling checked?", ["To verify system charge and performance", "To estimate property taxes", "To repaint the condenser", "To reset Wi-Fi"], 0),
        new(12, "Refrigerant must be recovered using?", ["Approved recovery equipment", "A garden hose", "An open bucket", "A vacuum cleaner"], 0),
        new(13, "A failed capacitor may cause?", ["Motor not starting properly", "More refrigerant in the lines", "Higher home resale value", "A larger air filter"], 0),
        new(14, "What component protects the compressor from high pressure?", ["High-pressure switch", "Drain pan only", "Thermostat cover", "Blower wheel"], 0),
        new(15, "If the condenser fan is not running, what should be checked first?", ["Power, contactor, and capacitor", "Grass height outside", "Kitchen sink pressure", "Window blinds"], 0),
        new(16, "What is the purpose of a contactor?", ["It switches high-voltage power using a control signal", "It cleans the evaporator coil", "It stores condensate water", "It measures duct insulation"], 0),
        new(17, "What should be documented after service is completed?", ["Work performed, readings, and recommendations", "Only the customer's ZIP code", "Paint color of the unit", "Nothing if the job was simple"], 0),
        new(18, "When should a condensate drain issue be addressed?", ["Immediately to avoid water damage", "Next year during repainting", "Only in winter", "After replacing the roof"], 0),
        new(19, "If a major safety concern is found, what is the best action?", ["Inform the customer clearly and make the system safe", "Ignore it and leave", "Raise the refrigerant charge", "Change the thermostat color"], 0),
        new(20, "What helps extend HVAC system life?", ["Regular maintenance and filter changes", "Closing every vent", "Running the system without a filter", "Skipping inspections"], 0),
    ];

    public static int TotalPages => (int)Math.Ceiling(Questions.Count / (double)QuestionsPerPage);

    public static IReadOnlyList<ExamQuestion> PageQuestions(int page)
    {
        page = Math.Max(1, page);
        return Questions
            .Skip((page - 1) * QuestionsPerPage)
            .Take(QuestionsPerPage)
            .ToList();
    }
}
