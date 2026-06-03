using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class KitchenExamCatalog
{
    public const string TradeCode = "kitchen";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "Before removing old cabinets, what should be done first?", ["Cut the new countertops", "Shut off utilities in the work area", "Install backsplash", "Paint the walls"], 1),
        new(2, "Why is it important to confirm final kitchen measurements before ordering cabinets?", ["To reduce demolition dust", "To make sure cabinets fit the layout correctly", "To avoid checking wall condition", "To skip appliance planning"], 1),
        new(3, "Which item should be protected before demolition begins?", ["Exposed wiring only", "Floors and nearby finished areas", "Cabinet hardware only", "Empty cardboard boxes"], 1),
        new(4, "When planning a kitchen layout, what should be verified early?", ["The music playlist", "Appliance sizes and placement", "The paint brand only", "The final cleaning date"], 1),
        new(5, "What is the main purpose of a kitchen work triangle?", ["To choose paint colors", "To improve movement between sink, stove, and refrigerator", "To reduce countertop thickness", "To select cabinet hardware"], 1),
        new(6, "Before installing new base cabinets, what must be checked?", ["That the floor is level", "That the backsplash is finished first", "That the windows are open", "That the refrigerator is running"], 0),
        new(7, "Why should backsplash and countertop selections be coordinated early?", ["To avoid checking measurements", "To make sure finishes and fit work together", "To increase demolition time", "To skip cabinet layout review"], 1),
        new(8, "When replacing kitchen lighting, what is most important first?", ["Matching the music playlist", "Confirming safe electrical planning and fixture placement", "Buying paint rollers", "Ordering bar stools"], 1),
        new(9, "Why is it important to verify rough plumbing and electrical locations before cabinet installation?", ["So cabinets and appliances fit without conflicts", "So flooring can be skipped", "So drywall never needs repair", "So paint can be chosen faster"], 0),
        new(10, "What should be done before templating for countertops?", ["Cabinets should be installed and secured in place", "The final cleaning should be finished", "Appliances should be removed from the store box only", "The backsplash should already be grouted"], 0),
        new(11, "What is a key reason to protect adjacent rooms during demolition?", ["To keep dust and debris from spreading", "To avoid ordering cabinets", "To change the sink style", "To increase labor time"], 0),
        new(12, "When installing cabinets, why are shims commonly used?", ["To decorate open shelves", "To level and align cabinets properly", "To waterproof countertops", "To hide plumbing leaks"], 1),
        new(13, "What is the best reason to review appliance specifications before finalizing the design?", ["To ensure openings, utility needs, and clearances are correct", "To choose wall paint first", "To reduce the number of cabinet doors", "To avoid checking ventilation"], 0),
        new(14, "When setting wall cabinets, what should be confirmed first?", ["That upper cabinet height and support attachment points are correct", "That the floor tile is already sealed", "That the garbage disposal is plugged in", "That the stools match the island color"], 0),
        new(15, "Why should ventilation be considered in a kitchen remodel?", ["To improve removal of heat, smoke, and cooking odors", "To make cabinet doors heavier", "To lower backsplash height", "To avoid measuring the range"], 0),
        new(16, "What is a good practice before starting finish work and punch-out?", ["Ignore minor defects", "Inspect surfaces, alignments, and hardware function", "Remove all appliance manuals", "Skip testing lights and outlets"], 1),
        new(17, "Why is waterproofing around sink and splash areas important?", ["To protect surrounding materials from moisture damage", "To reduce cabinet storage", "To select paint brushes", "To avoid measuring the countertop"], 0),
        new(18, "Before turning over a completed kitchen remodel, what should be tested?", ["Appliances, lights, outlets, plumbing fixtures, and cabinet hardware", "Only the paint color in daylight", "Only the broom closet door", "Only the countertop edge color"], 0),
        new(19, "What is the purpose of a final walkthrough with the customer?", ["To review completed work, confirm function, and note any punch-list items", "To skip documentation", "To remove all labels before inspection", "To choose a new sink color after installation"], 0),
        new(20, "What is the best reason for maintaining a clean and organized jobsite during the remodel?", ["To improve safety, efficiency, and customer confidence", "To make demolition louder", "To avoid using protective coverings", "To reduce layout accuracy"], 0),
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
