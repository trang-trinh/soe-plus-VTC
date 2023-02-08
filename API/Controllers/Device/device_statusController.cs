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
    public class device_statusController : ApiController
    {
        public string getipaddress()
        {return  HttpContext.Current.Request.UserHostAddress;
        }
 

        [HttpPost]
        public async Task<HttpResponseMessage> add_device_status([System.Web.Mvc.Bind(Include = "organization_id,created_date,created_by,created_token_id,created_ip,modified_by,modified_date,modified_ip,modified_token_id,device_status_name,device_status_id")] device_status device_Status)
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
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
   
      
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
           
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    
                    var str = device_Status.device_status_code.ToString();
                    var device_StatusOld = db.device_status.AsNoTracking().Where(s => s.device_status_code == str).FirstOrDefault<device_status>();
                    if (device_StatusOld != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã trạng thái đã tồn tại, vui lòng cập nhật lại!", err = "1" });

                    }
                    string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                    device_Status.organization_id = super == "True" ? 0 : int.Parse(dvid);
                    device_Status.created_date = DateTime.Now;
                    device_Status.created_by = uid;
                    device_Status.created_token_id = tid;
                    device_Status.created_ip = ip;
                    device_Status.modified_by = uid;
                    device_Status.modified_date = DateTime.Now;
                    device_Status.modified_ip = ip;
                    device_Status.modified_token_id = tid;
                    db.device_status.Add(device_Status);
                    await db.SaveChangesAsync();
                    #region add device_log
                    if (helper.wlog)
                    {

                        device_log log = new device_log();
                        log.title = "Thêm trạng thái tài sản " + device_Status.device_status_name;

                        log.log_module = "device_status";
                        log.log_type = 0;
                        log.id_key = device_Status.device_status_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Status/add_device_status", ip, tid, "Lỗi khi thêm trạng thái tài sản", 0, "device_Status");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Status/add_device_status", ip, tid, "Lỗi khi thêm trạng thái tài sản", 0, "device_Status");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_device_status([System.Web.Mvc.Bind(Include = "modified_by,modified_date,modified_ip,modified_token_id,device_status_name,device_status_id")] device_status device_Status)
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
                    var device_StatusOld = db.device_status.AsNoTracking().Where(s => s.device_status_code == device_Status.device_status_code && s.device_status_id != device_Status.device_status_id).FirstOrDefault<device_status>();
                    if (device_StatusOld != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã trạng thái đã tồn tại, vui lòng cập nhật lại!", err = "1" });

                    }
                    device_Status.modified_by = uid;
                    device_Status.modified_date = DateTime.Now;
                    device_Status.modified_ip = ip;
                    device_Status.modified_token_id = tid;
                    db.Entry(device_Status).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    #region add device_log
                    if (helper.wlog)
                    {

                        device_log log = new device_log();
                        log.title = "Sửa trạng thái tài sản " + device_Status.device_status_name;

                        log.log_module = "device_status";
                        log.id_key = device_Status.device_status_id.ToString();
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
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Status/update_device_status", ip, tid, "Lỗi khi cập nhật trạng thái tài sản", 0, "device_Status");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Status/update_device_status", ip, tid, "Lỗi khi cập nhật trạng thái tài sản", 0, "device_Status");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_device_status([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.device_status.Where(a => id.Contains(a.device_status_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<device_status> del = new List<device_status>();
                            foreach (var da in das)
                            {
                                if (uid == da.created_by || (int.Parse(dvid) == da.organization_id && ad) || super)
                                {
                                    var device_main = db.device_card.AsNoTracking().Where(s => s.status == da.device_status_code).FirstOrDefault<device_card>();
                                    if (device_main != null)
                                    {
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Có thẻ tài sản đang dùng trạng thái tài sản này!" });
                                    }
                                    del.Add(da);
                                    #region add device_log
                                    if (helper.wlog)
                                    {

                                        device_log log = new device_log();
                                        log.title = "Xóa trạng thái tài sản " + da.device_status_name;

                                        log.log_module = "device_status";
                                        log.id_key = da.device_status_id.ToString();
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


                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.device_status.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Status/Delete_device_status", ip, tid, "Lỗi khi xoá trạng thái tài sản", 0, "device_Status");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Status/Delete_device_status", ip, tid, "Lỗi khi xoá trạng thái tài sản", 0, "device_Status");
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
        public async Task<HttpResponseMessage> update_s_device_status([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                        var das = db.device_status.Where(a => (a.device_status_id == int_id)).FirstOrDefault<device_status>();
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
                                log.title = "Sửa trạng thái tài sản " + das.device_status_name;

                                log.log_module = "device_status";
                                log.id_key = das.device_status_id.ToString();
                                log.log_type = 1;
                                log.created_date = DateTime.Now;
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