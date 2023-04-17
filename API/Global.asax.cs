using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Client;
using ScheduledTasks;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace CMS
{
    public class MvcApplication : HttpApplication
    {
        settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));
        protected async void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log4net.Config.XmlConfigurator.Configure();
            await JobScheduler.Start();
            //
            try
            {
                if (config != null)
                {
                    helper.debug = config.debug;
                    helper.wlog = config.wlog;
                    helper.logCongtent = config.logCongtent;
                    helper.milisec = config.milisec;
                    helper.timeout = config.timeout;
                    helper.path_xml = config.path_xml;
                    helper.socketUrl = config.socketUrl;
                }
                SocketClient.onConnect(helper.socketUrl);
            }
            catch { }
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string ipAddress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : "";
            if (config != null && config.isBlockIP == true && IsValidIpAddress(ipAddress))
            {
                HttpContext.Current.Response.StatusCode = 403;  // (Forbidden)
            }
        }
        //protected void Application_BeginRequest(object sender, EventArgs e)
        //{
        //    string ipAddress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : "";
        //    if (config != null && config.isBlockIP == true && IsValidIpAddress(ipAddress))
        //    {
        //        HttpContext.Current.Response.StatusCode = 403;  // (Forbidden)
        //    }

        //    string rawURL = Request.RawUrl;
        //    if (rawURL.Contains("/Portals/") && !rawURL.Contains("/public/") && !rawURL.Contains("/Users/") && !rawURL.Contains("/Image/")
        //        && !rawURL.Contains("/Donvi/") && !rawURL.Contains("/Module/") && !rawURL.Contains("/FileChatSystem/") && !rawURL.Contains("/file/")
        //    )
        //    {
        //        HttpContext context = HttpContext.Current;
        //        if (Request.UrlReferrer == null || context.Request.Cookies["jwt"] == null)
        //        {
        //            //bool isImage = helper.IsImageFileName(Request.RawUrl);
        //            //var UrlReferrer = ConfigurationManager.AppSettings["UrlReferrer"];
        //            //if (!isImage || (isImage && !Request.RawUrl.Contains("?public=true")) || (UrlReferrer != null && !Request.UrlReferrer.OriginalString.Contains(UrlReferrer)))
        //            //{
        //            //    context.Response.ClearContent();
        //            //    context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        //            //    context.Response.Write("<script language=\"javascript\">" + "self.location='/Error/401_Unauthorized.html';</script>");
        //            //    context.Response.End();
        //            //}
        //            #region check quyền trong database nếu có
        //            var UrlReferrer = ConfigurationManager.AppSettings["UrlReferrer"];
        //            using (DBEntities db = new DBEntities())
        //            {
        //                var fileNonPublic = db.sys_config_file_public.Where(x => x.file_path == rawURL).OrderByDescending(x => x.type_public).FirstOrDefault();

        //                var tokenHandler = new JwtSecurityTokenHandler();
        //                SecurityToken validatedToken = null;
        //                ClaimsPrincipal claims = null;
        //                var validationParameters = new TokenValidationParameters()
        //                {
        //                    ValidateIssuer = true,
        //                    ValidateAudience = true,
        //                    ValidateIssuerSigningKey = true,
        //                    ValidIssuer = ConfigurationManager.AppSettings["ValidIssuer"], //some string, normally web url,  
        //                    ValidAudience = ConfigurationManager.AppSettings["ValidAudience"],
        //                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["IssuerSigningKey"]))
        //                };
        //                if (fileNonPublic != null)
        //                {
        //                    if (fileNonPublic.type_public == 1 || fileNonPublic.type_public == 2) // type_public = 1: yêu cầu đăng nhập, type_public = 2: yêu cầu quyền xem với tk đăng nhập
        //                    {
        //                        if (context.Request.Cookies["jwt"] == null)
        //                        {
        //                            context.Response.ClearContent();
        //                            context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        //                            context.Response.Write("<script language=\"javascript\">" + "self.location='/Error/401_Unauthorized.html';</script>");
        //                            context.Response.End();
        //                        }
        //                        else
        //                        {
        //                            var jwtCookie = context.Request.Cookies["jwt"].Value;
        //                            jwtCookie = HttpUtility.UrlDecode(jwtCookie);
        //                            string jwt = Codec.DecryptString(jwtCookie, ConfigurationManager.AppSettings["EncriptKey"]);
        //                            claims = tokenHandler.ValidateToken(jwt, validationParameters, out validatedToken);
        //                            if ((claims == null && validatedToken == null) || (UrlReferrer != null && Request.UrlReferrer != null && !Request.UrlReferrer.OriginalString.Contains(UrlReferrer)))
        //                            {
        //                                context.Response.ClearContent();
        //                                context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        //                                context.Response.Write("<script language=\"javascript\">" + "self.location='/Error/401_Unauthorized.html';</script>");
        //                                context.Response.End();
        //                            }
        //                            if (fileNonPublic.type_public == 2)
        //                            {
        //                                //bool is_admin = claims.Claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
        //                                //if (fileNonPublic.is_admin_view == true && is_admin != true)
        //                                //{
        //                                //    context.Response.ClearContent();
        //                                //    context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        //                                //    context.Response.Write("<script language=\"javascript\">" + "self.location='/Error/401_Unauthorized.html';</script>");
        //                                //    context.Response.End();
        //                                //}
        //                                // nếu check quyền (theo role) trong db thì xử lý tại đây
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (context.Request.Cookies["jwt"] == null)
        //                    {
        //                        context.Response.ClearContent();
        //                        context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        //                        context.Response.Write("<script language=\"javascript\">" + "self.location='/Error/401_Unauthorized.html';</script>");
        //                        context.Response.End();
        //                    }
        //                    else
        //                    {
        //                        var jwtCookie = context.Request.Cookies["jwt"].Value;
        //                        jwtCookie = HttpUtility.UrlDecode(jwtCookie);
        //                        string jwt = Codec.DecryptString(jwtCookie, ConfigurationManager.AppSettings["EncriptKey"]);
        //                        claims = tokenHandler.ValidateToken(jwt, validationParameters, out validatedToken);
        //                        if ((claims == null && validatedToken == null) || (UrlReferrer != null && Request.UrlReferrer != null && !Request.UrlReferrer.OriginalString.Contains(UrlReferrer)))
        //                        {
        //                            context.Response.ClearContent();
        //                            context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        //                            context.Response.Write("<script language=\"javascript\">" + "self.location='/Error/401_Unauthorized.html';</script>");
        //                            context.Response.End();
        //                        }
        //                    }
        //                }
        //            }
        //            #endregion
        //        }
        //    }
        //}

        protected void Application_EndRequest(Object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            if (context.Response.Status.Substring(0, 3).Equals("401"))
            {
                context.Response.ClearContent();
                context.Response.Write("<script language=\"javascript\">" +
                             "self.location='../Error/401.html';</script>");
            }
        }
        protected bool IsValidIpAddress(string ipAddress)
        {
            string jsonblockip = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/") + "/Config/" + Path.GetFileName(config.pathBlockIP));
            if (jsonblockip != null)
            {
                List<BlockIP> blockips = JsonConvert.DeserializeObject<List<BlockIP>>(jsonblockip);
                var blockip = blockips.FirstOrDefault(x => x.IP == ipAddress);
                if (blockip != null)
                {
                    return blockip.Count > config.wrongAcceptIP;
                }
            }
            return false;
        }
    }
}