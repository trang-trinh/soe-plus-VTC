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

namespace API.Controllers.HRM.Payroll
{
    [Authorize(Roles = "login")]
    public class hrm_payroll_userController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_payroll_user()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdhrm_payroll_user = "";
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
                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";

                        fdhrm_payroll_user = provider.FormData.GetValues("hrm_payroll_user").SingleOrDefault();
                        hrm_payroll_user hrm_payroll_user = JsonConvert.DeserializeObject<hrm_payroll_user>(fdhrm_payroll_user);



                        hrm_payroll_user.organization_id = int.Parse(dvid);
                        hrm_payroll_user.created_by = uid;
                        hrm_payroll_user.created_date = DateTime.Now;
                        hrm_payroll_user.created_ip = ip;
                        hrm_payroll_user.created_token_id = tid;
                        db.hrm_payroll_user.Add(hrm_payroll_user);
                        db.SaveChanges();


                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_payroll_user, contents }), domainurl + "hrm_payroll_user/Add_hrm_payroll_user", ip, tid, "Lỗi khi thêm cấp nhân sự", 0, "cấp nhân sự");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_payroll_user, contents }), domainurl + "hrm_payroll_user/Add_hrm_payroll_user", ip, tid, "Lỗi khi thêm cấp nhân sự", 0, "cấp nhân sự  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_li_hrm_payroll_user()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdhrm_payroll_user = "";
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
                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";

                        fdhrm_payroll_user = provider.FormData.GetValues("hrm_payroll_user").SingleOrDefault();
                        List<hrm_payroll_user> hrm_payroll_user = JsonConvert.DeserializeObject<List<hrm_payroll_user>>(fdhrm_payroll_user);
                        var idc = 1;
                         
                        foreach (var item in hrm_payroll_user)
                        {

                            idc = item.payroll_id;
                         
                            item.organization_id = int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        var dass = db.hrm_payroll_user.Where(a => a.payroll_id == idc && a.is_data == null).ToArray();
                        if (dass.Length > 0)
                        {
                            db.hrm_payroll_user.RemoveRange(dass);
                            db.SaveChanges();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_payroll_user, contents }), domainurl + "hrm_payroll_user/Add_hrm_payroll_user", ip, tid, "Lỗi khi thêm cấp nhân sự", 0, "cấp nhân sự");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_payroll_user, contents }), domainurl + "hrm_payroll_user/Add_hrm_payroll_user", ip, tid, "Lỗi khi thêm cấp nhân sự", 0, "cấp nhân sự  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


        [HttpPut]
        public async Task<HttpResponseMessage> update_hrm_payroll_user()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdhrm_payroll_user = "";
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
                        fdhrm_payroll_user = provider.FormData.GetValues("hrm_payroll_user").SingleOrDefault();
                        hrm_payroll_user hrm_payroll_user = JsonConvert.DeserializeObject<hrm_payroll_user>(fdhrm_payroll_user);






                        hrm_payroll_user.modified_by = uid;
                        hrm_payroll_user.modified_date = DateTime.Now;
                        hrm_payroll_user.modified_ip = ip;
                        hrm_payroll_user.modified_token_id = tid;
                        db.Entry(hrm_payroll_user).State = EntityState.Modified;
                        db.SaveChanges();



                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_payroll_user, contents }), domainurl + "hrm_payroll_user/Update_hrm_payroll_user", ip, tid, "Lỗi khi cập nhật hrm_payroll_user", 0, "hrm_payroll_user");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_payroll_user, contents }), domainurl + "hrm_payroll_user/Update_hrm_payroll_user", ip, tid, "Lỗi khi cập nhật hrm_payroll_user", 0, "hrm_payroll_user");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_hrm_payroll_user([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.hrm_payroll_user.Where(a => id.Contains(a.payroll_user_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<hrm_payroll_user> del = new List<hrm_payroll_user>();
                            foreach (var da in das)
                            {

                                del.Add(da);



                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_payroll_user.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_payroll_user/Delete_hrm_payroll_user", ip, tid, "Lỗi khi xoá tem", 0, "hrm_payroll_user");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_payroll_user/Delete_hrm_payroll_user", ip, tid, "Lỗi khi xoá tem", 0, "hrm_payroll_user");
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