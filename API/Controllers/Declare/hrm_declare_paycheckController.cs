using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Controllers.HRM.Category
{
    [Authorize(Roles = "login")]
    public class hrm_declare_paycheckController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        #region CallProc

        [HttpPost]
        public async Task<HttpResponseMessage> getData([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
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
                            log.controller = domainurl + "Proc/CallProc";
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

                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Proc/CallProc", ip, tid, "Lỗi khi gọi proc ", 0, "Proc");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Proc/CallProc", ip, tid, "Lỗi khi gọi proc ", 0, "Proc");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }
        #endregion
        [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_declare_paycheck()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fddeclare_paycheck = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");

                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fddeclare_paycheck = provider.FormData.GetValues("hrm_declare_paycheck").SingleOrDefault();
                        hrm_declare_paycheck declare_paycheck = JsonConvert.DeserializeObject<hrm_declare_paycheck>(fddeclare_paycheck);
                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                        declare_paycheck.organization_id = int.Parse(dvid);
                        declare_paycheck.created_by = uid;
                        declare_paycheck.created_date = DateTime.Now;
                        declare_paycheck.created_ip = ip;
                        declare_paycheck.created_token_id = tid;
                        db.hrm_declare_paycheck.Add(declare_paycheck);
                        db.SaveChanges();

                        #region add hrm_log
                        if (helper.wlog)
                        {

                            hrm_log log = new hrm_log();
                            log.title = "Thêm mẫu phiếu lương " + declare_paycheck.declare_paycheck_name;

                            log.log_module = "declare_paycheck";
                            log.log_type = 0;
                            log.id_key = declare_paycheck.declare_paycheck_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.hrm_log.Add(log);
                            db.SaveChanges();

                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fddeclare_paycheck, contents }), domainurl + "hrm_declare_paycheck/Add_declare_paycheck", ip, tid, "Lỗi khi thêm mẫu phiếu lương", 0, "mẫu phiếu lương");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fddeclare_paycheck, contents }), domainurl + "hrm_declare_paycheck/Add_declare_paycheck", ip, tid, "Lỗi khi thêm mẫu phiếu lương", 0, "mẫu phiếu lương  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_hrm_declare_paycheck()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fddeclare_paycheck = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");

                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fddeclare_paycheck = provider.FormData.GetValues("hrm_declare_paycheck").SingleOrDefault();
                        hrm_declare_paycheck declare_paycheck = JsonConvert.DeserializeObject<hrm_declare_paycheck>(fddeclare_paycheck);






                        declare_paycheck.modified_by = uid;
                        declare_paycheck.modified_date = DateTime.Now;
                        declare_paycheck.modified_ip = ip;
                        declare_paycheck.modified_token_id = tid;
                        db.Entry(declare_paycheck).State = EntityState.Modified;
                        db.SaveChanges();


                        #region add hrm_log
                        if (helper.wlog)
                        {

                            hrm_log log = new hrm_log();
                            log.title = "Sửa mẫu phiếu lương " + declare_paycheck.declare_paycheck_name;
                            log.log_module = "declare_paycheck";
                            log.log_type = 1;
                            log.id_key = declare_paycheck.declare_paycheck_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.hrm_log.Add(log);
                            db.SaveChanges();

                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fddeclare_paycheck, contents }), domainurl + "hrm_declare_paycheck/Update_declare_paycheck", ip, tid, "Lỗi khi cập nhật declare_paycheck", 0, "declare_paycheck");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fddeclare_paycheck, contents }), domainurl + "hrm_declare_paycheck/Update_declare_paycheck", ip, tid, "Lỗi khi cập nhật declare_paycheck", 0, "declare_paycheck");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_hrm_declare_paycheck([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;

            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.hrm_declare_paycheck.Where(a => id.Contains(a.declare_paycheck_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<hrm_declare_paycheck> del = new List<hrm_declare_paycheck>();
                            foreach (var da in das)
                            {

                                del.Add(da);

                                #region add hrm_log
                                if (helper.wlog)
                                {

                                    hrm_log log = new hrm_log();
                                    log.title = "Xóa mẫu phiếu lương " + da.declare_paycheck_name;

                                    log.log_module = "declare_paycheck";
                                    log.log_type = 2;
                                    log.id_key = da.declare_paycheck_id.ToString();
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.hrm_log.Add(log);
                                    db.SaveChanges();

                                }
                                #endregion

                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_declare_paycheck.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_declare_paycheck/Delete_declare_paycheck", ip, tid, "Lỗi khi xoá tem", 0, "declare_paycheck");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_declare_paycheck/Delete_declare_paycheck", ip, tid, "Lỗi khi xoá tem", 0, "declare_paycheck");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }


        [HttpPut]
        public async Task<HttpResponseMessage> update_s_hrm_declare_paycheck([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;

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
                        var int_id = int.Parse(trangthai.IntID.ToString());
                        var das = db.hrm_declare_paycheck.Where(a => (a.declare_paycheck_id == int_id)).FirstOrDefault<hrm_declare_paycheck>();
                        if (das != null)
                        {
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            das.status = !trangthai.BitTrangthai;


                            #region add hrm_log
                            if (helper.wlog)
                            {

                                hrm_log log = new hrm_log();
                                log.title = "Sửa mẫu phiếu lương " + das.declare_paycheck_name;

                                log.log_module = "declare_paycheck";
                                log.log_type = 1;
                                log.id_key = das.declare_paycheck_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.hrm_log.Add(log);
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_declare_paycheck/Update_Trangthaideclare_paycheck", ip, tid, "Lỗi khi cập nhật trạng thái declare_paychecks", 0, "hrm_declare_paycheck");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_declare_paycheck/Update_Trangthaideclare_paycheck", ip, tid, "Lỗi khi cập nhật trạng thái declare_paychecks", 0, "hrm_declare_paycheck");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }
    }
}