SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrderProductTemplateStage]
    @ProductTemplateIds NVARCHAR(MAX)
AS
BEGIN
    SELECT TS.*
    FROM [ProductTemplate] P INNER JOIN TemplateStage TS ON TS.ProductTemplateId = P.Id
    WHERE P.Id IN (SELECT value FROM STRING_SPLIT(@ProductTemplateIds, ','))
    AND TS.IsActive = 1
END;
GO
