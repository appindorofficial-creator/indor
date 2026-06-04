/*
  Landscaping provider exam and services catalog.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'landscaping';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'lawn_mowing', N'Lawn mowing', N'fa-seedling', 120),
    (N'lawn_maintenance', N'Lawn maintenance', N'fa-leaf', 121),
    (N'mulching', N'Mulching', N'fa-tree', 122),
    (N'planting', N'Planting', N'fa-seedling', 123),
    (N'tree_trimming', N'Tree trimming', N'fa-tree', 124),
    (N'hedge_trimming', N'Hedge trimming', N'fa-scissors', 125),
    (N'sod_installation', N'Sod installation', N'fa-layer-group', 126),
    (N'irrigation', N'Irrigation', N'fa-droplet', 127),
    (N'seasonal_cleanup', N'Seasonal cleanup', N'fa-broom', 128),
    (N'hardscape_maintenance', N'Hardscape maintenance', N'fa-border-all', 129)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'landscaping';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'landscaping', 1, 1, N'Before using a string trimmer near people, what should be worn first?', N'["Safety glasses and protective gear","Flip-flops","Loose jewelry","No protection is needed"]', 0),
(N'landscaping', 2, 1, N'What is the best first step before mowing a new property?', N'["Walk the site and remove debris","Start mowing immediately","Water the lawn heavily","Trim shrubs first"]', 0),
(N'landscaping', 3, 1, N'Why should grass not be cut too short?', N'["It stresses the turf and weakens roots","It grows faster overnight","It makes mulch unnecessary","It prevents irrigation"]', 0),
(N'landscaping', 4, 1, N'What should be checked before starting a mower?', N'["Fuel, oil, and blade condition","House paint color","Customer Wi-Fi password","Fence stain color"]', 0),
(N'landscaping', 5, 2, N'What is the best time to water most lawns?', N'["Early morning","At noon","Late night","Only after rain"]', 0),
(N'landscaping', 6, 2, N'What is a common sign that irrigation needs adjustment?', N'["Dry spots or runoff","Grass is green","Soil is cool","Wind is low"]', 0),
(N'landscaping', 7, 2, N'Why is spacing important when planting shrubs?', N'["Allows mature growth and airflow","Makes watering impossible","Forces roots upward","Prevents mulch use"]', 0),
(N'landscaping', 8, 2, N'What is a key benefit of mulch?', N'["Helps retain moisture and reduce weeds","Eliminates all watering","Replaces soil","Makes pruning unnecessary"]', 0),
(N'landscaping', 9, 3, N'What is the main purpose of edging?', N'["Creates clean separation lines","Makes grass grow taller","Removes irrigation","Fertilizes soil"]', 0),
(N'landscaping', 10, 3, N'What should be done before applying fertilizer?', N'["Identify turf needs and follow label directions","Apply the maximum amount","Skip the weather check","Water flowers with fuel"]', 0),
(N'landscaping', 11, 3, N'Which tool is best for pruning small shrubs?', N'["Hand pruners","Sledgehammer","Chainsaw for all cuts","Leaf blower"]', 0),
(N'landscaping', 12, 3, N'Why is grading important around a home?', N'["Directs water away from the structure","Traps water near the foundation","Increases roof load","Changes the paint color"]', 0),
(N'landscaping', 13, 4, N'Before repairing a sprinkler head, what should be done first?', N'["Shut off the irrigation zone or water","Add more pressure","Cut the grass first","Ignore the leak"]', 0),
(N'landscaping', 14, 4, N'What is a common sign of overwatering?', N'["Mushy soil and yellowing or fungus","Dry, cracking soil","Strong roots only","Cleaner sidewalks"]', 0),
(N'landscaping', 15, 4, N'Why are base layers important in hardscape work?', N'["They provide stability and drainage","They make plants taller","They replace edging","They eliminate compaction"]', 0),
(N'landscaping', 16, 4, N'What should a provider do if a task is outside their skill or scope?', N'["Inform the customer and decline or refer appropriately","Do it anyway","Hide the issue","Bill extra without explanation"]', 0),
(N'landscaping', 17, 5, N'What is the best way to avoid damage when using a blower near flower beds?', N'["Use low speed and keep the blower away from beds","Use maximum power toward flowers","Aim directly at mulch only","Skip clearing debris"]', 0),
(N'landscaping', 18, 5, N'What is the safest way to lift heavy bags of soil or mulch?', N'["Bend with legs and lift carefully or use a team","Lift with your back only","Throw bags from the truck","Drag bags without lifting"]', 0),
(N'landscaping', 19, 5, N'What is a best customer practice after the job is completed?', N'["Walk the site with the customer and confirm satisfaction","Leave without checking the work","Bill before cleanup","Skip final communication"]', 0),
(N'landscaping', 20, 5, N'If herbicide is used, what is most important?', N'["Follow label directions and use proper PPE","Apply extra for faster results","Mix with any other chemical","Skip protecting nearby plants"]', 0);
GO

PRINT 'Landscaping provider exam seeded.';
GO
