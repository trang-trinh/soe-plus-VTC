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
    public class device_inventory_slipController : ApiController
    {

        private const string const_module_key = "M7";
        public string getipaddress()
        {
         return  HttpContext.Current.Request.UserHostAddress;
        }


   
        [HttpPost]
        public async Task<HttpResponseMessage> add_device_inventory_slip()
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
                    string fddevice_Inventory = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");

                    string strPath = root + "/" + dvid + "/Inventory";
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
                        fddevice_Inventory = provider.FormData.GetValues("inventory").SingleOrDefault();
                        device_inventory_slip device_Inventory = JsonConvert.DeserializeObject<device_inventory_slip>(fddevice_Inventory);
                        device_Inventory.inventory_user_id = uid;
                        device_Inventory.organization_id = int.Parse(dvid);
                        device_Inventory.created_date = DateTime.Now;
                        device_Inventory.created_by = uid;
                        device_Inventory.created_ip = ip;
                        device_Inventory.modified_date = DateTime.Now;
                        device_Inventory.modified_by = uid;
                        device_Inventory.modified_ip = ip;
                        var device_InventoryOld = db.device_inventory_slip.Where(s => s.inventory_number == device_Inventory.inventory_number && device_Inventory.organization_id == s.organization_id
                        ).FirstOrDefault();
                        if (device_InventoryOld != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Đã tồn tại mã phiếu trong cơ sở dữ liệu. Vui lòng nhập lại!", err = "1" });
                        }
                        db.device_inventory_slip.Add(device_Inventory);
                        db.SaveChanges();
                        device_inventory_files device_Inventoryfile = new device_inventory_files();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Inventory", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Inventory", fileName);
                            }
                            device_Inventoryfile.file_path = "/Portals/" + dvid + "/Inventory/" + fileName;
                            device_Inventoryfile.file_name = fileName;

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

                            device_Inventoryfile.file_size = listSize[order];
                            device_Inventoryfile.file_type = helper.GetFileExtension(fileName);
                            device_Inventoryfile.device_inventory_id = device_Inventory.inventory_slip_id;
                            device_Inventoryfile.created_date = DateTime.Now;
                            device_Inventoryfile.created_by = uid;
                            device_Inventoryfile.created_ip = ip;
                            device_Inventoryfile.modified_date = DateTime.Now;
                            device_Inventoryfile.modified_by = uid;
                            device_Inventoryfile.modified_ip = ip;
                            db.device_inventory_files.Add(device_Inventoryfile);
                            db.SaveChanges();
                            order++;
                        }

                        string fddeviceInventoryDetails = "";
                        fddeviceInventoryDetails = provider.FormData.GetValues("inventorydetails").SingleOrDefault();
                        List<device_inventory_details> deviceInventoryDetails = JsonConvert.DeserializeObject<List<device_inventory_details>>(fddeviceInventoryDetails);

                        if (deviceInventoryDetails.Count > 0)
                        {

                            foreach (var item in deviceInventoryDetails)
                            {
                                var fdcard = db.device_card.AsNoTracking().Where(s => s.card_id == item.card_id).FirstOrDefault();
                                var fddevice = db.device_main.AsNoTracking().Where(s => s.device_id == item.device_id).FirstOrDefault();
                                var fdunit = db.device_unit.AsNoTracking().Where(s => s.device_unit_id == fddevice.device_unit_id).FirstOrDefault();
                                item.inventory_slip_id = device_Inventory.inventory_slip_id;
                                item.device_des = fdcard.device_des;
                                item.device_id = fdcard.device_id;
                                item.manufacture_country = fdcard.manufacture_country;

                                item.user_use_id = fdcard.device_user_id;
                                item.inventory_unit = fdunit.device_unit_name;
                                item.created_date = DateTime.Now;
                                item.created_by = uid;
                                item.created_ip = ip;
                                item.modified_date = DateTime.Now;
                                item.modified_by = uid;
                                item.modified_ip = ip;
                                db.device_inventory_details.Add(item);
                                db.SaveChanges();
                            }

                        }
                        string fddeviceInventoryPersonnel = "";
                        fddeviceInventoryPersonnel = provider.FormData.GetValues("inventorypersonnel").SingleOrDefault();
                        List<device_inventory_personnel> deviceInventoryPersonnel = JsonConvert.DeserializeObject<List<device_inventory_personnel>>(fddeviceInventoryPersonnel);

                        if (deviceInventoryPersonnel.Count > 0)
                        {

                            foreach (var item in deviceInventoryPersonnel)
                            {

                                foreach (var mana in deviceInventoryDetails)
                                {
                                    item.inventory_slip_id = device_Inventory.inventory_slip_id;
                                    item.inventory_details_id = mana.inventory_details_id;
                                    item.amount = mana.amount_after;
                                    item.is_approved = null;
                                    item.created_date = DateTime.Now;
                                    item.created_by = uid;
                                    item.created_ip = ip;
                                    item.modified_date = DateTime.Now;
                                    item.modified_by = uid;
                                    item.modified_ip = ip;
                                    db.device_inventory_personnel.Add(item);
                                    db.SaveChanges();
                                }

                            }

                        }
                        #region add device_process_log
        
                        device_process_log device_Process_Log = new device_process_log();
                        device_Process_Log.content = "Lập phiếu";
                        device_Process_Log.device_note_id = device_Inventory.inventory_slip_id;
                        device_Process_Log.device_process_type = 2;
                        device_Process_Log.device_process_id = null;
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
                        //    log.title = "Thêm phiếu kiểm kê " + device_Inventory.inventory_number;
                        //    log.log_module = "device_inventory_slip";
                        //    log.id_key = device_Inventory.inventory_slip_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_inventory_slip/add_device_inventory_slip", ip, tid, "Lỗi khi thêm phiếu kiểm kê", 0, "device_inventory_slip");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_inventory_slip/add_device_inventory_slip", ip, tid, "Lỗi khi thêm phiếu kiểm kê", 0, "device_inventory_slip");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_device_inventory_slip()
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
                    string fddevice_Inventory = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Inventory";
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
                        fddevice_Inventory = provider.FormData.GetValues("inventory").SingleOrDefault();
                        device_inventory_slip device_Inventory = JsonConvert.DeserializeObject<device_inventory_slip>(fddevice_Inventory);
                        device_Inventory.modified_date = DateTime.Now;
                        device_Inventory.modified_by = uid;
                        device_Inventory.modified_ip = ip;
                        db.Entry(device_Inventory).State = EntityState.Modified;

                        List<string> paths = new List<string>();
                        string fddevice_inventory_files = provider.FormData.GetValues("inventoryfiles").SingleOrDefault();
                        List<device_inventory_files> device_inventory_files = JsonConvert.DeserializeObject<List<device_inventory_files>>(fddevice_inventory_files);

                        var device_inventory_filesOld = db.device_inventory_files.Where(s => s.device_inventory_id == device_Inventory.inventory_slip_id).ToArray();
                        if (device_inventory_filesOld.Length > 0)
                        {

                            List<device_inventory_files> del = new List<device_inventory_files>();
                            foreach (var item in device_inventory_filesOld)
                            {
                                var checkDel = false;
                                foreach (var element in device_inventory_files)
                                {
                                    if (element.inventory_files_id == item.inventory_files_id)
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
                            db.device_inventory_files.RemoveRange(del);
                        }



                        string fddeviceInventoryDetails = "";
                        fddeviceInventoryDetails = provider.FormData.GetValues("inventorydetails").SingleOrDefault();

                        List<device_inventory_details> deviceInventoryDetails = JsonConvert.DeserializeObject<List<device_inventory_details>>(fddeviceInventoryDetails);
                        List<device_inventory_details> liID = deviceInventoryDetails;
                          var device_inventory_personnel_disctince = db.device_inventory_personnel.Where(s => s.inventory_slip_id == device_Inventory.inventory_slip_id)
                        .Select(x => new { x.full_name, x.avatar, x.organization_name, x.position_name, x.user_id }).Distinct().ToList();

                        var device_inventory_details_old = db.device_inventory_details.Where(s => s.inventory_slip_id == device_Inventory.inventory_slip_id).ToArray<device_inventory_details>();
                        if (device_inventory_details_old.Length > 0)
                        {
                            List<device_inventory_details> liDell = new List<device_inventory_details>();
                            foreach (var item in device_inventory_details_old)
                            {
                                var check = false;
                                foreach (var anu in deviceInventoryDetails)
                                {
                                    if (anu.card_id == item.card_id)
                                    {
                                        liID = liID.Where(val => val.card_id != anu.card_id).ToList();
                                        check = true;
                                        break;
                                    }
                                }

                                if (!check)
                                    liDell.Add(item);

                            }
                            deviceInventoryDetails = liID;
                            foreach (var item in liDell)
                            {
                                var device_in_sonnel = db.device_inventory_personnel.Where(s => s.inventory_details_id == item.inventory_details_id).ToList<device_inventory_personnel>();
                                db.device_inventory_personnel.RemoveRange(device_in_sonnel);
                                db.SaveChanges();

                            }
                            db.device_inventory_details.RemoveRange(liDell);

                        }

                        if (deviceInventoryDetails.Count > 0)
                        {


                            foreach (var item in deviceInventoryDetails)
                            {
                                var fdcard = db.device_card.AsNoTracking().Where(s => s.card_id == item.card_id).FirstOrDefault();
                                var fddevice = db.device_main.AsNoTracking().Where(s => s.device_id == item.device_id).FirstOrDefault();
                                var fdunit = db.device_unit.AsNoTracking().Where(s => s.device_unit_id == fddevice.device_unit_id).FirstOrDefault();

                                item.device_des = fdcard.device_des;
                                item.device_id = fdcard.device_id;
                                item.manufacture_country = fdcard.manufacture_country;
                                item.inventory_slip_id = device_Inventory.inventory_slip_id;
                                item.user_use_id = fdcard.device_user_id;
                                item.inventory_unit = fdunit.device_unit_name;
                                item.created_date = DateTime.Now;
                                item.created_by = uid;
                                item.created_ip = ip;
                                item.modified_date = DateTime.Now;
                                item.modified_by = uid;
                                item.modified_ip = ip;
                                db.device_inventory_details.Add(item);
                                db.SaveChanges();

                                foreach (var any in device_inventory_personnel_disctince)
                                {
                                    device_inventory_personnel anu = new device_inventory_personnel();
                                    anu.full_name = any.full_name;
                                    anu.position_name = any.position_name;
                                    anu.organization_name = any.organization_name;
                                    anu.avatar = any.avatar;
                                    anu.user_id = any.user_id;
                                    anu.inventory_slip_id = device_Inventory.inventory_slip_id;
                                    anu.inventory_details_id = item.inventory_details_id;
                                    anu.is_approved = null;
                                    anu.created_date = DateTime.Now; anu.amount = item.amount_after;
                                    anu.created_by = uid;
                                    anu.created_ip = ip;
                                    anu.modified_date = DateTime.Now;
                                    anu.modified_by = uid;
                                    anu.modified_ip = ip;
                                    db.device_inventory_personnel.Add(anu);
                                    db.SaveChanges();
                                }
                            }
                        }

                        device_inventory_files device_Inventoryfile = new device_inventory_files();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Inventory", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Inventory", fileName);
                            }
                            device_Inventoryfile.file_path = "/Portals/" + dvid + "/Inventory/" + fileName;
                            device_Inventoryfile.file_name = fileName;

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
                            device_Inventoryfile.file_size = listSize[order];
                            device_Inventoryfile.file_type = helper.GetFileExtension(fileName);
                            device_Inventoryfile.device_inventory_id = device_Inventory.inventory_slip_id;
                            device_Inventoryfile.created_date = DateTime.Now;
                            device_Inventoryfile.created_by = uid;
                            device_Inventoryfile.created_ip = ip;
                            device_Inventoryfile.modified_date = DateTime.Now;
                            device_Inventoryfile.modified_by = uid;
                            device_Inventoryfile.modified_ip = ip;
                            db.device_inventory_files.Add(device_Inventoryfile);

                            order++;
                        }

                        var device_inventory_personnel_old = db.device_inventory_personnel.Where(s => s.inventory_slip_id == device_Inventory.inventory_slip_id).ToArray<device_inventory_personnel>();

                        string fddeviceInventoryPersonnel = "";
                        fddeviceInventoryPersonnel = provider.FormData.GetValues("inventorypersonnel").SingleOrDefault();
                        List<device_inventory_personnel> deviceInventoryPersonnel = JsonConvert.DeserializeObject<List<device_inventory_personnel>>(fddeviceInventoryPersonnel);

                        List<device_inventory_personnel> liIP = deviceInventoryPersonnel;
                        if (device_inventory_personnel_old.Length > 0)
                        {
                            List<device_inventory_personnel> liDell1 = new List<device_inventory_personnel>();

                            foreach (var item in device_inventory_personnel_old)
                            {
                                var check = false;
                                foreach (var anu in deviceInventoryPersonnel)
                                {
                                    if (anu.user_id == item.user_id)
                                    {
                                        liIP = liIP.Where(val => val.user_id != anu.user_id).ToList();
                                        check = true;
                                        break;
                                    }
                                }

                                if (!check)
                                    liDell1.Add(item);

                            }
                            deviceInventoryPersonnel = liIP;
                            db.device_inventory_personnel.RemoveRange(liDell1);
                          

                        }
                        db.SaveChanges();
                        if (deviceInventoryPersonnel.Count > 0)
                        {

                            var linventoryDetails = db.device_inventory_details.Where(s => s.inventory_slip_id == device_Inventory.inventory_slip_id).ToArray<device_inventory_details>();

                            foreach (var item in deviceInventoryPersonnel)
                            {
                                foreach (var mana in linventoryDetails)
                                {
                                    item.inventory_slip_id = device_Inventory.inventory_slip_id;
                                    item.inventory_details_id = mana.inventory_details_id;
                                    item.is_approved = null;
                                    item.amount = mana.amount_after;
                                    item.created_date = DateTime.Now;
                                    item.created_by = uid;
                                    item.created_ip = ip;
                                    item.modified_date = DateTime.Now;
                                    item.modified_by = uid;
                                    item.modified_ip = ip;
                                    db.device_inventory_personnel.Add(item);
                                    db.SaveChanges();
                                }



                            }

                        }
                        #region add device_process_log


                        device_process_log device_Process_Log = new device_process_log();
                        device_Process_Log.content = "Cập nhật phiếu";
                        device_Process_Log.device_note_id = device_Inventory.inventory_slip_id;
                        device_Process_Log.device_process_type = 2;
                        device_Process_Log.device_process_id = null;
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
                        //    log.title = "Sửa phiếu kiểm kê " + device_Inventory.inventory_number;

                        //    log.log_module = "device_inventory_slip";
                        //    log.log_type = 1;
                        //    log.id_key = device_Inventory.inventory_slip_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.device_log.Add(log);

                        //}
                        //#endregion

                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(root+ "/" + dvid + "/Inventory/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(root+ "/" + dvid + "/Inventory/" + Path.GetFileName(strP));
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
        [HttpPut]
        public async Task<HttpResponseMessage> update_inventory_details()
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
                    string fddevice_Inventory = "";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Inventory";
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
                       

                        string fddeviceInventoryPersonnel = "";
                        fddeviceInventoryPersonnel = provider.FormData.GetValues("inventorypersonnel").SingleOrDefault();
                        List<device_inventory_personnel> deviceInventoryPersonnel = JsonConvert.DeserializeObject<List<device_inventory_personnel>>(fddeviceInventoryPersonnel);

                        if (deviceInventoryPersonnel.Count > 0)
                        {

     
                            foreach (var item in deviceInventoryPersonnel)
                            {
                                 item.is_approved = true;
                                item.date_reviews = DateTime.Now;
                                item.modified_date = DateTime.Now;
                                 item.modified_by = uid;
                                 item.modified_ip = ip;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();

                            }
                            #region sendhub
                            var inventory_id = deviceInventoryPersonnel[0].inventory_slip_id;
                            var count = db.device_inventory_personnel.Where(s => (s.is_approved == false && s.inventory_slip_id== inventory_id)).ToList();

                            if (count.Count == 0)
                            {
                                var device_Inventory_Slip = db.device_inventory_slip.Where(s => (s.inventory_slip_id == inventory_id)).FirstOrDefault();

                                var userSenHub = db.sys_users.Where(s => (s.user_id == device_Inventory_Slip.inventory_user_id)).FirstOrDefault();

                                var sh = new sys_sendhub();
                                sh.senhub_id = helper.GenKey();
                                sh.user_send = uid;
                                sh.module_key = const_module_key;
                                sh.receiver = userSenHub.user_id;
                                sh.icon = userSenHub.avatar;
                                sh.title = "Thiết bị";
                                sh.contents = "Hoàn thành kiểm kê: " + device_Inventory_Slip.inventory_number;
                                sh.type = 6;
                                sh.is_type = 3;
                                sh.date_send = DateTime.Now;
                                sh.id_key = device_Inventory_Slip.inventory_slip_id.ToString();
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

                        }
                        //#region add device_process_log


                        //device_process_log device_Process_Log = new device_process_log();
                        //device_Process_Log.content = "Cập nhật phiếu";
                        //device_Process_Log.device_note_id = device_Inventory.inventory_slip_id;
                        //device_Process_Log.device_process_type = 1;
                        //device_Process_Log.device_process_id = null;
                        //device_Process_Log.created_by = uid;
                        //device_Process_Log.created_date = DateTime.Now;
                        //device_Process_Log.created_ip = ip;
                        //db.device_process_log.Add(device_Process_Log);
                        //db.SaveChanges();
                        //#endregion
                        //#region add device_log
                        //if (helper.wlog)
                        //{

                        //    device_log log = new device_log();
                        //    log.title = "Sửa phiếu kiểm kê " + device_Inventory.inventory_number;

                        //    log.log_module = "device_inventory_slip";
                        //    log.log_type = 1;
                        //    log.id_key = device_Inventory.inventory_slip_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.device_log.Add(log);

                        //}
                        //#endregion

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
        public async Task<HttpResponseMessage> delete_device_inventory_slip([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.device_inventory_slip.Where(a => id.Contains(a.inventory_slip_id)).ToListAsync();
                        var das1 = await db.device_inventory_details.Where(a => id.Contains(a.inventory_slip_id)).ToListAsync();
                        var das2 = await db.device_inventory_files.Where(a => id.Contains(a.device_inventory_id)).ToListAsync();
                        var das3 = await db.device_inventory_personnel.Where(a => id.Contains(a.inventory_slip_id)).ToListAsync();
                       
                        List<string> paths = new List<string>();
                        if (das.Count > 0)
                        {
                            List<device_inventory_slip> del = new List<device_inventory_slip>();

                            foreach (var da in das)
                            {

                                if (uid == da.created_by || (int.Parse(dvid) == da.organization_id && ad) || super)
                                {

                                    del.Add(da);
                                    #region add device_log
                                    if (helper.wlog)
                                    {

                                        device_log log = new device_log();
                                        log.title = "Xóa phiếu kiểm kê " + da.inventory_number;
                                        log.log_module = "device_inventory_slip";
                                        log.log_type = 2;
                                        log.id_key = da.inventory_slip_id.ToString();
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
                            db.device_inventory_slip.RemoveRange(del);
                        }
                        if (das1.Count > 0)
                        {
                            List<device_inventory_details> del1 = new List<device_inventory_details>();

                            foreach (var da in das1)
                            {
                                del1.Add(da);
                            }
                            db.device_inventory_details.RemoveRange(del1);
                        }
                        if (das2.Count > 0)
                        {
                            List<device_inventory_files> del2 = new List<device_inventory_files>();

                            foreach (var da in das2)
                            {
                                del2.Add(da);

                                if (!string.IsNullOrWhiteSpace(da.file_path))
                                    paths.Add( da.file_path);
                            }

                            db.device_inventory_files.RemoveRange(del2);
                        }
                        if (das3.Count > 0)
                        {
                            List<device_inventory_personnel> del3 = new List<device_inventory_personnel>();

                            foreach (var da in das3)
                            {
                                del3.Add(da);


                            }

                            db.device_inventory_personnel.RemoveRange(del3);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/Inventory/") + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/Inventory/") + Path.GetFileName(strP));
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_inventory_slip/delete_device_inventory_slip", ip, tid, "Lỗi khi xoá phiếu kiểm kê", 0, "device_inventory_slip");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_inventory_slip/delete_device_inventory_slip", ip, tid, "Lỗi khi xoá phiếu kiểm kê", 0, "device_inventory_slip");
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
        public async Task<HttpResponseMessage> update_s_device_inventory_slip([System.Web.Mvc.Bind(Include = "IntID,IntTrangthai")] Trangthai trangthai)
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
                        var int_id = int.Parse(trangthai.IntID.ToString());
                        var das = db.device_inventory_slip.Where(a => (a.inventory_slip_id == int_id)).FirstOrDefault<device_inventory_slip>();
                        var des = db.device_inventory_personnel.Where(a => (a.inventory_slip_id == int_id)).ToArray<device_inventory_personnel>();
                        if (das != null)
                        {
                            if (des != null)
                            {
                                foreach (var item in des)
                                {
                                    //if(item.is_approved == null)
                                    item.is_approved = false;
                           
                                }
                    
                                var lidip= db.device_inventory_personnel.Where(a => (a.inventory_slip_id == trangthai.IntID))
                                    .Select(s=> new { s.user_id, s.full_name, s.avatar }).Distinct()
                                    .ToList();
                                foreach (var item in lidip)
                                {
                                    #region sendhub
                                    var userSenHub = db.sys_users.Where(s => (s.user_id == item.user_id)).FirstOrDefault();
                                    var sh = new sys_sendhub();
                                    sh.senhub_id = helper.GenKey();
                                    sh.user_send = uid;
                                    sh.module_key = const_module_key;
                                    sh.receiver = userSenHub.user_id;
                                    sh.icon = userSenHub.avatar;
                                    sh.title = "Thiết bị";
                                    sh.contents = "Gửi đánh giá kiểm kê: " + das.inventory_number;
                                    sh.type = 6;
                                    sh.is_type = 3;
                                    sh.date_send = DateTime.Now;
                                    sh.id_key = das.inventory_slip_id.ToString();
                                    sh.group_id = null;
                                    sh.token_id = tid;
                                    sh.created_date = DateTime.Now;
                                    sh.created_by = uid;
                                    sh.created_token_id = tid;
                                    sh.created_ip = ip;
                                    db.sys_sendhub.Add(sh);
                                    db.SaveChanges();

                                    #endregion
                                }
                            }
                          
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            das.status = trangthai.IntTrangthai;
                            await db.SaveChangesAsync();
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_status/update_s_device_status", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_status", 0, "device_status");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_status/update_s_device_status", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_status", 0, "device_status");
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