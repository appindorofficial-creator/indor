using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class ConstructionExamCatalog
{
    public const string TradeCode = "construction";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "What should be reviewed before starting a remodel project on site?", ["Project plans and scope", "Client's furniture", "Paint colors only", "Only the final invoice"], 0),
        new(2, "What is the main purpose of a site safety meeting?", ["Review hazards and safety expectations", "Set decoration ideas", "Choose lunch breaks", "Discuss marketing"], 0),
        new(3, "Why is it important to verify measurements before ordering materials?", ["To reduce waste and avoid delays", "To skip estimating", "To make the project longer", "To increase change orders"], 0),
        new(4, "Who is responsible for checking that subcontractors understand the work scope?", ["The site supervisor or project manager", "The material supplier", "The homeowner's neighbor", "The painter only"], 0),
        new(5, "What is the best reason to document job progress with photos?", ["To track work and keep records", "To replace permits", "To avoid communication", "To skip inspections"], 0),
        new(6, "What should be confirmed before demolition begins?", ["Utilities are identified or shut off as needed", "Only the flooring color", "The landscaping plan", "The social media post"], 0),
        new(7, "Why is a written change order important?", ["It documents scope, price, and approval changes", "It eliminates the contract", "It replaces the permit", "It delays all work"], 0),
        new(8, "When coordinating multiple trades, what is most important?", ["Scheduling work in the correct sequence", "Letting all trades arrive at once", "Ignoring material lead times", "Skipping supervision"], 0),
        new(9, "What does a permit generally help confirm?", ["That the work is approved under local rules", "That the owner chose the cheapest option", "That tools are new", "That no inspection is needed"], 0),
        new(10, "What is the main purpose of a project schedule?", ["To organize tasks, timing, and coordination", "To decorate the jobsite", "To replace the estimate", "To avoid ordering materials"], 0),
        new(11, "Before pouring concrete or closing walls, what should usually happen?", ["Required inspections should be completed", "The final payment should be collected", "The warranty should start", "The appliances should be installed"], 0),
        new(12, "What is a good practice when receiving materials on site?", ["Check delivery quantities and condition", "Leave materials in the street", "Ignore damaged items", "Use them without review"], 0),
        new(13, "What is a common role of the general contractor on a residential remodel?", ["Coordinate trades and project execution", "Only order paint supplies", "Skip safety planning", "Avoid client communication"], 0),
        new(14, "Why should subcontractors provide proof of insurance?", ["To reduce liability and verify coverage", "To replace permits", "To avoid scheduling", "To skip inspections"], 0),
        new(15, "What should be done if a structural concern is discovered during construction?", ["Stop work and evaluate with qualified professionals", "Continue without telling anyone", "Paint over the issue", "Skip documentation"], 0),
        new(16, "Before drywall installation, what should typically be complete?", ["Framing, rough-ins, and required inspections", "Final paint color selection only", "Landscaping", "Furniture delivery"], 0),
        new(17, "What should be checked before final walkthrough with the client?", ["Punch list items and completion status", "Only the company logo", "The next job address", "The crew lunch order"], 0),
        new(18, "Why is housekeeping important on a construction site?", ["It helps reduce trip hazards and keep the site organized", "It increases project cost", "It replaces PPE", "It eliminates scheduling"], 0),
        new(19, "What is a punch list?", ["A list of remaining corrections or incomplete items", "A permit application", "A demolition checklist only", "A labor timesheet"], 0),
        new(20, "What is the best reason to keep project records organized?", ["To support communication, billing, and accountability", "To avoid supervision", "To skip documentation", "To reduce safety meetings"], 0),
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
