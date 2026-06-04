using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class PaintingExamCatalog
{
    public const string TradeCode = "painting";
    public const int QuestionsPerPage = 4;

    public static readonly IReadOnlyList<ExamQuestion> Questions =
    [
        new(1, "Before painting a wall, what should be done first?", ["Apply finish coat immediately", "Clean and prepare the surface", "Install trim pieces", "Remove all windows"], 1),
        new(2, "What is the main reason for using painter's tape?", ["To speed up drying", "To mix paint colors", "To protect edges and create clean lines", "To make paint shinier"], 2),
        new(3, "Which protective item should be used when sanding painted surfaces?", ["Flip-flops", "Respirator or dust mask", "Sunglasses only", "Earbuds"], 1),
        new(4, "Why should floors and furniture be covered before painting?", ["To reduce paint cost", "To keep the room cooler", "To protect surfaces from splatters and spills", "To dry paint faster"], 2),
        new(5, "Which surface issue should be repaired before painting?", ["Loose paint and cracks", "Fresh air", "Clean drop cloths", "Full paint cans"], 0),
        new(6, "Why is primer often used on patched drywall?", ["To hide tools", "To improve adhesion and even out porosity", "To replace sanding", "To make paint smell stronger"], 1),
        new(7, "What is the best way to avoid roller lap marks?", ["Let each strip dry fully first", "Roll in random directions only", "Keep a wet edge while rolling", "Use the driest roller possible"], 2),
        new(8, "When cutting in around trim, what helps produce a cleaner finish?", ["A good angled brush and steady strokes", "A dirty brush", "Extra-thick paint blobs", "Painting with the lights off"], 0),
        new(9, "What should be done before opening a paint can that has been sitting for a while?", ["Shake it with the lid off", "Clean dust and debris from the lid area", "Add water immediately", "Throw away the lid"], 1),
        new(10, "Why is stirring paint important before use?", ["To blend pigments and sheen evenly", "To cool the room", "To harden the paint", "To dry the paint faster"], 0),
        new(11, "Which tool is most commonly used for smooth wall coverage?", ["Paint roller", "Pipe wrench", "Tin snips", "Crescent wrench"], 0),
        new(12, "What is the main purpose of a drop cloth?", ["To protect nearby areas from paint", "To make walls shinier", "To replace masking tape", "To measure the room"], 0),
        new(13, "If paint starts peeling soon after application, what is a likely cause?", ["Poor surface preparation", "Too much natural light", "Using a clean brush", "Protecting the floor"], 0),
        new(14, "What is the benefit of lightly sanding between coats when needed?", ["It improves smoothness and adhesion", "It makes the wall darker", "It eliminates the need for cleanup", "It replaces primer every time"], 0),
        new(15, "What should a painter do if overspray might affect nearby surfaces?", ["Mask and protect surrounding areas", "Increase spray pressure only", "Ignore nearby items", "Open the can wider"], 0),
        new(16, "Which practice is best when moving through a customer's home?", ["Keep the work area tidy and protected", "Leave tools in walkways", "Track paint through the house", "Block exits with ladders"], 0),
        new(17, "What is the best response if a customer asks about the paint finish being used?", ["Explain the selected finish clearly and professionally", "Guess and move on", "Avoid answering", "Change the product without notice"], 0),
        new(18, "Why is ventilation important when using many paints or coatings?", ["It helps reduce fumes and improve safety", "It makes brushes heavier", "It replaces surface prep", "It eliminates all drying time"], 0),
        new(19, "What should be done with brushes and rollers after the job if they will be reused?", ["Clean them properly according to the product type", "Leave them in the sun until hard", "Throw them on the floor", "Store them full of wet paint"], 0),
        new(20, "Before leaving the jobsite, what final step is most appropriate?", ["Walk the job with the customer and clean up the area", "Leave without checking the work", "Hide leftover paint with trash", "Remove wall plates after painting is done"], 0),
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
