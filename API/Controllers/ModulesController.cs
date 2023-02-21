using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Text.RegularExpressions;

namespace Controllers
{
    [Authorize(Roles = "login")]
    public class ModulesController : ApiController
    {
        void UpdateStatusChild(int? Parent_ID, List<sys_modules> Data, bool Status)
        {
            DBEntities db = new DBEntities();
            Data.ForEach(item =>
            {
                if (item.parent_id == Parent_ID)
                {
                    item.status = Status;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    List<sys_modules> listChild = db.sys_modules.AsNoTracking().Where(x => x.parent_id == item.module_id).ToList();
                    if (listChild.Count() > 0)
                        UpdateStatusChild(item.module_id, listChild, item.status);
                }
            });
        }
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

        [HttpPost]
        public async Task<HttpResponseMessage> Add_Module()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
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

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Module";
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
                     fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                     sys_modules model = JsonConvert.DeserializeObject<sys_modules>(fdmodel);
                     // This illustrates how to get thefile names.
                     FileInfo fileInfo = null;
                     MultipartFileData ffileData = null;
                     string newFileName = "";
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
                         newFileName = Path.Combine(root + "/Module", fileName);
                         fileInfo = new FileInfo(newFileName);
                         if (fileInfo.Exists)
                         {
                             fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                             fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;

                             newFileName = Path.Combine(root + "/Module", fileName);
                         }
                         model.image = "/Portals/Module/" + fileName;
                         ffileData = fileData;
                     }
                     model.modified_date = DateTime.Now;
                     model.modified_by = uid;
                     model.modified_token_id = tid;
                     model.modified_ip = ip;
                     model.created_date = DateTime.Now;
                     model.created_by = uid;
                     model.created_token_id = tid;
                     model.created_ip = ip;
                     db.sys_modules.Add(model);
                     db.SaveChanges();
                     //add role module
                     if (model.parent_id != null)
                     {
                         List<sys_role_modules> list_role_modules = db.sys_role_modules.Where(x => x.module_id == model.parent_id).ToList();
                         if (list_role_modules.Count > 0)
                         {
                             foreach (var item in list_role_modules)
                             {
                                 sys_role_modules role_module = new sys_role_modules();
                                 role_module.role_id = item.role_id;
                                 role_module.user_id = item.user_id;
                                 role_module.module_id = model.module_id;
                                 role_module.module_functions = item.module_functions;
                                 role_module.is_level = item.is_level;
                                 role_module.is_permission = item.is_permission;
                                 role_module.modified_date = DateTime.Now;
                                 role_module.modified_by = uid;
                                 role_module.modified_token_id = tid;
                                 role_module.modified_ip = ip;
                                 role_module.created_date = DateTime.Now;
                                 role_module.created_by = uid;
                                 role_module.created_token_id = tid;
                                 role_module.created_ip = ip;
                                 db.sys_role_modules.Add(role_module);
                             }
                         }
                     }

