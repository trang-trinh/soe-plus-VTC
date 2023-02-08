using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using API.Models;
using Helper;
using Newtonsoft.Json;
using System.IO;
using System.Web.UI.WebControls;
using API.Helper;

namespace API.Controllers.Tasks.Person_Report
{
    [Authorize(Roles = "login")]
    public class send_Process_ReportController : ApiController
    {
        public string getipaddress()
        {
            //var host = Dns.GetHostEntry(Dns.GetHostName());tr
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

        public async Task<HttpResponseMessage> addRound()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
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
                        string tempGroup = provider.FormData.GetValues("group").SingleOrDefault();
                        string tempUser = provider.FormData.GetValues("user").SingleOrDefault();
                        string tempreport = provider.FormData.GetValues("report").SingleOrDefault();
                        string group_id = "";
                        string user_id = "";
                        string report = "";
                        if (tempGroup != null)
                        {
                            group_id = JsonConvert.DeserializeObject<string>(tempGroup);
                        }
                        if (tempUser != "")
                        {
                            user_id = JsonConvert.DeserializeObject<string>(tempUser);
                        }
                        if (tempreport != null)
                        {
                            report = JsonConvert.DeserializeObject<string>(tempreport);
                        }
                        string[] listRpID = report.Split(',');
                        foreach (string id in listRpID)
                        {
                            task_person_report_processing rp_Process = new task_person_report_processing();
                            int rpID = int.Parse(id);
                            if (group_id != null)
                            {
                                int grID = int.Parse(group_id);
                                var group = db.task_browse_group.Where(z => z.group_id == grID).FirstOrDefault();
                                string[] userBr = group.user_id.Split(',');
                                List<task_person_report_processing> task_Person_Report_Processings = new List<task_person_report_processing>();
                                foreach (var o in userBr.Select((value, i) => new { i, value }))
                                {
                                    task_person_report_processing obj = new task_person_report_processing();

                                    obj.group_id = group.group_id;
                                    obj.group_role = group.group_role;
                                    var turnRP = db.task_person_report_processing.Where(x => x.report_id == rpID).ToArray().LastOrDefault();
                                    obj.report_id = int.Parse(id);
                                    obj.review_turn = turnRP != null ? turnRP.review_turn + 1 : 1;
                                    obj.user_id = o.value;
                                    obj.is_step = group.group_role == 0 ? o.i + 1 : 1;
                                    if (group.group_role == 0)
                                    {
                                        if (o.i == 0)
                                        {
                                            obj.is_type = -1;
                                        }
                                        else
                                        {
                                            obj.is_type = null;
                                        }
                                    }
                                    else
                                    {
                                        obj.is_type = -1;
                                    }
                                    obj.user_point = null;
                                    obj.user_messages = null;
                                    obj.created_by = uid;
                                    obj.created_date = DateTime.Now;
                                    obj.created_ip = ip;
                                    obj.created_token_id = tid;
                                    task_Person_Report_Processings.Add(obj);
                                }
                                db.task_person_report_processing.AddRange(task_Person_Report_Processings);
                            }
                            else if (user_id != null)
                            {
                                rp_Process.user_id = user_id;
                                rp_Process.group_id = null;
                                rp_Process.group_role = null;
                                var turnRP = db.task_person_report_processing.Where(x => x.report_id == rpID).ToArray().LastOrDefault();
                                rp_Process.report_id = int.Parse(id);
                                rp_Process.review_turn = turnRP != null ? turnRP.review_turn + 1 : 1;
                                rp_Process.is_step = 1;
                                rp_Process.is_type = -1;
                                rp_Process.user_point = null;
                                rp_Process.user_messages = null;
                                rp_Process.created_by = uid;
                                rp_Process.created_date = DateTime.Now;
                                rp_Process.created_ip = ip;
                                rp_Process.created_token_id = tid;
                                db.task_person_report_processing.Add(rp_Process);

                            }
                            var rp = db.task_person_report.Where(x => x.report_id == rpID).FirstOrDefault();
                            rp.status = 1;
                            db.Entry(rp).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                        #region add notify
                        #endregion
                        #region add tasklog
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "" }), domainurl + "send_Process_Report/addRound", ip, tid, "Thêm mới quy trình Đánh giá công việc cá nhân", 1, "Công việc");

                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "send_Process_Report/addRound", ip, tid, "Lỗi khi thêm quy trình xử lý Đánh giá công việc", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "send_Process_Report/addRound", ip, tid, "Lỗi khi thêm quy trình xử lý Đánh giá công việc", 0, "Công việc");
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
    }
}
