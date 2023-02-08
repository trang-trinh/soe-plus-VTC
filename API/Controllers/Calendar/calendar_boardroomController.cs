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
    public class calendar_boardroomController : ApiController
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

        [HttpPost]
        public async Task<HttpResponseMessage> filter_calendar_ca_boardroom_list([System.Web.Mvc.Bind(Include = "filter_organization_id,page_no,page_size,search,fields,order_by")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string filter_organization_id = data["filter_organization_id"].ToObject<string>();
            int page_no = data["page_no"].ToObject<int>();
            int page_size = data["page_size"].ToObject<int>();
            string search = data["search"].ToObject<string>();
            List<FieldSQL> fields = data["fields"].ToObject<List<FieldSQL>>();
            string order_by = data["order_by"].ToObject<string>();

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var userNow = db.sys_users.Find(uid);
                    if (userNow != null)
                    {
                        string sqlCount = @"Select Count(br.boardroom_id)total From calendar_ca_boardroom br";
                        string WhereSQL = "";

                        if (userNow.is_super == true)
                        {
                            WhereSQL += "";
                        }
                        else if (!string.IsNullOrWhiteSpace(filter_organization_id))
                        {
                            WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + " (br.organization_id = 0 or br.organization_id = " + int.Parse(filter_organization_id) + ")";
                        }
                        else
                        {
                            WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + " (br.organization_id = 0 or br.organization_id is null)";
                        }
                        WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + "(br.created_by = '" + uid + "' or br.status = 1)";
                        if (!string.IsNullOrWhiteSpace(search))
                        {

                            WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + " (br.law_name like N'%" + search + "%')";

                        }
                        if (fields != null && fields.Count > 0)
                        {
                            foreach (var field in fields)
                            {
                                if (field.filteroperator == "in")
                                {
                                    WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                                }
                                else
                                {
                                    foreach (var m in field.filterconstraints.Where(a => a.value != null))
                                    {
                                        switch (m.matchMode)
                                        {
                                            case "contains":
                                                WhereSQL += " " + field.filteroperator + " (N'" + m.value + "' like N'%' + br." + field.key + " + ',%')";
                                                break;
                                            case "containsMany":
                                                List<string> listKey = m.value.Split(',').ToList();
                                                WhereSQL += " " + field.filteroperator + " (";
                                                foreach (var str in listKey)
                                                {
                                                    if (str.Trim() != "")
                                                    {
                                                        WhereSQL += " ((br." + field.key + " + ',')" + " like N'%' + " + "N'" + str + "' + ',%')  or";
                                                    }
                                                }
                                                if (WhereSQL.EndsWith(" or"))
                                                {
                                                    WhereSQL = WhereSQL.Substring(0, WhereSQL.Length - 3);
                                                }
                                                WhereSQL += ")";
                                                break;
                                            case "equals":
                                                WhereSQL += " " + field.filteroperator + " br." + field.key + " = N'" + m.value + "'";
                                                break;
                                            case "dateBefore":
                                                WhereSQL += " " + field.filteroperator + " CAST(br." + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                                break;
                                            case "dateAfter":
                                                WhereSQL += " " + field.filteroperator + " CAST(br." + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                                break;

                                        }
                                    }
                                }
                            }
                        }
                        if (WhereSQL.StartsWith(" and "))
                        {
                            WhereSQL = WhereSQL.Substring(4);
                        }
                        else if (WhereSQL.StartsWith(" or "))
                        {
                            WhereSQL = WhereSQL.Substring(3);
                        }

                        if (WhereSQL.Trim() != "")
                        {
                            sqlCount += " where " + WhereSQL;
                            sql = @" Select br.*
                                        , us.avatar, us.full_name as created_by_name,
                                        REVERSE(SUBSTRING(REVERSE(us.full_name), 1, CASE WHEN CHARINDEX(' ', REVERSE(us.full_name))>0 THEN CHARINDEX(' ', REVERSE(us.full_name)) - 1 ELSE LEN(REVERSE(us.full_name)) END ) ) as last_name
                                        from calendar_ca_boardroom br
                                        join sys_users us on us.user_id = br.created_by
                                        where " + WhereSQL;
                        }
                        string OFFSET = @"(" + page_no + @") * (" + page_size + @")";
                        sql += @" ORDER BY br." + order_by
                            + @"\n OFFSET " + OFFSET + " ROWS FETCH NEXT " + page_size + " ROWS ONLY ";
                        sql += sqlCount;
                        sql = sql.Replace("--", "").Replace("\r", " ").Replace("   ", " ").Trim();
                        sql = Regex.Replace(sql, "drop", "");
                        sql = Regex.Replace(sql, "update", "");
                        sql = Regex.Replace(sql, "delete", "");
                        sql = Regex.Replace(sql, @"\s+", " ").Trim();
                        var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                        var tables = await task;
                        string JSONresult = JsonConvert.SerializeObject(tables);
                        return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { sql, err = "2", ms = "Bạn không có quyền truy cập chức năng này" });
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/calendar_boardroom/filter_calendar_ca_boardroom_list", ip, tid, "Lỗi khi gọi Filter_Law_Documents", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/calendar_boardroom/filter_calendar_ca_boardroom_list", ip, tid, "Lỗi khi gọi Filter_Law_Documents", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_ca_boardroom()
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
                    calendar_ca_boardroom model = JsonConvert.DeserializeObject<calendar_ca_boardroom>(md);
                    calendar_log log = new calendar_log();

                    if (isAdd)
                    {
                        model.boardroom_id = helper.GenKey();
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        log.message = "Thêm mới phòng họp: " + model.boardroom_name;
                        db.calendar_ca_boardroom.Add(model);
                    }
                    else
                    {
                        model.modified_by = uid;
                        model.modified_date = DateTime.Now;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        db.Entry(model).State = EntityState.Modified;
                        log.message = "Cập nhật phòng họp: " + model.boardroom_name;
                    }
                    #region add law_logs
                    if (helper.wlog)
                    {
                        log.log_type = 2;
                        log.key_id = model.boardroom_id;
                        log.created_by = uid;
                        log.is_view = false;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_boardroom/update_ca_boardroom", ip, tid, "Lỗi khi cập nhật phòng họp", 0, "update_ca_boardroom");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_boardroom/update_ca_boardroom", ip, tid, "Lỗi khi cập nhật phòng họp", 0, "update_ca_boardroom");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_ca_boardroom([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
                    var das = await db.calendar_ca_boardroom.Where(a => ids.Contains(a.boardroom_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<calendar_ca_boardroom> del = new List<calendar_ca_boardroom>();
                        foreach (var da in das)
                        {
                            var check = await db.calendar_week.CountAsync(x => x.boardroom_id == da.boardroom_id);
                            if (check > 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "2", ms = "Tồn tại dữ liệu liên quan, bạn không có quyền xóa dữ liệu." });
                            }
                            del.Add(da);
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                calendar_log log = new calendar_log();
                                log.log_type = 2;
                                log.message = "Xóa phòng họp: " + da.boardroom_name;
                                log.key_id = da.boardroom_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = da.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.calendar_ca_boardroom.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_boardroom/delete_ca_boardroom", ip, tid, "Lỗi khi cập nhật phòng họp họp", 0, "calendar_boardroom");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_boardroom/delete_ca_boardroom", ip, tid, "Lỗi khi xoá phòng họp", 0, "calendar_boardroom");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_status_ca_boardroom([System.Web.Mvc.Bind(Include = "id,status")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
                    var model = await db.calendar_ca_boardroom.FindAsync(id);
                    if (model != null)
                    {
                        model.status = status;
                        #region add cms_logs
                        if (helper.wlog)
                        {
                            calendar_log log = new calendar_log();
                            log.log_type = 2;
                            log.message = "Cập nhât trạng thái phòng họp: " + model.boardroom_name;
                            log.key_id = model.boardroom_id;
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

        [HttpPost]
        public async Task<HttpResponseMessage> import_excel()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }
                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/" + organization_id_user + "/Excel/");
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(strPath);
                    var task = Request.Content.ReadAsMultipartAsync(provider).ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        FileInfo finfo = new FileInfo(provider.FileData.First().LocalFileName);

                        string guid = Guid.NewGuid().ToString();

                        var fileNameTemp = Regex.Replace(finfo.FullName.Replace("\\", "/"), @"\.*/+", "/");
                        var listPathTemp = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        foreach (var item in listPathTemp)
                        {
                            if (item.Trim() != "")
                            {
                                pathConfigTemp += "/" + Path.GetFileName(item);
                            }
                        }
                        File.Move(pathConfigTemp.Substring(1), Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<calendar_ca_boardroom> dvs = new List<calendar_ca_boardroom>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                //var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    calendar_ca_boardroom dv = new calendar_ca_boardroom();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value == null)
                                        {
                                            break;
                                        }
                                        var column = ws.Cells[3, j].Value;
                                        var vl = ws.Cells[i, j].Value;
                                        switch (column)
                                        {
                                            case "boardroom_id":
                                                dv.boardroom_id = helper.GenKey();
                                                break;
                                            case "boardroom_name":
                                                dv.boardroom_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "is_order":
                                                dv.is_order = vl != null ? int.Parse(vl.ToString()) : 1;
                                                break;
                                            case "status":
                                                if (vl.ToString().ToUpper().Trim() == "CÓ" || vl.ToString().ToUpper().Trim() == "HIỂN THỊ") { dv.status = true; }
                                                else { dv.status = false; }
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dv.organization_id = user_now.organization_id;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.calendar_ca_boardroom.AddRange(dvs);
                                    db.SaveChanges();

                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1" + e.Message });
                            }
                            catch (Exception e)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1" + e });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/calendar_boardroom/ImportExcel", ip, tid, "Lỗi khi Import Excel", 0, "calendar_boardroom");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/calendar_boardroom/ImportExcel", ip, tid, "Lỗi khi Import Excel", 0, "calendar_boardroom");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }

    }
}