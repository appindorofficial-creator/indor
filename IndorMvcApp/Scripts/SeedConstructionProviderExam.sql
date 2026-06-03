/*
  Construction company provider exam and services catalog.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

IF COL_LENGTH(N'dbo.IndorProveedores', N'TeamSize') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD TeamSize NVARCHAR(40) NULL;
    PRINT 'Column IndorProveedores.TeamSize added.';
END
GO

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'construction';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'home_additions', N'Home additions', N'fa-house-medical', 50),
    (N'full_remodels', N'Full home remodels', N'fa-house-chimney', 51),
    (N'structural_framing', N'Structural framing', N'fa-hammer', 52),
    (N'drywall', N'Drywall', N'fa-border-all', 53),
    (N'concrete_work', N'Concrete work', N'fa-road', 54),
    (N'finish_carpentry', N'Finish carpentry', N'fa-ruler-combined', 55),
    (N'decks_porches', N'Decks & porches', N'fa-umbrella-beach', 56),
    (N'exterior_renovations', N'Exterior renovations', N'fa-house', 57),
    (N'demolition', N'Demolition', N'fa-person-digging', 58),
    (N'project_management', N'Project management', N'fa-clipboard-list', 59)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'construction';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'construction', 1, 1, N'What should be reviewed before starting a remodel project on site?', N'["Project plans and scope","Client''s furniture","Paint colors only","Only the final invoice"]', 0),
(N'construction', 2, 1, N'What is the main purpose of a site safety meeting?', N'["Review hazards and safety expectations","Set decoration ideas","Choose lunch breaks","Discuss marketing"]', 0),
(N'construction', 3, 1, N'Why is it important to verify measurements before ordering materials?', N'["To reduce waste and avoid delays","To skip estimating","To make the project longer","To increase change orders"]', 0),
(N'construction', 4, 1, N'Who is responsible for checking that subcontractors understand the work scope?', N'["The site supervisor or project manager","The material supplier","The homeowner''s neighbor","The painter only"]', 0),
(N'construction', 5, 2, N'What is the best reason to document job progress with photos?', N'["To track work and keep records","To replace permits","To avoid communication","To skip inspections"]', 0),
(N'construction', 6, 2, N'What should be confirmed before demolition begins?', N'["Utilities are identified or shut off as needed","Only the flooring color","The landscaping plan","The social media post"]', 0),
(N'construction', 7, 2, N'Why is a written change order important?', N'["It documents scope, price, and approval changes","It eliminates the contract","It replaces the permit","It delays all work"]', 0),
(N'construction', 8, 2, N'When coordinating multiple trades, what is most important?', N'["Scheduling work in the correct sequence","Letting all trades arrive at once","Ignoring material lead times","Skipping supervision"]', 0),
(N'construction', 9, 3, N'What does a permit generally help confirm?', N'["That the work is approved under local rules","That the owner chose the cheapest option","That tools are new","That no inspection is needed"]', 0),
(N'construction', 10, 3, N'What is the main purpose of a project schedule?', N'["To organize tasks, timing, and coordination","To decorate the jobsite","To replace the estimate","To avoid ordering materials"]', 0),
(N'construction', 11, 3, N'Before pouring concrete or closing walls, what should usually happen?', N'["Required inspections should be completed","The final payment should be collected","The warranty should start","The appliances should be installed"]', 0),
(N'construction', 12, 3, N'What is a good practice when receiving materials on site?', N'["Check delivery quantities and condition","Leave materials in the street","Ignore damaged items","Use them without review"]', 0),
(N'construction', 13, 4, N'What is a common role of the general contractor on a residential remodel?', N'["Coordinate trades and project execution","Only order paint supplies","Skip safety planning","Avoid client communication"]', 0),
(N'construction', 14, 4, N'Why should subcontractors provide proof of insurance?', N'["To reduce liability and verify coverage","To replace permits","To avoid scheduling","To skip inspections"]', 0),
(N'construction', 15, 4, N'What should be done if a structural concern is discovered during construction?', N'["Stop work and evaluate with qualified professionals","Continue without telling anyone","Paint over the issue","Skip documentation"]', 0),
(N'construction', 16, 4, N'Before drywall installation, what should typically be complete?', N'["Framing, rough-ins, and required inspections","Final paint color selection only","Landscaping","Furniture delivery"]', 0),
(N'construction', 17, 5, N'What should be checked before final walkthrough with the client?', N'["Punch list items and completion status","Only the company logo","The next job address","The crew lunch order"]', 0),
(N'construction', 18, 5, N'Why is housekeeping important on a construction site?', N'["It helps reduce trip hazards and keep the site organized","It increases project cost","It replaces PPE","It eliminates scheduling"]', 0),
(N'construction', 19, 5, N'What is a punch list?', N'["A list of remaining corrections or incomplete items","A permit application","A demolition checklist only","A labor timesheet"]', 0),
(N'construction', 20, 5, N'What is the best reason to keep project records organized?', N'["To support communication, billing, and accountability","To avoid supervision","To skip documentation","To reduce safety meetings"]', 0);
GO

PRINT 'Construction provider exam seeded.';
GO
