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
    public class device_mainController : ApiController
    {

        public string getipaddress()
        {
         return  HttpContext.Current.Request.UserHostAddress;
        }

      
        [HttpPost]
        public async Task<HttpResponseMessage> add_device_main([System.Web.Mvc.Bind(Include = "organization_id,created_date,created_by,created_token_id,created_ip,modified_by,modified_date,modified_ip,modified_token_id,device_name,device_id")] device_main device_Main)
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
                    device_Main.organization_id = super ? 0 : int.Parse(dvid);
                    device_Main.created_date = DateTime.Now;
                    device_Main.created_by = uid;
                    device_Main.created_token_id = tid;
                    device_Main.created_ip = ip;
                    device_Main.modified_by = uid;
                    device_Main.modified_date = DateTime.Now;
                    device_Main.modified_ip = ip;
                    device_Main.modified_token_id = tid;
                    db.device_main.Add(device_Main);
                    await db.SaveChangesAsync();
                    #region add device_log
                    if (helper.wlog)
                    {

                        device_log log = new device_log();
                        log.title = "Thêm tài sản " + device_Main.device_name;
      
                        log.log_module = "device_main";
                        log.id_key = device_Main.device_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Main/add_main", ip, tid, "Lỗi khi thêm tài sản", 0, "device_Main");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Main/add_main", ip, tid, "Lỗi khi thêm tài sản", 0, "device_Main");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_device_main([System.Web.Mvc.Bind(Include = "modified_by,modified_date,modified_ip,modified_token_id,device_name,device_id")] device_main device_Main)
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
                    device_Main.modified_by = uid;
                    device_Main.modified_date = DateTime.Now;
                    device_Main.modified_ip = ip;
                    device_Main.modified_token_id = tid;
                    db.Entry(device_Main).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    #region add device_log
                    if (helper.wlog)
                    {

                        device_log log = new device_log();
                        log.title = "Sửa tài sản " + device_Main.device_name;
           
                        log.log_module = "device_main";
                        log.log_type = 1;
                        log.id_key = device_Main.device_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Main/Update_email_groups", ip, tid, "Lỗi khi cập nhật tài sản", 0, "device_Main");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Main/Update_email_groups", ip, tid, "Lỗi khi cập nhật tài sản", 0, "device_Main");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_device_main([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.device_main.Where(a => id.Contains(a.device_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<device_main> del = new List<device_main>();
                            foreach (var da in das)
                            {
                                var device_card = db.device_card.AsNoTracking().Where(s => s.device_id == da.device_id).FirstOrDefault<device_card>();
                                if (device_card != null)
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Tồn tại thẻ tài sản thuộc tài sản này!" });
                                }
                                if (uid == da.created_by || (int.Parse(dvid) == da.organization_id && ad) || super)
                                {
                                    del.Add(da);
                                    #region add device_log
                                    if (helper.wlog)
                                    {

                                        device_log log = new device_log();
                                        log.title = "Xóa tài sản " + da.device_name;
                                        log.log_module = "device_main";
                                        log.log_type = 2;
                                        log.id_key = da.device_id.ToString();
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
                            db.device_main.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Main/Delete_main", ip, tid, "Lỗi khi xoá tài sản", 0, "device_Main");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Main/Delete_main", ip, tid, "Lỗi khi xoá tài sản", 0, "device_Main");
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
        public async Task<HttpResponseMessage> update_s_device_main([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                        var das = db.device_main.Where(a => (a.device_id == int_id)).FirstOrDefault<device_main>();
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
                                log.title = "Sửa tài sản " + das.device_name;
               
                                log.log_module = "device_main";
                                log.log_type = 1;
                                log.id_key = das.device_id.ToString();
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_main/update_s_device_main", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_main", 0, "device_main");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_main/update_s_device_main", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_main", 0, "device_main");
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