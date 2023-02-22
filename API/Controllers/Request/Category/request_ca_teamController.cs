using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers.Request.Category
{
    [Authorize(Roles = "login")]
    public class request_ca_teamController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> add_request_ca_team()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_group = "";
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
                        fdca_group = provider.FormData.GetValues("model").SingleOrDefault();
                        request_ca_team obj_data = JsonConvert.DeserializeObject<request_ca_team>(fdca_group);


                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                        obj_data.request_team_id = helper.GenKey();
                        obj_data.organization_id = super ? 0 : int.Parse(dvid);
                        obj_data.created_by = uid;
                        obj_data.created_date = DateTime.Now;
                        obj_data.created_ip = ip;
                        obj_data.created_token_id = tid;
                        db.request_ca_team.Add(obj_data);
                        db.SaveChanges();

                        #region add request_log
                        if (helper.wlog)
                        {
                            request_log log = new request_log();
                            log.title = "Thêm team " + obj_data.request_team_name;
                            log.log_module = "request_ca_team";
                            log.log_type = 0;
                            log.id_key = obj_data.request_team_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_group, contents }), domainurl + "request_ca_team/add_request_ca_team", ip, tid, "Lỗi khi thêm team", 0, "request_ca_team");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_group, contents }), domainurl + "request_ca_team/add_request_ca_team", ip, tid, "Lỗi khi thêm team", 0, "request_ca_team");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_request_ca_team()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_group = "";
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
                        fdca_group = provider.FormData.GetValues("model").SingleOrDefault();
                        request_ca_team obj_data = JsonConvert.DeserializeObject<request_ca_team>(fdca_group);
                        obj_data.modified_by = uid;
                        obj_data.modified_date = DateTime.Now;
                        obj_data.modified_ip = ip;
                        obj_data.modified_token_id = tid;
                        db.Entry(obj_data).State = EntityState.Modified;
                        db.SaveChanges();

                        #region add request_log
                        if (helper.wlog)
                        {
                            request_log log = new request_log();
                            log.title = "Cập nhật team " + obj_data.request_team_name;
                            log.log_module = "request_ca_team";
                            log.log_type = 1;
                            log.id_key = obj_data.request_team_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_group, contents }), domainurl + "request_ca_team/update_request_ca_team", ip, tid, "Lỗi khi cập nhật team", 0, "request_ca_team");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_group, contents }), domainurl + "request_ca_team/update_request_ca_team", ip, tid, "Lỗi khi cập nhật team", 0, "request_ca_team");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_request_ca_team([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                        var das = await db.request_ca_team.Where(a => id.Contains(a.request_team_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<request_ca_team> del = new List<request_ca_team>();
                            foreach (var da in das)
                            {
                                del.Add(da);

                                #region add request_log
                                if (helper.wlog)
                                {
                                    request_log log = new request_log();
                                    log.title = "Xóa team " + da.request_team_name;
                                    log.log_module = "request_ca_team";
                                    log.log_type = 2;
                                    log.id_key = da.request_team_id.ToString();
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
                            db.request_ca_team.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "request_ca_team/delete_request_ca_team", ip, tid, "Lỗi khi xoá team", 0, "request_ca_team");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "request_ca_team/delete_request_ca_team", ip, tid, "Lỗi khi xoá team", 0, "request_ca_team");
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

        [HttpPut]
        public async Task<HttpResponseMessage> update_status_request_ca_team([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var int_id = trangthai.IntID.ToString();
                        var das = db.request_ca_team.FirstOrDefault(a => a.request_team_id == int_id);
                        if (das != null)
                        {
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            das.status = !trangthai.BitTrangthai;

                            #region add request_log
                            if (helper.wlog)
                            {
                                request_log log = new request_log();
                                log.title = "Cập nhật trạng thái team " + das.request_team_name;
                                log.log_module = "request_ca_team";
                                log.log_type = 1;
                                log.id_key = das.request_team_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.request_log.Add(log);
                                //db.SaveChanges();
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "request_ca_team/update_status_request_ca_team", ip, tid, "Lỗi khi cập nhật trạng thái team", 0, "request_ca_team");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "request_ca_team/update_status_request_ca_team", ip, tid, "Lỗi khi cập nhật trạng thái team", 0, "request_ca_team");
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

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_request_ca_team([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(request_team_id) as totalRecords from request_ca_team tm";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " tm.* " +
                    Environment.NewLine + " from request_ca_team tm ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " tm.organization_id is not null " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in (" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( tm." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }
                        if (check == true)
                        {
                            WhereSQLR = field.filteroperator + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }

                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                        + Environment.NewLine + "("
                        + Environment.NewLine + " CONTAINS( tm.request_team_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') "
                        + Environment.NewLine + ")";
                }

                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz
                        + Environment.NewLine + @"ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL
                        + Environment.NewLine + " and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql 
                        + Environment.NewLine + @" ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC"))
                        + Environment.NewLine + ") as tbn ";
                }

                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/request_ca_team/Filter_request_ca_team", ip, tid, "Lỗi khi gọi Filter_request_ca_team", 0, "request_ca_team");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/request_ca_team/Filter_request_ca_team", ip, tid, "Lỗi khi gọi Filter_request_ca_team", 0, "request_ca_team");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> update_request_team_user()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_group = "";
            var requestUser = "";
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
                        fdca_group = provider.FormData.GetValues("model").SingleOrDefault();
                        request_ca_team obj_data = JsonConvert.DeserializeObject<request_ca_team>(fdca_group);

                        requestUser = provider.FormData.GetValues("listUser").SingleOrDefault();
                        List<request_team_user> listUser = JsonConvert.DeserializeObject<List<request_team_user>>(requestUser);

                        List<request_team_user> listUserAdd = new List<request_team_user>();
                        //var userAdd = listUser.Where(x => db.request_team_user.Count(y => y.request_team_id == obj_data.request_team_id && y.request_team_user_id == x.request_team_user_id) == 0).ToList();
                        var userDB = db.request_team_user.AsNoTracking().Where(x => x.request_team_id == obj_data.request_team_id).ToList();
                        var userDel = userDB.Where(x => listUser.Count(y => y.user_id != null && y.user_id == x.user_id) == 0).ToList();
                        var sttUser = 1;
                        foreach (var user in listUser)
                        {
                            var existData = db.request_team_user.FirstOrDefault(x => x.request_team_id == obj_data.request_team_id && x.user_id == user.user_id);
                            if (existData == null)
                            {
                                user.created_by = uid;
                                user.created_date = DateTime.Now;
                                user.created_ip = ip;
                                user.created_token_id = tid;
                                user.is_order = sttUser;
                                user.request_team_id = obj_data.request_team_id;
                                listUserAdd.Add(user);
                            }
                            else
                            {
                                existData.is_order = sttUser;
                                existData.is_type = user.is_type;
                                existData.status = user.status;
                                existData.modified_by = uid;
                                existData.modified_date = DateTime.Now;
                                existData.modified_ip = ip;
                                existData.modified_token_id = tid;
                            }
                            sttUser++;
                        }
                        if (listUserAdd.Count > 0) {
                            db.request_team_user.AddRange(listUserAdd);
                        }
                        if (userDel.Count > 0)
                        {
                            db.request_team_user.RemoveRange(userDel);
                        }
                        db.SaveChanges();

                        #region add request_log
                        if (helper.wlog)
                        {
                            request_log log = new request_log();
                            log.title = "Cập nhật user team " + obj_data.request_team_name;
                            log.log_module = "request_ca_team";
                            log.log_type = 1;
                            log.id_key = obj_data.request_team_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_group, dataUser = requestUser, contents }), domainurl + "request_ca_team/update_request_team_user", ip, tid, "Lỗi khi cập nhật user team", 0, "request_ca_team");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_group, dataUser = requestUser, contents }), domainurl + "request_ca_team/update_request_team_user", ip, tid, "Lỗi khi cập nhật user team", 0, "request_ca_team");
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
