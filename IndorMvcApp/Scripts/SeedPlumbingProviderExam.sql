/*
  Plumbing provider exam + RequiresTradeExam flag.
  Run after CreateProviderPortalTables.sql on Azure SQL.
*/
SET NOCOUNT ON;

IF COL_LENGTH(N'dbo.IndorProveedores', N'BusinessAddress') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD BusinessAddress NVARCHAR(300) NULL;
    PRINT 'Column IndorProveedores.BusinessAddress added.';
END
GO

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'plumbing';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'leak_repair', N'Leak repair', N'fa-droplet', 10),
    (N'drain_cleaning', N'Drain cleaning', N'fa-sink', 11),
    (N'water_heater', N'Water heater service', N'fa-fire-flame-simple', 12),
    (N'fixture_install', N'Fixture installation', N'fa-faucet-drip', 13)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'plumbing';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'plumbing', 1, 1, N'Before repairing a leaking supply line, what should be done first?', N'["Shut off the water supply","Replace the faucet handle","Open all drains","Increase water pressure"]', 0),
(N'plumbing', 2, 1, N'What is the best reason to verify pipe material before making a repair?', N'["To match fittings and repair method correctly","To increase water flow","To skip inspection","To avoid using tools"]', 0),
(N'plumbing', 3, 1, N'What tool is commonly used to tighten a compression fitting?', N'["Adjustable wrench","Paint brush","Voltage tester","Caulk gun"]', 0),
(N'plumbing', 4, 1, N'If a drain is slow but not completely blocked, what is usually the first basic troubleshooting step?', N'["Inspect and clear the trap or stopper area","Cut the pipe immediately","Replace the water heater","Increase supply pressure"]', 0),
(N'plumbing', 5, 2, N'What is the main purpose of a P-trap under a sink?', N'["To prevent sewer gases from entering the home","To increase water pressure","To cool the water","To hold extra tools"]', 0),
(N'plumbing', 6, 2, N'When soldering copper pipe, what must be done before applying solder?', N'["Clean and flux the joint","Fill the pipe with water","Paint the fitting","Tighten with a hammer"]', 0),
(N'plumbing', 7, 2, N'What is the safest first step before removing a toilet supply valve?', N'["Shut off the water and relieve pressure","Open the water heater drain","Cut the wall open","Install a new faucet"]', 0),
(N'plumbing', 8, 2, N'What is a common sign of a hidden water leak?', N'["Unexpected increase in water bill","Brighter light bulbs","Faster internet speed","Lower ceiling height"]', 0),
(N'plumbing', 9, 3, N'What should be used to seal threaded pipe connections in many basic plumbing installations?', N'["Thread seal tape or approved pipe compound","Wood glue","Spray paint","Dry paper towel"]', 0),
(N'plumbing', 10, 3, N'Why is pipe slope important on a drain line?', N'["It helps wastewater flow properly","It increases electrical voltage","It makes the pipe colder","It eliminates all venting needs"]', 0),
(N'plumbing', 11, 3, N'Before replacing a water heater, what should be confirmed first?', N'["Fuel type, size, and code requirements","Paint color in the room","Wi-Fi password","Number of windows nearby"]', 0),
(N'plumbing', 12, 3, N'What is the purpose of a shutoff valve at a fixture?', N'["To isolate water to that fixture for service","To increase drain speed","To vent sewer gas","To hold water permanently"]', 0),
(N'plumbing', 13, 4, N'If a customer reports low water pressure at only one faucet, what is a reasonable first check?', N'["Inspect the aerator for blockage","Replace the entire water main","Raise house temperature","Remove the roof vent"]', 0),
(N'plumbing', 14, 4, N'What is the function of a plumbing vent?', N'["It allows air into the system for proper drainage","It heats the drain pipe","It seals water lines shut","It replaces the need for traps"]', 0),
(N'plumbing', 15, 4, N'When using PVC solvent cement, what is important before assembly?', N'["Use the correct cleaner or primer when required","Soak the pipe in oil","Wait until the pipe is full of water","Hammer the fitting into place"]', 0),
(N'plumbing', 16, 4, N'What should be done after completing a small repair on a supply line?', N'["Restore water and check carefully for leaks","Cover the repair immediately with insulation only","Increase pressure beyond normal","Leave the valve off forever"]', 0),
(N'plumbing', 17, 5, N'What is one reason plumbers verify local code requirements before a replacement job?', N'["To ensure the installation meets required standards","To avoid using measurements","To skip permits automatically","To reduce water quality"]', 0),
(N'plumbing', 18, 5, N'What is a common use of a basin wrench?', N'["Tightening or loosening faucet nuts in tight spaces","Cutting ceramic tile","Testing electrical circuits","Cleaning roof shingles"]', 0),
(N'plumbing', 19, 5, N'Which situation usually requires urgent plumbing attention?', N'["An active burst pipe leak","A loose doormat","A dim hallway light","A squeaky door hinge"]', 0),
(N'plumbing', 20, 5, N'After finishing the plumbing exam, what happens next in this onboarding flow?', N'["INDOR reviews the exam and verification documents","The provider can immediately select every service category","The app deletes the profile automatically","No review is needed"]', 0);
GO

PRINT 'Plumbing provider exam seeded.';
GO
