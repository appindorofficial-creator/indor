using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class PlumbingExamCatalog
{
    public const string TradeCode = "plumbing";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "Before repairing a leaking supply line, what should be done first?", ["Shut off the water supply", "Replace the faucet handle", "Open all drains", "Increase water pressure"], 0),
        new(2, "What is the best reason to verify pipe material before making a repair?", ["To match fittings and repair method correctly", "To increase water flow", "To skip inspection", "To avoid using tools"], 0),
        new(3, "What tool is commonly used to tighten a compression fitting?", ["Adjustable wrench", "Paint brush", "Voltage tester", "Caulk gun"], 0),
        new(4, "If a drain is slow but not completely blocked, what is usually the first basic troubleshooting step?", ["Inspect and clear the trap or stopper area", "Cut the pipe immediately", "Replace the water heater", "Increase supply pressure"], 0),
        new(5, "What is the main purpose of a P-trap under a sink?", ["To prevent sewer gases from entering the home", "To increase water pressure", "To cool the water", "To hold extra tools"], 0),
        new(6, "When soldering copper pipe, what must be done before applying solder?", ["Clean and flux the joint", "Fill the pipe with water", "Paint the fitting", "Tighten with a hammer"], 0),
        new(7, "What is the safest first step before removing a toilet supply valve?", ["Shut off the water and relieve pressure", "Open the water heater drain", "Cut the wall open", "Install a new faucet"], 0),
        new(8, "What is a common sign of a hidden water leak?", ["Unexpected increase in water bill", "Brighter light bulbs", "Faster internet speed", "Lower ceiling height"], 0),
        new(9, "What should be used to seal threaded pipe connections in many basic plumbing installations?", ["Thread seal tape or approved pipe compound", "Wood glue", "Spray paint", "Dry paper towel"], 0),
        new(10, "Why is pipe slope important on a drain line?", ["It helps wastewater flow properly", "It increases electrical voltage", "It makes the pipe colder", "It eliminates all venting needs"], 0),
        new(11, "Before replacing a water heater, what should be confirmed first?", ["Fuel type, size, and code requirements", "Paint color in the room", "Wi-Fi password", "Number of windows nearby"], 0),
        new(12, "What is the purpose of a shutoff valve at a fixture?", ["To isolate water to that fixture for service", "To increase drain speed", "To vent sewer gas", "To hold water permanently"], 0),
        new(13, "If a customer reports low water pressure at only one faucet, what is a reasonable first check?", ["Inspect the aerator for blockage", "Replace the entire water main", "Raise house temperature", "Remove the roof vent"], 0),
        new(14, "What is the function of a plumbing vent?", ["It allows air into the system for proper drainage", "It heats the drain pipe", "It seals water lines shut", "It replaces the need for traps"], 0),
        new(15, "When using PVC solvent cement, what is important before assembly?", ["Use the correct cleaner or primer when required", "Soak the pipe in oil", "Wait until the pipe is full of water", "Hammer the fitting into place"], 0),
        new(16, "What should be done after completing a small repair on a supply line?", ["Restore water and check carefully for leaks", "Cover the repair immediately with insulation only", "Increase pressure beyond normal", "Leave the valve off forever"], 0),
        new(17, "What is one reason plumbers verify local code requirements before a replacement job?", ["To ensure the installation meets required standards", "To avoid using measurements", "To skip permits automatically", "To reduce water quality"], 0),
        new(18, "What is a common use of a basin wrench?", ["Tightening or loosening faucet nuts in tight spaces", "Cutting ceramic tile", "Testing electrical circuits", "Cleaning roof shingles"], 0),
        new(19, "Which situation usually requires urgent plumbing attention?", ["An active burst pipe leak", "A loose doormat", "A dim hallway light", "A squeaky door hinge"], 0),
        new(20, "After finishing the plumbing exam, what happens next in this onboarding flow?", ["INDOR reviews the exam and verification documents", "The provider can immediately select every service category", "The app deletes the profile automatically", "No review is needed"], 0),
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
