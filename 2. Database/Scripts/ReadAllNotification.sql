-- Create a new stored procedure called 'ReadAllNotification' in schema 'dbo'
-- Drop the stored procedure if it already exists
IF EXISTS (
SELECT *
    FROM INFORMATION_SCHEMA.ROUTINES
WHERE SPECIFIC_SCHEMA = N'dbo'
    AND SPECIFIC_NAME = N'ReadAllNotification'
    AND ROUTINE_TYPE = N'PROCEDURE'
)
DROP PROCEDURE dbo.ReadAllNotification
GO
-- Create the stored procedure in the specified schema
CREATE PROCEDURE dbo.ReadAllNotification
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