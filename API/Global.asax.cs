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