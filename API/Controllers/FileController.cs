//using API.Helper;
//using API.Models;
//using Helper;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using OfficeOpenXml;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data.Entity;
//using System.Data.Entity.Validation;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Sockets;
//using System.Reflection;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Http;

//namespace Controllers
//{
//    [Authorize]
//    public class FileController : ApiController
//    {
//        Upload upload = new Upload();
//        string PortalConfigs = ConfigurationManager.AppSettings["Portals"] ?? "";

//        public string getipaddress()
//        {
//            var host = Dns.GetHostEntry(Dns.GetHostName());
//            foreach (var ip in host.AddressList)
//            {
//                if (ip.AddressFamily == AddressFamily.InterNetwork)
//                {
//                    return ip.ToString();
//                }
//            }
//            return "localhost";
//        }

//        #region Folder

//        [HttpPost]
//        public async Task<HttpResponseMessage> Add_Thumuc(file_folder model)
//        {
//            var identity = User.Identity as ClaimsIdentity;

//            IEnumerable<Claim> claims = identity.Claims;
//            string ip = getipaddress();
//            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
//            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
//            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
//            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
//            try
//            {
//                if (identity == null)
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//                }
//            }
//            catch
//            {
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//            }
//            try
//            {
//                using (DBEntities db = new DBEntities())
//                {
//                    model.status = true;
//                    model.created_ip = ip;
//                    model.created_by = uid;
//                    model.created_date = DateTime.Now;
//                    model.created_token_id = tid;
//                    model.file_number = 0;
//                    model.views = 0;
//                    model.capacity = 0;
//                    db.file_folder.Add(model);
//                    #region  add Log
//                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents = "" }), domainurl + "File/Add_Thumuc", ip, tid, "Thêm mới thư mục " + model.folder_name, 1, "Folder");
//                    #endregion
//                    await db.SaveChangesAsync();
//                    string root = HttpContext.Current.Server.MapPath("~/Portals");
//                    string strPath = root + "/Folder/" + model.folder_id;
//                    bool exists = Directory.Exists(strPath);
//                    if (!exists)
//                        Directory.CreateDirectory(strPath);
//                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
//                }

//            }
//            catch (DbEntityValidationException e)
//            {
//                string contents = helper.getCatchError(e, null);
//                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "File/Add_Thumuc", ip, tid, "Lỗi khi thêm thư mục", 0, "Folder");
//                if (!helper.debug)
//                {
//                    contents = "";
//                }
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//            }
//            catch (Exception e)
//            {
//                string contents = helper.ExceptionMessage(e);
//                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "File/Add_Thumucs", ip, tid, "Lỗi khi thêm Thumuc", 0, "Thumuc");
//                if (!helper.debug)
//                {
//                    contents = "";
//                }
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//            }
//        }

//        [HttpPut]
//        public async Task<HttpResponseMessage> Update_Thumuc(file_folder model)
//        {
//            var identity = User.Identity as ClaimsIdentity;

//            IEnumerable<Claim> claims = identity.Claims;
//            string ip = getipaddress();
//            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
//            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
//            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
//            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
//            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
//            List<string> delfiles = new List<string>();
//            try
//            {
//                if (identity == null || !ad)
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//                }
//            }
//            catch
//            {
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//            }
//            try
//            {
//                using (DBEntities db = new DBEntities())
//                {
//                    model.modified_ip = ip;
//                    model.modified_by = uid;
//                    model.modified_date = DateTime.Now;
//                    model.modified_token_id = tid;
//                    db.Entry(model).State = EntityState.Modified;
//                    #region  add Log
//                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents = "" }), domainurl + "File/Update_Thumuc", ip, tid, "Chỉnh sửa thư mục " + model.folder_name, 1, "Folder");
//                    #endregion
//                    await db.SaveChangesAsync();
//                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
//                }

