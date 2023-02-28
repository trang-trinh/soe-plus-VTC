using API.Models;
using Helper;
using Newtonsoft.Json;
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
using System.Configuration;
using System.Globalization;
using Newtonsoft.Json.Linq;
using API.Controllers;
using API.Helper;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class task_originController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add_TaskOrigin()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdtask = "";
            string fdtaskmember;
            string fdXML = "";
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
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string rootXML = HttpContext.Current.Server.MapPath("~/");
                    //string strPath = root + "/" + task_origin.organization_id + "/TaskOrigin/" + task_origin.task_id;
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
                        fdtask = provider.FormData.GetValues("taskOrigin").SingleOrDefault();
                        fdtaskmember = provider.FormData.GetValues("taskmember").SingleOrDefault();
                        fdXML = provider.FormData.GetValues("isXML").SingleOrDefault();
                        task_origin task_origin = JsonConvert.DeserializeObject<task_origin>(fdtask);
                        List<task_member> members = JsonConvert.DeserializeObject<List<task_member>>(fdtaskmember);

                        task_origin.task_id = helper.GenKey();
                        task_origin.task_name_en = helper.convertToUnSign3(task_origin.task_name).ToLower();
                        task_origin.created_date = DateTime.Now;
                        task_origin.created_by = uid;
                        task_origin.modified_by = uid;
                        task_origin.modified_date = DateTime.Now;
                        task_origin.created_ip = ip;
                        task_origin.created_token_id = tid;
                        db.task_origin.Add(task_origin);

                        var file = provider.FileData;

                        if (file.Count > 0)
                        {
                            #region file
                            string path = root + "/" + task_origin.organization_id + "/TaskOrigin/" + task_origin.task_id;
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
                                string Duongdan = "/Portals/" + task_origin.organization_id + "/TaskOrigin/" + task_origin.task_id + "/" + name_file;
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
                                //File.Copy(fileData.LocalFileName, rootPathFile, true);
                                var df = new task_file();
                                df.file_id = helper.GenKey();
                                df.task_id = task_origin.task_id;
                                df.project_id = null;
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

                        #region add task_member
                        if (members.Count > 0)
                        {
                            List<task_member> listmems = new List<task_member>();
                            foreach (var item in members)
                            {
                                task_member member = new task_member
                                {
                                    member_id = helper.GenKey(),
                                    project_id = null,
                                    task_id = task_origin.task_id,
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

                        #region add task_logs
                        if (helper.wlog)
                        {
                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = task_origin.task_id;
                            log.project_id = null;
                            log.description = "Thêm công việc: " + task_origin.task_name;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();
                        }
                        #endregion

                        #region add sendhub
                        if (helper.wlog)
                        {
                            if (members.Count > 0)
                            {
                                List<sys_sendhub> listsendhubs = new List<sys_sendhub>();
                                var contentNoti = "Thêm công việc: " + (task_origin.task_name.Length > 100 ? task_origin.task_name.Substring(0, 97) + "..." : task_origin.task_name);
                                foreach (var item in members)
                                {
                                    if(listsendhubs.Where(x=>x.receiver == item.user_id).ToList().Count == 0 && item.user_id != uid)
                                    {
                                        var ns_sh = db.sys_users.Find(item.user_id);
                                        var created_by = db.sys_users.Find(uid);

                                        var sh = new sys_sendhub();
                                        sh.senhub_id = helper.GenKey();
                                        sh.user_send = uid;
                                        sh.module_key = "M4";
                                        sh.receiver = ns_sh.user_id;
                                        sh.icon = ns_sh.avatar;
                                        sh.title = "Công việc";
                                        sh.contents = contentNoti;
                                        sh.type = 2;
                                        sh.is_type = -1;
                                        sh.date_send = DateTime.Now;
                                        sh.id_key = task_origin.task_id.ToString();
                                        //sh.group_id = task_origin.group_id;
                                        sh.token_id = tid;
                                        sh.created_date = DateTime.Now;
                                        sh.created_by = uid;
                                        sh.created_token_id = tid;
                                        sh.created_ip = ip;

                                        if (bool.Parse(fdXML) == true)
                                        {
                                            string xml_result = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>";
                                            xml_result += "<document>";
                                            xml_result += "<element>";
                                            xml_result += "<taskid>" + (task_origin.task_id ?? "") + "</taskid>";
                                            xml_result += "<taskname>" + (task_origin.task_name ?? "") + "</taskname>";
                                            xml_result += "<created_by>" + (created_by.full_name ?? "") + "</created_by>";
                                            xml_result += "<receiver>" + (ns_sh.full_name ?? "") + "</receiver>";
                                            xml_result += "<created_date>" + (sh.created_date) + "</created_date>";
                                            xml_result += "<contents>" + (sh.contents) + "</contents>";
                                            xml_result += "</element>";
                                            xml_result += "</document>";

                                            var user_now = db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                                            System.Net.WebClient webc = new System.Net.WebClient();
                                            string path = rootXML + helper.path_xml + "/TaskOrigin/";
                                            bool exists = Directory.Exists(path);
                                            if (!exists)
                                                Directory.CreateDirectory(path);

                                            string name_file = task_origin.task_id + ".xml";
                                            string root_path = path + "/" + name_file;
                                            string duong_dan = helper.path_xml + "/TaskOrigin/" + name_file;
                                            string url = ConfigurationManager.AppSettings["ValidAudience"] + duong_dan;

                                            File.WriteAllText(root_path, xml_result);
                                            var res_encr = helper.encryptXML(root_path, "document", helper.psKey);
                                            if (res_encr != "OK")
                                            {
                                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Không thể mã hoã file XML!", err = "1" });
                                            };
                                        }

                                        listsendhubs.Add(sh);
                                    }
                                }
                                if (listsendhubs.Count > 0)
                                {
                                    db.sys_sendhub.AddRange(listsendhubs);

                                    #region sendSocket
                                    var users = listsendhubs.Where(x => x.receiver != uid).Select(x => x.receiver).Distinct().ToList();
                                    var message = new Dictionary<string, dynamic>
                                    {
                                        { "event", "sendNotify" },
                                        { "user_id", uid },
                                        { "title", "Công việc" },
                                        { "contents", contentNoti },
                                        { "date", DateTime.Now },
                                        { "uids", users },
                                    };
                                    if (helper.socketClient != null && helper.socketClient.Connected == true)
                                    {
                                        try
                                        {
                                            helper.socketClient.EmitAsync("sendData", message);
                                        }
                                        catch { };
                                    }
                                    #endregion
                                }
                            }
                        }
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtask, contents }), domainurl + "task_origin/Add_task", ip, tid, "Lỗi khi thêm TaskOrigin", 0, "TaskOrigin");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtask, contents }), domainurl + "task_origin/Add_task", ip, tid, "Lỗi khi thêm TaskOrigin", 0, "TaskOrigin");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_TaskOrigin()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdtask = ""; 
            string fdtaskmember = "";
            string fdXML = "";
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
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string rootXML = HttpContext.Current.Server.MapPath("~/");
                    //string strPath = root + "/TaskOrigin";
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
                        fdtask = provider.FormData.GetValues("taskOrigin").SingleOrDefault();
                        fdtaskmember = provider.FormData.GetValues("taskmember").SingleOrDefault();
                        fdXML = provider.FormData.GetValues("isXML").SingleOrDefault();
                        task_origin task_origin = JsonConvert.DeserializeObject<task_origin>(fdtask);
                        List<task_member> members = JsonConvert.DeserializeObject<List<task_member>>(fdtaskmember);

                        task_origin.task_name_en = helper.convertToUnSign3(task_origin.task_name).ToLower(); ;
                        task_origin.modified_date = DateTime.Now;
                        task_origin.modified_by = uid;
                        task_origin.modified_date = DateTime.Now;
                        task_origin.modified_ip = ip;
                        task_origin.modified_token_id = tid; List<task_origin> list_Mod = new List<task_origin>();
                        #region đóng công việc con nếu đóng công việc cha
                        if (task_origin.status == 3)
                        {
                            DateTime end_date = DateTime.Now;
                            int stt = 3;
                            List<string> list_status = new List<string>() { "Chưa bắt đầu", "đang làm", "Tạm ngừng", "Đã đóng", "Hoàn thành đúng hạn", "Đợi Review", "Bị trả lại", "Hoàn thành sau hạn", "đã review" };
                            List<string> list_task_orgin = new List<string>();
                            list_task_orgin.Add(task_origin.task_id);
                            if (stt == 3)
                            {
                                var temp = findChild(list_task_orgin);
                                if (temp != null)
                                {
                                    foreach (var id in temp)
                                    { list_task_orgin.Add(id); }

                                }
                            }

                            list_task_orgin = list_task_orgin.Distinct().ToList();
                            list_task_orgin.Remove(task_origin.task_id);
                            if (list_task_orgin != null)
                            {
                                foreach (var task in list_task_orgin)
                                {
                                    var task_orgin = db.task_origin.Where(x => x.task_id == task).FirstOrDefault();
                                    if (task_orgin.status == stt)
                                    {
                                        task_orgin.status = stt;
                                    }
                                    else
                                    {
                                        task_orgin.status = stt;
                                        if (stt == 4 || stt == 7)
                                        {
                                            task_orgin.end_real_date = end_date;
                                            task_orgin.finish_date = end_date;
                                        }
                                        else if (stt == 3)
                                        {
                                            task_orgin.close_date = DateTime.Now;
                                            task_orgin.close_by = uid;
                                        }
                                        else
                                        {
                                            task_orgin.end_real_date = null;
                                            task_orgin.finish_date = null;
                                            task_orgin.close_date = null;
                                            task_orgin.close_by = null;
                                        }

                                        #region add task_logs
                                        if (helper.wlog)
                                        {
                                            task_logs log = new task_logs();
                                            log.log_id = helper.GenKey();
                                            log.task_id = task_orgin.task_id;
                                            log.project_id = null;
                                            log.description = "đã sửa trạng thái công việc: " + list_status[stt];
                                            log.created_date = DateTime.Now;
                                            log.created_by = uid;
                                            log.created_token_id = tid;
                                            log.created_ip = ip;
                                            db.task_logs.Add(log);
                                            db.SaveChanges();
                                        }
                                        #endregion
                                        list_Mod.Add(task_orgin);
                                    };
                                }
                            }
                        }
                        #endregion
                        list_Mod.Add(task_origin);
                        foreach (var task in list_Mod)
                        {
                            db.Entry(task).State = EntityState.Modified;
                        }
                        var file = provider.FileData;

                        if (file.Count > 0)
                        {
                            #region file
                            string path = root + "/" + task_origin.organization_id + "/TaskOrigin/" + task_origin.task_id;
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
                                string Duongdan = "/Portals/" + task_origin.organization_id + "/TaskOrigin/" + task_origin.task_id + "/" + name_file;
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
                                //File.Copy(fileData.LocalFileName, rootPathFile, true);
                                var df = new task_file();
                                df.file_id = helper.GenKey();
                                df.task_id = task_origin.task_id;
                                df.project_id = null;
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

                        #region update task_member
                        List<task_member> del_member = new List<task_member>();
                        var model_del_members = db.task_member.Where(a => a.task_id == task_origin.task_id).ToList();
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
                                    project_id = null,
                                    task_id = task_origin.task_id,
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

                        #region add task_logs
                        if (helper.wlog)
                        {
                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = task_origin.task_id;
                            log.project_id = null;
                            log.description = "Sửa công việc: " + task_origin.task_name;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();
                        }
                        #endregion

                        #region add sendhub
                        if (helper.wlog)
                        {
                            if (members.Count > 0)
                            {
                                List<sys_sendhub> listsendhubs = new List<sys_sendhub>();
                                var contentNoti = "Sửa công việc: " + (task_origin.task_name.Length > 100 ? task_origin.task_name.Substring(0, 97) + "..." : task_origin.task_name);
                                foreach (var item in members)
                                {
                                    if (listsendhubs.Where(x => x.receiver == item.user_id).ToList().Count == 0 && item.user_id != uid)
                                    {
                                        var ns_sh = db.sys_users.Find(item.user_id);
                                        var created_by = db.sys_users.Find(uid);

                                        var sh = new sys_sendhub();
                                        sh.senhub_id = helper.GenKey();
                                        sh.user_send = uid;
                                        sh.module_key = "M4";
                                        sh.receiver = ns_sh.user_id;
                                        sh.icon = ns_sh.avatar;
                                        sh.title = "Công việc";
                                        sh.contents = contentNoti;
                                        sh.type = 2;
                                        sh.is_type = -1;
                                        sh.date_send = DateTime.Now;
                                        sh.id_key = task_origin.task_id.ToString();
                                        //sh.group_id = task_origin.group_id;
                                        sh.token_id = tid;
                                        sh.created_date = DateTime.Now;
                                        sh.created_by = uid;
                                        sh.created_token_id = tid;
                                        sh.created_ip = ip;

                                        if (bool.Parse(fdXML) == true)
                                        {
                                            string xml_result = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>";
                                            xml_result += "<document>";
                                            xml_result += "<element>";
                                            xml_result += "<taskid>" + (task_origin.task_id ?? "") + "</taskid>";
                                            xml_result += "<taskname>" + (task_origin.task_name ?? "") + "</taskname>";
                                            xml_result += "<created_by>" + (created_by.full_name ?? "") + "</created_by>";
                                            xml_result += "<receiver>" + (ns_sh.full_name ?? "") + "</receiver>";
                                            xml_result += "<created_date>" + (sh.created_date) + "</created_date>";
                                            xml_result += "<contents>" + (sh.contents) + "</contents>";
                                            xml_result += "</element>";
                                            xml_result += "</document>";

                                            var user_now = db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                                            System.Net.WebClient webc = new System.Net.WebClient();
                                            string path = rootXML + helper.path_xml + "/TaskOrigin/";
                                            bool exists = Directory.Exists(path);
                                            if (!exists)
                                                Directory.CreateDirectory(path);

                                            string name_file = task_origin.task_id + ".xml";
                                            string root_path = path + "/" + name_file;
                                            string duong_dan = helper.path_xml + "/TaskOrigin/" + name_file;
                                            string url = ConfigurationManager.AppSettings["ValidAudience"] + duong_dan;

                                            File.WriteAllText(root_path, xml_result);
                                            var res_encr = helper.encryptXML(root_path, "document", helper.psKey);
                                            if (res_encr != "OK")
                                            {
                                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Không thể mã hoã file XML!", err = "1" });
                                            };
                                        }

                                        listsendhubs.Add(sh);
                                    }
                                }
                                if (listsendhubs.Count > 0)
                                {
                                    db.sys_sendhub.AddRange(listsendhubs);

                                    #region sendSocket
                                    var users = listsendhubs.Where(x => x.receiver != uid).Select(x => x.receiver).Distinct().ToList();
                                    var message = new Dictionary<string, dynamic>
                                    {
                                        { "event", "sendNotify" },
                                        { "user_id", uid },
                                        { "title", "Công việc" },
                                        { "contents", contentNoti },
                                        { "date", DateTime.Now },
                                        { "uids", users },
                                    };
                                    if (helper.socketClient != null && helper.socketClient.Connected == true)
                                    {
                                        try
                                        {
                                            helper.socketClient.EmitAsync("sendData", message);
                                        }
                                        catch { };
                                    }
                                    #endregion
                                }
                            }
                        }
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtask, contents }), domainurl + "task_origin/Add_task", ip, tid, "Lỗi khi thêm TaskOrigin", 0, "TaskOrigin");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtask, contents }), domainurl + "task_origin/Add_task", ip, tid, "Lỗi khi thêm TaskOrigin", 0, "TaskOrigin");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_task_origin([System.Web.Mvc.Bind(Include = "")] List<string> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
            //try
            //{
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
                        var das = await db.task_origin.Where(a => ids.Contains(a.task_id)).ToListAsync();
                        int count = 0;
                        foreach (var da in das)
                        {
                            var rp = db.task_person_report.Where(x => x.list_task_id.Contains(da.task_id)).ToList();
                            if (rp.Count > 0)
                            {
                                count++;
                            }
                        }
                        if (count > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Công việc đang được báo cáo ở Đánh giá công việc!<br/>Không thể xóa!" });
                        }
                        if (das != null)
                        {
                            List<task_origin> del = new List<task_origin>();
                            List<task_member> del_member = new List<task_member>();
                            List<task_file> del_file = new List<task_file>();
                            List<task_reportprogress> del_reportprogress = new List<task_reportprogress>();
                            List<task_checklists> del_checklists = new List<task_checklists>();
                            List<task_comments> del_comments = new List<task_comments>();
                            List<task_review> del_review = new List<task_review>();
                            List<string> paths = new List<string>();
                            foreach (var da in das)
                            {
                                del.Add(da);

                                #region del member
                                var members = await db.task_member.Where(a => a.task_id == da.task_id).ToListAsync();
                                if (members.Count > 0)
                                {
                                    foreach (var m in members)
                                    {
                                        del_member.Add(m);
                                    }
                                }
                                #endregion

                                #region del file
                                var files = await db.task_file.Where(a => a.task_id == da.task_id).ToListAsync();
                                if (files.Count > 0)
                                {
                                    foreach (var f in files)
                                    {
                                        del_file.Add(f);
                                        paths.Add(root + f.file_path);
                                    }
                                }
                                #endregion

                                #region del reportprogress
                                var reportprogress = await db.task_reportprogress.Where(a => a.task_id == da.task_id).ToListAsync();
                                if (reportprogress.Count > 0)
                                {
                                    foreach (var f in reportprogress)
                                    {
                                        del_reportprogress.Add(f);
                                    }
                                }
                                #endregion

                                #region del checklists
                                var checklists = await db.task_checklists.Where(a => a.task_id == da.task_id).ToListAsync();
                                if (checklists.Count > 0)
                                {
                                    foreach (var f in checklists)
                                    {
                                        del_checklists.Add(f);
                                    }
                                }
                                #endregion

                                #region del comments
                                var comments = await db.task_comments.Where(a => a.task_id == da.task_id).ToListAsync();
                                if (comments.Count > 0)
                                {
                                    foreach (var f in comments)
                                    {
                                        del_comments.Add(f);
                                    }
                                }
                                #endregion

                                #region del review
                                var review = await db.task_review.Where(a => a.task_id == da.task_id).ToListAsync();
                                if (review.Count > 0)
                                {
                                    foreach (var f in review)
                                    {
                                        del_review.Add(f);
                                    }
                                }
                                #endregion

                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    cms_logs log = new cms_logs();
                                    log.log_title = "Xóa công việc: " + da.group_id;
                                    log.log_module = "Task Origin";
                                    log.id_key = da.group_id.ToString();
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.cms_logs.Add(log);
                                    db.SaveChanges();
                                }
                            #endregion

                            #region add sendhub
                            if (helper.wlog)
                            {
                                if (members.Count > 0)
                                {
                                    List<sys_sendhub> listsendhubs = new List<sys_sendhub>();
                                    foreach (var item in members)
                                    {
                                        if (listsendhubs.Where(x => x.receiver == item.user_id).ToList().Count == 0 && item.user_id != uid)
                                        {
                                            var ns_sh = db.sys_users.Find(item.user_id);

                                            var sh = new sys_sendhub();
                                            sh.senhub_id = helper.GenKey();
                                            sh.user_send = uid;
                                            sh.module_key = "M4";
                                            sh.receiver = ns_sh.user_id;
                                            sh.icon = ns_sh.avatar;
                                            sh.title = "Công việc";
                                            sh.contents = "Xóa công việc: " + (da.task_name.Length > 100 ? da.task_name.Substring(0, 97) + "..." : da.task_name);
                                            sh.type = 2;
                                            sh.is_type = -1;
                                            sh.date_send = DateTime.Now;
                                            sh.id_key = da.task_id.ToString();
                                            //sh.group_id = task_origin.group_id;
                                            sh.token_id = tid;
                                            sh.created_date = DateTime.Now;
                                            sh.created_by = uid;
                                            sh.created_token_id = tid;
                                            sh.created_ip = ip;
                                            listsendhubs.Add(sh);
                                        }
                                    }
                                    if (listsendhubs.Count > 0)
                                    {
                                        db.sys_sendhub.AddRange(listsendhubs);
                                    }
                                }
                            }
                            #endregion
                        }
                        if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.task_origin.RemoveRange(del);
                            db.task_member.RemoveRange(del_member);
                            db.task_file.RemoveRange(del_file);
                            db.task_reportprogress.RemoveRange(del_reportprogress);
                            db.task_checklists.RemoveRange(del_checklists);
                            db.task_comments.RemoveRange(del_comments);
                            db.task_review.RemoveRange(del_review);
                            foreach (string strPath in paths)
                            {
                                //bool exists = File.Exists(strPath);
                                //if (exists)
                                //{
                                //    System.IO.File.Delete(strPath);
                                //}
                                // Format logo

                                var listPathEdit_logo = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
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
                                bool exists = File.Exists(pathEdit_logo);
                                if (exists)
                                {
                                    System.IO.File.Delete(pathEdit_logo);
                                }
                            }
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Task_Ca_ProjectGroup/Delete_task_ca_projectgroup", ip, tid, "Lỗi khi xoá nhóm project", 0, "task_ca_projectgroup");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Task_Ca_ProjectGroup/Delete_task_ca_projectgroup", ip, tid, "Lỗi khi xoá nhóm project", 0, "task_ca_projectgroup");
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
        public async Task<HttpResponseMessage> Add_LinkTask([System.Web.Mvc.Bind(Include = "")] List<task_origin> list)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
            //try
            //{
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
                        if (list.Count > 0)
                        {
                            foreach (var l in list)
                            {
                                var task = await db.task_origin.FindAsync(l.task_id);
                                if (task.parent_id == null)
                                {
                                    task.parent_id = l.parent_id;
                                }
                                db.Entry(task).State = EntityState.Modified;
                            }
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = list, contents }), domainurl + "task_origin/Add_LinkTask", ip, tid, "Lỗi khi thêm liên kết công việc", 0, "task_origin");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = list, contents }), domainurl + "task_origin/Add_LinkTask", ip, tid, "Lỗi khi thêm liên kết công việc", 0, "task_origin");
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
        public async Task<HttpResponseMessage> Add_LinkTask_Doc([System.Web.Mvc.Bind(Include ="")] List<task_linkdoc> list)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
            //try
            //{
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
                        List<task_linkdoc> listTaskLinks = new List<task_linkdoc>();
                        if (list.Count > 0)
                        {
                            foreach (var l in list)
                            {
                                task_linkdoc linktask = new task_linkdoc
                                {
                                    task_id = l.task_id,
                                    is_main = l.is_main,
                                    organization_id = l.organization_id,
                                    doc_master_id = l.doc_master_id,
                                    created_date = DateTime.Now,
                                    created_by = uid,
                                    modified_by = uid,
                                    modified_date = DateTime.Now,
                                    created_ip = ip,
                                    created_token_id = tid,
                                };
                                listTaskLinks.Add(linktask);
                            }
                        }
                        if (listTaskLinks.Count > 0)
                        {
                            db.task_linkdoc.AddRange(listTaskLinks);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = list, contents }), domainurl + "task_origin/Add_LinkTask_Doc", ip, tid, "Lỗi khi thêm liên kết công việc", 0, "task_origin");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = list, contents }), domainurl + "task_origin/Add_LinkTask_Doc", ip, tid, "Lỗi khi thêm liên kết công việc", 0, "task_origin");
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

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_task_linkdoc([System.Web.Mvc.Bind(Include = "")] List<int> ids)
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
                        var das = await db.task_linkdoc.Where(a => ids.Contains(a.linkdoc_id)).ToListAsync();
                        if (das.Count > 0)
                        {
                            List<task_linkdoc> del = new List<task_linkdoc>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa file này." });
                            }
                            db.task_linkdoc.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "task_origin/Delete_task_linkdoc", ip, tid, "Lỗi khi xoá liên kết công việc", 0, "Delete_task_linkdoc");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "task_origin/Delete_task_linkdoc", ip, tid, "Lỗi khi xoá liên kết công việc", 0, "Delete_task_linkdoc");
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

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_task_link([System.Web.Mvc.Bind(Include = "")] List<string> ids)
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
                        var das = await db.task_origin.Where(a => ids.Contains(a.task_id)).ToListAsync();
                        if (das.Count > 0)
                        {
                            foreach (var da in das)
                            {
                                da.parent_id = null;
                                db.Entry(da).State = EntityState.Modified;
                            }
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "task_origin/Delete_task_linkdoc", ip, tid, "Lỗi khi xoá liên kết công việc", 0, "Delete_task_linkdoc");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "task_origin/Delete_task_linkdoc", ip, tid, "Lỗi khi xoá liên kết công việc", 0, "Delete_task_linkdoc");
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
        public async Task<HttpResponseMessage> Delete_file([System.Web.Mvc.Bind(Include = "")] List<string> ids)
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
                        var das = await db.task_file.Where(a => ids.Contains(a.file_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das.Count > 0)
                        {
                            List<task_file> del = new List<task_file>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                                var task = db.task_origin.Find(da.task_id);
                                var delPath = Path.Combine(HttpContext.Current.Server.MapPath("~/" + "/Portals/"), Path.GetFileName(task.organization_id.ToString()), "TaskOrigin", Path.GetFileName(da.task_id.ToString()), Path.GetFileName(da.file_path));
                                paths.Add(delPath);
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa file này." });
                            }
                            db.task_file.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strPath in paths)
                        {
                            if (File.Exists(strPath))
                                File.Delete(strPath);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "task_origin/Delete_file", ip, tid, "Lỗi khi xoá file", 0, "Delete_file");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "task_origin/Delete_file", ip, tid, "Lỗi khi xoá file", 0, "Delete_file");
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

        public List<string> findChild([System.Web.Mvc.Bind(Include = "")] List<string> id)
        {
            List<string> del = new List<string>();
            using (DBEntities db = new DBEntities())
            {
                var das = db.task_origin.Where(a => id.Contains(a.task_id)).ToArray();
                if (das != null)
                {
                    foreach (var da in das)
                    {
                        var arrC = db.task_origin.Where(a => a.parent_id == da.task_id).ToArray();
                        del.Add(da.task_id);
                        var arrId = new List<string>();
                        for (int i = 0; i < id.Count; i++)
                        {
                            for (int j = 0; j < arrC.Length; j++)
                            {
                                if (id[i] == arrC[j].parent_id)
                                {
                                    arrId.Add(arrC[j].task_id);
                                    del.AddRange(findChild(arrId));
                                }
                            }
                        }
                    }
                }
            }
            return del;
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Status_TaskOrigin([System.Web.Mvc.Bind(Include = "task_id,stt,end_date")] JObject data)
        {
            string task_id = data["task_id"].ToObject<string>();
            int stt = data["stt"].ToObject<int>();
            string date = data["end_date"].ToObject<string>() != null ? data["end_date"].ToObject<string>() : null;
            DateTime end_date = new DateTime();
            if (date != null)
            {
                end_date = Convert.ToDateTime(date, new CultureInfo("en-US", true));
            }
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
                using (DBEntities db = new DBEntities())
                {
                    List<string> list_status = new List<string>() { "Chưa bắt đầu", "đang làm", "Tạm ngừng", "Đã đóng", "Hoàn thành đúng hạn", "Đợi Review", "Bị trả lại", "Hoàn thành sau hạn", "đã review" };
                    List<string> list_task_orgin = new List<string>();
                    list_task_orgin.Add(task_id);
                    if (stt == 3)
                    {
                        var temp = findChild(list_task_orgin);
                        if (temp != null)
                        {
                            foreach (var id in temp)
                            { list_task_orgin.Add(id); }

                        }
                    }
                    foreach (var task in list_task_orgin)
                    {
                        var task_orgin = db.task_origin.Where(x => x.task_id == task).FirstOrDefault();
                        if (task_orgin.status == stt)
                        {
                            task_orgin.status = stt;
                        }
                        else
                        {
                            task_orgin.status = stt;
                            if (stt == 4 || stt == 7)
                            {
                                task_orgin.end_real_date = end_date;
                                task_orgin.finish_date = end_date;
                            }
                            else if (stt == 3)
                            {
                                task_orgin.close_date = DateTime.Now;
                                task_orgin.close_by = uid;
                            }
                            else
                            {
                                task_orgin.end_real_date = null;
                                task_orgin.finish_date = null;
                                task_orgin.close_date = null;
                                task_orgin.close_by = null;
                            }

                            #region add task_logs
                            if (helper.wlog)
                            {
                                task_logs log = new task_logs();
                                log.log_id = helper.GenKey();
                                log.task_id = task_orgin.task_id;
                                log.project_id = null;
                                log.description = "đã sửa trạng thái công việc: " + list_status[stt];
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.task_logs.Add(log);
                                db.SaveChanges();
                            }
                            #endregion
                            db.Entry(task_orgin).State = EntityState.Modified;
                        };
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });

                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_origin/Add_task", ip, tid, "Lỗi khi thêm TaskOrigin", 0, "TaskOrigin");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_origin/Add_task", ip, tid, "Lỗi khi thêm TaskOrigin", 0, "TaskOrigin");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Update_DepartmentConfiguration()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fddepartment = "";
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
                    string strPath = root + "/TaskOrigin";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var department = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fddepartment = provider.FormData.GetValues("department").SingleOrDefault();
                        task_department_configuration task_department_configuration = JsonConvert.DeserializeObject<task_department_configuration>(fddepartment);
                        var task_config = db.task_department_configuration.Where(x=> x.department_id == task_department_configuration.department_id).ToList();
                        if (task_config.Count == 0)
                        {
                            db.task_department_configuration.Add(task_department_configuration);
                        }
                        else
                        {
                            foreach (var item in task_config)
                            {
                                item.user_id = task_department_configuration.user_id;
                                db.Entry(item).State = EntityState.Modified;
                            }
                        }
                        
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await department;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fddepartment, contents }), domainurl + "task_origin/Update_Department", ip, tid, "Lỗi khi thêm Update_Department", 0, "TaskOrigin");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fddepartment, contents }), domainurl + "task_origin/Update_Department", ip, tid, "Lỗi khi thêm Update_Department", 0, "TaskOrigin");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_DepartmentConfiguration([System.Web.Mvc.Bind(Include = "")] List<int> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //try
            //{
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
                        var das = await db.task_department_configuration.Where(a => ids.Contains(a.id)).ToListAsync();
                        if (das != null)
                        {
                            List<task_department_configuration> del = new List<task_department_configuration>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.task_department_configuration.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "task_origin/Delete_DepartmentConfiguration", ip, tid, "Lỗi khi xoá cấu hình phòng ban", 0, "task_origin");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "task_origin/Delete_DepartmentConfiguration", ip, tid, "Lỗi khi xoá cấu hình phòng ban", 0, "task_origin");
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

        public class task_report
        {
            public string task_id { get; set; }
            public string task_name { get; set; }
            public string project_name { get; set; }
            public string Tennhom { get; set; }
            public string organization_name { get; set; }
            public string target { get; set; }
            public string progress { get; set; }
            public string status_name { get; set; }
            //public string start_date { get; set; }
            //public string end_date { get; set; }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> TaskDepartment_export_xml([System.Web.Mvc.Bind(Include = "")][FromBody] List<task_report> list)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //string dataProc = data["str"].ToObject<string>();
            //string des = Codec.DecryptString(dataProc, helper.psKey);
            //sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);
            try
            {
                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

                try
                {
                    string xml_result = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>";
                    if (list != null && list.Count > 0)
                    {
                        xml_result += "<document>";
                        foreach (var duty in list)
                        {
                            xml_result += "<element>";
                            xml_result += "<taskid>" + (duty.task_id ?? "") + "</taskid>";
                            xml_result += "<taskname>" + (duty.task_name ?? "") + "</taskname>";
                            xml_result += "<tennhom>" + (duty.Tennhom ?? "") + "</tennhom>";
                            xml_result += "<organizationname>" + (duty.organization_name ?? "") + "</organizationname>";
                            xml_result += "<target>" + (duty.target ?? "") + "</target>";
                            xml_result += "<progress>" + (duty.progress ?? "") + "</progress>";
                            xml_result += "<statusname>" + (duty.status_name ?? "") + "</statusname>";
                            xml_result += "</element>";
                        }
                        xml_result += "</document>";
                    }

                    using (DBEntities db = new DBEntities())
                    {
                        var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                        System.Net.WebClient webc = new System.Net.WebClient();
                        string root = HttpContext.Current.Server.MapPath("~/");
                        string path = root + helper.path_xml + "/TaskReport/TaskDepartment";
                        bool exists = Directory.Exists(path);
                        if (!exists)
                            Directory.CreateDirectory(path);

                        string name_file = helper.GenKey() + ".xml";
                        string root_path = path + "/" + name_file;
                        string duong_dan = helper.path_xml + "/TaskReport/TaskDepartment/" + name_file;
                        string url = ConfigurationManager.AppSettings["ValidAudience"] + duong_dan;

                        File.WriteAllText(root_path, xml_result);
                        var res_encr = helper.encryptXML(root_path, "document", helper.psKey);
                        if (res_encr != "OK")
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Không thể mã hoã file XML!", err = "1" });
                        };

                        //await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_origin/export_xml", ip, tid, "Lỗi khi export file xml", 0, "task_origin");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_origin/export_xml", ip, tid, "Lỗi khi export file xml", 0, "task_origin");
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
        public async Task<HttpResponseMessage> TaskPersonal_export_xml([System.Web.Mvc.Bind(Include = "")][FromBody] List<task_report> list)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //string dataProc = data["str"].ToObject<string>();
            //string des = Codec.DecryptString(dataProc, helper.psKey);
            //sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);
            try
            {
                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

                try
                {
                    string xml_result = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>";
                    if (list != null && list.Count > 0)
                    {
                        xml_result += "<document>";
                        foreach (var duty in list)
                        {
                            xml_result += "<element>";
                            xml_result += "<taskid>" + (duty.task_id ?? "") + "</taskid>";
                            xml_result += "<taskname>" + (duty.task_name ?? "") + "</taskname>";
                            xml_result += "<tennhom>" + (duty.Tennhom ?? "") + "</tennhom>";
                            xml_result += "<organizationname>" + (duty.organization_name ?? "") + "</organizationname>";
                            xml_result += "<target>" + (duty.target ?? "") + "</target>";
                            xml_result += "<progress>" + (duty.progress ?? "") + "</progress>";
                            xml_result += "<statusname>" + (duty.status_name ?? "") + "</statusname>";
                            xml_result += "</element>";
                        }
                        xml_result += "</document>";
                    }

                    using (DBEntities db = new DBEntities())
                    {
                        var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                        System.Net.WebClient webc = new System.Net.WebClient();
                        string root = HttpContext.Current.Server.MapPath("~/");
                        string path = root + helper.path_xml + "/TaskReport/TaskPersonal";
                        bool exists = Directory.Exists(path);
                        if (!exists)
                            Directory.CreateDirectory(path);

                        string name_file = helper.GenKey() + ".xml";
                        string root_path = path + "/" + name_file;
                        string duong_dan = helper.path_xml + "/TaskReport/TaskPersonal/" + name_file;
                        string url = ConfigurationManager.AppSettings["ValidAudience"] + duong_dan;

                        File.WriteAllText(root_path, xml_result);
                        var res_encr = helper.encryptXML(root_path, "document", helper.psKey);
                        if (res_encr != "OK")
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Không thể mã hoã file XML!", err = "1" });
                        };

                        //await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_origin/export_xml", ip, tid, "Lỗi khi export file xml", 0, "task_origin");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_origin/export_xml", ip, tid, "Lỗi khi export file xml", 0, "task_origin");
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