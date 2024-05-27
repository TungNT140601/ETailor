UPDATE ProductStage SET [Status] = 4, StartTime = DATEADD(HOUR, 7, GETUTCDATE()), FinishTime = DATEADD(DAY, 3, DATEADD(HOUR, 7, GETUTCDATE())) WHERE [Status] = 1;

UPDATE Product SET [Status] = 5, FinishTime = DATEADD(DAY, 3, DATEADD(HOUR, 7, GETUTCDATE())) WHERE [Status] = 1;

UPDATE [Order] SET [Status] = 5, FinishTime = DATEADD(DAY, 3, DATEADD(HOUR, 7, GETUTCDATE())) WHERE [Status] = 3;
-- UPDATE [Order] SET [Status] = 8, FinishTime = DATEADD(DAY, 3, DATEADD(HOUR, 7, GETUTCDATE())) WHERE [Status] = 5;

