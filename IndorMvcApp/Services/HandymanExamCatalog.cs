using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class HandymanExamCatalog
{
    public const string TradeCode = "handyman";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "Before drilling into a wall, what should be checked first?", ["Hidden wires or pipes", "Window height", "Paint color", "Floor finish"], 0),
        new(2, "What is the best way to prepare a surface before applying new caulk?", ["Clean it and remove old caulk", "Apply new caulk over dirt", "Wet the surface", "Skip preparation"], 0),
        new(3, "When hanging a heavy shelf, what provides the safest support?", ["Wall studs or proper anchors", "Tape only", "Thumbtacks", "Small finish nails only"], 0),
        new(4, "What should be done before patching a damaged drywall hole?", ["Remove loose material and clean the area", "Add water only", "Paint first", "Ignore cracked edges"], 0),
        new(5, "When mounting a TV bracket, what is the safest fastening method?", ["Secure it into wall studs", "Use glue only", "Use push pins", "Hang it from trim"], 0),
        new(6, "Which tool is best for checking whether a shelf is straight?", ["Level", "Hammer", "Paint brush", "Pliers"], 0),
        new(7, "Before removing a door hinge, what helps prevent the door from falling?", ["Support the door", "Paint the frame", "Open a window", "Loosen every screw at once"], 0),
        new(8, "After applying joint compound to a wall patch, what usually comes next before painting?", ["Let it dry and sand it smooth", "Wash it with soap", "Install anchors", "Apply grout"], 0),
        new(9, "What is the proper first step when a customer reports loose cabinet hardware?", ["Inspect the screws and mounting points", "Paint the cabinet", "Replace the countertop", "Ignore the issue"], 0),
        new(10, "What is the safest first step before replacing a light fixture?", ["Turn off the breaker and verify power is off", "Remove the bulbs only", "Touch the wires carefully", "Spray the fixture with water"], 0),
        new(11, "For a small hole in drywall, what repair material is commonly used?", ["Spackle or patch compound", "Roof shingles", "Concrete mix", "Plumbing putty"], 0),
        new(12, "When resealing around a bathtub, what condition should the area be in before new caulk is applied?", ["Clean and completely dry", "Wet and soapy", "Covered in dust", "Freshly painted"], 0),
        new(13, "What anchor type is commonly used when mounting into drywall without a stud?", ["A drywall anchor rated for the load", "A paper clip", "A carpet tack", "A zip tie"], 0),
        new(14, "When trimming a piece of baseboard for an inside corner, what tool is often used for a clean angled cut?", ["Miter saw or miter box", "Pipe wrench", "Garden shovel", "Stapler"], 0),
        new(15, "What is the best response if a repair request is beyond your skill or licensing scope?", ["Explain the limitation and recommend the proper specialist", "Attempt it anyway", "Hide the problem", "Charge extra and guess"], 0),
        new(16, "Before using a ladder indoors, what should be confirmed?", ["It is on a firm, level surface and fully opened", "It is leaning on a chair", "It is placed on a rug edge", "It is missing a foot"], 0),
        new(17, "What is a common fix for a squeaky interior door?", ["Lubricate the hinge pins", "Cut the frame", "Replace the floor", "Turn off the water"], 0),
        new(18, "What should be checked before installing wall-mounted hardware in a bathroom?", ["Wall type, moisture exposure, and secure fastening", "Only the mirror size", "Only the paint color", "Nothing, install immediately"], 0),
        new(19, "What is the best way to repair a loose towel bar bracket?", ["Re-anchor it securely to solid backing or proper anchors", "Add soap around it", "Use tape only", "Ignore the loose mount"], 0),
        new(20, "Why is it important to protect the work area before a repair?", ["To avoid damage and keep the customer's home clean", "To hide tools", "To make the job slower", "To reduce lighting"], 0),
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
