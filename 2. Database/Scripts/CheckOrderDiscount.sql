SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[CheckOrderDiscount]
    @OrderId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1 FROM [Order] WHERE Id = @OrderId AND [Status] > 0)
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
        
    DECLARE @TotalProducts INT,
            @TotalPrice DECIMAL(18,2),
            @DiscountPrice DECIMAL(18,2) = 0,
            @AfterDiscountPrice DECIMAL(18,2) = 0,
            @PaidMoney DECIMAL(18,2),
            @UnPaidMoney DECIMAL(18,2),
            @DiscountId NVARCHAR(50),
            @DiscountCode NVARCHAR(50),
            @DiscountPercent DECIMAL(5,2),
            @ConditionProductMin INT,
            @ConditionPriceMin DECIMAL(18,2),
            @ConditionPriceMax DECIMAL(18,2),
            @CalculatedDiscountPrice DECIMAL(18,2) = 0;
    -- Calculate total products and total price
    SELECT
        @TotalProducts = COUNT(*),
        @TotalPrice = SUM(p.Price)
    FROM Product p
    WHERE p.OrderId = @OrderId
    AND p.IsActive = 1
    AND p.Status != 0;

    -- Retrieve applicable discount if any
    SELECT
        @DiscountId = d.Id,
        @DiscountCode = d.Code,
        @DiscountPrice = d.DiscountPrice,
        @DiscountPercent = d.DiscountPercent,
        @ConditionProductMin = d.ConditionProductMin,
        @ConditionPriceMin = d.ConditionPriceMin,
        @ConditionPriceMax = d.ConditionPriceMax
    FROM Discount d
    INNER JOIN [Order] o ON o.DiscountId = d.Id
    WHERE o.Id = @OrderId
    AND d.IsActive = 1;

    -- Apply discount logic
    IF @DiscountId IS NOT NULL
    BEGIN
        IF @TotalProducts >= @ConditionProductMin OR
        (@TotalPrice >= @ConditionPriceMin)
        BEGIN
            IF @DiscountPrice IS NOT NULL AND @DiscountPrice > 0
                SET @CalculatedDiscountPrice = @DiscountPrice;
            ELSE IF @DiscountPercent IS NOT NULL AND @DiscountPercent > 0
                SET @CalculatedDiscountPrice = @TotalPrice * @DiscountPercent / 100;

            -- Check if calculated discount price is greater than condition max price
            IF @CalculatedDiscountPrice > @ConditionPriceMax
                SET @CalculatedDiscountPrice = @ConditionPriceMax;

            SET @AfterDiscountPrice = @TotalPrice - @CalculatedDiscountPrice;
        END
        ELSE
            SET @AfterDiscountPrice = @TotalPrice;
    END
    ELSE
    BEGIN
        SET @AfterDiscountPrice = @TotalPrice;
        SET @DiscountCode = '';
        SET @CalculatedDiscountPrice = 0;
    END

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
        DiscountPrice = ISNULL(@CalculatedDiscountPrice, 0),
        AfterDiscountPrice = @AfterDiscountPrice,
        PaidMoney = @PaidMoney,
        UnPaidMoney = @UnPaidMoney
    WHERE Id = @OrderId;

    -- Optionally, return a success indicator or the updated order details
    SELECT 1 AS ReturnValue; -- Instead of RETURN 1;
END;
GO
