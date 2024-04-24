DECLARE @NumDate INT = -1000
-- DELETE ProductBodySize WHERE ProductId IN (
--     SELECT Id
--     FROM [Product] WHERE OrderId IN (
--     SELECT Id
--     FROM [Order] WHERE CreatedTime > DATEADD(DAY, @NumDate, GETUTCDATE())
-- ));
DELETE ProductComponent WHERE ProductStageId IN (
    SELECT Id
    FROM ProductStage WHERE ProductId IN (
    SELECT Id
    FROM [Product] WHERE OrderId IN (
    SELECT Id
    FROM [Order] WHERE CreatedTime > DATEADD(DAY, @NumDate, GETUTCDATE())
)));
DELETE ProductStage WHERE ProductId IN (
    SELECT Id
    FROM [Product] WHERE OrderId IN (
    SELECT Id
    FROM [Order] WHERE CreatedTime > DATEADD(DAY, @NumDate, GETUTCDATE())
));
-- DELETE [Product] WHERE OrderId IN (
--     SELECT Id
--     FROM [Order] WHERE CreatedTime > DATEADD(DAY, @NumDate, GETUTCDATE())
-- );
-- DELETE [OrderMaterial] WHERE OrderId IN (
--     SELECT Id
--     FROM [Order] WHERE CreatedTime > DATEADD(DAY, @NumDate, GETUTCDATE())
-- );
-- DELETE [Payment] WHERE OrderId IN (
--     SELECT Id
--     FROM [Order] WHERE CreatedTime > DATEADD(DAY, @NumDate, GETUTCDATE())
-- );
-- DELETE [ChatList] WHERE ChatId IN (
--     SELECT Id
--     FROM [Chat] WHERE OrderId IN (
--     SELECT Id
--     FROM [Order] WHERE CreatedTime > DATEADD(DAY, @NumDate, GETUTCDATE())
-- ));
-- DELETE [Chat] WHERE OrderId IN (
--     SELECT Id
--     FROM [Order] WHERE CreatedTime > DATEADD(DAY, @NumDate, GETUTCDATE())
-- );
-- DELETE [Order] WHERE CreatedTime > DATEADD(DAY, @NumDate, GETUTCDATE())