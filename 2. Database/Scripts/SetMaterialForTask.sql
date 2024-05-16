SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[SetMaterialForTask]
    @TaskId NVARCHAR(30) NULL,
    @StageId NVARCHAR(30) NULL,
    @StageMaterials dbo.MaterialStageType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    -- DECLARE @Errmsg NVARCHAR(MAX) = '';

    DECLARE @OrderId NVARCHAR(30);
    DECLARE @OrderStatus INT;
    DECLARE @TaskStatus INT;
    DECLARE @TaskStageStatus INT;
    DECLARE @InsertTable TABLE (
        Id NVARCHAR(30) NULL,
        StageId NVARCHAR(30) NULL,
        MaterialId NVARCHAR(30) NULL,
        Quantity DECIMAL (18,3) NULL
    );

    IF NOT EXISTS(SELECT 1
    FROM Product
    WHERE Id = @TaskId AND IsActive = 1)
        THROW 50000, N'Không tìm thấy nhiệm vụ', 1;
    ELSE 
    BEGIN
        SELECT @OrderId = OrderId, @TaskStatus = [Status]
        FROM Product
        WHERE Id = @TaskId AND IsActive = 1;
        IF @TaskStatus = 0
            THROW 50000, N'Nhiệm vụ bị hủy', 1;
        ELSE IF @TaskStatus = 5
            THROW 50000, N'Nhiệm vụ đã hoàn thành', 1;

    END;

    SELECT @OrderStatus = [Status]
    FROM [Order]
    WHERE Id = @OrderId AND IsActive = 1;

    IF @OrderStatus IS NULL
        THROW 50000, N'Không tìm thấy hóa đơn', 1;
    ELSE IF @OrderStatus = 0
        THROW 50000, N'Hóa đơn bị hủy', 1;
    ELSE IF @OrderStatus = 1
        THROW 50000, N'Hóa đơn chưa được xác nhận', 1;
    ELSE IF @OrderStatus = 5
        THROW 50000, N'Các sản phẩm của hóa đơn đã xong', 1;
    ELSE IF @OrderStatus = 6
        THROW 50000, N'Hóa đơn đang chờ khách hàng kiểm thử', 1;
    ELSE IF @OrderStatus = 8
        THROW 50000, N'Hóa đơn đã hoàn thành', 1;

    SELECT @TaskStageStatus = [Status]
    FROM ProductStage
    WHERE Id = @StageId AND IsActive = 1;

    IF @TaskStageStatus IS NULL
        THROW 50000, N'Không tìm thấy quy trình của nhiệm vụ', 1;
    ELSE IF @TaskStageStatus = 0
        THROW 50000, N'Quy trình bị hủy', 1;
    ELSE IF @TaskStageStatus = 5
        THROW 50000, N'Quy trình đã hoàn thành', 1;

    IF NOT EXISTS (SELECT 1
    FROM @StageMaterials)
        THROW 50000, N'Không có nguyên phụ liệu', 1;

    DECLARE @MaterialId NVARCHAR(30);
    DECLARE @NewValue DECIMAL (18,3);
    DECLARE @ExistProductStageMaterialId NVARCHAR(30);

    DECLARE StageMaterialsCursor CURSOR FOR
    SELECT MaterialId, [Value]
    FROM @StageMaterials;

    OPEN StageMaterialsCursor;
    FETCH NEXT FROM StageMaterialsCursor INTO @MaterialId, @NewValue;

    -- Check input Material for stage
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF (SELECT COUNT(*)
        FROM @StageMaterials
        WHERE MaterialId = @MaterialId) > 1
            THROW 50000, N'Nguyên phụ liệu bị trùng', 1;
        IF @NewValue IS NULL OR @NewValue < 0
            THROW 50000, N'Số lượng nguyên phụ liệu không phù hợp', 1;

        DECLARE @MaterialName NVARCHAR(100);
        DECLARE @StockValue DECIMAL (18,3);
        DECLARE @ValueRecieve INT;
        DECLARE @ValueUsed INT;
        DECLARE @OldValue DECIMAL (18,3);

        SET @ExistProductStageMaterialId = NULL;

        SELECT @StockValue = [Quantity], @MaterialName = [Name]
        FROM Material
        WHERE Id = @MaterialId AND IsActive = 1;

        SELECT @ExistProductStageMaterialId = Id, @OldValue = Quantity
        FROM ProductStageMaterial
        WHERE MaterialId = @MaterialId AND ProductStageId = @StageId;

        IF @MaterialName IS NULL
            THROW 50000, N'Không tìm thấy nguyên phụ liệu', 1;
        ELSE
        BEGIN
            DECLARE @ErrorMsg NVARCHAR(MAX);
            IF NOT EXISTS (SELECT 1
            FROM OrderMaterial
            WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsCusMaterial = 1)
            BEGIN
                SET @ErrorMsg  = CAST(N'Số lượng ' + @MaterialName + N' trong kho không đủ' AS NVARCHAR(MAX));
                IF (@StockValue = 0 OR (@StockValue + ISNULL(@OldValue, 0)) < @NewValue)
                THROW 50000, @ErrorMsg, 1;
            END;
            ELSE
            BEGIN
                SET @ErrorMsg  = CAST(N'Số lượng ' + @MaterialName + N' của khách không đủ' AS NVARCHAR(MAX));
                SELECT @ValueRecieve = [Value], @ValueUsed = ValueUsed
                FROM OrderMaterial
                WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsCusMaterial = 1;
                IF ((ISNULL(@ValueRecieve, 0) - ISNULL(@ValueUsed, 0)) <= 0 OR (ISNULL(@ValueRecieve, 0) - ISNULL(@ValueUsed, 0) + ISNULL(@OldValue, 0)) < @NewValue)
                THROW 50000, @ErrorMsg, 1;
            END;
        END;


        INSERT INTO @InsertTable
            (Id, StageId, MaterialId, Quantity)
        VALUES
            (ISNULL(@ExistProductStageMaterialId, NULL), @StageId, @MaterialId, @NewValue)

        -- SET @Errmsg = @Errmsg + 'SetupData: ' + @MaterialId + ', Id :' + ISNULL(@ExistProductStageMaterialId, 'Null') + ' ';

        FETCH NEXT FROM StageMaterialsCursor INTO @MaterialId, @NewValue;
    END;
    CLOSE StageMaterialsCursor;
    DEALLOCATE StageMaterialsCursor;
    ----------------------------------------------------------------------------------------------
    DECLARE @ProductStageMaterialIdExist NVARCHAR(30);
    DECLARE @ExistValue DECIMAL (18,3);
    DECLARE CheckMaterial CURSOR FOR
    SELECT Id, MaterialId, Quantity
    FROM ProductStageMaterial
    WHERE ProductStageId = @StageId AND MaterialId NOT IN (
        SELECT MaterialId
        FROM @InsertTable
    )
    OPEN CheckMaterial;
    FETCH NEXT FROM CheckMaterial INTO @ProductStageMaterialIdExist, @MaterialId, @ExistValue;

    -- Add Old value if it not exist in new list
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsCusMaterial = 1)
        UPDATE Material SET Quantity = ISNULL(Quantity, 0) + ISNULL(@ExistValue, 0)
            WHERE Id = @MaterialId;

        UPDATE OrderMaterial SET ValueUsed = ISNULL(ValueUsed, 0) + ISNULL(@ExistValue, 0)
            WHERE OrderId = @OrderId AND MaterialId = @MaterialId;

        IF @ProductStageMaterialIdExist IS NOT NULL
        DELETE ProductStageMaterial WHERE Id = @ProductStageMaterialIdExist

        FETCH NEXT FROM CheckMaterial INTO @ProductStageMaterialIdExist, @MaterialId, @ExistValue;
    END;
    CLOSE CheckMaterial;
    DEALLOCATE CheckMaterial;

    DECLARE @ProductStageMaterialId NVARCHAR(30);
    DECLARE @InputValue DECIMAL (18,3);
    DECLARE UpdateMaterial CURSOR FOR
    SELECT Id, MaterialId, Quantity
    FROM @InsertTable

    OPEN UpdateMaterial;
    FETCH NEXT FROM UpdateMaterial INTO @ProductStageMaterialId, @MaterialId, @InputValue;

    -- Add/Update ProductStageMaterial And Update Material And Update OrderMaterial
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @ProductStageMaterialId IS NULL
        BEGIN
            INSERT INTO ProductStageMaterial
                (Id, ProductStageId, MaterialId, Quantity)
            VALUES
                (CONVERT(nvarchar(30),CAST(NEWID() AS nvarchar(MAX)),0),
                    @StageId,
                    @MaterialId,
                    @InputValue);

            IF NOT EXISTS (SELECT 1 FROM OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsCusMaterial = 1)
            UPDATE Material SET Quantity = Quantity - @InputValue WHERE Id = @MaterialId;

            UPDATE OrderMaterial SET ValueUsed = ValueUsed + @InputValue WHERE OrderId = @OrderId AND MaterialId = @MaterialId;
        END;
        ELSE
        BEGIN
            SELECT @OldValue = Quantity
            FROM ProductStageMaterial
            WHERE Id = @ProductStageMaterialId;

            IF NOT EXISTS (SELECT 1 FROM OrderMaterial WHERE OrderId = @OrderId AND MaterialId = @MaterialId AND IsCusMaterial = 1)
            UPDATE Material SET Quantity = Quantity + ISNULL(@OldValue, 0) - @InputValue WHERE Id = @MaterialId;

            UPDATE OrderMaterial SET ValueUsed = ValueUsed - ISNULL(@OldValue, 0) + @InputValue WHERE OrderId = @OrderId AND MaterialId = @MaterialId;

            UPDATE ProductStageMaterial
            SET Quantity = @InputValue
            WHERE Id = @ProductStageMaterialId;
        END;
        FETCH NEXT FROM UpdateMaterial INTO @ProductStageMaterialId, @MaterialId, @Value;
    END;
    CLOSE UpdateMaterial;
    DEALLOCATE UpdateMaterial;

    SELECT 1 AS ReturnValue;
END;
GO
