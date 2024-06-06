SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetActiveOrders]
        @CustomerId NVARCHAR(30) NULL
AS
BEGIN
    SELECT *
    FROM [Order]
    WHERE IsActive = 1
    AND (@CustomerId IS NULL OR CustomerId = @CustomerId)
    Order BY CreatedTime DESC;
END;
GO
