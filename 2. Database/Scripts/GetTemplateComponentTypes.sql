SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTemplateComponentTypes]
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
    FROM ComponentType CT INNER JOIN ProductTemplate PT ON (CT.CategoryId = PT.CategoryId AND CT.IsActive = 1)
END;
GO
