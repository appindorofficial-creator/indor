/*
  Cleaning provider exam and services catalog.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'cleaning';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'standard_cleaning', N'Standard Cleaning', N'fa-spray-can', 110),
    (N'deep_cleaning', N'Deep Cleaning', N'fa-bucket', 111),
    (N'move_in_move_out', N'Move-In / Move-Out', N'fa-box-open', 112),
    (N'post_construction_cleaning', N'Post-Construction Cleaning', N'fa-hard-hat', 113),
    (N'office_cleaning', N'Office Cleaning', N'fa-building', 114),
    (N'airbnb_turnover', N'Airbnb Turnover', N'fa-key', 115),
    (N'recurring_cleaning', N'Recurring Cleaning', N'fa-calendar-days', 116),
    (N'window_cleaning', N'Window Cleaning', N'fa-window-maximize', 117)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'cleaning';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'cleaning', 1, 1, N'Before mixing a cleaning chemical, what should you do first?', N'["Read the label and dilution instructions","Mix all products together","Use hot water automatically","Spray directly on surfaces"]', 0),
(N'cleaning', 2, 1, N'What is the safest order when cleaning a bathroom?', N'["Remove trash first","Use the same cloth on every surface","Start with the toilet first every time","Skip gloves to work faster"]', 0),
(N'cleaning', 3, 1, N'Which product should generally be avoided on natural stone?', N'["Acidic cleaner","Microfiber cloth","pH-neutral cleaner","Warm water"]', 0),
(N'cleaning', 4, 1, N'What should be done before vacuuming a room?', N'["Pick up large debris and inspect the floor","Close all vents","Spray bleach on carpet","Mop first"]', 0),
(N'cleaning', 5, 2, N'What helps prevent cross-contamination while cleaning?', N'["Use color-coded cloths by area","Use one sponge for all rooms","Skip sanitizing tools","Store clean and dirty tools together"]', 0),
(N'cleaning', 6, 2, N'When should a microfiber mop pad be changed?', N'["When visibly soiled or after contaminated areas","Only once a week","Only when it tears","Never during a job"]', 0),
(N'cleaning', 7, 2, N'How should high-touch surfaces be disinfected?', N'["Clean first and allow proper contact time","Wipe quickly and dry immediately","Use water only","Spray from a distance and walk away"]', 0),
(N'cleaning', 8, 2, N'How should vacuum cords be handled during service?', N'["Check for damage and keep away from walk paths","Pull the cord by force","Run over the cord with the vacuum","Leave it stretched across doorways"]', 0),
(N'cleaning', 9, 3, N'What is the purpose of disinfectant contact time?', N'["To let the surface stay wet long enough to work","To make the room smell better only","To skip rinsing","To dry surfaces immediately"]', 0),
(N'cleaning', 10, 3, N'Which cleaning direction is usually best for dusting a room?', N'["Top to bottom","Bottom to top only","Random order","Outside walls first always"]', 0),
(N'cleaning', 11, 3, N'What should you do if you find broken glass?', N'["Use gloves and a proper pickup tool","Sweep with bare hands","Ignore small pieces","Vacuum without inspection"]', 0),
(N'cleaning', 12, 3, N'What is a good first step before starting work in a customer''s home?', N'["Confirm the scope and areas to clean","Start spraying immediately","Move furniture without asking","Skip the walkthrough"]', 0),
(N'cleaning', 13, 4, N'Which flooring surface can be damaged by too much water?', N'["Hardwood flooring","Concrete only","Ceramic tile only","Metal flooring"]', 0),
(N'cleaning', 14, 4, N'What helps reduce streaks when cleaning mirrors?', N'["A clean microfiber cloth and proper glass cleaner","A strong abrasive pad","A dirty towel","Hot bleach solution"]', 0),
(N'cleaning', 15, 4, N'What should be documented after a cleaning job is completed?', N'["Completed tasks, issues found, and notes","Only the payment amount","Only the arrival time","Nothing at all"]', 0),
(N'cleaning', 16, 4, N'If a customer has pets in the home, what is the best practice?', N'["Use pet-safe practices and secure doors or gates","Spray strong chemicals near food bowls","Leave exterior doors open","Ignore pet instructions"]', 0),
(N'cleaning', 17, 5, N'What is the best practice after raw food contamination on a kitchen surface?', N'["Clean first, then disinfect with proper contact time","Use a dry cloth only","Add scented spray only","Skip sanitizing if it looks clean"]', 0),
(N'cleaning', 18, 5, N'How should cleaning chemicals be stored during service?', N'["Labeled and kept secure away from children and pets","In unlabeled bottles","Open on the floor","Mixed together in one container"]', 0),
(N'cleaning', 19, 5, N'What should you do if a stain does not respond to standard treatment?', N'["Stop and inform the customer before aggressive methods","Scrub harder with any chemical","Ignore possible damage","Cut the surface material"]', 0),
(N'cleaning', 20, 5, N'What is the final step before leaving the property?', N'["Do a final walkthrough and confirm satisfaction","Leave without checking","Turn off the water heater","Take all trash bags but skip inspection"]', 0);
GO

PRINT 'Cleaning provider exam seeded.';
GO
