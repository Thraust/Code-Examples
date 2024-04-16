------------------------------------------------------------
-- Stored Procedure to find non ascii characters		  --
-- Created By: William Reinhardt						  --
-- Created On: 04/18/14									  --
------------------------------------------------------------

-- Check to see IF sp_nonascii already exists, IF it does it will be dropped.
IF OBJECT_ID('sp_nonascii') IS NOT NULL
	BEGIN
		DROP PROCEDURE dbo.sp_nonascii
	END
GO

CREATE PROCEDURE dbo.sp_nonascii
@all		char(1) = 'n',
@table		VARCHAR(50) = 'null', -- Table to run against
@column		VARCHAR(50) = 'null'
AS
BEGIN
	DECLARE @sout		NVARCHAR(MAX),  -- Used to create SELECT statements that show values with non ascii characters
			@rout		NVARCHAR(MAX),	-- Used to create first half of a SELECT statement to see values without non ascii characters
			@rout2		NVARCHAR(MAX),	-- Used to create second half of a SELECT statement to see values without non ascii characters
			@uout		NVARCHAR(MAX),	-- Used to create first half of an UPDATE statement to remove non ascii characters
			@uout2		NVARCHAR(MAX),	-- Used to create second half of an UPDATE statement to remove non ascii characters
			@columns	NVARCHAR(MAX),	-- Used to hold colunm names for cursor
			@count		INT,			-- Used to increment during cursor
			@check		INT,			-- Used to check if there are any non ascii characters in a column
			@tloop		INT,			-- Used to check for declaring table cursor
			@sqlstring	NVARCHAR(MAX)	-- Used to execute check statement and count the number of rows WHERE non ascii characters exist in a column
	DECLARE	@allout		table	(
									tname VARCHAR(50),
									sout NVARCHAR(max)
								)	
	
	-- Create cursor for iterating through column names
	-- Only columns with datatype like 'char' will be selected
	IF (@all = 'y' AND @table = 'null')
		BEGIN
			DECLARE c_tab CURSOR FOR
				(
					SELECT DISTINCT c.TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS c 
						INNER JOIN INFORMATION_SCHEMA.TABLES t on c.TABLE_NAME=t.TABLE_NAME 
						WHERE t.TABLE_TYPE = 'BASE TABLE' AND DATA_TYPE LIKE '%char%'
				)
		END
	ELSE IF (@table != 'null' AND @all = 'n')
		BEGIN
			DECLARE c_cur CURSOR FOR
				(
					SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS c 
						INNER JOIN INFORMATION_SCHEMA.TABLES t on c.TABLE_NAME=t.TABLE_NAME 
						WHERE t.TABLE_TYPE = 'BASE TABLE' AND C.TABLE_NAME = @table 
							AND DATA_TYPE LIKE '%char%' AND c.COLUMN_NAME NOT IN
							(
								SELECT DISTINCT c.COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS c 
									INNER JOIN INFORMATION_SCHEMA.TABLES t on c.TABLE_NAME=t.TABLE_NAME 
									WHERE t.TABLE_TYPE = 'BASE TABLE' AND DATA_TYPE LIKE '%char%' AND c.COLUMN_NAME LIKE '%pw%'
							)
				)
		END
	
	SET @tloop = 0
	
	-- Open cursor 'c_cur' after column names have been selected
	IF (@all = 'n' AND @table != 'null')
		BEGIN
			OPEN c_cur
			FETCH NEXT FROM c_cur INTO @columns
			SET @check = 0
			SET @count = 0
			
			WHILE (@@FETCH_STATUS = 0)
				BEGIN
					-- This is used to check if there are non ascii characters in a given column
					-- If records are selected from within the executed sqlstring, @check will recieve the number of records affected
					SET @sqlstring = 'SELECT @check = COUNT([' + @columns + ']) FROM ' + @table + ' WHERE [' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL'
					EXEC sp_EXECutesql @sqlstring, N'@check INT OUTPUT', @check OUTPUT
				
				IF (@check > 0)
					BEGIN
						IF (@count = 0)
							BEGIN
								-- The first half of statements get created here
								SET @sout = 'SELECT * FROM ' + @table + ' WHERE [' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL'
								SET @rout = 'SELECT REPLACE([' + @columns + '],SUBSTRING([' + @columns + '],PATINDEX(''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN,[' + @columns + ']),1),'''') AS ' + '''' + @columns + ''''
								SET @rout2 = ' FROM ' + @table + ' WHERE [' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL'
								SET @uout = 'UPDATE ' + @table + ' SET [' + @columns + '] = REPLACE([' + @columns + '],SUBSTRING([' + @columns + '],PATINDEX(''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN,[' + @columns + ']),1),'''')' 
								SET @uout2 = ' WHERE [' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL'
							END
						ELSE
							BEGIN
								-- The second half of statements get created here
								SET @sout = @sout + ' OR ([' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL)'
								SET @rout = @rout + ', REPLACE([' + @columns + '],SUBSTRING([' + @columns + '],PATINDEX(''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN,[' + @columns + ']),1),'''') AS ' + '''' + @columns + ''''
								SET @rout2 = @rout2 + ' OR ([' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL)'
								SET @uout = @uout + ', [' + @columns + '] = REPLACE([' + @columns + '],SUBSTRING([' + @columns + '],PATINDEX(''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN,[' + @columns + ']),1),'''')'
								SET @uout2 = @uout2 + ' OR ([' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL)'
							END
					-- Increment count after the first column with bad values has been found
						SET @count = 1
					END
					FETCH NEXT FROM c_cur INTO @columns
				END
			
			-- The statements are pieced together
			SET @rout = @rout + @rout2
			SET @uout = @uout + @uout2
			
			-- The completed statements are selected for use by the user
			SELECT @sout as 'SELECT Statements' UNION ALL SELECT @rout
			SELECT @uout as 'UPDATE Statement'
			--EXEC sp_executesql @uout
			
			CLOSE c_cur
			DEALLOCATE c_cur
		END
	ELSE IF (@all = 'y' and @table = 'null')
		BEGIN
			IF (@tloop = 0)
				BEGIN
					OPEN c_tab
					FETCH NEXT FROM c_tab INTO @table
				END
			
			WHILE (@@FETCH_STATUS = 0)
				BEGIN
					DECLARE c_cur CURSOR FOR
					(
						SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS c 
						INNER JOIN INFORMATION_SCHEMA.TABLES t on c.TABLE_NAME=t.TABLE_NAME 
						WHERE t.TABLE_TYPE = 'BASE TABLE' AND C.TABLE_NAME = @table 
							AND DATA_TYPE LIKE '%char%' AND c.COLUMN_NAME NOT IN
							(
								SELECT DISTINCT c.COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS c 
									INNER JOIN INFORMATION_SCHEMA.TABLES t on c.TABLE_NAME=t.TABLE_NAME 
									WHERE t.TABLE_TYPE = 'BASE TABLE' AND DATA_TYPE LIKE '%char%' AND c.COLUMN_NAME LIKE '%pw%'
							)
					)
					
					OPEN c_cur
					FETCH NEXT FROM c_cur INTO @columns
					SET @check = 0
					SET @count = 0
					SET @rout = ''
					SET @rout2 = ''
					
					WHILE (@@FETCH_STATUS = 0)
						BEGIN
							-- This is used to check if there are non ascii characters in a given column
							-- If records are selected from within the executed sqlstring, @check will recieve the number of records affected
							SET @sqlstring = 'SELECT @check = COUNT([' + @columns + ']) FROM ' + @table + ' WHERE [' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL'
							EXEC sp_EXECutesql @sqlstring, N'@check INT OUTPUT', @check OUTPUT
							
							IF (@check > 0)
								BEGIN
									IF (@count = 0)
										BEGIN
											-- The first half of statements get created here
											SET @sout = 'SELECT * FROM ' + @table + ' WHERE [' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL'
											SET @rout = 'SELECT REPLACE([' + @columns + '],SUBSTRING([' + @columns + '],PATINDEX(''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN,[' + @columns + ']),1),'''') AS ' + '''' + @columns + ''''
											SET @rout2 = ' FROM ' + @table + ' WHERE [' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL'
											SET @uout = 'UPDATE ' + @table + ' SET [' + @columns + '] = REPLACE([' + @columns + '],SUBSTRING([' + @columns + '],PATINDEX(''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN,[' + @columns + ']),1),'''')'
											SET @uout2 = ' WHERE [' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL'
										END
									ELSE
										BEGIN
											-- The second half of statements get created here
											SET @sout = @sout + ' OR ([' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL)'
											SET @rout = @rout + ', REPLACE([' + @columns + '],SUBSTRING([' + @columns + '],PATINDEX(''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN,[' + @columns + ']),1),'''') AS ' + '''' + @columns + ''''
											SET @rout2 = @rout2 + ' OR ([' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL)'
											SET @uout = @uout + ', [' + @columns + '] = REPLACE([' + @columns + '],SUBSTRING([' + @columns + '],PATINDEX(''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN,[' + @columns + ']),1),'''')'
											SET @uout2 = @uout2 + ' OR ([' + @columns + '] LIKE ''%[^0-9a-zA-Z<>\/"'''' !-~]%'' COLLATE Latin1_General_BIN AND [' + @columns + '] IS NOT NULL)'
										END
								-- Increment count after the first column with bad values has been found
								SET @count = 1
								END
							SET @check = 0
							FETCH NEXT FROM c_cur INTO @columns
						END
					

					SET @rout = @rout + @rout2
					SET @uout = @uout + @uout2
					
					IF @rout != ''
						BEGIN
							INSERT INTO @allout(tname,sout) VALUES(@table,@sout)
							INSERT INTO @allout(tname,sout) VALUES(@table,@rout)
							EXEC sp_executesql @uout
						END
					
					SET @tloop = 1	
					CLOSE c_cur
					DEALLOCATE c_cur
					FETCH NEXT FROM c_tab INTO @table
				END -- end c_cur loop
			
			CLOSE c_tab
			DEALLOCATE c_tab
			
			SELECT tname as "TABLE", sout as "SELECT STATEMENTS" from @allout
		END -- end c_tab loop
		
END
go