//            }
//            catch (DbEntityValidationException e)
//            {
//                string contents = helper.getCatchError(e, null);
//                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "File/Update_Thumuc", ip, tid, "Lỗi khi cập nhật thư mục", 0, "Folder");
//                if (!helper.debug)
//                {
//                    contents = "";
//                }
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//            }
//            catch (Exception e)
//            {
//                string contents = helper.ExceptionMessage(e);
//                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "File/Update_Thumuc", ip, tid, "Lỗi khi cập nhật thư mục", 0, "Folder");
//                if (!helper.debug)
//                {
//                    contents = "";
//                }
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//            }
//        }

//        [HttpDelete]
//        public async Task<HttpResponseMessage> Del_Thumuc([FromBody] List<int> ids)
//        {
//            var identity = User.Identity as ClaimsIdentity;
//            IEnumerable<Claim> claims = identity.Claims;
//            try
//            {
//                if (identity == null)
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//                }
//            }
//            catch
//            {
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//            }
//            try
//            {
//                string ip = getipaddress();
//                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
//                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
//                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
//                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
//                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
//                try
//                {
//                    using (DBEntities db = new DBEntities())
//                    {
//                        List<string> paths = new List<string>();
//                        var das = await db.file_folder.Where(a => ids.Contains(a.folder_id)).ToListAsync();
//                        if (das != null)
//                        {
//                            List<file_folder> del = new List<file_folder>();
//                            foreach (var da in das)
//                            {
//                                if (da.created_by == uid || ad)
//                                {
//                                    del.Add(da);
//                                    paths.Add(HttpContext.Current.Server.MapPath("~/Portals/Folder/") + da.folder_id);
//                                    #region  add Log
//                                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "File/Del_Thumuc", ip, tid, "Xoá thư mục " + da.folder_name, 1, "Folder");
//                                    #endregion
//                                }
//                            }
//                            if (del.Count == 0)
//                            {
//                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa thư mục này." });
//                            }
//                            db.file_folder.RemoveRange(del);
//                        }
//                        await db.SaveChangesAsync();
//                        foreach (string strPath in paths)
//                        {
//                            bool exists = Directory.Exists(strPath);
//                            if (exists)
//                                Directory.Delete(strPath, true);
//                        }
//                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
//                    }
//                }
//                catch (DbEntityValidationException e)
//                {
//                    string contents = helper.getCatchError(e, null);
//                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "File/Del_Thumuc", ip, tid, "Lỗi khi xoá thư mục", 0, "Folder");
//                    if (!helper.debug)
//                    {
//                        contents = "";
//                    }
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//                }
//                catch (Exception e)
//                {
//                    string contents = helper.ExceptionMessage(e);
//                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "File/Del_Thumuc", ip, tid, "Lỗi khi xoá thư mục", 0, "Folder");
//                    if (!helper.debug)
//                    {
//                        contents = "";
//                    }
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//                }
//            }
//            catch (Exception)
//            {
//                return Request.CreateResponse(HttpStatusCode.BadRequest);
//            }

//        }

//        [HttpPut]
//        public async Task<HttpResponseMessage> Update_statusThumuc([FromBody] JObject data)
//        {
//            List<int> ids = data["ids"].ToObject<List<int>>();
//            List<bool> tts = data["tts"].ToObject<List<bool>>();
//            var identity = User.Identity as ClaimsIdentity;
//            IEnumerable<Claim> claims = identity.Claims;
//            try
//            {
//                if (identity == null)
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//                }
//            }
//            catch
//            {
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//            }
//            try
//            {
//                string ip = getipaddress();
//                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
//                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
//                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
//                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
//                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
//                try
//                {
//                    using (DBEntities db = new DBEntities())
//                    {
//                        var das = await db.file_folder.Where(a => ids.Contains(a.folder_id)).ToListAsync();
//                        if (das != null)
//                        {
//                            List<file_folder> del = new List<file_folder>();
//                            for (int i = 0; i < das.Count; i++)
//                            {
//                                var da = das[i];
//                                if (ad)
//                                {
//                                    #region  add Log
//                                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "File/Update_statusThumuc", ip, tid, "Cập nhật trạng thái thư mục " + da.folder_name, 1, "Folder");
//                                    #endregion
//                                    da.status = tts[i];
//                                }
//                            }
//                            await db.SaveChangesAsync();
//                        }
//                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
//                    }
//                }
//                catch (DbEntityValidationException e)
//                {
//                    string contents = helper.getCatchError(e, null);
//                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "File/Update_statusThumuc", ip, tid, "Lỗi khi cập nhật trạng thái thư mục", 0, "Folder");
//                    if (!helper.debug)
//                    {
//                        contents = "";
//                    }
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//                }
//                catch (Exception e)
//                {
//                    string contents = helper.ExceptionMessage(e);
//                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "File/Update_statusThumucs", ip, tid, "Lỗi khi cập nhật trạng thái thư mục", 0, "Folder");
//                    if (!helper.debug)
//                    {
//                        contents = "";
//                    }
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//                }
//            }
//            catch (Exception)
//            {
//                return Request.CreateResponse(HttpStatusCode.BadRequest);
//            }

