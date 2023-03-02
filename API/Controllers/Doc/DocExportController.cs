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
namespace API.Controllers.Doc
{
    public class DocExportController : ApiController
    {
        [Authorize(Roles = "login")]

        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> getData()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
         
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/");

                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                       
                        string rootFileBlock = Path.Combine(root, "Config/Doc");
                        var strType = provider.FormData.GetValues("type").SingleOrDefault();
                        string jsondocreport = "";
                        if (int.Parse(strType) == 1)
                        {
                            jsondocreport = System.IO.File.ReadAllText(Path.Combine(rootFileBlock, Path.GetFileName("/ReceivedReport.json")));
                        

                        }
                        if (int.Parse(strType) == 2)
                        {
                            jsondocreport = System.IO.File.ReadAllText(Path.Combine(rootFileBlock, Path.GetFileName("/InternalReport.json")));


                        }
                        if (int.Parse(strType) == 3)
                        {
                            jsondocreport = System.IO.File.ReadAllText(Path.Combine(rootFileBlock, Path.GetFileName("/SendReport.json")));


                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { data = jsondocreport, err = "0" });
                    });
                    return await task;

                  

                
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);


                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);

                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> add_doc_export()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_enecting_group = "";
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

                    string root = HttpContext.Current.Server.MapPath("~/");

                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdca_enecting_group = provider.FormData.GetValues("doc_export").SingleOrDefault();
                        docReport doc_report = JsonConvert.DeserializeObject<docReport>(fdca_enecting_group);

                        doc_report.timeExport = DateTime.Now;
                        doc_report.user_id = uid;
                        doc_report.user_name = name;
                        string rootFileBlock = Path.Combine(root, "Config/Doc");
                        if (doc_report.report_type == 1)
                        {
                            string jsondocreport = System.IO.File.ReadAllText(Path.Combine(rootFileBlock, Path.GetFileName("/ReceivedReport.json")));
                            if (jsondocreport != null && jsondocreport != "")
                            {
                                List<docReport> doc_Report_li = JsonConvert.DeserializeObject<List<docReport>>(jsondocreport).OrderByDescending(x => x.timeExport).Take(29).ToList(); ;
                                doc_Report_li.Add(doc_report);
                                doc_Report_li = doc_Report_li.OrderByDescending(x => x.timeExport).ToList();
                                string jsonData = JsonConvert.SerializeObject(doc_Report_li, Formatting.None);
                                //System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath(config.pathBlockIP), jsonData);
                                System.IO.File.WriteAllText(Path.Combine(rootFileBlock, Path.GetFileName("/ReceivedReport.json")), jsonData);
                            }
                            else
                            {
                                List<docReport> doc_Report_li_U = new List<docReport>();
                                doc_Report_li_U.Add(doc_report);
                                string jsonData = JsonConvert.SerializeObject(doc_Report_li_U, Formatting.None);
                                System.IO.File.WriteAllText(Path.Combine(rootFileBlock, Path.GetFileName("/ReceivedReport.json")), jsonData);
                            }
                        }
                        if (doc_report.report_type == 2)
                        {
                            string jsondocreport = System.IO.File.ReadAllText(Path.Combine(rootFileBlock, Path.GetFileName("/InternalReport.json")));
                            if (jsondocreport != null && jsondocreport != "")
                            {
                                List<docReport> doc_Report_li = JsonConvert.DeserializeObject<List<docReport>>(jsondocreport).OrderByDescending(x => x.timeExport).Take(29).ToList(); ;
                                doc_Report_li.Add(doc_report);
                                doc_Report_li = doc_Report_li.OrderByDescending(x => x.timeExport).ToList();
                                string jsonData = JsonConvert.SerializeObject(doc_Report_li, Formatting.None);
                                //System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath(config.pathBlockIP), jsonData);
                                System.IO.File.WriteAllText(Path.Combine(rootFileBlock, Path.GetFileName("/InternalReport.json")), jsonData);
                            }
                            else
                            {
                                List<docReport> doc_Report_li_U = new List<docReport>();
                                doc_Report_li_U.Add(doc_report);
                                string jsonData = JsonConvert.SerializeObject(doc_Report_li_U, Formatting.None);
                                System.IO.File.WriteAllText(Path.Combine(rootFileBlock, Path.GetFileName("/InternalReport.json")), jsonData);
                            }
                        }
                        if (doc_report.report_type == 3)
                        {
                            string jsondocreport = System.IO.File.ReadAllText(Path.Combine(rootFileBlock, Path.GetFileName("/SendReport.json")));
                            if (jsondocreport != null && jsondocreport != "")
                            {
                                List<docReport> doc_Report_li = JsonConvert.DeserializeObject<List<docReport>>(jsondocreport).OrderByDescending(x => x.timeExport).Take(29).ToList(); ;
                                doc_Report_li.Add(doc_report);
                                doc_Report_li = doc_Report_li.OrderByDescending(x => x.timeExport).ToList();
                                string jsonData = JsonConvert.SerializeObject(doc_Report_li, Formatting.None);
                                //System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath(config.pathBlockIP), jsonData);
                                System.IO.File.WriteAllText(Path.Combine(rootFileBlock, Path.GetFileName("/SendReport.json")), jsonData);
                            }
                            else
                            {
                                List<docReport> doc_Report_li_U = new List<docReport>();
                                doc_Report_li_U.Add(doc_report);
                                string jsonData = JsonConvert.SerializeObject(doc_Report_li_U, Formatting.None);
                                System.IO.File.WriteAllText(Path.Combine(rootFileBlock, Path.GetFileName("/SendReport.json")), jsonData);
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_enecting_group, contents }), domainurl + "hrm_ca_enecting_group/Add_ca_enecting_group", ip, tid, "Lỗi khi thêm nhóm đào tạo", 0, "nhóm đào tạo");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_enecting_group, contents }), domainurl + "hrm_ca_enecting_group/Add_ca_enecting_group", ip, tid, "Lỗi khi thêm nhóm đào tạo", 0, "nhóm đào tạo  ");
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
