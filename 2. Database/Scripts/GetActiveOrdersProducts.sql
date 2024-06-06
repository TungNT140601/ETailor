SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetActiveOrdersProducts]
    @OrderIds NVARCHAR(MAX)
AS
BEGIN
    SELECT *
    FROM [Product]
    WHERE IsActive = 1 
    AND [Status] > 0 
    AND OrderId IN (SELECT value FROM STRING_SPLIT(@OrderIds, ','))
END;
GO
