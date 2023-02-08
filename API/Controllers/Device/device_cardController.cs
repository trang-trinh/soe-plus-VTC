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
using ZXing;

namespace API.Controllers.Device
{
    [Authorize(Roles = "login")]
    public class device_cardController : ApiController
    {

        public string getipaddress()
        {
      return  HttpContext.Current.Request.UserHostAddress;
        }
        public class barCodeReader
        {
            public string barcodestr { get; set; }
            public string barcode_old { get; set; }
            public int barcode_type { get; set; }
        }
        #region CallProc
       
        [HttpPost]
        public async Task<HttpResponseMessage> getData([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
             var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);
          
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
                    var sqlpas = new List<SqlParameter>();
                    if (proc != null && proc.par != null)
                    {
                        foreach (sqlPar p in proc.par)
                        {
                            sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                        }
                    }
                    var arrpas = sqlpas.ToArray();
                    DateTime sdate = DateTime.Now;
                    var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables);
                    var tables = await task;
                    DateTime edate = DateTime.Now;
                    #region add SQLLog
                    if (helper.wlog)
                    {
                        using (DBEntities db = new DBEntities())
                        {
                            sql_log log = new sql_log();
                            log.controller = domainurl + "Proc/CallProc";
                            log.start_date = sdate;
                            log.end_date = edate;
                            log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                            log.user_id = uid;
                            log.token_id = tid;
                            log.created_ip = ip;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.modified_ip = ip;
                            log.modified_date = DateTime.Now;
                            log.modified_by = uid;
                            log.modified_token_id = tid;
                            log.full_name = name;
                            log.title = proc.proc;
                            log.log_content = JsonConvert.SerializeObject(new { data = proc });
                            db.sql_log.Add(log);
                            await db.SaveChangesAsync();
                        }
                    }
                    #endregion
                    string JSONresult = JsonConvert.SerializeObject(tables);
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);

                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Proc/CallProc", ip, tid, "Lỗi khi gọi proc " , 0, "Proc");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Proc/CallProc", ip, tid, "Lỗi khi gọi proc "  , 0, "Proc");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
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
        #endregion
        [HttpPost]
        public async Task<HttpResponseMessage> GenBarcode()
        {
             var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdcard = "";
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


                    fdcard = provider.FormData.GetValues("barcode").SingleOrDefault();
                    var card = JsonConvert.DeserializeObject<barCodeReader>(fdcard);
                        if (card.barcode_old != null && card.barcode_old != "")
                        {
                            if (File.Exists(root + card.barcode_old.Substring(8)))
                                File.Delete(root + card.barcode_old.Substring(8));
                        }
                        if (card.barcodestr == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                        }
                        else
                        {
                          
                            db.Configuration.LazyLoadingEnabled = false;
                            db.Configuration.ProxyCreationEnabled = false;
                            var barcodeWriter = new BarcodeWriter
                            {
                                Format = card.barcode_type == 1 ? BarcodeFormat.QR_CODE : BarcodeFormat.CODE_128,
                                //Format = BarcodeFormat.CODE_128,
                                Options = new ZXing.Common.EncodingOptions
                                {
                                    //Width = 300,
                                    //Height = 50,
                                    Width = card.barcode_type == 1 ? 200 : 300,
                                    Height = card.barcode_type == 1 ? 200 : 50,
                                    Margin = 0
                                }
                            };
                            try
                            {
                                var barcode = barcodeWriter.Write(card.barcodestr);
                                string nameFile = helper.GenKey().Substring(0, 5) + "-orient-" + card.barcodestr;

                                barcode.Save(strPath + "/" + nameFile + ".png");
                                string JSONresult = JsonConvert.SerializeObject("/Portals/" + dvid + "/Device/" + nameFile + ".png");
                                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                            }
                            catch (Exception)
                            {

                                return Request.CreateResponse(HttpStatusCode.OK, new { data = "Mã Barcode không hợp lệ!", err = "1" });
                            }

                        }
                    });
                
                    return await task;
                }

            }

            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdcard, contents }), domainurl + "device_card/Addshows_main", ip, tid, "Lỗi khi thêm Tin tức", 0, "device_card");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdcard, contents }), domainurl + "device_card/Addshows_main", ip, tid, "Lỗi khi thêm Tin tức", 0, "device_card");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_device_card()
        {
             var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdcard = "";
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
                        fdcard = provider.FormData.GetValues("card").SingleOrDefault();
                        device_card card = JsonConvert.DeserializeObject<device_card>(fdcard);
                        if(card.device_name!=null)
                        if (card.device_name.Length > 250)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tên tài sản không được lớn hơn 250 kí tự!", err = "1" });
                        }
                        if (card.producer != null)
                            if (card.producer.Length > 350)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Nhà sản xuất không được lớn hơn 350 kí tự!", err = "1" });
                        }
                       
                        if (card.warranty_company != null)
                            if (card.warranty_company.Length > 350)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Công ty bảo hành không được lớn hơn 350 kí tự!", err = "1" });
                        }
                        if (card.warranty_company_address != null)
                            if (card.warranty_company_address.Length > 350)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tên tài sản không được lớn hơn 250 kí tự!", err = "1" });
                        }
                        if (card.warranty_contact != null)
                            if (card.warranty_contact.Length > 250)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Người liên hệ không được lớn hơn 250 kí tự!", err = "1" });
                        }
                        if (card.barcode_id != null)
                            if (card.barcode_id.Length > 250)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã Barcode không được lớn hơn 250 kí tự!", err = "1" });
                        }
                        if (card.assets_condition != null)
                            if (card.assets_condition.Length > 250)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tình trạng không được lớn hơn 250 kí tự!", err = "1" });
                        }


                        var checkBarcode =    db.device_card.Where(a =>   a.barcode_id== card.barcode_id).FirstOrDefault();



                        if(checkBarcode!=null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã Barcode đã tồn tại! Vui lòng nhập lại", err = "1" } );
                        }


                        // This illustrates how to get thefile names.
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Device", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Device", fileName);
                            }
                            if (helper.IsImageFileName(newFileName))
                            {
                                card.image = "/Portals/" + dvid + "/Device/" + fileName;
                                ffileData = fileData;
                                //Add ảnh
                                if (fileInfo != null)
                                {
                                    if (!Directory.Exists(fileInfo.Directory.FullName))
                                    {
                                        Directory.CreateDirectory(fileInfo.Directory.FullName);
                                    }
                                    File.Move(ffileData.LocalFileName, newFileName);
                                    helper.ResizeImage(newFileName, 640, 640, 90);
                                }
                            }
                        }
                        if (card.barcode_image != null && card.barcode_image != "")
                        {
                            if (File.Exists(root + card.barcode_image.Substring(8)))
                                File.Delete(root + card.barcode_image.Substring(8));
                        }
                        if (card.barcode_id!=null )
                        {
                            db.Configuration.LazyLoadingEnabled = false;
                            db.Configuration.ProxyCreationEnabled = false;
                            var barcodeWriter = new BarcodeWriter
                            {
                                Format = card.barcode_type == 1 ? BarcodeFormat.QR_CODE : BarcodeFormat.CODE_128,
                                //Format = BarcodeFormat.CODE_128,
                                Options = new ZXing.Common.EncodingOptions
                                {
                                    //Width = 300,
                                    //Height = 50,
                                    Width = card.barcode_type == 1 ? 200 : 300,
                                    Height = card.barcode_type == 1 ? 200 : 50,
                                    Margin = 0
                                }
                            };
                            try
                            {
                                var barcode = barcodeWriter.Write(card.barcode_id);
                                string nameFile = helper.GenKey().Substring(0, 5) + "-orient-" + card.barcode_id;

                                barcode.Save(strPath + "/" + nameFile + ".png");
                                string str = "/Portals/" + dvid + "/Device/" + nameFile + ".png";
                                card.barcode_image = str;
                            }
                            catch (Exception e)
                            {

                                return Request.CreateResponse(HttpStatusCode.OK, new { data = "Mã Barcode không hợp lệ! vui lòng thử lại", err = "1" });
                            }


                        }
                        if (card.status == "DSD")
                            card.status = "CXN";
                        card.organization_id = int.Parse(dvid);
                        card.created_date = DateTime.Now;
                        card.created_by = uid;
                        card.created_ip = ip;
                        card.modified_date = DateTime.Now;
                        card.modified_by = uid;
                        card.modified_ip = ip;
                        db.device_card.Add(card);
                        db.SaveChanges();

                        if (card.status == "CXN")
                        {
                            var userR = db.sys_users.Where(s => s.user_id == card.device_user_id
                           ).FirstOrDefault();
                            var posiR = db.ca_positions.Where(s => s.position_id == userR.position_id
                          ).FirstOrDefault();
                            var fdhandover = "";
                            fdhandover = provider.FormData.GetValues("handover").SingleOrDefault();
                            device_handover handover = JsonConvert.DeserializeObject<device_handover>(fdhandover);
                            handover.user_receiver_id = card.device_user_id;
                            handover.user_receiver_name = userR.full_name;
                            handover.user_receiver_department_id = userR.department_id;
                            handover.user_receiver_position = posiR.position_name !=null? posiR.position_name : null;
                            handover.organization_id = int.Parse(dvid);
                            handover.created_date = DateTime.Now;
                            handover.created_by = uid;
                            handover.created_ip = ip;
                            handover.modified_date = DateTime.Now;
                            handover.modified_by = uid;
                            handover.modified_ip = ip;
                            handover.device_department_id = userR.organization_id;
                            db.device_handover.Add(handover);
                            db.SaveChanges();
                            device_handover_attach handover_Attach = new device_handover_attach();
                            handover_Attach.card_id = card.card_id;
                            handover_Attach.device_name = card.device_name;
                            handover_Attach.serial = card.barcode_id;
                            handover_Attach.device_id = card.device_id;
                            handover_Attach.condition = card.assets_condition;
                            handover_Attach.note = card.device_note;
                            handover_Attach.user_id = handover.user_receiver_id;
                            handover_Attach.price = card.current_price;
                            handover_Attach.organization_id = int.Parse(dvid);
                            handover_Attach.handover_id = handover.handover_id;
                            handover_Attach.created_date = DateTime.Now;
                            handover_Attach.created_by = uid;
                            handover_Attach.created_ip = ip;
                            handover_Attach.modified_date = DateTime.Now;
                            handover_Attach.modified_by = uid;
                            handover_Attach.modified_ip = ip;
                            db.device_handover_attach.Add(handover_Attach);
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

                            db.device_process.Add(device_Process_Pre); db.SaveChanges();
                        }
                        #region add device_log
                        if (helper.wlog)
                        {

                            device_log log = new device_log();
                            log.title = "Thêm thẻ tài sản " + card.device_name;
           
                            log.log_module = "device_card";
                            log.log_type = 0;
                            log.id_key = card.card_id.ToString();
                            log.created_date = DateTime.Now;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdcard, contents }), domainurl + "device_card/Adddevice_card", ip, tid, "Lỗi khi thêm Tin tức", 0, "device_card");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdcard, contents }), domainurl + "device_card/Adddevice_card", ip, tid, "Lỗi khi thêm Tin tức", 0, "device_card");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_device_card()
        {
             var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdcard = "";
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

                    string root = HttpContext.Current.Server.MapPath("~/Portals") ;
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
                        fdcard = provider.FormData.GetValues("card").SingleOrDefault();
                        device_card card = JsonConvert.DeserializeObject<device_card>(fdcard);








                        if (card.device_name != null)
                            if (card.device_name.Length > 250)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tên tài sản không được lớn hơn 250 kí tự!", err = "1" });
                            }
                        if (card.producer != null)
                            if (card.producer.Length > 350)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Nhà sản xuất không được lớn hơn 350 kí tự!", err = "1" });
                            }

                        if (card.warranty_company != null)
                            if (card.warranty_company.Length > 350)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Công ty bảo hành không được lớn hơn 350 kí tự!", err = "1" });
                            }
                        if (card.warranty_company_address != null)
                            if (card.warranty_company_address.Length > 350)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tên tài sản không được lớn hơn 250 kí tự!", err = "1" });
                            }
                        if (card.warranty_contact != null)
                            if (card.warranty_contact.Length > 250)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Người liên hệ không được lớn hơn 250 kí tự!", err = "1" });
                            }
                        if (card.barcode_id != null)
                            if (card.barcode_id.Length > 250)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã Barcode không được lớn hơn 250 kí tự!", err = "1" });
                            }
                        if (card.assets_condition != null)
                            if (card.assets_condition.Length > 250)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tình trạng không được lớn hơn 250 kí tự!", err = "1" });
                            }

                        var checkBarcode = db.device_card.Where(a => a.barcode_id == card.barcode_id && a.card_id !=card.card_id).FirstOrDefault();



                        if (checkBarcode != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã Barcode đã tồn tại! Vui lòng nhập lại", err = "1" });
                        }




                        var showsOld = db.device_card.AsNoTracking().Where(s => s.card_id == card.card_id).FirstOrDefault<device_card>();
                        // This illustrates how to get thefile names.
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Device", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Device", fileName);
                            }

                            if (helper.IsImageFileName(newFileName))
                            {
                                card.image = "/Portals/" + dvid + "/Device/" + fileName;
                                ffileData = fileData;
                       
                                //Add ảnh
                                if (fileInfo != null)
                                {
                                    if (!Directory.Exists(fileInfo.Directory.FullName))
                                    {
                                        Directory.CreateDirectory(fileInfo.Directory.FullName);
                                    }
                                    File.Move(ffileData.LocalFileName, newFileName);
                                    helper.ResizeImage(newFileName, 640, 640, 90);
                                }
                            }
                          
                        }
                        if (showsOld.image != null && showsOld.image != "" && showsOld.image != card.image)
                        {
                            string fileOld = showsOld.image.Substring(8);
                            delfiles.Add(  fileOld);
                        }
                        if (showsOld.barcode_id != card.barcode_id || showsOld.barcode_image!= card.barcode_image)
                        {
                            if (card.barcode_image != null && card.barcode_image != "")
                            {
                                
                                if (File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Device/" +  Path.GetFileName(card.barcode_image)))
                                    File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Device/" + Path.GetFileName(card.barcode_image));
                            }
                            if (showsOld.barcode_image != null && showsOld.barcode_image != "")
                            {
                                if (File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Device/" + Path.GetFileName(showsOld.barcode_image)))
                                    File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Device/" + Path.GetFileName(showsOld.barcode_image));
                            }
                            db.Configuration.LazyLoadingEnabled = false;
                            db.Configuration.ProxyCreationEnabled = false;
                            var barcodeWriter = new BarcodeWriter
                            {
                                Format = card.barcode_type == 1 ? BarcodeFormat.QR_CODE : BarcodeFormat.CODE_128,
                                //Format = BarcodeFormat.CODE_128,
                                Options = new ZXing.Common.EncodingOptions
                                {
                                    //Width = 300,
                                    //Height = 50,
                                    Width = card.barcode_type == 1 ? 200 : 300,
                                    Height = card.barcode_type == 1 ? 200 : 50,
                                    Margin = 0
                                }
                            };
                            try
                            {
                                var barcode = barcodeWriter.Write(card.barcode_id);
                                string nameFile = helper.GenKey().Substring(0, 5) + "-orient-" + card.barcode_id;

                                barcode.Save(strPath + "/" + nameFile + ".png");
                                string str = "/Portals/" + dvid + "/Device/" + nameFile + ".png";
                                card.barcode_image = str;
                            }
                            catch (Exception e)
                            {

                                return Request.CreateResponse(HttpStatusCode.OK, new { data = "Có lỗi xảy ra! Vui lòng kiểm tra lại." + e, err = "1" });
                            }
                        }
                        card.organization_id = int.Parse(dvid);
                        card.modified_date = DateTime.Now;
                        card.modified_by = uid;
                        card.modified_ip = ip;
                        db.Entry(card).State = EntityState.Modified;
                        db.SaveChanges();
                        //Add ảnh
                        if (delfiles.Count > 0)
                        {
                            foreach (string fpath in delfiles)
                            {
                                if (File.Exists(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Device/" + Path.GetFileName(fpath)))
                                    File.Delete(HttpContext.Current.Server.MapPath("~/Portals") + "/" + dvid + "/Device/" + Path.GetFileName(fpath));
                            }
                        }

                        #region add device_log
                        if (helper.wlog)
                        {

                            device_log log = new device_log();
                            log.title = "Sửa thẻ tài sản " + card.device_name;
                   
                            log.log_module = "device_card";
                            log.id_key = card.card_id.ToString();
                            log.log_type = 1;
                            log.created_date = DateTime.Now;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdcard, contents }), domainurl + "device_card/update_device_card", ip, tid, "Lỗi khi cập nhật Tin tức", 0, "card");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdcard, contents }), domainurl + "device_card/update_device_card", ip, tid, "Lỗi khi cập nhật Tin tức", 0, "card");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_device_card([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
             var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;

             
            try
            {
                string root = HttpContext.Current.Server.MapPath("~/Portals");

                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                string strPath = root + "/" + dvid + "/Device";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.device_card.Where(a => id.Contains(a.card_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        if (das != null)
                        {
                            List<device_card> del = new List<device_card>();
                            foreach (var da in das)
                            {
                                if (ad || da.created_by == uid)
                                {

                                    del.Add(da);
                                    if (!string.IsNullOrWhiteSpace(da.barcode_image))
                                        paths.Add(  da.barcode_image);
                                    if (!string.IsNullOrWhiteSpace(da.image))
                                        paths.Add(  da.image);
                                }
                                #region add device_log
                                if (helper.wlog)
                                {

                                    device_log log = new device_log();
                                    log.title = "Xóa thẻ tài sản " + da.device_name;
        
                                    log.log_module = "device_card";
                                    log.id_key = da.card_id.ToString();
                                    log.log_type = 2;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.device_log.Add(log);
                                    db.SaveChanges();


                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.device_card.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/Device/") + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/Portals/" + Path.GetFileName(dvid) + "/Device/") + Path.GetFileName(strP));
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_card/Delete_device_card", ip, tid, "Lỗi khi xoá tin tức", 0, "card");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_card/Delete_device_card", ip, tid, "Lỗi khi xoá tin tức", 0, "card");
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