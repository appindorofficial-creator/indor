-- Create Estimate wizard: title, description, priority, source type, delivery

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'Title') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD Title NVARCHAR(150) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorEstimates', 'Description') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD Description NVARCHAR(2000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorEstimates', 'Priority') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD Priority NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorEstimates', 'EstimateType') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD EstimateType NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorEstimates', 'ServiceCategoryId') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD ServiceCategoryId NVARCHAR(40) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorEstimates', 'ClienteId') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD ClienteId INT NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorEstimates', 'DeliveryMethod') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD DeliveryMethod NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorEstimates', 'EstimatedEndDate') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD EstimatedEndDate DATE NULL;
GO
