SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[GetSuitableDiscoutForOrder]
    @OrderId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1 FROM [Order] WHERE Id = @OrderId AND [Status] > 0)
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
    DECLARE @TotalPrice DECIMAL(18,0) = 0;
    DECLARE @TotalProduct INT = 0;
    SELECT TOP 1 @TotalPrice = TotalPrice, @TotalProduct = TotalProduct
    FROM [Order] 
    WHERE Id = @OrderId AND [Status] > 0

    SELECT *
    FROM Discount
    WHERE IsActive = 1
    AND StartDate <= DATEADD(HOUR, 7, GETUTCDATE())
    AND EndDate >= DATEADD(HOUR, 7, GETUTCDATE())
    AND (ConditionPriceMin <= @TotalPrice OR ConditionProductMin <= @TotalProduct)
END;
GO
