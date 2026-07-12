/*
  INDOR — Neighborhood feed (ZIP-scoped posts, comments, likes, saves).
  Backs the "INDOR Neighborhood" flow opened from the Nearby Network card.
  Safe to run multiple times (idempotent). Equivalent to EF migration
  20260712015345_AddNeighborhoodFeed for environments deployed via SQL.
*/

IF OBJECT_ID(N'dbo.IndorNeighborhoodPosts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNeighborhoodPosts (
        Id             INT            IDENTITY(1,1) NOT NULL,
        UserId         NVARCHAR(450)  NOT NULL,
        PropiedadId    INT            NULL,
        ZipCode        NVARCHAR(12)   NOT NULL,
        AuthorName     NVARCHAR(120)  NOT NULL,
        AuthorPhotoUrl NVARCHAR(500)  NULL,
        CategoryCode   NVARCHAR(40)   NULL,
        Title          NVARCHAR(200)  NULL,
        Body           NVARCHAR(2000) NOT NULL,
        ImagePath      NVARCHAR(500)  NULL,
        LocationLabel  NVARCHAR(200)  NULL,
        ProveedorId    INT            NULL,
        LikeCount      INT            NOT NULL CONSTRAINT DF_IndorNbhPost_Likes DEFAULT (0),
        CommentCount   INT            NOT NULL CONSTRAINT DF_IndorNbhPost_Comments DEFAULT (0),
        IsActive       BIT            NOT NULL CONSTRAINT DF_IndorNbhPost_Active DEFAULT (1),
        CreatedUtc     DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorNbhPost_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedUtc     DATETIME2(7)   NULL,
        CONSTRAINT PK_IndorNeighborhoodPosts PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNeighborhoodPosts_Propiedades_PropiedadId FOREIGN KEY (PropiedadId)
            REFERENCES dbo.Propiedades(Id) ON DELETE SET NULL
    );
    CREATE INDEX IX_IndorNeighborhoodPosts_ZipCode_IsActive_CreatedUtc
        ON dbo.IndorNeighborhoodPosts(ZipCode, IsActive, CreatedUtc);
    CREATE INDEX IX_IndorNeighborhoodPosts_PropiedadId
        ON dbo.IndorNeighborhoodPosts(PropiedadId);
    PRINT 'Table IndorNeighborhoodPosts created.';
END
GO

