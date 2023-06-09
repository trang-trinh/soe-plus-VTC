﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Models;
using API.Helper;
using Helper;
using Newtonsoft.Json;
namespace API.Controllers.Task_Origin1
{
    [Authorize(Roles = "login")]
    public class task_MemberController : ApiController
    {
        string module_key = "M4";
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add_task_Member([System.Web.Mvc.Bind(Include = "task_id,member_id,created_by,created_date,created_ip_created_token_id")] task_member task_Member)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var obj_task = db.task_origin.FirstOrDefault(x => x.task_id == task_Member.task_id);
                    if (obj_task != null)
                    {
                        task_Member.member_id = helper.GenKey();
                        task_Member.created_by = uid;
                        task_Member.created_date = DateTime.Now;
                        task_Member.created_ip = ip;
                        task_Member.created_token_id = tid;
                        db.task_member.Add(task_Member);
                        await db.SaveChangesAsync();
                        // notify
                        //var listuser = db.task_member.Where(x => x.task_id == task_Member.task_id).Select(x => x.user_id).Distinct().ToList();
                        //string task_name = db.task_origin.Where(x => x.task_id == task_Member.task_id).Select(x => x.task_name).FirstOrDefault().ToString();
                        //listuser.Remove(uid);
                        //foreach (var l in listuser)
                        //{
                        //    helper.saveNotify(uid, l, null, "Công việc", "Đã thêm thành viên tham gia vào công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                        //        null, 2, -1, false, module_key, task_Member.task_id, null, null, tid, ip);
                        //}

                        List<string> list_member_task = new List<string>();
                        var listuser = db.task_member.Where(x => x.task_id == obj_task.task_id && x.user_id != uid);
                        foreach (var mem in listuser)
                        {
                            if (!list_member_task.Contains(mem.user_id))
                            {
                                list_member_task.Add(mem.user_id);

                                helper.saveNotify(uid, mem.user_id, null, "Công việc",
                                "Đã thêm thành viên tham gia vào công việc: " + (obj_task.task_name.Length > 100 ? obj_task.task_name.Substring(0, 97) + "..." : obj_task.task_name),
                                null, 2, -1, false, module_key, obj_task.task_id, null, null, tid, ip);
                            }
                        }
                        #region add cms_logs
                        if (helper.wlog)
                        {
                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = task_Member.task_id;
                            log.project_id = null;
                            log.description = "Thêm mới thành viên ";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();

                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Công việc không tồn tại." });
                    }
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_Member/Add_task_Member", ip, tid, "Lỗi khi thêm mới thành viên", 0, "task_Member");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_Member/Add_task_Member", ip, tid, "Lỗi khi thêm mới thành viên", 0, "task_Member");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_task_Member([System.Web.Mvc.Bind(Include = "")] List<string> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;

            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var member = db.task_member.Where(x => ids.Contains(x.member_id)).ToList();
                    db.task_member.RemoveRange(member);
                    string userid = member[0].user_id;
                    var task = member[0];
                    var memberi4 = db.sys_users.Where(x => x.user_id == userid).FirstOrDefault();
                    if (helper.wlog)
                    {
                        task_logs log = new task_logs();
                        log.log_id = helper.GenKey();
                        log.task_id = member[0].task_id;
                        log.project_id = null;
                        log.description = "xóa thành viên " + memberi4.full_name;
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.task_logs.Add(log);
                        db.SaveChanges();
                    }

                    await db.SaveChangesAsync();
                    //notify
                    var listuser = db.task_member.Where(x => x.task_id == task.task_id).Select(x => x.user_id).Distinct().ToList();
                    string task_name = db.task_origin.Where(x => x.task_id == task.task_id).Select(x => x.task_name).FirstOrDefault().ToString();
                    listuser.Remove(uid);
                    foreach (var l in listuser)
                    {
                        helper.saveNotify(uid, l, null, "Công việc", "Đã xóa thành viên tham gia công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                            null, 2, -1, false, module_key, task.task_id, null, null, tid, ip);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "task_Member/Delete_task_Member", ip, tid, "Lỗi khi xóa thành viên", 0, "task_Member");
                if (!helper.debug)
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "task_Member/Delete_task_Member", ip, tid, "Lỗi khi xóa thành viên", 0, "task_Member");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_Task_Member([System.Web.Mvc.Bind(Include = "")] List<string> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    foreach (string id in ids)
                    {
                        var Viewer = db.task_member.Where(x => id == x.member_id).FirstOrDefault();
                        if (Viewer != null)
                        {
                            if (Viewer.is_view == false || Viewer.is_view == null)
                            {
                                Viewer.is_view = true;
                                Viewer.view_date = DateTime.Now; if (helper.wlog)
                                {
                                    task_logs log = new task_logs();
                                    log.log_id = helper.GenKey();
                                    log.task_id = Viewer.task_id;
                                    log.project_id = null;
                                    log.description = "đã xem công việc";
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.task_logs.Add(log);
                                    db.SaveChanges();
                                }
                            }
                            db.Entry(Viewer).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            #region add cms_logs

                        }
                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_Member/Update_View", ip, tid, "Lỗi khi chỉnh sửa trạng thái xem công việc của thành viên", 0, "task_Member");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_Member/Update_View", ip, tid, "Lỗi khi chỉnh sửa trạng thái xem công việc của thành viên", 0, "task_Member");

                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_Member_Info()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

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
                        ///Get data from FrontEnd
                        string tempID = provider.FormData.GetValues("member").SingleOrDefault();
                        List<task_member> listprocess_id = JsonConvert.DeserializeObject<List<task_member>>(tempID);
                        foreach (var item in listprocess_id)
                        {
                            item.modified_by = uid;
                            item.modified_date = DateTime.Now;
                            item.modified_ip = ip; ;
                            item.modified_token_id = tid;
                        db.Entry(item).State = EntityState.Modified;
                       
                        #region add tasklog
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = item }), domainurl + "/task_Member/Update_Member_Info", ip, tid, "Cập nhật thành viên công việc", 1, "Công việc");
                        #endregion
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "/task_Member/Update_Member_Info", ip, tid, "Lỗi khi cập nhật thành viên công việcc", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "/task_Member/Update_Member_Info", ip, tid, "Lỗi khi cập nhật thành viên công việc", 0, "Công việc");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
    }
}
