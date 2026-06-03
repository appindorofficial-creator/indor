using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class BathroomExamCatalog
{
    public const string TradeCode = "bathroom";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "Before bathroom demolition begins, what should be done first?", ["Shut off water and power to the work area", "Remove the vanity mirror", "Start breaking floor tile", "Open the bathroom window"], 0),
        new(2, "What is the main purpose of waterproofing behind shower tile?", ["Prevent water intrusion into walls and subfloor", "Improve paint adhesion", "Make grout dry faster", "Reduce the cost of tile"], 0),
        new(3, "Which tool is best for checking whether a wall is plumb before tile installation?", ["Level", "Tape measure", "Utility knife", "Caulk gun"], 0),
        new(4, "When removing an old toilet, what component should be replaced if it is worn or damaged?", ["Wax ring", "Shower valve", "Vanity top", "Medicine cabinet"], 0),
        new(5, "What should be verified before installing a new vanity?", ["Plumbing shutoffs and wall condition", "Only the paint color", "The ceiling fan brand", "The door hardware"], 0),
        new(6, "When cutting tile for a shower niche, what tool is commonly used?", ["Tile wet saw or appropriate tile cutter", "Wood chisel only", "Hammer and nails", "Spray foam gun"], 0),
        new(7, "Why should expansion joints be considered in large tile floors?", ["To allow for movement and help prevent cracking", "To eliminate grout", "To speed up drying only", "To avoid using spacers"], 0),
        new(8, "What helps protect tub and shower surfaces during construction?", ["Protective coverings over finished surfaces", "Leaving tools on the edge", "Skipping cleanup", "Painting before tile cure"], 0),
        new(9, "What is the purpose of a properly sloped shower pan or floor?", ["To direct water toward the drain", "To keep the room warmer", "To make tile shine more", "To reduce grout joints"], 0),
        new(10, "Which material is commonly used to seal the joint between a tub or shower and wall finish?", ["Silicone caulk", "Wood filler", "Drywall mud", "Spray paint"], 0),
        new(11, "Why is it important to inspect the wall studs after demolition?", ["To confirm the framing is sound and ready for new finishes", "To see if mirrors can be reused only", "To make the room look bigger", "To skip waterproofing"], 0),
        new(12, "Before installing a new bathroom exhaust fan, what should be verified?", ["Proper electrical supply and venting path to the exterior", "The color of the light bulbs", "The faucet finish", "The shower curtain size"], 0),
        new(13, "Why should the notched trowel size match the tile being installed?", ["For proper mortar coverage and bond", "To change the tile color", "To lower the ceiling height", "To replace grout"], 0),
        new(14, "Where should a toilet flange ideally finish after the bathroom floor is completed?", ["On top of the finished floor", "Below the subfloor", "Behind the vanity", "Inside the wall cavity"], 0),
        new(15, "Before grouting newly installed tile, what is the best practice?", ["Let the setting material cure according to manufacturer instructions", "Apply grout immediately while mortar is wet", "Remove all spacers and flood the floor with water", "Polish the tile with wax"], 0),
        new(16, "If subfloor water damage is discovered during a remodel, what should be done?", ["Repair or replace the damaged area before finishing the installation", "Cover it with tile and continue", "Paint over it", "Ignore it if the vanity hides it"], 0),
        new(17, "Why is GFCI protection important for bathroom receptacles?", ["To help protect people from electrical shock near water", "To increase water pressure", "To dry grout faster", "To replace the wax ring"], 0),
        new(18, "Before final project handoff, what should be tested and checked?", ["Fixtures, drainage, and finishes", "Only the company logo", "The next job address", "The crew lunch order"], 0),
        new(19, "What is the best way to protect the homeowner's property during a bathroom remodel?", ["Use floor protection and keep the work area clean", "Leave materials in walkways", "Skip daily cleanup", "Open all windows only"], 0),
        new(20, "If the project scope changes after work begins, what should the contractor do?", ["Discuss and document changes with the customer", "Continue without telling anyone", "Skip written approval", "Change the price silently"], 0),
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
