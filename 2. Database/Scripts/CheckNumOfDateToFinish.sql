ALTER PROCEDURE dbo.CheckNumOfDateToFinish
AS
BEGIN
    DECLARE @ProductId NVARCHAR(30);
    DECLARE @OrderId NVARCHAR(30);
    DECLARE @OrderPlannedTime DATETIME;
    DECLARE @ProductCreatedTime DATETIME;
    DECLARE @ProductPlannedTime DATETIME;
    DECLARE @DateNeed INT;

    DECLARE ProductCursor CURSOR FOR
    SELECT P.Id, P.OrderId, P.CreatedTime
    FROM Product P INNER JOIN [Order] O ON O.Id = P.OrderId
    WHERE P.IsActive = 1 AND O.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5 AND O.[Status] > 0 AND O.[Status] < 8


    OPEN ProductCursor;
    FETCH NEXT FROM ProductCursor INTO @ProductId, @OrderId,@ProductCreatedTime;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @OrderPlannedTime = PlannedTime
        FROM [Order]
        WHERE Id = @OrderId

        SET @DateNeed = dbo.CalculateNumOfDateToFinish(@ProductId);

        IF @DateNeed > 0
        BEGIN
            SET @ProductPlannedTime = DATEADD(DAY,@DateNeed,@ProductCreatedTime);

            UPDATE Product SET PlannedTime = @ProductPlannedTime WHERE Id = @ProductId;
            IF @OrderPlannedTime IS NULL OR @OrderPlannedTime < @ProductPlannedTime
        UPDATE [Order] SET PlannedTime = @ProductPlannedTime WHERE Id = @OrderId;
        END;

        FETCH NEXT FROM ProductCursor INTO @ProductId, @OrderId,@ProductCreatedTime;
    END

    CLOSE ProductCursor;
    DEALLOCATE ProductCursor;
END