using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
    public class ProjectMainController : ApiController
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
        public async Task<HttpResponseMessage> Add_ProjectMain()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdprojectmain = "";
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

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/ProjectMain";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var projectmain = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdprojectmain = provider.FormData.GetValues("ProjectMain").SingleOrDefault();
                        project_main projectmain = JsonConvert.DeserializeObject<project_main>(fdprojectmain);

                        projectmain.project_id = helper.GenKey();
                        projectmain.created_date = DateTime.Now;
                        projectmain.created_by = uid;
                        projectmain.modified_by = uid;
                        projectmain.modified_date = DateTime.Now;
                        projectmain.created_ip = ip;
                        projectmain.created_token_id = tid;
                        db.project_main.Add(projectmain);

                        var file = provider.FileData;

                        if (file.Count > 0)
                        {
                            #region file
                            string path = root + "/" + projectmain.organization_id + "/ProjectMain/" + projectmain.project_id;

                            // Format path
                            var pathFormatRoot = Regex.Replace(path.Replace("\\", "/"), @"\.*/+", "/");
                            var listPathRoot = pathFormatRoot.Split('/');
                            var pathConfigRoot = "";
                            var sttPartPath = 1;
                            foreach (var item in listPathRoot)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfigRoot += (item);
                                    }
                                    else
                                    {
                                        pathConfigRoot += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            bool exists = Directory.Exists(pathConfigRoot);
                            if (!exists)
                                Directory.CreateDirectory(pathConfigRoot);

                            List<task_file> dfs = new List<task_file>();
                            foreach (MultipartFileData fileData in provider.FileData)
                            {
                                string org_name_file = fileData.Headers.ContentDisposition.FileName;
                                if (org_name_file.StartsWith("\"") && org_name_file.EndsWith("\""))
                                {
                                    org_name_file = org_name_file.Trim('"');
                                }
                                if (org_name_file.Contains(@"/") || org_name_file.Contains(@"\"))
                                {
                                    org_name_file = System.IO.Path.GetFileName(org_name_file);
                                }
                                string name_file = helper.UniqueFileName(org_name_file);
                                string rootPath = pathConfigRoot + "/" + name_file;
                                string Duongdan = "/Portals/" + projectmain.organization_id + "/ProjectMain/" + projectmain.project_id + "/" + name_file;
                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                                if (rootPath.Length > 260)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 260 - (pathConfigRoot.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                File.Move(fileData.LocalFileName, rootPath);
                                projectmain.logo = Duongdan;
                            }
                            #endregion
                        }
                        #region add task_logs
                        if (helper.wlog)
                        {
                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = null;
                            log.project_id = projectmain.project_id;
                            log.description = "Thêm dự án: " + projectmain.project_name;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.is_type = 0;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();
                        }
                        #endregion
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await projectmain;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ProjectMain/Add_ProjectMain", ip, tid, "Lỗi khi thêm dự án", 0, "ProjectMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ProjectMain/Add_ProjectMain", ip, tid, "Lỗi khi thêm dự án", 0, "ProjectMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_ProjectMain()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdprojectmain = "";
            string fdLogoDonvi = "";
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

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string filepath = HttpContext.Current.Server.MapPath("~/");
                    string strPath = root + "/ProjectMain";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var projectmain = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdprojectmain = provider.FormData.GetValues("ProjectMain").SingleOrDefault();
                        fdLogoDonvi = provider.FormData.GetValues("LogoDonvi").SingleOrDefault();
                        if (fdLogoDonvi.StartsWith("\"") && fdLogoDonvi.EndsWith("\""))
                        {
                            fdLogoDonvi = fdLogoDonvi.Trim('"');
                        }
                        if (fdLogoDonvi.Contains(@"/") || fdLogoDonvi.Contains(@"\"))
                        {
                            fdLogoDonvi = System.IO.Path.GetFileName(fdLogoDonvi);
                        }
                        project_main project_main = JsonConvert.DeserializeObject<project_main>(fdprojectmain);

                        project_main.modified_date = DateTime.Now;
                        project_main.modified_by = uid;
                        project_main.modified_date = DateTime.Now;
                        project_main.modified_ip = ip;
                        project_main.modified_token_id = tid;

                        var file = provider.FileData;

                        if (file.Count > 0)
                        {
                            #region file
                            string path = root + "/" + project_main.organization_id + "/ProjectMain/" + project_main.project_id;

                            // Format path
                            var pathFormatRoot = Regex.Replace(path.Replace("\\", "/"), @"\.*/+", "/");
                            var listPathRoot = pathFormatRoot.Split('/');
                            var pathConfigRoot = "";
                            var sttPartPath = 1;
                            foreach (var item in listPathRoot)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfigRoot += (item);
                                    }
                                    else
                                    {
                                        pathConfigRoot += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            bool exists = Directory.Exists(pathConfigRoot);
                            if (!exists)
                                Directory.CreateDirectory(pathConfigRoot);
                            List<task_file> dfs = new List<task_file>();
                            foreach (MultipartFileData fileData in provider.FileData)
                            {
                                string org_name_file = fileData.Headers.ContentDisposition.FileName;
                                if (org_name_file.StartsWith("\"") && org_name_file.EndsWith("\""))
                                {
                                    org_name_file = org_name_file.Trim('"');
                                }
                                if (org_name_file.Contains(@"/") || org_name_file.Contains(@"\"))
                                {
                                    org_name_file = System.IO.Path.GetFileName(org_name_file);
                                }
                                string name_file = helper.UniqueFileName(org_name_file);
                                string rootPath = pathConfigRoot + "/" + name_file;
                                string Duongdan = "/Portals/" + project_main.organization_id + "/ProjectMain/" + project_main.project_id + "/" + name_file;
                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                                if (rootPath.Length > 260)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 260 - (pathConfigRoot.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                File.Move(fileData.LocalFileName, rootPath);
                                if (org_name_file == fdLogoDonvi)
                                {
                                    var data = db.project_main.AsNoTracking().Where(x => x.project_id == project_main.project_id).ToList();
                                    foreach (var item in data)
                                    {
                                        var logo = filepath + item.logo;
                                        // Format logo
                                        var listPathEdit_logo = Regex.Replace(logo.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                        var pathEdit_logo = "";
                                        var sttPathEdit_logo = 1;
                                        foreach (var itemEdit in listPathEdit_logo)
                                        {
                                            if (itemEdit.Trim() != "")
                                            {
                                                if (sttPathEdit_logo == 1)
                                                {
                                                    pathEdit_logo += itemEdit;
                                                }
                                                else
                                                {
                                                    pathEdit_logo += "/" + Path.GetFileName(itemEdit);
                                                }
                                            }
                                            sttPathEdit_logo++;
                                        }
                                        logo = pathEdit_logo;

                                        if (File.Exists(logo))
                                        {
                                            File.Delete(logo);
                                        }
                                        //File.Move(fileData.LocalFileName, rootPath);
                                    }
                                    project_main.logo = Duongdan;
                                }
                            }
                            #endregion
                        }
                        if (project_main.logo == null)
                        {
                            var data = db.project_main.AsNoTracking().Where(x => x.project_id == project_main.project_id).ToList();
                            foreach (var item in data)
                            {
                                var logo = filepath + item.logo;
                                // Format logo
                                var listPathEdit_logo = Regex.Replace(logo.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_logo = "";
                                var sttPathEdit_logo = 1;
                                foreach (var itemEdit in listPathEdit_logo)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        if (sttPathEdit_logo == 1)
                                        {
                                            pathEdit_logo += itemEdit;
                                        }
                                        else
                                        {
                                            pathEdit_logo += "/" + Path.GetFileName(itemEdit);
                                        }
                                    }
                                    sttPathEdit_logo++;
                                }
                                logo = pathEdit_logo;

                                if (File.Exists(logo))
                                {
                                    File.Delete(logo);
                                }
                                //File.Move(fileData.LocalFileName, rootPath);
                            }
                        }
                        #region add task_logs
                        if (helper.wlog)
                        {
                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = null;
                            log.project_id = project_main.project_id;
                            log.description = "Sửa dự án: " + project_main.project_name;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.is_type = 0;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();
                        }
                        #endregion
                        db.Entry(project_main).State = EntityState.Modified;
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await projectmain;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ProjectMain/Update_ProjectMain", ip, tid, "Lỗi khi sửa dự án", 0, "ProjectMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ProjectMain/Update_ProjectMain", ip, tid, "Lỗi khi sửa dự án", 0, "ProjectMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_ProjectMain([System.Web.Mvc.Bind(Include = "")] List<string> ids)
        {
            string root = HttpContext.Current.Server.MapPath("~/");
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
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.project_main.Where(a => ids.Contains(a.project_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<project_main> del = new List<project_main>();
                            foreach (var da in das)
                            {
                                var list_task = db.task_origin.Where(x=>x.project_id == da.project_id).ToList();
                                if(list_task.Count > 0)
                                {
                                    foreach(var item in list_task)
                                    {
                                        item.project_id = null;
                                        db.Entry(item).State = EntityState.Modified;
                                    }
                                }
                                del.Add(da);
                                if (da.logo != null)
                                {
                                    var delPath = Path.Combine(HttpContext.Current.Server.MapPath("~/" + "/Portals/"), Path.GetFileName(da.organization_id.ToString()), "ProjectMain", Path.GetFileName(da.project_id.ToString()), Path.GetFileName(da.logo));
                                    //if (File.Exists(root + model.logo))
                                    //{
                                    //    File.Delete(root + model.logo);
                                    //}
                                    if (File.Exists(delPath))
                                    {
                                        File.Delete(delPath);
                                    }
                                    //if (File.Exists(root + da.logo))
                                    //    File.Delete(root + da.logo);
                                }

                                #region add task_logs
                                if (helper.wlog)
                                {
                                    task_logs log = new task_logs();
                                    log.log_id = helper.GenKey();
                                    log.task_id = null;
                                    log.project_id = da.project_id;
                                    log.description = "Xóa dự án: " + da.project_name;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.is_type = 0;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.task_logs.Add(log);
                                    db.SaveChanges();
                                }
                                #endregion
                            }

                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.project_main.RemoveRange(del);
                            foreach (string strPath in paths)
                            {
                                // Format strPath
                                var pathFormatRoot = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/");
                                var listPathRoot = pathFormatRoot.Split('/');
                                var pathConfigRoot = "";
                                var sttPartPath = 1;
                                foreach (var item in listPathRoot)
                                {
                                    if (item.Trim() != "")
                                    {
                                        if (sttPartPath == 1)
                                        {
                                            pathConfigRoot += (item);
                                        }
                                        else
                                        {
                                            pathConfigRoot += "/" + Path.GetFileName(item);
                                        }
                                    }
                                    sttPartPath++;
                                }
                                bool exists = File.Exists(pathConfigRoot);
                                if (exists)
                                    System.IO.File.Delete(pathConfigRoot);
                            }
                    }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "ProjectMain/Delete_ProjectMain", ip, tid, "Lỗi khi xoá dự án", 0, "ProjectMain");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "ProjectMain/Delete_ProjectMain", ip, tid, "Lỗi khi xoá dự án", 0, "ProjectMain");
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
        public async Task<HttpResponseMessage> CHeck_Delete_ProjectMain([System.Web.Mvc.Bind(Include = "")] List<string> ids)
        {
            string root = HttpContext.Current.Server.MapPath("~/");
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
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.project_main.Where(a => ids.Contains(a.project_id)).ToListAsync();
                        var count_task = 0;
                        if (das != null)
                        {
                            foreach (var da in das)
                            {
                                var list_task = db.task_origin.Where(x => x.project_id == da.project_id).ToList();
                                count_task += list_task.Count;
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", count_task  = count_task });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "ProjectMain/CHeck_Delete_ProjectMain", ip, tid, "Lỗi khi xoá dự án", 0, "ProjectMain");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "ProjectMain/Delete_ProjectMain", ip, tid, "Lỗi khi xoá dự án", 0, "ProjectMain");
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

        string PortalConfigs = ConfigurationManager.AppSettings["Portals"] ?? "";

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_file([System.Web.Mvc.Bind(Include = "logo,organization_id,project_id")] project_main model)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                if (string.IsNullOrWhiteSpace(PortalConfigs))
                {
                    PortalConfigs = HttpContext.Current.Server.MapPath("~/");
                }
                string root = HttpContext.Current.Server.MapPath("~/");
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
                        var delPath = Path.Combine(HttpContext.Current.Server.MapPath("~/" + "/Portals/"), Path.GetFileName(model.organization_id.ToString()), "ProjectMain", Path.GetFileName(model.project_id.ToString()), Path.GetFileName(model.logo));
                        //if (File.Exists(root + model.logo))
                        //{
                        //    File.Delete(root + model.logo);
                        //}
                        if (File.Exists(delPath))
                        {
                            File.Delete(delPath);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model.logo, contents }), domainurl + "ProjectMain/Delete_file", ip, tid, "Lỗi khi xoá file", 0, "Delete_file");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model.logo, contents }), domainurl + "ProjectMain/Delete_file", ip, tid, "Lỗi khi xoá file", 0, "Delete_file");
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