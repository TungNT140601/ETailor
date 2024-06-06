SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetTotalFabricMaterialCommonUsed]
    @StartDate DATETIME
AS
BEGIN
    SELECT M.Id,M.Name,M.[Image],PO.TotalProducts, 0 AS TotalOrders, M.Quantity
    FROM Material M LEFT JOIN (
        SELECT P.FabricMaterialId, COUNT(P.Id) AS TotalProducts
        FROM Product P INNER JOIN (
            SELECT Id
            FROM [Order]
            WHERE [Status] = 8
            AND IsActive = 1
            AND MONTH(CreatedTime) = MONTH(@StartDate)
            AND YEAR(CreatedTime) = YEAR(@StartDate)
        ) AS O ON P.OrderId = O.Id
        WHERE P.[Status] > 0
        AND P.IsActive = 1
        GROUP BY P.FabricMaterialId
    ) AS PO ON M.Id = PO.FabricMaterialId
    WHERE M.IsActive = 1
END;
GO