                     db.SaveChanges();
                     //Add ảnh
                     if (fileInfo != null)
                     {
                         if (!Directory.Exists(fileInfo.Directory.FullName))
                         {
                             Directory.CreateDirectory(fileInfo.Directory.FullName);
                         }
                         File.Move(ffileData.LocalFileName, newFileName);
                         helper.ResizeImage(newFileName, 640, 640, 90);
                     }
                     return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                 });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Modules/Add_Module", ip, tid, "Lỗi khi thêm Module", 0, "Module");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Modules/Add_Module", ip, tid, "Lỗi khi thêm Module", 0, "Module");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Module()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
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

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Module";
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
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        sys_modules model = JsonConvert.DeserializeObject<sys_modules>(fdmodel);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            delfiles.Add(root.Replace("/Portals", "") + model.image);
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
                            //newFileName = Path.Combine(root + "/Module", fileName);
                            newFileName = Path.Combine("/Module", fileName);
                            var fileNameTemp_1 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/");
                            var listPathTemp_1 = fileNameTemp_1.Split('/');
                            var pathConfigTemp_1 = "";
                            foreach (var item in listPathTemp_1)
                            {
                                if (item.Trim() != "")
                                {
                                    pathConfigTemp_1 += "/" + Path.GetFileName(item);
                                }
                            }
                            newFileName = pathConfigTemp_1;
                            fileInfo = new FileInfo(root + newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;

                                //newFileName = Path.Combine(root + "/Module", fileName);
                                newFileName = Path.Combine("/Module", fileName);
                                var fileNameTemp_2 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/");
                                var listPathTemp_2 = fileNameTemp_2.Split('/');
                                var pathConfigTemp_2 = "";
                                foreach (var item in listPathTemp_2)
                                {
                                    if (item.Trim() != "")
                                    {
                                        pathConfigTemp_2 += "/" + Path.GetFileName(item);
                                    }
                                }
                                newFileName = pathConfigTemp_2;
                            }
                            model.image = "/Portals/Module/" + fileName;
                            ffileData = fileData;
                        }
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_token_id = tid;
                        model.modified_ip = ip;
                        db.Entry(model).State = EntityState.Modified;
                        //update status child
                        //get all chill from parent, and update status
                        UpdateStatusChild(model.module_id, db.sys_modules.AsNoTracking().ToList(), model.status);
                        db.SaveChanges();
                        //Add ảnh
                        if (fileInfo != null)
                        {
                            foreach (string fpath in delfiles)
                            {
                                var fileNameTemp = Regex.Replace(fpath.Replace("\\", "/"), @"\.*/+", "/");
                                var listPath = fileNameTemp.Split('/');
                                var pathConfig = "";
                                foreach (var item in listPath)
                                {
                                    if (item.Trim() != "")
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                var fpathTemp = pathConfig.Substring(1);
                                if (File.Exists(fpathTemp))
                                    File.Delete(fpathTemp);
                                //if (File.Exists(fpath))
                                //    File.Delete(fpath);
                            }

                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }

                            File.Move(ffileData.LocalFileName, root + newFileName);
                            helper.ResizeImage(root + newFileName, 640, 640, 90);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Modules/Update_Module", ip, tid, "Lỗi khi cập nhật Module", 0, "Module");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Modules/Update_Module", ip, tid, "Lỗi khi cập nhật Module", 0, "Module");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Del_Module([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> ids)
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
                    var das = await db.sys_modules.Where(a => ids.Contains(a.module_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<sys_modules> del = new List<sys_modules>();
                        foreach (var da in das)
                        {
                            if (da.modified_by == uid || ad)
                            {
                                del.Add(da);
                                if (!string.IsNullOrWhiteSpace(da.image))
                                    paths.Add(HttpContext.Current.Server.MapPath("~/") + da.image);
                                // del role module
                                List<sys_role_modules> itemToRemove = db.sys_role_modules.Where(a => a.module_id == da.module_id).ToList();
                                if (itemToRemove.Count > 0)
                                    db.sys_role_modules.RemoveRange(itemToRemove);
                            }
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa menu này." });
                        }
                        db.sys_modules.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();

                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/Portals/Module")))
                    {
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Portals/Module"));
                    }
                    foreach (string strPath in paths)
                    {
                        var delPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Portals/Module"), Path.GetFileName(strPath));
                        bool exists = Directory.Exists(delPath);
                        if (exists)
                            System.IO.File.Delete(delPath);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Modules/Del_Module", ip, tid, "Lỗi khi xoá Module", 0, "Module");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Modules/Del_Module", ip, tid, "Lỗi khi xoá Module", 0, "Module");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_statusModule([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> ids, [System.Web.Mvc.Bind(Include = "")][FromBody] List<bool> tts)
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
                    var das = await db.sys_modules.Where(a => ids.Contains(a.module_id)).ToListAsync();
                    if (das != null)
                    {
                        List<sys_modules> del = new List<sys_modules>();
                        for (int i = 0; i < das.Count; i++)
                        {
                            var da = das[i];
                            if (da.modified_by == uid || ad)
                            {
                                da.status = tts[i];
                            }
                        }
                        await db.SaveChangesAsync();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Modules/Update_statusModule", ip, tid, "Lỗi khi cập nhật trạng thái Module", 0, "Module");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Modules/Update_statusModule", ip, tid, "Lỗi khi cập nhật trạng thái Module", 0, "Module");
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
        public async Task<HttpResponseMessage> GetDataProc([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);

            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
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
                        log.controller = domainurl + "Modules/GetDataProc";
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Modules/GetDataProc", ip, tid, "Lỗi khi gọi proc", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Modules/GetDataProc", ip, tid, "Lỗi khi gọi proc", 0, "chat");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }


        }
        #endregion

        #region Excel
        [HttpPost]
        public async Task<HttpResponseMessage> ImportExcel()
        {
            string ListErr = "";
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;

            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
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

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Module";
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
                               //fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                               //fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;

                               //newFileName = Path.Combine(root + "/Import", fileName);
                               var extensionFile = fileInfo.Extension.ToLower() == ".xls" ? ".xls" : ".xlsx";
                               fileName = fileName.Replace(extensionFile, "");
                               fileName = fileName + helper.ranNumberFile() + extensionFile;

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
                               List<sys_modules> dvs = new List<sys_modules>();
                               ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                               List<string> cols = new List<string>();
                               var dvcs = db.sys_modules.Select(a => new { a.module_id, a.module_name }).ToList();
                               for (int i = 5; i <= ws.Dimension.End.Row; i++)
                               {
                                   if (ws.Cells[i, 1].Value == null)
                                   {
                                       break;
                                   }
                                   sys_modules dv = new sys_modules();
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
                                           PropertyInfo propertyInfo = db.sys_modules.GetType().GetProperty(column.ToString());
                                           propertyInfo.SetValue(db.sys_modules, Convert.ChangeType(vl,
                                           propertyInfo.PropertyType), null);
                                       }
                                   }
                                   if (dvcs.Count(a => a.module_id == dv.module_id || a.module_name == dv.module_name) > 0)
                                       break;
                                   dv.modified_ip = ip;
                                   dv.modified_by = uid;
                                   dv.modified_date = DateTime.Now;
                                   dv.modified_token_id = tid;
                                   dvs.Add(dv);
                               }
                               if (dvs.Count > 0)
                               {
                                   db.sys_modules.AddRange(dvs);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Modules/ImportExcel", ip, tid, "Lỗi khi import Module", 0, "Module");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Modules/ImportExcel", ip, tid, "Lỗi khi import Module", 0, "Module");
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