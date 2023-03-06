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
namespace API.Controllers.Task_Origin
{
    [Authorize(Roles = "login")]
    public class checkListController : ApiController
    {
        string module_key = "M4";
        public string getipaddress()
        {
            // var host = Dns.GetHostEntry(Dns.GetHostName());
            // foreach (var ip in host.AddressList)
            // {
            //     if (ip.AddressFamily == AddressFamily.InterNetwork)
            //     {
            //         return ip.ToString();
            //     }
            // }
            // return "localhost";
            return HttpContext.Current.Request.UserHostAddress;
        }

        #region Nhóm checklist
        [HttpPost]
        public async Task<HttpResponseMessage> addCheckList([System.Web.Mvc.Bind(Include = "task_id,checklist_id,created_date,created_by,created_date,created_ip,created_token_id")] task_checklists task_Checklists)
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
                    task_Checklists.checklist_id = helper.GenKey();
                    task_Checklists.created_by = uid;
                    task_Checklists.created_date = DateTime.Now;
                    task_Checklists.created_ip = ip;
                    task_Checklists.created_token_id = tid;
                    db.task_checklists.Add(task_Checklists);
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {

                        task_logs log = new task_logs();
                        log.log_id = helper.GenKey();
                        log.task_id = task_Checklists.task_id;
                        log.project_id = null;
                        log.description = "Thêm mới checklist công việc ";
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Checklists/addChecklists", ip, tid, "Lỗi khi thêm Checklists", 0, "Checklists");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Checklists/addChecklists", ip, tid, "Lỗi khi thêm Checklists", 0, "Checklists");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> updateChecklist([System.Web.Mvc.Bind(Include = "task_id,modified_date,modified_by,modified_ip,modified_token_id")] task_checklists task_Checklists)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            List<string> delfiles = new List<string>();
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    task_Checklists.modified_by = uid;
                    task_Checklists.modified_date = DateTime.Now;
                    task_Checklists.modified_ip = ip;
                    task_Checklists.modified_token_id = tid;
                    db.Entry(task_Checklists).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {

                        task_logs log = new task_logs();
                        log.log_id = helper.GenKey();
                        log.task_id = task_Checklists.task_id;
                        log.project_id = null;
                        log.description = "Sửa thông tin checklist công việc";
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "Checklists/updateChecklists", ip, tid, "Lỗi khi cập nhật Checklists", 0, "Checklists");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "Checklists/updateChecklists", ip, tid, "Lỗi khi cập nhật Checklists", 0, "Checklists");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> deleteChecklist([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                    List<task_checklists> del = new List<task_checklists>();
                    List<task_origin> delTask = new List<task_origin>();

                    
                        var das = await db.task_checklists.Where(a => id.Contains(a.checklist_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            foreach (var da in das)
                            {
                                del.Add(da);
                                var das1 = await db.task_origin.Where(t => da.checklist_id.Contains(t.checklist_id)).ToListAsync();
                                foreach (var de in das1)
                                {
                                    delTask.Add(de);

                                    #region add cms_logs
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
                                }
                                if(das1.Count>0)
                                {
                                    string ssid = das1[0].task_id;

                                    var listuser = db.task_member.Where(x => x.task_id == ssid).Select(x => x.user_id).Distinct().ToList();
                                    string task_name = db.task_origin.Where(x => x.task_id == ssid).Select(x => x.task_name).FirstOrDefault().ToString();
                                    listuser.Remove(uid);

                                    foreach (var l in listuser)
                                    {
                                        helper.saveNotify(uid, l, null, "Công việc", "Xóa checklist công việc: " + (task_name.Length > 100 ? task_name.Substring(0, 97) + "..." : task_name),
                                            null, 2, -1, false, module_key, ssid, null, null, tid, ip);
                                    }
                                }    
                                #region add cms_logs
                                if (helper.wlog)
                                {

                                    task_logs log = new task_logs();
                                    log.log_id = helper.GenKey();
                                    log.task_id = da.task_id;
                                    log.project_id = null;
                                    log.description = "xóa checklist công việc";
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
                    
                    if (del.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                    }
                    if (delTask.Count > 0)
                    { db.task_origin.RemoveRange(delTask); }
                    db.task_checklists.RemoveRange(del);
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

        #endregion
    }
}
