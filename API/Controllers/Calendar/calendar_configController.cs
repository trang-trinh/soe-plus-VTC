using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Helper;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.IO;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json.Linq;

namespace API.Controllers.Calendar
{
    [Authorize(Roles = "login")]
    public class calendar_configController : ApiController
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
        public async Task<HttpResponseMessage> update_procedureform()
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
                    bool isAdd = bool.Parse(provider.FormData.GetValues("isAdd").SingleOrDefault());
                    var md = provider.FormData.GetValues("model").SingleOrDefault();
                    calendar_procedureform model = JsonConvert.DeserializeObject<calendar_procedureform>(md);
                    calendar_log log = new calendar_log();

                    if (isAdd)
                    {
                        model.procedureform_id = helper.GenKey();
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        log.message = "Thêm mới quy trình: " + model.procedureform_name;
                        db.calendar_procedureform.Add(model);
                    }
                    else
                    {
                        model.modified_by = uid;
                        model.modified_date = DateTime.Now;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        db.Entry(model).State = EntityState.Modified;
                        log.message = "Cập nhật quy trình: " + model.procedureform_name;
                    }
                    #region add law_logs
                    if (helper.wlog)
                    {
                        log.log_type = 4;
                        log.key_id = model.procedureform_id;
                        log.is_view = false;
                        log.created_by = uid;
                        log.created_date = DateTime.Now;
                        log.created_ip = ip;
                        log.created_token_id = tid;
                        log.organization_id = model.organization_id;
                        db.calendar_log.Add(log);
                    }
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_config/update_procedureform", ip, tid, "Lỗi khi cập nhật quy trình", 0, "calendar_config");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_config/update_procedureform", ip, tid, "Lỗi khi cập nhật quy trình", 0, "calendar_config");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_signform()
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
                    bool isAdd = bool.Parse(provider.FormData.GetValues("isAdd").SingleOrDefault());
                    var md = provider.FormData.GetValues("model").SingleOrDefault();
                    calendar_signform model = JsonConvert.DeserializeObject<calendar_signform>(md);
                    var us = provider.FormData.GetValues("users").SingleOrDefault();
                    List<string> users = JsonConvert.DeserializeObject<List<string>>(us);
                    calendar_log log = new calendar_log();
                    #region model
                    var procedure = await db.calendar_procedureform.AsNoTracking().FirstOrDefaultAsync(x => x.procedureform_id == model.procedureform_id);
                    if (procedure != null)
                    {
                        model.is_group = procedure.is_group;
                    }
                    if (isAdd)
                    {
                        model.signform_id = helper.GenKey();
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        log.message = "Thêm mới nhóm duyệt: " + model.signform_name;
                        db.calendar_signform.Add(model);
                    }
                    else
                    {
                        model.modified_by = uid;
                        model.modified_date = DateTime.Now;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        db.Entry(model).State = EntityState.Modified;
                        log.message = "Cập nhật nhóm duyệt: " + model.signform_name;
                    }
                    #endregion
                    #region users
                    var userold = await db.calendar_signuserform.Where(x => x.signform_id == model.signform_id).ToListAsync();
                    if (userold.Count > 0)
                    {
                        db.calendar_signuserform.RemoveRange(userold);
                    }
                    if (users.Count > 0)
                    {
                        List<calendar_signuserform> signusers = new List<calendar_signuserform>();
                        int stt = 0;
                        foreach (var users_id in users)
                        {
                            stt++;
                            var signuser = new calendar_signuserform();
                            signuser.signuserform_id = helper.GenKey();
                            signuser.signform_id = model.signform_id;
                            signuser.user_id = users_id;
                            signuser.is_step = stt;
                            signuser.is_type = 0;
                            signuser.status = true;
                            signuser.created_by = uid;
                            signuser.created_date = DateTime.Now;
                            signuser.created_ip = ip;
                            signuser.created_token_id = tid;
                            signusers.Add(signuser);
                        }
                        if (signusers.Count > 0)
                        {
                            db.calendar_signuserform.AddRange(signusers);
                        }
                    }
                    #endregion
                    #region add law_logs
                    if (helper.wlog)
                    {
                        log.log_type = 5;
                        log.key_id = model.signform_id;
                        log.is_view = false;
                        log.created_by = uid;
                        log.created_date = DateTime.Now;
                        log.created_ip = ip;
                        log.created_token_id = tid;
                        log.organization_id = model.organization_id;
                        db.calendar_log.Add(log);
                    }
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_config/update_signform", ip, tid, "Lỗi khi cập nhật nhóm duyệt", 0, "calendar_config");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_config/update_signform", ip, tid, "Lỗi khi cập nhật nhóm duyệt", 0, "calendar_config");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_status_procedureform([System.Web.Mvc.Bind(Include = "id,status")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
                string id = data["id"].ToObject<string>();
                bool? status = data["status"]?.ToObject<bool?>();

                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var model = await db.calendar_procedureform.FindAsync(id);
                        if (model != null)
                        {
                            model.status = status;
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                calendar_log log = new calendar_log();
                                log.log_type = 4;
                                log.message = "Cập nhât trạng thái quy trình: " + model.procedureform_name;
                                log.key_id = model.procedureform_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = model.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_config/update_status_procedureform", ip, tid, "Lỗi khi cập nhật quy trình", 0, "calendar_config");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_config/update_status_procedureform", ip, tid, "Lỗi khi cập nhật quy trình", 0, "calendar_config");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_status_signform([System.Web.Mvc.Bind(Include = "id,status")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
                string id = data["id"].ToObject<string>();
                bool? status = data["status"]?.ToObject<bool?>();

                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var model = await db.calendar_signform.FindAsync(id);
                        if (model != null)
                        {
                            model.status = status;
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                calendar_log log = new calendar_log();
                                log.log_type = 5;
                                log.message = "Cập nhât trạng thái nhóm duyệt: " + model.signform_name;
                                log.key_id = model.signform_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = model.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_boardroom/update_status_ca_boardroom", ip, tid, "Lỗi khi cập nhật phòng họp", 0, "calendar_boardroom");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_boardroom/update_status_ca_boardroom", ip, tid, "Lỗi khi cập nhật phòng họp", 0, "calendar_boardroom");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_procedureform([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.calendar_procedureform.Where(a => ids.Contains(a.procedureform_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<calendar_procedureform> del = new List<calendar_procedureform>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    calendar_log log = new calendar_log();
                                    log.log_type = 4;
                                    log.message = "Xóa quy trình: " + da.procedureform_name;
                                    log.key_id = da.procedureform_id;
                                    log.organization_id = da.organization_id;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    log.is_view = false;
                                    db.calendar_log.Add(log);
                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.calendar_procedureform.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_config/delete_procedureform", ip, tid, "Lỗi khi xóa quy trình", 0, "calendar_config");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_config/delete_procedureform", ip, tid, "Lỗi khi xoá quy trình", 0, "calendar_config");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_signform([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.calendar_signform.Where(a => ids.Contains(a.signform_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<calendar_signform> del = new List<calendar_signform>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    calendar_log log = new calendar_log();
                                    log.log_type = 5;
                                    log.message = "Xóa nhóm duyệt: " + da.signform_name;
                                    log.key_id = da.signform_id;
                                    log.organization_id = da.organization_id;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    log.is_view = false;
                                    db.calendar_log.Add(log);
                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.calendar_signform.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_config/delete_signform", ip, tid, "Lỗi khi xóa quy trình", 0, "calendar_config");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_config/delete_signform", ip, tid, "Lỗi khi xoá quy trình", 0, "calendar_config");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
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