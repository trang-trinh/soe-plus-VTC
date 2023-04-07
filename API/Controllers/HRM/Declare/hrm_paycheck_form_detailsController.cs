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

namespace API.Controllers.HRM.Declare
{
    [Authorize(Roles = "login")]
    public class hrm_paycheck_form_detailsController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_paycheck_form_details()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdpaycheck_form_details = "";
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
                        fdpaycheck_form_details = provider.FormData.GetValues("hrm_paycheck_form_details").SingleOrDefault();
                        List< hrm_paycheck_form_details> paycheck_form_details = JsonConvert.DeserializeObject<List<hrm_paycheck_form_details>>(fdpaycheck_form_details);
                        var objx = provider.FormData.GetValues("hrm_paycheck_form").SingleOrDefault();
                        hrm_paycheck_form hrm_Paycheck_form = JsonConvert.DeserializeObject<hrm_paycheck_form>(objx);

                        var intx = paycheck_form_details[0].paycheck_id;


                        var das = db.hrm_paycheck_form_details.AsNoTracking().Where(x => x.paycheck_id == intx  && x.paycheck_form_id
                         == hrm_Paycheck_form.paycheck_form_id).FirstOrDefault();
                      if(das==null)
                            foreach (var item in paycheck_form_details)
                        {
           
                            item.organization_id =  int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_paycheck_form_details.Add(item);
                            db.SaveChanges();
                        }
                      else
                            foreach (var item in paycheck_form_details)
                            {

                                item.created_by = uid;
                                item.created_date = DateTime.Now;
                                item.created_ip = ip;
                                item.created_token_id = tid;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        //#region add hrm_log
                        //if (helper.wlog)
                        //{

                        //    hrm_log log = new hrm_log();
                        //    log.title = "Thêm Chỉ tiêu chi tiết " + paycheck_form_details.paycheck_name;

                        //    log.log_module = "paycheck_form_details";
                        //    log.log_type = 0;
                        //    log.id_key = paycheck_form_details.paycheck_form_details_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.hrm_log.Add(log);
                        //    db.SaveChanges();

                        //}
                        //#endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdpaycheck_form_details, contents }), domainurl + "hrm_paycheck_form_details/Add_paycheck_form_details", ip, tid, "Lỗi khi thêm Chỉ tiêu chi tiết", 0, "Chỉ tiêu chi tiết");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdpaycheck_form_details, contents }), domainurl + "hrm_paycheck_form_details/Add_paycheck_form_details", ip, tid, "Lỗi khi thêm Chỉ tiêu chi tiết", 0, "Chỉ tiêu chi tiết  ");
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