//        }

//        #region Excel
//        [HttpPost]
//        public async Task<HttpResponseMessage> ImportExcel()
//        {
//            string ListErr = "";
//            var identity = User.Identity as ClaimsIdentity;
//            IEnumerable<Claim> claims = identity.Claims;
//            try
//            {
//                if (identity == null)
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//                }
//            }
//            catch
//            {
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//            }
//            string ip = getipaddress();
//            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
//            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
//            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
//            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
//            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
//            try
//            {
//                using (DBEntities db = new DBEntities())
//                {
//                    if (!Request.Content.IsMimeMultipartContent())
//                    {
//                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
//                    }

//                    string root = HttpContext.Current.Server.MapPath("~/Portals");
//                    string strPath = root + "/Folder";
//                    bool exists = Directory.Exists(strPath);
//                    if (!exists)
//                        Directory.CreateDirectory(strPath);
//                    var provider = new MultipartFormDataStreamProvider(root);

//                    // Read the form data and return an async task.
//                    var task = Request.Content.ReadAsMultipartAsync(provider).
//                    ContinueWith<HttpResponseMessage>(t =>
//                    {
//                        if (t.IsFaulted || t.IsCanceled)
//                        {
//                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
//                        }
//                        foreach (MultipartFileData fileData in provider.FileData)
//                        {
//                            string fileName = "";
//                            if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
//                            {
//                                fileName = Guid.NewGuid().ToString();
//                            }
//                            fileName = fileData.Headers.ContentDisposition.FileName;
//                            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
//                            {
//                                fileName = fileName.Trim('"');
//                            }
//                            if (fileName.Contains(@"/") || fileName.Contains(@"\"))
//                            {
//                                fileName = Path.GetFileName(fileName);
//                            }
//                            if (!fileName.ToLower().Contains(".xls"))
//                            {
//                                ListErr = "File Excel không đúng định dạng! Kiểm tra lại mẫu Import";
//                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = ListErr });
//                            }
//                            var newFileName = Path.Combine(root + "/Import", fileName);
//                            var fileInfo = new FileInfo(newFileName);
//                            if (fileInfo.Exists)
//                            {
//                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
//                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

