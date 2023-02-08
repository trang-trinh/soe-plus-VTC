using API.Models;
using Helper;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.ApplicationBlocks.Data;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using Spire.Doc;
using System.Threading;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace API.Controllers.Statistical
{
    [Authorize]
    public class statisticalController : ApiController
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

        [HttpPost]
        public async Task<HttpResponseMessage> get_datas([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);
            try
            {
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "statistical/get_datas", ip, tid, "Lỗi khi gọi get_datas proc", 0, "statistical");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "statistical/get_datas", ip, tid, "Lỗi khi gọi get_datas proc", 0, "statistical");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> get_datas_public([System.Web.Mvc.Bind(Include = "excelname,proc,par,publictoken")][FromBody] sqlPublicProc proc)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
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
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_statistical()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlaw = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    // Provider
                    string rootTemp = HttpContext.Current.Server.MapPath("~/Portals");
                    bool existsTemp = Directory.Exists(rootTemp);
                    if (!existsTemp)
                        Directory.CreateDirectory(rootTemp);
                    var provider = new MultipartFormDataStreamProvider(rootTemp);
                    var task = await Request.Content.ReadAsMultipartAsync(provider);

                    // Params
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    bool isAdd = bool.Parse(provider.FormData.GetValues("isAdd").SingleOrDefault());
                    var md = provider.FormData.GetValues("model").SingleOrDefault();
                    statistical_procedure model = JsonConvert.DeserializeObject<statistical_procedure>(md);
                    calendar_log log = new calendar_log();

                    if (isAdd)
                    {
                        model.statistical_id = helper.GenKey();
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        log.message = "Thêm mới thủ tục: " + model.statistical_name;
                        db.statistical_procedure.Add(model);
                    }
                    else
                    {
                        model.modified_by = uid;
                        model.modified_date = DateTime.Now;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        db.Entry(model).State = EntityState.Modified;
                        log.message = "Cập nhật thủ tục: " + model.statistical_name;
                    }
                    #region add law_logs
                    if (helper.wlog)
                    {
                        log.log_type = 4;
                        log.key_id = model.statistical_id;
                        log.is_view = false;
                        log.created_by = uid;
                        log.created_date = DateTime.Now;
                        log.created_ip = ip;
                        log.created_token_id = tid;
                        db.calendar_log.Add(log);
                    }
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "statistical/update_statistical", ip, tid, "Lỗi khi cập nhật thủ tục", 0, "statistical");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "statistical/update_statistical", ip, tid, "Lỗi khi cập nhật thủ tục", 0, "statistical");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_status_statistical([System.Web.Mvc.Bind(Include = "id,status")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                string id = data["id"].ToObject<string>();
                bool? status = data["status"]?.ToObject<bool?>();

                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var model = await db.statistical_procedure.FindAsync(id);
                        if (model != null)
                        {
                            model.status = status;
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                calendar_log log = new calendar_log();
                                log.log_type = 5;
                                log.message = "Cập nhât trạng thái : " + model.statistical_name;
                                log.key_id = model.statistical_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                db.calendar_log.Add(log);
                            }
                            #endregion
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "statistical/update_status_ca_boardroom", ip, tid, "Lỗi khi cập nhật trạng thái", 0, "statistical");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "statistical/update_status_ca_boardroom", ip, tid, "Lỗi khi cập nhật trạng thái", 0, "statistical");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_statistical([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.statistical_procedure.Where(a => ids.Contains(a.statistical_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<statistical_procedure> del = new List<statistical_procedure>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    calendar_log log = new calendar_log();
                                    log.log_type = 4;
                                    log.message = "Xóa : " + da.statistical_name;
                                    log.key_id = da.statistical_id;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    log.is_view = false;
                                    db.calendar_log.Add(log);
                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.statistical_procedure.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_config/delete_procedureform", ip, tid, "Lỗi khi xóa quy trình", 0, "calendar_config");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_config/delete_procedureform", ip, tid, "Lỗi khi xoá quy trình", 0, "calendar_config");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> statistical_procedure_get([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            try
            {
                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    string search = jobject["search"].ToObject<string>();
                    var sqlpas = new List<SqlParameter>();
                    sqlpas.Add(new SqlParameter("@" + "search", search));
                    var arrpas = sqlpas.ToArray();
                    DateTime sdate = DateTime.Now;
                    var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "statistical_procedure_gets", arrpas).Tables);
                    var tables = await task;
                    DateTime edate = DateTime.Now;
                    string JSONresult = JsonConvert.SerializeObject(tables);
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "statistical/statistical_procedure_gets", ip, tid, "Lỗi khi cập lấy dữ liệu", 0, "statistical");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "statistical/statistical_procedure_gets", ip, tid, "Lỗi khi lấy dữ liệu", 0, "statistical");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> statistical_get([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            try
            {
                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    string statistical_id = jobject["statistical_id"].ToObject<string>();
                    var sqlpas = new List<SqlParameter>();
                    sqlpas.Add(new SqlParameter("@" + "statistical_id", statistical_id));
                    var arrpas = sqlpas.ToArray();
                    DateTime sdate = DateTime.Now;
                    var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "statistical_procedure_get", arrpas).Tables);
                    var tables = await task;
                    DateTime edate = DateTime.Now;
                    List<statistical_procedure> statistical = JsonConvert.DeserializeObject<List<statistical_procedure>>(JsonConvert.SerializeObject(tables[0]));
                    if (statistical != null && statistical.Count > 0)
                    {
                        //Get config
                        settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));

                        //Parameter
                        string proc = statistical.FirstOrDefault().procedure_name;
                        DateTime? tu = jobject["tu"]?.ToObject<DateTime?>();
                        DateTime? den = jobject["den"]?.ToObject<DateTime?>();
                        int? loai = statistical.FirstOrDefault().is_type;
                        DateTime date = DateTime.Now;
                        var firstDayOfMonth = (new DateTime(date.Year, date.Month, 1));
                        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                        List<sqlPar2> par = new List<sqlPar2>();
                        par.Add(new sqlPar2() { key = "tu", value = tu != null ? tu.ToString() : firstDayOfMonth.ToString() });
                        par.Add(new sqlPar2() { key = "den", value = den != null ? den.ToString() : lastDayOfMonth.ToString() });
                        par.Add(new sqlPar2() { key = "loai", value = loai.ToString() });
                        sqlProc2 jb = new sqlProc2();
                        jb.token = config.tokenBHBQP;
                        jb.proc = proc;
                        jb.par = par;
                        string strSQL = Codec.EncryptString(JsonConvert.SerializeObject(jb), config.keycodeBHBQP);

                        //Call api
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri(config.apiBHBQP);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        //JObject sqljson = new JObject();
                        //sqljson.Add(new JProperty("str", strSQL));
                        //string str = JsonConvert.SerializeObject(sqljson);
                        StringContent stringContent = new StringContent("\"" + strSQL + "\"", Encoding.UTF8, "application/json");
                        var response = await client.PostAsync("PostProc", stringContent);
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            JObject JSONresult = JsonConvert.DeserializeObject<JObject>(result);
                            var JSONdata = JSONresult["data"];
                            return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONdata, err = "0" });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1" });
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "", err = "1" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "statistical/statistical_get", ip, tid, "Lỗi khi cập lấy dữ liệu", 0, "statistical");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "statistical/statistical_get", ip, tid, "Lỗi khi lấy dữ liệu", 0, "statistical");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        #region class model
        private class sqlProc2 {
            public string token { get; set; }
            public string proc { get; set; }
            public List<sqlPar2> par { get; set; }
        }

        public class sqlPar2
        {
            public string key { get; set; }
            public string value { get; set; }
        }

        private class pars
        {
            public DateTime? tu { get; set; }
            public DateTime? den { get; set; }
            public int? loai { get; set; }
        }
        private class repot_1
        {
            public int soluong;
        }
        #endregion
    }
}
