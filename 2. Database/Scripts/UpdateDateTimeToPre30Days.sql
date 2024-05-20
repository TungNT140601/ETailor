DECLARE @TableName NVARCHAR(128)
DECLARE @ColumnName NVARCHAR(128)
DECLARE @SQL NVARCHAR(MAX)

DECLARE TableCursor CURSOR FOR
SELECT c.TABLE_NAME, c.COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS c
JOIN INFORMATION_SCHEMA.TABLES t ON c.TABLE_NAME = t.TABLE_NAME
WHERE c.DATA_TYPE = 'datetime' AND t.TABLE_TYPE = 'BASE TABLE'

EXECUTE [dbo].[CheckNumOfDateToFinish];

OPEN TableCursor

FETCH NEXT FROM TableCursor INTO @TableName, @ColumnName
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SQL = 'UPDATE ' + QUOTENAME(@TableName) + 
               ' SET ' + QUOTENAME(@ColumnName) + ' = DATEADD(MONTH, -2, ' +  + QUOTENAME(@ColumnName) + ')' +
               ' WHERE ' + QUOTENAME(@ColumnName) + ' IS NOT NULL' +
               ' AND MONTH(' + QUOTENAME(@ColumnName) + ') = 5;'
    PRINT @SQL;

    EXEC sp_executesql @SQL;

    FETCH NEXT FROM TableCursor INTO @TableName, @ColumnName
END

CLOSE TableCursor
DEALLOCATE TableCursor