//                                newFileName = Path.Combine(root + "/Import", fileName);
//                            }
//                            if (!Directory.Exists(fileInfo.Directory.FullName))
//                            {
//                                Directory.CreateDirectory(fileInfo.Directory.FullName);
//                            }
//                            File.Move(fileData.LocalFileName, newFileName);
//                            FileInfo temp = new FileInfo(newFileName);
//                            using (ExcelPackage pck = new ExcelPackage(temp))
//                            {
//                                List<file_folder> dvs = new List<file_folder>();
//                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
//                                List<string> cols = new List<string>();
//                                var dvcs = db.file_folder.Select(a => new { a.folder_id, a.folder_name, a.created_by }).ToList();
//                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
//                                {
//                                    if (ws.Cells[i, 1].Value == null)
//                                    {
//                                        break;
//                                    }
//                                    file_folder dv = new file_folder();
//                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
//                                    {
//                                        if (ws.Cells[3, j].Value == null)
//                                        {
//                                            break;
//                                        }
//                                        var column = ws.Cells[3, j].Value;
//                                        var vl = ws.Cells[i, j].Value;
//                                        if (column != null && vl != null)
//                                        {
//                                            PropertyInfo propertyInfo = db.file_folder.GetType().GetProperty(column.ToString());
//                                            propertyInfo.SetValue(db.file_folder, Convert.ChangeType(vl,
//                                            propertyInfo.PropertyType), null);
//                                        }
//                                    }
//                                    if (dvcs.Count(a => (a.folder_id == dv.folder_id || a.folder_name == dv.folder_name) && a.created_by == uid) > 0)
//                                        break;
//                                    dvs.Add(dv);
//                                }
//                                if (dvs.Count > 0)
//                                {
//                                    db.file_folder.AddRange(dvs);
//                                    db.SaveChangesAsync();
//                                    File.Delete(newFileName);
//                                }
//                            }
//                        }
//                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
//                    });
//                    return await task;
//                }

//            }
//            catch (DbEntityValidationException e)
//            {
//                string contents = helper.getCatchError(e, null);
//                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "File/ImportExcel", ip, tid, "Lỗi khi import thư mục", 0, "Folder");
//                if (!helper.debug)
//                {
//                    contents = "";
//                }
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//            }
//            catch (Exception e)
//            {
//                string contents = helper.ExceptionMessage(e);
//                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "File/ImportExcel", ip, tid, "Lỗi khi import thư mục", 0, "Folder");
//                if (!helper.debug)
//                {
//                    contents = "";
//                }
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//            }
//        }
//        #endregion
//        #endregion

//        #region File

//        [HttpPost]
//        public async Task<HttpResponseMessage> Update_File()
//        {
//            var identity = User.Identity as ClaimsIdentity;
//            string fdmodel = "";
//            bool IsPublic = true;
//            IEnumerable<Claim> claims = identity.Claims;
//            string ip = getipaddress();
//            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
//            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
//            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
//            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
//            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
//            List<string> delfiles = new List<string>();
//            List<string> ltFiles = new List<string>();
//            List<string> ltpathFiles = new List<string>();
//            List<string> ltnameFiles = new List<string>();
//            try
//            {
//                if (identity == null)
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//                }
//            }
//            catch
//            {
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//            }
//            try
//            {
//                using (DBEntities db = new DBEntities())
//                {
//                    if (!Request.Content.IsMimeMultipartContent())
//                    {
//                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
//                    }
//                    string root = HttpContext.Current.Server.MapPath("~/Portals");
//                    string jwtcookie = HttpContext.Current.Request.Cookies["jwt"].Value;
//                    string strPath = root + "/Folder";
//                    bool exists = Directory.Exists(strPath);
//                    if (!exists)
//                        Directory.CreateDirectory(strPath);
//                    var provider = new MultipartFormDataStreamProvider(root + "/Temp");

//                    // Read the form data and return an async task.
//                    var task = Request.Content.ReadAsMultipartAsync(provider).
//                    ContinueWith<HttpResponseMessage>(t =>
//                   {
//                       if (t.IsFaulted || t.IsCanceled)
//                       {
//                           Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
//                       }
//                       fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
//                       file_info model = JsonConvert.DeserializeObject<file_info>(fdmodel);
//                       IsPublic = provider.FormData.GetValues("IsPublic") != null ? provider.FormData.GetValues("IsPublic").SingleOrDefault().ToUpper() == "TRUE" : model.is_public;
//                       strPath = root + "/Folder/" + model.folder_id;
//                       exists = Directory.Exists(strPath);
//                       if (!exists)
//                           Directory.CreateDirectory(strPath);
//                       // This illustrates how to get thefile names.
//                       foreach (MultipartFileData fileData in provider.FileData)
//                       {
//                           string fileName = "";
//                           if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
//                           {
//                               fileName = Guid.NewGuid().ToString();
//                           }
//                           fileName = fileData.Headers.ContentDisposition.FileName;
//                           if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
//                           {
//                               fileName = fileName.Trim('"');
//                           }
//                           if (fileName.Contains(@"/") || fileName.Contains(@"\"))
//                           {
//                               fileName = Path.GetFileName(fileName);
//                           }
//                           var newFileName = Path.Combine(strPath, fileName);
//                           var fileInfo = new FileInfo(newFileName);
//                           if (fileInfo.Exists)
//                           {
//                               fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
//                               fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

