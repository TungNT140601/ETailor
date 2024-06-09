SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetOrderMaterialsCategoryData]
    @OrderId NVARCHAR(30)
AS
BEGIN
SELECT 
[MC].*
FROM MaterialCategory MC INNER JOIN (
    SELECT
    [M].[MaterialCategoryId]
    FROM Material M INNER JOIN (
        SELECT 
        [OM].[MaterialId]
        FROM OrderMaterial OM INNER JOIN (
            SELECT Id
            FROM [Order]
            WHERE Id = @OrderId AND [Status] > 0
        ) AS O ON OM.OrderId = O.Id
        WHERE IsActive = 1
    ) OM ON M.Id = OM.MaterialId
) AS M ON MC.Id = M.MaterialCategoryId
END;
GO
