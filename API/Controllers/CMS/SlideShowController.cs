using API.Helper;
using API.Models;
using Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Controllers
{
    [Authorize]
    public class SlideShowController : ApiController
    {
        Upload upload = new Upload();
        string PortalConfigs = ConfigurationManager.AppSettings["Portals"] ?? "";

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
        public async Task<HttpResponseMessage> EditSlideShowName(cms_slideshow model)
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
                    model.modified_by = uid;
                    model.modified_date = DateTime.Now;
                    model.modified_token_id = tid;
                    model.modified_ip = ip;
                    db.cms_slideshow.Add(model);
                    #region  add Log
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents = "" }), domainurl + "SlideShow/EditSlideShowName", ip, tid, "Thêm mới SlideShow " + model.slideshow_name, 1, "SlideShow");
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "SlideShow/EditSlideShowName", ip, tid, "Lỗi khi thêm SlideShow", 0, "SlideShow");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "SlideShow/EditSlideShowName", ip, tid, "Lỗi khi thêm SlideShow", 0, "SlideShow");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_SlideShow()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            string jwtcookie = HttpContext.Current.Request.Cookies["jwt"].Value;
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
                    string strPath = root + "/SlideShow";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root + "/Temp");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        cms_slideshow model = JsonConvert.DeserializeObject<cms_slideshow>(fdmodel);
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_token_id = tid;
                        model.modified_ip = ip;
                        if (model.slideshow_id == -1)
                        {
                            db.cms_slideshow.Add(model);
                            db.SaveChanges();
                            strPath = root + "/SlideShow/" + model.slideshow_id;
                            Directory.CreateDirectory(strPath);
                        }
                        else
                        {
                            var slid = db.cms_slideshow.Find(model.slideshow_id);
                            slid.modified_date = DateTime.Now;
                            slid.modified_by = uid;
                            slid.modified_token_id = tid;
                            slid.modified_ip = ip;
                            slid.slideshow_name = model.slideshow_name;
                            slid.topic_id = model.topic_id;
                            slid.is_type = model.is_type;
                            slid.keywords = model.keywords;
                            slid.is_order = model.is_order;
                            slid.status = model.status;
                            strPath = root + "/SlideShow/" + model.slideshow_id;
                        }
                        List<cms_slideshow_image> images = new List<cms_slideshow_image>();
                        fdmodel = provider.FormData.GetValues("images").SingleOrDefault();
                        if (!string.IsNullOrWhiteSpace(fdmodel))
                        {
                            images = JsonConvert.DeserializeObject<List<cms_slideshow_image>>(fdmodel);
                        }
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        string newFileName = "";
                        int i = 0;
                        int stt = 0;
                        var simages = db.cms_slideshow_image.Where(a => a.slideshow_id == model.slideshow_id).ToList();
                        stt = simages.Count() + 1;
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
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

                                newFileName = Path.Combine(strPath, fileName);
                            }
                            _ = Task.Run(() => upload.UpdateFile(jwtcookie, root, fileData, ("/SlideShow/" + model.slideshow_id + "/" + fileName), 72));
                            images[i].slideshow_id = model.slideshow_id;
                            images[i].is_filepath = "/Portals/SlideShow/" + model.slideshow_id + "/" + fileName;
                            images[i].modified_date = DateTime.Now;
                            images[i].modified_by = uid;
                            images[i].modified_token_id = tid;
                            images[i].modified_ip = ip;
                            images[i].status = true;
                            images[i].is_order = stt + i;
                            i++;
                        }
                        if (images.Count > 0)
                            db.cms_slideshow_image.AddRange(images);
                        #region  add CMSLog
                        helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = model.slideshow_id, contents = "" }), domainurl + "SlideShow/Add_SlideShow", ip, tid, "Thêm slideshow " + model.slideshow_name, "SlideShow");
                        #endregion
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "SlideShow/Add_SlideShow", ip, tid, "Lỗi khi thêm slideshow", 0, "SlideShow");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "SlideShow/Add_SlideShow", ip, tid, "Lỗi khi thêm slideshow", 0, "SlideShow");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> Del_SlideShow([FromBody] List<int> ids)
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
                if (string.IsNullOrWhiteSpace(PortalConfigs))
                {
                    PortalConfigs = HttpContext.Current.Server.MapPath("~/");
                }
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
                        var das = await db.cms_slideshow.Where(a => ids.Contains(a.slideshow_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<cms_slideshow> del = new List<cms_slideshow>();
                            foreach (var da in das)
                            {
                                if (ad)
                                {
                                    del.Add(da);
                                    paths.Add(PortalConfigs + "/Portals/SlideShow/" + da.slideshow_id);
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa slideshow này." });
                            }
                            db.cms_slideshow.RemoveRange(del);
                        }
                        #region  add CMSLog
                        helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = ids, contents = "" }), domainurl + "SlideShow/Del_SlideShow", ip, tid, "Xoá slideshow " + string.Join(",", ids), "SlideShow");
                        #endregion
                        await db.SaveChangesAsync();
                        foreach (string strPath in paths)
                        {
                            bool exists = Directory.Exists(strPath);
                            if (exists)
                                Directory.Delete(strPath, true);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "SlideShow/Del_SlideShow", ip, tid, "Lỗi khi xoá slideshow", 0, "SlideShow");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "SlideShow/Del_SlideShow", ip, tid, "Lỗi khi xoá slideshow", 0, "SlideShow");
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
        //SlideShow Image
        [HttpPost]
        public async Task<HttpResponseMessage> Update_SlideShowImageAll()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            string jwtcookie = HttpContext.Current.Request.Cookies["jwt"].Value;
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
                    string strPath = root + "/SlideShow";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root + "/Temp");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        cms_slideshow model = JsonConvert.DeserializeObject<cms_slideshow>(fdmodel);
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_token_id = tid;
                        model.modified_ip = ip;
                        strPath = root + "/SlideShow/" + model.slideshow_id;
                        List<cms_slideshow_image> images = new List<cms_slideshow_image>();
                        fdmodel = provider.FormData.GetValues("images").SingleOrDefault();
                        if (!string.IsNullOrWhiteSpace(fdmodel))
                        {
                            images = JsonConvert.DeserializeObject<List<cms_slideshow_image>>(fdmodel);
                        }
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        string newFileName = "";
                        int i = 0;
                        int stt = 0;
                        var simages = db.cms_slideshow_image.Where(a => a.slideshow_id == model.slideshow_id).ToList();
                        stt = simages.Count() + 1;
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
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

                                newFileName = Path.Combine(strPath, fileName);
                            }
                            _ = Task.Run(() => upload.UpdateFile(jwtcookie, root, fileData, ("/SlideShow/" + model.slideshow_id + "/" + fileName), 72));
                            images[i].slideshow_id = model.slideshow_id;
                            images[i].is_filepath = "/Portals/SlideShow/" + model.slideshow_id + "/" + fileName;
                            images[i].modified_date = DateTime.Now;
                            images[i].modified_by = uid;
                            images[i].modified_token_id = tid;
                            images[i].modified_ip = ip;
                            images[i].status = true;
                            images[i].is_order = stt + i;
                            i++;
                        }
                        if (images.Count > 0)
                            db.cms_slideshow_image.AddRange(images);
                        #region  add CMSLog
                        helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = model.slideshow_id, contents = "" }), domainurl + "SlideShow/Update_SlideShowImageAll", ip, tid, "Thêm ảnh cho slideshow " + model.slideshow_name, "SlideShow");
                        #endregion
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "SlideShow/Update_SlideShowImageAll", ip, tid, "Lỗi khi thêm ảnh cho slideshow", 0, "SlideShow");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "SlideShow/Update_SlideShowImageAll", ip, tid, "Lỗi khi thêm ảnh cho slideshow", 0, "SlideShow");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_SlideShowImage()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            string jwtcookie = HttpContext.Current.Request.Cookies["jwt"].Value;
            IEnumerable<Claim> claims = identity.Claims;
            if (string.IsNullOrWhiteSpace(PortalConfigs))
            {
                PortalConfigs = HttpContext.Current.Server.MapPath("~/");
            }
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
                    string strPath = root + "/SlideShow";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root + "/Temp");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        cms_slideshow_image model = JsonConvert.DeserializeObject<cms_slideshow_image>(fdmodel);
                        model.modified_by = uid;
                        model.modified_date = DateTime.Now;
                        model.modified_token_id = tid;
                        model.modified_ip = ip;
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
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
                            newFileName = Path.Combine(root + "/SlideShow/" + model.slideshow_id, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/SlideShow/" + model.slideshow_id, fileName);
                            }
                            Task.Run(() => upload.UpdateFile(jwtcookie, root, fileData, ("/SlideShow/" + model.slideshow_id + "/" + fileName), 72));
                            model.is_filepath = "/Portals/SlideShow/" + model.slideshow_id + "/" + fileName;
                            model.image_name = model.image_name ?? fileName;
                        }
                        if (model.image_id != -1)
                        {
                            var image = db.cms_slideshow_image.Find(model.image_id);
                            if (image.is_filepath != null && image.is_filepath != model.is_filepath)
                            {
                                if (Directory.Exists(PortalConfigs + image.is_filepath))
                                    File.Delete(PortalConfigs + image.is_filepath);
                            }
                            image.is_filepath = model.is_filepath;
                            image.image_name = model.image_name;
                            image.modified_by = uid;
                            image.modified_date = DateTime.Now;
                            image.modified_token_id = tid;
                            image.modified_ip = ip;
                            helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = model.slideshow_id, contents = "" }), domainurl + "SlideShow/Add_SlideShowImage", ip, tid, "Cập nhật ảnh " + model.image_name, "SlideShow");
                        }
                        else
                        {
                            db.cms_slideshow_image.Add(model);
                            #region  add CMSLog
                            helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = model.slideshow_id, contents = "" }), domainurl + "SlideShow/Add_SlideShowImage", ip, tid, "Thêm ảnh " + model.image_name, "SlideShow");
                            #endregion
                        }

                        db.SaveChanges();
                        //Add ảnh
                        if (fileInfo != null)
                        {
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "SlideShow/Add_SlideShowImage", ip, tid, "Lỗi khi thêm ảnh cho SlideShow", 0, "SlideShow");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "SlideShow/Add_SlideShowImage", ip, tid, "Lỗi khi thêm ảnh cho SlideShow", 0, "SlideShow");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Del_SlideShowImage([FromBody] List<int> ids)
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
                if (string.IsNullOrWhiteSpace(PortalConfigs))
                {
                    PortalConfigs = HttpContext.Current.Server.MapPath("~/");
                }
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
                        var das = await db.cms_slideshow_image.Where(a => ids.Contains(a.image_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<cms_slideshow_image> del = new List<cms_slideshow_image>();
                            foreach (var da in das)
                            {
                                if (ad)
                                {
                                    del.Add(da);
                                    paths.Add(PortalConfigs + da.is_filepath);
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa ảnh này." });
                            }
                            db.cms_slideshow_image.RemoveRange(del);
                        }
                        #region  add CMSLog
                        helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = ids, contents = "" }), domainurl + "SlideShow/Del_SlideShowImage", ip, tid, "Xoá ảnh " + string.Join(",", ids), "SlideShow");
                        #endregion
                        await db.SaveChangesAsync();
                        foreach (string strPath in paths)
                        {
                            bool exists = Directory.Exists(strPath);
                            if (exists)
                                File.Delete(strPath);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "SlideShow/Del_SlideShowImage", ip, tid, "Lỗi khi xoá ảnh trong slideshow", 0, "SlideShow");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "SlideShow/Del_SlideShowImage", ip, tid, "Lỗi khi xoá ảnh slideshow", 0, "SlideShow");
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

        [HttpPut]
        public async Task<HttpResponseMessage> Update_statusSlideShow([FromBody] JObject data)
        {
            List<int> ids = data["ids"].ToObject<List<int>>();
            List<bool> tts = data["tts"].ToObject<List<bool>>();
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
                        var das = await db.cms_slideshow.Where(a => ids.Contains(a.slideshow_id)).ToListAsync();
                        if (das != null)
                        {
                            for (int i = 0; i < das.Count; i++)
                            {
                                var da = das[i];
                                if (ad)
                                {
                                    da.status = tts[i];
                                }
                            }
                            #region  add CMSLog
                            helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = ids, contents = "" }), domainurl + "SlideShow/Update_statusSlideShow", ip, tid, "Update trạng thái slideshow " + string.Join(",", ids), "SlideShow");
                            #endregion
                            await db.SaveChangesAsync();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "SlideShow/Update_statusSlideShow", ip, tid, "Lỗi khi cập nhật trạng thái slideshow", 0, "SlideShow");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "SlideShow/Update_statusSlideShow", ip, tid, "Lỗi khi cập nhật trạng thái slideshow", 0, "SlideShow");
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

        [HttpPut]
        public async Task<HttpResponseMessage> Update_statusSlideShowImage([FromBody] JObject data)
        {
            List<int> ids = data["ids"].ToObject<List<int>>();
            List<bool> tts = data["tts"].ToObject<List<bool>>();
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
                        var das = await db.cms_slideshow_image.Where(a => ids.Contains(a.image_id)).ToListAsync();
                        if (das != null)
                        {
                            for (int i = 0; i < das.Count; i++)
                            {
                                var da = das[i];
                                if (ad)
                                {
                                    da.status = tts[i];
                                }
                            }
                            #region  add CMSLog
                            helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = ids, contents = "" }), domainurl + "SlideShow/Update_statusSlideShowImage", ip, tid, "Update trạng thái ảnh slideshow " + string.Join(",", ids), "SlideShow");
                            #endregion
                            await db.SaveChangesAsync();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "SlideShow/Update_statusSlideShowImage", ip, tid, "Lỗi khi cập nhật trạng thái ảnh slideshow", 0, "SlideShow");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "SlideShow/Update_statusSlideShowImage", ip, tid, "Lỗi khi cập nhật trạng thái ảnh slideshow", 0, "SlideShow");
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