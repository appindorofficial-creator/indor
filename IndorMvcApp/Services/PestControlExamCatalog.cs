using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class PestControlExamCatalog
{
    public const string TradeCode = "pest";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "Before mixing or applying pest control chemicals, what should always be read first?", ["The product label", "The customer invoice", "The weather app", "The company logo"], 0),
        new(2, "What is the safest first step before treating an interior area?", ["Identify people, pets, and sensitive areas", "Spray immediately", "Close all vents permanently", "Increase the dosage"], 0),
        new(3, "Which item is basic personal protective equipment for many pest control tasks?", ["Gloves", "Sandals", "Loose scarf", "Earbuds"], 0),
        new(4, "Why is correct pest identification important?", ["To choose the right treatment method", "To make the job longer", "To avoid inspection notes", "To skip documentation"], 0),
        new(5, "What should be checked before applying an exterior treatment?", ["Wind and weather conditions", "The neighbor's mailbox", "The paint color", "The office playlist"], 0),
        new(6, "Why is measuring chemical correctly important?", ["To follow label rates safely", "To make the smell stronger", "To finish the bottle faster", "To avoid wearing gloves"], 0),
        new(7, "What is a common sign of a termite problem?", ["Mud tubes", "Bright wall paint", "Cold air from vents", "Loose doorknob"], 0),
        new(8, "What is the best first step when a customer reports ants in the kitchen?", ["Inspect and identify the source", "Spray every room immediately", "Ignore entry points", "Turn off the refrigerator"], 0),
        new(9, "Which pest is commonly associated with droppings and gnaw marks?", ["Rodents", "Butterflies", "Ladybugs", "Dragonflies"], 0),
        new(10, "When treating near food-prep areas, what is most important?", ["Protect food-contact surfaces", "Use extra product", "Leave containers open", "Skip cleanup"], 0),
        new(11, "What is the purpose of a follow-up visit in pest control?", ["Check treatment results and activity", "Raise the invoice only", "Change the company logo", "Refill every chemical bottle"], 0),
        new(12, "What should be done with unused mixed chemical when the label gives disposal instructions?", ["Follow the label disposal instructions", "Pour it in any drain", "Store it in a soda bottle", "Leave it in the truck bed"], 0),
        new(13, "What is one reason to seal or note entry points during service?", ["To help reduce future pest entry", "To increase chemical use", "To make walls look darker", "To avoid inspections"], 0),
        new(14, "Which tool is commonly used to inspect hard-to-see pest areas?", ["Flashlight", "Television remote", "Hair dryer", "Paint roller"], 0),
        new(15, "Why should treatment records be completed clearly?", ["To document work and support safe service", "To avoid wearing PPE", "To replace the label", "To shorten the inspection"], 0),
        new(16, "If a customer has pets, what should the technician do?", ["Explain safety instructions and reentry guidance", "Double the product amount", "Ignore the pets", "Block the exits"], 0),
        new(17, "What is the best reason to identify the type of cockroach before treatment?", ["Different species may require different strategies", "All roaches live the same way", "It changes the company name", "It removes the need to inspect"], 0),
        new(18, "What should be done before using application equipment?", ["Inspect it for leaks or damage", "Leave it untested", "Remove all labels", "Overfill it past the line"], 0),
        new(19, "What is one key part of good customer communication after service?", ["Explain what was found and next steps", "Say nothing and leave", "Hide service notes", "Promise impossible results"], 0),
        new(20, "What does passing the INDOR pest control exam allow the provider to unlock?", ["Pest control jobs only", "All provider categories", "Banking access", "Unlimited free ads"], 0),
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
