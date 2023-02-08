using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Controllers
{
    [Authorize(Roles = "login")]
    public class AutoController : ApiController
    {
        public string getipaddress()
        {
            //var host = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (var ip in host.AddressList)
            //{
            //    if (ip.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        return ip.ToString();
            //    }
            //}
            //return "localhost";
            return HttpContext.Current.Request.UserHostAddress;
        }
        public static string Base64Decode([System.Web.Mvc.Bind(Include = "")] string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(Uri.UnescapeDataString(Uri.EscapeDataString(base64EncodedData)));
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        [HttpGet]
        public async Task<HttpResponseMessage> Get_TableDes([System.Web.Mvc.Bind(Include = "")] string table, [System.Web.Mvc.Bind(Include = "")] string db_name)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                    string sqlCommand = @" 
                    SELECT pk_table.name  AS PKTABLE_NAME, pk_column.name AS PKCOLUMN_NAME,fk_table.name  AS FKTABLE_NAME,fk_column.name AS FKCOLUMN_NAME
                    FROM sys.foreign_key_columns fkc
                    INNER JOIN sys.objects fk_obj ON fk_obj.object_id = fkc.constraint_object_id
                    INNER JOIN sys.tables fk_table ON fk_table.object_id = fkc.parent_object_id
                    INNER JOIN sys.schemas fk_schema ON fk_table.schema_id = fk_schema.schema_id
                    INNER JOIN sys.columns fk_column ON fk_column.column_id = parent_column_id AND fk_column.object_id = fk_table.object_id
                    INNER JOIN sys.tables pk_table ON pk_table.object_id = fkc.referenced_object_id
                    INNER JOIN sys.schemas pk_schema ON pk_table.schema_id = pk_schema.schema_id
                    INNER JOIN sys.columns pk_column ON pk_column.column_id = fkc.referenced_column_id AND pk_column.object_id = pk_table.object_id
                    Where fk_table.name='" + table + @"'
                    ";
                    sqlCommand += " Select * from sys_table_des where db_name=N'" + db_name + "' and table_name=N'" + table + "'";
                    var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, CommandType.Text, sqlCommand).Tables);
                    var tables = await task;
                    await db.SaveChangesAsync();
                    string JSONresult = JsonConvert.SerializeObject(tables);
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Auto/List_Tables", ip, tid, "Lỗi khi hiển thị danh sách table", 0, "Table");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Auto/List_Tables", ip, tid, "Lỗi khi hiển thị danh sách table", 0, "Table");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> List_Tables([System.Web.Mvc.Bind(Include = "")] string connect)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    connectString con = JsonConvert.DeserializeObject<connectString>(Base64Decode(connect));
                    string Connection = string.Format("Data Source={0};Initial Catalog={1};user id={2};password={3}", con.DataSource, con.InitialCatalog, con.UserId, con.Password);
                    string sqlCommand = "	SELECT *, CAST((CASE When Exists (Select col_id from sys_tables tb where table_name=TABLE_NAME and db_name=TABLE_CATALOG) then 1 else 0 end) as bit)as IsEdit FROM information_schema.tables where TABLE_CATALOG ='" + con.InitialCatalog + "' and TABLE_TYPE = 'BASE TABLE' order by TABLE_NAME";
                    var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, CommandType.Text, sqlCommand).Tables);
                    var tables = await task;
                    await db.SaveChangesAsync();
                    string JSONresult = JsonConvert.SerializeObject(tables);
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Auto/List_Tables", ip, tid, "Lỗi khi hiển thị danh sách table", 0, "Table");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Auto/List_Tables", ip, tid, "Lỗi khi hiển thị danh sách table", 0, "Table");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> List_Columns([System.Web.Mvc.Bind(Include = "")] string table, [System.Web.Mvc.Bind(Include = "")] string connect)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    connectString con = JsonConvert.DeserializeObject<connectString>(Base64Decode(connect));
                    string Connection = string.Format("Data Source={0};Initial Catalog={1};user id={2};password={3}", con.DataSource, con.InitialCatalog, con.UserId, con.Password);
                    table = table.Replace("--", "").Replace("\r", " ");
                    table = Regex.Replace(table, @"\s+", " ").Trim();
                    table = Regex.Replace(table, "drop", "");
                    table = Regex.Replace(table, "update", "");
                    table = Regex.Replace(table, "delete", "");
                    string sqlCommand = @"
                        SELECT 
                        ISNULL(col_id,-1) as col_id,
                        ISNULL(IsIdentity,COLUMNPROPERTY(object_id(TABLE_SCHEMA+'.'+TABLE_NAME), COLUMN_NAME, 'IsIdentity')) as IsIdentity
                        ,TABLE_CATALOG as db_name
                        ,TABLE_NAME as table_name
                        ,ISNULL(COLUMN_NAME,[Name]) as [Name]
                        ,IsNULL(Title,COLUMN_NAME) as Title
                        ,[Des]
                        ,IsNULL(DATA_TYPE,CType) as CType
                        ,IsNULL(CHARACTER_MAXIMUM_LENGTH,CLength) as CLength
                        ,ISNULL([IsNull],CASE When IS_NULLABLE='NO' then 0 else 1 end)IsNull
                        ,Required
                        ,IsKey
                        ,IsValue
                        ,Show
                        ,ShowForm
                        ,Search
                        ,Input
                        ,ReTable
                        ,ReCol
                        ,Css
                        ,ISNULL(STT,ORDINAL_POSITION)STT
                        ,created_by
                        ,created_date
                        FROM INFORMATION_SCHEMA.COLUMNS   
                        left join sys_tables tb on tb.table_name=TABLE_NAME and tb.db_name=TABLE_CATALOG and tb.Name=COLUMN_NAME
                        WHERE TABLE_CATALOG='" + con.InitialCatalog; 
                    sqlCommand += Environment.NewLine + @"' And TABLE_NAME = N'" + table + @"'";

                    var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, CommandType.Text, sqlCommand).Tables);
                    var tables = await task;
                    await db.SaveChangesAsync();
                    string JSONresult = JsonConvert.SerializeObject(tables);
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Auto/List_Tables", ip, tid, "Lỗi khi hiển thị danh sách table", 0, "Table");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Auto/List_Tables", ip, tid, "Lỗi khi hiển thị danh sách table", 0, "Table");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Add_Tables([System.Web.Mvc.Bind(Include = "")][FromBody] List<sys_tables> tables)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Auto";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var tbs = tables.Where(a => a.col_id == -1).ToList();
                    var tbes = tables.Where(a => a.col_id != -1).ToList();
                    foreach (var tb in tbes)
                    {
                        tb.created_by = uid;
                        tb.created_date = DateTime.Now;
                        db.Entry(tb).State = EntityState.Modified;
                    }
                    foreach (var tb in tbs)
                    {
                        tb.created_by = uid;
                        tb.created_date = DateTime.Now;
                    }
                    if (tbs.Count > 0)
                        db.sys_tables.AddRange(tbs);
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Auto/Add_Tables", ip, tid, "Lỗi khi thêm table", 0, "Table");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Auto/Add_Tables", ip, tid, "Lỗi khi thêm table", 0, "Table");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_TableDes([System.Web.Mvc.Bind(Include = "id,created_by,created_date")][FromBody] sys_table_des table)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    table.created_by = uid;
                    table.created_date = DateTime.Now;
                    if (table.id != -1)
                    {
                        db.Entry(table).State = EntityState.Modified;
                    }
                    else
                    {
                        db.sys_table_des.Add(table);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> DelColumn([System.Web.Mvc.Bind(Include = "")] int id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var cl = await db.sys_tables.FindAsync(id);
                    if (cl != null)
                    {
                        db.sys_tables.Remove(cl);
                        await db.SaveChangesAsync();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1" });
            }
        }


        #region Excel
        [HttpPost]
        public async Task<HttpResponseMessage> ImportExcel()
        {
            string ListErr = "";
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Table";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            string fileName = "";
                            if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                            {
                                fileName = Guid.NewGuid().ToString();
                            }
                            fileName = fileData.Headers.ContentDisposition.FileName;
                            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                            {
                                fileName = fileName.Trim('"');
                            }
                            if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                            {
                                fileName = Path.GetFileName(fileName);
                            }
                            if (!fileName.ToLower().Contains(".xls"))
                            {
                                ListErr = "File Excel không đúng định dạng! Kiểm tra lại mẫu Import";
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = ListErr });
                            }
                            var newFileName = Path.Combine(root + "/Import", fileName);
                            var fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/Import", fileName);
                            }
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            File.Move(fileData.LocalFileName, newFileName);
                            FileInfo temp = new FileInfo(newFileName);
                            using (ExcelPackage pck = new ExcelPackage(temp))
                            {
                                List<sys_tables> dvs = new List<sys_tables>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                var dvcs = db.sys_tables.Select(a => new { a.col_id, a.table_name, a.db_name, a.des }).ToList();
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    sys_tables dv = new sys_tables();
                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value == null)
                                        {
                                            break;
                                        }
                                        var column = ws.Cells[3, j].Value;
                                        var vl = ws.Cells[i, j].Value;
                                        if (column != null && vl != null)
                                        {
                                            PropertyInfo propertyInfo = db.sys_tables.GetType().GetProperty(column.ToString());
                                            propertyInfo.SetValue(db.sys_tables, Convert.ChangeType(vl,
                                            propertyInfo.PropertyType), null);
                                        }
                                    }
                                    if (dvcs.Count(a => a.col_id == dv.col_id || a.table_name == dv.table_name) > 0)
                                        break;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.sys_tables.AddRange(dvs);
                                    db.SaveChangesAsync();
                                    File.Delete(newFileName);
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Auto/ImportExcel", ip, tid, "Lỗi khi import table", 0, "Table");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Auto/ImportExcel", ip, tid, "Lỗi khi import table", 0, "Table");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

    }
}