SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[UpdateProduct]
    @OrderId NVARCHAR(30),
    @ProductId NVARCHAR(30),
    @ProductTemplateId NVARCHAR(30) NULL,
    @NewFabricMaterialId NVARCHAR(30) NULL,
    @MaterialValue DECIMAL(18,3),
    @IsCusMaterial BIT,
    @NewProductName NVARCHAR(500) NULL,
    @NewSaveOrderComponents NVARCHAR(MAX) NULL,
    @NewNote NVARCHAR(255) NULL,
    @NewProfileBodyId NVARCHAR(30) NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderStatus INT;
    DECLARE @ProducPrice DECIMAL(18, 0) = 0;
    DECLARE @MaterialPrice DECIMAL(18, 0);
    DECLARE @MaterialCategoryPricePerUnit DECIMAL(18, 2);
    DECLARE @CustomerId NVARCHAR(50);
    DECLARE @StaffMakeProfileBody NVARCHAR(50);
    DECLARE @OldProductTemplateId NVARCHAR(30);
    DECLARE @OldProfileBodyId NVARCHAR(30);
    DECLARE @OldProductName NVARCHAR(500);
    DECLARE @OldFabricMaterialId NVARCHAR(30);
    DECLARE @OldProductStatus INT;
    DECLARE @ProductBodySize TABLE (
        BodySizeId NVARCHAR(30),
        [Value] DECIMAL(18,0)
    );

    SELECT @OrderStatus = [Status], @CustomerId = CustomerId
    FROM [Order]
    WHERE Id = @orderId AND IsActive = 0;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;

    IF @OrderStatus >= 2
        THROW 50000, N'Đơn hàng đã duyệt. Không thể chỉnh sửa', 1;

    SELECT @OldFabricMaterialId = FabricMaterialId, @OldProductName = [Name], @OldProductStatus = [Status], @OldProfileBodyId = ReferenceProfileBodyId, @OldProductTemplateId = ProductTemplateId
    FROM Product
    WHERE Id = @ProductId AND [Status] > 0 AND IsActive = 1;

    IF @OldProductStatus IS NULL
        THROW 50000, N'Không tìm thấy sản phẩm', 1;
    IF @OldProductStatus > 1
        THROW 50000, N'Sản phẩm trong giai đoạn thực hiện. Không thể chỉnh sửa', 1;
    IF @OldProductTemplateId NOT LIKE @ProductTemplateId
        THROW 50000, N'Không thể thay đổi bản mẫu của sản phẩm', 1;

    SELECT @ProducPrice = [Price],
        @NewProductName = CASE WHEN @NewProductName IS NULL THEN [Name] ELSE @NewProductName END
    FROM [ProductTemplate]
    WHERE Id = @ProductTemplateId AND IsActive = 1;

    IF @ProducPrice IS NULL
    BEGIN
        RAISERROR('Không tìm thấy bản mẫu', 16, 1);
        RETURN;
    END
    IF @ProductTemplateId IS NULL
        THROW 50000, N'Vui lòng chọn bản mẫu cho sản phẩm', 1;

    SELECT @ProducPrice = [Price]
    FROM [ProductTemplate]
    WHERE Id = @ProductTemplateId AND IsActive = 1;

    IF NOT EXISTS(SELECT 1
    FROM [Material]
    WHERE Id = @NewFabricMaterialId AND IsActive = 1)
        THROW 50000, N'Không tìm thấy nguyên liệu', 1;
    ELSE
    BEGIN
        SELECT @MaterialPrice = MC.PricePerUnit
        FROM MaterialCategory MC INNER JOIN Material M ON MC.Id = M.MaterialCategoryId

        IF @NewFabricMaterialId NOT LIKE @OldFabricMaterialId
        BEGIN
            IF NOT EXISTS(
                SELECT 1
            FROM Product
            WHERE OrderId = @OrderId AND FabricMaterialId = @OldFabricMaterialId AND Id NOT LIKE @ProductId AND [Status] > 0 AND IsActive = 1
            )
            BEGIN
                DELETE OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @OldFabricMaterialId;

                IF NOT EXISTS(
                    SELECT 1
                FROM Product
                WHERE OrderId = @OrderId AND FabricMaterialId = @NewFabricMaterialId AND Id NOT LIKE @ProductId AND [Status] > 0 AND IsActive = 1
                )
                BEGIN
                    INSERT INTO [dbo].[OrderMaterial]
                        ([Id]
                        ,[MaterialId]
                        ,[OrderId]
                        ,[Image]
                        ,[Value]
                        ,[CreatedTime]
                        ,[LastestUpdatedTime]
                        ,[InactiveTime]
                        ,[IsActive]
                        ,[IsCusMaterial]
                        ,[ValueUsed])
                    VALUES
                        (CONVERT(nvarchar(30),CAST(NEWID() AS nvarchar(MAX)),0),
                            @NewFabricMaterialId,
                            @OrderId,
                            NULL,
                            @MaterialValue,
                            DATEADD(HOUR, 7, GETUTCDATE()),
                            DATEADD(HOUR, 7, GETUTCDATE()),
                            NULL,
                            1,
                            @IsCusMaterial,
                            0)
                END;
            END;
        END;
    END;

    IF(@IsCusMaterial = 0)
    SET @ProducPrice = @ProducPrice + ROUND((@MaterialPrice * @MaterialValue), 2);
    ELSE
    SET @ProducPrice = @ProducPrice

    IF NOT EXISTS(SELECT 1
    FROM ProfileBody
    WHERE Id = @NewProfileBodyId AND CustomerId = @CustomerId AND IsActive = 1)
        THROW 50000, N'Không tìm thấy hồ sơ số đo của khách', 1;
    ELSE
    BEGIN
        SELECT @StaffMakeProfileBody = StaffId
        FROM ProfileBody
        WHERE Id = @NewProfileBodyId AND CustomerId = @CustomerId AND IsActive = 1
        IF(@StaffMakeProfileBody IS NULL)
        THROW 50000, N'Hồ sơ số đo này được đo bởi khách. Vui lòng kiểm tra và cập nhật lại hồ sơ số đo', 1;
        ELSE
        BEGIN
            INSERT INTO @ProductBodySize
                (BodySizeId,[Value])
            SELECT BS.Id AS BodySizeId, BA.[Value]
            FROM TemplateBodySize TBS LEFT JOIN BodySize BS ON (TBS.BodySizeId = BS.Id AND TBS.IsActive = 1)
                LEFT JOIN BodyAttribute BA ON (BS.Id = BA.BodySizeId AND BA.IsActive = 1)
                LEFT JOIN ProfileBody PB ON (BA.ProfileBodyId = PB.Id AND PB.IsActive = 1)
            WHERE PB.Id = @NewProfileBodyId AND TBS.ProductTemplateId = @ProductTemplateId

            IF EXISTS(SELECT 1
            FROM @ProductBodySize
            WHERE [Value] IS NULL OR [Value] = 0)
            THROW 50000, N'Số đo cần thiết bị thiếu trong hồ sơ đo. Vui lòng đo và cập nhật bổ sung', 1;
        END;
    END;

    UPDATE [dbo].[Product]
    SET [Name] = @NewProductName
        ,[Note] = @NewNote
        ,[Status] = 1
        ,[LastestUpdatedTime] = DATEADD(HOUR, 7, GETUTCDATE())
        ,[IsActive] = 1
        ,[Price] = @ProducPrice
        ,[SaveOrderComponents] = @NewSaveOrderComponents
        ,[FabricMaterialId] = @NewFabricMaterialId
        ,[ReferenceProfileBodyId] = @NewProfileBodyId
    WHERE Id = @ProductId;

    UPDATE ProductBodySize SET IsActive = 0 WHERE ProductId = @ProductId;

    DECLARE @BodySizeId NVARCHAR(30);
    DECLARE @Value DECIMAL(18,0);

    DECLARE ProductBodySizeCursor CURSOR FOR
    SELECT BodySizeId, [Value]
    FROM @ProductBodySize;

    OPEN ProductBodySizeCursor;
    FETCH NEXT FROM ProductBodySizeCursor INTO @BodySizeId, @Value;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @ProductBodySizeId NVARCHAR(30) = CONVERT(nvarchar(30),CAST(NEWID() AS nvarchar(MAX)),0);
        IF NOT EXISTS(SELECT 1
        FROM ProductBodySize
        WHERE ProductId = @ProductId AND BodySizeId = @BodySizeId)
        BEGIN
            INSERT INTO [dbo].[ProductBodySize]
                ([Id]
                ,[ProductId]
                ,[BodySizeId]
                ,[Value]
                ,[CreatedTime]
                ,[LastestUpdatedTime]
                ,[InactiveTime]
                ,[IsActive])
            VALUES
                (@ProductBodySizeId,
                    @ProductId,
                    @BodySizeId,
                    @Value,
                    DATEADD(HOUR, 7, GETUTCDATE()),
                    DATEADD(HOUR, 7, GETUTCDATE()),
                    NULL,
                    1)
        END;
        ELSE
        BEGIN
            UPDATE ProductBodySize SET IsActive = 1, [Value] = @Value WHERE ProductId = @ProductId AND BodySizeId = @BodySizeId;
        END;

        FETCH NEXT FROM ProductBodySizeCursor INTO @BodySizeId, @Value;
    END

    CLOSE ProductBodySizeCursor;
    DEALLOCATE ProductBodySizeCursor;

    DECLARE @DayToFinish INT = dbo.CalculateNumOfDateToFinish(@ProductId);
    IF @DayToFinish > 0
    BEGIN
    UPDATE Product
    SET PlannedTime = DATEADD(DAY,@DayToFinish, CreatedTime)
    WHERE Id = @ProductId
    END;

    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
