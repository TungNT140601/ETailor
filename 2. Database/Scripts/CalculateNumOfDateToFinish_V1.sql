SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[CalculateNumOfDateToFinish](
    @ProductId NVARCHAR(30)
)
RETURNS INT
BEGIN
    DECLARE @OrderId NVARCHAR(30);
    DECLARE @OrderCreateTime DATETIME;
    DECLARE @ProductTemplateId NVARCHAR(30);
    DECLARE @CategoryId NVARCHAR(30);
    DECLARE @AveDateForComplete INT;
    DECLARE @TotalNotFinishProduct INT;
    DECLARE @TotalStaffHasMastery INT;

    SELECT @OrderId = OrderId, @ProductTemplateId = ProductTemplateId
    FROM Product
    WHERE Id = @ProductId AND IsActive = 1 AND [Status] > 0

    IF @OrderId IS NULL
        RETURN -1;
    --/'Không tìm thấy sản phẩm'

    IF NOT EXISTS( SELECT 1
    FROM [Order]
    WHERE Id = @OrderId AND [Status] > 0)
        RETURN -2; --/'Không tìm thấy hóa đơn'
    ELSE
    SELECT @OrderCreateTime = CreatedTime
    FROM [Order]
    WHERE Id= @OrderId

    SELECT @CategoryId = CategoryId, @AveDateForComplete = AveDateForComplete
    FROM ProductTemplate
    WHERE Id = @ProductTemplateId

    IF @CategoryId IS NULL
        RETURN -3;
    --/'Không tìm thấy bản mẫu'

    SELECT @TotalStaffHasMastery = COUNT(*)
    FROM Mastery
    WHERE CategoryId = @CategoryId AND StaffId IN (
        SELECT Id
        FROM Staff
        WHERE IsActive = 1
    );

    SELECT @TotalNotFinishProduct = COUNT(*)
    FROM Product P INNER JOIN [Order] O ON P.OrderId = O.Id
    WHERE P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5 AND P.ProductTemplateId IN (
            SELECT Id
        FROM [dbo].[ProductTemplate]
        WHERE CategoryId = @CategoryId
        )
        AND ((O.[Status] > 0 AND O.[Status] < 8 AND O.IsActive = 1 AND O.CreatedTime < @OrderCreateTime) OR O.Id = @OrderId)


    IF @TotalStaffHasMastery IS NULL OR @TotalStaffHasMastery = 0
    RETURN -4;
    --/'Không có staff phù hợp'

    DECLARE @Date DECIMAL(18,3);

    IF(@TotalNotFinishProduct >= @TotalStaffHasMastery)
    BEGIN
        SET @Date = (@TotalNotFinishProduct / @TotalStaffHasMastery + 1) * @AveDateForComplete;
    END;
    ELSE
    BEGIN
        SET @Date = @AveDateForComplete;
    END;

    RETURN CEILING(@Date);
END
GO
