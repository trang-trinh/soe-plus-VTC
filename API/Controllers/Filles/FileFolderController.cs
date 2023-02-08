using API.Models;
using Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
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

namespace API.Controllers.Filles
{
    public class FileFolderController : ApiController
    {
        public string getipaddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "localhost";
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_Folder()
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
                    //string strPath = root + "/FildeFolder";
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
                        //var md = provider.FormData.GetValues("model").SingleOrDefault();
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        var folder_share = provider.FormData.GetValues("folder_share").SingleOrDefault();

                        file_folder model = JsonConvert.DeserializeObject<file_folder>(fdmodel);
                        List<file_folder_share> listshare = JsonConvert.DeserializeObject<List<file_folder_share>>(folder_share);


                        // This illustrates how to get thefile names.
                        model.file_folder_share = listshare;
                        model.folder_id = helper.GenKey();
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        model.created_token_id = tid;
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_token_id = tid;
                        model.created_ip = ip;

                        db.file_folder.Add(model);
                        if (listshare.Count > 0)
                        {
                            var count = 1;
                            foreach (var item in listshare)
                            {
                                file_folder_share folder = new file_folder_share();
                                folder = item;
                                folder.folder_id = model.folder_id;
                                folder.is_order = count;
                                count++;
                                db.file_folder_share.Add(folder);
                            }
                        }

                        db.SaveChanges();
                        #region  add Log
                        helper.saveLogFiles(uid, 5, null, model.folder_id, "Thêm folder " + model.folder_name, ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = model.folder_id });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Folder()
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
                    //string strPath = root + "/FildeFolder";
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
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        db.Entry(model).State = EntityState.Modified;

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
                                count++;
                                db.file_folder_share.Add(folder);
                            }
                        }
                        // del dis_share
                        List<file_folder_share_user> item_share = db.file_folder_share_user.Where(a => a.folder_id == model.folder_id).ToList();
                        if (item_share.Count > 0) db.file_folder_share_user.RemoveRange(item_share);

                        db.SaveChanges();
                        #region  add Log
                        helper.saveLogFiles(uid, 3, null, model.folder_id, "Sửa folder " + model.folder_name, ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> Del_Folder([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
                        var das = await db.file_folder.Where(a => ids.Contains(a.folder_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das.Count > 0)
                        {
                            List<file_folder> del = new List<file_folder>();
                            foreach (var da in das)
                            {
                                //xoa file
                                var files = await db.file_info.Where(a => a.folder_id == da.folder_id).ToListAsync();
                                if (files.Count > 0)
                                {
                                    foreach (var f in files)
                                    {
                                        if (!string.IsNullOrWhiteSpace(f.is_filepath) && f.id_key == null)
                                            paths.Add(HttpContext.Current.Server.MapPath("~/") + f.is_filepath);
                                    }
                                }
                                del.Add(da);
                                #region  add Log
                                helper.saveLogFiles(uid, 4, null, da.folder_id, "Xóa folder " + da.folder_name, ip, tid);
                                #endregion
                            }
                            foreach (string strPath in paths)
                            {
                                bool exists = System.IO.File.Exists(strPath);
                                if (exists)
                                    System.IO.File.Delete(strPath);
                            }
                            db.file_folder.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FileFolder/Del_FildeFolder", ip, tid, "Lỗi khi xoá người dùng", 0, "FildeFolder");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FildeFolder/Del_FildeFolder", ip, tid, "Lỗi khi xoá người dùng", 0, "FildeFolder");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Del_SharePublic([System.Web.Mvc.Bind(Include = "user_id,id")] JObject data)
        {
            string user_id = data["user_id"].ToObject<string>();
            var ids = data["id"].ToObject<List<string>>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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

                        if (ids.Count > 0)
                        {
                            foreach (var id in ids)
                            {
                                file_folder_share_user folder = new file_folder_share_user();
                                folder.folder_id = id;
                                folder.user_id = user_id;
                                folder.created_date = DateTime.Now;
                                folder.created_by = uid;
                                folder.created_ip = ip;
                                folder.created_token_id = tid;
                                db.file_folder_share_user.Add(folder);
                            }
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FileFoler/Del_SharePublic", ip, tid, "Lỗi khi xoá file", 0, "FileMain");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FileFoler/Del_SharePublic", ip, tid, "Lỗi khi xoá file", 0, "FileMain");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
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