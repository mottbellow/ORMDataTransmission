IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('tablespaceinfo') AND type in ('U'))
TRUNCATE TABLE tablespaceinfo;
DROP TABLE tablespaceinfo;
GO

CREATE TABLE tablespaceinfo(
	nameinfo nvarchar(50) NULL,
	rowsinfo int NULL,
	reserved nvarchar(50) NULL,
	datainfo nvarchar(50) NULL,
	index_size nvarchar(50) NULL,
	unused nvarchar(50) NULL
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('get_tableinfo') AND type in ('P', 'PC'))
DROP PROCEDURE get_tableinfo
GO


CREATE PROCEDURE get_tableinfo AS
  
  DECLARE @tablespaceinfo TABLE (   

    nameinfo varchar(50),   

    rowsinfo int,   

    reserved varchar(20),   

    datainfo varchar(20),   

    index_size varchar(20),   

    unused varchar(20)   

)   

 
 
DECLARE @tablename varchar(255);   

 
 
DECLARE Info_cursor CURSOR FOR 

    SELECT name FROM sys.tables WHERE type='U';   

 
 
OPEN Info_cursor   

FETCH NEXT FROM Info_cursor INTO @tablename   

 
 
WHILE @@FETCH_STATUS = 0   

BEGIN 

    insert into @tablespaceinfo exec sp_spaceused @tablename   

    FETCH NEXT FROM Info_cursor   

    INTO @tablename   

END 

 
 
CLOSE Info_cursor   

DEALLOCATE Info_cursor   

TRUNCATE TABLE tablespaceinfo;
INSERT INTO tablespaceinfo (nameinfo, rowsinfo, reserved, datainfo, index_size, unused)
SELECT nameinfo, rowsinfo, reserved, datainfo, index_size, unused FROM @tablespaceinfo   

ORDER BY Cast(Replace(reserved,'KB','') as INT) DESC 
SELECT * FROM tablespaceinfo ORDER BY Cast(Replace(reserved,'KB','') as INT) DESC 
GO

EXEC get_tableinfo;