using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using API.Models;
using Helper;
using API.Helper;
using Newtonsoft.Json;
using System.IO;

namespace API.Controllers.Dictionary
{
    [Authorize(Roles = "login")]
    public class role_group_browse_groupController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add_browse_groups()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập duyệt này!", err = "1" });
            }
            try
            {
                string fdstamp;
                string user;
                using (DBEntities db = new DBEntities())
                {

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Stamp";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdstamp = provider.FormData.GetValues("role").SingleOrDefault();
                        user = provider.FormData.GetValues("User").SingleOrDefault();

                        doc_ca_role_groups stamp = JsonConvert.DeserializeObject<doc_ca_role_groups>(fdstamp);
                        stamp.created_by = uid;
                        stamp.created_date = DateTime.Now;
                        stamp.created_ip = ip;
                        stamp.created_token_id = tid;
                        if (stamp.is_default == true)
                        {
                            var oldDef = db.doc_ca_role_groups.Where(x => x.is_default == true).FirstOrDefault();
                            if (oldDef != null)
                            {
                                oldDef.is_default = false;
                            }
                            db.Entry(oldDef).State = EntityState.Modified;
                        }

                        db.doc_ca_role_groups.Add(stamp);
                        if (stamp.is_bydepartment == true)
                        {
                            List<doc_ca_role_group_department> members = JsonConvert.DeserializeObject<List<doc_ca_role_group_department>>(user);
                            List<doc_ca_role_group_department> membersAdd = new List<doc_ca_role_group_department>();
                            List<doc_ca_role_group_department> membersDel = new List<doc_ca_role_group_department>();

                            foreach (var m in members)
                            {
                                if (m.user_id == null)
                                {
                                    var xoa = db.doc_ca_role_group_department.Where(w => w.role_group_id == stamp.role_group_id).FirstOrDefault();
                                    if (xoa != null)
                                    {
                                        membersDel.Add(xoa);
                                    }
                                }
                                else
                                {
                                    m.role_group_id = stamp.role_group_id;
                                    m.created_by = uid;
                                    m.created_date = DateTime.Now;
                                    m.created_ip = ip;
                                    m.created_token_id = tid;
                                    membersAdd.Add(m);
                                    #region addlog
                                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = m.role_group_id, m.user_id, m.created_by, m.created_date, }), domainurl + "role_group_browse_group/Add_browse_groups", ip, tid, "Thêm mới thành viên nhóm duyệt", 1, "Văn bản");
                                    #endregion
                                }
                            }
                            db.doc_ca_role_group_department.AddRange(membersAdd);
                            db.doc_ca_role_group_department.RemoveRange(membersDel);

                        }
                        else
                        {
                            var del = db.doc_ca_role_group_department.Where(x => x.role_group_id == stamp.role_group_id).ToList();
                            if (del.Count > 0)
                            { db.doc_ca_role_group_department.RemoveRange(del); }
                        }
                        db.SaveChanges();
                        #region add sys_logs
                        if (helper.wlog)
                        {
                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = stamp.role_group_id, stamp.role_group_name, stamp.created_by, stamp.created_date, }), domainurl + "role_group_browse_group/Add_browse_groups", ip, tid, "Thêm mới nhóm duyệt", 1, "Văn bản");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "/role_group_browse_group/Add_browse_groups", ip, tid, "Lỗi khi thêm nhóm duyệt", 0, "Nhóm duyệt");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "/role_group_browse_group/Add_browse_groups", ip, tid, "Lỗi khi thêm nhóm duyệt", 0, "Nhóm duyệt");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_browse_groups()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập duyệt này!", err = "1" });
            }
            try
            {
                string fdstamp;
                string user;
                using (DBEntities db = new DBEntities())
                {

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Stamp";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdstamp = provider.FormData.GetValues("role").SingleOrDefault();
                        user = provider.FormData.GetValues("User").SingleOrDefault();

                        doc_ca_role_groups stamp = JsonConvert.DeserializeObject<doc_ca_role_groups>(fdstamp);
                        stamp.modified_by = uid;
                        stamp.modified_date = DateTime.Now;
                        stamp.modified_ip = ip;
                        stamp.modified_token_id = tid;
                        db.Entry(stamp).State = EntityState.Modified;
                        if (stamp.is_bydepartment == true)
                        {

                            List<doc_ca_role_group_department> members = JsonConvert.DeserializeObject<List<doc_ca_role_group_department>>(user);
                            List<doc_ca_role_group_department> membersDel = new List<doc_ca_role_group_department>();
                            foreach (var m in members)
                            {
                                if (m.user_id == null)
                                {
                                    var xoa = db.doc_ca_role_group_department.Where(w => w.role_group_id == stamp.role_group_id).FirstOrDefault();
                                    if (xoa != null)
                                    {
                                        membersDel.Add(xoa);
                                    }
                                }
                                else
                                {
                                    var mem = db.doc_ca_role_group_department.Where(a => a.department_id == m.department_id && a.role_group_id == stamp.role_group_id).FirstOrDefault();
                                    if (mem != null)
                                    {
                                        mem.user_id = m.user_id;
                                        mem.modified_by = uid;
                                        mem.modified_date = DateTime.Now;
                                        mem.modified_ip = ip;
                                        mem.modified_token_id = tid;
                                        db.Entry(mem).State = EntityState.Modified;
                                        #region addlog
                                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = m.role_group_id, m.user_id, m.created_by, m.created_date, }), domainurl + "role_group_browse_group/Updare_browse_groups", ip, tid, "Chỉnh sửa thành viên nhóm duyệt", 1, "Văn bản");
                                        #endregion
                                    }
                                    else
                                    {
                                        m.role_group_id = stamp.role_group_id;
                                        m.created_by = uid;
                                        m.created_date = DateTime.Now;
                                        m.created_ip = ip;
                                        m.created_token_id = tid;
                                        db.doc_ca_role_group_department.Add(m);
                                        #region addlog
                                        sys_logs log = new sys_logs();
                                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = m.role_group_id, m.user_id, m.created_by, m.created_date, }), domainurl + "role_group_browse_group/Updare_browse_groups", ip, tid, "Chỉnh sửa thành viên nhóm duyệt", 1, "Văn bản");
                                        #endregion
                                    }
                                }
                            }
                            if (membersDel != null)
                            {
                                db.doc_ca_role_group_department.RemoveRange(membersDel);
                            }
                        }
                        else
                        {
                            var del = db.doc_ca_role_group_department.Where(x => x.role_group_id == stamp.role_group_id).ToList();
                            if (del.Count > 0)
                            { db.doc_ca_role_group_department.RemoveRange(del); }
                        }
                        db.SaveChanges();
                        #region add logs
                        if (helper.wlog)
                        {
                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = stamp.role_group_id, stamp.role_group_name, stamp.modified_by, stamp.modified_date, }), domainurl + "role_group_browse_group/Updare_browse_groups", ip, tid, "Thêm mới nhóm duyệt", 1, "Văn bản");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "/role_group_browse_group/Updare_browse_groups", ip, tid, "Lỗi khi cập nhật Nhóm duyệt", 0, "Nhóm duyệt");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "/role_group_browse_group/Update_browse_groups", ip, tid, "Lỗi khi cập nhật Nhóm duyệt", 0, "Nhóm duyệt");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_browse_groups([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập duyệt này!", err = "1" });
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
                    var das = await db.doc_ca_role_groups.Where(a => id.Contains(a.role_group_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<doc_ca_role_groups> del = new List<doc_ca_role_groups>();
                        foreach (var da in das)
                        {
                            del.Add(da);
                            var user = db.doc_ca_role_group_users.Where(u => u.role_group_id == da.role_group_id).ToList();
                            var dept = db.doc_ca_role_group_department.Where(u => u.role_group_id == da.role_group_id).ToList();
                            if (user != null)
                            {
                                db.doc_ca_role_group_users.RemoveRange(user);
                            }
                            if (user != null)
                            {
                                db.doc_ca_role_group_department.RemoveRange(dept);
                            }
                            #region add sys_logs
                            if (helper.wlog)
                            {
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = user, dept }), domainurl + "role_group_browse_group/Delete_browse_groups", ip, tid, "Thêm xóa nhóm duyệt và thành viên", 1, "Văn bản");

                            }
                            #endregion
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.doc_ca_role_groups.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "/role_group_browse_group/Delete_browse_groups", ip, tid, "Lỗi khi xoá nhóm duyệt", 0, "Nhóm duyệt");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "/role_group_browse_group/Delete_browse_groups", ip, tid, "Lỗi khi xoá nhóm duyệt", 0, "nhóm duyệt");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_DefaultGroup_Role([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập duyệt này!", err = "1" });
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
                    var oldDef = db.doc_ca_role_groups.Where(x => x.is_default == true).FirstOrDefault();
                    var das = db.doc_ca_role_groups.FirstOrDefault(a => (a.role_group_id == trangthai.IntID));
                    if (das != null)
                    {
                        if (oldDef != null)
                        {
                            oldDef.is_default = trangthai.BitTrangthai;
                            oldDef.modified_by = uid;
                            oldDef.modified_date = DateTime.Now;
                            oldDef.modified_ip = ip;
                            oldDef.modified_token_id = tid;
                        }
                        das.is_default = !trangthai.BitTrangthai;
                        das.modified_by = uid;
                        das.modified_date = DateTime.Now;
                        das.modified_ip = ip;
                        das.modified_token_id = tid;
                        #region add sys_logs
                        if (helper.wlog)
                        {
                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = das.role_group_id, das.role_group_name, das.is_default, }), domainurl + "role_group_browse_group/Update_DefaultGroup_Role", ip, tid, "Cập nhật trạng thái nhóm duyệt", 1, "Văn bản");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "/role_group_browse_group/Update_defaultGroup_role", ip, tid, "Lỗi khi cập nhật trạng thái Nhóm duyệt", 0, "Nhóm duyệt");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "/role_group_browse_group/Update_defaultGroup_role", ip, tid, "Lỗi khi cập nhật trạng thái Nhóm duyệt", 0, "nhóm duyệt");
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
