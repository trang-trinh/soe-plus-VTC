using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
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
using OfficeOpenXml;
namespace API.Controllers.News
{
    [Authorize(Roles = "login")]
    public class BirthDay_CMTController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_Birthday_CMT()
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
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;

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
                    string strPath = root + "/BirthDay_CMT";
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
                        string receive_user = provider.FormData.GetValues("receive_user").SingleOrDefault();
                        string gif = provider.FormData.GetValues("gif").SingleOrDefault();
                        gif = gif.Replace("\"", "");
                        gif = gif.Replace("[", "");
                        gif = gif.Replace("]", "");

                        receive_user = receive_user.Replace("\"", "");
                        receive_user = receive_user.Trim();
                        birthday_cmt cmtbug = JsonConvert.DeserializeObject<birthday_cmt>(fdcmtbug);
                        // This illustrates how to get thefile names.
                        if (cmtbug.contents != null && cmtbug.contents != "")
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(cmtbug.contents);

                            var imgs = doc.DocumentNode.SelectNodes("//img");

                            var htmlBody = doc.DocumentNode.SelectSingleNode("//body");
                            if (imgs != null)
                            {

                                foreach (var img in imgs)
                                {
                                    HtmlNode oldChild = img;
                                    HtmlNode newChild = HtmlNode.CreateNode(img.OuterHtml); ;
                                    var pathFolderDes = "/Portals/BirthDay_CMT";
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


                                        img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;max-width:30vw;object-fit:contain");
                                        img.SetAttributeValue("src", domainurl + "/Portals/BirthDay_CMT" + pathShow);

                                        cmtbug.contents = cmtbug.contents.Replace(newChild.OuterHtml, img.OuterHtml);
                                        cmtbug.paths += (cmtbug.paths != null && cmtbug.paths != "" ? "," : "") + (pathShow == null || pathShow == "" ? "" : "/Portals/BirthDay_CMT" + pathShow);

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
                            newFileName = Path.Combine(root + "/BirthDay_CMT", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/BirthDay_CMT", fileName);
                            }

                            ffileData = fileData;
                            if (helper.IsImageFileName(newFileName))
                            {
                                listFiles += detached + "/Portals/BirthDay_CMT/" + fileName;
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
                                listFiles += detached + "/Portals/BirthDay_CMT/" + fileName;
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
                        listFiles = (listFiles != "" ? listFiles : "") + (gif != "" && listFiles != "" ? "," : "") + (gif != "" ? gif : "");

                        cmtbug.receive_user = receive_user;
                        cmtbug.created_by = uid;
                        cmtbug.image = listFiles;
                        cmtbug.created_date = DateTime.Now;
                        cmtbug.created_by = uid;
                        cmtbug.created_ip = ip;
                        cmtbug.created_token_ip = tid;
                        db.birthday_cmt.Add(cmtbug);
                        db.SaveChanges();
                        #region add notify
                        var de = db.birthday_cmt.Where(a => (a.id == cmtbug.id)).FirstOrDefault<birthday_cmt>();
                        task_notify task_Notify = new task_notify();
                        task_Notify.module_key = "SendSN";
                        task_Notify.user_send = uid;
                        task_Notify.user_receive = receive_user;
                        task_Notify.content = fname + " đã chúc mừng sinh nhật bạn!";
                        task_Notify.status = 0;
                        task_Notify.send_time = DateTime.Now;
                        task_Notify.created_date = DateTime.Now;
                        task_Notify.created_by = uid;
                        task_Notify.created_ip = ip;
                        task_Notify.created_token_id = tid;
                        db.task_notify.Add(task_Notify);
                        db.SaveChanges();
                        #endregion
                        #region add cms_logs
                        if (helper.wlog && cmtbug.id != null)
                        {

                            cms_logs log = new cms_logs();
                            log.log_id = cmtbug.id;
                            log.log_content = "Thêm lời chúc mừng sinh nhật" + cmtbug.id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.cms_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Birthday_CMT/add_Birthday_CMT", ip, tid, "Lỗi khi thêm BirthDay Comment", 0, "BirthDay_Comment");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Birthday_CMT/add_Birthday_CMT", ip, tid, "Lỗi khi thêm BirthDay Comment", 0, "BirthDay_Comment");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_BD_CMT()
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
                    string strPath = root + "/BirthDay_CMT";
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
                        string id = provider.FormData.GetValues("id").SingleOrDefault();
                        int int_id = int.Parse(id);
                        string gif = provider.FormData.GetValues("gif").SingleOrDefault();
                        gif = gif.Replace("\"", "");
                        gif = gif.Replace("[", "");
                        gif = gif.Replace("]", "");
                        birthday_cmt task_Comment = JsonConvert.DeserializeObject<birthday_cmt>(fdtask);
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(task_Comment.contents);
                        var imgs = doc.DocumentNode.SelectNodes("//img");
                        if (imgs != null)
                        {
                            foreach (var img in imgs)
                            {
                                var pathFolderDes = "/Portals/BirthDay_CMT";
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
                                    img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;max-width:30vw;object-fit:contain");
                                    img.SetAttributeValue("src", domainurl + "/Portals/BirthDay_CMT" + pathShow);
                                    task_Comment.contents = task_Comment.contents.Replace(img.Attributes["src"].Value, domainurl + "/Portals/BirthDay_CMT" + pathShow);
                                    task_Comment.paths += (task_Comment.paths != null && task_Comment.paths != "" ? "," : "") + (pathShow == null || pathShow == "" ? "" : "/Portals/BirthDay_CMT" + pathShow);
                                }
                            }
                        }
                        var oldCmt = db.birthday_cmt.AsNoTracking().Where(a => a.id == int_id).FirstOrDefault();
                        string[] newImg = gif != null ? gif.Split(',') : null;
                        string[] oldImg = oldCmt.image.Trim() != null ? oldCmt.image.Split(',') : null;
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
                            newFileName = Path.Combine(root + "/BirthDay_CMT", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/BirthDay_CMT", fileName);
                            }

                            ffileData = fileData;
                            if (helper.IsImageFileName(newFileName))
                            {
                                listFiles += detached + "/Portals/BirthDay_CMT/" + fileName;
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
                                listFiles += detached + "/Portals/BirthDay_CMT/" + fileName;
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

                        string urlFiles = "";
                        string detached1 = "";
                        if (oldCmt.image != null && oldCmt.image != "")
                        {
                            string[] listFile = gif.Split(',');
                            string[] listFileOld = oldCmt.image.Split(',');

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
                            string[] listFiles1 = urlFiles.Split(',');
                            for (int i = 0; i < listFileOld.Length; i++)
                            {
                                var checkDel = false;
                                for (int j = 0; j < listFiles1.Length; j++)
                                {
                                    if (listFileOld[i] == listFiles1[j])
                                    {
                                        checkDel = true;
                                    }
                                }
                                if (!checkDel)
                                    if (listFileOld[i].Contains("/Gif"))
                                    { }
                                    else
                                    { delfiles.Add(root + listFileOld[i].Substring(8)); }

                            }

                        }
                        if (!string.IsNullOrWhiteSpace(oldCmt.paths))
                        {
                            string[] listFile = oldCmt.paths.Split(',');
                            for (int i = 0; i < listFile.Length; i++)
                            {
                                delfiles.Add(root + listFile[i].Substring(8));
                            }
                        }
                        oldCmt.image = (gif != null || gif != "" ? gif : "") + (gif != null && gif != "" && listFiles != null && listFiles != "" ? "," : "")
                        + (listFiles != null && listFiles != "" ? listFiles : "");
                        oldCmt.contents = task_Comment.contents;
                        oldCmt.paths = task_Comment.paths;
                        db.Entry(oldCmt).State = EntityState.Modified;
                        db.SaveChanges();
                        foreach (string fpath in delfiles)
                        {
                            if (File.Exists(fpath))
                                File.Delete(fpath);
                        }
                        #region add cms_logs
                        if (helper.wlog)
                        {

                            cms_logs log = new cms_logs();
                            log.log_id = task_Comment.id;
                            log.log_module = "Truyền thông";
                            log.log_content = "Sửa lời chúc mừng sinh nhật" + task_Comment.id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.cms_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "/Birthday_CMTController/Edit_CMT", ip, tid, "Lỗi khi sửa lời chúc", 1, "birthday_cmt");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "/Birthday_CMTController/Edit_CMT", ip, tid, "Lỗi khi sửa lời chúc", 1, "birthday_cmt");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }


        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_BD_CMT([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    var das = await db.birthday_cmt.Where(a => id.Contains(a.id)).ToListAsync();

                    if (das.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                    }


                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<birthday_cmt> del = new List<birthday_cmt>();
                        foreach (var da in das)
                        {
                            if (uid == da.created_by)
                            {
                                del.Add(da);


                                if (!string.IsNullOrWhiteSpace(da.image))
                                {
                                    string[] listFile = da.image.Split(',');
                                    for (int i = 0; i < listFile.Length; i++)
                                    {
                                        if (listFile[i].Contains("/Portals/BirthDay_CMT") == true)
                                        {
                                            paths.Add(listFile[i]);
                                        }
                                    }
                                }
                                if (!string.IsNullOrWhiteSpace(da.paths))
                                {
                                    string[] listFile = da.paths.Split(',');
                                    for (int i = 0; i < listFile.Length; i++)
                                    {
                                        paths.Add(listFile[i]);
                                    }
                                }
                            }
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                var de = db.birthday_cmt.Where(a => (a.id == da.id)).FirstOrDefault<birthday_cmt>();
                                if (de.id != null)
                                {
                                    cms_logs log = new cms_logs();
                                    log.log_title = "Xóa lời chúc mừng sinh nhật ";

                                    log.log_module = "Truyền thông";
                                    log.id_key = da.id.ToString();
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
                        db.birthday_cmt.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    foreach (string strPath in paths)
                    {
                        bool exists = File.Exists(root + "/BirthDay_CMT/" + Path.GetFileName(strPath));
                        if (exists)
                            File.Delete(HttpContext.Current.Server.MapPath("~/Portals/BirthDay_CMT/") + Path.GetFileName(strPath));
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "/Birthday_CMTController/Delete_BD_CMT", ip, tid, "Lỗi khi xoá lời chúc", 1, "birthday_cmt");

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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "/Birthday_CMTController/Delete_BD_CMT", ip, tid, "Lỗi khi xóa lời chúc", 1, "birthday_cmt");

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
