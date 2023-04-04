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
namespace API.Controllers.Task_Ca
{

    [Authorize(Roles = "login")]
    public class taskGroupsController : ApiController
    {

        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Add_taskgroup([System.Web.Mvc.Bind(Include = "group_name,group_id,created_by,created_date,created_ip_created_token_id")] task_ca_taskgroup taskgroup)
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
                    taskgroup.created_by = uid;
                    taskgroup.created_date = DateTime.Now;
                    taskgroup.created_ip = ip;
                    taskgroup.created_token_id = tid;
                    //taskgroup.department_id = helper.Department(claims);
                    taskgroup.organization_child_id = helper.OrgainzationChild(claims);
                    taskgroup.organization_id = helper.Orgainzation(claims);
                    db.task_ca_taskgroup.Add(taskgroup);
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {

                        cms_logs log = new cms_logs();
                        log.log_title = "Thêm taskgroup " + taskgroup.group_name;
                        log.log_content = JsonConvert.SerializeObject(new { data = taskgroup });
                        log.log_module = "taskgroup";
                        log.id_key = taskgroup.group_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "taskgroup/Add_taskgroup", ip, tid, "Lỗi khi thêm taskgroup", 0, "taskgroup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "taskgroup/Add_taskgroup", ip, tid, "Lỗi khi thêm taskgroup", 0, "taskgroup");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents); Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_taskgroup([System.Web.Mvc.Bind(Include = "group_id,group_name,modified_by,modified_date,modified_ip_modified_token_id")] task_ca_taskgroup taskgroup)
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
                    taskgroup.modified_by = uid;
                    taskgroup.modified_date = DateTime.Now;
                    taskgroup.modified_ip = ip;
                    taskgroup.modified_token_id = tid;
                    db.Entry(taskgroup).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    #region add cms_logs
                    if (helper.wlog)
                    {

                        cms_logs log = new cms_logs();
                        log.log_title = "Sửa taskgroup " + taskgroup.group_name;
                        log.log_content = JsonConvert.SerializeObject(new { data = taskgroup });
                        log.log_module = "taskgroup";
                        log.id_key = taskgroup.group_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "taskgroup/Update_taskgroup", ip, tid, "Lỗi khi cập nhật taskgroup", 0, "taskgroup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "taskgroup/Update_taskgroup", ip, tid, "Lỗi khi cập nhật taskgroup", 0, "taskgroup");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        public List<int> findChild(List<int> id)
        {
            List<int> del = new List<int>();
            using (DBEntities db = new DBEntities())
            {
                var das = db.task_ca_taskgroup.Where(a => id.Contains(a.group_id)).ToArray();
                if (das != null)
                {
                    foreach (var da in das)
                    {
                        var arrC = db.task_ca_taskgroup.Where(a => a.parent_id != null).ToArray();
                        del.Add(da.group_id);
                        var arrId = new List<int>();
                        for (int i = 0; i < id.Count; i++)
                        {
                            for (int j = 0; j < arrC.Length; j++)
                            {
                                if (id[i] == arrC[j].parent_id)
                                {

                                    arrId.Add(arrC[j].group_id);
                                    del.AddRange(findChild(arrId));
                                }
                            }
                        }
                    }
                }
            }
            return del;
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_taskgroup([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                    var arr = findChild(id);
                    var das = await db.task_ca_taskgroup.Where(a => arr.Contains(a.group_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<task_ca_taskgroup> del = new List<task_ca_taskgroup>();
                        foreach (var da in das)
                        {

                            del.Add(da);
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                cms_logs log = new cms_logs();
                                log.log_title = "Xóa taskgroup" + da.group_name;
                                log.log_module = "taskgroup";
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
                        int count = 0;
                        foreach (var da in del)
                        {
                            var used = db.task_origin.Where(x => x.group_id == da.group_id).ToList();
                            if (used.Count != 0)
                            {
                                count++;
                            }
                        }
                        if (count > 0)
                        { return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Nhóm công việc đang được sử dụng.<br/>Bạn không thể xóa." }); }
                        db.task_ca_taskgroup.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();


                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "taskgroup/Delete_taskgroup", ip, tid, "Lỗi khi xoá taskgroup", 0, "taskgroup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "taskgroup/Delete_taskgroup", ip, tid, "Lỗi khi xoá taskgroup", 0, "taskgroup");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }



        [HttpPut]
        public async Task<HttpResponseMessage> Update_StatusTaskGroups([System.Web.Mvc.Bind(Include = "BitTrangthai,IntID")] Trangthai trangthai)
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
                    var das = db.task_ca_taskgroup.FirstOrDefault(a => (a.group_id == trangthai.IntID));
                    if (das != null)
                    {
                        das.modified_by = uid;
                        das.modified_date = DateTime.Now;
                        das.modified_ip = ip;
                        das.modified_token_id = tid;
                        das.status = !trangthai.BitTrangthai;

                        #region add cms_logs
                        if (helper.wlog)
                        {

                            cms_logs log = new cms_logs();
                            log.log_title = "Sửa trạng thái taskgroup" + das.group_name;

                            log.log_module = "taskgroup";
                            log.id_key = das.group_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.cms_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "taskgroup/Update_StatusTaskGroups", ip, tid, "Lỗi khi cập nhật trạng thái taskgroup", 0, "taskgroup");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "taskgroup/Update_StatusTaskGroups", ip, tid, "Lỗi khi cập nhật trạng thái taskgroup", 0, "taskgroup");
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

