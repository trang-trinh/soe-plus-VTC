﻿using System;
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
                        if (task_Follow.start_date <= DateTime.Now && task_Follow.status==0)
                        {
                            task_Follow.start_real_date = DateTime.Now;
                            task_Follow.status = 1;
                        }

                        db.task_follow_step.Add(task_Follow);

                        //Noti
                        string ssid = task_Follow.task_id;
                        var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);

                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Thêm bước trong quy trình công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, ssid, null, null, tid, ip);
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
                        task_Follow.modified_token_id = tid;

                        if (task_Follow.start_date <= DateTime.Now && task_Follow.status == 0)
                        {
                            task_Follow.start_real_date = DateTime.Now;
                            task_Follow.status = 1;
                        }
                        db.Entry(task_Follow).State = EntityState.Modified;

                        //Noti
                        string ssid = task_Follow.task_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow_step/UpdateStep", ip, tid, "Lỗi khi cập nhật bước trong  quy trình", 0, "task_follow");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_follow_step/UpdateStep", ip, tid, "Lỗi khi cập nhật bước trong quy trình", 0, "task_follow");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
    }
}