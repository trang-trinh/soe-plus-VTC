using API.Helper;
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
                                if(org_name_file == fdLogoDonvi)
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
                        if(projectmain.logo == null)
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
                                if(da.logo != null)
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
    }
}