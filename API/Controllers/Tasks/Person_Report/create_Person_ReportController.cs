using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using API.Models;
using Helper;
using Newtonsoft.Json;
using API.Helper;
namespace API.Controllers.Tasks
{
    [Authorize(Roles = "login")]
    public class create_Person_ReportController : ApiController
    {
        public string getipaddress()
        {
            //var host = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (var ip in host.AddressList)
            //{
            //    if (ip.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        return ip.ToString();
            //    }
            //}
            //return "localhost";
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_Report([System.Web.Mvc.Bind(Include = "self_point,organization_id,modified_by,modified_date,modified_ip,modified_token_id")] task_person_report role_Groups)
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

                    int id = Convert.ToInt32(dvid);
                    if (role_Groups.self_point > 100) role_Groups.self_point = 100;
                    role_Groups.created_by = uid;
                    role_Groups.created_date = DateTime.Now;
                    role_Groups.created_ip = ip;
                    role_Groups.created_token_id = tid;
                    role_Groups.organization_id = id;
                    db.task_person_report.Add(role_Groups);
                    await db.SaveChangesAsync();
                    #region add sys_logs
                    if (helper.wlog)
                    {
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = role_Groups, }), domainurl + "create_Person_Report/add_Report", ip, tid, "Thêm mới Đánh giá công việc cá nhân", 1, "Công việc");
                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "create_Person_Report/add_Report", ip, tid, "Lỗi khi thêm  Đánh giá công việc cá nhân", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "create_Person_Report/add_Report", ip, tid, "Lỗi khi thêm  Đánh giá công việc cá nhân", 0, "Công việc");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        public async Task<HttpResponseMessage> add_Report_BH([System.Web.Mvc.Bind(Include = "report_id,status,self_point,organization_id,modified_by,modified_date,modified_ip,modified_token_id")] task_person_report role_Groups)
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

                    int id = Convert.ToInt32(dvid);
                    if (role_Groups.self_point > 100) role_Groups.self_point = 100;
                    role_Groups.created_by = uid;
                    role_Groups.created_date = DateTime.Now;
                    role_Groups.created_ip = ip;
                    role_Groups.created_token_id = tid;
                    role_Groups.organization_id = id;
                    db.task_person_report.Add(role_Groups);
                    await db.SaveChangesAsync(); if (helper.wlog)
                    {
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = role_Groups, }), domainurl + "create_Person_Report/add_Report", ip, tid, "Thêm mới Đánh giá công việc cá nhân", 1, "Công việc");
                    }
                    var user = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    var udp = db.task_department_configuration.FirstOrDefault(x => x.department_id == user.department_id || x.department_id == user.organization_id);
                    if (udp != null)
                    {
                        task_person_report_processing tp = new task_person_report_processing();
                        role_Groups.status = 1;
                        db.Entry(role_Groups).State = EntityState.Modified;
                        tp.report_id = role_Groups.report_id;
                        tp.review_turn = 1;
                        tp.user_id = udp.user_id;
                        tp.is_step = 1;
                        tp.is_type = -1;
                        tp.created_date = DateTime.Now;
                        tp.created_ip = ip;
                        tp.created_by = uid;
                        tp.created_token_id = tid;
                        db.task_person_report_processing.Add(tp);
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", ms = "Chưa có người duyệt!<br/>Đã lưu lại báo cáo!<br/> Vui lòng thiết lập phòng ban và trình duyệt lại!" });
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "create_Person_Report/add_Report", ip, tid, "Lỗi khi thêm  Đánh giá công việc cá nhân", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "create_Person_Report/add_Report", ip, tid, "Lỗi khi thêm  Đánh giá công việc cá nhân", 0, "Công việc");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_report([System.Web.Mvc.Bind(Include = "self_point,organization_id,modified_by,modified_date,modified_ip,modified_token_id")] task_person_report role_Groups)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            List<string> delfiles = new List<string>();

            try
            {
                using (DBEntities db = new DBEntities())
                {

                    int id = Convert.ToInt32(dvid);
                    if (role_Groups.self_point > 100) role_Groups.self_point = 100;
                    role_Groups.organization_id = id;
                    role_Groups.modified_by = uid;
                    role_Groups.modified_date = DateTime.Now;
                    role_Groups.modified_ip = ip;
                    role_Groups.modified_token_id = tid;
                    db.Entry(role_Groups).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = role_Groups, }), domainurl + "create_Person_Report/Update_report", ip, tid, "Cập nhật  Đánh giá công việc cá nhân", 1, "Công việc");
                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "create_Person_Report/Update_report", ip, tid, "Lỗi khi cập nhật  Đánh giá công việc cá nhân", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "create_Person_Report/Update_report", ip, tid, "Lỗi khi cập nhật  Đánh giá công việc cá nhân", 0, "Công việc");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_group([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                    var das = await db.task_person_report.Where(a => id.Contains(a.report_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<task_person_report> del = new List<task_person_report>();
                        foreach (var da in das)
                        {
                            del.Add(da);
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, }), domainurl + "create_Person_Report/Delete_Weights", ip, tid, "Xóa  Đánh giá công việc cá nhân", 1, "Công việc");
                            }
                            #endregion
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.task_person_report.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "create_Person_Report/Delete_Weights", ip, tid, "Lỗi khi xoá  Đánh giá công việc cá nhân", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "create_Person_Report/Delete_Weights", ip, tid, "Lỗi khi xoá  Đánh giá công việc cá nhân", 0, "Công việc");
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

