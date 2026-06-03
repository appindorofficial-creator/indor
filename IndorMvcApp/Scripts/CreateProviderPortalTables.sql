/*
  INDOR Provider Portal — registration, catalog, trade exams, documents.
  Run on IndorDB (Azure or local). Safe to run multiple times.
*/

-- ---------- Catalog: provider trade categories ----------
IF OBJECT_ID(N'dbo.IndorProveedorCategoriasCatalogo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorCategoriasCatalogo (
        Id                NVARCHAR(40)  NOT NULL,
        LabelEn           NVARCHAR(120) NOT NULL,
        IconClass         NVARCHAR(60)  NOT NULL,
        SortOrder         INT           NOT NULL CONSTRAINT DF_IndorProvCatCat_Sort DEFAULT (0),
        RequiresTradeExam BIT           NOT NULL CONSTRAINT DF_IndorProvCatCat_Exam DEFAULT (0),
        Activo            BIT           NOT NULL CONSTRAINT DF_IndorProvCatCat_Activo DEFAULT (1),
        CONSTRAINT PK_IndorProveedorCategoriasCatalogo PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table IndorProveedorCategoriasCatalogo created.';
END
GO

-- ---------- Catalog: service offerings (installations, repairs, etc.) ----------
IF OBJECT_ID(N'dbo.IndorProveedorOfertasCatalogo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorOfertasCatalogo (
        Id        NVARCHAR(40)  NOT NULL,
        LabelEn   NVARCHAR(120) NOT NULL,
        IconClass NVARCHAR(60)  NOT NULL,
        SortOrder INT           NOT NULL CONSTRAINT DF_IndorProvOfCat_Sort DEFAULT (0),
        Activo    BIT           NOT NULL CONSTRAINT DF_IndorProvOfCat_Activo DEFAULT (1),
        CONSTRAINT PK_IndorProveedorOfertasCatalogo PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table IndorProveedorOfertasCatalogo created.';
END
GO

-- ---------- Main provider profile (portal registration) ----------
IF OBJECT_ID(N'dbo.IndorProveedores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedores (
        Id                      INT              IDENTITY(1,1) NOT NULL,
        UserId                  NVARCHAR(450)    NULL,
        RegistrationToken       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IndorProveedores_Token DEFAULT (NEWID()),
        RegistrationStatus      NVARCHAR(30)     NOT NULL CONSTRAINT DF_IndorProveedores_Status DEFAULT (N'Draft'),
        CurrentStep             INT              NOT NULL CONSTRAINT DF_IndorProveedores_Step DEFAULT (1),
        ProviderType            NVARCHAR(20)     NOT NULL CONSTRAINT DF_IndorProveedores_Type DEFAULT (N'Company'),
        BusinessName            NVARCHAR(200)    NULL,
        DbaName                 NVARCHAR(200)    NULL,
        PrimaryContact          NVARCHAR(120)    NULL,
        Phone                   NVARCHAR(30)     NULL,
        Email                   NVARCHAR(256)    NULL,
        YearsExperience         NVARCHAR(40)     NULL,
        LanguagesJson           NVARCHAR(200)    NULL,
        LicenseNumber           NVARCHAR(80)     NULL,
        PrimaryCity             NVARCHAR(120)    NULL,
        TravelRadiusMiles       INT              NOT NULL CONSTRAINT DF_IndorProveedores_Radius DEFAULT (25),
        ZipNeighborhoodsJson    NVARCHAR(500)    NULL,
        EmergencyService        BIT              NOT NULL CONSTRAINT DF_IndorProveedores_Emergency DEFAULT (1),
        SameDayJobs             BIT              NOT NULL CONSTRAINT DF_IndorProveedores_SameDay DEFAULT (1),
        AvailableDaysJson       NVARCHAR(80)     NULL,
        PreferredHours          NVARCHAR(60)     NULL,
        JobSizesJson            NVARCHAR(120)    NULL,
        LogoUploaded            BIT              NOT NULL CONSTRAINT DF_IndorProveedores_Logo DEFAULT (0),
        ScopeTradeUnderstood    BIT              NOT NULL CONSTRAINT DF_IndorProveedores_Scope1 DEFAULT (0),
        ScopeStandardsAgreed    BIT              NOT NULL CONSTRAINT DF_IndorProveedores_Scope2 DEFAULT (0),
        ExamScorePercent        INT              NULL,
        ExamPassed              BIT              NULL,
        ExamSubmittedUtc        DATETIME2(7)     NULL,
        ProfileSubmittedUtc     DATETIME2(7)     NULL,
        FechaCreacion           DATETIME2(7)     NOT NULL CONSTRAINT DF_IndorProveedores_Creado DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion      DATETIME2(7)     NULL,
        CONSTRAINT PK_IndorProveedores PRIMARY KEY CLUSTERED (Id)
    );

    CREATE UNIQUE INDEX UX_IndorProveedores_Token ON dbo.IndorProveedores (RegistrationToken);
    CREATE INDEX IX_IndorProveedores_UserId ON dbo.IndorProveedores (UserId);
    CREATE INDEX IX_IndorProveedores_Email ON dbo.IndorProveedores (Email);

    ALTER TABLE dbo.IndorProveedores WITH CHECK ADD CONSTRAINT FK_IndorProveedores_AspNetUsers
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id);
    PRINT 'Table IndorProveedores created.';
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorCategoriasSel', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorCategoriasSel (
        ProveedorId INT          NOT NULL,
        CategoriaId NVARCHAR(40) NOT NULL,
        CONSTRAINT PK_IndorProveedorCategoriasSel PRIMARY KEY (ProveedorId, CategoriaId),
        CONSTRAINT FK_IndorProvCatSel_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores (Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorProvCatSel_Catalogo FOREIGN KEY (CategoriaId) REFERENCES dbo.IndorProveedorCategoriasCatalogo (Id)
    );
    PRINT 'Table IndorProveedorCategoriasSel created.';
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorOfertasSel', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorOfertasSel (
        ProveedorId INT          NOT NULL,
        OfertaId    NVARCHAR(40) NOT NULL,
        CONSTRAINT PK_IndorProveedorOfertasSel PRIMARY KEY (ProveedorId, OfertaId),
        CONSTRAINT FK_IndorProvOfSel_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores (Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorProvOfSel_Catalogo FOREIGN KEY (OfertaId) REFERENCES dbo.IndorProveedorOfertasCatalogo (Id)
    );
    PRINT 'Table IndorProveedorOfertasSel created.';
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorExamPreguntas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorExamPreguntas (
        TradeCode       NVARCHAR(30)  NOT NULL,
        QuestionNumber  INT           NOT NULL,
        PageNumber      INT           NOT NULL,
        TextEn          NVARCHAR(500) NOT NULL,
        OptionsJson     NVARCHAR(MAX) NOT NULL,
        CorrectIndex    INT           NOT NULL,
        Activo          BIT           NOT NULL CONSTRAINT DF_IndorExamPreg_Activo DEFAULT (1),
        CONSTRAINT PK_IndorProveedorExamPreguntas PRIMARY KEY (TradeCode, QuestionNumber)
    );
    PRINT 'Table IndorProveedorExamPreguntas created.';
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorExamRespuestas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorExamRespuestas (
        Id               INT IDENTITY(1,1) NOT NULL,
        ProveedorId      INT NOT NULL,
        TradeCode        NVARCHAR(30) NOT NULL,
        QuestionNumber   INT NOT NULL,
        SelectedIndex    INT NOT NULL,
        IsCorrect        BIT NOT NULL,
        AnsweredUtc      DATETIME2(7) NOT NULL CONSTRAINT DF_IndorExamResp_Fecha DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProveedorExamRespuestas PRIMARY KEY (Id),
        CONSTRAINT FK_IndorExamResp_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores (Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX UX_IndorExamResp_ProveedorPregunta ON dbo.IndorProveedorExamRespuestas (ProveedorId, TradeCode, QuestionNumber);
    PRINT 'Table IndorProveedorExamRespuestas created.';
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorDocumentos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorDocumentos (
        Id            INT IDENTITY(1,1) NOT NULL,
        ProveedorId   INT NOT NULL,
        DocumentType  NVARCHAR(40) NOT NULL,
        Status        NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorProvDoc_Status DEFAULT (N'Required'),
        FileUrl       NVARCHAR(500) NULL,
        UploadedUtc   DATETIME2(7) NULL,
        CONSTRAINT PK_IndorProveedorDocumentos PRIMARY KEY (Id),
        CONSTRAINT FK_IndorProvDoc_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorProvDoc_Proveedor ON dbo.IndorProveedorDocumentos (ProveedorId);
    PRINT 'Table IndorProveedorDocumentos created.';
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorAlcanceReglas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorAlcanceReglas (
        Id          INT IDENTITY(1,1) NOT NULL,
        TradeCode   NVARCHAR(30) NOT NULL,
        LabelEn     NVARCHAR(120) NOT NULL,
        IsAllowed   BIT NOT NULL,
        SortOrder   INT NOT NULL CONSTRAINT DF_IndorAlcance_Sort DEFAULT (0),
        Activo      BIT NOT NULL CONSTRAINT DF_IndorAlcance_Activo DEFAULT (1),
        CONSTRAINT PK_IndorProveedorAlcanceReglas PRIMARY KEY (Id)
    );
    PRINT 'Table IndorProveedorAlcanceReglas created.';
END
GO

-- ---------- Seed catalogs (idempotent) ----------
MERGE dbo.IndorProveedorCategoriasCatalogo AS t
USING (VALUES
    (N'electrical', N'Electrical', N'fa-bolt', 1, 1),
    (N'plumbing', N'Plumbing', N'fa-faucet', 2, 0),
    (N'hvac', N'HVAC', N'fa-fan', 3, 0),
    (N'handyman', N'Handyman', N'fa-wrench', 4, 0),
    (N'construction', N'Construction Company', N'fa-hard-hat', 5, 0),
    (N'bathroom', N'Bathroom Remodeling', N'fa-bath', 6, 0),
    (N'kitchen', N'Kitchen Remodeling', N'fa-utensils', 7, 0),
    (N'roofing', N'Roofing', N'fa-house-chimney', 8, 0),
    (N'painting', N'Painting', N'fa-paint-roller', 9, 0),
    (N'flooring', N'Flooring', N'fa-border-all', 10, 0),
    (N'cleaning', N'Cleaning', N'fa-spray-can-sparkles', 11, 0),
    (N'landscaping', N'Landscaping', N'fa-leaf', 12, 0),
    (N'pest', N'Pest Control', N'fa-bug', 13, 0),
    (N'appliance', N'Appliance Repair', N'fa-plug-circle-bolt', 14, 0)
) AS s (Id, LabelEn, IconClass, SortOrder, RequiresTradeExam)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, RequiresTradeExam = s.RequiresTradeExam, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder, RequiresTradeExam) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder, s.RequiresTradeExam);
GO

MERGE dbo.IndorProveedorOfertasCatalogo AS t
USING (VALUES
    (N'installations', N'Installations', N'fa-plug', 1),
    (N'repairs', N'Repairs', N'fa-screwdriver-wrench', 2),
    (N'maintenance', N'Maintenance', N'fa-calendar-check', 3),
    (N'upgrades', N'Upgrades', N'fa-arrow-up', 4),
    (N'inspections', N'Inspections', N'fa-magnifying-glass', 5),
    (N'emergency', N'Emergency Services', N'fa-truck-medical', 6)
) AS s (Id, LabelEn, IconClass, SortOrder)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET LabelEn = s.LabelEn, IconClass = s.IconClass, SortOrder = s.SortOrder, Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, SortOrder) VALUES (s.Id, s.LabelEn, s.IconClass, s.SortOrder);
GO

-- Electrical exam questions (trade code: electrical)
DELETE FROM dbo.IndorProveedorExamPreguntas WHERE TradeCode = N'electrical';
INSERT INTO dbo.IndorProveedorExamPreguntas (TradeCode, QuestionNumber, PageNumber, TextEn, OptionsJson, CorrectIndex) VALUES
(N'electrical', 1, 1, N'Which breaker size should match the circuit load and wire rating?', N'["The smallest breaker available","The breaker one size larger than the wire","The breaker that matches the wire ampacity","The largest breaker available"]', 2),
(N'electrical', 2, 1, N'Before replacing a receptacle, what should be done first?', N'["Test for voltage","Remove the wall plate","Turn off the circuit","Disconnect the neutral wire"]', 2),
(N'electrical', 3, 1, N'What is the minimum working clearance in front of an electrical panel rated 600V or less?', N'["24 inches (600 mm)","36 inches (900 mm)","30 inches (750 mm)","42 inches (1050 mm)"]', 1),
(N'electrical', 4, 1, N'Which of the following is the correct order for lockout/tagout?', N'["Lockout, verify, tagout","Notify, lockout, verify, tagout","Tagout, lockout, verify","Lockout, tagout, verify"]', 1),
(N'electrical', 5, 2, N'Which wire type is commonly used for indoor branch circuit wiring in residential work?', N'["Bare copper grounding wire only","NM-B cable","Flexible extension cord","Low-voltage thermostat wire"]', 1),
(N'electrical', 6, 2, N'What device is required to protect receptacles in bathrooms?', N'["Standard breaker","GFCI protection","Surge protector","Fuse only"]', 1),
(N'electrical', 7, 2, N'What is the main purpose of bonding metal electrical parts?', N'["Increase voltage","Provide a safe fault path","Reduce wire size","Lower utility bills"]', 1),
(N'electrical', 8, 2, N'What should be verified after installing a new breaker?', N'["Paint color","Correct fit, rating, and operation","Panel label only","Only that the cover closes"]', 1),
(N'electrical', 9, 3, N'Which receptacle type is typically required in a garage?', N'["Ungrounded two-slot","GFCI-protected receptacle","240V dryer outlet","Telephone jack"]', 1),
(N'electrical', 10, 3, N'What must be done before working inside an energized panel?', N'["Nothing if the job is quick","Wear PPE and follow safe procedures","Stand on concrete barefoot","Use a plastic hammer"]', 1),
(N'electrical', 11, 3, N'What does AFCI protection help reduce?', N'["Water leaks","Arc-fault fire risk","Low water pressure","Paint damage"]', 1),
(N'electrical', 12, 3, N'What is the purpose of a service disconnect?', N'["Decorate the panel","Shut off power to the service","Boost current","Replace a transformer"]', 1),
(N'electrical', 13, 4, N'What is the safest way to confirm a circuit is de-energized?', N'["Touch the wire quickly","Ask someone else","Use an approved tester or meter","Look at the light switch"]', 2),
(N'electrical', 14, 4, N'Why are electrical panels required to be clearly labeled?', N'["For decoration","To identify circuits and improve safety","To increase voltage","To hide spare breakers"]', 1),
(N'electrical', 15, 4, N'Which area commonly requires weather-resistant devices outdoors?', N'["Attic only","Exterior receptacles","Closet shelving","Interior bedrooms"]', 1),
(N'electrical', 16, 4, N'What is one sign that a receptacle may need replacement?', N'["It holds plugs tightly","Cracks, heat, or loose connections","The wall is painted","The cover plate is white"]', 1),
(N'electrical', 17, 5, N'What should be checked before replacing a light fixture?', N'["Only the paint color","The homeowner''s furniture","Voltage, mounting support, and circuit condition","Internet speed"]', 2),
(N'electrical', 18, 5, N'What is the main function of a grounding conductor?', N'["Carry normal load current","Increase appliance speed","Provide a path for fault current","Reduce room lighting"]', 2),
(N'electrical', 19, 5, N'If an electrical box is overfilled, what is the risk?', N'["Better cooling","Faster installation","Loose or damaged conductors","Lower ampacity demand"]', 2),
(N'electrical', 20, 5, N'After finishing the exam, what unlocks job eligibility?', N'["Submitting any photo","Choosing multiple trades","Passing the exam and completing verification","Skipping the license upload"]', 2);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.IndorProveedorAlcanceReglas WHERE TradeCode = N'electrical')
BEGIN
    INSERT INTO dbo.IndorProveedorAlcanceReglas (TradeCode, LabelEn, IsAllowed, SortOrder) VALUES
    (N'electrical', N'Electrical repairs', 1, 1),
    (N'electrical', N'Panel work', 1, 2),
    (N'electrical', N'Outlets & switches', 1, 3),
    (N'electrical', N'Lighting installation', 1, 4),
    (N'electrical', N'Plumbing jobs', 0, 5),
    (N'electrical', N'HVAC jobs', 0, 6),
    (N'electrical', N'Roofing jobs', 0, 7),
    (N'electrical', N'General handyman jobs', 0, 8);
END
GO

-- Role for provider portal (AspNetRoles)
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = N'PROVEEDORSERVICIOS')
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), N'ProveedorServicios', N'PROVEEDORSERVICIOS', NEWID());
GO

PRINT 'CreateProviderPortalTables.sql completed.';
GO
