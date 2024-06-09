SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetComponentStage]
    @ProductTemplateStageIds NVARCHAR(MAX)
AS
BEGIN
    SELECT CS.*
    FROM TemplateStage TS
    INNER JOIN ComponentStage CS ON CS.TemplateStageId = TS.Id
    WHERE TS.Id IN (SELECT value FROM STRING_SPLIT(@ProductTemplateStageIds, ','))
    AND TS.IsActive = 1
END;
GO