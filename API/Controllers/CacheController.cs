using API.Models;
using Filter;
using Helper;
//using MSSQL.SqlHelper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using API.Helper;
using System.Text.RegularExpressions;

namespace Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = true)]
    [Authorize(Roles = "login")]
    public class CacheController : ApiController
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
        #region Config
        //[Authorize]
        [HttpGet]
        public HttpResponseMessage GetConfig()
        {
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
            //try
            //{
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    string json = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Config/Config.json"));
                    var numUserEncrypt = db.sys_users.Count(x => x.is_psword.Contains("=="));
                    var is_user_encrypt = false;
                    if (numUserEncrypt > 0)
                    {
                        is_user_encrypt = true;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.DeserializeObject<settings>(json), u_crypt = is_user_encrypt });
                }
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Cache/SetConfig", ip, tid, "Lỗi khi cấu hình tham số hệ thống", 0, "Cache");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        //[Authorize]
        [HttpGet]
        public HttpResponseMessage SwitchPublicToken()
        {
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
            //try
            //{
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                string json = helper.GenKey();
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = json });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Cache/GetPublicToken", ip, tid, "Lỗi khi tạo mới Token", 0, "Cache");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetPublicToken([System.Web.Mvc.Bind(Include = "")] string StrToken)
        {

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
            //try
            //{
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (StrToken.ToString() == helper.publicprockey.ToString())
                {
                    string json = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Config/Config.json"));

                    var data = JsonConvert.DeserializeObject<settings>(json);
                    if (data.publictoken == null)
                    {
                        string depass = Codec.EncryptString(helper.GenKey(), helper.psKey);
                        helper.publictoken = depass;
                        string cog = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Config/Config.json"));
                        var cogR = JsonConvert.DeserializeObject<settings>(cog);
                        cogR.publictoken = depass;
                        string jsonData = JsonConvert.SerializeObject(cogR, Formatting.None);
                        System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath("~/Config/Config.json"), jsonData);
                    }
                    else
                    {
                        helper.publictoken = data.publictoken;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = Codec.DecryptString(helper.publictoken, helper.psKey) });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã Token không hợp lệ! Vui lòng thử lại.", err = "1" });

            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Cache/GetPublicToken", ip, tid, "Lỗi khi tạo mới Token", 0, "Cache");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }
        //[Authorize]
        [HttpPost]
        //public HttpResponseMessage SetConfig([System.Web.Mvc.Bind(Include = "socket,debug,wlog,logCongtent,milisec,timeout,publictoken,apiBHBQP,tokenBHBQP,keycodeBHBQP, path_xml")][FromBody] settings cog)
        public async Task<HttpResponseMessage> SetConfig()
        {
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
            //try
            //{
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (!ad)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                string root = HttpContext.Current.Server.MapPath("~/");
                var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    var dataConfig = provider.FormData.GetValues("modelConfig").SingleOrDefault();
                    settings cog = JsonConvert.DeserializeObject<settings>(dataConfig);

                    helper.socket = cog.socket;
                    helper.debug = cog.debug;
                    helper.wlog = cog.wlog;
                    helper.logCongtent = cog.logCongtent;
                    helper.milisec = cog.milisec;
                    helper.timeout = cog.timeout;
                    string depass = Codec.EncryptString(cog.publictoken, helper.psKey);
                    helper.publictoken = depass;
                    cog.publictoken = depass;

                    #region file setting app (.apk)
                    FileInfo fileInfo = null;
                    string newFileName = root + "/Portals/Apk_App/setting_app_tivi.apk";
                    List<string> listPathFileUp = new List<string>();
                    foreach (MultipartFileData fileData in provider.FileData)
                    {
                        string fileName = "";
                        if (!string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                        {
                            fileName = fileData.Headers.ContentDisposition.FileName;
                            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                            {
                                fileName = fileName.Trim('"');
                            }
                        }
                        fileInfo = new FileInfo(newFileName);
                        if (fileInfo.Exists)
                        {
                            if (File.Exists(newFileName))
                            {
                                File.Delete(newFileName);
                            }
                        }
                        //Add file
                        if (fileInfo != null)
                        {
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            File.Move(fileData.LocalFileName, newFileName);
                            listPathFileUp.Add(fileData.LocalFileName);
                            cog.fileNameSettingApp = Path.GetFileName(fileName);
                            cog.filePathSettingApp = "/Portals/Apk_App/setting_app_tivi.apk";
                        }
                    }
                    if (listPathFileUp.Count > 0)
                    {
                        foreach (var path in listPathFileUp)
                        {
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                        }
                    }
                    if (cog.fileNameSettingApp == null && cog.filePathSettingApp == null)
                    {
                        if (File.Exists(root + "/Portals/Apk_App/setting_app_tivi.apk"))
                        {
                            File.Delete(root + "/Portals/Apk_App/setting_app_tivi.apk");
                        }
                    }
                    #endregion

                    cog.path_xml = Codec.DecryptString(cog.path_xml, helper.psKey);
                    if (String.IsNullOrEmpty(cog.path_xml))
                    {
                        cog.path_xml = root + "/Portals/XML/";
                    }
                    else
                    {
                        cog.path_xml = Regex.Replace(cog.path_xml.Replace("\\", "/"), @"\.*/+", "/");
                        var listPath = cog.path_xml.Split('/');
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
                        bool check_exists = System.IO.Directory.Exists(pathConfig);
                        if (check_exists == false)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Đường dẫn lưu file XML không tồn tại, vui lòng nhập lại!" });
                        }
                    }
                    helper.path_xml = cog.path_xml;

                    string jsonData = JsonConvert.SerializeObject(cog, Formatting.None);
                    System.IO.File.WriteAllText((root + "/Config/Config.json"), jsonData);
                    if (cog.socket)
                    {
                        var pathSocket = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\APIV2", "") + "Socket";
                        //var pathSocket = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\API", "") + "Socket";
                        string disk = pathSocket.Substring(0, 1);
                        string strCmdText = disk + ": & cd " + pathSocket + " & node app.js /C";
                        var proc = System.Diagnostics.Process.Start("CMD.exe", strCmdText);
                    }
                    else
                    {
                        var pathSocket = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\APIV2", "") + "Socket";
                        //var pathSocket = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\API", "") + "Socket";
                        string disk = pathSocket.Substring(0, 1);
                        string strCmdText = disk + ": & cd " + pathSocket + " & kill-port 3333 /C";
                        var proc = System.Diagnostics.Process.Start("CMD.exe", strCmdText);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                });
                return await task;
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Cache/SetConfig", ip, tid, "Lỗi khi cấu hình tham số hệ thống", 0, "Cache");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        //[Authorize]
        [HttpGet]
        //[CacheWebApi(Duration = 3600)]
        public async Task<HttpResponseMessage> ListModuleUserCache([System.Web.Mvc.Bind(Include = "")] string cache)
        {
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
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                DateTime sdate = DateTime.Now;
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, "sys_modules_ListUser", uid).Tables);
                var tables = await task;
                DateTime edate = DateTime.Now;
                #region add SQLLog
                if (helper.wlog)
                {
                    using (DBEntities db = new DBEntities())
                    {
                        sql_log log = new sql_log();
                        log.controller = domainurl + "Cache/sys_modules_ListUser";
                        log.start_date = sdate;
                        log.end_date = edate;
                        log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                        log.user_id = uid;
                        log.token_id = tid;
                        log.created_ip = ip;
                        log.full_name = name;
                        log.title = "sys_modules_ListUser";
                        log.log_content = JsonConvert.SerializeObject(new { data = uid });
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = uid, contents }), domainurl + "Cache/ListModuleUser", ip, tid, "Lỗi khi gọi sys_modules_ListUser", 0, "Cache");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = uid, contents }), domainurl + "Cache/ListModuleUser", ip, tid, "Lỗi khi gọi proc sys_modules_ListUser", 0, "Cache");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        //[Authorize]
        [HttpGet]
        [CacheWebApi(Duration = 3600)]
        public async Task<HttpResponseMessage> ListModuleUser([System.Web.Mvc.Bind(Include = "")] string cache)
        {
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
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                DateTime sdate = DateTime.Now;
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, "sys_modules_ListUser", uid).Tables);
                var tables = await task;
                DateTime edate = DateTime.Now;
                #region add SQLLog
                if (helper.wlog)
                {
                    using (DBEntities db = new DBEntities())
                    {
                        sql_log log = new sql_log();
                        log.controller = domainurl + "Cache/sys_modules_ListUser";
                        log.start_date = sdate;
                        log.end_date = edate;
                        log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                        log.user_id = uid;
                        log.token_id = tid;
                        log.created_ip = ip;
                        log.full_name = name;
                        log.title = "sys_modules_ListUser";
                        log.log_content = JsonConvert.SerializeObject(new { data = uid });
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = uid, contents }), domainurl + "Cache/ListModuleUser", ip, tid, "Lỗi khi gọi sys_modules_ListUser", 0, "Cache");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = uid, contents }), domainurl + "Cache/ListModuleUser", ip, tid, "Lỗi khi gọi proc sys_modules_ListUser", 0, "Cache");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        //[Authorize]
        [HttpGet]
        [CacheWebApi(Duration = 3600)]
        public async Task<HttpResponseMessage> ListUsers([System.Web.Mvc.Bind(Include = "")] string cache)
        {
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
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                DateTime sdate = DateTime.Now;
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, "User_ListTudien", uid).Tables);
                var tables = await task;
                DateTime edate = DateTime.Now;
                #region add SQLLog
                if (helper.wlog)
                {
                    using (DBEntities db = new DBEntities())
                    {
                        sql_log log = new sql_log();
                        log.controller = domainurl + "Cache/ListUsers";
                        log.start_date = sdate;
                        log.end_date = edate;
                        log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                        log.user_id = uid;
                        log.token_id = tid;
                        log.created_ip = ip;
                        log.full_name = name;
                        log.title = "User_ListTudien";
                        log.log_content = JsonConvert.SerializeObject(new { data = uid });
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = uid, contents }), domainurl + "Cache/ListUsers", ip, tid, "Lỗi khi gọi User_ListTudien", 0, "Cache");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = uid, contents }), domainurl + "Cache/ListUsers", ip, tid, "Lỗi khi gọi proc User_ListTudien", 0, "Cache");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> ExportFileLogError([System.Web.Mvc.Bind(Include = "start_date,end_date")][FromBody] JObject data)
        {
            using (DBEntities db = new DBEntities())
            {
                DateTime? startDate = data["start_date"].ToObject<DateTime?>();
                DateTime? endDate = data["end_date"].ToObject<DateTime?>();
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
                //try
                //{
                var user_now = db.sys_users.Find(uid);
                if (user_now == null || (user_now != null && user_now.is_admin != true))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
                else
                {
                    string Connection = db.Database.Connection.ConnectionString;
                    var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, "sys_list_log_export", uid, startDate, endDate).Tables);
                    var tables = await task;
                    var text = "";
                    text = "Log từ ngày " + (startDate != null ? startDate?.ToString("dd/MM/yyyy") : "...") + " đến ngày " + (endDate != null ? endDate?.ToString("dd/MM/yyyy") : "...") + ":\n";
                    //text += "Thời gian\t Họ tên\t Nội dung\n";
                    var tableData = tables[0];
                    var tableTittle = tables[1];
                    var len = tableTittle.Columns.Count;
                    for (int k = 0; k < tableData.Rows.Count; k++)
                    {
                        var dr = tableData.Rows[k];
                        for (int i = 1; i <= len; i++)
                        {
                            var nameCol = tableTittle.Rows[0]["col_" + i.ToString()]?.ToString();
                            if (nameCol.Contains("_base64"))
                            {
                                nameCol = nameCol.Substring(0, nameCol.IndexOf("_base64"));
                                text += (" :  " + Convert.ToBase64String(Encoding.UTF8.GetBytes(dr[nameCol]?.ToString())));
                            }
                            else
                            {
                                text += dr[nameCol]?.ToString();
                            }
                            if (i == len)
                            {
                                text += "\n";
                            }
                        }
                    }
                    //Byte[] fileBytes = Encoding.ASCII.GetBytes(text);
                    string fname = helper.convertToUnSign3("filelogerror_" + DateTime.Now.ToString("yyyyddMM_HHmmss"));
                    var strPath = HttpContext.Current.Server.MapPath("~/Portals/Export/FileLogError");
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                    {
                        Directory.CreateDirectory(strPath);
                    }
                    else
                    {
                        Directory.Delete(strPath, true);
                        Directory.CreateDirectory(strPath);
                    }
                    string path = HttpContext.Current.Server.MapPath("~/Portals/Export/FileLogError/" + fname + ".txt");
                    File.WriteAllText(path, text);
                    return Request.CreateResponse(HttpStatusCode.OK, new { path = "/Portals/Export/FileLogError/" + fname + ".txt", filename = fname + ".txt", err = "0" });
                }
                //}
                //catch (Exception)
                //{
                //    string contents = helper.ExceptionMessage(e);
                //    if (!helper.debug)
                //    {
                //        contents = "";
                //    }
                //    Log.Error(contents);
                //    return Request.CreateResponse(HttpStatusCode.BadRequest);
                //}
            }
        }
        #endregion
    }
}