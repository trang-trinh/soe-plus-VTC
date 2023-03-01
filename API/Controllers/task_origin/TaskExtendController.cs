using System;
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
using Helper;
using API.Helper;
using Newtonsoft.Json;

namespace API.Controllers.Task_Origin1
{
    [Authorize(Roles = "login")]
    public class TaskExtendController : ApiController
    {
        string module_key = "M4";
        public string getipaddress()
        {

            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add_TaskExtend([System.Web.Mvc.Bind(Include = "extend_new_date,task_id,extend_id,is_agree,created_by,created_date,created_ip_created_token_id")] task_extend task_Extend)
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
                    task_Extend.extend_id = helper.GenKey();
                    task_Extend.created_by = uid;
                    task_Extend.created_date = DateTime.Now;
                    task_Extend.created_ip = ip;
                    task_Extend.created_token_id = tid;
                    db.task_extend.Add(task_Extend);
                    if (task_Extend.is_agree == true)
                    {
                        var task_origin = db.task_origin.Where(x => x.task_id == task_Extend.task_id).FirstOrDefault();
                        task_origin.end_date = task_Extend.extend_new_date;
                        db.Entry(task_origin).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    await db.SaveChangesAsync();
                    var listuser = db.task_member.Where(x => x.task_id == task_Extend.task_id).Select(x => x.user_id).Distinct().ToList();
                    string task_name = db.task_origin.Where(x => x.task_id == task_Extend.task_id).Select(x => x.task_name).FirstOrDefault().ToString();
                    listuser.Remove(uid);
                    foreach (var l in listuser)
                    {
                        helper.saveNotify(uid, l, null, "Công việc", "Đã xin gia hạn công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                            null, 2, -1, false, module_key, task_Extend.task_id, null, null, tid, ip);
                    }
                    #region add cms_logs
                    if (helper.wlog)
                    {
                        task_logs log = new task_logs();
                        log.log_id = helper.GenKey();
                        log.task_id = task_Extend.task_id;
                        log.project_id = null;
                        log.description = "Thêm mới gia hạn công việc";
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

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_Extend/Add_task_Extend", ip, tid, "Lỗi khi thêm mới gia hạn công việc", 0, "task_Extend");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_Extend/Add_task_Extend", ip, tid, "Lỗi khi thêm mới gia hạn công việc", 0, "task_Extend");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Upgrade_Status_TaskExtend([System.Web.Mvc.Bind(Include = "task_id,accept_user,accept_date,extend_new_date")] task_extend task_Extend)
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
                    task_Extend.accept_date = DateTime.Now;
                    task_Extend.accept_user = uid;
                    db.Entry(task_Extend).State = EntityState.Modified;
                    if (task_Extend.is_agree == true)
                    {
                        var task_origin = db.task_origin.Where(x => x.task_id == task_Extend.task_id).FirstOrDefault();
                        task_origin.end_date = task_Extend.extend_new_date;
                        db.Entry(task_origin).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    await db.SaveChangesAsync();
                    var listuser = db.task_member.Where(x => x.task_id == task_Extend.task_id).Select(x => x.user_id).Distinct().ToList();
                    string task_name = db.task_origin.Where(x => x.task_id == task_Extend.task_id).Select(x => x.task_name).FirstOrDefault().ToString();
                    listuser.Remove(uid);

                    foreach (var l in listuser)
                    {
                        helper.saveNotify(uid, l, null, "Công việc", "Đã duyệt gia hạn công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                            null, 2, -1, false, module_key, task_Extend.task_id, null, null, tid, ip);
                    }

                    #region add cms_logs
                    if (helper.wlog)
                    {


                        task_logs log = new task_logs();
                        log.log_id = helper.GenKey();
                        log.task_id = task_Extend.task_id;
                        log.project_id = null;
                        log.description = "Duyệt gia hạn công việc ";
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

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_Extend/Upgrade_Status_TaskExtend", ip, tid, "Lỗi khi duyệt gia hạn công việc", 0, "task_Extend");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "task_Extend/Upgrade_Status_TaskExtend", ip, tid, "Lỗi khi duyệt gia hạn công việc", 0, "task_Extend");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> deleteTaskExtendTaskExtend([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                    List<task_extend> del = new List<task_extend>();
           
                        var das = await db.task_extend.Where(a => id.Contains(a.extend_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            foreach (var da in das)
                            {


                                del.Add(da);
                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    task_logs log = new task_logs();
                                    log.log_id = helper.GenKey();
                                    log.task_id = da.task_id;
                                    log.project_id = null;
                                    log.description = "xóa gia hạn công việc";
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.task_logs.Add(log);
                                    db.SaveChanges();
                                }
                                #endregion
                            }
                            string ssid = das[0].task_id;

                            var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                            string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                            listuser.Remove(uid);

                            foreach (var l in listuser)
                            {
                                helper.saveNotify(uid, l, null, "Công việc", "Xóa gia hạn công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                    null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                            }
                        }
                
                    if (del.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                    }
                    if (del.Count > 0)
                        db.task_extend.RemoveRange(del);
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Task_extend/deleteTaskExtend", ip, tid, "Lỗi khi xoá gia hạn công việc", 0, "gia hạn công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Task_extend/deleteTaskExtend", ip, tid, "Lỗi khi xoá gia hạn công việc", 0, "gia hạn công việc");
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
