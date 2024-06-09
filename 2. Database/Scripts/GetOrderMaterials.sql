SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetOrderMaterials]
    @OrderId NVARCHAR(30)
AS
BEGIN
SELECT 
[OM].*
FROM OrderMaterial OM INNER JOIN (
    SELECT Id
    FROM [Order]
    WHERE Id = @OrderId AND [Status] > 0
) AS O ON OM.OrderId = O.Id
WHERE IsActive = 1
END;
GO
