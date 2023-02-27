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
using API.Models;
using Helper;
using HtmlAgilityPack;
using API.Helper;
using Newtonsoft.Json;
using OfficeOpenXml;
namespace API.Controllers.Task_Ca
{
    [Authorize(Roles = "login")]
    public class task_commentsController : ApiController
    {
        string module_key = "M4";
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_Comments()
        {

            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;

            string fdcmtbug = "";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
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
                    var provider = new MultipartFormDataStreamProvider(root);
                    List<task_file> task_Files = new List<task_file>();
                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdcmtbug = provider.FormData.GetValues("comment").SingleOrDefault();
                        string id = provider.FormData.GetValues("task_id").SingleOrDefault();
                        string is_reply = provider.FormData.GetValues("is_reply").SingleOrDefault();
                        is_reply = is_reply.Replace("\"", "");
                        is_reply = is_reply.Trim();
                        task_comments cmtbug = JsonConvert.DeserializeObject<task_comments>(fdcmtbug);
                        cmtbug.comment_id = helper.GenKey();
                        cmtbug.parent_id = is_reply == "true" ? provider.FormData.GetValues("parent_id").SingleOrDefault() : null;
                        cmtbug.parent_id = is_reply == "true" ? cmtbug.parent_id.Replace("\"", "") : null;
                        cmtbug.parent_id = is_reply == "true" ? cmtbug.parent_id.Trim() : null;
                        id = id.Replace("\"", "");
                        id = id.Trim();
                        // This illustrates how to get thefile names.
                        string strPath = root + "/" + dvid + "/TaskOrigin/" + id + "/" + cmtbug.comment_id;
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);
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

                                    var checkBase64 = newChild.Attributes["src"].Value.Substring(newChild.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                                    checkBase64 = checkBase64.Trim();
                                    if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                                    {
                                        byte[] bytes = Convert.FromBase64String(newChild.Attributes["src"].Value.Substring(newChild.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                                        bool existsFolder = System.IO.Directory.Exists(strPath);
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


                                        img.SetAttributeValue("style", "width:100%;max-width:30vw;object-fit:contain");
                                        img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/TaskOrigin/" + id + "/" + cmtbug.comment_id + pathShow);
                                        cmtbug.contents = cmtbug.contents.Replace(newChild.OuterHtml, img.OuterHtml);
                                        task_file task_File = new task_file();
                                        task_File.file_id = helper.GenKey();
                                        task_File.comment_id = cmtbug.comment_id;
                                        task_File.file_name = pathShow.Replace("/", "");
                                        task_File.file_path = "/Portals" + "/" + dvid + "/TaskOrigin/" + id + "/" + cmtbug.comment_id + pathShow;
                                        task_File.file_type = typeFileHL.Substring(1);
                                        var file_info = new FileInfo(root + "/" + dvid + "/TaskOrigin/" + id + "/" + cmtbug.comment_id + pathShow);
                                        task_File.file_size = file_info.Length;
                                        task_File.is_image = helper.IsImageFileName(pathShow);
                                        task_File.is_type = 2;
                                        task_File.status = false;
                                        task_File.created_by = uid;
                                        task_File.created_date = DateTime.Now;
                                        task_File.created_ip = ip;
                                        task_File.task_id = id;
                                        task_File.created_token_id = tid;
                                        task_Files.Add(task_File);
                                    }
                                }

                            }

                        }
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
                            newFileName = Path.Combine(strPath, fileName);
                            fileInfo = new FileInfo(newFileName);
                            string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                            if (fileInfo != null)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + "_(" + helper.ranNumberFile() + ")" + fileInfo.Extension;

