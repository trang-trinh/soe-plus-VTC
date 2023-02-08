using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers.Calendar
{
    //[Authorize(Roles = "login")]
    public class calendarController : ApiController
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
            return HttpContext.Current.Request.UserHostAddress;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> Tivi_GetCalendar([System.Web.Mvc.Bind(Include = "")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string str = data["str"].ToObject<string>();
            string des = Codec.DecryptString(str, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            string token = jobject["token"].ToObject<string>();
            settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));
            if (string.IsNullOrEmpty(token) || token != config.publictoken)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập dữ liệu này!", err = "1" });
            }
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = "administrator";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                DateTime? week_start_date = jobject["tungay"]?.ToObject<DateTime?>();
                DateTime? week_end_date = jobject["denngay"]?.ToObject<DateTime?>();
                var sqlpas = new List<SqlParameter>();
                sqlpas.Add(new SqlParameter("@" + "week_start_date", week_start_date));
                sqlpas.Add(new SqlParameter("@" + "week_end_date", week_end_date));
                var arrpas = sqlpas.ToArray();
                DateTime sdate = DateTime.Now;
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_tivi_get", arrpas).Tables);
                var tables = await task;
                DateTime edate = DateTime.Now;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar/Tivi_GetCalendar", ip, tid, "Lỗi khi lấy dữ liệu lich", 0, "calendar");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar/Tivi_GetCalendar", ip, tid, "Lỗi khi lấy dữ liệu lich", 0, "calendar");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [Authorize(Roles = "login")]
        [HttpPost]
        public async Task<HttpResponseMessage> add_log([System.Web.Mvc.Bind(Include = "title,controller,log_date,log_content,full_name,user_id,token_id,modified_ip,is_type,module,status,note,modified_date,modified_by,search_name,created_by,created_date,created_ip,created_token_id,modified_token_id")] sys_logs log)
        {
            using (DBEntities db = new DBEntities())
            {
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    if (ModelState.IsValid)
                    {
                        log.log_content = JsonConvert.SerializeObject(new { contents = log.log_content });
                        log.log_date = DateTime.Now;
                        log.full_name = name;
                        log.user_id = uid;
                        log.token_id = tid;
                        log.modified_ip = ip;
                        log.module = "Console";
                        log.status = 0;
                        log.created_ip = ip;
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.modified_ip = ip;
                        log.modified_date = DateTime.Now;
                        log.modified_by = uid;
                        log.modified_token_id = tid;
                        db.sys_logs.Add(log);
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = log, contents }), domainurl + "Proc/AddLog", ip, tid, "Lỗi khi Thêm log Console", 0, "Proc");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = log, contents }), domainurl + "Proc/AddLog", ip, tid, "Lỗi khi Thêm log Console", 0, "Proc");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }

        #region get datas
        [Authorize(Roles = "login")]
        [HttpPost]
        public async Task<HttpResponseMessage> get_datas([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                var sqlpas = new List<SqlParameter>();
                if (proc != null && proc.par != null)
                {
                    foreach (sqlPar p in proc.par)
                    {
                        sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                    }
                }
                var arrpas = sqlpas.ToArray();
                DateTime sdate = DateTime.Now;
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables);
                var tables = await task;
                DateTime edate = DateTime.Now;
                #region add SQLLog
                if (helper.wlog)
                {
                    using (DBEntities db = new DBEntities())
                    {
                        sql_log log = new sql_log();
                        log.controller = domainurl + "calendar/get_datas";
                        log.start_date = sdate;
                        log.end_date = edate;
                        log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                        log.user_id = uid;
                        log.token_id = tid;
                        log.created_ip = ip;
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.modified_ip = ip;
                        log.modified_date = DateTime.Now;
                        log.modified_by = uid;
                        log.modified_token_id = tid;
                        log.full_name = name;
                        log.title = proc.proc;
                        log.log_content = JsonConvert.SerializeObject(new { data = proc });
                        db.sql_log.Add(log);
                        await db.SaveChangesAsync();
                    }
                }
                #endregion
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "calendar/get_datas", ip, tid, "Lỗi khi gọi get_datas (proc)", 0, "calendar");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "calendar/get_datas", ip, tid, "Lỗi khi gọi get_datas (proc)", 0, "calendar");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

        #region get datas public
        [Authorize(Roles = "login")]
        [HttpPost]
        public async Task<HttpResponseMessage> get_datas_public([System.Web.Mvc.Bind(Include = "excelname,proc,par,publictoken")][FromBody] sqlPublicProc proc)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {

                if (proc.publictoken != helper.publictoken)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã Token không hợp lệ! Vui lòng thử lại.", err = "1" });
                }
                var sqlpas = new List<SqlParameter>();
                if (proc != null && proc.par != null)
                {
                    foreach (sqlPar p in proc.par)
                    {
                        sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                    }
                }
                var arrpas = sqlpas.ToArray();
                DateTime sdate = DateTime.Now;
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables);
                var tables = await task;
                DateTime edate = DateTime.Now;

                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

    }
}