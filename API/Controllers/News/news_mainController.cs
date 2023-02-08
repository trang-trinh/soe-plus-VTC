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

namespace API.Controllers.News
{
    [Authorize(Roles = "login")]
    public class news_mainController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

    
        [HttpPost]
        public async Task<HttpResponseMessage> add_news()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdtintuc = "";
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
                    string strPath = root + "/" + dvid + "/News";
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
                        fdtintuc = provider.FormData.GetValues("news").SingleOrDefault();
                        news_main news = JsonConvert.DeserializeObject<news_main>(fdtintuc);
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(news.contents);

                        var imgs = doc.DocumentNode.SelectNodes("//img");

                        if (imgs != null)
                        {

                            foreach (var img in imgs)
                            {
                                HtmlNode oldChild = img;
                                HtmlNode newChild = HtmlNode.CreateNode(img.OuterHtml);
                                var pathFolderDes = "/Portals/" + dvid + "/News";
                                var checkBase64 = img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                                checkBase64 = checkBase64.Trim();
                                if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                                {
                                    byte[] bytes = Convert.FromBase64String(img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                                    bool existsFolder = System.IO.Directory.Exists(strPath + pathFolderDes);
                                    if (!existsFolder)
                                    {
                                        System.IO.Directory.CreateDirectory(strPath);
                                    }

                                    var index1 = img.Attributes["src"].Value.LastIndexOf("data:image/") + 11;
                                    var index2 = img.Attributes["src"].Value.IndexOf("base64,");
                                    var typeFileHL = "." + img.Attributes["src"].Value.Substring(index1, index2 - index1 - 1);
                                    var pathShow = "/" + helper.GenKey() + typeFileHL;



                                    using (var imageFile = new FileStream(strPath + pathShow, FileMode.Create))
                                    {
                                        imageFile.Write(bytes, 0, bytes.Length);
                                        imageFile.Flush();
                                    }



                                    img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;object-fit:contain");
                                    img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/News" + pathShow);
                                    if (news.image == null || news.image == "")
                                    {

                                        news.image = "/Portals/" + dvid + "/News" + pathShow;
                                    }
                                    news.contents = news.contents.Replace(newChild.OuterHtml, img.OuterHtml);

                                    helper.ResizeImage(strPath + pathShow, 640, 640, 90);
                                }
                                else
                                {
                                    bool existsFolder = System.IO.File.Exists(img.Attributes["src"].Value);
                                    if (!existsFolder)
                                    {
                                        System.Net.WebClient webc = new System.Net.WebClient();
                                        var pathShow = "/" + helper.GenKey() + ".png";
                                        FileStream imageFile1 = null;
                                        try
                                        {
                                            imageFile1 = new FileStream(strPath + pathShow, FileMode.Create);

                                        }

                                        finally
                                        {
                                            imageFile1.Close();
                                        }
                                        webc.DownloadFile(img.Attributes["src"].Value, strPath + pathShow);
                                        img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;object-fit:contain");
                                        img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/News" + pathShow);
                                        news.contents = news.contents.Replace(newChild.OuterHtml, img.OuterHtml);
                                        helper.ResizeImage(strPath + pathShow, 640, 640, 90);
                                        if (news.image == null || news.image == "")
                                        {

                                            news.image = "/Portals/" + dvid + "/News" + pathShow;
                                        }
                                    }
                                }
                            }
                        }
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
                            newFileName = Path.Combine(root + "/" + dvid + "/News", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/News", fileName);
                            }
                            if (helper.IsImageFileName(newFileName) && news.news_type == 0)
                            {
                                news.image = "/Portals/" + dvid + "/News/" + fileName;

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
                                news_main_files news_Main_Files = new news_main_files();
                                news_Main_Files.file_path = "/Portals/" + dvid + "/News/" + fileName;
                                news_Main_Files.file_name = fileName;
                                news_Main_Files.file_type = helper.GetFileExtension(fileName);

                                news.url_file += detached1 + "/Portals/" + dvid + "/News/" + fileName;
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
                                    if (helper.IsImageFileName(newFileName))
                                        helper.ResizeImage(newFileName, 640, 640, 90);



                                }
                            }

                        }
                        if (news.news_type == 1)
                            news.image = null;
                        news.created_date = DateTime.Now;
                        news.created_by = uid;
                        news.created_ip = ip;
                        news.modified_date = DateTime.Now;
                        news.modified_by = uid;
                        news.modified_ip = ip;
                        db.news_main.Add(news);
                        db.SaveChanges();

                        #region add cms_logs
                        if (helper.wlog)
                        {

                            task_log log = new task_log();
                            log.task_id = news.news_id;
                            if (news.news_type == 0)
                                log.des = "Thêm tin tức" + news.news_id;
                            if (news.news_type == 1)
                                log.des = "Thêm thông báo" + news.news_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtintuc, contents }), domainurl + "news_main/Addnews_main", ip, tid, "Lỗi khi thêm Tin tức", 0, "news_main");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtintuc, contents }), domainurl + "news_main/Addnews_main", ip, tid, "Lỗi khi thêm Tin tức", 0, "news_main");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_news()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdtintuc = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/News";
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
                        fdtintuc = provider.FormData.GetValues("news").SingleOrDefault();
                        news_main news = JsonConvert.DeserializeObject<news_main>(fdtintuc);
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(news.contents);
                        var imgs = doc.DocumentNode.SelectNodes("//img");
                        if (imgs != null)
                        {
                            foreach (var img in imgs)
                            {
                                HtmlNode oldChild = img;
                                HtmlNode newChild = HtmlNode.CreateNode(img.OuterHtml);
                                var pathFolderDes = "/Portals/" + dvid + "/News";
                                var checkBase64 = img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                                checkBase64 = checkBase64.Trim();
                                if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                                {
                                    byte[] bytes = Convert.FromBase64String(img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                                    bool existsFolder = System.IO.Directory.Exists(strPath + pathFolderDes);
                                    if (!existsFolder)
                                    {
                                        System.IO.Directory.CreateDirectory(strPath);
                                    }

                                    var index1 = img.Attributes["src"].Value.LastIndexOf("data:image/") + 11;
                                    var index2 = img.Attributes["src"].Value.IndexOf("base64,");
                                    var typeFileHL = "." + img.Attributes["src"].Value.Substring(index1, index2 - index1 - 1);
                                    var pathShow = "/" + helper.GenKey() + typeFileHL;



                                    using (var imageFile = new FileStream(strPath + pathShow, FileMode.Create))
                                    {
                                        imageFile.Write(bytes, 0, bytes.Length);
                                        imageFile.Flush();
                                    }



                                    img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;object-fit:contain");
                                    img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/News" + pathShow);
                                    if (news.image == null || news.image == "")
                                    {

                                        news.image = "/Portals/" + dvid + "/News" + pathShow;
                                    }
                                    news.contents = news.contents.Replace(newChild.OuterHtml, img.OuterHtml);
                                    helper.ResizeImage(strPath + pathShow, 640, 640, 90);
                                }
                                else
                                {

                                    bool existsFolder = System.IO.File.Exists(strPath + img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.IndexOf("/News") + 5));
                                    if (!existsFolder)
                                    {
                                        System.Net.WebClient webc = new System.Net.WebClient();
                                        var pathShow = "/" + helper.GenKey() + ".png";
                                        FileStream imageFile1 = null;
                                        try
                                        {
                                            imageFile1 = new FileStream(strPath + pathShow, FileMode.Create);

                                        }

                                        finally
                                        {
                                            imageFile1.Close();
                                        }

                                        webc.DownloadFile(img.Attributes["src"].Value, strPath + pathShow);


                                        img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;object-fit:contain");
                                        img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/News" + pathShow);
                                        news.contents = news.contents.Replace(newChild.OuterHtml, img.OuterHtml);
                                        helper.ResizeImage(strPath + pathShow, 640, 640, 90);
                                        if (news.image == null || news.image == "")
                                        {

                                            news.image = "/Portals/" + dvid + "/News" + pathShow;
                                        }
                                    }
                                    else if (news.image == null || news.image == "")
                                    {
                                        news.image = "/Portals/" + dvid + "/News" + img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.IndexOf("Portals/" + dvid + "/News") + 12);
                                    }
                                }
                            }
                        }
                        var newsOld = db.news_main.AsNoTracking().Where(s => s.news_id == news.news_id).FirstOrDefault<news_main>();
                        doc.LoadHtml(newsOld.contents);
                        imgs = doc.DocumentNode.SelectNodes("//img");
                        if (imgs != null)
                        {
                            foreach (var img in imgs)
                            {
                                var imgdel = img.Attributes["src"].Value;
                                var dlt = "/" + imgdel.Substring(imgdel.IndexOf("Portals/" + dvid + "/News"));
                                imgdel = root + imgdel.Substring(imgdel.IndexOf("Portals/" + dvid + "/News") + 7);
                                if (!news.contents.Contains(img.Attributes["src"].Value) && news.image != dlt)
                                {
                                    delfiles.Add(imgdel);
                                }

                            }
                        }
                      
                        var detached1 = "";
                        if (newsOld.url_file != null && newsOld.url_file != "" && newsOld.url_file != news.url_file)
                        {
                            var newUrl = "";
                            var arrF = newsOld.url_file.Split(',');
                            var arrF1 = news.url_file.Split(',');
                            foreach (var item in arrF)
                            {
                                var checL = false;
                                foreach (var ele in arrF1)
                                {
                                    if (ele == item)
                                    {
                                        checL = true;
                                    }
                                }
                                if (!checL)
                                {

                                    var news_Main_Files_Del = db.news_main_files.AsNoTracking().Where(s => s.file_path == item).FirstOrDefault<news_main_files>();
                                    if(news_Main_Files_Del !=null)
                                    db.news_main_files.Remove(news_Main_Files_Del);
                                    if (item.Length > 0)
                                        delfiles.Add(root + item.Substring(8));
                                }
                                else
                                {
                                    newUrl += detached1 + item;
                                    detached1 = ",";
                                }

                            }
                            news.url_file = newUrl;

                        }
                        detached1 = "";
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
                            newFileName = Path.Combine(root + "/" + dvid + "/News", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/News", fileName);
                            }

                            if (helper.IsImageFileName(newFileName) && news.news_type == 0)
                            {

                                news.image = "/Portals/" + dvid + "/News/" + fileName;

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
                                if (news.url_file != "" && news.url_file != null)
                                    detached1 = ",";
                                news.url_file += detached1 + "/Portals/" + dvid + "/News/" + fileName;
                                detached1 = ",";
                                ffileData = fileData;
                                //Add file
                                if (fileInfo != null)
                                {
                                    if (!Directory.Exists(fileInfo.Directory.FullName))
                                    {
                                        Directory.CreateDirectory(fileInfo.Directory.FullName);
                                    }
                                    File.Move(ffileData.LocalFileName, newFileName);
                                    if (helper.IsImageFileName(newFileName))
                                        helper.ResizeImage(newFileName, 640, 640, 90);
                                }
                            }
                        }
                        if (newsOld.image != null && newsOld.image != "" && newsOld.image != news.image)
                        {
                            string fileOld = newsOld.image.Substring(8);
                            delfiles.Add(root + fileOld);
                        }
                        if (news.status == 2)
                        {

                            news.approved_by = uid;
                            news.approved_date = DateTime.Now;
                            news.approved_ip = ip;
                        }
                        if (news.news_type == 1)
                            news.image = null;
                        news.modified_date = DateTime.Now;
                        news.modified_by = uid;
                        news.modified_ip = ip;
                        db.Entry(news).State = EntityState.Modified;
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
                            log.task_id = news.news_id;
                            if (news.news_type == 0)
                                log.des = "Sửa tin tức" + news.news_id;
                            if (news.news_type == 1)
                                log.des = "Sửa thông báo" + news.news_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtintuc, contents }), domainurl + "CMS_New/Update_News", ip, tid, "Lỗi khi cập nhật Tin tức", 0, "News");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtintuc, contents }), domainurl + "CMS_New/Update_News", ip, tid, "Lỗi khi cập nhật Tin tức", 0, "News");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_news([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;

            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string root = HttpContext.Current.Server.MapPath("~/Portals");
            string strPath = root + "/" + dvid + "/News";

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var das = await db.news_main.Where(a => id.Contains(a.news_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    if (das != null)
                    {
                        List<news_main> del = new List<news_main>();
                        foreach (var da in das)
                        {
                            if (ad || da.created_by == uid)
                            {

                                del.Add(da);
                                if (!string.IsNullOrWhiteSpace(da.image))
                                    paths.Add(da.image);
                                if (!string.IsNullOrWhiteSpace(da.url_file))
                                {
                                    var arrF = da.url_file.Split(',');
                                    foreach (var item in arrF)
                                    {
                                        var news_Main_Files_Del = db.news_main_files.AsNoTracking().Where(s => s.file_path == item).FirstOrDefault<news_main_files>();
                                        if (news_Main_Files_Del  != null)
                                        db.news_main_files.Remove(news_Main_Files_Del);
                                        if (item.Length > 0)
                                            paths.Add(item);

                                    }

                                }
                                doc.LoadHtml(da.contents);
                                var imgs = doc.DocumentNode.SelectNodes("//img");
                                if (imgs != null)
                                {
                                    foreach (var img in imgs)
                                    {
                                        var imgdel = img.Attributes["src"].Value;
                                        imgdel = imgdel.Substring(imgdel.IndexOf("Portals/" + dvid + "/News") + 7);

                                        paths.Add(imgdel);

                                    }
                                }
                            }
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                task_log log = new task_log();
                                log.task_id = da.news_id;
                                if (da.news_type == 0)
                                    log.des = "Xóa tin tức" + da.news_id;
                                if (da.news_type == 1)
                                    log.des = "Xóa thông báo" + da.news_id;
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
                        db.news_main.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    foreach (string strP in paths)
                    {
                        bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/News/") + Path.GetFileName(strP));
                        if (exists)
                        {

                            System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/News/") + Path.GetFileName(strP));
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "news_main/Deletenews_main", ip, tid, "Lỗi khi xoá tin tức", 0, "News");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "news_main/Deletenews_main", ip, tid, "Lỗi khi xoá tin tức", 0, "News");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }


        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_ishot([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
        {

            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
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
                        var int_id = int.Parse(trangthai.IntID.ToString());
                        var das = db.news_main.Where(a => (a.news_id == int_id)).FirstOrDefault<news_main>();
                        if (das != null)
                        {

                            das.is_hot = !trangthai.BitTrangthai;
                            das.modified_date = DateTime.Now;
                            das.modified_by = uid;

                            das.modified_ip = ip;
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                task_log log = new task_log();
                                log.task_id = das.news_id;
                                if (das.news_type == 0)
                                    log.des = "Sửa tin tức" + das.news_id;
                                if (das.news_type == 1)
                                    log.des = "Sửa thông báo" + das.news_id;
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "news_main/Update_IsHot", ip, tid, "Lỗi khi cập nhật News", 0, "News");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "news_main/Update_IsHot", ip, tid, "Lỗi khi cập nhật News", 0, "News");
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
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
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
                        var int_id = int.Parse(trangthai.IntID.ToString());
                        var das = db.news_main.Where(a => (a.news_id == int_id)).FirstOrDefault<news_main>();
                        if (das != null)
                        {

                            das.status = trangthai.IntTrangthai;
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            if (das.status == 2)
                            {
                                das.approved_date = DateTime.Now;
                                das.approved_by = uid;
                                das.approved_ip = ip;
                            }
                            else
                            {
                                das.approved_date = null;
                                das.approved_by = null;
                                das.approved_ip = null;
                            }
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                task_log log = new task_log();
                                log.task_id = das.news_id;
                                if (das.news_type == 0)
                                    log.des = "Sửa tin tức" + das.news_id;
                                if (das.news_type == 1)
                                    log.des = "Sửa thông báo" + das.news_id;
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
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;


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
                    var das = db.news_main.Where(a => (a.news_id == int_id)).FirstOrDefault<news_main>();
                    if (das != null)
                    {
                        if (das.is_visitor == null)
                            das.is_visitor = 1;
                        else
                            das.is_visitor += 1;
                        var dew = db.news_users_visitor.Where(a => (a.user_id == visitor.user_id && a.news_id == visitor.IntID)).FirstOrDefault<news_users_visitor>();
                        if (dew != null)
                        {
                            dew.times += 1;
                            dew.modified_date = DateTime.Now;
                            dew.modified_by = uid;
                            db.SaveChanges();
                        }
                        else
                        {
                            news_users_visitor news_Users_Visitor = new news_users_visitor();
                            news_Users_Visitor.times = 1;
                            news_Users_Visitor.user_id = visitor.user_id;
                            news_Users_Visitor.news_id = visitor.IntID;
                            news_Users_Visitor.created_date = DateTime.Now;
                            news_Users_Visitor.created_by = uid;
                            news_Users_Visitor.created_ip = ip;
                            news_Users_Visitor.modified_date = DateTime.Now;
                            news_Users_Visitor.modified_by = uid;

                            db.news_users_visitor.Add(news_Users_Visitor);
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
    }
}