                                if (fileName.Length > 500)
                                {
                                    fileName = fileName.Substring(0, fileName.LastIndexOf('.') - 1);
                                    int le = 500 - (strPath.Length + 1) - Dinhdang.Length;
                                    fileName = fileName.Substring(0, le) + fileName;
                                }
                                newFileName = Path.Combine(strPath, fileName);
                            }
                            ffileData = fileData;
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }
                            task_file task_File = new task_file();
                            task_File.file_id = helper.GenKey();
                            task_File.comment_id = cmtbug.comment_id;
                            task_File.task_id = id;
                            task_File.file_name = fileName;
                            task_File.file_path = "/" + "Portals" + "/" + dvid + "/TaskOrigin/" + id + "/" + cmtbug.comment_id + "/" + fileName;
                            task_File.file_type = Dinhdang;
                            var file_info = new FileInfo(strPath + "/" + fileName);
                            task_File.file_size = file_info.Length;
                            task_File.is_image = helper.IsImageFileName(fileName);
                            task_File.is_type = 2;
                            task_File.status = true;
                            task_File.created_by = uid;
                            task_File.created_date = DateTime.Now;
                            task_File.created_ip = ip;
                            task_File.created_token_id = tid;
                            task_Files.Add(task_File);
                        }
                        cmtbug.task_id = id;
                        cmtbug.is_type = 1;
                        cmtbug.is_delete = false;
                        cmtbug.created_by = uid;
                        cmtbug.created_date = DateTime.Now;
                        cmtbug.created_by = uid;
                        cmtbug.created_ip = ip;
                        cmtbug.created_token_id = tid;
                        db.task_file.AddRange(task_Files);
                        db.task_comments.Add(cmtbug);
                        db.SaveChanges();
                        #region add notify
                        var listuser = db.task_member.Where(x => x.task_id == cmtbug.task_id).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == cmtbug.task_id).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);
                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Đã bình luận công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, cmtbug.task_id, null, null, tid, ip);
                        }
                        #endregion
                        #region add task_logs
                        if (helper.wlog && cmtbug.comment_id != null)
                        {
                            var task = db.task_origin.Where(x => x.task_id == id).FirstOrDefault();
                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = id;
                            log.comment_id = cmtbug.comment_id;
                            log.project_id = null;
                            log.description = "Thêm mới bình luận công việc " + "'<b>" + task.task_name + "</b>'";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_comments/add_Task_Comments", ip, tid, "Lỗi khi thêm bình luận công việc", 0, "task_comments");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_comments/add_Task_Comments", ip, tid, "Lỗi khi thêm bình luận công việc", 0, "task_comments");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> updateComments()
        {
            string rootDel = HttpContext.Current.Server.MapPath("~/");
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string fdcmtbug = "";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
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
                    var provider = new MultipartFormDataStreamProvider(root);
                    List<task_file> task_Files = new List<task_file>();
                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        fdcmtbug = provider.FormData.GetValues("comment").SingleOrDefault();
                        string id = provider.FormData.GetValues("task_id").SingleOrDefault();
                        id = id.Replace("\"", "");
                        id = id.Trim();
                        string cmt_id = provider.FormData.GetValues("cmt_id").SingleOrDefault();
                        cmt_id = cmt_id.Replace("\"", "");
                        cmt_id = cmt_id.Trim();
                        string delFilesID = provider.FormData.GetValues("Del_file_ID").SingleOrDefault();

                        foreach (var idwww in delFilesID)
                        {
                            var nine = idwww;
                        }
                        List<string> ListDelFile = JsonConvert.DeserializeObject<List<string>>(delFilesID);
                        task_comments cmtbug = JsonConvert.DeserializeObject<task_comments>(fdcmtbug);
                        // This illustrates how to get thefile names.
                        string strPath = root + "/" + dvid + "/TaskOrigin/" + id + "/" + cmt_id;
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);
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

                                    var checkBase64 = newChild.Attributes["src"].Value.Substring(newChild.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                                    checkBase64 = checkBase64.Trim();
                                    if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                                    {
                                        byte[] bytes = Convert.FromBase64String(newChild.Attributes["src"].Value.Substring(newChild.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                                        bool existsFolder = System.IO.Directory.Exists(strPath);
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


                                        img.SetAttributeValue("style", "width:100%;max-width:30vw;object-fit:contain");
                                        img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/TaskOrigin/" + id + "/" + cmt_id + pathShow);
                                        cmtbug.contents = cmtbug.contents.Replace(newChild.OuterHtml, img.OuterHtml);
                                        task_file task_File = new task_file();
                                        task_File.file_id = helper.GenKey();
                                        task_File.comment_id = cmt_id;
                                        task_File.file_name = pathShow.Replace("/", "");
                                        task_File.file_path = "/Portals" + "/" + dvid + "/TaskOrigin/" + id + "/" + cmt_id + pathShow;
                                        task_File.file_type = typeFileHL.Substring(1);
                                        var file_info = new FileInfo(root + "/" + dvid + "/TaskOrigin/" + id + "/" + cmt_id + pathShow);
                                        task_File.file_size = file_info.Length;
                                        task_File.is_image = helper.IsImageFileName(pathShow);
                                        task_File.is_type = 2;
                                        task_File.status = false;
                                        task_File.created_by = uid;
                                        task_File.created_date = DateTime.Now;
                                        task_File.created_ip = ip;
                                        task_File.task_id = id;
                                        task_File.created_token_id = tid;
                                        task_Files.Add(task_File);
                                    }
                                }

                            }

                        }
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
                            newFileName = Path.Combine(strPath, fileName);
                            fileInfo = new FileInfo(newFileName);
                            string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                            if (fileInfo != null)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + "_(" + helper.ranNumberFile() + ")" + fileInfo.Extension;

                                if (fileName.Length > 500)
                                {
                                    fileName = fileName.Substring(0, fileName.LastIndexOf('.') - 1);
                                    int le = 500 - (strPath.Length + 1) - Dinhdang.Length;
                                    fileName = fileName.Substring(0, le) + fileName;
                                }
                                newFileName = Path.Combine(strPath, fileName);
                            }
                            ffileData = fileData;
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }
                            task_file task_File = new task_file();
                            task_File.file_id = helper.GenKey();
                            task_File.comment_id = cmt_id;
                            task_File.task_id = id;
                            task_File.file_name = fileName;
                            task_File.file_path = "/" + "Portals" + "/" + dvid + "/TaskOrigin/" + id + "/" + cmt_id + "/" + fileName;
                            task_File.file_type = Dinhdang;
                            var file_info = new FileInfo(strPath + "/" + fileName);
                            task_File.file_size = file_info.Length;
                            task_File.is_image = helper.IsImageFileName(fileName);
                            task_File.is_type = 2;
                            task_File.status = true;
                            task_File.created_by = uid;
                            task_File.created_date = DateTime.Now;
                            task_File.created_ip = ip;
                            task_File.created_token_id = tid;
                            task_Files.Add(task_File);
                        }
                        var editCmt = db.task_comments.Where(x => x.comment_id == cmt_id).FirstOrDefault();
                        editCmt.contents = cmtbug.contents;
                        editCmt.modified_by = uid;
                        editCmt.modified_date = DateTime.Now;
                        editCmt.modified_ip = ip;
                        editCmt.modified_token_id = tid;
                        db.task_file.AddRange(task_Files);
                        #region Xóa file cũ
                        List<string> delFiles = new List<string>();
                        List<string> file_paths = new List<string>();
                        List<task_file> delFilesDatabase = new List<task_file>();
                        var ImagesInCmt = db.task_file.Where(x => x.comment_id == cmt_id && x.status == false).ToList();
                        foreach (var img in ImagesInCmt)
                        {
                            delFilesDatabase.Add(img);
                            file_paths.Add(img.file_path);
                        }
                        foreach (var fileID in ListDelFile)
                        {
                            var filedata = db.task_file.Where(x => x.file_id == fileID).FirstOrDefault();
                            delFilesDatabase.Add(filedata);
                            file_paths.Add(filedata.file_path);
                        }
                        foreach (var file in file_paths)
                        {
                            //var pathsss = rootDel + file;
                            //bool ehxists = File.Exists(rootDel + file);
                            //if (ehxists == true)
                            //{
                            //    File.Delete(rootDel + file);
                            //}

                            // Format file
                            var strPathFormat = Regex.Replace(file.Replace("\\", "/"), @"\.*/+", "/");
                            var listStrPath = strPathFormat.Split('/');
                            var strPathConfig = "";
                            foreach (var item in listStrPath)
                            {
                                if (item.Trim() != "")
                                {
                                    strPathConfig += "/" + Path.GetFileName(item);
                                }
                            }
                            bool ehxists = File.Exists(rootDel + strPathConfig);
                            if (ehxists == true)
                            {
                                File.Delete(rootDel + strPathConfig);
                            }
                        }
                        #endregion
                        db.task_file.RemoveRange(delFilesDatabase);
                        db.Entry(editCmt).State = EntityState.Modified;
                        db.SaveChanges();
                        //notify
                        var listuser = db.task_member.Where(x => x.task_id == editCmt.task_id).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == editCmt.task_id).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);
                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Chỉnh sửa bình luận công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, editCmt.task_id, null, null, tid, ip);
                        }
                        #region add task_logs
                        if (helper.wlog && cmt_id != null)
                        {
                            var task = db.task_origin.Where(x => x.task_id == id).FirstOrDefault();
                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = id;
                            log.comment_id = cmt_id;
                            log.project_id = null;
                            log.description = "Chỉnh sửa bình luận công việc " + "'<b>" + task.task_name + "</b>'";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_comments/updateComments", ip, tid, "Lỗi khi chỉnh sửa bình luận công việc", 0, "task_comments");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_comments/updateComments", ip, tid, "Lỗi khi chỉnh sửa bình luận công việc", 0, "task_comments");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> deleteTaskComments([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    var das = await db.task_comments.Where(a => id.Contains(a.comment_id)).ToListAsync();

                    if (das.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Có lỗi xảy ra! Vui lòng kiểm tra lại." });
                    }
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<task_comments> del = new List<task_comments>();
                        List<task_file> delFile = new List<task_file>();
                        foreach (var da in das)
                        {
                            if (uid == da.created_by)
                            {
                                del.Add(da);
                                var file = db.task_file.Where(x => x.comment_id == da.comment_id).FirstOrDefault();

                                if (file != null)
                                { 
                                    paths.Add(file.file_path); 
                                    delFile.Add(file); 
                                }

                            }
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                var de = db.task_comments.Where(a => (a.comment_id == da.comment_id)).FirstOrDefault<task_comments>();
                                var des = db.task_origin.Where(a => (a.task_id == da.task_id)).FirstOrDefault<task_origin>();

                                if (de.comment_id != null)
                                {
                                    task_logs log = new task_logs();
                                    log.log_id = helper.GenKey();
                                    log.comment_id = de.comment_id;
                                    log.project_id = null;
                                    log.description = "Xóa bình luận" + "'<b>" + des.task_name + "</b>'";
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.task_logs.Add(log);
                                    db.SaveChanges();
                                }
                            }
                            #endregion
                        }
                        string ssid = das[0].task_id;
                        //notify
                        var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);
                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Xóa bình luận công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                        }

                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.task_comments.RemoveRange(del);
                        db.task_file.RemoveRange(delFile);
                    }
                    await db.SaveChangesAsync();
                    foreach (string strPath in paths)
                    {
                        //bool exists = File.Exists(strPath);
                        //if (exists)
                        //{ 
                        //    File.Delete(strPath); 
                        //}
                        var strPathFormat = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/");
                        var listStrPath = strPathFormat.Split('/');
                        var strPathConfig = "";
                        foreach (var item in listStrPath)
                        {
                            if (item.Trim() != "")
                            {
                                strPathConfig += "/" + Path.GetFileName(item);
                            }
                        }
                        bool ex = System.IO.File.Exists(root + strPathConfig);
                        if (ex)
                        {
                            System.IO.File.Delete(root + strPathConfig);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "/task_comments/deleteTaskComments", ip, tid, "Lỗi khi xoá bình luận", 1, "task_comments");

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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "/task_comments/deleteTaskComments", ip, tid, "Lỗi khi xóa bình luận", 1, "task_comments");

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
