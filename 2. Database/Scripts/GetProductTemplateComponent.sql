SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetProductTemplateComponent]
    @ProductTemplateIds NVARCHAR(MAX)
AS
BEGIN
    SELECT *
    FROM Component
    WHERE ProductTemplateId IN (SELECT value FROM STRING_SPLIT(@ProductTemplateIds, ','))
    AND IsActive = 1
END;
GO