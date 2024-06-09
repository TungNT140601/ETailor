SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[StartTask]
    @ProductId NVARCHAR(30),
    @ProductStageId NVARCHAR(30),
    @StaffId NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderId NVARCHAR(30);
    DECLARE @OrderStatus INT;
    DECLARE @ProductStatus INT;
    DECLARE @ProductIndex INT;
    DECLARE @ProductStageStatus INT;
    DECLARE @ProductStageNum INT;

    SELECT @OrderId = OrderId, @ProductStatus = [Status], @ProductIndex = [Index]
    FROM Product
    WHERE Id = @ProductId AND StaffMakerId = @StaffId AND IsActive = 1;

    IF @OrderId IS NULL OR @ProductStatus IS NULL
        THROW 50000, N'Không tìm thấy sản phẩm', 1;

    IF @ProductStatus = 0
        THROW 50000, N'Sản phẩm bị hủy', 1;
    ELSE IF @ProductStatus = 5
        THROW 50000, N'Sản phẩm đã hoàn thành', 1;

    SELECT @OrderStatus = [Status]
    FROM [Order]
    WHERE Id = @OrderId AND IsActive = 1;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
    ELSE IF @OrderStatus = 0
        THROW 50000, N'Hóa đơn bị hủy', 1;
    ELSE IF @OrderStatus = 1
        THROW 50000, N'Hóa đơn chưa được duyệt', 1;
    ELSE IF @OrderStatus = 2
        THROW 50000, N'Hóa đơn chưa được bắt đầu', 1;
    ELSE IF @OrderStatus = 5
        THROW 50000, N'Hóa đơn đã hoàn thiện', 1;
    ELSE IF @OrderStatus = 6
        THROW 50000, N'Hóa đơn đang chờ khách kiểm duyệt', 1;
    ELSE IF @OrderStatus = 8
        THROW 50000, N'Hóa đơn đã hoàn thành', 1;

    IF EXISTS (SELECT 1 FROM Product P INNER JOIN [Order] O ON P.OrderId = O.Id 
        WHERE P.StaffMakerId = @StaffId AND P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5 AND P.[Index] IS NOT NULL AND P.[Index] < @ProductIndex AND O.[Status] > 0 AND O.IsActive = 1)
        THROW 50000, N'Nhiệm vụ trước chưa hoàn thành. Vui lòng thực hiện theo trình tự', 1;

    SELECT @ProductStageStatus = [Status], @ProductStageNum = StageNum
    FROM ProductStage
    WHERE Id = @ProductStageId AND IsActive = 1;

    IF @ProductStageStatus IS NULL
        THROW 50000, N'Không tìm thấy nhiệm vụ', 1;
    ELSE IF @ProductStageStatus = 0
        THROW 50000, N'Công đoạn bị hủy', 1;
    ELSE IF @ProductStageStatus = 2
        THROW 50000, N'Công đoạn đang thực hiện', 1;
    ELSE IF @ProductStageStatus = 4 AND @ProductStatus NOT LIKE 4
        THROW 50000, N'Công đoạn đã hoàn thành', 1;

    IF EXISTS (SELECT 1 FROM ProductStage WHERE Id NOT LIKE @ProductStageId AND ProductId = @ProductId AND [Status] > 1 AND [Status] < 4 AND IsActive = 1)
        THROW 50000, N'Có công đoạn đang chờ hoặc đang thực hiện. Vui lòng hoàn thành trước khi bắt đầu công đoạn này', 1;

    IF EXISTS (SELECT 1 FROM ProductStage WHERE Id NOT LIKE @ProductStageId AND ProductId = @ProductId AND [Status] > 0 AND [Status] < 4 AND IsActive = 1 AND StageNum < @ProductStageNum)
        THROW 50000, N'Công đoạn trước chưa hoàn thành. Vui lòng thực hiện theo trình tự', 1;

    IF NOT EXISTS (SELECT 1 FROM ProductStageMaterial WHERE ProductStageId = @ProductStageId)
        THROW 50000, N'Công đoạn chưa được phân nguyên phụ liệu. Vui lòng liên hệ quản lý để phân nguyên liệu cho công đoạn.', 1;

    UPDATE Product SET 
    [Status] = 2,
    LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE())
    WHERE Id = @ProductId;

    IF @OrderStatus = 7
    UPDATE [Order] SET
    [Status] = 7,
    LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE())
    WHERE Id = @OrderId;
    ELSE
    UPDATE [Order] SET
    [Status] = 4,
    LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE())
    WHERE Id = @OrderId;

    UPDATE ProductStage SET
    [Status] = 2,
    StaffId = @StaffId,
    StartTime = DATEADD(HOUR,7,GETUTCDATE())
    WHERE Id = @ProductStageId;


    SELECT 1 AS ReturnValue;
END;
GO