//                               newFileName = Path.Combine(strPath, fileName);
//                           }
//                           //File.Move(fileData.LocalFileName, newFileName);
//                           //if (helper.IsImageFileName(newFileName))
//                           //{
//                           //    helper.ResizeImage(newFileName, 1920, 1080, 90);
//                           //    helper.ResizeThumbImage(newFileName, 160, 160);

//                           //}
//                           Task.Run(() => upload.UpdateFile(jwtcookie, root, fileData, ("/Folder/" + model.folder_id + "/" + fileName), 160));
//                           ltFiles.Add(newFileName);
//                           ltpathFiles.Add("/Portals/Folder/" + model.folder_id + "/" + fileName);
//                           ltnameFiles.Add(fileName);
//                       }
//                       try
//                       {
//                           if (ltFiles.Count > 0)
//                           {
//                               int c = db.file_info.Count(a => a.folder_id == model.folder_id);
//                               string filenames = "";
//                               List<file_info> plf = new List<file_info>();
//                               for (int i = 0; i < ltFiles.Count; i++)
//                               {
//                                   string pathFile = PortalConfigs != "" ? (PortalConfigs + ltpathFiles[i].Replace("/", "\\")) : ltFiles[i];
//                                   var ifi = new FileInfo(pathFile);
//                                   file_info fi = new file_info();
//                                   fi.folder_id = model.folder_id;
//                                   fi.is_public = IsPublic;
//                                   fi.views = 0;
//                                   fi.download = 0;
//                                   fi.created_date = DateTime.Now;
//                                   fi.created_by = uid;
//                                   fi.file_name = ltnameFiles[i];
//                                   fi.is_filepath = ltpathFiles[i];
//                                   fi.capacity = ifi.Length;
//                                   filenames += "<file>" + ltnameFiles[i] + "</file>";
//                                   fi.is_order = c + i;
//                                   fi.status = true;
//                                   fi.created_ip = ip;
//                                   fi.created_token_id = tid;
//                                   fi.is_image = helper.IsImageFileName(ltnameFiles[i]);
//                                   plf.Add(fi);
//                               }
//                               if (plf.Count > 0)
//                               {
//                                   db.file_info.AddRange(plf);
//                                   var tm = db.file_folder.Find(model.folder_id);
//                                   tm.views = plf.Count() + c;
//                                   tm.capacity += plf.Sum(a => a.capacity);
//                                   #region  add Log
//                                   file_log log = new file_log();
//                                   log.folder_id = model.folder_id;
//                                   log.contents = "Thêm File " + filenames;
//                                   log.created_date = DateTime.Now;
//                                   log.user_id = uid;
//                                   log.created_token_id = tid;
//                                   log.created_ip = ip;
//                                   log.log_type = 0;
//                                   db.file_log.Add(log);
//                                   #endregion
//                               }
//                           }
//                           db.SaveChanges();
//                           //Add ảnh
//                           foreach (string fpath in delfiles)
//                           {
//                               File.Delete(fpath);
//                           }
//                           return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
//                       }
//                       catch (DbEntityValidationException e)
//                       {
//                           foreach (string fpath in ltFiles)
//                           {
//                               File.Delete(fpath);
//                           }
//                           string contents = helper.getCatchError(e, null);
//                           helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "File/Update_File", ip, tid, "Lỗi khi thêm file trong thư mục", 0, "File");
//                           if (!helper.debug)
//                           {
//                               contents = "";
//                           }
//                           return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//                       }
//                       catch (Exception e)
//                       {
//                           foreach (string fpath in ltFiles)
//                           {
//                               File.Delete(fpath);
//                           }
//                           string contents = helper.ExceptionMessage(e);
//                           helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "File/Update_File", ip, tid, "Lỗi khi thêm file trong thư mục", 0, "File");
//                           if (!helper.debug)
//                           {
//                               contents = "";
//                           }
//                           return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//                       }
//                   });
//                    return await task;
//                }

