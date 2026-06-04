using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class CleaningExamCatalog
{
    public const string TradeCode = "cleaning";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "Before mixing a cleaning chemical, what should you do first?", ["Read the label and dilution instructions", "Mix all products together", "Use hot water automatically", "Spray directly on surfaces"], 0),
        new(2, "What is the safest order when cleaning a bathroom?", ["Remove trash first", "Use the same cloth on every surface", "Start with the toilet first every time", "Skip gloves to work faster"], 0),
        new(3, "Which product should generally be avoided on natural stone?", ["Acidic cleaner", "Microfiber cloth", "pH-neutral cleaner", "Warm water"], 0),
        new(4, "What should be done before vacuuming a room?", ["Pick up large debris and inspect the floor", "Close all vents", "Spray bleach on carpet", "Mop first"], 0),
        new(5, "What helps prevent cross-contamination while cleaning?", ["Use color-coded cloths by area", "Use one sponge for all rooms", "Skip sanitizing tools", "Store clean and dirty tools together"], 0),
        new(6, "When should a microfiber mop pad be changed?", ["When visibly soiled or after contaminated areas", "Only once a week", "Only when it tears", "Never during a job"], 0),
        new(7, "How should high-touch surfaces be disinfected?", ["Clean first and allow proper contact time", "Wipe quickly and dry immediately", "Use water only", "Spray from a distance and walk away"], 0),
        new(8, "How should vacuum cords be handled during service?", ["Check for damage and keep away from walk paths", "Pull the cord by force", "Run over the cord with the vacuum", "Leave it stretched across doorways"], 0),
        new(9, "What is the purpose of disinfectant contact time?", ["To let the surface stay wet long enough to work", "To make the room smell better only", "To skip rinsing", "To dry surfaces immediately"], 0),
        new(10, "Which cleaning direction is usually best for dusting a room?", ["Top to bottom", "Bottom to top only", "Random order", "Outside walls first always"], 0),
        new(11, "What should you do if you find broken glass?", ["Use gloves and a proper pickup tool", "Sweep with bare hands", "Ignore small pieces", "Vacuum without inspection"], 0),
        new(12, "What is a good first step before starting work in a customer's home?", ["Confirm the scope and areas to clean", "Start spraying immediately", "Move furniture without asking", "Skip the walkthrough"], 0),
        new(13, "Which flooring surface can be damaged by too much water?", ["Hardwood flooring", "Concrete only", "Ceramic tile only", "Metal flooring"], 0),
        new(14, "What helps reduce streaks when cleaning mirrors?", ["A clean microfiber cloth and proper glass cleaner", "A strong abrasive pad", "A dirty towel", "Hot bleach solution"], 0),
        new(15, "What should be documented after a cleaning job is completed?", ["Completed tasks, issues found, and notes", "Only the payment amount", "Only the arrival time", "Nothing at all"], 0),
        new(16, "If a customer has pets in the home, what is the best practice?", ["Use pet-safe practices and secure doors or gates", "Spray strong chemicals near food bowls", "Leave exterior doors open", "Ignore pet instructions"], 0),
        new(17, "What is the best practice after raw food contamination on a kitchen surface?", ["Clean first, then disinfect with proper contact time", "Use a dry cloth only", "Add scented spray only", "Skip sanitizing if it looks clean"], 0),
        new(18, "How should cleaning chemicals be stored during service?", ["Labeled and kept secure away from children and pets", "In unlabeled bottles", "Open on the floor", "Mixed together in one container"], 0),
        new(19, "What should you do if a stain does not respond to standard treatment?", ["Stop and inform the customer before aggressive methods", "Scrub harder with any chemical", "Ignore possible damage", "Cut the surface material"], 0),
        new(20, "What is the final step before leaving the property?", ["Do a final walkthrough and confirm satisfaction", "Leave without checking", "Turn off the water heater", "Take all trash bags but skip inspection"], 0),
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
