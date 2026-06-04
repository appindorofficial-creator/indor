/*
  Flooring provider exam and services catalog.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'flooring';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'hardwood_installation', N'Hardwood Installation', N'fa-tree', 100),
    (N'laminate', N'Laminate', N'fa-layer-group', 101),
    (N'vinyl_lvp', N'Vinyl / LVP', N'fa-square', 102),
    (N'tile_flooring', N'Tile Flooring', N'fa-border-all', 103),
    (N'carpet_installation', N'Carpet Installation', N'fa-rug', 104),
    (N'floor_repair', N'Floor Repair', N'fa-hammer', 105),
    (N'subfloor_repair', N'Subfloor Repair', N'fa-table-cells', 106),
    (N'refinishing', N'Refinishing', N'fa-spray-can', 107)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'flooring';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'flooring', 1, 1, N'What should be checked before installing any flooring?', N'["Subfloor is clean, dry, and level","Appliance warranty","Wall color","Window blinds"]', 0),
(N'flooring', 2, 1, N'Why is leaving an expansion gap important for floating floors?', N'["Allows material to expand and contract","Hides uneven walls","Makes the floor quieter only","Reduces labor cost"]', 0),
(N'flooring', 3, 1, N'Which tool is commonly used to verify a subfloor is level?', N'["Level or straightedge","Pipe wrench","Paint roller","Wet saw"]', 0),
(N'flooring', 4, 1, N'If moisture is too high in the subfloor, what should be done first?', N'["Stop and resolve the moisture issue","Install more adhesive","Nail down faster","Add extra trim"]', 0),
(N'flooring', 5, 2, N'What is the best way to start a room layout?', N'["Measure the room and plan the first row","Open all boxes randomly","Install from the middle without measuring","Cut planks before marking"]', 0),
(N'flooring', 6, 2, N'Why should flooring cartons acclimate to the room?', N'["So material adjusts to site conditions","To make boxes lighter","To change the color","To reduce trim work"]', 0),
(N'flooring', 7, 2, N'What is a common reason to stagger end joints?', N'["Improve appearance and stability","Reduce cleaning time","Eliminate expansion gaps","Avoid underlayment"]', 0),
(N'flooring', 8, 2, N'When cutting flooring indoors, what is most important?', N'["Use PPE and control dust","Work barefoot","Cut without measuring","Leave tools plugged in unattended"]', 0),
(N'flooring', 9, 3, N'What must be verified before installing tile in a wet area?', N'["Proper waterproofing or approved substrate","Ceiling fan size","Cabinet paint color","Appliance brand"]', 0),
(N'flooring', 10, 3, N'Which underlayment function is correct?', N'["Helps with support, moisture, or sound depending on product","Replaces subfloor repairs","Prevents all movement forever","Eliminates need for layout"]', 0),
(N'flooring', 11, 3, N'For glue-down flooring, why is trowel size important?', N'["It controls adhesive spread rate","It changes board color","It removes need for rolling","It replaces expansion gaps"]', 0),
(N'flooring', 12, 3, N'Before installing hardwood planks, the installer should:', N'["Check moisture content of wood and subfloor","Turn on all faucets","Remove all baseboards forever","Skip site inspection"]', 0),
(N'flooring', 13, 4, N'What is the main purpose of a transition strip?', N'["Create a clean change between floor surfaces or heights","Hold cabinets in place","Waterproof every room","Replace baseboard"]', 0),
(N'flooring', 14, 4, N'If a plank is damaged during install, what should the installer do?', N'["Replace it before finishing the floor","Hide it under a rug","Glue it and continue if cracked","Ignore it"]', 0),
(N'flooring', 15, 4, N'In customer areas, the worksite should be kept:', N'["Clean, protected, and safe to walk through","Loud and crowded","Full of loose nails","Open to children"]', 0),
(N'flooring', 16, 4, N'What should be done after finishing an installation?', N'["Inspect the floor and review care instructions with the customer","Leave scrap everywhere","Remove receipts only","Tell customer not to ask questions"]', 0),
(N'flooring', 17, 5, N'Which direction is often preferred when laying planks in a narrow room?', N'["A planned layout that fits the room and light direction","Always the shortest wall no matter what","Random direction with no plan","Toward the door only"]', 0),
(N'flooring', 18, 5, N'If the final row will be too narrow, the installer should:', N'["Adjust the starting row so first and last rows balance","Force narrow pieces into place","Skip the final row","Add more adhesive"]', 0),
(N'flooring', 19, 5, N'Why is it important to check manufacturer installation instructions?', N'["Products have specific requirements and warranty rules","To avoid measuring","To skip acclimation","All floors install the same way"]', 0),
(N'flooring', 20, 5, N'What is best practice when handing over the finished flooring job?', N'["Confirm completion, cleanup, care guidance, and any next steps","Leave without speaking","Ask customer to clean debris","Promise repairs without inspection"]', 0);
GO

PRINT 'Flooring provider exam seeded.';
GO
