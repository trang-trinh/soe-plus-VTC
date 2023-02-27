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
using System.Text.RegularExpressions;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers.Calendar
{
    [Authorize(Roles = "login")]
    public class calendar_weekController : ApiController
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
        public async Task<HttpResponseMessage> update_calendar_week()
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
                    calendar_week model = JsonConvert.DeserializeObject<calendar_week>(md);
                    var ct = provider.FormData.GetValues("chutris").SingleOrDefault();
                    List<string> chutris = JsonConvert.DeserializeObject<List<string>>(ct);
                    var tg = provider.FormData.GetValues("thamgias").SingleOrDefault();
                    List<string> thamgias = JsonConvert.DeserializeObject<List<string>>(tg);
                    var pb = provider.FormData.GetValues("phongbans").SingleOrDefault();
                    List<int> phongbans = JsonConvert.DeserializeObject<List<int>>(pb);
                    #region Model
                    calendar_log log = new calendar_log();
                    if (isAdd)
                    {
                        model.calendar_id = helper.GenKey();
                        if (!string.IsNullOrEmpty(model.contents))
                        {
                            model.contents_en = helper.convertToUnSign3(model.contents);
                        }
                        model.is_order = db.calendar_week.Count() + 1;
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        log.message = "Thêm mới: " + model.contents;
                        db.calendar_week.Add(model);
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
                    //var userold = await db.calendar_member.Where(x => x.calendar_id == model.calendar_id).ToListAsync();
                    //if (userold.Count > 0)
                    //{
                    //    db.calendar_member.RemoveRange(userold);
                    //}
                    var temps = await db.calendar_member.AsNoTracking().Where(x => x.calendar_id == model.calendar_id).ToListAsync();
                    List<calendar_member> members = new List<calendar_member>();
                    if (chutris.Count > 0)
                    {
                        var chutri_old = await db.calendar_member.Where(x => x.calendar_id == model.calendar_id && x.is_type == 0).ToListAsync();
                        if (chutri_old.Count > 0)
                        {
                            db.calendar_member.RemoveRange(chutri_old);
                        }
                        foreach (var user_id in chutris)
                        {
                            calendar_member mb = new calendar_member();
                            mb.member_id = helper.GenKey();
                            mb.calendar_id = model.calendar_id;
                            mb.user_id = user_id;
                            mb.is_type = 0;
                            mb.is_attend = true;
                            mb.status = true;
                            mb.is_order = 0;
                            mb.created_by = uid;
                            mb.created_date = DateTime.Now;
                            mb.created_ip = ip;
                            mb.created_token_id = tid;
                            members.Add(mb);
                            temps.Add(mb);
                        }
                    }
                    if (thamgias.Count > 0)
                    {
                        int stt = 0;
                        foreach (var user_id in thamgias)
                        {
                            calendar_member mb = new calendar_member();
                            mb.member_id = helper.GenKey();
                            mb.calendar_id = model.calendar_id;
                            mb.user_id = user_id;
                            mb.is_type = 1;
                            mb.is_attend = true;
                            mb.status = true;
                            mb.is_order = stt++;
                            mb.created_by = uid;
                            mb.created_date = DateTime.Now;
                            mb.created_ip = ip;
                            mb.created_token_id = tid;
                            members.Add(mb);
                            temps.Add(mb);
                        }
                    }
                    if (members.Count > 0)
                    {
                        db.calendar_member.AddRange(members);
                    }
                    #endregion
                    #region Phongban
                    List<calendar_attend> departments = new List<calendar_attend>();
                    if (phongbans.Count > 0)
                    {
                        var departmentold = await db.calendar_attend.Where(x => x.calendar_id == model.calendar_id).ToListAsync();
                        if (departmentold.Count > 0)
                        {
                            db.calendar_attend.RemoveRange(departmentold);
                        }
                        int stt = 0;
                        foreach (int department_id in phongbans)
                        {
                            calendar_attend department = new calendar_attend();
                            department.attend_id = helper.GenKey();
                            department.calendar_id = model.calendar_id;
                            department.department_id = department_id;
                            department.status = true;
                            department.is_order = stt++;
                            department.created_by = uid;
                            department.created_date = DateTime.Now;
                            department.created_ip = ip;
                            department.created_token_id = tid;
                            departments.Add(department);
                        }
                    }
                    if (departments.Count > 0)
                    {
                        db.calendar_attend.AddRange(departments);
                    }
                    #endregion
                    #region Trùng lịch
                    var coincides = await db.calendar_coincide.Where(x => x.calendar_id == model.calendar_id || x.calendar_coincide_id == model.calendar_id).ToListAsync();
                    if (coincides.Count > 0)
                    {
                        db.calendar_coincide.RemoveRange(coincides);
                    }
                    var coincide_news = await db.calendar_week.Where(x => x.organization_id == user_now.organization_id && x.calendar_id != model.calendar_id && ((x.start_date >= model.start_date && x.start_date <= model.end_date) || (x.end_date >= model.start_date && x.end_date <= model.end_date) || (x.start_date <= model.start_date && x.end_date >= model.start_date))).ToListAsync();
                    coincides = new List<calendar_coincide>();
                    var check_coincides = coincide_news;
                    //Trùng phòng họp
                    if (!string.IsNullOrEmpty(model.boardroom_id))
                    {
                        check_coincides = coincide_news.Where(a => a.boardroom_id == model.boardroom_id).ToList();
                        foreach (calendar_week calendar in check_coincides)
                        {
                            calendar_coincide coincide = new calendar_coincide();
                            coincide.coincide_id = helper.GenKey();
                            coincide.calendar_id = calendar.calendar_id;
                            coincide.calendar_coincide_id = model.calendar_id;
                            coincide.is_type = 0;
                            coincide.contents = "Trùng phòng họp: " + (await db.calendar_ca_boardroom.FindAsync(model.boardroom_id))?.boardroom_name;
                            coincide.created_by = uid;
                            coincide.created_date = DateTime.Now;
                            coincide.created_ip = ip;
                            coincide.created_token_id = tid;
                            coincides.Add(coincide);
                        }
                    }
                    //Trùng người tham gia
                    if (temps.Count > 0)
                    {
                        var id_coincides = coincide_news.Select(x => x.calendar_id).ToList();
                        var member_coincides = await db.calendar_member.Where(a => id_coincides.Contains(a.calendar_id)).Select(a => new { a.calendar_id, a.user_id, a.is_type }).ToListAsync();
                        foreach (var mb in temps)
                        {
                            var usr = await db.sys_users.FindAsync(mb.user_id);
                            var check_member_coincides = member_coincides.Where(x => x.user_id == mb.user_id).ToList();
                            foreach (var calendar in check_member_coincides)
                            {
                                calendar_coincide coincide = new calendar_coincide();
                                coincide.coincide_id = helper.GenKey();
                                coincide.calendar_id = calendar.calendar_id;
                                coincide.calendar_coincide_id = model.calendar_id;
                                if (calendar.is_type == 0)
                                {
                                    coincide.is_type = 1;
                                    coincide.contents = "Trùng chủ trì: " + usr?.full_name;
                                }
                                else if (calendar.is_type == 1)
                                {
                                    coincide.is_type = 2;
                                    coincide.contents = "Trùng người tham gia: " + usr?.full_name;
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
                        db.calendar_coincide.AddRange(coincides);
                    }
                    #endregion
                    #region file
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string path = root + "/" + model.organization_id + "/Calendar/" + model.calendar_id;
                    bool exists = Directory.Exists(path);
                    if (!exists)
                        Directory.CreateDirectory(path);
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
                        string rootPath = path + "/" + name_file;
                        string Duongdan = "/Portals/" + model.organization_id + "/Calendar/" + model.calendar_id + "/" + name_file;
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
                        df.calendar_id = model.calendar_id;
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
                    if (dfs.Count > 0)
                    {
                        db.calendar_file.AddRange(dfs);
                    }
                    #endregion
                    #region add law_logs
                    if (helper.wlog)
                    {
                        log.log_type = 0;
                        log.key_id = model.calendar_id;
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
                        string sendTitle = model.is_group == 0 ? "Lịch họp" : model.is_group == 1 ? "Lịch công tác" : "";
                        string sendContent = "Vừa cập nhật lịch: \"" + model.contents + "\".";
                        //Notify tất cả user tham gia lịch
                        string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                        var sqlpas = new List<SqlParameter>();
                        sqlpas.Add(new SqlParameter("@" + "calendar_id", model.calendar_id));
                        var arrpas = sqlpas.ToArray();
                        var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_member_get", arrpas).Tables);
                        var tables = await tasks;
                        List<temp> tempss = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                        List<string> sendUsers = tempss.Select(x => x.user_id).ToList();
                        send_message(uid, model.calendar_id, sendUsers, sendTitle, sendContent, (model.is_group ?? 0));
                        notifications.Add(new Notification()
                        {
                            calendar_id = model.calendar_id,
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/update_calendar_week", ip, tid, "Lỗi khi cập nhật phòng họp", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/update_calendar_week", ip, tid, "Lỗi khi cập nhật phòng họp", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_calendar_week_multiple()
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
                    List<calendar_week> multiple = JsonConvert.DeserializeObject<List<calendar_week>>(provider.FormData.GetValues("multiple").SingleOrDefault());
                    List<calendar_member> mbs = JsonConvert.DeserializeObject<List<calendar_member>>(provider.FormData.GetValues("members").SingleOrDefault());
                    List<calendar_attend> dps = JsonConvert.DeserializeObject<List<calendar_attend>>(provider.FormData.GetValues("departments").SingleOrDefault());

                    foreach (var model in multiple)
                    {
                        var key_id = model.calendar_id;
                        #region Model
                        calendar_log log = new calendar_log();
                        model.calendar_id = helper.GenKey();
                        if (!string.IsNullOrEmpty(model.contents))
                        {
                            model.contents_en = helper.convertToUnSign3(model.contents);
                        }
                        model.is_order = db.calendar_week.Count() + 1;
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        log.message = "Thêm mới: " + model.contents;
                        db.calendar_week.Add(model);
                        #endregion
                        #region Member
                        var temps = await db.calendar_member.AsNoTracking().Where(x => x.calendar_id == model.calendar_id).ToListAsync();
                        List<calendar_member> members = new List<calendar_member>();
                        if (mbs.Count > 0)
                        {
                            var filtermbs = mbs.Where(x => x.calendar_id == key_id).ToList();
                            int stt = 0;
                            foreach (var user in filtermbs)
                            {
                                calendar_member mb = new calendar_member();
                                mb.member_id = helper.GenKey();
                                mb.calendar_id = model.calendar_id;
                                mb.user_id = user.user_id;
                                mb.is_type = user.is_type;
                                mb.is_attend = true;
                                mb.status = true;
                                mb.is_order = stt++;
                                mb.created_by = uid;
                                mb.created_date = DateTime.Now;
                                mb.created_ip = ip;
                                mb.created_token_id = tid;
                                members.Add(mb);
                                temps.Add(mb);
                            }
                        }
                        if (members.Count > 0)
                        {
                            db.calendar_member.AddRange(members);
                        }
                        #endregion
                        #region Phongban
                        List<calendar_attend> departments = new List<calendar_attend>();
                        if (dps.Count > 0)
                        {
                            var filterdps = dps.Where(x => x.calendar_id == key_id).ToList();
                            int stt = 0;
                            foreach (var dp in filterdps)
                            {
                                calendar_attend department = new calendar_attend();
                                department.attend_id = helper.GenKey();
                                department.calendar_id = model.calendar_id;
                                department.department_id = dp.department_id;
                                department.status = true;
                                department.is_order = stt++;
                                department.created_by = uid;
                                department.created_date = DateTime.Now;
                                department.created_ip = ip;
                                department.created_token_id = tid;
                                departments.Add(department);
                            }
                        }
                        if (departments.Count > 0)
                        {
                            db.calendar_attend.AddRange(departments);
                        }
                        #endregion
                        #region Trùng lịch
                        var coincides = await db.calendar_coincide.Where(x => x.calendar_id == model.calendar_id || x.calendar_coincide_id == model.calendar_id).ToListAsync();
                        if (coincides.Count > 0)
                        {
                            db.calendar_coincide.RemoveRange(coincides);
                        }
                        var coincide_news = await db.calendar_week.Where(x => x.organization_id == user_now.organization_id && x.calendar_id != model.calendar_id && ((x.start_date >= model.start_date && x.start_date <= model.end_date) || (x.end_date >= model.start_date && x.end_date <= model.end_date) || (x.start_date <= model.start_date && x.end_date >= model.start_date))).ToListAsync();
                        coincides = new List<calendar_coincide>();
                        var check_coincides = coincide_news;
                        //Trùng phòng họp
                        if (!string.IsNullOrEmpty(model.boardroom_id))
                        {
                            check_coincides = coincide_news.Where(a => a.boardroom_id == model.boardroom_id).ToList();
                            foreach (calendar_week calendar in check_coincides)
                            {
                                calendar_coincide coincide = new calendar_coincide();
                                coincide.coincide_id = helper.GenKey();
                                coincide.calendar_id = calendar.calendar_id;
                                coincide.calendar_coincide_id = model.calendar_id;
                                coincide.is_type = 0;
                                coincide.contents = "Trùng phòng họp: " + (await db.calendar_ca_boardroom.FindAsync(model.boardroom_id))?.boardroom_name;
                                coincide.created_by = uid;
                                coincide.created_date = DateTime.Now;
                                coincide.created_ip = ip;
                                coincide.created_token_id = tid;
                                coincides.Add(coincide);
                            }
                        }
                        //Trùng người tham gia
                        if (temps.Count > 0)
                        {
                            var id_coincides = coincide_news.Select(x => x.calendar_id).ToList();
                            var member_coincides = await db.calendar_member.Where(a => id_coincides.Contains(a.calendar_id)).Select(a => new { a.calendar_id, a.user_id, a.is_type }).ToListAsync();
                            foreach (var mb in temps)
                            {
                                var usr = await db.sys_users.FindAsync(mb.user_id);
                                var check_member_coincides = member_coincides.Where(x => x.user_id == mb.user_id).ToList();
                                foreach (var calendar in check_member_coincides)
                                {
                                    calendar_coincide coincide = new calendar_coincide();
                                    coincide.coincide_id = helper.GenKey();
                                    coincide.calendar_id = calendar.calendar_id;
                                    coincide.calendar_coincide_id = model.calendar_id;
                                    if (calendar.is_type == 0)
                                    {
                                        coincide.is_type = 1;
                                        coincide.contents = "Trùng chủ trì: " + usr?.full_name;
                                    }
                                    else if (calendar.is_type == 1)
                                    {
                                        coincide.is_type = 2;
                                        coincide.contents = "Trùng người tham gia: " + usr?.full_name;
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
                            db.calendar_coincide.AddRange(coincides);
                        }
                        #endregion
                        #region file
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        string path = root + "/" + model.organization_id + "/Calendar/" + model.calendar_id;
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
                                string Duongdan = "/Portals/" + model.organization_id + "/Calendar/" + model.calendar_id + "/" + name_file;
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
                                df.calendar_id = model.calendar_id;
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
                            log.key_id = model.calendar_id;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/update_calendar_week_multiple", ip, tid, "Lỗi khi cập nhật lịch họp ", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/update_calendar_week_multiple", ip, tid, "Lỗi khi cập nhật lịch họp", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_calendar_week([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        var das = await db.calendar_week.Where(a => ids.Contains(a.calendar_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            string root = HttpContext.Current.Server.MapPath("~/");
                            List<calendar_week> del = new List<calendar_week>();
                            foreach (var da in das)
                            {
                                var coincides = await db.calendar_coincide.Where(x => x.calendar_id == da.calendar_id || x.calendar_coincide_id == da.calendar_id).ToListAsync();
                                if (coincides != null && coincides.Count > 0)
                                {
                                    db.calendar_coincide.RemoveRange(coincides);
                                }
                                var procedures = await db.calendar_procedure.Where(x => x.calendar_id == da.calendar_id).ToListAsync();
                                if (procedures != null && procedures.Count > 0)
                                {
                                    db.calendar_procedure.RemoveRange(procedures);
                                }
                                var signs = await db.calendar_sign.Where(x => x.calendar_id == da.calendar_id).ToListAsync();
                                if (signs != null && signs.Count > 0)
                                {
                                    db.calendar_sign.RemoveRange(signs);
                                }
                                var signusers = await db.calendar_signuser.Where(x => x.calendar_id == da.calendar_id).ToListAsync();
                                if (signusers != null && signusers.Count > 0)
                                {
                                    db.calendar_signuser.RemoveRange(signusers);
                                }
                                del.Add(da);

                                var files = await db.calendar_file.Where(x => x.calendar_id == da.calendar_id).ToListAsync();
                                if (files.Count > 0)
                                {
                                    foreach (var f in files)
                                    {
                                        var rootPath = root + "/" + f.file_path;
                                        if (System.IO.File.Exists(rootPath))
                                        {
                                            System.IO.File.Delete(rootPath);
                                        }
                                    }
                                }

                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    calendar_log log = new calendar_log();
                                    log.log_type = 0;
                                    log.message = "Xóa lịch họp: " + da.contents;
                                    log.key_id = da.calendar_id;
                                    log.organization_id = da.organization_id;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    log.is_view = false;
                                    db.calendar_log.Add(log);
                                }
                                #endregion

                                #region Send Message
                                if (da.status == 2)
                                {
                                    //Send Message
                                    string sendTitle = da.is_group == 0 ? "Lịch họp" : da.is_group == 1 ? "Lịch công tác" : "";
                                    string sendContent = "Vừa xóa lịch: \"" + da.contents + "\".";
                                    //Notify tất cả user tham gia lịch
                                    string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                                    var sqlpas = new List<SqlParameter>();
                                    sqlpas.Add(new SqlParameter("@" + "calendar_id", da.calendar_id));
                                    var arrpas = sqlpas.ToArray();
                                    var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_member_get", arrpas).Tables);
                                    var tables = await tasks;
                                    List<temp> tempss = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                                    List<string> users = tempss.Select(x => x.user_id).ToList();
                                    send_message(uid, da.calendar_id, users, sendTitle, sendContent, (da.is_group ?? 0));
                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.calendar_week.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_week/delete_calendar_week", ip, tid, "Lỗi khi cập nhật phòng họp họp", 0, "calendar_week");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_week/delete_calendar_week", ip, tid, "Lỗi khi xoá phòng họp", 0, "calendar_week");
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
        public async Task<HttpResponseMessage> delete_member([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        var das = await db.calendar_member.Where(a => ids.Contains(a.member_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<calendar_member> del = new List<calendar_member>();
                            foreach (var da in das)
                            {
                                var model = await db.calendar_week.FindAsync(da.calendar_id);
                                model.numeric_attendees -= 1;
                                var usr = await db.sys_users.FindAsync(da.user_id);
                                del.Add(da);
                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    calendar_log log = new calendar_log();
                                    log.log_type = 0;
                                    log.message = "Xóa thành viên: " + usr.full_name;
                                    log.key_id = da.calendar_id;
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
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.calendar_member.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_week/delete_member", ip, tid, "Lỗi khi xóa người tham gia", 0, "calendar_week");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_week/delete_member", ip, tid, "Lỗi khi xóa người tham gia", 0, "calendar_week");
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
                        var dasUrl = await db.calendar_file.AsNoTracking().Where(a => ids.Contains(a.file_id) && (ad || a.created_by == uid) && a.file_path != null).Select(a => a.file_path).ToListAsync();
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
                                    log.log_type = 0;
                                    log.message = "Xóa tệp đính kèm: " + da.file_name;
                                    log.key_id = da.calendar_id;
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
                                var rootPath = root + "/" + p;
                                if (System.IO.File.Exists(rootPath))
                                {
                                    System.IO.File.Delete(rootPath);
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_week/delete_file", ip, tid, "Lỗi khi cập nhật phòng họp họp", 0, "calendar_week");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_week/delete_file", ip, tid, "Lỗi khi xoá phòng họp", 0, "calendar_week");
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
                    #region Xử lý 
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

                    foreach (var calendar_id in calendars)
                    {
                        var calendar = await db.calendar_week.FindAsync(calendar_id);
                        if (calendar != null)
                        {
                            //Send Message
                            List<string> sendUsers = new List<string>();
                            string sendTitle = calendar.is_group == 0 ? "Lịch họp" : calendar.is_group == 1 ? "Lịch công tác" : "";
                            string sendContent = "Vừa gửi đến bạn lịch chờ duyệt: \"" + calendar.contents + "\".";
                            //Log
                            calendar_log log = new calendar_log();

                            #region đóng quy trinh cũ
                            foreach (var item in await db.calendar_procedure.Where(a => a.calendar_id == calendar_id).ToListAsync())
                            {
                                item.is_close = true;
                            }
                            foreach (var item in await db.calendar_sign.Where(a => a.calendar_id == calendar_id).ToListAsync())
                            {
                                item.is_close = true;
                            }
                            foreach (var item in await db.calendar_signuser.Where(a => a.calendar_id == calendar_id).ToListAsync())
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
                            if (is_type_send == 0) //Quy trình
                            {
                                //Quy trình
                                calendar_procedure procedure = new calendar_procedure();
                                procedure.procedure_id = helper.GenKey();
                                procedure.calendar_id = calendar_id;
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
                                    sign.calendar_id = calendar_id;
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
                                        signusercreate.calendar_id = calendar_id;
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
                                        signuser.calendar_id = calendar_id;
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
                            else if (is_type_send == 1) //Nhóm
                            {
                                //Nhóm duyệt
                                List<calendar_sign> signs = new List<calendar_sign>();
                                List<calendar_signuser> signusers = new List<calendar_signuser>();
                                foreach (var signform in signforms)
                                {
                                    calendar_sign sign = new calendar_sign();
                                    sign.sign_id = helper.GenKey();
                                    sign.calendar_id = calendar_id;
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
                                    signusercreate.calendar_id = calendar_id;
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
                                        signuser.calendar_id = calendar_id;
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
                            else if (is_type_send == 2) //Dich danh
                            {
                                //Người lập
                                List<calendar_signuser> signusers = new List<calendar_signuser>();
                                calendar_signuser signusercreate = new calendar_signuser();
                                signusercreate.signuser_id = helper.GenKey();
                                signusercreate.calendar_id = calendar_id;
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
                                signuser.calendar_id = calendar_id;
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
                                comment.calendar_id = calendar_id;
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
                            string path = root + "/" + calendar.organization_id + "/Calendar/" + calendar_id;
                            bool exists = Directory.Exists(path);
                            if (!exists)
                                Directory.CreateDirectory(path);
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
                                string rootPath = path + "/" + name_file;
                                string Duongdan = "/Portals/" + calendar.organization_id + "/Calendar/" + calendar_id + "/" + name_file;
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
                                df.calendar_id = calendar.calendar_id;
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
                                log.key_id = calendar_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = calendar.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion

                            #region Send Message
                            if (sendUsers.Count > 0)
                            {
                                send_message(uid, calendar.calendar_id, sendUsers, sendTitle, sendContent, (calendar.is_group ?? 0));
                                notifications.Add(new Notification()
                                {
                                    calendar_id = calendar.calendar_id,
                                    uids = sendUsers,
                                    title = sendTitle,
                                    text = sendContent,
                                });
                            }
                            #endregion
                        }
                    }
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.SerializeObject(notifications) });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/send_nextcalendar", ip, tid, "Lỗi khi trình duyệt", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/send_nextcalendar", ip, tid, "Lỗi khi trình duyệt", 0, "calendar_week");
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

                    foreach (var calendar_id in calendars)
                    {
                        var calendar = await db.calendar_week.FindAsync(calendar_id);
                        if (calendar != null)
                        {
                            //Send Message
                            List<string> sendUsers = new List<string>();
                            string sendTitle = calendar.is_group == 0 ? "Lịch họp" : calendar.is_group == 1 ? "Lịch công tác" : "";
                            string sendContent = "Vừa gửi đến bạn lịch chờ duyệt: \"" + calendar.contents + "\".";

                            calendar_procedure procedure = await db.calendar_procedure.FirstOrDefaultAsync(x => x.calendar_id == calendar_id && x.is_close != true);
                            var procedure_id = procedure?.procedure_id;
                            List<calendar_sign> signs = await db.calendar_sign.Where(x => x.calendar_id == calendar_id && x.procedure_id == procedure_id && x.is_close != true).OrderBy(x => x.is_step).ToListAsync();
                            List<calendar_signuser> signusers = await db.calendar_signuser.Where(x => x.calendar_id == calendar_id && x.is_close != true).OrderBy(x => x.is_step).ToListAsync();

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

                            switch (is_type_approve) {
                                case 0: // Duyệt lích
                                    if (calendar.is_type_send == 0)
                                    {
                                        if (procedure.is_type == 0) // quy trình duyệt lần lượt
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
                                        else if (procedure.is_type == 2) // quy trình ngẫu nhiên
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

                            #region lịch lặp
                            if (calendar.status == 2 && calendar.is_iterations > 0)
                            {
                                int day = 0, numeric_iterations = 0;
                                if (calendar.distance_iterations != null)
                                {
                                    day = calendar.distance_iterations.Value;
                                }
                                if (calendar.numeric_iterations != null)
                                {
                                    numeric_iterations = calendar.numeric_iterations.Value;
                                }

                                int? cy = calendar.start_date?.Year;
                                DateTime? start_date = calendar.start_date;
                                DateTime? end_date = calendar.end_date ?? calendar.start_date;
                                switch (calendar.is_iterations)
                                {
                                    case 1://Ngày
                                        start_date = start_date?.AddDays(day);
                                        end_date = end_date?.AddDays(day);
                                        break;
                                    case 2://Tuần
                                        start_date = start_date?.AddDays(7 * day);
                                        end_date = end_date?.AddDays(7 * day);
                                        break;
                                    case 3://Tháng
                                        start_date = start_date?.AddMonths(day);
                                        end_date = end_date?.AddMonths(day);
                                        break;
                                    case 4://Năm
                                        start_date = start_date?.AddYears(day);
                                        end_date = end_date?.AddYears(day);
                                        break;
                                }
                                List<calendar_week> repeat_calendars = new List<calendar_week>();
                                List<calendar_member> repeat_members = new List<calendar_member>();
                                List<calendar_attend> repeat_attends = new List<calendar_attend>();
                                List<calendar_file> repeat_files = new List<calendar_file>();
                                List<calendar_log> repeat_logs = new List<calendar_log>();
                                int iterations = 1;
                                while (start_date?.Year == cy && iterations <= numeric_iterations) {
                                    iterations++;
                                    //model repeat
                                    calendar_week repeat_calendar = new calendar_week();
                                    repeat_calendar.calendar_id = helper.GenKey();
                                    repeat_calendar.parent_id = calendar.calendar_id;
                                    repeat_calendar.start_date = start_date;
                                    repeat_calendar.end_date = end_date;
                                    repeat_calendar.boardroom_id = calendar.boardroom_id;
                                    repeat_calendar.place_name = calendar.place_name;
                                    repeat_calendar.contents = calendar.contents;
                                    repeat_calendar.contents_en = calendar.contents_en;
                                    repeat_calendar.equip = calendar.equip;
                                    repeat_calendar.note = calendar.note;
                                    repeat_calendar.is_type = calendar.is_type;
                                    repeat_calendar.is_iterations = calendar.is_iterations;
                                    repeat_calendar.distance_iterations = calendar.distance_iterations;
                                    repeat_calendar.numeric_iterations = calendar.numeric_iterations;
                                    repeat_calendar.numeric_attendees = calendar.numeric_attendees;
                                    repeat_calendar.invitee = calendar.invitee;
                                    repeat_calendar.is_important = calendar.is_important;
                                    repeat_calendar.is_private = calendar.is_private;
                                    repeat_calendar.is_meeting = calendar.is_meeting;
                                    repeat_calendar.is_type_send = calendar.is_type_send;
                                    repeat_calendar.status = calendar.status;
                                    repeat_calendar.is_order = calendar.is_order;
                                    repeat_calendar.organization_id = calendar.organization_id;
                                    repeat_calendar.created_by = uid;
                                    repeat_calendar.created_date = DateTime.Now;
                                    repeat_calendar.created_ip = ip;
                                    repeat_calendar.created_token_id = tid;
                                    repeat_calendar.modified_by = null;
                                    repeat_calendar.modified_date = null;
                                    repeat_calendar.modified_ip = null;
                                    repeat_calendar.modified_token_id = null;
                                    repeat_calendars.Add(repeat_calendar);
                                    //member repeat
                                    var filter_repeat_members = await db.calendar_member.Where(a => a.calendar_id == calendar.calendar_id).ToListAsync();
                                    foreach (calendar_member mb in filter_repeat_members)
                                    {
                                        calendar_member repeat_member = new calendar_member();
                                        repeat_member.member_id = helper.GenKey();
                                        repeat_member.calendar_id = repeat_calendar.calendar_id;
                                        repeat_member.user_id = mb.user_id;
                                        repeat_member.is_type = mb.is_type;
                                        repeat_member.reason = mb.reason;
                                        repeat_member.note = mb.note;
                                        repeat_member.is_attend = mb.is_attend;
                                        repeat_member.status = mb.status;
                                        repeat_member.is_order = mb.is_order;
                                        repeat_member.created_by = uid;
                                        repeat_member.created_date = DateTime.Now;
                                        repeat_member.created_ip = ip;
                                        repeat_member.created_token_id = tid;
                                        repeat_member.modified_by = null;
                                        repeat_member.modified_date = null;
                                        repeat_member.modified_ip = null;
                                        repeat_member.modified_token_id = null;
                                        repeat_members.Add(repeat_member);
                                    }
                                    //attend repeat
                                    var filter_repeat_sttends = await db.calendar_attend.Where(a => a.calendar_id == calendar.calendar_id).ToListAsync();
                                    foreach (calendar_attend at in filter_repeat_sttends)
                                    {
                                        calendar_attend repeat_attend = new calendar_attend();
                                        repeat_attend.attend_id = helper.GenKey();
                                        repeat_attend.calendar_id = repeat_calendar.calendar_id;
                                        repeat_attend.department_id = at.department_id;
                                        repeat_attend.status = at.status;
                                        repeat_attend.is_order = at.is_order;
                                        repeat_attend.created_by = uid;
                                        repeat_attend.created_date = DateTime.Now;
                                        repeat_attend.created_ip = ip;
                                        repeat_attend.created_token_id = tid;
                                        repeat_attends.Add(repeat_attend);
                                    }
                                    //file repeat
                                    var filter_repeat_files = await db.calendar_file.Where(a => a.calendar_id == calendar.calendar_id && a.is_type == 0).ToListAsync();
                                    foreach (calendar_file fl in filter_repeat_files)
                                    {
                                        calendar_file repeat_file = new calendar_file();
                                        repeat_file.file_id = helper.GenKey();
                                        repeat_file.calendar_id = repeat_calendar.calendar_id;
                                        repeat_file.file_name = fl.file_name;
                                        repeat_file.file_path = fl.file_path;
                                        repeat_file.file_type = fl.file_type;
                                        repeat_file.file_size = fl.file_size;
                                        repeat_file.is_image = fl.is_image;
                                        repeat_file.is_type = fl.is_type;
                                        repeat_file.status = fl.status;
                                        repeat_file.is_order = fl.is_order;
                                        repeat_file.created_by = uid;
                                        repeat_file.created_date = DateTime.Now;
                                        repeat_file.created_ip = ip;
                                        repeat_file.created_token_id = tid;
                                        repeat_files.Add(repeat_file);
                                    }
                                    //log repeat
                                    calendar_log repeat_log = new calendar_log();
                                    repeat_log.message = "Nhân bản từ lịch \"" + calendar.contents + "\"";
                                    repeat_log.log_type = 0;
                                    repeat_log.key_id = repeat_calendar.calendar_id;
                                    repeat_log.is_view = false;
                                    repeat_log.created_by = uid;
                                    repeat_log.created_date = DateTime.Now;
                                    repeat_log.created_ip = ip;
                                    repeat_log.created_token_id = tid;
                                    repeat_log.organization_id = calendar.organization_id;
                                    repeat_logs.Add(repeat_log);

                                    switch (calendar.is_iterations)
                                    {
                                        case 1://Ngày
                                            start_date = start_date?.AddDays(day);
                                            end_date = end_date?.AddDays(day);
                                            break;
                                        case 2://Tuần
                                            start_date = start_date?.AddDays(7 * day);
                                            end_date = end_date?.AddDays(7 * day);
                                            break;
                                        case 3://Tháng
                                            start_date = start_date?.AddMonths(day);
                                            end_date = end_date?.AddMonths(day);
                                            break;
                                        case 4://Năm
                                            start_date = start_date?.AddYears(day);
                                            end_date = end_date?.AddYears(day);
                                            break;
                                    }
                                }
                                if (repeat_calendars.Count > 0)
                                {
                                    db.calendar_week.AddRange(repeat_calendars);
                                }
                                if (repeat_members.Count > 0)
                                {
                                    db.calendar_member.AddRange(repeat_members);
                                }
                                if (repeat_attends.Count > 0)
                                {
                                    db.calendar_attend.AddRange(repeat_attends);
                                }
                                if (repeat_files.Count > 0)
                                {
                                    db.calendar_file.AddRange(repeat_files);
                                }
                                if (repeat_logs.Count > 0)
                                {
                                    db.calendar_log.AddRange(repeat_logs);
                                }
                            }
                            #endregion

                            #region Comment
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                calendar_comment comment = new calendar_comment();
                                comment.comment_id = helper.GenKey();
                                comment.calendar_id = calendar_id;
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
                            string path = root + "/" + calendar.organization_id + "/Calendar/" + calendar_id;
                            bool exists = Directory.Exists(path);
                            if (!exists)
                                Directory.CreateDirectory(path);
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
                                string rootPath = path + "/" + name_file;
                                string Duongdan = "/Portals/" + calendar.organization_id + "/Calendar/" + calendar_id + "/" + name_file;
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
                                df.calendar_id = calendar.calendar_id;
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

                                switch (is_type_approve) {
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
                                log.key_id = calendar_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = calendar.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion

                            #region Send Message
                            switch (calendar.status)
                            {
                                case 2:
                                    sendContent = "Vừa ban hành lịch: \"" + calendar.contents + "\".";
                                    //Notify tất cả user tham gia lịch
                                    string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                                    var sqlpas = new List<SqlParameter>();
                                    sqlpas.Add(new SqlParameter("@" + "calendar_id", calendar.calendar_id));
                                    var arrpas = sqlpas.ToArray();
                                    var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_member_get", arrpas).Tables);
                                    var tables = await tasks;
                                    List<temp> temps = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                                    List<string> users = temps.Where(x => !sendUsers.Contains(x.user_id)).Select(x => x.user_id).ToList();
                                    send_message(uid, calendar.calendar_id, users, sendTitle, sendContent, (calendar.is_group ?? 0));
                                    notifications.Add(new Notification()
                                    {
                                        calendar_id = calendar.calendar_id,
                                        uids = sendUsers,
                                        title = sendTitle,
                                        text = sendContent,
                                    });
                                    break;
                                case 3:
                                    sendContent = "Vừa trả lại lịch: \"" + calendar.contents + "\".";
                                    break;
                                case 4:
                                    sendContent = "Vừa hủy lịch: \"" + calendar.contents + "\".";
                                    break;
                            }
                            if (sendUsers.Count > 0)
                            {
                                send_message(uid, calendar.calendar_id, sendUsers, sendTitle, sendContent, (calendar.is_group ?? 0));
                                notifications.Add(new Notification()
                                {
                                    calendar_id = calendar.calendar_id,
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/approve_calendar", ip, tid, "Lỗi khi duyệt lịch", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/approve_calendar", ip, tid, "Lỗi khi duyệt lịch", 0, "calendar_week");
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

                    foreach (var calendar_id in calendars)
                    {
                        var calendar = await db.calendar_week.FindAsync(calendar_id);
                        if (calendar != null)
                        {
                            //Send Message
                            string sendTitle = calendar.is_group == 0 ? "Lịch họp" : calendar.is_group == 1 ? "Lịch công tác" : "";
                            string sendContent = "Vừa ban hành lịch: \"" + calendar.contents + "\".";

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
                                signuser.calendar_id = calendar_id;
                                signuser.user_id = uid;
                                signuser.is_step = db.calendar_signuser.Count(x => x.calendar_id == calendar_id) + 1;
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

                            #region lịch lặp
                            if (calendar.status == 2 && calendar.is_iterations > 0)
                            {
                                int day = 0, numeric_iterations = 0;
                                if (calendar.distance_iterations != null)
                                {
                                    day = calendar.distance_iterations.Value;
                                }
                                if (calendar.numeric_iterations != null)
                                {
                                    numeric_iterations = calendar.numeric_iterations.Value;
                                }

                                int? cy = calendar.start_date?.Year;
                                DateTime? start_date = calendar.start_date;
                                DateTime? end_date = calendar.end_date ?? calendar.start_date;
                                switch (calendar.is_iterations)
                                {
                                    case 1://Ngày
                                        start_date = start_date?.AddDays(day);
                                        end_date = end_date?.AddDays(day);
                                        break;
                                    case 2://Tuần
                                        start_date = start_date?.AddDays(7 * day);
                                        end_date = end_date?.AddDays(7 * day);
                                        break;
                                    case 3://Tháng
                                        start_date = start_date?.AddMonths(day);
                                        end_date = end_date?.AddMonths(day);
                                        break;
                                    case 4://Năm
                                        start_date = start_date?.AddYears(day);
                                        end_date = end_date?.AddYears(day);
                                        break;
                                }
                                List<calendar_week> repeat_calendars = new List<calendar_week>();
                                List<calendar_member> repeat_members = new List<calendar_member>();
                                List<calendar_attend> repeat_attends = new List<calendar_attend>();
                                List<calendar_file> repeat_files = new List<calendar_file>();
                                List<calendar_log> repeat_logs = new List<calendar_log>();
                                int iterations = 1;
                                while (start_date?.Year == cy && iterations <= numeric_iterations)
                                {
                                    iterations++;
                                    //model repeat
                                    calendar_week repeat_calendar = new calendar_week();
                                    repeat_calendar.calendar_id = helper.GenKey();
                                    repeat_calendar.parent_id = calendar.calendar_id;
                                    repeat_calendar.start_date = start_date;
                                    repeat_calendar.end_date = end_date;
                                    repeat_calendar.boardroom_id = calendar.boardroom_id;
                                    repeat_calendar.place_name = calendar.place_name;
                                    repeat_calendar.contents = calendar.contents;
                                    repeat_calendar.contents_en = calendar.contents_en;
                                    repeat_calendar.equip = calendar.equip;
                                    repeat_calendar.note = calendar.note;
                                    repeat_calendar.is_type = calendar.is_type;
                                    repeat_calendar.is_iterations = calendar.is_iterations;
                                    repeat_calendar.distance_iterations = calendar.distance_iterations;
                                    repeat_calendar.numeric_iterations = calendar.numeric_iterations;
                                    repeat_calendar.numeric_attendees = calendar.numeric_attendees;
                                    repeat_calendar.invitee = calendar.invitee;
                                    repeat_calendar.is_important = calendar.is_important;
                                    repeat_calendar.is_private = calendar.is_private;
                                    repeat_calendar.is_meeting = calendar.is_meeting;
                                    repeat_calendar.is_type_send = calendar.is_type_send;
                                    repeat_calendar.status = calendar.status;
                                    repeat_calendar.is_order = calendar.is_order;
                                    repeat_calendar.organization_id = calendar.organization_id;
                                    repeat_calendar.created_by = uid;
                                    repeat_calendar.created_date = DateTime.Now;
                                    repeat_calendar.created_ip = ip;
                                    repeat_calendar.created_token_id = tid;
                                    repeat_calendar.modified_by = null;
                                    repeat_calendar.modified_date = null;
                                    repeat_calendar.modified_ip = null;
                                    repeat_calendar.modified_token_id = null;
                                    repeat_calendars.Add(repeat_calendar);
                                    //member repeat
                                    var filter_repeat_members = await db.calendar_member.Where(a => a.calendar_id == calendar.calendar_id).ToListAsync();
                                    foreach (calendar_member mb in filter_repeat_members)
                                    {
                                        calendar_member repeat_member = new calendar_member();
                                        repeat_member.member_id = helper.GenKey();
                                        repeat_member.calendar_id = repeat_calendar.calendar_id;
                                        repeat_member.user_id = mb.user_id;
                                        repeat_member.is_type = mb.is_type;
                                        repeat_member.reason = mb.reason;
                                        repeat_member.note = mb.note;
                                        repeat_member.is_attend = mb.is_attend;
                                        repeat_member.status = mb.status;
                                        repeat_member.is_order = mb.is_order;
                                        repeat_member.created_by = uid;
                                        repeat_member.created_date = DateTime.Now;
                                        repeat_member.created_ip = ip;
                                        repeat_member.created_token_id = tid;
                                        repeat_member.modified_by = null;
                                        repeat_member.modified_date = null;
                                        repeat_member.modified_ip = null;
                                        repeat_member.modified_token_id = null;
                                        repeat_members.Add(repeat_member);
                                    }
                                    //attend repeat
                                    var filter_repeat_sttends = await db.calendar_attend.Where(a => a.calendar_id == calendar.calendar_id).ToListAsync();
                                    foreach (calendar_attend at in filter_repeat_sttends)
                                    {
                                        calendar_attend repeat_attend = new calendar_attend();
                                        repeat_attend.attend_id = helper.GenKey();
                                        repeat_attend.calendar_id = repeat_calendar.calendar_id;
                                        repeat_attend.department_id = at.department_id;
                                        repeat_attend.status = at.status;
                                        repeat_attend.is_order = at.is_order;
                                        repeat_attend.created_by = uid;
                                        repeat_attend.created_date = DateTime.Now;
                                        repeat_attend.created_ip = ip;
                                        repeat_attend.created_token_id = tid;
                                        repeat_attends.Add(repeat_attend);
                                    }
                                    //file repeat
                                    var filter_repeat_files = await db.calendar_file.Where(a => a.calendar_id == calendar.calendar_id && a.is_type == 0).ToListAsync();
                                    foreach (calendar_file fl in filter_repeat_files)
                                    {
                                        calendar_file repeat_file = new calendar_file();
                                        repeat_file.file_id = helper.GenKey();
                                        repeat_file.calendar_id = repeat_calendar.calendar_id;
                                        repeat_file.file_name = fl.file_name;
                                        repeat_file.file_path = fl.file_path;
                                        repeat_file.file_type = fl.file_type;
                                        repeat_file.file_size = fl.file_size;
                                        repeat_file.is_image = fl.is_image;
                                        repeat_file.is_type = fl.is_type;
                                        repeat_file.status = fl.status;
                                        repeat_file.is_order = fl.is_order;
                                        repeat_file.created_by = uid;
                                        repeat_file.created_date = DateTime.Now;
                                        repeat_file.created_ip = ip;
                                        repeat_file.created_token_id = tid;
                                        repeat_files.Add(repeat_file);
                                    }
                                    //log repeat
                                    calendar_log repeat_log = new calendar_log();
                                    repeat_log.message = "Nhân bản từ lịch \"" + calendar.contents + "\"";
                                    repeat_log.log_type = 0;
                                    repeat_log.key_id = repeat_calendar.calendar_id;
                                    repeat_log.is_view = false;
                                    repeat_log.created_by = uid;
                                    repeat_log.created_date = DateTime.Now;
                                    repeat_log.created_ip = ip;
                                    repeat_log.created_token_id = tid;
                                    repeat_log.organization_id = calendar.organization_id;
                                    repeat_logs.Add(repeat_log);

                                    switch (calendar.is_iterations)
                                    {
                                        case 1://Ngày
                                            start_date = start_date?.AddDays(day);
                                            end_date = end_date?.AddDays(day);
                                            break;
                                        case 2://Tuần
                                            start_date = start_date?.AddDays(7 * day);
                                            end_date = end_date?.AddDays(7 * day);
                                            break;
                                        case 3://Tháng
                                            start_date = start_date?.AddMonths(day);
                                            end_date = end_date?.AddMonths(day);
                                            break;
                                        case 4://Năm
                                            start_date = start_date?.AddYears(day);
                                            end_date = end_date?.AddYears(day);
                                            break;
                                    }
                                }
                                if (repeat_calendars.Count > 0)
                                {
                                    db.calendar_week.AddRange(repeat_calendars);
                                }
                                if (repeat_members.Count > 0)
                                {
                                    db.calendar_member.AddRange(repeat_members);
                                }
                                if (repeat_attends.Count > 0)
                                {
                                    db.calendar_attend.AddRange(repeat_attends);
                                }
                                if (repeat_files.Count > 0)
                                {
                                    db.calendar_file.AddRange(repeat_files);
                                }
                                if (repeat_logs.Count > 0)
                                {
                                    db.calendar_log.AddRange(repeat_logs);
                                }
                            }
                            #endregion

                            #region file
                            string root = HttpContext.Current.Server.MapPath("~/Portals");
                            string path = root + "/" + calendar.organization_id + "/Calendar/" + calendar_id;
                            bool exists = Directory.Exists(path);
                            if (!exists)
                                Directory.CreateDirectory(path);
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
                                string rootPath = path + "/" + name_file;
                                string Duongdan = "/Portals/" + calendar.organization_id + "/Calendar/" + calendar_id + "/" + name_file;
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
                                df.calendar_id = calendar.calendar_id;
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
                                log.key_id = calendar_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = calendar.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion

                            #region Send Message
                            //Notify tất cả user tham gia lịch
                            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                            var sqlpas = new List<SqlParameter>();
                            sqlpas.Add(new SqlParameter("@" + "calendar_id", calendar.calendar_id));
                            var arrpas = sqlpas.ToArray();
                            var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_member_get", arrpas).Tables);
                            var tables = await tasks;
                            List<temp> temps = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                            List<string> sendUsers = temps.Select(x => x.user_id).ToList();
                            send_message(uid, calendar.calendar_id, sendUsers, sendTitle, sendContent, (calendar.is_group ?? 0));
                            notifications.Add(new Notification()
                            {
                                calendar_id = calendar.calendar_id,
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/enact_calendar", ip, tid, "Lỗi khi duyệt lịch", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/enact_calendar", ip, tid, "Lỗi khi duyệt lịch", 0, "calendar_week");
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
                        foreach (var calendar_id in ids)
                        {
                            #region Model
                            var calendar = await db.calendar_week.FindAsync(calendar_id);

                            //Send Message
                            string sendTitle = calendar.is_group == 0 ? "Lịch họp" : calendar.is_group == 1 ? "Lịch công tác" : "";
                            string sendContent = "Vừa ban hủy lịch: \"" + calendar.contents + "\".";

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
                                log.key_id = calendar_id;
                                log.created_by = uid;
                                log.is_view = false;
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                log.organization_id = calendar.organization_id;
                                db.calendar_log.Add(log);
                            }
                            #endregion

                            #region Send Message
                            //Notify tất cả user tham gia lịch
                            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                            var sqlpas = new List<SqlParameter>();
                            sqlpas.Add(new SqlParameter("@" + "calendar_id", calendar.calendar_id));
                            var arrpas = sqlpas.ToArray();
                            var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_member_get", arrpas).Tables);
                            var tables = await tasks;
                            List<temp> temps = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                            List<string> sendUsers = temps.Select(x => x.user_id).ToList();
                            send_message(uid, calendar.calendar_id, sendUsers, sendTitle, sendContent, (calendar.is_group ?? 0));
                            notifications.Add(new Notification()
                            {
                                calendar_id = calendar.calendar_id,
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_week/cancel_calendar", ip, tid, "Lỗi khi cập nhật phòng họp họp", 0, "calendar_week");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_week/cancel_calendar", ip, tid, "Lỗi khi xoá phòng họp", 0, "calendar_week");
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

        #region Comment
        [HttpPut]
        public async Task<HttpResponseMessage> send_comment()
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
                    bool isEdit = bool.Parse(provider.FormData.GetValues("isEdit").SingleOrDefault());
                    bool isReply = bool.Parse(provider.FormData.GetValues("isReply").SingleOrDefault());
                    string calendar_id = provider.FormData.GetValues("calendar_id").SingleOrDefault();
                    string reply_comment_id = provider.FormData.GetValues("reply_comment_id").SingleOrDefault();
                    var md = provider.FormData.GetValues("model").SingleOrDefault();
                    calendar_comment model = JsonConvert.DeserializeObject<calendar_comment>(md);

                    //Send Message
                    var calendar = await db.calendar_week.FindAsync(calendar_id);
                    if (calendar != null)
                    {
                        string sendTitle = calendar.is_group == 0 ? "Lịch họp" : calendar.is_group == 1 ? "Lịch công tác" : "";
                        string sendContent = "Vừa gửi bình luận đến bạn trong lịch: \"" + calendar.contents + "\".";

                        #region model
                        if (isReply)
                        {
                            model.parent_id = reply_comment_id;
                        }
                        if (!isEdit)
                        {
                            model.comment_id = helper.GenKey();
                            model.calendar_id = calendar_id;
                            model.is_type = 0;
                            model.is_delete = false;
                            model.created_by = uid;
                            model.created_date = DateTime.Now;
                            model.created_ip = ip;
                            model.created_token_id = tid;
                            db.calendar_comment.Add(model);
                        }
                        else
                        {
                            model.modified_by = uid;
                            model.modified_date = DateTime.Now;
                            model.modified_ip = ip;
                            model.modified_token_id = tid;
                            db.Entry(model).State = EntityState.Modified;
                        }
                        #endregion

                        #region file
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        string path = root + "/" + model.organization_id + "/Calendar/" + model.calendar_id;
                        bool exists = Directory.Exists(path);
                        if (!exists)
                            Directory.CreateDirectory(path);
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
                            string name_file = org_name_file; // helper.UniqueFileName(org_name_file);
                            string rootPath = path + "/" + name_file;
                            string Duongdan = "/Portals/" + model.organization_id + "/Calendar/" + model.calendar_id + "/" + name_file;
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
                            df.calendar_id = model.calendar_id;
                            df.comment_id = model.comment_id;
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
                            df.is_type = 1;
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

                        await db.SaveChangesAsync();

                        #region Send Message
                        //Notify tất cả user tham gia lịch
                        string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                        var sqlpas = new List<SqlParameter>();
                        sqlpas.Add(new SqlParameter("@" + "calendar_id", calendar.calendar_id));
                        var arrpas = sqlpas.ToArray();
                        var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_member_get", arrpas).Tables);
                        var tables = await tasks;
                        List<temp> temps = JsonConvert.DeserializeObject<List<temp>>(JsonConvert.SerializeObject(tables[0]));
                        List<string> sendUsers = temps.Select(x => x.user_id).ToList();
                        send_message(uid, calendar.calendar_id, sendUsers, sendTitle, sendContent, (calendar.is_group ?? 0));
                        notifications.Add(new Notification()
                        {
                            calendar_id = calendar.calendar_id,
                            uids = sendUsers,
                            title = sendTitle,
                            text = sendContent,
                        });
                        #endregion
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.SerializeObject(notifications) });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/approve_calendar", ip, tid, "Lỗi khi duyệt lịch", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/approve_calendar", ip, tid, "Lỗi khi duyệt lịch", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_calendar_comment([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        var das = await db.calendar_comment.Where(a => ids.Contains(a.comment_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            string root = HttpContext.Current.Server.MapPath("~/");
                            List<calendar_comment> del = new List<calendar_comment>();
                            foreach (var da in das)
                            {
                                del.Add(da);

                                var files = await db.calendar_file.Where(x => x.calendar_id == da.calendar_id && x.is_type == 1).ToListAsync();
                                if (files.Count > 0)
                                {
                                    foreach (var f in files)
                                    {
                                        var rootPath = root + "/" + f.file_path;
                                        if (System.IO.File.Exists(rootPath))
                                        {
                                            System.IO.File.Delete(rootPath);
                                        }
                                    }
                                }

                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    calendar_log log = new calendar_log();
                                    log.log_type = 0;
                                    log.message = "Xóa binh luận: " + da.contents;
                                    log.key_id = da.calendar_id;
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
                            db.calendar_comment.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_week/delete_calendar_comment", ip, tid, "Lỗi khi xoa binh luan", 0, "calendar_week");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "calendar_week/delete_calendar_comment", ip, tid, "Lỗi khi xoa binh luan", 0, "calendar_week");
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

        #region cut rice
        [HttpPut]
        public async Task<HttpResponseMessage> cut_rice()
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
                    var d = provider.FormData.GetValues("dates").SingleOrDefault();
                    List<DateTime> dates = JsonConvert.DeserializeObject<List<DateTime>>(d);
                    if (dates != null && dates.Count > 0)
                    {
                        booking_meal book = new booking_meal();
                        book.booking_id = helper.GenKey();
                        book.reason = "Đi công tác";
                        book.user_id = uid;
                        book.created_date = DateTime.Now;
                        book.created_by = uid;
                        book.created_ip = ip;
                        book.organization_id = user_now.organization_id;
                        book.is_order = db.booking_meal.Count() + 1;
                        db.booking_meal.Add(book);

                        List<booking_user_date> user_dates = new List<booking_user_date>();
                        int stt = 0;
                        foreach (DateTime date in dates)
                        {
                            booking_user_date user_date = new booking_user_date();
                            user_date.booking_date = date;
                            user_date.user_id = uid;
                            user_date.booking_id = book.booking_id;
                            user_date.is_order = stt++;
                            user_dates.Add(user_date);
                        }
                        if (user_dates.Count > 0)
                        {
                            db.booking_user_date.AddRange(user_dates);
                        }
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/cut_rice", ip, tid, "Lỗi khi cập nhật cắt cơm", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/cut_rice", ip, tid, "Lỗi khi cập nhật cắt cơm", 0, "calendar_week");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

        #region Export XML
        private const string InitVector = "T=A4rAzu94ez-dra";
        private const int KeySize = 256;
        private const int PasswordIterations = 1000; //2;
        private const string SaltValue = "d=?ustAF=UstenAr3B@pRu8=ner5sW&h59_Xe9P2za-eFr2fa&ePHE@ras!a+uc@";
        public class calendardutyxml
        {
            public string id { get; set; }
            public string thu { get; set; }
            public string ngaytruc { get; set; }
            //public string catruc { get; set; }
            public string trucban { get; set; }
            public string capbac { get; set; }
            public string trucchihuy { get; set; }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> export_xml([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);
            try
            {
                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

                try
                {
                    DateTime week_start_date = new DateTime();
                    DateTime week_end_date = new DateTime();
                    var sqlpas = new List<SqlParameter>();
                    if (proc != null && proc.par != null)
                    {
                        foreach (sqlPar p in proc.par)
                        {
                            if (p.par == "week_start_date")
                            {
                                week_start_date = DateTime.Parse(p.va);
                            }
                            if (p.par == "week_end_date")
                            {
                                week_end_date = DateTime.Parse(p.va);
                            }
                            sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                        }
                    }
                    var arrpas = sqlpas.ToArray();
                    DateTime sdate = DateTime.Now;
                    var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables);
                    var tables = await task;
                    DateTime edate = DateTime.Now;
                    #region add SQLLog
                    if (helper.wlog)
                    {
                        using (DBEntities db = new DBEntities())
                        {
                            sql_log log = new sql_log();
                            log.controller = domainurl + "calendar/export_xml";
                            log.start_date = sdate;
                            log.end_date = edate;
                            log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                            log.user_id = uid;
                            log.token_id = tid;
                            log.created_ip = ip;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.modified_ip = ip;
                            log.modified_date = DateTime.Now;
                            log.modified_by = uid;
                            log.modified_token_id = tid;
                            log.full_name = name;
                            log.title = proc.proc;
                            log.log_content = JsonConvert.SerializeObject(new { data = proc });
                            db.sql_log.Add(log);
                            await db.SaveChangesAsync();
                        }
                    }
                    #endregion
                    List<calendardutyxml> result = JsonConvert.DeserializeObject<List<calendardutyxml>>(JsonConvert.SerializeObject(tables[0]));
                    string xml_result = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>";
                    if (result != null && result.Count > 0)
                    {
                        xml_result += "<document>";
                        foreach (var duty in result)
                        {
                            xml_result += "<element>";
                            xml_result += "<id>" + (duty.id ?? "") + "</id>";
                            xml_result += "<thu>" + (duty.thu ?? "") + "</thu>";
                            xml_result += "<ngaytruc>" + (duty.ngaytruc ?? "") + "</ngaytruc>";
                            //xml_result += "<catruc>" + (duty.catruc ?? "") + "</catruc>";
                            xml_result += "<trucban>" + (duty.trucban ?? "") + "</trucban>";
                            xml_result += "<capbac>" + (duty.capbac ?? "") + "</capbac>";
                            xml_result += "<trucchihuy>" + (duty.trucchihuy ?? "") + "</trucchihuy>";
                            xml_result += "</element>";
                        }
                        xml_result += "</document>";
                    }
                    using (DBEntities db = new DBEntities())
                    {
                        var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                        System.Net.WebClient webc = new System.Net.WebClient();
                        string path = helper.path_xml + "/Calendar";
                        bool exists = Directory.Exists(path);
                        if (!exists)
                            Directory.CreateDirectory(path);

                        string name_file = helper.GenKey() + ".xml";
                        string root_path = path + "/" + name_file;
                        string duong_dan = helper.path_xml + "/Calendar/" + name_file;

                        File.WriteAllText(root_path, xml_result);
                        var res_encr = helper.encryptXML(root_path, "document", helper.psKey);
                        if (res_encr != "OK") {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Không thể mã hoã file XML!", err = "1" });
                        };

                        //xml_result = Encrypt(xml_result, helper.psKey);
                        //File.WriteAllText(root_path, xml_result);

                        //file
                        calendar_file file = new calendar_file();
                        file.file_id = helper.GenKey();
                        file.file_name = "lichtrucban" + "_" + week_start_date.ToString("ddMMyyyy") + "_" + week_end_date.ToString("ddMMyyyy") + ".xml";
                        file.file_path = duong_dan;
                        file.file_type = "xml";
                        var file_info = new FileInfo(root_path);
                        file.file_size = file_info.Length;
                        file.is_image = helper.IsImageFileName(name_file);
                        file.is_type = 5;
                        file.status = true;
                        file.created_by = uid;
                        file.created_date = DateTime.Now;
                        file.created_ip = ip;
                        file.created_token_id = tid;
                        db.calendar_file.Add(file);
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/export_xml", ip, tid, "Lỗi khi export file xml", 0, "calendar_week");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/export_xml", ip, tid, "Lỗi khi export file xml", 0, "calendar_week");
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

        //[HttpPost]
        //public async Task<HttpResponseMessage> download_xml([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    IEnumerable<Claim> claims = identity.Claims;
        //    string dataProc = data["str"].ToObject<string>();
        //    string des = Codec.DecryptString(dataProc, helper.psKey);
        //    sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);
        //    try
        //    {
        //        string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
        //        string ip = getipaddress();
        //        string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
        //        string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
        //        string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
        //        string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

        //        try
        //        {
        //            var sqlpas = new List<SqlParameter>();
        //            if (proc != null && proc.par != null)
        //            {
        //                foreach (sqlPar p in proc.par)
        //                {
        //                    sqlpas.Add(new SqlParameter("@" + p.par, p.va));
        //                }
        //            }
        //            var arrpas = sqlpas.ToArray();
        //            DateTime sdate = DateTime.Now;
        //            var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables);
        //            var tables = await task;
        //            DateTime edate = DateTime.Now;
        //            #region add SQLLog
        //            if (helper.wlog)
        //            {
        //                using (DBEntities db = new DBEntities())
        //                {
        //                    sql_log log = new sql_log();
        //                    log.controller = domainurl + "calendar/export_xml";
        //                    log.start_date = sdate;
        //                    log.end_date = edate;
        //                    log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
        //                    log.user_id = uid;
        //                    log.token_id = tid;
        //                    log.created_ip = ip;
        //                    log.created_date = DateTime.Now;
        //                    log.created_by = uid;
        //                    log.created_token_id = tid;
        //                    log.modified_ip = ip;
        //                    log.modified_date = DateTime.Now;
        //                    log.modified_by = uid;
        //                    log.modified_token_id = tid;
        //                    log.full_name = name;
        //                    log.title = proc.proc;
        //                    log.log_content = JsonConvert.SerializeObject(new { data = proc });
        //                    db.sql_log.Add(log);
        //                    await db.SaveChangesAsync();
        //                }
        //            }
        //            #endregion
        //            List<calendar_file> result = JsonConvert.DeserializeObject<List<calendar_file>>(JsonConvert.SerializeObject(tables[0]));

        //            using (DBEntities db = new DBEntities())
        //            {
        //                var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
        //                WebClient webc = new WebClient();
        //                string root = HttpContext.Current.Server.MapPath("~/Portals");
        //                string path = root + "/" + user_now.organization_id + "/XML/" + user_now.user_id;
        //                bool exists = Directory.Exists(path);
        //                if (!exists)
        //                    Directory.CreateDirectory(path);

        //                string name_file = helper.GenKey() + ".xml";
        //                string root_path = path + "/" + name_file;
        //                string duong_dan = "/Portals/" + user_now.organization_id + "/XML/" + user_now.user_id + "/" + name_file;
        //                string url = ConfigurationManager.AppSettings["ValidAudience"] + duong_dan;
        //                var file_path = result.Select(x => x.file_path).FirstOrDefault();
        //                if (file_path != null)
        //                {
        //                    string xml_result = "";
        //                    string old = root + "/" + file_path;
        //                    //Read file
        //                    var OuterXml = File.ReadAllText(old, Encoding.UTF8);
        //                    //Decrypt
        //                    xml_result = Decrypt(OuterXml, helper.psKey);
        //                    //Download
        //                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

        //                    //byte[] bytes = File.ReadAllBytes(root_path);
        //                    //response.Content = new ByteArrayContent(bytes);
        //                    //response.Content.Headers.ContentLength = bytes.LongLength;
        //                    //response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        //                    //{
        //                    //    FileName = file.file_name
        //                    //};
        //                    //response.Content.Headers.ContentDisposition.FileName = file.file_name;
        //                    //response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(file.file_name));
        //                    //return response;

        //                    var builder = new StringBuilder();
        //                    using (var writer = new StringWriter(builder))
        //                    {
        //                        response.Content = new StringContent(xml_result, Encoding.UTF8, "application/xml");
        //                        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
        //                    }
        //                    return response;
        //                }
        //                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
        //            }
        //        }
        //        catch (DbEntityValidationException e)
        //        {
        //            string contents = helper.getCatchError(e, null);
        //            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/download_xml", ip, tid, "Lỗi khi tải xuống file xml", 0, "calendar_week");
        //            if (!helper.debug)
        //            {
        //                contents = "";
        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
        //        }
        //        catch (Exception e)
        //        {
        //            string contents = helper.ExceptionMessage(e);
        //            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "calendar_week/download_xml", ip, tid, "Lỗi khi tải xuống file xml", 0, "calendar_week");
        //            if (!helper.debug)
        //            {
        //                contents = "";
        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);
        //    }
        //}
        public string Encrypt(string plainText, string psPhrase)
        {
            string encryptedText;
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(InitVector);
            byte[] pswordBytes = Encoding.UTF8.GetBytes(psPhrase);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] saltValueBytes = Encoding.UTF8.GetBytes(SaltValue);

            Rfc2898DeriveBytes psword = new Rfc2898DeriveBytes(pswordBytes, saltValueBytes, PasswordIterations);
            byte[] keyBytes = psword.GetBytes(KeySize / 8);

            RijndaelManaged rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC };

            using (ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(keyBytes, initVectorBytes))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cryptoStream.FlushFinalBlock();

                        byte[] cipherTextBytes = memoryStream.ToArray();
                        encryptedText = Convert.ToBase64String(cipherTextBytes);
                    }
                }
            }

            return encryptedText;
        }
        public string Decrypt(string encryptedText, string psPhrase)
        {
            byte[] encryptedTextBytes = Convert.FromBase64String(encryptedText);
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(InitVector);
            byte[] pswordBytes = Encoding.UTF8.GetBytes(psPhrase);
            string plainText;
            byte[] saltValueBytes = Encoding.UTF8.GetBytes(SaltValue);

            Rfc2898DeriveBytes psword = new Rfc2898DeriveBytes(pswordBytes, saltValueBytes, PasswordIterations);
            byte[] keyBytes = psword.GetBytes(KeySize / 8);

            RijndaelManaged rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC };

            using (ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor(keyBytes, initVectorBytes))
            {
                using (MemoryStream memoryStream = new MemoryStream(encryptedTextBytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        //TODO: Need to look into this more. Assuming encrypted text is longer than plain but there is probably a better way
                        byte[] plainTextBytes = new byte[encryptedTextBytes.Length];

                        int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                    }
                }
            }

            return plainText;
        }
        #endregion

        #region filter doc
        [HttpPost]
        public async Task<HttpResponseMessage> filter_doc_master_list_receive([System.Web.Mvc.Bind(Include = "organization_id,user_key,typeCount,pageno,pagesize,search,fields,order_by")][FromBody] JObject data)
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
                int organization_id = data["organization_id"].ToObject<int>();
                int user_key = data["user_key"].ToObject<int>();
                int typeCount = data["typeCount"].ToObject<int>();
                int pageno = data["pageno"].ToObject<int>();
                int pagesize = data["pagesize"].ToObject<int>();
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
                        var usernow = db.sys_users.Find(uid);
                        if (usernow != null)
                        {
                            string WhereSQL = "";

                            if (usernow.is_super == true)
                            {
                                WhereSQL += "";
                            }
                            //Default
                            WhereSQL += @"
                                (" + typeCount + @" is null or " + typeCount + @" = 0 or (" + typeCount + @" is not null and " + typeCount + @"<>0 and 
	                            ((" + typeCount + @" = 1 and nav_type = 1) or (" + typeCount + @" = 2 and nav_type = 2) or (" + typeCount + @" = 3 and nav_type = 3) 
	                            or (" + typeCount + @" = 4 and 0 > 0 /*Bổ sung task lien quan sau*/) 
	                            or (" + typeCount + @" = 5 and (select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") is not null) 
	                            or (" + typeCount + @" = 6 and stt.is_handle = 1) 
	                            or (" + typeCount + @" = 7 and date_deadline is not null and date_deadline <0 and status_id not in ('hoanthanh','phanphat','dadongdau')))))
                            ";
                            //serch
                            if (!string.IsNullOrWhiteSpace(search))
                            {
                                WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + " (do.compendium like N'%" + search + "%')";
                            }
                            //filed
                            if (fields != null && fields.Count > 0)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.filteroperator == "in")
                                    {
                                        WhereSQL += (WhereSQL != "" ? " and " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => (field.type_of == 1 ? "N" : "") + "'" + a.value + "'").ToList()) + ")";
                                    }
                                    else
                                    {
                                        foreach (var m in field.filterconstraints.Where(a => a.value != null))
                                        {
                                            switch (m.matchMode)
                                            {
                                                case "contains":
                                                    WhereSQL += " " + field.filteroperator + " (N'" + m.value + "' like N'%' + do." + field.key + " + ',%')";
                                                    break;
                                                case "containsMany":
                                                    List<string> listKey = m.value.Split(',').ToList();
                                                    WhereSQL += " " + field.filteroperator + " (";
                                                    foreach (var str in listKey)
                                                    {
                                                        if (str.Trim() != "")
                                                        {
                                                            WhereSQL += " ((do." + field.key + " + ',')" + " like N'%' + " + "N'" + str + "' + ',%')  or";
                                                        }
                                                    }
                                                    if (WhereSQL.EndsWith(" or"))
                                                    {
                                                        WhereSQL = WhereSQL.Substring(0, WhereSQL.Length - 3);
                                                    }
                                                    WhereSQL += ")";
                                                    break;
                                                case "equals":
                                                    WhereSQL += " " + field.filteroperator + " do." + field.key + " = N'" + m.value + "'";
                                                    break;
                                                case "dateBefore":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                                    break;
                                                case "dateAfter":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                                    break;

                                            }
                                        }
                                    }
                                }
                            }
                            //Cut
                            if (WhereSQL.StartsWith(" and "))
                            {
                                WhereSQL = WhereSQL.Substring(4);
                            }
                            else if (WhereSQL.StartsWith(" or "))
                            {
                                WhereSQL = WhereSQL.Substring(3);
                            }
                            //Select
                            sql = @" 
                                        ;WITH us_group AS(
		                                    select role_group_id
		                                    from doc_ca_role_groups gr
		                                    where '" + usernow.user_id + @"' in (select user_id from doc_ca_role_group_users where role_group_id = gr.role_group_id)
	                                    ),
	                                    approval_doc AS (
				                                    select fl.doc_master_id, max(send_date) as send_date
				                                    from doc_follows fl
				                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and receive_by in (select role_group_id from us_group)) or (receive_type = 2 and receive_by = " + organization_id + @")) and doc_status_id in ('xulychinh','chopheduyet','chodongdau','dadongdau','tralai') and is_recall = 0 and not exists(select follow_id from doc_follows where follow_parent_id = fl.follow_id and is_recall = 0)
				                                    group by fl.doc_master_id,fl.organization_id
	                                    ),
	                                    follows as (
		                                    select send_by, ap.send_date, send_by_name, follow_id, fl.doc_master_id,doc_status_id,[message],case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end as date_deadline,
		                                    view_date, is_completed, deadline_date
		                                    from doc_follows fl join approval_doc ap on fl.doc_master_id = ap.doc_master_id and fl.send_date = ap.send_date
		                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and receive_by in (select role_group_id from us_group)) or (receive_type = 2 and receive_by = " + organization_id + @")) and doc_status_id in ('xulychinh','chopheduyet','chodongdau','dadongdau','tralai') and is_recall = 0
		                                    union all
		                                    select send_by, send_date, send_by_name, follow_id, fl.doc_master_id,doc_status_id,[message],case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end as date_deadline,
		                                    view_date, is_completed,deadline_date
		                                    from doc_follows fl
		                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and receive_by in (select role_group_id from us_group)) or (receive_type = 2 and receive_by = " + organization_id + @")) and doc_status_id not in ('xulychinh','chopheduyet','chodongdau','dadongdau','tralai') and is_recall = 0
		                                    union all 
		                                    select created_by, created_date, (select full_name from sys_users where user_key = doc_master.created_by), null, doc_master_id,doc_status_id,null,null,null,null,null
		                                    from doc_master
		                                    where created_by = " + user_key + @" and doc_status_id in ('duthao','sohoa')
	                                    )
	                                    select handle_date, do.doc_master_id, fl.[message],compendium, doc_code, nav_type, doc_date, fl.send_by,fl.send_by_name,fl.send_date,fl.follow_id,us.avatar,
	                                    stt.status_id, stt.status_name, stt.background_color, stt.text_color,do.first_doc_status_id,do.file_path,fl.is_completed,
	                                    (select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") as view_id, view_date, date_deadline, abs(date_deadline) as abs_date_deadline, fl.deadline_date, do.deadline_date as deadline_date_master
	                                    from follows fl join doc_master do on do.doc_master_id = fl.doc_master_id
	                                    join sys_users us on fl.send_by = us.user_key
	                                    join doc_ca_status stt on ((fl.is_completed is null and fl.doc_status_id = stt.status_id) or (fl.is_completed = 1 and stt.status_id = 'hoanthanh'))";

                            if (WhereSQL.Trim() != "")
                            {
                                sql += " where " + WhereSQL;
                            }
                            string OFFSET = @"(" + pageno + @") * (" + pagesize + @")";
                            sql += @" ORDER BY " + order_by
                                + @"\n OFFSET " + OFFSET + " ROWS FETCH NEXT " + pagesize + " ROWS ONLY ";
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
                        domainurl + "/Doc_Main/filter_doc_master_list_receive", ip, tid, "Lỗi khi gọi filter_doc_master_list_receive", 0, "SQL");
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
                       domainurl + "/Doc_Main/filter_doc_master_list_receive", ip, tid, "Lỗi khi gọi filter_doc_master_list_receive", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> filter_doc_master_count_receive([System.Web.Mvc.Bind(Include = "organization_id,user_key,search,fields")][FromBody] JObject data)
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
                int organization_id = data["organization_id"].ToObject<int>();
                int user_key = data["user_key"].ToObject<int>();
                string search = data["search"].ToObject<string>();
                List<FieldSQL> fields = data["fields"].ToObject<List<FieldSQL>>();

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
                        var usernow = db.sys_users.Find(uid);
                        if (usernow != null)
                        {
                            string WhereSQL = "";

                            if (usernow.is_super == true)
                            {
                                WhereSQL += "";
                            }
                            //serch
                            if (!string.IsNullOrWhiteSpace(search))
                            {
                                WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + " (do.compendium like N'%" + search + "%')";
                            }
                            //filed
                            if (fields != null && fields.Count > 0)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.filteroperator == "in")
                                    {
                                        WhereSQL += (WhereSQL != "" ? " and " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => (field.type_of == 1 ? "N" : "") + "'" + a.value + "'").ToList()) + ")";
                                    }
                                    else
                                    {
                                        foreach (var m in field.filterconstraints.Where(a => a.value != null))
                                        {
                                            switch (m.matchMode)
                                            {
                                                case "contains":
                                                    WhereSQL += " " + field.filteroperator + " (N'" + m.value + "' like N'%' + do." + field.key + " + ',%')";
                                                    break;
                                                case "containsMany":
                                                    List<string> listKey = m.value.Split(',').ToList();
                                                    WhereSQL += " " + field.filteroperator + " (";
                                                    foreach (var str in listKey)
                                                    {
                                                        if (str.Trim() != "")
                                                        {
                                                            WhereSQL += " ((do." + field.key + " + ',')" + " like N'%' + " + "N'" + str + "' + ',%')  or";
                                                        }
                                                    }
                                                    if (WhereSQL.EndsWith(" or"))
                                                    {
                                                        WhereSQL = WhereSQL.Substring(0, WhereSQL.Length - 3);
                                                    }
                                                    WhereSQL += ")";
                                                    break;
                                                case "equals":
                                                    WhereSQL += " " + field.filteroperator + " do." + field.key + " = N'" + m.value + "'";
                                                    break;
                                                case "dateBefore":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                                    break;
                                                case "dateAfter":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                                    break;

                                            }
                                        }
                                    }
                                }
                            }
                            //Cut
                            if (WhereSQL.StartsWith(" and "))
                            {
                                WhereSQL = WhereSQL.Substring(4);
                            }
                            else if (WhereSQL.StartsWith(" or "))
                            {
                                WhereSQL = WhereSQL.Substring(3);
                            }
                            //Select
                            sql = @" 
                                        ;WITH us_group AS(
		                                    select role_group_id
		                                    from doc_ca_role_groups gr
		                                    where '" + usernow.user_id + @"' in (select user_id from doc_ca_role_group_users where role_group_id = gr.role_group_id)
	                                    ),
	                                    approval_doc AS (
				                                    select fl.doc_master_id, max(send_date) as send_date
				                                    from doc_follows fl
				                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and receive_by in (select role_group_id from us_group)) or (receive_type = 2 and receive_by = " + organization_id + @")) and doc_status_id in ('xulychinh','chopheduyet','chodongdau','dadongdau','tralai') and is_recall = 0 and not exists(select follow_id from doc_follows where follow_parent_id = fl.follow_id and is_recall = 0)
				                                    group by fl.doc_master_id,fl.organization_id
	                                    ),
	                                    follows as (
		                                    select send_by, ap.send_date, send_by_name, follow_id, fl.doc_master_id,doc_status_id,[message],deadline_date
		                                    from doc_follows fl join approval_doc ap on fl.doc_master_id = ap.doc_master_id and fl.send_date = ap.send_date
		                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and receive_by in (select role_group_id from us_group)) or (receive_type = 2 and receive_by = " + organization_id + @")) and doc_status_id in ('xulychinh','chopheduyet','dadongdau','chodongdau','tralai') and is_recall = 0
		                                    union all
		                                    select send_by, send_date, send_by_name, follow_id, fl.doc_master_id,doc_status_id,[message],deadline_date
		                                    from doc_follows fl
		                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and receive_by in (select role_group_id from us_group)) or (receive_type = 2 and receive_by = " + organization_id + @")) and doc_status_id not in ('xulychinh','chopheduyet','dadongdau','chodongdau','tralai') and is_recall = 0
		                                    union all 
		                                    select created_by, created_date, (select full_name from sys_users where user_key = doc_master.created_by), null, doc_master_id,doc_status_id,null,null
		                                    from doc_master
		                                    where created_by = " + user_key + @" and doc_status_id in ('duthao','sohoa')
	                                    )
	                                    select do.doc_master_id, fl.[message],compendium, doc_code, nav_type, doc_date, fl.send_by,fl.send_by_name,fl.send_date,fl.follow_id,us.avatar,
	                                    stt.status_id, stt.status_name, stt.background_color, stt.text_color, stt.is_handle,case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end as date_deadline,
	                                    (select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") as view_id,
	                                    0 as countTask
	                                    into #Doc
	                                    from follows fl join doc_master do on do.doc_master_id = fl.doc_master_id
	                                    join sys_users us on fl.send_by = us.user_key
	                                    join doc_ca_status stt on fl.doc_status_id = stt.status_id";

                            if (WhereSQL.Trim() != "") {
                                sql += " where " + WhereSQL;
                            }
                            sql += @"
                                Declare @handle int,@ood int, @receive int, @send int, @internal int, @notseen int, @all int, @reltask int
		                        set @all = (select count(*) from #Doc)
		                        set @receive = (select count(*) from #Doc where nav_type = 1)
		                        set @send = (select count(*) from #Doc where nav_type = 2 )
		                        set @internal = (select count(*) from #Doc where nav_type = 3)
		                        SET @handle=(Select count(*) from #Doc where is_handle=1)
		                        SET @notseen=(Select count(*) from #Doc where view_id is null)
		                        SET @ood=(Select count(*) from #Doc where date_deadline is not null and date_deadline <0 and status_id not in ('hoanthanh','phanphat','dadongdau'))
		                        SET @reltask = (Select count(*) from #Doc where countTask > 0)
		                        Select @all as [all], @receive as [receive], @send [send],@internal internal,@handle as handle,@notseen notseen, @ood as ood, @reltask as reltask
                            ";
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
                        domainurl + "/Doc_Main/filter_doc_master_count_receive", ip, tid, "Lỗi khi gọi filter_doc_master_list_receive", 0, "SQL");
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
                       domainurl + "/Doc_Main/filter_doc_master_count_receive", ip, tid, "Lỗi khi gọi filter_doc_master_list_receive", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        #endregion

        #region Send Message
        public void send_message(string user_send, string id_key, List<string> users, string title, string content, int is_type)
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

        public class temp {
            public string user_id { get; set; }
        }

        public class Notification {

            public string calendar_id { get; set; }
            public List<string> uids { get; set; }
            public string title { get; set; }
            public string text { get; set; }
        }
        #endregion
    }
}