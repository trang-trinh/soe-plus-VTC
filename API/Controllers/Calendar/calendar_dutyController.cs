using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Helper;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.IO;
using Microsoft.ApplicationBlocks.Data;
using GemBox.Document;
using System.Text;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace API.Controllers.Calendar
{
    [Authorize(Roles = "login")]
    public class calendar_dutyController : ApiController
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

        #region Update Calendar
        [HttpPut]
        public  async Task<HttpResponseMessage> update_calendar_duty()
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
                    List<Notification> notifications = new List<Notification>();
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    bool isAdd = bool.Parse(provider.FormData.GetValues("isAdd").SingleOrDefault());
                    var md = provider.FormData.GetValues("model").SingleOrDefault();
                    calendar_duty model = JsonConvert.DeserializeObject<calendar_duty>(md);
                    string trucban = provider.FormData.GetValues("trucban").SingleOrDefault();
                    string chihuy = provider.FormData.GetValues("chihuy").SingleOrDefault();
                    #region check
                    //bool coincide = db.calendar_duty.Count(x => x.date_timelot == model.date_timelot && x.organization_id == user_now.organization_id) > 0;
                    //if (isAdd && coincide)
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.OK, new { err = "2" , ms = ("Trùng lịch trực ban ngày (" + model.date_timelot?.ToString("dd/MM/yyyy") + "), ca trực (" + (model.is_timelot == 0 ? "Sáng" : model.is_timelot == 1 ? "Chiều" : "Cả ngày") + ")!") });
                    //}
                    #endregion
                    #region model
                    calendar_log log = new calendar_log();
                    if (isAdd)
                    {
                        model.calendar_duty_id = helper.GenKey();
                        if (!string.IsNullOrEmpty(model.contents))
                        {
                            model.contents_en = helper.convertToUnSign3(model.contents);
                        }
                        model.is_order = db.calendar_duty.Count() + 1;
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        log.message = "Thêm mới: " + model.contents;
                        db.calendar_duty.Add(model);
                    }
                    else
                    {
                        model.modified_by = uid;
                        model.modified_date = DateTime.Now;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        db.Entry(model).State = EntityState.Modified;
                        log.message = "Cập nhật: " + model.contents;
                    }
                    #endregion
                    #region Member
                    var old = await db.calendar_duty_person.Where(x => x.calendar_duty_id == model.calendar_duty_id).ToListAsync();
                    if (old.Count > 0)
                    {
                        db.calendar_duty_person.RemoveRange(old);
                    }
                    List<calendar_duty_person> members = new List<calendar_duty_person>();
                    if (!string.IsNullOrEmpty(trucban))
                    {
                        calendar_duty_person mb = new calendar_duty_person();
                        mb.duty_person_id = helper.GenKey();
                        mb.calendar_duty_id = model.calendar_duty_id;
                        mb.user_id = trucban;
                        mb.is_type = 0;
                        mb.status = true;
                        mb.is_order = 0;
                        mb.created_by = uid;
                        mb.created_date = DateTime.Now;
                        mb.created_ip = ip;
                        mb.created_token_id = tid;
                        members.Add(mb);
                    }
                    if (!string.IsNullOrEmpty(chihuy))
                    {
                        calendar_duty_person mb = new calendar_duty_person();
                        mb.duty_person_id = helper.GenKey();
                        mb.calendar_duty_id = model.calendar_duty_id;
                        mb.user_id = chihuy;
                        mb.is_type = 1;
                        mb.status = true;
                        mb.is_order = 1;
                        mb.created_by = uid;
                        mb.created_date = DateTime.Now;
                        mb.created_ip = ip;
                        mb.created_token_id = tid;
                        members.Add(mb);
                    }
                    if (members.Count > 0)
                    {
                        db.calendar_duty_person.AddRange(members);
                    }
                    #endregion
                    #region Trùng lịch
                    var coincides = await db.calendar_duty_coincide.Where(x => x.calendar_duty_id == model.calendar_duty_id || x.calendar_duty_coincide_id == model.calendar_duty_id).ToListAsync();
                    if (coincides.Count > 0)
                    {
                        db.calendar_duty_coincide.RemoveRange(coincides);
                    }
                    var coincide_news = await db.calendar_duty.Where(x => x.organization_id == user_now.organization_id && x.calendar_duty_id != model.calendar_duty_id && x.date_timelot == model.date_timelot).ToListAsync();
                    coincides = new List<calendar_duty_coincide>();
                    var check_coincides = coincide_news;
                    //Trùng phòng họp
                    if (!string.IsNullOrEmpty(model.boardroom_id))
                    {
                        check_coincides = coincide_news.Where(a => a.boardroom_id == model.boardroom_id).ToList();
                        foreach (calendar_duty calendar in check_coincides)
                        {
                            calendar_duty_coincide coincide = new calendar_duty_coincide();
                            coincide.coincide_id = helper.GenKey();
                            coincide.calendar_duty_id = calendar.calendar_duty_id;
                            coincide.calendar_duty_coincide_id = model.calendar_duty_id;
                            coincide.is_type = 0;
                            coincide.contents = "Trùng địa điểm: " + (await db.calendar_ca_boardroom.FindAsync(model.boardroom_id))?.boardroom_name;
                            coincide.created_by = uid;
                            coincide.created_date = DateTime.Now;
                            coincide.created_ip = ip;
                            coincide.created_token_id = tid;
                            coincides.Add(coincide);
                        }
                    }
                    //Trùng người tham gia
                    if (members.Count > 0)
                    {
                        var id_coincides = coincide_news.Select(x => x.calendar_duty_id).ToList();
                        var member_coincides = await db.calendar_duty_person.Where(a => id_coincides.Contains(a.calendar_duty_id)).Select(a => new { a.calendar_duty_id, a.user_id, a.is_type }).ToListAsync();
                        foreach (var mb in members)
                        {
                            var usr = await db.sys_users.FindAsync(mb.user_id);
                            var check_member_coincides = member_coincides.Where(x => x.user_id == mb.user_id).ToList();
                            foreach (var calendar in check_member_coincides)
                            {
                                calendar_duty_coincide coincide = new calendar_duty_coincide();
                                coincide.coincide_id = helper.GenKey();
                                coincide.calendar_duty_id = calendar.calendar_duty_id;
                                coincide.calendar_duty_coincide_id = model.calendar_duty_id;
                                if (calendar.is_type == 0)
                                {
                                    coincide.is_type = 1;
                                    coincide.contents = "Trùng người trực ban: " + usr?.full_name;
                                }
                                else if (calendar.is_type == 1)
                                {
                                    coincide.is_type = 2;
                                    coincide.contents = "Trùng trực chỉ huy: " + usr?.full_name;
                                }
                                coincide.created_by = uid;
                                coincide.created_date = DateTime.Now;
                                coincide.created_ip = ip;
                                coincide.created_token_id = tid;
                                coincides.Add(coincide);
                            }
                        }
                    }
                    if (coincides.Count > 0)
                    {
                        db.calendar_duty_coincide.AddRange(coincides);
                    }
                    #endregion
                    #region File
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string path = root + "/" + model.organization_id + "/Calendar/" + model.calendar_duty_id;
                    bool exists = Directory.Exists(path);
                    if (!exists)
                        Directory.CreateDirectory(path);
                    List<calendar_file> dfs = new List<calendar_file>();
                    foreach (MultipartFileData fileData in provider.FileData)
                    {
                        var fileold = await db.calendar_file.Where(x => x.calendar_duty_id == model.calendar_duty_id && x.is_type == 4).ToListAsync();
                        if (fileold.Count > 0)
                        {
                            foreach(var f in fileold)
                            {
                                //var rp = root + "/" + f.file_path;
                                //if (System.IO.File.Exists(rp))
                                //{
                                //    System.IO.File.Delete(rp);
                                //}
                                // Format f.file_path
                                var pathFormat = Regex.Replace(f.file_path.Replace("\\", "/"), @"\.*/+", "/");
                                var listPath = pathFormat.Split('/');
                                var pathConfig = "";
                                foreach (var item in listPath)
                                {
                                    if (item.Trim() != "")
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                if (System.IO.File.Exists(root + pathConfig))
                                {
                                    System.IO.File.Delete(root + pathConfig);
                                }
                            }
                            db.calendar_file.RemoveRange(fileold);
                        }

                        string org_name_file = fileData.Headers.ContentDisposition.FileName;
                        if (org_name_file.StartsWith("\"") && org_name_file.EndsWith("\""))
                        {
                            org_name_file = org_name_file.Trim('"');
                        }
                        if (org_name_file.Contains(@"/") || org_name_file.Contains(@"\"))
                        {
                            org_name_file = System.IO.Path.GetFileName(org_name_file);
                        }
                        string name_file = org_name_file; // helper.UniqueFileName(org_name_file);
                        string rootPath = path + "/" + name_file;
                        string Duongdan = "/Portals/" + model.organization_id + "/Calendar/" + model.calendar_duty_id + "/" + name_file;
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
                        var df = new calendar_file();
                        df.file_id = helper.GenKey();
                        df.calendar_duty_id = model.calendar_duty_id;
                        df.file_name = name_file;
                        df.file_path = Duongdan;
                        df.file_type = Dinhdang;
                        var file_info = new FileInfo(rootPath);
                        df.file_size = file_info.Length;
                        df.is_image = helper.IsImageFileName(name_file);
                        if (df.is_image == true)
                        {
                            //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                        }
                        df.is_type = 4;
                        df.status = true;
                        df.created_by = uid;
                        df.created_date = DateTime.Now;
                        df.created_ip = ip;
                        df.created_token_id = tid;
                        dfs.Add(df);
                    }
                    if (dfs.Count > 0)
                    {
                        db.calendar_file.AddRange(dfs);
                    }
                    #endregion
                    #region logs
                    if (helper.wlog)
                    {
                        log.log_type = 1;
                        log.key_id = model.calendar_duty_id;
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
                    #region Send Message
                    if (model.status == 2)
                    {
                        //Send Message
                        string sendTitle = "Lịch trực ban";
                        string sendContent = "Vừa cập nhật lịch trực ngày: \"" + model.date_timelot?.ToString("MM/dd/yyyy") + "\".";
                        //Notify tất cả user tham gia lịch
                        string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                        var sqlpas = new List<SqlParameter>();
                        sqlpas.Add(new SqlParameter("@" + "calendar_duty_id", model.calendar_duty_id));
                        var arrpas = sqlpas.ToArray();
                        var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_duty_member_get", arrpas).Tables);
                        var tables = await tasks;
                        List<temp> tempss = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                        List<string> sendUsers = tempss.Select(x => x.user_id).ToList();
                        send_message(uid, model.calendar_duty_id, sendUsers, sendTitle, sendContent, 2);
                        notifications.Add(new Notification()
                        {
                            calendar_id = model.calendar_duty_id,
                            uids = sendUsers,
                            title = sendTitle,
                            text = sendContent,
                        });
                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.SerializeObject(notifications) });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/update_calendar_duty", ip, tid, "Lỗi khi cập nhật lịch trực ban", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/update_calendar_duty", ip, tid, "Lỗi khi cập nhật lịch trực ban", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_calendar_duty_multiple()
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
                    List<calendar_duty> multiple = JsonConvert.DeserializeObject<List<calendar_duty>>(provider.FormData.GetValues("multiple").SingleOrDefault());
                    List<calendar_duty_person> mbs = JsonConvert.DeserializeObject<List<calendar_duty_person>>(provider.FormData.GetValues("members").SingleOrDefault());

                    foreach (var model in multiple)
                    {
                        var key_id = model.calendar_duty_id;
                        #region Model
                        calendar_log log = new calendar_log();
                        model.calendar_duty_id = helper.GenKey();
                        if (!string.IsNullOrEmpty(model.contents))
                        {
                            model.contents_en = helper.convertToUnSign3(model.contents);
                        }
                        model.is_order = db.calendar_duty.Count() + 1;
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        log.message = "Thêm mới: " + model.contents;
                        db.calendar_duty.Add(model);
                        #endregion
                        #region Member
                        List<calendar_duty_person> members = new List<calendar_duty_person>();
                        if (mbs.Count > 0)
                        {
                            var filtermbs = mbs.Where(x => x.calendar_duty_id == key_id).ToList();
                            int stt = 0;
                            foreach (var user in filtermbs)
                            {
                                calendar_duty_person mb = new calendar_duty_person();
                                mb.duty_person_id = helper.GenKey();
                                mb.calendar_duty_id = model.calendar_duty_id;
                                mb.user_id = user.user_id;
                                mb.is_type = user.is_type;
                                mb.status = true;
                                mb.is_order = stt++;
                                mb.created_by = uid;
                                mb.created_date = DateTime.Now;
                                mb.created_ip = ip;
                                mb.created_token_id = tid;
                                members.Add(mb);
                            }
                        }
                        if (members.Count > 0)
                        {
                            db.calendar_duty_person.AddRange(members);
                        }
                        #endregion
                        #region Trùng lịch
                        var coincides = await db.calendar_duty_coincide.Where(x => x.calendar_duty_id == model.calendar_duty_id || x.calendar_duty_coincide_id == model.calendar_duty_id).ToListAsync();
                        if (coincides.Count > 0)
                        {
                            db.calendar_duty_coincide.RemoveRange(coincides);
                        }
                        var coincide_news = await db.calendar_duty.Where(x => x.organization_id == user_now.organization_id && x.calendar_duty_id != model.calendar_duty_id && x.date_timelot == model.date_timelot).ToListAsync();
                        coincides = new List<calendar_duty_coincide>();
                        var check_coincides = coincide_news;
                        //Trùng phòng họp
                        if (!string.IsNullOrEmpty(model.boardroom_id))
                        {
                            check_coincides = coincide_news.Where(a => a.boardroom_id == model.boardroom_id).ToList();
                            foreach (calendar_duty calendar in check_coincides)
                            {
                                calendar_duty_coincide coincide = new calendar_duty_coincide();
                                coincide.coincide_id = helper.GenKey();
                                coincide.calendar_duty_id = calendar.calendar_duty_id;
                                coincide.calendar_duty_coincide_id = model.calendar_duty_id;
                                coincide.is_type = 0;
                                coincide.contents = "Trùng địa điểm: " + (await db.calendar_ca_boardroom.FindAsync(model.boardroom_id))?.boardroom_name;
                                coincide.created_by = uid;
                                coincide.created_date = DateTime.Now;
                                coincide.created_ip = ip;
                                coincide.created_token_id = tid;
                                coincides.Add(coincide);
                            }
                        }
                        //Trùng người tham gia
                        if (members.Count > 0)
                        {
                            var id_coincides = coincide_news.Select(x => x.calendar_duty_id).ToList();
                            var member_coincides = await db.calendar_duty_person.Where(a => id_coincides.Contains(a.calendar_duty_id)).Select(a => new { a.calendar_duty_id, a.user_id, a.is_type }).ToListAsync();
                            foreach (var mb in members)
                            {
                                var usr = await db.sys_users.FindAsync(mb.user_id);
                                var check_member_coincides = member_coincides.Where(x => x.user_id == mb.user_id).ToList();
                                foreach (var calendar in check_member_coincides)
                                {
                                    calendar_duty_coincide coincide = new calendar_duty_coincide();
                                    coincide.coincide_id = helper.GenKey();
                                    coincide.calendar_duty_id = calendar.calendar_duty_id;
                                    coincide.calendar_duty_coincide_id = model.calendar_duty_id;
                                    if (calendar.is_type == 0)
                                    {
                                        coincide.is_type = 1;
                                        coincide.contents = "Trùng người trực ban: " + usr?.full_name;
                                    }
                                    else if (calendar.is_type == 1)
                                    {
                                        coincide.is_type = 2;
                                        coincide.contents = "Trùng trực chỉ huy: " + usr?.full_name;
                                    }
                                    coincide.created_by = uid;
                                    coincide.created_date = DateTime.Now;
                                    coincide.created_ip = ip;
                                    coincide.created_token_id = tid;
                                    coincides.Add(coincide);
                                }
                            }
                        }
                        if (coincides.Count > 0)
                        {
                            db.calendar_duty_coincide.AddRange(coincides);
                        }
                        #endregion
                        #region file
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        string path = root + "/" + model.organization_id + "/Calendar/" + model.calendar_duty_id;
                        bool exists = Directory.Exists(path);
                        if (!exists)
                            Directory.CreateDirectory(path);
                        List<calendar_file> dfs = new List<calendar_file>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            if (fileData.Headers.ContentDisposition.Name == "\"files" + key_id + "\"")
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
                                string Duongdan = "/Portals/" + model.organization_id + "/Calendar/" + model.calendar_duty_id + "/" + name_file;
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
                                var df = new calendar_file();
                                df.file_id = helper.GenKey();
                                df.calendar_duty_id = model.calendar_duty_id;
                                df.file_name = name_file;
                                df.file_path = Duongdan;
                                df.file_type = Dinhdang;
                                var file_info = new FileInfo(rootPath);
                                df.file_size = file_info.Length;
                                df.is_image = helper.IsImageFileName(name_file);
                                if (df.is_image == true)
                                {
                                    //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                }
                                df.is_type = 0;
                                df.status = true;
                                df.created_by = uid;
                                df.created_date = DateTime.Now;
                                df.created_ip = ip;
                                df.created_token_id = tid;
                                dfs.Add(df);
                            }
                        }
                        if (dfs.Count > 0)
                        {
                            db.calendar_file.AddRange(dfs);
                        }
                        #endregion
                        #region add law_logs
                        if (helper.wlog)
                        {
                            log.log_type = 0;
                            log.key_id = model.calendar_duty_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/update_calendar_week_multiple", ip, tid, "Lỗi khi cập nhật lịch họp ", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/update_calendar_week_multiple", ip, tid, "Lỗi khi cập nhật lịch họp", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_calendar_duty([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        var das = await db.calendar_duty.Where(a => ids.Contains(a.calendar_duty_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            string root = HttpContext.Current.Server.MapPath("~/");
                            List<calendar_duty> del = new List<calendar_duty>();
                            foreach (var da in das)
                            {
                                var coincides = await db.calendar_duty_coincide.Where(x => x.calendar_duty_id == da.calendar_duty_id || x.calendar_duty_coincide_id == da.calendar_duty_id).ToListAsync();
                                if (coincides != null && coincides.Count > 0)
                                {
                                    db.calendar_duty_coincide.RemoveRange(coincides);
                                }
                                var procedures = await db.calendar_procedure.Where(x => x.calendar_duty_id == da.calendar_duty_id).ToListAsync();
                                if (procedures != null && procedures.Count > 0)
                                {
                                    db.calendar_procedure.RemoveRange(procedures);
                                }
                                var signs = await db.calendar_sign.Where(x => x.calendar_duty_id == da.calendar_duty_id).ToListAsync();
                                if (signs != null && signs.Count > 0)
                                {
                                    db.calendar_sign.RemoveRange(signs);
                                }
                                var signusers = await db.calendar_signuser.Where(x => x.calendar_duty_id == da.calendar_duty_id).ToListAsync();
                                if (signusers != null && signusers.Count > 0)
                                {
                                    db.calendar_signuser.RemoveRange(signusers);
                                }

                                del.Add(da);

                                var files = await db.calendar_file.Where(x => x.calendar_duty_id == da.calendar_duty_id).ToListAsync();
                                if (files.Count > 0)
                                {
                                    foreach (var f in files)
                                    {
                                        //var rootPath = root + "/" + f.file_path;
                                        //if (System.IO.File.Exists(rootPath))
                                        //{
                                        //    System.IO.File.Delete(rootPath);
                                        //}
                                        // Format f.file_path
                                        var pathFormat = Regex.Replace(f.file_path.Replace("\\", "/"), @"\.*/+", "/");
                                        var listPath = pathFormat.Split('/');
                                        var pathConfig = "";
                                        foreach (var item in listPath)
                                        {
                                            if (item.Trim() != "")
                                            {
                                                pathConfig += "/" + Path.GetFileName(item);
                                            }
                                        }
                                        if (System.IO.File.Exists(root + pathConfig))
                                        {
                                            System.IO.File.Delete(root + pathConfig);
                                        }
                                    }
                                }

                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    calendar_log log = new calendar_log();
                                    log.log_type = 1;
                                    log.message = "Xóa lịch trưc ban: " + da.contents;
                                    log.key_id = da.calendar_duty_id;
                                    log.organization_id = da.organization_id;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    log.is_view = false;
                                    db.calendar_log.Add(log);
                                }
                                #endregion

                                await db.SaveChangesAsync();
                                #region Send Message
                                if (da.status == 2)
                                {
                                    //Send Message
                                    string sendTitle = "Lịch trực ban";
                                    string sendContent = "Vừa xóa lịch trực ngày: \"" + da.date_timelot?.ToString("MM/dd/yyyy") + "\".";
                                    //Notify tất cả user tham gia lịch
                                    string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                                    var sqlpas = new List<SqlParameter>();
                                    sqlpas.Add(new SqlParameter("@" + "calendar_duty_id", da.calendar_duty_id));
                                    var arrpas = sqlpas.ToArray();
                                    var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_duty_member_get", arrpas).Tables);
                                    var tables = await tasks;
                                    List<temp> tempss = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                                    List<string> users = tempss.Select(x => x.user_id).ToList();
                                    send_message(uid, da.calendar_duty_id, users, sendTitle, sendContent, 2);
                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.calendar_duty.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_duty/delete_calendar_week", ip, tid, "Lỗi khi cập nhật phòng họp họp", 0, "calendar_duty");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_duty/delete_calendar_week", ip, tid, "Lỗi khi xoá phòng họp", 0, "calendar_duty");
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
        public async Task<HttpResponseMessage> delete_file([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        var newuser = await db.sys_users.FindAsync(uid);
                        var das = await db.calendar_file.Where(a => ids.Contains(a.file_id) && (ad || a.created_by == uid)).ToListAsync();
                        var dasUrl = await db.calendar_file.AsNoTracking().Where(a => ids.Contains(a.file_id) && (ad || a.created_by == uid) && a.file_path != null).Select(a=>a.file_path).ToListAsync();
                        List<string> paths = new List<string>();
                        string root = HttpContext.Current.Server.MapPath("~/");
                        if (das != null)
                        {
                            List<calendar_file> del = new List<calendar_file>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    calendar_log log = new calendar_log();
                                    log.log_type = 1;
                                    log.message = "Xóa tệp đính kèm: " + da.file_name;
                                    log.key_id = da.calendar_duty_id;
                                    log.created_by = uid;
                                    log.is_view = false;
                                    log.created_date = DateTime.Now;
                                    log.created_ip = ip;
                                    log.created_token_id = tid;
                                    log.organization_id = newuser.organization_id;
                                    db.calendar_log.Add(log);
                                }
                                #endregion
                            }
                            foreach (var p in dasUrl)
                            {
                                paths.Add(p);
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            if (del != null && del.Count > 0)
                            {
                                db.calendar_file.RemoveRange(del);
                            }
                        }
                        await db.SaveChangesAsync();
                        if (paths != null && paths.Count > 0)
                        {
                            foreach (var p in paths)
                            {
                                //var rootPath = root + "/" + p;
                                //if (System.IO.File.Exists(rootPath))
                                //{
                                //    System.IO.File.Delete(rootPath);
                                //}
                                // Format p
                                var pathFormat = Regex.Replace(p.Replace("\\", "/"), @"\.*/+", "/");
                                var listPath = pathFormat.Split('/');
                                var pathConfig = "";
                                foreach (var item in listPath)
                                {
                                    if (item.Trim() != "")
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                if (System.IO.File.Exists(root + pathConfig))
                                {
                                    System.IO.File.Delete(root + pathConfig);
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_duty/delete_file", ip, tid, "Lỗi khi cập nhật phòng họp họp", 0, "calendar_duty");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_duty/delete_file", ip, tid, "Lỗi khi xoá phòng họp", 0, "calendar_duty");
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
        public async Task<HttpResponseMessage> send_duty()
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
                    var dt = provider.FormData.GetValues("dutys").SingleOrDefault();
                    List<string> dutys = JsonConvert.DeserializeObject<List<string>>(dt);

                    foreach(var calendar_duty_id in dutys)
                    {
                        #region model
                        var model = await db.calendar_duty.FindAsync(calendar_duty_id);
                        if (model != null)
                        {
                            model.status = 1;

                            #region logs
                            calendar_log log = new calendar_log();
                            if (helper.wlog)
                            {
                                log.message = "Trình duyệt";
                                log.log_type = 1;
                                log.key_id = model.calendar_duty_id;
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
                        #endregion
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/send_duty", ip, tid, "Lỗi khi cập nhật lịch trực ban", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/send_duty", ip, tid, "Lỗi khi cập nhật lịch trực ban", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> approve_duty()
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
                    int status = int.Parse(provider.FormData.GetValues("status").SingleOrDefault());
                    var dt = provider.FormData.GetValues("dutys").SingleOrDefault();
                    List<string> dutys = JsonConvert.DeserializeObject<List<string>>(dt);

                    foreach (var calendar_duty_id in dutys)
                    {
                        #region model
                        var model = await db.calendar_duty.FindAsync(calendar_duty_id);
                        if (model != null)
                        {
                            model.status = status;

                            #region logs
                            calendar_log log = new calendar_log();
                            if (helper.wlog)
                            {
                                if (status == 2)
                                {
                                    log.message = "Phê duyệt";
                                }
                                else if(status == 3)
                                {
                                    log.message = "Hủy";
                                }
                                log.log_type = 1;
                                log.key_id = model.calendar_duty_id;
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
                        #endregion
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/send_duty", ip, tid, "Lỗi khi cập nhật lịch trực ban", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/send_duty", ip, tid, "Lỗi khi cập nhật lịch trực ban", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

        #region Send Calendar
        [HttpPut]
        public async Task<HttpResponseMessage> send_calendar()
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
                    List<Notification> notifications = new List<Notification>();
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    int is_type_send = int.Parse(provider.FormData.GetValues("is_type_send").SingleOrDefault());
                    var key_id = provider.FormData.GetValues("key_id").SingleOrDefault();
                    string content = provider.FormData.GetValues("content").SingleOrDefault();
                    var date = provider.FormData.GetValues("read_date").SingleOrDefault();
                    DateTime read_date = date != null ? DateTime.Parse(date) : DateTime.Now;
                    var cl = provider.FormData.GetValues("calendars").SingleOrDefault();
                    List<string> calendars = JsonConvert.DeserializeObject<List<string>>(cl);

                    calendar_procedureform procedureform = new calendar_procedureform();
                    List<calendar_signform> signforms = new List<calendar_signform>();
                    List<calendar_signuserform> signuserforms = new List<calendar_signuserform>();

                    if (is_type_send == 0)
                    {
                        procedureform = await db.calendar_procedureform.FindAsync(key_id);
                        signforms = await db.calendar_signform.AsNoTracking().Where(x => x.procedureform_id == procedureform.procedureform_id && x.status == true).OrderBy(x => x.is_step).ToListAsync();
                        var signform_ids = signforms.Select(b => b.signform_id).ToList();
                        signuserforms = await db.calendar_signuserform.AsNoTracking().Where(a => signform_ids.Contains(a.signform_id) && a.status == true && a.user_id != uid).OrderBy(x => x.is_step).ToListAsync();
                    }
                    else if (is_type_send == 1)
                    {
                        signforms = await db.calendar_signform.AsNoTracking().Where(x => x.signform_id == key_id).ToListAsync();
                        var signform_ids = signforms.Select(b => b.signform_id).ToList();
                        signuserforms = await db.calendar_signuserform.AsNoTracking().Where(a => signform_ids.Contains(a.signform_id) && a.status == true && a.user_id != uid).OrderBy(x => x.is_step).ToListAsync();
                    }
                    else if (is_type_send == 2)
                    {
                        signuserforms = new List<calendar_signuserform>();
                    }

                    foreach (var calendar_duty_id in calendars)
                    {
                        var calendar = await db.calendar_duty.FindAsync(calendar_duty_id);
                        if (calendar != null)
                        {
                            //Send Message
                            List<string> sendUsers = new List<string>();
                            string sendTitle = "Lịch trực ban";
                            string sendContent = "Vừa gửi đến bạn lịch trực chờ duyệt ngày: \"" + calendar.date_timelot?.ToString("MM/dd/yyyy") + "\".";
                            //Log
                            calendar_log log = new calendar_log();

                            #region đóng quy trinh cũ
                            foreach (var item in await db.calendar_procedure.Where(a => a.calendar_duty_id == calendar_duty_id).ToListAsync())
                            {
                                item.is_close = true;
                            }
                            foreach (var item in await db.calendar_sign.Where(a => a.calendar_duty_id == calendar_duty_id).ToListAsync())
                            {
                                item.is_close = true;
                            }
                            foreach (var item in await db.calendar_signuser.Where(a => a.calendar_duty_id == calendar_duty_id).ToListAsync())
                            {
                                item.is_close = true;
                            }
                            #endregion

                            #region model
                            calendar.is_type_send = is_type_send;
                            calendar.status = 1; //Chờ duyệt
                            calendar.modified_by = uid;
                            calendar.modified_date = DateTime.Now;
                            calendar.modified_ip = ip;
                            calendar.modified_token_id = tid;
                            #endregion

                            #region Gen quy trình
                            if (is_type_send == 0)
                            {
                                //Quy trình
                                calendar_procedure procedure = new calendar_procedure();
                                procedure.procedure_id = helper.GenKey();
                                procedure.calendar_duty_id = calendar_duty_id;
                                procedure.procedure_name = procedureform.procedureform_name;
                                procedure.is_type = procedureform.is_type;
                                procedure.status = true;
                                procedure.is_order = procedureform.is_order;
                                procedure.created_by = uid;
                                procedure.created_date = DateTime.Now;
                                procedure.created_ip = ip;
                                procedure.created_token_id = tid;
                                procedure.organization_id = procedureform.organization_id;
                                db.calendar_procedure.Add(procedure);

                                //Nhóm duyệt
                                List<calendar_sign> signs = new List<calendar_sign>();
                                List<calendar_signuser> signusers = new List<calendar_signuser>();
                                int is_stepsign = 0;
                                foreach (var signform in signforms)
                                {
                                    is_stepsign++;
                                    calendar_sign sign = new calendar_sign();
                                    sign.sign_id = helper.GenKey();
                                    sign.calendar_duty_id = calendar_duty_id;
                                    sign.procedure_id = procedure.procedure_id;
                                    sign.sign_name = signform.signform_name;
                                    sign.is_step = signform.is_step;
                                    sign.is_type = signform.is_type;
                                    sign.is_sign = false;
                                    sign.is_skip = false;
                                    switch (procedure.is_type)
                                    {
                                        case 0:
                                            sign.status = false;
                                            if (is_stepsign == 1 || signusers.Count == 1)
                                            {
                                                sign.status = true;
                                            }
                                            break;
                                        case 1:
                                            sign.status = true;
                                            break;
                                        case 2:

                                            break;
                                    }
                                    sign.created_by = uid;
                                    sign.created_date = DateTime.Now;
                                    sign.created_ip = ip;
                                    sign.created_token_id = tid;
                                    sign.organization_id = procedureform.organization_id;
                                    signs.Add(sign);

                                    //Người lập
                                    calendar_signuser signusercreate = new calendar_signuser();
                                    if (sign.is_step == 1)
                                    {
                                        signusercreate.signuser_id = helper.GenKey();
                                        signusercreate.calendar_duty_id = calendar_duty_id;
                                        signusercreate.sign_id = sign.sign_id;
                                        signusercreate.user_id = uid;
                                        signusercreate.is_step = 0;
                                        signusercreate.is_type = 1; //Người lập
                                        signusercreate.is_sign = 2;
                                        signusercreate.sign_date = DateTime.Now;
                                        signusercreate.sign_content = content;
                                        signusercreate.read_date = read_date;
                                        signusercreate.status = true;
                                        signusercreate.created_by = uid;
                                        signusercreate.created_date = DateTime.Now;
                                        signusercreate.created_ip = ip;
                                        signusercreate.created_token_id = tid;
                                        signusers.Add(signusercreate);
                                    }
                                    //Người duyệt
                                    var signuserformfilters = signuserforms.Where(x => x.signform_id == signform.signform_id).ToList();
                                    int is_stepsignuser = 0;
                                    foreach (var signuserform in signuserformfilters)
                                    {
                                        is_stepsignuser++;
                                        calendar_signuser signuser = new calendar_signuser();
                                        signuser.signuser_id = helper.GenKey();
                                        signuser.calendar_duty_id = calendar_duty_id;
                                        signuser.sign_id = sign.sign_id;
                                        signuser.user_id = signuserform.user_id;
                                        signuser.is_step = signuserform.is_step;
                                        signuser.is_type = signuserform.is_type;
                                        signuser.is_sign = 0;
                                        if (sign.status == true)
                                        {
                                            switch (sign.is_type)
                                            {
                                                case 0:
                                                    signuser.status = false;
                                                    if (is_stepsignuser == 1)
                                                    {
                                                        signuser.status = true;
                                                    }
                                                    break;
                                                case 1:
                                                    signuser.status = true;
                                                    break;
                                                case 2:

                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            signuser.status = false;
                                        }
                                        signuser.created_by = uid;
                                        signuser.created_date = DateTime.Now;
                                        signuser.created_ip = ip;
                                        signuser.created_token_id = tid;
                                        signusers.Add(signuser);
                                    }
                                    if (signusers.Count > 0)
                                    {
                                        db.calendar_signuser.AddRange(signusers);
                                    }
                                }
                                if (signs.Count > 0)
                                {
                                    db.calendar_sign.AddRange(signs);
                                }

                                //log
                                log.message = "Trình duyệt: " + content;
                                //send
                                sendUsers = signusers.Where(x => x.status == true && (x.is_sign == 0 || x.is_sign == 1)).Select(x => x.user_id).ToList();
                            }
                            else if (is_type_send == 1)
                            {
                                //Nhóm duyệt
                                List<calendar_sign> signs = new List<calendar_sign>();
                                List<calendar_signuser> signusers = new List<calendar_signuser>();
                                foreach (var signform in signforms)
                                {
                                    calendar_sign sign = new calendar_sign();
                                    sign.sign_id = helper.GenKey();
                                    sign.calendar_duty_id = calendar_duty_id;
                                    sign.sign_name = signform.signform_name;
                                    sign.is_step = signform.is_step;
                                    sign.is_type = signform.is_type;
                                    sign.is_sign = false;
                                    sign.is_skip = false;
                                    sign.status = true;
                                    sign.created_by = uid;
                                    sign.created_date = DateTime.Now;
                                    sign.created_ip = ip;
                                    sign.created_token_id = tid;
                                    sign.organization_id = procedureform.organization_id;
                                    signs.Add(sign);

                                    //Người lập
                                    calendar_signuser signusercreate = new calendar_signuser();
                                    signusercreate.signuser_id = helper.GenKey();
                                    signusercreate.calendar_duty_id = calendar_duty_id;
                                    signusercreate.sign_id = sign.sign_id;
                                    signusercreate.user_id = uid;
                                    signusercreate.is_step = 0;
                                    signusercreate.is_type = 1;
                                    signusercreate.is_sign = 2;
                                    signusercreate.sign_date = DateTime.Now;
                                    signusercreate.sign_content = content;
                                    signusercreate.read_date = read_date;
                                    signusercreate.status = true;
                                    signusercreate.created_by = uid;
                                    signusercreate.created_date = DateTime.Now;
                                    signusercreate.created_ip = ip;
                                    signusercreate.created_token_id = tid;
                                    signusers.Add(signusercreate);
                                    //Người duyệt
                                    var signuserformfilters = signuserforms.Where(x => x.signform_id == signform.signform_id).ToList();
                                    int is_stepsignuser = 0;
                                    foreach (var signuserform in signuserformfilters)
                                    {
                                        is_stepsignuser++;
                                        calendar_signuser signuser = new calendar_signuser();
                                        signuser.signuser_id = helper.GenKey();
                                        signuser.calendar_duty_id = calendar_duty_id;
                                        signuser.sign_id = sign.sign_id;
                                        signuser.user_id = signuserform.user_id;
                                        signuser.is_step = signuserform.is_step;
                                        signuser.is_type = signuserform.is_type;
                                        signuser.is_sign = 0;
                                        switch (sign.is_type)
                                        {
                                            case 0:
                                                signuser.status = false;
                                                if (is_stepsignuser == 1)
                                                {
                                                    signuser.status = true;
                                                }
                                                break;
                                            case 1:
                                                signuser.status = true;
                                                break;
                                            case 2:

                                                break;
                                        }
                                        signuser.created_by = uid;
                                        signuser.created_date = DateTime.Now;
                                        signuser.created_ip = ip;
                                        signuser.created_token_id = tid;
                                        signusers.Add(signuser);
                                    }
                                    if (signusers.Count > 0)
                                    {
                                        db.calendar_signuser.AddRange(signusers);
                                    }
                                }
                                if (signs.Count > 0)
                                {
                                    db.calendar_sign.AddRange(signs);
                                }

                                //log
                                log.message = "Trình duyệt: " + content;
                                //send
                                sendUsers = signusers.Where(x => x.status == true && (x.is_sign == 0 || x.is_sign == 1)).Select(x => x.user_id).ToList();
                            }
                            else if (is_type_send == 2)
                            {
                                //Người lập
                                List<calendar_signuser> signusers = new List<calendar_signuser>();
                                calendar_signuser signusercreate = new calendar_signuser();
                                signusercreate.signuser_id = helper.GenKey();
                                signusercreate.calendar_duty_id = calendar_duty_id;
                                signusercreate.user_id = uid;
                                signusercreate.is_step = 0;
                                signusercreate.is_type = 1;
                                signusercreate.is_sign = 2;
                                signusercreate.sign_date = DateTime.Now;
                                signusercreate.sign_content = content;
                                signusercreate.read_date = read_date;
                                signusercreate.status = true;
                                signusercreate.created_by = uid;
                                signusercreate.created_date = DateTime.Now;
                                signusercreate.created_ip = ip;
                                signusercreate.created_token_id = tid;
                                signusers.Add(signusercreate);

                                //Người duyệt
                                calendar_signuser signuser = new calendar_signuser();
                                signuser.signuser_id = helper.GenKey();
                                signuser.calendar_duty_id = calendar_duty_id;
                                signuser.user_id = key_id;
                                signuser.is_step = 1;
                                signuser.is_type = 0;
                                signuser.is_sign = 0;
                                signuser.status = true;
                                signuser.created_by = uid;
                                signuser.created_date = DateTime.Now;
                                signuser.created_ip = ip;
                                signuser.created_token_id = tid;
                                signusers.Add(signuser);

                                if (signusers.Count > 0)
                                {
                                    db.calendar_signuser.AddRange(signusers);
                                }

                                //log
                                //var usr = await db.sys_users.FindAsync(signusers.FirstOrDefault().user_id);
                                log.message = "Trình duyệt: " + content;
                                //send
                                sendUsers = signusers.Where(x => x.status == true && (x.is_sign == 0 || x.is_sign == 1)).Select(x => x.user_id).ToList();
                            }
                            #endregion

                            #region Comment
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                calendar_comment comment = new calendar_comment();
                                comment.comment_id = helper.GenKey();
                                comment.calendar_duty_id = calendar_duty_id;
                                comment.contents = content;
                                comment.is_type = 1;
                                comment.is_delete = false;
                                comment.created_by = uid;
                                comment.created_date = DateTime.Now;
                                comment.created_ip = ip;
                                comment.created_token_id = tid;
                                comment.organization_id = calendar.organization_id;
                                db.calendar_comment.Add(comment);
                            }
                            #endregion

                            #region file
                            string root = HttpContext.Current.Server.MapPath("~/Portals");
                            string path = root + "/" + calendar.organization_id + "/Calendar/" + calendar_duty_id;

                            // Format path
                            var pathFormat = Regex.Replace(path.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = pathFormat.Split('/');
                            var pathConfig = "";
                            var sttPartPath = 1;
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfig += (item);
                                    }
                                    else
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            bool exists = Directory.Exists(pathConfig);
                            if (!exists)
                                Directory.CreateDirectory(pathConfig);

                            List<calendar_file> dfs = new List<calendar_file>();
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
                                string rootPath = pathConfig + "/" + name_file;
                                string Duongdan = "/Portals/" + calendar.organization_id + "/Calendar/" + calendar_duty_id + "/" + name_file;
                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                                if (rootPath.Length > 260)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 260 - (pathConfig.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                File.Move(fileData.LocalFileName, rootPath);
                                //File.Copy(fileData.LocalFileName, rootPathFile, true);
                                var df = new calendar_file();
                                df.file_id = helper.GenKey();
                                df.calendar_duty_id = calendar.calendar_duty_id;
                                df.file_name = name_file;
                                df.file_path = Duongdan;
                                df.file_type = Dinhdang;
                                var file_info = new FileInfo(rootPath);
                                df.file_size = file_info.Length;
                                df.is_image = helper.IsImageFileName(name_file);
                                if (df.is_image == true)
                                {
                                    //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                }
                                df.is_type = 3;
                                df.status = true;
                                df.created_by = uid;
                                df.created_date = DateTime.Now;
                                df.created_ip = ip;
                                df.created_token_id = tid;
                                dfs.Add(df);
                            }
                            if (dfs.Count > 0)
                            {
                                db.calendar_file.AddRange(dfs);
                            }
                            #endregion

                            #region log
                            if (helper.wlog)
                            {
                                log.log_type = 4;
                                log.key_id = calendar_duty_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = calendar.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion

                            await db.SaveChangesAsync();
                            #region Send Message
                            if (sendUsers.Count > 0)
                            {
                                send_message(uid, calendar.calendar_duty_id, sendUsers, sendTitle, sendContent, 2);
                                notifications.Add(new Notification()
                                {
                                    calendar_id = calendar.calendar_duty_id,
                                    uids = sendUsers,
                                    title = sendTitle,
                                    text = sendContent,
                                });
                            }
                            #endregion
                        }
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.SerializeObject(notifications) });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/send_nextcalendar", ip, tid, "Lỗi khi trình duyệt", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/send_nextcalendar", ip, tid, "Lỗi khi trình duyệt", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

        #region Approve Calendar
        [HttpPut]
        public async Task<HttpResponseMessage> approve_calendar()
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
                    List<Notification> notifications = new List<Notification>();
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    int is_type_approve = int.Parse(provider.FormData.GetValues("is_type_approve").SingleOrDefault());
                    string content = provider.FormData.GetValues("content").SingleOrDefault();
                    var date = provider.FormData.GetValues("read_date").SingleOrDefault();
                    DateTime read_date = date != null ? DateTime.Parse(date) : DateTime.Now;
                    var cl = provider.FormData.GetValues("calendars").SingleOrDefault();
                    List<string> calendars = JsonConvert.DeserializeObject<List<string>>(cl);

                    foreach (var calendar_duty_id in calendars)
                    {
                        var calendar = await db.calendar_duty.FindAsync(calendar_duty_id);
                        if (calendar != null)
                        {
                            //Send Message
                            List<string> sendUsers = new List<string>();
                            string sendTitle = "Lịch trực ban";
                            string sendContent = "Vừa gửi đến bạn lịch trực chờ duyệt ngày: \"" + calendar.date_timelot?.ToString("MM/dd/yyyy") + "\".";

                            calendar_procedure procedure = await db.calendar_procedure.FirstOrDefaultAsync(x => x.calendar_duty_id == calendar_duty_id && x.is_close != true);
                            var procedure_id = procedure?.procedure_id;
                            List<calendar_sign> signs = await db.calendar_sign.Where(x => x.calendar_duty_id == calendar_duty_id && x.procedure_id == procedure_id && x.is_close != true).OrderBy(x => x.is_step).ToListAsync();
                            List<calendar_signuser> signusers = await db.calendar_signuser.Where(x => x.calendar_duty_id == calendar_duty_id && x.is_close != true).OrderBy(x => x.is_step).ToListAsync();

                            #region next step
                            //Current step
                            var sign_current = signs.Where(x => x.is_sign != true).FirstOrDefault();
                            int stepsign_current = sign_current?.is_step ?? 0;
                            var sign_id = sign_current?.sign_id;
                            var signuser_currents = signusers.Where(x => (x.is_sign == 0 || x.is_sign == 1)).OrderBy(x => x.is_step).ToList();
                            if (procedure != null && procedure.is_type == 0) //Là quy trinh duyệt tuần tự
                            {
                                signuser_currents = signusers.Where(x => x.sign_id == sign_id && (x.is_sign == 0 || x.is_sign == 1)).OrderBy(x => x.is_step).ToList();
                            }

                            //Cập nhật nguời ký duyệt
                            var user_current = signuser_currents.FirstOrDefault(a => a.user_id == uid && a.status == true);
                            user_current.is_sign = 2;
                            user_current.sign_date = DateTime.Now;
                            user_current.sign_content = content;
                            user_current.read_date = read_date;
                            user_current.modified_by = uid;
                            user_current.modified_date = DateTime.Now;
                            user_current.modified_ip = ip;
                            user_current.modified_token_id = tid;

                            //Danh sách chưa ký
                            signuser_currents = signusers.Where(x => x.sign_id == user_current.sign_id && (x.is_sign == 0 || x.is_sign == 1)).OrderBy(x => x.is_step).ToList();
                            if (procedure != null && procedure.is_type == 0) //Là quy trinh duyệt tuần tự
                            {
                                signuser_currents = signusers.Where(x => x.sign_id == user_current.sign_id && (x.is_sign == 0 || x.is_sign == 1)).OrderBy(x => x.is_step).ToList();
                            }

                            switch (is_type_approve)
                            {
                                case 0: // Duyệt lích
                                    if (calendar.is_type_send == 0)
                                    {
                                        if (procedure.is_type == 0) //Nhóm duyệt lần lượt
                                        {
                                            var countapprove = 0;
                                            if (sign_current != null && sign_current.is_type == 1)
                                            {
                                                countapprove = signusers.Count(a => a.sign_id == sign_current.sign_id && a.is_type != 1 && (a.is_sign == 2 || a.is_sign == -1 || a.is_sign == -3));
                                            }
                                            while (sign_current != null && (signuser_currents.Count(a => a.is_sign == 0 || a.is_sign == 1) == 0 || countapprove > 0))
                                            {
                                                sign_current.is_sign = true;
                                                sign_current = signs.Where(a => a.is_sign != true && a.is_step > stepsign_current).OrderBy(a => a.is_step).FirstOrDefault();
                                                if (sign_current != null)
                                                {
                                                    stepsign_current = sign_current.is_step;
                                                    signuser_currents = signusers.Where(a => a.sign_id == sign_current.sign_id && (a.is_sign == 0 || a.is_sign == 1)).OrderBy(a => a.is_step).ToList();
                                                    countapprove = 0;
                                                    if (sign_current.is_type == 1)
                                                    {
                                                        countapprove = signuser_currents.Count(a => a.is_type != 1 && (a.is_sign == 2 || a.is_sign == -1 || a.is_sign == -3));
                                                    }
                                                }
                                            }
                                            if (sign_current != null && signuser_currents.Count > 0)
                                            {
                                                sign_current.status = true;
                                                switch (sign_current.is_type)
                                                {
                                                    case 0: //Nhóm duyệt lần lượt
                                                        var signuser = signuser_currents.FirstOrDefault();
                                                        signuser.status = true;
                                                        signuser.modified_by = uid;
                                                        signuser.modified_date = DateTime.Now;
                                                        signuser.modified_ip = ip;
                                                        signuser.modified_token_id = tid;
                                                        break;
                                                    case 1: //Nhóm duyệt 1 nhiều
                                                        foreach (var user in signuser_currents)
                                                        {
                                                            user.status = true;
                                                            user.modified_by = uid;
                                                            user.modified_date = DateTime.Now;
                                                            user.modified_ip = ip;
                                                            user.modified_token_id = tid;
                                                        }
                                                        break;
                                                    case 2: //Nhóm ngẫu nhiên

                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                calendar.status = 2; //Ban hành
                                                calendar.modified_by = uid;
                                                calendar.modified_date = DateTime.Now;
                                                calendar.modified_ip = ip;
                                                calendar.modified_token_id = tid;
                                            }
                                        }
                                        else if (procedure.is_type == 1) // quy trình duyệt 1 nhiều
                                        {
                                            sign_current = signs.FirstOrDefault(x => x.sign_id == user_current.sign_id);
                                            stepsign_current = sign_current?.is_step ?? 0;
                                            sign_current.is_sign = true;
                                            sign_current.status = true;
                                            if (sign_current != null && signuser_currents.Count > 0)
                                            {
                                                switch (sign_current.is_type)
                                                {
                                                    case 0: //Nhóm duyệt lần lượt
                                                        var signuser = signuser_currents.FirstOrDefault();
                                                        signuser.status = true;
                                                        signuser.modified_by = uid;
                                                        signuser.modified_date = DateTime.Now;
                                                        signuser.modified_ip = ip;
                                                        signuser.modified_token_id = tid;
                                                        break;
                                                    case 1: //Nhóm duyệt 1 nhiều
                                                        foreach (var user in signuser_currents)
                                                        {
                                                            user.status = true;
                                                            user.modified_by = uid;
                                                            user.modified_date = DateTime.Now;
                                                            user.modified_ip = ip;
                                                            user.modified_token_id = tid;
                                                        }
                                                        break;
                                                    case 2: //Nhóm ngẫu nhiên

                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                calendar.status = 2; //Ban hành
                                                calendar.modified_by = uid;
                                                calendar.modified_date = DateTime.Now;
                                                calendar.modified_ip = ip;
                                                calendar.modified_token_id = tid;
                                            }
                                        }
                                        else if (procedure.is_type == 2) //Nhóm ngẫu nhiên
                                        {

                                        }
                                    }
                                    else if (calendar.is_type_send == 1)
                                    {
                                        if (sign_current != null && signuser_currents.Count > 0)
                                        {
                                            sign_current.status = true;
                                            switch (sign_current.is_type)
                                            {
                                                case 0: //Nhóm duyệt lần lượt
                                                    var signuser = signuser_currents.FirstOrDefault();
                                                    signuser.status = true;
                                                    signuser.modified_by = uid;
                                                    signuser.modified_date = DateTime.Now;
                                                    signuser.modified_ip = ip;
                                                    signuser.modified_token_id = tid;
                                                    break;
                                                case 1: //Nhóm duyệt 1 nhiều
                                                    foreach (var user in signuser_currents)
                                                    {
                                                        user.status = true;
                                                        user.modified_by = uid;
                                                        user.modified_date = DateTime.Now;
                                                        user.modified_ip = ip;
                                                        user.modified_token_id = tid;
                                                    }
                                                    calendar.status = 2; //Ban hành
                                                    calendar.modified_by = uid;
                                                    calendar.modified_date = DateTime.Now;
                                                    calendar.modified_ip = ip;
                                                    calendar.modified_token_id = tid;
                                                    break;
                                                case 2: //Nhóm ngẫu nhiên

                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            calendar.status = 2; //Ban hành
                                            calendar.modified_by = uid;
                                            calendar.modified_date = DateTime.Now;
                                            calendar.modified_ip = ip;
                                            calendar.modified_token_id = tid;
                                        }
                                    }
                                    else if (calendar.is_type_send == 2)
                                    {
                                        calendar.status = 2; //Ban hành
                                        calendar.modified_by = uid;
                                        calendar.modified_date = DateTime.Now;
                                        calendar.modified_ip = ip;
                                        calendar.modified_token_id = tid;
                                    }
                                    break;
                                case 1: // Trả lại
                                    user_current.is_sign = -1; //Trả lại
                                    calendar.status = 3; //Trả lại
                                    calendar.modified_by = uid;
                                    calendar.modified_date = DateTime.Now;
                                    calendar.modified_ip = ip;
                                    calendar.modified_token_id = tid;
                                    break;
                                case 2: // Ban hành
                                    user_current.is_sign = 3; // Ban hành
                                    calendar.status = 2; // Ban hành
                                    calendar.modified_by = uid;
                                    calendar.modified_date = DateTime.Now;
                                    calendar.modified_ip = ip;
                                    calendar.modified_token_id = tid;
                                    break;
                                case 3: // Hủy lịch
                                    user_current.is_sign = -3; // Hủy lịch
                                    calendar.status = 4; // Hủy lịch
                                    calendar.modified_by = uid;
                                    calendar.modified_date = DateTime.Now;
                                    calendar.modified_ip = ip;
                                    calendar.modified_token_id = tid;
                                    break;
                            }

                            // Update
                            foreach (var item in signs)
                            {
                                var is_exists = db.calendar_sign.FirstOrDefault(x => x.sign_id == item.sign_id);
                                if (is_exists != null)
                                {
                                    db.Entry(item).State = EntityState.Modified;
                                }
                                else
                                {
                                    db.calendar_sign.Add(item);
                                }
                            }
                            foreach (var item in signusers)
                            {
                                var is_exists = db.calendar_signuser.FirstOrDefault(x => x.sign_id == item.sign_id);
                                if (is_exists != null)
                                {
                                    db.Entry(item).State = EntityState.Modified;
                                }
                                else
                                {
                                    db.calendar_signuser.Add(item);
                                }
                            }

                            //send
                            sendUsers = signuser_currents.Where(x => x.status == true && (x.is_sign == 0 || x.is_sign == 1)).Select(x => x.user_id).ToList();
                            #endregion

                            #region Comment
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                calendar_comment comment = new calendar_comment();
                                comment.comment_id = helper.GenKey();
                                comment.calendar_duty_id = calendar_duty_id;
                                comment.contents = content;
                                comment.is_type = 1;
                                comment.is_delete = false;
                                comment.created_by = uid;
                                comment.created_date = DateTime.Now;
                                comment.created_ip = ip;
                                comment.created_token_id = tid;
                                comment.organization_id = calendar.organization_id;
                                db.calendar_comment.Add(comment);
                            }
                            #endregion

                            #region file
                            string root = HttpContext.Current.Server.MapPath("~/Portals");
                            string path = root + "/" + calendar.organization_id + "/Calendar/" + calendar_duty_id;

                            // Format path
                            var pathFormat = Regex.Replace(path.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = pathFormat.Split('/');
                            var pathConfig = "";
                            var sttPartPath = 1;
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfig += (item);
                                    }
                                    else
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            bool exists = Directory.Exists(pathConfig);
                            if (!exists)
                                Directory.CreateDirectory(pathConfig);

                            List<calendar_file> dfs = new List<calendar_file>();
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
                                string rootPath = pathConfig + "/" + name_file;
                                string Duongdan = "/Portals/" + calendar.organization_id + "/Calendar/" + calendar_duty_id + "/" + name_file;
                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                                if (rootPath.Length > 260)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 260 - (pathConfig.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                File.Move(fileData.LocalFileName, rootPath);
                                //File.Copy(fileData.LocalFileName, rootPathFile, true);
                                var df = new calendar_file();
                                df.file_id = helper.GenKey();
                                df.calendar_duty_id = calendar.calendar_duty_id;
                                df.file_name = name_file;
                                df.file_path = Duongdan;
                                df.file_type = Dinhdang;
                                var file_info = new FileInfo(rootPath);
                                df.file_size = file_info.Length;
                                df.is_image = helper.IsImageFileName(name_file);
                                if (df.is_image == true)
                                {
                                    //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                }
                                df.is_type = 3;
                                df.status = true;
                                df.created_by = uid;
                                df.created_date = DateTime.Now;
                                df.created_ip = ip;
                                df.created_token_id = tid;
                                dfs.Add(df);
                            }
                            if (dfs.Count > 0)
                            {
                                db.calendar_file.AddRange(dfs);
                            }
                            #endregion

                            #region log
                            if (helper.wlog)
                            {
                                //Log
                                calendar_log log = new calendar_log();

                                switch (is_type_approve)
                                {
                                    case 0:
                                        log.message = "Duyệt lịch: " + content;
                                        break;
                                    case 1:
                                        log.message = "Trả lại: " + content;
                                        break;
                                    case 2:
                                        log.message = "Ban hành: " + content;
                                        break;
                                    case 3:
                                        log.message = "Hủy lịch: " + content;
                                        break;
                                }
                                log.log_type = 4;
                                log.key_id = calendar_duty_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = calendar.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion

                            await db.SaveChangesAsync();
                            #region Send Message
                            switch (calendar.status)
                            {
                                case 2:
                                    sendContent = "Vừa ban hành lịch trực ngày \"" + calendar.date_timelot?.ToString("MM/dd/yyyy") + "\".";
                                    //Notify tất cả user tham gia lịch
                                    string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                                    var sqlpas = new List<SqlParameter>();
                                    sqlpas.Add(new SqlParameter("@" + "calendar_duty_member_get", calendar.calendar_duty_id));
                                    var arrpas = sqlpas.ToArray();
                                    var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_duty_member_get", arrpas).Tables);
                                    var tables = await tasks;
                                    List<temp> temps = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                                    List<string> users = temps.Where(x => !sendUsers.Contains(x.user_id)).Select(x => x.user_id).ToList();
                                    send_message(uid, calendar.calendar_duty_id, users, sendTitle, sendContent, 2);
                                    break;
                                case 3:
                                    sendContent = "Vừa trả lại lịch trực ngày: \"" + calendar.date_timelot?.ToString("MM/dd/yyyy") + "\".";
                                    break;
                                case 4:
                                    sendContent = "Vừa hủy lịch trực ngày: \"" + calendar.date_timelot?.ToString("MM/dd/yyyy") + "\".";
                                    break;
                            }
                            if (sendUsers.Count > 0)
                            {
                                send_message(uid, calendar.calendar_duty_id, sendUsers, sendTitle, sendContent, 2);
                                notifications.Add(new Notification()
                                {
                                    calendar_id = calendar.calendar_duty_id,
                                    uids = sendUsers,
                                    title = sendTitle,
                                    text = sendContent,
                                });
                            }
                            #endregion
                        }
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.SerializeObject(notifications) });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/approve_calendar", ip, tid, "Lỗi khi duyệt lịch", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/approve_calendar", ip, tid, "Lỗi khi duyệt lịch", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

        #region Enact calendar
        [HttpPut]
        public async Task<HttpResponseMessage> enact_calendar()
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
                    List<Notification> notifications = new List<Notification>();
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    string content = provider.FormData.GetValues("content").SingleOrDefault();
                    var date = provider.FormData.GetValues("read_date").SingleOrDefault();
                    DateTime read_date = date != null ? DateTime.Parse(date) : DateTime.Now;
                    var cl = provider.FormData.GetValues("calendars").SingleOrDefault();
                    List<string> calendars = JsonConvert.DeserializeObject<List<string>>(cl);

                    foreach (var calendar_duty_id in calendars)
                    {
                        var calendar = await db.calendar_duty.FindAsync(calendar_duty_id);
                        if (calendar != null)
                        {
                            //Send Message
                            string sendTitle = "Lịch trực ban";
                            string sendContent  = "Vừa ban hành lịch trực ngày \"" + calendar.date_timelot?.ToString("MM/dd/yyyy") + "\".";

                            #region model
                            calendar.status = 2; //Ban hành
                            calendar.modified_by = uid;
                            calendar.modified_date = DateTime.Now;
                            calendar.modified_ip = ip;
                            calendar.modified_token_id = tid;
                            #endregion

                            #region Add user co quyền ban hành vào quy trình
                            if (calendar.is_type_send == null || calendar.is_type_send == -1)
                            {
                                calendar.is_type_send = 2;
                                calendar_signuser signuser = new calendar_signuser();
                                signuser.signuser_id = helper.GenKey();
                                signuser.calendar_duty_id = calendar_duty_id;
                                signuser.user_id = uid;
                                signuser.is_step = db.calendar_signuser.Count(x => x.calendar_duty_id == calendar_duty_id) + 1;
                                signuser.is_type = 0;
                                signuser.is_sign = 2;
                                signuser.sign_date = DateTime.Now;
                                signuser.sign_content = content;
                                signuser.read_date = read_date;
                                signuser.status = true;
                                signuser.created_by = uid;
                                signuser.created_date = DateTime.Now;
                                signuser.created_ip = ip;
                                signuser.created_token_id = tid;
                                db.calendar_signuser.Add(signuser);
                            }
                            #endregion

                            #region file
                            string root = HttpContext.Current.Server.MapPath("~/Portals");
                            string path = root + "/" + calendar.organization_id + "/Calendar/" + calendar_duty_id;

                            // Format path
                            var pathFormat = Regex.Replace(path.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = pathFormat.Split('/');
                            var pathConfig = "";
                            var sttPartPath = 1;
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfig += (item);
                                    }
                                    else
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            bool exists = Directory.Exists(pathConfig);
                            if (!exists)
                                Directory.CreateDirectory(pathConfig);

                            List<calendar_file> dfs = new List<calendar_file>();
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
                                string rootPath = pathConfig + "/" + name_file;
                                string Duongdan = "/Portals/" + calendar.organization_id + "/Calendar/" + calendar_duty_id + "/" + name_file;
                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                                if (rootPath.Length > 260)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 260 - (pathConfig.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                File.Move(fileData.LocalFileName, rootPath);
                                //File.Copy(fileData.LocalFileName, rootPathFile, true);
                                var df = new calendar_file();
                                df.file_id = helper.GenKey();
                                df.calendar_duty_id = calendar.calendar_duty_id;
                                df.file_name = name_file;
                                df.file_path = Duongdan;
                                df.file_type = Dinhdang;
                                var file_info = new FileInfo(rootPath);
                                df.file_size = file_info.Length;
                                df.is_image = helper.IsImageFileName(name_file);
                                if (df.is_image == true)
                                {
                                    //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                }
                                df.is_type = 3;
                                df.status = true;
                                df.created_by = uid;
                                df.created_date = DateTime.Now;
                                df.created_ip = ip;
                                df.created_token_id = tid;
                                dfs.Add(df);
                            }
                            if (dfs.Count > 0)
                            {
                                db.calendar_file.AddRange(dfs);
                            }
                            #endregion

                            #region log
                            if (helper.wlog)
                            {
                                //Log
                                calendar_log log = new calendar_log();
                                log.message = "Ban hành: " + content;
                                log.log_type = 4;
                                log.key_id = calendar_duty_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = calendar.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion

                            await db.SaveChangesAsync();
                            #region Send Message
                            //Notify tất cả user tham gia lịch
                            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                            var sqlpas = new List<SqlParameter>();
                            sqlpas.Add(new SqlParameter("@" + "calendar_duty_id", calendar.calendar_duty_id));
                            var arrpas = sqlpas.ToArray();
                            var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_duty_member_get", arrpas).Tables);
                            var tables = await tasks;
                            List<temp> temps = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                            List<string> sendUsers = temps.Select(x => x.user_id).ToList();
                            send_message(uid, calendar.calendar_duty_id, sendUsers, sendTitle, sendContent, 2);
                            notifications.Add(new Notification()
                            {
                                calendar_id = calendar.calendar_duty_id,
                                uids = sendUsers,
                                title = sendTitle,
                                text = sendContent,
                            });
                            #endregion
                        }
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.SerializeObject(notifications) });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/enact_calendar", ip, tid, "Lỗi khi duyệt lịch", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_duty/enact_calendar", ip, tid, "Lỗi khi duyệt lịch", 0, "calendar_duty");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

        #region Cancel Calendar
        [HttpDelete]
        public async Task<HttpResponseMessage> cancel_calendar([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        List<Notification> notifications = new List<Notification>();
                        #region Model
                        foreach (var calendar_duty_id in ids)
                        {
                            #region model
                            var calendar = await db.calendar_duty.FindAsync(calendar_duty_id);

                            //Send Message
                            string sendTitle = "Lịch trực ban";
                            string sendContent = "Vừa hủy lịch trực: \"" + calendar.date_timelot?.ToString("MM/dd/yyyy") + "\".";

                            calendar.status = 4; //Hủy
                            calendar.modified_by = uid;
                            calendar.modified_date = DateTime.Now;
                            calendar.modified_ip = ip;
                            calendar.modified_token_id = tid;
                            #endregion

                            #region log
                            if (helper.wlog)
                            {
                                //Log
                                calendar_log log = new calendar_log();
                                log.message = "Hủy lịch họp";
                                log.log_type = 4;
                                log.key_id = calendar_duty_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = calendar.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion

                            await db.SaveChangesAsync();
                            #region Send Message
                            //Notify tất cả user tham gia lịch
                            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                            var sqlpas = new List<SqlParameter>();
                            sqlpas.Add(new SqlParameter("@" + "calendar_duty_id", calendar.calendar_duty_id));
                            var arrpas = sqlpas.ToArray();
                            var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_duty_member_get", arrpas).Tables);
                            var tables = await tasks;
                            List<temp> temps = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                            List<string> sendUsers = temps.Select(x => x.user_id).ToList();
                            send_message(uid, calendar.calendar_duty_id, sendUsers, sendTitle, sendContent, 2);
                            notifications.Add(new Notification()
                            {
                                calendar_id = calendar.calendar_duty_id,
                                uids = sendUsers,
                                title = sendTitle,
                                text = sendContent,
                            });
                            #endregion
                        }
                        #endregion
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.SerializeObject(notifications) });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_duty/cancel_calendar", ip, tid, "Lỗi khi cập nhật phòng họp họp", 0, "calendar_duty");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_duty/cancel_calendar", ip, tid, "Lỗi khi xoá phòng họp", 0, "calendar_duty");
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
        #endregion

        #region export doc
        [HttpPost]
        public HttpResponseMessage ExportDoc([System.Web.Mvc.Bind(Include = "lib,name,html,opition")][FromBody] modelHTML model)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
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
                        var user_now = db.sys_users.AsNoTracking().FirstOrDefault(x => x.user_id == uid);
                        string rootPath = HttpContext.Current.Server.MapPath("~/Portals/" + user_now.organization_id + "/Word/");

                        // Format rootPath
                        var pathFormat_1 = Regex.Replace(rootPath.Replace("\\", "/"), @"\.*/+", "/");
                        var listPath_1 = pathFormat_1.Split('/');
                        var pathRootConfig = "";
                        var sttPartPath_1 = 1;
                        foreach (var item in listPath_1)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath_1 == 1)
                                {
                                    pathRootConfig += (item);
                                }
                                else
                                {
                                    pathRootConfig += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath_1++;
                        }
                        bool existPath = System.IO.Directory.Exists(pathRootConfig);
                        if (!existPath)
                        {
                            System.IO.Directory.CreateDirectory(pathRootConfig);
                        }
                        string path = "/Portals/" + user_now.organization_id + "/Word/" + model.name;
                        string strPath = Path.Combine(rootPath + model.name);
                        using (var htmlStream = new MemoryStream(Encoding.UTF8.GetBytes(model.html)))
                        {
                            ComponentInfo.SetLicense("DTZX-HTZ5-B7Q6-2GA6");
                            var htmlLoadOptions = new HtmlLoadOptions();
                            var document = DocumentModel.Load(htmlStream, htmlLoadOptions);
                            document.DefaultCharacterFormat.Size = 13;
                            var opt = model.opition;
                            if (opt == null || (opt.left == 0 && opt.top == 0 && opt.right == 0 && opt.bottom == 0))
                            {
                                opt = new PDFOpition()
                                {
                                    orientation = opt.orientation ?? "Portrait",
                                    pageSize = opt.pageSize ?? "A4",
                                    left = opt.left,
                                    top = opt.top,
                                    right = opt.right,
                                    bottom = opt.bottom,
                                };
                            }
                            Section section = document.Sections[0];
                            PageSetup pageSetup = section.PageSetup;
                            PageMargins pageMargins = pageSetup.PageMargins;
                            if(opt.orientation == "Portrait")
                            {
                                section.PageSetup.Orientation = Orientation.Portrait;
                            }
                            else if (opt.orientation == "Landscape")
                            {
                                section.PageSetup.Orientation = Orientation.Landscape;
                            }
                            pageMargins.Top = opt.top;
                            pageMargins.Top = opt.top;
                            pageMargins.Right = opt.right;
                            pageMargins.Bottom = opt.bottom;
                            pageMargins.Left = opt.left;
                            SaveOptions opit = SaveOptions.DocxDefault;

                            // Format path strPath
                            var pathFormat = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = pathFormat.Split('/');
                            var pathConfig = "";
                            var sttPartPath = 1;
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfig += (item);
                                    }
                                    else
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            if (File.Exists(pathConfig))
                            {
                                File.Delete(pathConfig);
                            }
                            document.Save(pathConfig, opit);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { path = path, err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_duty/ExportDoc", ip, tid, "Lỗi khi export file doc", 0, "calendar_duty");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_duty/ExportDoc", ip, tid, "Lỗi khi export file doc", 0, "calendar_duty");
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
        #endregion

        #region Send Message
        public void send_message(string user_send, string id_key, [System.Web.Mvc.Bind(Include = "")][FromBody] List<string> users, string title, string content, int is_type)
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        #region Sendhub
                        List<sys_sendhub> sendhubs = new List<sys_sendhub>();
                        users = users.Where(x => x != user_send).ToList();
                        foreach (String user_id in users)
                        {
                            sys_sendhub sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.module_key = "M2";
                            sh.user_send = user_send;
                            sh.receiver = user_id;
                            sh.title = title;
                            sh.contents = content;
                            sh.type = 5; //Lịch họp
                            sh.is_type = is_type;
                            sh.seen = false;
                            sh.date_send = DateTime.Now;
                            sh.id_key = id_key;
                            sh.created_by = user_send;
                            sh.created_date = DateTime.Now;
                            sendhubs.Add(sh);
                        }
                        if (sendhubs.Count > 0)
                        {
                            db.sys_sendhub.AddRange(sendhubs);
                            await db.SaveChangesAsync();
                        }
                        #endregion
                        #region SendSocket
                        //send socket
                        var message = new Dictionary<string, dynamic>
                        {
                            { "event", "sendNotify" },
                            { "user_id", user_send },
                            { "title", title },
                            { "contents", content },
                            { "date", DateTime.Now },
                            { "uids", users },
                        };
                        if (helper.socketClient != null && helper.socketClient.Connected == true)
                        {
                            try
                            {
                                await helper.socketClient.EmitAsync("sendData", message);
                            }
                            catch { };
                        }
                        #endregion
                    }
                }
                catch { }
            });
        }

        public class temp
        {
            public string user_id { get; set; }
        }

        public class Notification
        {

            public string calendar_id { get; set; }
            public List<string> uids { get; set; }
            public string title { get; set; }
            public string text { get; set; }
        }
        #endregion
    }
}