SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[FinishTask]
    @ProductId NVARCHAR(30),
    @ProductStageId NVARCHAR(30),
    @StaffId NVARCHAR(30),
    @EvidenceImage NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderId NVARCHAR(30);
    DECLARE @OrderStatus INT;
    DECLARE @ProductStatus INT;
    DECLARE @ProductIndex INT;
    DECLARE @ProductStageStatus INT;
    DECLARE @ProductStageNum INT;
    DECLARE @ReturnValue INT = 1;

    SELECT @OrderId = OrderId, @ProductStatus = [Status], @ProductIndex = [Index]
    FROM Product
    WHERE Id = @ProductId AND StaffMakerId = @StaffId AND IsActive = 1;

    IF @OrderId IS NULL OR @ProductStatus IS NULL
        THROW 50000, N'Không tìm thấy sản phẩm', 1;

    IF @ProductStatus = 0
        THROW 50000, N'Sản phẩm bị hủy', 1;
    ELSE IF @ProductStatus = 1
        THROW 50000, N'Sản phẩm đang chờ', 1;
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
    ELSE IF @ProductStageStatus = 1
        THROW 50000, N'Công đoạn chưa được bắt đầu', 1;
    ELSE IF @ProductStageStatus = 3
        THROW 50000, N'Công đoạn đang tạm dừng', 1;
    ELSE IF @ProductStageStatus = 4 AND @ProductStatus NOT LIKE 4
        THROW 50000, N'Công đoạn đã hoàn thành', 1;

    IF EXISTS (SELECT 1 FROM ProductStage WHERE Id NOT LIKE @ProductStageId AND ProductId = @ProductId AND [Status] > 1 AND [Status] < 4 AND IsActive = 1)
        THROW 50000, N'Có công đoạn đang chờ hoặc đang thực hiện.', 1;

    IF EXISTS (SELECT 1 FROM ProductStage WHERE Id NOT LIKE @ProductStageId AND ProductId = @ProductId AND [Status] > 0 AND [Status] < 4 AND IsActive = 1 AND StageNum < @ProductStageNum)
        THROW 50000, N'Công đoạn trước chưa hoàn thành.', 1;

    IF (SELECT TOP 1 Id FROM ProductStage WHERE ProductId = @ProductId AND IsActive = 1 ORDER BY StageNum DESC) = @ProductStageId
    BEGIN
        UPDATE Product SET 
        [Status] = 5,
        LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE()),
        FinishTime = DATEADD(HOUR,7,GETUTCDATE())
        WHERE Id = @ProductId;

        SET @ReturnValue = 2; -- Notify hoàn thành sản phẩm
        

        IF NOT EXISTS (SELECT 1 FROM Product WHERE Id NOT LIKE @ProductId AND OrderId = @OrderId AND [Status] > 0 AND [Status] < 5 AND IsActive = 1)
        BEGIN

            UPDATE [Order] SET
            [Status] = 5,
            LastestUpdatedTime = DATEADD(HOUR,7,GETUTCDATE()),
            FinishTime = DATEADD(HOUR,7,GETUTCDATE())
            WHERE Id = @OrderId;

            IF @OrderStatus NOT LIKE 7 -- Đơn không bị từ chối
            BEGIN
                SET @ReturnValue = 3; -- Notify hoàn thành đơn
            END;
            ELSE
            BEGIN
                SET @ReturnValue = 4; -- Notify hoàn thành đơn bị từ chối
            END;
        END;
    END;

    UPDATE ProductStage SET
    [Status] = 4,
    StaffId = @StaffId,
    FinishTime = DATEADD(HOUR,7,GETUTCDATE()),
    EvidenceImage = @EvidenceImage
    WHERE Id = @ProductStageId;

    SELECT @ReturnValue AS ReturnValue;
END;
GO
