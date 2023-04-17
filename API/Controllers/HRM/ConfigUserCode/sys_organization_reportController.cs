using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data;
using API.Helper;
using System.Security.Cryptography;
using System.Data.Entity;
using System.Web.UI.WebControls;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class sys_organization_reportController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> AddReportForm()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
                        ///Get data from FrontEnd
                        string tempID = provider.FormData.GetValues("sys_organization_report").SingleOrDefault();
                        List<sys_organization_report> sys_Organization_Report = JsonConvert.DeserializeObject<List<sys_organization_report>>(tempID);
                        List<sys_organization_report> list = new List<sys_organization_report>();
                        foreach(var item in sys_Organization_Report)
                        {
                            item.created_by=uid; 
                            item.created_date = DateTime.Now;   
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            list.Add(item);
                            if (helper.wlog)
                            {
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = item, }), domainurl + "/sys_organization_report/AddReportForm",
                                    ip, tid, "Thêm mới mẫu báo cáo", 1, "HRM");
                            }
                        }
                        if (list.Count > 0)
                        {
                            db.sys_organization_report.AddRange(list);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "sys_organization_report/AddReportForm", ip, tid, "Lỗi khi thêm mẫu báo cáo", 0, "HRM");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "sys_organization_report/AddReportForm", ip, tid, "Lỗi khi thêm mãu báo cáo", 0, "HRM");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateReportForm()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
                        ///Get data from FrontEnd
                        string tempID = provider.FormData.GetValues("sys_organization_report").SingleOrDefault();
                        List<sys_organization_report> sys_Organization_Report = JsonConvert.DeserializeObject<List<sys_organization_report>>(tempID);
                        List<sys_organization_report> list = new List<sys_organization_report>();
                        foreach (var item in sys_Organization_Report)
                        {
                            item.modified_by = uid;
                            item.modified_date = DateTime.Now;
                            item.modified_ip = ip;
                            item.modified_token_id = tid;
                            if (helper.wlog)
                            {
                                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = item, }), domainurl + "/sys_organization_report/UpdateReportForm",
                                    ip, tid, "Cập nhật mẫu báo cáo", 1, "HRM");
                            }
                            db.Entry(item).State = EntityState.Modified;
                        }
                        if (sys_Organization_Report.Count> 0)
                        {
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "sys_organization_report/UpdateReportForm", ip, tid, "Lỗi khi cập nhật mẫu báo cáo", 0, "HRM");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "sys_organization_report/UpdateReportForm", ip, tid, "Lỗi khi cập nhật mãu báo cáo", 0, "HRM");
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
