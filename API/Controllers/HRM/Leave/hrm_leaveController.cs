using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers.Leave
{
    public class hrm_leaveController : ApiController
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
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_leave_profile()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                    string en_model = provider.FormData.GetValues("model").SingleOrDefault();
                    hrm_leave_profile model = JsonConvert.DeserializeObject<hrm_leave_profile>(en_model);

                    var leave = await db.hrm_leave_profile.FirstOrDefaultAsync(x => x.profile_id == model.profile_id && x.year == model.year);
                    if (leave != null) {
                        leave.month1 = model.month1;
                        leave.month2 = model.month2;
                        leave.month3 = model.month3;
                        leave.month4 = model.month4;
                        leave.month5 = model.month5;
                        leave.month6 = model.month6;
                        leave.month7 = model.month7;
                        leave.month8 = model.month8;
                        leave.month9 = model.month9;
                        leave.month10 = model.month10;
                        leave.month11 = model.month11;
                        leave.month12 = model.month12;
                        leave.modified_by = uid;
                        leave.modified_date = DateTime  .Now;
                        leave.modified_ip = ip;
                        leave.modified_token_id = tid;
                        db.Entry(leave).State = EntityState.Modified;
                    }
                    else
                    {
                        var profile = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_id == model.profile_id);
                        hrm_leave_profile md = new hrm_leave_profile();
                        md = model;
                        md.created_by = uid;
                        md.created_date = DateTime.Now;
                        md.created_ip = ip;
                        md.created_token_id = tid;
                        md.organization_id = profile.organization_id;
                        db.hrm_leave_profile.Add(md);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_leave/update_leave_profile", ip, tid, "Lỗi khi cập nhật", 0, "hrm_leave");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_leave/update_leave_profile", ip, tid, "Lỗi khi cập nhật", 0, "hrm_leave");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
    }
}
