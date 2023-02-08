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
    public class device_recallController : ApiController
    {
        public string getipaddress()
        {
          return  HttpContext.Current.Request.UserHostAddress;
        }
  

        [HttpPost]
        public async Task<HttpResponseMessage> add_device_recall()
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
                    string fddevice_Recall = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");

                    string strPath = root + "/" + dvid + "/Recall";
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
                        var fdUser = db.sys_users.Where(s => s.user_id == uid).FirstOrDefault();
                        var fdPosition = new ca_positions();
                        if (fdUser.position_id != null)
                            fdPosition = db.ca_positions.Where(s => s.position_id == fdUser.position_id).FirstOrDefault();
                        fddevice_Recall = provider.FormData.GetValues("recall").SingleOrDefault();
                        device_recall device_Recall = JsonConvert.DeserializeObject<device_recall>(fddevice_Recall);
                      
                        device_Recall.user_collector = uid;
                        device_Recall.organization_id = int.Parse(dvid);
                        device_Recall.created_date = DateTime.Now;
                        device_Recall.created_by = uid;
                        device_Recall.created_ip = ip;
                        device_Recall.modified_date = DateTime.Now;
                        device_Recall.modified_by = uid;
                        device_Recall.modified_ip = ip;
                        var device_RecallOld = db.device_recall.Where(s => s.recall_number == device_Recall.recall_number && device_Recall.organization_id == s.organization_id
                        ).FirstOrDefault();
                        if (device_RecallOld != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Đã tồn tại mã phiếu trong cơ sở dữ liệu. Vui lòng nhập lại!", err = "1" });

                        }
                        db.device_recall.Add(device_Recall);
                        db.SaveChanges();

               
                        var fdlistSize = "";
                        fdlistSize = provider.FormData.GetValues("filesize").SingleOrDefault();
                        List<int> listSize = JsonConvert.DeserializeObject<List<int>>(fdlistSize);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int order = 0;
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            device_recall_files device_Recallfile = new device_recall_files();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Recall", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Recall", fileName);
                            }
                            device_Recallfile.file_path = "/Portals/" + dvid + "/Recall/" + fileName;
                            device_Recallfile.files_name = fileName;

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

                            device_Recallfile.file_size = listSize[order];
                            device_Recallfile.file_type = helper.GetFileExtension(fileName);
                            device_Recallfile.device_recall_id = device_Recall.device_recall_id;
                            device_Recallfile.created_date = DateTime.Now;
                            device_Recallfile.created_by = uid;
                            device_Recallfile.created_ip = ip;
                            device_Recallfile.modified_date = DateTime.Now;
                            device_Recallfile.modified_by = uid;
                            device_Recallfile.modified_ip = ip;
                            db.device_recall_files.Add(device_Recallfile);
                            db.SaveChanges();
                            order++;
                        }

                        string fddeviceRecallDetails = "";
                        fddeviceRecallDetails = provider.FormData.GetValues("recalldetails").SingleOrDefault();
                        List<device_recall_details> deviceRecallDetails = JsonConvert.DeserializeObject<List<device_recall_details>>(fddeviceRecallDetails);

                        if (deviceRecallDetails.Count > 0)
                        {

                            foreach (var item in deviceRecallDetails)
                            {
                                var fdcard = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                if (fdcard != null)
                                {
                                    fdcard.check_recall = "L";
                                }
                                item.barcode_id = fdcard.barcode_id;
                                item.device_id = fdcard.device_id;
                                item.purchase_date = fdcard.purchase_date;
                                item.price = fdcard.price;
                                item.current_price = fdcard.current_price;
                                item.device_recall_id = device_Recall.device_recall_id;
                                item.created_date = DateTime.Now;
                                item.created_by = uid;
                                item.created_ip = ip;
                                item.modified_date = DateTime.Now;
                                item.modified_by = uid;
                                item.modified_ip = ip;
                                db.device_recall_details.Add(item);
                                db.SaveChanges();
                            }

                        }
                        #region add device_process_log
                        var fddevice_process_log = db.device_process_log.Where(s => s.device_note_id == device_Recall.device_recall_id && s.device_process_type == 1
                        && s.device_process_id == null).ToArray();
                        if (fddevice_process_log.Length > 0)
                        {

                            db.device_process_log.RemoveRange(fddevice_process_log);
                            db.SaveChanges();
                        }
                        device_process_log device_Process_Log = new device_process_log();
                        device_Process_Log.content = "Lập phiếu thu hồi";
                        device_Process_Log.device_note_id = device_Recall.device_recall_id;
                        device_Process_Log.device_process_type = 3;
                        device_Process_Log.device_process_id = null;
                        device_Process_Log.created_by = uid;
                        device_Process_Log.created_date = DateTime.Now;
                        device_Process_Log.created_ip = ip;
                        db.device_process_log.Add(device_Process_Log);
                        db.SaveChanges();



                        #endregion
                        #region add device_log
                        if (helper.wlog)
                        {

                            device_log log = new device_log();
                            log.title = "Thêm phiếu thu hồi " + device_Recall.recall_number;
                            log.log_module = "device_recall";
                            log.id_key = device_Recall.device_recall_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_recall/add_device_recall", ip, tid, "Lỗi khi thêm phiếu thu hồi", 0, "device_recall");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_recall/add_device_recall", ip, tid, "Lỗi khi thêm phiếu thu hồi", 0, "device_recall");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_device_recall()
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
                    string fddevice_Recall = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Recall";
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
                        fddevice_Recall = provider.FormData.GetValues("recall").SingleOrDefault();
                        device_recall device_Recall = JsonConvert.DeserializeObject<device_recall>(fddevice_Recall);
                       
                        device_Recall.modified_date = DateTime.Now;
                        device_Recall.modified_by = uid;
                        device_Recall.modified_ip = ip;
                        db.Entry(device_Recall).State = EntityState.Modified;
                        string fddeviceRecallDetails = "";
                        fddeviceRecallDetails = provider.FormData.GetValues("recalldetails").SingleOrDefault();

                        List<string> paths = new List<string>();
                        string fddevice_recall_files = provider.FormData.GetValues("recallfiles").SingleOrDefault();
                        List<device_recall_files> device_recall_files = JsonConvert.DeserializeObject<List<device_recall_files>>(fddevice_recall_files);

                        var device_recall_filesOld = db.device_recall_files.Where(s => s.device_recall_id == device_Recall.device_recall_id).ToArray();
                        if (device_recall_filesOld.Length > 0)
                        {

                            List<device_recall_files> del = new List<device_recall_files>();
                            foreach (var item in device_recall_filesOld)
                            {
                                var checkDel = false;
                                foreach (var element in device_recall_files)
                                {
                                    if (element.recall_files_id == item.recall_files_id)
                                    {
                                        checkDel = true;
                                        break;
                                    }
                                }
                                if (!checkDel)
                                {
                                    del.Add(item);
                                    if (!string.IsNullOrWhiteSpace(item.file_path))
                                        paths.Add(root + item.file_path.Substring(8));
                                }


                            }
                            db.device_recall_files.RemoveRange(del);
                        }




                        List<device_recall_details> deviceRecallDetails = JsonConvert.DeserializeObject<List<device_recall_details>>(fddeviceRecallDetails);
                        var device_recall_details_old = db.device_recall_details.Where(s => s.device_recall_id == device_Recall.device_recall_id).ToArray<device_recall_details>();
                        if (device_recall_details_old.Length > 0)
                        {
                            List<device_recall_details> liDell = new List<device_recall_details>();
                            foreach (var item in device_recall_details_old)
                            {
                                var fdcard = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                if (fdcard != null)
                                {
                                    fdcard.check_recall = null;
                                }
                                liDell.Add(item);

                            }
                            db.device_recall_details.RemoveRange(liDell);

                        }

                        if (deviceRecallDetails.Count > 0)
                        {

                            foreach (var item in deviceRecallDetails)
                            {
                                var fdcard = db.device_card.Where(s => s.card_id == item.card_id).FirstOrDefault();

                                if (fdcard != null)
                                {
                                    fdcard.check_recall ="L";
                                }
                                item.barcode_id = fdcard.barcode_id;
                                item.device_id = fdcard.device_id;
                                item.purchase_date = fdcard.purchase_date;
                                item.price = fdcard.price;
                                item.current_price = fdcard.current_price;
                           
                                item.device_recall_id = device_Recall.device_recall_id;
                                item.created_date = DateTime.Now;
                                item.created_by = uid;
                                item.created_ip = ip;
                                item.modified_date = DateTime.Now;
                                item.modified_by = uid;
                                item.modified_ip = ip;
                                db.device_recall_details.Add(item);
                                db.SaveChanges();
                            }

                        }

                        
                        var fdlistSize = "";
                        fdlistSize = provider.FormData.GetValues("filesize").SingleOrDefault();
                        List<double> listSize = JsonConvert.DeserializeObject<List<double>>(fdlistSize);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int order = 0;
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            device_recall_files device_Recallfile = new device_recall_files();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Recall", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Recall", fileName);
                            }
                            device_Recallfile.file_path = "/Portals/" + dvid + "/Recall/" + fileName;
                            device_Recallfile.files_name = fileName;

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
                            device_Recallfile.file_size = listSize[order];
                            device_Recallfile.file_type = helper.GetFileExtension(fileName);
                            device_Recallfile.device_recall_id = device_Recall.device_recall_id;
                            device_Recallfile.created_date = DateTime.Now;
                            device_Recallfile.created_by = uid;
                            device_Recallfile.created_ip = ip;
                            device_Recallfile.modified_date = DateTime.Now;
                            device_Recallfile.modified_by = uid;
                            device_Recallfile.modified_ip = ip;
                            db.device_recall_files.Add(device_Recallfile);

                            order++;
                        }
                        #region add device_process_log


                        device_process_log device_Process_Log = new device_process_log();
                        device_Process_Log.content = "Cập nhật phiếu";
                        device_Process_Log.device_note_id = device_Recall.device_recall_id;
                        device_Process_Log.device_process_type = 3;
                        device_Process_Log.device_process_id = null;
                        device_Process_Log.created_by = uid;
                        device_Process_Log.created_date = DateTime.Now;
                        device_Process_Log.created_ip = ip;
                        db.device_process_log.Add(device_Process_Log);
                        db.SaveChanges();
                        #endregion
                        #region add device_log
                        if (helper.wlog)
                        {

                            device_log log = new device_log();
                            log.title = "Sửa phiếu thu hồi " + device_Recall.recall_number;

                            log.log_module = "device_recall";
                            log.log_type = 1;
                            log.id_key = device_Recall.device_recall_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.device_log.Add(log);

                        }
                        #endregion

                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Recall/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Recall/" + Path.GetFileName(strP));
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
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_device_recall([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.device_recall.Where(a => id.Contains(a.device_recall_id)).ToListAsync();
                        var das1 = await db.device_recall_details.Where(a => id.Contains(a.device_recall_id)).ToListAsync();
                        var das2 = await db.device_recall_files.Where(a => id.Contains(a.device_recall_id)).ToListAsync();


                        List<string> paths = new List<string>();
                        if (das.Count > 0)
                        {
                            List<device_recall> del = new List<device_recall>();

                            foreach (var da in das)
                            {

                                if (uid == da.created_by || (int.Parse(dvid) == da.organization_id && ad) || super)
                                {

                                    del.Add(da);
                                    #region add device_log
                                    if (helper.wlog)
                                    {

                                        device_log log = new device_log();
                                        log.title = "Xóa phiếu thu hồi " + da.recall_number;
                                        log.log_module = "device_recall";
                                        log.log_type = 2;
                                        log.id_key = da.device_recall_id.ToString();
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
                            db.device_recall.RemoveRange(del);
                        }
                        if (das1.Count > 0)
                        {
                            List<device_recall_details> del1 = new List<device_recall_details>();

                            foreach (var da in das1)
                            {
                                var fdcard = db.device_card.Where(s => s.card_id == da.card_id).FirstOrDefault();
                                if (fdcard != null)
                                {
                                    fdcard.check_recall = null;
                                }
                                del1.Add(da);
                            }
                            db.device_recall_details.RemoveRange(del1);
                        }
                        if (das2.Count > 0)
                        {
                            List<device_recall_files> del2 = new List<device_recall_files>();

                            foreach (var da in das2)
                            {
                                del2.Add(da);

                                if (!string.IsNullOrWhiteSpace(da.file_path))
                                    paths.Add(da.file_path);
                            }

                            db.device_recall_files.RemoveRange(del2);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/Recall/") + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/Recall/") + Path.GetFileName(strP));
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_recall/delete_device_recall", ip, tid, "Lỗi khi xoá phiếu thu hồi", 0, "device_recall");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_recall/delete_device_recall", ip, tid, "Lỗi khi xoá phiếu thu hồi", 0, "device_recall");
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