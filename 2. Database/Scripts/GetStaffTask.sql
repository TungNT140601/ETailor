SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create the stored procedure in the specified schema
ALTER PROCEDURE [dbo].[GetStaffTask]
    @StaffId NVARCHAR(30)
AS
BEGIN
    SELECT P.*
    FROM Product P
    INNER JOIN [Order] O ON O.Id = P.OrderId
    WHERE P.StaffMakerId = @StaffId
    AND P.IsActive = 1 
    AND P.[Status] > 0
    AND O.IsActive = 1 
    AND O.[Status] > 0
    ORDER BY 
    CASE 
        WHEN P.[Status] = 4 THEN 1
        WHEN P.[Status] = 2 THEN 2
        WHEN P.[Status] = 1 THEN 3
        WHEN P.[Status] = 5 THEN 4
        ELSE 5 -- Optional: for any other statuses not explicitly mentioned
    END,
    P.[Index] ASC
END
GO
