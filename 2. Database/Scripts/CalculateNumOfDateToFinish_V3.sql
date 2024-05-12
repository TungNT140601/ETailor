DECLARE @ProductId NVARCHAR(30) = '2d70e392-d42b-4a5e-9c69-7f4882';


DECLARE @OrderId NVARCHAR(30);
DECLARE @ProductTemplateId NVARCHAR(30);
DECLARE @OrderCreateTime DATETIME;
DECLARE @ProductCreateTime DATETIME;

SELECT @OrderId = O.Id, @ProductTemplateId = P.ProductTemplateId, @OrderCreateTime = O.CreatedTime, @ProductCreateTime = P.CreatedTime
FROM Product P INNER JOIN [Order] O ON (P.OrderId = O.Id AND P.IsActive = 1 AND O.IsActive = 1 AND P.Status > 0 AND P.Status < 5 AND O.Status > 0 AND O.Status < 8)
WHERE P.Id = @ProductId;

DECLARE @TotalDaysNeeded FLOAT = 0;
DECLARE @CategoryId NVARCHAR(30);
DECLARE @AveDate FLOAT;
DECLARE @EffectiveStaff INT;
DECLARE @ProductCount INT;

DECLARE CategoryCursor CURSOR FOR
        SELECT PT.CategoryId
FROM ProductTemplate PT INNER JOIN Product P ON PT.Id = P.ProductTemplateId
    INNER JOIN [Order] O ON P.OrderId = O.Id
WHERE ((P.Status > 0 AND P.Status < 5
    AND P.IsActive = 1 AND P.CreatedTime < @ProductCreateTime) OR P.Id = @ProductId)
    AND ((O.Status > 0 AND O.Status < 8 AND O.IsActive = 1 AND O.CreatedTime < @OrderCreateTime) OR O.Id = @OrderId)
GROUP BY PT.CategoryId;

OPEN CategoryCursor;

FETCH NEXT FROM CategoryCursor INTO @CategoryId;

WHILE @@FETCH_STATUS = 0
    BEGIN
    -- Calculate days needed for the current category
    SELECT @AveDate = AveDateForComplete, @ProductCount = SUM(ProductCount)
    FROM (
        SELECT PT.AveDateForComplete, COUNT(*) AS ProductCount
        FROM Product P INNER JOIN ProductTemplate PT ON P.ProductTemplateId = PT.Id
            INNER JOIN [Order] O ON P.OrderId = O.Id
        WHERE PT.CategoryId = @CategoryId
            AND ((P.Status > 0 AND P.Status < 5
            AND P.IsActive = 1 AND P.CreatedTime < @ProductCreateTime) OR P.Id = @ProductId)
            AND ((O.Status > 0 AND O.Status < 8 AND O.IsActive = 1 AND O.CreatedTime < @OrderCreateTime) OR O.Id = @OrderId)
        GROUP BY PT.AveDateForComplete
    ) AS DataProduct
    GROUP BY AveDateForComplete;

    -- Calculate effective staff for the current category
    SELECT @EffectiveStaff = COUNT(DISTINCT M.StaffId)
    FROM Mastery M
        JOIN Staff S ON M.StaffId = S.Id
    WHERE M.CategoryId = @CategoryId AND S.IsActive = 1;

    -- Accumulate total days needed, adjusted for effective staff
    IF(@ProductCount > @EffectiveStaff)
    SET @TotalDaysNeeded += @AveDate * (@ProductCount / @EffectiveStaff + 1);
    -- SET @TotalDaysNeeded += (@AveDate / NULLIF(@EffectiveStaff, 0));
    ELSE
    SET @TotalDaysNeeded += @AveDate

    FETCH NEXT FROM CategoryCursor INTO @CategoryId;
END;

CLOSE CategoryCursor;
DEALLOCATE CategoryCursor;

RETURN CEILING(@TotalDaysNeeded);