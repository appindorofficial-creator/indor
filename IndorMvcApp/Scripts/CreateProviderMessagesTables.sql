/*
  INDOR PRO — provider-customer messaging.
  Run after CreateProviderOperationsTables.sql.
*/

IF OBJECT_ID(N'dbo.IndorProveedorConversations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorConversations (
        Id                  INT            IDENTITY(1,1) NOT NULL,
        ProveedorId         INT            NOT NULL,
        ClienteId           INT            NULL,
        JobId               INT            NULL,
        LeadId              INT            NULL,
        Category            NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorProvConv_Category DEFAULT (N'Job'),
        Status              NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorProvConv_Status DEFAULT (N'New'),
        UnreadCount         INT            NOT NULL CONSTRAINT DF_IndorProvConv_Unread DEFAULT (0),
        LastMessagePreview  NVARCHAR(250)  NULL,
        LastMessageAt       DATETIME2      NOT NULL CONSTRAINT DF_IndorProvConv_LastMsg DEFAULT (SYSUTCDATETIME()),
        IsCustomerOnline    BIT            NOT NULL CONSTRAINT DF_IndorProvConv_Online DEFAULT (0),
        FechaCreacion       DATETIME2      NOT NULL CONSTRAINT DF_IndorProvConv_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProveedorConversations PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorProvConv_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorProvConv_Cliente FOREIGN KEY (ClienteId) REFERENCES dbo.IndorProveedorClientes(Id),
        CONSTRAINT FK_IndorProvConv_Job FOREIGN KEY (JobId) REFERENCES dbo.IndorProveedorJobs(Id),
        CONSTRAINT FK_IndorProvConv_Lead FOREIGN KEY (LeadId) REFERENCES dbo.IndorProveedorLeads(Id)
    );
    CREATE INDEX IX_IndorProvConv_Proveedor ON dbo.IndorProveedorConversations(ProveedorId, LastMessageAt DESC);
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorMessages (
        Id               INT            IDENTITY(1,1) NOT NULL,
        ConversationId   INT            NOT NULL,
        SenderType       NVARCHAR(20)   NOT NULL,
        Body             NVARCHAR(2000) NOT NULL,
        SentAt           DATETIME2      NOT NULL CONSTRAINT DF_IndorProvMsg_Sent DEFAULT (SYSUTCDATETIME()),
        IsRead           BIT            NOT NULL CONSTRAINT DF_IndorProvMsg_Read DEFAULT (0),
        AttachmentType   NVARCHAR(40)   NULL,
        AttachmentLabel  NVARCHAR(120)  NULL,
        CONSTRAINT PK_IndorProveedorMessages PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorProvMsg_Conversation FOREIGN KEY (ConversationId) REFERENCES dbo.IndorProveedorConversations(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorProvMsg_Conversation ON dbo.IndorProveedorMessages(ConversationId, SentAt);
END
GO
