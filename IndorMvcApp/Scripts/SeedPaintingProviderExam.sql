/*
  Painting provider exam, services catalog, and category flags.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'painting';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'interior_painting', N'Interior Painting', N'fa-paint-roller', 90),
    (N'exterior_painting', N'Exterior Painting', N'fa-house', 91),
    (N'cabinet_painting', N'Cabinet Painting', N'fa-box-archive', 92),
    (N'drywall_prep_patching', N'Drywall Prep & Patching', N'fa-trowel', 93),
    (N'trim_doors', N'Trim & Doors', N'fa-door-open', 94),
    (N'deck_fence_staining', N'Deck & Fence Staining', N'fa-tree', 95),
    (N'wallpaper_removal', N'Wallpaper Removal', N'fa-scroll', 96),
    (N'pressure_washing_prep', N'Pressure Washing Prep', N'fa-spray-can', 97)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'painting';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'painting', 1, 1, N'Before painting a wall, what should be done first?', N'["Apply finish coat immediately","Clean and prepare the surface","Install trim pieces","Remove all windows"]', 1),
(N'painting', 2, 1, N'What is the main reason for using painter''s tape?', N'["To speed up drying","To mix paint colors","To protect edges and create clean lines","To make paint shinier"]', 2),
(N'painting', 3, 1, N'Which protective item should be used when sanding painted surfaces?', N'["Flip-flops","Respirator or dust mask","Sunglasses only","Earbuds"]', 1),
(N'painting', 4, 1, N'Why should floors and furniture be covered before painting?', N'["To reduce paint cost","To keep the room cooler","To protect surfaces from splatters and spills","To dry paint faster"]', 2),
(N'painting', 5, 2, N'Which surface issue should be repaired before painting?', N'["Loose paint and cracks","Fresh air","Clean drop cloths","Full paint cans"]', 0),
(N'painting', 6, 2, N'Why is primer often used on patched drywall?', N'["To hide tools","To improve adhesion and even out porosity","To replace sanding","To make paint smell stronger"]', 1),
(N'painting', 7, 2, N'What is the best way to avoid roller lap marks?', N'["Let each strip dry fully first","Roll in random directions only","Keep a wet edge while rolling","Use the driest roller possible"]', 2),
(N'painting', 8, 2, N'When cutting in around trim, what helps produce a cleaner finish?', N'["A good angled brush and steady strokes","A dirty brush","Extra-thick paint blobs","Painting with the lights off"]', 0),
(N'painting', 9, 3, N'What should be done before opening a paint can that has been sitting for a while?', N'["Shake it with the lid off","Clean dust and debris from the lid area","Add water immediately","Throw away the lid"]', 1),
(N'painting', 10, 3, N'Why is stirring paint important before use?', N'["To blend pigments and sheen evenly","To cool the room","To harden the paint","To dry the paint faster"]', 0),
(N'painting', 11, 3, N'Which tool is most commonly used for smooth wall coverage?', N'["Paint roller","Pipe wrench","Tin snips","Crescent wrench"]', 0),
(N'painting', 12, 3, N'What is the main purpose of a drop cloth?', N'["To protect nearby areas from paint","To make walls shinier","To replace masking tape","To measure the room"]', 0),
(N'painting', 13, 4, N'If paint starts peeling soon after application, what is a likely cause?', N'["Poor surface preparation","Too much natural light","Using a clean brush","Protecting the floor"]', 0),
(N'painting', 14, 4, N'What is the benefit of lightly sanding between coats when needed?', N'["It improves smoothness and adhesion","It makes the wall darker","It eliminates the need for cleanup","It replaces primer every time"]', 0),
(N'painting', 15, 4, N'What should a painter do if overspray might affect nearby surfaces?', N'["Mask and protect surrounding areas","Increase spray pressure only","Ignore nearby items","Open the can wider"]', 0),
(N'painting', 16, 4, N'Which practice is best when moving through a customer''s home?', N'["Keep the work area tidy and protected","Leave tools in walkways","Track paint through the house","Block exits with ladders"]', 0),
(N'painting', 17, 5, N'What is the best response if a customer asks about the paint finish being used?', N'["Explain the selected finish clearly and professionally","Guess and move on","Avoid answering","Change the product without notice"]', 0),
(N'painting', 18, 5, N'Why is ventilation important when using many paints or coatings?', N'["It helps reduce fumes and improve safety","It makes brushes heavier","It replaces surface prep","It eliminates all drying time"]', 0),
(N'painting', 19, 5, N'What should be done with brushes and rollers after the job if they will be reused?', N'["Clean them properly according to the product type","Leave them in the sun until hard","Throw them on the floor","Store them full of wet paint"]', 0),
(N'painting', 20, 5, N'Before leaving the jobsite, what final step is most appropriate?', N'["Walk the job with the customer and clean up the area","Leave without checking the work","Hide leftover paint with trash","Remove wall plates after painting is done"]', 0);
GO

PRINT 'Painting provider exam seeded.';
GO
