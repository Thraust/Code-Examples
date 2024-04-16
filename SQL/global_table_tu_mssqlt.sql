--
-- BEGIN TRIGGER: <SCHEMA>.<TABLE>_tu
--

PRINT '<< TRIGGER:  <SCHEMA>.<TABLE>_tu >>' 
GO

IF OBJECT_ID('<TABLE>_tu') IS NOT NULL
BEGIN
	DROP TRIGGER <TABLE>_tu
END
GO

CREATE TRIGGER <TABLE>_tu
ON <SCHEMA>.<TABLE>
FOR UPDATE AS
BEGIN

----------------------------
-- MODIFY USER/DATE SECTION
----------------------------

--<MOD>		IF NOT UPDATE(modify_user) update <TABLE> set modify_user = 'TU_TRIGGER' from inserted where inserted.<TID>=<TABLE>.<TID>
--<MOD>		IF NOT UPDATE(modify_date) update <TABLE> set modify_date = GETDATE() from inserted where inserted.<TID>=<TABLE>.<TID>

----------------------------
-- HISTORY TRACKING SECTION
----------------------------

	IF (SELECT count(history_field) from aim.history_objects where history_table = '<TABLE>' and history_enabled = 'Y') > 0
	BEGIN
		-- Temporary tables for creating the insert statement
		SELECT <COLUMNS> INTO #deleted FROM deleted
		SELECT <COLUMNS> INTO #inserted FROM inserted
		
		DECLARE @field			NVARCHAR(255),	-- History Tracking column name
				@sql_statement	NVARCHAR(MAX),	-- Executable variable used for counting rows and creating insert statements
				@count			INT				-- Used to determine if there are rows to record in hist_<TABLE>
		
		-- Cursor for object(s) being tracked on the table
		DECLARE hist_obj_cursor CURSOR FOR
		SELECT history_field 
		FROM aim.history_objects 
		WHERE history_table = '<TABLE>'
		AND history_enabled = 'Y'

		OPEN hist_obj_cursor
		FETCH NEXT FROM hist_obj_cursor
		INTO @field

		-- Loop through fields set to enable in the history_objects table
		-- If changes have been made, a record will be inserted into hist.hist_<TABLE> 
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @sql_statement = N'SELECT @count = COUNT(ISNULL(i.'+@field+',1)) FROM #inserted i INNER JOIN #deleted d ON i.<TID>=d.<TID> 
									WHERE ISNULL(CAST(i.'+@field+' AS VARCHAR(255)),''NULL_VALUE'') != ISNULL(CAST(d.'+@field+' AS VARCHAR(255)),''NULL_VALUE'')'
			EXEC sp_executesql @sql_statement, N'@count INT OUTPUT', @count OUTPUT

			-- Check to see if the column has changed, if it has then an insert will be created and executed
			IF (@count > 0)
			BEGIN
				SET @sql_statement = N'INSERT INTO hist.hist_<TABLE>(unique_tab_id,changed_object,changed_from,changed_to,changed_by,changed_on)
						SELECT h.<TID>,'''+@field+''',CAST(h.'+@field+' AS VARCHAR(255)),CAST(i.'+@field+' AS VARCHAR(255)),ISNULL(i.modify_user,''TU_TRIGGER''),GETDATE() 
						FROM #deleted h INNER JOIN #inserted i ON h.<TID>=i.<TID>'
				EXEC(@sql_statement)
			END /* end if */
			
			FETCH NEXT FROM hist_obj_cursor
			INTO @field
		END /* end while */

		CLOSE hist_obj_cursor
		DEALLOCATE hist_obj_cursor
	END /* end if */
END /* end trigger */
GO

--
-- END TRIGGER: <TABLE>_tu
--
