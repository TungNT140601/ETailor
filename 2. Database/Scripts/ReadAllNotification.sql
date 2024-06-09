SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Create the stored procedure in the specified schema
ALTER PROCEDURE [dbo].[ReadAllNotification]
    @UserId NVARCHAR(30),
    @Role INT 
AS
BEGIN
    SET NOCOUNT ON;

    IF @Role = 3
    UPDATE [Notification] SET IsRead = 1, ReadTime = DATEADD(HOUR, 7, GETUTCDATE()) WHERE CustomerId = @UserId AND IsRead = 0;
    ELSE
    UPDATE [Notification] SET IsRead = 1, ReadTime = DATEADD(HOUR, 7, GETUTCDATE()) WHERE StaffId = @UserId AND IsRead = 0;

    SELECT 1 AS ReturnValue;
END;
GO
