/*
  Appliance repair provider exam and services catalog.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'appliance';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'refrigerator', N'Refrigerator', N'fa-snowflake', 140),
    (N'freezer', N'Freezer', N'fa-icicles', 141),
    (N'dishwasher', N'Dishwasher', N'fa-sink', 142),
    (N'oven_range', N'Oven / Range', N'fa-fire-burner', 143),
    (N'cooktop', N'Cooktop', N'fa-kitchen-set', 144),
    (N'microwave', N'Microwave', N'fa-box', 145),
    (N'washer', N'Washer', N'fa-shirt', 146),
    (N'dryer', N'Dryer', N'fa-wind', 147),
    (N'garbage_disposal', N'Garbage Disposal', N'fa-recycle', 148),
    (N'ice_maker', N'Ice Maker', N'fa-cubes', 149),
    (N'trash_compactor', N'Trash Compactor', N'fa-trash-can', 150),
    (N'other_small_appliances', N'Other Small Appliances', N'fa-blender', 151)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'appliance';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'appliance', 1, 1, N'What should be done first before servicing any appliance?', N'["Disconnect power","Replace parts","Remove panels","Test the motor"]', 0),
(N'appliance', 2, 1, N'What is the purpose of a multimeter in appliance repair?', N'["Measure voltage and continuity","Lift heavy appliances","Clean internal parts","Seal refrigerant lines"]', 0),
(N'appliance', 3, 1, N'If a dryer does not heat, which component is commonly checked first?', N'["Heating element","Water valve","Door handle","Ice maker arm"]', 0),
(N'appliance', 4, 1, N'Why is it important to verify the model number before ordering parts?', N'["To match the correct replacement part","To increase water pressure","To reduce cleaning time","To bypass diagnostics"]', 0),
(N'appliance', 5, 2, N'A refrigerator is running but not cooling well. What should be checked first?', N'["Condenser coils","Cabinet color","Door logo","Control knob shape"]', 0),
(N'appliance', 6, 2, N'What does continuity testing help confirm?', N'["Whether a circuit path is complete","Whether the appliance is level","Whether the paint is dry","Whether the customer approved the repair"]', 0),
(N'appliance', 7, 2, N'If a washer does not drain, which part is commonly inspected?', N'["Drain pump","Bake element","Water filter pitcher","Light bulb socket"]', 0),
(N'appliance', 8, 2, N'Why should the correct replacement part number be used?', N'["To ensure proper fit and function","To reduce floor noise","To improve packaging","To speed up cleaning only"]', 0),
(N'appliance', 9, 3, N'What is a common sign of a faulty refrigerator door gasket?', N'["Warm air leaking around the door","The shelves are full","The cord is too long","The ice tray is blue"]', 0),
(N'appliance', 10, 3, N'When diagnosing an electric oven that will not heat, what is commonly checked?', N'["Bake element","Door paint","Handle screws only","Rack color"]', 0),
(N'appliance', 11, 3, N'What is the function of a thermal fuse in many appliances?', N'["Protect against overheating","Increase water pressure","Change motor speed automatically","Improve exterior appearance"]', 0),
(N'appliance', 12, 3, N'If a dishwasher is not filling with water, which item is often checked first?', N'["Water inlet valve","Dryer vent","Cabinet feet","Door sticker"]', 0),
(N'appliance', 13, 4, N'Why is it important to inspect appliance wiring for damage?', N'["To prevent shorts and unsafe operation","To improve shelf appearance","To increase detergent flow","To reduce packaging waste"]', 0),
(N'appliance', 14, 4, N'A microwave is completely dead. What is one basic item to verify first?', N'["Power supply to the unit","Color of the control panel","Shape of the glass tray","Position of the kitchen table"]', 0),
(N'appliance', 15, 4, N'What does a technician use a wiring diagram for?', N'["To trace circuits and components","To estimate appliance weight","To clean the appliance exterior","To advertise services"]', 0),
(N'appliance', 16, 4, N'If a dryer tumbles but has weak airflow, what is a common cause?', N'["Blocked venting","Loose cabinet paint","Wrong timer font","Cold water valve leak"]', 0),
(N'appliance', 17, 5, N'Why should moving parts be inspected for wear?', N'["To prevent failure and noise","To speed up packaging","To brighten the appliance finish","To increase room lighting"]', 0),
(N'appliance', 18, 5, N'What is a common first step when troubleshooting an appliance complaint?', N'["Verify the reported symptom","Order parts without testing","Replace the control board immediately","Ignore the customer description"]', 0),
(N'appliance', 19, 5, N'What customer service practice is most important after completing a repair?', N'["Explain the repair and test results","Hide replaced parts","Leave without speaking","Skip cleanup"]', 0),
(N'appliance', 20, 5, N'What should be done before closing the job?', N'["Confirm the appliance operates correctly","Remove the serial tag","Turn off the house water main","Reset unrelated appliances"]', 0);
GO

PRINT 'Appliance repair provider exam seeded.';
GO
