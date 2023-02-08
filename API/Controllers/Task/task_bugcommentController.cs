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
using API.Helper;

namespace API.Controllers.Task
{
    [Authorize(Roles = "login")]
    public class task_bugcommentController : ApiController
    {

        public string getipaddress()
        {
       return  HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add_bugcomment()
        {

            var identity = User.Identity as ClaimsIdentity;
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
                    string strPath = root + "/CommentWork";
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
                        fdcmtbug = provider.FormData.GetValues("bugcomment").SingleOrDefault();
                        task_bugcomment cmtbug = JsonConvert.DeserializeObject<task_bugcomment>(fdcmtbug);
                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        var obj = jss.Deserialize<dynamic>(fdcmtbug);
                        var strImg = obj["strImg"];
                        var pathFolderDes = "/Portals/CommentWork";
                        var detached = "";
                        if(strImg!=null&& strImg!="")
                        foreach (var item in strImg.Split(','))
                        {
                            if ((item.Length % 4 == 0) && Regex.IsMatch(item, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                            {
                                byte[] bytes = Convert.FromBase64String(item);
                                    if (bytes.Length > 0)
                                    {
                                        bool existsFolder = System.IO.Directory.Exists(strPath + pathFolderDes);
                                if (!existsFolder)
                                {
                                    System.IO.Directory.CreateDirectory(strPath);
                                }

                                var typeFileHL = ".png";
                                var pathShow = "/" + helper.GenKey() + typeFileHL;
                                   
                                        using (var imageFile = new FileStream(strPath + pathShow, FileMode.Create))
                                        {
                                            imageFile.Write(bytes, 0, bytes.Length);
                                            imageFile.Flush();
                                        }
                                        cmtbug.url_file += detached + "/Portals/CommentWork" + pathShow;
                                        detached = ",";
                                    }
                            }
                        }
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        string listFiles = "";
                        detached = "";
                        //string strImg = fdcmtbug.strImg; 
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
                            newFileName = Path.Combine(root + "/CommentWork", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/CommentWork", fileName);
                            }

                            ffileData = fileData;
                            if (helper.IsImageFileName(newFileName))
                            {
                                listFiles += detached + "/Portals/CommentWork/" + fileName;
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
                                listFiles += detached + "/Portals/CommentWork/" + fileName;
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

                        if ((cmtbug.url_file != null || cmtbug.url_file != "") && listFiles != "")
                            cmtbug.url_file += "," + listFiles;
                        if ((cmtbug.url_file == null || cmtbug.url_file == "") && listFiles != "")
                            cmtbug.url_file = listFiles;
                        cmtbug.created_date = DateTime.Now;
                        cmtbug.created_by = uid;
                        cmtbug.created_ip = ip;
                        cmtbug.created_token_id = tid;
                        cmtbug.modified_by = uid;
                        cmtbug.modified_date = DateTime.Now;
                        db.task_bugcomment.Add(cmtbug);
                        db.SaveChanges();
                        var de = db.api_bug.Where(a => (a.bug_id == cmtbug.bug_id)).FirstOrDefault<api_bug>();
                        if (de != null)
                        {
                            var de1 = db.task_main.Where(a => (a.task_id == de.task_id)).FirstOrDefault<task_main>();
                            if (de1 != null)
                            {
                                if (de1.user_id == uid)
                                {
                                    de1.is_view_work = true;
                                    de.is_view_work = true;
                                }

                                else
                                {
                                    de1.is_view_test = true;
                                    de.is_view_test = true;
                                }
                                db.SaveChanges();
                            }
                        }
                        #region add notify
                        var api_Bug = db.api_bug.Where(a => (a.bug_id == cmtbug.bug_id)).FirstOrDefault();
                        var task_Main = db.task_main.Where(a => (a.task_id == api_Bug.task_id)).FirstOrDefault();
                        task_notify task_Notify = new task_notify();
                        task_Notify.task_id = api_Bug.task_id;
                        task_Notify.bug_id = api_Bug.bug_id;
                        task_Notify.bugcomment_id = cmtbug.bugcomment_id;
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
                        if (listU != ""  )
                                task_Notify.user_receive = listU + "," + task_Main.user_id;


                        task_Notify.send_time = DateTime.Now;
                        task_Notify.status = 0;
                        task_Notify.content = name + " thêm Check List: " + cmtbug.des;
                        task_Notify.created_by = uid;
                        task_Notify.created_date = DateTime.Now;
                        task_Notify.created_ip = ip;
                        task_Notify.created_token_id = tid;
                        db.task_notify.Add(task_Notify);
                        db.SaveChanges();
                        #endregion
                        #region add cms_logs
                        if (helper.wlog)
                        {

                            task_log log = new task_log();
                            log.task_id = cmtbug.bugcomment_id;
                            log.des = "Thêm lỗi công việc" + cmtbug.bugcomment_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "BugComment/Add_BugComment", ip, tid, "Lỗi khi thêm BugComment Work", 0, "BugComment");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "BugComment/Add_BugComment", ip, tid, "Lỗi khi thêm BugComment Work", 0, "BugComment");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_Bugcomment()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
           
            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string fdcmtbug = "";
                List<string> delfiles = new List<string>();
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
                        string strPath = root + "/CommentWork";
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
                            fdcmtbug = provider.FormData.GetValues("bugcomment").SingleOrDefault();
                            task_bugcomment cmtbug = JsonConvert.DeserializeObject<task_bugcomment>(fdcmtbug);

                            var cmtbugOld = db.task_bugcomment.AsNoTracking().Where(s => s.bugcomment_id == cmtbug.bugcomment_id).FirstOrDefault<task_bugcomment>();
                            if (cmtbugOld.url_file != null && cmtbugOld.url_file != "")
                            {
                             
                                    string[] listFileOld = cmtbugOld.url_file.Split(',');
                                if (cmtbug.url_file!=null && cmtbug.url_file!="")
                                for (int j = 0; j < listFileOld.Length; j++)
                                {
                                    if (!cmtbug.url_file.Contains(listFileOld[j]))
                                    {
                                        delfiles.Add( listFileOld[j].Substring(8));
                                    }
                                }
                                else
                                    for (int j = 0; j < listFileOld.Length; j++)
                                    {
                                            delfiles.Add(  listFileOld[j].Substring(8));
                                    }
                            }
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            var obj = jss.Deserialize<dynamic>(fdcmtbug);
                            var strImg = obj["strImg"];
                            var pathFolderDes = "/Portals/CommentWork";
                            var detached = "";
                            var listFileBase = "";
                            if (strImg != null&& strImg != "")
                                foreach (var item in strImg.Split(','))
                                {
                                    if ((item.Length % 4 == 0) && Regex.IsMatch(item, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                                    {
                                        byte[] bytes = Convert.FromBase64String(item);
                                        bool existsFolder = System.IO.Directory.Exists(strPath + pathFolderDes);
                                        if (!existsFolder)
                                        {
                                            System.IO.Directory.CreateDirectory(strPath);
                                        }

                                        var typeFileHL = ".png";
                                        var pathShow = "/" + helper.GenKey() + typeFileHL;
                                        using (var imageFile = new FileStream(strPath + pathShow, FileMode.Create))
                                        {
                                            imageFile.Write(bytes, 0, bytes.Length);
                                            imageFile.Flush();
                                        }
                                        listFileBase += detached + "/Portals/CommentWork" + pathShow;
                                        detached = ",";
                                    }
                                }
                            // This illustrates how to get thefile names.
                            FileInfo fileInfo = null;
                            MultipartFileData ffileData = null;
                            string newFileName = "";
                            string listFiles = "";
                            detached = "";
                            //string strImg = fdcmtbug.strImg; 
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
                                newFileName = Path.Combine(root + "/CommentWork", fileName);
                                fileInfo = new FileInfo(newFileName);
                                if (fileInfo.Exists)
                                {
                                    fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                    fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                    newFileName = Path.Combine(root + "/CommentWork", fileName);
                                }

                                ffileData = fileData;
                                if (helper.IsImageFileName(newFileName))
                                {
                                    listFiles += detached + "/Portals/CommentWork/" + fileName;
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
                                    listFiles += detached + "/Portals/CommentWork/" + fileName;
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


                            if (cmtbug.url_file == null)
                                cmtbug.url_file = "";

                            if (cmtbug.url_file != ""&& listFiles != "")
                                cmtbug.url_file += "," + listFiles;

                            if (cmtbug.url_file == "" && listFiles != "")
                                cmtbug.url_file = listFiles;
                            if (cmtbug.url_file != "" && listFileBase != "")
                                cmtbug.url_file += "," + listFileBase;
                          
                            if (cmtbug.url_file == "" && listFileBase != "")
                                cmtbug.url_file = listFileBase;
                            if (cmtbug.url_file == "")
                                cmtbug.url_file = null;
                                cmtbug.modified_by = uid;
                            cmtbug.modified_date = DateTime.Now;
                            if(cmtbug.status==1)
                                cmtbug.actual_date= DateTime.Now;
                            if (cmtbug.status == 3)
                                cmtbug.finished_date = DateTime.Now;
                            if (cmtbug.status == 2)
                                cmtbug.switch_test_date = DateTime.Now;
                            if (cmtbug.status == 2 || cmtbug.status == 1)
                            {
                                cmtbug.check_by = uid;
                                cmtbug.check_date = DateTime.Now;
                            }
                            db.Entry(cmtbug).State = EntityState.Modified;
                            db.SaveChanges();
                            var de = db.api_bug.Where(a => (a.bug_id == cmtbug.bug_id)).FirstOrDefault<api_bug>();
                            if (de != null)
                            {
                                var de1 = db.task_main.Where(a => (a.task_id == de.task_id)).FirstOrDefault<task_main>();
                                if (de1 != null)
                                {
                                    if (de1.user_id == uid)
                                    {
                                        de1.is_view_work = true;
                                        de.is_view_work = true;
                                    }

                                    else
                                    {
                                        de1.is_view_test = true;
                                        de.is_view_test = true;
                                    }
                                    db.SaveChanges();
                                }
                            }
                            foreach (string fpath in delfiles)
                            {
                                if (File.Exists(root +fpath))
                                    File.Delete(root + fpath);
                            }
                            #region add notify
                            var api_Bug = db.api_bug.Where(a => (a.bug_id == cmtbug.bug_id)).FirstOrDefault();
                            var task_Main = db.task_main.Where(a => (a.task_id == api_Bug.task_id)).FirstOrDefault();
                            task_notify task_Notify = new task_notify();
                            task_Notify.task_id = api_Bug.task_id;
                            task_Notify.bug_id = api_Bug.bug_id;
                            task_Notify.bugcomment_id = cmtbug.bugcomment_id;
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
                            if (listU != "")
                                task_Notify.user_receive = listU + "," + task_Main.user_id;


                            task_Notify.send_time = DateTime.Now;
                            task_Notify.status = 0;
                            task_Notify.content = name + " cập nhật Check List: " + cmtbug.des;
                            task_Notify.created_by = uid;
                            task_Notify.created_date = DateTime.Now;
                            task_Notify.created_ip = ip;
                            task_Notify.created_token_id = tid;
                            db.task_notify.Add(task_Notify);
                            db.SaveChanges();
                            #endregion
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                task_log log = new task_log();
                                log.task_id = cmtbug.bugcomment_id;
                                log.des = "Sửa lỗi công việc" + cmtbug.bugcomment_id;
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "task_bugcomment/Update_task_bugcomment", ip, tid, "Lỗi khi cập nhật task_bugcomment", 0, "task_bugcomment");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "task_bugcomment/Update_task_bugcomment", ip, tid, "Lỗi khi cập nhật task_bugcomment ", 0, "task_bugcomment");
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


        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Bugcomment([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
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
                        var das = await db.task_bugcomment.Where(a => id.Contains(a.bugcomment_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<task_bugcomment> del = new List<task_bugcomment>();
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

                                                paths.Add(listFile[i].Substring(8));


                                            }

                                        }
                                    }

                                }
                                #region add notify
                                var api_Bug = db.api_bug.Where(a => (a.bug_id == da.bug_id)).FirstOrDefault();
                                var task_Main = db.task_main.Where(a => (a.task_id == api_Bug.task_id)).FirstOrDefault();
                                task_notify task_Notify = new task_notify();
                                task_Notify.task_id = api_Bug.task_id;
                                task_Notify.bug_id = api_Bug.bug_id;
                                task_Notify.bugcomment_id = da.bugcomment_id;
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
                                if (listU != "")
                                    task_Notify.user_receive = listU + "," + task_Main.user_id;


                                task_Notify.send_time = DateTime.Now;
                                task_Notify.status = 0;
                                task_Notify.content = name + " xóa Check List: " + da.des;
                                task_Notify.created_by = uid;
                                task_Notify.created_date = DateTime.Now;
                                task_Notify.created_ip = ip;
                                task_Notify.created_token_id = tid;
                                db.task_notify.Add(task_Notify);
                                db.SaveChanges();
                                #endregion
                                #region add cms_logs
                                if (helper.wlog)
                                {

                                    task_log log = new task_log();
                                    log.task_id = da.bugcomment_id;
                                    log.des = "Xóa lỗi công việc" + da.bugcomment_id;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.task_log.Add(log);
                                    db.SaveChanges();

                                }
                                #endregion
                                var de = db.api_bug.Where(a => (a.bug_id == da.bug_id)).FirstOrDefault<api_bug>();
                                if (de != null)
                                {
                                    var de1 = db.task_main.Where(a => (a.task_id == de.task_id)).FirstOrDefault<task_main>();
                                    if (de1 != null)
                                    {

                                        de1.is_view_test = true;
                                        de.is_view_test = true;

                                        db.SaveChanges();
                                    }
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.task_bugcomment.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strPath in paths)
                        {
                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals")+"/CommentWork/" + Path.GetFileName(strPath));
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals ") +"/CommentWork/"+ Path.GetFileName(strPath));
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "BugComment/Delete_bugcomment", ip, tid, "Lỗi khi xoá BugComment Work", 0, "BugComment");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "BugComment/Delete_bugcomment", ip, tid, "Lỗi khi xoá BugComment Work", 0, "BugComment");
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
        public async Task<HttpResponseMessage> Update_GroupName([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> group_name)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
         
            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + " /";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var check = group_name[0].ToString();
                        var bug_id =int.Parse( group_name[2].ToString());
                        var das = await db.task_bugcomment.Where(a =>a.bug_id== bug_id && a.group_name.Contains(check)).ToListAsync();
                        if (das.Count > 0)
                        {
                            foreach (var da in das)
                            {

                                da.group_name = group_name[1].ToString();
                                #region add notify
                                 var api_Bug = db.api_bug.Where(a => (a.bug_id == da.bug_id)).FirstOrDefault();
                                var task_Main = db.task_main.Where(a => (a.task_id == api_Bug.task_id)).FirstOrDefault();
                                task_notify task_Notify = new task_notify();
                                task_Notify.task_id = api_Bug.task_id;
                                task_Notify.bug_id = api_Bug.bug_id;
                            
                                task_Notify.bugcomment_id = da.bugcomment_id;
                                task_Notify.user_send = task_Main.user_id;

                                task_Notify.user_receive = task_Main.test_user_ids;


                                task_Notify.send_time = DateTime.Now;
                                task_Notify.status = 0;
                                task_Notify.content = name + " đổi tên nhóm lỗi: " + check +"thành" + da.group_name;
                                task_Notify.created_by = uid;
                                task_Notify.created_date = DateTime.Now;
                                task_Notify.created_ip = ip;
                                task_Notify.created_token_id = tid;
                                db.task_notify.Add(task_Notify);

                                #endregion
                            

                                var de = db.api_bug.Where(a => (a.bug_id == da.bug_id)).FirstOrDefault<api_bug>();
                                if (de != null)
                                {
                                    var de1 = db.task_main.Where(a => (a.task_id == de.task_id)).FirstOrDefault<task_main>();
                                    if (de1 != null)
                                    {
                                        if (de1.user_id == uid)
                                        {
                                            de1.is_view_work = true;
                                            de.is_view_work = true;
                                        }

                                        else
                                        {
                                            de1.is_view_test = true;
                                            de.is_view_test = true;
                                        }
                                       
                                    }
                                }
                                db.SaveChanges();
                            }
                        }





                        await db.SaveChangesAsync();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                
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
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_StatusBugcomment([System.Web.Mvc.Bind(Include = "IntID,IntTrangthai")] Trangthai trangthai)
        {
            var identity = User.Identity as ClaimsIdentity;
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
                        var das = db.task_bugcomment.Where(a => (a.bugcomment_id == trangthai.IntID)).FirstOrDefault<task_bugcomment>();
                        if (trangthai.IntTrangthai == 1)
                            das.actual_date = DateTime.Now;
                        if (trangthai.IntTrangthai == 3)
                            das.finished_date = DateTime.Now;
                        if (trangthai.IntTrangthai  == 2)
                            das.switch_test_date = DateTime.Now;
                        if (das != null)
                        {

                            das.status = trangthai.IntTrangthai;
                            if (trangthai.IntTrangthai == 2|| trangthai.IntTrangthai == 1)
                            {
                                das.check_by = uid;
                                das.check_date = DateTime.Now;

                                #region add notify
                                var api_Bug = db.api_bug.Where(a => (a.bug_id == das.bug_id)).FirstOrDefault();
                                var task_Main = db.task_main.Where(a => (a.task_id == api_Bug.task_id)).FirstOrDefault();
                                task_notify task_Notify = new task_notify();
                                task_Notify.task_id = api_Bug.task_id;
                                task_Notify.bug_id = api_Bug.bug_id;
                                task_Notify.bugcomment_id = das.bugcomment_id;
                                task_Notify.user_send = task_Main.user_id;

                                task_Notify.user_receive = task_Main.test_user_ids;


                                task_Notify.send_time = DateTime.Now;
                                task_Notify.status = 0;
                                task_Notify.content = name + " chuyển Test: " + das.des;
                                task_Notify.created_by = uid;
                                task_Notify.created_date = DateTime.Now;
                                task_Notify.created_ip = ip;
                                task_Notify.created_token_id = tid;
                                db.task_notify.Add(task_Notify);
                                db.SaveChanges();
                                #endregion
                            }
                            else
                            {
                                #region add notify
                                var api_Bug = db.api_bug.Where(a => (a.bug_id == das.bug_id)).FirstOrDefault();
                                var task_Main = db.task_main.Where(a => (a.task_id == api_Bug.task_id)).FirstOrDefault();
                                task_notify task_Notify = new task_notify();
                                task_Notify.task_id = api_Bug.task_id;
                                task_Notify.bug_id = api_Bug.bug_id;
                                task_Notify.bugcomment_id = das.bugcomment_id;
                                task_Notify.user_send = task_Main.user_id;

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
                                if (listU != "")
                                    if (uid != task_Main.user_id)
                                        task_Notify.user_receive = listU + "," + task_Main.user_id;
                                    else
                                        task_Notify.user_receive = listU;


                                task_Notify.send_time = DateTime.Now;
                                task_Notify.status = 0;
                                task_Notify.content = name + " chuyển Check Test: " + das.des;
                                task_Notify.created_by = uid;
                                task_Notify.created_date = DateTime.Now;
                                task_Notify.created_ip = ip;
                                task_Notify.created_token_id = tid;
                                db.task_notify.Add(task_Notify);
                                db.SaveChanges();
                                #endregion
                               
                            }
                            var de = db.api_bug.Where(a => (a.bug_id == das.bug_id)).FirstOrDefault<api_bug>();
                            if (de != null)
                            {
                                var de1 = db.task_main.Where(a => (a.task_id == de.task_id)).FirstOrDefault<task_main>();
                                if (de1 != null)
                                {
                                    if (de1.user_id == uid)
                                    {
                                        de1.is_view_work = true;
                                        de.is_view_work = true;
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
                            if (helper.wlog)
                            {

                                cms_logs log = new cms_logs();
                                log.log_title = "Cập nhật người xử lý ";

                                log.log_module = "bình luận";
                                log.id_key = das.bugcomment_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.cms_logs.Add(log);
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "dispatch_Books/Update_StatusPlace", ip, tid, "Lỗi khi cập nhật trạng thái Công văn", 0, "dispatch_Books");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "dispatch_Books/Update_StatusPlace", ip, tid, "Lỗi khi cập nhật trạng thái Công văn", 0, "dispatch_Books");
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