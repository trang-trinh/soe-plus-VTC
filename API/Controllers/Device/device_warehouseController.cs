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
    public class device_warehouseController : ApiController
    {

        public string getipaddress()
        {
           return  HttpContext.Current.Request.UserHostAddress;
        }

       
        [HttpPost]
        public async Task<HttpResponseMessage> add_device_warehouse([System.Web.Mvc.Bind(Include = "organization_id,created_date,created_by,created_token_id,created_ip,modified_by,modified_date,modified_ip,modified_token_id,warehouse_name,warehouse_id")] device_warehouse device_Warehouse)
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
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                    bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                    device_Warehouse.organization_id = super ? 0 : int.Parse(dvid);
                    device_Warehouse.created_date = DateTime.Now;
                    device_Warehouse.created_by = uid;
                    device_Warehouse.created_token_id = tid;
                    device_Warehouse.created_ip = ip;
                    device_Warehouse.modified_by = uid;
                    device_Warehouse.modified_date = DateTime.Now;
                    device_Warehouse.modified_ip = ip;
                    device_Warehouse.modified_token_id = tid;
                    db.device_warehouse.Add(device_Warehouse);
                    await db.SaveChangesAsync();
                    if (device_Warehouse.stocker_name != null)
                    {
                       
                        foreach (var item in device_Warehouse.stocker_name.Split(','))
                        {
                            device_warehouse_stocker device_Warehouse_Stocker = new device_warehouse_stocker();
                            device_Warehouse_Stocker.warehouse_id = device_Warehouse.warehouse_id;
                            device_Warehouse_Stocker.stocker = item;
                            device_Warehouse_Stocker.created_by = uid;
                            device_Warehouse_Stocker.created_token_id = tid;
                            device_Warehouse_Stocker.created_ip = ip;
                            device_Warehouse_Stocker.modified_by = uid;
                            device_Warehouse_Stocker.modified_date = DateTime.Now;
                            device_Warehouse_Stocker.modified_ip = ip;
                            device_Warehouse_Stocker.modified_token_id = tid;
                            db.device_warehouse_stocker.Add(device_Warehouse_Stocker);
                            await db.SaveChangesAsync();
                        }
                    }
                    #region add device_log
                    if (helper.wlog)
                    {

                        device_log log = new device_log();
                        log.title = "Thêm kho " + device_Warehouse.warehouse_name;
     
                        log.log_module = "device_warehouse";
                        log.id_key = device_Warehouse.warehouse_id.ToString();
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
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Warehouse/Add_email_Groups", ip, tid, "Lỗi khi thêm Kho", 0, "device_Warehouse");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Warehouse/Add_email_Groups", ip, tid, "Lỗi khi thêm Kho", 0, "device_Warehouse");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_device_warehouse([System.Web.Mvc.Bind(Include = "modified_by,modified_date,modified_ip,modified_token_id,warehouse_name,warehouse_id")] device_warehouse device_Warehouse)
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
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
            
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    device_Warehouse.modified_by = uid;
                    device_Warehouse.modified_date = DateTime.Now;
                    device_Warehouse.modified_ip = ip;
                    device_Warehouse.modified_token_id = tid;
                    db.Entry(device_Warehouse).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    var das = await db.device_warehouse_stocker.Where(a =>  a.warehouse_id==device_Warehouse.warehouse_id).ToListAsync();
                    db.device_warehouse_stocker.RemoveRange(das);
                    await db.SaveChangesAsync();
                    if (device_Warehouse.stocker_name != null)
                    {

                        foreach (var item in device_Warehouse.stocker_name.Split(','))
                        {
                            device_warehouse_stocker device_Warehouse_Stocker = new device_warehouse_stocker();
                            device_Warehouse_Stocker.warehouse_id = device_Warehouse.warehouse_id;
                            device_Warehouse_Stocker.stocker = item;
                            device_Warehouse_Stocker.created_by = uid;
                            device_Warehouse_Stocker.created_token_id = tid;
                            device_Warehouse_Stocker.created_ip = ip;
                            device_Warehouse_Stocker.modified_by = uid;
                            device_Warehouse_Stocker.modified_date = DateTime.Now;
                            device_Warehouse_Stocker.modified_ip = ip;
                            device_Warehouse_Stocker.modified_token_id = tid;
                            db.device_warehouse_stocker.Add(device_Warehouse_Stocker);
                            await db.SaveChangesAsync();
                        }
                    }

                    #region add device_log
                    if (helper.wlog)
                    {

                        device_log log = new device_log();
                        log.title = "Sửa kho " + device_Warehouse.warehouse_name;
             
                        log.log_module = "device_warehouse";
                        log.id_key = device_Warehouse.warehouse_id.ToString();
                        log.created_date = DateTime.Now;
                        log.log_type = 1;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.device_log.Add(log);
                        db.SaveChanges();


                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Warehouse/Update_email_groups", ip, tid, "Lỗi khi cập nhật Nhóm email", 0, "device_Warehouse");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Warehouse/Update_email_groups", ip, tid, "Lỗi khi cập nhật Nhóm email", 0, "device_Warehouse");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_device_warehouse([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.device_warehouse.Where(a => id.Contains(a.warehouse_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<device_warehouse> del = new List<device_warehouse>();
                            foreach (var da in das)
                            {
                                
                                del.Add(da);
                                var saaas = await db.device_warehouse_stocker.Where(a => a.warehouse_id == da.warehouse_id).ToListAsync();
                                db.device_warehouse_stocker.RemoveRange(saaas);
                                await db.SaveChangesAsync();
                                #region add device_log
                                if (helper.wlog)
                                {

                                    device_log log = new device_log();
                                    log.title = "Xóa kho " + da.warehouse_name;
                                 
                                    log.log_module = "device_warehouse";
                                    log.id_key = da.warehouse_id.ToString();
                                    log.created_date = DateTime.Now;
                                    log.log_type = 2;
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
                            db.device_warehouse.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Warehouse/Delete_email_groups", ip, tid, "Lỗi khi xoá Kho", 0, "device_Warehouse");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Warehouse/Delete_email_groups", ip, tid, "Lỗi khi xoá Kho", 0, "device_Warehouse");
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
        public async Task<HttpResponseMessage> update_s_device_warehouse([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                        var das = db.device_warehouse.Where(a => (a.warehouse_id == int_id)).FirstOrDefault<device_warehouse>();
                        if (das != null)
                        {
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            das.status = !trangthai.BitTrangthai;
                            #region add device_log
                            if (helper.wlog)
                            {

                                device_log log = new device_log();
                                log.title = "Sửa kho " + das.warehouse_id;
                     
                                log.log_module = "device_warehouse";
                                log.id_key = das.warehouse_id.ToString();
                                log.created_date = DateTime.Now;
                                log.log_type = 1;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.device_log.Add(log);
                                db.SaveChanges();


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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_warehouse/update_s_device_warehouse", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_warehouse", 0, "device_warehouse");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_warehouse/update_s_device_warehouse", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_warehouse", 0, "device_warehouse");
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