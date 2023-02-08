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
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Models;
using Helper;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json.Linq;
using API.Helper;

namespace API.Controllers.video
{
    [Authorize(Roles = "login")]
    public class video_mainController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_video()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdvideo = "";
            string urlBase64 = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    string strPath = root + "/" + dvid + "/Videos";
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
                        fdvideo = provider.FormData.GetValues("video").SingleOrDefault();
                        urlBase64 = provider.FormData.GetValues("urlBase64").SingleOrDefault();
                        video_main video = JsonConvert.DeserializeObject<video_main>(fdvideo);

                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        var detached1 = "";
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
                            fileName = helper.convertToUnSign3(fileName);
                            newFileName = Path.Combine(root + "/" + dvid + "/Videos", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Videos", fileName);
                            }
                            if (helper.IsImageFileName(newFileName))
                            {
                                video.image = "/Portals/" + dvid + "/Videos/" + fileName;
                                ffileData = fileData;
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
                            }
                            else
                            {
                                video.file_name = fileName;
                                video.path = detached1 + "/Portals/" + dvid + "/Videos/" + fileName;
                                detached1 = ",";
                                ffileData = fileData;
                                //Add file zip
                                if (fileInfo != null)
                                {
                                    if (!Directory.Exists(fileInfo.Directory.FullName))
                                    {
                                        Directory.CreateDirectory(fileInfo.Directory.FullName);
                                    }
                                    File.Move(ffileData.LocalFileName, newFileName);

                                }
                            }

                        }
                        if (urlBase64.IndexOf("base64,") > 0)
                        {
                            Random rnd = new Random();
                            byte[] bytes = Convert.FromBase64String(urlBase64.Substring(urlBase64.IndexOf("base64,") + 7));
                            var filePath = "/" + dvid + "/Videos/ImgShotcut" + rnd.Next() + ".jpg";
                            if (System.IO.File.Exists(root + filePath))
                            {
                                System.IO.File.Delete(root + filePath);
                            }
                            using (var imageFile = new FileStream(root + filePath, FileMode.Create))
                            {
                                imageFile.Write(bytes, 0, bytes.Length);
                                imageFile.Flush();
                            }
                            video.image = "/Portals" + filePath;
                        }
                        if (video.is_trimfile == true)
                        {
                            var FileNameCovert = "";
                            FileNameCovert = provider.FormData.GetValues("FileNameCovert").SingleOrDefault();
                            video.file_name = FileNameCovert;
                            video.path = "/Portals/" + dvid + "/Videos/" + FileNameCovert;
                        }
                        video.created_date = DateTime.Now;
                        video.created_by = uid;
                        video.created_ip = ip;
                        video.modified_date = DateTime.Now;
                        video.modified_by = uid;
                        video.modified_ip = ip;
                        db.video_main.Add(video);
                        db.SaveChanges();

                        #region add cms_logs
                        if (helper.wlog)
                        {

                            task_log log = new task_log();
                            log.task_id = video.video_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_log.Add(log);
                            db.SaveChanges();


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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdvideo, contents }), domainurl + "video_main/Addvideo_main", ip, tid, "Lỗi khi thêm Tin tức", 0, "video_main");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdvideo, contents }), domainurl + "video_main/Addvideo_main", ip, tid, "Lỗi khi thêm Tin tức", 0, "video_main");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_video()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdvideo = "";
            string urlBase64 = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    string strPath = root + "/" + dvid + "/Videos";
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
                        fdvideo = provider.FormData.GetValues("video").SingleOrDefault();
                        urlBase64 = provider.FormData.GetValues("urlBase64").SingleOrDefault();
                        video_main video = JsonConvert.DeserializeObject<video_main>(fdvideo);
                        var videoOld = db.video_main.AsNoTracking().Where(s => s.video_id == video.video_id).FirstOrDefault<video_main>();

                        var detached1 = "";
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
                            fileName = helper.convertToUnSign3(fileName);
                            newFileName = Path.Combine(root + "/" + dvid + "/Videos", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Videos", fileName);
                            }

                            if (helper.IsImageFileName(newFileName))
                            {
                                video.image = "/Portals/" + dvid + "/Videos/" + fileName;
                                ffileData = fileData;
                                ffileData = fileData;
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
                            }
                            else
                            {
                                video.file_name = fileName;
                                video.path = detached1 + "/Portals/" + dvid + "/Videos/" + fileName;
                                detached1 = ",";
                                ffileData = fileData;
                                //Add file zip
                                if (fileInfo != null)
                                {
                                    if (!Directory.Exists(fileInfo.Directory.FullName))
                                    {
                                        Directory.CreateDirectory(fileInfo.Directory.FullName);
                                    }
                                    File.Move(ffileData.LocalFileName, newFileName);

                                }
                            }
                        }
                        if (urlBase64.IndexOf("base64,") > 0)
                        {
                            Random rnd = new Random();
                            byte[] bytes = Convert.FromBase64String(urlBase64.Substring(urlBase64.IndexOf("base64,") + 7));
                            var filePath = "/" + dvid + "/Videos/ImgShotcut" + rnd.Next() + ".jpg";
                            if (System.IO.File.Exists(root + filePath))
                            {
                                System.IO.File.Delete(root + filePath);
                            }
                            using (var imageFile = new FileStream(root + filePath, FileMode.Create))
                            {
                                imageFile.Write(bytes, 0, bytes.Length);
                                imageFile.Flush();
                            }
                            video.image = "/Portals" + filePath;
                        }
                        //if (video.is_trimfile == true)
                        //{
                        //    var FileNameCovert = "";
                        //    FileNameCovert = provider.FormData.GetValues("FileNameCovert").SingleOrDefault();
                        //    video.file_name = FileNameCovert;
                        //    video.path = "/Portals/" + dvid + "/Videos/" + FileNameCovert;
                        //}
                        video.modified_date = DateTime.Now;
                        video.modified_by = uid;
                        video.modified_ip = ip;
                        db.Entry(video).State = EntityState.Modified;
                        db.SaveChanges();
                        //Add ảnh
                        if (delfiles.Count > 0)
                        {
                            foreach (string fpath in delfiles)
                            {
                                if (File.Exists(fpath))
                                    File.Delete(fpath);
                            }
                        }

                        #region add cms_logs
                        if (helper.wlog)
                        {

                            task_log log = new task_log();
                            log.task_id = video.video_id;
                            log.des = "Sửa video " + video.video_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_log.Add(log);
                            db.SaveChanges();

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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdvideo, contents }), domainurl + "CMS_New/Update_video", ip, tid, "Lỗi khi cập nhật Tin tức", 0, "video");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdvideo, contents }), domainurl + "CMS_New/Update_video", ip, tid, "Lỗi khi cập nhật Tin tức", 0, "video");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_video([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                string root = HttpContext.Current.Server.MapPath("~/Portals");
                string strPath = root + "/Videos";
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.video_main.Where(a => id.Contains(a.video_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        if (das != null)
                        {
                            List<video_main> del = new List<video_main>();
                            foreach (var da in das)
                            {
                                if (ad || da.created_by == uid)
                                {
                                    bool check_exist = da.path.Contains(@"/Portals/" + dvid);
                                    del.Add(da);
                                    if (!string.IsNullOrWhiteSpace(da.path))
                                    {
                                        var path_temp = HttpContext.Current.Server.MapPath("~/") + "/Portals/" + (check_exist == true ? dvid : "") + "/Videos/" + Path.GetFileName(da.path);
                                        paths.Add(path_temp);
                                    }
                                    if (!string.IsNullOrWhiteSpace(da.image))
                                    {
                                        var path_temp = HttpContext.Current.Server.MapPath("~/") + "/Portals/" + (check_exist == true ? dvid : "") + "/Videos/" + Path.GetFileName(da.path);
                                        paths.Add(path_temp);
                                    }

                                }
                                #region add cms_logs
                                if (helper.wlog)
                                {

                                    task_log log = new task_log();
                                    log.task_id = da.video_id;
                                    log.des = "Xóa video " + da.video_id;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.task_log.Add(log);
                                    db.SaveChanges();

                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.video_main.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(strP);
                            if (exists)
                                System.IO.File.Delete(strP);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "video_main/Deletevideo_main", ip, tid, "Lỗi khi xoá tin tức", 0, "video");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "video_main/Deletevideo_main", ip, tid, "Lỗi khi xoá tin tức", 0, "video");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_status([System.Web.Mvc.Bind(Include = "IntID,IntTrangthai")] Trangthai trangthai)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;

            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = db.video_main.Where(a => (a.video_id == trangthai.IntID)).FirstOrDefault<video_main>();
                        if (das != null)
                        {

                            das.status = trangthai.IntTrangthai;
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;

                            #region add cms_logs
                            if (helper.wlog)
                            {

                                task_log log = new task_log();
                                log.task_id = das.video_id;
                                log.des = "Sửa shows " + das.video_id;
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.task_log.Add(log);
                                db.SaveChanges();


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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "news_main/Update_Trangthai", ip, tid, "Lỗi khi cập nhật News", 0, "News");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "news_main/Update_Trangthai", ip, tid, "Lỗi khi cập nhật News", 0, "News");
                    if (!helper.debug)
                    {
                        contents = "";
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
                            log.controller = domainurl + "video_main/GetDataProc";
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "VideoMain/GetDataProc", ip, tid, "Lỗi khi gọi proc", 0, "Booking");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "VideoMain/GetDataProc", ip, tid, "Lỗi khi gọi proc", 0, "Booking");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
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