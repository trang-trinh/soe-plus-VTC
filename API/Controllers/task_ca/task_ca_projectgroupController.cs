using API.Models;
using Helper;
using Newtonsoft.Json;
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
using API.Helper;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class task_ca_projectgroupController : ApiController
    {
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
        [HttpPost]
        public async Task<HttpResponseMessage> Add_Task_Ca_ProjectGroup([System.Web.Mvc.Bind(Include = "group_id,group_name,created_by,created_date,created_ip_created_token_id")] task_ca_projectgroup pg)
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
                    
                    pg.created_by = uid;
                    pg.created_date = DateTime.Now;
                    pg.created_ip = ip;
                    pg.created_token_id = tid;
                    //pg.department_id = helper.Department(claims);
                    pg.organization_child_id = helper.OrgainzationChild(claims);
                    pg.organization_id = helper.Orgainzation(claims);
                    db.task_ca_projectgroup.Add(pg);
                    await db.SaveChangesAsync();

                    if (helper.wlog)
                    {

                        cms_logs log = new cms_logs();
                        log.log_title = "Thêm nhóm project " + pg.group_name;
                        log.log_content = JsonConvert.SerializeObject(new { data = pg });
                        log.log_module = "Nhóm project";
                        log.id_key = pg.group_id.ToString();
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.cms_logs.Add(log);
                        db.SaveChanges();

                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Task_Ca_ProjectGroup/Add_Task_Ca_ProjectGroup", ip, tid, "Lỗi khi thêm nhóm dự án", 0, "Task_Ca_ProjectGroup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Task_Ca_ProjectGroup/Add_Task_Ca_ProjectGroup", ip, tid, "Lỗi khi thêm nhóm dự án", 0, "Task_Ca_ProjectGroup");
                if (!helper.debug)
                {
                    contents = "";
                }
            Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_Task_Ca_ProjectGroup([System.Web.Mvc.Bind(Include = "group_name,group_id,modified_by,modified_date,modified_ip_modified_token_id")] task_ca_projectgroup pg)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
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
            List<string> delfiles = new List<string>();

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    pg.modified_by = uid;
                    pg.modified_date = DateTime.Now;
                    pg.modified_ip = ip;
                    pg.modified_token_id = tid;
                    db.Entry(pg).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {

                        cms_logs log = new cms_logs();
                        log.log_title = "Sửa nhóm project " + pg.group_name;
                        log.log_content = JsonConvert.SerializeObject(new { data = pg });
                        log.log_module = "Nhóm project";
                        log.id_key = pg.group_id.ToString();
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.cms_logs.Add(log);
                        db.SaveChanges();

                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Task_Ca_ProjectGroup/Update_Task_Ca_ProjectGroup", ip, tid, "Lỗi khi sửa nhóm dự án", 0, "Task_Ca_ProjectGroup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Task_Ca_ProjectGroup/Update_Task_Ca_ProjectGroup", ip, tid, "Lỗi khi sửa nhóm dự án", 0, "Task_Ca_ProjectGroup");
                if (!helper.debug)
                {
                    contents = "";
                }
            Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_task_ca_projectgroup([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> ids)
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
                        var das = await db.task_ca_projectgroup.Where(a => ids.Contains(a.group_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<task_ca_projectgroup> del = new List<task_ca_projectgroup>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                                #region add cms_logs
                                if (helper.wlog)
                                {

                                    cms_logs log = new cms_logs();
                                    log.log_title = "Xóa nhóm project" + da.group_id;

                                    log.log_module = "Nhóm email";
                                    log.id_key = da.group_id.ToString();
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.cms_logs.Add(log);
                                    db.SaveChanges();

                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.task_ca_projectgroup.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Task_Ca_ProjectGroup/Delete_task_ca_projectgroup", ip, tid, "Lỗi khi xoá nhóm project", 0, "task_ca_projectgroup");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Task_Ca_ProjectGroup/Delete_task_ca_projectgroup", ip, tid, "Lỗi khi xoá nhóm project", 0, "task_ca_projectgroup");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

        }
    }
}