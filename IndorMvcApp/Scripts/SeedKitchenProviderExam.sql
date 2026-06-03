/*
  Kitchen remodeling provider exam and services catalog.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'kitchen';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'full_kitchen_remodel', N'Full kitchen remodel', N'fa-utensils', 70),
    (N'cabinet_installation', N'Cabinet installation', N'fa-box-archive', 71),
    (N'cabinet_replacement', N'Cabinet replacement', N'fa-boxes-stacked', 72),
    (N'countertop_installation', N'Countertop installation', N'fa-table-cells', 73),
    (N'backsplash_installation', N'Backsplash installation', N'fa-border-all', 74),
    (N'kitchen_flooring', N'Flooring', N'fa-grip-lines', 75),
    (N'painting_finish_work', N'Painting & finish work', N'fa-paint-roller', 76),
    (N'appliance_installation', N'Appliance installation', N'fa-blender', 77),
    (N'sink_faucet_replacement', N'Sink & faucet replacement', N'fa-faucet', 78),
    (N'lighting_fixture_coordination', N'Lighting & fixture coordination', N'fa-lightbulb', 79),
    (N'kitchen_demolition', N'Demolition', N'fa-hammer', 80),
    (N'trim_finish_carpentry', N'Trim & finish carpentry', N'fa-ruler-combined', 81)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'kitchen';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'kitchen', 1, 1, N'Before removing old cabinets, what should be done first?', N'["Cut the new countertops","Shut off utilities in the work area","Install backsplash","Paint the walls"]', 1),
(N'kitchen', 2, 1, N'Why is it important to confirm final kitchen measurements before ordering cabinets?', N'["To reduce demolition dust","To make sure cabinets fit the layout correctly","To avoid checking wall condition","To skip appliance planning"]', 1),
(N'kitchen', 3, 1, N'Which item should be protected before demolition begins?', N'["Exposed wiring only","Floors and nearby finished areas","Cabinet hardware only","Empty cardboard boxes"]', 1),
(N'kitchen', 4, 1, N'When planning a kitchen layout, what should be verified early?', N'["The music playlist","Appliance sizes and placement","The paint brand only","The final cleaning date"]', 1),
(N'kitchen', 5, 2, N'What is the main purpose of a kitchen work triangle?', N'["To choose paint colors","To improve movement between sink, stove, and refrigerator","To reduce countertop thickness","To select cabinet hardware"]', 1),
(N'kitchen', 6, 2, N'Before installing new base cabinets, what must be checked?', N'["That the floor is level","That the backsplash is finished first","That the windows are open","That the refrigerator is running"]', 0),
(N'kitchen', 7, 2, N'Why should backsplash and countertop selections be coordinated early?', N'["To avoid checking measurements","To make sure finishes and fit work together","To increase demolition time","To skip cabinet layout review"]', 1),
(N'kitchen', 8, 2, N'When replacing kitchen lighting, what is most important first?', N'["Matching the music playlist","Confirming safe electrical planning and fixture placement","Buying paint rollers","Ordering bar stools"]', 1),
(N'kitchen', 9, 3, N'Why is it important to verify rough plumbing and electrical locations before cabinet installation?', N'["So cabinets and appliances fit without conflicts","So flooring can be skipped","So drywall never needs repair","So paint can be chosen faster"]', 0),
(N'kitchen', 10, 3, N'What should be done before templating for countertops?', N'["Cabinets should be installed and secured in place","The final cleaning should be finished","Appliances should be removed from the store box only","The backsplash should already be grouted"]', 0),
(N'kitchen', 11, 3, N'What is a key reason to protect adjacent rooms during demolition?', N'["To keep dust and debris from spreading","To avoid ordering cabinets","To change the sink style","To increase labor time"]', 0),
(N'kitchen', 12, 3, N'When installing cabinets, why are shims commonly used?', N'["To decorate open shelves","To level and align cabinets properly","To waterproof countertops","To hide plumbing leaks"]', 1),
(N'kitchen', 13, 4, N'What is the best reason to review appliance specifications before finalizing the design?', N'["To ensure openings, utility needs, and clearances are correct","To choose wall paint first","To reduce the number of cabinet doors","To avoid checking ventilation"]', 0),
(N'kitchen', 14, 4, N'When setting wall cabinets, what should be confirmed first?', N'["That upper cabinet height and support attachment points are correct","That the floor tile is already sealed","That the garbage disposal is plugged in","That the stools match the island color"]', 0),
(N'kitchen', 15, 4, N'Why should ventilation be considered in a kitchen remodel?', N'["To improve removal of heat, smoke, and cooking odors","To make cabinet doors heavier","To lower backsplash height","To avoid measuring the range"]', 0),
(N'kitchen', 16, 4, N'What is a good practice before starting finish work and punch-out?', N'["Ignore minor defects","Inspect surfaces, alignments, and hardware function","Remove all appliance manuals","Skip testing lights and outlets"]', 1),
(N'kitchen', 17, 5, N'Why is waterproofing around sink and splash areas important?', N'["To protect surrounding materials from moisture damage","To reduce cabinet storage","To select paint brushes","To avoid measuring the countertop"]', 0),
(N'kitchen', 18, 5, N'Before turning over a completed kitchen remodel, what should be tested?', N'["Appliances, lights, outlets, plumbing fixtures, and cabinet hardware","Only the paint color in daylight","Only the broom closet door","Only the countertop edge color"]', 0),
(N'kitchen', 19, 5, N'What is the purpose of a final walkthrough with the customer?', N'["To review completed work, confirm function, and note any punch-list items","To skip documentation","To remove all labels before inspection","To choose a new sink color after installation"]', 0),
(N'kitchen', 20, 5, N'What is the best reason for maintaining a clean and organized jobsite during the remodel?', N'["To improve safety, efficiency, and customer confidence","To make demolition louder","To avoid using protective coverings","To reduce layout accuracy"]', 0);
GO

PRINT 'Kitchen remodeling provider exam seeded.';
GO
