using System;
using System.Collections.Generic;
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
using API.Models;
using API.Helper;
using Helper;
using Newtonsoft.Json;
namespace API.Controllers.Task_Origin1
{
    [Authorize(Roles = "login")]
    public class task_FilesController : ApiController
    {
        string module_key = "M4";
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> add_Task_File()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string report1 = "";

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

                    string root = HttpContext.Current.Server.MapPath("~/");

                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        report1 = provider.FormData.GetValues("task_id").SingleOrDefault();
                        string report = JsonConvert.DeserializeObject<string>(report1);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        var task = db.task_origin.AsNoTracking().Where(x => x.task_id == report).FirstOrDefault();
                        var file = provider.FileData;
                        var check = "";

                        if (file.Count > 0)
                        {
                            check += "có " + file.Count + " file";
                            #region file
                            string path = root + "/Portals/" + task.organization_id + "/TaskOrigin/" + task.task_id + "/";
                            bool exists = Directory.Exists(path);
                            if (!exists)
                            {
                                check += "vừa tạo folder";
                                Directory.CreateDirectory(path);
                            }
                            else
                            {
                                check += "đã có folder";
                            }

                            List<task_file> dfs = new List<task_file>();
                            foreach (MultipartFileData fileData in provider.FileData)
                            {
                                string org_name_file = fileData.Headers.ContentDisposition.FileName;
                                check += "file " + org_name_file;
                                if (org_name_file.StartsWith("\"") && org_name_file.EndsWith("\""))
                                {
                                    org_name_file = org_name_file.Trim('"');
                                }
                                if (org_name_file.Contains(@"/") || org_name_file.Contains(@"\"))
                                {
                                    org_name_file = System.IO.Path.GetFileName(org_name_file);
                                }
                                string name_file = "";
                                if (org_name_file.Length > 500)
                                {
                                    name_file = helper.UniqueFileName(org_name_file);
                                }
                                else
                                {
                                    name_file = (org_name_file);
                                }
                                string rootPath = path + "/" + name_file;
                                string Duongdan = "/Portals/" + task.organization_id + "/TaskOrigin/" + task.task_id + "/" + name_file;

                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName).Replace("\"", "");
                                if (rootPath.Length > 500)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 500 - (path.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                check += rootPath;
                                File.Move(fileData.LocalFileName, rootPath);
                                File.Delete(fileData.LocalFileName);
                                //File.Copy(fileData.LocalFileName, rootPathFile, true);
                                var df = new task_file();
                                df.file_id = helper.GenKey();
                                df.task_id = report;
                                df.project_id = null;
                                df.file_name = name_file;
                                df.file_path = Duongdan;
                                df.file_type = "." + Dinhdang;
                                var file_info = new FileInfo(rootPath);
                                df.file_size = file_info.Length;
                                df.is_image = helper.IsImageFileName(name_file);
                                if (df.is_image == true)
                                {
                                    //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                }
                                df.is_type = 1;
                                df.status = true;
                                df.created_by = uid;
                                df.created_date = DateTime.Now;
                                df.created_ip = ip;
                                df.created_token_id = tid;
                                dfs.Add(df);
                            }
                            if (dfs.Count > 0)
                            {
                                db.task_file.AddRange(dfs);
                            }
                            #endregion
                        }
                        db.SaveChanges();
                        //notify
                        var listuser = db.task_member.Where(x => x.task_id == task.task_id).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == task.task_id).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);
                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Thêm tệp tài liệu công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, task.task_id, null, null, tid, ip);
                        }

                        #region add tasklog
                        task_logs log = new task_logs();
                        log.log_id = helper.GenKey();
                        log.task_id = report;
                        log.description = fname + " đã thêm tệp tài liệu";
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.task_logs.Add(log);
                        db.SaveChanges();
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_Files/add_Task_File", ip, tid, "Lỗi khi thêm tệp tài liệu", 0, "task_Files");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_Files/add_Task_File", ip, tid, "Lỗi khi thêm tệp tài liệu", 0, "task_Files");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> delete_Task_File([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;


            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {

                using (DBEntities db = new DBEntities())
                {
                    List<task_file> delTask = new List<task_file>();


                    var das = await db.task_file.Where(t => id.Contains(t.file_id)).ToListAsync();
                    foreach (var de in das)
                    {
                        delTask.Add(de);
                    }
                    if (delTask.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                    }
                    if (delTask.Count > 0)
                    { db.task_file.RemoveRange(delTask); }
                    string root = HttpContext.Current.Server.MapPath("~/");
                    foreach (var task_file in delTask)
                    {
                        var listPath = task_file.file_path.Split('/');
                        var pathConfig = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfig += Path.GetFileName(item);
                                }
                                else
                                {
                                    pathConfig += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        if (File.Exists(root + pathConfig))
                        {
                            File.Delete(root + pathConfig);
                            #region add task_logs
                            if (helper.wlog)
                            {

                                task_logs log = new task_logs();
                                log.log_id = helper.GenKey();
                                log.task_id = task_file.task_id;
                                log.project_id = null;
                                log.description = fname + " xóa tệp tài liệu";
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.task_logs.Add(log);
                                db.SaveChanges();
                            }
                            #endregion
                        }
                    }
                    string ssid = delTask[0].task_id;

                    var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                    string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                    listuser.Remove(uid);
                    foreach (var l in listuser)
                    {
                        helper.saveNotify(uid, l, null, "Công việc", "Xóa tệp tài liệu công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                            null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_Files/delete_Task_File", ip, tid, "Lỗi khi xoá tệp tài liệu", 0, "task_Files");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_Files/delete_Task_File", ip, tid, "Lỗi khi xoá tệp tài liệu", 0, "task_Files");
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
