-- # Nested Content SQL scripts

-- ## Renaming a DocType alias

UPDATE
	cmsPropertyData
SET
	dataNtext = CAST(REPLACE(CAST(dataNtext AS nvarchar(max)), '"ncContentTypeAlias":"OLD_ALIAS"', '"ncContentTypeAlias":"NEW_ALIAS"') AS ntext)
WHERE
	dataNtext LIKE '%"ncContentTypeAlias":"OLD_ALIAS"%'
;

UPDATE
	cmsDataTypePreValues
SET
	[value] = CAST(REPLACE(CAST([value] AS nvarchar(max)), '"ncAlias": "OLD_ALIAS"', '"ncAlias": "NEW_ALIAS"') AS ntext)
WHERE
	[value] LIKE '%"ncAlias": "OLD_ALIAS"%'
;

-- ## Renaming a property alias

UPDATE
	cmsPropertyData
SET
	dataNtext = CAST(REPLACE(CAST(dataNtext AS nvarchar(max)), ',"OLD_ALIAS":', ',"NEW_ALIAS":') AS ntext)
WHERE
	dataNtext LIKE '%"ncContentTypeAlias":"CONTENT_TYPE_ALIAS"%'
;
