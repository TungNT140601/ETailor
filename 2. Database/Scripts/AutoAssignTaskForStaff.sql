SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[AutoAssignTaskForStaff]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ProductId NVARCHAR(30);
    DECLARE @CategoryId NVARCHAR(30);
    DECLARE @StaffMakerId NVARCHAR(30);
    DECLARE @IndexTask INT;

    DECLARE NotAssignProducts CURSOR FOR
    SELECT P.Id, PT.CategoryId
    FROM Product P INNER JOIN [Order] O ON O.Id = P.OrderId
        INNER JOIN ProductTemplate PT ON P.ProductTemplateId = PT.Id
    WHERE P.IsActive = 1 AND O.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5 AND O.[Status] = 3 AND P.StaffMakerId IS NULL

    OPEN NotAssignProducts;
    FETCH NEXT FROM NotAssignProducts INTO @ProductId, @CategoryId;

    WHILE @@FETCH_STATUS = 0
    BEGIN

        SET @IndexTask = 0;
        SET @StaffMakerId = NULL;

        SELECT TOP 1 @StaffMakerId = S.Id, @IndexTask = P.[Index]
            FROM Staff S INNER JOIN (
            SELECT TOP 1 S.Id, COUNT(P.Id) AS NumOfTask
            FROM Staff S INNER JOIN Mastery M ON (M.StaffId = S.Id AND M.CategoryId = @CategoryId)
            LEFT JOIN Product P ON (P.StaffMakerId = S.Id AND P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5)
            LEFT JOIN [Order] O ON (O.Id = P.OrderId AND O.IsActive = 1 AND O.[Status] >= 3 AND O.Status < 8)
            WHERE S.IsActive = 1
            GROUP BY S.Id
            ORDER BY NumOfTask ASC
        ) AS ST ON S.Id = ST.Id
        INNER JOIN Product P ON P.StaffMakerId = S.Id
        INNER JOIN [Order] O ON O.Id = P.OrderId
        WHERE S.IsActive = 1
        AND P.IsActive = 1 AND P.[Status] > 0 AND P.[Status] <= 5
        AND O.IsActive = 1 AND O.[Status] >= 3 AND O.Status <= 8
        ORDER BY P.[Index] DESC

        IF @StaffMakerId IS NOT NULL
        BEGIN
            IF @IndexTask IS NULL
            SET @IndexTask = 1;
            ELSE
            SET @IndexTask = @IndexTask + 1;

            UPDATE Product SET
            StaffMakerId = @StaffMakerId,
            [Index] = @IndexTask
            WHERE Id = @ProductId;

            UPDATE ProductStage SET
            StaffId = @StaffMakerId,
            TaskIndex = StageNum
            WHERE ProductId = @ProductId AND IsActive = 1 AND [Status] > 0 AND [Status] < 4;
        END;
        ELSE
        BEGIN
            SELECT TOP 1 @IndexTask = P.[Index]
            FROM Product P
            INNER JOIN [Order] O ON O.Id = P.OrderId
            WHERE  P.IsActive = 1 AND P.[Status] > 0
            AND O.IsActive = 1 AND O.[Status] >= 3 AND O.Status < 8
            AND P.StaffMakerId IS NULL
            ORDER BY P.[Index] DESC

            IF @IndexTask IS NULL
            SET @IndexTask = 1;
            ELSE
            SET @IndexTask = @IndexTask + 1;

            UPDATE Product SET
            StaffMakerId = NULL,
            [Index] = @IndexTask
            WHERE Id = @ProductId;
        END;

    FETCH NEXT FROM NotAssignProducts INTO @ProductId, @CategoryId;
    END;

    CLOSE NotAssignProducts;
    DEALLOCATE NotAssignProducts;

    SELECT 1 AS ReturnValue;
END
GO
