using API.Helper;
using API.Models;
using Helper;
using HtmlAgilityPack;
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
        string module_key = "M4";
        [HttpPost]
        public async Task<HttpResponseMessage> Add_ProjectMain()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdprojectmain = "";
            string fdprojectmember = "";
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
                    //string strPath = root + "/ProjectMain";
                    //bool exists = Directory.Exists(strPath);
                    //if (!exists)
                    //    Directory.CreateDirectory(strPath);
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
                        fdprojectmember = provider.FormData.GetValues("projectmainmember").SingleOrDefault();
                        fdLogoDonvi = provider.FormData.GetValues("LogoDonvi").SingleOrDefault();
                        if (fdLogoDonvi.StartsWith("\"") && fdLogoDonvi.EndsWith("\""))
                        {
                            fdLogoDonvi = fdLogoDonvi.Trim('"');
                        }
                        if (fdLogoDonvi.Contains(@"/") || fdLogoDonvi.Contains(@"\"))
                        {
                            fdLogoDonvi = System.IO.Path.GetFileName(fdLogoDonvi);
                        }
                        project_main projectmain = JsonConvert.DeserializeObject<project_main>(fdprojectmain);
                        List<task_member> members = JsonConvert.DeserializeObject<List<task_member>>(fdprojectmember);

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
                            bool exists = Directory.Exists(path);
                            if (!exists)
                                Directory.CreateDirectory(path);
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
                                string rootPath = path + "/" + name_file;
                                string Duongdan = "/Portals/" + projectmain.organization_id + "/ProjectMain/" + projectmain.project_id + "/" + name_file;
                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                                if (rootPath.Length > 260)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 260 - (path.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                File.Move(fileData.LocalFileName, rootPath);
                                if (org_name_file == fdLogoDonvi)
                                {
                                    projectmain.logo = Duongdan;
                                }
                                else
                                {
                                    var df = new task_file();
                                    df.file_id = helper.GenKey();
                                    df.task_id = null;
                                    df.project_id = projectmain.project_id;
                                    df.file_name = name_file;
                                    df.file_path = Duongdan;
                                    df.file_type = Dinhdang;
                                    var file_info = new FileInfo(rootPath);
                                    df.file_size = file_info.Length;
                                    df.is_image = helper.IsImageFileName(name_file);
                                    if (df.is_image == true)
                                    {
                                        //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                    }
                                    df.is_type = 0;
                                    df.status = true;
                                    df.created_by = uid;
                                    df.created_date = DateTime.Now;
                                    df.created_ip = ip;
                                    df.created_token_id = tid;
                                    dfs.Add(df);
                                }
                            }
                            if (dfs.Count > 0)
                            {
                                db.task_file.AddRange(dfs);
                            }
                            #endregion
                        }
                        #region add task_member
                        if (members.Count > 0)
                        {
                            List<task_member> listmems = new List<task_member>();
                            foreach (var item in members)
                            {
                                task_member member = new task_member
                                {
                                    member_id = helper.GenKey(),
                                    project_id = projectmain.project_id,
                                    task_id = null,
                                    user_id = item.user_id,
                                    is_type = item.is_type, // 0: người quản lý, 1: người tham gia
                                    status = item.status,
                                    created_date = DateTime.Now,
                                    created_by = uid,
                                    modified_by = uid,
                                    modified_date = DateTime.Now,
                                    created_ip = ip,
                                    created_token_id = tid,
                                };
                                listmems.Add(member);
                            }
                            if (listmems.Count > 0)
                            {
                                db.task_member.AddRange(listmems);
                            }
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
                Log.Error(contents);
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
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_ProjectMain()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdprojectmain = "";
            string fdprojectmember = "";
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
                    //string strPath = root + "/ProjectMain";
                    //bool exists = Directory.Exists(strPath);
                    //if (!exists)
                    //    Directory.CreateDirectory(strPath);
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
                        fdprojectmember = provider.FormData.GetValues("projectmainmember").SingleOrDefault();
                        fdLogoDonvi = provider.FormData.GetValues("LogoDonvi").SingleOrDefault();
                        if (fdLogoDonvi.StartsWith("\"") && fdLogoDonvi.EndsWith("\""))
                        {
                            fdLogoDonvi = fdLogoDonvi.Trim('"');
                        }
                        if (fdLogoDonvi.Contains(@"/") || fdLogoDonvi.Contains(@"\"))
                        {
                            fdLogoDonvi = System.IO.Path.GetFileName(fdLogoDonvi);
                        }
                        List<task_member> members = JsonConvert.DeserializeObject<List<task_member>>(fdprojectmember);
                        project_main projectmain = JsonConvert.DeserializeObject<project_main>(fdprojectmain);

                        projectmain.modified_date = DateTime.Now;
                        projectmain.modified_by = uid;
                        projectmain.modified_date = DateTime.Now;
                        projectmain.modified_ip = ip;
                        projectmain.modified_token_id = tid;

                        var file = provider.FileData;

                        if (file.Count > 0)
                        {
                            #region file
                            string path = root + "/" + projectmain.organization_id + "/ProjectMain/" + projectmain.project_id;
                            bool exists = Directory.Exists(path);
                            if (!exists)
                                Directory.CreateDirectory(path);
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
                                string rootPath = path + "/" + name_file;
                                string Duongdan = "/Portals/" + projectmain.organization_id + "/ProjectMain/" + projectmain.project_id + "/" + name_file;
                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                                if (rootPath.Length > 260)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 260 - (path.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                File.Move(fileData.LocalFileName, rootPath);
                                if (org_name_file == fdLogoDonvi)
                                {
                                    var data = db.project_main.AsNoTracking().Where(x => x.project_id == projectmain.project_id).ToList();
                                    foreach (var item in data)
                                    {
                                        var logo = filepath + item.logo;
                                        if (File.Exists(logo))
                                        {
                                            File.Delete(logo);
                                        }
                                        //File.Move(fileData.LocalFileName, rootPath);
                                    }
                                    projectmain.logo = Duongdan;
                                }
                                else
                                {
                                    var df = new task_file();
                                    df.file_id = helper.GenKey();
                                    df.task_id = null;
                                    df.project_id = projectmain.project_id;
                                    df.file_name = name_file;
                                    df.file_path = Duongdan;
                                    df.file_type = Dinhdang;
                                    var file_info = new FileInfo(rootPath);
                                    df.file_size = file_info.Length;
                                    df.is_image = helper.IsImageFileName(name_file);
                                    if (df.is_image == true)
                                    {
                                        //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                    }
                                    df.is_type = 0;
                                    df.status = true;
                                    df.created_by = uid;
                                    df.created_date = DateTime.Now;
                                    df.created_ip = ip;
                                    df.created_token_id = tid;
                                    dfs.Add(df);
                                }
                            }
                            if (dfs.Count > 0)
                            {
                                db.task_file.AddRange(dfs);
                            }
                            #endregion
                        }
                        if (projectmain.logo == null)
                        {
                            var data = db.project_main.AsNoTracking().Where(x => x.project_id == projectmain.project_id).ToList();
                            foreach (var item in data)
                            {
                                var logo = filepath + item.logo;
                                if (File.Exists(logo))
                                {
                                    File.Delete(logo);
                                }
                                //File.Move(fileData.LocalFileName, rootPath);
                            }
                        }
                        #region update task_member
                        List<task_member> del_member = new List<task_member>();
                        var model_del_members = db.task_member.Where(a => a.project_id == projectmain.project_id).ToList();
                        if (model_del_members.Count > 0)
                        {
                            foreach (var m in model_del_members)
                            {
                                del_member.Add(m);
                            }
                        }
                        db.task_member.RemoveRange(del_member);
                        if (members.Count > 0)
                        {
                            List<task_member> listmems = new List<task_member>();
                            foreach (var item in members)
                            {
                                task_member member = new task_member
                                {
                                    member_id = helper.GenKey(),
                                    project_id = projectmain.project_id,
                                    task_id = null,
                                    user_id = item.user_id,
                                    is_type = item.is_type,
                                    status = item.status,
                                    created_date = DateTime.Now,
                                    created_by = uid,
                                    modified_by = uid,
                                    modified_date = DateTime.Now,
                                    created_ip = ip,
                                    created_token_id = tid,
                                };
                                listmems.Add(member);
                            }
                            if (listmems.Count > 0)
                            {
                                db.task_member.AddRange(listmems);
                            }
                        }
                        #endregion
                        db.Entry(projectmain).State = EntityState.Modified;
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
                Log.Error(contents);
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
                Log.Error(contents);
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
            //try
            //{
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
                        List<task_member> del_member = new List<task_member>();
                        List<task_file> del_file = new List<task_file>();
                        foreach (var da in das)
                        {
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
                            }
                            //if (File.Exists(root + da.logo))
                            //    File.Delete(root + da.logo);
                            #region add cms_logs

                            #region del member
                            var members = await db.task_member.Where(a => a.project_id == da.project_id).ToListAsync();
                            if (members.Count > 0)
                            {
                                foreach (var m in members)
                                {
                                    del_member.Add(m);
                                }
                            }
                            #endregion

                            #region del file
                            var files = await db.task_file.Where(a => a.project_id == da.project_id).ToListAsync();
                            if (files.Count > 0)
                            {
                                foreach (var f in files)
                                {
                                    del_file.Add(f);
                                    paths.Add(root + f.file_path);
                                }
                            }
                            #endregion

                            if (helper.wlog)
                            {

                                cms_logs log = new cms_logs();
                                log.log_title = "Xóa dự án " + da.project_id;

                                log.log_module = "Dự án";
                                log.id_key = da.project_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.cms_logs.Add(log);
                                db.SaveChanges();

                            }
                            #endregion
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.task_member.RemoveRange(del_member);
                        db.task_file.RemoveRange(del_file);
                        db.project_main.RemoveRange(del);
                        foreach (string strPath in paths)
                        {
                            bool exists = File.Exists(strPath);
                            if (exists)
                                System.IO.File.Delete(strPath);
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
                Log.Error(contents);
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
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
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
            //try
            //{
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
                Log.Error(contents);
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
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_DiscussProjectMain()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fddiscussproject = "";
            string fddiscussmember = "";
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
                    //string strPath = root + "/ProjectMain";
                    //bool exists = Directory.Exists(strPath);
                    //if (!exists)
                    //    Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var projectmain = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fddiscussproject = provider.FormData.GetValues("discussProject").SingleOrDefault();
                        fddiscussmember = provider.FormData.GetValues("discussMember").SingleOrDefault();
                        discuss_project discussproject = JsonConvert.DeserializeObject<discuss_project>(fddiscussproject);
                        List<discuss_member> members = JsonConvert.DeserializeObject<List<discuss_member>>(fddiscussmember);

                        discussproject.discuss_project_id = helper.GenKey();
                        discussproject.created_date = DateTime.Now;
                        discussproject.created_by = uid;
                        discussproject.modified_by = uid;
                        discussproject.modified_date = DateTime.Now;
                        discussproject.created_ip = ip;
                        discussproject.created_token_id = tid;
                        db.discuss_project.Add(discussproject);

                        #region add discuss_member
                        if (members.Count > 0)
                        {
                            List<discuss_member> listmems = new List<discuss_member>();
                            foreach (var item in members)
                            {
                                discuss_member member = new discuss_member
                                {
                                    discuss_member_id = helper.GenKey(),
                                    discuss_project_id = discussproject.discuss_project_id,
                                    user_id = item.user_id,
                                    status = true,
                                    created_date = DateTime.Now,
                                    created_by = uid,
                                    modified_by = uid,
                                    modified_date = DateTime.Now,
                                    created_ip = ip,
                                    created_token_id = tid,
                                };
                                listmems.Add(member);
                            }
                            if (listmems.Count > 0)
                            {
                                db.discuss_member.AddRange(listmems);
                            }
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
                Log.Error(contents);
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
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Update_DiscussProjectMain()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fddiscussproject = "";
            string fddiscussmember = "";
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
                    //string strPath = root + "/ProjectMain";
                    //bool exists = Directory.Exists(strPath);
                    //if (!exists)
                    //    Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var projectmain = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fddiscussproject = provider.FormData.GetValues("discussProject").SingleOrDefault();
                        fddiscussmember = provider.FormData.GetValues("discussMember").SingleOrDefault();
                        discuss_project discussproject = JsonConvert.DeserializeObject<discuss_project>(fddiscussproject);
                        List<discuss_member> members = JsonConvert.DeserializeObject<List<discuss_member>>(fddiscussmember);

                        //discussproject.discuss_project_id = helper.GenKey();
                        discussproject.modified_date = DateTime.Now;
                        discussproject.modified_by = uid;
                        discussproject.modified_date = DateTime.Now;
                        discussproject.modified_ip = ip;
                        discussproject.modified_token_id = tid;


                        #region add discuss_member
                        List<discuss_member> del_member = new List<discuss_member>();
                        var model_del_members = db.discuss_member.Where(a => a.discuss_project_id == discussproject.discuss_project_id).ToList();
                        if (model_del_members.Count > 0)
                        {
                            foreach (var m in model_del_members)
                            {
                                del_member.Add(m);
                            }
                        }
                        db.discuss_member.RemoveRange(del_member);
                        if (members.Count > 0)
                        {
                            List<discuss_member> listmems = new List<discuss_member>();
                            foreach (var item in members)
                            {
                                discuss_member member = new discuss_member
                                {
                                    discuss_member_id = helper.GenKey(),
                                    discuss_project_id = discussproject.discuss_project_id,
                                    user_id = item.user_id,
                                    status = true,
                                    created_date = DateTime.Now,
                                    created_by = uid,
                                    modified_by = uid,
                                    modified_date = DateTime.Now,
                                    created_ip = ip,
                                    created_token_id = tid,
                                };
                                listmems.Add(member);
                            }
                            if (listmems.Count > 0)
                            {
                                db.discuss_member.AddRange(listmems);
                            }
                        }
                        #endregion
                        db.SaveChanges();
                        db.Entry(discussproject).State = EntityState.Modified;
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
                Log.Error(contents);
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
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        //[HttpPost]
        //public async Task<HttpResponseMessage> Delete_DiscussProjectMain()
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    string fddiscussproject = "";
        //    string fddiscussmember = "";
        //    IEnumerable<Claim> claims = identity.Claims;
        //    string ip = getipaddress();
        //    string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
        //    string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
        //    string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
        //    string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
        //    if (identity == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
        //    }
        //    try
        //    {
        //        using (DBEntities db = new DBEntities())
        //        {
        //            if (!Request.Content.IsMimeMultipartContent())
        //            {
        //                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //            }

        //            string root = HttpContext.Current.Server.MapPath("~/Portals");
        //            //string strPath = root + "/ProjectMain";
        //            //bool exists = Directory.Exists(strPath);
        //            //if (!exists)
        //            //    Directory.CreateDirectory(strPath);
        //            var provider = new MultipartFormDataStreamProvider(root);

        //            // Read the form data and return an async task.
        //            var projectmain = Request.Content.ReadAsMultipartAsync(provider).
        //            ContinueWith<HttpResponseMessage>(t =>
        //            {
        //                if (t.IsFaulted || t.IsCanceled)
        //                {
        //                    Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
        //                }
        //                fddiscussproject = provider.FormData.GetValues("discussProject").SingleOrDefault();
        //                fddiscussmember = provider.FormData.GetValues("discussMember").SingleOrDefault();
        //                discuss_project discussproject = JsonConvert.DeserializeObject<discuss_project>(fddiscussproject);
        //                List<discuss_member> members = JsonConvert.DeserializeObject<List<discuss_member>>(fddiscussmember);

        //                //discussproject.discuss_project_id = helper.GenKey();
        //                discussproject.modified_date = DateTime.Now;
        //                discussproject.modified_by = uid;
        //                discussproject.modified_date = DateTime.Now;
        //                discussproject.modified_ip = ip;
        //                discussproject.modified_token_id = tid;


        //                #region add discuss_member
        //                List<discuss_member> del_member = new List<discuss_member>();
        //                var model_del_members = db.discuss_member.Where(a => a.discuss_project_id == discussproject.discuss_project_id).ToList();
        //                if (model_del_members.Count > 0)
        //                {
        //                    foreach (var m in model_del_members)
        //                    {
        //                        del_member.Add(m);
        //                    }
        //                }
        //                db.discuss_member.RemoveRange(del_member);
        //                if (members.Count > 0)
        //                {
        //                    List<discuss_member> listmems = new List<discuss_member>();
        //                    foreach (var item in members)
        //                    {
        //                        discuss_member member = new discuss_member
        //                        {
        //                            discuss_member_id = helper.GenKey(),
        //                            discuss_project_id = discussproject.discuss_project_id,
        //                            user_id = item.user_id,
        //                            status = true,
        //                            created_date = DateTime.Now,
        //                            created_by = uid,
        //                            modified_by = uid,
        //                            modified_date = DateTime.Now,
        //                            created_ip = ip,
        //                            created_token_id = tid,
        //                        };
        //                        listmems.Add(member);
        //                    }
        //                    if (listmems.Count > 0)
        //                    {
        //                        db.discuss_member.AddRange(listmems);
        //                    }
        //                }
        //                #endregion
        //                db.SaveChanges();
        //                db.Entry(discussproject).State = EntityState.Modified;
        //                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
        //            });
        //            return await projectmain;
        //        }

        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        string contents = helper.getCatchError(e, null);
        //        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ProjectMain/Delete_DiscussProjectMain", ip, tid, "Lỗi khi xóa thảo luận dự án", 0, "ProjectMain");
        //        if (!helper.debug)
        //        {
        //            contents = "";
        //        }
        //        Log.Error(contents);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
        //    }
        //    catch (Exception e)
        //    {
        //        string contents = helper.ExceptionMessage(e);
        //        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ProjectMain/Delete_DiscussProjectMain", ip, tid, "Lỗi khi xóa thảo luận dự án", 0, "ProjectMain");
        //        if (!helper.debug)
        //        {
        //            contents = "";
        //        }
        //        Log.Error(contents);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
        //    }
        //}

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_DiscussProjectMain([System.Web.Mvc.Bind(Include = "")] List<string> ids)
        {
            string root = HttpContext.Current.Server.MapPath("~/");
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //try
            //{
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
                    var das = await db.discuss_project.Where(a => ids.Contains(a.discuss_project_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<discuss_project> del = new List<discuss_project>();
                        List<discuss_member> del_member = new List<discuss_member>();
                        foreach (var da in das)
                        {
                            del.Add(da);
                            #region del member
                            var members = await db.discuss_member.Where(a => a.discuss_project_id == da.discuss_project_id).ToListAsync();
                            if (members.Count > 0)
                            {
                                foreach (var m in members)
                                {
                                    del_member.Add(m);
                                }
                            }
                            #endregion

                            if (helper.wlog)
                            {

                                cms_logs log = new cms_logs();
                                log.log_title = "Xóa thảo luận dự án " + da.discuss_project_id;

                                log.log_module = "Thảo luận dự án";
                                log.id_key = da.discuss_project_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.cms_logs.Add(log);
                                db.SaveChanges();

                            }
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.discuss_member.RemoveRange(del_member);
                        db.discuss_project.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "ProjectMain/Delete_DiscussProjectMain", ip, tid, "Lỗi khi xoá dự án", 0, "ProjectMain");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "ProjectMain/Delete_DiscussProjectMain", ip, tid, "Lỗi khi xoá dự án", 0, "ProjectMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> add_Comments()
        {

            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;

            string fdcmtbug = "";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
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
                    List<discuss_file> discuss_Files = new List<discuss_file>();
                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdcmtbug = provider.FormData.GetValues("discuss").SingleOrDefault();
                        string id = provider.FormData.GetValues("discuss_project_id").SingleOrDefault();
                        string is_reply = provider.FormData.GetValues("is_reply").SingleOrDefault();
                        is_reply = is_reply.Replace("\"", "");
                        is_reply = is_reply.Trim();
                        discuss cmtbug = JsonConvert.DeserializeObject<discuss>(fdcmtbug);
                        cmtbug.discuss_id = helper.GenKey();
                        cmtbug.parent_id = is_reply == "true" ? provider.FormData.GetValues("parent_id").SingleOrDefault() : null;
                        cmtbug.parent_id = is_reply == "true" ? cmtbug.parent_id.Replace("\"", "") : null;
                        cmtbug.parent_id = is_reply == "true" ? cmtbug.parent_id.Trim() : null;
                        id = id.Replace("\"", "");
                        id = id.Trim();
                        // This illustrates how to get thefile names.
                        string strPath = root + "/" + dvid + "/ProjectMain/Discuss/" + id + "/" + cmtbug.discuss_id;
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);
                        if (cmtbug.contents != null && cmtbug.contents != "")
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(cmtbug.contents);

                            var imgs = doc.DocumentNode.SelectNodes("//img");

                            var htmlBody = doc.DocumentNode.SelectSingleNode("//body");
                            if (imgs != null)
                            {

                                foreach (var img in imgs)
                                {
                                    HtmlNode oldChild = img;
                                    HtmlNode newChild = HtmlNode.CreateNode(img.OuterHtml); ;

                                    var checkBase64 = newChild.Attributes["src"].Value.Substring(newChild.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                                    checkBase64 = checkBase64.Trim();
                                    if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                                    {
                                        byte[] bytes = Convert.FromBase64String(newChild.Attributes["src"].Value.Substring(newChild.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                                        bool existsFolder = System.IO.Directory.Exists(strPath);
                                        if (!existsFolder)
                                        {
                                            System.IO.Directory.CreateDirectory(strPath);
                                        }

                                        var index1 = newChild.Attributes["src"].Value.LastIndexOf("data:image/") + 11;
                                        var index2 = newChild.Attributes["src"].Value.IndexOf("base64,");
                                        var typeFileHL = "." + newChild.Attributes["src"].Value.Substring(index1, index2 - index1 - 1);
                                        var pathShow = "/" + helper.GenKey() + typeFileHL;
                                        using (var imageFile = new FileStream(strPath + pathShow, FileMode.Create))
                                        {
                                            imageFile.Write(bytes, 0, bytes.Length);
                                            imageFile.Flush();
                                        }


                                        img.SetAttributeValue("style", "width:100%;max-width:30vw;object-fit:contain");
                                        img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/TaskOrigin/" + id + "/" + cmtbug.discuss_id + pathShow);
                                        cmtbug.contents = cmtbug.contents.Replace(newChild.OuterHtml, img.OuterHtml);
                                        discuss_file discuss_file = new discuss_file();
                                        discuss_file.file_id = helper.GenKey();
                                        discuss_file.discuss_id = cmtbug.discuss_id;
                                        discuss_file.file_name = pathShow.Replace("/", "");
                                        discuss_file.file_path = "/Portals" + "/" + dvid + "/ProjectMain/Discuss/" + id + "/" + cmtbug.discuss_id + pathShow;
                                        discuss_file.file_type = typeFileHL.Substring(1);
                                        var file_info = new FileInfo(root + "/" + dvid + "/ProjectMain/Discuss/" + id + "/" + cmtbug.discuss_id + pathShow);
                                        discuss_file.file_size = file_info.Length;
                                        discuss_file.is_image = helper.IsImageFileName(pathShow);
                                        discuss_file.status = false;
                                        discuss_file.created_by = uid;
                                        discuss_file.created_date = DateTime.Now;
                                        discuss_file.created_ip = ip;
                                        discuss_file.created_token_id = tid;
                                        discuss_Files.Add(discuss_file);
                                    }
                                }

                            }

                        }
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
                            newFileName = Path.Combine(strPath, fileName);
                            fileInfo = new FileInfo(newFileName);
                            string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                            if (fileInfo != null)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + "_(" + helper.ranNumberFile() + ")" + fileInfo.Extension;

                                if (fileName.Length > 500)
                                {
                                    fileName = fileName.Substring(0, fileName.LastIndexOf('.') - 1);
                                    int le = 500 - (strPath.Length + 1) - Dinhdang.Length;
                                    fileName = fileName.Substring(0, le) + fileName;
                                }
                                newFileName = Path.Combine(strPath, fileName);
                            }
                            ffileData = fileData;
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }
                            discuss_file discuss_file = new discuss_file();
                            discuss_file.file_id = helper.GenKey();
                            discuss_file.discuss_id = cmtbug.discuss_id;
                            discuss_file.file_name = fileName;
                            discuss_file.file_path = "/" + "Portals" + "/" + dvid + "/ProjectMain/Discuss/" + id + "/" + cmtbug.discuss_id + "/" + fileName;
                            discuss_file.file_type = Dinhdang;
                            var file_info = new FileInfo(strPath + "/" + fileName);
                            discuss_file.file_size = file_info.Length;
                            discuss_file.is_image = helper.IsImageFileName(fileName);
                            discuss_file.status = true;
                            discuss_file.created_by = uid;
                            discuss_file.created_date = DateTime.Now;
                            discuss_file.created_ip = ip;
                            discuss_file.created_token_id = tid;
                            discuss_Files.Add(discuss_file);
                        }
                        cmtbug.discuss_project_id = id;
                        cmtbug.created_by = uid;
                        cmtbug.created_date = DateTime.Now;
                        cmtbug.created_by = uid;
                        cmtbug.created_ip = ip;
                        cmtbug.created_token_id = tid;
                        db.discuss_file.AddRange(discuss_Files);
                        db.discusses.Add(cmtbug);
                        db.SaveChanges();
                        #region add notify
                        var listuser = db.discuss_member.Where(x => x.discuss_project_id == cmtbug.discuss_project_id).Select(x => x.user_id).Distinct().ToList();
                        string discuss_project_name = db.discuss_project.Where(x => x.discuss_project_id == cmtbug.discuss_project_id).Select(x => x.discuss_project_content).FirstOrDefault().ToString();
                        listuser.Remove(uid);
                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Thảo luận dự án", "Đã thảo luận nội dung: " + (discuss_project_name.Length > 100 ? discuss_project_name.Substring(0, 97) + "..." : discuss_project_name),
                                null, 2, -1, false, module_key, cmtbug.discuss_id, null, null, tid, ip);
                        }
                        #endregion
                        //#region add task_logs
                        //if (helper.wlog && cmtbug.discuss_id != null)
                        //{
                        //    var task = db.task_origin.Where(x => x.task_id == id).FirstOrDefault();
                        //    task_logs log = new task_logs();
                        //    log.log_id = helper.GenKey();
                        //    log.task_id = id;
                        //    log.comment_id = cmtbug.discuss_id;
                        //    log.project_id = null;
                        //    log.description = "Thêm mới bình luận công việc " + "'<b>" + task.task_name + "</b>'";
                        //    log.created_date = DateTime.Now;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.task_logs.Add(log);
                        //    db.SaveChanges();
                        //}
                        //#endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_comments/add_Task_Comments", ip, tid, "Lỗi khi thêm bình luận công việc", 0, "task_comments");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_comments/add_Task_Comments", ip, tid, "Lỗi khi thêm bình luận công việc", 0, "task_comments");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> updateComments()
        {
            string rootDel = HttpContext.Current.Server.MapPath("~/");
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string fdcmtbug = "";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
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
                    List<task_file> task_Files = new List<task_file>();
                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        fdcmtbug = provider.FormData.GetValues("comment").SingleOrDefault();
                        string id = provider.FormData.GetValues("task_id").SingleOrDefault();
                        id = id.Replace("\"", "");
                        id = id.Trim();
                        string cmt_id = provider.FormData.GetValues("cmt_id").SingleOrDefault();
                        cmt_id = cmt_id.Replace("\"", "");
                        cmt_id = cmt_id.Trim();
                        string delFilesID = provider.FormData.GetValues("Del_file_ID").SingleOrDefault();

                        foreach (var idwww in delFilesID)
                        {
                            var nine = idwww;
                        }
                        List<string> ListDelFile = JsonConvert.DeserializeObject<List<string>>(delFilesID);
                        task_comments cmtbug = JsonConvert.DeserializeObject<task_comments>(fdcmtbug);
                        // This illustrates how to get thefile names.
                        string strPath = root + "/" + dvid + "/TaskOrigin/" + id + "/" + cmt_id;
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);
                        if (cmtbug.contents != null && cmtbug.contents != "")
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(cmtbug.contents);

                            var imgs = doc.DocumentNode.SelectNodes("//img");

                            var htmlBody = doc.DocumentNode.SelectSingleNode("//body");
                            if (imgs != null)
                            {

                                foreach (var img in imgs)
                                {
                                    HtmlNode oldChild = img;
                                    HtmlNode newChild = HtmlNode.CreateNode(img.OuterHtml); ;

                                    var checkBase64 = newChild.Attributes["src"].Value.Substring(newChild.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                                    checkBase64 = checkBase64.Trim();
                                    if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                                    {
                                        byte[] bytes = Convert.FromBase64String(newChild.Attributes["src"].Value.Substring(newChild.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                                        bool existsFolder = System.IO.Directory.Exists(strPath);
                                        if (!existsFolder)
                                        {
                                            System.IO.Directory.CreateDirectory(strPath);
                                        }

                                        var index1 = newChild.Attributes["src"].Value.LastIndexOf("data:image/") + 11;
                                        var index2 = newChild.Attributes["src"].Value.IndexOf("base64,");
                                        var typeFileHL = "." + newChild.Attributes["src"].Value.Substring(index1, index2 - index1 - 1);
                                        var pathShow = "/" + helper.GenKey() + typeFileHL;
                                        using (var imageFile = new FileStream(strPath + pathShow, FileMode.Create))
                                        {
                                            imageFile.Write(bytes, 0, bytes.Length);
                                            imageFile.Flush();
                                        }


                                        img.SetAttributeValue("style", "width:100%;max-width:30vw;object-fit:contain");
                                        img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/TaskOrigin/" + id + "/" + cmt_id + pathShow);
                                        cmtbug.contents = cmtbug.contents.Replace(newChild.OuterHtml, img.OuterHtml);
                                        task_file task_File = new task_file();
                                        task_File.file_id = helper.GenKey();
                                        task_File.comment_id = cmt_id;
                                        task_File.file_name = pathShow.Replace("/", "");
                                        task_File.file_path = "/Portals" + "/" + dvid + "/TaskOrigin/" + id + "/" + cmt_id + pathShow;
                                        task_File.file_type = typeFileHL.Substring(1);
                                        var file_info = new FileInfo(root + "/" + dvid + "/TaskOrigin/" + id + "/" + cmt_id + pathShow);
                                        task_File.file_size = file_info.Length;
                                        task_File.is_image = helper.IsImageFileName(pathShow);
                                        task_File.is_type = 2;
                                        task_File.status = false;
                                        task_File.created_by = uid;
                                        task_File.created_date = DateTime.Now;
                                        task_File.created_ip = ip;
                                        task_File.task_id = id;
                                        task_File.created_token_id = tid;
                                        task_Files.Add(task_File);
                                    }
                                }

                            }

                        }
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
                            newFileName = Path.Combine(strPath, fileName);
                            fileInfo = new FileInfo(newFileName);
                            string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                            if (fileInfo != null)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + "_(" + helper.ranNumberFile() + ")" + fileInfo.Extension;

                                if (fileName.Length > 500)
                                {
                                    fileName = fileName.Substring(0, fileName.LastIndexOf('.') - 1);
                                    int le = 500 - (strPath.Length + 1) - Dinhdang.Length;
                                    fileName = fileName.Substring(0, le) + fileName;
                                }
                                newFileName = Path.Combine(strPath, fileName);
                            }
                            ffileData = fileData;
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }
                            task_file task_File = new task_file();
                            task_File.file_id = helper.GenKey();
                            task_File.comment_id = cmt_id;
                            task_File.task_id = id;
                            task_File.file_name = fileName;
                            task_File.file_path = "/" + "Portals" + "/" + dvid + "/TaskOrigin/" + id + "/" + cmt_id + "/" + fileName;
                            task_File.file_type = Dinhdang;
                            var file_info = new FileInfo(strPath + "/" + fileName);
                            task_File.file_size = file_info.Length;
                            task_File.is_image = helper.IsImageFileName(fileName);
                            task_File.is_type = 2;
                            task_File.status = true;
                            task_File.created_by = uid;
                            task_File.created_date = DateTime.Now;
                            task_File.created_ip = ip;
                            task_File.created_token_id = tid;
                            task_Files.Add(task_File);
                        }
                        var editCmt = db.task_comments.Where(x => x.comment_id == cmt_id).FirstOrDefault();
                        editCmt.contents = cmtbug.contents;
                        editCmt.modified_by = uid;
                        editCmt.modified_date = DateTime.Now;
                        editCmt.modified_ip = ip;
                        editCmt.modified_token_id = tid;
                        db.task_file.AddRange(task_Files);
                        #region Xóa file cũ
                        List<string> delFiles = new List<string>();
                        List<string> file_paths = new List<string>();
                        List<task_file> delFilesDatabase = new List<task_file>();
                        var ImagesInCmt = db.task_file.Where(x => x.comment_id == cmt_id && x.status == false).ToList();
                        foreach (var img in ImagesInCmt)
                        {
                            delFilesDatabase.Add(img);
                            file_paths.Add(img.file_path);
                        }
                        foreach (var fileID in ListDelFile)
                        {
                            var filedata = db.task_file.Where(x => x.file_id == fileID).FirstOrDefault();
                            delFilesDatabase.Add(filedata);
                            file_paths.Add(filedata.file_path);
                        }
                        foreach (var file in file_paths)
                        {
                            //var pathsss = rootDel + file;
                            //bool ehxists = File.Exists(rootDel + file);
                            //if (ehxists == true)
                            //{
                            //    File.Delete(rootDel + file);
                            //}

                            // Format file
                            var strPathFormat = Regex.Replace(file.Replace("\\", "/"), @"\.*/+", "/");
                            var listStrPath = strPathFormat.Split('/');
                            var strPathConfig = "";
                            foreach (var item in listStrPath)
                            {
                                if (item.Trim() != "")
                                {
                                    strPathConfig += "/" + Path.GetFileName(item);
                                }
                            }
                            bool ehxists = File.Exists(rootDel + strPathConfig);
                            if (ehxists == true)
                            {
                                File.Delete(rootDel + strPathConfig);
                            }
                        }
                        #endregion
                        db.task_file.RemoveRange(delFilesDatabase);
                        db.Entry(editCmt).State = EntityState.Modified;
                        db.SaveChanges();
                        //notify
                        var listuser = db.task_member.Where(x => x.task_id == editCmt.task_id).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == editCmt.task_id).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);
                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Chỉnh sửa bình luận công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, editCmt.task_id, null, null, tid, ip);
                        }
                        #region add task_logs
                        if (helper.wlog && cmt_id != null)
                        {
                            var task = db.task_origin.Where(x => x.task_id == id).FirstOrDefault();
                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = id;
                            log.comment_id = cmt_id;
                            log.project_id = null;
                            log.description = "Chỉnh sửa bình luận công việc " + "'<b>" + task.task_name + "</b>'";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_comments/updateComments", ip, tid, "Lỗi khi chỉnh sửa bình luận công việc", 0, "task_comments");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_comments/updateComments", ip, tid, "Lỗi khi chỉnh sửa bình luận công việc", 0, "task_comments");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        public List<string> deleteChild(List<string> id)
        {
            List<string> del = new List<string>();
            using (DBEntities db = new DBEntities())
            {

                var das = db.discusses.Where(a => id.Contains(a.discuss_id)).ToArray();

                if (das != null)
                {


                    foreach (var da in das)
                    {
                        var arrC = db.discusses.Where(a => a.parent_id != null).ToArray();
                        del.Add(da.discuss_id);
                        var arrId = new List<string>();
                        for (int i = 0; i < id.Count; i++)
                        {
                            for (int j = 0; j < arrC.Length; j++)
                            {
                                if (id[i] == arrC[j].parent_id)
                                {

                                    arrId.Add(arrC[j].discuss_id);
                                    del.AddRange(deleteChild(arrId));
                                }
                            }
                        }



                    }


                }


            }
            return del;
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Discuss([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    var arrC = deleteChild(id);
                    var das = await db.discusses.Where(a=>arrC.Contains(a.discuss_id)).ToListAsync();

                    if (das.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Có lỗi xảy ra! Vui lòng kiểm tra lại." });
                    }
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<discuss> del = new List<discuss>();
                        List<discuss_file> delFile = new List<discuss_file>();
                        foreach (var da in das)
                        {
                            if (uid == da.created_by)
                            {
                                del.Add(da);
                                
                                var file = db.discuss_file.Where(x => x.discuss_id == da.discuss_id).FirstOrDefault();

                                if (file != null)
                                {
                                    paths.Add(file.file_path);
                                    delFile.Add(file);
                                }

                            }
                            //#region add cms_logs
                            //if (helper.wlog)
                            //{
                            //    var de = db.discusses.Where(a => (a.discuss_id == da.discuss_id)).FirstOrDefault<discuss>();

                            //    if (de.discuss_id != null)
                            //    {
                            //        task_logs log = new task_logs();
                            //        log.log_id = helper.GenKey();
                            //        log.comment_id = de.discuss_id;
                            //        log.project_id = null;
                            //        log.description = "Xóa bình luận" + "'<b>" + des.task_name + "</b>'";
                            //        log.created_date = DateTime.Now;
                            //        log.created_by = uid;
                            //        log.created_token_id = tid;
                            //        log.created_ip = ip;
                            //        db.task_logs.Add(log);
                            //        db.SaveChanges();
                            //    }
                            //}
                            //#endregion
                        }
                        //string ssid = das[0].discuss_id;
                        ////notify
                        //var listuser = db.di.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                        //string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                        //listuser.Remove(uid);
                        //foreach (var l in listuser)
                        //{
                        //    helper.saveNotify(uid, l, null, "Công việc", "Xóa bình luận công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                        //        null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                        //}

                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.discusses.RemoveRange(del);
                        db.discuss_file.RemoveRange(delFile);
                    }
                    await db.SaveChangesAsync();
                    foreach (string strPath in paths)
                    {
                        //bool exists = File.Exists(strPath);
                        //if (exists)
                        //{ 
                        //    File.Delete(strPath); 
                        //}
                        var strPathFormat = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/");
                        var listStrPath = strPathFormat.Split('/');
                        var strPathConfig = "";
                        foreach (var item in listStrPath)
                        {
                            if (item.Trim() != "")
                            {
                                strPathConfig += "/" + Path.GetFileName(item);
                            }
                        }
                        bool ex = System.IO.File.Exists(root + strPathConfig);
                        if (ex)
                        {
                            System.IO.File.Delete(root + strPathConfig);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "/task_comments/deleteTaskComments", ip, tid, "Lỗi khi xoá bình luận", 1, "task_comments");

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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "/task_comments/deleteTaskComments", ip, tid, "Lỗi khi xóa bình luận", 1, "task_comments");

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