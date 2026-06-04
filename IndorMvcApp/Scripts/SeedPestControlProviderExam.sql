/*
  Pest control provider exam and services catalog.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'pest';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'general_pest_control', N'General Pest Control', N'fa-bug', 130),
    (N'ant_control', N'Ant Control', N'fa-bug', 131),
    (N'roach_control', N'Roach Control', N'fa-bug', 132),
    (N'rodent_control', N'Rodent Control', N'fa-paw', 133),
    (N'termite_inspection', N'Termite Inspection', N'fa-magnifying-glass', 134),
    (N'termite_treatment', N'Termite Treatment', N'fa-shield-halved', 135),
    (N'mosquito_treatment', N'Mosquito Treatment', N'fa-mosquito', 136),
    (N'bed_bug_service', N'Bed Bug Service', N'fa-bed', 137),
    (N'wasp_bee_removal', N'Wasp / Bee Removal', N'fa-bee', 138),
    (N'preventive_maintenance', N'Preventive Maintenance', N'fa-calendar-check', 139)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'pest';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'pest', 1, 1, N'Before mixing or applying pest control chemicals, what should always be read first?', N'["The product label","The customer invoice","The weather app","The company logo"]', 0),
(N'pest', 2, 1, N'What is the safest first step before treating an interior area?', N'["Identify people, pets, and sensitive areas","Spray immediately","Close all vents permanently","Increase the dosage"]', 0),
(N'pest', 3, 1, N'Which item is basic personal protective equipment for many pest control tasks?', N'["Gloves","Sandals","Loose scarf","Earbuds"]', 0),
(N'pest', 4, 1, N'Why is correct pest identification important?', N'["To choose the right treatment method","To make the job longer","To avoid inspection notes","To skip documentation"]', 0),
(N'pest', 5, 2, N'What should be checked before applying an exterior treatment?', N'["Wind and weather conditions","The neighbor''s mailbox","The paint color","The office playlist"]', 0),
(N'pest', 6, 2, N'Why is measuring chemical correctly important?', N'["To follow label rates safely","To make the smell stronger","To finish the bottle faster","To avoid wearing gloves"]', 0),
(N'pest', 7, 2, N'What is a common sign of a termite problem?', N'["Mud tubes","Bright wall paint","Cold air from vents","Loose doorknob"]', 0),
(N'pest', 8, 2, N'What is the best first step when a customer reports ants in the kitchen?', N'["Inspect and identify the source","Spray every room immediately","Ignore entry points","Turn off the refrigerator"]', 0),
(N'pest', 9, 3, N'Which pest is commonly associated with droppings and gnaw marks?', N'["Rodents","Butterflies","Ladybugs","Dragonflies"]', 0),
(N'pest', 10, 3, N'When treating near food-prep areas, what is most important?', N'["Protect food-contact surfaces","Use extra product","Leave containers open","Skip cleanup"]', 0),
(N'pest', 11, 3, N'What is the purpose of a follow-up visit in pest control?', N'["Check treatment results and activity","Raise the invoice only","Change the company logo","Refill every chemical bottle"]', 0),
(N'pest', 12, 3, N'What should be done with unused mixed chemical when the label gives disposal instructions?', N'["Follow the label disposal instructions","Pour it in any drain","Store it in a soda bottle","Leave it in the truck bed"]', 0),
(N'pest', 13, 4, N'What is one reason to seal or note entry points during service?', N'["To help reduce future pest entry","To increase chemical use","To make walls look darker","To avoid inspections"]', 0),
(N'pest', 14, 4, N'Which tool is commonly used to inspect hard-to-see pest areas?', N'["Flashlight","Television remote","Hair dryer","Paint roller"]', 0),
(N'pest', 15, 4, N'Why should treatment records be completed clearly?', N'["To document work and support safe service","To avoid wearing PPE","To replace the label","To shorten the inspection"]', 0),
(N'pest', 16, 4, N'If a customer has pets, what should the technician do?', N'["Explain safety instructions and reentry guidance","Double the product amount","Ignore the pets","Block the exits"]', 0),
(N'pest', 17, 5, N'What is the best reason to identify the type of cockroach before treatment?', N'["Different species may require different strategies","All roaches live the same way","It changes the company name","It removes the need to inspect"]', 0),
(N'pest', 18, 5, N'What should be done before using application equipment?', N'["Inspect it for leaks or damage","Leave it untested","Remove all labels","Overfill it past the line"]', 0),
(N'pest', 19, 5, N'What is one key part of good customer communication after service?', N'["Explain what was found and next steps","Say nothing and leave","Hide service notes","Promise impossible results"]', 0),
(N'pest', 20, 5, N'What does passing the INDOR pest control exam allow the provider to unlock?', N'["Pest control jobs only","All provider categories","Banking access","Unlimited free ads"]', 0);
GO

PRINT 'Pest control provider exam seeded.';
GO
