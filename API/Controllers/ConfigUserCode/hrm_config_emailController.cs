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
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace API.Controllers.HRM.ConfigUserCode
{
    [Authorize(Roles = "login")]
    public class hrm_config_emailController : ApiController
    {

        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_data()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_certificate = "";
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


                    var dv = int.Parse(dvid);
                    var hrm_Config_Email = db.hrm_config_email.Where(p => p.organization_id == dv).FirstOrDefault();
                    var is_admin = db.sys_users.Where(p => p.organization_id == dv && (p.organization_parent_id == null || p.organization_parent_id == p.organization_id)).FirstOrDefault();

                    if (hrm_Config_Email == null)
                    {
                        var hrm_CogfinUAD = new hrm_config_email();
             
                        hrm_CogfinUAD.organization_id = dv;
                        hrm_CogfinUAD.created_by = uid;
                        hrm_CogfinUAD.created_date = DateTime.Now;
                        hrm_CogfinUAD.created_ip = ip;
                        db.hrm_config_email.Add(hrm_CogfinUAD);
                        db.SaveChanges();

                    }
         
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });

                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_certificate, contents }), domainurl + "ConfigUserCode/update_data", ip, tid, "Lỗi khi cập nhật Mã nhân sự", 0, "Mã nhân sự");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_certificate, contents }), domainurl + "ConfigUserCode/update_data", ip, tid, "Lỗi khi cập nhật Mã nhân sự", 0, "Mã nhân sự  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


        [HttpPut]
        public async Task<HttpResponseMessage> update_config_email()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_certificate = "";
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
                        fdca_certificate = provider.FormData.GetValues("hrm_config_email").SingleOrDefault();


                        hrm_config_email hrm_Config_Email = JsonConvert.DeserializeObject<hrm_config_email>(fdca_certificate);


                        string depass = BCrypt.Net.BCrypt.HashPassword(hrm_Config_Email.email_pasw);
                        hrm_Config_Email.email_pasw = depass;
                        hrm_Config_Email.key_encript = Convert.ToBase64String(Encoding.UTF8.GetBytes(helper.psKey));
                        hrm_Config_Email.modified_by = uid;
                        hrm_Config_Email.modified_date = DateTime.Now;
                        hrm_Config_Email.modified_ip = ip;
                        db.Entry(hrm_Config_Email).State = EntityState.Modified;
                        db.SaveChanges();

                        #region add hrm_log
                        if (helper.wlog)
                        {

                            hrm_log log = new hrm_log();
                            log.title = "Cập nhật mã nhân sự công ty ";
                            log.log_module = "hrm_config_usercode";
                            log.log_type = 1;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_certificate, contents }), domainurl + "ConfigUserCode/update_data", ip, tid, "Lỗi khi cập nhật Mã nhân sự", 0, "Mã nhân sự");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_certificate, contents }), domainurl + "ConfigUserCode/update_data", ip, tid, "Lỗi khi cập nhật Mã nhân sự", 0, "Mã nhân sự  ");
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