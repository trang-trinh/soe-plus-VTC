using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Controllers.HRM.ConfigUserCode
{
    [Authorize(Roles = "login")]
    public class hrm_config_insurance_rateController : ApiController
    {
        private DBEntities db = new DBEntities();


        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

           [HttpPut]
        public async Task<HttpResponseMessage> update_hrm_config_insurance_rate()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdconfig_insurance_rate = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();

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
                        fdconfig_insurance_rate = provider.FormData.GetValues("hrm_config_insurance_rate").SingleOrDefault();
                        var ins_checked = provider.FormData.GetValues("ins_checked").SingleOrDefault();
                        List<hrm_config_insurance_rate> hrm_Config_insurance_rate = JsonConvert.DeserializeObject<List<hrm_config_insurance_rate>>(fdconfig_insurance_rate);
                        var dv = int.Parse(dvid);

                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";


                        foreach (var item in hrm_Config_insurance_rate)
                        {
                            var das =   db.hrm_config_insurance_rate.AsNoTracking().Where(a => a.insurance_rate_id==item.insurance_rate_id).FirstOrDefault();
                            if (das == null)
                            {
                        
                                item.organization_id = int.Parse(dvid);
                                item.created_by = uid;
                                item.created_date = DateTime.Now;
                                item.created_ip = ip;
                                if(ins_checked=="True")
                                item.is_auto = true ;
                                else
                                    item.is_auto = false;
                                db.hrm_config_insurance_rate.Add(item);
                                db.SaveChanges();

                                #region add hrm_log
                                if (helper.wlog)
                                {

                                    hrm_log log = new hrm_log();
                                    log.title = "Cấu hình tỷ lệ bảo hiểm ";
                                    log.log_module = "hrm_config_insurance_rate";
                                    log.log_type = 0;
                                    log.id_key = null;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.hrm_log.Add(log);
                                    db.SaveChanges();

                                }
                                #endregion
                            }
                            else
                            {
                                if (ins_checked == "True")
                                    item.is_auto = true;
                                else
                                    item.is_auto = false;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                #region add hrm_log
                                if (helper.wlog)
                                {

                                    hrm_log log = new hrm_log();
                                    log.title = "Cập nhật cấu hình tỷ lệ bảo hiểm ";
                                    log.log_module = "hrm_config_insurance_rate";
                                    log.log_type = 1;
                                    log.id_key = null;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.hrm_log.Add(log);
                                    db.SaveChanges();

                                }
                                #endregion
                            }

                        }




                      
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdconfig_insurance_rate, contents }), domainurl + "hrm_config_insurance_rate/Update_config_insurance_rate", ip, tid, "Lỗi khi cập nhật config_insurance_rate", 0, "config_insurance_rate");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdconfig_insurance_rate, contents }), domainurl + "hrm_config_insurance_rate/Update_config_insurance_rate", ip, tid, "Lỗi khi cập nhật config_insurance_rate", 0, "config_insurance_rate");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_hrm_config_insurance_rate([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;

            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.hrm_config_insurance_rate.Where(a => id.Contains(a.insurance_rate_id)).ToListAsync();

                        if (das != null)
                        {
                            List<hrm_config_insurance_rate> del = new List<hrm_config_insurance_rate>();
                            foreach (var da in das)
                            {
                                del.Add(da);


                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_config_insurance_rate.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_config_insurance_rate/Delete_config_insurance_rate", ip, tid, "Lỗi khi xoá dân tộc", 0, "config_insurance_rate");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_config_insurance_rate/Delete_config_insurance_rate", ip, tid, "Lỗi khi xoá dân tộc", 0, "config_insurance_rate");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

    }
}