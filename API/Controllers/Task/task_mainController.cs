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
using System.Web.Script.Serialization;
using API.Helper;
using API.Models;
using Helper;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class task_mainController : ApiController
    {
        public string getipaddress()
        {
       return  HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add_task()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdtask = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
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
                    string strPath = root + "/Task";
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
                        fdtask = provider.FormData.GetValues("task").SingleOrDefault();
                        task_main task_main = JsonConvert.DeserializeObject<task_main>(fdtask);
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(task_main.des);

                        var imgs = doc.DocumentNode.SelectNodes("//img");

                        if (imgs != null)
                        {

                            foreach (var img in imgs)
                            {

                                var pathFolderDes = "/Portals/Task";
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

                                    task_main.des = task_main.des.Replace(img.Attributes["src"].Value, domainurl + "/Portals/Task" + pathShow);
                                    helper.ResizeImage(domainurl + "/Portals/Task" + pathShow, 640, 640, 90);
                                }
                            }
                        }
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

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
                            newFileName = Path.Combine(root + "/Task", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/Task", fileName);
                            }

                            task_main.url_file += detached + "/Portals/Task/" + fileName;
                            detached = ",";
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }

                        }
                      
                        task_main.created_date = DateTime.Now;
                        task_main.created_by = uid;
                        task_main.modified_by = uid;
                        task_main.modified_date= DateTime.Now;
                        task_main.created_ip = ip;
                        task_main.created_token_id = tid;
                        db.task_main.Add(task_main);
                        db.SaveChanges();

                        #region add notify
                            task_notify task_Notify = new task_notify();
                            task_Notify.task_id = task_main.task_id;
                            task_Notify.user_send = uid;
                            task_Notify.user_receive = task_main.test_user_ids;
                            if (task_main.user_id != uid)
                                task_Notify.user_receive += "," + task_main.user_id;
                            task_Notify.send_time = DateTime.Now;
                            task_Notify.status = 0;
                            task_Notify.content = name + " thêm công việc: " + task_main.task_name;
                            task_Notify.created_by = uid;
                            task_Notify.created_date = DateTime.Now;
                            task_Notify.created_ip = ip;
                            task_Notify.created_token_id = tid;
                            db.task_notify.Add(task_Notify);
                            db.SaveChanges();
                        #endregion
                        #region add task_log
                        if (helper.wlog)
                        {

                            task_log log = new task_log();
                            log.task_id = task_main.task_id;
                            log.des = "Thêm công việc " + task_main.task_name;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtask, contents }), domainurl + "task_main/Add_task", ip, tid, "Lỗi khi thêm Task", 0, "Task");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtask, contents }), domainurl + "task_main/Add_task", ip, tid, "Lỗi khi thêm Task", 0, "Task  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_task()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdtask = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
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
                    string strPath = root + "/Task";
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
                        fdtask = provider.FormData.GetValues("task").SingleOrDefault();
                        task_main task_main = JsonConvert.DeserializeObject<task_main>(fdtask);
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(task_main.des);
                        var imgs = doc.DocumentNode.SelectNodes("//img");
                        if (imgs != null)
                        {
                            foreach (var img in imgs)
                            {
                                var pathFolderDes = "/Portals/Task";
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
                                    task_main.des = task_main.des.Replace(img.Attributes["src"].Value, domainurl + "/Portals/Task" + pathShow);
                                }
                            }
                        }

                        string urlFiles = "";


                        string detached1 = "";
                        var taskOld = db.task_main.AsNoTracking().Where(s => s.task_id == task_main.task_id).FirstOrDefault<task_main>();
                        doc.LoadHtml(taskOld.des);
                        imgs = doc.DocumentNode.SelectNodes("//img");
                        if (imgs != null)
                        {
                            foreach (var img in imgs)
                            {
                                var imgdel = img.Attributes["src"].Value;
                                imgdel =   imgdel.Substring(imgdel.IndexOf("Portals/Task") + 7);
                                if (!task_main.des.Contains(img.Attributes["src"].Value))
                                {
                                    delfiles.Add(imgdel);
                                }

                            }
                        }


                        if (taskOld.url_file != null && taskOld.url_file != "")
                        {
                            string[] listFile = task_main.url_file.Split(',');
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
                                    delfiles.Add(  listFileOld[i].Substring(8));
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
                            newFileName = Path.Combine(root + "/Task", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/Task", fileName);
                            }

                            if (urlFiles.Length > 0)
                            {
                                urlFiles += ",";
                            }
                            urlFiles += detached1 + "/Portals/Task/" + fileName;
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
                            task_main.url_file = null;
                        else
                            task_main.url_file = urlFiles;
                        if (task_main.status == 3)
                        {
                            task_main.end_date = DateTime.Now;
                        }
                        if (task_main.status == 2)
                        {
                            task_main.actual_date = DateTime.Now;
                            TimeSpan timeSpan = DateTime.Now.Subtract(task_main.created_date);

                            task_main.actual_hours = timeSpan.Days != null ? timeSpan.Days * 24 : 0 + timeSpan.Hours;
                        }
                        task_main.modified_by = uid;
                        task_main.modified_date = DateTime.Now;
                        db.Entry(task_main).State = EntityState.Modified;
                        db.SaveChanges();
                        foreach (string fpath in delfiles)
                        {
                            if (File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/Task/" + Path.GetFileName(fpath)))
                                File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/Task/" + Path.GetFileName(fpath));
                        }
                        if (task_main.test_user_ids.Length > 0)
                        {
                            var arrUserTest = task_main.test_user_ids.Split(',');
                            var testGroup = helper.GenKey();
                            var task_test_old = db.task_test.AsNoTracking().Where(s => s.task_id == task_main.task_id && s.is_main == true).ToArray<task_test>();

                            if (arrUserTest.Length > 0)
                            {
                                foreach (var item in task_test_old)
                                {
                                    var check = false;
                                    foreach (var element in arrUserTest)
                                    {
                                        if ((item.test_user_id == element || item.test_user_id == null) && item.is_main == true)
                                        {
                                            testGroup = item.test_group_id;
                                            check = true;
                                        }
                                    }
                                    if (!check)
                                    {
                                        var task_test_del = db.task_test.Where(s => s.task_id == task_main.task_id && s.test_group_id == item.test_group_id).ToList();
                                        foreach (var task1 in task_test_del)
                                        {
                                            task1.is_delete = true;
                                        }
                                        db.SaveChanges();
                                    }
                                }

                            }
                        }
                        #region add notify
                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        var obj = jss.Deserialize<dynamic>(fdtask);
                        var strImg = obj["update_dealine"];
                        if (strImg != true)
                        {
                            task_notify task_Notify = new task_notify();
                            task_Notify.task_id = task_main.task_id;
                            task_Notify.user_send = uid;
                            task_Notify.user_receive = task_main.test_user_ids;

                            task_Notify.send_time = DateTime.Now;
                            task_Notify.status = 0;
                            task_Notify.content = name + " cập nhật công việc: " + task_main.task_name;
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
                            log.task_id = task_main.task_id;
                            log.des = "Sửa  công việc " + task_main.task_name;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtask, contents }), domainurl + "task_main/Update_Task", ip, tid, "Lỗi khi cập nhật Task", 0, "Task");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtask, contents }), domainurl + "task_main/Update_Task", ip, tid, "Lỗi khi cập nhật Task", 0, "Task");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_task([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        var das = await db.task_main.Where(a => id.Contains(a.task_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<task_main> del = new List<task_main>();
                            foreach (var da in das)
                            {
                                if (uid == da.created_by)
                                {
                                    del.Add(da);
                                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                                    doc.LoadHtml(da.des);
                                    var imgs = doc.DocumentNode.SelectNodes("//img");
 
                                    if (imgs != null)
                                    {
                                        foreach (var img in imgs)
                                        {
                                            var imgdel = img.Attributes["src"].Value;
                                            imgdel =  imgdel.Substring(imgdel.IndexOf("Portals/Task") + 7);
                                            if (!da.des.Contains(img.Attributes["src"].Value))
                                            {
                                                paths.Add(imgdel);
                                            }

                                        }
                                    }
                                    if (!string.IsNullOrWhiteSpace(da.url_file))
                                    {
                                        string[] listFile = da.url_file.Split(',');
                                        for (int i = 0; i < listFile.Length; i++)
                                        {
                                            if (listFile[i].Length > 8)
                                            {

                                                paths.Add(  listFile[i].Substring(8));
                                            }

                                        }
                                    }

                                }
                                #region add notify
                                task_notify task_Notify = new task_notify();
                                task_Notify.task_id = da.task_id;
                                task_Notify.user_send = uid;
                                task_Notify.user_receive = da.test_user_ids;
                                if (da.user_id != uid)
                                    task_Notify.user_receive += "," + da.user_id;
                                task_Notify.send_time = DateTime.Now;
                                task_Notify.status = 0;
                                task_Notify.content = name + " xóa công việc: " + da.task_name;
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

                                    //task_log log = new task_log();
                                    //log.task_id = da.task_id;
                                    //log.des = "Xóa  công việc " + da.task_name;
                                    //log.created_date = DateTime.Now;
                                    //log.created_by = uid;
                                    //log.created_token_id = tid;
                                    //log.created_ip = ip;
                                    //db.task_log.Add(log);
                                    //db.SaveChanges();

                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.task_main.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                    
                        foreach (string fpath in paths)
                        {
                            if (File.Exists(root + "/Task/" + Path.GetFileName(fpath)))
                                File.Delete(root + "/Task/" + Path.GetFileName(fpath));
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_main/Delete_task", ip, tid, "Lỗi khi xoá tem", 0, "Task");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_main/Delete_task", ip, tid, "Lỗi khi xoá công việc", 0, "Task");
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
        public async Task<HttpResponseMessage> Update_StatusTask([System.Web.Mvc.Bind(Include = "IntID,IntTrangthai")] Trangthai status)
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
                        var das = db.task_main.Where(a => (a.task_id == status.IntID)).FirstOrDefault<task_main>();
                        if (das != null)
                        {

                            das.status = status.IntTrangthai;
                            das.modified_date = DateTime.Now;
                            das.modified_by = uid;
                            if (status.IntTrangthai == 2)
                            {
                                task_test task_Test = new task_test();
                                task_Test.test_group_id = helper.GenKey();
                                task_Test.task_id = das.task_id;
                                task_Test.is_delete = false;
                                task_Test.is_main = true;
                                task_Test.created_date = DateTime.Now;
                                task_Test.created_by = uid;
                                task_Test.created_ip = ip;
                                task_Test.created_token_id = tid;
                                db.task_test.Add(task_Test);
                                db.SaveChanges();
                                #region add notify
                                task_notify task_Noti = new task_notify();
                                task_Noti.task_id = das.task_id;
                                task_Noti.user_send = uid;
                                task_Noti.user_receive = das.test_user_ids;
                                task_Noti.send_time = DateTime.Now;
                                task_Noti.status = 0;
                                task_Noti.content = name + "đã chuyển Test công việc: " + das.task_name;
                                task_Noti.created_by = uid;
                                task_Noti.created_date = DateTime.Now;
                                task_Noti.created_ip = ip;
                                task_Noti.created_token_id = tid;
                                db.task_notify.Add(task_Noti);
                                db.SaveChanges();
                                #endregion
                            }
                            else
                            {
                                #region add notify
                                task_notify task_Notify = new task_notify();
                                task_Notify.task_id = das.task_id;
                                task_Notify.user_send = uid;
                                var listU = "";
                                var detach = "";
                                if (das.user_id == uid)
                                    task_Notify.user_receive = das.test_user_ids;
                                else
                                {
                                    if (das.test_user_ids != null && das.test_user_ids != "")
                                    {
                                        var arr = das.test_user_ids.Split(',');
                                        foreach (var item in arr)
                                        {
                                            if (uid != item)
                                            {
                                                listU += detach + item;
                                                detach = ",";
                                            }
                                        }
                                    }
                                }
                                if (listU !="")
                                task_Notify.user_receive = listU;
                                task_Notify.send_time = DateTime.Now;
                                task_Notify.status = 0;
                                task_Notify.content = name + " cập nhật trạng thái công việc: " + das.task_name;
                                task_Notify.created_by = uid;
                                task_Notify.created_date = DateTime.Now;
                                task_Notify.created_ip = ip;
                                task_Notify.created_token_id = tid;
                                db.task_notify.Add(task_Notify);
                                db.SaveChanges();
                                #endregion
                            }
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                task_log log = new task_log();
                                log.task_id = das.task_id;
                                log.des = "Sửa trạng thái công việc" + das.task_name;


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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = status.IntID, contents }), domainurl + "task_main/Update_TrangthaiTask", ip, tid, "Lỗi khi cập nhật trạng thái Tasks", 0, "task_main");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = status.IntID, contents }), domainurl + "task_main/Update_TrangthaiTask", ip, tid, "Lỗi khi cập nhật trạng thái Tasks", 0, "task_main");
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
        public async Task<HttpResponseMessage> Update_IsViewTest([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        int idr = int.Parse(id[0].ToString());
                        var das = db.task_main.Where(a => (a.task_id == idr)).FirstOrDefault<task_main>();
                        if (das != null)
                        {

                            var de = db.api_bug.Where(a => (a.task_id == idr)).ToList();
                            if (de != null)
                            {
                                var check = false;
                                foreach (var item in de)
                                {
                                    if (item.is_view_test == true)
                                    {
                                        check = true;
                                    }
                                }
                                if (check == false)
                                {
                                    var dy = db.task_main.Where(a => (a.task_id == idr)).FirstOrDefault<task_main>();
                                    dy.is_view_test  = false;
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_main/Update_IsViewTest", ip, tid, "Lỗi khi cập nhật Update_IsViewTest Tasks", 0, "task_main");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_main/Update_IsViewTest", ip, tid, "Lỗi khi cập nhật Update_IsViewTest Tasks", 0, "task_main");
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
        public async Task<HttpResponseMessage> Update_IsViewWork([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        int idr = int.Parse(id[0].ToString());
                            var de = db.api_bug.Where(a => (a.task_id == idr)).ToList();
                            if (de != null)
                            {
                                var check = false;
                                foreach (var item in de)
                                {
                                    if (item.is_view_work == true)
                                    {
                                        check = true;
                                    }
                                }
                                if (check == false)
                                {
                                    var dy = db.task_main.Where(a => (a.task_id == idr)).FirstOrDefault<task_main>();
                                    dy.is_view_work = false;
                                }
                            }
                            await db.SaveChangesAsync();
                        

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_main/Update_IsViewWork", ip, tid, "Lỗi khi cập nhật Update_IsViewWork Tasks", 0, "task_main");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_main/Update_IsViewWork", ip, tid, "Lỗi khi cập nhật Update_IsViewWork Tasks", 0, "task_main");
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
        [HttpPost]
        public async Task<HttpResponseMessage> Add_TaskTest([System.Web.Mvc.Bind(Include = "task_Test,api_Bugs")] modelTask_Test modelTask_Test)
        {
            var task_Test = modelTask_Test.task_Test;
            var list_Bugs = modelTask_Test.api_Bugs;
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

                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        string strPath = root + "/TestTask";
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);
                        foreach (var item in list_Bugs)
                        {
                            item.created_date = DateTime.Now;
                            item.created_by = uid;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.api_bug.Add(item);
                            db.SaveChanges();
                            if (task_Test.test_bugs.Length > 0)
                                task_Test.test_bugs += "," + item.bug_id;
                            else
                                task_Test.test_bugs += item.bug_id;
                        }
                        if (task_Test.test_content != null)
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(task_Test.test_content);
                            var imgs = doc.DocumentNode.SelectNodes("//img");
                            if (imgs != null)
                            {
                                foreach (var img in imgs)
                                {
                                    var pathFolderDes = "/Portals/TestTask";
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
                                        task_Test.test_content = task_Test.test_content.Replace(img.Attributes["src"].Value, domainurl + "/Portals/TestTask" + pathShow);
                                    }
                                }
                            }

                        }
                        if (task_Test.test_group_id == null)
                            task_Test.test_group_id = helper.GenKey();
                        task_Test.test_date = DateTime.Now;
                        task_Test.is_delete = false;
                        task_Test.created_date = DateTime.Now;
                        task_Test.created_by = uid;
                        task_Test.created_ip = ip;
                        task_Test.created_token_id = tid;
                        db.task_test.Add(task_Test);
                        #region add notify
                        var task_Main = db.task_main.Where(a => (a.task_id == task_Test.task_id)).FirstOrDefault();
                        task_notify task_Notify = new task_notify();
                        task_Notify.task_id = task_Test.task_id;
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
                            task_Notify.user_receive = listU+","+ task_Main.user_id;
                        task_Notify.send_time = DateTime.Now;
                        task_Notify.status = 0;
                        task_Notify.content = name + " thêm Test công việc: " + task_Main.task_name;
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
                            log.task_id = task_Test.task_id;
                            log.des = "Thêm Task Test" + task_Test.task_id;


                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_log.Add(log);
                            db.SaveChanges();

                        }
                        #endregion

                        await db.SaveChangesAsync();
                        var list_Task_Check = db.task_test.AsNoTracking().Where(s => s.test_group_id == task_Test.test_group_id && s.is_main == false && s.is_delete == false && s.test_pass == false).ToArray<task_test>();
                        var list_Task_Count = db.task_test.AsNoTracking().Where(s => s.test_group_id == task_Test.test_group_id && s.is_main == false && s.is_delete == false).ToArray<task_test>();
                        if (list_Task_Check.Length == 0 && list_Task_Count.Length > 0)
                        {
                            var task_Test_Save = db.task_test.Where(s => s.test_group_id == task_Test.test_group_id && s.is_main == true && s.is_delete == false).FirstOrDefault<task_test>();
                            task_Test_Save.test_pass = true;
                            db.SaveChanges();
                        }
                        else
                        {
                            if (list_Task_Check.Length > 0 && list_Task_Count.Length > 0)
                            {
                                var task_Test_Save = db.task_test.Where(s => s.test_group_id == task_Test.test_group_id && s.is_main == true && s.is_delete == false).FirstOrDefault<task_test>();
                                task_Test_Save.test_pass = false;
                                db.SaveChanges();
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "task_main/Add_TaskTest", ip, tid, "Lỗi khi cập nhật Add_TaskTest", 0, "task_main");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "task_main/Add_TaskTest", ip, tid, "Lỗi khi cập nhật Add_TaskTest", 0, "task_main");
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
        public async Task<HttpResponseMessage> Update_TaskTest([System.Web.Mvc.Bind(Include = "task_Test,api_Bugs")] modelTask_Test modelTask_Test)
        {
            var task_Test = modelTask_Test.task_Test;
            var list_Bugs = modelTask_Test.api_Bugs;
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


                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        string strPath = root + "/TestTask";
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);

                        List<string> delfiles = new List<string>();

                        foreach (var item in list_Bugs)
                        {
                            item.created_date = DateTime.Now;
                            item.created_by = uid;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.api_bug.Add(item);
                            db.SaveChanges();
                            if (task_Test.test_bugs!=null)
                                task_Test.test_bugs += "," + item.bug_id;
                            else
                                task_Test.test_bugs +=""+item.bug_id;
                        }
                        if (task_Test.test_content != null)
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(task_Test.test_content);
                            var imgs = doc.DocumentNode.SelectNodes("//img");
                            if (imgs != null)
                            {
                                foreach (var img in imgs)
                                {
                                    var pathFolderDes = "/Portals/TestTask";
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
                                        task_Test.test_content = task_Test.test_content.Replace(img.Attributes["src"].Value, domainurl + "/Portals/TestTask" + pathShow);
                                    }
                                }
                            }


                            doc.LoadHtml(task_Test.test_content);
                            imgs = doc.DocumentNode.SelectNodes("//img");
                            if (imgs != null)
                            {
                                foreach (var img in imgs)
                                {
                                    var imgdel = img.Attributes["src"].Value;
                                    imgdel = root + imgdel.Substring(imgdel.IndexOf("Portals/TestTask") + 7);
                                    if (!task_Test.test_content.Contains(img.Attributes["src"].Value))
                                    {
                                        delfiles.Add(imgdel);
                                    }

                                }
                            }
                        }


                        task_Test.test_date = DateTime.Now;
                        db.Entry(task_Test).State = EntityState.Modified;
                        db.SaveChanges();
                        var list_Task_Check = db.task_test.AsNoTracking().Where(s => s.test_group_id == task_Test.test_group_id && s.is_main == false && s.is_delete == false && s.test_pass == false).ToArray<task_test>();
                        var list_Task_Count = db.task_test.AsNoTracking().Where(s => s.test_group_id == task_Test.test_group_id && s.is_main == false && s.is_delete == false).ToArray<task_test>();
                        if (list_Task_Check.Length == 0 && list_Task_Count.Length>0)
                        {
                            var task_Test_Save = db.task_test.Where(s => s.test_group_id == task_Test.test_group_id && s.is_main == true && s.is_delete == false).FirstOrDefault<task_test>();
                            task_Test_Save.test_pass = true;
                            db.SaveChanges();
                        }
                        else
                        {
                            if (list_Task_Check.Length > 0 && list_Task_Count.Length > 0)
                            {
                                var task_Test_Save = db.task_test.Where(s => s.test_group_id == task_Test.test_group_id && s.is_main == true && s.is_delete == false).FirstOrDefault<task_test>();
                                task_Test_Save.test_pass = false;
                                db.SaveChanges();
                            }
                        }
                        foreach (string fpath in delfiles)
                        {
                            if (File.Exists(fpath))
                                File.Delete(fpath);
                        }
                        #region add notify
                        var task_Main = db.task_main.Where(a => (a.task_id == task_Test.task_id)).FirstOrDefault();
                        task_notify task_Notify = new task_notify();
                        task_Notify.task_id = task_Test.task_id;
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
                        task_Notify.content = name + " cập nhật Test công việc: " + task_Main.task_name;
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
                            log.task_id = task_Test.task_id;
                            log.des = "Cập nhật Task Test" + task_Test.task_id;
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
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "task_main/Add_TaskTest", ip, tid, "Lỗi khi cập nhật Add_TaskTest", 0, "task_main");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "task_main/Add_TaskTest", ip, tid, "Lỗi khi cập nhật Add_TaskTest", 0, "task_main");
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
        public async Task<HttpResponseMessage> Delete_TaskTest([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                List<string> delfiles = new List<string>();
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {

                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        string strPath = root + "/TestTask";
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);
                        var das = await db.task_test.Where(a => id.Contains(a.test_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<task_test> del = new List<task_test>();
                            foreach (var da in das)
                            {

                                if (uid == da.created_by)
                                {
                                    if (da.test_content != null)
                                    {
                                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                                        doc.LoadHtml(da.test_content);
                                        var imgs = doc.DocumentNode.SelectNodes("//img");

                                        if (imgs != null)
                                        {
                                            foreach (var img in imgs)
                                            {
                                                var imgdel = img.Attributes["src"].Value;
                                                imgdel =   imgdel.Substring(imgdel.IndexOf("Portals/TestTask") + 7);
                                                if (!da.test_content.Contains(img.Attributes["src"].Value))
                                                {
                                                    delfiles.Add(imgdel);
                                                }

                                            }
                                        }
                                    }
                                    del.Add(da);
                                }
                                #region add cms_logs
                                if (helper.wlog)
                                {

                                    task_log log = new task_log();
                                    log.task_id = da.task_id;
                                    log.des = "Xóa  Task Test " + da.test_id;
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
                            db.task_test.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        foreach (string fpath in delfiles)
                        {
                            if (File.Exists(root + "/TestTask/" + Path.GetFileName(fpath)))
                                File.Delete(root + "/TestTask/" + Path.GetFileName(fpath));
                        }
     

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_main/Delete_TaskTest", ip, tid, "Lỗi khi Test", 0, "task_main");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_main/Delete_TaskTest", ip, tid, "Lỗi khi xoá test", 0, "task_main");
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