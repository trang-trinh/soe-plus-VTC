using API.Helper;
using API.Models;
using Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Controllers
{
    [Authorize(Roles = "login")]
    public class GroupController : ApiController
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

        [HttpPost]
        public async Task<HttpResponseMessage> Add_Group([System.Web.Mvc.Bind(Include = "created_by,created_date,created_token_id,created_ip,modified_by,modified_date,modified_token_id,modified_ip,group_name")]sys_group model)
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
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    model.created_by = uid;
                    model.created_date = DateTime.Now;
                    model.created_token_id = tid;
                    model.created_ip = ip;
                    model.modified_by = uid;
                    model.modified_date = DateTime.Now;
                    model.modified_token_id = tid;
                    model.modified_ip = ip;
                    db.sys_group.Add(model);
                    #region  add Log
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents = "" }), domainurl + "Group/Add_Group", ip, tid, "Thêm mới nhóm duyệt (group_name)", 1, "Group");
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "Group/Add_Group", ip, tid, "Lỗi khi thêm nhóm duyệt", 0, "Group");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "Group/Add_Group", ip, tid, "Lỗi khi thêm nhóm duyệt", 0, "Group");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Group([System.Web.Mvc.Bind(Include = "modified_by,modified_date,modified_token_id,modified_ip,group_name")] sys_group model)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
            if (identity == null || !ad)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    model.modified_by = uid;
                    model.modified_date = DateTime.Now;
                    model.modified_token_id = tid;
                    model.modified_ip = ip;
                    db.Entry(model).State = EntityState.Modified;
                    #region  add Log
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents = "" }), domainurl + "Group/Update_Group", ip, tid, "Chỉnh sửa nhóm duyệt (group_name)", 1, "Group");
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "Group/Update_Group", ip, tid, "Lỗi khi cập nhật nhóm duyệt", 0, "Group");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "Group/Update_Group", ip, tid, "Lỗi khi cập nhật nhóm duyệt", 0, "Group");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Del_Group([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //try
            //{
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
                        var das = await db.sys_group.Where(a => ids.Contains(a.group_id)).ToListAsync();
                        if (das != null)
                        {
                            List<sys_group> del = new List<sys_group>();
                            foreach (var da in das)
                            {
                                if (ad)
                                {
                                    del.Add(da);
                                    #region  add Log
                                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "Group/Del_Group", ip, tid, "Xoá nhóm duyệt (group_name)", 1, "Group");
                                    #endregion
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa nhóm này." });
                            }
                            db.sys_group.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Group/Del_Group", ip, tid, "Lỗi khi xoá nhóm duyệt", 0, "Group");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Group/Del_Group", ip, tid, "Lỗi khi xoá nhóm duyệt", 0, "Group");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_statusGroup([System.Web.Mvc.Bind(Include = "ids,tts")] [FromBody] JObject data)
        {
            List<int> ids = data["ids"].ToObject<List<int>>();
            List<bool> tts = data["tts"].ToObject<List<bool>>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //try
            //{
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
                        var das = await db.sys_group.Where(a => ids.Contains(a.group_id)).ToListAsync();
                        if (das != null)
                        {
                            List<sys_group> del = new List<sys_group>();
                            for (int i = 0; i < das.Count; i++)
                            {
                                var da = das[i];
                                if (ad)
                                {
                                    #region  add Log
                                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "Group/Update_statusGroup", ip, tid, "Cập nhật trạng thái nhóm duyệt (group_name)", 1, "Group");
                                    #endregion
                                    da.status = tts[i];
                                }
                            }
                            await db.SaveChangesAsync();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Group/Update_statusGroup", ip, tid, "Lỗi khi cập nhật trạng thái nhóm duyệt", 0, "Group");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Group/Update_statusGroup", ip, tid, "Lỗi khi cập nhật trạng thái nhóm duyệt", 0, "Group");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        #region GroupUser
        [HttpPost]
        public async Task<HttpResponseMessage> Add_GroupUsers([System.Web.Mvc.Bind(Include = "created_date,modified_by,created_token_id,created_ip,modified_date,modified_by,modified_token_id,modified_ip")][FromBody] List<sys_group_users> datas)
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
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var newdatas = datas.Where(a => a.group_user_id == -1);
                    var updatedatas = datas.Where(a => a.group_user_id != -1);
                    foreach (var md in newdatas)
                    {
                        md.created_date = DateTime.Now;
                        md.modified_by = uid;
                        md.created_token_id = tid;
                        md.created_ip = ip;
                        md.modified_date = DateTime.Now;
                        md.modified_by = uid;
                        md.modified_token_id = tid;
                        md.modified_ip = ip;
                    }
                    db.sys_group_users.AddRange(newdatas);
                    foreach (var md in updatedatas)
                    {
                        md.modified_date = DateTime.Now;
                        md.modified_by = uid;
                        md.modified_token_id = tid;
                        md.modified_ip = ip;
                        db.Entry(md).State = EntityState.Modified;
                    }
                    #region  add Log
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = datas, contents = "" }), domainurl + "Group/Add_GroupUsers", ip, tid, "Thêm người dùng cho nhóm (group_id)", 1, "Group");
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = datas, contents }), domainurl + "Group/Add_GroupUsers", ip, tid, "Lỗi thêm người dùng cho nhóm", 0, "Group");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = datas, contents }), domainurl + "Group/Add_GroupUsers", ip, tid, "Lỗi thêm người dùng cho nhóm", 0, "Group");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Del_GroupUsers([System.Web.Mvc.Bind(Include = "")] [FromBody] List<int> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //try
            //{
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
                        db.Configuration.ProxyCreationEnabled = false;
                        db.Configuration.LazyLoadingEnabled = false;
                        var das = await db.sys_group_users.Where(a => ids.Contains(a.group_user_id)).ToListAsync();
                        if (das != null)
                        {
                            List<sys_group_users> del = new List<sys_group_users>();
                            foreach (var da in das)
                            {
                                if (ad || da.modified_by == uid || da.user_id == uid)
                                {
                                    del.Add(da);
                                    #region  add Log
                                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "Group/Del_GroupUsers", ip, tid, "Xoá người dùng trong nhóm (group_id)", 1, "Group");
                                    #endregion
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa lớp này." });
                            }
                            int id = del.FirstOrDefault().group_id;
                            db.sys_group_users.RemoveRange(del);
                            await db.SaveChangesAsync();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Group/Del_GroupUsers", ip, tid, "Lỗi khi xoá người dùng trong nhóm", 0, "Group");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Group/Del_GroupUsers", ip, tid, "Lỗi khi xoá người dùng trong nhóm", 0, "Group");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_GroupUsers([System.Web.Mvc.Bind(Include = "group_user_id,user_id,is_order,status,modified_date,modified_by,modified_token_id,modified_ip")] [FromBody] List<sys_group_users> datas)
        {
            var identity = User.Identity as ClaimsIdentity;

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
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    foreach (var model in datas)
                    {
                        var obj = await db.sys_group_users.FindAsync(model.group_user_id);
                        obj.user_id = model.user_id;
                        obj.is_order = model.is_order;
                        obj.status = model.status;
                        obj.modified_date = DateTime.Now;
                        obj.modified_by = uid;
                        obj.modified_token_id = tid;
                        obj.modified_ip = ip;
                    }
                    #region  add Log
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = datas, contents = "" }), domainurl + "Group/Update_GroupUsers", ip, tid, "Chỉnh sửa thông tin nhóm (group_id)", 1, "Group");
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = datas, contents }), domainurl + "Group/Update_GroupUsers", ip, tid, "Lỗi khi cập nhật thông tin nhóm", 0, "Group");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = datas, contents }), domainurl + "Group/Update_GroupUsers", ip, tid, "Lỗi khi cập nhật thông tin nhóm", 0, "Group");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_statusGroupUsers([System.Web.Mvc.Bind(Include = "ids,tts")] [FromBody] JObject data)
        {
            List<int> ids = data["ids"].ToObject<List<int>>();
            List<bool> tts = data["tts"].ToObject<List<bool>>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //try
            //{
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
                        var das = await db.sys_group_users.Where(a => ids.Contains(a.group_user_id)).ToListAsync();
                        if (das != null)
                        {
                            for (int i = 0; i < das.Count; i++)
                            {
                                var da = das[i];
                                if (ad)
                                {
                                    #region  add Log
                                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "Group/Update_statusGroupUsers", ip, tid, "Cập nhật trạng thái user trong nhóm duyệt (group_id)", 1, "Group");
                                    #endregion
                                    da.status = tts[i];
                                }
                            }
                            await db.SaveChangesAsync();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Group/Update_statusGroupUsers", ip, tid, "Lỗi khi cập nhật trạng thái user trong nhóm duyệt", 0, "Group");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Group/Update_statusGroupUsers", ip, tid, "Lỗi khi cập nhật trạng thái user trong nhóm duyệt", 0, "Group");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        #endregion

        #region Excel
        [HttpPost]
        public async Task<HttpResponseMessage> ImportExcel()
        {
            string ListErr = "";
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Group";
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
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            string fileName = "";
                            if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                            {
                                fileName = Guid.NewGuid().ToString();
                            }
                            fileName = fileData.Headers.ContentDisposition.FileName;
                            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                            {
                                fileName = fileName.Trim('"');
                            }
                            if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                            {
                                fileName = Path.GetFileName(fileName);
                            }
                            if (!fileName.ToLower().Contains(".xls"))
                            {
                                ListErr = "File Excel không đúng định dạng! Kiểm tra lại mẫu Import";
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = ListErr });
                            }
                            var newFileName = Path.Combine(root + "/Import", fileName);
                            var fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                var extensionFile = fileInfo.Extension.ToLower() == ".xls" ? ".xls" : ".xlsx";
                                fileName = fileName.Replace(extensionFile, "");
                                fileName = fileName + helper.ranNumberFile() + extensionFile;

                                newFileName = Path.Combine(root + "/Import", fileName);
                            }
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            File.Move(fileData.LocalFileName, newFileName);
                            FileInfo temp = new FileInfo(newFileName);
                            using (ExcelPackage pck = new ExcelPackage(temp))
                            {
                                List<sys_group> dvs = new List<sys_group>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                var dvcs = db.sys_group.Select(a => new { a.group_id, a.group_name }).ToList();
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    sys_group dv = new sys_group();
                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value == null)
                                        {
                                            break;
                                        }
                                        var column = ws.Cells[3, j].Value;
                                        var vl = ws.Cells[i, j].Value;
                                        if (column != null && vl != null)
                                        {
                                            PropertyInfo propertyInfo = db.sys_group.GetType().GetProperty(column.ToString());
                                            propertyInfo.SetValue(db.sys_group, Convert.ChangeType(vl,
                                            propertyInfo.PropertyType), null);
                                        }
                                    }
                                    if (dvcs.Count(a => a.group_id == dv.group_id || a.group_name == dv.group_name) > 0)
                                        break;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.sys_group.AddRange(dvs);
                                    db.SaveChangesAsync();
                                    File.Delete(newFileName);
                                }
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Group/ImportExcel", ip, tid, "Lỗi khi import nhóm duyệt", 0, "Group");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Group/ImportExcel", ip, tid, "Lỗi khi import nhóm duyệt", 0, "Group");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion
    }
}