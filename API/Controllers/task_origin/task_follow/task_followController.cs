using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Models;
using Helper;
using API.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Microsoft.Ajax.Utilities;
using System.CodeDom;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class task_followController : ApiController
    {
        string module_key = "M4";
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> addFollows()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string temp1 = "";
            string temp2 = "";
            try
            {
                using (DBEntities db = new DBEntities())
                {

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Task_Files";
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
                        temp1 = provider.FormData.GetValues("task_follow").SingleOrDefault();
                        task_follow task_Follow = JsonConvert.DeserializeObject<task_follow>(temp1);
                        task_Follow.follow_id = helper.GenKey();
                        task_Follow.created_by = uid;
                        task_Follow.created_date = DateTime.Now;
                        task_Follow.created_ip = ip;
                        task_Follow.created_token_id = tid;
                        string ssid = task_Follow.task_id;
                        if (task_Follow.is_template != true)
                        {
                            if (task_Follow.start_date <= DateTime.Now && task_Follow.status == 0)
                            {
                                task_Follow.status = 1;
                                task_Follow.start_real_date = DateTime.Now;
                            }



                            //Noti

                            var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                            string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                            listuser.Remove(uid);

                            foreach (var l in listuser)
                            {
                                helper.saveNotify(uid, l, null, "Công việc", "Thêm quy trình công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                    null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                            }
                        }
                        //Logs
                        if (helper.wlog)
                        {

                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = task_Follow.is_template != true ? ssid : null;
                            log.project_id = null;
                            log.description = task_Follow.is_template != true ? "Thêm quy trình công việc" : "Thêm mẫu quy trình công việc";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();
                        }
                        db.task_follow.Add(task_Follow);
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow/addFollows", ip, tid, "Lỗi khi thêm quy trình", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow/addFollows", ip, tid, "Lỗi khi thêm quy trình", 0, "Công việc");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateFollow()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string temp1 = "";
            string temp2 = "";
            try
            {
                using (DBEntities db = new DBEntities())
                {

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Task_Files";
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
                        temp1 = provider.FormData.GetValues("task_follow").SingleOrDefault();
                        task_follow task_Follow = JsonConvert.DeserializeObject<task_follow>(temp1);
                        //task_Follow.follow_id = helper.GenKey();
                        task_Follow.modified_by = uid;
                        task_Follow.modified_date = DateTime.Now;
                        task_Follow.modified_ip = ip;
                        task_Follow.modified_token_id = tid; string ssid = task_Follow.task_id;
                        if (task_Follow.is_template != true)
                        {
                            if (task_Follow.start_date <= DateTime.Now && task_Follow.status == 0)
                            {
                                task_Follow.start_real_date = DateTime.Now;
                                task_Follow.status = 1;
                            }


                            //Noti

                            var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                            string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                            listuser.Remove(uid);

                            foreach (var l in listuser)
                            {
                                helper.saveNotify(uid, l, null, "Công việc", "Cập nhật quy trình công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                    null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                            }
                            //Logs
                        }
                        db.Entry(task_Follow).State = EntityState.Modified;
                        if (helper.wlog)
                        {

                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = ssid;
                            log.project_id = null;
                            log.description = "Cập nhật quy trình công việc";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow/UpdateFollow", ip, tid, "Lỗi khi cập nhật quy trình", 0, "task_follow");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow/UpdateFollow", ip, tid, "Lỗi khi cập nhật quy trình", 0, "task_follow");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteFollow([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                    List<task_follow> del = new List<task_follow>();
                    var das = await db.task_follow.Where(a => id.Contains(a.follow_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        foreach (var da in das)
                        {
                            del.Add(da);
                            #region add logs
                            if (helper.wlog)
                            {

                                task_logs log = new task_logs();
                                log.log_id = helper.GenKey();
                                log.task_id = da.task_id;
                                log.project_id = null;
                                log.description = "xóa quy trình công việc";
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.task_logs.Add(log);
                                db.SaveChanges();
                            }
                            #endregion
                        }
                    }
                    if (das.Count > 0)
                    {
                        string ssid = das[0].task_id;

                        var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);

                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Xóa quy trình công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                        }
                    }
                    if (del.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                    }
                    if (del.Count > 0)
                    {
                        db.task_follow.RemoveRange(del);
                        db.SaveChanges();
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_follow/deletetask_follow", ip, tid, "Lỗi khi xoá quy trình", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "task_follow/deletetask_follow", ip, tid, "Lỗi khi xoá quy trình", 0, "Công việc");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Status_Follow()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                string temp1 = "";
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Task_Files";
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
                        temp1 = provider.FormData.GetValues("model").SingleOrDefault();
                        task_follow task_Follow = JsonConvert.DeserializeObject<task_follow>(temp1);
                        var finder = db.task_follow.FirstOrDefault(x => x.follow_id == task_Follow.follow_id);
                        finder.modified_by = uid;
                        finder.modified_date = DateTime.Now;
                        finder.modified_ip = ip;
                        finder.modified_token_id = tid;
                        finder.status = task_Follow.status;
                        if (finder.status == 0)
                        {
                            finder.start_real_date = null;
                            finder.end_real_date = null;
                        }
                        if (finder.status == 1)
                        {
                            finder.start_real_date = DateTime.Now;
                        }
                        if (finder.status == 2)
                        {
                            finder.end_real_date = finder.end_date != null ? finder.end_date : DateTime.Now;
                        }
                        if (finder.status == 3)
                        {
                            finder.end_real_date = DateTime.Now;
                        }
                        db.Entry(finder).State = EntityState.Modified;

                        //Noti
                        string ssid = finder.task_id;
                        var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);

                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Cập nhật quy trình công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                        }
                        //Logs
                        if (helper.wlog)
                        {

                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = ssid;
                            log.project_id = null;
                            log.description = "Cập nhật quy trình công việc";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow/Update_Status_Follow", ip, tid, "Lỗi khi cập nhật trạng thái quy trình", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow/Update_Status_Follow", ip, tid, "Lỗi khi cập nhật trạng thái quy trình", 0, "Công việc");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> addFollowToTask()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string temp1 = "";
            string temp2 = "";
            string temp3 = "";
            try
            {
                using (DBEntities db = new DBEntities())
                {

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Task_Files";
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
                        temp1 = provider.FormData.GetValues("follow_id").SingleOrDefault();
                        temp2 = provider.FormData.GetValues("task_id").SingleOrDefault();
                        temp3 = provider.FormData.GetValues("start_date").SingleOrDefault();
                        string follow_id = JsonConvert.DeserializeObject<string>(temp1);
                        string task_id = JsonConvert.DeserializeObject<string>(temp2);

                        string start_date = JsonConvert.DeserializeObject<string>(temp3) ?? null;
                        var task_Follow_Template = db.task_follow.AsNoTracking().FirstOrDefault(x => x.follow_id == follow_id);
                        if (task_Follow_Template != null)
                        {
                            task_follow new_Follow = new task_follow();
                            new_Follow = task_Follow_Template;
                            new_Follow.follow_id = helper.GenKey();
                            new_Follow.task_id = task_id;
                            new_Follow.status = 0;
                            new_Follow.created_by = uid;
                            new_Follow.created_date = DateTime.Now;
                            new_Follow.created_ip = ip;
                            new_Follow.created_token_id = tid;
                            new_Follow.modified_by = null;
                            new_Follow.modified_date = null;
                            new_Follow.modified_ip = null;
                            new_Follow.modified_token_id = null;
                            new_Follow.is_template = false;
                            new_Follow.organization_id = null;
                            new_Follow.process_time = null;
                            if (task_Follow_Template.process_time > 0)
                            {
                                new_Follow.start_date = start_date != null && start_date != "" ? Convert.ToDateTime(start_date) : DateTime.Now;
                                new_Follow.end_date = DateTime.Now.AddMinutes((double)task_Follow_Template.process_time);
                                if (new_Follow.start_date <= DateTime.Now && new_Follow.status == 0)
                                {
                                    new_Follow.start_real_date = DateTime.Now;
                                    new_Follow.status = 1;
                                }
                                else
                                {
                                    new_Follow.start_real_date = null;
                                }

                            }
                            else
                            {
                                new_Follow.start_date = null;
                                new_Follow.end_date = null;
                            }
                            db.task_follow.Add(new_Follow);
                            db.SaveChanges();

                            var follow_steps = db.task_follow_step.AsNoTracking().Where(x => x.follow_id == follow_id).ToList();
                            if (follow_steps.Count > 0)
                            {
                                List<task_follow_step> list_steps = new List<task_follow_step>();
                                foreach (var item in follow_steps)
                                {
                                    string stepID = item.follow_step_id;
                                    task_follow_step new_step = new task_follow_step();
                                    new_step = item;
                                    new_step.follow_step_id = helper.GenKey();
                                    new_step.follow_id = new_Follow.follow_id;
                                    new_step.task_id = task_id;
                                    new_step.status = 0;
                                    new_step.created_by = uid;
                                    new_step.created_date = DateTime.Now;
                                    new_step.created_ip = ip;
                                    new_step.created_token_id = tid;
                                    new_step.modified_by = null;
                                    new_step.modified_date = null;
                                    new_step.modified_ip = null;
                                    new_step.modified_token_id = null;
                                    new_step.is_template = false;
                                    new_step.time_process = null;
                                    if (new_step.time_process > 0)
                                    {
                                        new_step.start_date = start_date != null && start_date != "" ? Convert.ToDateTime(start_date) : DateTime.Now;
                                        new_step.end_date = DateTime.Now.AddMinutes((double)new_step.time_process);
                                        if (new_step.start_date <= DateTime.Now && item.status == 0)
                                        {
                                            new_step.start_real_date = DateTime.Now;
                                            new_step.status = 1;
                                        }
                                        else
                                        {
                                            new_step.start_real_date = null;
                                        }
                                    }
                                    else
                                    {
                                        new_step.start_date = null;
                                        new_step.end_date = null;
                                    }
                                    db.task_follow_step.Add(new_step);
                                    //
                                    var list_task_follow_task = db.task_follow_task.AsNoTracking().Where(x => x.follow_step_id == stepID).ToList();
                                    if (list_task_follow_task.Count > 0)
                                    {
                                        List<task_follow_task> task_Follow_Tasks = new List<task_follow_task>();
                                        foreach (var follow_task in list_task_follow_task)
                                        {
                                            var task_orgin1 = db.task_origin.AsNoTracking().FirstOrDefault(x => x.task_id == follow_task.task_id_follow);
                                            if (task_orgin1 != null)
                                            {
                                                task_orgin1.task_id = helper.GenKey();
                                                task_orgin1.parent_id = task_id;
                                                if (task_orgin1.process_time > 0)
                                                {
                                                    task_orgin1.start_date = start_date != null && start_date != "" ? Convert.ToDateTime(start_date) : DateTime.Now;
                                                    task_orgin1.end_date = DateTime.Now.AddMinutes((double)task_orgin1.process_time);
                                                    if (task_orgin1.start_date <= DateTime.Now && item.status == 0)
                                                    {
                                                        task_orgin1.start_real_date = DateTime.Now;
                                                        task_orgin1.status = 1;
                                                    }
                                                    else
                                                    {
                                                        task_orgin1.start_real_date = null;
                                                    }
                                                }
                                                else
                                                {
                                                    task_orgin1.start_date = null;
                                                    task_orgin1.end_date = null;
                                                }
                                                task_orgin1.created_by = uid;
                                                task_orgin1.created_date = DateTime.Now;
                                                task_orgin1.created_ip = ip;
                                                task_orgin1.created_token_id = tid;
                                                task_orgin1.modified_by = null;
                                                task_orgin1.modified_date = null;
                                                task_orgin1.modified_ip = null;
                                                task_orgin1.modified_token_id = null;
                                                task_orgin1.is_template = false;
                                                task_orgin1.process_time = null;
                                                db.task_origin.Add(task_orgin1);
                                                //
                                                var members = db.task_member.AsNoTracking().Where(x => x.task_id == follow_task.task_id_follow).ToList();
                                                if (members.Count > 0)
                                                {
                                                    foreach (var mem in members)
                                                    {
                                                        mem.member_id = helper.GenKey();
                                                        mem.task_id = task_orgin1.task_id;
                                                        mem.created_by = uid;
                                                        mem.created_date = DateTime.Now;
                                                        mem.created_ip = ip;
                                                        mem.created_token_id = tid;
                                                        mem.modified_by = null;
                                                        mem.modified_date = null;
                                                        mem.modified_ip = null;
                                                        mem.modified_token_id = null;
                                                    }
                                                    db.task_member.AddRange(members);
                                                }
                                                follow_task.follow_task_id = helper.GenKey();
                                                follow_task.follow_step_id = item.follow_step_id;
                                                follow_task.task_id = task_id;
                                                follow_task.task_id_follow = task_orgin1.task_id;
                                                follow_task.created_by = uid;
                                                follow_task.created_date = DateTime.Now;
                                                follow_task.created_ip = ip;
                                                follow_task.created_token_id = tid;
                                                follow_task.modified_by = null;
                                                follow_task.modified_date = null;
                                                follow_task.modified_ip = null;
                                                follow_task.modified_token_id = null;
                                                db.task_follow_task.Add(follow_task);
                                            }
                                        }
                                        db.task_follow_task.AddRange(list_task_follow_task);
                                    }

                                }

                                db.SaveChanges();
                            }

                            if (helper.wlog)
                            {
                                task_logs log = new task_logs();
                                log.log_id = helper.GenKey();
                                log.task_id = task_id;
                                log.project_id = null;
                                log.description = "Thêm quy trình công việc";
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.task_logs.Add(log);
                                db.SaveChanges();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow/addFollows", ip, tid, "Lỗi khi thêm quy trình", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow/addFollows", ip, tid, "Lỗi khi thêm quy trình", 0, "Công việc");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
    }
}
