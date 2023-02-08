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
using API.Helper;
using API.Models;
using Helper;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class api_commentbugController : ApiController
    {
        public string getipaddress()
        {
          return  HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_comment()
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
            string fdcmtbug = "";
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
                    string strPath = root + "/CommentBug";
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
                        fdcmtbug = provider.FormData.GetValues("comment").SingleOrDefault();
                        api_commentbug cmtbug = JsonConvert.DeserializeObject<api_commentbug>(fdcmtbug);
                        // This illustrates how to get thefile names.
                        if (cmtbug.des != null && cmtbug.des != "")
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(cmtbug.des);

                            var imgs = doc.DocumentNode.SelectNodes("//img");

                            var htmlBody = doc.DocumentNode.SelectSingleNode("//body");
                            if (imgs != null)
                            {

                                foreach (var img in imgs)
                                {
                                    HtmlNode oldChild = img;
                                    HtmlNode newChild = HtmlNode.CreateNode(img.OuterHtml); ;
                                    var pathFolderDes = "/Portals/CommentBug";
                                    var checkBase64 = newChild.Attributes["src"].Value.Substring(newChild.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                                    checkBase64 = checkBase64.Trim();
                                    if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                                    {
                                        byte[] bytes = Convert.FromBase64String(newChild.Attributes["src"].Value.Substring(newChild.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                                        bool existsFolder = System.IO.Directory.Exists(strPath + pathFolderDes);
                                        if (!existsFolder)
                                        {
                                            System.IO.Directory.CreateDirectory(strPath);
                                        }

                                        var index1 = newChild.Attributes["src"].Value.LastIndexOf("data:image/") + 11;
                                        var index2 = newChild.Attributes["src"].Value.IndexOf("base64,");
                                        var typeFileHL = "." + newChild.Attributes["src"].Value.Substring(index1, index2 - index1 - 1);
                                        var pathShow = "/" + helper.GenKey() + typeFileHL;
                                        using (var imageFile = new FileStream(strPath + pathShow, FileMode.Create))
                                        {
                                            imageFile.Write(bytes, 0, bytes.Length);
                                            imageFile.Flush();
                                        }


                                        img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;object-fit:contain");
                                        img.SetAttributeValue("src", domainurl + "/Portals/CommentBug" + pathShow);
                                       
                                        cmtbug.des = cmtbug.des.Replace(newChild.OuterHtml, img.OuterHtml);
                                    
                                    }
                                }
                                
                            }
                           
                        }
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        string listFiles = "";
                        string detached = "";
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
                            newFileName = Path.Combine(root + "/CommentBug", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/CommentBug", fileName);
                            }

                            ffileData = fileData;
                            if (helper.IsImageFileName(newFileName))
                            {
                                listFiles += detached + "/Portals/CommentBug/" + fileName;
                                detached = ",";
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
                                listFiles += detached + "/Portals/CommentBug/" + fileName;
                                detached = ",";
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
                        cmtbug.user_id = uid;
                        cmtbug.url_file = listFiles;
                        cmtbug.created_date = DateTime.Now;
                        cmtbug.created_by = uid;
                        cmtbug.created_ip = ip;
                        cmtbug.created_token_id = tid;
                        db.api_commentbug.Add(cmtbug);
                        db.SaveChanges();
                    #region add notify
                    var de = db.api_bug.Where(a => (a.bug_id == cmtbug.bug_id)).FirstOrDefault<api_bug>();
                        if (de.task_id != null)
                        {
                            var task_Main = db.task_main.Where(a => (a.task_id == de.task_id)).FirstOrDefault();
                            task_notify task_Notify = new task_notify();
                            task_Notify.task_id = de.task_id;

                            task_Notify.user_send = uid;

                            var listU = "";
                            var detach = "";

                            if (task_Main.test_user_ids != null && task_Main.test_user_ids != "")
                            {
                                var arr = task_Main.test_user_ids.Split(',');
                                foreach (var item in arr)
                                {
                                    if (uid != item)
                                    {
                                        listU += detach + item;
                                        detach = ",";
                                    }
                                }
                            }

                            if (listU != "" && task_Main.user_id != uid)
                                task_Notify.user_receive = listU + "," + task_Main.user_id;

                            task_Notify.send_time = DateTime.Now;
                            task_Notify.status = 0;
                            task_Notify.content = name + " thêm bình luận: " + task_Main.task_name;
                            task_Notify.created_by = uid;
                            task_Notify.created_date = DateTime.Now;
                            task_Notify.created_ip = ip;
                            task_Notify.created_token_id = tid;
                            db.task_notify.Add(task_Notify);
                            db.SaveChanges();
                        }
                        #endregion
                    
                        if (de.task_id != null)
                        {
                            var de1 = db.task_main.Where(a => (a.task_id == de.task_id)).FirstOrDefault<task_main>();
                            if (de1 != null)
                            {
                                if (de1.user_id == uid)
                                {
                                    de.is_view_work = true;
                                    de1.is_view_work = true;
                                }
                                else
                                {
                                    de1.is_view_test = true;
                                    de.is_view_test = true;
                                }
                                    
                                db.SaveChanges();
                            }
                        }
                        #region add cms_logs
                        if (helper.wlog && de.task_id!=null)
                        {

                            task_log log = new task_log();
                            log.task_id = cmtbug.comment_id;
                            log.des = "Thêm bình luận check list" + cmtbug.comment_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Commentbug/Add_Commentbug", ip, tid, "Lỗi khi thêm Comment Bug", 0, "Commentbug");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Commentbug/Add_Commentbug", ip, tid, "Lỗi khi thêm Comment Bug", 0, "Commentbug");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_comment()
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
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                List<string> delfiles = new List<string>(); string fdtask = "";
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
                        string strPath = root + "/CommentBug";
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
                            fdtask = provider.FormData.GetValues("comment").SingleOrDefault();
                            api_commentbug task_Comment = JsonConvert.DeserializeObject<api_commentbug>(fdtask);
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(task_Comment.des);
                            var imgs = doc.DocumentNode.SelectNodes("//img");
                            if (imgs != null)
                            {
                                foreach (var img in imgs)
                                {
                                    var pathFolderDes = "/Portals/CommentBug";
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
                                        task_Comment.des = task_Comment.des.Replace(img.Attributes["src"].Value, domainurl + "/Portals/CommentBug" + pathShow);
                                    }
                                }
                            }

                            string urlFiles = "";


                            string detached1 = "";
                            var taskOld = db.api_commentbug.AsNoTracking().Where(s => s.comment_id == task_Comment.comment_id).FirstOrDefault<api_commentbug>();
                            doc.LoadHtml(taskOld.des);
                            imgs = doc.DocumentNode.SelectNodes("//img");
                            if (imgs != null)
                            {
                                foreach (var img in imgs)
                                {
                                    var imgdel = img.Attributes["src"].Value;
                                    imgdel =   imgdel.Substring(imgdel.IndexOf("Portals/CommentBug") + 7);
                                    if (!task_Comment.des.Contains(img.Attributes["src"].Value))
                                    {
                                        delfiles.Add(imgdel);
                                    }

                                }
                            }


                            if (taskOld.url_file != null && taskOld.url_file != "")
                            {
                                string[] listFile = task_Comment.url_file.Split(',');
                                string[] listFileOld = taskOld.url_file.Split(',');

                                for (int i = 0; i < listFile.Length; i++)
                                {

                                    for (int j = 0; j < listFileOld.Length; j++)
                                    {


                                        if (listFile[i] == listFileOld[j])
                                        {


                                            urlFiles += listFile[i];
                                            if (i < listFile.Length - 1)
                                                urlFiles += ',';

                                        }
                                    }

                                }
                                string[] listFiles = urlFiles.Split(',');
                                for (int i = 0; i < listFileOld.Length; i++)
                                {
                                    var checkDel = false;
                                    for (int j = 0; j < listFiles.Length; j++)
                                    {
                                        if (listFileOld[i] == listFiles[j])
                                        {
                                            checkDel = true;


                                        }
                                    }
                                    if (!checkDel)
                                        delfiles.Add( listFileOld[i].Substring(8));
                                }

                            }
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
                                newFileName = Path.Combine(root + "/CommentBug", fileName);
                                fileInfo = new FileInfo(newFileName);
                                if (fileInfo.Exists)
                                {
                                    fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                    fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                    newFileName = Path.Combine(root + "/CommentBug", fileName);
                                }

                                if (urlFiles.Length > 0)
                                {
                                    urlFiles += ",";
                                }
                                urlFiles += detached1 + "/Portals/CommentBug/" + fileName;
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
                            if (urlFiles == "")
                                task_Comment.url_file = null;
                            else
                                task_Comment.url_file = urlFiles;

                            db.Entry(task_Comment).State = EntityState.Modified;
                            db.SaveChanges();
                            foreach (string fpath in delfiles)
                            {
                                if (File.Exists(HttpContext.Current.Server.MapPath("~/Portals/CommentBug/") + Path.GetFileName(fpath)))
                                    File.Delete(HttpContext.Current.Server.MapPath("~/Portals/CommentBug/") + Path.GetFileName(fpath));
                            }
                            #region add notify
                            var de = db.api_bug.Where(a => (a.bug_id == task_Comment.bug_id)).FirstOrDefault<api_bug>();
                            if (de != null)
                            {
                                var de1 = db.task_main.Where(a => (a.task_id == de.task_id)).FirstOrDefault<task_main>();
                                if (de1 != null)
                                {
                                    if (de1.user_id == uid)
                                    {
                                        de.is_view_work = true;
                                        de1.is_view_work = true;
                                    }
                                    else
                                    {
                                        de1.is_view_test = true;
                                        de.is_view_test = true;
                                    }

                                    db.SaveChanges();
                                }
                            }
                            var api_Bugcmt = db.api_bug.Where(a => (a.bug_id == task_Comment.bug_id)).FirstOrDefault();
                            if (api_Bugcmt.task_id != null)
                            {
                                var task_Main = db.task_main.Where(a => (a.task_id == api_Bugcmt.task_id)).FirstOrDefault();
                                task_notify task_Notify = new task_notify();
                                task_Notify.task_id = api_Bugcmt.task_id;
                                task_Notify.user_send = uid;
                                var listU = "";
                                var detach = "";

                                if (task_Main.test_user_ids != null && task_Main.test_user_ids != "")
                                {
                                    var arr = task_Main.test_user_ids.Split(',');
                                    foreach (var item in arr)
                                    {
                                        if (uid != item)
                                        {
                                            listU += detach + item;
                                            detach = ",";
                                        }
                                    }
                                }
                                if (listU != "" && task_Main.user_id != uid)
                                    task_Notify.user_receive = listU + "," + task_Main.user_id;

                                task_Notify.send_time = DateTime.Now;
                                task_Notify.status = 0;
                                task_Notify.content = name + " sửa bình luận trong: " + task_Main.task_name;
                                task_Notify.created_by = uid;
                                task_Notify.created_date = DateTime.Now;
                                task_Notify.created_ip = ip;
                                task_Notify.created_token_id = tid;
                                db.task_notify.Add(task_Notify);
                                db.SaveChanges();
                            }
                            #endregion
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                task_log log = new task_log();
                                log.task_id = task_Comment.comment_id;
                                log.des = "Sửa bình luận công việc" + task_Comment.comment_id;
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "task_comment/Update_task_comment", ip, tid, "Lỗi khi cập nhật task_comment", 0, "task_comment");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "task_comment/Update_task_comment", ip, tid, "Lỗi khi cập nhật task_comment ", 0, "task_comment");
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
         

        public List<int> deleteChild([System.Web.Mvc.Bind(Include = "")] List<int> id)
        {
            List<int> del = new List<int>();
            using (DBEntities db = new DBEntities())
            {

                var das = db.api_commentbug.Where(a => id.Contains(a.comment_id)).ToArray();

                if (das != null)
                {


                    foreach (var da in das)
                    {
                        var arrC = db.api_commentbug.Where(a => a.parent_id != null).ToArray();
                        del.Add(da.comment_id);
                        var arrId = new List<int>();
                        for (int i = 0; i < id.Count; i++)
                        {
                            for (int j = 0; j < arrC.Length; j++)
                            {
                                if (id[i] == arrC[j].parent_id)
                                {

                                    arrId.Add(arrC[j].comment_id);
                                    del.AddRange(deleteChild(arrId));
                                }
                            }
                        }



                    }


                }


            }
            return del;
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> delete_comment([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var arrC = deleteChild(id);
                        var das = await db.api_commentbug.Where(a => arrC.Contains(a.comment_id)).ToListAsync();

                        if (das.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }


                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<api_commentbug> del = new List<api_commentbug>();
                            foreach (var da in das)
                            {
                                if (uid==da.created_by)
                                {
                                    del.Add(da);


                                    if (!string.IsNullOrWhiteSpace(da.url_file))
                                    {
                                        string[] listFile = da.url_file.Split(',');
                                        for (int i = 0; i < listFile.Length; i++)
                                        {
                                            if (listFile[i].Length > 8)
                                            {

                                                paths.Add(  listFile[i] );


                                            }

                                        }
                                    }

                                }
                                #region add cms_logs
                                if (helper.wlog )
                                {
                                    var de = db.api_bug.Where(a => (a.bug_id == da.bug_id)).FirstOrDefault<api_bug>();
                                    if (de.task_id != null)
                                    {
                                        var task_B = db.task_main.Where(a => (a.task_id == de.task_id)).FirstOrDefault();
                                        if (helper.wlog && task_B.task_id != null)
                                        {
                                            task_log log1 = new task_log();
                                            log1.task_id = da.comment_id;
                                            log1.des = "Xóa bình luận Check List" + da.comment_id;
                                            log1.created_date = DateTime.Now;
                                            log1.created_by = uid;
                                            log1.created_token_id = tid;
                                            log1.created_ip = ip;
                                            db.task_log.Add(log1);
                                            db.SaveChanges();


                                        }
                                    }
                                    else
                                    {
                                        cms_logs log = new cms_logs();
                                        log.log_title = "Xóa bình luận ";

                                        log.log_module = "Bình luận";
                                        log.id_key = da.comment_id.ToString();
                                        log.created_date = DateTime.Now;
                                        log.created_by = uid;
                                        log.created_token_id = tid;
                                        log.created_ip = ip;
                                        db.cms_logs.Add(log);
                                        db.SaveChanges();
                                    }
                                }
                                #endregion
                               
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.api_commentbug.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                      
                        foreach (string strPath in paths)
                        {
                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals/CommentBug/") + Path.GetFileName(strPath));
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals/CommentBug/") + Path.GetFileName(strPath));
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Commentbug/Delete_commentbug", ip, tid, "Lỗi khi xoá Comment Bug", 0, "Commentbug");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Commentbug/Delete_commentbug", ip, tid, "Lỗi khi xoá Comment Bug", 0, "Commentbug");
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
        public async Task<HttpResponseMessage> Update_CheckComment([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = db.api_commentbug.Where(a => (a.comment_id == trangthai.IntID)).FirstOrDefault<api_commentbug>();
                        if (das != null)
                        {

                       
                            if (trangthai.BitTrangthai == false)
                            {
                                das.created_by = uid;
                            }
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                task_log log = new task_log();
                                log.task_id = das.comment_id;
                                log.des = "Cập nhật người xử lý " + das.comment_id;
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "api_commentbug/Update_CheckComment", ip, tid, "Lỗi khi cập nhật Update_CheckComment", 0, "api_commentbug");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "api_commentbug/Update_CheckComment", ip, tid, "Lỗi khi cập nhật Update_CheckComment", 0, "api_commentbug");
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