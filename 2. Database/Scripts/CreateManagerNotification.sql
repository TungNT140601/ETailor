SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateManagerNotification]
    @Title NVARCHAR(255),
    @Content NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ManagerId NVARCHAR(30);

    DECLARE ManagerCursor CURSOR FOR
    SELECT Id
    FROM Staff
    WHERE IsActive = 1 AND [Role] = 1

    OPEN ManagerCursor;
    FETCH NEXT FROM ManagerCursor INTO @ManagerId;

    WHILE @@FETCH_STATUS = 0
    BEGIN

    INSERT INTO [dbo].[Notification]
            ([Id]
            ,[CustomerId]
            ,[StaffId]
            ,[Title]
            ,[Content]
            ,[SendTime]
            ,[ReadTime]
            ,[IsRead]
            ,[IsActive])
        VALUES
            (CONVERT(nvarchar(30),CAST(NEWID() AS nvarchar(MAX)),0),
            NULL,
            @ManagerId,
            @Title,
            @Content,
            DATEADD(HOUR, 7, GETUTCDATE()),
            NULL,
            0,
            1)

        FETCH NEXT FROM ManagerCursor INTO @ManagerId;
    END

    CLOSE ManagerCursor;
    DEALLOCATE ManagerCursor;

    RETURN 1
END
GO
