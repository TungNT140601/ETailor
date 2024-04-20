SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[AddProduct]
    @OrderId NVARCHAR(30),
    @ProductId NVARCHAR(30),
    @ProductTemplateId NVARCHAR(30) NULL,
    @FabricMaterialId NVARCHAR(30) NULL,
    @MaterialValue DECIMAL(18,3),
    @IsCusMaterial BIT,
    @ProductName NVARCHAR(500) NULL,
    @SaveOrderComponents NVARCHAR(MAX) NULL,
    @Note NVARCHAR(255) NULL,
    @ProfileBodyId NVARCHAR(30) NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderStatus INT;
    DECLARE @ProducPrice DECIMAL(18, 0) = 0;
    DECLARE @MaterialPrice DECIMAL(18, 0);
    DECLARE @MaterialCategoryPricePerUnit DECIMAL(18, 2);
    DECLARE @CustomerId NVARCHAR(50);
    DECLARE @StaffMakeProfileBody NVARCHAR(50);
    DECLARE @ProductBodySize TABLE (
        BodySizeId NVARCHAR(30),
        [Value] DECIMAL(18,0)
    );

    SELECT @OrderStatus = [Status], @CustomerId = CustomerId
    FROM [Order]
    WHERE Id = @orderId AND IsActive = 0;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;

    IF @OrderStatus > 2
        THROW 50000, N'Đơn hàng đã vào giai đoạn thực hiện. Không thể thêm sản phẩm', 1;

    SELECT @ProducPrice = [Price],
        @ProductName = CASE WHEN @ProductName IS NULL THEN [Name] ELSE @ProductName END
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
    WHERE Id = @FabricMaterialId AND IsActive = 1)
        THROW 50000, N'Không tìm thấy nguyên liệu', 1;
    ELSE
    BEGIN
        SELECT @MaterialPrice = MC.PricePerUnit
        FROM MaterialCategory MC INNER JOIN Material M ON MC.Id = M.MaterialCategoryId

        IF EXISTS(SELECT 1
        FROM OrderMaterial
        WHERE OrderId = @OrderId AND MaterialId = @FabricMaterialId)
        BEGIN
            UPDATE OrderMaterial SET [Value] = [Value] + @MaterialValue WHERE OrderId = @OrderId AND MaterialId = @FabricMaterialId
        END;
        ELSE
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
                    @FabricMaterialId,
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

    IF(@IsCusMaterial = 0)
    SET @ProducPrice = @ProducPrice + ROUND((@MaterialPrice * @MaterialValue), 2);
    ELSE
    SET @ProducPrice = @ProducPrice

    IF NOT EXISTS(SELECT 1
    FROM ProfileBody
    WHERE Id = @ProfileBodyId AND CustomerId = @CustomerId AND IsActive = 1)
        THROW 50000, N'Không tìm thấy hồ sơ số đo của khách', 1;
    ELSE
    BEGIN
        SELECT @StaffMakeProfileBody = StaffId
        FROM ProfileBody
        WHERE Id = @ProfileBodyId AND CustomerId = @CustomerId AND IsActive = 1
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
            WHERE PB.Id = @ProfileBodyId AND TBS.ProductTemplateId = @ProductTemplateId

            IF EXISTS(SELECT 1
            FROM @ProductBodySize
            WHERE [Value] IS NULL OR [Value] = 0)
            THROW 50000, N'Số đo cần thiết bị thiếu trong hồ sơ đo. Vui lòng đo và cập nhật bổ sung', 1;
        END;
    END;

    INSERT INTO [dbo].[Product]
        ([Id]
        ,[OrderId]
        ,[ProductTemplateId]
        ,[Name]
        ,[Note]
        ,[Status]
        ,[EvidenceImage]
        ,[FinishTime]
        ,[CreatedTime]
        ,[LastestUpdatedTime]
        ,[InactiveTime]
        ,[IsActive]
        ,[Price]
        ,[SaveOrderComponents]
        ,[FabricMaterialId]
        ,[ReferenceProfileBodyId]
        ,[Index]
        ,[StaffMakerId]
        ,[PlannedTime])
    VALUES
        (@ProductId,
            @OrderId,
            @ProductTemplateId,
            @ProductName,
            @Note,
            1,
            NULL,
            NULL,
            DATEADD(HOUR, 7, GETUTCDATE()),
            DATEADD(HOUR, 7, GETUTCDATE()),
            NULL,
            1,
            @ProducPrice,
            @SaveOrderComponents,
            @FabricMaterialId,
            @ProfileBodyId,
            NULL,
            NULL,
            NULL)

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

    
    SELECT 1 AS ReturnValue; -- Instead of RETURN 1;
END;
GO
