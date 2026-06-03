/*
  Handyman provider exam, services catalog, and profile columns.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

IF COL_LENGTH(N'dbo.IndorProveedores', N'ServiceDescription') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD ServiceDescription NVARCHAR(200) NULL;
    PRINT 'Column IndorProveedores.ServiceDescription added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'IsInsured') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD IsInsured BIT NOT NULL CONSTRAINT DF_IndorProveedores_IsInsured DEFAULT (0);
    PRINT 'Column IndorProveedores.IsInsured added.';
END
GO

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'handyman';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'drywall_patch', N'Drywall patch & repair', N'fa-trowel', 40),
    (N'door_adjustments', N'Door adjustments', N'fa-door-open', 41),
    (N'tv_mounting', N'TV / picture mounting', N'fa-tv', 42),
    (N'shelving_install', N'Shelving installation', N'fa-boxes-stacked', 43),
    (N'furniture_assembly', N'Furniture assembly', N'fa-chair', 44),
    (N'hardware_replacement', N'Hardware replacement', N'fa-screwdriver', 45),
    (N'caulking_sealing', N'Caulking & sealing', N'fa-fill-drip', 46),
    (N'punch_list', N'Minor punch-list repairs', N'fa-clipboard-list', 47)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'handyman';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'handyman', 1, 1, N'Before drilling into a wall, what should be checked first?', N'["Hidden wires or pipes","Window height","Paint color","Floor finish"]', 0),
(N'handyman', 2, 1, N'What is the best way to prepare a surface before applying new caulk?', N'["Clean it and remove old caulk","Apply new caulk over dirt","Wet the surface","Skip preparation"]', 0),
(N'handyman', 3, 1, N'When hanging a heavy shelf, what provides the safest support?', N'["Wall studs or proper anchors","Tape only","Thumbtacks","Small finish nails only"]', 0),
(N'handyman', 4, 1, N'What should be done before patching a damaged drywall hole?', N'["Remove loose material and clean the area","Add water only","Paint first","Ignore cracked edges"]', 0),
(N'handyman', 5, 2, N'When mounting a TV bracket, what is the safest fastening method?', N'["Secure it into wall studs","Use glue only","Use push pins","Hang it from trim"]', 0),
(N'handyman', 6, 2, N'Which tool is best for checking whether a shelf is straight?', N'["Level","Hammer","Paint brush","Pliers"]', 0),
(N'handyman', 7, 2, N'Before removing a door hinge, what helps prevent the door from falling?', N'["Support the door","Paint the frame","Open a window","Loosen every screw at once"]', 0),
(N'handyman', 8, 2, N'After applying joint compound to a wall patch, what usually comes next before painting?', N'["Let it dry and sand it smooth","Wash it with soap","Install anchors","Apply grout"]', 0),
(N'handyman', 9, 3, N'What is the proper first step when a customer reports loose cabinet hardware?', N'["Inspect the screws and mounting points","Paint the cabinet","Replace the countertop","Ignore the issue"]', 0),
(N'handyman', 10, 3, N'What is the safest first step before replacing a light fixture?', N'["Turn off the breaker and verify power is off","Remove the bulbs only","Touch the wires carefully","Spray the fixture with water"]', 0),
(N'handyman', 11, 3, N'For a small hole in drywall, what repair material is commonly used?', N'["Spackle or patch compound","Roof shingles","Concrete mix","Plumbing putty"]', 0),
(N'handyman', 12, 3, N'When resealing around a bathtub, what condition should the area be in before new caulk is applied?', N'["Clean and completely dry","Wet and soapy","Covered in dust","Freshly painted"]', 0),
(N'handyman', 13, 4, N'What anchor type is commonly used when mounting into drywall without a stud?', N'["A drywall anchor rated for the load","A paper clip","A carpet tack","A zip tie"]', 0),
(N'handyman', 14, 4, N'When trimming a piece of baseboard for an inside corner, what tool is often used for a clean angled cut?', N'["Miter saw or miter box","Pipe wrench","Garden shovel","Stapler"]', 0),
(N'handyman', 15, 4, N'What is the best response if a repair request is beyond your skill or licensing scope?', N'["Explain the limitation and recommend the proper specialist","Attempt it anyway","Hide the problem","Charge extra and guess"]', 0),
(N'handyman', 16, 4, N'Before using a ladder indoors, what should be confirmed?', N'["It is on a firm, level surface and fully opened","It is leaning on a chair","It is placed on a rug edge","It is missing a foot"]', 0),
(N'handyman', 17, 5, N'What is a common fix for a squeaky interior door?', N'["Lubricate the hinge pins","Cut the frame","Replace the floor","Turn off the water"]', 0),
(N'handyman', 18, 5, N'What should be checked before installing wall-mounted hardware in a bathroom?', N'["Wall type, moisture exposure, and secure fastening","Only the mirror size","Only the paint color","Nothing, install immediately"]', 0),
(N'handyman', 19, 5, N'What is the best way to repair a loose towel bar bracket?', N'["Re-anchor it securely to solid backing or proper anchors","Add soap around it","Use tape only","Ignore the loose mount"]', 0),
(N'handyman', 20, 5, N'Why is it important to protect the work area before a repair?', N'["To avoid damage and keep the customer''s home clean","To hide tools","To make the job slower","To reduce lighting"]', 0);
GO

PRINT 'Handyman provider exam seeded.';
GO
