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
using System.IO.Compression;
using System.Web;
using System.Web.Http;
using API.Models;
using Helper;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using HtmlAgilityPack;
using API.Helper;

namespace API.Controllers.shows
{
    [Authorize(Roles = "login")]
    public class shows_mainController : ApiController
    {
        public string getipaddress()
        {
           
            return  HttpContext.Current.Request.UserHostAddress;
        }
     [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> DownloadHtml5(int id, int dvid)
        {

            var identity = User.Identity as ClaimsIdentity;
          
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
         
            try
            {
                using (DBEntities db = new DBEntities())
                {

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Shows";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                  
                    var shows =  db.shows_main.AsNoTracking().FirstOrDefault(x => x.shows_id == id);
                    string urlDownload= shows.file_folder+ ".zip";
                    urlDownload = urlDownload.Replace("_", " ");
                    if (shows != null)
                    {

                        string zipPath = strPath + "/" + shows.file_name;
                        string filePath = root + urlDownload.Substring(8);
                        bool existZipPath = System.IO.File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Shows/" + Path.GetFileName(urlDownload));
                      
                        if (!existZipPath)
                        {
                    
                            System.IO.Compression.ZipFile.CreateFromDirectory( root + shows.file_folder.Substring(8), zipPath, System.IO.Compression.CompressionLevel.Fastest, true);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { error = 0, url = urlDownload });
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
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }








        }

      
        [HttpPost]
        public async Task<HttpResponseMessage> add_shows()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdshows = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; 
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    string strPath = root + "/" + dvid + "/Shows";
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
                        fdshows = provider.FormData.GetValues("shows").SingleOrDefault();
                        shows_main shows = JsonConvert.DeserializeObject<shows_main>(fdshows);

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
                            newFileName = Path.Combine(root + "/" + dvid + "/Shows", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Shows", fileName);
                            }
                            if (helper.IsImageFileName(newFileName))
                            {
                                shows.image = "/Portals/" + dvid + "/Shows/" + fileName;
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
                                
                                 if (helper.IsPptFileName(newFileName))
                                     {
                                    shows.file_name = fileName;
                                    shows.path = "/Portals/" + dvid + "/Shows/" + fileName;

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
                                else {

                                    var fileName_convert = helper.convertToUnSign3(fileName);
                                    fileName_convert = Regex.Replace(fileName_convert.Trim(), @"\s+", "_");
                                    shows.path = "/Portals/" + dvid + "/Shows/" + fileName_convert.Substring(0, fileName_convert.LastIndexOf(".")) + "/index.html";
                                    #region del folder zip old if exist
                                    if (shows.file_folder != null && shows.file_folder != "")
                                    {
                                        bool exists = Directory.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Shows/" + Path.GetFileName(shows.file_folder) );
                                        if (exists)
                                            Directory.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Shows/" + Path.GetFileName(shows.file_folder), true);
                                    }
                                    #endregion
                                    #region zip
                                    var pathfilezip =  "/" + dvid + "/Shows/" + fileName_convert;

                                    string extractPath =root + pathfilezip.Replace(".zip", "");
                                    bool existPathZip = System.IO.Directory.Exists(extractPath);
                                    if (!existPathZip)
                                    {
                                        System.IO.Directory.CreateDirectory(extractPath);
                                    }
                                    System.IO.Compression.ZipFile.ExtractToDirectory(fileData.LocalFileName, extractPath);
                                    shows.file_folder = "/Portals/" + pathfilezip.Replace(".zip", "");
                                    #endregion
                                    if (pathfilezip != null && pathfilezip != "")
                                    {
                                        if (System.IO.File.Exists(root+pathfilezip.Substring(8)))
                                        {
                                            System.IO.File.Delete(root + pathfilezip.Substring(8));
                                        }
                                    }
                                    shows.file_name = fileName;

                                }
                            }

                        }

                        shows.created_date = DateTime.Now;
                        shows.created_by = uid;
                        shows.created_ip = ip;
                        shows.modified_date = DateTime.Now;
                        shows.modified_by = uid;
                        shows.modified_ip = ip;
                        db.shows_main.Add(shows);
                        db.SaveChanges();

                        #region add cms_logs
                        if (helper.wlog)
                        {

                            task_log log = new task_log();
                            log.task_id = shows.shows_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdshows, contents }), domainurl + "shows_main/Addshows_main", ip, tid, "Lỗi khi thêm Tin tức", 0, "shows_main");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdshows, contents }), domainurl + "shows_main/Addshows_main", ip, tid, "Lỗi khi thêm Tin tức", 0, "shows_main");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_shows()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdshows = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; 
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
            List<string> delfolder = new List<string>();
           
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Shows";
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
                        fdshows = provider.FormData.GetValues("shows").SingleOrDefault();
                        shows_main shows = JsonConvert.DeserializeObject<shows_main>(fdshows);
                        var showsOld = db.shows_main.AsNoTracking().Where(s => s.shows_id == shows.shows_id).FirstOrDefault<shows_main>();

                     
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Shows", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Shows", fileName);
                            }

                            if (helper.IsImageFileName(newFileName))
                            {
                                shows.image = "/Portals/" + dvid + "/Shows/" + fileName;
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
                                if (helper.IsPptFileName(newFileName))
                                {
                                    shows.file_name = fileName;
                                    shows.path = "/Portals/" + dvid + "/Shows/" + fileName;

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
                                else
                                {

                                    var fileName_convert = helper.convertToUnSign3(fileName);
                                    fileName_convert = Regex.Replace(fileName_convert.Trim(), @"\s+", "_");
                                    shows.path = "/Portals/" + dvid + "/Shows/" + fileName_convert.Substring(0, fileName_convert.LastIndexOf(".")) + "/index.html";
                                    #region del folder zip old if exist
                                    if (shows.file_folder != null && shows.file_folder != "")
                                    {
                                        bool exists = Directory.Exists(root + shows.file_folder.Substring(8));
                                        if (exists)
                                            Directory.Delete(root + shows.file_folder.Substring(8),true);
                                    }
                                    #endregion
                                    #region zip
                                    var pathfilezip = "/" + dvid + "/Shows/" + fileName_convert;

                                    string extractPath =root+ pathfilezip.Replace(".zip", "");
                                    bool existPathZip = System.IO.Directory.Exists(extractPath);
                                    if (!existPathZip)
                                    {
                                        System.IO.Directory.CreateDirectory(extractPath);
                                    }
                                    System.IO.Compression.ZipFile.ExtractToDirectory(fileData.LocalFileName, extractPath);
                                    shows.file_folder = "/Portals/" + pathfilezip.Replace(".zip", "");
                                    #endregion
                                    if (pathfilezip != null && pathfilezip != "")
                                    {
                                        if (System.IO.File.Exists(root + pathfilezip.Substring(8)))
                                        {
                                            System.IO.File.Delete(root + pathfilezip.Substring(8));
                                        }
                                    }
                                    shows.file_name = fileName;

                                }
                            }
                        }
                        if (showsOld.image != null && showsOld.image != "" && showsOld.image != shows.image)
                        {
                            string fileOld = showsOld.image.Substring(8);
                            delfiles.Add(  fileOld);
                        }
                        if (showsOld.path != null && showsOld.path != "" && showsOld.path != shows.path)
                        {
                            if (helper.IsPptFileName(showsOld.file_name))
                            {
                                string fileOld = showsOld.path.Substring(8);
                                delfiles.Add(  fileOld);
                            }
                            else
                            {
                                string folderOld = showsOld.file_folder;
                                delfolder.Add(  folderOld);
                            }
                               
                            
                        }
                        shows.modified_date = DateTime.Now;
                        shows.modified_by = uid;
                        shows.modified_ip = ip;
                        db.Entry(shows).State = EntityState.Modified;
                        db.SaveChanges();
                        //Add ảnh
                        if (delfiles.Count > 0)
                        {
                            foreach (string fpath in delfiles)
                            {
                                if (File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Shows/" + Path.GetFileName(fpath)))
                                    File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Shows/" + Path.GetFileName(fpath));
                            }
                        }
                        if (delfolder.Count > 0)
                        {
                            foreach (string fpath in delfolder)
                            {
                                if (Directory.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Shows/" + Path.GetFileName(fpath)))
                                    Directory.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Shows/" + Path.GetFileName(fpath), true);
                            }
                        }
                        #region add cms_logs
                        if (helper.wlog)
                        {

                            task_log log = new task_log();
                            log.task_id = shows.shows_id;
                            log.des = "Sửa shows " + shows.shows_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdshows, contents }), domainurl + "CMS_New/Update_shows", ip, tid, "Lỗi khi cập nhật Tin tức", 0, "shows");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdshows, contents }), domainurl + "CMS_New/Update_shows", ip, tid, "Lỗi khi cập nhật Tin tức", 0, "shows");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
      
        [HttpDelete]
        public async Task<HttpResponseMessage> delete_shows([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;

           
            try
            {
                string root = HttpContext.Current.Server.MapPath("~/Portals");
             
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                 
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                string strPath = root + "/" + dvid + "/Shows";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.shows_main.Where(a => id.Contains(a.shows_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        List<string> path_folder = new List<string>();
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        if (das != null)
                        {
                            List<shows_main> del = new List<shows_main>();
                            foreach (var da in das)
                            {
                                if (ad || da.created_by == uid)
                                {

                                    del.Add(da);
                                    if (!string.IsNullOrWhiteSpace(da.file_folder + ".zip"))
                                        paths.Add( da.file_folder + ".zip");
                                    if (!string.IsNullOrWhiteSpace(da.file_folder))
                                        path_folder.Add( da.file_folder);
                                    if (!string.IsNullOrWhiteSpace(da.path))
                                        paths.Add(  da.path);
                                    if (!string.IsNullOrWhiteSpace(da.image))
                                        paths.Add(  da.image);
                                   
                                }
                                #region add cms_logs
                                if (helper.wlog)
                                {

                                    task_log log = new task_log();
                                    log.task_id = da.shows_id;
                                    log.des = "Xóa shows " + da.shows_id;
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
                            db.shows_main.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals/"+Path.GetFileName(dvid)+"/Shows/") + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/Shows/") + Path.GetFileName(strP));
                        }
                        foreach (string strP in path_folder)
                        {
                            bool exists = Directory.Exists(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/Shows/") + Path.GetFileName(strP));
                            if (exists)
                                System.IO.Directory.Delete(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/Shows/") + Path.GetFileName(strP), true);
                        }
                        
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "shows_main/Deleteshows_main", ip, tid, "Lỗi khi xoá tin tức", 0, "shows");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "shows_main/Deleteshows_main", ip, tid, "Lỗi khi xoá tin tức", 0, "shows");
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
                        var int_id = int.Parse(trangthai.IntID.ToString());
                        var das = db.shows_main.Where(a => (a.shows_id == int_id)).FirstOrDefault<shows_main>();
                        if (das != null)
                        {

                            das.status = trangthai.IntTrangthai;
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                       
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                task_log log = new task_log();
                                log.task_id = das.shows_id;
                                log.des = "Sửa shows " + das.shows_id;
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
      
        [HttpPut]
        public async Task<HttpResponseMessage> update_visitor([System.Web.Mvc.Bind(Include = "IntID,user_id")] IsVisitor visitor)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
      
            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var int_id = int.Parse(visitor.IntID.ToString());
                        var das = db.shows_main.Where(a => (a.shows_id == int_id)).FirstOrDefault<shows_main>();
                        if (das != null)
                        {
                            if (das.is_visitor == null)
                                das.is_visitor = 1;
                            else
                                das.is_visitor += 1;
                            var dew = db.shows_users_visitor.Where(a => (a.user_id == visitor.user_id && a.shows_id == visitor.IntID)).FirstOrDefault<shows_users_visitor>();
                            if (dew != null)
                            {
                                dew.times += 1;
                                dew.modified_date = DateTime.Now;
                                dew.modified_by = uid;
                                db.SaveChanges();
                            }
                            else
                            {
                                shows_users_visitor shows_Users_Visitor = new shows_users_visitor();
                                shows_Users_Visitor.times = 1;
                                shows_Users_Visitor.user_id = visitor.user_id;
                                shows_Users_Visitor.shows_id = visitor.IntID;
                                shows_Users_Visitor.created_date = DateTime.Now;
                                shows_Users_Visitor.created_by = uid;
                                shows_Users_Visitor.created_ip = ip;
                                shows_Users_Visitor.modified_date = DateTime.Now;
                                shows_Users_Visitor.modified_by = uid;

                                db.shows_users_visitor.Add(shows_Users_Visitor);
                                db.SaveChanges();
                            }
                            await db.SaveChangesAsync();
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = visitor.IntID, contents }), domainurl + "visitor/Update_visitor", ip, tid, "Lỗi khi cập nhật News", 0, "News");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = visitor.IntID, contents }), domainurl + "visitor/Update_visitor", ip, tid, "Lỗi khi cập nhật News", 0, "News");
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

    }
}