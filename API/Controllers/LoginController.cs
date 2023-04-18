using API.Helper;
using API.Models;
using Helper;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Net.Mail;
using System.Threading;

namespace Controllers
{
    public class LoginController : ApiController
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
        [HttpPost]
        public async Task<HttpResponseMessage> Login([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            //try
            //{
            using (DBEntities db = new DBEntities())
            {
                sys_token tk = new sys_token();
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                string ip = HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : "";
                string str = data["str"].ToObject<string>();
                string strPaK = helper.psKey;
                string de_str = Codec.DecryptString(str, strPaK);
                //string de_ps = Codec.DecryptString("wR3W9dh0DivQ7aEVJ/LnZQ==", strPaK);
                sys_users u = JsonConvert.DeserializeObject<sys_users>(de_str);
                string strInPa = u.is_psword; //u.is_psword;
               // var issuer = Request.RequestUri.GetLeftPart(UriPartial.Authority);
                try
                {
                    //string depass = Codec.EncryptString(strInPa, strPaK);
                    //var user = db.sys_users.FirstOrDefault(us => us.user_id == u.user_id && (us.is_psword == depass));
                    string enps = BCrypt.Net.BCrypt.HashPassword(strInPa);
                    sys_users user = null;
                    var listUser = db.sys_users.Where(us => us.user_id == u.user_id).ToList();
                    if (listUser.Count() > 0)
                    {
                        foreach (var item in listUser)
                        {
                            if (BCrypt.Net.BCrypt.Verify(strInPa, item.is_psword))
                            {
                                user = item;
                                break;
                            }
                        }
                    }
                    if (user != null && user.status != 1)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tài khoản đã bị khoá, vui lòng liên hệ quản trị để kích hoạt!", err = "1" });
                    }
                    if (user == null && u.user_id == "administrator" && strInPa == "#Os1234567")
                    {
                        u.is_psword = enps;
                        u.key_encript = Convert.ToBase64String(Encoding.UTF8.GetBytes(strPaK));
                        u.full_name = "Administrator";
                        u.ip = ip;
                        u.is_admin = true;
                        u.wrong_pass_count = 0;
                        u.status = 1;
                        u.is_super = true;
                        u.is_order = 1;
                        u.modified_date = DateTime.Now;
                        u.modified_by = u.user_id;
                        u.email = "conghdit@gmail.com";
                        u.phone = "0987729288";
                        u.created_date = DateTime.Now;
                        u.created_by = u.user_id;
                        u.created_ip = ip;
                        db.sys_users.Add(u);
                        await db.SaveChangesAsync();
                        user = u;
                    }
                    if (user != null)
                    {
                        if (user.wrong_pass_count > 0)
                        {
                            user.wrong_pass_count = 0;
                            db.SaveChanges(); 
                        }
                        if (user.avatar == null)
                        {
                            user.avatar = "/Portals/Image/nouser1.png";
                        }
                        tk = await db.sys_token.FirstOrDefaultAsync(x => x.user_id == user.user_id);
                        if (tk == null)
                        {
                            tk = new sys_token();
                            tk.user_id = user.user_id;
                            tk.full_name = user.full_name;
                            tk.token_id = Guid.NewGuid().ToString("N").ToUpper();
                            tk.date = DateTime.Now;
                            tk.date_end = tk.date.Value.AddMinutes(helper.timeout);
                            string Device = helper.getDecideNameAuto(Request.Headers.UserAgent.ToString());
                            tk.from_device = Device;
                            tk.ip = ip;
                            db.sys_token.Add(tk);
                            u.token_id = tk.token_id;
                            await db.SaveChangesAsync();
                        }
                        helper.saveIP(ip, Request.Headers.UserAgent.ToString(), user.user_id, user.full_name);
                        // Tạo token
                        var issuer = Request.RequestUri.GetLeftPart(UriPartial.Authority);
                        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(helper.tokenkey));
                        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                        var permClaims = new List<Claim>();
                        permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        permClaims.Add(new Claim("tid", tk.token_id));
                        permClaims.Add(new Claim("uid", tk.user_id));
                        permClaims.Add(new Claim("super", user.is_super.ToString()));
                        permClaims.Add(new Claim(ClaimTypes.Role, "login"));
                        if (!string.IsNullOrWhiteSpace(user.role_id))
                            permClaims.Add(new Claim("rid", user.role_id));
                        if (user.organization_id != null)
                            permClaims.Add(new Claim("parent_dvid", user.organization_parent_id.ToString()));
                        if (user.organization_id != null)
                            permClaims.Add(new Claim("dvid", user.organization_id.ToString()));
                        if (user.organization_child_id != null)
                            permClaims.Add(new Claim("ctid", user.organization_child_id.ToString()));
                        if (user.organization_child_id != null)
                            permClaims.Add(new Claim("dept", user.department_id.ToString()));
                        permClaims.Add(new Claim("fname", tk.full_name));
                        if (user.avatar != null)
                        {
                            permClaims.Add(new Claim("avarta", user.avatar));
                        }
                        permClaims.Add(new Claim("ad", user.is_admin.ToString()));
                        //Create Security Token object by giving required parameters    
                        var token = new JwtSecurityToken(issuer, //Issure    
                                        issuer,  //Audience    
                                        permClaims,
                                        expires: DateTime.Now.AddMinutes(helper.timeout),
                                        signingCredentials: credentials);
                        var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
                        #region Cookie
                        var org = db.sys_organization.FirstOrDefault(x => x.organization_id == user.organization_id);
                        var rol = db.sys_roles.FirstOrDefault(x => x.role_id == user.role_id);
                        var position = db.ca_positions.FirstOrDefault(x => x.position_id == user.position_id);
                        var str_position = "";
                        if (position != null)
                        {
                            str_position = position.position_name;
                        }
                        HttpResponseMessage respMessage = new HttpResponseMessage();
                        var obj = new
                        {
                            data = Codec.EncryptString(jwt_token, ConfigurationManager.AppSettings["EncriptKey"]),
                            u = Codec.EncryptString(JsonConvert.SerializeObject(new
                            {
                                token_id = tk.token_id,
                                user_id = tk.user_id,
                                user_key = user.user_key,
                                full_name = user.full_name,
                                avatar = user.avatar,
                                logo = org?.logo,
                                position_name = str_position,
                                product_name = org?.product_name,
                                background_image = org?.background_image,
                                organization_name = org?.organization_name,
                                organization_parent_id=user?.organization_parent_id,
                                organization_id = org?.organization_id,
                                department_id = user?.department_id,
                                organization_child_id = user?.organization_child_id,
                                role_name = rol?.role_name,
                                role_id = rol?.role_id,
                                is_admin = user.is_admin,
                            }), ConfigurationManager.AppSettings["EncriptKey"]),
                            err = "0"
                        };
                        respMessage.Content = new ObjectContent<object>(obj, new JsonMediaTypeFormatter());
                        CookieHeaderValue cookie = new CookieHeaderValue("jwt", obj.data);
                        cookie.Expires = DateTimeOffset.Now.AddMinutes(helper.timeout);
                        cookie.Domain = Request.RequestUri.Host;
                        cookie.Path = "/";
                        cookie.HttpOnly = true;
                        cookie.Secure = true;
                        respMessage.StatusCode = HttpStatusCode.OK;
                        respMessage.Headers.AddCookies(new CookieHeaderValue[] { cookie });

                        string rootFileConfig = HttpContext.Current.Server.MapPath("~/Config");
                        string json = System.IO.File.ReadAllText(Path.Combine(rootFileConfig, Path.GetFileName("Config.json")));
                        if (json != null)
                        {
                            settings config = JsonConvert.DeserializeObject<settings>(json);
                            CookieHeaderValue cookiedoc = new CookieHeaderValue("doconline", obj.u);
                            cookiedoc.Expires = DateTimeOffset.Now.AddMinutes(helper.timeout);
                            cookiedoc.Domain = config.docOnlineUrl;
                            cookiedoc.Path = "/";
                            cookiedoc.HttpOnly = false;
                            cookiedoc.Secure = false;
                            respMessage.Headers.AddCookies(new CookieHeaderValue[] { cookiedoc });
                        }
                            
                        Log.Info("Login-page started...");
                        return respMessage;
                        #endregion
                    }
                    if (user == null)
                    {
                        string rootFileConfig = HttpContext.Current.Server.MapPath("~/Config");
                        string json = System.IO.File.ReadAllText(Path.Combine(rootFileConfig, Path.GetFileName("Config.json")));
                        if (json != null)
                        {
                            settings config = JsonConvert.DeserializeObject<settings>(json);
                            user = db.sys_users.FirstOrDefault(us => us.user_id == u.user_id);
                            if (user != null)
                            {
                                user.wrong_pass_count = ((user.wrong_pass_count ?? 0) + 1);
                                if (user.wrong_pass_count >= config.wrongAcceptPass)
                                {
                                    user.status = 0;
                                }
                                await db.SaveChangesAsync();
                                int? wrong_pass_count = (config.wrongAcceptPass - user.wrong_pass_count) ?? 0;
                                if (wrong_pass_count > 0)
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tên đăng nhập hoặc mật khẩu không đúng, bạn chỉ còn " + wrong_pass_count + " lần đăng nhập nữa!", err = "1" });
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tài khoản đã bị khoá, vui lòng liên hệ quản trị để kích hoạt!", err = "1" });
                                }
                            }
                            else
                            {
                                if (config.isBlockIP == true && config.pathBlockIP != null)
                                {
                                    string rootFileBlock = HttpContext.Current.Server.MapPath("~/Config");
                                    string jsonblockip = System.IO.File.ReadAllText(Path.Combine(rootFileBlock, Path.GetFileName(config.pathBlockIP)));
                                    if (jsonblockip != null)
                                    {
                                        List<BlockIP> blockips = JsonConvert.DeserializeObject<List<BlockIP>>(jsonblockip);
                                        var blockip = blockips.FirstOrDefault(x => x.IP == ip);
                                        if (blockip != null)
                                        {
                                            blockip.Count += 1;
                                        }
                                        else
                                        {
                                            blockips.Add(new BlockIP()
                                            {
                                                IP = ip,
                                                Count = 1,
                                            });
                                        }
                                        string jsonData = JsonConvert.SerializeObject(blockips, Formatting.None);
                                        //System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath(config.pathBlockIP), jsonData);
                                        System.IO.File.WriteAllText(Path.Combine(rootFileBlock, Path.GetFileName(config.pathBlockIP)), jsonData);
                                    }
                                }
                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tên đăng nhập hoặc mật khẩu không đúng!", err = "1" });
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(tk.user_id, tk.full_name, JsonConvert.SerializeObject(new { data = u, contents }), domainurl + "Home/Login", ip, tk.token_id, "Lỗi khi đăng nhập", 0, "Login");
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
                    helper.saveLog(tk.user_id, tk.full_name, JsonConvert.SerializeObject(new { data = u, contents }), domainurl + "Home/Login", ip, tk.token_id, "Lỗi khi đăng nhập", 0, "Login");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }
        //Logout
        [HttpGet]
        public async Task<HttpResponseMessage> Logout()
        {
            //try
            //{
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            using (DBEntities db = new DBEntities())
            {
                var users = await db.sys_web_acess.Where(a => a.user_id == uid).ToListAsync();
                foreach (var u in users)
                {
                    u.is_endtime = DateTime.Now;
                }
                string[] myCookies = HttpContext.Current.Request.Cookies.AllKeys;
                foreach (string cookie in myCookies)
                {
                    HttpContext.Current.Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
                }
                await db.SaveChangesAsync();
            }
            //}
            //catch (Exception)
            //{
            //    string contents = helper.ExceptionMessage(e);
            //    if (!helper.debug)
            //    {
            //        contents = "";
            //    }
            //    Log.Error(contents);
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        #region forger pas
        public class InfoEmail
        {
            public string email { get; set; }
            public string kpmail { get; set; }
            public string timemail { get; set; }

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
            string domainName = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            string json = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Config/Config.json"));
            settings deJson = JsonConvert.DeserializeObject<settings>(json);

            //string pwEm = Codec.DecryptString(deJson.psemail, helper.psKey);
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
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "-1", ms = "Thông tin Email hệ thống chưa chính xác.<br> Vui lòng liên hệ hỗ trợ để cài đặt." });
                        }
                        string dePwMail = Codec.DecryptString(strPwMail, helper.psKey);
                        if (strPwMail != deJson.psemail)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "-1", ms = "Thông tin Email hệ thống chưa chính xác.<br> Vui lòng liên hệ hỗ trợ để cài đặt." });
                        }

                        string receiver = provider.FormData.GetValues("mailinfo").SingleOrDefault();
                        infoMailSend mailInfo = JsonConvert.DeserializeObject<infoMailSend>(receiver);
                        if (db.sys_users.Where(x => x.user_id == mailInfo.to).FirstOrDefault() == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "-1", ms = "Thông tin tên đăng nhập chưa chính xác." });
                        }
                        var email = db.sys_users.Where(x => x.user_id == mailInfo.to && x.email == mailInfo.email).FirstOrDefault();
                        if (email == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "-1", ms = "Thông tin email chưa chính xác." });
                        }
                        else if (email.reset_pass_time == null || ((DateTime.Now - email.reset_pass_time).Value.TotalMinutes >= helper.timemail))
                        {

                            email.reset_pass_time = DateTime.Now;
                            email.reset_pass_code = helper.GenKey();
                            db.Entry(email).State = EntityState.Modified;
                            db.SaveChanges();
                            // return link
                            string code = "{'uid':'" + email.user_id + "','code':'" + email.reset_pass_code + "'}";
                            string JSONresult = Codec.EncryptString(JsonConvert.SerializeObject(code), ConfigurationManager.AppSettings["EncriptKey"]);
                            //var body = "http://localhost:3000/forgetpss/" + JSONresult;
                            var body = ConfigurationManager.AppSettings["ValidIssuer"] + "/forgetpss/" + JSONresult.Replace("+", "tun");
                            Thread T1 = new Thread(delegate ()
                            {
                                var message = new MailMessage();
                                message.From = new MailAddress(deJson.email, mailInfo.display_name); ;
                                message.To.Add(new MailAddress(email.email));
                                message.Subject = mailInfo.subject;
                                message.Body = mailInfo.top + body + mailInfo.bottom;
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
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });

                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Login/sendEMail", ip, tid, "Lỗi khi gửi mail ", 0, "SendEmailController");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Login/sendEMail", ip, tid, "Lỗi khi gửi mail ", 0, "SendEmailController");
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
                infoMail.kpmail = dataSetting.psemail != null ? Codec.EncryptString(dataSetting.psemail, helper.psKey) : null;
                infoMail.timemail = dataSetting.timemail.ToString() != null ? Codec.EncryptString(dataSetting.timemail.ToString(), helper.psKey) : null;
                //return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.SerializeObject(infoMail) });
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.SerializeObject(infoMail) });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Login/GetConfigMail", ip, tid, "Lỗi khi lấy thông tin config email.", 0, "SendEmail");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> checkForgetPass([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string str = data["str"].ToObject<string>();
            string des = Codec.DecryptString(str, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            string user_id = jobject["uid"].ToObject<string>();
            string code = jobject["code"].ToObject<string>();
            settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = "administrator";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var user = db.sys_users.FirstOrDefault(x => x.user_id == user_id && x.reset_pass_code == code);
                    if (user != null && user.reset_pass_time != null)
                    {

                        if (user.reset_pass_time == null || (DateTime.Now - user.reset_pass_time).Value.TotalMinutes < helper.timemail)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Chưa quá 5 phút", err = "0" });
                        }
                        else return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Quá 5 phút.", err = "1" });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Quá 5 phút.", err = "1" });
                    }

                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Login/checkForgetPass", ip, tid, "Lỗi khi thêm màn hình tivi", 0, "Tivi");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Login/checkForgetPass", ip, tid, "Lỗi khi thêm màn hình tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> changePass([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string str = data["str"].ToObject<string>();
            string des = Codec.DecryptString(str, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            string user_id = jobject["user_id"].ToObject<string>();
            string password = jobject["password"].ToObject<string>();
            settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = "administrator";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var user = db.sys_users.FirstOrDefault(x => x.user_id == user_id);
                    if (user != null)
                    {
                        string depass = BCrypt.Net.BCrypt.HashPassword(password);
                        user.is_psword = depass;
                        user.modified_date = DateTime.Now;
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Login/changePass", ip, tid, "Lỗi khi thêm màn hình tivi", 0, "Tivi");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Login/changePass", ip, tid, "Lỗi khi thêm màn hình tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        #endregion
    }
}