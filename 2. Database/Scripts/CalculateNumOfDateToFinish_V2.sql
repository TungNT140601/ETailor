DECLARE @CategoryTable TABLE(
    Id NVARCHAR(30)
);
DECLARE @OrderCreateTime DATETIME;
DECLARE @NumDayToFinish INT = 0;

BEGIN
    INSERT INTO @CategoryTable (Id)
    SELECT Id
    FROM Category
    WHERE Id IN (
    SELECT CategoryId
    FROM ProductTemplate
    WHERE Id IN (
        SELECT ProductTemplateId
    FROM Product
    WHERE OrderId IN (
            SELECT Id
        FROM [Order]
        WHERE IsActive = 1 AND [Status] > 0 AND [Status] < 8 AND CreatedTime < @OrderCreateTime
            ) AND IsActive = 1 AND [Status] > 0 AND [Status] < 5
        )
    )
    
    DECLARE @CategoryId NVARCHAR(30);

    DECLARE CategoryCusor CURSOR FOR
    SELECT Id
    FROM @CategoryTable


    OPEN CategoryCursor;
    FETCH NEXT FROM CategoryCursor INTO @CategoryId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @NumOfStaffHasMastery INT;
        DECLARE @ProductWithTemplate TABLE(
            TotalProduct INT,
            AveTime INT
        );

        SELECT @NumOfStaffHasMastery = COUNT(StaffId)
        FROM Mastery
        WHERE CategoryId = @CategoryId;

        IF(@NumOfStaffHasMastery > 0)
        BEGIN
            INSERT INTO @ProductWithTemplate
                (TotalProduct, AveTime)
            SELECT COUNT(P.Id) AS TotalProduct, PT.AveDateForComplete
            FROM Product P INNER JOIN [Order] O ON (
            P.OrderId = O.Id
                    AND O.IsActive = 1
                    AND O.[Status] > 0 AND O.[Status] < 8
                    AND O.CreatedTime < @OrderCreateTime
            )
                INNER JOIN ProductTemplate PT ON (
                P.ProductTemplateId = PT.Id
                    AND PT.CategoryId = @CategoryId
            )
            WHERE P.IsActive = 1
                AND P.[Status] > 0 AND P.[Status] < 5
            GROUP BY PT.AveDateForComplete

            DECLARE @TotalProduct INT;
            DECLARE @AveTime INT;

            DECLARE TotalProductAndAveTime CURSOR FOR
            SELECT TotalProduct, AveTime
            FROM @ProductWithTemplate


            OPEN TotalProductAndAveTime;
            FETCH NEXT FROM TotalProductAndAveTime INTO @TotalProduct, @AveTime;

            WHILE @@FETCH_STATUS = 0
            BEGIN
                IF (@NumOfStaffHasMastery > 0 AND @TotalProduct > 0 AND @TotalProduct > @NumOfStaffHasMastery)
                SET @NumDayToFinish = @NumDayToFinish + CEILING((@TotalProduct * @AveTime) / @NumOfStaffHasMastery);
                ELSE IF(@TotalProduct = 0 OR @TotalProduct <= @NumOfStaffHasMastery)
                SET @NumDayToFinish = @NumDayToFinish + @AveTime;
            END;
        END;
    END;
END;