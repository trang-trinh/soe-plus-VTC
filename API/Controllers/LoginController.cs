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
                        user.wrong_pass_count = 0;
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
                            permClaims.Add(new Claim("dvid", user.organization_id.ToString()));
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
                                organization_id = org?.organization_id,
                                department_id = user.department_id,
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
    }
}