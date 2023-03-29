using API.Models;
using Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace API.Controllers.HRM.Tiemkeep
{
    [Authorize(Roles = "login")]
    public class hrm_timekeepController : ApiController
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
        public async Task<HttpResponseMessage> update_timekeep()
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
                    var md = provider.FormData.GetValues("model").SingleOrDefault();
                    hrm_timekeep model = JsonConvert.DeserializeObject<hrm_timekeep>(md);
                    var sl = provider.FormData.GetValues("selected").SingleOrDefault();
                    List<hrm_timekeep> selected = JsonConvert.DeserializeObject<List<hrm_timekeep>>(sl);
                    if (selected != null)
                    {
                        List<hrm_timekeep> timekeeps = new List<hrm_timekeep>();
                        foreach (var item in selected)
                        {
                            var exists = await db.hrm_timekeep.FirstOrDefaultAsync(x => x.profile_id == item.profile_id && x.workday == item.workday);
                            if (exists != null)
                            {
                                exists.symbol_id = model.symbol_id;
                                exists.modified_by = uid;
                                exists.modified_date = DateTime.Now;
                                exists.modified_ip = ip;
                                exists.modified_token_id = tid;
                            }
                            else
                            {
                                hrm_timekeep timekeep = new hrm_timekeep();
                                timekeep = item;
                                timekeep.timekeep_id = helper.GenKey();
                                timekeep.symbol_id = model.symbol_id;
                                timekeep.created_by = uid;
                                timekeep.created_date = DateTime.Now;
                                timekeep.created_ip = ip;
                                timekeep.created_token_id = tid;
                                timekeeps.Add(timekeep);
                            }
                        }
                        if (timekeeps.Count > 0)
                        {
                            db.hrm_timekeep.AddRange(timekeeps);
                        }
                        await db.SaveChangesAsync();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_timekeep/update_timekeep", ip, tid, "Lỗi khi cập nhật", 0, "hrm_timekeep");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_timekeep/update_timekeep", ip, tid, "Lỗi khi cập nhật", 0, "hrm_timekeep");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> delete_timekeep()
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
                    var sl = provider.FormData.GetValues("selected").SingleOrDefault();

                    List<hrm_timekeep> selected = JsonConvert.DeserializeObject<List<hrm_timekeep>>(sl);
                    if (selected != null)
                    {
                        List<hrm_timekeep> timekeeps = new List<hrm_timekeep>();
                        foreach (var item in selected)
                        {
                            var timekeep = await db.hrm_timekeep.Where(x => x.profile_id == item.profile_id && x.workday == item.workday).ToListAsync();
                            if (timekeep.Count > 0)
                            {
                                foreach (var c in timekeep)
                                {
                                    timekeeps.Add(c);
                                }
                            }
                        }
                        if (timekeeps.Count > 0)
                        {
                            db.hrm_timekeep.RemoveRange(timekeeps);
                        }
                        await db.SaveChangesAsync();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_timekeep/delete_timekeep", ip, tid, "Lỗi khi cập nhật", 0, "hrm_timekeep");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_timekeep/delete_timekeep", ip, tid, "Lỗi khi cập nhật", 0, "hrm_timekeep");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
    }
}
