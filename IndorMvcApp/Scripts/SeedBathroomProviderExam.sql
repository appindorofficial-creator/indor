/*
  Bathroom remodeling provider exam and services catalog.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

IF COL_LENGTH(N'dbo.IndorProveedores', N'IsLicensed') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD IsLicensed BIT NOT NULL CONSTRAINT DF_IndorProveedores_IsLicensed DEFAULT (0);
    PRINT 'Column IndorProveedores.IsLicensed added.';
END
GO

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'bathroom';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'full_bathroom_renovation', N'Full Bathroom Renovation', N'fa-bath', 60),
    (N'shower_tub_install', N'Shower / Tub Installation', N'fa-shower', 61),
    (N'vanity_install', N'Vanity Installation', N'fa-sink', 62),
    (N'tile_flooring', N'Tile & Flooring', N'fa-border-all', 63),
    (N'toilet_install', N'Toilet Installation', N'fa-toilet', 64),
    (N'fixture_replacement', N'Fixture Replacement', N'fa-faucet', 65),
    (N'waterproofing', N'Waterproofing', N'fa-droplet', 66),
    (N'drywall_paint', N'Drywall & Paint', N'fa-paint-roller', 67),
    (N'accessibility_upgrades', N'Accessibility Upgrades', N'fa-wheelchair', 68),
    (N'glass_door_install', N'Glass Door Installation', N'fa-door-open', 69)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'bathroom';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'bathroom', 1, 1, N'Before bathroom demolition begins, what should be done first?', N'["Shut off water and power to the work area","Remove the vanity mirror","Start breaking floor tile","Open the bathroom window"]', 0),
(N'bathroom', 2, 1, N'What is the main purpose of waterproofing behind shower tile?', N'["Prevent water intrusion into walls and subfloor","Improve paint adhesion","Make grout dry faster","Reduce the cost of tile"]', 0),
(N'bathroom', 3, 1, N'Which tool is best for checking whether a wall is plumb before tile installation?', N'["Level","Tape measure","Utility knife","Caulk gun"]', 0),
(N'bathroom', 4, 1, N'When removing an old toilet, what component should be replaced if it is worn or damaged?', N'["Wax ring","Shower valve","Vanity top","Medicine cabinet"]', 0),
(N'bathroom', 5, 2, N'What should be verified before installing a new vanity?', N'["Plumbing shutoffs and wall condition","Only the paint color","The ceiling fan brand","The door hardware"]', 0),
(N'bathroom', 6, 2, N'When cutting tile for a shower niche, what tool is commonly used?', N'["Tile wet saw or appropriate tile cutter","Wood chisel only","Hammer and nails","Spray foam gun"]', 0),
(N'bathroom', 7, 2, N'Why should expansion joints be considered in large tile floors?', N'["To allow for movement and help prevent cracking","To eliminate grout","To speed up drying only","To avoid using spacers"]', 0),
(N'bathroom', 8, 2, N'What helps protect tub and shower surfaces during construction?', N'["Protective coverings over finished surfaces","Leaving tools on the edge","Skipping cleanup","Painting before tile cure"]', 0),
(N'bathroom', 9, 3, N'What is the purpose of a properly sloped shower pan or floor?', N'["To direct water toward the drain","To keep the room warmer","To make tile shine more","To reduce grout joints"]', 0),
(N'bathroom', 10, 3, N'Which material is commonly used to seal the joint between a tub or shower and wall finish?', N'["Silicone caulk","Wood filler","Drywall mud","Spray paint"]', 0),
(N'bathroom', 11, 3, N'Why is it important to inspect the wall studs after demolition?', N'["To confirm the framing is sound and ready for new finishes","To see if mirrors can be reused only","To make the room look bigger","To skip waterproofing"]', 0),
(N'bathroom', 12, 3, N'Before installing a new bathroom exhaust fan, what should be verified?', N'["Proper electrical supply and venting path to the exterior","The color of the light bulbs","The faucet finish","The shower curtain size"]', 0),
(N'bathroom', 13, 4, N'Why should the notched trowel size match the tile being installed?', N'["For proper mortar coverage and bond","To change the tile color","To lower the ceiling height","To replace grout"]', 0),
(N'bathroom', 14, 4, N'Where should a toilet flange ideally finish after the bathroom floor is completed?', N'["On top of the finished floor","Below the subfloor","Behind the vanity","Inside the wall cavity"]', 0),
(N'bathroom', 15, 4, N'Before grouting newly installed tile, what is the best practice?', N'["Let the setting material cure according to manufacturer instructions","Apply grout immediately while mortar is wet","Remove all spacers and flood the floor with water","Polish the tile with wax"]', 0),
(N'bathroom', 16, 4, N'If subfloor water damage is discovered during a remodel, what should be done?', N'["Repair or replace the damaged area before finishing the installation","Cover it with tile and continue","Paint over it","Ignore it if the vanity hides it"]', 0),
(N'bathroom', 17, 5, N'Why is GFCI protection important for bathroom receptacles?', N'["To help protect people from electrical shock near water","To increase water pressure","To dry grout faster","To replace the wax ring"]', 0),
(N'bathroom', 18, 5, N'Before final project handoff, what should be tested and checked?', N'["Fixtures, drainage, and finishes","Only the company logo","The next job address","The crew lunch order"]', 0),
(N'bathroom', 19, 5, N'What is the best way to protect the homeowner''s property during a bathroom remodel?', N'["Use floor protection and keep the work area clean","Leave materials in walkways","Skip daily cleanup","Open all windows only"]', 0),
(N'bathroom', 20, 5, N'If the project scope changes after work begins, what should the contractor do?', N'["Discuss and document changes with the customer","Continue without telling anyone","Skip written approval","Change the price silently"]', 0);
GO

PRINT 'Bathroom remodeling provider exam seeded.';
GO
