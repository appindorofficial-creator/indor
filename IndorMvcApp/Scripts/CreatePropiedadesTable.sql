-- =============================================================
-- Script para crear la tabla Propiedades
-- Coincide con el modelo IndorMvcApp.Models.Propiedad y la
-- configuración de AppDbContext.
-- Base de datos: SQL Server / LocalDB
-- =============================================================

IF OBJECT_ID(N'[dbo].[Propiedades]', N'U') IS NOT NULL
BEGIN
	PRINT 'La tabla [dbo].[Propiedades] ya existe. No se realizan cambios.';
	RETURN;
END

CREATE TABLE [dbo].[Propiedades]
(
	[Id]             INT              IDENTITY(1,1) NOT NULL,
	[Direccion]      NVARCHAR(500)    NULL,
	[DatosJson]      NVARCHAR(MAX)    NOT NULL,
	[FechaCreacion]  DATETIME2(7)     NOT NULL CONSTRAINT [DF_Propiedades_FechaCreacion] DEFAULT (SYSDATETIME()),
	[Activo]         BIT              NOT NULL CONSTRAINT [DF_Propiedades_Activo]        DEFAULT (1),
	[UserId]         NVARCHAR(450)    NOT NULL,

	CONSTRAINT [PK_Propiedades] PRIMARY KEY CLUSTERED ([Id] ASC),

	CONSTRAINT [FK_Propiedades_AspNetUsers_UserId]
		FOREIGN KEY ([UserId])
		REFERENCES [dbo].[AspNetUsers] ([Id])
		ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Propiedades_UserId] ON [dbo].[Propiedades] ([UserId]);
GO

PRINT 'Tabla [dbo].[Propiedades] creada correctamente.';
GO
