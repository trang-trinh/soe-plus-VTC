using API.Models;
using API.Helper;
using Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class AccountController : ApiController
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

        [HttpPut]
        public async Task<HttpResponseMessage> Update_InfoUser([System.Web.Mvc.Bind(Include = "user_id,phone,email,birthday,gender,full_name")] sys_users model)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
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
                using (DBEntities db = new DBEntities())
                {
                    var userChange = db.sys_users.Find(model.user_id);
                    if (userChange != null)
                    {
                        userChange.phone = model.phone;
                        userChange.email = model.email;
                        userChange.birthday = model.birthday;
                        userChange.gender = model.gender;
                        userChange.modified_by = uid;
                        userChange.modified_date = DateTime.Now;
                        userChange.modified_ip = ip;
                        userChange.modified_token_id = tid;
                        await db.SaveChangesAsync();

                        #region add cms_logs
                        if (helper.wlog)
                        {
                            cms_logs log = new cms_logs();
                            log.log_title = "Sửa thông tin người dùng " + model.full_name;
                            log.log_content = JsonConvert.SerializeObject(new { data = model });
                            log.log_module = "Thiết lập thông tin tài khoản cá nhân";
                            log.id_key = model.user_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.cms_logs.Add(log);
                            db.SaveChanges();
                        }
                        #endregion
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "Account/Update_InfoUser", ip, tid, "Lỗi khi cập nhật thông tin tài khoản cá nhân", 0, "Account");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "Account/Update_InfoUser", ip, tid, "Lỗi khi cập nhật thông tin tài khoản cá nhân", 0, "Account");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_Avatar()
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
                    string avatar_old = "";
                    string avatar_new = "";
                    string user_id_change = "";
                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        user_id_change = provider.FormData.GetValues("user_id_change_avt").SingleOrDefault();
                        if (user_id_change != null && user_id_change != "" && user_id_change == uid)
                        {
                            var user_change = db.sys_users.Find(user_id_change);
                            avatar_old = user_change.avatar;
                            //string strPath = root + "/Portals/" + organization_id_user + "/Users";
                            string strPath = "/Portals/" + organization_id_user + "/Users";
                            var fileNameTemp = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = fileNameTemp.Split('/');
                            var pathConfig = "";
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    pathConfig += "/" + Path.GetFileName(item);
                                }
                            }
                            strPath = pathConfig;
                            bool exists = Directory.Exists(root + strPath);
                            if (!exists)
                                Directory.CreateDirectory(root + strPath);

                            // This illustrates how to get thefile names.
                            FileInfo fileInfo = null;
                            MultipartFileData ffileData = null;
                            string newFileName = "";
                            List<string> listPathFileUp = new List<string>();
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
                                //newFileName = Path.Combine(root + "/Portals/" + organization_id_user + "/Users", fileName);
                                newFileName = Path.Combine("/Portals/" + organization_id_user + "/Users", fileName);
                                var fileNameTemp_1 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/");
                                var listPathTemp_1 = fileNameTemp_1.Split('/');
                                var pathConfigTemp_1 = "";
                                foreach (var item in listPathTemp_1)
                                {
                                    if (item.Trim() != "")
                                    {
                                        pathConfigTemp_1 += "/" + Path.GetFileName(item);
                                    }
                                }
                                newFileName = pathConfigTemp_1;
                                fileInfo = new FileInfo(root + newFileName);
                                if (fileInfo.Exists)
                                {
                                    fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                    fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                    // Convert to unsign
                                    Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                    fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                    newFileName = Path.Combine("/Portals/" + organization_id_user + "/Users", fileName);
                                    var fileNameTemp_2 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/");
                                    var listPathTemp_2 = fileNameTemp_2.Split('/');
                                    var pathConfigTemp_2 = "";
                                    foreach (var item in listPathTemp_2)
                                    {
                                        if (item.Trim() != "")
                                        {
                                            pathConfigTemp_2 += "/" + Path.GetFileName(item);
                                        }
                                    }
                                    newFileName = pathConfigTemp_2;
                                }

                                user_change.avatar = "/Portals/" + organization_id_user + "/Users/" + fileName;
                                avatar_new = user_change.avatar;
                                ffileData = fileData;
                                //Add file
                                if (fileInfo != null)
                                {
                                    var strDirectory = "/Portals/" + organization_id_user + "/ConfigTivi/";
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
                                    File.Move(ffileData.LocalFileName, root + pathEdit_1);
                                    //File.Move(ffileData.LocalFileName, newFileName);
                                    helper.ResizeImage(root + pathEdit_1, 1920, 1080, 90);
                                    //helper.ResizeImage(newFileName, 1920, 1080, 90);
                                    listPathFileUp.Add(ffileData.LocalFileName);
                                }
                            }

                            #region add cms_logs
                            if (helper.wlog)
                            {
                                cms_logs log = new cms_logs();
                                log.log_title = "Sửa avatar người dùng " + user_change.full_name;
                                log.log_content = "Thay đổi avatar người dùng " + user_change.full_name;
                                log.log_module = "Thiết lập thông tin tài khoản cá nhân";
                                log.id_key = user_change.user_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.cms_logs.Add(log);
                            }
                            #endregion

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
                            if (avatar_old != null && avatar_old.Contains("/Portals/"))
                            {
                                var listPathEdit_1 = Regex.Replace(avatar_old.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_1 = "";
                                foreach (var itemEdit in listPathEdit_1)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                var avatar_edit = pathEdit_1;
                                if (System.IO.File.Exists(root + avatar_edit))
                                {
                                    System.IO.File.Delete(root + avatar_edit);
                                }
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Người dùng không tồn tại." });
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", urlAvatar = avatar_new });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Account/Update_Avatar", ip, tid, "Lỗi khi cập nhật avatar người dùng", 0, "Account");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Account/Update_Avatar", ip, tid, "Lỗi khi cập nhật avatar người dùng", 0, "Account");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Delete_Avatar()
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
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    //var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    //var organization_id_user = "other";
                    //if (user_now != null && user_now.organization_id != null)
                    //{
                    //    organization_id_user = user_now.organization_id.ToString();
                    //}
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    string avatar_old = "";
                    string user_id_change = "";
                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        user_id_change = provider.FormData.GetValues("user_id_change_avt").SingleOrDefault();
                        if (user_id_change != null && user_id_change != "" && user_id_change == uid)
                        {
                            var user_change = db.sys_users.Find(user_id_change);
                            avatar_old = user_change.avatar;
                            user_change.avatar = null;
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                cms_logs log = new cms_logs();
                                log.log_title = "Xóa avatar người dùng " + user_change.full_name;
                                log.log_content = "Xóa avatar người dùng " + user_change.full_name;
                                log.log_module = "Thiết lập thông tin tài khoản cá nhân";
                                log.id_key = user_change.user_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.cms_logs.Add(log);
                            }
                            #endregion

                            if (avatar_old != null && avatar_old.Contains("/Portals/"))
                            {
                                var listPathEdit_1 = Regex.Replace(avatar_old.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_1 = "";
                                foreach (var itemEdit in listPathEdit_1)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                var avatar_edit = pathEdit_1;
                                if (System.IO.File.Exists(root + avatar_edit))
                                {
                                    System.IO.File.Delete(root + avatar_edit);
                                }
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Người dùng không tồn tại." });
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Account/Delete_Avatar", ip, tid, "Lỗi khi xóa avatar người dùng", 0, "Account");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Account/Delete_Avatar", ip, tid, "Lỗi khi xóa avatar người dùng", 0, "Account");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Change_PassUser([System.Web.Mvc.Bind(Include = "old_pass,new_pass,user_id")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
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
                using (DBEntities db = new DBEntities())
                {
                    string old_pass = data["old_pass"].ToObject<string>();
                    string new_pass = data["new_pass"].ToObject<string>();
                    string user_id = data["user_id"].ToObject<string>();
                    string des = Codec.DecryptString(old_pass, helper.psKey);
                    string desNewPass = Codec.DecryptString(new_pass, helper.psKey);

                    //string dop = helper.Encrypt("os", Codec.DecryptString(old_pass, helper.passkey));
                    string dop = Codec.EncryptString(des, helper.psKey);
                    var ou = db.sys_users.FirstOrDefault(x => x.user_id == user_id);
                    if (ou != null)
                    {
                        if (ou.is_psword != dop)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", op = 0 });
                        }
                        else
                        {
                            ou.is_psword = Codec.EncryptString(desNewPass, helper.psKey); //helper.Encrypt("os", u.matKhau);
                        }

                        #region add cms_logs
                        if (helper.wlog)
                        {
                            cms_logs log = new cms_logs();
                            log.log_title = "Cập nhật mật khẩu người dùng " + ou.full_name;
                            log.log_content = "Cập nhật mật khẩu";
                            log.log_module = "Thiết lập thông tin tài khoản cá nhân";
                            log.id_key = ou.user_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.cms_logs.Add(log);
                        }
                        db.SaveChanges();
                        #endregion

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "Account/Change_PassUser", ip, tid, "Lỗi khi cập nhật thông tin tài khoản cá nhân", 0, "Account");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "Account/Change_PassUser", ip, tid, "Lỗi khi cập nhật thông tin tài khoản cá nhân", 0, "Account");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        // end
    }
}
