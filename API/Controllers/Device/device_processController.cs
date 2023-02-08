using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Controllers.Device
{
    [Authorize(Roles = "login")]
    public class device_processController : ApiController
    {
        private const string const_module_key = "M7";
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

     

        [HttpPost]
        public async Task<HttpResponseMessage> add_process()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string fdprocess = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Process";
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
                        fdprocess = provider.FormData.GetValues("process").SingleOrDefault();
                        device_process process = JsonConvert.DeserializeObject<device_process>(fdprocess);
                        device_process device_Process_Pre = new device_process();


                        device_Process_Pre.device_process_code = process.device_process_code;
                        device_Process_Pre.content = process.content;
                        
                        device_Process_Pre.date_approved = DateTime.Now;
                        device_Process_Pre.approved_group_id = null;
                        device_Process_Pre.device_process_type = process.device_process_type;
                        device_Process_Pre.device_note_id = process.device_note_id;
                        device_Process_Pre.is_order = 0;
                        device_Process_Pre.organization_id = int.Parse(dvid);
                        device_Process_Pre.created_date = DateTime.Now;
                        device_Process_Pre.created_by = uid;
                        device_Process_Pre.created_ip = ip;
                        device_Process_Pre.date_send = DateTime.Now;
                        device_Process_Pre.modified_date = DateTime.Now;
                        device_Process_Pre.modified_by = uid;
                        device_Process_Pre.modified_ip = ip;
                        device_Process_Pre.approved_user_id = uid;
                        device_Process_Pre.is_approved = true;
                        device_Process_Pre.is_last = true;
                        var preNews = db.device_process.Where(s => (s.device_note_id == process.device_note_id && s.device_process_code == process.device_process_code)).FirstOrDefault();
                        if (preNews == null)
                        {
                            device_Process_Pre.is_view = true;
                            device_Process_Pre.date_view = DateTime.Now;
                        }
                        db.device_process.Add(device_Process_Pre);
                        db.SaveChanges();
                        string fduserfollows = "";
                        fduserfollows = provider.FormData.GetValues("userfollows").SingleOrDefault();
                        List<device_user_follows> device_User_s = JsonConvert.DeserializeObject<List<device_user_follows>>(fduserfollows);
                        foreach (var item in device_User_s)
                        {
                            item.device_process_id = device_Process_Pre.device_process_id;
                            item.organization_id = int.Parse(dvid);
                            item.created_date = DateTime.Now;
                            item.created_by = uid;
                            item.created_ip = ip;
                            db.device_user_follows.Add(item);

                            #region sendhub
                            var userSenHub = db.sys_users.Where(s => (s.user_id == item.user_id)).FirstOrDefault();
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.user_send = uid;
                            sh.module_key = const_module_key;
                            sh.receiver = userSenHub.user_id;
                            sh.icon = userSenHub.avatar;
                            sh.title = "Thiết bị";
                            sh.type = 6;
                            sh.date_send = DateTime.Now;
                            sh.id_key = process.device_note_id.ToString();
                            sh.group_id = null;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            sh.is_type = 0;
                            if (process.device_process_type == 1)
                            {
                                var rePair_FL = db.device_repair.Where(s => (s.device_repair_id == process.device_note_id)).FirstOrDefault();
                                sh.contents = "Theo dõi phiếu sửa chữa: " + rePair_FL.repair_number;


                            }
                            if (process.device_process_type == 2)
                            {
                                var inventory_FL = db.device_inventory_slip.Where(s => (s.inventory_slip_id == process.device_note_id)).FirstOrDefault();
                                sh.contents = "Theo dõi phiếu kiểm kê: " + inventory_FL.inventory_number;


                            }
                            if (process.device_process_type == 3)
                            {
                                var recall_FL = db.device_recall.Where(s => (s.device_recall_id == process.device_note_id)).FirstOrDefault();
                                sh.contents = "Theo dõi phiếu thu hồi: " + recall_FL.recall_number;


                            }
                            db.sys_sendhub.Add(sh);
                            db.SaveChanges();
                            #endregion
                        }
                        if (process.device_process_type == 1)
                        {
                            var device_Repair = db.device_repair.Where(s => s.device_repair_id == process.device_note_id).FirstOrDefault();
                            device_Repair.status = 1;
                            device_Repair.confirmation_date = DateTime.Now;
                        }
                        if (process.device_process_type == 2)
                        {
                            var device_Inventory_Slip = db.device_inventory_slip.Where(s => s.inventory_slip_id == process.device_note_id).FirstOrDefault();
                            device_Inventory_Slip.status = 3;
                            device_Inventory_Slip.confirmation_date = DateTime.Now;
                        }
                        if (process.device_process_type == 3)
                        {
                            var device_Inventory_Slip = db.device_recall.Where(s => s.device_recall_id == process.device_note_id).FirstOrDefault();
                            device_Inventory_Slip.status = 1;
                            device_Inventory_Slip.confirmation_date = DateTime.Now;
                        }

                        db.SaveChanges();

                        device_process_files processfile = new device_process_files();
                        var fdlistSize = provider.FormData.GetValues("filesize").SingleOrDefault();
                        List<int> listSize = JsonConvert.DeserializeObject<List<int>>(fdlistSize);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int order = 0;
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                            }
                            processfile.file_path = "/Portals/" + dvid + "/Process/" + fileName;
                            processfile.process_files_name = fileName;

                            ffileData = fileData;

                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }

                            processfile.file_size = listSize[order];
                            processfile.file_type = helper.GetFileExtension(fileName);
                            processfile.device_process_id = device_Process_Pre.device_process_id;
                            processfile.created_date = DateTime.Now;
                            processfile.created_by = uid;
                            processfile.created_ip = ip;
                            processfile.modified_date = DateTime.Now;
                            processfile.modified_by = uid;
                            processfile.modified_ip = ip;
                            db.device_process_files.Add(processfile);
                            db.SaveChanges();
                            order++;
                        }

                        process.pre_process_id = device_Process_Pre.device_process_id;
                        process.content = null;
                        process.organization_id = int.Parse(dvid);
                        process.created_date = DateTime.Now;
                        process.created_by = uid;
                        process.created_ip = ip;
                        process.date_send = DateTime.Now;
                        process.modified_date = DateTime.Now;
                        process.modified_by = uid;
                        process.modified_ip = ip;
                        var device_Approved = db.device_approved_group.Where(s => s.approved_group_id == process.approved_group_id).FirstOrDefault();


                        if (device_Approved.approved_type == 1)
                        {
                            process.is_last = true;
                        }
                        if (device_Approved != null)
                        {
                            if (device_Approved.is_approved_by_department == false)
                            {
                                var device_Approved_Users = db.device_approved_department_user.Where(s => s.approved_group_id == device_Approved.approved_group_id).FirstOrDefault();
                                process.approved_user_id = device_Approved_Users.approved_user_id;
                                process.is_approved = false;
                            }
                            else
                            {
                                var Or_user = db.sys_users.Where(s => s.user_id == uid).FirstOrDefault();
                                var strC = dvid;
                                if (Or_user.department_id != null)
                                {
                                    strC = Or_user.department_id.ToString();
                                }
                                var srw = int.Parse(strC);
                                var device_Approved_Department = db.device_approved_department_group.Where(s => s.approved_group_id == device_Approved.approved_group_id &&
                                s.department_id == srw
                                ).FirstOrDefault();

                                if (device_Approved_Department != null)
                                {
                                    process.approved_user_id = device_Approved_Department.approved_user_id;
                                    process.is_last = true;
                                    process.is_approved = false;
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Nhóm duyệt không chứa phòng ban của bạn! Vui lòng cấu hình lại.", err = "1" });
                                }

                            }
                        }

                        if (process.approved_user_id != null)
                        {
                            #region sendhub
                            var userSenHub = db.sys_users.Where(s => (s.user_id == process.approved_user_id)).FirstOrDefault();
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.user_send = uid;
                            sh.module_key = const_module_key;
                            sh.receiver = userSenHub.user_id;
                            sh.icon = userSenHub.avatar;
                            sh.title = "Thiết bị";
                            sh.type = 6;
                            sh.date_send = DateTime.Now;
                            sh.id_key = process.device_note_id.ToString();
                            sh.group_id = null;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            sh.is_type = 0;
                            if (process.device_process_type == 1)
                            {
                                var rePair_FL = db.device_repair.Where(s => (s.device_repair_id == process.device_note_id)).FirstOrDefault();
                                sh.contents = "Duyệt phiếu sửa chữa: " + rePair_FL.repair_number;


                            }
                            if (process.device_process_type == 2)
                            {
                                var inventory_FL = db.device_inventory_slip.Where(s => (s.inventory_slip_id == process.device_note_id)).FirstOrDefault();
                                sh.contents = "Duyệt phiếu kiểm kê: " + inventory_FL.inventory_number;


                            }
                            if (process.device_process_type == 3)
                            {
                                var recall_FL = db.device_recall.Where(s => (s.device_recall_id == process.device_note_id)).FirstOrDefault();
                                sh.contents = "Duyệt phiếu thu hồi: " + recall_FL.recall_number;


                            }
                            db.sys_sendhub.Add(sh);
                            db.SaveChanges();
                            #endregion
                        }
                        else
                        {
                            var device_Approved_Users = db.device_approved_department_user
                            .Where(s => s.approved_group_id == device_Approved.approved_group_id).ToArray();
                            foreach (var item in device_Approved_Users)
                            {

                                #region sendhub
                                var userSenHub = db.sys_users.Where(s => (s.user_id == item.approved_user_id)).FirstOrDefault();
                                var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.user_send = uid;
                                sh.module_key = const_module_key;
                                sh.receiver = userSenHub.user_id;
                                sh.icon = userSenHub.avatar;
                                sh.title = "Thiết bị";
                                sh.type = 6;
                                sh.date_send = DateTime.Now;
                                sh.id_key = process.device_note_id.ToString();
                                sh.group_id = null;
                                sh.token_id = tid;
                                sh.created_date = DateTime.Now;
                                sh.created_by = uid;
                                sh.created_token_id = tid;
                                sh.created_ip = ip;
                                sh.is_type = 0;
                                if (process.device_process_type == 1)
                                {
                                    var rePair_FL = db.device_repair.Where(s => (s.device_repair_id == process.device_note_id)).FirstOrDefault();
                                    sh.contents = "Duyệt phiếu sửa chữa: " + rePair_FL.repair_number;


                                }
                                if (process.device_process_type == 2)
                                {
                                    var inventory_FL = db.device_inventory_slip.Where(s => (s.inventory_slip_id == process.device_note_id)).FirstOrDefault();
                                    sh.contents = "Duyệt phiếu kiểm kê: " + inventory_FL.inventory_number;


                                }
                                if (process.device_process_type == 3)
                                {
                                    var recall_FL = db.device_recall.Where(s => (s.device_recall_id == process.device_note_id)).FirstOrDefault();
                                    sh.contents = "Duyệt phiếu thu hồi: " + recall_FL.recall_number;


                                }
                                db.sys_sendhub.Add(sh);
                                db.SaveChanges();
                                #endregion   }
                            }

                        }
                        db.device_process.Add(process);
                        db.SaveChanges();




















                        #region add device_process_log
                        device_process_log device_Process_Log = new device_process_log();
                        device_Process_Log.content = "Trình duyệt " + process.device_process_code;
                        device_Process_Log.device_note_id = process.device_note_id;
                        device_Process_Log.device_process_type = process.device_process_type;
                        device_Process_Log.device_process_id = process.device_process_id;
                        device_Process_Log.created_by = uid;
                        device_Process_Log.created_date = DateTime.Now;
                        device_Process_Log.created_ip = ip;
                        db.device_process_log.Add(device_Process_Log);
                        db.SaveChanges();



                        var fddevice_process_log = db.device_process_log.Where(s => s.device_note_id == process.device_note_id && s.device_process_type == process.device_process_type
                        && s.device_process_id == null).ToArray();
                        if (fddevice_process_log.Length > 0)
                        {
                            foreach (var item in fddevice_process_log)
                            {
                                item.device_process_id = process.device_process_id;

                            }
                            db.SaveChanges();

                        }


                        #endregion
                        #region add device_log
                        if (helper.wlog)
                        {

                            device_log log = new device_log();
                            log.title = "Trình duyệt " + process.device_process_code;

                            log.log_module = "device_process";
                            log.id_key = process.device_process_id.ToString();
                            log.created_date = DateTime.Now;
                            log.log_type = 0;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.device_log.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_process", ip, tid, "Lỗi khi thêm tiến trình xử lý", 0, "device_Handover");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_process", ip, tid, "Lỗi khi thêm tiến trình xử lý", 0, "device_Handover");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }





        [HttpPut]
        public async Task<HttpResponseMessage> accept_device_process()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string fdprocess = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Process";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);


                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdprocess = provider.FormData.GetValues("process").SingleOrDefault();
                        device_process device_Process = JsonConvert.DeserializeObject<device_process>(fdprocess);
                        var process = db.device_process.AsNoTracking().Where(s => s.device_process_id == device_Process.device_process_id).FirstOrDefault();
                        process.content = device_Process.content;
                        process.approved_user_id = uid;
                        process.date_approved = DateTime.Now;
                        process.is_approved = true;
                        db.Entry(process).State = EntityState.Modified;
                        db.SaveChanges();
                        var fduserfollows = "";
                        fduserfollows = provider.FormData.GetValues("userfollows").SingleOrDefault();

                        List<device_user_follows> device_User_s = JsonConvert.DeserializeObject<List<device_user_follows>>(fduserfollows);
                        foreach (var item in device_User_s)
                        {
                            item.device_process_id = process.device_process_id;
                            item.organization_id = int.Parse(dvid);
                            item.created_date = DateTime.Now;
                            item.created_by = uid;
                            item.created_ip = ip;
                            db.device_user_follows.Add(item);
                            #region sendhub
                            var userSenHub = db.sys_users.Where(s => (s.user_id == item.user_id)).FirstOrDefault();
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.user_send = uid;
                            sh.module_key = const_module_key;
                            sh.receiver = userSenHub.user_id;
                            sh.icon = userSenHub.avatar;
                            sh.title = "Thiết bị";
                            sh.type = 6;
                            sh.date_send = DateTime.Now;
                            sh.id_key = process.device_note_id.ToString();
                            sh.group_id = null;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            sh.is_type = 0;
                            if (process.device_process_type == 1)
                            {
                                var rePair_FL = db.device_repair.Where(s => (s.device_repair_id == process.device_note_id)).FirstOrDefault();
                                sh.contents = "Theo dõi phiếu sửa chữa: " + rePair_FL.repair_number;


                            }
                            if (process.device_process_type == 2)
                            {
                                var inventory_FL = db.device_inventory_slip.Where(s => (s.inventory_slip_id == process.device_note_id)).FirstOrDefault();
                                sh.contents = "Theo dõi phiếu kiểm kê: " + inventory_FL.inventory_number;


                            }
                            if (process.device_process_type == 3)
                            {
                                var recall_FL = db.device_recall.Where(s => (s.device_recall_id == process.device_note_id)).FirstOrDefault();
                                sh.contents = "Theo dõi phiếu thu hồi: " + recall_FL.recall_number;


                            }
                            db.sys_sendhub.Add(sh);
                            db.SaveChanges();
                            #endregion
                        }

                        device_process_files processfile = new device_process_files();
                        var fdlistSize = provider.FormData.GetValues("filesize").SingleOrDefault();
                        List<int> listSize = JsonConvert.DeserializeObject<List<int>>(fdlistSize);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int order = 0;

                        var check_add_next = false;
                        if (process.device_process_type == 1)
                        {
                            var fdDetailsRepair = "";
                            fdDetailsRepair = provider.FormData.GetValues("details").SingleOrDefault();
                            List<device_repair_details> liDetailsRepair = JsonConvert.DeserializeObject<List<device_repair_details>>(fdDetailsRepair);
                            if (liDetailsRepair != null)
                            {
                                foreach (var item in liDetailsRepair)
                                {
                                    item.modified_by = uid;
                                    item.modified_date = DateTime.Now;
                                    item.modified_ip = ip;
                                    db.Entry(item).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }

                        };

                        var add_Process = new device_process();
                        add_Process.organization_id = process.organization_id;
                        add_Process.approved_group_id = process.approved_group_id;
                        add_Process.pre_process_id = process.pre_process_id;
                        add_Process.device_note_id = process.device_note_id;
                        add_Process.device_process_code = process.device_process_code;
                        add_Process.device_process_type = process.device_process_type;
                        add_Process.date_send = process.date_send;
                        add_Process.content = null;
                        add_Process.returned_content = null;
                        add_Process.is_approved = false;
                        add_Process.approved_user_id = null;
                        add_Process.date_approved = null;
                        add_Process.modified_date = DateTime.Now;
                        add_Process.modified_by = uid;
                        add_Process.modified_ip = ip;
                        add_Process.created_date = DateTime.Now;
                        add_Process.created_by = uid;
                        add_Process.created_ip = ip;

                        var device_Approved = db.device_approved_group.Where(s => s.approved_group_id == process.approved_group_id).FirstOrDefault();
                        var classify_D = device_Approved.classify.Split(',');
                        var check_class = false;
                        if (classify_D.Length > 0)
                            foreach (var item in classify_D)
                            {
                                if (item == "17")
                                {
                                    check_class = true;
                                }
                            }
                        if (device_Approved != null)
                        {
                            if (device_Approved.is_approved_by_department == true)
                            {
                                if (process.device_process_type == 1 && check_class == true)
                                {
                                    var device_Repair = db.device_repair.Where(s => s.device_repair_id == process.device_note_id).FirstOrDefault();
                                    device_Repair.status = 2;
                                    var device_Repair_Details = db.device_repair_details.Where(s => s.device_repair_id == device_Repair.device_repair_id).ToArray();
                                    foreach (var item in device_Repair_Details)
                                    {
                                        if (item.repair_condition == 2)
                                        {
                                            item.repair_condition = 4;
                                        }
                                        var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                        dkItem.assets_condition = item.condition;
                                        if (item.repair_condition == 3)
                                        {
                                            dkItem.nearest_user = dkItem.device_user_id;
                                            dkItem.device_user_id = null;
                                            dkItem.manage_department_id = null;
                                            dkItem.warehouse_id = dkItem.warehouse_id_old;
                                            dkItem.status = "HKS";
                                        }
                                        else
                                            dkItem.status = dkItem.old_status;
                                        dkItem.old_status = "DSC";
                                    }
                                }
                                if (process.device_process_type == 2 && check_class == true)
                                {
                                    var device_Inventory_Slip = db.device_inventory_slip.Where(s => s.inventory_slip_id == process.device_note_id).FirstOrDefault();
                                    device_Inventory_Slip.status = 4;

                                }
                                if (process.device_process_type == 3 && check_class == true)
                                {
                                    var device_Recall = db.device_recall.Where(s => s.device_recall_id == process.device_note_id).FirstOrDefault();
                                    device_Recall.status = 2;
                                    var device_Recall_Details = db.device_recall_details.Where(s => s.device_recall_id == device_Recall.device_recall_id).ToArray();
                                    foreach (var item in device_Recall_Details)
                                    {
                                        var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                        dkItem.old_status = dkItem.status;
                                        dkItem.status = "TK";
                                        dkItem.is_recall = true;
                                        dkItem.device_user_id = null;
                                        dkItem.use_date = null;

                                        dkItem.manage_department_id = null;
                                        dkItem.confirmed = null;
                                        dkItem.issued = null;
                                        dkItem.warehouse_id = device_Recall.warehouse_id;

                                    }
                                }
                            }
                            if (device_Approved.approved_type == 1 && device_Approved.is_approved_by_department == false)
                            {
                                if (process.device_process_type == 1 && check_class == true)
                                {
                                    var device_Repair = db.device_repair.Where(s => s.device_repair_id == process.device_note_id).FirstOrDefault();
                                    device_Repair.status = 2;

                                    var device_Repair_Details = db.device_repair_details.Where(s => s.device_repair_id == device_Repair.device_repair_id).ToArray();
                                    foreach (var item in device_Repair_Details)
                                    {
                                        if (item.repair_condition == 2)
                                        {
                                            item.repair_condition = 4;
                                        }
                                        var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                        dkItem.assets_condition = item.condition;
                                        if (item.repair_condition == 3)
                                        {
                                            dkItem.nearest_user = dkItem.device_user_id;
                                            dkItem.device_user_id = null; dkItem.manage_department_id = null;
                                            dkItem.warehouse_id = dkItem.warehouse_id_old;
                                            dkItem.status = "HKS";
                                        }
                                        else
                                            dkItem.status = dkItem.old_status;
                                        dkItem.old_status = "DSC";
                                    }
                                }
                                if (process.device_process_type == 2 && check_class == true)
                                {
                                    var device_Inventory_Slip = db.device_inventory_slip.Where(s => s.inventory_slip_id == process.device_note_id).FirstOrDefault();
                                    device_Inventory_Slip.status = 4;

                                }
                                if (process.device_process_type == 3 && check_class == true)
                                {
                                    var device_Recall = db.device_recall.Where(s => s.device_recall_id == process.device_note_id).FirstOrDefault();
                                    device_Recall.status = 2;
                                    var device_Recall_Details = db.device_recall_details.Where(s => s.device_recall_id == device_Recall.device_recall_id).ToArray();
                                    foreach (var item in device_Recall_Details)
                                    {
                                        var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                        dkItem.old_status = dkItem.status;
                                        dkItem.status = "TK";
                                        dkItem.is_recall = true;
                                        dkItem.device_user_id = null;
                                        dkItem.use_date = null;

                                        dkItem.manage_department_id = null;
                                        dkItem.confirmed = null;
                                        dkItem.issued = null;
                                        dkItem.warehouse_id = device_Recall.warehouse_id;

                                    }
                                }
                            }
                            if (device_Approved.approved_type == 3 && device_Approved.is_approved_by_department == false)
                            {
                                if (process.device_process_type == 1)
                                {
                                    var device_Approved_Users_1 = db.device_approved_department_user.Where(s => s.approved_group_id == device_Approved.approved_group_id).OrderBy(s => s.approved_department_user_id).ToArray();
                                    check_add_next = false;
                                    foreach (var item in device_Approved_Users_1)
                                    {
                                        var device_Process_Check = db.device_process.Where(s =>
                                           s.device_note_id == process.device_note_id && s.device_process_type == 1 && s.pre_process_id == process.pre_process_id && item.approved_user_id == s.approved_user_id && s.is_approved == true
                                        ).FirstOrDefault();
                                        if (device_Process_Check == null)
                                        {
                                            var device_Process_Count = db.device_process.Where(s => s.pre_process_id == process.pre_process_id &&
                                            item.approved_user_id != s.approved_user_id && s.is_approved == true
                                                        && process.device_process_type == s.device_process_type
                                        && s.device_note_id == process.device_note_id).Select(x => x.approved_user_id).Distinct().ToArray().Length;


                                            if (device_Process_Count == device_Approved_Users_1.Length - 1)
                                            {
                                                add_Process.is_last = true;
                                            }


                                            if (device_Process_Count == device_Approved_Users_1.Length - 1)
                                            {
                                                add_Process.is_last = true;
                                            }

                                            db.device_process.Add(add_Process);
                                            db.SaveChanges();
                                            check_add_next = true;
                                            break;
                                        }
                                    }
                                    if (check_add_next == false && check_class == true)
                                    {
                                        var device_Repair = db.device_repair.Where(s => s.device_repair_id == process.device_note_id).FirstOrDefault();
                                        device_Repair.status = 2;
                                        var device_Repair_Details = db.device_repair_details.Where(s => s.device_repair_id == device_Repair.device_repair_id).ToArray();
                                        foreach (var item in device_Repair_Details)
                                        {
                                            if (item.repair_condition == 2)
                                            {
                                                item.repair_condition = 4;
                                            }
                                            var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                            dkItem.assets_condition = item.condition;
                                            if (item.repair_condition == 3)
                                            {
                                                dkItem.nearest_user = dkItem.device_user_id;
                                                dkItem.device_user_id = null; dkItem.manage_department_id = null;
                                                dkItem.warehouse_id = dkItem.warehouse_id_old;
                                                dkItem.status = "HKS";
                                            }
                                            else
                                                dkItem.status = dkItem.old_status;
                                            dkItem.old_status = "DSC";
                                        }
                                    }
                                }

                                if (process.device_process_type == 2)
                                {
                                    var device_Approved_Users_1 = db.device_approved_department_user.Where(s => s.approved_group_id == device_Approved.approved_group_id).OrderBy(s => s.approved_department_user_id).ToArray();
                                    check_add_next = false;
                                    foreach (var item in device_Approved_Users_1)
                                    {
                                        var device_Process_Check = db.device_process.Where(s =>
                                        s.device_note_id == process.device_note_id && s.device_process_type == 2 && s.pre_process_id == process.pre_process_id && item.approved_user_id == s.approved_user_id && s.is_approved == true
                                        ).FirstOrDefault();
                                        if (device_Process_Check == null)
                                        {
                                            var device_Process_Count = db.device_process.Where(s => s.pre_process_id == process.pre_process_id &&
                                            item.approved_user_id != s.approved_user_id && s.is_approved == true && process.device_process_type == s.device_process_type
                                        && s.device_note_id == process.device_note_id
                                                                                ).Select(x => x.approved_user_id).Distinct().ToArray().Length;


                                            if (device_Process_Count == device_Approved_Users_1.Length - 1)
                                            {
                                                add_Process.is_last = true;
                                            }

                                            db.device_process.Add(add_Process);
                                            db.SaveChanges();
                                            check_add_next = true;
                                            break;
                                        }
                                    }
                                    if (check_add_next == false && check_class == true)
                                    {
                                        var deivce_Inventory = db.device_inventory_slip.Where(s => s.inventory_slip_id == process.device_note_id).FirstOrDefault();
                                        deivce_Inventory.status = 4;

                                    }
                                }
                                if (process.device_process_type == 3)
                                {
                                    var device_Approved_Users_1 = db.device_approved_department_user.Where(s => s.approved_group_id == device_Approved.approved_group_id).OrderBy(s => s.approved_department_user_id).ToArray();
                                    check_add_next = false;
                                    foreach (var item in device_Approved_Users_1)
                                    {
                                        var device_Process_Check = db.device_process.Where(s =>
                                        s.device_note_id == process.device_note_id && s.device_process_type == 3 && s.pre_process_id == process.pre_process_id && item.approved_user_id == s.approved_user_id && s.is_approved == true
                                        ).FirstOrDefault();
                                        if (device_Process_Check == null)
                                        {
                                            var device_Process_Count = db.device_process.Where(s => s.pre_process_id == process.pre_process_id &&
                                            item.approved_user_id != s.approved_user_id && s.is_approved == true && process.device_process_type == s.device_process_type
                                        && s.device_note_id == process.device_note_id
                                                                                ).Select(x => x.approved_user_id).Distinct().ToArray().Length;

                                            if (device_Process_Count == device_Approved_Users_1.Length - 1)
                                            {
                                                add_Process.is_last = true;
                                            }

                                            db.device_process.Add(add_Process);
                                            db.SaveChanges();
                                            check_add_next = true;
                                            break;
                                        }
                                    }
                                    if (check_add_next == false && check_class == true)
                                    {
                                        var device_Recall = db.device_recall.Where(s => s.device_recall_id == process.device_note_id).FirstOrDefault();
                                        device_Recall.status = 2;
                                        var device_Recall_Details = db.device_recall_details.Where(s => s.device_recall_id == device_Recall.device_recall_id).ToArray();
                                        foreach (var item in device_Recall_Details)
                                        {
                                            var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                            dkItem.old_status = dkItem.status;
                                            dkItem.status = "TK";
                                            dkItem.is_recall = true;
                                            dkItem.device_user_id = null;
                                            dkItem.use_date = null;

                                            dkItem.manage_department_id = null;
                                            dkItem.confirmed = null;
                                            dkItem.issued = null;
                                            dkItem.warehouse_id = device_Recall.warehouse_id;
                                        }
                                    }
                                }

                            }
                            if (device_Approved.approved_type == 2 && device_Approved.is_approved_by_department == false)
                            {
                                if (process.device_process_type == 1)
                                {
                                    var device_Approved_Users = db.device_approved_department_user.Where(s => s.approved_group_id == device_Approved.approved_group_id).OrderBy(s => s.approved_department_user_id).ToArray();
                                    check_add_next = false;




                                    if (process.approved_user_id == device_Approved_Users[device_Approved_Users.Length - 2].approved_user_id)
                                    {
                                        add_Process.is_last = true;
                                    }
                                    var device_Process_Check = db.device_process.Where(s => s.device_process_type == 1 && s.approved_group_id == process.approved_group_id && s.device_process_code == process.device_process_code
                                      && s.device_note_id == process.device_note_id && (s.is_approved == true || s.is_returned == true)
                                      ).OrderByDescending(s => s.device_process_id).FirstOrDefault();


                                    if (device_Process_Check != null)
                                    {
                                        if (device_Process_Check.is_approved == true)
                                        {
                                            for (int i = 0; i < device_Approved_Users.Length - 1; i++)
                                            {
                                                if (device_Process_Check.approved_user_id == device_Approved_Users[i].approved_user_id)
                                                {

                                                    add_Process.approved_user_id = device_Approved_Users[i + 1].approved_user_id;
                                                    add_Process.date_approved = null;
                                                    add_Process.is_approved = false;
                                                    add_Process.created_date = DateTime.Now;
                                                    add_Process.created_by = uid;
                                                    add_Process.created_ip = ip;
                                                    #region sendhub
                                                    var userSenHub = db.sys_users.Where(s => (s.user_id == add_Process.approved_user_id)).FirstOrDefault();
                                                    var repairFL = db.device_repair.Where(s => (s.device_repair_id == add_Process.device_note_id)).FirstOrDefault();
                                                    var sh = new sys_sendhub();
                                                    sh.senhub_id = helper.GenKey();
                                                    sh.user_send = uid;
                                                    sh.module_key = const_module_key;
                                                    sh.receiver = userSenHub.user_id;
                                                    sh.icon = userSenHub.avatar;
                                                    sh.title = "Thiết bị";
                                                    sh.type = 6;
                                                    sh.date_send = DateTime.Now;
                                                    sh.id_key = process.device_note_id.ToString();
                                                    sh.group_id = null;
                                                    sh.token_id = tid;
                                                    sh.created_date = DateTime.Now;
                                                    sh.created_by = uid;
                                                    sh.created_token_id = tid;
                                                    sh.created_ip = ip;
                                                    sh.contents = "Duyệt phiếu sửa chữa: " + repairFL.repair_number;
                                                    sh.is_type = 0;

                                                    db.sys_sendhub.Add(sh);
                                                    db.SaveChanges();
                                                    #endregion
                                                    db.device_process.Add(add_Process);
                                                    db.SaveChanges();
                                                    check_add_next = true;
                                                    break;

                                                }


                                            }

                                        }
                                        else
                                        {
                                            add_Process.approved_user_id = device_Process_Check.approved_user_id;
                                            add_Process.date_approved = null;
                                            add_Process.is_approved = false;
                                            add_Process.created_date = DateTime.Now;
                                            add_Process.created_by = uid;
                                            add_Process.created_ip = ip;
                                            #region sendhub
                                            var userSenHub = db.sys_users.Where(s => (s.user_id == add_Process.approved_user_id)).FirstOrDefault();
                                            var repairFL = db.device_repair.Where(s => (s.device_repair_id == add_Process.device_note_id)).FirstOrDefault();
                                            var sh = new sys_sendhub();
                                            sh.senhub_id = helper.GenKey();
                                            sh.user_send = uid;
                                            sh.module_key = const_module_key;
                                            sh.receiver = userSenHub.user_id;
                                            sh.icon = userSenHub.avatar;
                                            sh.title = "Thiết bị";
                                            sh.type = 6;
                                            sh.date_send = DateTime.Now;
                                            sh.id_key = process.device_note_id.ToString();
                                            sh.group_id = null;
                                            sh.token_id = tid;
                                            sh.created_date = DateTime.Now;
                                            sh.created_by = uid;
                                            sh.created_token_id = tid;
                                            sh.created_ip = ip;
                                            sh.contents = "Duyệt phiếu sửa chữa: " + repairFL.repair_number;
                                            sh.is_type = 0;

                                            db.sys_sendhub.Add(sh);
                                            db.SaveChanges();
                                            #endregion
                                            db.device_process.Add(add_Process);
                                            db.SaveChanges();
                                            check_add_next = true;

                                        }
                                    }
                                    if (check_add_next == false && check_class == true)
                                    {
                                        var device_Repair = db.device_repair.Where(s => s.device_repair_id == process.device_note_id).FirstOrDefault();
                                        device_Repair.status = 2;
                                        var device_Repair_Details = db.device_repair_details.Where(s => s.device_repair_id == device_Repair.device_repair_id).ToArray();
                                        foreach (var item in device_Repair_Details)
                                        {
                                            if (item.repair_condition == 2)
                                            {
                                                item.repair_condition = 4;
                                            }
                                            var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                            dkItem.assets_condition = item.condition;
                                            if (item.repair_condition == 3)
                                            {
                                                dkItem.nearest_user = dkItem.device_user_id;
                                                dkItem.device_user_id = null; dkItem.manage_department_id = null;
                                                dkItem.warehouse_id = dkItem.warehouse_id_old;
                                                dkItem.status = "HKS";
                                            }
                                            else
                                                dkItem.status = dkItem.old_status;
                                            dkItem.old_status = "DSC";
                                        }
                                    }
                                }

                                if (process.device_process_type == 2)
                                {
                                    var device_Approved_Users = db.device_approved_department_user.Where(s => s.approved_group_id == device_Approved.approved_group_id).OrderBy(s => s.approved_department_user_id).ToArray();
                                    check_add_next = false;

                                    if (process.approved_user_id == device_Approved_Users[device_Approved_Users.Length - 2].approved_user_id)
                                    {
                                        add_Process.is_last = true;
                                    }
                                    var device_Process_Check = db.device_process.Where(s =>
                                    s.approved_group_id == process.approved_group_id
                                    && s.device_process_code == process.device_process_code
                                    && s.device_note_id == process.device_note_id
                                      && s.device_process_type == 2
                                    && (s.is_approved == true || s.is_returned == true)
                                      ).OrderByDescending(s => s.device_process_id).FirstOrDefault();


                                    if (device_Process_Check != null)
                                    {
                                        if (device_Process_Check.is_approved == true)
                                        {
                                            for (int i = 0; i < device_Approved_Users.Length - 1; i++)
                                            {
                                                if (device_Process_Check.approved_user_id == device_Approved_Users[i].approved_user_id)
                                                {

                                                    add_Process.approved_user_id = device_Approved_Users[i + 1].approved_user_id;
                                                    add_Process.date_approved = null;
                                                    add_Process.is_approved = false;
                                                    add_Process.created_date = DateTime.Now;
                                                    add_Process.created_by = uid;
                                                    add_Process.created_ip = ip;
                                                    #region sendhub
                                                    var userSenHub = db.sys_users.Where(s => (s.user_id == add_Process.approved_user_id)).FirstOrDefault();
                                                    var inventoryFL = db.device_inventory_slip.Where(s => (s.inventory_slip_id == add_Process.device_note_id)).FirstOrDefault();
                                                    var sh = new sys_sendhub();
                                                    sh.senhub_id = helper.GenKey();
                                                    sh.user_send = uid;
                                                    sh.module_key = const_module_key;
                                                    sh.receiver = userSenHub.user_id;
                                                    sh.icon = userSenHub.avatar;
                                                    sh.title = "Thiết bị";
                                                    sh.type = 6;
                                                    sh.date_send = DateTime.Now;
                                                    sh.id_key = process.device_note_id.ToString();
                                                    sh.group_id = null;
                                                    sh.token_id = tid;
                                                    sh.created_date = DateTime.Now;
                                                    sh.created_by = uid;
                                                    sh.created_token_id = tid;
                                                    sh.created_ip = ip;
                                                    sh.contents = "Duyệt phiếu kiểm kê: " + inventoryFL.inventory_number;
                                                    sh.is_type = 0;

                                                    db.sys_sendhub.Add(sh);
                                                    db.SaveChanges();
                                                    #endregion
                                                    db.device_process.Add(add_Process);
                                                    db.SaveChanges();
                                                    check_add_next = true;
                                                    break;

                                                }


                                            }

                                        }
                                        else
                                        {
                                            add_Process.approved_user_id = device_Process_Check.approved_user_id;
                                            add_Process.date_approved = null;
                                            add_Process.is_approved = false;
                                            add_Process.created_date = DateTime.Now;
                                            add_Process.created_by = uid;
                                            add_Process.created_ip = ip;
                                            #region sendhub
                                            var userSenHub = db.sys_users.Where(s => (s.user_id == add_Process.approved_user_id)).FirstOrDefault();
                                            var inventoryFL = db.device_inventory_slip.Where(s => (s.inventory_slip_id == add_Process.device_note_id)).FirstOrDefault();
                                            var sh = new sys_sendhub();
                                            sh.senhub_id = helper.GenKey();
                                            sh.user_send = uid;
                                            sh.module_key = const_module_key;
                                            sh.receiver = userSenHub.user_id;
                                            sh.icon = userSenHub.avatar;
                                            sh.title = "Thiết bị";
                                            sh.type = 6;
                                            sh.date_send = DateTime.Now;
                                            sh.id_key = process.device_note_id.ToString();
                                            sh.group_id = null;
                                            sh.token_id = tid;
                                            sh.created_date = DateTime.Now;
                                            sh.created_by = uid;
                                            sh.created_token_id = tid;
                                            sh.created_ip = ip;
                                            sh.contents = "Duyệt phiếu kiểm kê: " + inventoryFL.inventory_number;
                                            sh.is_type = 0;

                                            db.sys_sendhub.Add(sh);
                                            db.SaveChanges();
                                            #endregion
                                            db.device_process.Add(add_Process);
                                            db.SaveChanges();
                                            check_add_next = true;

                                        }
                                    }
                                    if (check_add_next == false && check_class == true)
                                    {
                                        var device_Inventory_Slip = db.device_inventory_slip.Where(s => s.inventory_slip_id == process.device_note_id).FirstOrDefault();
                                        device_Inventory_Slip.status = 4;


                                    }
                                }

                                if (process.device_process_type == 3)
                                {
                                    var device_Approved_Users = db.device_approved_department_user.Where(s => s.approved_group_id == device_Approved.approved_group_id).OrderBy(s => s.approved_department_user_id).ToArray();
                                    check_add_next = false;

                                    if (process.approved_user_id == device_Approved_Users[device_Approved_Users.Length - 2].approved_user_id)
                                    {
                                        add_Process.is_last = true;
                                    }
                                    var device_Process_Check = db.device_process.Where(s =>
                                    s.approved_group_id == process.approved_group_id
                                    && s.device_process_code == process.device_process_code
                                    && s.device_note_id == process.device_note_id
                                      && s.device_process_type == 3
                                    && (s.is_approved == true || s.is_returned == true)
                                      ).OrderByDescending(s => s.device_process_id).FirstOrDefault();


                                    if (device_Process_Check != null)
                                    {
                                        if (device_Process_Check.is_approved == true)
                                        {
                                            for (int i = 0; i < device_Approved_Users.Length - 1; i++)
                                            {
                                                if (device_Process_Check.approved_user_id == device_Approved_Users[i].approved_user_id)
                                                {

                                                    add_Process.approved_user_id = device_Approved_Users[i + 1].approved_user_id;
                                                    add_Process.date_approved = null;
                                                    add_Process.is_approved = false;
                                                    add_Process.created_date = DateTime.Now;
                                                    add_Process.created_by = uid;
                                                    add_Process.created_ip = ip;
                                                    #region sendhub
                                                    var userSenHub = db.sys_users.Where(s => (s.user_id == add_Process.approved_user_id)).FirstOrDefault();
                                                    var recallFL = db.device_recall.Where(s => (s.device_recall_id == add_Process.device_note_id)).FirstOrDefault();
                                                    var sh = new sys_sendhub();
                                                    sh.senhub_id = helper.GenKey();
                                                    sh.user_send = uid;
                                                    sh.module_key = const_module_key;
                                                    sh.receiver = userSenHub.user_id;
                                                    sh.icon = userSenHub.avatar;
                                                    sh.title = "Thiết bị";
                                                    sh.type = 6;
                                                    sh.date_send = DateTime.Now;
                                                    sh.id_key = process.device_note_id.ToString();
                                                    sh.group_id = null;
                                                    sh.token_id = tid;
                                                    sh.created_date = DateTime.Now;
                                                    sh.created_by = uid;
                                                    sh.created_token_id = tid;
                                                    sh.created_ip = ip;
                                                    sh.contents = "Duyệt phiếu thu hồi: " + recallFL.recall_number;
                                                    sh.is_type = 0;

                                                    db.sys_sendhub.Add(sh);
                                                    db.SaveChanges();
                                                    #endregion
                                                    db.device_process.Add(add_Process);
                                                    db.SaveChanges();
                                                    check_add_next = true;
                                                    break;

                                                }


                                            }

                                        }
                                        else
                                        {
                                            add_Process.approved_user_id = device_Process_Check.approved_user_id;
                                            add_Process.date_approved = null;
                                            add_Process.is_approved = false;
                                            add_Process.created_date = DateTime.Now;
                                            add_Process.created_by = uid;
                                            add_Process.created_ip = ip;
                                            #region sendhub
                                            var userSenHub = db.sys_users.Where(s => (s.user_id == add_Process.approved_user_id)).FirstOrDefault();
                                            var recallFL = db.device_recall.Where(s => (s.device_recall_id == add_Process.device_note_id)).FirstOrDefault();
                                            var sh = new sys_sendhub();
                                            sh.senhub_id = helper.GenKey();
                                            sh.user_send = uid;
                                            sh.module_key = const_module_key;
                                            sh.receiver = userSenHub.user_id;
                                            sh.icon = userSenHub.avatar;
                                            sh.title = "Thiết bị";
                                            sh.type = 6;
                                            sh.date_send = DateTime.Now;
                                            sh.id_key = process.device_note_id.ToString();
                                            sh.group_id = null;
                                            sh.token_id = tid;
                                            sh.created_date = DateTime.Now;
                                            sh.created_by = uid;
                                            sh.created_token_id = tid;
                                            sh.created_ip = ip;
                                            sh.contents = "Duyệt phiếu thu hồi: " + recallFL.recall_number;
                                            sh.is_type = 0;

                                            db.sys_sendhub.Add(sh);
                                            db.SaveChanges();
                                            #endregion
                                            db.device_process.Add(add_Process);
                                            db.SaveChanges();
                                            check_add_next = true;

                                        }
                                    }
                                    if (check_add_next == false && check_class == true)
                                    {
                                        var device_Recall = db.device_recall.Where(s => s.device_recall_id == process.device_note_id).FirstOrDefault();
                                        device_Recall.status = 2;
                                        var device_Recall_Details = db.device_recall_details.Where(s => s.device_recall_id == device_Recall.device_recall_id).ToArray();
                                        foreach (var item in device_Recall_Details)
                                        {
                                            var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                            dkItem.old_status = dkItem.status;
                                            dkItem.status = "TK";
                                            dkItem.is_recall = true;
                                            dkItem.device_user_id = null;
                                            dkItem.use_date = null;

                                            dkItem.manage_department_id = null;
                                            dkItem.confirmed = null;
                                            dkItem.issued = null;
                                            dkItem.warehouse_id = device_Recall.warehouse_id;

                                        }

                                    }
                                }

                            }
                        }
                        if (process != null)
                        {
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
                                newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                                fileInfo = new FileInfo(newFileName);
                                if (fileInfo.Exists)
                                {
                                    fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                    fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                    newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                                }
                                processfile.file_path = "/Portals/" + dvid + "/Process/" + fileName;
                                processfile.process_files_name = fileName;

                                ffileData = fileData;
                                //Add ảnh
                                if (fileInfo != null)
                                {
                                    if (!Directory.Exists(fileInfo.Directory.FullName))
                                    {
                                        Directory.CreateDirectory(fileInfo.Directory.FullName);
                                    }
                                    File.Move(ffileData.LocalFileName, newFileName);

                                }

                                processfile.file_size = listSize[order];
                                processfile.file_type = helper.GetFileExtension(fileName);
                                processfile.device_process_id = device_Process.device_process_id;
                                processfile.created_date = DateTime.Now;
                                processfile.created_by = uid;
                                processfile.created_ip = ip;
                                processfile.modified_date = DateTime.Now;
                                processfile.modified_by = uid;
                                processfile.modified_ip = ip;
                                db.device_process_files.Add(processfile);
                                db.SaveChanges();
                                order++;
                            }
                        }

                        db.SaveChanges();

                        if (!check_add_next)
                        {
                            var device_Add = new device_process();
                            device_Add.approved_group_id = device_Process.approved_group_id;
                            device_Add.pre_process_id = process.device_process_id;
                            device_Add.device_note_id = process.device_note_id;
                            device_Add.device_process_code = process.device_process_code;
                            device_Add.device_process_type = process.device_process_type;
                            device_Add.content = null;
                            device_Add.organization_id = int.Parse(dvid);
                            device_Add.created_date = DateTime.Now;
                            device_Add.created_by = uid;
                            device_Add.created_ip = ip;
                            device_Add.date_send = DateTime.Now;
                            device_Add.modified_date = DateTime.Now;
                            device_Add.modified_by = uid;
                            device_Add.modified_ip = ip;
                            var device_Approved_Pre = db.device_approved_group.Where(s => s.approved_group_id == device_Add.approved_group_id).FirstOrDefault();
                            if (device_Approved_Pre.approved_type == 1)
                            {
                                device_Add.is_last = true;

                            }
                            else
                            if (device_Approved_Pre != null)
                            {
                                if (device_Approved_Pre.is_approved_by_department == false)
                                {
                                    var device_Approved_Users = db.device_approved_department_user.Where(s => s.approved_group_id == device_Approved_Pre.approved_group_id).FirstOrDefault();
                                    device_Add.approved_user_id = device_Approved_Users.approved_user_id;
                                    if (device_Approved_Pre.approved_type == 1 || device_Approved_Pre.approved_type == 3)
                                    {
                                        device_Add.approved_user_id = null;
                                    }
                                    device_Add.is_approved = false;
                                }
                                else
                                {
                                    var Or_user = db.sys_users.Where(s => s.user_id == uid).FirstOrDefault();
                                    var strC = dvid;
                                    if (Or_user.department_id != null)
                                    {
                                        strC = Or_user.department_id.ToString();
                                    }
                                    var srw = int.Parse(strC);
                                    var device_Approved_Department = db.device_approved_department_group.Where(s => s.approved_group_id == device_Approved_Pre.approved_group_id &&
                                    s.department_id == srw
                                    ).FirstOrDefault();

                                    if (device_Approved_Department != null)
                                    {
                                        device_Add.approved_user_id = device_Approved_Department.approved_user_id;
                                        device_Add.is_last = true;
                                        device_Add.is_approved = false;
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Nhóm duyệt không chứa phòng ban của bạn! Vui lòng cấu hình lại.", err = "1" });
                                    }

                                }
                            }
                            if (device_Add.approved_user_id != null)
                            {
                                #region sendhub
                                var userSenHub = db.sys_users.Where(s => (s.user_id == device_Add.approved_user_id)).FirstOrDefault();
                                var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.user_send = uid;
                                sh.module_key = const_module_key;
                                sh.receiver = userSenHub.user_id;
                                sh.icon = userSenHub.avatar;
                                sh.title = "Thiết bị";
                                sh.type = 6;
                                sh.date_send = DateTime.Now;
                                sh.id_key = device_Add.device_note_id.ToString();
                                sh.group_id = null;
                                sh.token_id = tid;
                                sh.created_date = DateTime.Now;
                                sh.created_by = uid;
                                sh.created_token_id = tid;
                                sh.created_ip = ip;
                                sh.is_type = 0;
                                if (device_Add.device_process_type == 1)
                                {
                                    var rePair_FL = db.device_repair.Where(s => (s.device_repair_id == device_Add.device_note_id)).FirstOrDefault();
                                    sh.contents = "Duyệt phiếu sửa chữa: " + rePair_FL.repair_number;


                                }
                                if (device_Add.device_process_type == 2)
                                {
                                    var inventory_FL = db.device_inventory_slip.Where(s => (s.inventory_slip_id == device_Add.device_note_id)).FirstOrDefault();
                                    sh.contents = "Duyệt phiếu kiểm kê: " + inventory_FL.inventory_number;

                                }
                                if (device_Add.device_process_type == 3)
                                {
                                    var recall_FL = db.device_recall.Where(s => (s.device_recall_id == device_Add.device_note_id)).FirstOrDefault();
                                    sh.contents = "Duyệt phiếu thu hồi: " + recall_FL.recall_number;


                                }
                                db.sys_sendhub.Add(sh);
                                db.SaveChanges();
                                #endregion
                            }
                            else
                            {
                                var device_Approved_Users = db.device_approved_department_user
                                .Where(s => s.approved_group_id == device_Add.approved_group_id).ToArray();
                                foreach (var item in device_Approved_Users)
                                {

                                    #region sendhub
                                    var userSenHub = db.sys_users.Where(s => (s.user_id == item.approved_user_id)).FirstOrDefault();
                                    var sh = new sys_sendhub();
                                    sh.senhub_id = helper.GenKey();
                                    sh.user_send = uid;
                                    sh.module_key = const_module_key;
                                    sh.receiver = userSenHub.user_id;
                                    sh.icon = userSenHub.avatar;
                                    sh.title = "Thiết bị";
                                    sh.type = 6;
                                    sh.date_send = DateTime.Now;
                                    sh.id_key = process.device_note_id.ToString();
                                    sh.group_id = null;
                                    sh.token_id = tid;
                                    sh.created_date = DateTime.Now;
                                    sh.created_by = uid;
                                    sh.created_token_id = tid;
                                    sh.created_ip = ip;
                                    sh.is_type = 0;
                                    if (process.device_process_type == 1)
                                    {
                                        var rePair_FL = db.device_repair.Where(s => (s.device_repair_id == process.device_note_id)).FirstOrDefault();
                                        sh.contents = "Duyệt phiếu sửa chữa: " + rePair_FL.repair_number;


                                    }
                                    if (process.device_process_type == 2)
                                    {
                                        var inventory_FL = db.device_inventory_slip.Where(s => (s.inventory_slip_id == process.device_note_id)).FirstOrDefault();
                                        sh.contents = "Duyệt phiếu kiểm kê: " + inventory_FL.inventory_number;


                                    }
                                    if (process.device_process_type == 3)
                                    {
                                        var recall_FL = db.device_recall.Where(s => (s.device_recall_id == process.device_note_id)).FirstOrDefault();
                                        sh.contents = "Duyệt phiếu thu hồi: " + recall_FL.recall_number;


                                    }
                                    db.sys_sendhub.Add(sh);
                                    db.SaveChanges();
                                    #endregion   }
                                }

                            }
                            db.device_process.Add(device_Add);
                            db.SaveChanges();
                        }

                        //#region add device_process_log
                        //device_process_log device_Process_Log = new device_process_log();
                        //device_Process_Log.content = "Trình duyệt";
                        //device_Process_Log.device_note_id = process.device_note_id;
                        //device_Process_Log.device_process_type = process.device_process_type;
                        //device_Process_Log.device_process_id = process.device_process_id;
                        //device_Process_Log.created_by = uid;
                        //device_Process_Log.created_date = DateTime.Now;
                        //device_Process_Log.created_ip = ip;
                        //db.device_process_log.Add(device_Process_Log);
                        //db.SaveChanges();



                        //var fddevice_process_log = db.device_process_log.Where(s => s.device_note_id == process.device_note_id && s.device_process_type == process.device_process_type
                        //&& s.device_process_id == null).ToArray();
                        //if (fddevice_process_log.Length > 0)
                        //{
                        //    foreach (var item in fddevice_process_log)
                        //    {
                        //        item.device_process_id = process.device_process_id;

                        //    }
                        //    db.SaveChanges();

                        //}


                        //#endregion
                        //#region add device_log
                        //if (helper.wlog)
                        //{

                        //    device_log log = new device_log();
                        //    log.title = "Thêm tiến trình xử lý " + process.device_process_code;

                        //    log.log_module = "device_process";
                        //    log.id_key = process.device_process_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.log_type = 0;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.device_log.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_process", ip, tid, "Lỗi khi thêm tiến trình xử lý", 0, "device_Handover");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_process", ip, tid, "Lỗi khi thêm tiến trình xử lý", 0, "device_Handover");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


        [HttpPut]
        public async Task<HttpResponseMessage> finish_device_process()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string fdprocess = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Process";
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
                        fdprocess = provider.FormData.GetValues("process").SingleOrDefault();
                        device_process process = JsonConvert.DeserializeObject<device_process>(fdprocess);


                        process.approved_user_id = uid;
                        process.date_approved = DateTime.Now;
                        process.is_approved = true;
                        db.Entry(process).State = EntityState.Modified;
                        db.SaveChanges();

                        device_process_files processfile = new device_process_files();
                        var fdlistSize = provider.FormData.GetValues("filesize").SingleOrDefault();
                        List<int> listSize = JsonConvert.DeserializeObject<List<int>>(fdlistSize);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int order = 0;


                        if (process.device_process_type == 1)
                        {
                            var fdDetailsRepair = "";
                            fdDetailsRepair = provider.FormData.GetValues("details").SingleOrDefault();
                            List<device_repair_details> liDetailsRepair = JsonConvert.DeserializeObject<List<device_repair_details>>(fdDetailsRepair);
                            if (liDetailsRepair != null)
                            {
                                foreach (var item in liDetailsRepair)
                                {


                                    db.Entry(item).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }

                        };


                        var add_Process = new device_process();
                        var device_Approved = db.device_approved_group.Where(s => s.approved_group_id == process.approved_group_id).FirstOrDefault();
                        var classify_D = device_Approved.classify.Split(',');
                        var check_class = false;
                        if (classify_D.Length > 0)
                            foreach (var item in classify_D)
                            {
                                if (item == "17")
                                {
                                    check_class = true;
                                }
                            }
                        if (device_Approved != null)
                        {
                            if (device_Approved.approved_type == 1 && device_Approved.is_approved_by_department == false)
                            {
                                if (process.device_process_type == 1 && check_class == true)
                                {
                                    var device_Repair = db.device_repair.Where(s => s.device_repair_id == process.device_note_id).FirstOrDefault();
                                    device_Repair.status = 2;
                                    #region sendhub
                                    var userSenHub = db.sys_users.Where(s => (s.user_id == device_Repair.created_by)).FirstOrDefault();
                                    var sh = new sys_sendhub();
                                    sh.senhub_id = helper.GenKey();
                                    sh.user_send = uid;
                                    sh.module_key = const_module_key;
                                    sh.receiver = userSenHub.user_id;
                                    sh.icon = userSenHub.avatar;
                                    sh.title = "Thiết bị";
                                    sh.type = 6;
                                    sh.date_send = DateTime.Now;
                                    sh.id_key = process.device_note_id.ToString();
                                    sh.group_id = null;
                                    sh.token_id = tid;
                                    sh.created_date = DateTime.Now;
                                    sh.created_by = uid;
                                    sh.created_token_id = tid;
                                    sh.created_ip = ip;
                                    sh.is_type = 2;

                                    sh.contents = "Hoàn thành sửa chữa: " + device_Repair.repair_number;

                                    db.sys_sendhub.Add(sh);
                                    db.SaveChanges();
                                    #endregion


                                    var device_Repair_Details = db.device_repair_details.Where(s => s.device_repair_id == device_Repair.device_repair_id).ToArray();
                                    foreach (var item in device_Repair_Details)
                                    {
                                        if (item.repair_condition == 2)
                                        {
                                            item.repair_condition = 4;
                                        }
                                        var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                        dkItem.assets_condition = item.condition;
                                        if (item.repair_condition == 3)
                                        {
                                            dkItem.nearest_user = dkItem.device_user_id;
                                            dkItem.device_user_id = null;
                                            dkItem.warehouse_id = dkItem.warehouse_id_old;
                                            dkItem.manage_department_id = null;
                                            dkItem.status = "HKS";
                                        }
                                        else
                                            dkItem.status = dkItem.old_status;
                                        dkItem.old_status = "DSC";
                                    }
                                }
                                if (process.device_process_type == 2 && check_class == true)
                                {
                                    var device_Inventory_Slip = db.device_inventory_slip.Where(s => s.inventory_slip_id == process.device_note_id).FirstOrDefault();
                                    device_Inventory_Slip.status = 4;
                                    #region sendhub
                                    var userSenHub = db.sys_users.Where(s => (s.user_id == device_Inventory_Slip.created_by)).FirstOrDefault();
                                    var sh = new sys_sendhub();
                                    sh.senhub_id = helper.GenKey();
                                    sh.user_send = uid;
                                    sh.module_key = const_module_key;
                                    sh.receiver = userSenHub.user_id;
                                    sh.icon = userSenHub.avatar;
                                    sh.title = "Thiết bị";
                                    sh.type = 6;
                                    sh.date_send = DateTime.Now;
                                    sh.id_key = process.device_note_id.ToString();
                                    sh.group_id = null;
                                    sh.token_id = tid;
                                    sh.created_date = DateTime.Now;
                                    sh.created_by = uid;
                                    sh.created_token_id = tid;
                                    sh.created_ip = ip;
                                    sh.is_type = 3;

                                    sh.contents = "Hoàn thành kiểm kê: " + device_Inventory_Slip.inventory_number;

                                    db.sys_sendhub.Add(sh);
                                    db.SaveChanges();
                                    #endregion
                                }
                                if (process.device_process_type == 3 && check_class == true)
                                {
                                    var device_Recall = db.device_recall.Where(s => s.device_recall_id == process.device_note_id).FirstOrDefault();
                                    device_Recall.status = 2;
                                    #region sendhub
                                    var userSenHub = db.sys_users.Where(s => (s.user_id == device_Recall.created_by)).FirstOrDefault();
                                    var sh = new sys_sendhub();
                                    sh.senhub_id = helper.GenKey();
                                    sh.user_send = uid;
                                    sh.module_key = const_module_key;
                                    sh.receiver = userSenHub.user_id;
                                    sh.icon = userSenHub.avatar;
                                    sh.title = "Thiết bị";
                                    sh.type = 6;
                                    sh.date_send = DateTime.Now;
                                    sh.id_key = process.device_note_id.ToString();
                                    sh.group_id = null;
                                    sh.token_id = tid;
                                    sh.created_date = DateTime.Now;
                                    sh.created_by = uid;
                                    sh.created_token_id = tid;
                                    sh.created_ip = ip;
                                    sh.is_type = 4;

                                    sh.contents = "Hoàn thành thu hồi: " + device_Recall.recall_number;

                                    db.sys_sendhub.Add(sh);
                                    db.SaveChanges();
                                    #endregion
                                    var device_Recall_Details = db.device_recall_details.Where(s => s.device_recall_id == device_Recall.device_recall_id).ToArray();
                                    foreach (var item in device_Recall_Details)
                                    {
                                        var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                        dkItem.old_status = dkItem.status;
                                        dkItem.status = "TK";
                                        dkItem.is_recall = true;
                                        dkItem.device_user_id = null;
                                        dkItem.use_date = null;

                                        dkItem.manage_department_id = null;
                                        dkItem.confirmed = null;
                                        dkItem.issued = null;
                                        dkItem.warehouse_id = device_Recall.warehouse_id;

                                    }
                                }

                            }
                            if (device_Approved.approved_type == 3 && device_Approved.is_approved_by_department == false)
                            {
                                if (process.device_process_type == 1)
                                {


                                    var device_Repair = db.device_repair.Where(s => s.device_repair_id == process.device_note_id).FirstOrDefault();
                                    device_Repair.status = 2;
                                    var device_Repair_Details = db.device_repair_details.Where(s => s.device_repair_id == device_Repair.device_repair_id).ToArray();
                                    foreach (var item in device_Repair_Details)
                                    {
                                        if (item.repair_condition == 2)
                                        {
                                            item.repair_condition = 4;
                                        }
                                        var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                        dkItem.assets_condition = item.condition;
                                        if (item.repair_condition == 3)
                                        {
                                            dkItem.nearest_user = dkItem.device_user_id;
                                            dkItem.device_user_id = null; dkItem.manage_department_id = null;
                                            dkItem.warehouse_id = dkItem.warehouse_id_old;
                                            dkItem.status = "HKS";
                                        }
                                        else
                                            dkItem.status = dkItem.old_status;
                                        dkItem.old_status = "DSC";
                                        db.SaveChanges();
                                    }

                                }
                                if (process.device_process_type == 2)
                                {

                                    var device_Repair = db.device_inventory_slip.Where(s => s.inventory_slip_id == process.device_note_id).FirstOrDefault();
                                    device_Repair.status = 4;


                                }
                                if (process.device_process_type == 3)
                                {

                                    var device_Recall = db.device_recall.Where(s => s.device_recall_id == process.device_note_id).FirstOrDefault();
                                    device_Recall.status = 2;
                                    var device_Recall_Details = db.device_recall_details.Where(s => s.device_recall_id == device_Recall.device_recall_id).ToArray();
                                    foreach (var item in device_Recall_Details)
                                    {
                                        var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                        dkItem.old_status = dkItem.status;
                                        dkItem.status = "TK";
                                        dkItem.is_recall = true;
                                        dkItem.device_user_id = null;
                                        dkItem.use_date = null;

                                        dkItem.manage_department_id = null;
                                        dkItem.confirmed = null;
                                        dkItem.issued = null;
                                        dkItem.warehouse_id = device_Recall.warehouse_id;
                                        db.SaveChanges();
                                    }

                                }
                            }
                            if (device_Approved.approved_type == 2 && device_Approved.is_approved_by_department == false)
                            {


                                if (process.device_process_type == 1)
                                {
                                    var device_Repair = db.device_repair.Where(s => s.device_repair_id == process.device_note_id).FirstOrDefault();
                                    device_Repair.status = 2;
                                    var device_Repair_Details = db.device_repair_details.Where(s => s.device_repair_id == device_Repair.device_repair_id).ToArray();
                                    foreach (var item in device_Repair_Details)
                                    {
                                        if (item.repair_condition == 2)
                                        {
                                            item.repair_condition = 4;
                                        }
                                        var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                        dkItem.assets_condition = item.condition;
                                        if (item.repair_condition == 3)
                                        {
                                            dkItem.nearest_user = dkItem.device_user_id;
                                            dkItem.device_user_id = null; dkItem.manage_department_id = null;
                                            dkItem.warehouse_id = dkItem.warehouse_id_old;
                                            dkItem.status = "HKS";
                                        }
                                        else
                                            dkItem.status = dkItem.old_status;
                                        dkItem.old_status = "DSC";
                                        db.SaveChanges();
                                    }
                                }
                                if (process.device_process_type == 2)
                                {
                                    var device_Repair = db.device_repair.Where(s => s.device_repair_id == process.device_note_id).FirstOrDefault();
                                    device_Repair.status = 4;
                                }
                                if (process.device_process_type == 3)
                                {
                                    var device_Recall = db.device_recall.Where(s => s.device_recall_id == process.device_note_id).FirstOrDefault();
                                    device_Recall.status = 2;
                                    var device_Recall_Details = db.device_recall_details.Where(s => s.device_recall_id == device_Recall.device_recall_id).ToArray();
                                    foreach (var item in device_Recall_Details)
                                    {
                                        var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                        dkItem.old_status = dkItem.status;
                                        dkItem.status = "TK";
                                        dkItem.is_recall = true;
                                        dkItem.device_user_id = null;
                                        dkItem.use_date = null;
                                        dkItem.assets_condition = item.condition;
                                        dkItem.manage_department_id = null;
                                        dkItem.confirmed = null;
                                        dkItem.issued = null;
                                        dkItem.warehouse_id = device_Recall.warehouse_id;
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        if (process != null)
                        {
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
                                newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                                fileInfo = new FileInfo(newFileName);
                                if (fileInfo.Exists)
                                {
                                    fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                    fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                    newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                                }
                                processfile.file_path = "/Portals/" + dvid + "/Process/" + fileName;
                                processfile.process_files_name = fileName;

                                ffileData = fileData;
                                //Add ảnh
                                if (fileInfo != null)
                                {
                                    if (!Directory.Exists(fileInfo.Directory.FullName))
                                    {
                                        Directory.CreateDirectory(fileInfo.Directory.FullName);
                                    }
                                    File.Move(ffileData.LocalFileName, newFileName);

                                }

                                processfile.file_size = listSize[order];
                                processfile.file_type = helper.GetFileExtension(fileName);
                                processfile.device_process_id = process.device_process_id;
                                processfile.created_date = DateTime.Now;
                                processfile.created_by = uid;
                                processfile.created_ip = ip;
                                processfile.modified_date = DateTime.Now;
                                processfile.modified_by = uid;
                                processfile.modified_ip = ip;
                                db.device_process_files.Add(processfile);
                                db.SaveChanges();
                                order++;
                            }
                        }

                        db.SaveChanges();




                        #region add device_process_log
                        device_process_log device_Process_Log = new device_process_log();
                        device_Process_Log.content = "Hoàn thành";
                        device_Process_Log.device_note_id = process.device_note_id;
                        device_Process_Log.device_process_type = process.device_process_type;
                        device_Process_Log.device_process_id = process.device_process_id;
                        device_Process_Log.created_by = uid;
                        device_Process_Log.created_date = DateTime.Now;
                        device_Process_Log.created_ip = ip;
                        db.device_process_log.Add(device_Process_Log);
                        db.SaveChanges();
                        #endregion
                        //#region add device_log
                        //if (helper.wlog)
                        //{

                        //    device_log log = new device_log();
                        //    log.title = "Thêm tiến trình xử lý " + process.device_process_code;

                        //    log.log_module = "device_process";
                        //    log.id_key = process.device_process_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.log_type = 0;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.device_log.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_process", ip, tid, "Lỗi khi thêm tiến trình xử lý", 0, "device_Handover");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_process", ip, tid, "Lỗi khi thêm tiến trình xử lý", 0, "device_Handover");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpPut]
        public async Task<HttpResponseMessage> return_device_process()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string fdprocess = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Process";
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
                        fdprocess = provider.FormData.GetValues("process").SingleOrDefault();
                        device_process process = JsonConvert.DeserializeObject<device_process>(fdprocess);
                        process.approved_user_id = uid;
                        process.date_approved = DateTime.Now;
                        process.is_approved = false;
                        process.is_returned = true;
                        db.Entry(process).State = EntityState.Modified;
                        db.SaveChanges();

                        device_process_files processfile = new device_process_files();
                        var fdlistSize = provider.FormData.GetValues("filesize").SingleOrDefault();
                        List<int> listSize = JsonConvert.DeserializeObject<List<int>>(fdlistSize);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int order = 0;
                        var li_App_D = db.device_approved_group.AsNoTracking().Where(s => s.approved_group_id == process.approved_group_id
                   ).FirstOrDefault();
                        var device_App_Gr = new device_approved_department_user();
                        var check = false;
                        if (li_App_D.approved_type == 2)
                        {
                            device_App_Gr = db.device_approved_department_user.AsNoTracking().Where(s => s.approved_group_id == process.approved_group_id
                               ).OrderBy(s => s.approved_department_user_id).FirstOrDefault();
                            if (device_App_Gr.approved_user_id == process.approved_user_id)
                                check = true;
                        }
                        else if (li_App_D.approved_type == 3)
                        {
                            var device_Pr_C = db.device_process.AsNoTracking().Where(s => s.pre_process_id == process.pre_process_id
                                  ).OrderBy(s => s.device_process_id).FirstOrDefault();
                            if (device_Pr_C.approved_user_id == process.approved_user_id)
                                check = true;
                        }
                        else
                        {
                            check = true;
                        }
                        var device_Process_Old = new device_process();
                        var listProcessOld = new List<String>();
                        if (!check)
                        {
                            var li_App_Gr_N = new List<String>();
                            if (li_App_D.approved_type == 2)
                            {
                                li_App_Gr_N = db.device_approved_department_user.AsNoTracking().Where(s => s.approved_group_id == process.approved_group_id
                                 ).OrderBy(s => s.approved_department_user_id).Select(s => s.approved_user_id).ToList();
                            }
                            else
                            {
                                li_App_Gr_N = db.device_process.AsNoTracking().Where(s => s.pre_process_id == process.pre_process_id
                                 ).OrderBy(s => s.device_process_id).Select(s => s.approved_user_id).ToList();
                            }
                            for (int i = 0; i < li_App_Gr_N.Count; i++)
                            {
                                if (
                                li_App_Gr_N[i] == process.approved_user_id
                                )
                                {
                                    var appTemp = li_App_Gr_N[i - 1];
                                    device_Process_Old = db.device_process.AsNoTracking().Where(s => s.approved_user_id == appTemp &&
                                    s.approved_group_id == process.approved_group_id && s.device_note_id == process.device_note_id &&
                                   process.device_process_code == s.device_process_code && s.is_approved == true && process.device_process_type == s.device_process_type
                                   ).OrderByDescending(s => s.device_process_id).FirstOrDefault();
                                    var liTemp = new List<String>();
                                    foreach (var item in listProcessOld)
                                    {
                                        if (item != appTemp)
                                            liTemp.Add(item);
                                    }
                                    listProcessOld = liTemp;
                                    break;

                                }
                                listProcessOld.Add(li_App_Gr_N[i]);
                            }

                        }
                        else
                        {
                            if (process.pre_process_id != null)
                            {
                                device_Process_Old = db.device_process.AsNoTracking().Where(s => s.device_process_id == process.pre_process_id).FirstOrDefault();
                                if (device_Process_Old.approved_group_id == null)
                                {
                                    device_Process_Old = null;
                                }
                            }
                            else
                                device_Process_Old = null;
                        }
                        //var time = DateTime.Now;
                        //var list_Process_Old = db.device_process.AsNoTracking().Where(s =>  s.device_note_id == process.device_note_id &&
                        //process.device_process_code == s.device_process_code && s.is_approved == true
                        //&& s.approved_user_id != process.created_by && s.date_send == device_Process_Old.date_send
                        //).OrderBy(s => s.device_process_id).ToArray();
                        //if(list_Process_Old != null)
                        //{
                        //    foreach (var item in list_Process_Old)
                        //    {
                        //        item.date_send = time;
                        //        db.device_process.Add(item);
                        //        db.SaveChanges();

                        //    }
                        //}
                        var add_Process = new device_process();
                        var timeNow = DateTime.Now;
                        //var device_Process_Files_Old = db.device_process_files.AsNoTracking().Where(s => s.device_process_id == device_Process_Old.device_process_id).ToArray();

                        var device_Approved_Users = db.device_approved_department_user.Where(s => s.approved_group_id == process.approved_group_id).OrderBy(s => s.approved_department_user_id).ToArray();

                        if (device_Process_Old != null)
                        {

                            var device_Approved = db.device_approved_group.Where(s => s.approved_group_id == device_Process_Old.approved_group_id).FirstOrDefault();
                            if (device_Approved != null)
                            {



                                if (device_Approved.is_approved_by_department == false)
                                {
                                    if (listProcessOld.Count > 0)
                                    {
                                        foreach (var item in listProcessOld)
                                        {

                                            var element = db.device_process.AsNoTracking().Where(s => s.approved_user_id == item &&
                                  s.approved_group_id == process.approved_group_id && s.device_note_id == process.device_note_id &&
                                 process.device_process_code == s.device_process_code && s.is_approved == true && process.device_process_type == s.device_process_type
                                      ).OrderByDescending(s => s.device_process_id).FirstOrDefault();
                                            var addPreProcess = new device_process();
                                            addPreProcess = element;
                                            addPreProcess.date_send = timeNow;
                                            db.device_process.Add(addPreProcess);
                                            db.SaveChanges();
                                        }
                                    }

                                    add_Process = device_Process_Old;
                                    add_Process.content = null;

                                    add_Process.approved_user_id = device_Process_Old.approved_user_id;
                                    add_Process.created_by = uid;
                                    add_Process.created_date = DateTime.Now;
                                    add_Process.created_ip = ip;
                                    add_Process.returned_content = null;
                                    add_Process.is_approved = false;
                                    add_Process.date_send = timeNow;
                                    add_Process.date_approved = null;
                                    db.device_process.Add(add_Process);
                                    db.SaveChanges();
                                    #region sendhub
                                    var userSenHub = db.sys_users.Where(s => (s.user_id == device_Process_Old.approved_user_id)).FirstOrDefault();
                                    var sh = new sys_sendhub();
                                    sh.senhub_id = helper.GenKey();
                                    sh.user_send = uid;
                                    sh.module_key = const_module_key;
                                    sh.receiver = userSenHub.user_id;
                                    sh.icon = userSenHub.avatar;
                                    sh.title = "Thiết bị";
                                    sh.type = 6;
                                    sh.date_send = DateTime.Now;
                                    sh.id_key = add_Process.device_note_id.ToString();
                                    sh.group_id = null;
                                    sh.token_id = tid;
                                    sh.created_date = DateTime.Now;
                                    sh.created_by = uid;
                                    sh.created_token_id = tid;
                                    sh.created_ip = ip;
                                    sh.is_type = 0;

                                    if (add_Process.device_process_type == 1)
                                    {
                                        var rePair_FL = db.device_repair.Where(s => (s.device_repair_id == add_Process.device_note_id)).FirstOrDefault();
                                        sh.contents = "Người duyệt trả lại phiếu sửa chữa: " + rePair_FL.repair_number;


                                    }
                                    if (add_Process.device_process_type == 2)
                                    {
                                        var inventory_FL = db.device_inventory_slip.Where(s => (s.inventory_slip_id == add_Process.device_note_id)).FirstOrDefault();
                                        sh.contents = "Người duyệt trả lại phiếu kiểm kê: " + inventory_FL.inventory_number;


                                    }
                                    if (add_Process.device_process_type == 3)
                                    {
                                        var recall_FL = db.device_recall.Where(s => (s.device_recall_id == add_Process.device_note_id)).FirstOrDefault();
                                        sh.contents = "Người duyệt trả lại phiếu thu hồi: " + recall_FL.recall_number;


                                    }
                                    db.sys_sendhub.Add(sh);
                                    db.SaveChanges();
                                    #endregion


                                }
                                else
                                {
                                    add_Process = device_Process_Old;
                                    add_Process.content = null;

                                    add_Process.approved_user_id = device_Process_Old.approved_user_id;
                                    add_Process.created_by = uid;
                                    add_Process.created_date = DateTime.Now;
                                    add_Process.created_ip = ip;
                                    add_Process.returned_content = null;
                                    add_Process.is_approved = false;
                                    add_Process.date_send = DateTime.Now;
                                    add_Process.date_approved = null;
                                    db.device_process.Add(add_Process);
                                    db.SaveChanges();
                                    #region sendhub
                                    var userSenHub = db.sys_users.Where(s => (s.user_id == device_Process_Old.approved_user_id)).FirstOrDefault();
                                    var sh = new sys_sendhub();
                                    sh.senhub_id = helper.GenKey();
                                    sh.user_send = uid;
                                    sh.module_key = const_module_key;
                                    sh.receiver = userSenHub.user_id;
                                    sh.icon = userSenHub.avatar;
                                    sh.title = "Thiết bị";
                                    sh.type = 6;
                                    sh.date_send = DateTime.Now;
                                    sh.id_key = add_Process.device_note_id.ToString();
                                    sh.group_id = null;
                                    sh.token_id = tid;
                                    sh.created_date = DateTime.Now;
                                    sh.created_by = uid;
                                    sh.created_token_id = tid;
                                    sh.created_ip = ip;
                                    sh.is_type = 0;

                                    if (add_Process.device_process_type == 1)
                                    {
                                        var rePair_FL = db.device_repair.Where(s => (s.device_repair_id == add_Process.device_note_id)).FirstOrDefault();
                                        sh.contents = "Người duyệt trả lại phiếu sửa chữa: " + rePair_FL.repair_number;


                                    }
                                    if (add_Process.device_process_type == 2)
                                    {
                                        var inventory_FL = db.device_inventory_slip.Where(s => (s.inventory_slip_id == add_Process.device_note_id)).FirstOrDefault();
                                        sh.contents = "Người duyệt trả lại phiếu kiểm kê: " + inventory_FL.inventory_number;


                                    }
                                    if (add_Process.device_process_type == 3)
                                    {
                                        var recall_FL = db.device_recall.Where(s => (s.device_recall_id == add_Process.device_note_id)).FirstOrDefault();
                                        sh.contents = "Người duyệt trả lại phiếu thu hồi: " + recall_FL.recall_number;


                                    }
                                    db.sys_sendhub.Add(sh);
                                    db.SaveChanges();
                                    #endregion

                                }


                            }


                            db.SaveChanges();

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
                                newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                                fileInfo = new FileInfo(newFileName);
                                if (fileInfo.Exists)
                                {
                                    fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                    fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                    newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                                }
                                processfile.file_path = "/Portals/" + dvid + "/Process/" + fileName;
                                processfile.process_files_name = fileName;

                                ffileData = fileData;
                                //Add ảnh
                                if (fileInfo != null)
                                {
                                    if (!Directory.Exists(fileInfo.Directory.FullName))
                                    {
                                        Directory.CreateDirectory(fileInfo.Directory.FullName);
                                    }
                                    File.Move(ffileData.LocalFileName, newFileName);

                                }

                                processfile.file_size = listSize[order];
                                processfile.file_type = helper.GetFileExtension(fileName);
                                processfile.device_process_id = process.device_process_id;
                                processfile.created_date = DateTime.Now;
                                processfile.created_by = uid;
                                processfile.created_ip = ip;
                                processfile.modified_date = DateTime.Now;
                                processfile.modified_by = uid;
                                processfile.modified_ip = ip;
                                db.device_process_files.Add(processfile);
                                db.SaveChanges();
                                order++;
                            }
                        }
                        else if (process.device_process_type == 1)
                        {
                            var device_Repair_R = db.device_repair.Where(s => s.repair_number == process.device_process_code && s.device_repair_id == process.device_note_id).FirstOrDefault();
                            if (device_Repair_R != null)
                            {
                                device_Repair_R.is_return = process.device_process_id;
                                device_Repair_R.date_return = DateTime.Now;
                                device_Repair_R.status = 3;
                                db.SaveChanges();

                                var device_Repair_Details = db.device_repair_details.Where(s => s.device_repair_id == device_Repair_R.device_repair_id).ToArray();
                                foreach (var item in device_Repair_Details)
                                {
                                    if (item.repair_condition == 2)
                                    {
                                        item.repair_condition = 4;
                                    }
                                    var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                    dkItem.assets_condition = item.condition;
                                    if (item.repair_condition == 3)
                                    {
                                        dkItem.nearest_user = dkItem.device_user_id;
                                        dkItem.device_user_id = null; dkItem.manage_department_id = null;
                                        dkItem.warehouse_id = dkItem.warehouse_id_old;
                                        dkItem.status = "HKS";
                                    }
                                    else
                                        dkItem.status = dkItem.old_status;
                                    dkItem.old_status = "DSC";
                                }
                            }

                        }
                        else if (process.device_process_type == 2)
                        {
                            var device_Inventory_Slip = db.device_inventory_slip.Where(s => s.inventory_number == process.device_process_code && s.inventory_slip_id == process.device_note_id).FirstOrDefault();
                            if (device_Inventory_Slip != null)
                            {
                                device_Inventory_Slip.is_return = process.device_process_id;
                                device_Inventory_Slip.date_return = DateTime.Now;
                                device_Inventory_Slip.status = 5;
                                db.SaveChanges();


                            }

                        }

                        else if (process.device_process_type == 3)
                        {
                            var device_Inventory_Slip = db.device_recall.Where(s => s.recall_number == process.device_process_code && s.device_recall_id == process.device_note_id).FirstOrDefault();
                            if (device_Inventory_Slip != null)
                            {
                                device_Inventory_Slip.is_return = process.device_process_id;
                                device_Inventory_Slip.date_return = DateTime.Now;
                                device_Inventory_Slip.status = 3;
                                db.SaveChanges();


                            }

                        }
                        #region add device_process_log
                        device_process_log device_Process_Log = new device_process_log();
                        device_Process_Log.content = "Trả lại người duyệt trước";
                        device_Process_Log.device_note_id = process.device_note_id;
                        device_Process_Log.device_process_type = process.device_process_type;
                        device_Process_Log.device_process_id = process.device_process_id;
                        device_Process_Log.created_by = uid;
                        device_Process_Log.created_date = DateTime.Now;
                        device_Process_Log.created_ip = ip;
                        db.device_process_log.Add(device_Process_Log);
                        db.SaveChanges();
                        #endregion

                        //#region add device_log
                        //if (helper.wlog)
                        //{

                        //    device_log log = new device_log();
                        //    log.title = "Thêm tiến trình xử lý " + process.device_process_code;

                        //    log.log_module = "device_process";
                        //    log.id_key = process.device_process_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.log_type = 0;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.device_log.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_process", ip, tid, "Lỗi khi thêm tiến trình xử lý", 0, "device_Handover");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_process", ip, tid, "Lỗi khi thêm tiến trình xử lý", 0, "device_Handover");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> return_device_repair()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string fdprocess = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Process";
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
                        fdprocess = provider.FormData.GetValues("process").SingleOrDefault();
                        device_process process = JsonConvert.DeserializeObject<device_process>(fdprocess);
                        process.approved_user_id = uid;
                        process.date_approved = DateTime.Now;
                        process.is_approved = false;
                        process.is_returned = true;
                        process.is_type = true;
                        db.Entry(process).State = EntityState.Modified;
                        db.SaveChanges();

                        device_process_files processfile = new device_process_files();
                        var fdlistSize = provider.FormData.GetValues("filesize").SingleOrDefault();
                        List<int> listSize = JsonConvert.DeserializeObject<List<int>>(fdlistSize);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int order = 0;
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                            }
                            processfile.file_path = "/Portals/" + dvid + "/Process/" + fileName;
                            processfile.process_files_name = fileName;

                            ffileData = fileData;
                            //Add ảnh
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }

                            processfile.file_size = listSize[order];
                            processfile.file_type = helper.GetFileExtension(fileName);
                            processfile.device_process_id = process.device_process_id;
                            processfile.created_date = DateTime.Now;
                            processfile.created_by = uid;
                            processfile.created_ip = ip;
                            processfile.modified_date = DateTime.Now;
                            processfile.modified_by = uid;
                            processfile.modified_ip = ip;
                            db.device_process_files.Add(processfile);
                            db.SaveChanges();
                            order++;
                        }











                        if (process.device_process_type == 1)
                        {

                            var device_Repair_R = db.device_repair.Where(s => s.repair_number == process.device_process_code && s.device_repair_id == process.device_note_id).FirstOrDefault();
                            if (device_Repair_R != null)
                            {
                                device_Repair_R.is_return = process.device_process_id;
                                device_Repair_R.date_return = DateTime.Now;
                                device_Repair_R.status = 3;
                                db.SaveChanges();
                                var device_Repair_Details = db.device_repair_details.Where(s => s.device_repair_id == device_Repair_R.device_repair_id).ToArray();
                                foreach (var item in device_Repair_Details)
                                {
                                    var dkItem = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                    dkItem.status = dkItem.old_status;
                                    dkItem.old_status = "DSC";
                                }
                                #region sendhub
                                var userSenHub = db.sys_users.Where(s => (s.user_id == device_Repair_R.created_by)).FirstOrDefault();
                                var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.user_send = uid;
                                sh.module_key = const_module_key;
                                sh.receiver = userSenHub.user_id;
                                sh.icon = userSenHub.avatar;
                                sh.title = "Thiết bị";
                                sh.type = 6;
                                sh.date_send = DateTime.Now;
                                sh.id_key = process.device_note_id.ToString();
                                sh.group_id = null;
                                sh.token_id = tid;
                                sh.created_date = DateTime.Now;
                                sh.created_by = uid;
                                sh.created_token_id = tid;
                                sh.created_ip = ip;
                                sh.is_type = 2;

                                sh.contents = "Trả lại phiếu sửa chữa: " + device_Repair_R.repair_number;

                                db.sys_sendhub.Add(sh);
                                db.SaveChanges();
                                #endregion
                            }
                        }

                        else if (process.device_process_type == 2)
                        {
                            var device_Inventory_Slip = db.device_inventory_slip.Where(s => s.inventory_number == process.device_process_code && s.inventory_slip_id == process.device_note_id).FirstOrDefault();
                            if (device_Inventory_Slip != null)
                            {
                                device_Inventory_Slip.is_return = process.device_process_id;
                                device_Inventory_Slip.date_return = DateTime.Now;
                                device_Inventory_Slip.status = 5;
                                db.SaveChanges();
                                #region sendhub
                                var userSenHub = db.sys_users.Where(s => (s.user_id == device_Inventory_Slip.created_by)).FirstOrDefault();
                                var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.user_send = uid;
                                sh.module_key = const_module_key;
                                sh.receiver = userSenHub.user_id;
                                sh.icon = userSenHub.avatar;
                                sh.title = "Thiết bị";
                                sh.type = 6;
                                sh.date_send = DateTime.Now;
                                sh.id_key = process.device_note_id.ToString();
                                sh.group_id = null;
                                sh.token_id = tid;
                                sh.created_date = DateTime.Now;
                                sh.created_by = uid;
                                sh.created_token_id = tid;
                                sh.created_ip = ip;
                                sh.is_type = 3;

                                sh.contents = "Trả lại phiếu kiểm kê: " + device_Inventory_Slip.inventory_number;

                                db.sys_sendhub.Add(sh);
                                db.SaveChanges();
                                #endregion

                            }

                        }
                        else if (process.device_process_type == 3)
                        {
                            var device_Inventory_Slip = db.device_recall.Where(s => s.recall_number == process.device_process_code && s.device_recall_id == process.device_note_id).FirstOrDefault();
                            if (device_Inventory_Slip != null)
                            {
                                device_Inventory_Slip.is_return = process.device_process_id;
                                device_Inventory_Slip.date_return = DateTime.Now;
                                device_Inventory_Slip.status = 3;
                                db.SaveChanges();
                                #region sendhub
                                var userSenHub = db.sys_users.Where(s => (s.user_id == device_Inventory_Slip.created_by)).FirstOrDefault();
                                var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.user_send = uid;
                                sh.module_key = const_module_key;
                                sh.receiver = userSenHub.user_id;
                                sh.icon = userSenHub.avatar;
                                sh.title = "Thiết bị";
                                sh.type = 6;
                                sh.date_send = DateTime.Now;
                                sh.id_key = process.device_note_id.ToString();
                                sh.group_id = null;
                                sh.token_id = tid;
                                sh.created_date = DateTime.Now;
                                sh.created_by = uid;
                                sh.created_token_id = tid;
                                sh.created_ip = ip;
                                sh.is_type = 4;

                                sh.contents = "Trả lại phiếu thu hồi: " + device_Inventory_Slip.recall_number;

                                db.sys_sendhub.Add(sh);
                                db.SaveChanges();
                                #endregion


                            }

                        }
                        #region add device_process_log
                        device_process_log device_Process_Log = new device_process_log();
                        device_Process_Log.content = "Trả lại người tạo ";
                        device_Process_Log.device_note_id = process.device_note_id;
                        device_Process_Log.device_process_type = process.device_process_type;
                        device_Process_Log.device_process_id = process.device_process_id;
                        device_Process_Log.created_by = uid;
                        device_Process_Log.created_date = DateTime.Now;
                        device_Process_Log.created_ip = ip;
                        db.device_process_log.Add(device_Process_Log);
                        db.SaveChanges();
                        #endregion
                        //#region add device_log
                        //if (helper.wlog)
                        //{

                        //    device_log log = new device_log();
                        //    log.title = "Thêm tiến trình xử lý " + process.device_process_code;

                        //    log.log_module = "device_process";
                        //    log.id_key = process.device_process_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.log_type = 0;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.device_log.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_process", ip, tid, "Lỗi khi thêm tiến trình xử lý", 0, "device_Handover");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_process", ip, tid, "Lỗi khi thêm tiến trình xử lý", 0, "device_Handover");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_device_process()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdlang = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string fdprocess = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Process";
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
                        fdprocess = provider.FormData.GetValues("process").SingleOrDefault();
                        device_process process = JsonConvert.DeserializeObject<device_process>(fdprocess);
                        process.modified_date = DateTime.Now;
                        process.modified_by = uid;
                        process.modified_ip = ip;
                        db.Entry(process).State = EntityState.Modified;
                        var fduserfollows = "";
                        fduserfollows = provider.FormData.GetValues("userfollows").SingleOrDefault();
                        List<device_user_follows> device_User_s = JsonConvert.DeserializeObject<List<device_user_follows>>(fduserfollows);
                        foreach (var item in device_User_s)
                        {
                            item.device_process_id = process.device_process_id;
                            item.organization_id = int.Parse(dvid);
                            item.created_date = DateTime.Now;
                            item.created_by = uid;
                            item.created_ip = ip;
                            db.device_user_follows.Add(item);
                        }

                        List<string> paths = new List<string>();
                        string fddevice_process_files = provider.FormData.GetValues("processfiles").SingleOrDefault();
                        List<device_process_files> device_process_files = JsonConvert.DeserializeObject<List<device_process_files>>(fddevice_process_files);

                        var device_process_filesOld = db.device_process_files.Where(s => s.device_process_id == process.device_process_id).ToArray();
                        if (device_process_filesOld.Length > 0)
                        {

                            List<device_process_files> del = new List<device_process_files>();
                            foreach (var item in device_process_filesOld)
                            {
                                var checkDel = false;
                                foreach (var element in device_process_files)
                                {
                                    if (element.process_files_id == item.process_files_id)
                                    {
                                        checkDel = true;
                                        break;
                                    }
                                }
                                if (!checkDel)
                                {
                                    del.Add(item);
                                    if (!string.IsNullOrWhiteSpace(item.file_path))
                                        paths.Add(  item.file_path.Substring(8));
                                }


                            }
                            db.device_process_files.RemoveRange(del);
                        }



                        device_process_files processfile = new device_process_files();
                        var fdlistSize = provider.FormData.GetValues("filesize").SingleOrDefault();
                        List<double> listSize = JsonConvert.DeserializeObject<List<double>>(fdlistSize);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int order = 0;
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Process", fileName);
                            }
                            processfile.file_path = "/Portals/" + dvid + "/Process/" + fileName;
                            processfile.process_files_name = fileName;

                            ffileData = fileData;
                            //Add ảnh
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }
                            processfile.file_size = listSize[order];
                            processfile.file_type = helper.GetFileExtension(fileName);

                            processfile.device_process_id = process.device_process_id;
                            processfile.created_date = DateTime.Now;
                            processfile.created_by = uid;
                            processfile.created_ip = ip;
                            processfile.modified_date = DateTime.Now;
                            processfile.modified_by = uid;
                            processfile.modified_ip = ip;
                            db.device_process_files.Add(processfile);

                            order++;
                        }



                        #region add device_log
                        if (helper.wlog)
                        {

                            device_log log = new device_log();
                            log.title = "Sửa tiến trình xử lý " + process.device_process_code;

                            log.log_module = "device_process";
                            log.log_type = 1;
                            log.id_key = process.device_process_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.device_log.Add(log);

                        }
                        #endregion

                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Process/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Process/" + Path.GetFileName(strP));
                        }
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Handover/update_device_process", ip, tid, "Lỗi khi cập nhật tiến trình xử lý", 0, "device_Handover");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Handover/update_device_process", ip, tid, "Lỗi khi cập nhật tiến trình xử lý", 0, "device_Handover");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_device_process([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;

            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.device_process.Where(a => id.Contains(a.device_process_id)).ToListAsync();

                        var das2 = await db.device_process_files.Where(a => id.Contains(a.device_process_id)).ToListAsync();

                        var das3 = await db.device_user_follows.Where(a => id.Contains(a.device_process_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das.Count > 0)
                        {
                            List<device_process> del = new List<device_process>();

                            foreach (var da in das)
                            {

                                if (uid == da.created_by || (int.Parse(dvid) == da.organization_id && ad) || super)
                                {

                                    del.Add(da);
                                    #region add device_log
                                    if (helper.wlog)
                                    {

                                        device_log log = new device_log();
                                        log.title = "Xóa tiến trình xử lý " + da.device_process_code;
                                        log.log_module = "device_process";
                                        log.log_type = 2;
                                        log.id_key = da.device_process_id.ToString();
                                        log.created_date = DateTime.Now;
                                        log.created_by = uid;
                                        log.created_token_id = tid;
                                        log.created_ip = ip;
                                        db.device_log.Add(log);
                                        db.SaveChanges();


                                    }
                                    #endregion
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.device_process.RemoveRange(del);
                        }

                        if (das2.Count > 0)
                        {
                            List<device_process_files> del2 = new List<device_process_files>();

                            foreach (var da in das2)
                            {
                                del2.Add(da);

                                if (!string.IsNullOrWhiteSpace(da.file_path))
                                    paths.Add(da.file_path);
                            }

                            db.device_process_files.RemoveRange(del2);
                        }
                        if (das3.Count > 0)
                        {
                            List<device_user_follows> del3 = new List<device_user_follows>();

                            foreach (var da in das3)
                            {
                                del3.Add(da);
                            }

                            db.device_user_follows.RemoveRange(del3);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strP in paths)
                        {
                            string pathDel = Path.Combine(HttpContext.Current.Server.MapPath("~/Portals/"), Path.GetFileName(dvid), "Process", Path.GetFileName(strP));
                            bool exists = File.Exists(pathDel);
                            if (exists)
                                System.IO.File.Delete(pathDel);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Handover/delete_device_process", ip, tid, "Lỗi khi xoá tiến trình xử lý", 0, "device_Handover");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Handover/delete_device_process", ip, tid, "Lỗi khi xoá tiến trình xử lý", 0, "device_Handover");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
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