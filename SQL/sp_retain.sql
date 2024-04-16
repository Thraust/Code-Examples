------------------------------------------------------------
-- Stored Procedure to retain data for migrations		  --
-- Created By: William Reinhardt						  --
-- Created On: 10/27/15									  --
------------------------------------------------------------

IF OBJECT_ID('sp_retain') IS NOT NULL
	BEGIN
		DROP PROCEDURE dbo.sp_retain
	END
GO

CREATE PROCEDURE dbo.sp_retain
@preserve	VARCHAR(255) = NULL,
@dr			char(1) = 'N'
AS
BEGIN
	Declare @table VARCHAR(25),
	@createtable NVARCHAR(max),
	@columns VARCHAR(MAX),
	@sqlstring NVARCHAR(MAX),
	@deletestring NVARCHAR(MAX),
	@insertc VARCHAR(MAX),
	@insertv VARCHAR(MAX),
	@startinsert VARCHAR(MAX),
	@count int,
	@ParmDefinition NVARCHAR(500),
	@dropstring	NVARCHAR(MAX),	
	@sqlstringdeux	NVARCHAR(MAX),
	@countstring	NVARCHAR(MAX),
	@before INT,
	@after INT

	-- Create Tables where they do not exist
	IF @preserve IS NOT NULL
	BEGIN
		SET NOCOUNT ON
		
		IF @preserve = 'CDD'
			DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name like 'bsi_%' or table_name IN ('us_rolesec_dtl','us_secobj_mstr')
				)
		ELSE IF @preserve = 'DEFAULTS'
			DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name IN ('ifas_data','ifas_output_dtl','ifas_output_mstr')
				)
		ELSE IF @preserve = 'DS'
			DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name IN ('ds_atno_mstr','ds_clno_mstr','ds_link_dtl')
				)
		ELSE IF @preserve = 'RC'
			DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name like 'rc%'
				)
		ELSE IF @preserve = 'WFM'
			DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name = 'wf_model'
				)
		ELSE IF @preserve = 'WFG'
			DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name IN ('us_role_dtl','us_role_mstr','us_role_output')
				)
		ELSE IF @preserve = 'USERS'
			DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name IN ('us_audit_dtl','us_jlog_dtl','us_pw_dtl','us_ulink_dtl','us_usno_mstr','usa_assoc_dtl')
				)
		ELSE IF @preserve = 'CODES'
				DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name = 'cd_codes_mstr'
				)
		ELSE IF @preserve = 'AR'
				DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name IN ('ar_div_mstr','ar_type_mstr','ar_payt_mstr','ar_stat_mstr','ar_custtype_mstr','ar_cust_mstr','arat_link_dtl','pe_name_mstr')
				)
		ELSE IF @preserve = 'QS'
				DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name like 'qs%' or table_name IN ('nq_next_mstr','nq_jcl_dtl')
				)
		ELSE IF @preserve = 'SPFM'
				DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name like 'oh_spfm%'
				)
		ELSE IF @preserve = 'TEMPLATE'
				DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name IN ('oh_template_dtl','oh_template_mstr')
				)
		ELSE IF @preserve = 'LOGGING'
				DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name LIKE 'btlog_%'
				)
		ELSE IF @preserve = 'XL'
				DECLARE table_cur CURSOR FOR
				(
					SELECT DISTINCT table_name FROM OLD_FINANCE.information_schema.columns WHERE table_name LIKE '%_X' OR (table_name LIKE '%_L' and table_name not like '%ol' and table_name not like '%tl' and table_name not like '%bill' and table_name not like '%trvl' and table_name not like '%proposal' and table_name not like '%el' and table_name not like '%il' and table_name not like '%ll' and table_name not like '%bl' and table_name not like '%al' and table_name not like '%pl')
				)
		ELSE
			BEGIN
				PRINT @preserve + ' is not a valid option, please correct this before running again.'
				RETURN
			END
		OPEN table_cur
		FETCH NEXT FROM table_cur INTO @table
		WHILE @@fetch_status=0
			BEGIN
				SET @ParmDefinition = N'@countOUT int OUTPUT';
				SET @sqlstring = 'SELECT @countOUT = COUNT(*) FROM data_replaced..sysobjects where name = ''' + @table + ''''
				EXEC sp_executesql @sqlstring,@ParmDefinition,@countOUT=@count OUTPUT

				IF OBJECT_ID(@table) IS NULL OR (@count = 0 and @dr = 'Y')
				BEGIN
					SELECT @createtable = 'create table [dbo].[' + @table + ']('

					Declare column_cur cursor for
						(
							SELECT '[' + column_name + '] [' + data_type + ']' + 
								CASE  WHEN data_type = 'char' THEN '(' + CAST(character_maximum_length AS VARCHAR(50)) + ')'
									  WHEN data_type = 'varchar' THEN '(' + CAST(character_maximum_length AS VARCHAR(50)) + ')'
									  WHEN data_type in('decimal','numeric') THEN '(' + CAST(numeric_precision AS VARCHAR(50)) + ',' + CAST(numeric_scale AS VARCHAR(50)) + ')'
									  ELSE ' '
								END  + 
								CASE WHEN is_nullable = 'YES' THEN ' NULL'
									 ELSE ' NOT NULL'
								END + ','
							FROM OLD_FINANCE.information_schema.columns WHERE table_name = @table
						)

					OPEN column_cur
					FETCH NEXT FROM column_cur INTO @columns
					WHILE @@fetch_status=0
						BEGIN
							SET @createtable = @createtable + REPLACE(@columns,'[unique_id] [int]  NOT NULL','[unique_id] [int] IDENTITY(1,1) NOT NULL')
							FETCH NEXT FROM column_cur INTO @columns
						END -- end while column_cur
					CLOSE column_cur
					DEALLOCATE column_cur
				END -- end if table is null

				SET @createtable = substring(@createtable,1,LEN(@createtable)-1) + ')'

				-- Creates table in Production_finance
				IF OBJECT_ID(@table) IS NULL
				exec sp_executesql @createtable
				
				IF @dr = 'Y'
				BEGIN
					IF @count=0
					BEGIN
						set @createtable = REPLACE(@createtable,' table ',' table [data_replaced].')
						exec sp_executesql @createtable
					END
				END

				FETCH NEXT FROM table_cur into @table
			END -- end while table_cur
		CLOSE table_cur
		--DEALLOCATE table_cur
	END -- end create tables

	-- Retain Data
	IF @preserve IS NOT NULL
	BEGIN
		SET @ParmDefinition = N'@countOUT int OUTPUT';

		PRINT CASE WHEN @preserve = 'CDD' THEN '**Retain CDD Report**' + N''
				   WHEN @preserve = 'DEFAULTS' THEN '**Retain Defaults Report**' + N''
				   WHEN @preserve = 'DS' THEN '**Retain DS Tables Report**' + N''
				   WHEN @preserve = 'RC' THEN '**Retain Recurring Calculations Report**' + N''
				   WHEN @preserve = 'WFM' THEN '**Retain Workflow Models Report**' + N''
				   WHEN @preserve = 'WFG' THEN '**Retain Workflow Groups Report**' + N''
				   WHEN @preserve = 'USERS' THEN '**Retain Users, Roles and Security Report**' + N''
				   WHEN @preserve = 'CODES' THEN '**Retain Common Codes Report**' + N''
				   WHEN @preserve = 'AR' THEN '**Retain Accounts Receivable\Cash Receipts Report**' + N''
				   WHEN @preserve = 'QS' THEN '**Retain Menus\Questions Report**' + N''
				   WHEN @preserve = 'SPFM' THEN '**Retain SPFM Tables Report**' + N''
				   WHEN @preserve = 'TEMPLATE' THEN '**Retain Templates Report**' + N''
				   ELSE ''
			 END
		PRINT '*****************************************'

		OPEN table_cur
		FETCH NEXT FROM table_cur INTO @table
		WHILE @@fetch_status=0
			BEGIN
				
				SET @insertc = ''
				SET @insertv = ''
				SELECT @startinsert = 'INSERT INTO [dbo].[' + @table + ']('

				Declare column_cur cursor for
					(
						SELECT '[' + column_name + '],' FROM information_schema.columns WHERE table_name = @table and column_name != 'unique_id'
					)
				OPEN column_cur
				FETCH NEXT FROM column_cur INTO @columns
				WHILE @@fetch_status=0
					BEGIN
						SET @insertc = @insertc + @columns
						SET @insertv = @insertv + @columns
						
						FETCH NEXT FROM column_cur INTO @columns
					END -- end column_cur
				CLOSE column_cur
				DEALLOCATE column_cur
				
				SET @insertc = substring(@insertc,1,LEN(@insertc)-1) + ')'
				SET @insertv = substring(@insertv,1,LEN(@insertv)-1)

				SET @sqlstring = @startinsert + @insertc + ' SELECT ' + REPLACE(@insertv, ')', '') + ' FROM old_finance.dbo.' + @table + CASE WHEN @preserve = 'CODES' then ' where cd_category!=''SYNO''' 
																																					 WHEN @preserve = 'AR' THEN CASE WHEN @table = 'ar_custtype_mstr' THEN  ' where ar_cust_id = ''ZZBLANK'''
																																													 WHEN @table = 'ar_cust_mstr' THEN ' where ara_cust_id = ''ZZBLANK'''
																																													 WHEN @table = 'arat_link_dtl' THEN ' where arat_cust_id = ''ZZBLANK'''
																																													 WHEN @table = 'pe_name_mstr' THEN ' where exists(select 1 from ' + DB_NAME() + '.dbo.ar_cust_mstr a where old_finance.dbo.pe_name_mstr.pe_id=a.ara_cust_id) and not exists(select 1 from ' + DB_NAME() + '.dbo.pe_name_mstr p where old_finance.dbo.pe_name_mstr.pe_id=p.pe_id)'

																																													 ELSE ''
																																												END
																																					ELSE '' END
				-- Deletes Current Records
				SELECT @deletestring = 'delete from dbo.' + @table + CASE WHEN @preserve = 'CODES' then ' where cd_category!=''SYNO''' 
																	   WHEN @preserve = 'AR' THEN CASE WHEN @table = 'ar_custtype_mstr' THEN  ' where ar_cust_id = ''ZZBLANK'''
																									   WHEN @table = 'ar_cust_mstr' THEN ' where ara_cust_id = ''ZZBLANK'''
																									   WHEN @table = 'arat_link_dtl' THEN ' where arat_cust_id = ''ZZBLANK'''
																									   ELSE ''
																								  END
																	ELSE '' END
				IF @preserve='AR' and @table='pe_name_mstr'
					set @sqlstring = ''
				
				-- Detailing what was done
				SET @count = 0
				SET @after = 0
				SET @before = 0
				PRINT 'Retaining ' + @table + ' on ' + CAST(GETDATE() AS VARCHAR(255))
				PRINT '----------------------------------------------------'

				IF @dr='Y'
				GOTO DRS -- SKIP UNIL AFTER data_replaced IS POPULATED

				SET @countstring = 'SELECT @countOUT = COUNT(*) FROM ' + DB_NAME() + '.dbo.' + @table
				EXEC sp_executesql @countstring,@ParmDefinition,@countOUT=@count OUTPUT
				SET @before = @count
				
				-- Removes Records for retain
				EXEC sp_executesql @deletestring

				-- Detailing what was done
				EXEC sp_executesql @countstring,@ParmDefinition,@countOUT=@count OUTPUT
				SET @after = (@before - @count)
				PRINT '# of records in ' + DB_NAME() + '.dbo.' + @table + ' = ' + CAST(@before AS VARCHAR(255))
				PRINT '# of records Removed = ' + CAST(@after AS VARCHAR(255))
				
				-- Retains Records
				EXEC sp_executesql @sqlstring 
				--print @sqlstring
				-- Detailing what was done
				SET @before = @count
				EXEC sp_executesql @countstring,@ParmDefinition,@countOUT=@count OUTPUT
				SET @count = (@count - @before)
				PRINT '# of records Retained from old_finance.dbo.' + @table + ' = ' + CAST(@count AS VARCHAR(255))
				PRINT ''

				DRS:
				-- Moving records to data_replaced database if no table or data is found
				IF @dr='Y'
				BEGIN
					SET @count = 0
					SET @countstring = 'SELECT @countOUT = COUNT(*) FROM data_replaced.dbo.' + @table
					EXEC sp_executesql @countstring,@ParmDefinition,@countOUT=@count OUTPUT

					IF @count=0
						BEGIN
							IF @table != 'pe_name_mstr'
							BEGIN
								SET @sqlstringdeux = REPLACE(@startinsert,'INTO [dbo]','INTO [data_replaced].[dbo]') + @insertc + ' SELECT ' + REPLACE(@insertv, ')', '') + ' FROM [' + DB_NAME() + '].[dbo].[' + @table + ']'
								EXEC sp_executesql @sqlstringdeux

								-- Detailing what was done
								SET @countstring = 'SELECT @countOUT = COUNT(*) FROM data_replaced.dbo.' + @table
								EXEC sp_executesql @countstring,@ParmDefinition,@countOUT=@count OUTPUT
								PRINT '# of records copied to data_replaced.dbo.' + @table + ' = ' + CAST(@count AS VARCHAR(255))
							END
						END
					ELSE
					BEGIN
						PRINT 'Data already exists in data_replaced.dbo.' + @table
						PRINT 'No records copied'
						PRINT ''
					END

					SET @countstring = 'SELECT @countOUT = COUNT(*) FROM ' + DB_NAME() + '.dbo.' + @table
					EXEC sp_executesql @countstring,@ParmDefinition,@countOUT=@count OUTPUT
					SET @before = @count

					-- Removes Records for retain
					EXEC sp_executesql @deletestring

					-- Detailing what was done
					EXEC sp_executesql @countstring,@ParmDefinition,@countOUT=@count OUTPUT
					SET @after = (@before - @count)
					PRINT '# of records in ' + DB_NAME() + '.dbo.' + @table + ' = ' + CAST(@before AS VARCHAR(255))
					PRINT '# of records Removed = ' + CAST(@after AS VARCHAR(255))

					-- Retains Records
					EXEC sp_executesql @sqlstring 

					-- Detailing what was done
					SET @before = @count
					EXEC sp_executesql @countstring,@ParmDefinition,@countOUT=@count OUTPUT
					SET @count = (@count - @before)
					PRINT '# of records Retained from old_finance.dbo.' + @table + ' = ' + CAST(@count AS VARCHAR(255))
					PRINT ''
				END

				FETCH NEXT FROM table_cur INTO @table
			END -- end while table_cur
		CLOSE table_cur
		DEALLOCATE table_cur
	END -- end Retain Data
END
GO

-----------------------
-- End sp_retain.sql --
-----------------------