/*
  HVAC provider exam, services catalog, and profile columns.
  Run after CreateProviderPortalTables.sql
*/
SET NOCOUNT ON;

IF COL_LENGTH(N'dbo.IndorProveedores', N'EpaCertificationNumber') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD EpaCertificationNumber NVARCHAR(80) NULL;
    PRINT 'Column IndorProveedores.EpaCertificationNumber added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'BackgroundCheckConsent') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD BackgroundCheckConsent BIT NOT NULL CONSTRAINT DF_IndorProveedores_BgCheck DEFAULT (0);
    PRINT 'Column IndorProveedores.BackgroundCheckConsent added.';
END
GO

UPDATE dbo.IndorProveedorCategoriasCatalogo
SET RequiresTradeExam = 1
WHERE Id = N'hvac';
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'ac_repair', N'AC Repair', N'fa-snowflake', 20),
    (N'ac_install', N'AC Installation', N'fa-fan', 21),
    (N'heating_repair', N'Heating Repair', N'fa-fire-flame-simple', 22),
    (N'heat_pump', N'Heat Pump Service', N'fa-wind', 23),
    (N'ductwork', N'Ductwork', N'fa-layer-group', 24),
    (N'thermostat', N'Thermostat', N'fa-temperature-half', 25),
    (N'indoor_air_quality', N'Indoor Air Quality', N'fa-wind', 26),
    (N'preventive_maintenance', N'Preventive Maintenance', N'fa-clipboard-check', 27),
    (N'mini_split', N'Mini-Split', N'fa-border-all', 28),
    (N'commercial_hvac', N'Commercial HVAC', N'fa-building', 29)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'hvac';

INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'hvac', 1, 1, N'Before servicing an HVAC unit, what should be done first?', N'["Disconnect power and verify it is off","Open the refrigerant valves","Replace the thermostat","Increase fan speed"]', 0),
(N'hvac', 2, 1, N'What is the main purpose of an air filter?', N'["Protect equipment and improve airflow quality","Raise refrigerant pressure","Increase compressor voltage","Lubricate the blower motor"]', 0),
(N'hvac', 3, 1, N'Which tool is commonly used to check voltage?', N'["Multimeter","Pipe cutter","Manifold hose only","Thermometer"]', 0),
(N'hvac', 4, 1, N'If the thermostat screen is blank, what should be checked first?', N'["Power supply or batteries","Compressor oil","Condenser coil color","Duct insulation"]', 0),
(N'hvac', 5, 2, N'A dirty evaporator coil most commonly causes?', N'["Reduced cooling performance","Higher thermostat accuracy","Increased refrigerant recovery","Stronger breaker protection"]', 0),
(N'hvac', 6, 2, N'Low airflow across the evaporator can lead to?', N'["Coil icing","Higher gas pressure only","Larger duct size","Automatic refrigerant refill"]', 0),
(N'hvac', 7, 2, N'A common cause of short cycling is?', N'["Dirty filter or thermostat issue","Fresh paint on walls","Oversized shoes on the installer","Low ladder height"]', 0),
(N'hvac', 8, 2, N'When some rooms are much warmer than others, what should be checked?', N'["Airflow, ducts, and dampers","Roof color only","Water heater settings","Exterior brick pattern"]', 0),
(N'hvac', 9, 3, N'Before adding refrigerant, what should a technician do first?', N'["Verify charge and check for leaks","Open the condensate drain","Replace the thermostat immediately","Shut off the furnace gas valve"]', 0),
(N'hvac', 10, 3, N'Symptoms such as low pressure and poor cooling may suggest?', N'["Possible low refrigerant charge","Perfect system balance","Too much duct insulation","An upgraded air filter only"]', 0),
(N'hvac', 11, 3, N'Why are superheat and subcooling checked?', N'["To verify system charge and performance","To estimate property taxes","To repaint the condenser","To reset Wi-Fi"]', 0),
(N'hvac', 12, 3, N'Refrigerant must be recovered using?', N'["Approved recovery equipment","A garden hose","An open bucket","A vacuum cleaner"]', 0),
(N'hvac', 13, 4, N'A failed capacitor may cause?', N'["Motor not starting properly","More refrigerant in the lines","Higher home resale value","A larger air filter"]', 0),
(N'hvac', 14, 4, N'What component protects the compressor from high pressure?', N'["High-pressure switch","Drain pan only","Thermostat cover","Blower wheel"]', 0),
(N'hvac', 15, 4, N'If the condenser fan is not running, what should be checked first?', N'["Power, contactor, and capacitor","Grass height outside","Kitchen sink pressure","Window blinds"]', 0),
(N'hvac', 16, 4, N'What is the purpose of a contactor?', N'["It switches high-voltage power using a control signal","It cleans the evaporator coil","It stores condensate water","It measures duct insulation"]', 0),
(N'hvac', 17, 5, N'What should be documented after service is completed?', N'["Work performed, readings, and recommendations","Only the customer''s ZIP code","Paint color of the unit","Nothing if the job was simple"]', 0),
(N'hvac', 18, 5, N'When should a condensate drain issue be addressed?', N'["Immediately to avoid water damage","Next year during repainting","Only in winter","After replacing the roof"]', 0),
(N'hvac', 19, 5, N'If a major safety concern is found, what is the best action?', N'["Inform the customer clearly and make the system safe","Ignore it and leave","Raise the refrigerant charge","Change the thermostat color"]', 0),
(N'hvac', 20, 5, N'What helps extend HVAC system life?', N'["Regular maintenance and filter changes","Closing every vent","Running the system without a filter","Skipping inspections"]', 0);
GO

PRINT 'HVAC provider exam seeded.';
GO
