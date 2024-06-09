SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetOrderChatList]
    @OrderId NVARCHAR(30),
    @Role INT
AS

BEGIN
IF(@Role = 3)
BEGIN
    UPDATE ChatList
    SET IsRead = 1
    FROM ChatList CL
    INNER JOIN (
        SELECT C.Id
        FROM  [Chat] C
        INNER JOIN [dbo].[Order] O ON C.OrderId = O.Id
        WHERE O.Id = @OrderId
        AND O.[Status] >= 0 AND O.[Status] <= 8
        AND O.IsActive = 1
        AND C.IsActive = 1
    ) AS C ON C.Id = CL.ChatId
    WHERE CL.FromCus = 1;
END
ELSE
BEGIN
    UPDATE ChatList
    SET IsRead = 1
    FROM ChatList CL
    INNER JOIN (
        SELECT C.Id
        FROM  [Chat] C
        INNER JOIN [dbo].[Order] O ON C.OrderId = O.Id
        WHERE O.Id = @OrderId
        AND O.[Status] >= 0 AND O.[Status] <= 8
        AND O.IsActive = 1
        AND C.IsActive = 1
    ) AS C ON C.Id = CL.ChatId
    WHERE CL.FromCus = 0;
END
    SELECT [CL].*
    FROM [ChatList] CL INNER JOIN (
        SELECT TOP 1 C.Id
        FROM Chat C INNER JOIN (
            SELECT TOP 1 Id FROM [dbo].[Order]
            WHERE Id = @OrderId AND [Status] >= 0 AND [Status] <= 8 AND IsActive = 1
        ) AS O ON C.OrderId = O.Id
        WHERE C.IsActive = 1
    ) AS C ON C.Id = CL.ChatId
    WHERE CL.IsActive = 1
    ORDER BY CL.SendTime ASC
END;
GO
