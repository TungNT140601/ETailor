SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetGreatestIndexValue]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ReturnValue INT;

    SELECT TOP 1 @ReturnValue = [Index]
    FROM Product
    WHERE IsActive = 1 AND [Status] > 0
    ORDER BY [Index] DESC

    IF @ReturnValue IS NULL
    SET @ReturnValue = 0;

    SELECT @ReturnValue AS ReturnValue;
END;
GO
