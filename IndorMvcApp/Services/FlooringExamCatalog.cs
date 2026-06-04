using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class FlooringExamCatalog
{
    public const string TradeCode = "flooring";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "What should be checked before installing any flooring?", ["Subfloor is clean, dry, and level", "Appliance warranty", "Wall color", "Window blinds"], 0),
        new(2, "Why is leaving an expansion gap important for floating floors?", ["Allows material to expand and contract", "Hides uneven walls", "Makes the floor quieter only", "Reduces labor cost"], 0),
        new(3, "Which tool is commonly used to verify a subfloor is level?", ["Level or straightedge", "Pipe wrench", "Paint roller", "Wet saw"], 0),
        new(4, "If moisture is too high in the subfloor, what should be done first?", ["Stop and resolve the moisture issue", "Install more adhesive", "Nail down faster", "Add extra trim"], 0),
        new(5, "What is the best way to start a room layout?", ["Measure the room and plan the first row", "Open all boxes randomly", "Install from the middle without measuring", "Cut planks before marking"], 0),
        new(6, "Why should flooring cartons acclimate to the room?", ["So material adjusts to site conditions", "To make boxes lighter", "To change the color", "To reduce trim work"], 0),
        new(7, "What is a common reason to stagger end joints?", ["Improve appearance and stability", "Reduce cleaning time", "Eliminate expansion gaps", "Avoid underlayment"], 0),
        new(8, "When cutting flooring indoors, what is most important?", ["Use PPE and control dust", "Work barefoot", "Cut without measuring", "Leave tools plugged in unattended"], 0),
        new(9, "What must be verified before installing tile in a wet area?", ["Proper waterproofing or approved substrate", "Ceiling fan size", "Cabinet paint color", "Appliance brand"], 0),
        new(10, "Which underlayment function is correct?", ["Helps with support, moisture, or sound depending on product", "Replaces subfloor repairs", "Prevents all movement forever", "Eliminates need for layout"], 0),
        new(11, "For glue-down flooring, why is trowel size important?", ["It controls adhesive spread rate", "It changes board color", "It removes need for rolling", "It replaces expansion gaps"], 0),
        new(12, "Before installing hardwood planks, the installer should:", ["Check moisture content of wood and subfloor", "Turn on all faucets", "Remove all baseboards forever", "Skip site inspection"], 0),
        new(13, "What is the main purpose of a transition strip?", ["Create a clean change between floor surfaces or heights", "Hold cabinets in place", "Waterproof every room", "Replace baseboard"], 0),
        new(14, "If a plank is damaged during install, what should the installer do?", ["Replace it before finishing the floor", "Hide it under a rug", "Glue it and continue if cracked", "Ignore it"], 0),
        new(15, "In customer areas, the worksite should be kept:", ["Clean, protected, and safe to walk through", "Loud and crowded", "Full of loose nails", "Open to children"], 0),
        new(16, "What should be done after finishing an installation?", ["Inspect the floor and review care instructions with the customer", "Leave scrap everywhere", "Remove receipts only", "Tell customer not to ask questions"], 0),
        new(17, "Which direction is often preferred when laying planks in a narrow room?", ["A planned layout that fits the room and light direction", "Always the shortest wall no matter what", "Random direction with no plan", "Toward the door only"], 0),
        new(18, "If the final row will be too narrow, the installer should:", ["Adjust the starting row so first and last rows balance", "Force narrow pieces into place", "Skip the final row", "Add more adhesive"], 0),
        new(19, "Why is it important to check manufacturer installation instructions?", ["Products have specific requirements and warranty rules", "To avoid measuring", "To skip acclimation", "All floors install the same way"], 0),
        new(20, "What is best practice when handing over the finished flooring job?", ["Confirm completion, cleanup, care guidance, and any next steps", "Leave without speaking", "Ask customer to clean debris", "Promise repairs without inspection"], 0),
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
