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
    public class hrm_payrollController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
    [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_payroll()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdpayroll = "";
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
                        fdpayroll = provider.FormData.GetValues("hrm_payroll").SingleOrDefault();
                        hrm_payroll payroll = JsonConvert.DeserializeObject<hrm_payroll>(fdpayroll);
                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                        var das =   db.hrm_payroll.AsNoTracking().Where(a =>  a.payroll_month == payroll.payroll_month && a.payroll_year == payroll.payroll_year).ToList();
                        if (das.Count > 0) {

                            foreach (var item in das)
                            {

                                var check = item.list_profile_id.Split(',').Intersect(payroll.list_profile_id.Split(','));

                                if (check.Any())
                                {
                                    var str = "";
                                    var strv = "";
                                    foreach (var sitem in check)
                                    {
                                        var user = db.hrm_profile.AsNoTracking().Where(a => a.profile_id==sitem).FirstOrDefault();
                                        str +=strv+ user.profile_user_name;
                                        strv = ",";
                                    }
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Nhân sự đã được khai báo ở bảng lương khác trong tháng: "+ str });
                                }

                            }
                    
                        }

                        if (payroll.is_approved == true)
                        {
                            var payyroll_ap = db.hrm_payroll.Where(a =>a.is_approved==true && a.payroll_month == payroll.payroll_month 
                            && a.payroll_year == payroll.payroll_year).ToList();
                            foreach (var item in payyroll_ap)
                            {
                                item.is_approved = false;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        payroll.organization_id = int.Parse(dvid);
                        payroll.created_by = uid;
                        payroll.created_date = DateTime.Now;
                        payroll.created_ip = ip;
                        payroll.created_token_id = tid;
                        db.hrm_payroll.Add(payroll);
                        db.SaveChanges();
                        foreach (var item in payroll.list_profile_id.Split(','))
                        {
                            var puser = new hrm_payroll_user();
                            puser.is_data = null;
                            puser.organization_id = payroll.organization_id;
                            puser.payroll_id = payroll.payroll_id;
                            puser.profile_id = item;
                            puser.created_by = uid;
                            puser.created_date = DateTime.Now;
                            puser.created_ip = ip;
                            db.hrm_payroll_user.Add(puser);
                            db.SaveChanges();
                        }
                        #region add hrm_log
                        if (helper.wlog)
                        {
                            hrm_log log = new hrm_log();
                            log.title = "Thêm bảng lương " + payroll.payroll_name;

                            log.log_module = "payroll";
                            log.log_type = 0;
                            log.id_key = payroll.payroll_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdpayroll, contents }), domainurl + "hrm_payroll/Add_payroll", ip, tid, "Lỗi khi thêm bảng lương", 0, "bảng lương");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdpayroll, contents }), domainurl + "hrm_payroll/Add_payroll", ip, tid, "Lỗi khi thêm bảng lương", 0, "bảng lương  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_hrm_payroll()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdpayroll = "";
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
                        fdpayroll = provider.FormData.GetValues("hrm_payroll").SingleOrDefault();
                        hrm_payroll payroll = JsonConvert.DeserializeObject<hrm_payroll>(fdpayroll);

                        var dasp = db.hrm_payroll.AsNoTracking().Where(a => a.payroll_id !=payroll.payroll_id &&
                        a.payroll_month == payroll.payroll_month && a.payroll_year == payroll.payroll_year).ToList();
                        if (dasp.Count > 0)
                        {

                            foreach (var item in dasp)
                            {

                                var check = item.list_profile_id.Split(',').Intersect(payroll.list_profile_id.Split(','));

                                if (check.Any())
                                {
                                    var str = "";
                                    var strv = "";
                                    foreach (var sitem in check)
                                    {
                                        var user = db.hrm_profile.AsNoTracking().Where(a => a.profile_id == sitem).FirstOrDefault();
                                        str += strv + user.profile_user_name;
                                        strv = ",";
                                    }
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Nhân sự đã được khai báo ở bảng lương khác trong tháng: " + str });
                                }

                            }

                        }

                        if (payroll.is_approved == true)
                        {
                            var payyroll_ap = db.hrm_payroll.Where(a => a.payroll_id != payroll.payroll_id && a.is_approved == true && a.payroll_month == payroll.payroll_month
                            && a.payroll_year == payroll.payroll_year).ToList();
                            foreach (var item in payyroll_ap)
                            {
                                item.is_approved = false;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        payroll.modified_by = uid;
                        payroll.modified_date = DateTime.Now;
                        payroll.modified_ip = ip;
                        payroll.modified_token_id = tid;
                        db.Entry(payroll).State = EntityState.Modified;
                        db.SaveChanges();
                     
                        foreach (var item in payroll.list_profile_id.Split(','))
                        {
                            var das = db.hrm_payroll_user.Where(a => a.payroll_id == payroll.payroll_id && a.profile_id == item).FirstOrDefault();
                            if (das == null)
                            {
                                var puser = new hrm_payroll_user();
                                puser.organization_id = payroll.organization_id;
                                puser.is_data = null;
                                puser.payroll_id = payroll.payroll_id;
                                puser.profile_id = item;
                                puser.created_by = uid;
                                puser.created_date = DateTime.Now;
                                puser.created_ip = ip;
                                db.hrm_payroll_user.Add(puser);
                                db.SaveChanges();
                            }
                        }
                        var dase = db.hrm_payroll_user.Where(a => a.payroll_id == payroll.payroll_id).ToList();
                        foreach (var item in dase)
                        {
                            if (payroll.list_profile_id.Contains(item.profile_id) == false)
                            {

                                db.hrm_payroll_user.Remove(item);
                                db.SaveChanges();
                            }
                        }
                        #region add hrm_log
                        if (helper.wlog)
                        {

                            hrm_log log = new hrm_log();
                            log.title = "Sửa bảng lương " + payroll.payroll_name;
                            log.log_module = "payroll";
                            log.log_type = 1;
                            log.id_key = payroll.payroll_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdpayroll, contents }), domainurl + "hrm_payroll/Update_payroll", ip, tid, "Lỗi khi cập nhật payroll", 0, "payroll");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdpayroll, contents }), domainurl + "hrm_payroll/Update_payroll", ip, tid, "Lỗi khi cập nhật payroll", 0, "payroll");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_hrm_payroll([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.hrm_payroll.Where(a => id.Contains(a.payroll_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<hrm_payroll> del = new List<hrm_payroll>();
                            foreach (var da in das)
                            {

                                del.Add(da);

                                #region add hrm_log
                                if (helper.wlog)
                                {

                                    hrm_log log = new hrm_log();
                                    log.title = "Xóa bảng lương " + da.payroll_name;

                                    log.log_module = "payroll";
                                    log.log_type = 2;
                                    log.id_key = da.payroll_id.ToString();
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
                            db.hrm_payroll.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_payroll/Delete_payroll", ip, tid, "Lỗi khi xoá tem", 0, "payroll");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_payroll/Delete_payroll", ip, tid, "Lỗi khi xoá tem", 0, "payroll");
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
        public async Task<HttpResponseMessage> update_s_hrm_payroll([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                        var das = db.hrm_payroll.Where(a => (a.payroll_id == int_id)).FirstOrDefault<hrm_payroll>();
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
                                log.title = "Sửa bảng lương " + das.payroll_name;

                                log.log_module = "payroll";
                                log.log_type = 1;
                                log.id_key = das.payroll_id.ToString();
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_payroll/Update_Trangthaipayroll", ip, tid, "Lỗi khi cập nhật trạng thái payrolls", 0, "hrm_payroll");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_payroll/Update_Trangthaipayroll", ip, tid, "Lỗi khi cập nhật trạng thái payrolls", 0, "hrm_payroll");
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
        public async Task<HttpResponseMessage> update_a_hrm_payroll([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                        var das = db.hrm_payroll.Where(a => (a.payroll_id == int_id)).FirstOrDefault<hrm_payroll>();
                        if (das != null)
                        {
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            das.is_approved = !trangthai.BitTrangthai;


                            #region add hrm_log
                            if (helper.wlog)
                            {

                                hrm_log log = new hrm_log();
                                log.title = "Sửa bảng lương " + das.payroll_name;

                                log.log_module = "payroll";
                                log.log_type = 1;
                                log.id_key = das.payroll_id.ToString();
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_payroll/Update_Approvedpayroll", ip, tid, "Lỗi khi cập nhật trạng thái payrolls", 0, "hrm_payroll");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_payroll/Update_Approvedpayroll", ip, tid, "Lỗi khi cập nhật trạng thái payrolls", 0, "hrm_payroll");
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