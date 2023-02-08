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
    public class calendar_positionController : ApiController
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
        public async Task<HttpResponseMessage> update_ca_position()
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
                    int type = int.Parse(provider.FormData.GetValues("type").SingleOrDefault());
                    var md = provider.FormData.GetValues("ids").SingleOrDefault();
                    List<string> users = JsonConvert.DeserializeObject<List<string>>(md);
                    List<calendar_log> logs = new List<calendar_log>();

                    if (users.Count > 0)
                    {
                        List<calendar_ca_position> positions = new List<calendar_ca_position>();
                        foreach (var user_id in users)
                        {
                            var usr = await db.sys_users.FindAsync(user_id);
                            calendar_log log = new calendar_log();
                            var check = await db.calendar_ca_position.FirstOrDefaultAsync(x => x.user_id == user_id && x.is_type == type && x.organization_id == user_now.organization_id);
                            if (check == null)
                            {
                                calendar_ca_position position = new calendar_ca_position();
                                position.position_code = helper.GenKey();
                                position.user_id = user_id;
                                position.is_type = type;
                                position.status = true;
                                position.created_by = uid;
                                position.created_date = DateTime.Now;
                                position.created_ip = ip;
                                position.created_token_id = tid;
                                position.organization_id = user_now.organization_id;
                                if (type == 0)
                                {
                                    log.message = "Thêm mới người trực ban: " + usr.full_name;
                                }
                                else if(type == 1)
                                {
                                    log.message = "Thêm mới người chỉ huy: " + usr.full_name;
                                }
                                positions.Add(position);

                                #region add law_logs
                                if (helper.wlog)
                                {
                                    log.log_type = 3;
                                    log.key_id = position.position_code;
                                    log.created_by = uid;
                                    log.is_view = false;
                                    log.created_date = DateTime.Now;
                                    log.created_ip = ip;
                                    log.created_token_id = tid;
                                    log.organization_id = position.organization_id;
                                    logs.Add(log);
                                }
                                #endregion
                            }
                        }
                        if (positions.Count > 0)
                        {
                            db.calendar_ca_position.AddRange(positions);
                        }
                        if (logs.Count > 0)
                        {
                            db.calendar_log.AddRange(logs);
                        }
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_ca_position/update_ca_position", ip, tid, "Lỗi khi cập nhật chức vụ", 0, "calendar_ca_position");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_ca_position/update_ca_position", ip, tid, "Lỗi khi cập nhật chức vụ", 0, "calendar_ca_position");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_ca_position([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        var das = await db.calendar_ca_position.Where(a => ids.Contains(a.position_code)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<calendar_ca_position> del = new List<calendar_ca_position>();
                            foreach (var da in das)
                            {
                                var usr = await db.sys_users.FindAsync(da.user_id);
                                del.Add(da);
                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    calendar_log log = new calendar_log();
                                    log.log_type = 3;
                                    if (da.is_type == 0)
                                    {
                                        log.message = "Xóa người trực ban: " + usr.full_name;
                                    }
                                    else if(da.is_type == 1)
                                    {
                                        log.message = "Xóa người chỉ huy: " + usr.full_name;
                                    }
                                    log.key_id = da.position_code;
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
                            db.calendar_ca_position.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_position/delete_ca_position", ip, tid, "Lỗi khi xoá chức vụ", 0, "calendar_position");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_position/delete_ca_position", ip, tid, "Lỗi khi xoá chức vụ", 0, "calendar_position");
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
        public async Task<HttpResponseMessage> update_status_ca_position([System.Web.Mvc.Bind(Include = "id,status")][FromBody] JObject data)
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
                        var model = await db.calendar_ca_position.FindAsync(id);
                        if (model != null)
                        {
                            var usr = await db.sys_users.FindAsync(model.user_id);
                            model.status = status;
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                calendar_log log = new calendar_log();
                                log.log_type = 2;
                                if (model.is_type == 0)
                                {
                                    log.message = "Cập nhât trạng người trực ban: " + usr.full_name;
                                }
                                else if(model.is_type == 1)
                                {
                                    log.message = "Cập nhât trạng người chỉ huy: " + usr.full_name;
                                }
                                log.key_id = model.position_code;
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_position/update_status_ca_position", ip, tid, "Lỗi khi cập nhật phòng họp", 0, "calendar_position");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_position/update_status_ca_position", ip, tid, "Lỗi khi cập nhật phòng họp", 0, "calendar_position");
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
                        int type = int.Parse(provider.FormData.GetValues("type").SingleOrDefault());
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        FileInfo finfo = new FileInfo(provider.FileData.First().LocalFileName);

                        string guid = Guid.NewGuid().ToString();

                        File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<calendar_ca_position> dvs = new List<calendar_ca_position>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                //var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    calendar_ca_position dv = new calendar_ca_position();

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
                                            case "position_code":
                                                dv.position_code = helper.GenKey();
                                                break;
                                            case "user_id":
                                                var check = db.calendar_ca_position.FirstOrDefault(x => x.user_id == vl && x.organization_id == user_now.organization_id);
                                                if (check == null)
                                                {
                                                    break;
                                                }
                                                dv.user_id = vl != null ? vl.ToString() : null;
                                                break;
                                            case "is_order":
                                                dv.is_order = vl != null ? int.Parse(vl.ToString()) : 1;
                                                break;
                                            case "status":
                                                if (vl.ToString().ToUpper().Trim() == "CÓ" || vl.ToString().ToUpper().Trim() == "HIỂN THỊ" || vl.ToString().ToUpper().Trim() == "KÍCH HOẠT") { dv.status = true; }
                                                else { dv.status = false; }
                                                break;
                                        }
                                    }
                                    dv.is_type = type;
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dv.organization_id = user_now.organization_id;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.calendar_ca_position.AddRange(dvs);
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/calendar_position/ImportExcel", ip, tid, "Lỗi khi Import Excel", 0, "calendar_position");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/calendar_position/ImportExcel", ip, tid, "Lỗi khi Import Excel", 0, "calendar_position");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_ca_mission()
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
                    int type = int.Parse(provider.FormData.GetValues("type").SingleOrDefault());
                    string mission = provider.FormData.GetValues("mission").SingleOrDefault();
                    string address = provider.FormData.GetValues("address").SingleOrDefault();
                    var md = provider.FormData.GetValues("model").SingleOrDefault();
                    calendar_ca_mission model = JsonConvert.DeserializeObject<calendar_ca_mission>(md);
                    
                    if (model.mission_id == "-1" || model.mission_id == "" || model.mission_id == null)
                    {
                        model.mission_id = helper.GenKey();
                        model.modified_by = uid;
                        model.modified_date = DateTime.Now;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        if (type == 0)
                        {
                            model.mission = mission;
                        }
                        else if (type == 1)
                        {
                            model.address = address;
                        }
                        db.calendar_ca_mission.Add(model);
                    }
                    else
                    {
                        if (type == 0)
                        {
                            model.mission = mission;
                        }
                        else if (type == 1)
                        {
                            model.address = address;
                        }
                        model.modified_by = uid;
                        model.modified_date = DateTime.Now;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        db.Entry(model).State = EntityState.Modified;
                    }
                    #region file
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string path = root + "/" + model.organization_id + "/Calendar/Mission/" + model.mission_id;
                    bool exists = Directory.Exists(path);
                    if (!exists)
                        Directory.CreateDirectory(path);
                    foreach (MultipartFileData fileData in provider.FileData)
                    {
                        string org_name_file = fileData.Headers.ContentDisposition.FileName;
                        if (org_name_file.StartsWith("\"") && org_name_file.EndsWith("\""))
                        {
                            org_name_file = org_name_file.Trim('"');
                        }
                        if (org_name_file.Contains(@"/") || org_name_file.Contains(@"\"))
                        {
                            org_name_file = System.IO.Path.GetFileName(org_name_file);
                        }
                        string name_file = org_name_file; //helper.UniqueFileName(org_name_file);
                        string rootPath = path + "/" + name_file;
                        string Duongdan = "/Portals/" + model.organization_id + "/Calendar/Mission/" + model.mission_id + "/" + name_file;
                        string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                        if (rootPath.Length > 260)
                        {
                            name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                            int le = 260 - (path.Length + 1) - Dinhdang.Length;
                            name_file = name_file.Substring(0, le) + Dinhdang;
                        }
                        if (File.Exists(rootPath))
                        {
                            File.Delete(rootPath);
                        }
                        File.Move(fileData.LocalFileName, rootPath);
                        //File.Copy(fileData.LocalFileName, rootPathFile, true);

                        string path_signature_old = null;
                        string path_stamp_old = null;
                        var modelold = await db.calendar_ca_mission.FindAsync(model.mission_id);
                        if (modelold != null)
                        {
                            path_signature_old = modelold.path_signature;
                            path_stamp_old = modelold.path_stamp;
                        }
                        if (fileData.Headers.ContentDisposition.Name == "\"signature\"")
                        {
                            model.path_signature = Duongdan;
                            if (!string.IsNullOrWhiteSpace(path_signature_old) && path_signature_old.Contains("Portals"))
                            {
                                var strPath = root + "\\" + path_signature_old;
                                bool ex = System.IO.Directory.Exists(strPath);
                                if (ex)
                                {
                                    System.IO.Directory.Delete(strPath, true);
                                }
                            }
                        }
                        if (fileData.Headers.ContentDisposition.Name == "\"stamp\"")
                        {
                            model.path_stamp = Duongdan;
                            if (!string.IsNullOrWhiteSpace(path_signature_old) && path_signature_old.Contains("Portals"))
                            {
                                var strPath = root + "\\" + path_signature_old;
                                bool ex = System.IO.Directory.Exists(strPath);
                                if (ex)
                                {
                                    System.IO.Directory.Delete(strPath, true);
                                }
                            }
                        }
                    }
                    #endregion
                    #region add law_logs
                    if (helper.wlog)
                    {
                        calendar_log log = new calendar_log();
                        if (type == 0)
                        {
                            log.message = "Cập nhật nhiệm vụ trực ban";
                        }
                        else if (type == 1)
                        {
                            log.message = "Cập nhật nơi nhận";
                        }
                        log.log_type = 3;
                        log.key_id = model.mission_id;
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
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", model.path_signature, model.path_stamp });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_ca_position/update_ca_mission", ip, tid, "Lỗi khi cập nhật chức vụ", 0, "calendar_ca_position");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_ca_position/update_ca_mission", ip, tid, "Lỗi khi cập nhật chức vụ", 0, "calendar_ca_position");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

    }
}