using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
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
using API.Models;
using Helper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using OfficeOpenXml;
using API.Helper;
namespace API.Controllers.Task_Origin1
{
    [Authorize(Roles = "login")]
    public class ReportProgressController : ApiController
    {
        string module_key = "M4";
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> addReportProgress()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string report = "";

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
                    string strPath = root + "/Task_Files";
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
                        report = provider.FormData.GetValues("comment").SingleOrDefault();
                        task_reportprogress cmtbug = JsonConvert.DeserializeObject<task_reportprogress>(report);
                        cmtbug.report_id = helper.GenKey();
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        var task = db.task_origin.AsNoTracking().Where(x => x.task_id == cmtbug.task_id).FirstOrDefault();
                        var file = provider.FileData;

                        if (file.Count > 0)
                        {
                            #region file
                            string path = root + "/" + task.organization_id + "/TaskOrigin/" + task.task_id + "/" + cmtbug.report_id;

                            // Format path
                            var pathFormat = Regex.Replace(path.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = pathFormat.Split('/');
                            var pathConfig = "";
                            var sttPartPath = 1;
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfig += (item);
                                    }
                                    else
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            bool exists = Directory.Exists(pathConfig);
                            if (!exists)
                                Directory.CreateDirectory(pathConfig);
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
                                string rootPath = pathConfig + "/" + name_file;
                                string Duongdan = "/Portals/" + task.organization_id + "/TaskOrigin/" + task.task_id + "/" + cmtbug.report_id + "/" + name_file;

                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName).Replace("\"", "");
                                if (rootPath.Length > 500)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 500 - (pathConfig.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                File.Move(fileData.LocalFileName, rootPath);
                                File.Delete(fileData.LocalFileName);
                                //File.Copy(fileData.LocalFileName, rootPathFile, true);
                                var df = new task_file();
                                df.file_id = helper.GenKey();
                                df.report_id = cmtbug.report_id;
                                df.project_id = null;
                                df.file_name = name_file;
                                df.file_path = Duongdan;
                                df.file_type = Dinhdang;
                                df.task_id = task.task_id;
                                var file_info = new FileInfo(rootPath);
                                df.file_size = file_info.Length;
                                df.is_image = helper.IsImageFileName(name_file);
                                if (df.is_image == true)
                                {
                                    //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                }
                                df.is_type = 3;
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

                        var chag = db.task_origin.Where(x => x.task_id == cmtbug.task_id).FirstOrDefault();

                        cmtbug.created_by = uid;
                        cmtbug.created_date = DateTime.Now;
                        cmtbug.created_by = uid;
                        cmtbug.created_ip = ip;
                        cmtbug.created_token_id = tid;

                        var members = db.task_member.Where(x => x.task_id == cmtbug.task_id && x.is_type == 0).Select(x => x.user_id).ToList();
                        members.Add(task.created_by);
                        if (members.Contains(cmtbug.created_by))
                        {
                            cmtbug.status = 1;
                            chag.progress = cmtbug.progress;
                            if (cmtbug.request_progress == 100)
                            {
                                cmtbug.progress = 100;
                                if (chag.end_date == null || chag.end_date <= DateTime.Now)
                                {
                                    chag.status = 4;
                                    chag.end_real_date = DateTime.Now;
                                }
                                else
                                {
                                    chag.status = 7;
                                    chag.end_real_date = DateTime.Now;
                                }
                            }
                        }
                        else { chag.status = 5; }
                        db.Entry(chag).State = EntityState.Modified;

                        db.task_reportprogress.Add(cmtbug);

                        db.SaveChanges();
                        #region add notify
                        var listuser = db.task_member.Where(x => x.task_id == chag.task_id).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == chag.task_id).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);
                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Đã báo cáo tiến độ công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, chag.task_id, null, null, tid, ip);
                        }
                        #endregion
                        #region add tasklog
                        task_logs log = new task_logs();
                        log.log_id = helper.GenKey();
                        log.task_id = cmtbug.task_id;
                        log.report_id = cmtbug.report_id;
                        log.description = "Thêm báo cáo công việc ";
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.task_logs.Add(log);
                        db.SaveChanges();
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ReportProgress/addReportProgress", ip, tid, "Lỗi khi thêm báo cáo công việc", 0, "ReportProgress");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ReportProgress/addReportProgress", ip, tid, "Lỗi khi thêm báo cáo công việc", 0, "ReportProgress");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
    }
}
