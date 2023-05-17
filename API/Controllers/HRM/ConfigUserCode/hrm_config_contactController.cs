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

namespace API.Controllers.HRM.ConfigUserCode
{
    public class hrm_config_contactController : ApiController
    {
        [Authorize(Roles = "login")]

 
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
                    var year = DateTime.Now.Year;
                    var Superior = db.hrm_config_contact.AsNoTracking().Where(p => (p.organization_id == dv && p.year == year) || p.is_active==true).FirstOrDefault();
                    if (Superior == null)
                    {
                        var hrm_Cogfin = new hrm_config_contact();
                        hrm_Cogfin.is_date_of_birth = false;
                        hrm_Cogfin.is_phone_number = false;
                        hrm_Cogfin.year = year;
                        hrm_Cogfin.is_active = true;
                        hrm_Cogfin.organization_id = dv;
                        hrm_Cogfin.created_by = uid;
                        hrm_Cogfin.created_date = DateTime.Now;
                        hrm_Cogfin.created_ip = ip;
                        db.hrm_config_contact.Add(hrm_Cogfin);
                        db.SaveChanges();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_certificate, contents }), domainurl + "hrm_config_contact/update_data", ip, tid, "Lỗi khi cập nhật mã số hợp đồng", 0, "mã số hợp đồng");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_certificate, contents }), domainurl + "hrm_config_contact/update_data", ip, tid, "Lỗi khi cập nhật mã số hợp đồng", 0, "mã số hợp đồng");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


        [HttpPut]
        public async Task<HttpResponseMessage> update_config_contact()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

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
                        string fdca_certificate = "";
                        var dv = int.Parse(dvid);
                        var year = DateTime.Now.Year;
                        fdca_certificate = provider.FormData.GetValues("hrm_config_contact").SingleOrDefault();
                        hrm_config_contact hrm_Config_Usercode = JsonConvert.DeserializeObject<hrm_config_contact>(fdca_certificate);
                        var checkCer = db.hrm_config_contact.AsNoTracking().Where(p => p.organization_id == dv && p.year == hrm_Config_Usercode.year ).FirstOrDefault();

                        var checkActive = db.hrm_config_contact.Where(p => p.organization_id == dv && p.is_active == true && p.config_contact_id != hrm_Config_Usercode.config_contact_id).FirstOrDefault();
                        if (checkActive != null)
                        {
                            checkActive.is_active = false;
                            db.SaveChanges();
                        }
                        if (checkCer == null)
                        {
                            var hrm_Cogfin = new hrm_config_contact();
                            hrm_Config_Usercode.is_active = true;
                            hrm_Config_Usercode.organization_id = dv;
                            hrm_Config_Usercode.created_by = uid;
                            hrm_Config_Usercode.created_date = DateTime.Now;
                            hrm_Config_Usercode.created_ip = ip;
                            db.hrm_config_contact.Add(hrm_Config_Usercode);
                            db.SaveChanges();

                        }
                        else
                        {
                            hrm_Config_Usercode.is_active = true;
                            hrm_Config_Usercode.created_by = uid;
                            hrm_Config_Usercode.created_date = DateTime.Now;
                            hrm_Config_Usercode.created_ip = ip;
                            db.Entry(hrm_Config_Usercode).State = EntityState.Modified;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "config_contact/update_config_contact", ip, tid, "Lỗi khi cập nhật Số hợp đồng", 0, "Số hợp đồng");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "config_contact/update_config_contact", ip, tid, "Lỗi khi cập nhật Số hợp đồng", 0, "Số hợp đồng");
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