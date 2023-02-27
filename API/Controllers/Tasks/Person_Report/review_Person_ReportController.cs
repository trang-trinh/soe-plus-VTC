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
    public class review_Person_ReportController : ApiController
    {
        public string getipaddress()
        {
            //var host = Dns.GetHostEntry(Dns.GetHostName());
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

        public async Task<HttpResponseMessage> Processing()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            int dvidInt = Convert.ToInt32(dvid);
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
                        ///Get data from FrontEnd
                        string tempID = provider.FormData.GetValues("id").SingleOrDefault();
                        string tempMessages = provider.FormData.GetValues("message").SingleOrDefault();
                        string tempPoint = provider.FormData.GetValues("point").SingleOrDefault();
                        string tempType = provider.FormData.GetValues("type").SingleOrDefault();
                        string listprocess_id = JsonConvert.DeserializeObject<string>(tempID);
                        string messages = JsonConvert.DeserializeObject<string>(tempMessages);
                        int type = Convert.ToInt32(JsonConvert.DeserializeObject<string>(tempType));
                        int point = 0;
                        if (tempPoint != null)
                        {
                            point = Convert.ToInt32(JsonConvert.DeserializeObject<string>(tempPoint));
                        }
                        string[] list_process_id = listprocess_id.Split(',');
                        foreach (string process in list_process_id)
                        {  
                        
                            int process_id = Convert.ToInt32(process);
                
                            var Report = db.task_person_report_processing.Where(x => x.process_id == process_id).FirstOrDefault();           
                            string recei = Report.created_by;
                            helper.saveNotify(uid, recei, null, "Công việc", "Đã xử lý báo cáo đánh giá công việc trong Đánh giá công việc",
                                    null, 12, -1, false, "M4", "", null, null, tid, ip);//12 chạy ra duyệt đánh giá
                            Report.user_messages = messages;
                            Report.user_point = point;
                            Report.is_type = type;
                            Report.modified_by = uid;
                            Report.modified_date = DateTime.Now;
                            Report.modified_ip = ip;
                            Report.modified_token_id = tid;
                            var person_report = db.task_person_report.Where(x => x.report_id == Report.report_id).FirstOrDefault();
                            ///Personal
                            #region Personal
                            if (Report.group_id == null && Report.group_role == null)
                            {
                                if (type == 0)
                                {
                                    //duyệt
                                    person_report.status = 2;
                                    person_report.reviewed_point = point;
                                }
                                else if (type == 1)
                                {
                                    //trả lại
                                    person_report.status = 3;
                                    person_report.reviewed_point = point;
                                }
                            }
                            #endregion
                            #region Group
                            else
                            {
                                #region Tuần tự
                                if (Report.group_role == 0)
                                {
                                    if (type == 0)
                                    {
                                        //duyệt
                                        int nextStep = Convert.ToInt32(Report.is_step) + 1;
                                        var nextRound = db.task_person_report_processing.Where(x => x.report_id == Report.report_id
                                            && x.group_id == Report.group_id && x.group_role == Report.group_role
                                            && x.is_step == nextStep && x.review_turn == Report.review_turn).FirstOrDefault();
                                        if (nextRound != null)
                                        {
                                            nextRound.is_type = -1;
                                            db.Entry(nextRound).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            var howtomark = db.Task_Marks.Where(x => x.organization_id == dvidInt).FirstOrDefault();
                                            if (howtomark.HowToCalcMark == false)
                                            {
                                                var list = db.task_person_report_processing.Where(x => x.report_id == Report.report_id
                                                                                                      && x.group_id == Report.group_id && x.group_role == Report.group_role
                                                                                                      && x.review_turn == Report.review_turn && x.user_point != null).ToList();
                                                double total = 0;
                                                foreach (var l in list)
                                                {
                                                    total += Convert.ToInt32(l.user_point);
                                                }
                                                total += point;
                                                double avg = (total) / (list.Count + 1);
                                                person_report.reviewed_point = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(avg)));
                                            }
                                            else
                                            {
                                                person_report.reviewed_point = point;
                                            }
                                            person_report.status = 2;
                                        }
                                    }
                                    else if (type == 1)
                                    {
                                        //trả lại
                                        var list = db.task_person_report_processing.Where(x => x.report_id == Report.report_id
                                                                                                     && x.group_id == Report.group_id && x.group_role == Report.group_role
                                                                                                     && x.review_turn == Report.review_turn && x.is_step > Report.is_step).ToList();
                                        foreach (var l in list)
                                        {
                                            l.is_type = 2;
                                            db.Entry(l).State = EntityState.Modified;
                                        }
                                        person_report.status = 3;
                                        person_report.reviewed_point = null;
                                    }
                                }
                                #endregion
                                #region Ngẫu nhiên
                                else if (Report.group_role == 2)
                                {
                                    if (type == 0)
                                    {
                                        //duyệt
                                        var nextRound = db.task_person_report_processing.Where(x => x.report_id == Report.report_id
                                            && x.group_id == Report.group_id && x.group_role == Report.group_role
                                            && x.user_point == null && x.review_turn == Report.review_turn).ToList();
                                        if (nextRound.Count < 2)
                                        {
                                            var howtomark = db.Task_Marks.Where(x => x.organization_id == dvidInt).FirstOrDefault();
                                            if (howtomark.HowToCalcMark == false)
                                            {
                                                var list = db.task_person_report_processing.Where(x => x.report_id == Report.report_id
                                                                                                      && x.group_id == Report.group_id && x.group_role == Report.group_role
                                                                                                      && x.review_turn == Report.review_turn && x.user_point != null).ToList();
                                                double total = 0;
                                                foreach (var l in list)
                                                {
                                                    total += Convert.ToInt32(l.user_point);
                                                }
                                                total += point;
                                                double avg = (total) / (list.Count + 1);
                                                person_report.reviewed_point = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(avg)));
                                            }
                                            else
                                            {
                                                person_report.reviewed_point = point;
                                            }
                                            person_report.status = 2;
                                        }
                                    }
                                    else if (type == 1)
                                    {
                                        //trả lại
                                        var list = db.task_person_report_processing.Where(x => x.report_id == Report.report_id
                                                                                                     && x.group_id == Report.group_id && x.group_role == Report.group_role
                                                                                                     && x.review_turn == Report.review_turn && x.is_step > Report.is_step).ToList();
                                        foreach (var l in list)
                                        {
                                            l.is_type = 2;
                                            db.Entry(l).State = EntityState.Modified;
                                        }
                                        person_report.status = 3;
                                        person_report.reviewed_point = null;
                                    }
                                }
                                #endregion
                                #region Một nhiều
                                else if (Report.group_role == 1)
                                {
                                    if (type == 0)
                                    {
                                        //duyệt
                                        var nextRound = db.task_person_report_processing.Where(x => x.report_id == Report.report_id
                                            && x.group_id == Report.group_id && x.group_role == Report.group_role
                                            && x.user_point == null && x.review_turn == Report.review_turn && x.process_id != Report.process_id).ToList();
                                        foreach (var l in nextRound)
                                        {
                                            l.is_type = 2;
                                            db.Entry(l).State = EntityState.Modified;
                                        }
                                        person_report.status = 2;
                                        person_report.reviewed_point = point;
                                    }
                                    else if (type == 1)
                                    {
                                        //trả lại
                                        var list = db.task_person_report_processing.Where(x => x.report_id == Report.report_id
                                                                                                     && x.group_id == Report.group_id && x.group_role == Report.group_role
                                                                                                     && x.review_turn == Report.review_turn && x.is_step > Report.is_step).ToList();
                                        foreach (var l in list)
                                        {
                                            l.is_type = 2;
                                            db.Entry(l).State = EntityState.Modified;
                                        }
                                        person_report.status = 3;
                                        person_report.reviewed_point = null;
                                    }
                                }
                                #endregion
                            }
                            db.Entry(Report).State = EntityState.Modified;
                            db.Entry(person_report).State = EntityState.Modified;
                            #endregion
                        }
                        db.SaveChanges();
                        #region add notify
                   
                        #endregion
                        #region add tasklog
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = uid }), domainurl + "review_Person_Report/Processing", ip, tid, "Xử lý báo cáo Đánh giá công việc", 1, "Công việc");

                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "review_Person_Report/Processing", ip, tid, "Lỗi khi Xử lý báo cáo Đánh giá công việc", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "review_Person_Report/Processing", ip, tid, "Lỗi khi Xử lý báo cáo Đánh giá công việc", 0, "Công việc");
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
    }
}
