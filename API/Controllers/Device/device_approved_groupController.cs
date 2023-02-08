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
    public class device_approved_groupController : ApiController
    {
        public string getipaddress()
        {
          return  HttpContext.Current.Request.UserHostAddress;
        }
       
        [HttpPost]
        public async Task<HttpResponseMessage> add_device_approved_group()
        {
             var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdhandover = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            int dvid = int.Parse(claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value);

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
                    string strPath = root + "/" + dvid + "/Handover";
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
                        fdhandover = provider.FormData.GetValues("approved").SingleOrDefault();
                        device_approved_group device_Approved_Group = JsonConvert.DeserializeObject<device_approved_group>(fdhandover);
                        if (device_Approved_Group.is_default == true)
                        {
                            device_Approved_Group.status = true;
                            var das = db.device_approved_group.Where(a => (a.classify == device_Approved_Group.classify && a.organization_id == dvid
                           )).ToList<device_approved_group>();
                            if (das.Count > 0)
                            {
                                foreach (var item in das)
                                {
                                    item.is_default = false;
                                }
                                db.SaveChanges();
                            }
                        }
                        if (device_Approved_Group.is_approved_by_department == true)
                        {
                            device_Approved_Group.approved_type = null;
                        }
                        device_Approved_Group.organization_id =dvid;
                        device_Approved_Group.created_date = DateTime.Now;
                        device_Approved_Group.created_by = uid;
                        device_Approved_Group.created_ip = ip;
                        device_Approved_Group.modified_date = DateTime.Now;
                        device_Approved_Group.modified_by = uid;
                        device_Approved_Group.modified_ip = ip;
                        db.device_approved_group.Add(device_Approved_Group);
                        db.SaveChanges();
                        #region add device_log
                        if (helper.wlog)
                        {
                            device_log log = new device_log();
                            log.title = "Thêm nhóm duyệt " + device_Approved_Group.approved_group_name;
                            log.log_module = "device_approved_group";
                            log.id_key = device_Approved_Group.approved_group_id.ToString();
                            log.created_date = DateTime.Now;
                            log.log_type = 0;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.device_log.Add(log);
                            db.SaveChanges();


                        }
                        #endregion

                        if (device_Approved_Group.is_approved_by_department == true)
                        {

                            var fdhandover_d_groups = provider.FormData.GetValues("approvedgroups").SingleOrDefault();
                            List<device_approved_department_group> list_adg = JsonConvert.DeserializeObject<List<device_approved_department_group>>(fdhandover_d_groups);
                            if (list_adg.Count > 0)
                            {

                                foreach (var item in list_adg)
                                {
                                    item.organization_id = dvid;
                                    item.approved_group_id = device_Approved_Group.approved_group_id;
                               
                                    item.created_date = DateTime.Now;
                                    item.created_by = uid;
                                    item.created_ip = ip;
                                    item.modified_date = DateTime.Now;
                                    item.modified_by = uid;
                                    item.modified_ip = ip;
                                    db.device_approved_department_group.Add(item);
                                    db.SaveChanges();
                                }

                            }
                        }
                        else
                        {
                            var fdhandover_d_users = provider.FormData.GetValues("approvedusers").SingleOrDefault();
                            List<device_approved_department_user> list_du = JsonConvert.DeserializeObject<List<device_approved_department_user>>(fdhandover_d_users);
                            if (list_du.Count > 0)
                            {
                                var i = 1;
                                foreach (var item in list_du)
                                {
                                    item.organization_id = dvid;
                                    item.approved_group_id = device_Approved_Group.approved_group_id;
                                    item.created_date = DateTime.Now;
                                    item.created_by = uid;
                                    item.is_order = i;
                                    i++;
                                    item.created_ip = ip;
                                    item.modified_date = DateTime.Now;
                                    item.modified_by = uid;
                                    item.modified_ip = ip;
                                    db.device_approved_department_user.Add(item);
                                    db.SaveChanges();
                                }

                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhandover, contents }), domainurl + "device_approved_group/add_device_approved_group", ip, tid, "Lỗi khi thêm tệp tin đi kèm biên bản bàn giao", 0, "device_approved_group");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhandover, contents }), domainurl + "device_approved_group/add_device_approved_group", ip, tid, "Lỗi khi thêm tệp tin đi kèm biên bản bàn giao", 0, "device_approved_group");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_device_approved_group()
        {
             var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdhandover = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            int dvid =int.Parse( claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value);
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
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
                    string strPath = root + "/" + dvid + "/Handover";
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
                        fdhandover = provider.FormData.GetValues("approved").SingleOrDefault();
                        device_approved_group handover = JsonConvert.DeserializeObject<device_approved_group>(fdhandover);
                      
                        handover.modified_date = DateTime.Now;
                        handover.modified_by = uid;
                        handover.modified_ip = ip;
                        var device_handover_attachOld = db.device_approved_department_group.Where(s => s.approved_group_id == handover.approved_group_id).ToArray<device_approved_department_group>();
                        var device_Approved_Users = db.device_approved_department_user.Where(s => s.approved_group_id == handover.approved_group_id).ToArray<device_approved_department_user>();

                        if (handover.is_default == true) {
                            handover.status = true;
                        var das = db.device_approved_group.Where(a => (a.approved_group_id != handover.approved_group_id && a.classify == handover.classify && a.organization_id == dvid
                       )).ToList<device_approved_group>();
                        if (das.Count > 0)
                        {
                            foreach (var item in das)
                            {
                                item.is_default = false;
                            }
                                db.SaveChanges();
                        }
                        }
                        var fdhandover_d_groups = provider.FormData.GetValues("approvedgroups").SingleOrDefault();
                        List<device_approved_department_group> list_adg = JsonConvert.DeserializeObject<List<device_approved_department_group>>(fdhandover_d_groups);

                        var fdhandover_d_users = provider.FormData.GetValues("approvedusers").SingleOrDefault();
                        List<device_approved_department_user> list_du = JsonConvert.DeserializeObject<List<device_approved_department_user>>(fdhandover_d_users);

                        if (device_handover_attachOld.Length > 0)
                        {
                            db.device_approved_department_group.RemoveRange(device_handover_attachOld);
                            db.SaveChanges();
                        }
                        if (device_Approved_Users.Length > 0)
                        {
                            db.device_approved_department_user.RemoveRange(device_Approved_Users);
                            db.SaveChanges();
                        }
                        if (handover.is_approved_by_department == true)
                        {

                        if (list_adg.Count > 0)
                            {

                                foreach (var item in list_adg)
                                {
                                    item.organization_id = dvid;
                                    item.approved_group_id = handover.approved_group_id;
                                    item.created_date = DateTime.Now;
                                    item.created_by = uid;
                                    item.created_ip = ip;
                                    item.modified_date = DateTime.Now;
                                    item.modified_by = uid;
                                    item.modified_ip = ip;
                                    db.device_approved_department_group.Add(item);
                                    db.SaveChanges();
                                }

                            }
                        }
                        else
                        {
                        if (list_du.Count > 0)
                            {
                                var i = 1;

                                foreach (var item in list_du)
                                {
                                    item.organization_id = dvid;
                                    item.approved_group_id = handover.approved_group_id;
                                    item.created_date = DateTime.Now;
                                    item.is_order = i;
                                    i++;
                                    item.created_by = uid;
                                    item.created_ip = ip;
                                    item.modified_date = DateTime.Now;
                                    item.modified_by = uid;
                                    item.modified_ip = ip;
                                    db.device_approved_department_user.Add(item);
                                    db.SaveChanges();
                                }

                            }

                        }

                        db.Entry(handover).State = EntityState.Modified;
                        db.SaveChanges();
 
                        #region add device_log
                        if (helper.wlog)
                        {

                            device_log log = new device_log();
                            log.title = "Sửa nhóm duyệt " + handover.approved_group_name;

                            log.log_module = "device_approved_group";
                            log.id_key = handover.approved_group_id.ToString();
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
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhandover, contents }), domainurl + "device_approved_group/update_device_approved_group", ip, tid, "Lỗi khi cập nhật tệp tin đi kèm biên bản bàn giao", 0, "device_approved_group");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhandover, contents }), domainurl + "device_approved_group/update_device_approved_group", ip, tid, "Lỗi khi cập nhật tệp tin đi kèm biên bản bàn giao", 0, "device_approved_group");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> delete_device_approved_group([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.device_approved_group.Where(a => id.Contains(a.approved_group_id)).ToListAsync();
                        var das1 = await db.device_approved_department_group.Where(a => id.Contains(a.approved_group_id)).ToListAsync();
                        var das2 = await db.device_approved_department_user.Where(a => id.Contains(a.approved_group_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das.Count > 0)
                        {
                            List<device_approved_group> del = new List<device_approved_group>();

                            foreach (var da in das)
                            {
                                if (da.is_default == true)
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không được xóa nhóm duyệt mặc định." });
                                }
                                if (uid == da.created_by || (int.Parse(dvid) == da.organization_id && ad) || super)
                                {

                                    del.Add(da);
                                    #region add device_log
                                    if (helper.wlog)
                                    {

                                        device_log log = new device_log();
                                        log.title = "Xóa nhóm duyệt " + da.approved_group_id;
                                        log.log_module = "device_approved_group";
                                        log.log_type = 2;
                                        log.id_key = da.approved_group_id.ToString();
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
                            db.device_approved_group.RemoveRange(del);
                        }
                        if (das1.Count > 0)
                        {
                            List<device_approved_department_group> del1 = new List<device_approved_department_group>();

                            foreach (var da in das1)
                            {

                                del1.Add(da);
                            }
                            db.device_approved_department_group.RemoveRange(del1);
                        }
                        if (das2.Count > 0)
                        {
                            List<device_approved_department_user> del2 = new List<device_approved_department_user>();

                            foreach (var da in das2)
                            {
                                del2.Add(da);
                            }

                            db.device_approved_department_user.RemoveRange(del2);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strP in paths)
                        {
                            bool exists = File.Exists(strP);
                            if (exists)
                                System.IO.File.Delete(strP);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_approved_group/delete_device_approved_group", ip, tid, "Lỗi khi xoá nhóm duyệt", 0, "device_approved_group");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_approved_group/delete_device_approved_group", ip, tid, "Lỗi khi xoá nhóm duyệt", 0, "device_approved_group");
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
        public async Task<HttpResponseMessage> update_s_approved_group([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                        var das = db.device_approved_group.Where(a => (a.approved_group_id == int_id

                        )).FirstOrDefault<device_approved_group>();
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
                                log.title = "Sửa nhóm duyệt " + das.approved_group_name;

                                log.log_module = "device_approved_group";
                                log.id_key = das.approved_group_id.ToString();
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_groups/update_s_device_groups", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_groups", 0, "device_groups");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_groups/update_s_device_groups", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_groups", 0, "device_groups");
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
        public async Task<HttpResponseMessage> update_d_approved_group([System.Web.Mvc.Bind(Include = "IntID")] Trangthai trangthai)
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
                int dvid =int.Parse( claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value);
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var int_id = int.Parse(trangthai.IntID.ToString());
                        var was = db.device_approved_group.Where(a => (a.approved_group_id == int_id
                     )).FirstOrDefault<device_approved_group>();
                        var das = db.device_approved_group.Where(a => ( a.module == was.module && a.organization_id == dvid
                        )).ToList<device_approved_group>();
                        if (das.Count>0)
                        {
                            foreach (var item in das)
                            {
                                item.is_default = false;
                            }
                        }
                        if (was != null)
                        {
                            was.modified_by = uid;
                            was.modified_date = DateTime.Now;
                            was.modified_ip = ip;
                            was.modified_token_id = tid;
                            was.is_default = true;
                            was.status = true;

                            #region add device_log
                            if (helper.wlog)
                            {
                                device_log log = new device_log();
                                log.title = "Sửa nhóm duyệt mặc định " + was.approved_group_name;

                                log.log_module = "device_approved_group";
                                log.id_key = was.approved_group_id.ToString();
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_groups/update_s_device_groups", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_groups", 0, "device_groups");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_groups/update_s_device_groups", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_groups", 0, "device_groups");
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