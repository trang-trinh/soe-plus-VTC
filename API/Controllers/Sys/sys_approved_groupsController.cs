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

namespace API.Controllers.Sys
{
    [Authorize(Roles = "login")]
    public class sys_approved_groupsController : ApiController
    {

        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> add_sys_approved_groups()
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
                        sys_approved_groups device_Approved_Group = JsonConvert.DeserializeObject<sys_approved_groups>(fdhandover);

                        device_Approved_Group.organization_id = dvid;
                        device_Approved_Group.created_date = DateTime.Now;
                        device_Approved_Group.created_by = uid;
                        device_Approved_Group.created_ip = ip;
                        device_Approved_Group.modified_date = DateTime.Now;
                        device_Approved_Group.modified_by = uid;
                        device_Approved_Group.modified_ip = ip;
                        db.sys_approved_groups.Add(device_Approved_Group);
                        db.SaveChanges();
                        //#region add device_log
                        //if (helper.wlog)
                        //{
                        //    device_log log = new device_log();
                        //    log.title = "Thêm nhóm duyệt " + device_Approved_Group.approved_group_name;
                        //    log.log_module = "sys_approved_groups";
                        //    log.id_key = device_Approved_Group.approved_groups_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.log_type = 0;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.device_log.Add(log);
                        //    db.SaveChanges();
                        //}
                        //#endregion
                        var fdhandover_d_groups = provider.FormData.GetValues("approvedusers").SingleOrDefault();
                        List<sys_approved_users> list_adg = JsonConvert.DeserializeObject<List<sys_approved_users>>(fdhandover_d_groups);
                        if (list_adg.Count > 0)
                        {
                            var i = 1;
                            foreach (var item in list_adg)
                            {
                                if (device_Approved_Group.is_department == false)
                                    item.department_id = null;
                                item.is_order = i;
                                item.approved_groups_id = device_Approved_Group.approved_groups_id;
                                item.created_date = DateTime.Now;
                                item.created_by = uid;
                                item.created_ip = ip;
                                item.modified_date = DateTime.Now;
                                item.modified_by = uid;
                                item.modified_ip = ip;
                                db.sys_approved_users.Add(item);
                                db.SaveChanges();
                                i++;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhandover, contents }), domainurl + "sys_approved_groups/add_sys_approved_groups", ip, tid, "Lỗi khi thêm tệp tin đi kèm biên bản bàn giao", 0, "sys_approved_groups");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhandover, contents }), domainurl + "sys_approved_groups/add_sys_approved_groups", ip, tid, "Lỗi khi thêm tệp tin đi kèm biên bản bàn giao", 0, "sys_approved_groups");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_sys_approved_groups()
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
                        sys_approved_groups handover = JsonConvert.DeserializeObject<sys_approved_groups>(fdhandover);

                        handover.modified_date = DateTime.Now;
                        handover.modified_by = uid;
                        handover.modified_ip = ip;
                        var approved_Users = db.sys_approved_users.Where(s => s.approved_groups_id == handover.approved_groups_id).ToArray<sys_approved_users>();



                        var fdhandover_d_users = provider.FormData.GetValues("approvedusers").SingleOrDefault();
                        List<sys_approved_users> list_du = JsonConvert.DeserializeObject<List<sys_approved_users>>(fdhandover_d_users);


                        if (approved_Users.Length > 0)
                        {
                            db.sys_approved_users.RemoveRange(approved_Users);
                            db.SaveChanges();
                        }


                        if (list_du.Count > 0)
                        {
                            var i = 1;
                            foreach (var item in list_du)
                            {
                                if (handover.is_department == false)
                                    item.department_id = null;
                                item.is_order = i;
                                item.approved_groups_id = handover.approved_groups_id;
                                item.created_date = DateTime.Now;
                                item.created_by = uid;
                                item.created_ip = ip;
                                item.modified_date = DateTime.Now;
                                item.modified_by = uid;
                                item.modified_ip = ip;
                                db.sys_approved_users.Add(item);
                                db.SaveChanges();
                                i++;
                            }

                        }


                        db.Entry(handover).State = EntityState.Modified;
                        db.SaveChanges();

                        //#region add device_log
                        //if (helper.wlog)
                        //{

                        //    device_log log = new device_log();
                        //    log.title = "Sửa nhóm duyệt " + handover.approved_group_name;

                        //    log.log_module = "sys_approved_groups";
                        //    log.id_key = handover.approved_groups_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.log_type = 1;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhandover, contents }), domainurl + "sys_approved_groups/update_sys_approved_groups", ip, tid, "Lỗi khi cập nhật tệp tin đi kèm biên bản bàn giao", 0, "sys_approved_groups");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhandover, contents }), domainurl + "sys_approved_groups/update_sys_approved_groups", ip, tid, "Lỗi khi cập nhật tệp tin đi kèm biên bản bàn giao", 0, "sys_approved_groups");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> delete_sys_approved_groups([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.sys_approved_groups.Where(a => id.Contains(a.approved_groups_id)).ToListAsync();

                        List<string> paths = new List<string>();
                        if (das.Count > 0)
                        {
                            List<sys_approved_groups> del = new List<sys_approved_groups>();

                            foreach (var da in das)
                            {

                                if (uid == da.created_by || (int.Parse(dvid) == da.organization_id && ad) || super)
                                {

                                    del.Add(da);
                                    //#region add device_log
                                    //if (helper.wlog)
                                    //{

                                    //    device_log log = new device_log();
                                    //    log.title = "Xóa nhóm duyệt " + da.approved_groups_id;
                                    //    log.log_module = "sys_approved_groups";
                                    //    log.log_type = 2;
                                    //    log.id_key = da.approved_groups_id.ToString();
                                    //    log.created_date = DateTime.Now;
                                    //    log.created_by = uid;
                                    //    log.created_token_id = tid;
                                    //    log.created_ip = ip;
                                    //    db.device_log.Add(log);
                                    //    db.SaveChanges();


                                    //}
                                    //#endregion
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.sys_approved_groups.RemoveRange(del);
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "sys_approved_groups/delete_sys_approved_groups", ip, tid, "Lỗi khi xoá nhóm duyệt", 0, "sys_approved_groups");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "sys_approved_groups/delete_sys_approved_groups", ip, tid, "Lỗi khi xoá nhóm duyệt", 0, "sys_approved_groups");
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
                        var das = db.sys_approved_groups.Where(a => (a.approved_groups_id == int_id

                        )).FirstOrDefault<sys_approved_groups>();
                        if (das != null)
                        {
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            das.status = !trangthai.BitTrangthai;

                            //#region add device_log
                            //if (helper.wlog)
                            //{
                            //    device_log log = new device_log();
                            //    log.title = "Sửa nhóm duyệt " + das.approved_group_name;

                            //    log.log_module = "sys_approved_groups";
                            //    log.id_key = das.approved_groups_id.ToString();
                            //    log.log_type = 1;
                            //    log.created_date = DateTime.Now;
                            //    log.created_by = uid;
                            //    log.created_token_id = tid;
                            //    log.created_ip = ip;
                            //    db.device_log.Add(log);
                            //    db.SaveChanges();


                            //}
                            //#endregion
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