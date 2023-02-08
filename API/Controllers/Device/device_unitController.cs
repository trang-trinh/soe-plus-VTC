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
    public class device_unitController : ApiController
    {

            public string getipaddress()
            {
              return  HttpContext.Current.Request.UserHostAddress;
            }
 

        [HttpPost]
            public async Task<HttpResponseMessage> add_device_unit([System.Web.Mvc.Bind(Include = "organization_id,created_date,created_by,created_token_id,created_ip,modified_by,modified_date,modified_ip,modified_token_id,device_unit_name,device_unit_id")] device_unit device_Unit)
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
                    device_Unit.organization_id = super ? 0 : int.Parse(dvid);

                    device_Unit.created_date = DateTime.Now;
                        device_Unit.created_by = uid;
                        device_Unit.created_token_id = tid;
                        device_Unit.created_ip = ip;
                        device_Unit.modified_by = uid;
                        device_Unit.modified_date = DateTime.Now;
                        device_Unit.modified_ip = ip;
                        device_Unit.modified_token_id = tid;
                        db.device_unit.Add(device_Unit);
                        await db.SaveChangesAsync();

                    #region add device_log
                    if (helper.wlog)
                    {

                        device_log log = new device_log();
                        log.title = "Thêm đơn vị tính " + device_Unit.device_unit_name;
                
                        log.log_module = "device_unit";
                        log.id_key = device_Unit.device_unit_id.ToString();
                        log.log_type = 0;
                        log.created_date = DateTime.Now;
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Unit/add_unit", ip, tid, "Lỗi khi thêm Đơn vị tính", 0, "device_Unit");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Unit/add_unit", ip, tid, "Lỗi khi thêm Đơn vị tính", 0, "device_Unit");
                    if (!helper.debug)
                    {
                        contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            [HttpPut]
            public async Task<HttpResponseMessage> update_device_unit([System.Web.Mvc.Bind(Include = "modified_by,modified_date,modified_ip,modified_token_id,device_unit_name,device_unit_id")] device_unit device_Unit)
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
                        device_Unit.modified_by = uid;
                        device_Unit.modified_date = DateTime.Now;
                        device_Unit.modified_ip = ip;
                        device_Unit.modified_token_id = tid;
                        db.Entry(device_Unit).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    #region add device_log
                    if (helper.wlog)
                    {

                        device_log log = new device_log();
                        log.title = "Sửa đơn vị tính " + device_Unit.device_unit_name;
               
                        log.log_module = "device_unit";
                        log.id_key = device_Unit.device_unit_id.ToString();
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Unit/Update_email_groups", ip, tid, "Lỗi khi cập nhật Nhóm email", 0, "device_Unit");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Unit/Update_email_groups", ip, tid, "Lỗi khi cập nhật Nhóm email", 0, "device_Unit");
                    if (!helper.debug)
                    {
                        contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_device_unit([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.device_unit.Where(a => id.Contains(a.device_unit_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {

                            List<device_unit> del = new List<device_unit>();
                            foreach (var da in das)
                            {
                                var device_ubit = db.device_main.AsNoTracking().Where(s => s.device_unit_id == da.device_unit_id).FirstOrDefault<device_main>();
                                if (device_ubit != null)
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Có tài sản đang dùng đơn vị tính này!" });
                                }
                                del.Add(da);
                                #region add device_log
                                if (helper.wlog)
                                {

                                    device_log log = new device_log();
                                    log.title = "Xóa đơn vị tính " + da.device_unit_name;
                         
                                    log.log_module = "device_unit";
                                    log.id_key = da.device_unit_id.ToString();
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
                            db.device_unit.RemoveRange(del);
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
        public async Task<HttpResponseMessage> update_s_device_unit([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                        var das = db.device_unit.Where(a => (a.device_unit_id == int_id)).FirstOrDefault<device_unit>();
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
                                log.title = "Sửa đơn vị tính " + das.device_unit_name;
                       
                                log.log_module = "device_unit";
                                log.id_key = das.device_unit_id.ToString();
                                log.created_date = DateTime.Now;
                                log.log_type = 0;
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_unit/update_s_device_unit", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_unit", 0, "device_unit");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_unit/update_s_device_unit", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_unit", 0, "device_unit");
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
