SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTemplateComponents]
                    @ProductTemplateId NVARCHAR(30) NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @ProductTemplateId IS NULL
        THROW 50000, N'Vui lòng chọn bản mẫu cho sản phẩm', 1;

    IF NOT EXISTS(
        SELECT 1
        FROM ProductTemplate
        WHERE Id = @ProductTemplateId AND IsActive = 1
    )
        THROW 50000, N'Không tìm thấy bản mẫu', 1;

    SELECT CT.*
    FROM Component CT INNER JOIN ProductTemplate PT ON (CT.ProductTemplateId = PT.Id AND CT.IsActive = 1 AND PT.Id = @ProductTemplateId)
END;
GO
