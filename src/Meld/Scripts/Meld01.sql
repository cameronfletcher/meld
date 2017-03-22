CREATE TABLE [dbo].[Version]
(
	[Version] INT NOT NULL , 
    [Type] VARCHAR(511) NOT NULL, 
    [Schema] VARCHAR(128) NOT NULL, 
    [Description] VARCHAR(MAX) NOT NULL, 
    PRIMARY KEY ([Version], [Type], [Schema])
);
GO

CREATE PROCEDURE [dbo].[GetVersion]
    @Schema VARCHAR(128),
    @Type VARCHAR(511)
AS
SET NOCOUNT ON;
SELECT ISNULL(MAX([Version]), 0) AS [Version]
FROM [dbo].[Version]
WHERE [Schema] = @Schema AND [Type] = @Type;
GO

CREATE PROCEDURE [dbo].[SetVersion]
    @Schema VARCHAR(128),
    @Type VARCHAR(511),
	@Version INT,
    @Description VARCHAR(MAX)
AS
SET NOCOUNT ON;
INSERT INTO [dbo].[Version]
VALUES (@Version, @Schema, @Type, @Description);
GO
