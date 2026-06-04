using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class LandscapingExamCatalog
{
    public const string TradeCode = "landscaping";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "Before using a string trimmer near people, what should be worn first?", ["Safety glasses and protective gear", "Flip-flops", "Loose jewelry", "No protection is needed"], 0),
        new(2, "What is the best first step before mowing a new property?", ["Walk the site and remove debris", "Start mowing immediately", "Water the lawn heavily", "Trim shrubs first"], 0),
        new(3, "Why should grass not be cut too short?", ["It stresses the turf and weakens roots", "It grows faster overnight", "It makes mulch unnecessary", "It prevents irrigation"], 0),
        new(4, "What should be checked before starting a mower?", ["Fuel, oil, and blade condition", "House paint color", "Customer Wi-Fi password", "Fence stain color"], 0),
        new(5, "What is the best time to water most lawns?", ["Early morning", "At noon", "Late night", "Only after rain"], 0),
        new(6, "What is a common sign that irrigation needs adjustment?", ["Dry spots or runoff", "Grass is green", "Soil is cool", "Wind is low"], 0),
        new(7, "Why is spacing important when planting shrubs?", ["Allows mature growth and airflow", "Makes watering impossible", "Forces roots upward", "Prevents mulch use"], 0),
        new(8, "What is a key benefit of mulch?", ["Helps retain moisture and reduce weeds", "Eliminates all watering", "Replaces soil", "Makes pruning unnecessary"], 0),
        new(9, "What is the main purpose of edging?", ["Creates clean separation lines", "Makes grass grow taller", "Removes irrigation", "Fertilizes soil"], 0),
        new(10, "What should be done before applying fertilizer?", ["Identify turf needs and follow label directions", "Apply the maximum amount", "Skip the weather check", "Water flowers with fuel"], 0),
        new(11, "Which tool is best for pruning small shrubs?", ["Hand pruners", "Sledgehammer", "Chainsaw for all cuts", "Leaf blower"], 0),
        new(12, "Why is grading important around a home?", ["Directs water away from the structure", "Traps water near the foundation", "Increases roof load", "Changes the paint color"], 0),
        new(13, "Before repairing a sprinkler head, what should be done first?", ["Shut off the irrigation zone or water", "Add more pressure", "Cut the grass first", "Ignore the leak"], 0),
        new(14, "What is a common sign of overwatering?", ["Mushy soil and yellowing or fungus", "Dry, cracking soil", "Strong roots only", "Cleaner sidewalks"], 0),
        new(15, "Why are base layers important in hardscape work?", ["They provide stability and drainage", "They make plants taller", "They replace edging", "They eliminate compaction"], 0),
        new(16, "What should a provider do if a task is outside their skill or scope?", ["Inform the customer and decline or refer appropriately", "Do it anyway", "Hide the issue", "Bill extra without explanation"], 0),
        new(17, "What is the best way to avoid damage when using a blower near flower beds?", ["Use low speed and keep the blower away from beds", "Use maximum power toward flowers", "Aim directly at mulch only", "Skip clearing debris"], 0),
        new(18, "What is the safest way to lift heavy bags of soil or mulch?", ["Bend with legs and lift carefully or use a team", "Lift with your back only", "Throw bags from the truck", "Drag bags without lifting"], 0),
        new(19, "What is a best customer practice after the job is completed?", ["Walk the site with the customer and confirm satisfaction", "Leave without checking the work", "Bill before cleanup", "Skip final communication"], 0),
        new(20, "If herbicide is used, what is most important?", ["Follow label directions and use proper PPE", "Apply extra for faster results", "Mix with any other chemical", "Skip protecting nearby plants"], 0),
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
