SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create the stored procedure in the specified schema
ALTER PROCEDURE [dbo].[GetApproveOrdersProducts]
    @OrderIds NVARCHAR(MAX)
AS
BEGIN
    SELECT P.*
    FROM Product P
    LEFT JOIN ProductStage PS ON PS.ProductId = P.Id
    WHERE P.OrderId IN (SELECT value FROM STRING_SPLIT(@OrderIds, ',')) 
    AND PS.Id IS NULL
END
GO
