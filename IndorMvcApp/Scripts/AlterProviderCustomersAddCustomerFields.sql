-- Add Customer wizard fields

IF COL_LENGTH('dbo.IndorProveedorClientes', 'CustomerCode') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD CustomerCode NVARCHAR(30) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'CustomerType') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD CustomerType NVARCHAR(30) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'FirstName') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD FirstName NVARCHAR(60) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'LastName') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD LastName NVARCHAR(60) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'PreferredContactMethod') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD PreferredContactMethod NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'CompanyName') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD CompanyName NVARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'PhotoUrl') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD PhotoUrl NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'StreetAddress') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD StreetAddress NVARCHAR(200) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'AptUnit') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD AptUnit NVARCHAR(40) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'City') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD City NVARCHAR(80) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'State') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD State NVARCHAR(10) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'ZipCode') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD ZipCode NVARCHAR(15) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'PropertyType') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD PropertyType NVARCHAR(30) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'Bedrooms') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD Bedrooms INT NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'Bathrooms') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD Bathrooms DECIMAL(3,1) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'IsBillingAddressSame') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD IsBillingAddressSame BIT NOT NULL CONSTRAINT DF_IndorProvCli_BillingSame DEFAULT (1);
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'PropertyPhotoUrl') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD PropertyPhotoUrl NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'AccessNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD AccessNotes NVARCHAR(250) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'EstimateDeliveryPref') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD EstimateDeliveryPref NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'InvoiceDeliveryPref') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD InvoiceDeliveryPref NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'PreferredLanguage') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD PreferredLanguage NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'CustomerSource') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD CustomerSource NVARCHAR(30) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'TagsJson') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD TagsJson NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'InternalNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD InternalNotes NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'SendIndorInvite') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD SendIndorInvite BIT NOT NULL CONSTRAINT DF_IndorProvCli_SendInvite DEFAULT (1);
GO
IF COL_LENGTH('dbo.IndorProveedorClientes', 'AllowServiceUpdates') IS NULL
    ALTER TABLE dbo.IndorProveedorClientes ADD AllowServiceUpdates BIT NOT NULL CONSTRAINT DF_IndorProvCli_AllowUpdates DEFAULT (1);
GO
