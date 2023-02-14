using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Models;
using Helper;
using System.Web.Script.Serialization;
using HtmlAgilityPack;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class insuranceController : ApiController
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

        //[Authorize(Roles = "login")]
        //[Authorize]
        [HttpPost]
        public async Task<HttpResponseMessage> add_insurance()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
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

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Hrm/Profile/Avatar/";
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
                        fdmodel = provider.FormData.GetValues("insurance").SingleOrDefault();
                        hrm_insurance model = JsonConvert.DeserializeObject<hrm_insurance>(fdmodel);
                        if (db.hrm_insurance.Count(a => a.insurance_id == model.insurance_id) > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Đã có số sổ bảo hiểm này trong hệ thống rồi!", err = "2" });
                        }

                        var insurance_pay = provider.FormData.GetValues("insurance_pay").SingleOrDefault();
                        var insurance_resolve = provider.FormData.GetValues("insurance_resolve").SingleOrDefault();

                        List<hrm_insurance_pay> insurance_pays = JsonConvert.DeserializeObject<List<hrm_insurance_pay>>(insurance_pay);
                        List<hrm_insurance_resolve> insurance_resolves = JsonConvert.DeserializeObject<List<hrm_insurance_resolve>>(insurance_resolve);
                        model.is_order = db.hrm_insurance.Count() + 1;
                        model.status = 0;
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        model.created_token_id = tid;
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_token_id = tid;
                        model.created_ip = ip;

                        db.hrm_insurance.Add(model);
                        #region add pay
                        if (insurance_pays != null)
                        {
                            var count = db.hrm_insurance_pay.Count();
                            foreach (var item in insurance_pays)
                            {
                                count++;
                                hrm_insurance_pay sk = new hrm_insurance_pay();
                                sk = item;
                                sk.insurance_pay_id = helper.GenKey();
                                sk.insurance_id = model.insurance_id;
                                sk.is_order = count;
                                sk.created_token_id = tid;
                                sk.created_date = DateTime.Now;
                                sk.created_by = uid;
                                sk.created_token_id = tid;
                                sk.created_ip = ip;
                                db.hrm_insurance_pay.Add(sk);
                            }
                        }
                        #endregion
                        #region add resolves
                        if (insurance_resolves != null)
                        {
                            var count = db.hrm_insurance_resolve.Count();
                            foreach (var item in insurance_resolves)
                            {
                                count++;
                                hrm_insurance_resolve rl = new hrm_insurance_resolve();
                                rl = item;
                                rl.insurance_resolve_id = helper.GenKey();
                                rl.insurance_id = model.insurance_id;
                                rl.is_order = count;
                                rl.created_token_id = tid;
                                rl.created_date = DateTime.Now;
                                rl.created_by = uid;
                                rl.created_token_id = tid;
                                rl.created_ip = ip;
                                db.hrm_insurance_resolve.Add(rl);
                            }
                        }
                        #endregion
                        db.SaveChanges();
                        #region  add Log
                        //helper.saveLogFiles(uid, 5, null, model.profile_id, "Thêm folder " + model.folder_name, ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = model.insurance_id });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_insurance()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
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

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Hrm/Profile/Avatar/";
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
                        fdmodel = provider.FormData.GetValues("insurance").SingleOrDefault();
                        hrm_insurance model = JsonConvert.DeserializeObject<hrm_insurance>(fdmodel);
                        var insurance_pay = provider.FormData.GetValues("insurance_pay").SingleOrDefault();
                        var insurance_resolve = provider.FormData.GetValues("insurance_resolve").SingleOrDefault();

                        List<hrm_insurance_pay> insurance_pays = JsonConvert.DeserializeObject<List<hrm_insurance_pay>>(insurance_pay);
                        List<hrm_insurance_resolve> insurance_resolves = JsonConvert.DeserializeObject<List<hrm_insurance_resolve>>(insurance_resolve);
                        model.is_order = db.hrm_insurance.Count() + 1;
                        model.status = 0;
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;

                        db.Entry(model).State = EntityState.Modified;
                        #region add skill
                        //delte all 
                        List<hrm_insurance_pay> itemToRemove = db.hrm_insurance_pay.Where(a => a.insurance_id == model.insurance_id).ToList();
                        if (itemToRemove.Count > 0)
                            db.hrm_insurance_pay.RemoveRange(itemToRemove);
                        if (insurance_pays != null)
                        {
                            var count = db.hrm_insurance_pay.Count();
                            foreach (var item in insurance_pays)
                            {
                                count++;
                                hrm_insurance_pay sk = new hrm_insurance_pay();
                                sk = item;
                                sk.insurance_pay_id = helper.GenKey();
                                sk.insurance_id = model.insurance_id;
                                sk.is_order = count;
                                sk.created_token_id = tid;
                                sk.created_date = DateTime.Now;
                                sk.created_by = uid;
                                sk.created_token_id = tid;
                                sk.created_ip = ip;
                                db.hrm_insurance_pay.Add(sk);
                            }
                        }
                        #endregion
                        #region add relative
                        List<hrm_insurance_resolve> itemToRemove2 = db.hrm_insurance_resolve.Where(a => a.insurance_id == model.insurance_id).ToList();
                        if (itemToRemove.Count > 0)
                            db.hrm_insurance_resolve.RemoveRange(itemToRemove2);
                        if (insurance_resolves != null)
                        {
                            var count = db.hrm_insurance_resolve.Count();
                            foreach (var item in insurance_resolves)
                            {
                                count++;
                                hrm_insurance_resolve rl = new hrm_insurance_resolve();
                                rl = item;
                                rl.insurance_resolve_id = helper.GenKey();
                                rl.insurance_id = model.insurance_id;
                                rl.is_order = count;
                                rl.created_token_id = tid;
                                rl.created_date = DateTime.Now;
                                rl.created_by = uid;
                                rl.created_token_id = tid;
                                rl.created_ip = ip;
                                db.hrm_insurance_resolve.Add(rl);
                            }
                        }
                        #endregion
                        db.SaveChanges();
                        #region  add Log
                        //helper.saveLogFiles(uid, 5, null, model.profile_id, "Thêm folder " + model.folder_name, ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = model.insurance_id });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> del_insurance([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
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
                        var das = await db.hrm_insurance.Where(a => ids.Contains(a.insurance_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das.Count > 0)
                        {
                            List<hrm_insurance> del = new List<hrm_insurance>();
                            foreach (var da in das)
                            {
                                //xoa file
                                var profiles = await db.hrm_insurance.Where(a => a.insurance_id == da.insurance_id).ToListAsync();
                                del.Add(da);
                                #region  add Log
                                #endregion
                            }
                            //foreach (string strPath in paths)
                            //{
                            //    bool exists = System.IO.File.Exists(strPath);
                            //    if (exists)
                            //        System.IO.File.Delete(strPath);
                            //}
                            db.hrm_insurance.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FileFolder/Del_FildeFolder", ip, tid, "Lỗi khi xoá người dùng", 0, "FildeFolder");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FildeFolder/Del_FildeFolder", ip, tid, "Lỗi khi xoá người dùng", 0, "FildeFolder");
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

        #region CallProc
        [HttpPost]
        public async Task<HttpResponseMessage> GetDataProc([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
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

            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
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
                        log.controller = domainurl + "User/GetDataProc";
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "chat/GetDataProc", ip, tid, "Lỗi khi gọi proc '" + proc + "'", 0, "chat");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "chat/GetDataProc", ip, tid, "Lỗi khi gọi proc '" + proc + "'", 0, "chat");
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