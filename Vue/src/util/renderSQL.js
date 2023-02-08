const renderSQL = (table, columns) => {
    console.log(table);
    console.log(columns);
    let primarykey = columns[0];
    let arrwhereSearch = [];
    let arrExports = [];
    let arrselectColumn = [];
    let selectColumn = "*";
    columns.filter(x => x.Search).forEach(cl => {
        arrwhereSearch.push(`${cl.Name} like N'%'+@s+'%'`);
    });
    columns.forEach(cl => {
        arrExports.push(`m.${cl.Name} as [${cl.Name}|${cl.Title}|20|C]`);
        arrselectColumn.push(`${cl.Name}`);
    });
    selectColumn = arrselectColumn.join(",");
    let whereSearch = (arrwhereSearch.length > 1 ? '(' : '') + arrwhereSearch.join(" or ") + (arrwhereSearch.length > 1 ? ')' : '');
    let sql = `
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[${table.TABLE_NAME}_Get]
@${primarykey.Name} varchar(50)
AS
Select * from ${table.TABLE_NAME} where ${primarykey.Name}=@${primarykey.Name}
GO
/****** Object:  StoredProcedure [dbo].[${table.TABLE_NAME}_List]    Script Date: 20/04/2022 3:37:15 CH ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[${table.TABLE_NAME}_List]
@s nvarchar(250)
AS
Select ${selectColumn} from ${table.TABLE_NAME}
where @s is null or @s='' or (@s is not null and @s<>'' and ${whereSearch} )
order by STT
GO
/****** Object:  StoredProcedure [dbo].[${table.TABLE_NAME}_ListExport]    Script Date: 20/04/2022 3:37:16 CH ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[${table.TABLE_NAME}_ListExport]
@user_id varchar(250)
AS
Select 
${arrExports.join(",\n")}
from ${table.TABLE_NAME} m 
where m.Trangthai=1 
order by m.STT
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[${table.TABLE_NAME}_ListExportMau]
@user_id varchar(50)
AS
Select 
${arrExports.join(",\n")}
from  ${table.TABLE_NAME} m 
where 1=-1 
    `;
    return sql;
}
export default renderSQL;