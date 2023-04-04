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
    [Authorize(Roles = "login")]
    public class hrm_config_usercodeController : ApiController
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
                 var dv= int.Parse(dvid );
                    var Superior = db.hrm_config_usercode.Where(p => p.organization_type == 3 && p.organization_id == dv).FirstOrDefault();
                    if (Superior == null)
                    {
                        var hrm_CogfinUAD = new hrm_config_usercode();
                        hrm_CogfinUAD.organization_name = "Mã cấp trên";
                        hrm_CogfinUAD.symbol = "";
                   
                        hrm_CogfinUAD.organization_id =dv;
                        hrm_CogfinUAD.organization_type = 3;
                        hrm_CogfinUAD.is_order = 0;
                        hrm_CogfinUAD.status =true;
                        hrm_CogfinUAD.created_by =uid;
                        hrm_CogfinUAD.created_date = DateTime.Now;
                        hrm_CogfinUAD.created_ip =ip;
                        hrm_CogfinUAD.created_token_id =tid;
                        db.hrm_config_usercode.Add(hrm_CogfinUAD);
                        db.SaveChanges();
                       
                    }
                    var ListOrganization = db.sys_organization.Where(p => (p.organization_id == dv || p.parent_id == dv) && p.organization_type==0).OrderBy(x=>x.is_order).ToList();
                    var i = 1;
                    foreach (var item in ListOrganization)
                    {
                        var SuperiorCheck = db.hrm_config_usercode.AsNoTracking().Where(p => p.organization_type == 3 && p.organization_id == dv).FirstOrDefault();
                        var itemAdd = db.hrm_config_usercode.Where(p => p.organization_type == 0 && p.organization_id == item.organization_id).FirstOrDefault();
                        if (itemAdd == null)
                        {
                            var hrm_Cogfin = new hrm_config_usercode();
                            hrm_Cogfin.organization_name =item.organization_name;
                            hrm_Cogfin.organization_id = item.organization_id;
                            hrm_Cogfin.parent_id = item.parent_id;
                            hrm_Cogfin.organization_type = 0;
                            hrm_Cogfin.is_order = i;
                            hrm_Cogfin.status = true;
                            hrm_Cogfin.created_by = uid;
                            hrm_Cogfin.created_ip = ip;
                            hrm_Cogfin.created_token_id = tid;
                            db.hrm_config_usercode.Add(hrm_Cogfin);
                            db.SaveChanges();
                            i++;
                        }


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
        public async Task<HttpResponseMessage> update_config_usercode()
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
                        fdca_certificate = provider.FormData.GetValues("hrm_config_usercode").SingleOrDefault();
                      List<hrm_config_usercode> hrm_Config_Usercode = JsonConvert.DeserializeObject<List<hrm_config_usercode>>(fdca_certificate);

                         
                        foreach (var item in hrm_Config_Usercode)
                        {
                            item.modified_by = uid;
                            item.modified_date = DateTime.Now;
                            item.modified_ip = ip;
                            item.modified_token_id = tid;
                            db.Entry(item).State = EntityState.Modified;
                           
                        }
                        #region add hrm_log
                        if (helper.wlog)
                        {

                            hrm_log log = new hrm_log();
                            log.title = "Cập nhật mã nhân sự công ty " ;
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