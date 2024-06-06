SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetOrderChat]
    @OrderId NVARCHAR(30)
AS

BEGIN
    SELECT TOP 1 
    [C].*
    FROM Chat C INNER JOIN [Order] O ON C.OrderId = O.Id
    WHERE O.Id = @OrderId
    AND O.[Status] >= 0
    AND O.[Status] <= 8
    AND O.IsActive = 1
    AND C.IsActive = 1
END;
GO
