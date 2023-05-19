using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data;
using System.Data.Entity.Validation;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Models;
using Helper;
using API.Helper;
using Newtonsoft.Json;
using System.Data.Entity;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class task_follow_stepController : ApiController
    {
        string module_key = "M4";
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> addStep()
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
                        temp1 = provider.FormData.GetValues("task_step").SingleOrDefault();
                        task_follow_step task_Follow = JsonConvert.DeserializeObject<task_follow_step>(temp1);
                        task_Follow.follow_step_id= helper.GenKey();
                        task_Follow.created_by = uid;
                        task_Follow.created_date = DateTime.Now;
                        task_Follow.created_ip = ip;
                        task_Follow.created_token_id = tid;
                        string ssid = task_Follow.is_template == true?task_Follow.task_id:null;
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
                                helper.saveNotify(uid, l, null, "Công việc", "Thêm bước trong quy trình công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                    null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                            }
                        }
                        //Logs
                        if (helper.wlog)
                        {

                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = ssid;
                            log.project_id = null;
                            log.description = "Thêm quy trình công việc";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();
                        }
                        db.task_follow_step.Add(task_Follow);
                        temp2 = provider.FormData.GetValues("task_follow_task").SingleOrDefault();
                        List<task_follow_task> task_Follow_Task = JsonConvert.DeserializeObject<List<task_follow_task>>(temp2);
                        List<task_follow_task> task_ADd= new List<task_follow_task>();
                        int i = 1;
                        foreach(var model in task_Follow_Task)
                        {
                            model.follow_task_id = helper.GenKey();
                            model.follow_step_id = task_Follow.follow_step_id;
                            model.task_id = ssid;
                            model.step = i;
                            i++;
                            model.created_by= uid;
                            model.created_token_id = tid;
                            model.created_ip = ip;
                            model.created_date= DateTime.Now;
                            task_ADd.Add(model);
                        }    
                        db.task_follow_task.AddRange(task_ADd);
                     
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow_step/addStep", ip, tid, "Lỗi khi thêm bước vào quy trình", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow_step/addStep", ip, tid, "Lỗi khi thêm bước vào quy trình", 0, "Công việc");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> UpdateStep()
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
                        temp1 = provider.FormData.GetValues("task_step").SingleOrDefault();
                        task_follow_step task_Follow = JsonConvert.DeserializeObject<task_follow_step>(temp1);
                        //task_Follow.follow_id = helper.GenKey();
                        task_Follow.modified_by = uid;
                        task_Follow.modified_date = DateTime.Now;
                        task_Follow.modified_ip = ip;
                            string ssid = task_Follow.task_id;
                        task_Follow.modified_token_id = tid;
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
                                helper.saveNotify(uid, l, null, "Công việc", "Cập nhật bước trong quy trình công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
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
                        }
                        var listDel = db.task_follow_task.Where(x => x.follow_step_id == task_Follow.follow_step_id).ToList();
                        if(listDel.Count > 0)
                        {
                            db.task_follow_task.RemoveRange(listDel);
                        }
                        db.Entry(task_Follow).State = EntityState.Modified;
                        temp2 = provider.FormData.GetValues("task_follow_task").SingleOrDefault();
                        List<task_follow_task> task_Follow_Task = JsonConvert.DeserializeObject<List<task_follow_task>>(temp2);
                        List<task_follow_task> task_ADd = new List<task_follow_task>();
                        int i = 1;
                        foreach (var model in task_Follow_Task)
                        {
                            model.follow_task_id = helper.GenKey();
                            model.follow_step_id = task_Follow.follow_step_id;
                            model.task_id = ssid;
                            model.step = i;
                            i++;
                            model.created_by = uid;
                            model.created_token_id = tid;
                            model.created_ip = ip;
                            model.created_date = DateTime.Now;
                            task_ADd.Add(model);
                        }
                        db.task_follow_task.AddRange(task_ADd);

                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow_step/UpdateStep", ip, tid, "Lỗi khi cập nhật bước trong  quy trình", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow_step/UpdateStep", ip, tid, "Lỗi khi cập nhật bước trong quy trình", 0, "Công việc");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteStep([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                    List<task_follow_step> del = new List<task_follow_step>();
                    var das = await db.task_follow_step.Where(a => id.Contains(a.follow_step_id)).ToListAsync();
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
                                log.description = "xóa bước trong quy trình công việc";
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
                        if(das[0].is_template!=true) {
                        string ssid = das[0].task_id;

                        var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);

                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Xóa quy trình công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                        } }
                    }
                    if (del.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                    }
                    if (del.Count > 0)
                    {
                        db.task_follow_step.RemoveRange(del);
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
        public async Task<HttpResponseMessage> ReOdersFollow()
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
                        List<task_follow_task> task_Follow = JsonConvert.DeserializeObject<List<task_follow_task>>(temp1);
               
                        int index = 1;
                        foreach (var item in task_Follow)
                        {
                            item.step = index;
                            item.modified_by = uid;
                            item.modified_date = DateTime.Now;
                            item.modified_ip = ip;
                            item.modified_token_id = tid;
                            db.Entry(item).State = EntityState.Modified;
                            index++;
                        }
                        //Noti
                        string ssid = task_Follow[0].task_id;
                        var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);

                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Cập nhật thứ tự thực hiện công việc trong quy trình công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                        }
                        //Logs
                        if (helper.wlog)
                        {

                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = ssid;
                            log.project_id = null;
                            log.description = "Cập nhật thứ tự thực hiện công việc quy trình công việc";
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow_step/ReOdersFollow", ip, tid, "Lỗi khi cập nhật thứ tự công việc trong quy trình", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow_step/ReOdersFollow", ip, tid, "Lỗi khi cập nhật thứ tự công việc trong quy trình", 0, "Công việc");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_Status_Step()
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
                        task_follow_step task_Follow = JsonConvert.DeserializeObject<task_follow_step>(temp1);
                        var finder = db.task_follow_step.FirstOrDefault(x => x.follow_step_id == task_Follow.follow_step_id);
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
                            helper.saveNotify(uid, l, null, "Công việc", "Cập nhật bước trong quy trình công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow_step/Update_Status_Step", ip, tid, "Lỗi khi cập nhật trạng thái bước trong quy trình", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow_step/Update_Status_Step", ip, tid, "Lỗi khi cập nhật trạng thái bước trong quy trình", 0, "Công việc");
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
