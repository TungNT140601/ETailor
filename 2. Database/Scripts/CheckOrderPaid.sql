SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[CheckOrderPaid]
    @OrderId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1
    FROM [Order]
    WHERE Id = @OrderId AND [Status] > 0)
        THROW 50000, N'Không tìm thấy hóa đơn', 1;

    DECLARE @TotalProducts INT,
            @TotalPrice DECIMAL(18,2),
            @DiscountPrice DECIMAL(18,2) = 0,
            @AfterDiscountPrice DECIMAL(18,2) = 0,
            @PaidMoney DECIMAL(18,2),
            @UnPaidMoney DECIMAL(18,2),
            @DiscountCode NVARCHAR(30),
            @DiscountId NVARCHAR(50);
    -- Calculate total products and total price
    SELECT
        @TotalProducts = COUNT(*),
        @TotalPrice = SUM(p.Price)
    FROM Product p
    WHERE p.OrderId = @OrderId
        AND p.IsActive = 1
        AND p.Status != 0;

    SELECT @AfterDiscountPrice = AfterDiscountPrice, @DiscountPrice = DiscountPrice, @DiscountCode = DiscountCode, @DiscountId = DiscountId
    FROM [Order]
    WHERE Id = @OrderId

    -- Apply discount logic
    IF @DiscountId IS NULL
    BEGIN
        SET @AfterDiscountPrice = @TotalPrice;
        SET @DiscountPrice = 0;
        SET @DiscountCode = '';
    END;

    -- Calculate paid money
    SELECT @PaidMoney = ISNULL(SUM(Amount), 0)
    FROM Payment
    WHERE OrderId = @OrderId
        AND Status = 0;

    -- Calculate unpaid money
    IF(@AfterDiscountPrice IS NOT NULL)
    SET @UnPaidMoney = @AfterDiscountPrice - @PaidMoney;
    ELSE
    SET @UnPaidMoney = @TotalPrice - @PaidMoney;

    -- Update the order
    UPDATE [Order]
    SET TotalProduct = @TotalProducts,
        TotalPrice = @TotalPrice,
        DiscountCode = @DiscountCode,
        DiscountId = @DiscountId,
        DiscountPrice = ISNULL(@DiscountPrice, 0),
        AfterDiscountPrice = @AfterDiscountPrice,
        PaidMoney = @PaidMoney,
        UnPaidMoney = @UnPaidMoney
    WHERE Id = @OrderId;

    IF @PaidMoney > 0
    UPDATE [Order]
    SET IsActive = 1
    WHERE Id = @OrderId;

    -- Optionally, return a success indicator or the updated order details
    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
