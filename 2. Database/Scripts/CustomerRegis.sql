SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[CustomerRegis]
    @Username NVARCHAR(255) NULL,
    @Email NVARCHAR(255) NULL,
    @Fullname NVARCHAR(255) NULL,
    @Phone NVARCHAR(10) NULL,
    @Address NVARCHAR(MAX) NULL,
    @Password NVARCHAR(MAX) NULL,
    @Avatar TEXT NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CustomerId NVARCHAR(30);

    IF @Username IS NOT NULL AND EXISTS (SELECT 1 FROM Customer WHERE [Username] LIKE @Username AND IsActive = 1)
    SELECT -1 AS ReturnValue; -- dupplicate Username

    IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM Customer WHERE [Email] LIKE @Email AND [Password] IS NOT NULL AND IsActive = 1)
    SELECT -2 AS ReturnValue; -- dupplicate Email

    IF @Phone IS NOT NULL AND EXISTS (SELECT 1 FROM Customer WHERE [Phone] LIKE @Phone AND [Password] IS NOT NULL AND IsActive = 1)
    SELECT -3 AS ReturnValue; -- dupplicate Phone

    IF @Email IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Customer WHERE [Email] LIKE @Email AND [Password] IS NULL AND EmailVerified = 1 AND IsActive = 1)
    SELECT -4 AS ReturnValue; -- Email not verify
    ELSE
    SELECT @CustomerId = Id FROM Customer WHERE [Email] LIKE @Email AND [Password] IS NULL AND EmailVerified = 1 AND IsActive = 1

    -- Update rows in table '[Customer]' in schema '[dbo]'
    UPDATE [dbo].[Customer]
    SET
        [Username] = @Username,
        [Password] = @Password,
        [IsActive] = 1,
        [CreatedTime] = DATEADD(HOUR, 7, GETUTCDATE()),
        [LastestUpdatedTime] = DATEADD(HOUR, 7, GETUTCDATE())
    WHERE Id = @CustomerId

    IF(@Phone IS NOT NULL)
    UPDATE [dbo].[Customer]
    SET
        [Phone] = @Phone
    WHERE Id = @CustomerId

    IF(@Fullname IS NOT NULL)
    UPDATE [dbo].[Customer]
    SET
        [Fullname] = @Fullname
    WHERE Id = @CustomerId
    
    IF(@Address IS NOT NULL)
    UPDATE [dbo].[Customer]
    SET
        [Address] = @Address
    WHERE Id = @CustomerId

    -- Optionally, return a success indicator or the updated order details
    SELECT 1 AS ReturnValue;
-- Instead of RETURN 1;
END;
GO
