using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Models;
using Helper;
using API.Helper;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class ca_groupsController : ApiController
    {

        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;

        }


        [HttpPost]
        public async Task<HttpResponseMessage> Add_groups()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
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
                        string tempGroup = provider.FormData.GetValues("group").SingleOrDefault();
                        string tempDept = provider.FormData.GetValues("current").SingleOrDefault();
                        doc_ca_groups ca_Groups = JsonConvert.DeserializeObject<doc_ca_groups>(tempGroup);
                        ca_Groups.created_date = DateTime.Now;
                        ca_Groups.created_by = uid;
                        ca_Groups.created_ip = ip;
                        ca_Groups.created_token_id = tid;
                        db.doc_ca_groups.Add(ca_Groups);

                        if (tempDept != null)
                        {
                            List<doc_ca_group_departments> doc_Ca_Groups_Departments = JsonConvert.DeserializeObject<List<doc_ca_group_departments>>(tempDept);
                            if (doc_Ca_Groups_Departments.Count > 0)
                            {
                                foreach (var doc in doc_Ca_Groups_Departments)
                                {
                                    doc.doc_group_id = ca_Groups.doc_group_id;
                                    doc.created_date = DateTime.Now;
                                    doc.created_by = uid;
                                    doc.created_ip = ip;
                                    doc.created_token_id = tid;
                                }
                                db.doc_ca_group_departments.AddRange(doc_Ca_Groups_Departments);
                            }
                        }

                        db.SaveChanges();
                        #region add log
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "" }), domainurl + "ca_group/addGroup", ip, tid, "Thêm mới nhóm văn bản", 1, "Từ điển chung");
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ca_groups/Add_ca_groups", ip, tid, "Lỗi khi thêm nhóm", 0, "ca_groups");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "ca_groups/Add_ca_groups", ip, tid, "Lỗi khi thêm nhóm", 0, "ca_groups");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_groups()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
            if (identity == null)
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
                        string tempGroup = provider.FormData.GetValues("group").SingleOrDefault();
                        string tempDept = provider.FormData.GetValues("current").SingleOrDefault();
                        doc_ca_groups ca_Groups = JsonConvert.DeserializeObject<doc_ca_groups>(tempGroup);
                        ca_Groups.modified_date = DateTime.Now;
                        ca_Groups.modified_by = uid;
                        ca_Groups.modified_ip = ip;
                        ca_Groups.modified_token_id = tid;
                        db.Entry(ca_Groups).State = EntityState.Modified;

                        if (tempDept != null)
                        {
                            var old = db.doc_ca_group_departments.Where(x => x.doc_group_id == ca_Groups.doc_group_id).ToList();
                            if (old.Count > 0)
                            {
                                db.doc_ca_group_departments.RemoveRange(old); db.SaveChanges();
                            }
                            List<doc_ca_group_departments> doc_Ca_Groups_Departments = JsonConvert.DeserializeObject<List<doc_ca_group_departments>>(tempDept);
                            if (doc_Ca_Groups_Departments.Count > 0)
                            {

                                foreach (var doc in doc_Ca_Groups_Departments)
                                {

                                    doc.doc_group_id = ca_Groups.doc_group_id;
                                    doc.created_date = DateTime.Now;
                                    doc.created_by = uid;
                                    doc.created_ip = ip;
                                    doc.created_token_id = tid;
                                    db.doc_ca_group_departments.Add(doc);
                                }
                            }
                        }

                        db.SaveChanges();
                        #region add log
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "" }), domainurl + "ca_group/UpdateGroup", ip, tid, "Sửa nhóm văn bản", 1, "Từ điển chung");
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "ca_groups/Update_ca_group", ip, tid, "Lỗi khi cập nhật Nhóm", 0, "ca_groups");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "ca_groups/Update_ca_group", ip, tid, "Lỗi khi cập nhật Nhóm", 0, "ca_groups");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_groups([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

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
                    var das = await db.doc_ca_groups.Where(a => id.Contains(a.doc_group_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<doc_ca_groups> del = new List<doc_ca_groups>();


                        foreach (var da in das)
                        {
                            var delDpt = db.doc_ca_group_departments.Where(x => x.doc_group_id == da.doc_group_id).ToList();
                            del.Add(da);
                            db.doc_ca_group_departments.RemoveRange(delDpt);
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                cms_logs log = new cms_logs();
                                log.log_title = "Xóa nhóm" + da.doc_group_name;

                                log.log_module = "Nhóm";
                                log.id_key = da.doc_group_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.cms_logs.Add(log);
                                db.SaveChanges();

                            }
                            #endregion
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.doc_ca_groups.RemoveRange(del);

                    }
                    await db.SaveChangesAsync();


                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "ca_groups/Delete_ca_groups", ip, tid, "Lỗi khi xoá nhóm", 0, "ca_groups");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "ca_groups/Delete_ca_groups", ip, tid, "Lỗi khi xoá nhóm", 0, "ca_groups");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_StatusGroups([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

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
                    var das = db.doc_ca_groups.FirstOrDefault(a => (a.doc_group_id == trangthai.IntID));
                    if (das != null)
                    {
                        das.modified_by = uid;
                        das.modified_date = DateTime.Now;
                        das.modified_ip = ip;
                        das.modified_token_id = tid;
                        das.status = !trangthai.BitTrangthai;

                        #region add cms_logs
                        if (helper.wlog)
                        {

                            cms_logs log = new cms_logs();
                            log.log_title = "Sửa trạng thái nhóm" + das.doc_group_name;
                            log.log_module = "Nhóm";
                            log.id_key = das.doc_group_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.cms_logs.Add(log);
                            db.SaveChanges();

                        }
                        #endregion
                        await db.SaveChangesAsync();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "ca_groups/Update_StatusGroup", ip, tid, "Lỗi khi cập nhật trạng thái Nhóm", 0, "ca_groups");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "ca_groups/Update_StatusGroup", ip, tid, "Lỗi khi cập nhật trạng thái Nhóm", 0, "ca_groups");
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