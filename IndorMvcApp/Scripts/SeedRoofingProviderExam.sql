/*
  Roofing provider exam and services catalog.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'roofing';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'shingle_roof_replacement', N'Shingle roof replacement', N'fa-house-chimney', 90),
    (N'roof_repairs', N'Roof repairs', N'fa-hammer', 91),
    (N'metal_roofing', N'Metal roofing', N'fa-sheet-plastic', 92),
    (N'flat_roofing', N'Flat roofing', N'fa-square', 93),
    (N'leak_detection', N'Leak detection', N'fa-magnifying-glass-droplet', 94),
    (N'flashing_ventilation', N'Flashing & ventilation', N'fa-wind', 95),
    (N'gutter_installation', N'Gutter installation', N'fa-grip-lines', 96),
    (N'emergency_tarp_service', N'Emergency tarp service', N'fa-house-crack', 97)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'roofing';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'roofing', 1, 1, N'What should a roofer check first before accessing a roof?', N'["Ladder condition and fall protection","Interior flooring","Paint color of the house","Mailbox location"]', 0),
(N'roofing', 2, 1, N'What is the main purpose of flashing around roof penetrations?', N'["Prevent water intrusion","Decorate the roof","Increase roof weight","Reduce attic storage"]', 0),
(N'roofing', 3, 1, N'A lifted or missing shingle most commonly increases the risk of:', N'["Water leaks and wind damage","Stronger ventilation","Lower utility bills","Bigger gutters"]', 0),
(N'roofing', 4, 1, N'What do ridge and soffit vents help improve?', N'["Attic airflow","Window insulation","Driveway drainage","Cabinet installation"]', 0),
(N'roofing', 5, 2, N'When tracing a roof leak from an interior stain, where should the inspection usually begin?', N'["Upslope from the stain","At the mailbox","Inside kitchen cabinets","At the driveway edge"]', 0),
(N'roofing', 6, 2, N'A damaged pipe boot most often causes leaks around:', N'["Vent pipe penetrations","Window blinds","Concrete slabs","Fence posts"]', 0),
(N'roofing', 7, 2, N'Excessive shingle granule loss is commonly a sign of:', N'["Aging or impact wear","Improved durability","Better attic storage","Lower roof temperature"]', 0),
(N'roofing', 8, 2, N'What is the main purpose of drip edge metal?', N'["Direct water away from the fascia","Hold insulation in place","Increase roof height","Decorate the eaves"]', 0),
(N'roofing', 9, 3, N'Soft or spongy roof decking usually indicates:', N'["Moisture damage or rot","Perfect ventilation","Stronger shingles","Fresh paint"]', 0),
(N'roofing', 10, 3, N'Which fastener is typically appropriate for asphalt shingles?', N'["Roofing nails","Drywall screws","Staples for paper","Wood glue"]', 0),
(N'roofing', 11, 3, N'Where is ice and water shield commonly installed for extra protection?', N'["Valleys and vulnerable eaves","Only on interior walls","Behind kitchen sinks","Around ceiling fans only"]', 0),
(N'roofing', 12, 3, N'On a flat roof, standing water is a concern because it can:', N'["Lead to leaks and membrane wear","Improve insulation","Reduce maintenance forever","Strengthen the roof deck"]', 0),
(N'roofing', 13, 4, N'Step flashing is primarily used where the roof meets a:', N'["Sidewall or chimney","Garage floor","Mailbox post","Driveway curb"]', 0),
(N'roofing', 14, 4, N'A common sign of hail damage on shingles is:', N'["Bruising or impact marks","Bigger gutters","Straighter fence lines","Higher attic shelving"]', 0),
(N'roofing', 15, 4, N'If a shingle is missing after a storm, the best action is to:', N'["Replace and secure it properly","Ignore it for a year","Paint the roof","Open attic windows"]', 0),
(N'roofing', 16, 4, N'Good attic ventilation helps reduce:', N'["Heat and moisture buildup","The number of shingles","Property taxes","Foundation size"]', 0),
(N'roofing', 17, 5, N'Improper nail placement on shingles can lead to:', N'["Leaks and blow-offs","Wider rafters","Better curb appeal","Lower ceilings"]', 0),
(N'roofing', 18, 5, N'Gutter overflow during rain often suggests:', N'["A clog or drainage issue","Extra attic airflow","Perfect roof performance","Stronger underlayment"]', 0),
(N'roofing', 19, 5, N'What is a good customer best practice after a roof inspection?', N'["Document findings and explain recommendations","Promise work outside your scope","Leave without notes","Remove roof vents"]', 0),
(N'roofing', 20, 5, N'Before leaving a completed roofing job, the crew should:', N'["Perform final cleanup and safety check","Scatter leftover nails","Skip material review","Block drainage paths"]', 0);
GO

PRINT 'Roofing provider exam seeded.';
GO
