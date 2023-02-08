using AngleSharp.Dom;
using AngleSharp.Html.Parser;
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
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Controllers
{
    [Authorize]
    public class NewsController : ApiController
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
        public async Task<HttpResponseMessage> Add_News()
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
                    if (string.IsNullOrWhiteSpace(PortalConfigs))
                    {
                        PortalConfigs = HttpContext.Current.Server.MapPath("~/");
                    }
                    string root = PortalConfigs + "Portals";
                    string strPath = root + "/News";
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
                        cms_news model = JsonConvert.DeserializeObject<cms_news>(fdmodel);
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
                            newFileName = Path.Combine(root + "/News", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/News", fileName);
                            }
                            Task.Run(() => upload.UpdateFile(jwtcookie, root, fileData, ("/News/" + fileName), 72));
                            model.image = "/Portals/News/" + fileName;
                        }
                        model.is_visitor = 0;
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_ip = ip;
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        db.cms_news.Add(model);
                        #region  add CMSLog
                        helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = model.news_id, contents = "" }), domainurl + "News/Add_News", ip, tid, "Thêm tin tức " + model.news_name, "News");
                        #endregion
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "News/Add_News", ip, tid, "Lỗi khi thêm tin tức", 0, "News");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "News/Add_News", ip, tid, "Lỗi khi thêm tin tức", 0, "News");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_News()
        {
            if (string.IsNullOrWhiteSpace(PortalConfigs))
            {
                PortalConfigs = HttpContext.Current.Server.MapPath("~/");
            }
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string jwtcookie = HttpContext.Current.Request.Cookies["jwt"].Value;
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
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

                    string root = PortalConfigs + "Portals";
                    string strPath = root + "/News";
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
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        cms_news model = JsonConvert.DeserializeObject<cms_news>(fdmodel);
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
                            newFileName = Path.Combine(root + "/News", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/News", fileName);
                            }
                            if (!string.IsNullOrWhiteSpace(model.image))
                                delfiles.Add(root.Replace("Portals", "") + model.image);
                            Task.Run(() => upload.UpdateFile(jwtcookie, root, fileData, ("/News/" + fileName), 72));
                            model.image = "/Portals/News/" + fileName;
                        }
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        db.Entry(model).State = EntityState.Modified;
                        #region  add CMSLog
                        helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = model.news_id, contents = "" }), domainurl + "News/Update_News", ip, tid, "Cập nhật tin tức " + model.news_name, "News");
                        #endregion
                        db.SaveChanges();
                        //Add ảnh
                        if (fileInfo != null)
                        {
                            foreach (string fpath in delfiles)
                            {
                                File.Delete(fpath);
                            }
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "News/Update_News", ip, tid, "Lỗi khi cập nhật tin tức", 0, "News");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "News/Update_News", ip, tid, "Lỗi khi cập nhật tin tức", 0, "News");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Del_News([FromBody] List<int> ids)
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
                        var das = await db.cms_news.Where(a => ids.Contains(a.news_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<cms_news> del = new List<cms_news>();
                            foreach (var da in das)
                            {
                                if (ad)
                                {
                                    del.Add(da);
                                    if (!string.IsNullOrWhiteSpace(da.image))
                                        paths.Add(PortalConfigs + da.image);
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa tin tức này." });
                            }
                            db.cms_news.RemoveRange(del);
                        }
                        #region  add CMSLog
                        helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = ids, contents = "" }), domainurl + "News/Del_News", ip, tid, "Xoá tin tức " + string.Join(",", ids), "News");
                        #endregion
                        await db.SaveChangesAsync();
                        foreach (string strPath in paths)
                        {
                            bool exists = Directory.Exists(strPath);
                            if (exists)
                                System.IO.File.Delete(strPath);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "News/Del_News", ip, tid, "Lỗi khi xoá tin tức", 0, "News");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "News/Del_News", ip, tid, "Lỗi khi xoá tin tức", 0, "News");
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
        public async Task<HttpResponseMessage> Update_statusNews([FromBody] JObject data)
        {
            List<int> ids = data["ids"].ToObject<List<int>>();
            List<int> tts = data["tts"].ToObject<List<int>>();
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
                        var das = await db.cms_news.Where(a => ids.Contains(a.news_id)).ToListAsync();
                        if (das != null)
                        {
                            List<cms_news> del = new List<cms_news>();
                            for (int i = 0; i < das.Count; i++)
                            {
                                var da = das[i];
                                if (ad)
                                {
                                    da.status = tts[i];
                                    da.modified_date = DateTime.Now;
                                    da.modified_by = uid;
                                    da.modified_ip = ip;
                                    da.modified_token_id = tid;
                                    if (da.status != 0)
                                    {
                                        da.date_censor = DateTime.Now;
                                        da.censor = uid;
                                        da.ip_censor = ip;
                                    }
                                }
                            }
                            #region  add CMSLog
                            helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = ids, contents = "" }), domainurl + "News/Update_statusNews", ip, tid, "Update trạng thái tin tức " + string.Join(",", ids), "News");
                            #endregion
                            await db.SaveChangesAsync();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "News/Update_statusNews", ip, tid, "Lỗi khi cập nhật trạng thái tin tức", 0, "News");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "News/Update_statusNews", ip, tid, "Lỗi khi cập nhật trạng thái tin tức", 0, "News");
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
        public async Task<HttpResponseMessage> Update_statusNewsHot([FromBody] JObject data)
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
                        var das = await db.cms_news.Where(a => ids.Contains(a.news_id)).ToListAsync();
                        if (das != null)
                        {
                            List<cms_news> del = new List<cms_news>();
                            for (int i = 0; i < das.Count; i++)
                            {
                                var da = das[i];
                                if (ad)
                                {
                                    da.is_hot = tts[i];
                                }
                            }
                            #region  add CMSLog
                            helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = ids, contents = "" }), domainurl + "News/Update_statusNewsHot", ip, tid, "Update trạng thái nổi bật của tin tức " + string.Join(",", ids), "News");
                            #endregion
                            await db.SaveChangesAsync();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "News/Update_statusNewsHot", ip, tid, "Lỗi khi cập nhật trạng thái nổi bật của tin tức", 0, "News");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "News/Update_statusNewsHot", ip, tid, "Lỗi khi cập nhật trạng thái nổi bật của tin tức", 0, "News");
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

        [HttpPost]
        public async Task<HttpResponseMessage> Update_UsersNews(int news_id)
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
                        var das = await db.cms_news.FindAsync(news_id);
                        if (das != null)
                        {
                            das.is_visitor = (das.is_visitor ?? 0) + 1;
                            cms_news_users nu = await db.cms_news_users.FirstOrDefaultAsync(a => a.news_id == news_id && a.user_id == uid);
                            if (nu != null)
                            {
                                nu = new cms_news_users();
                                nu.news_id = news_id;
                                nu.user_id = uid;
                                nu.ip = ip;
                                nu.views = 1;
                                nu.start_date = DateTime.Now;
                                nu.end_date = DateTime.Now;
                                nu.views = (nu.views ?? 0) + 1;
                                db.cms_news_users.Add(nu);
                            }
                            else
                            {
                                nu.end_date = DateTime.Now;
                                nu.views = (nu.views ?? 0) + 1;
                            }
                            #region  add CMSLog
                            helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { data = news_id, contents = "" }), domainurl + "News/Update_UsersNews", ip, tid, "Update lượt xem tin tức " + news_id, "News");
                            #endregion
                            await db.SaveChangesAsync();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = news_id, contents }), domainurl + "News/Update_UsersNews", ip, tid, "Lỗi khi lượt xem tin tức", 0, "News");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = news_id, contents }), domainurl + "News/Update_UsersNews", ip, tid, "Lỗi khi lượt xem tin tức", 0, "News");
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
        public async Task<HttpResponseMessage> ScraperNews([FromBody] List<cms_news> datas)
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
                string webdomain = datas.FirstOrDefault().relate_id;
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        foreach (var model in datas)
                        {
                            model.relate_id = null;
                            model.is_visitor = 0;
                            model.created_date = DateTime.Now;
                            model.created_by = uid;
                            model.created_ip = ip;
                            model.modified_date = DateTime.Now;
                            model.modified_by = uid;
                            model.modified_ip = ip;
                            model.modified_token_id = tid;
                            model.status = ad ? 1 : 0;
                        }
                        db.cms_news.AddRange(datas);
                        #region  add CMSLog
                        helper.saveCMSLog(uid, JsonConvert.SerializeObject(new { contents = "" }), domainurl + "News/ScraperNews", ip, tid, "Thêm tin tự động", "News");
                        #endregion
                        await db.SaveChangesAsync();
                        List<string> images = new List<string>();
                        using (WebClient client = new WebClient())
                        {
                            string root = PortalConfigs + "Portals";
                            string strPath = root + "/News";
                            string jwtcookie = HttpContext.Current.Request.Cookies["jwt"].Value;
                            string fileName = "";
                            HttpClient httpClient = new HttpClient();
                            var parser = new HtmlParser();
                            foreach (var model in datas)
                            {
                                fileName = model.news_id + ".jpg";
                                var responseResult = httpClient.GetAsync(new Uri(model.image));
                                using (var memStream = responseResult.Result.Content.ReadAsStreamAsync().Result)
                                {
                                    using (var fileStream = File.Create(strPath + "/" + fileName))
                                    {
                                        memStream.CopyTo(fileStream);
                                        images.Add(model.image);
                                    }

                                }
                                helper.ResizeImage(strPath + "/" + fileName, 1920, 1080, 90);
                                helper.ResizeThumbImage(strPath + "/" + fileName, 160, 160);
                                model.image = "/Portals/News/" + fileName;
                                #region Tìm Image
                                var document = await parser.ParseDocumentAsync(model.details);
                                IEnumerable<IElement> image = document.QuerySelectorAll("img").ToList();
                                foreach (var item in image)
                                {
                                    if (item.GetAttribute("src") != null)
                                    {
                                        bool exists = Directory.Exists(strPath + "/" + model.news_id);
                                        if (!exists)
                                            Directory.CreateDirectory(strPath + "/" + model.news_id);
                                        string imgsrc = item.GetAttribute("src");
                                        if (!imgsrc.Contains("http"))
                                        {
                                            imgsrc = webdomain + item.GetAttribute("src");
                                        }
                                        responseResult = httpClient.GetAsync(new Uri(imgsrc));
                                        fileName = "/" + model.news_id + "/" + helper.GenKey() + ".jpg";
                                        try
                                        {
                                            if (responseResult.Result.StatusCode == HttpStatusCode.OK)
                                            {
                                                using (var memStream = responseResult.Result.Content.ReadAsStreamAsync().Result)
                                                {
                                                    using (var fileStream = File.Create(strPath + fileName))
                                                    {
                                                        memStream.CopyTo(fileStream);
                                                        images.Add(item.GetAttribute("src"));
                                                    }

                                                }
                                                helper.ResizeImage(strPath + fileName, 1920, 1080, 90);
                                                item.SetAttribute("src", domainurl + "/Portals/News/" + fileName);
                                            }
                                            else
                                            {
                                                item.Remove();
                                            }
                                        }
                                        catch
                                        {
                                            item.Remove();
                                        }
                                    }
                                }
                                model.details = document.DocumentElement.OuterHtml;
                                #endregion

                            }
                            await db.SaveChangesAsync();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", images });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "News/ScraperNews", ip, tid, "Lỗi khi thêm tin tự động", 0, "News");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "News/ScraperNews", ip, tid, "Lỗi khi thêm tin tự động", 0, "News");
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

        #region Excel
        [HttpPost]
        public async Task<HttpResponseMessage> ImportExcel()
        {
            string ListErr = "";
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/News";
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
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;

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
                                List<cms_news> dvs = new List<cms_news>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                var dvcs = db.cms_news.Select(a => new { a.news_id, a.news_name }).ToList();
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    cms_news dv = new cms_news();
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
                                            PropertyInfo propertyInfo = db.cms_news.GetType().GetProperty(column.ToString());
                                            propertyInfo.SetValue(db.cms_news, Convert.ChangeType(vl,
                                            propertyInfo.PropertyType), null);
                                        }
                                    }
                                    if (dvcs.Count(a => a.news_id == dv.news_id || a.news_name == dv.news_name) > 0)
                                        break;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.cms_news.AddRange(dvs);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "News/ImportExcel", ip, tid, "Lỗi khi import tin tức", 0, "News");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "News/ImportExcel", ip, tid, "Lỗi khi import tin tức", 0, "News");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

    }
}