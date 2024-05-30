DECLARE @TableName NVARCHAR(128)
DECLARE @ColumnName NVARCHAR(128)
DECLARE @SQL NVARCHAR(MAX)

DECLARE TableCursor CURSOR FOR
SELECT c.TABLE_NAME, c.COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS c
INNER JOIN INFORMATION_SCHEMA.TABLES t ON c.TABLE_NAME = t.TABLE_NAME
WHERE (c.DATA_TYPE = 'datetime' OR c.DATA_TYPE = 'datetime2') AND t.TABLE_TYPE = 'BASE TABLE' AND c.TABLE_SCHEMA = 'dbo'
AND (c.TABLE_NAME = 'Order' OR c.TABLE_NAME = 'Product' 
OR c.TABLE_NAME = 'ProductStage')

EXECUTE [dbo].[CheckNumOfDateToFinish];

OPEN TableCursor

FETCH NEXT FROM TableCursor INTO @TableName, @ColumnName
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = 'UPDATE ' + QUOTENAME(@TableName) + 
               ' SET ' + QUOTENAME(@ColumnName) + ' = DATEADD(DAY, -30, ' +  + QUOTENAME(@ColumnName) + ')' +
               ' WHERE ' + QUOTENAME(@ColumnName) + ' IS NOT NULL;'
               + '--'+ QUOTENAME(@ColumnName) +' > DATEADD(HOUR, 7, GETUTCDATE());'
    PRINT @SQL;

    EXEC sp_executesql @SQL;

    FETCH NEXT FROM TableCursor INTO @TableName, @ColumnName
END

CLOSE TableCursor
DEALLOCATE TableCursor

