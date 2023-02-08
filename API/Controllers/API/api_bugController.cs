using API.Helper;
using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class api_bugController : ApiController
    {
        public string getipaddress()
        {
   return  HttpContext.Current.Request.UserHostAddress;
        }
        //Thêm mới
        [HttpPost]
        public async Task<HttpResponseMessage> Add_bug()
        {
             var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdbug = "";
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
                    string strPath = root + "/Bug";
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
                        fdbug = provider.FormData.GetValues("bug").SingleOrDefault();
                        api_bug bug = JsonConvert.DeserializeObject<api_bug>(fdbug);
                        if (bug.des != null && bug.des != "")
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(bug.des);

                            var imgs = doc.DocumentNode.SelectNodes("//img");

                            if (imgs != null)
                            {

                                foreach (var img in imgs)
                                {

                                    var pathFolderDes = "/Portals/Bug";
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

                                        bug.des = bug.des.Replace(img.Attributes["src"].Value, domainurl + "/Portals/Bug" + pathShow);
                                        helper.ResizeImage(domainurl + "/Portals/Bug" + pathShow, 640, 640, 90);
                                    }
                                }
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
                            newFileName = Path.Combine(root + "/Bug", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/Bug", fileName);
                            }
                            bug.url_file = "/Portals/Bug/" + fileName;
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
                        bug.created_date = DateTime.Now;
                        bug.created_by = uid;
                        bug.created_ip = ip;
                        bug.created_token_id = tid;
                        db.api_bug.Add(bug);

                        db.SaveChanges();
                        if (bug.task_id != null)
                        {
                            var das = db.task_main.Where(a => (a.task_id == bug.task_id)).FirstOrDefault<task_main>();
                            if (das != null)
                            {

                                das.is_view_test = true;

                                db.SaveChanges();
                            }
                        }
                    
                        #region add notify
                    
                        var task_Main = db.task_main.Where(a => (a.task_id == bug.task_id)).FirstOrDefault();
                        task_notify task_Notify = new task_notify();
                        task_Notify.task_id = bug.task_id;
                        task_Notify.bug_id = bug.bug_id;
                      
                        task_Notify.user_send = task_Main.user_id;
                        task_Notify.user_receive = task_Main.test_user_ids;
                  


                        task_Notify.send_time = DateTime.Now;
                        task_Notify.status = 0;
                        task_Notify.content = name + " thêm lỗi: " + task_Main.task_name;
                        task_Notify.created_by = uid;
                        task_Notify.created_date = DateTime.Now;
                        task_Notify.created_ip = ip;
                        task_Notify.created_token_id = tid;
                        db.task_notify.Add(task_Notify);
                        db.SaveChanges();
                        #endregion

                    
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdbug, contents }), domainurl + "api_bug/Add_bug", ip, tid, "Lỗi khi thêm Bug", 0, "Bug");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdbug, contents }), domainurl + "api_bug/Add_bug", ip, tid, "Lỗi khi thêm Bug", 0, "Bug");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_bug()
        {
             var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdbug = "";
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
                    string strPath = root + "/Bug";
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
                        fdbug = provider.FormData.GetValues("bug").SingleOrDefault();
                        api_bug bug = JsonConvert.DeserializeObject<api_bug>(fdbug);
                        var BugOld = db.api_bug.AsNoTracking().Where(s => s.bug_id == bug.bug_id).FirstOrDefault<api_bug>();
                        if (bug.des != null && bug.des != "")
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(bug.des);
                            var imgs = doc.DocumentNode.SelectNodes("//img");
                            if (imgs != null)
                            {
                                foreach (var img in imgs)
                                {
                                    var pathFolderDes = "/Portals/Bug";
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
                                        bug.des = bug.des.Replace(img.Attributes["src"].Value, domainurl + "/Portals/Bug" + pathShow);
                                    }
                                }
                            }
                            if (BugOld.des != null && BugOld.des != "")
                            {
                                doc.LoadHtml(BugOld.des);
                                imgs = doc.DocumentNode.SelectNodes("//img");
                                if (imgs != null)
                                {
                                    foreach (var img in imgs)
                                    {
                                        var imgdel = img.Attributes["src"].Value;
                                        imgdel =  imgdel.Substring(imgdel.IndexOf("Portals/Bug") + 7);
                                        if (!bug.des.Contains(img.Attributes["src"].Value))
                                        {
                                            delfiles.Add(imgdel);
                                        }

                                    }
                                }
                            }
                        }
                        if (BugOld.url_file != null && BugOld.url_file != "")
                        {
                                string fileZ = BugOld.url_file.Substring(8);
                                delfiles.Add(  fileZ);
                            
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
                            newFileName = Path.Combine(root + "/Bug", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;
                                newFileName = Path.Combine(root + "/Bug", fileName);
                            }
                                bug.url_file =  "/Portals/Bug/" + fileName;
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
                   
                        db.Entry(bug).State = EntityState.Modified;
                        db.SaveChanges();

                        if (bug.task_id != null)
                        {
                            var das = db.task_main.Where(a => (a.task_id == bug.task_id)).FirstOrDefault<task_main>();
                            if (das != null)
                            {
                             
                                if (das.user_id == uid)
                                    das.is_view_work = true;
                                else
                                    das.is_view_test = true;
                                db.SaveChanges();
                            }
                        }
                        foreach (string fpath in delfiles)
                        {
                            if (File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/Bug/" + Path.GetFileName(fpath)))
                                File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/Bug/" + Path.GetFileName(fpath));
                        }
                        #region add notify

                        var task_Main = db.task_main.Where(a => (a.task_id == bug.task_id)).FirstOrDefault();
                        task_notify task_Notify = new task_notify();
                        task_Notify.task_id = bug.task_id;
                        task_Notify.bug_id = bug.bug_id;

                        task_Notify.user_send = task_Main.user_id;
                        task_Notify.user_receive = task_Main.test_user_ids;



                        task_Notify.send_time = DateTime.Now;
                        task_Notify.status = 0;
                        task_Notify.content = name + " cập nhật lỗi: " + task_Main.task_name;
                        task_Notify.created_by = uid;
                        task_Notify.created_date = DateTime.Now;
                        task_Notify.created_ip = ip;
                        task_Notify.created_token_id = tid;
                        db.task_notify.Add(task_Notify);
                        db.SaveChanges();
                        #endregion
                       
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdbug, contents }), domainurl + "api_bug/Update_bug", ip, tid, "Lỗi khi cập nhật Bug", 0, "Bug");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdbug, contents }), domainurl + "api_bug/Update_bug", ip, tid, "Lỗi khi cập nhật Bug", 0, "Bug");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_bug([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.api_bug.Where(a => id.Contains(a.bug_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<api_bug> del = new List<api_bug>();
                        
                            foreach (var da in das)
                            {
                                if (uid==da.created_by)
                                {
                                    del.Add(da);
                                
                                    if (!string.IsNullOrWhiteSpace(da.url_file))
                                        paths.Add( da.url_file );
                                }
                                #region add notify

                                var task_Main = db.task_main.Where(a => (a.task_id == da.task_id)).FirstOrDefault();
                                task_notify task_Notify = new task_notify();
                                task_Notify.task_id = da.task_id;
                                task_Notify.bug_id = da.bug_id;

                                task_Notify.user_send = task_Main.user_id;
                                task_Notify.user_receive = task_Main.test_user_ids;



                                task_Notify.send_time = DateTime.Now;
                                task_Notify.status = 0;
                                task_Notify.content = name + " xóa lỗi: " + task_Main.task_name;
                                task_Notify.created_by = uid;
                                task_Notify.created_date = DateTime.Now;
                                task_Notify.created_ip = ip;
                                task_Notify.created_token_id = tid;
                                db.task_notify.Add(task_Notify);
                                db.SaveChanges();
                                #endregion
                              
                            }
                          
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.api_bug.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals/Bug/") + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals/Bug/") + Path.GetFileName(strP));
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "api_bug/Delete_bug", ip, tid, "Lỗi khi xoá tem", 0, "Bug");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "api_bug/Delete_bug", ip, tid, "Lỗi khi xoá bug", 0, "Bug");
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
        public async Task<HttpResponseMessage> Update_TrangthaiBug([System.Web.Mvc.Bind(Include = "IntID,IntTrangthai")] Trangthai trangthai)
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
                        var das = db.api_bug.Where(a => (a.bug_id == trangthai.IntID)).FirstOrDefault<api_bug>();
                        if (das != null)
                        {
                            das.status = trangthai.IntTrangthai;
                            #region add notify

                            var task_Main = db.task_main.Where(a => (a.task_id == das.task_id)).FirstOrDefault();
                            task_notify task_Notify = new task_notify();
                            task_Notify.task_id = das.task_id;
                            task_Notify.bug_id = das.bug_id;
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
                            task_Notify.user_send = task_Main.user_id;
                
                            task_Notify.send_time = DateTime.Now;
                            task_Notify.status = 0;
                            task_Notify.content = name + " cập nhật trạng thái lỗi: " + task_Main.task_name;
                            task_Notify.created_by = uid;
                            task_Notify.created_date = DateTime.Now;
                            task_Notify.created_ip = ip;
                            task_Notify.created_token_id = tid;
                            db.task_notify.Add(task_Notify);
                            db.SaveChanges();
                            #endregion
                            
                            await db.SaveChangesAsync();
                        }
                        if (das.task_id != null)
                        {
                            var das1 = db.task_main.Where(a => (a.task_id == das.task_id)).FirstOrDefault<task_main>();
                            if (das1 != null)
                            {
                                if (das1.user_id == uid)
                                    das1.is_view_work = true;
                                else
                                    das1.is_view_test = true;
                                db.SaveChanges();
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "api_bug/Update_TrangthaiBug", ip, tid, "Lỗi khi cập nhật trạng thái Bugs", 0, "Bug");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "api_bug/Update_TrangthaiBug", ip, tid, "Lỗi khi cập nhật trạng thái Bugs", 0, "Bug");
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
                        var idr = id[0];
                        var das = db.api_bug.Where(a => (a.bug_id == idr)).FirstOrDefault<api_bug>();
                        if (das != null)
                        {

                            das.is_view_test = false;
                            var de = db.api_bug.Where(a => (a.task_id == das.task_id)).ToList();
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
                                    var dy = db.task_main.Where(a => (a.task_id == das.task_id)).FirstOrDefault<task_main>();
                                    dy.is_view_test = false;
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "api_Bug/Update_IsViewTest", ip, tid, "Lỗi khi cập nhật Update_IsViewTest Tasks", 0, "task_main");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "api_Bug/Update_IsViewTest", ip, tid, "Lỗi khi cập nhật Update_IsViewTest Tasks", 0, "task_main");
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
                        var idr = id[0];
                   
                        var das = db.api_bug.Where(a => (a.bug_id == idr)).FirstOrDefault<api_bug>();
                        if (das != null)
                        {

                            das.is_view_work = false;
                            var de = db.api_bug.Where(a => (a.task_id == das.task_id)).ToList();
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
                                    var dy = db.task_main.Where(a => (a.task_id == das.task_id)).FirstOrDefault<task_main>();
                                    dy.is_view_work = false;
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id[0], contents }), domainurl + "api_Bug/Update_IsViewWork", ip, tid, "Lỗi khi cập nhật Update_IsViewWork Tasks", 0, "task_main");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id[0], contents }), domainurl + "api_Bug/Update_IsViewWork", ip, tid, "Lỗi khi cập nhật Update_IsViewWork Tasks", 0, "task_main");
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
