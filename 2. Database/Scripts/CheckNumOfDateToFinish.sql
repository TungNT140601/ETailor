DECLARE @ProductId NVARCHAR(30);

DECLARE ProductCursor CURSOR FOR
SELECT P.Id
FROM Product P INNER JOIN [Order] O ON O.Id = P.OrderId
WHERE P.IsActive = 1 AND O.IsActive = 1 AND P.[Status] > 0 AND P.[Status] < 5 AND O.[Status] > 0 AND O.[Status] < 8


OPEN ProductCursor;
FETCH NEXT FROM ProductCursor INTO @ProductId;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @DateNeed INT;

    SET @DateNeed = dbo.CalculateNumOfDateToFinish(@ProductId);

    IF @DateNeed > 0
    UPDATE Product SET PlannedTime = DATEADD(DAY,@DateNeed,CreatedTime) WHERE Id = @ProductId


    FETCH NEXT FROM ProductCursor INTO @ProductId;
END

CLOSE ProductCursor;
DEALLOCATE ProductCursor;