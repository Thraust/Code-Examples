--
-- BEGIN OBJECT: hist.hist_<TABLE>
--

PRINT '<< OBJECT:  hist.hist_<TABLE> >>' 
GO

IF OBJECT_ID('hist.hist_<TABLE>') IS NOT NULL
BEGIN
	DROP TABLE hist.hist_<TABLE>
END

IF OBJECT_ID('hist_<TABLE>') IS NOT NULL
BEGIN
	DROP SYNONYM hist_<TABLE>
END

CREATE TABLE hist.hist_<TABLE>(
unique_tab_id	VARCHAR(50)		NULL,
unique_hist_id	NUMERIC(18,0)	IDENTITY NOT NULL,
changed_object	VARCHAR(100)	NULL,
changed_from	VARCHAR(255)	NULL,
changed_to		VARCHAR(255)	NULL,
changed_by		VARCHAR(50)		NULL,
changed_on		DATETIME		DEFAULT GETDATE() NOT NULL,
	PRIMARY KEY(unique_hist_id)
)
GO

GRANT SELECT ON hist.hist_<TABLE> TO aa_admin_group
GO
GRANT UPDATE ON hist.hist_<TABLE> TO aa_admin_group
GO
GRANT DELETE ON hist.hist_<TABLE> TO aa_admin_group
GO
GRANT INSERT ON hist.hist_<TABLE> TO aa_admin_group
GO
GRANT SELECT ON hist.hist_<TABLE> TO aa_sentry_group
GO

CREATE SYNONYM hist_<TABLE> for hist.hist_<TABLE>
GO
GRANT SELECT ON hist_<TABLE> TO aa_admin_group
GO
GRANT UPDATE ON hist_<TABLE> TO aa_admin_group
GO
GRANT DELETE ON hist_<TABLE> TO aa_admin_group
GO
GRANT INSERT ON hist_<TABLE> TO aa_admin_group
GO
GRANT SELECT ON hist_<TABLE> TO aa_sentry_group
GO

--
-- END OBJECT: hist.hist_<TABLE>
--
