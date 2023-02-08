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

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class ReportModuleController : ApiController
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

        //[Authorize(Roles = "login")]
        //[Authorize]
        [HttpPut]
        public async Task<HttpResponseMessage> update_module()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlaw = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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
                    // Provider
                    string rootTemp = HttpContext.Current.Server.MapPath("~/Portals");
                    bool existsTemp = Directory.Exists(rootTemp);
                    if (!existsTemp)
                        Directory.CreateDirectory(rootTemp);
                    var provider = new MultipartFormDataStreamProvider(rootTemp);
                    var task = await Request.Content.ReadAsMultipartAsync(provider);

                    // Params
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    List<report_modules> multiple = JsonConvert.DeserializeObject<List<report_modules>>(provider.FormData.GetValues("user_module").SingleOrDefault());
                    //List<calendar_member> mbs = JsonConvert.DeserializeObject<List<calendar_member>>(provider.FormData.GetValues("members").SingleOrDefault());
                    //List<calendar_attend> dps = JsonConvert.DeserializeObject<List<calendar_attend>>(provider.FormData.GetValues("departments").SingleOrDefault());
                    if (multiple.Count > 0)
                    {
                        // del all
                        List<report_modules> itemToRemove = db.report_modules.Where(a => a.module_id == 195 && a.is_link == "/reportmodule/history").ToList();
                        if (itemToRemove.Count > 0) db.report_modules.RemoveRange(itemToRemove);
                        foreach (var model in multiple)
                        {
                            #region Model
                            model.report_module_id = helper.GenKey();

                            model.created_by = uid;
                            model.created_date = DateTime.Now;
                            model.created_ip = ip;
                            model.created_token_id = tid;
                            //db.report_modules.Add(model);

                            #endregion
                            #region nguoi dung

                            if (model.report_module_user.Count > 0)
                            {
                                foreach (var user in model.report_module_user)
                                {
                                    user.report_module_user_id = helper.GenKey();
                                    user.report_module_id = model.report_module_id;
                                    user.created_by = uid;
                                    user.created_date = DateTime.Now;
                                    user.created_ip = ip;
                                    user.created_token_id = tid;
                                }

                            }
                            #endregion
                            #region Phongban
                            else if (model.report_module_organization.Count > 0)
                            {
                                foreach (var dp in model.report_module_organization)
                                {
                                    dp.report_module_organization_id = helper.GenKey();
                                    dp.report_module_id = model.report_module_id;
                                    dp.created_by = uid;
                                    dp.created_date = DateTime.Now;
                                    dp.created_ip = ip;
                                    dp.created_token_id = tid;
                                }
                            }

                            #endregion
                        }
                        db.report_modules.AddRange(multiple);
                    }
                    await db.SaveChangesAsync();
                    //  await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/update_calendar_week_multiple", ip, tid, "Lỗi khi cập nhật lịch họp ", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/update_calendar_week_multiple", ip, tid, "Lỗi khi cập nhật lịch họp", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


    }
}