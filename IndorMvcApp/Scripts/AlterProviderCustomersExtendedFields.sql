/*
  Extended fields for INDOR PRO Customers cards.
  Run after CreateProviderOperationsTables.sql.
*/

IF COL_LENGTH('dbo.IndorProveedorClientes', 'Address') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD Address NVARCHAR(250) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorClientes', 'ConnectionStatus') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD ConnectionStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorProvCli_Conn DEFAULT (N'Connected');
GO

IF COL_LENGTH('dbo.IndorProveedorClientes', 'IsPropertyVerified') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD IsPropertyVerified BIT NOT NULL CONSTRAINT DF_IndorProvCli_Verified DEFAULT (0);
GO

IF COL_LENGTH('dbo.IndorProveedorClientes', 'IsAppConnected') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD IsAppConnected BIT NOT NULL CONSTRAINT DF_IndorProvCli_AppConn DEFAULT (0);
GO

IF COL_LENGTH('dbo.IndorProveedorClientes', 'PropertiesCount') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD PropertiesCount INT NOT NULL CONSTRAINT DF_IndorProvCli_Props DEFAULT (1);
GO

IF COL_LENGTH('dbo.IndorProveedorClientes', 'HouseFactsCount') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD HouseFactsCount INT NOT NULL CONSTRAINT DF_IndorProvCli_HouseFacts DEFAULT (0);
GO

IF COL_LENGTH('dbo.IndorProveedorClientes', 'LastActivityNote') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD LastActivityNote NVARCHAR(200) NULL;
GO
