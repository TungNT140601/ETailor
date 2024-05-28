SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[CancelOrder]
    @OrderId NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderStatus INT;

    SELECT @OrderStatus = [Status] FROM [Order] WHERE Id = @OrderId;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
    ELSE IF @OrderStatus = 0
        THROW 50000, N'Hóa đơn đã bị hủy', 1;
    ELSE
    BEGIN
        UPDATE [Order] SET [Status] = 0, [CancelTime] = DATEADD(HOUR, 7, GETUTCDATE()) WHERE Id = @OrderId;

        UPDATE [Product] SET [Status] = 0 WHERE OrderId = @OrderId AND IsActive = 1;

        UPDATE [ProductStage] SET [Status] = 0 WHERE ProductId IN (
            SELECT Id
            FROM Product
            WHERE OrderId = @OrderId AND IsActive = 1
        );
    END;

    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
