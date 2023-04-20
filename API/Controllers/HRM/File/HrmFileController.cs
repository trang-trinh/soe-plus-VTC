using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Models;
using Helper;
using System.Web.Script.Serialization;
using HtmlAgilityPack;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using API.Helper;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class HrmFileController : ApiController
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

        [HttpPut]
        public async Task<HttpResponseMessage> Update_View([System.Web.Mvc.Bind(Include = "file_id")] hrm_file_log file_log)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    string Device = helper.getDecideNameAuto(Request.Headers.UserAgent.ToString());
                    var da = db.hrm_file_log.FirstOrDefault(x => x.file_id == file_log.file_id && x.created_by == uid);
                    if (da != null)
                    {

                        da.count_view++;
                        da.created_ip = ip;
                        da.created_date = DateTime.Now;
                        da.created_by = uid;
                        da.created_token_id = tid;
                        da.from_device = Device;
                        db.Entry(da).State = EntityState.Modified;
                    }
                    else
                    {
                        hrm_file_log log = new hrm_file_log();
                        log.count_view = 1;
                        log.file_id = file_log.file_id;
                        log.created_ip = ip;
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.from_device = Device;
                        db.hrm_file_log.Add(log);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
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