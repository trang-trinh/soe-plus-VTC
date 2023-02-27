using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace API.Controllers.Filles
{
    [Authorize(Roles = "login")]
    public class FileMainController : ApiController
    {
        // GET: FileMain
        //File module key
        private const string const_module_key = "M11";

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
        public async Task<HttpResponseMessage> Add_FileMain()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/FileShare/" + uid;
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
                        var md = provider.FormData.GetValues("model").SingleOrDefault();
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        file_info model = JsonConvert.DeserializeObject<file_info>(fdmodel);
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
                            fileName = helper.convertToUnSign3(fileName);
                            newFileName = Path.Combine(root + "/" + dvid + "/FileShare/" + uid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/FileShare/" + uid, fileName);
                            }
                            model.name_file = fileName;
                            model.is_filepath = "/Portals/" + dvid + "/FileShare/" + uid + "/" + fileName;
                            if (helper.IsImageFileName(newFileName))
                            {
                                model.is_image = true;
                                ffileData = fileData;
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
                        // This illustrates how to get thefile names.
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        model.created_token_id = tid;
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_token_id = tid;
                        model.created_ip = ip;

                        db.file_info.Add(model);
                        db.SaveChanges();
                        #region  add Log
                        helper.saveLogFiles(uid, 5, model.file_id, null, "Thêm mới file " + model.file_name, ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_FileMain()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string root1 = HttpContext.Current.Server.MapPath("~/");
                    string strPath = root + "/" + dvid + "/FileShare/" + uid;
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
                        var md = provider.FormData.GetValues("model").SingleOrDefault();
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        file_info model = JsonConvert.DeserializeObject<file_info>(fdmodel);
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            //xoa file cu
                            //file_info file_del = db.file_info.Where(a => a.file_id == model.file_id).FirstOrDefault();
                            //if (file_del != null)
                            //{
                            //    bool exists = System.IO.File.Exists(root1 + file_del.is_filepath);
                            //    if (exists)
                            //        System.IO.File.Delete(root1 + file_del.is_filepath);
                            //}
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
                            fileName = helper.convertToUnSign3(fileName);
                            newFileName = Path.Combine(root + "/" + dvid + "/FileShare/" + uid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/FileShare/" + uid, fileName);
                            }
                            model.name_file = fileName;
                            if (helper.IsImageFileName(newFileName))
                            {
                                model.is_filepath = "/Portals/" + dvid + "/FileShare/" + uid + "/" + fileName;
                                model.is_image = true;
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
                                model.is_filepath = "/Portals/" + dvid + "/FileShare/" + uid + "/" + fileName;
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
                        // This illustrates how to get thefile names.
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        model.created_token_id = tid;
                        db.Entry(model).State = EntityState.Modified;
                        db.SaveChanges();
                        #region  add Log
                        helper.saveLogFiles(uid, 3, model.file_id, null, "sửa file " + model.file_name, ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_File_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string root1 = HttpContext.Current.Server.MapPath("~/");
                    string strPath = root + "/" + dvid + "/FileShare/" + uid;
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
                        fdmodel = provider.FormData.GetValues("file").SingleOrDefault();
                        file_info model = JsonConvert.DeserializeObject<file_info>(fdmodel);
                        var id_doc = Int32.Parse(model.id_key);
                        doc_master doc = db.doc_master.Where(a => a.doc_master_id == id_doc).FirstOrDefault();
                        if (doc != null) // get properties from doc_master
                        {
                            model.name_file = doc.file_name != null ? doc.file_name : "File coppy";
                            model.is_public = true;
                            model.is_order = 100;
                            model.is_image = false;
                            model.status = true;
                            model.capacity = doc.file_size;
                            model.file_type = doc.file_type != null ? doc.file_type : "doc";
                            model.is_image = helper.IsImageFileName(doc.file_name);

                        }
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        model.created_token_id = tid;
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_token_id = tid;
                        model.created_ip = ip;

                        //lien ket vb
                        if (model.doc_type == 2)
                        {
                            model.is_filepath = doc.file_path;
                            file_info del = db.file_info.Where(a => a.id_key == model.id_key && a.doc_type == model.doc_type).FirstOrDefault();
                            if (del != null)
                            {
                                db.file_info.Remove(del); // xoa file cu
                            }
                            db.file_info.Add(model);
                        }
                        //coppy vb
                        else if (model.doc_type == 1)
                        {
                            string list_folder_id = provider.FormData.GetValues("ids").SingleOrDefault();
                            string[] ids = list_folder_id.Split(',');
                            if (ids.Count() > 0)
                            {
                                List<file_info> add_files = new List<file_info>();
                                int count = 1;
                                foreach (var da in ids)
                                {
                                    //check ton tai cua file lien ket trong folder "da"
                                    file_info del = db.file_info.Where(a => a.id_key == model.id_key && a.doc_type == model.doc_type && a.folder_id == da).FirstOrDefault();
                                    if (del == null)
                                    {
                                        // xoa phan tu da co trong file cu
                                        //ids = ids.Where(val => val != da).ToArray();
                                        file_info file = new file_info();
                                        file = model;
                                        file.folder_id = da;
                                        file.is_order = count;
                                        count++;
                                        if (System.IO.Directory.Exists(strPath))
                                        {
                                            string[] files = System.IO.Directory.GetFiles(strPath);
                                            // Copy the files and overwrite destination files if they already exist.
                                            var fileName = System.IO.Path.GetFileName(root1 + doc.file_path);
                                            var desFile = System.IO.Path.Combine(strPath, fileName);
                                            var fileInfo = new FileInfo(System.IO.Path.GetFileName(desFile));
                                            if (File.Exists(desFile))
                                            {
                                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                                                desFile = System.IO.Path.Combine(strPath, fileName);
                                            }
                                            System.IO.File.Copy(root1 + doc.file_path, desFile, true);
                                            file.is_filepath = "/Portals/" + dvid + "/FileShare/" + uid + "/" + fileName;
                                            file.file_name = "Coppy-" + model.file_name;
                                            file.name_file = fileName;

                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Source directory does not exist or could not be found.", err = "1" });

                                        }
                                        db.file_info.Add(file);
                                        db.SaveChanges();

                                    }
                                }
                                //db.file_info.AddRange(add_files);
                                //list file cu -> khong co torng list moi
                                List<file_info> itemFileExist = db.file_info.Where(a => a.id_key == model.id_key).ToList();
                                if (itemFileExist.Count() > 0)
                                {
                                    List<string> paths = new List<string>();
                                    List<file_info> del_exists = new List<file_info>();
                                    foreach (var item in itemFileExist)
                                    {
                                        if (!ids.Contains(item.folder_id))
                                        {
                                            del_exists.Add(item); // del_exists - file cu khong co trong list moi
                                            if (!string.IsNullOrWhiteSpace(item.is_filepath))
                                                paths.Add(root1 + item.is_filepath);
                                        }
                                    }
                                    db.file_info.RemoveRange(del_exists);
                                    foreach (string strPath in paths) // xoa file cu khong co trong list moi
                                    {
                                        bool exists = System.IO.File.Exists(strPath);
                                        if (exists)
                                            System.IO.File.Delete(strPath);
                                    }
                                }
                            }

                        }
                        db.SaveChanges();
                        #region  add Log
                        helper.saveLogFiles(uid, 1, model.file_id, null, "Link từ kho dữ liệu file " + model.file_name, ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> Del_Files([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.file_info.Where(a => ids.Contains(a.file_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<file_info> del = new List<file_info>();
                            if (das.Count > 0)
                            {
                                foreach (var da in das)
                                {
                                    del.Add(da);
                                    #region  add Log
                                    helper.saveLogFiles(uid, 4, da.file_id, null, "Xóa file " + da.file_name, ip, tid);
                                    #endregion
                                    bool check_exist = da.is_filepath.Contains(@"/Portals/" + dvid);
                                    if (!string.IsNullOrWhiteSpace(da.is_filepath) && da.id_key == null)
                                    {
                                        var path_temp = HttpContext.Current.Server.MapPath("~/") + "/Portals/" + (check_exist == true ? dvid : "") + "/FileShare/" + uid + "/" + Path.GetFileName(da.is_filepath);
                                        paths.Add(path_temp);
                                    }
                                }
                                db.file_info.RemoveRange(del);
                            }
                        }
                        await db.SaveChangesAsync();
                        foreach (string strPath in paths)
                        {
                            bool exists = System.IO.File.Exists(strPath);
                            if (exists)
                                System.IO.File.Delete(strPath);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FileMain/Del_File", ip, tid, "Lỗi khi xoá file", 0, "FileMain");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FileMain/Del_File", ip, tid, "Lỗi khi xoá file", 0, "FileMain");
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
        public async Task<HttpResponseMessage> Del_Share([System.Web.Mvc.Bind(Include = "user_id,id")] JObject data)
        {
            string user_id = data["user_id"].ToObject<string>();
            var ids = data["id"].ToObject<List<int>>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.file_info_share.Where(a => ids.Contains(a.file_id) && a.user_id == user_id).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<file_info_share> del = new List<file_info_share>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                                #region  add Log
                                //  helper.saveLogFiles(uid, 4, da.folder_id, null, "Bỏ chia sẻ file " + da.folder_id, ip, tid);
                                #endregion


                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa người dùng này." });
                            }
                            db.file_info_share.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FileMain/Del_File", ip, tid, "Lỗi khi xoá file", 0, "FileMain");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FileMain/Del_File", ip, tid, "Lỗi khi xoá file", 0, "FileMain");
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
        public async Task<HttpResponseMessage> Share_File_Module()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string rootXML = HttpContext.Current.Server.MapPath("~/");
                    //string strPath = root + "/FileFolder";
                    //bool exists = Directory.Exists(strPath);
                    //if (!exists)
                    //    Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();

                        List<file_info_share> listshare = JsonConvert.DeserializeObject<List<file_info_share>>(fdmodel);

                        // add or update table file_info
                        string filestring = "";
                        filestring = provider.FormData.GetValues("file_model").SingleOrDefault();
                        file_info file_model = JsonConvert.DeserializeObject<file_info>(filestring);
                        // check exist
                        file_info file_add = db.file_info.Where(a => a.id_key == file_model.file_id.ToString() && a.module_key == file_model.module_key).FirstOrDefault();
                        var file_id = 0;
                        if (file_add == null)
                        {
                            file_model.id_key = file_model.file_id.ToString();
                            file_model.is_public = true;
                            file_model.name_file = file_model.file_name;
                            file_model.created_by = null;
                            file_model.modified_by = null;
                            file_model.status = true;
                            file_model.file_id = 0;
                            file_model.is_image = helper.IsImageFileName(file_model.file_name);
                            db.file_info.Add(file_model);
                            db.SaveChanges();
                            file_id = file_model.file_id;
                        }
                        else file_id = file_add.file_id;

                        //delte all 
                        List<file_info_share> itemToRemove = db.file_info_share.Where(a => a.file_id == file_id).ToList();
                        if (itemToRemove.Count > 0)
                            db.file_info_share.RemoveRange(itemToRemove);
                        if (listshare.Count > 0)
                        {
                            var count = 1;
                            foreach (var item in listshare)
                            {
                                file_info_share folder = new file_info_share();
                                folder = item;
                                folder.file_id = file_id;
                                folder.modified_date = DateTime.Now;
                                folder.modified_by = uid;
                                folder.modified_ip = ip;
                                folder.modified_token_id = tid;
                                folder.created_token_id = tid;
                                folder.created_date = DateTime.Now;
                                folder.created_by = uid;
                                folder.created_token_id = tid;
                                folder.created_ip = ip;
                                count++;
                                db.file_info_share.Add(folder);
                                // send hub

                                #region senhub
                                sys_users user_send = db.sys_users.Find(uid);
                                sys_users user_receiver = db.sys_users.Find(item.user_id);
                                file_info file_info = db.file_info.Find(file_id);

                                var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.module_key = const_module_key;
                                sh.user_send = uid;
                                sh.receiver = item.user_id;
                                sh.icon = user_send.avatar;
                                sh.title = "Chia sẻ dữ liệu";
                                sh.contents = "<b>" + user_send.full_name + "</b> đã chia sẻ tài liệu <b>" + file_info.file_name + "</b> cho bạn ";
                                sh.is_type = 2;
                                sh.date_send = DateTime.Now;
                                sh.id_key = file_id.ToString();
                                sh.token_id = tid;
                                sh.created_date = DateTime.Now;
                                sh.created_by = uid;
                                sh.created_token_id = tid;
                                sh.created_ip = ip;
                                db.sys_sendhub.Add(sh);
                                string xml_result = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>";
                                xml_result += "<document>";
                                xml_result += "<element>";
                                xml_result += "<user_send>" + (sh.user_send ?? "") + "</user_send>";
                                xml_result += "<title>" + (sh.title ?? "") + "</title>";
                                xml_result += "<contents>" + (sh.contents ?? "") + "</contents>";
                                xml_result += "<date_send>" + (sh.date_send) + "</date_send>";
                                xml_result += "<file_name>" + (file_info.file_name) + "</file_name>";
                                xml_result += "<file_path>" + (file_info.is_filepath) + "</file_path>";
                                xml_result += "</element>";
                                xml_result += "</document>";

                                var user_now = db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                                System.Net.WebClient webc = new System.Net.WebClient();
                                string path = rootXML + helper.path_xml + "/FileMain/";
                                bool exists = Directory.Exists(path);
                                if (!exists)
                                    Directory.CreateDirectory(path);

                                string name_file = file_id.ToString() + ".xml";
                                string root_path = path + "/" + name_file;
                                string duong_dan = helper.path_xml + "/FileMain/" + name_file;
                                string url = ConfigurationManager.AppSettings["ValidAudience"] + duong_dan;

                                File.WriteAllText(root_path, xml_result);
                                var res_encr = helper.encryptXML(root_path, "document", helper.psKey);
                                if (res_encr != "OK")
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Không thể mã hoã file XML!", err = "1" });
                                };

                                #endregion
                            }
                            #region sendSocket
                            sys_users user_send_noti = db.sys_users.Find(uid);
                            var contentNoti = user_send_noti.full_name + " đã chia sẻ tài liệu cho bạn";
                            var users = listshare.Where(x => x.user_id != uid).Select(x => x.user_id).Distinct().ToList();
                            var message = new Dictionary<string, dynamic>
                                    {
                                        { "event", "sendNotify" },
                                        { "user_id", uid },
                                        { "title", "Công việc" },
                                        { "contents", contentNoti },
                                        { "date", DateTime.Now },
                                        { "uids", users },
                                    };
                            if (helper.socketClient != null && helper.socketClient.Connected == true)
                            {
                                try
                                {
                                    helper.socketClient.EmitAsync("sendData", message);
                                }
                                catch { };
                            }
                            #endregion

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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Share_File()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string rootXML = HttpContext.Current.Server.MapPath("~/");

                    //string strPath = root + "/FileFolder";
                    //bool exists = Directory.Exists(strPath);
                    //if (!exists)
                    //    Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var file_id = Int32.Parse(provider.FormData.GetValues("file_id").SingleOrDefault());

                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();

                        List<file_info_share> listshare = JsonConvert.DeserializeObject<List<file_info_share>>(fdmodel);


                        //delte all 
                        List<file_info_share> itemToRemove = db.file_info_share.Where(a => a.file_id == file_id).ToList();
                        if (itemToRemove.Count > 0)
                            db.file_info_share.RemoveRange(itemToRemove);
                        if (listshare.Count > 0)
                        {
                            var count = 1;
                            foreach (var item in listshare)
                            {
                                file_info_share folder = new file_info_share();
                                folder = item;
                                folder.file_id = file_id;
                                folder.modified_date = DateTime.Now;
                                folder.modified_by = uid;
                                folder.modified_ip = ip;
                                folder.modified_token_id = tid;
                                folder.created_token_id = tid;
                                folder.created_date = DateTime.Now;
                                folder.created_by = uid;
                                folder.created_token_id = tid;
                                folder.created_ip = ip;
                                count++;
                                db.file_info_share.Add(folder);
                                // send hub
                                sys_users user_send = db.sys_users.Find(uid);
                                sys_users user_receiver = db.sys_users.Find(item.user_id);
                                file_info file_info = db.file_info.Find(file_id);

                                var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.module_key = const_module_key;
                                sh.user_send = uid;
                                sh.receiver = item.user_id;
                                sh.icon = user_send.avatar;
                                sh.title = "Chia sẻ dữ liệu";
                                sh.contents = "<b>" + user_send.full_name + "</b> đã chia sẻ tài liệu <b>" + file_info.file_name + "</b> cho bạn ";
                                sh.is_type = 2;
                                sh.date_send = DateTime.Now;
                                sh.id_key = file_id.ToString();
                                sh.token_id = tid;
                                sh.created_date = DateTime.Now;
                                sh.created_by = uid;
                                sh.created_token_id = tid;
                                sh.created_ip = ip;
                                db.sys_sendhub.Add(sh);
                                #region XML
                                string xml_result = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>";
                                xml_result += "<document>";
                                xml_result += "<element>";
                                xml_result += "<user_send>" + (sh.user_send ?? "") + "</user_send>";
                                xml_result += "<title>" + (sh.title ?? "") + "</title>";
                                xml_result += "<contents>" + (sh.contents ?? "") + "</contents>";
                                xml_result += "<date_send>" + (sh.date_send) + "</date_send>";
                                xml_result += "<file_name>" + (file_info.file_name) + "</file_name>";
                                xml_result += "<file_path>" + (file_info.is_filepath) + "</file_path>";
                                xml_result += "</element>";
                                xml_result += "</document>";

                                var user_now = db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                                System.Net.WebClient webc = new System.Net.WebClient();
                                string path = rootXML + helper.path_xml + "/FileMain/";
                                // Format path
                                var listPathEdit_path = Regex.Replace(path.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_path = "";
                                var sttPathEdit_path = 1;
                                foreach (var itemEdit in listPathEdit_path)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        if (sttPathEdit_path == 1)
                                        {
                                            pathEdit_path += itemEdit;
                                        }
                                        else
                                        {
                                            pathEdit_path += "/" + Path.GetFileName(itemEdit);
                                        }
                                    }
                                    sttPathEdit_path++;
                                }
                                path = pathEdit_path;
                                bool exists = Directory.Exists(path);
                                if (!exists)
                                    Directory.CreateDirectory(path);


                                string name_file = file_info.file_name + ".xml";
                                string root_path = path + "/" + name_file;
                                string duong_dan = helper.path_xml + "/FileMain/" + name_file;
                                string url = ConfigurationManager.AppSettings["ValidAudience"] + duong_dan;
                                // Format root_path
                                var listPathEdit_root_path = Regex.Replace(root_path.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_root_path = "";
                                var sttPathEdit_root_path = 1;
                                foreach (var itemEdit in listPathEdit_root_path)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        if (sttPathEdit_root_path == 1)
                                        {
                                            pathEdit_root_path += itemEdit;
                                        }
                                        else
                                        {
                                            pathEdit_root_path += "/" + Path.GetFileName(itemEdit);
                                        }
                                    }
                                    sttPathEdit_root_path++;
                                }
                                root_path = pathEdit_root_path;
                                File.WriteAllText(root_path, xml_result);
                                var res_encr = helper.encryptXML(root_path, "document", helper.psKey);
                                if (res_encr != "OK")
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Không thể mã hoã file XML!", err = "1" });
                                };
                                #endregion
                                #region sendSocket
                                sys_users user_send_noti = db.sys_users.Find(uid);
                                var contentNoti = user_send_noti.full_name + " đã chia sẻ tài liệu cho bạn";
                                var users = listshare.Where(x => x.user_id != uid).Select(x => x.user_id).Distinct().ToList();
                                var message = new Dictionary<string, dynamic>
                                    {
                                        { "event", "sendNotify" },
                                        { "user_id", uid },
                                        { "title", "Công việc" },
                                        { "contents", contentNoti },
                                        { "date", DateTime.Now },
                                        { "uids", users },
                                    };
                                if (helper.socketClient != null && helper.socketClient.Connected == true)
                                {
                                    try
                                    {
                                        helper.socketClient.EmitAsync("sendData", message);
                                    }
                                    catch { };
                                }
                                #endregion
                            }
                        }

                        db.SaveChanges();
                        #region  add Log
                        helper.saveLogFiles(uid, 2, file_id, null, "Chia sẻ file ", ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Share_Folder()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string rootXML = HttpContext.Current.Server.MapPath("~/");
                    //string strPath = root + "/FileFolder";
                    //bool exists = Directory.Exists(strPath);
                    //if (!exists)
                    //    Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        file_folder model = JsonConvert.DeserializeObject<file_folder>(fdmodel);

                        var folder_share = provider.FormData.GetValues("folder_share").SingleOrDefault();
                        List<file_folder_share> listshare = JsonConvert.DeserializeObject<List<file_folder_share>>(folder_share);

                        // This illustrates how to get thefile names.
                        var da = db.file_folder.Find(model.folder_id);
                        if (da != null)
                        {
                            da.type_share = model.type_share;
                        }
                        db.Entry(da).State = EntityState.Modified;

                        //delte all 
                        List<file_folder_share> itemToRemove = db.file_folder_share.Where(a => a.folder_id == model.folder_id).ToList();
                        if (itemToRemove.Count > 0)
                            db.file_folder_share.RemoveRange(itemToRemove);
                        if (listshare.Count > 0)
                        {
                            var count = 1;
                            foreach (var item in listshare)
                            {
                                file_folder_share folder = new file_folder_share();
                                folder = item;
                                folder.folder_id = model.folder_id;
                                folder.is_order = count;
                                folder.created_date = DateTime.Now;
                                folder.created_by = uid;
                                folder.created_token_id = tid;
                                folder.created_ip = ip;
                                count++;
                                db.file_folder_share.Add(folder);
                                // send log
                                var lg = db.sys_sendhub.Where(a => (a.id_key == model.folder_id && a.receiver == uid && a.module_key == const_module_key)).FirstOrDefault<sys_sendhub>();
                                if (lg == null)
                                {
                                    sys_users user_send = db.sys_users.Find(uid);
                                    sys_users user_receiver = db.sys_users.Find(item.user_id);
                                    file_folder file_folder = db.file_folder.Find(model.folder_id);

                                    var sh = new sys_sendhub();
                                    sh.senhub_id = helper.GenKey();
                                    sh.module_key = const_module_key;
                                    sh.date_send = DateTime.Now;
                                    sh.user_send = uid;
                                    sh.receiver = item.user_id;
                                    sh.icon = user_send.avatar;
                                    sh.title = "Chia sẻ dữ liệu";
                                    sh.contents = "<b>" + user_send.full_name + "</b> đã chia sẻ kho dữ liệu <b>" + file_folder.folder_name + "</b> cho bạn ";
                                    sh.is_type = 1;
                                    sh.date_send = DateTime.Now;
                                    sh.id_key = model.folder_id;
                                    sh.token_id = tid;
                                    sh.created_date = DateTime.Now;
                                    sh.created_by = uid;
                                    sh.created_token_id = tid;
                                    sh.created_ip = ip;
                                    db.sys_sendhub.Add(sh);
                                    #region XMl
                                    string xml_result = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>";
                                    xml_result += "<document>";
                                    xml_result += "<element>";
                                    xml_result += "<user_send>" + (sh.user_send ?? "") + "</user_send>";
                                    xml_result += "<title>" + (sh.title ?? "") + "</title>";
                                    xml_result += "<contents>" + (sh.contents ?? "") + "</contents>";
                                    xml_result += "<date_send>" + (sh.date_send) + "</date_send>";
                                    xml_result += "<folder_name>" + (file_folder.folder_name) + "</folder_name>";
                                    xml_result += "</element>";
                                    xml_result += "</document>";

                                    var user_now = db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                                    System.Net.WebClient webc = new System.Net.WebClient();
                                    string path = rootXML + helper.path_xml + "/FileMain/";
                                    bool exists = Directory.Exists(path);
                                    if (!exists)
                                        Directory.CreateDirectory(path);

                                    string name_file = file_folder.folder_name.ToString() + ".xml";
                                    string root_path = path + "/" + name_file;
                                    string duong_dan = helper.path_xml + "/FileMain/" + name_file;
                                    string url = ConfigurationManager.AppSettings["ValidAudience"] + duong_dan;

                                    // Format root_path
                                    var listPathEdit_root_path = Regex.Replace(root_path.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                    var pathEdit_root_path = "";
                                    var sttPathEdit_root_path = 1;
                                    foreach (var itemEdit in listPathEdit_root_path)
                                    {
                                        if (itemEdit.Trim() != "")
                                        {
                                            if (sttPathEdit_root_path == 1)
                                            {
                                                pathEdit_root_path += itemEdit;
                                            }
                                            else
                                            {
                                                pathEdit_root_path += "/" + Path.GetFileName(itemEdit);
                                            }
                                        }
                                        sttPathEdit_root_path++;
                                    }
                                    root_path = pathEdit_root_path;

                                    File.WriteAllText(root_path, xml_result);
                                    var res_encr = helper.encryptXML(root_path, "document", helper.psKey);
                                    if (res_encr != "OK")
                                    {
                                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Không thể mã hoã file XML!", err = "1" });
                                    };
                                    #endregion
                                    #region sendSocket
                                    sys_users user_send_noti = db.sys_users.Find(uid);
                                    var contentNoti = user_send_noti.full_name + " đã chia sẻ kho dữ liệu cho bạn";
                                    var users = listshare.Where(x => x.user_id != uid).Select(x => x.user_id).Distinct().ToList();
                                    var message = new Dictionary<string, dynamic>
                                    {
                                        { "event", "sendNotify" },
                                        { "user_id", uid },
                                        { "title", "Công việc" },
                                        { "contents", contentNoti },
                                        { "date", DateTime.Now },
                                        { "uids", users },
                                    };
                                    if (helper.socketClient != null && helper.socketClient.Connected == true)
                                    {
                                        try
                                        {
                                            helper.socketClient.EmitAsync("sendData", message);
                                        }
                                        catch { };
                                    }
                                    #endregion
                                }
                                else
                                {
                                    lg.date_send = DateTime.Now;
                                    lg.modified_date = DateTime.Now;
                                    lg.modified_by = uid;
                                    db.SaveChanges();
                                }
                            }
                        }

                        db.SaveChanges();
                        #region  add Log
                        helper.saveLogFiles(uid, 2, null, model.folder_id, "Chia sẻ folder " + model.folder_name, ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FileFolder/Add_FileFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FileFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> AddLog([System.Web.Mvc.Bind(Include = "contents,file_id,folder_id,log_type,count_view_file,count_download_file,name, full_name")] file_log log)
        {
            using (DBEntities db = new DBEntities())
            {
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    var lg = db.file_log.Where(a => ((a.file_id == log.file_id && log.folder_id == null) || (a.folder_id == log.folder_id && log.file_id == null)) && (a.user_id == uid) && (a.log_type == log.log_type)).FirstOrDefault<file_log>();
                    if (lg != null)
                    {
                        lg.contents = log.contents;
                        lg.date_view = DateTime.Now;
                        if (lg.log_type == 0)
                            lg.count_view_file += 1;
                        if (lg.log_type == 1)
                            lg.count_download_file += 1;
                        lg.modified_date = DateTime.Now;
                        lg.modified_by = uid;
                        lg.date_string += "," + DateTime.Now.ToString();
                        db.SaveChanges();
                    }
                    else
                    {
                        file_log log_add = new file_log();
                        log_add = log;
                        if (log.log_type == 0)
                            log_add.count_view_file = 1;
                        if (log.log_type == 1)
                            log_add.count_download_file = 1;
                        log_add.user_id = uid;
                        log_add.date_string = DateTime.Now.ToString();
                        log_add.date_view = DateTime.Now;
                        log_add.created_date = DateTime.Now;
                        log_add.created_by = uid;
                        log_add.created_ip = ip;
                        log_add.modified_date = DateTime.Now;
                        log_add.modified_by = uid;


                        db.file_log.Add(log_add);
                        db.SaveChanges();
                        // update view main
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = log, contents }), domainurl + "Proc/AddLog", ip, tid, "Lỗi khi Thêm log Console", 0, "Proc");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = log, contents }), domainurl + "Proc/AddLog", ip, tid, "Lỗi khi Thêm log Console", 0, "Proc");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_config()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlaw = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            int dvid = Int32.Parse(claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value);
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
                    // Provider
                    string rootTemp = HttpContext.Current.Server.MapPath("~/Portals");
                    bool existsTemp = Directory.Exists(rootTemp);
                    if (!existsTemp)
                        Directory.CreateDirectory(rootTemp);
                    var provider = new MultipartFormDataStreamProvider(rootTemp);
                    var task = await Request.Content.ReadAsMultipartAsync(provider);

                    // Params
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    List<file_modules> multiple = JsonConvert.DeserializeObject<List<file_modules>>(provider.FormData.GetValues("user_module").SingleOrDefault());
                    //List<calendar_member> mbs = JsonConvert.DeserializeObject<List<calendar_member>>(provider.FormData.GetValues("members").SingleOrDefault());
                    //List<calendar_attend> dps = JsonConvert.DeserializeObject<List<calendar_attend>>(provider.FormData.GetValues("departments").SingleOrDefault());
                    if (multiple.Count > 0)
                    {
                        // del all
                        List<file_modules> itemToRemove = db.file_modules.Where(a => a.organization_id == dvid).ToList();
                        if (itemToRemove.Count > 0) db.file_modules.RemoveRange(itemToRemove);
                        foreach (var model in multiple)
                        {
                            #region Model
                            model.file_module_id = helper.GenKey();
                            model.organization_id = dvid;
                            model.created_by = uid;
                            model.created_date = DateTime.Now;
                            model.created_ip = ip;
                            model.created_token_id = tid;
                            //db.report_modules.Add(model);

                            #endregion
                            #region nguoi dung

                            if (model.file_module_user.Count > 0)
                            {
                                foreach (var user in model.file_module_user)
                                {
                                    user.file_module_user_id = helper.GenKey();
                                    user.file_module_id = model.file_module_id;
                                    user.created_by = uid;
                                    user.created_date = DateTime.Now;
                                    user.created_ip = ip;
                                    user.created_token_id = tid;
                                }

                            }
                            #endregion
                            #region Phongban
                            else if (model.file_module_organization.Count > 0)
                            {
                                foreach (var dp in model.file_module_organization)
                                {
                                    dp.file_module_organization_id = helper.GenKey();
                                    dp.file_module_id = model.file_module_id;
                                    dp.created_by = uid;
                                    dp.created_date = DateTime.Now;
                                    dp.created_ip = ip;
                                    dp.created_token_id = tid;
                                }
                            }

                            #endregion
                        }
                        db.file_modules.AddRange(multiple);
                    }
                    await db.SaveChangesAsync();
                    //  await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/update_calendar_week_multiple", ip, tid, "Lỗi khi cập nhật lịch họp ", 0, "calendar_week");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/update_calendar_week_multiple", ip, tid, "Lỗi khi cập nhật lịch họp", 0, "calendar_week");
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
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
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
                            log.controller = domainurl + "FileMain/GetDataProc";
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "chat/GetDataProc", ip, tid, "Lỗi khi gọi proc", 0, "chat");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "chat/GetDataProc", ip, tid, "Lỗi khi gọi proc", 0, "chat");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
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
        #endregion
        #region Scan file
        public async Task<HttpResponseMessage> ScanFileUpload()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                string root = HttpContext.Current.Server.MapPath("~/");
                string strPath = root + "/Portals/CheckUpload";
                bool exists = Directory.Exists(strPath);
                if (!exists)
                {
                    Directory.CreateDirectory(strPath);
                }
                var provider = new MultipartFormDataStreamProvider(root + "/Portals/CheckUpload");
                // Read the form data and return an async task.
                var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    List<string> listPathFileUp = new List<string>();
                    bool hasVirus = false;
                    foreach (MultipartFileData fileData in provider.FileData)
                    {
                        var scanner = new AntiVirus.Scanner();
                        var resultScan = scanner.ScanAndClean(fileData.LocalFileName);
                        listPathFileUp.Add(fileData.LocalFileName);
                        if (resultScan.ToString() != "VirusNotFound")
                        {
                            hasVirus = true;
                        }
                    }
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
                    if (hasVirus)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Warning: File exists virus.", err = "1" });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                });
                return await task;
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/ScanFileUpload", ip, tid, "Lỗi khi scan file", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/ScanFileUpload", ip, tid, "Lỗi khi scan file", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

    }
}
