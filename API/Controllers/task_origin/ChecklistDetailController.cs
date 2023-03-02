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
using API.Helper;
using Helper;
using Newtonsoft.Json;
namespace API.Controllers.Task_Ca
{
    [Authorize(Roles = "login")]
    public class ChecklistDetailController : ApiController
    {
        string module_key = "M4";
        public string getipaddress()
        {
            //     var host = Dns.GetHostEntry(Dns.GetHostName());
            //     foreach (var ip in host.AddressList)
            //     {
            //         if (ip.AddressFamily == AddressFamily.InterNetwork)
            //         {
            //             return ip.ToString();
            //         }
            //     }
            //     return "localhost";
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> addTaskCheckList([System.Web.Mvc.Bind(Include = "start_date,is_deadline,task_id,task_name,task_name_en,is_check,close_by,close_date,end_real_date,end_date,created_date,created_by,created_ip,created_token_id")] task_origin task)
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
                    task.task_id = helper.GenKey();
                    task.task_name_en = helper.convertToUnSign3(task.task_name);
                    if (task.is_check == true && task.end_date != null)
                    {
                        task.close_by = uid;
                        task.end_real_date = DateTime.Now;
                    }
                    else if (task.is_check == true && task.end_date == null)
                    {
                        task.close_by = uid;
                        task.close_date = DateTime.Now;
                        task.end_real_date = DateTime.Now;
                    }
                    else
                    {
                        task.close_by = null;
                        task.close_date = null;
                        task.end_real_date = null;
                    }
                    if (task.end_date != null)
                    {
                        task.is_deadline = true;
                    }
                    task.start_date = DateTime.Now;
                    task.created_by = uid;
                    task.created_date = DateTime.Now;
                    task.created_ip = ip;
                    task.created_token_id = tid;
                    db.task_origin.Add(task);
                    //notify
                    await db.SaveChangesAsync();
                    var listuser = db.task_member.Where(x => x.task_id == task.parent_id).Select(x => x.user_id).Distinct().ToList();
                    string task_name = db.task_origin.Where(x => x.task_id == task.parent_id).Select(x => x.task_name).FirstOrDefault().ToString();
                    listuser.Remove(uid);
                    foreach (var l in listuser)
                    {
                        helper.saveNotify(uid, l, null, "Công việc", "Thêm công việc checklist, công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                            null, 2, -1, false, module_key, task.parent_id, null, null, tid, ip);
                    }
                    #region add task_logs
                    if (helper.wlog)
                    {
                        task_logs log = new task_logs();
                        log.log_id = helper.GenKey();
                        log.task_id = task.task_id;
                        log.project_id = null;
                        log.description = "Thêm công việc checklist ";
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ChecklistDetail/addTaskCheckList", ip, tid, "Lỗi khi thêm công việc checklist", 0, "ChecklistDetail");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ChecklistDetail/addTaskCheckList", ip, tid, "Lỗi khi thêm công việc checklist", 0, "ChecklistDetail");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateTaskChecklists([System.Web.Mvc.Bind(Include = "task_id,task_name,task_name_en,is_check,close_by,close_date,end_real_date,modified_by,modified_date,modified_ip_modified_token_id")] task_origin task)
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
                    task.task_name_en = helper.convertToUnSign3(task.task_name);
                    if (task.is_check == true)
                    {
                        task.close_by = uid;
                        task.close_date = task.close_date != null ? task.close_date : DateTime.Now;
                        task.end_real_date = task.end_real_date != null ? task.end_real_date : DateTime.Now;
                    }
                    else
                    {
                        task.close_by = null;
                        task.close_date = null;
                        task.end_real_date = null;
                    }
                    task.modified_by = uid;
                    task.modified_date = DateTime.Now;
                    task.modified_ip = ip;
                    task.modified_token_id = tid;
                    db.Entry(task).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //notify
                    var listuser = db.task_member.Where(x => x.task_id == task.parent_id).Select(x => x.user_id).Distinct().ToList();
                    string task_name = db.task_origin.Where(x => x.task_id == task.parent_id).Select(x => x.task_name).FirstOrDefault().ToString();
                    listuser.Remove(uid);
                    foreach (var l in listuser)
                    {
                        helper.saveNotify(uid, l, null, "Công việc", "Chỉnh sửa công việc checklist,công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                            null, 2, -1, false, module_key, task.parent_id, null, null, tid, ip);
                    }
                    #region add task_logs
                    if (helper.wlog)
                    {
                        task_logs log = new task_logs();
                        log.log_id = helper.GenKey();
                        log.task_id = task.task_id;
                        log.project_id = null;
                        log.description = "Cập nhật công việc checklist ";
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ChecklistDetail/updateTaskCheckList", ip, tid, "Lỗi khi cập nhật công việc checklist", 0, "ChecklistDetail");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ChecklistDetail/updateTaskCheckList", ip, tid, "Lỗi khi cập nhật công việc checklist", 0, "ChecklistDetail");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_StatusTaskChecklist([System.Web.Mvc.Bind(Include = "TextID")] Trangthai trangthai)
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
                    var id = trangthai.TextID;
                    var das = db.task_origin.Where(a => (id == a.task_id)).FirstOrDefault<task_origin>();
                    if (das != null)
                    {
                        das.is_check = das.is_check == false || das.is_check == null ? true : false;
                        if (das.is_check == true)
                        {
                            das.close_by = uid;
                            das.close_date = DateTime.Now;
                            das.end_real_date = DateTime.Now;
                        }
                        else
                        {
                            das.close_by = null;
                            das.close_date = null;
                            das.end_real_date = null;
                        }
                        das.modified_by = uid;
                        das.modified_date = DateTime.Now;
                        das.modified_ip = ip;
                        das.modified_token_id = tid;
                        #region add task_logs
                        if (helper.wlog)
                        {
                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = das.task_id;
                            log.project_id = null;
                            log.description = "cập nhật trạng thái công việc checklist ";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();
                        }
                        #endregion
                        await db.SaveChangesAsync();
                        //notify
                        var listuser = db.task_member.Where(x => x.task_id == das.parent_id).Select(x => x.user_id).Distinct().ToList();
                        string task_name = db.task_origin.Where(x => x.task_id == das.parent_id).Select(x => x.task_name).FirstOrDefault().ToString();
                        listuser.Remove(uid);
                        foreach (var l in listuser)
                        {
                            helper.saveNotify(uid, l, null, "Công việc", "Cập nhật trạng thái công việc checklist,công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                null, 2, -1, false, module_key, das.parent_id, null, null, tid, ip);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "ChecklistDetail/Update_StatusTaskChecklist", ip, tid, "Lỗi khi cập nhật công việc checklist", 0, "ChecklistDetail");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "ChecklistDetail/Update_StatusTaskChecklist", ip, tid, "Lỗi khi cập nhật công việc checklist", 0, "ChecklistDetail");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> deleteTaskChecklist([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                    List<task_origin> delTask = new List<task_origin>();


                    var das = await db.task_origin.Where(t => id.Contains(t.task_id)).ToListAsync();
                    foreach (var de in das)
                    {
                        delTask.Add(de);

                        #region add task_logs
                        if (helper.wlog)
                        {

                            task_logs log = new task_logs();
                            log.log_id = helper.GenKey();
                            log.task_id = de.task_id;
                            log.project_id = null;
                            log.description = "xóa công việc checklist ";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_logs.Add(log);
                            db.SaveChanges();
                        }
                        #endregion
                    }//notify
                    string ssid = das[0].parent_id;

                    var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                    string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                    listuser.Remove(uid);
                    foreach (var l in listuser)
                    {
                        helper.saveNotify(uid, l, null, "Công việc", "Xóa công việc checklist,công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                            null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                    }

                    if (delTask.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                    }
                    if (delTask.Count > 0)
                    { db.task_origin.RemoveRange(delTask); }

                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Checklists/deleteChecklists", ip, tid, "Lỗi khi xoá Checklists", 0, "Checklists");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Checklists/deleteChecklists", ip, tid, "Lỗi khi xoá Checklists", 0, "Checklists");
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