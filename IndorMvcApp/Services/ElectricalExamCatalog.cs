using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class ElectricalExamCatalog
{
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "Which breaker size should match the circuit load and wire rating?", ["The smallest breaker available", "The breaker one size larger than the wire", "The breaker that matches the wire ampacity", "The largest breaker available"], 2),
        new(2, "Before replacing a receptacle, what should be done first?", ["Test for voltage", "Remove the wall plate", "Turn off the circuit", "Disconnect the neutral wire"], 2),
        new(3, "What is the minimum working clearance in front of an electrical panel rated 600V or less?", ["24 inches (600 mm)", "36 inches (900 mm)", "30 inches (750 mm)", "42 inches (1050 mm)"], 1),
        new(4, "Which of the following is the correct order for lockout/tagout?", ["Lockout, verify, tagout", "Notify, lockout, verify, tagout", "Tagout, lockout, verify", "Lockout, tagout, verify"], 1),
        new(5, "Which wire type is commonly used for indoor branch circuit wiring in residential work?", ["Bare copper grounding wire only", "NM-B cable", "Flexible extension cord", "Low-voltage thermostat wire"], 1),
        new(6, "What device is required to protect receptacles in bathrooms?", ["Standard breaker", "GFCI protection", "Surge protector", "Fuse only"], 1),
        new(7, "What is the main purpose of bonding metal electrical parts?", ["Increase voltage", "Provide a safe fault path", "Reduce wire size", "Lower utility bills"], 1),
        new(8, "What should be verified after installing a new breaker?", ["Paint color", "Correct fit, rating, and operation", "Panel label only", "Only that the cover closes"], 1),
        new(9, "Which receptacle type is typically required in a garage?", ["Ungrounded two-slot", "GFCI-protected receptacle", "240V dryer outlet", "Telephone jack"], 1),
        new(10, "What must be done before working inside an energized panel?", ["Nothing if the job is quick", "Wear PPE and follow safe procedures", "Stand on concrete barefoot", "Use a plastic hammer"], 1),
        new(11, "What does AFCI protection help reduce?", ["Water leaks", "Arc-fault fire risk", "Low water pressure", "Paint damage"], 1),
        new(12, "What is the purpose of a service disconnect?", ["Decorate the panel", "Shut off power to the service", "Boost current", "Replace a transformer"], 1),
        new(13, "What is the safest way to confirm a circuit is de-energized?", ["Touch the wire quickly", "Ask someone else", "Use an approved tester or meter", "Look at the light switch"], 2),
        new(14, "Why are electrical panels required to be clearly labeled?", ["For decoration", "To identify circuits and improve safety", "To increase voltage", "To hide spare breakers"], 1),
        new(15, "Which area commonly requires weather-resistant devices outdoors?", ["Attic only", "Exterior receptacles", "Closet shelving", "Interior bedrooms"], 1),
        new(16, "What is one sign that a receptacle may need replacement?", ["It holds plugs tightly", "Cracks, heat, or loose connections", "The wall is painted", "The cover plate is white"], 1),
        new(17, "What should be checked before replacing a light fixture?", ["Only the paint color", "The homeowner's furniture", "Voltage, mounting support, and circuit condition", "Internet speed"], 2),
        new(18, "What is the main function of a grounding conductor?", ["Carry normal load current", "Increase appliance speed", "Provide a path for fault current", "Reduce room lighting"], 2),
        new(19, "If an electrical box is overfilled, what is the risk?", ["Better cooling", "Faster installation", "Loose or damaged conductors", "Lower ampacity demand"], 2),
        new(20, "After finishing the exam, what unlocks job eligibility?", ["Submitting any photo", "Choosing multiple trades", "Passing the exam and completing verification", "Skipping the license upload"], 2),
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
