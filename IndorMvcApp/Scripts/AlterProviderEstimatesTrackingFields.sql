-- Estimate hub flow: Ready/Sent/Viewed/Approved tracking, pricing, timeline

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'FechaActualizacion') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD FechaActualizacion DATETIME2 NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'SubtotalAmount') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD SubtotalAmount DECIMAL(12,2) NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'TaxRate') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD TaxRate DECIMAL(5,4) NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'TaxAmount') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD TaxAmount DECIMAL(12,2) NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'DiscountAmount') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD DiscountAmount DECIMAL(12,2) NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'EstimatedStartDate') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD EstimatedStartDate DATE NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'EstimatedDuration') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD EstimatedDuration NVARCHAR(80) NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'LaborWarranty') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD LaborWarranty NVARCHAR(120) NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'PartsWarranty') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD PartsWarranty NVARCHAR(120) NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'ValidDays') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD ValidDays INT NOT NULL CONSTRAINT DF_IndorProvEst_ValidDays DEFAULT (30);



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'ViewedUtc') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD ViewedUtc DATETIME2 NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'ApprovedUtc') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD ApprovedUtc DATETIME2 NULL;



IF COL_LENGTH('dbo.IndorProveedorEstimates', 'ImageUrl') IS NULL

    ALTER TABLE dbo.IndorProveedorEstimates ADD ImageUrl NVARCHAR(500) NULL;



GO


