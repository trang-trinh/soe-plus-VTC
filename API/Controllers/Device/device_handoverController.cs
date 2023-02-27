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
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Helper;
using API.Models;
using GemBox.Document;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Controllers.Device
{
    [Authorize(Roles = "login")]
    public class device_handoverController : ApiController
    {
        private const string const_module_key = "M7";
        public string getipaddress()
        {
       return  HttpContext.Current.Request.UserHostAddress;
        }
 

        [HttpPost]
        public async Task<HttpResponseMessage> add_device_handover()
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
                    string fdhandover = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Device";
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
                        fdhandover = provider.FormData.GetValues("handover").SingleOrDefault();
                        device_handover handover = JsonConvert.DeserializeObject<device_handover>(fdhandover);
                        handover.organization_id = int.Parse(dvid);
                        handover.created_date = DateTime.Now;
                        handover.created_by = uid;
                        handover.created_ip = ip;
                        handover.modified_date = DateTime.Now;
                        handover.modified_by = uid;
                        handover.modified_ip = ip;
                        var handoverOld = db.device_handover.Where(s => s.handover_number == handover.handover_number && handover.organization_id == s.organization_id
                        ).FirstOrDefault();
                        if (handoverOld != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Đã tồn tại mã phiếu trong cơ sở dữ liệu. Vui lòng nhập lại!", err = "1" });
                    
                        }
                        db.device_handover.Add(handover);
                        db.SaveChanges();
                        device_process device_Process_Pre = new device_process();
                        device_Process_Pre.device_process_code = handover.handover_number;
                        device_Process_Pre.date_approved = DateTime.Now;
                        device_Process_Pre.approved_group_id = -2;
                        device_Process_Pre.device_process_type = -2;
                        device_Process_Pre.device_note_id = handover.handover_id;
                        device_Process_Pre.is_order = 0;
                        device_Process_Pre.is_view = true;
                        device_Process_Pre.date_view = DateTime.Now;
                        device_Process_Pre.organization_id = int.Parse(dvid);
                        device_Process_Pre.created_date = DateTime.Now;
                        device_Process_Pre.created_by = handover.user_deliver_id;
                        device_Process_Pre.created_ip = ip;
                        device_Process_Pre.date_send = DateTime.Now;
                        device_Process_Pre.modified_date = DateTime.Now;
                        device_Process_Pre.modified_by = handover.user_receiver_id;
                        device_Process_Pre.modified_ip = ip;
                        device_Process_Pre.approved_user_id = handover.user_verifier_id;
                        device_Process_Pre.is_approved = true;
                        device_Process_Pre.is_last = true;

                        db.device_process.Add(device_Process_Pre);

                        var fdlistSize = provider.FormData.GetValues("filesize").SingleOrDefault();
                        List<int> listSize = JsonConvert.DeserializeObject<List<int>>(fdlistSize);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int order = 0;
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            device_handover_files handoverfile = new device_handover_files();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Device", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Device", fileName);
                            }
                            handoverfile.file_path = "/Portals/" + dvid + "/Device/" + fileName;
                            handoverfile.file_name = fileName;

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
                            handoverfile.file_size = listSize[order];
                            handoverfile.file_type = helper.GetFileExtension(fileName);
                            handoverfile.handover_id = handover.handover_id;
                            handoverfile.created_date = DateTime.Now;
                            handoverfile.created_by = uid;
                            handoverfile.created_ip = ip;
                            handoverfile.modified_date = DateTime.Now;
                            handoverfile.modified_by = uid;
                            handoverfile.modified_ip = ip;
                            db.device_handover_files.Add(handoverfile);
                            db.SaveChanges();
                            order++;
                        }

                        string fdhandoverattach = provider.FormData.GetValues("handoverattach").SingleOrDefault();
                        List<device_handover_attach> handoverattach = JsonConvert.DeserializeObject<List<device_handover_attach>>(fdhandoverattach);

                        if (handoverattach.Count > 0)
                        {

                            foreach (var item in handoverattach)
                            {
                                var fdcard = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                               
                                if (fdcard != null)
                                {
                                    fdcard.old_status = fdcard.status;
                                    fdcard.status = "CXN";
                                    fdcard.is_recall = false;
                                    fdcard.check_recall = null;
                                    fdcard.device_user_id = handover.user_receiver_id;
                                  
                                }
                                item.user_id = handover.user_receiver_id;
                                item.organization_id = int.Parse(dvid);
                                item.handover_id = handover.handover_id;
                                item.created_date = DateTime.Now;
                                item.created_by = uid;
                                item.created_ip = ip;
                                item.modified_date = DateTime.Now;
                                item.modified_by = uid;
                                item.modified_ip = ip;
                                db.device_handover_attach.Add(item);
                                db.SaveChanges();
                                 if (handover.device_repair_id != null)
                                {
                                    if (item.repair_details_id != null)
                                    {
                                        var fdReplace = db.device_repair_details.Where(s => s.repair_details_id == item.repair_details_id).FirstOrDefault();
                                        fdReplace.is_replace = item.handover_attach_id;
                                    }
                                }
                            }

                        }
                        #region add device_log
                        if (helper.wlog)
                        {

                            device_log log = new device_log();
                            log.title = "Thêm biên bản bàn giao tài sản " + handover.handover_number;

                            log.log_module = "device_handover";
                            log.id_key = handover.handover_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_handover", ip, tid, "Lỗi khi thêm biên bản bàn giao tài sản", 0, "device_Handover");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Handover/add_device_handover", ip, tid, "Lỗi khi thêm biên bản bàn giao tài sản", 0, "device_Handover");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_device_handover()
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
                    string fdhandover = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Device";
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
                        fdhandover = provider.FormData.GetValues("handover").SingleOrDefault();
                        device_handover handover = JsonConvert.DeserializeObject<device_handover>(fdhandover);
                        handover.modified_date = DateTime.Now;
                        handover.modified_by = uid;
                        handover.modified_ip = ip;
                        db.Entry(handover).State = EntityState.Modified;
                        var device_handover_process = db.device_process.Where(s => s.device_note_id == handover.handover_id && s.device_process_type==-2).FirstOrDefault();
                        device_handover_process.created_by = handover.user_deliver_id;
                        device_handover_process.modified_by = handover.user_receiver_id;
          
                        db.Entry(device_handover_process).State = EntityState.Modified;
                        List<string> paths = new List<string>();
                        string fddevice_handover_files = provider.FormData.GetValues("handoverfiles").SingleOrDefault();
                        List<device_handover_files> device_handover_files = JsonConvert.DeserializeObject<List<device_handover_files>>(fddevice_handover_files);

                        var device_handover_filesOld = db.device_handover_files.Where(s => s.handover_id == handover.handover_id).ToArray();
                        if (device_handover_filesOld.Length > 0)
                        {

                            List<device_handover_files> del = new List<device_handover_files>();
                            foreach (var item in device_handover_filesOld)
                            {
                                var checkDel = false;
                                foreach (var element in device_handover_files)
                                {
                                    if (element.handover_files_id == item.handover_files_id)
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
                            db.device_handover_files.RemoveRange(del);
                        }


                        string fdhandoverattach = provider.FormData.GetValues("handoverattach").SingleOrDefault();
                        List<device_handover_attach> handoverattach = JsonConvert.DeserializeObject<List<device_handover_attach>>(fdhandoverattach);

                        var device_handover_attachOld = db.device_handover_attach.Where(s => s.handover_id == handover.handover_id).ToArray<device_handover_attach>();


                        if (device_handover_attachOld.Length > 0)
                        {
                            List<device_handover_attach> dell = new List<device_handover_attach>();
                            foreach (var item in device_handover_attachOld)
                            {
                                var checkDel = false;
                                foreach (var element in handoverattach)
                                {
                                    if (element.handover_attach_id == item.handover_attach_id)
                                    {
                                        checkDel = true;
                                        handoverattach = handoverattach.Where(val => val.handover_attach_id != element.handover_attach_id).ToList();
                                        break;
                                    }
                                }
                                if (!checkDel)
                                {
                                    var fdcard = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                    if (fdcard != null)
                                    {
                                        fdcard.old_status = fdcard.status;
                                        fdcard.status = "TK";
                                        fdcard.device_user_id =  null;
                                        fdcard.is_recall = false;
                                        fdcard.check_recall = null;
                                    }
                                    if (handover.device_repair_id != null)
                                    {
                                        if (item.repair_details_id != null)
                                        {
                                            var fdReplace = db.device_repair_details.Where(s => s.repair_details_id == item.repair_details_id).FirstOrDefault();
                                            fdReplace.is_replace = null;
                                        }
                                    }
                                    dell.Add(item);

                                }

                            }
                            db.device_handover_attach.RemoveRange(dell);
                        }


                        if (handoverattach.Count > 0)
                        {

                            foreach (var item in handoverattach)
                            {
                                var fdcard = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();
                                if (fdcard != null)
                                {
                                    fdcard.old_status = fdcard.status;
                                    fdcard.status = "CXN";
                                    fdcard.device_user_id = handover.user_receiver_id;
                                    fdcard.is_recall = false;
                                    fdcard.check_recall = null;
                                }
                              
                                item.user_id = handover.user_receiver_id;
                                item.organization_id = int.Parse(dvid);
                                item.handover_id = handover.handover_id;
                                item.created_date = DateTime.Now;
                                item.created_by = uid;
                                item.created_ip = ip;
                                item.modified_date = DateTime.Now;
                                item.modified_by = uid;
                                item.modified_ip = ip;
                                db.device_handover_attach.Add(item);
db.SaveChanges();
    if (handover.device_repair_id != null)
                                {
                                    if (item.repair_details_id != null)
                                    {
                                        var fdReplace = db.device_repair_details.Where(s => s.repair_details_id == item.repair_details_id).FirstOrDefault();
                                        fdReplace.is_replace = item.handover_attach_id;
                                    }
                                }
                            }

                        }



                   
                        var fdlistSize = provider.FormData.GetValues("filesize").SingleOrDefault();
                        List<double> listSize = JsonConvert.DeserializeObject<List<double>>(fdlistSize);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int order = 0;
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            device_handover_files handoverfile = new device_handover_files();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Device", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Device", fileName);
                            }
                            handoverfile.file_path = "/Portals/" + dvid + "/Device/" + fileName;
                            handoverfile.file_name = fileName;

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
                            handoverfile.file_size = listSize[order];
                            handoverfile.file_type = helper.GetFileExtension(fileName);
                            handoverfile.handover_id = handover.handover_id;
                            handoverfile.created_date = DateTime.Now;
                            handoverfile.created_by = uid;
                            handoverfile.created_ip = ip;
                            handoverfile.modified_date = DateTime.Now;
                            handoverfile.modified_by = uid;
                            handoverfile.modified_ip = ip;
                            db.device_handover_files.Add(handoverfile);

                            order++;
                        }



                        #region add device_log
                        if (helper.wlog)
                        {

                            device_log log = new device_log();
                            log.title = "Sửa biên bản bàn giao tài sản " + handover.handover_number;

                            log.log_module = "device_handover";
                            log.log_type = 1;
                            log.id_key = handover.handover_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.device_log.Add(log);

                        }
                        #endregion

                        foreach (string strP in paths)
                        {

                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Device/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Device/" + Path.GetFileName(strP));
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Handover/update_device_handover", ip, tid, "Lỗi khi cập nhật biên bản bàn giao tài sản", 0, "device_Handover");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Handover/update_device_handover", ip, tid, "Lỗi khi cập nhật biên bản bàn giao tài sản", 0, "device_Handover");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_device_handover([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.device_handover.Where(a => id.Contains(a.handover_id)).ToListAsync();
                        string root = HttpContext.Current.Server.MapPath("~/Portals");



                        List<string> paths = new List<string>();
                        
                        if (das.Count > 0)
                        {
                            List<device_handover> del = new List<device_handover>();

                            foreach (var da in das)
                            {

                                if (uid == da.created_by || (int.Parse(dvid) == da.organization_id && ad) || super)
                                {

                                    var device_handover_process = db.device_process.Where(s => s.device_note_id == da.handover_id && s.device_process_type == -2).FirstOrDefault();
                                    var das1 = await db.device_handover_attach.Where(a => a.handover_id ==da.handover_id).ToListAsync();

                                    if (das1.Count > 0)
                                    {
                                        List<device_handover_attach> del1 = new List<device_handover_attach>();

                                        foreach (var da1 in das1)
                                        {
                                            var fdcard = db.device_card.Where(s => s.card_id == da1.card_id).FirstOrDefault();
                                            if (fdcard != null)
                                            {
                                                if (da.receipt_type == 0)
                                                {
                                                    fdcard.old_status = fdcard.status;
                                                    fdcard.status = "DSD";
                                                    fdcard.device_user_id = da.user_deliver_id;
                                                    var das3 =  db.device_handover.Where(a => a.handover_id == da1.handover_id).FirstOrDefault();
                                                    if (das3 != null)
                                                    { 
                                                        fdcard.manage_department_id = das3.device_department_id;
                                                        fdcard.warehouse_id_old = fdcard.warehouse_id;
                                                        fdcard.warehouse_id = null;
                                                    }
                                                    else
                                                    {
                                                       
                                                        fdcard.manage_department_id = null;
                                                        fdcard.warehouse_id = fdcard.warehouse_id_old;
                                                    }

                                                }
                                                else
                                                {

                                                    fdcard.old_status = fdcard.status;
                                                    fdcard.status = "TK";

                                                    fdcard.device_user_id = null;
                                                }
                                              
                                            }
                                            if (da.device_repair_id != null)
                                            {
                                                if (da1.repair_details_id != null)
                                                {
                                                    var fdReplace = db.device_repair_details.Where(s => s.repair_details_id == da1.repair_details_id).FirstOrDefault();
                                                    fdReplace.is_replace = null;
                                                }
                                            }
                                            del1.Add(da1);
                                        }
                                        db.device_handover_attach.RemoveRange(del1);
                                    }
                                    var das2 = await db.device_handover_files.Where(a => a.handover_id == da.handover_id).ToListAsync();

                                    if (das2.Count > 0)
                                    {
                                        List<device_handover_files> del2 = new List<device_handover_files>();

                                        foreach (var da2 in das2)
                                        {
                                            del2.Add(da2);

                                            if (!string.IsNullOrWhiteSpace(da2.file_path))
                                                paths.Add(  da2.file_path);
                                        }

                                        db.device_handover_files.RemoveRange(del2);
                                    }




                                    db.device_process.Remove(device_handover_process);


                                    del.Add(da);





                                    #region add device_log
                                    if (helper.wlog)
                                    {

                                        device_log log = new device_log();
                                        log.title = "Xóa biên bản bàn giao tài sản " + da.handover_number;
                                        log.log_module = "device_handover";
                                        log.log_type = 2;
                                        log.id_key = da.handover_id.ToString();
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
                            db.device_handover.RemoveRange(del);
                        }
                      
                        
                        await db.SaveChangesAsync();
                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Device/"   + Path.GetFileName(strP) );
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Device/" + Path.GetFileName(strP));
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Handover/delete_device_handover", ip, tid, "Lỗi khi xoá biên bản bàn giao tài sản", 0, "device_Handover");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Handover/delete_device_handover", ip, tid, "Lỗi khi xoá biên bản bàn giao tài sản", 0, "device_Handover");
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

        [HttpPut]
        public async Task<HttpResponseMessage> update_s_device_handover([System.Web.Mvc.Bind(Include = "IntID,IntTrangthai")] Trangthai trangthai)
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
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = db.device_handover.Where(a => (a.handover_id == trangthai.IntID)).FirstOrDefault<device_handover>();
                        das.status = trangthai.IntTrangthai;

                        if (das != null)
                        {
                            if (trangthai.IntTrangthai == 2)
                            {
                                if (das.user_verifier_id == uid)
                                {
                                    das.verifier_date = DateTime.Now;
                                    das.is_verifier_accept = true;
                                   
                                }
                                if (das.user_receiver_id == uid)
                                {
                                    das.receiver_date = DateTime.Now;
                                    das.is_receiver_accept = true;
                                }
                            }
                            else if (trangthai.IntTrangthai == 1)
                            {
                                if (das.user_verifier_id == uid)
                                {
                                    das.verifier_date = null;
                                    das.is_verifier_accept = false;
                                }
                                if (das.user_receiver_id == uid)
                                {
                                  
                                    das.receiver_date = null;
                                    das.is_receiver_accept = false;
                                }
                            }

                            var fdhandover_card = db.device_handover_attach.Where(s => s.handover_id == das.handover_id).ToArray();

                            if (fdhandover_card.Length > 0)
                            {
                                foreach (var item in fdhandover_card)
                                {
                                    var fdcard = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                    if (fdcard != null)
                                    {
                                        fdcard.old_status = fdcard.status;
                                        if (trangthai.IntTrangthai == 1)
                                        {

                                          fdcard.status = "CXN";
                                          
                                        }
                                        if (trangthai.IntTrangthai == 2)
                                        {
                                            if ((das.handover_type == 0 && das.is_receiver_accept == true) ||
                                              (das.handover_type == 1 && das.is_verifier_accept == true && das.is_receiver_accept == true))
                                            {
                                                var fdUserU = db.sys_users.Where(s => s.user_id == das.user_receiver_id).FirstOrDefault();
                                                fdcard.device_user_id = das.user_receiver_id;
                                                fdcard.use_date = DateTime.Now;
                                                fdcard.device_department_id = fdUserU.organization_id;
                                                fdcard.status = "DSD";
                                                fdcard.warehouse_id_old = fdcard.warehouse_id;
                                                fdcard.warehouse_id = null;
                                                fdcard.manage_department_id = das.device_department_id;


                                            }
                                            else
                                            {
                                                das.status = 1;
                                            }
                                        }
                                    }
                                }
                            }
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            #region sendhub

                            if (trangthai.IntTrangthai == 1)
                                        {
                            var userSenHub = db.sys_users.Where(s =>   s.user_id == das.user_receiver_id
                            || s.user_id == das.user_verifier_id ).ToList();
                            foreach (var fiu in userSenHub)
                            {
                                var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.user_send = uid;
                                sh.module_key = const_module_key;
                                sh.receiver = fiu.user_id;
                                sh.icon = fiu.avatar;
                                sh.title = "Thiết bị";
                                sh.contents = "Gửi cấp phát thiết bị: " + das.handover_number;
                                sh.type = 6;
                                sh.is_type = 1;
                                sh.date_send = DateTime.Now;
                                sh.id_key = das.handover_id.ToString();
                                sh.group_id = null;
                                sh.token_id = tid;
                                sh.created_date = DateTime.Now;
                                sh.created_by = uid;
                                sh.created_token_id = tid;
                                sh.created_ip = ip;
                                db.sys_sendhub.Add(sh);
                                db.SaveChanges();
                            }
                                        }
                                        else  if (trangthai.IntTrangthai == 2){
                                            var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.user_send = uid;
                                sh.module_key = const_module_key;
                                var userG = db.sys_users.Where(s =>   s.user_id == das.user_deliver_id
                              ).FirstOrDefault();
                                sh.receiver = das.user_deliver_id;
                                sh.icon = userG.avatar;
                                sh.title = "Thiết bị";
                                sh.contents = "Đã xác nhận phiếu cấp phát: " + das.handover_number;
                                sh.type = 6;
                                sh.is_type = 1;
                                sh.date_send = DateTime.Now;
                                sh.id_key = das.handover_id.ToString();
                                sh.group_id = null;
                                sh.token_id = tid;
                                sh.created_date = DateTime.Now;
                                sh.created_by = uid;
                                sh.created_token_id = tid;
                                sh.created_ip = ip;
                                db.sys_sendhub.Add(sh);
                                db.SaveChanges();
                                        }

                            #endregion

                            #region add device_log
                            if (helper.wlog)
                            {
                                device_log log = new device_log();
                                log.title = "Sửa biên bản bàn giao tài sản " + das.handover_number;
                                log.log_module = "device_handover";
                                log.log_type = 1;
                                log.id_key = das.handover_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.device_log.Add(log);

                            }
                            #endregion
                            await db.SaveChangesAsync();
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_handover/update_s_device_handover", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_handover", 0, "device_handover");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_handover/update_s_device_handover", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_handover", 0, "device_handover");
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
        [HttpPut]
        public async Task<HttpResponseMessage> update_cancel_device_handover([System.Web.Mvc.Bind(Include = "IntID,IntTrangthai")] Trangthai trangthai)
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
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = db.device_handover.Where(a => (a.handover_id == trangthai.IntID)).FirstOrDefault<device_handover>();
                    
                        if (das != null)
                        {
                             if (trangthai.IntTrangthai == 3)
                            {
                                if (das.user_verifier_id == uid)
                                {

                                    das.verifier_date = null;
                                    das.is_verifier_accept = false;
                                }
                                if (das.user_receiver_id == uid)
                                {
                                    das.receiver_date = null;
                                    das.is_receiver_accept = false;
                                }
                            }

                            var fdhandover_card = db.device_handover_attach.Where(s => s.handover_id == das.handover_id).ToArray();

                            if (fdhandover_card.Length > 0)
                            {
                                foreach (var item in fdhandover_card)
                                {
                                    var fdcard = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                    if (fdcard != null)
                                    {
                                        fdcard.old_status = fdcard.status;
                                        if (trangthai.IntTrangthai == 3)
                                        {

                                            fdcard.status = "CXN";
                                            fdcard.device_user_id = das.user_receiver_id;
                                           
                                        }

                                    }

                                    if (das.device_repair_id != null)
                                    {
                                        if (item.repair_details_id != null)
                                        {
                                            var fdReplace = db.device_repair_details.Where(s => s.repair_details_id == item.repair_details_id).FirstOrDefault();
                                            fdReplace.is_replace = null;
                                        }
                                    }
                                }
                            }
                            #region sendhub
                            var userSenHub = db.sys_users.Where(s => (s.user_id == das.user_deliver_id || s.user_id == das.user_receiver_id
                            || s.user_id == das.user_verifier_id ) && s.user_id != das.user_cancel_id).ToList();
                            foreach (var fiu in userSenHub)
                            {
                                var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.user_send = uid;
                                sh.module_key = const_module_key;
                                sh.receiver = fiu.user_id;
                                sh.icon = fiu.avatar;
                                sh.title = "Thiết bị";
                                sh.contents = "Trả lại thiết bị: " + das.handover_number;
                                sh.type = 6;
                                sh.is_type = 1;
                                sh.date_send = DateTime.Now;
                                sh.id_key = das.handover_id.ToString();
                                sh.group_id = null;
                                sh.token_id = tid;
                                sh.created_date = DateTime.Now;
                                sh.created_by = uid;
                                sh.created_token_id = tid;
                                sh.created_ip = ip;
                                db.sys_sendhub.Add(sh);
                                db.SaveChanges();
                            }
                            #endregion
                            das.status = trangthai.IntTrangthai;
                            das.is_cancel = true;
                            das.user_cancel_id = uid;
                            das.cancel_date = DateTime.Now;
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                        

                            #region add device_log
                            if (helper.wlog)
                            {
                                device_log log = new device_log();
                                log.title = "Sửa biên bản bàn giao tài sản " + das.handover_number;
                                log.log_module = "device_handover";
                                log.log_type = 1;
                                log.id_key = das.handover_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.device_log.Add(log);

                            }
                            #endregion
                            await db.SaveChangesAsync();
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_handover/update_s_device_handover", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_handover", 0, "device_handover");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_handover/update_s_device_handover", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_handover", 0, "device_handover");
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
        [HttpPut]
        public async Task<HttpResponseMessage> update_config_user_handover([System.Web.Mvc.Bind(Include = "organization_id,user_receiver")][FromBody] List<userReceiver> userReceiver)
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

                        foreach (var item in userReceiver)
                        {
                            var int_id = int.Parse(item.organization_id.ToString());
                            var das = db.sys_organization.Where(a => (a.organization_id == int_id)).FirstOrDefault<sys_organization>();
                            das.user_receiver = item.user_receiver;
                            db.SaveChanges();
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = userReceiver, contents }), domainurl + "device_Handover/update_config_user_handover", ip, tid, "Lỗi khi cập nhật người nhận phòng ban", 0, "device_Handover");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = userReceiver, contents }), domainurl + "device_Handover/update_config_user_handover", ip, tid, "Lỗi khi cập nhật người nhận phòng ban", 0, "device_Handover");
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
        #region export doc
        [HttpPost]
        public HttpResponseMessage ExportDoc([System.Web.Mvc.Bind(Include = "lib,name,html,opition")][FromBody] modelHTML model)
        {
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
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var user_now = db.sys_users.AsNoTracking().FirstOrDefault(x => x.user_id == uid);
                        string rootPath = HttpContext.Current.Server.MapPath("~/Portals/" + user_now.organization_id + "/Word/");
                        bool existPath = System.IO.Directory.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Word/");
                        if (!existPath)
                        {
                            System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Word/");
                        }
                        string path = "/Portals/" + user_now.organization_id + "/Word/" + model.name + "_" + DateTime.Now.ToString("ddMMyyyy")+".doc";
                        string strPath = Path.Combine(rootPath + model.name + "_" + DateTime.Now.ToString("ddMMyyyy") + ".doc");
                        using (var htmlStream = new MemoryStream(Encoding.UTF8.GetBytes(model.html)))
                        {
                            ComponentInfo.SetLicense("DTZX-HTZ5-B7Q6-2GA6");
                            var htmlLoadOptions = new HtmlLoadOptions();
                            var document = DocumentModel.Load(htmlStream, htmlLoadOptions);

                            document.DefaultCharacterFormat.FontName = "Times New Roman";
                            var opt = model.opition;
                            if (opt == null || (opt.left == 0 && opt.top == 0 && opt.right == 0 && opt.bottom == 0))
                            {
                                opt = new PDFOpition()
                                {
                                    orientation = opt.orientation ?? "Portrait",
                                    pageSize = opt.pageSize ?? "A4",
                                    left = opt.left,
                                    top = opt.top,
                                    right = opt.right,
                                    bottom = opt.bottom,
                                };
                            }
                            Section section = document.Sections[0];
                            PageSetup pageSetup = section.PageSetup;
                            PageMargins pageMargins = pageSetup.PageMargins;
                            pageMargins.Top = opt.top;
                            pageMargins.Right = opt.right;
                            pageMargins.Bottom = opt.bottom;
                            pageMargins.Left = opt.left;
                            SaveOptions opit = SaveOptions.DocxDefault;

                            if (File.Exists(HttpContext.Current.Server.MapPath("~/Portals")+"/" + dvid + "/Word/"+ Path.GetFileName(strPath)))
                            {
                                File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Word/" + Path.GetFileName(strPath));
                            }
                            document.Save(strPath, opit);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { path = path, err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "device_handover/ExportDoc", ip, tid, "Lỗi khi export file doc", 0, "device_handover");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "device_handover/ExportDoc", ip, tid, "Lỗi khi export file doc", 0, "device_handover");
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
        #endregion
    }
}