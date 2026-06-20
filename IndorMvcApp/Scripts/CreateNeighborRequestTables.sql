/*
  INDOR — Neighbor requests for homeowner Nearby Network feed.
  Run on Azure after CreateNearbyNetworkTables.sql. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorNeighborRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNeighborRequests (
        Id           INT            IDENTITY(1,1) NOT NULL,
        PropiedadId  INT            NOT NULL,
        UserId       NVARCHAR(450)  NOT NULL,
        Title        NVARCHAR(200)  NOT NULL,
        Description  NVARCHAR(500)  NULL,
        Latitude     DECIMAL(9,6)   NULL,
        Longitude    DECIMAL(9,6)   NULL,
        IsActive     BIT            NOT NULL CONSTRAINT DF_IndorNeighborReq_Active DEFAULT (1),
        CreatedUtc   DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorNeighborReq_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorNeighborRequests PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNeighborReq_Propiedad FOREIGN KEY (PropiedadId) REFERENCES dbo.Propiedades(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorNeighborReq_Active ON dbo.IndorNeighborRequests(IsActive, CreatedUtc DESC);
    CREATE INDEX IX_IndorNeighborReq_Propiedad ON dbo.IndorNeighborRequests(PropiedadId);
    CREATE INDEX IX_IndorNeighborReq_User ON dbo.IndorNeighborRequests(UserId);
    PRINT 'Table IndorNeighborRequests created.';
END
GO
