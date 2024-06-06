SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[InsertChatList]
    @ChatId NVARCHAR(30),
    @Message NVARCHAR(500) = NULL,
    @Images TEXT = NULL,
    @ReplierId NVARCHAR(30) = NULL,
    @CustomerId NVARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FromCustomer BIT = CASE WHEN @CustomerId IS NOT NULL AND LEN(@CustomerId) > 0 THEN 1 ELSE 0 END
    
    IF(@Message IS NOT NULL)
    BEGIN
        DECLARE @Id1 NVARCHAR(30) = (SELECT LEFT(CAST(NEWID() AS NVARCHAR(36)), 30))
        INSERT INTO ChatList (Id, ChatId, ReplierId, [Message], FromCus, SendTime, IsRead, ReadTime, InactiveTime, IsActive, Images)
        VALUES (@Id1, @ChatId, @ReplierId, @Message, @FromCustomer, DATEADD(HOUR, 7, GETUTCDATE()), 0, NULL, NULL, 1, NULL)
    END

    IF(@Images IS NOT NULL)
    BEGIN
        DECLARE @Id2 NVARCHAR(30) = (SELECT LEFT(CAST(NEWID() AS NVARCHAR(36)), 30))
        INSERT INTO ChatList (Id, ChatId, ReplierId, [Message], FromCus, SendTime, IsRead, ReadTime, InactiveTime, IsActive, Images)
        VALUES (@Id2, @ChatId, @ReplierId, NULL, @FromCustomer, DATEADD(HOUR, 7, GETUTCDATE()), 0, NULL, NULL, 1, @Images)
    END
    
    SELECT 1 AS ReturnValue;
END
GO
