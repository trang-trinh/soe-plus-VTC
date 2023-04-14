using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Helper;
using System.Data.Entity;
using System.IO;
using Org.BouncyCastle.Utilities;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class BackupController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_Form_Backup()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdback = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdback = provider.FormData.GetValues("model").SingleOrDefault();
                        backup_schedule model = JsonConvert.DeserializeObject<backup_schedule>(fdback);

                        var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                        model.backup_id = helper.GenKey();
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        db.backup_schedule.Add(model);
                        db.SaveChanges();

                        #region add sys_logs
                        if (helper.wlog)
                        {
                            helper.saveLog(uid, name,
                                JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(model, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) }),
                                domainurl + "Backup/Add_Form_Backup", ip, tid, "Thêm mới lịch backup dữ liệu", 1, "Backup");
                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdback, contents }), domainurl + "Backup/Add_Form_Backup", ip, tid, "Lỗi khi Thêm mới lịch backup dữ liệu", 0, "Backup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdback, contents }), domainurl + "Backup/Add_Form_Backup", ip, tid, "Lỗi khi Thêm mới lịch backup dữ liệu", 0, "Backup");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Form_Backup()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdback = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdback = provider.FormData.GetValues("model").SingleOrDefault();
                        backup_schedule model = JsonConvert.DeserializeObject<backup_schedule>(fdback);

                        db.Entry(model).State = EntityState.Modified;
                        db.backup_schedule.Add(model);
                        db.SaveChanges();

                        #region add sys_logs
                        if (helper.wlog)
                        {
                            helper.saveLog(uid, name,
                                JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(model, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) }),
                                domainurl + "Backup/Update_Form_Backup", ip, tid, "Cập nhật lịch backup dữ liệu", 1, "Backup");
                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdback, contents }), domainurl + "Backup/Update_Form_Backup", ip, tid, "Lỗi khi Cập nhật lịch backup dữ liệu", 0, "Backup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdback, contents }), domainurl + "Backup/Update_Form_Backup", ip, tid, "Lỗi khi Cập nhật lịch backup dữ liệu", 0, "Backup");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_StatusFormBackup([System.Web.Mvc.Bind(Include = "TextID,BitTrangthai")] Trangthai trangthai)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

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
                    var das = db.backup_schedule.FirstOrDefault(a => a.backup_id == trangthai.TextID);
                    if (das != null)
                    {
                        das.is_active = !trangthai.BitTrangthai;

                        #region add sys_logs
                        if (helper.wlog)
                        {
                            helper.saveLog(uid, name,
                                JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(das, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) }),
                                domainurl + "Backup/Update_StatusFormBackup", ip, tid, "Cập nhật trạng thái lịch backup dữ liệu", 1, "Backup");
                        }
                        #endregion
                        await db.SaveChangesAsync();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.TextID, contents }), domainurl + "Backup/Update_StatusFormBackup", ip, tid, "Lỗi khi cập nhật trạng thái Backup schedule", 0, "Backup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.TextID, contents }), domainurl + "Backup/Update_StatusFormBackup", ip, tid, "Lỗi khi cập nhật trạng thái Backup schedule", 0, "ca_positions");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_FormBackup([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

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
                    var das = await db.backup_schedule.Where(a => id.Contains(a.backup_id)).ToListAsync();

                    if (das != null)
                    {
                        List<backup_schedule> del = new List<backup_schedule>();
                        foreach (var da in das)
                        {
                            var history_backup = db.backup_history.Where(x => x.backup_id == da.backup_id).ToList();
                            if (history_backup.Count() == 0)
                            {
                                del.Add(da);
                            }
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không thể xóa dữ liệu." });
                        }
                        else
                        {
                            #region add sys_logs
                            if (helper.wlog)
                            {
                                helper.saveLog(uid, name,
                                    JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(del, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) }),
                                    domainurl + "Backup/Delete_FormBackup", ip, tid, "Xóa lịch backup dữ liệu", 1, "Backup");
                            }
                            #endregion
                            db.backup_schedule.RemoveRange(del);
                            await db.SaveChangesAsync();
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Backup/Delete_FormBackup", ip, tid, "Lỗi khi xoá lịch backup dữ liệu", 0, "Backup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Backup/Delete_FormBackup", ip, tid, "Lỗi khi xoá lịch backup dữ liệu", 0, "Backup");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Run_Backup()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdback = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdback = provider.FormData.GetValues("model").SingleOrDefault();
                        backup_schedule model = JsonConvert.DeserializeObject<backup_schedule>(fdback);
                        // backup ...                        
                        if (model.type_backup == 0)
                        {
                            // cmd
                            var pathBackup = "C://Program Files/Microsoft SQL Server/MSSQL15.MSSQLSERVER/MSSQL/Backup";
                            //string disk = pathSocket.Substring(0, 1);
                            //string strCmdText = disk + ": & cd " + pathSocket + " & sqlcmd -e -s touch -q \" backup database SOEVTC to disk = '" + pathSocket + "/" + "soevtc_backupdb_" + DateTime.Now.ToString() + ".bak" + "' \" /C";
                            //var proc = System.Diagnostics.Process.Start("CMD.exe", strCmdText);

                            // proc backup
                            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                            var sqlpas = new List<SqlParameter>();
                            if (model.type_path_backup == 0)
                            {
                                sqlpas.Add(new SqlParameter("@path_backup", pathBackup + "/" + "soevtc_backupdb_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".bak"));
                            }
                            else
                            {
                                sqlpas.Add(new SqlParameter("@path_backup", model.folder_backup_path + "/" + "soevtc_backupdb_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".bak"));
                            }
                            var arrpas = sqlpas.ToArray();
                            System.Threading.Tasks.Task.Run(async () =>
                                await System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "backup_db_to_path", arrpas))                                
                            );

                        }
                        else if (model.type_backup == 1)
                        {
                            //var pathBackup = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\api", "") + "vue/backup_nodejs";
                            Regex regex = new Regex(@"\\api", RegexOptions.IgnoreCase);
                            string resultChange = regex.Replace(AppDomain.CurrentDomain.BaseDirectory, "", 1);
                            var pathBackup = resultChange + "vue/backup_nodejs";

                            string disk = pathBackup.Substring(0, 1);
                            var pathEdit = Regex.Replace(pathBackup.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = pathEdit.Split('/');
                            var pathConfig = "";
                            var sttPartPath = 1;
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfig += (item);
                                    }
                                    else
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            //string strCmdText = disk + ": & cd " + pathSocket + " & node backup_file_data.js /C";
                            string strCmdText = "/k " + disk + ": & cd " + pathConfig.Substring(3) + " & node backup_file_data.js /c";
                            var proc = System.Diagnostics.Process.Start("cmd.exe", strCmdText);
                        }
                        else if (model.type_backup == 2)
                        {
                            // Backup database
                            var pathBackup = "C://Program Files/Microsoft SQL Server/MSSQL15.MSSQLSERVER/MSSQL/Backup";
                            // proc backup
                            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                            var sqlpas = new List<SqlParameter>();
                            if (model.type_path_backup == 0)
                            {
                                sqlpas.Add(new SqlParameter("@path_backup", pathBackup + "/" + "soevtc_backupdb_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".bak"));
                            }
                            else
                            {
                                sqlpas.Add(new SqlParameter("@path_backup", model.folder_backup_path + "/" + "soevtc_backupdb_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".bak"));
                            }
                            var arrpas = sqlpas.ToArray();
                            System.Threading.Tasks.Task.Run(async () =>
                                await System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "backup_db_to_path", arrpas))
                            );

                            // Backup file
                            //var pathBackupFile = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\api", "") + "vue/backup_nodejs";
                            Regex regex = new Regex(@"\\api", RegexOptions.IgnoreCase);
                            string resultChange = regex.Replace(AppDomain.CurrentDomain.BaseDirectory, "", 1);
                            var pathBackupFile = resultChange + "vue/backup_nodejs";
                            
                            string disk = pathBackupFile.Substring(0, 1);
                            var pathEdit = Regex.Replace(pathBackupFile.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = pathEdit.Split('/');
                            var pathConfig = "";
                            var sttPartPath = 1;
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfig += (item);
                                    }
                                    else
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            string strCmdText = "/k " + disk + ": & cd " + pathConfig.Substring(3) + " & node backup_file_data.js /c";
                            var proc = System.Diagnostics.Process.Start("cmd.exe", strCmdText);
                        }
                        #region add sys_logs
                        //if (helper.wlog)
                        //{
                        //    helper.saveLog(uid, name,
                        //        JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(model, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) }),
                        //        domainurl + "Backup/Run_Backup", ip, tid, "Thực hiện backup dữ liệu", 1, "Backup");
                        //}
                        #endregion
                        
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdback, contents }), domainurl + "Backup/Run_Backup", ip, tid, "Lỗi khi backup dữ liệu", 0, "Backup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdback, contents }), domainurl + "Backup/Run_Backup", ip, tid, "Lỗi khi backup dữ liệu", 0, "Backup");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        #region CallProc
        [HttpPost]
        public async Task<HttpResponseMessage> getData([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);

            try
            {
                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    var sqlpas = new List<SqlParameter>();
                    if (proc != null && proc.par != null)
                    {
                        foreach (sqlPar p in proc.par)
                        {
                            sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                        }
                    }
                    var arrpas = sqlpas.ToArray();
                    DateTime sdate = DateTime.Now;
                    var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables);
                    var tables = await task;
                    DateTime edate = DateTime.Now;
                    #region add SQLLog
                    if (helper.wlog)
                    {
                        using (DBEntities db = new DBEntities())
                        {
                            sql_log log = new sql_log();
                            log.controller = domainurl + "Backup/getData";
                            log.start_date = sdate;
                            log.end_date = edate;
                            log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                            log.user_id = uid;
                            log.token_id = tid;
                            log.created_ip = ip;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.modified_ip = ip;
                            log.modified_date = DateTime.Now;
                            log.modified_by = uid;
                            log.modified_token_id = tid;
                            log.full_name = name;
                            log.title = proc.proc;
                            log.log_content = JsonConvert.SerializeObject(new { data = proc });
                            db.sql_log.Add(log);
                            await db.SaveChangesAsync();
                        }
                    }
                    #endregion
                    string JSONresult = JsonConvert.SerializeObject(tables);
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Backup/getData", ip, tid, "Lỗi khi gọi proc ", 0, "Backup");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Backup/getData", ip, tid, "Lỗi khi gọi proc ", 0, "Backup");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }
        #endregion
    }
}
