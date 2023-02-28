using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Helper;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.IO;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using API.Helper;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class law_documentsController : ApiController
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
        public async Task<HttpResponseMessage> Add_Law()
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
                        fdlaw = provider.FormData.GetValues("law_docs").SingleOrDefault();
                        law_documents law_main = JsonConvert.DeserializeObject<law_documents>(fdlaw);
                        if (db.law_documents.FirstOrDefault(x => x.organization_id == law_main.organization_id && x.law_id != law_main.law_id && x.law_name.Trim().ToLower() == law_main.law_name.Trim().ToLower()) != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tên văn bản luật đã tồn tại trong hệ thống.", err = "2" });
                        }
                        else if (db.law_documents.FirstOrDefault(x => x.organization_id == law_main.organization_id && x.law_id != law_main.law_id && x.law_number.Trim().ToLower() == law_main.law_number.Trim().ToLower()) != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Số hiệu văn bản luật đã tồn tại trong hệ thống.", err = "2" });
                        }
                        law_main.law_id = helper.GenKey();
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(law_main.summary);

                        string strPath = "/Portals/" + organization_id_user + "/Law/" + law_main.law_id;

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
                                var pathFolderDes = "/Portals/" + organization_id_user + "/Law/" + law_main.law_id;
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
                                    var pathImageSummary = "/Portals/" + organization_id_user + "/Law/" + law_main.law_id + "/Img_Summary";

                                    var listPathEdit_1 = Regex.Replace(pathImageSummary.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                    var pathEdit_1 = "";
                                    foreach (var itemEdit in listPathEdit_1)
                                    {
                                        if (itemEdit.Trim() != "")
                                        {
                                            pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                        }
                                    }
                                    pathImageSummary = pathEdit_1;

                                    if (!Directory.Exists(root + pathImageSummary))
                                        Directory.CreateDirectory(root + pathImageSummary);

                                    law_main.summary = law_main.summary.Replace(img.Attributes["src"].Value, domainurl + "/Portals/" + organization_id_user + "/Law/" + law_main.law_id + "/Img_Summary/" + pathShow);
                                    helper.ResizeImage(domainurl + "/Portals/" + organization_id_user + "/Law/" + law_main.law_id + "/Img_Summary/" + pathShow, 640, 640, 90);
                                }
                            }
                        }
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        var numfileLaw = db.law_files.Count(x => x.law_id == law_main.law_id);
                        List<law_files> listFileUp = new List<law_files>();
                        List<string> listPathFileUp = new List<string>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            numfileLaw++;
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

                            newFileName = Path.Combine("/Portals/" + organization_id_user + "/Law/" + law_main.law_id, fileName);

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

                                newFileName = Path.Combine("/Portals/" + organization_id_user + "/Law/" + law_main.law_id, fileName);
                            }
                            law_files file_law = new law_files();
                            file_law.law_id = law_main.law_id;
                            file_law.file_path = "/Portals/" + organization_id_user + "/Law/" + law_main.law_id + "/" + fileName;
                            file_law.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_law.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_law.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_law.file_type = file_law.file_name.Substring(file_law.file_name.LastIndexOf(".") + 1).ToLower();
                            file_law.is_order = numfileLaw;
                            file_law.is_drafted = false;
                            file_law.law_file_type = 0;
                            file_law.message = "Thêm file văn bản luật " + law_main.law_name;
                            file_law.created_by = uid;
                            file_law.created_date = DateTime.Now;
                            file_law.created_ip = ip;
                            file_law.created_token_id = tid;
                            listFileUp.Add(file_law);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                var strDirectory = "/Portals/" + organization_id_user + "/Law/" + law_main.law_id;
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
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }

                        string law_relates = provider.FormData.GetValues("law_relates").SingleOrDefault();
                        List<InfoLaw_Short> listRelateLaw = JsonConvert.DeserializeObject<List<InfoLaw_Short>>(law_relates);
                        if (listRelateLaw != null && listRelateLaw.Count > 0)
                        {
                            List<law_doc_related> listRelate = new List<law_doc_related>();
                            foreach (var item in listRelateLaw)
                            {
                                law_doc_related relate = new law_doc_related();
                                relate.law_id = law_main.law_id;
                                relate.law_related_id = item.law_id;
                                relate.created_by = uid;
                                relate.created_date = DateTime.Now;
                                relate.created_ip = ip;
                                relate.created_token_id = tid;
                                listRelate.Add(relate);
                            }
                            if (listRelate.Count > 0)
                            {
                                db.law_doc_related.AddRange(listRelate);
                            }
                        }

                        string law_replaces = provider.FormData.GetValues("law_replaces").SingleOrDefault();
                        List<InfoLaw_Short> listReplaceLaw = JsonConvert.DeserializeObject<List<InfoLaw_Short>>(law_replaces);
                        if (listReplaceLaw != null && listReplaceLaw.Count > 0)
                        {
                            List<law_doc_replace> listReplace = new List<law_doc_replace>();
                            foreach (var item in listReplaceLaw)
                            {
                                law_doc_replace replace = new law_doc_replace();
                                replace.law_id = law_main.law_id;
                                replace.law_replace_id = item.law_id;
                                replace.created_by = uid;
                                replace.created_date = DateTime.Now;
                                replace.created_ip = ip;
                                replace.created_token_id = tid;
                                listReplace.Add(replace);
                            }
                            if (listReplace.Count > 0)
                            {
                                db.law_doc_replace.AddRange(listReplace);
                            }
                        }

                        law_main.created_by = uid;
                        law_main.created_date = DateTime.Now;
                        law_main.created_ip = ip;
                        law_main.created_token_id = tid;
                        db.law_documents.Add(law_main);
                        if (listFileUp.Count > 0)
                        {
                            db.law_files.AddRange(listFileUp);
                        }
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
                            log.message = "Thêm mới văn bản luật: " + law_main.law_name;
                            log.law_name = law_main.law_name;
                            log.law_id = law_main.law_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_documents/Add_Law", ip, tid, "Lỗi khi thêm văn bản luật", 0, "law_documents");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_documents/Add_Law", ip, tid, "Lỗi khi thêm văn bản luật", 0, "law_documents");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Law()
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
                        fdlaw = provider.FormData.GetValues("law_docs").SingleOrDefault();
                        law_documents law_main = JsonConvert.DeserializeObject<law_documents>(fdlaw);
                        if (db.law_documents.FirstOrDefault(x => x.organization_id == law_main.organization_id && x.law_id != law_main.law_id && x.law_name.Trim().ToLower() == law_main.law_name.Trim().ToLower()) != null)
                        {
                            Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tên văn bản luật đã tồn tại trong hệ thống.", err = "2" });
                        }
                        else if (db.law_documents.FirstOrDefault(x => x.organization_id == law_main.organization_id && x.law_id != law_main.law_id && x.law_number.Trim().ToLower() == law_main.law_number.Trim().ToLower()) != null)
                        {
                            Request.CreateResponse(HttpStatusCode.OK, new { ms = "Số hiệu văn bản luật đã tồn tại trong hệ thống.", err = "2" });
                        }
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(law_main.summary);

                        string strPath = "/Portals/" + organization_id_user + "/Law/" + law_main.law_id;
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
                            var pathImageSummary = "/Portals/" + organization_id_user + "/Law/" + law_main.law_id + "/Img_Summary";
                            var listPathEdit_1 = Regex.Replace(pathImageSummary.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                            var pathEdit_1 = "";
                            foreach (var itemEdit in listPathEdit_1)
                            {
                                if (itemEdit.Trim() != "")
                                {
                                    pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                }
                            }
                            pathImageSummary = pathEdit_1;
                            if (!Directory.Exists(root + pathImageSummary))
                            {
                                Directory.Delete(root + pathImageSummary, true);
                            }
                            foreach (var img in imgs)
                            {
                                var pathFolderDes = "/Portals/" + organization_id_user + "/Law/" + law_main.law_id;
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
                                    if (!Directory.Exists(root + pathImageSummary))
                                        Directory.CreateDirectory(root + pathImageSummary);

                                    law_main.summary = law_main.summary.Replace(img.Attributes["src"].Value, domainurl + "/Portals/" + organization_id_user + "/Law/" + law_main.law_id + "/Img_Summary/" + pathShow);
                                    helper.ResizeImage(domainurl + "/Portals/" + organization_id_user + "/Law/" + law_main.law_id + "/Img_Summary/" + pathShow, 640, 640, 90);
                                }
                            }
                        }
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        var numfileLaw = db.law_files.Count(x => x.law_id == law_main.law_id) == 0 ? 0 : db.law_files.Where(x => x.law_id == law_main.law_id).Max(y => y.is_order);
                        List<law_files> listFileUp = new List<law_files>();
                        List<string> listPathFileUp = new List<string>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            numfileLaw++;
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
                            newFileName = Path.Combine("/Portals/" + organization_id_user + "/Law/" + law_main.law_id, fileName);

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

                                newFileName = Path.Combine("/Portals/" + organization_id_user + "/Law/" + law_main.law_id, fileName);
                            }
                            law_files file_law = new law_files();
                            file_law.law_id = law_main.law_id;
                            file_law.file_path = "/Portals/" + organization_id_user + "/Law/" + law_main.law_id + "/" + fileName;
                            file_law.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_law.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_law.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_law.file_type = file_law.file_name.Substring(file_law.file_name.LastIndexOf(".") + 1).ToLower();
                            file_law.is_order = numfileLaw;
                            file_law.is_drafted = false;
                            file_law.law_file_type = 0;
                            file_law.message = "Thêm file văn bản luật " + law_main.law_name;
                            file_law.created_by = uid;
                            file_law.created_date = DateTime.Now;
                            file_law.created_ip = ip;
                            file_law.created_token_id = tid;
                            listFileUp.Add(file_law);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                var strDirectory = "/Portals/" + organization_id_user + "/Law/" + law_main.law_id;
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
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }

                        string law_files = provider.FormData.GetValues("fileUploadOld").SingleOrDefault();
                        List<InfoFile_Upload> listFileOld = JsonConvert.DeserializeObject<List<InfoFile_Upload>>(law_files);
                        List<string> pathFilesDel = new List<string>();
                        if (listFileOld != null && listFileOld.Count > 0)
                        {
                            List<law_files> listFileDel = new List<law_files>();
                            var listFileDelTemp = db.law_files.Where(x => x.law_id == law_main.law_id).ToList();
                            var delItems = listFileDelTemp.Where(x => listFileOld.Count(y => y.file_id == x.file_id) > 0).ToList();
                            foreach (var item in delItems)
                            {
                                listFileDel.Add(item);
                                pathFilesDel.Add(item.file_path);
                            }
                            if (listFileDel.Count > 0)
                            {
                                db.law_files.RemoveRange(listFileDel);
                            }
                        }

                        string law_relates = provider.FormData.GetValues("law_relates").SingleOrDefault();
                        List<InfoLaw_Short> listRelateLaw = JsonConvert.DeserializeObject<List<InfoLaw_Short>>(law_relates);
                        if (listRelateLaw != null && listRelateLaw.Count > 0)
                        {
                            List<law_doc_related> listRelate = new List<law_doc_related>();
                            var listRelateAdd = listRelateLaw.Where(x => db.law_doc_related.Count(y => y.law_related_id == x.law_id && y.law_id == law_main.law_id) == 0).ToList();
                            var lawRelateTemp = db.law_doc_related.Where(x => x.law_id == law_main.law_id).ToList();
                            var listRelateDel = lawRelateTemp.Where(x => listRelateLaw.Count(y => y.law_id == x.law_related_id) == 0).ToList();
                            foreach (var item in listRelateAdd)
                            {
                                law_doc_related relate = new law_doc_related();
                                relate.law_id = law_main.law_id;
                                relate.law_related_id = item.law_id;
                                relate.created_by = uid;
                                relate.created_date = DateTime.Now;
                                relate.created_ip = ip;
                                relate.created_token_id = tid;
                                listRelate.Add(relate);
                            }
                            if (listRelate.Count > 0)
                            {
                                db.law_doc_related.AddRange(listRelate);
                            }
                            if (listRelateDel.Count > 0)
                            {
                                db.law_doc_related.RemoveRange(listRelateDel);
                            }
                        }
                        else
                        {
                            var lawRelateDel = db.law_doc_related.Where(x => x.law_id == law_main.law_id).ToList();
                            if (lawRelateDel.Count > 0)
                            {
                                db.law_doc_related.RemoveRange(lawRelateDel);
                            }
                        }

                        string law_replaces = provider.FormData.GetValues("law_replaces").SingleOrDefault();
                        List<InfoLaw_Short> listReplaceLaw = JsonConvert.DeserializeObject<List<InfoLaw_Short>>(law_replaces);
                        if (listReplaceLaw != null && listReplaceLaw.Count > 0)
                        {

                            List<law_doc_replace> listReplace = new List<law_doc_replace>();
                            var listReplaceAdd = listReplaceLaw.Where(x => db.law_doc_replace.Count(y => y.law_replace_id == x.law_id && y.law_id == law_main.law_id) == 0).ToList();
                            var lawReplaceTemp = db.law_doc_replace.Where(x => x.law_id == law_main.law_id).ToList();
                            var listReplaceDel = lawReplaceTemp.Where(x => listReplaceLaw.Count(y => y.law_id == x.law_replace_id) == 0).ToList();
                            foreach (var item in listReplaceAdd)
                            {
                                law_doc_replace replace = new law_doc_replace();
                                replace.law_id = law_main.law_id;
                                replace.law_replace_id = item.law_id;
                                replace.created_by = uid;
                                replace.created_date = DateTime.Now;
                                replace.created_ip = ip;
                                replace.created_token_id = tid;
                                listReplace.Add(replace);
                            }
                            if (listReplace.Count > 0)
                            {
                                db.law_doc_replace.AddRange(listReplace);
                            }
                            if (listReplaceDel.Count > 0)
                            {
                                db.law_doc_replace.RemoveRange(listReplaceDel);
                            }
                        }
                        else
                        {
                            var listReplaceDel = db.law_doc_replace.Where(x => x.law_id == law_main.law_id).ToList();
                            if (listReplaceDel.Count > 0)
                            {
                                db.law_doc_replace.RemoveRange(listReplaceDel);
                            }
                        }
                        law_main.modified_by = uid;
                        law_main.modified_date = DateTime.Now;
                        law_main.modified_ip = ip;
                        law_main.modified_token_id = tid;
                        db.Entry(law_main).State = EntityState.Modified;
                        if (listFileUp.Count > 0)
                        {
                            db.law_files.AddRange(listFileUp);
                        }
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
                        foreach (var pathDel in pathFilesDel)
                        {
                            if (pathDel.Contains("/Portals/") && pathDel.Contains("/Law/") && !pathDel.Contains("../"))
                            {
                                bool existFiles = System.IO.File.Exists(root + pathDel);
                                if (existFiles)
                                    System.IO.File.Delete(root + pathDel);
                            }
                        }

                        #region add law_logs
                        if (helper.wlog)
                        {
                            law_logs log = new law_logs();
                            log.log_type = 0;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = "Cập nhật văn bản luật: " + law_main.law_name;
                            log.law_name = law_main.law_name;
                            log.law_id = law_main.law_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_documents/Update_Law", ip, tid, "Lỗi khi cập nhật văn bản luật", 0, "law_documents");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_documents/Update_Law", ip, tid, "Lỗi khi cập nhật văn bản luật", 0, "law_documents");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Law([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                    var das = await db.law_documents.Where(a => id.Contains(a.law_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<law_documents> del = new List<law_documents>();
                        foreach (var da in das)
                        {
                            if (ad || da.created_by == uid)
                            {
                                var listFileLaw = db.law_files.Where(x => x.law_id == da.law_id).ToList();
                                if (listFileLaw.Count > 0)
                                {
                                    foreach (var item in listFileLaw)
                                    {
                                        var organization_id_law = db.law_documents.Find(item.law_id) != null ? db.law_documents.Find(item.law_id).organization_id.ToString() : "other";
                                        var pathFile = "/Portals/" + organization_id_law + "/";
                                        if (item.file_path != null && item.file_path.Contains(pathFile))
                                        {
                                            paths.Add(item.file_path);
                                        }
                                    }
                                }
                                del.Add(da);
                            }
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                law_logs log = new law_logs();
                                log.log_type = 0;
                                //log.message = JsonConvert.SerializeObject(new { data = law_main });
                                log.message = "Xóa văn bản luật: " + da.law_name;
                                log.law_name = da.law_name;
                                log.law_id = da.law_id;
                                log.organization_id = da.organization_id;
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
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.law_documents.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    foreach (string strPath in paths)
                    {
                        if (strPath.Contains("/Portals/") && strPath.Contains("/Law/") && !strPath.Contains("../"))
                        {
                            var strPathFormat = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = strPathFormat.Split('/');
                            var pathConfig = "";
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    pathConfig += "/" + Path.GetFileName(item);
                                }
                            }
                            var pathDelFile = HttpContext.Current.Server.MapPath("~/" + pathConfig);
                            bool existFiles = System.IO.Directory.Exists(pathDelFile);
                            if (existFiles)
                                System.IO.Directory.Delete(pathDelFile, true);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "law_documents/Delete_Law", ip, tid, "Lỗi khi xoá văn bản luật", 0, "law_documents");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "law_documents/Delete_Law", ip, tid, "Lỗi khi xoá văn bản luật", 0, "law_documents");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_History_View()
        {

            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string docs_law = "";
            string type_view = "";
            int type = 0;
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
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        docs_law = provider.FormData.GetValues("law_docs").SingleOrDefault();
                        law_documents law_main = JsonConvert.DeserializeObject<law_documents>(docs_law);

                        type_view = provider.FormData.GetValues("type_view").SingleOrDefault();
                        var type_update = JsonConvert.DeserializeObject<string>(type_view);
                        if (type_update != null && type_update != "")
                        {
                            type = Int32.Parse(type_update);
                        }
                        var exist_record = db.law_user_visitor.Count(x => x.law_id == law_main.law_id && x.user_id == uid && x.type_view == type);
                        var law_docs = db.law_documents.Find(law_main.law_id);
                        if (exist_record > 0)
                        {
                            var record_update = db.law_user_visitor.FirstOrDefault(x => x.law_id == law_main.law_id && x.user_id == uid && x.type_view == type);
                            record_update.modified_date = DateTime.Now;
                            record_update.modified_by = uid;
                            record_update.modified_token_id = tid;
                            record_update.modified_ip = ip;
                            record_update.times += 1;
                            if (record_update.type_view == 0)
                            {
                                if (law_docs.times_view == null)
                                {
                                    law_docs.times_view = 0;
                                    var listView = db.law_user_visitor.AsNoTracking().Where(x => x.law_id == law_main.law_id && x.type_view == 0).ToList();
                                    if (listView.Count > 0)
                                    {
                                        foreach (var item in listView)
                                        {
                                            law_docs.times_view += item.times;
                                        }
                                    }
                                }
                                law_docs.times_view += 1;
                            }
                            else
                            {
                                if (law_docs.times_download == null)
                                {
                                    law_docs.times_download = 0;
                                    var listDownload = db.law_user_visitor.AsNoTracking().Where(x => x.law_id == law_main.law_id && x.type_view == 1).ToList();
                                    if (listDownload.Count > 0)
                                    {
                                        foreach (var item in listDownload)
                                        {
                                            law_docs.times_download += item.times;
                                        }
                                    }
                                }
                                law_docs.times_download += 1;
                            }
                        }
                        else
                        {
                            int stt = db.law_user_visitor.Count(x => x.law_id == law_main.law_id && x.type_view == type);
                            var record_new = new law_user_visitor();
                            record_new.law_id = law_main.law_id;
                            record_new.user_id = uid;
                            record_new.organization_id_user = user_now.organization_id;
                            record_new.times = 1;
                            record_new.is_order = stt + 1;
                            record_new.type_view = type;
                            record_new.created_by = uid;
                            record_new.created_date = DateTime.Now;
                            record_new.created_token_id = tid;
                            record_new.created_ip = ip;
                            db.law_user_visitor.Add(record_new);
                            if (record_new.type_view == 0)
                            {
                                law_docs.times_view = 1;
                            }
                            else
                            {
                                law_docs.times_download = 1;
                            }
                        }
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_documents/Update_History_View", ip, tid, "Lỗi khi cập nhật số lượt xem/download văn bản luật", 0, "law_documents");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_documents/Update_History_View", ip, tid, "Lỗi khi cập nhật số lượt xem/download văn bản luật", 0, "law_documents");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Law_Documents([System.Web.Mvc.Bind(Include = "fieldSQLS,sqlF,sqlO,Search,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;

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
            string sql = "";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var userNow = db.sys_users.Find(uid);
                    if (userNow != null)
                    {
                        string sqlCount = @"select count(ld.law_id) as totalRecords from law_documents ld";
                        string WhereSQL = "";

                        if (userNow.is_super == true)
                        {
                            WhereSQL += "";
                        }
                        else if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                        {
                            WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " (ld.organization_id = 0 or ld.organization_id = " + int.Parse(filterSQL.sqlF) + ")";
                        }
                        else
                        {
                            WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " (ld.organization_id = 0 or ld.organization_id is null)";
                        }
                        WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + "(ld.created_by = '" + uid + "' or ld.is_active = 1)";
                        if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                        {
                            //WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + " (ld.law_name like N'%" + filterSQL.Search + "%')";
                            WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " (contains(ld.law_name, '\"*" + filterSQL.Search + "*\"'))";

                        }
                        if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                        {
                            foreach (var field in filterSQL.fieldSQLS)
                            {
                                if (field.filteroperator == "in")
                                {
                                    WhereSQL += Environment.NewLine + (WhereSQL != "" ? "and " : "") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                                }
                                else
                                {
                                    foreach (var m in field.filterconstraints.Where(a => a.value != null))
                                    {
                                        switch (m.matchMode)
                                        {
                                            case "contains":
                                                WhereSQL += Environment.NewLine + field.filteroperator + " (N'" + m.value + "' like N'%' + ld." + field.key + " + ',%')";
                                                break;
                                            case "containsMany":
                                                List<string> listKey = m.value.Split(',').ToList();
                                                WhereSQL += Environment.NewLine + field.filteroperator + " (";
                                                foreach (var str in listKey)
                                                {
                                                    if (str.Trim() != "")
                                                    {
                                                        WhereSQL += " ((ld." + field.key + " + ',')" + " like N'%' + " + "N'" + str + "' + ',%')  or";
                                                    }
                                                }
                                                if (WhereSQL.EndsWith(" or"))
                                                {
                                                    WhereSQL = WhereSQL.Substring(0, WhereSQL.Length - 3);
                                                }
                                                WhereSQL += ")";
                                                break;
                                            case "equals":
                                                WhereSQL += Environment.NewLine + field.filteroperator + " ld." + field.key + " = N'" + m.value + "'";
                                                break;
                                            case "dateBefore":
                                                WhereSQL += Environment.NewLine + field.filteroperator + " CAST(ld." + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                                break;
                                            case "dateAfter":
                                                WhereSQL += Environment.NewLine + field.filteroperator + " CAST(ld." + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                                break;

                                        }
                                    }
                                }
                            }
                        }
                        WhereSQL = WhereSQL.Trim();
                        if (WhereSQL.StartsWith("and "))
                        {
                            WhereSQL = WhereSQL.Substring(3);
                        }
                        else if (WhereSQL.StartsWith("or "))
                        {
                            WhereSQL = WhereSQL.Substring(2);
                        }

                        if (WhereSQL.Trim() != "")
                        {
                            sqlCount += Environment.NewLine + " where " + WhereSQL;
                            sql = @"select ld.*, u.last_name,"
                                + Environment.NewLine + @"cast ((case when ld.created_by = '" + uid + @"' or " + (userNow.is_super == true ? 1 : 0) + @" = 1 then 1 else 0 end) as bit) as allowDel,"
                                + Environment.NewLine + @"(select top 1 file_path from law_files where law_id = ld.law_id order by is_order) as file_path,"
                                + Environment.NewLine + @"cast ((case when convert(date, isnull(ld.expiration_date, getdate())) < convert(date, getdate()) then 1 else 0 end) as bit) as exp_law"
                                + Environment.NewLine + @"from law_documents ld"
                                + Environment.NewLine + @"join sys_users u on ld.created_by = u.user_id"
                                + Environment.NewLine + @"where " + WhereSQL;
                        }
                        string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                        //sql += @"ORDER BY ld." + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Contains("ASC") ? filterSQL.sqlO.Replace("ASC", "DESC") : (filterSQL.sqlO + " DESC"))
                        sql += Environment.NewLine + "ORDER BY ld." + (filterSQL.sqlO.Length > 50 || filterSQL.sqlO.Contains("select") || filterSQL.sqlO.Contains("update") || filterSQL.sqlO.Contains("delete") || filterSQL.sqlO.Contains("drop") ? "created_date" : filterSQL.sqlO)
                                + Environment.NewLine + "DESC"
                                + Environment.NewLine + "OFFSET " + OFFSET
                                + Environment.NewLine + "ROWS FETCH NEXT " + filterSQL.PageSize
                                + Environment.NewLine + "ROWS ONLY ";
                        sql += Environment.NewLine + sqlCount;
                        //sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                        //sql = Regex.Replace(sql, @"\s+", " ").Trim();
                        var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                        var tables = await task;
                        string JSONresult = JsonConvert.SerializeObject(tables);
                        return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { sql, err = "2", ms = "Bạn không có quyền truy cập chức năng này" });
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/law_documents/Filter_Law_Documents", ip, tid, "Lỗi khi gọi Filter_Law_Documents", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/law_documents/Filter_Law_Documents", ip, tid, "Lỗi khi gọi Filter_Law_Documents", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }

        #region ImportLaw
        [HttpPost]
        public async Task<HttpResponseMessage> ImportLaw()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
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
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = "/" + organization_id_user + "/Excel/Law/";

                    var listPathEdit_1 = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                    var pathEdit_1 = "";
                    foreach (var itemEdit in listPathEdit_1)
                    {
                        if (itemEdit.Trim() != "")
                        {
                            pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                        }
                    }
                    strPath = pathEdit_1;

                    bool exists = Directory.Exists(root + strPath);
                    if (!exists)
                        Directory.CreateDirectory(root + strPath);
                    var provider = new MultipartFormDataStreamProvider(root + strPath);
                    var task = Request.Content.ReadAsMultipartAsync(provider).ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        MultipartFileData fileLawExcel = null;
                        MultipartFileData fileLawZip = null;
                        string folderExtractImport = helper.GenKey();
                        List<string> fileUpLocalRoot = new List<string>();
                        List<string> fileUpLocal = new List<string>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            if (fileData.Headers.ContentDisposition.FileName.Contains(".xlsx") || fileData.Headers.ContentDisposition.FileName.Contains(".xls"))
                            {
                                fileLawExcel = fileData;
                            }
                            else if (fileData.Headers.ContentDisposition.FileName.Contains(".zip"))
                            {
                                fileLawZip = fileData;
                            }
                            fileUpLocalRoot.Add(fileData.LocalFileName);
                        }
                        string nameFolderZip = "";
                        string extractPath = strPath + "/" + folderExtractImport;
                        bool existPathZip = System.IO.Directory.Exists(root + extractPath);
                        if (!existPathZip)
                        {
                            System.IO.Directory.CreateDirectory(root + extractPath);
                        }
                        if (fileLawZip != null)
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(fileLawZip.LocalFileName, root + extractPath);
                            string nameFileZip = fileLawZip.Headers.ContentDisposition.FileName.Replace("\"", "");
                            nameFolderZip = nameFileZip.Substring(0, nameFileZip.LastIndexOf("."));
                        }
                        if (fileLawExcel != null)
                        {
                            FileInfo finfo = new FileInfo(fileLawExcel.LocalFileName);
                            string guid = Guid.NewGuid().ToString();
                            File.Move(finfo.FullName, Path.Combine(root + strPath, guid + "_" + fileLawExcel.Headers.ContentDisposition.FileName.Replace("\"", "")));
                            fpath = strPath + "/" + guid + "_" + fileLawExcel.Headers.ContentDisposition.FileName.Replace("\"", "");
                            fileUpLocal.Add(fpath);
                            FileInfo temp = new FileInfo(root + fpath);

                            #region read excel
                            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                            using (ExcelPackage pck = new ExcelPackage(temp))
                            {
                                try
                                {
                                    List<law_documents> dvs = new List<law_documents>();
                                    List<law_files> fileLaws = new List<law_files>();
                                    ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                    List<string> cols = new List<string>();
                                    var is_admin = user_now.organization_id;
                                    if (user_now != null && user_now.is_super == true)
                                    {
                                        is_admin = 0;
                                    }
                                    #region

                                    for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                    {
                                        if (ws.Cells[i, 1].Value == null)
                                        {
                                            break;
                                        }
                                        law_documents dv = new law_documents();
                                        dv.law_id = helper.GenKey();
                                        for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                        {
                                            if (ws.Cells[3, j].Value == null)
                                            {
                                                break;
                                            }
                                            var column = ws.Cells[3, j].Value;
                                            var vl = ws.Cells[i, j].Value;
                                            switch (column)
                                            {
                                                case "law_name":
                                                    dv.law_name = vl != null ? vl.ToString().Trim() : null;
                                                    break;
                                                case "law_number":
                                                    dv.law_number = vl != null ? vl.ToString().Trim() : null;
                                                    break;
                                                case "law_type":
                                                    if (vl != null && vl.ToString().Trim() != "")
                                                    {
                                                        var lawType = vl.ToString().ToLower().Trim();
                                                        var typeName = db.law_doc_types.FirstOrDefault(x => x.law_type_name.ToLower().Trim() == lawType);
                                                        dv.law_type = typeName != null ? typeName.law_type_name : lawType;
                                                    }
                                                    break;
                                                case "field_name":
                                                    if (vl != null && vl.ToString().Trim() != "")
                                                    {
                                                        var fieldNames = vl.ToString().Trim();
                                                        string[] listFields = fieldNames.Split(',');
                                                        dv.field_name = "";
                                                        foreach (var field in listFields)
                                                        {
                                                            var fieldTrim = field.ToLower().Trim();
                                                            if (fieldTrim != "")
                                                            {
                                                                var fieldLaw = db.law_doc_fields.FirstOrDefault(x => x.field_name.ToLower().Trim() == fieldTrim);
                                                                dv.field_name += fieldLaw != null ? ((dv.field_name != "" ? "," : "") + fieldLaw.field_name) : fieldTrim;
                                                            }
                                                        }
                                                        if (dv.field_name == "")
                                                        {
                                                            dv.field_name = null;
                                                        }
                                                    }
                                                    break;
                                                case "user_signed":
                                                    if (vl != null && vl.ToString().Trim() != "")
                                                    {
                                                        var userSigned = vl.ToString().ToLower().Trim();
                                                        var userSignedName = db.law_doc_signers.FirstOrDefault(x => x.signer_name.ToLower().Trim() == userSigned);
                                                        dv.user_signed = userSignedName != null ? userSignedName.signer_name : userSigned;
                                                    }
                                                    break;
                                                case "summary":
                                                    dv.summary = vl != null ? vl.ToString().Trim() : null;
                                                    break;
                                                case "tags":
                                                    dv.tags = vl != null ? vl.ToString().Trim() : null;
                                                    break;
                                                case "publish_number":
                                                    dv.publish_number = vl != null ? vl.ToString().Trim() : null;
                                                    break;
                                                case "publish_date":
                                                    if (vl != null && vl.ToString() != null && vl.ToString().Trim() != "")
                                                    {
                                                        //if (vl.ToString().Contains("/"))
                                                        //{
                                                        var dateStr = vl.ToString() ?? "";
                                                        var dates = dateStr.Split('/');
                                                        if (dates[0] != null && dates[1] != null && dates[2] != null)
                                                        {
                                                            var dd = int.Parse(dates[0]);
                                                            var mm = int.Parse(dates[1]);
                                                            var yyyy = int.Parse(dates[2]);
                                                            dv.publish_date = new DateTime(yyyy, mm, dd, 0, 0, 0);
                                                        }
                                                        else
                                                        {
                                                            dv.publish_date = null;
                                                        }
                                                        //}
                                                        //else
                                                        //{
                                                        //    DateTime myDate = DateTime.FromOADate(Double.Parse(vl.ToString()));
                                                        //    dv.publish_date = myDate;
                                                        //}
                                                    }
                                                    else
                                                    {
                                                        dv.publish_date = null;
                                                    }
                                                    break;
                                                case "issue_place":
                                                    if (vl != null && vl.ToString().Trim() != "")
                                                    {
                                                        var issuePlace = vl.ToString().ToLower().Trim();
                                                        var issuePlaceName = db.law_doc_issue_places.FirstOrDefault(x => x.issue_place_name.ToLower().Trim() == issuePlace);
                                                        dv.issue_place = issuePlaceName != null ? issuePlaceName.issue_place_name : issuePlace;
                                                    }
                                                    break;
                                                case "issued_date":
                                                    if (vl != null && vl.ToString() != null && vl.ToString().Trim() != "")
                                                    {
                                                        //if (vl.ToString().Contains("/"))
                                                        //{
                                                        var dateStr = vl.ToString() ?? "";
                                                        var dates = dateStr.Split('/');
                                                        if (dates[0] != null && dates[1] != null && dates[2] != null)
                                                        {
                                                            var dd = int.Parse(dates[0]);
                                                            var mm = int.Parse(dates[1]);
                                                            var yyyy = int.Parse(dates[2]);
                                                            dv.issued_date = new DateTime(yyyy, mm, dd, 0, 0, 0);
                                                        }
                                                        else
                                                        {
                                                            dv.issued_date = null;
                                                        }
                                                        //}
                                                        //else
                                                        //{
                                                        //    DateTime myDate = DateTime.FromOADate(Double.Parse(vl.ToString()));
                                                        //    dv.issued_date = myDate;
                                                        //}
                                                    }
                                                    else
                                                    {
                                                        dv.issued_date = null;
                                                    }
                                                    break;
                                                case "expiration_date":
                                                    if (vl != null && vl.ToString() != null && vl.ToString().Trim() != "")
                                                    {
                                                        //if (vl.ToString().Contains("/"))
                                                        //{
                                                        var dateStr = vl.ToString() ?? "";
                                                        var dates = dateStr.Split('/');
                                                        if (dates[0] != null && dates[1] != null && dates[2] != null)
                                                        {
                                                            var dd = int.Parse(dates[0]);
                                                            var mm = int.Parse(dates[1]);
                                                            var yyyy = int.Parse(dates[2]);
                                                            dv.expiration_date = new DateTime(yyyy, mm, dd, 0, 0, 0);
                                                        }
                                                        else
                                                        {
                                                            dv.expiration_date = null;
                                                        }
                                                        //}
                                                        //else
                                                        //{
                                                        //    DateTime myDate = DateTime.FromOADate(Double.Parse(vl.ToString()));
                                                        //    dv.expiration_date = myDate;
                                                        //}
                                                    }
                                                    else
                                                    {
                                                        dv.expiration_date = null;
                                                    }
                                                    break;
                                                case "path_file":
                                                    if (vl != null && vl.ToString().Trim() != "")
                                                    {
                                                        FileInfo fileInfo = null;
                                                        string newFileName = "";
                                                        string fileName = vl.ToString().Trim();
                                                        //if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                                                        //{
                                                        //    fileName = fileName.Trim('"');
                                                        //}
                                                        //if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                                                        //{
                                                        //    fileName = Path.GetFileName(fileName);
                                                        //}
                                                        fileName = Path.GetFileName(fileName);
                                                        string folderFile = "/" + organization_id_user + "/Law/" + dv.law_id;
                                                        newFileName = Path.Combine(folderFile, Path.GetFileName(fileName));

                                                        var listPathEdit_File = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                                        var pathEdit_File = "";
                                                        foreach (var itemEdit in listPathEdit_File)
                                                        {
                                                            if (itemEdit.Trim() != "")
                                                            {
                                                                pathEdit_File += "/" + Path.GetFileName(itemEdit);
                                                            }
                                                        }

                                                        fileInfo = new FileInfo(root + pathEdit_File);
                                                        if (fileInfo.Exists)
                                                        {
                                                            fileName = fileName.Replace(fileInfo.Extension, "");
                                                            fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                                            // Convert to unsign
                                                            Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                                            fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                                            newFileName = Path.Combine("/" + organization_id_user + "/Law/" + dv.law_id, fileName);
                                                        }

                                                        var pathFileLaw = "";
                                                        var path_from_excel = Path.GetFileName(vl.ToString().Trim());
                                                        if (File.Exists(root + extractPath + "/" + path_from_excel))
                                                        {
                                                            pathFileLaw = extractPath + "/" + path_from_excel;
                                                        }
                                                        else if (File.Exists(root + extractPath + "/" + nameFolderZip + "/" + path_from_excel))
                                                        {
                                                            pathFileLaw = extractPath + "/" + nameFolderZip + "/" + path_from_excel;
                                                        }
                                                        var listPathEdit_2 = Regex.Replace(pathFileLaw.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                                        var pathEdit_2 = "";
                                                        foreach (var itemEdit in listPathEdit_2)
                                                        {
                                                            if (itemEdit.Trim() != "")
                                                            {
                                                                pathEdit_2 += "/" + Path.GetFileName(itemEdit);
                                                            }
                                                        }
                                                        pathFileLaw = pathEdit_2;

                                                        law_files file_law = new law_files();
                                                        file_law.law_id = dv.law_id;
                                                        file_law.file_path = "/Portals/" + organization_id_user + "/Law/" + dv.law_id + "/" + fileName;
                                                        file_law.file_name = vl.ToString().Trim().Trim('"');
                                                        file_law.file_size = new FileInfo(root + pathFileLaw).Length;
                                                        file_law.file_type = file_law.file_name.Substring(file_law.file_name.LastIndexOf(".") + 1).ToLower();
                                                        file_law.is_order = 1;
                                                        file_law.is_drafted = false;
                                                        file_law.law_file_type = 0;
                                                        file_law.message = "Thêm file văn bản luật " + dv.law_name;
                                                        file_law.created_by = uid;
                                                        file_law.created_date = DateTime.Now;
                                                        file_law.created_ip = ip;
                                                        file_law.created_token_id = tid;
                                                        fileLaws.Add(file_law);

                                                        if (fileInfo != null)
                                                        {
                                                            var strDirectory = "/" + organization_id_user + "/Law/" + dv.law_id;
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

                                                            var listPathEdit_1 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                                            var pathEdit_1 = "";
                                                            foreach (var itemEdit in listPathEdit_1)
                                                            {
                                                                if (itemEdit.Trim() != "")
                                                                {
                                                                    pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                                                }
                                                            }
                                                            File.Move(root + pathFileLaw, root + pathEdit_1);
                                                            //File.Move(pathFileLaw, root + newFileName);
                                                        }
                                                    }
                                                    break;
                                                case "is_new":
                                                    dv.is_new = vl != null && vl.ToString().Trim() != "" ? int.Parse(vl.ToString()) : 1;
                                                    break;
                                            }
                                        }
                                        dv.is_active = true;
                                        dv.times_view = 0;
                                        dv.times_download = 0;
                                        dv.organization_id = is_admin;
                                        dv.created_by = uid;
                                        dv.created_date = DateTime.Now;
                                        dv.created_ip = ip;
                                        dv.created_token_id = tid;
                                        dvs.Add(dv);
                                    }

                                    if (dvs.Count > 0)
                                    {
                                        db.law_documents.AddRange(dvs);
                                    }
                                    if (fileLaws.Count > 0)
                                    {
                                        db.law_files.AddRange(fileLaws);
                                    }
                                    db.SaveChanges();

                                    #endregion
                                    if (fileUpLocal.Count > 0)
                                    {
                                        foreach (var itemPath in fileUpLocal)
                                        {
                                            var listPathEdit = Regex.Replace(itemPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                            var pathEdit = "";
                                            foreach (var itemEdit in listPathEdit)
                                            {
                                                if (itemEdit.Trim() != "")
                                                {
                                                    pathEdit += "/" + Path.GetFileName(itemEdit);
                                                }
                                            }
                                            string pathDel = root + pathEdit;
                                            if (System.IO.File.Exists(pathDel))
                                            {
                                                System.IO.File.Delete(pathDel);
                                            }
                                        }
                                    }
                                    if (fileUpLocalRoot.Count > 0)
                                    {
                                        foreach (var path in fileUpLocalRoot)
                                        {
                                            if (System.IO.File.Exists(path))
                                            {
                                                System.IO.File.Delete(path);
                                            }
                                        }
                                    }
                                    if (extractPath.Contains("\\Law\\") && extractPath.Contains("\\Excel\\"))
                                    {
                                        if (Directory.Exists(root + extractPath))
                                        {
                                            Directory.Delete(root + extractPath, true);
                                        }
                                    }
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", ms = "ok" });
                                }
                                catch (DbEntityValidationException e)
                                {
                                    string contents = helper.ExceptionMessage(e);
                                    if (!helper.debug)
                                    {
                                        contents = "";
                                    }
                                    Log.Error(contents);
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e.Message });
                                }
                                catch (Exception e)
                                {
                                    string contents = helper.ExceptionMessage(e);
                                    if (!helper.debug)
                                    {
                                        contents = "";
                                    }
                                    Log.Error(contents);
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e.Message });
                                }

                            }

                            #endregion end read excel
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/law_documents/ImportLaw", ip, tid, "Lỗi khi import law documents", 0, "law_documents");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/law_documents/ImportLaw", ip, tid, "Lỗi khi import law documents", 0, "law_documents");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }
        #endregion

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
                        log.controller = domainurl + "law_documents/GetDataProc";
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "law_documents/GetDataProc", ip, tid, nameErrProc, 0, "law_documents");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "law_documents/GetDataProc", ip, tid, nameErrProc, 0, "law_documents");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }
        #endregion

        public class InfoLaw_Short
        {
            public string law_id { get; set; }
            public string law_name { get; set; }
        }
        public class InfoFile_Upload
        {
            public int file_id { get; set; }
            public string law_id { get; set; }
            public string file_name { get; set; }
        }
        // end
    }
}
