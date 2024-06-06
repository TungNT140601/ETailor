SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetTemplateDashboard]
    @Date DATETIME
AS
BEGIN
SELECT [PT].[Id],
[PT].[Name],
[PT].[ThumbnailImage],
B.Total
FROM ProductTemplate PT LEFT JOIN (
    SELECT P.ProductTemplateId, COUNT(P.Id) AS Total
    FROM Product P INNER JOIN (
        Select 
            Id
        FROM 
            [Order]
        WHERE 
            IsActive = 1 
            AND [Status] = 8
            AND MONTH(CreatedTime) = MONTH(@Date) 
            AND YEAR(CreatedTime) = YEAR(@Date)
    ) AS A ON P.OrderId = A.Id AND P.IsActive = 1
    GROUP BY P.ProductTemplateId
) AS B ON PT.Id = B.ProductTemplateId
WHERE PT.IsActive = 1 OR PT.Id IN (
    SELECT P.ProductTemplateId
    FROM Product P INNER JOIN (
        Select 
            Id
        FROM 
            [Order]
        WHERE 
            IsActive = 1 
            AND [Status] = 8
            AND MONTH(CreatedTime) = MONTH(@Date) 
            AND YEAR(CreatedTime) = YEAR(@Date)
    ) AS A ON P.OrderId = A.Id AND P.IsActive = 1
    GROUP BY P.ProductTemplateId
)
END;
GO
