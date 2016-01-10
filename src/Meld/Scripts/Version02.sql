EXEC sp_rename '[dbo].[Version]', 'Versions'
GO

EXEC sp_rename '[dbo].[Versions].[Type]', 'Database', 'COLUMN'
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