IF OBJECT_ID(N'dbo.IndorNeighborhoodComments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNeighborhoodComments (
        Id             INT            IDENTITY(1,1) NOT NULL,
        PostId         INT            NOT NULL,
        UserId         NVARCHAR(450)  NOT NULL,
        AuthorName     NVARCHAR(120)  NOT NULL,
        AuthorPhotoUrl NVARCHAR(500)  NULL,
        Body           NVARCHAR(1000) NOT NULL,
        IsActive       BIT            NOT NULL CONSTRAINT DF_IndorNbhComment_Active DEFAULT (1),
        CreatedUtc     DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorNbhComment_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorNeighborhoodComments PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNeighborhoodComments_IndorNeighborhoodPosts_PostId FOREIGN KEY (PostId)
            REFERENCES dbo.IndorNeighborhoodPosts(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorNeighborhoodComments_PostId_CreatedUtc
        ON dbo.IndorNeighborhoodComments(PostId, CreatedUtc);
    PRINT 'Table IndorNeighborhoodComments created.';
END
GO

IF OBJECT_ID(N'dbo.IndorNeighborhoodPostLikes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNeighborhoodPostLikes (
        Id         INT            IDENTITY(1,1) NOT NULL,
        PostId     INT            NOT NULL,
        UserId     NVARCHAR(450)  NOT NULL,
        CreatedUtc DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorNbhLike_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorNeighborhoodPostLikes PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNeighborhoodPostLikes_IndorNeighborhoodPosts_PostId FOREIGN KEY (PostId)
            REFERENCES dbo.IndorNeighborhoodPosts(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_IndorNeighborhoodPostLikes_PostId_UserId
        ON dbo.IndorNeighborhoodPostLikes(PostId, UserId);
    PRINT 'Table IndorNeighborhoodPostLikes created.';
END
GO

IF OBJECT_ID(N'dbo.IndorNeighborhoodPostSaves', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNeighborhoodPostSaves (
        Id         INT            IDENTITY(1,1) NOT NULL,
        PostId     INT            NOT NULL,
        UserId     NVARCHAR(450)  NOT NULL,
        CreatedUtc DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorNbhSave_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorNeighborhoodPostSaves PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNeighborhoodPostSaves_IndorNeighborhoodPosts_PostId FOREIGN KEY (PostId)
            REFERENCES dbo.IndorNeighborhoodPosts(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_IndorNeighborhoodPostSaves_PostId_UserId
        ON dbo.IndorNeighborhoodPostSaves(PostId, UserId);
    PRINT 'Table IndorNeighborhoodPostSaves created.';
END
GO

/* Media (photos/videos) attached to a post — supports galleries. */
IF OBJECT_ID(N'dbo.IndorNeighborhoodPostMedia', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNeighborhoodPostMedia (
        Id         INT            IDENTITY(1,1) NOT NULL,
        PostId     INT            NOT NULL,
        FilePath   NVARCHAR(500)  NOT NULL,
        MediaType  NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorNbhMedia_Type DEFAULT ('image'),
        SortOrder  INT            NOT NULL CONSTRAINT DF_IndorNbhMedia_Sort DEFAULT (0),
        CreatedUtc DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorNbhMedia_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorNeighborhoodPostMedia PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNeighborhoodPostMedia_IndorNeighborhoodPosts_PostId FOREIGN KEY (PostId)
            REFERENCES dbo.IndorNeighborhoodPosts(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorNeighborhoodPostMedia_PostId_SortOrder
        ON dbo.IndorNeighborhoodPostMedia(PostId, SortOrder);
    PRINT 'Table IndorNeighborhoodPostMedia created.';
END
GO

/* Individual comments saved as tips ("Mis Guardados" → Comentarios). */
IF OBJECT_ID(N'dbo.IndorNeighborhoodCommentSaves', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNeighborhoodCommentSaves (
        Id         INT            IDENTITY(1,1) NOT NULL,
        CommentId  INT            NOT NULL,
        UserId     NVARCHAR(450)  NOT NULL,
        CreatedUtc DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorNbhCmtSave_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorNeighborhoodCommentSaves PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNeighborhoodCommentSaves_IndorNeighborhoodComments_CommentId FOREIGN KEY (CommentId)
            REFERENCES dbo.IndorNeighborhoodComments(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_IndorNeighborhoodCommentSaves_CommentId_UserId
        ON dbo.IndorNeighborhoodCommentSaves(CommentId, UserId);
    PRINT 'Table IndorNeighborhoodCommentSaves created.';
END
GO

/* --- Post type, audience, and comment replies (added iteration) --- */
IF COL_LENGTH(N'dbo.IndorNeighborhoodPosts', N'PostType') IS NULL
    ALTER TABLE dbo.IndorNeighborhoodPosts ADD [PostType] NVARCHAR(30) NULL;
GO

IF COL_LENGTH(N'dbo.IndorNeighborhoodPosts', N'Audience') IS NULL
    ALTER TABLE dbo.IndorNeighborhoodPosts ADD [Audience] NVARCHAR(20) NOT NULL
        CONSTRAINT DF_IndorNbhPost_Audience DEFAULT ('Public');
GO

IF COL_LENGTH(N'dbo.IndorNeighborhoodComments', N'ParentCommentId') IS NULL
    ALTER TABLE dbo.IndorNeighborhoodComments ADD [ParentCommentId] INT NULL;
GO

IF COL_LENGTH(N'dbo.IndorNeighborhoodComments', N'ZipCode') IS NULL
    ALTER TABLE dbo.IndorNeighborhoodComments ADD [ZipCode] NVARCHAR(12) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_IndorNeighborhoodComments_ParentCommentId'
              AND object_id = OBJECT_ID(N'dbo.IndorNeighborhoodComments'))
    CREATE INDEX IX_IndorNeighborhoodComments_ParentCommentId
        ON dbo.IndorNeighborhoodComments(ParentCommentId);
GO
PRINT 'Neighborhood post type / audience / replies columns ready.';
GO
