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
    public class BookingMealController : ApiController
    {
        //Booking module key
        private const string const_module_key = "M10";
        public string getipaddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "localhost";
        }
        public class bookings
        {
            public int organization_id { get; set; }
            public int price { get; set; }
            public string[] working_days { get; set; }
            public List<logs_price> list_price { get; set; }
        }
        public class logs_price
        {
            public int price { get; set; }
            public string day { get; set; }
            public string day_string { get; set; }
        }
        [Authorize(Roles = "login")]
        [HttpPost]
        public async Task<HttpResponseMessage> add_booking()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string fbooking = "";
            string listdates = "";
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                        fbooking = provider.FormData.GetValues("booking").SingleOrDefault();
                        booking_meal booking = JsonConvert.DeserializeObject<booking_meal>(fbooking);
                        listdates = provider.FormData.GetValues("listdates").SingleOrDefault();
                        List<DateTime> dates = JsonConvert.DeserializeObject<List<DateTime>>(listdates);
                        booking.booking_id = helper.GenKey();
                        booking.created_by = uid;
                        booking.created_ip = ip;
                        booking.modified_date = DateTime.Now;
                        booking.modified_by = uid;
                        booking.modified_ip = ip;
                        db.booking_meal.Add(booking);
                        if (dates.Count > 0)
                        {
                            var count = 1;
                            foreach (DateTime item in dates)
                            {
                                booking_user_date bk = new booking_user_date();
                                bk.user_id = booking.user_id;
                                bk.booking_id = booking.booking_id;
                                bk.booking_date = item;
                                bk.is_order = count;
                                count++;
                                db.booking_user_date.Add(bk);
                            }
                        }

                        db.SaveChanges();
                        #region sendhub
                        sys_users created_user = db.sys_users.Find(booking.created_by);
                        sys_users booking_user = db.sys_users.Find(booking.user_id);


                        var sh = new sys_sendhub();
                        sh.senhub_id = helper.GenKey();
                        sh.module_key = const_module_key;
                        sh.user_send = uid;
                        sh.receiver = booking.user_id;
                        sh.icon = created_user.avatar;
                        sh.title = "Báo cắt cơm";
                        sh.contents = "<b>" + created_user.full_name + "</b> đã thêm mới phiếu cắt cơm cho <b>" + booking_user.full_name + "</b> " + (booking.reason != null ? ": " + booking.reason : "");
                        sh.is_type = 2;
                        sh.date_send = DateTime.Now;
                        sh.module_key = const_module_key;
                        sh.id_key = booking.booking_id;
                        sh.token_id = tid;
                        sh.created_date = DateTime.Now;
                        sh.created_by = uid;
                        sh.created_token_id = tid;
                        sh.created_ip = ip;
                        db.sys_sendhub.Add(sh);

                        #endregion

                        #region add cms_logs
                        if (helper.wlog)
                        {

                            task_log log = new task_log();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_log.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fbooking, contents }), domainurl + "BokingMeal/add_booking", ip, tid, "Lỗi khi thêm đặt cơm", 0, "BokingMeal");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fbooking, contents }), domainurl + "BokingMeal/add_booking", ip, tid, "Lỗi khi thêm đặt cơm", 0, "BokingMeal");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_booking()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string fbooking = "";
            string listdates = "";
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
                        fbooking = provider.FormData.GetValues("booking").SingleOrDefault();
                        booking_meal booking = JsonConvert.DeserializeObject<booking_meal>(fbooking);
                        listdates = provider.FormData.GetValues("listdates").SingleOrDefault();
                        List<DateTime> dates = JsonConvert.DeserializeObject<List<DateTime>>(listdates);
                        booking.modified_date = DateTime.Now;
                        booking.modified_by = uid;
                        booking.modified_ip = ip;
                        db.Entry(booking).State = EntityState.Modified;
                        if (dates.Count > 0)
                        {
                            //delte all 
                            List<booking_user_date> itemToRemove = db.booking_user_date.Where(a => a.booking_id == booking.booking_id).ToList();
                            if (itemToRemove != null)
                                db.booking_user_date.RemoveRange(itemToRemove);
                            var count = 0;
                            foreach (DateTime item in dates)
                            {
                                booking_user_date bk = new booking_user_date();
                                bk.user_id = booking.user_id;
                                bk.booking_id = booking.booking_id;
                                bk.booking_date = item;
                                bk.is_order = count;
                                count++;
                                db.booking_user_date.Add(bk);
                                db.SaveChanges();
                            }
                        }

                        db.SaveChanges();
                        #region sendhub
                        sys_users created_user = db.sys_users.Find(booking.created_by);
                        sys_users booking_user = db.sys_users.Find(booking.user_id);


                        var sh = new sys_sendhub();
                        sh.senhub_id = helper.GenKey();
                        sh.module_key = const_module_key;
                        sh.user_send = uid;
                        sh.receiver = booking.user_id;
                        sh.icon = created_user.avatar;
                        sh.title = "Báo cắt cơm";
                        sh.contents = "<b>" + created_user.full_name + "</b> đã sửa phiếu cắt cơm cho <b>" + booking_user.full_name + "</b> " + (booking.reason != null ? ": " + booking.reason : "");
                        sh.is_type = 2;
                        sh.date_send = DateTime.Now;
                        sh.module_key = const_module_key;
                        sh.id_key = booking.booking_id;
                        sh.created_date = DateTime.Now;
                        sh.created_by = uid;
                        sh.created_token_id = tid;
                        sh.created_ip = ip;
                        db.sys_sendhub.Add(sh);

                        #endregion
                        #region add cms_logs
                        if (helper.wlog)
                        {

                            task_log log = new task_log();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.task_log.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fbooking, contents }), domainurl + "BokingMeal/update_booking", ip, tid, "Lỗi khi sửa đặt cơm", 0, "BokingMeal");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fbooking, contents }), domainurl + "BokingMeal/update_booking", ip, tid, "Lỗi khi sửa đặt cơm", 0, "BokingMeal");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Del_Booking([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        var das = await db.booking_meal.Where(a => ids.Contains(a.booking_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<booking_meal> del = new List<booking_meal>();
                            foreach (var da in das)
                            {

                                del.Add(da);
                                #region  add Log
                                //helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "Users/Del_Users", ip, tid, "Xoá User " + da.user_id, 1, "Users");
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa phiếu này." });
                            }
                            db.booking_meal.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "BookingMeal/Del_Booking", ip, tid, "Lỗi khi xoá phiếu đặt cơm", 0, "BookingMeal");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "BookingMeal/Del_Booking", ip, tid, "Lỗi khi xoá phiếu đặt cơm", 0, "BookingMeal");
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

        [HttpPut]
        public async Task<HttpResponseMessage> Update_statusUser([System.Web.Mvc.Bind(Include = "ids,tts")][FromBody] JObject data)
        {
            List<string> ids = data["ids"].ToObject<List<string>>();
            List<bool> tts = data["tts"].ToObject<List<bool>>();
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
                        var das = await db.sys_users.Where(a => ids.Contains(a.user_id)).ToListAsync();
                        if (das != null)
                        {
                            List<sys_users> del = new List<sys_users>();
                            for (int i = 0; i < das.Count; i++)
                            {
                                var da = das[i];
                                if (ad)
                                {
                                    #region  add Log
                                    #endregion
                                    da.is_booking = tts[i];
                                }
                            }
                            await db.SaveChangesAsync();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Booking/Update_status", ip, tid, "Lỗi khi cập nhật trạng thái user", 0, "user");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Booking/Update_status", ip, tid, "Lỗi khi cập nhật trạng thái user", 0, "user");
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
        #region Config
        
        [HttpGet]
        public HttpResponseMessage GetConfig()
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
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    string json = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Config/BookingConfig.json"));
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.DeserializeObject<bookings>(json) });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "BookingMeal/SetConfig", ip, tid, "Lỗi khi cấu hình tham số hệ thống", 0, "Cache");
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
        public HttpResponseMessage SetConfig([FromBody] bookings cog)
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
                if (!ad)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
                try
                {
                    //helper.debug = cog.debug;
                    //helper.wlog = cog.wlog;
                    //helper.logCongtent = cog.logCongtent;
                    //helper.milisec = cog.milisec;
                    //helper.timeout = cog.timeout;
                    string jsonData = JsonConvert.SerializeObject(cog, Formatting.None);
                    System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath("~/Config/BookingConfig.json"), jsonData);
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "BookingMeal/SetConfig", ip, tid, "Lỗi khi cấu hình tham số hệ thống", 0, "Cache");
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
                            log.controller = domainurl + "Booking/GetDataProc";
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Booking/GetDataProc", ip, tid, "Lỗi khi gọi proc", 0, "Booking");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Booking/GetDataProc", ip, tid, "Lỗi khi gọi proc", 0, "Booking");
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
        #endregion
        #endregion


    }
}