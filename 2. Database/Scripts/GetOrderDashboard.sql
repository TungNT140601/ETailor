SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetOrderDashboard]
    @StartDate DATETIME
AS
BEGIN
Select 
    [Status],
    COUNT(Id) As Total,
    SUM(CASE 
            WHEN AfterDiscountPrice IS NOT NULL THEN AfterDiscountPrice 
            ELSE TotalPrice 
        END) AS TotalPrice
FROM 
    [Order]
WHERE 
    IsActive = 1 
    AND MONTH(CreatedTime) = MONTH(@StartDate) 
    AND YEAR(CreatedTime) = YEAR(@StartDate)
GROUP BY 
    [Status];
END;
GO
