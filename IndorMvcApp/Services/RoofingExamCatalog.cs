using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class RoofingExamCatalog
{
    public const string TradeCode = "roofing";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "What should a roofer check first before accessing a roof?", ["Ladder condition and fall protection", "Interior flooring", "Paint color of the house", "Mailbox location"], 0),
        new(2, "What is the main purpose of flashing around roof penetrations?", ["Prevent water intrusion", "Decorate the roof", "Increase roof weight", "Reduce attic storage"], 0),
        new(3, "A lifted or missing shingle most commonly increases the risk of:", ["Water leaks and wind damage", "Stronger ventilation", "Lower utility bills", "Bigger gutters"], 0),
        new(4, "What do ridge and soffit vents help improve?", ["Attic airflow", "Window insulation", "Driveway drainage", "Cabinet installation"], 0),
        new(5, "When tracing a roof leak from an interior stain, where should the inspection usually begin?", ["Upslope from the stain", "At the mailbox", "Inside kitchen cabinets", "At the driveway edge"], 0),
        new(6, "A damaged pipe boot most often causes leaks around:", ["Vent pipe penetrations", "Window blinds", "Concrete slabs", "Fence posts"], 0),
        new(7, "Excessive shingle granule loss is commonly a sign of:", ["Aging or impact wear", "Improved durability", "Better attic storage", "Lower roof temperature"], 0),
        new(8, "What is the main purpose of drip edge metal?", ["Direct water away from the fascia", "Hold insulation in place", "Increase roof height", "Decorate the eaves"], 0),
        new(9, "Soft or spongy roof decking usually indicates:", ["Moisture damage or rot", "Perfect ventilation", "Stronger shingles", "Fresh paint"], 0),
        new(10, "Which fastener is typically appropriate for asphalt shingles?", ["Roofing nails", "Drywall screws", "Staples for paper", "Wood glue"], 0),
        new(11, "Where is ice and water shield commonly installed for extra protection?", ["Valleys and vulnerable eaves", "Only on interior walls", "Behind kitchen sinks", "Around ceiling fans only"], 0),
        new(12, "On a flat roof, standing water is a concern because it can:", ["Lead to leaks and membrane wear", "Improve insulation", "Reduce maintenance forever", "Strengthen the roof deck"], 0),
        new(13, "Step flashing is primarily used where the roof meets a:", ["Sidewall or chimney", "Garage floor", "Mailbox post", "Driveway curb"], 0),
        new(14, "A common sign of hail damage on shingles is:", ["Bruising or impact marks", "Bigger gutters", "Straighter fence lines", "Higher attic shelving"], 0),
        new(15, "If a shingle is missing after a storm, the best action is to:", ["Replace and secure it properly", "Ignore it for a year", "Paint the roof", "Open attic windows"], 0),
        new(16, "Good attic ventilation helps reduce:", ["Heat and moisture buildup", "The number of shingles", "Property taxes", "Foundation size"], 0),
        new(17, "Improper nail placement on shingles can lead to:", ["Leaks and blow-offs", "Wider rafters", "Better curb appeal", "Lower ceilings"], 0),
        new(18, "Gutter overflow during rain often suggests:", ["A clog or drainage issue", "Extra attic airflow", "Perfect roof performance", "Stronger underlayment"], 0),
        new(19, "What is a good customer best practice after a roof inspection?", ["Document findings and explain recommendations", "Promise work outside your scope", "Leave without notes", "Remove roof vents"], 0),
        new(20, "Before leaving a completed roofing job, the crew should:", ["Perform final cleanup and safety check", "Scatter leftover nails", "Skip material review", "Block drainage paths"], 0),
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