//            }
//            catch (DbEntityValidationException e)
//            {
//                string contents = helper.getCatchError(e, null);
//                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "File/Update_PlanFile", ip, tid, "Lỗi khi cập nhật file cho dự án", 0, "PlanFile");
//                if (!helper.debug)
//                {
//                    contents = "";
//                }
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//            }
//            catch (Exception e)
//            {
//                string contents = helper.ExceptionMessage(e);
//                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "File/Update_PlanFile", ip, tid, "Lỗi khi cập nhật file cho dự án", 0, "PlanFile");
//                if (!helper.debug)
//                {
//                    contents = "";
//                }
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//            }
//        }

//        [HttpPut]
//        public async Task<HttpResponseMessage> Del_File([FromBody] List<int> ids)
//        {
//            var identity = User.Identity as ClaimsIdentity;
//            IEnumerable<Claim> claims = identity.Claims;
//            try
//            {
//                if (identity == null)
//                {
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//                }
//            }
//            catch
//            {
//                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
//            }
//            try
//            {
//                string ip = getipaddress();
//                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
//                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
//                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
//                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
//                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
//                try
//                {
//                    using (DBEntities db = new DBEntities())
//                    {
//                        List<string> paths = new List<string>();
//                        var das = await db.file_info.Where(a => ids.Contains(a.file_id)).ToListAsync();
//                        if (das != null)
//                        {
//                            List<file_info> del = new List<file_info>();
//                            for (int i = 0; i < das.Count; i++)
//                            {
//                                var da = das[i];
//                                if (da.created_by == uid || ad)
//                                {
//                                    #region  add Log
//                                    file_log log = new file_log();
//                                    log.folder_id = da.folder_id;
//                                    log.file_id = da.file_id;
//                                    log.contents = "Xoá File <b>" + da.file_name + "</b>";
//                                    log.created_date = DateTime.Now;
//                                    log.user_id = uid;
//                                    log.created_token_id = tid;
//                                    log.created_ip = ip;
//                                    log.log_type = 0;
//                                    db.file_log.Add(log);
//                                    #endregion
//                                    if (da.created_by == uid || ad)
//                                    {
//                                        del.Add(da);
//                                        var tm = db.file_folder.Find(da.folder_id);
//                                        tm.views -= 1;
//                                        tm.capacity -= da.capacity;
//                                        if (!string.IsNullOrWhiteSpace(da.is_filepath))
//                                            paths.Add(HttpContext.Current.Server.MapPath("~/") + da.is_filepath);
//                                    }
//                                }
//                                if (del.Count == 0)
//                                {
//                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa file này." });
//                                }
//                                db.file_info.RemoveRange(del);
//                            }
//                            await db.SaveChangesAsync();
//                            foreach (string strPath in paths)
//                            {
//                                bool exists = Directory.Exists(strPath);
//                                if (exists)
//                                    System.IO.File.Delete(strPath);
//                            }
//                        }
//                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
//                    }
//                }
//                catch (DbEntityValidationException e)
//                {
//                    string contents = helper.getCatchError(e, null);
//                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "File/Del_File", ip, tid, "Lỗi khi xoá file", 0, "File");
//                    if (!helper.debug)
//                    {
//                        contents = "";
//                    }
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//                }
//                catch (Exception e)
//                {
//                    string contents = helper.ExceptionMessage(e);
//                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "File/Del_File", ip, tid, "Lỗi khi xoá file", 0, "File");
//                    if (!helper.debug)
//                    {
//                        contents = "";
//                    }
//                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
//                }
//            }
//            catch (Exception)
//            {
//                return Request.CreateResponse(HttpStatusCode.BadRequest);
//            }

//        }

//        #endregion
//    }
//}