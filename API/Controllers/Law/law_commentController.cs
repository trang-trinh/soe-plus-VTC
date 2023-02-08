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
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json.Linq;
using API.Helper;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class law_commentController : ApiController
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

        [HttpPost]
        public async Task<HttpResponseMessage> Add_Comment()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlaw = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
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
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdlaw = provider.FormData.GetValues("comment").SingleOrDefault();
                        law_comment comment = JsonConvert.DeserializeObject<law_comment>(fdlaw);
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(comment.des);

                        if (comment.law_id == null || comment.law_id == "")
                        {
                            comment.law_id = "other";
                        }
                        string strPath = "/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment";

                        var listPathEdit_0 = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                        var pathEdit_0 = "";
                        foreach (var itemEdit in listPathEdit_0)
                        {
                            if (itemEdit.Trim() != "")
                            {
                                pathEdit_0 += "/" + Path.GetFileName(itemEdit);
                            }
                        }
                        strPath = root + pathEdit_0;
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);

                        var imgs = doc.DocumentNode.SelectNodes("//img");

                        if (imgs != null)
                        {

                            foreach (var img in imgs)
                            {

                                var pathFolderDes = "/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment";
                                var checkBase64 = img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                                checkBase64 = checkBase64.Trim();
                                if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                                {
                                    byte[] bytes = Convert.FromBase64String(img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                                    bool existsFolder = System.IO.Directory.Exists(strPath);
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

                                    comment.des = comment.des.Replace(img.Attributes["src"].Value, domainurl + "/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment" + pathShow);
                                    helper.ResizeImage(domainurl + "/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment" + pathShow, 640, 640, 90);
                                }
                            }
                        }
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        List<string> listPathFileUp = new List<string>();
                        var listFiles = "";
                        var detached = "";
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
                            newFileName = Path.Combine("/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment", fileName);

                            var listPathEdit_1 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                            var pathEdit_1 = "";
                            foreach (var itemEdit in listPathEdit_1)
                            {
                                if (itemEdit.Trim() != "")
                                {
                                    pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                }
                            }
                            newFileName = pathEdit_1;

                            fileInfo = new FileInfo(root + newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = Path.Combine("/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment", fileName);
                            } 
                            if (comment.url_file != null && comment.url_file != "")
                            {
                                detached = ",";
                            }
                            else if (comment.url_file == null)
                            {
                                comment.url_file = "";
                            }
                            comment.url_file += detached + ("/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment/" + fileName);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                var strDirectory = "/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment/";
                                var listPathEdit = Regex.Replace(strDirectory.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit = "";
                                foreach (var itemEdit in listPathEdit)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                if (!Directory.Exists(root + pathEdit))
                                {
                                    Directory.CreateDirectory(root + pathEdit);
                                }
                                //if (!Directory.Exists(fileInfo.Directory.FullName))
                                //{
                                //    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                //}

                                var listPathEdit_2 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_2 = "";
                                foreach (var itemEdit in listPathEdit_2)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit_2 += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                File.Move(ffileData.LocalFileName, root + pathEdit_2);
                                //File.Move(ffileData.LocalFileName, newFileName);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }
                        var numComment = db.law_comment.Where(x => x.law_id == comment.law_id).Max(y => y.is_order);
                        comment.is_order = numComment + 1;
                        comment.created_by = uid;
                        comment.created_date = DateTime.Now;
                        comment.created_ip = ip;
                        comment.created_token_id = tid;
                        db.law_comment.Add(comment);

                        db.SaveChanges();

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
                        #region add law_logs
                        if (helper.wlog)
                        {
                            law_logs log = new law_logs();
                            log.log_type = 0;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            var law_main = db.law_documents.AsNoTracking().FirstOrDefault(x => x.law_id == comment.law_id);
                            log.message = "Thêm mới comment: " + law_main.law_name;
                            log.law_name = law_main.law_name;
                            log.law_id = comment.law_id;
                            log.organization_id = law_main.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.law_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_comment/Add_Comment", ip, tid, "Lỗi khi thêm comment", 0, "law_comment");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_comment/Add_Comment", ip, tid, "Lỗi khi thêm comment", 0, "law_comment");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Comment()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlaw = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
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

                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdlaw = provider.FormData.GetValues("comment").SingleOrDefault();
                        law_comment comment = JsonConvert.DeserializeObject<law_comment>(fdlaw);
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(comment.des);

                        if (comment.law_id == null || comment.law_id == "")
                        {
                            comment.law_id = "other";
                        }
                        string strPath = "/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment";

                        var listPathEdit_0 = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                        var pathEdit_0 = "";
                        foreach (var itemEdit in listPathEdit_0)
                        {
                            if (itemEdit.Trim() != "")
                            {
                                pathEdit_0 += "/" + Path.GetFileName(itemEdit);
                            }
                        }
                        strPath = root + pathEdit_0;
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);

                        var imgs = doc.DocumentNode.SelectNodes("//img");

                        if (imgs != null)
                        {

                            foreach (var img in imgs)
                            {

                                var pathFolderDes = "/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment";
                                var checkBase64 = img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                                checkBase64 = checkBase64.Trim();
                                if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                                {
                                    byte[] bytes = Convert.FromBase64String(img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                                    bool existsFolder = System.IO.Directory.Exists(strPath);
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

                                    comment.des = comment.des.Replace(img.Attributes["src"].Value, domainurl + "/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment" + pathShow);
                                    helper.ResizeImage(domainurl + "/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment" + pathShow, 640, 640, 90);
                                }
                            }
                        }
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        List<string> listPathFileUp = new List<string>();
                        var listFiles = "";
                        var detached = "";
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
                            newFileName = Path.Combine("/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment", fileName);

                            var listPathEdit_1 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                            var pathEdit_1 = "";
                            foreach (var itemEdit in listPathEdit_1)
                            {
                                if (itemEdit.Trim() != "")
                                {
                                    pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                }
                            }
                            newFileName = pathEdit_1;

                            fileInfo = new FileInfo(root + newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileName.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = Path.Combine("/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment", fileName);
                            }

                            if (comment.url_file != null && comment.url_file != "")
                            {
                                detached = ",";
                            }
                            else if (comment.url_file == null)
                            {
                                comment.url_file = "";
                            }
                            comment.url_file += detached + ("/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment/" + fileName);

                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                var strDirectory = "/Portals/" + organization_id_user + "/Law/" + comment.law_id + "/Comment/";
                                var listPathEdit = Regex.Replace(strDirectory.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit = "";
                                foreach (var itemEdit in listPathEdit)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                if (!Directory.Exists(root + pathEdit))
                                {
                                    Directory.CreateDirectory(root + pathEdit);
                                }
                                //if (!Directory.Exists(fileInfo.Directory.FullName))
                                //{
                                //    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                //}

                                var listPathEdit_2 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_2 = "";
                                foreach (var itemEdit in listPathEdit_2)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit_2 += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                File.Move(ffileData.LocalFileName, root + pathEdit_2);
                                //File.Move(ffileData.LocalFileName, newFileName);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }

                        comment.modified_by = uid;
                        comment.modified_date = DateTime.Now;
                        comment.modified_ip = ip;
                        comment.modified_token_id = tid;
                        db.Entry(comment).State = EntityState.Modified;
                        db.SaveChanges();

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
                        #region add law_logs
                        if (helper.wlog)
                        {
                            law_logs log = new law_logs();
                            log.log_type = 0;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            var law_main = db.law_documents.AsNoTracking().FirstOrDefault(x => x.law_id == comment.law_id);
                            log.message = "Cập nhật comment: " + law_main.law_name;
                            log.law_name = law_main.law_name;
                            log.law_id = comment.law_id;
                            log.organization_id = law_main.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.law_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_comment/Update_Comment", ip, tid, "Lỗi khi cập nhật comment", 0, "law_comment");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_comment/Update_Comment", ip, tid, "Lỗi khi cập nhật comment", 0, "law_comment");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Comment([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var das = await db.law_comment.Where(a => id.Contains(a.comment_id) && (ad || a.created_by == uid)).ToListAsync();
                    //var dasUrl = await db.law_comment.AsNoTracking().Where(a => id.Contains(a.comment_id) && (ad || a.created_by == uid) && a.url_file != null).Select(x => x.url_file).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<law_comment> del = new List<law_comment>();
                        foreach (var da in das)
                        {
                            del.Add(da);
                            var user_now = db.sys_users.FirstOrDefault(x => x.user_id == da.created_by);
                            var organization_id_user = "other";
                            if (user_now != null && user_now.organization_id != null)
                            {
                                organization_id_user = user_now.organization_id.ToString();
                            }
                            string[] listFilePath = da.url_file.Split(',');
                            foreach (string path in listFilePath)
                            {
                                string filePath = "/Portals/" + organization_id_user + "/Law/" + da.law_id + "/Comment/" + Path.GetFileName(path);
                                if (filePath == (path)) {
                                    paths.Add(filePath);
                                }
                            }
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                law_logs log = new law_logs();
                                log.log_type = 0;
                                //log.message = JsonConvert.SerializeObject(new { data = law_main });
                                var law_main = db.law_documents.AsNoTracking().FirstOrDefault(x => x.law_id == da.law_id);
                                log.message = "Xóa comment: " + da.des;
                                log.law_name = law_main.law_name;
                                log.law_id = da.law_id;
                                log.organization_id = law_main.organization_id;
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                log.is_view = false;
                                db.law_logs.Add(log);
                                db.SaveChanges();

                            }
                            #endregion
                        }
                        //foreach (var da in dasUrl)
                        //{
                        //    paths.Add(da);
                        //}
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.law_comment.RemoveRange(del);
                    }
                    //await db.SaveChangesAsync();
                    foreach (string strPath in paths)
                    {
                        var listPathEdit = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                        var pathEdit = "";
                        foreach (var itemEdit in listPathEdit)
                        {
                            if (itemEdit.Trim() != "")
                            {
                                pathEdit += "/" + Path.GetFileName(itemEdit);
                            }
                        }
                        string pathDel = root + pathEdit;
                        bool exists = File.Exists(pathDel);
                        if (exists && strPath.Contains("Portals") && strPath.Contains("Law") && strPath.Contains("Comment"))
                        {
                            System.IO.File.Delete(pathDel);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "law_comment/Delete_Comment", ip, tid, "Lỗi khi xoá comment", 0, "law_comment");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "law_comment/Delete_Comment", ip, tid, "Lỗi khi xoá comment", 0, "law_comment");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
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
            string nameErrProc = "Lỗi khi gọi proc";// + proc.proc + "'";

            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
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
                        log.controller = domainurl + "law_comment/GetDataProc";
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "law_comment/GetDataProc", ip, tid, nameErrProc, 0, "law_comment");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "law_comment/GetDataProc", ip, tid, nameErrProc, 0, "law_comment");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }
        #endregion

    }
}
