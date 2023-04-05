using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static Helper.helper;
using System.Configuration;
namespace Controllers
{
    [Authorize(Roles = "login")]
    public class UsersController : ApiController
    {
        string[] statuss = new string[] { "Khoá", "Kích hoạt", "Đợi xác thực", "Khoá sai Pass quá 5 lần" };
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
        public async Task<HttpResponseMessage> Add_Users()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Users";
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
                        sys_users model = JsonConvert.DeserializeObject<sys_users>(fdmodel);
                        if (db.sys_users.Count(a => a.user_id == model.user_id) > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Đã có tài khoản người dùng này trong hệ thống rồi!", err = "1" });
                        }
                        // This illustrates how to get thefile names.
                        //string depass = Codec.EncryptString(model.is_psword, helper.psKey);
                        string depass = BCrypt.Net.BCrypt.HashPassword(model.is_psword);
                        model.is_psword = depass;
                        model.key_encript = Convert.ToBase64String(Encoding.UTF8.GetBytes(helper.psKey));
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
                            newFileName = Path.Combine(root + "/Users/" + model.user_id, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/Users/" + model.user_id, fileName);
                            }
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            if (fileData.Headers.ContentDisposition.Name.Contains("Anhdaidien"))
                            {
                                model.avatar = "/Portals/Users/" + model.user_id + "/" + fileName;
                            }
                            else if (fileData.Headers.ContentDisposition.Name.Contains("Chuky"))
                            {
                                model.signature = "/Portals/Users/" + model.user_id + "/" + fileName;
                            }
                            else if (fileData.Headers.ContentDisposition.Name.Contains("Kynhay"))
                            {
                                model.flash_signature = "/Portals/Users/" + model.user_id + "/" + fileName;
                            }
                            ffileData = fileData;
                            File.Move(fileData.LocalFileName, newFileName);
                            helper.ResizeImage(newFileName, 1920, 1080, 90);
                        }
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.token_id = tid;
                        model.ip = ip;
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_token_id = tid;
                        model.created_ip = ip;
                        model.wrong_pass_count = 0;
                        model.key_encode_data = helper.GenerateRandomKey();
                        MemoryCacheHelper.Add(model.user_id, model.key_encode_data, DateTimeOffset.UtcNow.AddMinutes(30));
                        // if (!ad)
                        // {
                        //     model.is_admin = false;
                        // }
                        db.sys_users.Add(model);
                        db.SaveChanges();
                        #region  add Log
                        // helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents = "" }), domainurl + "Users/Add_Users", ip, tid, "Thêm mới User " + model.user_id, 1, "Users");
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Users/Add_Users", ip, tid, "Lỗi khi thêm người dùng", 0, "Users");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Users/Add_Users", ip, tid, "Lỗi khi thêm người dùng", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Users()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
            List<string> ltFiles = new List<string>();

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
                    string root_path = HttpContext.Current.Server.MapPath("~/");
                    string strPath = root + "/Users";
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
                        string noidung = provider.FormData.GetValues("noidung") != null ? provider.FormData.GetValues("noidung").SingleOrDefault() : "";
                        sys_users model = JsonConvert.DeserializeObject<sys_users>(fdmodel);
                        var um = db.sys_users.AsNoTracking().FirstOrDefault(x => x.user_id == model.user_id);
                        //string depass = Codec.EncryptString(model.is_psword, helper.psKey);
                        var ischangeps = false;
                        if (um?.is_psword != model.is_psword)
                        {
                            //model.is_psword = depass;
                            model.is_psword = BCrypt.Net.BCrypt.HashPassword(model.is_psword);
                            ischangeps = true;
                        }
                        if (um?.status != model.status && um?.status == 0 && model.status == 1)
                        {
                            model.wrong_pass_count = 0;
                        }
                        #region nội dung thay đổi
                        if (noidung == "")
                        {
                            if (um.full_name != model.full_name)
                            {
                                noidung += "Chỉnh sửa tên tài khoản\n";
                            }
                            if (provider.FileData.Count > 0)
                            {
                                noidung += "Chỉnh sửa ảnh tài khoản\n";
                            }
                            if (um.role_id != model.role_id)
                            {
                                noidung += "Chỉnh sửa nhóm tài khoản\n";
                            }
                            //if (um?.is_psword != depass)
                            if (ischangeps)
                            {
                                noidung += "Chỉnh sửa mật khẩu\n";
                            }
                        }
                        #endregion
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        // This illustrates how to get thefile names.
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
                            newFileName = Path.Combine(root + "/Users/" + model.user_id, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/Users/" + model.user_id, fileName);
                            }
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            if (fileData.Headers.ContentDisposition.Name.Contains("Anhdaidien"))
                            {
                                bool exists = System.IO.File.Exists(root_path + model.avatar);
                                if (exists)
                                    System.IO.File.Delete(root_path + model.avatar);// xoa file cu
                                model.avatar = "/Portals/Users/" + model.user_id + "/" + fileName;
                            }
                            else if (fileData.Headers.ContentDisposition.Name.Contains("Chuky"))
                            {
                                bool exists = System.IO.File.Exists(root_path + model.signature);
                                if (exists)
                                    System.IO.File.Delete(root_path + model.signature);// xoa file cu
                                model.signature = "/Portals/Users/" + model.user_id + "/" + fileName;
                            }
                            else if (fileData.Headers.ContentDisposition.Name.Contains("Kynhay"))
                            {
                                bool exists = System.IO.File.Exists(root_path + model.flash_signature);
                                if (exists)
                                    System.IO.File.Delete(root_path + model.flash_signature);// xoa file cu
                                model.flash_signature = "/Portals/Users/" + model.user_id + "/" + fileName;
                            }
                            ffileData = fileData;
                            File.Move(fileData.LocalFileName, newFileName);
                            helper.ResizeImage(newFileName, 1920, 1080, 90);
                        }
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.token_id = tid;
                        model.ip = ip;
                        db.Entry(model).State = EntityState.Modified;
                        #region  add Log
                        // helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents = noidung }), domainurl + "Users/Update_Users", ip, tid, "Cập nhật User " + model.user_id, 1, "Users");
                        #endregion
                        try
                        {
                            db.SaveChanges();
                            //Add ảnh
                            foreach (string fpath in delfiles)
                            {
                                if (File.Exists(fpath))
                                    File.Delete(fpath);
                            }
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                        }
                        catch (DbEntityValidationException e)
                        {
                            foreach (string fpath in ltFiles)
                            {
                                if (File.Exists(fpath))
                                    File.Delete(fpath);
                            }
                            string contents = helper.getCatchError(e, null);
                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Users/Update_Users", ip, tid, "Lỗi khi cập nhật người dùng", 0, "Users");
                            if (!helper.debug)
                            {
                                contents = "";
                            }
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                        }
                        catch (Exception e)
                        {
                            foreach (string fpath in ltFiles)
                            {
                                if (File.Exists(fpath))
                                    File.Delete(fpath);
                            }
                            string contents = helper.ExceptionMessage(e);
                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Users/Update_Users", ip, tid, "Lỗi khi cập nhật người dùng", 0, "Users");
                            if (!helper.debug)
                            {
                                contents = "";
                            }
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                        }
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Users/Update_Users", ip, tid, "Lỗi khi cập nhật người dùng", 0, "Users");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Users/Update_Users", ip, tid, "Lỗi khi cập nhật người dùng", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> Del_Users([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                    var das = await db.sys_users.Where(a => ids.Contains(a.user_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<sys_users> del = new List<sys_users>();
                        foreach (var da in das)
                        {
                                del.Add(da);
                                #region  add Log
                                //helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "Users/Del_Users", ip, tid, "Xoá User " + da.user_id, 1, "Users");
                                #endregion
                                if (!string.IsNullOrWhiteSpace("/Portals/Users/" + da.user_id))
                                    paths.Add(HttpContext.Current.Server.MapPath("~/") + "/Portals/Users/" + da.user_id);
                                //if (!string.IsNullOrWhiteSpace(da.signature))
                                //    paths.Add(HttpContext.Current.Server.MapPath("~/") + da.signature);
                                //if (!string.IsNullOrWhiteSpace(da.flash_signature))
                                //    paths.Add(HttpContext.Current.Server.MapPath("~/") + da.flash_signature);
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa người dùng này." });
                        }
                        db.sys_users.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();

                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/Portals/Users")))
                    {
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Portals/Users"));
                    }
                    foreach (string strPath in paths)
                    {
                        var delPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Portals/Users"), Path.GetFileName(strPath.TrimEnd('/')));
                        bool exists = System.IO.Directory.Exists(delPath);
                        if (exists)
                            System.IO.Directory.Delete(delPath, true);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Users/Del_Users", ip, tid, "Lỗi khi xoá người dùng", 0, "Users");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Users/Del_Users", ip, tid, "Lỗi khi xoá người dùng", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }


        [HttpDelete]
        public async Task<HttpResponseMessage> Del_Roles_User()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        string user_id = "";
                        user_id = provider.FormData.GetValues("user_id").SingleOrDefault();
                        List<sys_role_modules> das = db.sys_role_modules.Where(a => a.user_id == user_id).ToList();
                        if (das.Count > 0) db.sys_role_modules.RemoveRange(das);
                        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "Users/Del_Roles_User", ip, tid, "Lỗi khi cập nhật trạng thái người dùng", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "Users/Del_Roles_User", ip, tid, "Lỗi khi cập nhật trạng thái người dùng", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_statusUsers([System.Web.Mvc.Bind(Include = "ids,tts")][FromBody] JObject data)
        {
            List<string> ids = data["ids"].ToObject<List<string>>();
            List<int> tts = data["tts"].ToObject<List<int>>();
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
                    var das = await db.sys_users.Where(a => ids.Contains(a.user_id)).ToListAsync();
                    if (das != null)
                    {
                        List<sys_users> del = new List<sys_users>();
                        for (int i = 0; i < das.Count; i++)
                        {
                            var da = das[i];
                            if (da.modified_by == uid || ad)
                            {
                                #region  add Log
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "Users/Update_statusUsers", ip, tid, "Cập nhật trạng thái người dùng <b>" + da.user_id + "</b> từ <i>" + statuss[da.status] + "</i> thành <i>" + statuss[tts[i]] + "</i>", 1, "Users");
                                #endregion
                                da.status = tts[i];
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Users/Update_statusUsers", ip, tid, "Lỗi khi cập nhật trạng thái người dùng", 0, "Users");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Users/Update_statusUsers", ip, tid, "Lỗi khi cập nhật trạng thái người dùng", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }
        [HttpPut]
        public async Task<HttpResponseMessage> Refresh_Key()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            var fdmodel = "";
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

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
                        var md = provider.FormData.GetValues("model").SingleOrDefault();
                        sys_users user = JsonConvert.DeserializeObject<sys_users>(md);

                        var model = db.sys_users.FirstOrDefault(a => a.user_id == user.user_id);
                        if (model != null) model.key_encode_data = helper.GenerateRandomKey();
                        // This illustrates how to get thefile names.
                        db.Entry(model).State = EntityState.Modified;
                        db.SaveChanges();

                        MemoryCacheHelper.Set(model.user_id, model.key_encode_data, DateTimeOffset.UtcNow.AddMinutes(30));
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "User/Refrehkey", ip, tid, "Lỗi khi User/Refrehkey", 0, "User");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "User/Refrehkey", ip, tid, "Lỗi khi User/Refrehkey", 0, "User");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        #region Excel
        [HttpPost]
        public async Task<HttpResponseMessage> ImportExcel()
        {
            string ListErr = "";
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Users";
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
                            if (!fileName.ToLower().Contains(".xls"))
                            {
                                ListErr = "File Excel không đúng định dạng! Kiểm tra lại mẫu Import";
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = ListErr });
                            }
                            var newFileName = Path.Combine(root + "/Import", fileName);
                            var fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                var extensionFile = fileInfo.Extension.ToLower() == ".xls" ? ".xls" : ".xlsx";
                                fileName = fileName.Replace(extensionFile, "");
                                fileName = fileName + helper.ranNumberFile() + extensionFile;

                                newFileName = Path.Combine(root + "/Import", fileName);
                            }
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            File.Move(fileData.LocalFileName, newFileName);
                            FileInfo temp = new FileInfo(newFileName);
                            using (ExcelPackage pck = new ExcelPackage(temp))
                            {
                                List<sys_users> dvs = new List<sys_users>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                var dvcs = db.sys_users.Select(a => new { a.user_id, a.full_name }).ToList();
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    sys_users dv = new sys_users();
                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value == null)
                                        {
                                            break;
                                        }
                                        var column = ws.Cells[3, j].Value;
                                        var vl = ws.Cells[i, j].Value;
                                        if (column != null && vl != null)
                                        {
                                            PropertyInfo propertyInfo = db.sys_users.GetType().GetProperty(column.ToString());
                                            propertyInfo.SetValue(db.sys_users, Convert.ChangeType(vl,
                                            propertyInfo.PropertyType), null);
                                        }
                                    }
                                    if (dvcs.Count(a => a.user_id == dv.user_id || a.full_name == dv.full_name) > 0)
                                        break;
                                    dv.ip = ip;
                                    dv.modified_by = uid;
                                    dv.modified_date = DateTime.Now;
                                    dv.token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.sys_users.AddRange(dvs);
                                    db.SaveChangesAsync();
                                    File.Delete(newFileName);
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Users/ImportExcel", ip, tid, "Lỗi khi import người dùng", 0, "Users");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Users/ImportExcel", ip, tid, "Lỗi khi import người dùng", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Get_InfoUser(sys_users user)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;

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
                    var id = await db.sys_users.FindAsync(user.user_id);

                    var pass = db.sys_users.FirstOrDefault(e => e.user_id == user.user_id).is_psword;
                    var depass = Codec.DecryptString(pass, helper.psKey);
                    var res = JsonConvert.SerializeObject(new { error = 0, data = depass });
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = res });
                }
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
        #region CallProc
        [HttpPost]
        public async Task<HttpResponseMessage> GetDataProc([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
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
                        log.controller = domainurl + "User/GetDataProc";
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
                (string JSONresult_code, string data_Key) = helper.checkEncodeData(JSONresult, uid, ConfigurationManager.AppSettings["EncriptKey"]);
                //string data_Key = Codec.EncryptString((helper.isEncodeProc ? "1111" : "0000") + "26$#" + key_code, ConfigurationManager.AppSettings["EncriptKey"]);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult_code, err = "0", dataKey = data_Key });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Users/GetDataProc", ip, tid, "Lỗi khi gọi proc", 0, "Users");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Users/GetDataProc", ip, tid, "Lỗi khi gọi proc", 0, "Users");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }
        #endregion

        #endregion

        [HttpPost]
        public async Task<HttpResponseMessage> EncryptAllUser()
        {
            using (DBEntities db = new DBEntities())
            {
                var identity = User.Identity as ClaimsIdentity;
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
                    var listUsers = db.sys_users.Where(x => x.is_psword != null && x.is_psword.Contains("==")).ToList();
                    if (listUsers.Count > 0)
                    {
                        foreach (var user in listUsers)
                        {
                            var deps = Codec.DecryptString(user.is_psword, helper.psKey);
                            user.is_psword = BCrypt.Net.BCrypt.HashPassword(deps);
                        }
                        await db.SaveChangesAsync();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tên đăng nhập hoặc mật khẩu không đúng!", err = "1" });
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Login/EncryptAllUser", ip, tid, "Lỗi khi encrypt ps", 0, "Login");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Login/EncryptAllUser", ip, tid, "Lỗi khi  encrypt ps", 0, "Login");
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
}