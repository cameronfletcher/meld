BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
SET XACT_ABORT ON;

CREATE TABLE [dbo].[Versions_temp]
(
    [Version] INT NOT NULL , 
    [Database] VARCHAR(511) NOT NULL, 
    [Schema] VARCHAR(128) NOT NULL, 
    [Description] VARCHAR(MAX) NOT NULL, 
    [Sequence] INT IDENTITY (1, 1) NOT NULL, 
    [Script] VARCHAR(MAX) NULL,
    CONSTRAINT [PK_Versions_temp] PRIMARY KEY CLUSTERED ([Version], [Database], [Schema])
);

IF EXISTS (SELECT TOP 1 1 FROM [dbo].[Versions])
BEGIN
    SET IDENTITY_INSERT [dbo].[Versions_temp] ON;
    INSERT INTO [dbo].[Versions_temp]
    (
        [Version],
        [Database],
        [Schema],
        [Description],
        [Sequence]
    )
    SELECT
        [Number].[number] AS [Version],
        [Version].[Database],
        [Version].[Schema],
        ISNULL([Description].[Description], '[migration]') AS [Description],
        ROW_NUMBER() OVER (ORDER BY [Version].[Database], [Version].[Schema], [Number].[number]) AS [Sequence]
    FROM [database].[Versions] [Version]
        CROSS JOIN [master]..[spt_values] [Number]
        LEFT JOIN (
        SELECT
            [Version],
            [Database],
            [Schema],
            [Description]
        FROM [database].[Versions]) [Description] ON [Description].[Version] = [Number].[number]
            AND [Description].[Database] = [Version].[Database]
            AND [Description].[Schema] = [Version].[Schema]
    WHERE [Number].[type] = 'P'
        AND [Number].[number] BETWEEN 1 AND [Version].[Version]
    ORDER BY [Version].[Database], [Version].[Schema], [Number].[number]
    SET IDENTITY_INSERT [dbo].[Versions_temp] OFF;
END

DROP TABLE [dbo].[Versions];

EXECUTE sp_rename N'[dbo].[Versions_temp]', N'Versions';
EXECUTE sp_rename N'[dbo].[PK_Versions_temp]', N'PK_Versions', N'OBJECT';

COMMIT TRANSACTION;
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
GO

ALTER PROCEDURE [dbo].[GetVersion]
    @Database VARCHAR(511),
    @Schema VARCHAR(128)
AS
SET NOCOUNT ON;
SELECT MAX([Version]) AS [Version]
FROM [dbo].[Versions]
WHERE [Schema] = @Schema AND [Database] = @Database;
GO

ALTER PROCEDURE [dbo].[SetVersion]
    @Database VARCHAR(511),
    @Schema VARCHAR(128),
    @Version INT,
    @Description VARCHAR(MAX)
AS
SET NOCOUNT ON;
INSERT INTO [dbo].[Versions]
VALUES (@Version, @Database, @Schema, @Description);
GO
