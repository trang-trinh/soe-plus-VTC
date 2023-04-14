using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using API.Models;
using Newtonsoft.Json;
using Helper;
using System.Data.Entity.Validation;
using API.Helper;
using System.Data.Entity;

namespace API.Controllers.Request.Category
{
    [Authorize(Roles = "login")]
    public class request_ca_formController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]

        public async Task<HttpResponseMessage> add_request_ca_form()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_form = "";
            string fdca_form_sign = "";
            string fdca_form_sign_user = "";
            string fdca_form_team = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

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
                        fdca_form = provider.FormData.GetValues("request_form").SingleOrDefault();
                        fdca_form_sign = provider.FormData.GetValues("request_form_sign").SingleOrDefault();
                        fdca_form_sign_user = provider.FormData.GetValues("request_form_sign_user").SingleOrDefault();
                        fdca_form_team = provider.FormData.GetValues("request_ca_from_team").SingleOrDefault();
                        request_ca_form obj_data = JsonConvert.DeserializeObject<request_ca_form>(fdca_form);
                        List<request_ca_form_sign> form_signs = JsonConvert.DeserializeObject<List<request_ca_form_sign>>(fdca_form_sign);
                        List<request_form_sign_user> form_sign_users = JsonConvert.DeserializeObject<List<request_form_sign_user>>(fdca_form_sign_user);
                        List<request_ca_form_team> from_teams = JsonConvert.DeserializeObject<List<request_ca_form_team>>(fdca_form_team);

                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                        obj_data.organization_id = super ? 0 : int.Parse(dvid);
                        obj_data.request_form_id = helper.GenKey();
                        obj_data.created_by = uid;
                        obj_data.created_date = DateTime.Now;
                        obj_data.created_ip = ip;
                        obj_data.created_token_id = tid;
                        db.request_ca_form.Add(obj_data);

                        if (form_signs.Count > 0)
                        {
                            List<request_ca_form_sign> listformsigns = new List<request_ca_form_sign>();
                            List<request_form_sign_user> listformsignusers = new List<request_form_sign_user>();
                            foreach (var fs in form_signs)
                            {
                                fs.request_form_sign_id = helper.GenKey();
                                fs.request_team_id = null;
                                fs.request_form_id = obj_data.request_form_id;
                                fs.created_by = uid;
                                fs.created_date = DateTime.Now;
                                fs.created_ip = ip;
                                fs.created_token_id = tid;
                                listformsigns.Add(fs);

                                if(form_sign_users.Count > 0)
                                {
                                    foreach(var fsu in form_sign_users)
                                    {
                                        fsu.request_form_sign_user_id = helper.GenKey();
                                        fsu.request_form_sign_id = fs.request_form_sign_id;
                                        fsu.created_by = uid;
                                        fsu.created_date = DateTime.Now;
                                        fsu.created_ip = ip;
                                        fsu.created_token_id = tid;
                                        listformsignusers.Add(fsu);
                                    }
                                }
                            }
                            if (listformsigns.Count > 0)
                            {
                                db.request_ca_form_sign.AddRange(listformsigns);
                            }
                            if (listformsignusers.Count > 0)
                            {
                                db.request_form_sign_user.AddRange(listformsignusers);
                            }
                        }

                        if(from_teams.Count > 0)
                        {
                            List<request_ca_form_team> listformteams = new List<request_ca_form_team>();
                            foreach (var ft in from_teams)
                            {
                                ft.request_form_team_id = helper.GenKey();
                                ft.request_form_id = obj_data.request_form_id;
                                listformteams.Add(ft);
                            }
                            if (listformteams.Count > 0)
                            {
                                db.request_ca_form_team.AddRange(listformteams);
                            }
                        }
                        db.SaveChanges();

                        #region add request_log
                        if (helper.wlog)
                        {
                            request_log log = new request_log();
                            log.title = "Thêm form " + obj_data.request_form_name;
                            log.log_module = "request_ca_form";
                            log.log_type = 0;
                            log.id_key = obj_data.request_form_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.request_log.Add(log);
                            db.SaveChanges();

                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_form, contents }), domainurl + "request_ca_form/add_request_ca_form", ip, tid, "Lỗi khi thêm form", 0, "request_ca_form");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_form, contents }), domainurl + "request_ca_form/add_request_ca_form", ip, tid, "Lỗi khi thêm form", 0, "request_ca_form");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]

        public async Task<HttpResponseMessage> update_request_ca_form()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_form = "";
            string fdca_form_sign = "";
            string fdca_form_sign_user = "";
            string fdca_form_team = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

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
                        fdca_form = provider.FormData.GetValues("request_form").SingleOrDefault();
                        fdca_form_sign = provider.FormData.GetValues("request_form_sign").SingleOrDefault();
                        fdca_form_sign_user = provider.FormData.GetValues("request_form_sign_user").SingleOrDefault();
                        fdca_form_team = provider.FormData.GetValues("request_ca_from_team").SingleOrDefault();
                        request_ca_form obj_data = JsonConvert.DeserializeObject<request_ca_form>(fdca_form);
                        List<request_ca_form_sign> form_signs = JsonConvert.DeserializeObject<List<request_ca_form_sign>>(fdca_form_sign);
                        List<request_form_sign_user> form_sign_users = JsonConvert.DeserializeObject<List<request_form_sign_user>>(fdca_form_sign_user);
                        List<request_ca_form_team> from_teams = JsonConvert.DeserializeObject<List<request_ca_form_team>>(fdca_form_team);

                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                        obj_data.created_by = uid;
                        obj_data.created_date = DateTime.Now;
                        obj_data.created_ip = ip;
                        obj_data.created_token_id = tid;
                        db.Entry(obj_data).State = EntityState.Modified;

                        List<request_ca_form_sign> del_listformsigns = new List<request_ca_form_sign>();
                        List<request_form_sign_user> del_listformsignusers = new List<request_form_sign_user>();
                        List<request_ca_form_team> del_listformteams = new List<request_ca_form_team>();

                        var model_del_signs = db.request_ca_form_sign.Where(a => a.request_form_id == obj_data.request_form_id).ToList();
                        if (model_del_signs.Count > 0)
                        {
                            foreach (var m in model_del_signs)
                            {
                                var model_del_sign_users = db.request_form_sign_user.Where(a => a.request_form_sign_id == m.request_form_sign_id).ToList();
                                if (model_del_sign_users.Count > 0)
                                {
                                    foreach (var m1 in model_del_sign_users)
                                    {
                                        del_listformsignusers.Add(m1);
                                    }
                                }
                                del_listformsigns.Add(m);
                            }
                        }
                        var model_del_teams = db.request_ca_form_team.Where(a => a.request_form_id == obj_data.request_form_id).ToList();
                        if (model_del_teams.Count > 0)
                        {
                            foreach (var m2 in model_del_teams)
                            {
                                del_listformteams.Add(m2);
                            }
                        }
                        db.request_ca_form_team.RemoveRange(del_listformteams);
                        db.request_form_sign_user.RemoveRange(del_listformsignusers);
                        db.request_ca_form_sign.RemoveRange(del_listformsigns);

                        if (form_signs.Count > 0)
                        {
                            List<request_ca_form_sign> listformsigns = new List<request_ca_form_sign>();
                            List<request_form_sign_user> listformsignusers = new List<request_form_sign_user>();
                            foreach (var fs in form_signs)
                            {
                                fs.request_form_sign_id = helper.GenKey();
                                fs.request_team_id = null;
                                fs.request_form_id = obj_data.request_form_id;
                                fs.created_by = uid;
                                fs.created_date = DateTime.Now;
                                fs.created_ip = ip;
                                fs.created_token_id = tid;
                                listformsigns.Add(fs);

                                if (form_sign_users.Count > 0)
                                {
                                    foreach (var fsu in form_sign_users)
                                    {
                                        fsu.request_form_sign_user_id = helper.GenKey();
                                        fsu.request_form_sign_id = fs.request_form_sign_id;
                                        fsu.created_by = uid;
                                        fsu.created_date = DateTime.Now;
                                        fsu.created_ip = ip;
                                        fsu.created_token_id = tid;
                                        listformsignusers.Add(fsu);
                                    }
                                }
                            }
                            if (listformsigns.Count > 0)
                            {
                                db.request_ca_form_sign.AddRange(listformsigns);
                            }
                            if (listformsignusers.Count > 0)
                            {
                                db.request_form_sign_user.AddRange(listformsignusers);
                            }
                        }

                        if (from_teams.Count > 0)
                        {
                            List<request_ca_form_team> listformteams = new List<request_ca_form_team>();
                            foreach (var ft in from_teams)
                            {
                                ft.request_form_team_id = helper.GenKey();
                                ft.request_form_id = obj_data.request_form_id;
                                listformteams.Add(ft);
                            }
                            if (listformteams.Count > 0)
                            {
                                db.request_ca_form_team.AddRange(listformteams);
                            }
                        }
                        db.SaveChanges();

                        #region add request_log
                        if (helper.wlog)
                        {
                            request_log log = new request_log();
                            log.title = "Thêm form " + obj_data.request_form_name;
                            log.log_module = "request_ca_form";
                            log.log_type = 0;
                            log.id_key = obj_data.request_form_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.request_log.Add(log);
                            db.SaveChanges();

                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_form, contents }), domainurl + "request_ca_form/add_request_ca_form", ip, tid, "Lỗi khi thêm form", 0, "request_ca_form");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_form, contents }), domainurl + "request_ca_form/add_request_ca_form", ip, tid, "Lỗi khi thêm form", 0, "request_ca_form");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        public async Task<HttpResponseMessage> update_request_ca_from_team()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_form = "";
            string fdca_form_team = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

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
                        fdca_form = provider.FormData.GetValues("request_form").SingleOrDefault();
                        fdca_form_team = provider.FormData.GetValues("request_ca_from_team").SingleOrDefault();
                        request_ca_form obj_data = JsonConvert.DeserializeObject<request_ca_form>(fdca_form);
                        List<request_ca_form_team> from_teams = JsonConvert.DeserializeObject<List<request_ca_form_team>>(fdca_form_team);

                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";

                        List<request_ca_form_team> del_listformteams = new List<request_ca_form_team>();

                        var model_del_teams = db.request_ca_form_team.Where(a => a.request_form_id == obj_data.request_form_id).ToList();
                        if (model_del_teams.Count > 0)
                        {
                            foreach (var m2 in model_del_teams)
                            {
                                del_listformteams.Add(m2);
                            }
                        }
                        db.request_ca_form_team.RemoveRange(del_listformteams);

                        if (from_teams.Count > 0)
                        {
                            List<request_ca_form_team> listformteams = new List<request_ca_form_team>();
                            foreach (var ft in from_teams)
                            {
                                ft.request_form_team_id = helper.GenKey();
                                ft.request_form_id = obj_data.request_form_id;
                                listformteams.Add(ft);
                            }
                            if (listformteams.Count > 0)
                            {
                                db.request_ca_form_team.AddRange(listformteams);
                            }
                        }
                        db.SaveChanges();

                        //#region add request_log
                        //if (helper.wlog)
                        //{
                        //    request_log log = new request_log();
                        //    log.title = "Thêm request_ca_form_team " + obj_data.request_form_name;
                        //    log.log_module = "request_ca_form";
                        //    log.log_type = 0;
                        //    log.id_key = obj_data.request_form_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.request_log.Add(log);
                        //    db.SaveChanges();

                        //}
                        //#endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_form, contents }), domainurl + "request_ca_form/update_request_ca_from_team", ip, tid, "Lỗi khi thêm form_team", 0, "request_ca_form");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_form, contents }), domainurl + "request_ca_form/update_request_ca_from_team", ip, tid, "Lỗi khi thêm form_team", 0, "request_ca_form");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> delete_request_ca_form([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                        var das = await db.request_ca_form.Where(a => id.Contains(a.request_form_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<request_ca_form> del = new List<request_ca_form>();
                            foreach (var da in das)
                            {
                                del.Add(da);

                                #region add request_log
                                if (helper.wlog)
                                {
                                    request_log log = new request_log();
                                    log.title = "Xóa form " + da.request_form_name;
                                    log.log_module = "request_ca_form";
                                    log.log_type = 2;
                                    log.id_key = da.request_form_id.ToString();
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.request_log.Add(log);
                                    db.SaveChanges();
                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.request_ca_form.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "request_ca_form/delete_request_ca_form", ip, tid, "Lỗi khi xoá form", 0, "request_ca_form");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "request_ca_form/delete_request_ca_form", ip, tid, "Lỗi khi xoá form", 0, "request_ca_form");
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
        [HttpDelete]
        public async Task<HttpResponseMessage> delete_request_ca_form_team([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                        var das = await db.request_ca_form_team.Where(a => id.Contains(a.request_form_team_id)).ToListAsync();
                        if (das != null)
                        {
                            List<request_ca_form_team> del = new List<request_ca_form_team>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.request_ca_form_team.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "request_ca_form/delete_request_ca_form_team", ip, tid, "Lỗi khi xoá request_ca_form_team", 0, "delete_request_ca_form_team");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "request_ca_form/delete_request_ca_form_team", ip, tid, "Lỗi khi xoá request_ca_form_team", 0, "delete_request_ca_form_team");
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

        public async Task<HttpResponseMessage> add_request_ca_formd()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_formd = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

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
                        fdca_formd = provider.FormData.GetValues("request_ca_formd").SingleOrDefault();
                        List<request_ca_formd> obj_datas = JsonConvert.DeserializeObject<List<request_ca_formd>>(fdca_formd);

                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";

                        foreach (var fs in obj_datas)
                        {
                            var formd = db.request_ca_formd.Where(x => x.request_formd_id == fs.request_formd_id).ToList();
                            if(formd.Count > 0)
                            {
                                var formd_update = db.request_ca_formd.Find(fs.request_formd_id);
                                formd_update.is_class = fs.is_class;
                                formd_update.is_label = fs.is_label;
                                formd_update.is_length = fs.is_length;
                                formd_update.is_order = fs.is_order;
                                formd_update.is_parent_id = fs.is_parent_id;
                                formd_update.is_permission = fs.is_permission;
                                formd_update.is_required = fs.is_required;
                                formd_update.is_type = fs.is_type;
                                formd_update.is_width = fs.is_width;
                                formd_update.tudien_id = fs.tudien_id;
                                formd_update.text_key = fs.text_key;
                                formd_update.ten_truong = fs.ten_truong;
                                formd_update.kieu_truong = fs.kieu_truong;
                                formd_update.value_key = fs.value_key;
                                formd_update.lv = (fs.lv != null) ? fs.lv : 1;
                                db.Entry(formd_update).State = EntityState.Modified;
                            }
                            else
                            {
                                db.request_ca_formd.Add(fs);
                            }
                        }
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_formd, contents }), domainurl + "request_ca_form/add_request_ca_formd", ip, tid, "Lỗi khi thêm formd", 0, "add_request_ca_formd");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_formd, contents }), domainurl + "request_ca_form/add_request_ca_formd", ip, tid, "Lỗi khi thêm formd", 0, "add_request_ca_formd");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        public async Task<HttpResponseMessage> change_status_request_form_sign()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_form_sign = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

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
                        fdca_form_sign = provider.FormData.GetValues("request_form_sign").SingleOrDefault();
                        request_ca_form_sign obj_data = JsonConvert.DeserializeObject<request_ca_form_sign>(fdca_form_sign);

                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";

                        obj_data.status = !obj_data.status;
                        db.Entry(obj_data).State = EntityState.Modified;
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_form_sign, contents }), domainurl + "request_ca_form/add_request_ca_formd", ip, tid, "Lỗi khi thêm formd", 0, "add_request_ca_formd");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_form_sign, contents }), domainurl + "request_ca_form/add_request_ca_formd", ip, tid, "Lỗi khi thêm formd", 0, "add_request_ca_formd");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        public async Task<HttpResponseMessage> add_request_ca_form_sign()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_form_sign = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

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
                        fdca_form_sign = provider.FormData.GetValues("request_form_sign").SingleOrDefault();
                        request_ca_form_sign obj_data = JsonConvert.DeserializeObject<request_ca_form_sign>(fdca_form_sign);

                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";

                        obj_data.request_form_sign_id = helper.GenKey();
                        db.request_ca_form_sign.Add(obj_data);
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_form_sign, contents }), domainurl + "request_ca_form/add_request_ca_form_sign", ip, tid, "Lỗi khi thêm nhóm duyệt", 0, "add_request_ca_form_sign");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_form_sign, contents }), domainurl + "request_ca_form/add_request_ca_form_sign", ip, tid, "Lỗi khi thêm nhóm duyệt", 0, "add_request_ca_form_sign");
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
