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
using API.Helper;
using System.Threading;
using System.Net.Mail;
using System.IO;

namespace API.Controllers
{
    public class SendEmailController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> sendEMail()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string email = "";
            string domainName = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            string json = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Config/Config.json"));
            settings deJson = JsonConvert.DeserializeObject<settings>(json);

            //string pwEm = Codec.DecryptString(deJson.pwEmail, helper.psKey);
            //if (deJson.email == null || pwEm == null)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { err = "-1", ms = "Hệ thống chưa có Email.<br> Vui lòng liên hệ hỗ trợ để cài đặt." });
            //}
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
                    var task = Request.Content.ReadAsMultipartAsync(provider).ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        string strPwMail = provider.FormData.GetValues("pwMail").SingleOrDefault();
                        if (deJson.email == null || strPwMail == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "-1", ms = "Hệ thống chưa có Email.<br> Vui lòng liên hệ hỗ trợ để cài đặt." });
                        }
                        string dePwMail = Codec.DecryptString(strPwMail, helper.psKey);
                        if (strPwMail != deJson.email)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "-1", ms = "Thông tin Email hệ thống chưa chính xác.<br> Vui lòng liên hệ hỗ trợ để cài đặt." });
                        }

                        string receiver = provider.FormData.GetValues("mailinfo").SingleOrDefault();
                        MailInfo mailInfo = JsonConvert.DeserializeObject<MailInfo>(receiver);
                        email = db.sys_users.Where(x => x.user_id == mailInfo.to).Select(x => x.email).FirstOrDefault();
                        if (email == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "-1", ms = "Người đánh giá không có email." });
                        }
                        else
                        {
                            Thread T1 = new Thread(delegate ()
                        {
                            var message = new MailMessage();
                            message.From = new MailAddress(deJson.email, mailInfo.display_name); ;
                            message.To.Add(new MailAddress(email));
                            message.Subject = mailInfo.subject;
                            message.Body = mailInfo.body;
                            message.IsBodyHtml = mailInfo.isBodyHtml;
                            SmtpClient smtp = new SmtpClient();
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtp.UseDefaultCredentials = true;
                            smtp.EnableSsl = true;
                            smtp.Host = "smtp.gmail.com";
                            smtp.Port = 587;
                            smtp.Credentials = new NetworkCredential(deJson.email, dePwMail);
                            smtp.Send(message);
                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = message.Subject }), domainurl + "SendEmailController/add_group", ip, tid, uid + " gửi Email", 0, "SendEmailController");
                        });
                            T1.Start();
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                        }
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "SendEmailController/add_group", ip, tid, "Lỗi khi gửi mail ", 0, "SendEmailController");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "SendEmailController/add_group", ip, tid, "Lỗi khi gửi mail ", 0, "SendEmailController");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpGet]
        public HttpResponseMessage GetConfigMail()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                string json = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Config/Config.json"));
                var dataSetting = JsonConvert.DeserializeObject<settings>(json);
                InfoEmail infoMail = new InfoEmail();
                infoMail.email = dataSetting.email != null ? Codec.EncryptString(dataSetting.email, helper.psKey) : null;
                infoMail.kpmail = dataSetting.pwEmail;
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = infoMail });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "SendEmail/GetConfigMail", ip, tid, "Lỗi khi lấy thông tin config email.", 0, "SendEmail");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        public class InfoEmail
        {
            public string email { get; set; }
            public string kpmail { get; set; }
        }
    }
}
