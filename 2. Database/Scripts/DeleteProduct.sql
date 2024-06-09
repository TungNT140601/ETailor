SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[DeleteProduct]
    @ProductId NVARCHAR(30),
    @OrderId NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderStatus INT;
    DECLARE @OrderPaidMoney DECIMAL(18,3);
    DECLARE @OrderDeposit DECIMAL(18,3);
    DECLARE @FabricMaterialId NVARCHAR(30);

    IF NOT EXISTS (SELECT 1
    FROM Product
    WHERE Id = @ProductId AND IsActive = 1 AND [Status] > 0)
        THROW 50000, N'Không tìm thấy sản phẩm', 1;

    SELECT @FabricMaterialId = FabricMaterialId
    FROM Product
    WHERE Id = @ProductId

    SELECT @OrderStatus = [Status], @OrderPaidMoney = PaidMoney, @OrderDeposit = Deposit
    FROM [Order]
    WHERE Id = @OrderId

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;

    IF @OrderStatus >= 2
    BEGIN
        IF @OrderStatus = 0
        THROW 50000, N'Đơn hàng đã hủy. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 2
        THROW 50000, N'Đơn hàng đã duyệt. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 3
        THROW 50000, N'Đơn hàng chuẩn bị vào giai đoạn thực hiện. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 4
        THROW 50000, N'Đơn hàng đang trong giai đoạn thực hiện. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 5
        THROW 50000, N'Đơn hàng đã vào giai đoạn hoàn thiện. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 6
        THROW 50000, N'Đơn hàng đang chờ khách hàng kiểm thử. Không thể xóa sản phẩm', 1;
        ELSE IF @OrderStatus = 7
        THROW 50000, N'Đơn hàng đã bàn giao cho khách. Không thể xóa sản phẩm', 1;
        ELSE
        THROW 50000, N'Đơn hàng đã hủy. Không thể xóa sản phẩm', 1;
    END;
    ELSE IF (@OrderPaidMoney IS NOT NULL AND @OrderPaidMoney > 0) OR (@OrderDeposit IS NOT NULL AND @OrderDeposit > 0)
        THROW 50000, N'Đơn hàng đã thanh toán. Không thể xóa sản phẩm', 1;
    ELSE
    BEGIN
        UPDATE Product
        SET
        LastestUpdatedTime = DATEADD(HOUR, 7, GETUTCDATE()),
        IsActive = 0,
        InactiveTime = DATEADD(HOUR, 7, GETUTCDATE())
        WHERE Id = @ProductId

        DECLARE @GreatestDatePlan DATETIME2;

        SELECT TOP 1 @GreatestDatePlan = PlannedTime
        FROM Product WHERE OrderId = @OrderId AND IsActive = 1 AND [Status] > 0
        ORDER BY PlannedTime DESC

        UPDATE [Order]
        SET PlannedTime = @GreatestDatePlan
        WHERE Id = @OrderId
        
        IF NOT EXISTS (SELECT 1
        FROM Product
        WHERE Id NOT LIKE @ProductId AND OrderId = @OrderId AND FabricMaterialId = @FabricMaterialId AND IsActive = 1 AND [Status] > 0)
        BEGIN
            DELETE FROM OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @FabricMaterialId AND IsActive = 1 AND IsCusMaterial = 0;
        END;
    END;

    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
