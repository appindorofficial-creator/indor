-- Pending invoices flow: record payment, customer notes, property meta

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'PaidAmount') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD PaidAmount DECIMAL(12,2) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'PaymentReference') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD PaymentReference NVARCHAR(80) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'InternalNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD InternalNotes NVARCHAR(500) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'CustomerNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD CustomerNotes NVARCHAR(2000) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'PropertyType') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD PropertyType NVARCHAR(40) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'LastReminderChannel') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD LastReminderChannel NVARCHAR(20) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'LastReminderMessage') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD LastReminderMessage NVARCHAR(MAX) NULL;

GO
