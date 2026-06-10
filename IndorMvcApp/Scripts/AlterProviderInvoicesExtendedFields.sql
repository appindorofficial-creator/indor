-- Payments & Invoices flow: invoice details, line items, reminders

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'InvoiceCode') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD InvoiceCode NVARCHAR(20) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'Address') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD Address NVARCHAR(300) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'ServiceType') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD ServiceType NVARCHAR(120) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'CustomerName') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD CustomerName NVARCHAR(200) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'CustomerEmail') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD CustomerEmail NVARCHAR(256) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'CustomerPhone') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD CustomerPhone NVARCHAR(40) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'InvoiceDate') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD InvoiceDate DATE NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'PaymentMethod') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD PaymentMethod NVARCHAR(40) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'NotesToCustomer') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD NotesToCustomer NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'LineItemsJson') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD LineItemsJson NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'LastReminderUtc') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD LastReminderUtc DATETIME2 NULL;

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'FechaActualizacion') IS NULL
    ALTER TABLE dbo.IndorProveedorInvoices ADD FechaActualizacion DATETIME2 NULL;

GO
