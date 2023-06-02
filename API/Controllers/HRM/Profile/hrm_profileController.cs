using API.Models;
using API.Helper;
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
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Globalization;
using Microsoft.Owin;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System.Diagnostics.Contracts;
using System.Drawing;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace API.Controllers.HRM.Profile
{
    [Authorize(Roles = "login")]
    public class hrm_profileController : ApiController
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

        #region Update contract
        [HttpPut]
        public async Task<HttpResponseMessage> update_profile()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                    bool isAdd = bool.Parse(provider.FormData.GetValues("isAdd").SingleOrDefault());
                    var md = provider.FormData.GetValues("model").SingleOrDefault();
                    hrm_profile model = JsonConvert.DeserializeObject<hrm_profile>(md);
                    if (isAdd)
                    {
                        if (db.hrm_profile.Count(a => a.profile_id == model.profile_id) > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Đã tồn tại mã nhân sự này trong hệ thống!", err = "1" });
                        }
                    }
                    else
                    {
                        var old = await db.hrm_profile.AsNoTracking().FirstOrDefaultAsync(a => a.profile_id == uid);
                        if (old != null && old.profile_id != model.profile_id)
                        {
                            if (db.hrm_profile.Count(a => a.profile_id == model.profile_id) > 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Đã tồn tại mã nhân sự này trong hệ thống!", err = "1" });
                            }
                        }
                    }
                    var de_relative = provider.FormData.GetValues("relative").SingleOrDefault();
                    var de_skill = provider.FormData.GetValues("skill").SingleOrDefault();
                    var de_clan_history = provider.FormData.GetValues("clan_history").SingleOrDefault();
                    var se_experience = provider.FormData.GetValues("experience").SingleOrDefault();

                    List<hrm_profile_relative> relatives = JsonConvert.DeserializeObject<List<hrm_profile_relative>>(de_relative);
                    List<hrm_profile_skill> skills = JsonConvert.DeserializeObject<List<hrm_profile_skill>>(de_skill);
                    List<hrm_profile_clan_history> clan_historys = JsonConvert.DeserializeObject<List<hrm_profile_clan_history>>(de_clan_history);
                    List<hrm_profile_experience> experiences = JsonConvert.DeserializeObject<List<hrm_profile_experience>>(se_experience);
                    #region Model
                    if (isAdd)
                    {
                        model.profile_id = helper.GenKey();
                        model.profile_name = model.profile_user_name;
                        model.profile_name_en = helper.convertToUnSign3(model.profile_name);
                        model.profile_last_name = model.profile_name.Split(' ').Last();
                        model.is_order = model.is_order ?? (db.hrm_profile.Count() + 1);
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id ?? user_now.organization_parent_id;
                        db.hrm_profile.Add(model);
                    }
                    else
                    {
                        var check = await db.hrm_profile.CountAsync(x => x.profile_id != model.profile_id && x.profile_code == model.profile_code) > 0;
                        if (check)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Mã hồ sơ đã tồn tại!" });
                        }
                        model.modified_by = uid;
                        model.modified_date = DateTime.Now;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        db.Entry(model).State = EntityState.Modified;
                    }
                    #endregion
                    #region add relative
                    //var check = await db.hrm_profile_relative.CountAsync(x => (x.is_bout ?? 0) > 0) > 0;
                    //if (!check)
                    //{
                    //    var relatives_old = await db.hrm_profile_relative.Where(x => x.profile_id == model.profile_id && x.is_root == true).ToListAsync();
                    //    if (relatives_old.Count > 0)
                    //    {
                    //        db.hrm_profile_relative.RemoveRange(relatives_old);
                    //    }
                    //    if (relatives != null)
                    //    {
                    //        var count = 0;
                    //        foreach (var rl in relatives)
                    //        {
                    //            rl.profile_relative_id = helper.GenKey();
                    //            rl.profile_id = model.profile_id;
                    //            rl.is_order = count++;
                    //            rl.created_token_id = tid;
                    //            rl.created_date = DateTime.Now;
                    //            rl.created_by = uid;
                    //            rl.created_token_id = tid;
                    //            rl.created_ip = ip;
                    //            rl.is_root = true;
                    //            rl.is_bout = 0;
                    //            db.hrm_profile_relative.Add(rl);
                    //        }
                    //    }
                    //}

                    var relative_news = relatives.Where(p => string.IsNullOrWhiteSpace(p.profile_relative_id)).ToList();
                    var relative_olds = await db.hrm_profile_relative.Where(x => x.profile_id == model.profile_id).ToListAsync();
                    if (relative_olds.Count > 0)
                    {
                        var remove_ids = relatives.Where(x => !string.IsNullOrWhiteSpace(x.profile_relative_id)).Select(x => x.profile_relative_id).ToList();
                        var removes = relative_olds.Where(x => !remove_ids.Contains(x.profile_relative_id)).ToList();
                        if (removes.Count > 0)
                        {
                            db.hrm_profile_relative.RemoveRange(removes);
                        }
                        var updates = relative_olds.Where(x => remove_ids.Contains(x.profile_relative_id)).ToList();
                        if (updates.Count > 0)
                        {
                            foreach (var item in updates)
                            {
                                var update = relatives.FirstOrDefault(x => x.profile_relative_id == item.profile_relative_id);
                                if (update != null)
                                {
                                    item.is_type = update.is_type;
                                    item.relative_name = update.relative_name;
                                    item.relationship_id = update.relationship_id;
                                    item.birthday = update.birthday;
                                    item.phone = update.phone;
                                    item.tax_code = update.tax_code;
                                    item.identification_citizen = update.identification_citizen;
                                    item.identification_date_issue = update.identification_date_issue;
                                    item.identification_place_issue = update.identification_place_issue;
                                    item.is_dependent = update.is_dependent;
                                    item.start_date = update.start_date;
                                    item.end_date = update.end_date;
                                    item.info = update.info;
                                    item.note = update.note;
                                    item.is_company = update.is_company;
                                    item.is_die = update.is_die;
                                    //db.Entry(update).State = EntityState.Modified;
                                }
                            }
                        }
                    }
                    if (relative_news.Count > 0)
                    {
                        List<hrm_profile_relative> new_relatives = new List<hrm_profile_relative>();
                        int sttt = 0;
                        int is_bout = db.hrm_profile_relative.Max(x => x.is_bout) ?? 0;

                        foreach (var rl in relative_news)
                        {
                            hrm_profile_relative relative = new hrm_profile_relative();
                            relative = rl;
                            relative.profile_relative_id = helper.GenKey();
                            relative.profile_id = model.profile_id;
                            relative.is_order = sttt++;
                            relative.created_by = uid;
                            relative.created_date = DateTime.Now;
                            relative.created_ip = ip;
                            relative.created_token_id = tid;
                            relative.is_root = false;
                            relative.is_bout = is_bout + 1;
                            new_relatives.Add(relative);
                        }
                        if (new_relatives.Count > 0)
                        {
                            db.hrm_profile_relative.AddRange(new_relatives);
                        }
                    }
                    #endregion
                    #region add skill
                    var skill_old = await db.hrm_profile_skill.Where(x => x.profile_id == model.profile_id).ToListAsync();
                    if (skill_old.Count > 0)
                    {
                        db.hrm_profile_skill.RemoveRange(skill_old);
                    }
                    if (skills != null)
                    {
                        var count = 0;
                        foreach (var sk in skills)
                        {
                            sk.profile_skill_id = helper.GenKey();
                            sk.profile_id = model.profile_id;
                            sk.is_order = count++;
                            sk.created_token_id = tid;
                            sk.created_date = DateTime.Now;
                            sk.created_by = uid;
                            sk.created_token_id = tid;
                            sk.created_ip = ip;
                            db.hrm_profile_skill.Add(sk);
                        }
                    }
                    #endregion
                    #region add clan_history
                    if (clan_historys != null)
                    {
                        var clan_historys_old = await db.hrm_profile_clan_history.Where(x => x.profile_id == model.profile_id).ToListAsync();
                        if (clan_historys_old.Count > 0)
                        {
                            db.hrm_profile_clan_history.RemoveRange(clan_historys_old);
                        }
                        var count = 0;
                        foreach (var clan in clan_historys)
                        {
                            clan.profile_clan_history_id = helper.GenKey();
                            clan.profile_id = model.profile_id;
                            clan.is_order = count++;
                            clan.created_token_id = tid;
                            clan.created_date = DateTime.Now;
                            clan.created_by = uid;
                            clan.created_token_id = tid;
                            clan.created_ip = ip;
                            db.hrm_profile_clan_history.Add(clan);
                        }
                    }
                    #endregion
                    #region add experience
                    if (experiences != null)
                    {
                        var experiences_old = await db.hrm_profile_experience.Where(x => x.profile_id == model.profile_id).ToListAsync();
                        if (experiences_old.Count > 0)
                        {
                            db.hrm_profile_experience.RemoveRange(experiences_old);
                        }
                        var count = 0;
                        foreach (var ex in experiences)
                        {
                            ex.profile_experience_id = helper.GenKey();
                            ex.profile_id = model.profile_id;
                            ex.is_order = count++;
                            ex.created_token_id = tid;
                            ex.created_date = DateTime.Now;
                            ex.created_by = uid;
                            ex.created_token_id = tid;
                            ex.created_ip = ip;
                            db.hrm_profile_experience.Add(ex);
                        }
                    }
                    #endregion
                    #region file
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string path = root + "/" + model.organization_id + "/Hrm/Profile/" + model.profile_id;
                    bool exists = Directory.Exists(path);
                    if (!exists)
                        Directory.CreateDirectory(path);
                    List<hrm_file> dfs = new List<hrm_file>();
                    string avatar_old = null;
                    var modelold = await db.hrm_profile.FindAsync(model.profile_id);
                    if (modelold != null)
                    {
                        avatar_old = modelold.avatar;
                    }
                    int stt = 0;
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
                        string Duongdan = "/Portals/" + model.organization_id + "/Hrm/Profile/" + model.profile_id + "/" + name_file;
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

                        if (fileData.Headers.ContentDisposition.Name == "\"avatar\"")
                        {
                            model.avatar = Duongdan;
                            if (!string.IsNullOrWhiteSpace(avatar_old) && avatar_old.Contains("Portals"))
                            {
                                var strPath = root + "\\" + avatar_old;
                                bool ex = System.IO.Directory.Exists(strPath);
                                if (ex)
                                {
                                    System.IO.Directory.Delete(strPath, true);
                                }
                            }
                        }
                        else
                        {
                            var df = new hrm_file();
                            df.key_id = model.profile_id;
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
                            df.is_order = stt++;
                            df.created_by = uid;
                            df.created_date = DateTime.Now;
                            df.created_ip = ip;
                            df.created_token_id = tid;
                            df.organization_id = user_now.organization_id ?? user_now.organization_parent_id;
                            dfs.Add(df);
                        }
                    }
                    if (dfs.Count > 0)
                    {
                        db.hrm_file.AddRange(dfs);
                    }
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_profile_history()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                    var md = provider.FormData.GetValues("model").SingleOrDefault();
                    var de_relative = provider.FormData.GetValues("relative").SingleOrDefault();
                    var de_skill = provider.FormData.GetValues("skill").SingleOrDefault();
                    var de_clan_history = provider.FormData.GetValues("clan_history").SingleOrDefault();
                    var se_experience = provider.FormData.GetValues("experience").SingleOrDefault();

                    hrm_profile_history model = JsonConvert.DeserializeObject<hrm_profile_history>(md);
                    List<hrm_profile_relative> relatives = JsonConvert.DeserializeObject<List<hrm_profile_relative>>(de_relative);
                    List<hrm_profile_skill> skills = JsonConvert.DeserializeObject<List<hrm_profile_skill>>(de_skill);
                    List<hrm_profile_clan_history> clan_historys = JsonConvert.DeserializeObject<List<hrm_profile_clan_history>>(de_clan_history);
                    List<hrm_profile_experience> experiences = JsonConvert.DeserializeObject<List<hrm_profile_experience>>(se_experience);


                    var profile = await db.hrm_profile.AsNoTracking().FirstOrDefaultAsync(x=>x.profile_id == model.profile_id);
                    if (profile != null)
                    {
                        #region Model
                        model.profile_code = profile.profile_code;
                        model.recruitment_date = profile.recruitment_date;
                        model.profile_user_name = profile.profile_user_name;
                        model.profile_name = profile.profile_name;
                        model.profile_last_name = profile.profile_last_name;
                        model.superior_id = profile.superior_id;
                        model.check_in_id = profile.check_in_id;

                        var historys = await db.hrm_profile_history.AsNoTracking().Where(x => x.profile_id == model.profile_id).ToListAsync();
                        var historyRoot = historys.FirstOrDefault(x => x.is_root == true);
                        if (historyRoot == null)
                        {
                            hrm_profile_history history = new hrm_profile_history();
                            Type typeA = typeof(hrm_profile);
                            Type typeB = typeof(hrm_profile_history);

                            foreach (var propertyA in typeA.GetProperties())
                            {
                                var propertyB = typeB.GetProperty(propertyA.Name);
                                if (propertyB != null && propertyB.PropertyType == propertyA.PropertyType)
                                {
                                    propertyB.SetValue(history, propertyA.GetValue(profile));
                                }
                            }

                            history.history_id = helper.GenKey();
                            history.is_root = true;
                            db.hrm_profile_history.Add(history);

                            var root_relatives = await db.hrm_profile_relative.Where(x => x.profile_id == model.profile_id && x.history_id == null).ToListAsync();
                            if (root_relatives.Count > 0)
                            {
                                List<hrm_profile_relative> new_relatives = new List<hrm_profile_relative>();
                                foreach (var rl in root_relatives)
                                {
                                    hrm_profile_relative new_relative = new hrm_profile_relative();
                                    Type typeAA = typeof(hrm_profile_relative);
                                    Type typeBB = typeof(hrm_profile_relative);

                                    foreach (var propertyA in typeAA.GetProperties())
                                    {
                                        var propertyB = typeBB.GetProperty(propertyA.Name);
                                        if (propertyB != null && propertyB.PropertyType == propertyA.PropertyType)
                                        {
                                            propertyB.SetValue(new_relative, propertyA.GetValue(rl));
                                        }
                                    }
                                    new_relative.profile_relative_id = helper.GenKey();
                                    new_relative.history_id = history.history_id;
                                    new_relatives.Add(new_relative);
                                }
                                if (new_relatives.Count > 0)
                                {
                                    db.hrm_profile_relative.AddRange(new_relatives);
                                }
                            }
                            var root_skills = await db.hrm_profile_skill.Where(x => x.profile_id == model.profile_id && x.history_id == null).ToListAsync();
                            if (root_skills.Count > 0)
                            {
                                List<hrm_profile_skill> new_skills = new List<hrm_profile_skill>();
                                foreach (var rl in root_skills)
                                {
                                    hrm_profile_skill new_skill = new hrm_profile_skill();
                                    Type typeAA = typeof(hrm_profile_skill);
                                    Type typeBB = typeof(hrm_profile_skill);

                                    foreach (var propertyA in typeAA.GetProperties())
                                    {
                                        var propertyB = typeBB.GetProperty(propertyA.Name);
                                        if (propertyB != null && propertyB.PropertyType == propertyA.PropertyType)
                                        {
                                            propertyB.SetValue(new_skill, propertyA.GetValue(rl));
                                        }
                                    }
                                    new_skill.profile_skill_id = helper.GenKey();
                                    new_skill.history_id = history.history_id;
                                    new_skills.Add(new_skill);
                                }
                                if (new_skills.Count > 0)
                                {
                                    db.hrm_profile_skill.AddRange(new_skills);
                                }
                            }
                            var root_clan_historys = await db.hrm_profile_clan_history.Where(x => x.profile_id == model.profile_id && x.history_id == null).ToListAsync();
                            if (root_clan_historys.Count > 0)
                            {
                                List<hrm_profile_clan_history> new_clan_historys = new List<hrm_profile_clan_history>();
                                foreach (var rl in root_clan_historys)
                                {
                                    hrm_profile_clan_history new_clan_history = new hrm_profile_clan_history();
                                    Type typeAA = typeof(hrm_profile_clan_history);
                                    Type typeBB = typeof(hrm_profile_clan_history);

                                    foreach (var propertyA in typeAA.GetProperties())
                                    {
                                        var propertyB = typeBB.GetProperty(propertyA.Name);
                                        if (propertyB != null && propertyB.PropertyType == propertyA.PropertyType)
                                        {
                                            propertyB.SetValue(new_clan_history, propertyA.GetValue(rl));
                                        }
                                    }
                                    new_clan_history.profile_clan_history_id = helper.GenKey();
                                    new_clan_history.history_id = history.history_id;
                                    new_clan_historys.Add(new_clan_history);
                                }
                                if (new_clan_historys.Count > 0)
                                {
                                    db.hrm_profile_clan_history.AddRange(new_clan_historys);
                                }
                            }
                            var root_experiences = await db.hrm_profile_experience.Where(x => x.profile_id == model.profile_id && x.history_id == null).ToListAsync();
                            if (root_experiences.Count > 0)
                            {
                                List<hrm_profile_experience> new_experiences = new List<hrm_profile_experience>();
                                foreach (var rl in root_experiences)
                                {
                                    hrm_profile_experience new_experience = new hrm_profile_experience();
                                    Type typeAA = typeof(hrm_profile_experience);
                                    Type typeBB = typeof(hrm_profile_experience);

                                    foreach (var propertyA in typeAA.GetProperties())
                                    {
                                        var propertyB = typeBB.GetProperty(propertyA.Name);
                                        if (propertyB != null && propertyB.PropertyType == propertyA.PropertyType)
                                        {
                                            propertyB.SetValue(new_experience, propertyA.GetValue(rl));
                                        }
                                    }
                                    new_experience.profile_experience_id = helper.GenKey();
                                    new_experience.history_id = history.history_id;
                                    new_experiences.Add(new_experience);
                                }
                                if (new_experiences.Count > 0)
                                {
                                    db.hrm_profile_experience.AddRange(new_experiences);
                                }
                            }
                        }
                        var approve = historys.FirstOrDefault(x => x.is_approve == 1);
                        if (approve != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn đang có phiếu chờ xét duyệt, không thể lưu!" });
                        }
                        var upgrade = historys.FirstOrDefault(x => x.is_approve == 0);
                        if (upgrade != null)
                        {
                            model.history_id = upgrade.history_id;
                            model.is_root = false;
                            model.is_approve = 0;
                            model.approve_profile_id = null;
                            model.approve_date = null;

                            model.created_date = DateTime.Now;
                            model.modified_by = uid;
                            model.modified_date = DateTime.Now;
                            model.modified_ip = ip;
                            model.modified_token_id = tid;
                            db.Entry(model).State = EntityState.Modified;
                        }
                        else
                        {
                            model.history_id = helper.GenKey();
                            model.is_order = db.hrm_profile_history.Count(x=>x.profile_id == model.profile_id) + 1;
                            model.created_by = uid;
                            model.created_date = DateTime.Now;
                            model.created_ip = ip;
                            model.created_token_id = tid;
                            model.organization_id = profile.organization_id;

                            model.is_approve = 0;
                            model.is_root = false;
                            db.hrm_profile_history.Add(model);
                        }
                        #endregion
                        #region add relative
                        var relative_old = await db.hrm_profile_relative.Where(x => x.history_id == model.history_id).ToListAsync();
                        if (relative_old.Count > 0)
                        {
                            db.hrm_profile_relative.RemoveRange(relative_old);
                        }
                        if (relatives != null)
                        {
                            List<hrm_profile_relative> new_relatives = new List<hrm_profile_relative>();
                            var count = 0;
                            foreach (var rl in relatives)
                            {
                                rl.profile_relative_id = helper.GenKey();
                                rl.profile_id = model.profile_id;
                                rl.history_id = model.history_id;
                                rl.is_order = count++;
                                rl.created_by = uid;
                                rl.created_date = DateTime.Now;
                                rl.created_ip = ip;
                                rl.created_token_id = tid;
                                new_relatives.Add(rl);
                            }
                            if (new_relatives.Count > 0)
                            {
                                db.hrm_profile_relative.AddRange(new_relatives);
                            }
                        }
                        #endregion
                        #region add skill
                        var skill_old = await db.hrm_profile_skill.Where(x => x.history_id == model.history_id).ToListAsync();
                        if (skill_old.Count > 0)
                        {
                            db.hrm_profile_skill.RemoveRange(skill_old);
                        }
                        if (skills != null)
                        {
                            List<hrm_profile_skill> new_skills = new List<hrm_profile_skill>();
                            var count = 0;
                            foreach (var sk in skills)
                            {
                                sk.profile_skill_id = helper.GenKey();
                                sk.profile_id = model.profile_id;
                                sk.history_id = model.history_id;
                                sk.is_order = count++;
                                sk.created_by = uid;
                                sk.created_date = DateTime.Now;
                                sk.created_ip = ip;
                                sk.created_token_id = tid;
                                new_skills.Add(sk);
                            }
                            if (new_skills.Count > 0)
                            {
                                db.hrm_profile_skill.AddRange(new_skills);
                            }
                        }
                        #endregion
                        #region add clan_history
                        var clan_historys_old = await db.hrm_profile_clan_history.Where(x => x.history_id == model.history_id).ToListAsync();
                        if (clan_historys_old.Count > 0)
                        {
                            db.hrm_profile_clan_history.RemoveRange(clan_historys_old);
                        }
                        if (clan_historys != null)
                        {
                            List<hrm_profile_clan_history> new_clan_historys = new List<hrm_profile_clan_history>();
                            var count = 0;
                            foreach (var clan in clan_historys)
                            {
                                clan.profile_clan_history_id = helper.GenKey();
                                clan.profile_id = model.profile_id;
                                clan.history_id = model.history_id;
                                clan.is_order = count++;
                                clan.created_by = uid;
                                clan.created_date = DateTime.Now;
                                clan.created_ip = ip;
                                clan.created_token_id = tid;
                                new_clan_historys.Add(clan);
                            }
                            if (new_clan_historys.Count > 0)
                            {
                                db.hrm_profile_clan_history.AddRange(new_clan_historys);
                            }
                        }
                        #endregion
                        #region add experience
                        var experiences_old = await db.hrm_profile_experience.Where(x => x.history_id == model.history_id).ToListAsync();
                        if (experiences_old.Count > 0)
                        {
                            db.hrm_profile_experience.RemoveRange(experiences_old);
                        }
                        if (experiences != null)
                        {
                            List<hrm_profile_experience> new_experiences = new List<hrm_profile_experience>();
                            var count = 0;
                            foreach (var ex in experiences)
                            {
                                ex.profile_experience_id = helper.GenKey();
                                ex.profile_id = model.profile_id;
                                ex.history_id = model.history_id;
                                ex.is_order = count++;
                                ex.created_by = uid;
                                ex.created_date = DateTime.Now;
                                ex.created_ip = ip;
                                ex.created_token_id = tid;
                                new_experiences.Add(ex);
                            }
                            if (new_experiences.Count > 0)
                            {
                                db.hrm_profile_experience.AddRange(new_experiences);
                            }
                        }
                        #endregion
                        #region file
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        string path = root + "/" + model.organization_id + "/Hrm/Profile/" + model.history_id;
                        bool exists = Directory.Exists(path);
                        if (!exists)
                            Directory.CreateDirectory(path);
                        List<hrm_file> dfs = new List<hrm_file>();
                        string avatar_old = null;
                        var modelold = await db.hrm_profile_history.FindAsync(model.history_id);
                        if (modelold != null)
                        {
                            avatar_old = modelold.avatar;
                        }
                        int stt = 0;
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
                            string Duongdan = "/Portals/" + model.organization_id + "/Hrm/Profile/" + model.history_id + "/" + name_file;
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

                            if (fileData.Headers.ContentDisposition.Name == "\"avatar\"")
                            {
                                model.avatar = Duongdan;
                                if (!string.IsNullOrWhiteSpace(avatar_old) && avatar_old.Contains("Portals"))
                                {
                                    var strPath = root + "\\" + avatar_old;
                                    bool ex = System.IO.Directory.Exists(strPath);
                                    if (ex)
                                    {
                                        System.IO.Directory.Delete(strPath, true);
                                    }
                                }
                            }
                            else
                            {
                                var df = new hrm_file();
                                df.key_id = model.history_id;
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
                                df.is_order = stt++;
                                df.created_by = uid;
                                df.created_date = DateTime.Now;
                                df.created_ip = ip;
                                df.created_token_id = tid;
                                df.organization_id = profile.organization_id;
                                dfs.Add(df);
                            }
                        }
                        if (dfs.Count > 0)
                        {
                            db.hrm_file.AddRange(dfs);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_history", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_history", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> send_profile_history()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                    string ids = provider.FormData.GetValues("ids").SingleOrDefault();
                    int? is_approve = int.Parse(provider.FormData.GetValues("is_approve")?.SingleOrDefault() ?? "1");
                    string content = provider.FormData.GetValues("content").SingleOrDefault();
                    DateTime approve_date = DateTime.Parse(provider.FormData.GetValues("approve_date")?.SingleOrDefault());

                    List<string> historys = JsonConvert.DeserializeObject<List<string>>(ids);
                    if (historys != null && historys.Count > 0)
                    {
                        foreach (var id in historys)
                        {
                            var history = await db.hrm_profile_history.FindAsync(id);
                            if (history != null)
                            {
                                history.is_approve = is_approve;
                                if (is_approve == 2 || is_approve == 3)
                                {
                                    history.approve_profile_id = db.hrm_matchaccount.FirstOrDefault(x=>x.user_id == uid)?.profile_id ?? null;
                                    history.approve_date = approve_date;
                                }
                                if (is_approve == 2)
                                {
                                    hrm_profile profile = await db.hrm_profile.FindAsync(history.profile_id);
                                    if (profile != null)
                                    {
                                        DateTime? created_date = profile.created_date;
                                        int? is_order = profile.is_order;
                                        Type typeA = typeof(hrm_profile_history);
                                        Type typeB = typeof(hrm_profile);

                                        foreach (var propertyA in typeA.GetProperties())
                                        {
                                            var propertyB = typeB.GetProperty(propertyA.Name);
                                            if (propertyB != null && propertyB.PropertyType == propertyA.PropertyType)
                                            {
                                                propertyB.SetValue(profile, propertyA.GetValue(history));
                                            }
                                        }
                                        profile.created_date = created_date;
                                        profile.is_order = is_order;
                                        db.Entry(profile).State = EntityState.Modified;

                                        var old_relatives = await db.hrm_profile_relative.Where(x => x.profile_id == history.profile_id && x.history_id == null).ToListAsync();
                                        if (old_relatives.Count > 0)
                                        {
                                            db.hrm_profile_relative.RemoveRange(old_relatives);
                                        }
                                        var root_relatives = await db.hrm_profile_relative.Where(x => x.history_id == history.history_id).ToListAsync();
                                        if (root_relatives.Count > 0)
                                        {
                                            List<hrm_profile_relative> new_relatives = new List<hrm_profile_relative>();
                                            foreach (var rl in root_relatives)
                                            {
                                                hrm_profile_relative new_relative = new hrm_profile_relative();
                                                Type typeAA = typeof(hrm_profile_relative);
                                                Type typeBB = typeof(hrm_profile_relative);

                                                foreach (var propertyA in typeAA.GetProperties())
                                                {
                                                    var propertyB = typeBB.GetProperty(propertyA.Name);
                                                    if (propertyB != null && propertyB.PropertyType == propertyA.PropertyType)
                                                    {
                                                        propertyB.SetValue(new_relative, propertyA.GetValue(rl));
                                                    }
                                                }
                                                new_relative.profile_relative_id = helper.GenKey();
                                                new_relative.profile_id = history.profile_id;
                                                new_relative.history_id = null;
                                                new_relatives.Add(new_relative);
                                            }
                                            if (new_relatives.Count > 0)
                                            {
                                                db.hrm_profile_relative.AddRange(new_relatives);
                                            }
                                        }
                                        var old_skills = await db.hrm_profile_skill.Where(x => x.profile_id == history.profile_id && x.history_id == null).ToListAsync();
                                        if (old_skills.Count > 0)
                                        {
                                            db.hrm_profile_skill.RemoveRange(old_skills);
                                        }
                                        var root_skills = await db.hrm_profile_skill.Where(x => x.history_id == history.history_id).ToListAsync();
                                        if (root_skills.Count > 0)
                                        {
                                            List<hrm_profile_skill> new_skills = new List<hrm_profile_skill>();
                                            foreach (var rl in root_skills)
                                            {
                                                hrm_profile_skill new_skill = new hrm_profile_skill();
                                                Type typeAA = typeof(hrm_profile_skill);
                                                Type typeBB = typeof(hrm_profile_skill);

                                                foreach (var propertyA in typeAA.GetProperties())
                                                {
                                                    var propertyB = typeBB.GetProperty(propertyA.Name);
                                                    if (propertyB != null && propertyB.PropertyType == propertyA.PropertyType)
                                                    {
                                                        propertyB.SetValue(new_skill, propertyA.GetValue(rl));
                                                    }
                                                }
                                                new_skill.profile_skill_id = helper.GenKey();
                                                new_skill.profile_id = history.profile_id;
                                                new_skill.history_id = null;
                                                new_skills.Add(new_skill);
                                            }
                                            if (new_skills.Count > 0)
                                            {
                                                db.hrm_profile_skill.AddRange(new_skills);
                                            }
                                        }
                                        var old_clan_historys = await db.hrm_profile_clan_history.Where(x => x.profile_id == history.profile_id && x.history_id == null).ToListAsync();
                                        if (old_clan_historys.Count > 0)
                                        {
                                            db.hrm_profile_clan_history.RemoveRange(old_clan_historys);
                                        }
                                        var root_clan_historys = await db.hrm_profile_clan_history.Where(x => x.history_id == history.history_id).ToListAsync();
                                        if (root_clan_historys.Count > 0)
                                        {
                                            List<hrm_profile_clan_history> new_clan_historys = new List<hrm_profile_clan_history>();
                                            foreach (var rl in root_clan_historys)
                                            {
                                                hrm_profile_clan_history new_clan_history = new hrm_profile_clan_history();
                                                Type typeAA = typeof(hrm_profile_clan_history);
                                                Type typeBB = typeof(hrm_profile_clan_history);

                                                foreach (var propertyA in typeAA.GetProperties())
                                                {
                                                    var propertyB = typeBB.GetProperty(propertyA.Name);
                                                    if (propertyB != null && propertyB.PropertyType == propertyA.PropertyType)
                                                    {
                                                        propertyB.SetValue(new_clan_history, propertyA.GetValue(rl));
                                                    }
                                                }
                                                new_clan_history.profile_clan_history_id = helper.GenKey();
                                                new_clan_history.profile_id = history.profile_id;
                                                new_clan_history.history_id = null;
                                                new_clan_historys.Add(new_clan_history);
                                            }
                                            if (new_clan_historys.Count > 0)
                                            {
                                                db.hrm_profile_clan_history.AddRange(new_clan_historys);
                                            }
                                        }
                                        var old_experiences = await db.hrm_profile_experience.Where(x => x.profile_id == history.profile_id && x.history_id == null).ToListAsync();
                                        if (old_experiences.Count > 0)
                                        {
                                            db.hrm_profile_experience.RemoveRange(old_experiences);
                                        }
                                        var root_experiences = await db.hrm_profile_experience.Where(x => x.history_id == history.history_id).ToListAsync();
                                        if (root_experiences.Count > 0)
                                        {
                                            List<hrm_profile_experience> new_experiences = new List<hrm_profile_experience>();
                                            foreach (var rl in root_experiences)
                                            {
                                                hrm_profile_experience new_experience = new hrm_profile_experience();
                                                Type typeAA = typeof(hrm_profile_experience);
                                                Type typeBB = typeof(hrm_profile_experience);

                                                foreach (var propertyA in typeAA.GetProperties())
                                                {
                                                    var propertyB = typeBB.GetProperty(propertyA.Name);
                                                    if (propertyB != null && propertyB.PropertyType == propertyA.PropertyType)
                                                    {
                                                        propertyB.SetValue(new_experience, propertyA.GetValue(rl));
                                                    }
                                                }
                                                new_experience.profile_experience_id = helper.GenKey();
                                                new_experience.profile_id = history.profile_id;
                                                new_experience.history_id = null;
                                                new_experiences.Add(new_experience);
                                            }
                                            if (new_experiences.Count > 0)
                                            {
                                                db.hrm_profile_experience.AddRange(new_experiences);
                                            }
                                        }
                                    }
                                    
                                }
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/send_profile_history", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/send_profile_history", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_star_profile()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    var is_star = bool.Parse(provider.FormData.GetValues("is_star").SingleOrDefault());
                    var ids = provider.FormData.GetValues("ids").SingleOrDefault();
                    List<string> profiles = JsonConvert.DeserializeObject<List<string>>(ids);
                    if (profiles.Count > 0)
                    {
                        foreach (var profile_id in profiles)
                        {
                            var profile = db.hrm_profile.Find(profile_id);
                            if (profile != null)
                            {
                                profile.is_star = is_star;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_star_contract", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_star_contract", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_profile([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        var das = await db.hrm_profile.Where(a => ids.Contains(a.profile_id)).ToListAsync();
                        var dasUrl = await db.hrm_file.AsNoTracking().Where(a => ids.Contains(a.key_id) && (ad || a.created_by == uid) && a.file_path != null).Select(a => a.file_path).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<hrm_profile> del = new List<hrm_profile>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                                if (!string.IsNullOrWhiteSpace(da.avatar) && da.avatar.Contains("Portals"))
                                {
                                    paths.Add(da.avatar);
                                }
                            }
                            foreach (var p in dasUrl)
                            {
                                paths.Add(p);
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_profile.RemoveRange(del);
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "hrm_profile/delete_profile", ip, tid, "Lỗi khi xóa", 0, "hrm_profile");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "hrm_profile/delete_profile", ip, tid, "Lỗi khi xoá", 0, "hrm_profile");
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

        [HttpPut]
        public async Task<HttpResponseMessage> update_profile_receipt()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                    string profile_id = provider.FormData.GetValues("profile_id").SingleOrDefault();
                    int? receipt_status = int.Parse(provider.FormData.GetValues("receipt_status").SingleOrDefault());
                    var re = provider.FormData.GetValues("receipts").SingleOrDefault();
                    List<hrm_profile_receipt> receipts = JsonConvert.DeserializeObject<List<hrm_profile_receipt>>(re);
                    var profile = db.hrm_profile.Find(profile_id);
                    profile.receipt_status = receipt_status ?? 0;
                    var receipt_old = await db.hrm_profile_receipt.Where(x => x.profile_id == profile_id).ToListAsync();
                    if (receipt_old.Count > 0)
                    {
                        var exists = receipts.Select(b => b.receipt_id);
                        var notexists = receipt_old.Where(a => !exists.Contains(a.receipt_id)).ToList();
                        if (notexists.Count > 0)
                        {
                            db.hrm_profile_receipt.RemoveRange(notexists);
                        }
                    }
                    if (receipts != null)
                    {
                        List<hrm_profile_receipt> new_receipts = new List<hrm_profile_receipt>();
                        var stt = 0;
                        foreach (var receipt in receipts)
                        {
                            if (string.IsNullOrEmpty(receipt.receipt_profile_id.ToString()) || receipt.receipt_profile_id == -1)
                            {
                                receipt.profile_id = profile_id;
                                receipt.is_order = stt++;
                                receipt.created_by = uid;
                                receipt.created_date = DateTime.Now;
                                receipt.created_ip = ip;
                                receipt.created_token_id = tid;
                                new_receipts.Add(receipt);
                            }
                            else
                            {
                                var model = await db.hrm_profile_receipt.FindAsync(receipt.receipt_profile_id);
                                model.receipt_date = receipt.receipt_date;
                                model.note = receipt.note;
                                model.modified_by = uid;
                                model.modified_date = DateTime.Now;
                                model.modified_ip = ip;
                                model.modified_token_id = tid;
                            }
                        }
                        if (new_receipts.Count > 0)
                        {
                            db.hrm_profile_receipt.AddRange(new_receipts);
                        }
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_receipt", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_receipt", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_profile_tags()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                    string profile_id = provider.FormData.GetValues("profile_id").SingleOrDefault();
                    var re = provider.FormData.GetValues("tags").SingleOrDefault();
                    List<int> tags = JsonConvert.DeserializeObject<List<int>>(re);
                    //var profile = db.hrm_profile.Find(profile_id);
                    var receipt_old = await db.hrm_profile_tags.Where(x => x.profile_id == profile_id).ToListAsync();
                    if (receipt_old.Count > 0)
                    {
                        db.hrm_profile_tags.RemoveRange(receipt_old);
                    }
                    if (tags != null)
                    {
                        List<hrm_profile_tags> new_receipts = new List<hrm_profile_tags>();

                        var stt = 0;
                        foreach (var tags_id in tags)
                        {
                            hrm_profile_tags receipt = new hrm_profile_tags();
                            receipt.tags_id = tags_id;
                            receipt.profile_id = profile_id;
                            receipt.is_order = stt++;
                            receipt.created_by = uid;
                            receipt.created_date = DateTime.Now;
                            receipt.created_ip = ip;
                            receipt.created_token_id = tid;
                            new_receipts.Add(receipt);
                        }
                        if (new_receipts.Count > 0)
                        {
                            db.hrm_profile_tags.AddRange(new_receipts);
                        }
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_tags", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_tags", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_profile_health()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                    string profile_id = provider.FormData.GetValues("profile_id").SingleOrDefault();
                    var he = provider.FormData.GetValues("health").SingleOrDefault();
                    var vac = provider.FormData.GetValues("vaccines").SingleOrDefault();

                    hrm_profile_health health = JsonConvert.DeserializeObject<hrm_profile_health>(he);
                    List<hrm_health_vaccine> vaccines = JsonConvert.DeserializeObject<List<hrm_health_vaccine>>(vac);

                    if (string.IsNullOrEmpty(health.health_id) || health.health_id == "-1")
                    {
                        health.health_id = helper.GenKey();
                        health.profile_id = profile_id;
                        health.created_by = uid;
                        health.created_date = DateTime.Now;
                        health.created_ip = ip;
                        health.created_token_id = tid;
                        db.hrm_profile_health.Add(health);
                    }
                    else
                    {
                        db.Entry(health).State = EntityState.Modified;
                    }

                    var vaccine_old = await db.hrm_health_vaccine.Where(x => x.profile_id == profile_id).ToListAsync();
                    if (vaccine_old.Count > 0)
                    {
                        var exists = vaccines.Select(b => b.vaccine_id);
                        var notexists = vaccine_old.Where(a => !exists.Contains(a.vaccine_id)).ToList();
                        if (notexists.Count > 0)
                        {
                            db.hrm_health_vaccine.RemoveRange(notexists);
                        }
                    }
                    if (vaccines != null)
                    {
                        List<hrm_health_vaccine> new_vaccines = new List<hrm_health_vaccine>();
                        var stt = 0;
                        foreach (var vaccine in vaccines)
                        {
                            if (string.IsNullOrEmpty(vaccine.vaccine_id.ToString()) || vaccine.vaccine_id == -1)
                            {
                                vaccine.health_id = health.health_id;
                                vaccine.profile_id = profile_id;
                                vaccine.is_order = stt++;
                                new_vaccines.Add(vaccine);
                            }
                            else
                            {
                                var model = await db.hrm_health_vaccine.FindAsync(vaccine.vaccine_id);
                                model.injection_id = vaccine.injection_id;
                                model.injection_date = vaccine.injection_date;
                                model.type_vaccine = vaccine.type_vaccine;
                                model.lot_number = vaccine.lot_number;
                                model.vaccination_facility = vaccine.vaccination_facility;
                                model.sign_user = vaccine.sign_user;
                                model.sign_user_position = vaccine.sign_user_position;
                            }
                        }
                        if (new_vaccines.Count > 0)
                        {
                            db.hrm_health_vaccine.AddRange(new_vaccines);
                        }
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_health", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_health", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_profile_relative_late()
        {
            var identity = User.Identity as ClaimsIdentity;
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

                    string profile_id = provider.FormData.GetValues("profile_id").SingleOrDefault();
                    var rel = provider.FormData.GetValues("relative").SingleOrDefault();

                    List<hrm_profile_relative> relatives = JsonConvert.DeserializeObject<List<hrm_profile_relative>>(rel);
                    List<hrm_profile_relative> new_relatives = new List<hrm_profile_relative>();

                    var relative_news = relatives.Where(p => string.IsNullOrWhiteSpace(p.profile_relative_id)).ToList();
                    var relative_olds = await db.hrm_profile_relative.Where(x => x.profile_id == profile_id).ToListAsync();
                    if (relative_olds.Count > 0)
                    {
                        var remove_ids = relatives.Where(x => !string.IsNullOrWhiteSpace(x.profile_relative_id)).Select(x => x.profile_relative_id).ToList();
                        var removes = relative_olds.Where(x => !remove_ids.Contains(x.profile_relative_id)).ToList();
                        if (removes.Count > 0)
                        {
                            db.hrm_profile_relative.RemoveRange(removes);
                        }
                        var updates = relative_olds.Where(x => remove_ids.Contains(x.profile_relative_id)).ToList();
                        if (updates.Count > 0)
                        {
                            foreach (var item in updates)
                            {
                                var update = relatives.FirstOrDefault(x => x.profile_relative_id == item.profile_relative_id);
                                if (update != null)
                                {
                                    item.is_type = update.is_type;
                                    item.relative_name = update.relative_name;
                                    item.relationship_id = update.relationship_id;
                                    item.birthday = update.birthday;
                                    item.phone = update.phone;
                                    item.tax_code = update.tax_code;
                                    item.identification_citizen = update.identification_citizen;
                                    item.identification_date_issue = update.identification_date_issue;
                                    item.identification_place_issue = update.identification_place_issue;
                                    item.is_dependent = update.is_dependent;
                                    item.start_date = update.start_date;
                                    item.end_date = update.end_date;
                                    item.info = update.info;
                                    item.note = update.note;
                                    item.is_company = update.is_company;
                                    item.is_die = update.is_die;
                                    //db.Entry(update).State = EntityState.Modified;
                                }
                            }
                        }
                    }
                    int stt = 0;
                    int is_bout = db.hrm_profile_relative.Max(x => x.is_bout) ?? 0;

                    foreach (var rl in relative_news)
                    {
                        hrm_profile_relative relative = new hrm_profile_relative();
                        relative = rl;
                        relative.profile_relative_id = helper.GenKey();
                        relative.profile_id = profile_id;
                        relative.is_order = stt++;
                        relative.created_by = uid;
                        relative.created_date = DateTime.Now;
                        relative.created_ip = ip;
                        relative.created_token_id = tid;
                        relative.is_root = false;
                        relative.is_bout = is_bout + 1;
                        new_relatives.Add(relative);
                    }
                    if (new_relatives.Count > 0)
                    {
                        db.hrm_profile_relative.AddRange(new_relatives);
                    }

                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_profile_edit()
        {
            var identity = User.Identity as ClaimsIdentity;
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

                    bool isEdit = bool.Parse(provider.FormData.GetValues("isEdit").SingleOrDefault());
                    var md = provider.FormData.GetValues("model").SingleOrDefault();

                    hrm_profile_edit model = JsonConvert.DeserializeObject<hrm_profile_edit>(md);
                    if (isEdit)
                    {
                        db.Entry(model).State = EntityState.Modified;
                    }
                    else
                    {
                        model.is_flag = db.hrm_profile_edit.Count(x => x.profile_id == model.profile_id) + 1;
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        db.hrm_profile_edit.Add(model);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_edit", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_edit", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_profile_edit([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> ids)
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
                        var das = await db.hrm_profile_edit.Where(a => ids.Contains(a.key_id) && (ad || a.created_by == uid)).ToListAsync();
                        if (das != null)
                        {
                            List<hrm_profile_edit> del = new List<hrm_profile_edit>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            if (del != null && del.Count > 0)
                            {
                                db.hrm_profile_edit.RemoveRange(del);
                            }
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "hrm_profile/delete_profile_edit", ip, tid, "Lỗi khi cập nhật phòng họp họp", 0, "hrm_profile");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "hrm_profile/delete_profile_edit", ip, tid, "Lỗi khi xoá phòng họp", 0, "hrm_profile");
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
        public async Task<HttpResponseMessage> update_profile_relate()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    var profile_id = provider.FormData.GetValues("profile_id").SingleOrDefault();
                    var rl = provider.FormData.GetValues("relate").SingleOrDefault();

                    hrm_profile_relate relate = JsonConvert.DeserializeObject<hrm_profile_relate>(rl);

                    var relate_old = await db.hrm_profile_relate.Where(x => x.profile_id == profile_id).ToListAsync();
                    if (relate_old.Count > 0)
                    {
                        db.hrm_profile_relate.RemoveRange(relate_old);
                    }
                    var check = await db.hrm_profile_relate.CountAsync(x => x.relate_id == profile_id) > 0;
                    if (!check)
                    {
                        relate.profile_id = profile_id;
                        relate.created_by = uid;
                        relate.created_date = DateTime.Now;
                        relate.created_ip = ip;
                        relate.created_token_id = tid;
                        db.hrm_profile_relate.Add(relate);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_replate", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_replate", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_profile_insurance()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    bool isAdd = bool.Parse(provider.FormData.GetValues("isAdd").SingleOrDefault());
                    var de_insurance = provider.FormData.GetValues("insurance").SingleOrDefault();
                    var de_insurance_pay = provider.FormData.GetValues("insurance_pay").SingleOrDefault();
                    var de_insurance_resolve = provider.FormData.GetValues("insurance_resolve").SingleOrDefault();

                    hrm_insurance insurance = JsonConvert.DeserializeObject<hrm_insurance>(de_insurance);
                    List<hrm_insurance_pay> insurance_pays = JsonConvert.DeserializeObject<List<hrm_insurance_pay>>(de_insurance_pay);
                    List<hrm_insurance_resolve> insurance_resolves = JsonConvert.DeserializeObject<List<hrm_insurance_resolve>>(de_insurance_resolve);

                    if (de_insurance != null)
                    {
                        if (isAdd)
                        {
                            insurance.is_order = db.hrm_insurance.Count() + 1;
                            insurance.status = 0;
                            insurance.created_token_id = tid;
                            insurance.created_date = DateTime.Now;
                            insurance.created_by = uid;
                            insurance.created_token_id = tid;
                            insurance.created_ip = ip;
                            db.hrm_insurance.Add(insurance);
                        }
                        else
                        {
                            insurance.modified_date = DateTime.Now;
                            insurance.modified_by = uid;
                            insurance.modified_ip = ip;
                            insurance.modified_token_id = tid;
                            db.Entry(insurance).State = EntityState.Modified;
                        }

                        if (insurance_pays != null)
                        {
                            var insurance_pays_old = await db.hrm_insurance_pay.Where(x => x.insurance_id == insurance.insurance_id).ToListAsync();
                            if (insurance_pays_old.Count > 0)
                            {
                                db.hrm_insurance_pay.RemoveRange(insurance_pays_old);
                            }

                            List<hrm_insurance_pay> pays = new List<hrm_insurance_pay>();
                            int stt = 0;
                            foreach (var item in insurance_pays)
                            {
                                hrm_insurance_pay pay = new hrm_insurance_pay();
                                pay = item;
                                pay.insurance_pay_id = helper.GenKey();
                                pay.insurance_id = insurance.insurance_id;
                                pay.profile_id = insurance.profile_id;
                                pay.is_order = stt++;
                                pay.created_token_id = tid;
                                pay.created_date = DateTime.Now;
                                pay.created_by = uid;
                                pay.created_token_id = tid;
                                pay.created_ip = ip;
                                pays.Add(pay);
                            }
                            if (pays.Count > 0)
                            {
                                db.hrm_insurance_pay.AddRange(pays);
                            }
                        }
                        if (insurance_pays != null)
                        {
                            var insurance_pays_old = await db.hrm_insurance_resolve.Where(x => x.insurance_id == insurance.insurance_id).ToListAsync();
                            if (insurance_pays_old.Count > 0)
                            {
                                db.hrm_insurance_resolve.RemoveRange(insurance_pays_old);
                            }

                            List<hrm_insurance_resolve> resolves = new List<hrm_insurance_resolve>();
                            int stt = 0;
                            foreach (var item in insurance_resolves)
                            {
                                hrm_insurance_resolve resolve = new hrm_insurance_resolve();
                                resolve = item;
                                resolve.insurance_resolve_id = helper.GenKey();
                                resolve.insurance_id = insurance.insurance_id;
                                resolve.profile_id = insurance.profile_id;
                                resolve.is_order = stt++;
                                resolve.created_token_id = tid;
                                resolve.created_date = DateTime.Now;
                                resolve.created_by = uid;
                                resolve.created_token_id = tid;
                                resolve.created_ip = ip;
                                resolves.Add(resolve);
                            }
                            if (resolves.Count > 0)
                            {
                                db.hrm_insurance_resolve.AddRange(resolves);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_replate", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_replate", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        class obj
        {
            public int status { get; set; }
            public DateTime? start_date { get; set; }
            public DateTime? end_date { get; set; }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> update_profile_status()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    string profile_id = provider.FormData.GetValues("profile_id").SingleOrDefault();
                    string str_object = provider.FormData.GetValues("object").SingleOrDefault();
                    string srt_datas = provider.FormData.GetValues("datas").SingleOrDefault();
                    obj de_object = JsonConvert.DeserializeObject<obj>(str_object);
                    List<hrm_profile_status> de_datas = JsonConvert.DeserializeObject<List<hrm_profile_status>>(srt_datas);
                    List<hrm_profile_status> new_datas = new List<hrm_profile_status>();
                    if (de_object != null)
                    {
                        hrm_profile_status status = new hrm_profile_status();
                        status.profile_id = profile_id;
                        status.status = de_object.status;
                        status.start_date = de_object.start_date;
                        status.end_date = de_object.end_date;
                        status.created_by = uid;
                        status.created_date = DateTime.Now;
                        status.created_ip = ip;
                        status.created_token_id = tid;
                        new_datas.Add(status);

                        var profile = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_id == profile_id);
                        if (profile != null)
                        {
                            profile.status = de_object.status;
                        }
                    }
                    var datas_old = await db.hrm_profile_status.Where(x => x.profile_id == profile_id).ToListAsync();
                    if (datas_old.Count > 0)
                    {
                        db.hrm_profile_status.RemoveRange(datas_old);
                    }
                    if (de_datas != null)
                    {
                        foreach (var item in de_datas)
                        {
                            hrm_profile_status status = new hrm_profile_status();
                            status.profile_id = profile_id;
                            status.status = item.status;
                            status.start_date = item.start_date;
                            status.end_date = item.end_date;
                            status.created_by = uid;
                            status.created_date = DateTime.Now;
                            status.created_ip = ip;
                            status.created_token_id = tid;
                            new_datas.Add(status);
                        }
                    }
                    if (new_datas.Count > 0)
                    {
                        db.hrm_profile_status.AddRange(new_datas);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_replate", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/update_profile_replate", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> match_account()
        {
            var identity = User.Identity as ClaimsIdentity;
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
                    var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);

                    int en_type = int.Parse(provider.FormData.GetValues("type").SingleOrDefault());
                    string profile_id = provider.FormData.GetValues("profile_id").SingleOrDefault();
                    if (en_type == 1)
                    {
                        var en_user_id = provider.FormData.GetValues("user_id").SingleOrDefault();
                        var exists = await db.hrm_matchaccount.CountAsync(x => x.profile_id == profile_id && x.user_id == en_user_id) > 0;
                        if (!exists)
                        {
                            var matchaccountold = await db.hrm_matchaccount.Where(x => x.profile_id == profile_id).ToListAsync();
                            if (matchaccountold.Count > 0)
                            {
                                db.hrm_matchaccount.RemoveRange(matchaccountold);
                            }

                            hrm_matchaccount matchaccount = new hrm_matchaccount();
                            matchaccount.profile_id = profile_id;
                            matchaccount.user_id = en_user_id;
                            matchaccount.created_by = uid;
                            matchaccount.created_date = DateTime.Now;
                            matchaccount.created_ip = ip;
                            matchaccount.created_token_id = tid;
                            db.hrm_matchaccount.Add(matchaccount);
                        }
                    }
                    else if (en_type == 2)
                    {
                        var profile = await db.hrm_profile.AsNoTracking().FirstOrDefaultAsync(x => x.profile_id == profile_id);
                        if (profile != null)
                        {

                            var en_user = provider.FormData.GetValues("user").SingleOrDefault();
                            sys_users user = JsonConvert.DeserializeObject<sys_users>(en_user);

                            if (db.sys_users.Count(a => a.user_id == user.user_id) > 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Đã có tài khoản người dùng này trong hệ thống rồi!", err = "1" });
                            }
                            user.is_psword = BCrypt.Net.BCrypt.HashPassword(user.is_psword);
                            user.full_name = profile.profile_user_name;
                            user.full_name_en = helper.convertToUnSign(profile.profile_user_name);
                            user.last_name = (user.full_name ?? "").Split(' ').Last();
                            user.organization_id = profile.organization_id;
                            user.organization_parent_id = db.sys_organization.FirstOrDefault(x => x.organization_id == user.organization_id)?.parent_id ?? user.organization_id;
                            user.position_id = db.hrm_contract.FirstOrDefault(x => x.profile_id == profile_id && x.is_active == true)?.position_id;
                            user.birthday = profile.birthday;
                            user.phone = profile.phone;
                            user.email = profile.email;
                            user.gender = profile.gender;
                            user.status = 1;
                            if (!string.IsNullOrEmpty(profile.avatar))
                            {
                                var path = HttpContext.Current.Server.MapPath("~/") + profile.avatar;
                                if (File.Exists(path))
                                {
                                    var path_copy = "/Portals/" + user.organization_id + "/Users/" + user.user_id;
                                    var exists = HttpContext.Current.Server.MapPath("~/") + path_copy;
                                    if (!Directory.Exists(exists))
                                    {
                                        Directory.CreateDirectory(exists);
                                    }
                                    user.avatar = path_copy + "/" + helper.GenKey() + ".jpg";
                                    var root_copy = HttpContext.Current.Server.MapPath("~/") + user.avatar;
                                    File.Copy(path, root_copy, true);
                                }
                            }
                            db.sys_users.Add(user);

                            hrm_matchaccount matchaccount = new hrm_matchaccount();
                            matchaccount.profile_id = profile_id;
                            matchaccount.user_id = user.user_id;
                            matchaccount.created_by = uid;
                            matchaccount.created_date = DateTime.Now;
                            matchaccount.created_ip = ip;
                            matchaccount.created_token_id = tid;
                            db.hrm_matchaccount.Add(matchaccount);
                        }
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/match_account", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_profile/match_account", ip, tid, "Lỗi khi cập nhật", 0, "hrm_profile");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> import_excel_profile()
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
                    var user_now = await db.sys_users.FirstOrDefaultAsync(x => x.user_id == uid);
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
                    var task = await Request.Content.ReadAsMultipartAsync(provider);

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
                    fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                    FileInfo temp = new FileInfo(fpath);
                    using (ExcelPackage pck = new ExcelPackage(temp))
                    {
                        int? error_sheet = null;
                        int? error_row = null;
                        int? error_column = null;
                        try
                        {
                            List<ExcelWorksheet> sheets = pck.Workbook.Worksheets.ToList();
                            for (int s = 0; s < sheets.Count; s++)
                            {
                                error_sheet = s + 1;
                                if (sheets[s] != null)
                                {
                                    ExcelWorksheet sheet = sheets[s];
                                    switch (sheets[s].Name)
                                    {
                                        case "Hồ sơ nhân sự":
                                            List<hrm_profile> profiles = new List<hrm_profile>();
                                            List<hrm_profile_health> healths = new List<hrm_profile_health>();
                                            int number_profile = db.hrm_profile.Count(x => x.organization_id == user_now.organization_id) + 1;
                                            for (int r = 5; r <= sheet.Dimension.End.Row; r++)
                                            {
                                                error_row = r;
                                                if (sheet.Cells[r, 2].Value == null)
                                                {
                                                    break;
                                                }
                                                hrm_profile profile = new hrm_profile();
                                                hrm_profile_health health = new hrm_profile_health();
                                                for (int c = 2; c <= sheet.Dimension.End.Column; c++)
                                                {
                                                    if (sheet.Cells[4, c].Value == null)
                                                    {
                                                        break;
                                                    }
                                                    var column = sheet.Cells[4, c].Value;
                                                    error_column = int.Parse(column.ToString() ?? c.ToString());
                                                    var vl = sheet.Cells[r, c].Value;
                                                    if (vl != null && !string.IsNullOrEmpty(vl.ToString()))
                                                    {
                                                        string value = vl.ToString().Trim();
                                                        switch (column)
                                                        {
                                                            case "2":
                                                                profile.profile_code = value;
                                                                profile.is_order = number_profile++;
                                                                var p = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == profile.profile_code);
                                                                if (p != null)
                                                                {
                                                                    profile.profile_id = p.profile_id;
                                                                    profile.organization_id = p.organization_id;
                                                                }
                                                                break;
                                                            case "3":
                                                                profile.profile_name = value;
                                                                profile.profile_name_en = helper.convertToUnSign3(profile.profile_name);
                                                                profile.profile_last_name = profile.profile_name.Split(' ').Last();
                                                                profile.profile_user_name = value;
                                                                break;
                                                            case "4":
                                                                profile.identity_papers_code = value;
                                                                break;
                                                            case "5":
                                                                profile.identity_date_issue = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "6":
                                                                profile.identity_end_date_issue = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "7":
                                                                var identity_place_name = value;
                                                                var identity_place_exists = await db.hrm_ca_identity_place.FirstOrDefaultAsync(x => x.identity_place_name.Contains(identity_place_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (identity_place_exists != null)
                                                                {
                                                                    profile.identity_place_id = identity_place_exists.identity_place_id;
                                                                }
                                                                break;
                                                            case "8":
                                                                profile.tax_code = value;
                                                                break;
                                                            case "9":
                                                                profile.gender = value.ToLower().Contains("nam") ? 1 : value.ToLower().Contains("nữ") ? 2 : 3;
                                                                break;
                                                            case "10":
                                                                profile.birthday = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "11":
                                                                var ethnic_name = value;
                                                                var ethnic_exists = await db.hrm_ca_ethnic.FirstOrDefaultAsync(x => x.ethnic_name.Contains(ethnic_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (ethnic_exists != null)
                                                                {
                                                                    profile.ethnic_id = ethnic_exists.ethnic_id;
                                                                }
                                                                break;
                                                            case "12":
                                                                var religion_name = value;
                                                                var religio_exists = await db.hrm_ca_religion.FirstOrDefaultAsync(x => x.religion_name.Contains(religion_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (religio_exists != null)
                                                                {
                                                                    profile.religion_id = religio_exists.religion_id;
                                                                }
                                                                break;
                                                            case "13":
                                                                var nationality_name = value;
                                                                var nationality_exists = await db.hrm_ca_nationality.FirstOrDefaultAsync(x => x.nationality_name.Contains(nationality_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (nationality_exists != null)
                                                                {
                                                                    profile.nationality_id = nationality_exists.nationality_id;
                                                                }
                                                                break;
                                                            case "14":

                                                                break;
                                                            case "15":
                                                                var marital_status = value;
                                                                if (marital_status.ToLower().Contains("đã"))
                                                                {
                                                                    profile.marital_status = 1;
                                                                }
                                                                else if (marital_status.ToLower().Contains("ly"))
                                                                {
                                                                    profile.marital_status = 2;
                                                                }
                                                                else
                                                                {
                                                                    profile.marital_status = 0;
                                                                }
                                                                break;
                                                            case "16":
                                                                var bank_name = value;
                                                                var bank_exists = await db.hrm_ca_bank.FirstOrDefaultAsync(x => x.bank_name.Contains(bank_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (bank_exists != null)
                                                                {
                                                                    profile.bank_id = bank_exists.bank_id;
                                                                }
                                                                break;
                                                            case "17":
                                                                profile.bank_number = value;
                                                                break;
                                                            case "18":
                                                                profile.bank_account = value;
                                                                break;
                                                            case "19":
                                                                break;
                                                            case "20":
                                                                profile.place_permanent = value;
                                                                break;
                                                            case "21":
                                                                profile.place_residence_name = value;
                                                                break;
                                                            case "22":
                                                                profile.birthplace_name = value;
                                                                break;
                                                            case "23":
                                                                profile.birthplace_origin_name = value;
                                                                break;
                                                            case "24":
                                                                profile.place_register_permanent_first = value;
                                                                break;
                                                            case "25":
                                                                profile.place_register_permanent_name = value;
                                                                break;
                                                            case "26":
                                                                profile.involved_name = value;
                                                                break;
                                                            case "27":
                                                                profile.involved_phone = value;
                                                                break;
                                                            case "28":
                                                                profile.involved_place = value;
                                                                break;
                                                            case "29":
                                                                var relationship_name = value;
                                                                var relationship_exists = await db.hrm_ca_relationship.FirstOrDefaultAsync(x => x.relationship_name.Contains(relationship_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (relationship_exists != null)
                                                                {
                                                                    profile.relationship_id = relationship_exists.relationship_id;
                                                                }
                                                                break;
                                                            case "30":
                                                                profile.profile_nick_name = value;
                                                                break;
                                                            case "31":

                                                                break;
                                                            case "32":
                                                                profile.email = value;
                                                                break;
                                                            case "33":
                                                                profile.phone = value;
                                                                break;
                                                            case "34":
                                                                var olds = value.Split(',').ToList();
                                                                if (olds.Count > 1)
                                                                {
                                                                    var yearold = olds[0].ToLower().Trim().Replace("năm", "").Trim();
                                                                    if (!string.IsNullOrEmpty(yearold))
                                                                    {
                                                                        profile.seniority_year = int.Parse(yearold);
                                                                    }
                                                                    var monthold = olds[1].ToLower().Trim().Replace("tháng", "").Trim();
                                                                    if (!string.IsNullOrEmpty(monthold))
                                                                    {
                                                                        profile.seniority_month = int.Parse(monthold);
                                                                    }
                                                                }
                                                                else if (olds.Count > 0)
                                                                {
                                                                    if (olds[0].ToLower().Contains("năm"))
                                                                    {
                                                                        var yearold = olds[0].ToLower().Trim().Replace("năm", "").Trim();
                                                                        profile.seniority_year = int.Parse(yearold);
                                                                    }
                                                                    else if (olds[0].ToLower().Contains("tháng"))
                                                                    {
                                                                        var yearold = olds[0].ToLower().Trim().Replace("tháng", "").Trim();
                                                                        profile.seniority_year = int.Parse(yearold);
                                                                    }
                                                                }
                                                                break;
                                                            case "35":
                                                                profile.recruitment_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "36":
                                                                var cultural_level_name = value;
                                                                var cultural_level_exists = await db.hrm_ca_cultural_level.FirstOrDefaultAsync(x => x.cultural_level_name.Contains(cultural_level_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (cultural_level_exists != null)
                                                                {
                                                                    profile.cultural_level_id = cultural_level_exists.cultural_level_id;
                                                                }
                                                                break;
                                                            case "37":
                                                                var political_theory_name = value;
                                                                var political_theory_exists = await db.hrm_ca_political_theory.FirstOrDefaultAsync(x => x.political_theory_name.Contains(political_theory_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (political_theory_exists != null)
                                                                {
                                                                    profile.political_theory_id = political_theory_exists.political_theory_id;
                                                                }
                                                                break;
                                                            case "38":
                                                                var informatic_level_name = value;
                                                                var informatic_level_exists = await db.hrm_ca_informatic_level.FirstOrDefaultAsync(x => x.informatic_level_name.Contains(informatic_level_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (informatic_level_exists != null)
                                                                {
                                                                    profile.informatic_level_id = informatic_level_exists.informatic_level_id;
                                                                }
                                                                break;
                                                            case "39":
                                                                var language_level_name = value;
                                                                var language_level_exists = await db.hrm_ca_language_level.FirstOrDefaultAsync(x => x.language_level_name.Contains(language_level_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (language_level_exists != null)
                                                                {
                                                                    profile.language_level_id = language_level_exists.language_level_id;
                                                                }
                                                                break;
                                                            case "40":
                                                                var management_state_name = value;
                                                                var management_state_exists = await db.hrm_ca_management_state.FirstOrDefaultAsync(x => x.management_state_name.Contains(management_state_name) && (x.organization_id == profile.organization_id || x.is_system == true));
                                                                if (management_state_exists != null)
                                                                {
                                                                    profile.management_state_id = management_state_exists.management_state_id;
                                                                }
                                                                break;
                                                            case "41":
                                                                profile.is_partisan = value.ToLower() == "x" ? true : false;
                                                                break;
                                                            case "42":
                                                                profile.card_partisan = value;
                                                                break;
                                                            case "43":
                                                                profile.partisan_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "44":
                                                                profile.partisan_main_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "45":
                                                                break;
                                                            case "46":
                                                                profile.partisan_branch = value;
                                                                break;
                                                            case "47":
                                                                break;
                                                            case "48":
                                                                break;
                                                            case "49":
                                                                profile.military_start_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "50":
                                                                profile.military_end_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "51":
                                                                profile.military_rank = value;
                                                                break;
                                                            case "52":
                                                                profile.military_title = value;
                                                                break;
                                                            case "53":
                                                                profile.military_veterans_rank = value;
                                                                break;
                                                            case "54":
                                                                health.military_health = value;
                                                                break;
                                                            case "55":
                                                                health.height = value;
                                                                break;
                                                            case "56":
                                                                health.weight = value;
                                                                break;
                                                            case "57":
                                                                health.blood_group = value;
                                                                break;
                                                            case "58":
                                                                health.heartbeat = value;
                                                                break;
                                                            case "59":
                                                                health.blood_pressure = value;
                                                                break;
                                                            case "60":
                                                                health.note = value;
                                                                break;
                                                            case "61":
                                                                profile.mission_forte = value;
                                                                break;
                                                            case "62":
                                                                profile.biography_first = value;
                                                                break;
                                                            case "63":
                                                                profile.biography_second = value;
                                                                break;
                                                            case "64":
                                                                profile.biography_third = value;
                                                                break;
                                                            case "65":
                                                                profile.salary_family = double.Parse(value);
                                                                break;
                                                            case "66":
                                                                profile.salary_orther = double.Parse(value);
                                                                break;
                                                            case "67":
                                                                profile.type_rent = value;
                                                                break;
                                                            case "68":
                                                                profile.area_level = double.Parse(value);
                                                                break;
                                                            case "69":
                                                                profile.type_house = value;
                                                                break;
                                                            case "70":
                                                                profile.area_buy = double.Parse(value);
                                                                break;
                                                            case "71":
                                                                profile.area_granted = double.Parse(value);
                                                                break;
                                                            case "72":
                                                                profile.area_buy_yourself = double.Parse(value);
                                                                break;
                                                            case "73":
                                                                break;
                                                            case "74":
                                                                profile.note = value;
                                                                break;
                                                        }
                                                    }
                                                }
                                                var exs = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_id == profile.profile_id);
                                                if (exs != null)
                                                {
                                                    exs.profile_name = profile.profile_name;
                                                    exs.profile_name_en = profile.profile_name_en;
                                                    exs.profile_last_name = profile.profile_last_name;
                                                    exs.profile_user_name = profile.profile_user_name;
                                                    exs.identity_papers_code = profile.identity_papers_code;
                                                    exs.identity_date_issue = profile.identity_date_issue;
                                                    exs.identity_end_date_issue = profile.identity_end_date_issue;
                                                    exs.identity_place_id = profile.identity_place_id;
                                                    exs.tax_code = profile.tax_code;
                                                    exs.gender = profile.gender;
                                                    exs.birthday = profile.birthday;
                                                    exs.ethnic_id = profile.ethnic_id;
                                                    exs.religion_id = profile.religion_id;
                                                    exs.nationality_id = profile.nationality_id;
                                                    exs.bank_id = profile.bank_id;
                                                    exs.bank_number = profile.bank_number;
                                                    exs.bank_account = profile.bank_account;
                                                    exs.place_permanent = profile.place_permanent;
                                                    exs.place_residence_name = profile.place_residence_name;
                                                    exs.birthplace_name = profile.birthplace_name;
                                                    exs.birthplace_origin_name = profile.birthplace_origin_name;
                                                    exs.place_register_permanent_first = profile.place_register_permanent_first;
                                                    exs.place_register_permanent_name = profile.place_register_permanent_name;
                                                    exs.involved_name = profile.involved_name;
                                                    exs.involved_phone = profile.involved_phone;
                                                    exs.involved_place = profile.involved_place;
                                                    exs.relationship_id = profile.relationship_id;
                                                    exs.profile_nick_name = profile.profile_nick_name;
                                                    exs.email = profile.email;
                                                    exs.phone = profile.phone;
                                                    exs.seniority_year = profile.seniority_year;
                                                    exs.seniority_month = profile.seniority_month;
                                                    exs.recruitment_date = profile.recruitment_date;
                                                    exs.cultural_level_id = profile.cultural_level_id;
                                                    exs.political_theory_id = profile.political_theory_id;
                                                    exs.informatic_level_id = profile.informatic_level_id;
                                                    exs.language_level_id = profile.language_level_id;
                                                    exs.management_state_id = profile.management_state_id;
                                                    exs.is_partisan = profile.is_partisan;
                                                    exs.card_partisan = profile.card_partisan;
                                                    exs.partisan_date = profile.partisan_date;
                                                    exs.partisan_main_date = profile.partisan_main_date;
                                                    exs.partisan_branch = profile.partisan_branch;
                                                    exs.military_start_date = profile.military_start_date;
                                                    exs.military_end_date = profile.military_end_date;
                                                    exs.military_rank = profile.military_rank;
                                                    exs.military_title = profile.military_title;
                                                    exs.military_veterans_rank = profile.military_veterans_rank;
                                                    exs.mission_forte = profile.mission_forte;
                                                    exs.biography_first = profile.biography_first;
                                                    exs.biography_second = profile.biography_second;
                                                    exs.biography_third = profile.biography_third;
                                                    exs.salary_family = profile.salary_family;
                                                    exs.salary_orther = profile.salary_orther;
                                                    exs.type_rent = profile.type_rent;
                                                    exs.area_level = profile.area_level;
                                                    exs.type_house = profile.type_house;
                                                    exs.area_buy = profile.area_buy;
                                                    exs.area_granted = profile.area_granted;
                                                    exs.area_buy_yourself = profile.area_buy_yourself;
                                                    exs.note = profile.note;
                                                }
                                                else
                                                {
                                                    profile.profile_id = helper.GenKey();
                                                    profile.organization_id = user_now.organization_id;
                                                    profiles.Add(profile);
                                                }
                                                var exss = await db.hrm_profile_health.FirstOrDefaultAsync(x => x.profile_id == profile.profile_id);
                                                if (exss != null)
                                                {
                                                    exss.height = health.height;
                                                    exss.weight = health.weight;
                                                    exss.blood_group = health.blood_group;
                                                    exss.heartbeat = health.heartbeat;
                                                    exss.blood_pressure = health.blood_pressure;
                                                    exss.note = health.note;
                                                }
                                                else
                                                {
                                                    health.health_id = helper.GenKey();
                                                    health.profile_id = profile.profile_id;
                                                    healths.Add(health);
                                                }
                                            }
                                            if (profiles.Count > 0)
                                            {
                                                db.hrm_profile.AddRange(profiles);
                                            }
                                            if (healths.Count > 0)
                                            {
                                                db.hrm_profile_health.AddRange(healths);
                                            }
                                            await db.SaveChangesAsync();
                                            break;
                                        case "Quá trình làm việc trong đơn vị":
                                            int asstt = 1;
                                            List<hrm_profile_assignment> assignments = new List<hrm_profile_assignment>();
                                            for (int r = 4; r <= sheet.Dimension.End.Row; r++)
                                            {
                                                error_row = r;
                                                if (sheet.Cells[r, 2].Value == null)
                                                {
                                                    break;
                                                }
                                                hrm_profile_assignment assignment = new hrm_profile_assignment();
                                                for (int c = 2; c <= sheet.Dimension.End.Column; c++)
                                                {
                                                    if (sheet.Cells[3, c].Value == null)
                                                    {
                                                        break;
                                                    }
                                                    var column = sheet.Cells[3, c].Value;
                                                    error_column = int.Parse(column.ToString() ?? c.ToString());
                                                    var vl = sheet.Cells[r, c].Value;
                                                    if (vl != null && !string.IsNullOrEmpty(vl.ToString()))
                                                    {
                                                        string value = vl.ToString().Trim();
                                                        switch (column)
                                                        {
                                                            case "2":
                                                                var p = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == value);
                                                                if (p != null)
                                                                {
                                                                    assignment.profile_id = p.profile_id;
                                                                    assignment.organization_id = p.organization_id;
                                                                    asstt = db.hrm_profile_assignment.Count(x => x.profile_id == p.profile_id) + 1;
                                                                }
                                                                break;
                                                            case "3":

                                                                break;
                                                            case "4":
                                                                var listparent_id = db.sys_organization.FirstOrDefault(x => x.organization_id == assignment.organization_id)?.listparent_id ?? null;
                                                                var department_name = value;
                                                                var department_exists = await db.sys_organization.FirstOrDefaultAsync(x => x.organization_key == department_name && x.organization_type == 1 && listparent_id != null && x.listparent_id.Contains(listparent_id));
                                                                if (department_exists != null)
                                                                {
                                                                    assignment.department_id = department_exists.organization_id;
                                                                    assignment.department_names = value;
                                                                    var ps = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_id == assignment.profile_id);
                                                                    if (ps != null)
                                                                    {
                                                                        ps.id_department = assignment.department_id;
                                                                    }
                                                                }
                                                                break;
                                                            case "5":
                                                                var position_name = value;
                                                                var position_exists = await db.ca_positions.FirstOrDefaultAsync(x => x.position_name == position_name && (x.organization_id == assignment.organization_id || x.is_system == true));
                                                                if (position_exists != null)
                                                                {
                                                                    assignment.position_id = position_exists.position_id;
                                                                    assignment.position_names = value;
                                                                }
                                                                break;
                                                            case "6":
                                                                var title_name = value;
                                                                var title_exists = await db.hrm_ca_title.FirstOrDefaultAsync(x => x.title_name == title_name);
                                                                if (title_exists != null)
                                                                {
                                                                    assignment.title_id = title_exists.title_id;
                                                                    assignment.title_names = value;
                                                                }
                                                                break;
                                                            case "7":
                                                                var personel_groups_name = value;
                                                                var personel_groups_exists = await db.hrm_ca_personel_groups.FirstOrDefaultAsync(x => x.personel_groups_name == personel_groups_name && (x.organization_id == assignment.organization_id || x.is_system == true));
                                                                if (personel_groups_exists != null)
                                                                {
                                                                    assignment.personel_groups_id = personel_groups_exists.personel_groups_id;
                                                                    assignment.personel_groups_names = value;
                                                                }
                                                                break;
                                                            case "8":
                                                                var professional_work_name = value;
                                                                var professional_work_exists = await db.hrm_ca_professional_work.FirstOrDefaultAsync(x => x.professional_work_name == professional_work_name && (x.organization_id == assignment.organization_id || x.is_system == true));
                                                                if (professional_work_exists != null)
                                                                {
                                                                    assignment.professional_works = professional_work_exists.professional_work_id.ToString();
                                                                }
                                                                break;
                                                            case "9":
                                                                assignment.description = value;
                                                                break;
                                                            case "10":
                                                                assignment.is_active = value.ToLower().Contains("x") ? true : false;
                                                                break;
                                                            case "11":
                                                                assignment.is_experiment = value.ToLower().Contains("x") ? true : false;
                                                                break;
                                                            case "12":
                                                                assignment.start_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "13":
                                                                assignment.end_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "14":
                                                                break;
                                                            case "15":
                                                                break;
                                                            case "16":
                                                                break;
                                                            case "17":
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(assignment.profile_id))
                                                {
                                                    assignment.is_active = false;
                                                    assignment.is_main = false;
                                                    assignment.is_order = asstt++;
                                                    assignments.Add(assignment);

                                                    //var assignments_old = await db.hrm_profile_assignment.Where(x => x.profile_id == assignment.profile_id).ToListAsync();
                                                    //foreach(var item in assignments_old)
                                                    //{
                                                    //    item.is_active = false;
                                                    //    item.is_main = false;
                                                    //}
                                                }
                                            }
                                            if (assignments.Count > 0)
                                            {
                                                db.hrm_profile_assignment.AddRange(assignments);
                                            }
                                            await db.SaveChangesAsync();
                                            break;
                                        case "Hợp đồng":
                                            int cstt = 1;
                                            List<hrm_contract> contracts = new List<hrm_contract>();
                                            for (int r = 4; r <= sheet.Dimension.End.Row; r++)
                                            {
                                                error_row = r;
                                                if (sheet.Cells[r, 2].Value == null)
                                                {
                                                    break;
                                                }
                                                hrm_contract contract = new hrm_contract();
                                                for (int c = 2; c <= sheet.Dimension.End.Column; c++)
                                                {
                                                    if (sheet.Cells[3, c].Value == null)
                                                    {
                                                        break;
                                                    }
                                                    var column = sheet.Cells[3, c].Value;
                                                    error_column = int.Parse(column.ToString() ?? c.ToString());
                                                    var vl = sheet.Cells[r, c].Value;
                                                    if (vl != null && !string.IsNullOrEmpty(vl.ToString()))
                                                    {
                                                        string value = vl.ToString().Trim();
                                                        switch (column)
                                                        {
                                                            case "2":
                                                                var p = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == value);
                                                                if (p != null)
                                                                {
                                                                    contract.profile_id = p.profile_id;
                                                                    contract.organization_id = p.organization_id;
                                                                    cstt = db.hrm_contract.Count(x => x.profile_id == p.profile_id) + 1;
                                                                }
                                                                break;
                                                            case "3":

                                                                break;
                                                            case "4":
                                                                contract.contract_code = value;
                                                                break;
                                                            case "5":
                                                                var department_name = value;
                                                                var department_exists = await db.sys_organization.FirstOrDefaultAsync(x => x.short_name == department_name && x.organization_type == 1);
                                                                if (department_exists != null)
                                                                {
                                                                    contract.department_id = department_exists.organization_id;
                                                                }
                                                                break;
                                                            case "6":
                                                                var position_name = value;
                                                                var position_exists = await db.ca_positions.FirstOrDefaultAsync(x => x.position_name == position_name && (x.organization_id == contract.organization_id || x.is_system == true));
                                                                if (position_exists != null)
                                                                {
                                                                    contract.position_id = position_exists.position_id;
                                                                }
                                                                break;
                                                            case "7":
                                                                var title_name = value;
                                                                var title_exists = await db.hrm_ca_title.FirstOrDefaultAsync(x => x.title_name == title_name && (x.organization_id == contract.organization_id || x.is_system == true));
                                                                if (title_exists != null)
                                                                {
                                                                    contract.title_id = title_exists.title_id;
                                                                }
                                                                break;
                                                            case "8":
                                                                var personel_groups_name = value;
                                                                var personel_groups_exists = await db.hrm_ca_personel_groups.FirstOrDefaultAsync(x => x.personel_groups_name == personel_groups_name && (x.organization_id == contract.organization_id || x.is_system == true));
                                                                if (personel_groups_exists != null)
                                                                {
                                                                    contract.personel_groups_id = personel_groups_exists.personel_groups_id;
                                                                }
                                                                break;
                                                            case "9":
                                                                var type_contract_name = value;
                                                                var type_contract_exists = await db.hrm_ca_type_contract.FirstOrDefaultAsync(x => x.type_contract_name == type_contract_name && (x.organization_id == contract.organization_id || x.is_system == true));
                                                                if (type_contract_exists != null)
                                                                {
                                                                    contract.type_contract_id = type_contract_exists.type_contract_id;
                                                                }
                                                                break;
                                                            case "10":
                                                                break;
                                                            case "11":
                                                                contract.start_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "12":
                                                                contract.end_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "13":
                                                                contract.sign_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "14":
                                                                var ps = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == value);
                                                                if (ps != null)
                                                                {
                                                                    contract.sign_user_id = ps.profile_id;
                                                                }
                                                                break;
                                                            case "15":
                                                                contract.sign_address = value;
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(contract.profile_id))
                                                {
                                                    contract.contract_id = helper.GenKey();
                                                    contract.is_order = cstt++;
                                                    contracts.Add(contract);
                                                }
                                            }
                                            if (contracts.Count > 0)
                                            {
                                                db.hrm_contract.AddRange(contracts);
                                            }
                                            await db.SaveChangesAsync();
                                            break;
                                        case "QHGĐ":
                                            int rlstt = 1;
                                            List<hrm_profile_relative> relatives = new List<hrm_profile_relative>();
                                            for (int r = 4; r <= sheet.Dimension.End.Row; r++)
                                            {
                                                error_row = r;
                                                if (sheet.Cells[r, 2].Value == null)
                                                {
                                                    break;
                                                }
                                                hrm_profile_relative relative = new hrm_profile_relative();
                                                for (int c = 2; c <= sheet.Dimension.End.Column; c++)
                                                {
                                                    if (sheet.Cells[3, c].Value == null)
                                                    {
                                                        break;
                                                    }
                                                    var column = sheet.Cells[3, c].Value;
                                                    error_column = int.Parse(column.ToString() ?? c.ToString());
                                                    var vl = sheet.Cells[r, c].Value;
                                                    if (vl != null && !string.IsNullOrEmpty(vl.ToString()))
                                                    {
                                                        string value = vl.ToString().Trim();
                                                        int? rlorganization_id = null;
                                                        switch (column)
                                                        {
                                                            case "2":
                                                                var p = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == value);
                                                                if (p != null)
                                                                {
                                                                    relative.profile_id = p.profile_id;
                                                                    rlstt = db.hrm_profile_relative.Count(x => x.profile_id == p.profile_id) + 1;
                                                                }
                                                                break;
                                                            case "3":

                                                                break;
                                                            case "4":
                                                                relative.is_type = value.ToLower().Contains("chồng") ? 1 : value.ToLower().Contains("vợ") ? 2 : 1;
                                                                var relationship_name = value;
                                                                var relationship_exists = await db.hrm_ca_relationship.FirstOrDefaultAsync(x => x.relationship_name == relationship_name && (x.organization_id == rlorganization_id || x.is_system == true));
                                                                if (relationship_exists != null)
                                                                {
                                                                    relative.relationship_id = relationship_exists.relationship_id;
                                                                    relative.relationship_names = value;
                                                                }
                                                                break;
                                                            case "5":
                                                                relative.relative_name = value;
                                                                break;
                                                            case "6":
                                                                relative.birthday = value;
                                                                break;
                                                            case "7":
                                                                relative.address = value;
                                                                break;
                                                            case "8":
                                                                relative.countryside = value;
                                                                break;
                                                            case "9":
                                                                relative.occupation = value;
                                                                break;
                                                            case "10":
                                                                relative.company = value;
                                                                break;
                                                            case "11":
                                                                relative.organization = value;
                                                                break;
                                                            case "12":
                                                                relative.is_company = value.ToLower() == "x" ? true : false;
                                                                break;
                                                            case "13":
                                                                relative.is_dependent = value.ToLower() == "x" ? 1 : 0;
                                                                break;
                                                            case "14":
                                                                relative.is_die = value.ToLower() == "x" ? true : false;
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(relative.profile_id))
                                                {
                                                    relative.profile_relative_id = helper.GenKey();
                                                    relative.is_order = rlstt++;
                                                    relatives.Add(relative);
                                                }
                                            }
                                            if (relatives.Count > 0)
                                            {
                                                db.hrm_profile_relative.AddRange(relatives);
                                            }
                                            await db.SaveChangesAsync();
                                            break;
                                        case "Quá trình bồi dưỡng":
                                            //List<hrm_profile_skill> skills = new List<hrm_profile_skill>();
                                            //for (int r = 4; r <= sheet.Dimension.End.Row; r++)
                                            //{
                                            //    error_row = r;
                                            //    if (sheet.Cells[r, 2].Value == null)
                                            //    {
                                            //        break;
                                            //    }
                                            //    hrm_profile_skill skill = new hrm_profile_skill();
                                            //    for (int c = 2; c <= sheet.Dimension.End.Column; c++)
                                            //    {
                                            //        if (sheet.Cells[3, c].Value == null)
                                            //        {
                                            //            break;
                                            //        }
                                            //        var column = sheet.Cells[3, c].Value;
                                            //        error_column = int.Parse(column.ToString() ?? c.ToString());
                                            //        var vl = sheet.Cells[r, c].Value;
                                            //        if (vl != null && !string.IsNullOrEmpty(vl.ToString()))
                                            //        {
                                            //            string value = vl.ToString().Trim();
                                            //            switch (column)
                                            //            {
                                            //                case "2":
                                            //                    var p = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == value);
                                            //                    if (p != null)
                                            //                    {
                                            //                        skill.profile_id = p.profile_id;
                                            //                    }
                                            //                    break;
                                            //                case "3":

                                            //                    break;
                                            //                case "4":

                                            //                    break;
                                            //                case "5":
                                            //                    skill.start_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                            //                    break;
                                            //                case "6":
                                            //                    skill.end_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                            //                    break;
                                            //                case "7":
                                            //                    var specialization_name = value;
                                            //                    var specialization_exists = await db.hrm_ca_specialization.FirstOrDefaultAsync(x => x.specialization_name == specialization_name);
                                            //                    if (specialization_exists != null)
                                            //                    {
                                            //                        skill.specialized = specialization_exists.specialization_id;
                                            //                    }
                                            //                    break;
                                            //                case "8":
                                            //                    var form_traning_name = value;
                                            //                    var form_traning_exists = await db.hrm_ca_form_traning.FirstOrDefaultAsync(x => x.form_traning_name == form_traning_name);
                                            //                    if (form_traning_exists != null)
                                            //                    {
                                            //                        skill.form_traning_id = form_traning_exists.form_traning_id;
                                            //                    }
                                            //                    break;
                                            //                case "9":
                                            //                    skill.university_name = value;
                                            //                    break;
                                            //                case "10":
                                            //                    skill.rating = value;
                                            //                    break;
                                            //                case "11":
                                            //                    skill.is_man_degree = value.ToLower() == "x" ? true : false;
                                            //                    break;
                                            //                case "12":
                                            //                    var certificate_name = value;
                                            //                    var certificat_exists = await db.hrm_ca_certificate.FirstOrDefaultAsync(x => x.certificate_name == certificate_name);
                                            //                    if (certificat_exists != null)
                                            //                    {
                                            //                        skill.certificate_id = certificat_exists.certificate_id;
                                            //                    }
                                            //                    break;
                                            //                case "13":
                                            //                    break;
                                            //                case "14":
                                            //                    break;
                                            //                case "15":
                                            //                    skill.certificate_start_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                            //                    break;
                                            //                case "16":
                                            //                    skill.certificate_end_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                            //                    break;
                                            //                default:
                                            //                    break;
                                            //            }
                                            //        }
                                            //    }
                                            //    if (!string.IsNullOrEmpty(skill.profile_id))
                                            //    {
                                            //        skills.Add(skill);
                                            //    }
                                            //}
                                            //if (skills.Count > 0)
                                            //{
                                            //    db.hrm_profile_skill.AddRange(skills);
                                            //}
                                            //await db.SaveChangesAsync();
                                            break;
                                        case "Học vấn":
                                            int psstt = 1;
                                            List<hrm_profile_skill> skills = new List<hrm_profile_skill>();
                                            for (int r = 3; r <= sheet.Dimension.End.Row; r++)
                                            {
                                                error_row = r;
                                                if (sheet.Cells[r, 2].Value == null)
                                                {
                                                    break;
                                                }
                                                hrm_profile_skill skill = new hrm_profile_skill();
                                                for (int c = 2; c <= sheet.Dimension.End.Column; c++)
                                                {
                                                    if (sheet.Cells[2, c].Value == null)
                                                    {
                                                        break;
                                                    }
                                                    var column = sheet.Cells[2, c].Value;
                                                    error_column = int.Parse(column.ToString() ?? c.ToString());
                                                    var vl = sheet.Cells[r, c].Value;
                                                    if (vl != null && !string.IsNullOrEmpty(vl.ToString()))
                                                    {
                                                        string value = vl.ToString().Trim();
                                                        int? skorganization_id = null;
                                                        switch (column)
                                                        {
                                                            case "2":
                                                                var p = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == value);
                                                                if (p != null)
                                                                {
                                                                    skill.profile_id = p.profile_id;
                                                                    skorganization_id = p.organization_id;
                                                                    psstt = db.hrm_profile_skill.Count(x => x.profile_id == p.profile_id) + 1;
                                                                }
                                                                break;
                                                            case "3":

                                                                break;
                                                            case "4":
                                                                skill.start_date = value;
                                                                break;
                                                            case "5":
                                                                skill.end_date = value;
                                                                break;
                                                            case "6":
                                                                var academic_level_name = value;
                                                                var academic_level_exists = await db.hrm_ca_academic_level.FirstOrDefaultAsync(x => x.academic_level_name == academic_level_name && (x.organization_id == skorganization_id || x.is_system == true));
                                                                if (academic_level_exists != null)
                                                                {
                                                                    skill.academic_level_id = academic_level_exists.academic_level_id;
                                                                    skill.academic_level_names = value;
                                                                }
                                                                break;
                                                            case "7":
                                                                skill.branch = value;
                                                                break;
                                                            case "8":
                                                                var specialization_name = value;
                                                                var specialization_exists = await db.hrm_ca_specialization.FirstOrDefaultAsync(x => x.specialization_name == specialization_name && (x.organization_id == skorganization_id || x.is_system == true));
                                                                if (specialization_exists != null)
                                                                {
                                                                    skill.specialized = specialization_exists.specialization_id;
                                                                    skill.specialized_names = value;
                                                                }
                                                                break;
                                                            case "9":
                                                                skill.university_name = value;
                                                                break;
                                                            case "10":
                                                                var form_traning_name = value;
                                                                var form_traning_exists = await db.hrm_ca_form_traning.FirstOrDefaultAsync(x => x.form_traning_name == form_traning_name && (x.organization_id == skorganization_id || x.is_system == true));
                                                                if (form_traning_exists != null)
                                                                {
                                                                    skill.form_traning_id = form_traning_exists.form_traning_id;
                                                                    skill.form_traning_names = value;
                                                                }
                                                                break;
                                                            case "11":
                                                                skill.graduation_year = value;
                                                                break;
                                                            case "12":
                                                                skill.rating = value;
                                                                break;
                                                            case "13":
                                                                var certificate_name = value;
                                                                var certificat_exists = await db.hrm_ca_certificate.FirstOrDefaultAsync(x => x.certificate_name == certificate_name && (x.organization_id == skorganization_id || x.is_system == true));
                                                                if (certificat_exists != null)
                                                                {
                                                                    skill.certificate_id = certificat_exists.certificate_id;
                                                                    skill.certificate_names = value;
                                                                }
                                                                break;
                                                            case "14":
                                                                skill.is_man_degree = value.ToLower() == "x" ? true : false;
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(skill.profile_id))
                                                {
                                                    skill.profile_skill_id = helper.GenKey();
                                                    skill.is_order = psstt++;
                                                    skills.Add(skill);
                                                }
                                            }
                                            if (skills.Count > 0)
                                            {
                                                db.hrm_profile_skill.AddRange(skills);
                                            }
                                            await db.SaveChangesAsync();
                                            break;
                                        case "QT làm việc ngoài đơn vị":
                                            int exstt = 1;
                                            List<hrm_profile_experience> experiences = new List<hrm_profile_experience>();
                                            for (int r = 4; r <= sheet.Dimension.End.Row; r++)
                                            {
                                                error_row = r;
                                                if (sheet.Cells[r, 2].Value == null)
                                                {
                                                    break;
                                                }
                                                hrm_profile_experience experience = new hrm_profile_experience();
                                                for (int c = 2; c <= sheet.Dimension.End.Column; c++)
                                                {
                                                    if (sheet.Cells[3, c].Value == null)
                                                    {
                                                        break;
                                                    }
                                                    var column = sheet.Cells[3, c].Value;
                                                    error_column = int.Parse(column.ToString() ?? c.ToString());
                                                    var vl = sheet.Cells[r, c].Value;
                                                    if (vl != null && !string.IsNullOrEmpty(vl.ToString()))
                                                    {
                                                        string value = vl.ToString().Trim();
                                                        switch (column)
                                                        {
                                                            case "2":
                                                                var p = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == value);
                                                                if (p != null)
                                                                {
                                                                    experience.profile_id = p.profile_id;
                                                                    exstt = db.hrm_profile_experience.Count(x => x.profile_id == p.profile_id) + 1;
                                                                }
                                                                break;
                                                            case "3":

                                                                break;
                                                            case "4":
                                                                experience.company = value;
                                                                break;
                                                            case "5":
                                                                experience.address = value;
                                                                break;
                                                            case "6":
                                                                experience.start_date = value;
                                                                break;
                                                            case "7":
                                                                experience.end_date = value;
                                                                break;
                                                            case "8":
                                                                experience.title = value;
                                                                break;
                                                            case "9":
                                                                experience.role = value;
                                                                break;
                                                            case "10":
                                                                experience.description = value;
                                                                break;
                                                            case "11":
                                                                experience.reason = value;
                                                                break;
                                                            case "12":
                                                                experience.wage = value;
                                                                break;
                                                            case "13":
                                                                experience.reference_name = value;
                                                                break;
                                                            case "14":
                                                                experience.reference_phone = value;
                                                                break;
                                                            case "15":
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(experience.profile_id))
                                                {
                                                    experience.profile_experience_id = helper.GenKey();
                                                    experience.is_order = exstt++;
                                                    experiences.Add(experience);
                                                }
                                            }
                                            if (experiences.Count > 0)
                                            {
                                                db.hrm_profile_experience.AddRange(experiences);
                                            }
                                            await db.SaveChangesAsync();
                                            break;
                                        case "QT Khen Thưởng":
                                            int rstt = 1;
                                            List<hrm_reward> rewards = new List<hrm_reward>();
                                            for (int r = 4; r <= sheet.Dimension.End.Row; r++)
                                            {
                                                error_row = r;
                                                if (sheet.Cells[r, 2].Value == null)
                                                {
                                                    break;
                                                }
                                                hrm_reward reward = new hrm_reward();
                                                for (int c = 2; c <= sheet.Dimension.End.Column; c++)
                                                {
                                                    if (sheet.Cells[3, c].Value == null)
                                                    {
                                                        break;
                                                    }
                                                    var column = sheet.Cells[3, c].Value;
                                                    error_column = int.Parse(column.ToString() ?? c.ToString());
                                                    var vl = sheet.Cells[r, c].Value;
                                                    if (vl != null && !string.IsNullOrEmpty(vl.ToString()))
                                                    {
                                                        string value = vl.ToString().Trim();
                                                        switch (column)
                                                        {
                                                            case "2":
                                                                reward.reward_type = 1;
                                                                var p = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == value);
                                                                if (p != null)
                                                                {
                                                                    reward.reward_name = p.profile_id;
                                                                    reward.organization_id = p.organization_id;
                                                                    rstt = db.hrm_reward.Count(x => x.reward_name == p.profile_id) + 1;
                                                                }
                                                                break;
                                                            case "3":
                                                                break;
                                                            case "4":
                                                                reward.decision_date = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "5":
                                                                break;
                                                            case "6":
                                                                reward.reward_number = value;
                                                                break;
                                                            case "7":
                                                                var reward_level_name = value;
                                                                var reward_level_exists = await db.hrm_ca_reward_level.FirstOrDefaultAsync(x => x.reward_level_name == reward_level_name && (x.organization_id == reward.organization_id || x.is_system == true));
                                                                if (reward_level_exists != null)
                                                                {
                                                                    reward.reward_level_id = reward_level_exists.reward_level_id;
                                                                }
                                                                break;
                                                            case "8":
                                                                var reward_title_name = value;
                                                                var reward_title_exists = await db.hrm_ca_reward_title.FirstOrDefaultAsync(x => x.reward_title_name == reward_title_name && (x.organization_id == reward.organization_id || x.is_system == true));
                                                                if (reward_title_exists != null)
                                                                {
                                                                    reward.reward_title_id = reward_title_exists.reward_title_id;
                                                                }
                                                                break;
                                                            case "9":
                                                                reward.reward_content = value;
                                                                break;
                                                            case "10":
                                                                reward.decision_place = value;
                                                                break;
                                                            case "11":
                                                                var ps = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == value);
                                                                if (ps != null)
                                                                {
                                                                    reward.signer = ps.profile_id;
                                                                }
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(reward.reward_name))
                                                {
                                                    reward.is_order = rstt++;
                                                    rewards.Add(reward);
                                                }
                                            }
                                            if (rewards.Count > 0)
                                            {
                                                db.hrm_reward.AddRange(rewards);
                                            }
                                            await db.SaveChangesAsync();
                                            break;
                                        case "Bảo hiểm":
                                            int isstt = 1;
                                            List<hrm_insurance> insurances = new List<hrm_insurance>();
                                            List<hrm_insurance_pay> insurance_pays = new List<hrm_insurance_pay>();
                                            for (int r = 4; r <= sheet.Dimension.End.Row; r++)
                                            {
                                                error_row = r;
                                                if (sheet.Cells[r, 2].Value == null)
                                                {
                                                    break;
                                                }
                                                hrm_insurance insurance = new hrm_insurance();
                                                hrm_insurance_pay insurance_pay = new hrm_insurance_pay();
                                                for (int c = 2; c <= sheet.Dimension.End.Column; c++)
                                                {
                                                    if (sheet.Cells[3, c].Value == null)
                                                    {
                                                        break;
                                                    }
                                                    var column = sheet.Cells[3, c].Value;
                                                    error_column = int.Parse(column.ToString() ?? c.ToString());
                                                    var vl = sheet.Cells[r, c].Value;
                                                    if (vl != null && !string.IsNullOrEmpty(vl.ToString()))
                                                    {
                                                        string value = vl.ToString().Trim();
                                                        switch (column)
                                                        {
                                                            case "2":
                                                                var p = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_code == value);
                                                                if (p != null)
                                                                {
                                                                    insurance.profile_id = p.profile_id;
                                                                    insurance_pay.profile_id = p.profile_id;
                                                                    insurance.organization_id = p.organization_id;
                                                                }
                                                                break;
                                                            case "3":
                                                                break;
                                                            case "4":
                                                                insurance.insurance_id = value;
                                                                isstt = db.hrm_insurance_pay.Count(x => x.insurance_id == value) + 1;
                                                                break;
                                                            case "5":
                                                                insurance_pay.start_date = DateTime.ParseExact(value, "MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "6":
                                                                insurance_pay.end_date = DateTime.ParseExact(value, "MM/yyyy", CultureInfo.InvariantCulture);
                                                                break;
                                                            case "7":
                                                                var company_payment = value;
                                                                var company_payment_exists = await db.sys_organization.FirstOrDefaultAsync(x => x.organization_key == company_payment || x.organization_name == company_payment);
                                                                if (company_payment_exists != null)
                                                                {
                                                                    insurance_pay.company_payment = company_payment_exists.organization_id;
                                                                }
                                                                break;
                                                            case "8":
                                                                insurance_pay.title_name = value;
                                                                break;
                                                            case "9":
                                                                insurance_pay.salary = double.Parse(value);
                                                                break;
                                                            case "10":
                                                                insurance_pay.coef_salary = double.Parse(value.Replace(",", "."), CultureInfo.InvariantCulture);
                                                                break;
                                                            case "11":
                                                                insurance_pay.coef_allowance = double.Parse(value.Replace(",", "."), CultureInfo.InvariantCulture);
                                                                break;
                                                            case "12":
                                                                insurance_pay.total_payment = double.Parse(value);
                                                                break;
                                                            case "13":
                                                                insurance_pay.region = value;
                                                                break;
                                                            case "14":
                                                                insurance.insurance_code = value;
                                                                break;
                                                            case "15":
                                                                insurance.hospital_name = value;
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                }
                                                var check = insurances.Count(x => x.insurance_id == insurance.insurance_id) > 0;
                                                if (!string.IsNullOrEmpty(insurance.profile_id) && !check)
                                                {
                                                    insurance.is_order = isstt++;
                                                    insurances.Add(insurance);
                                                }
                                                if (insurance_pay.start_date != null)
                                                {
                                                    insurance_pay.insurance_pay_id = helper.GenKey();
                                                    insurance_pay.insurance_id = insurance.insurance_id;
                                                    insurance_pay.is_order = isstt++;
                                                    insurance_pays.Add(insurance_pay);
                                                }
                                            }
                                            if (insurances.Count > 0)
                                            {
                                                db.hrm_insurance.AddRange(insurances);
                                            }
                                            if (insurance_pays.Count > 0)
                                            {
                                                db.hrm_insurance_pay.AddRange(insurance_pays);
                                            }
                                            await db.SaveChangesAsync();
                                            break;
                                        default:
                                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = $"Lỗi định dạng trang thứ {error_sheet} sai tên sheet!" });
                                    }
                                }
                            }
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                        }
                        catch (DbEntityValidationException e)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e });
                        }
                        catch (Exception e)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = $"Lỗi định dạng dữ liệu tại trang thứ {error_sheet}, dòng thứ {error_row}, cột thứ {error_column}!", mss = e });
                        }
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "hrm_leave/import_excel", ip, tid, "Lỗi khi Import Excel", 0, "hrm_leave");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "hrm_leave/import_excel", ip, tid, "Lỗi khi Import Excel", 0, "hrm_leave");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }

        // Tim kiem nang cao freetext
        public class sqlProcQuery
        {
            public string? proc { get; set; }
            public bool query { get; set; }
            public List<sqlParQuery>? par { get; set; }
        }
        public class sqlParQuery
        {
            public string par { get; set; }
            public string va { get; set; }
            public sqlParQuery(string p, string v)
            {
                this.par = p;
                this.va = v;
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> PostProc([System.Web.Mvc.Bind(Include = "")][FromBody] JObject data)
        {
            string strSQL = data["str"].ToObject<string>();
            strSQL = Codec.DecryptString(strSQL, helper.psKey);
            sqlProcQuery proc = JsonConvert.DeserializeObject<sqlProcQuery>(strSQL)!;
            try
            {
                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                var sqlpas = new List<SqlParameter>();
                if (proc != null && proc.par != null)
                {
                    foreach (sqlParQuery p in proc.par)
                    {
                        sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                    }
                }
                var arrpas = sqlpas.ToArray();
                DateTime sdate = DateTime.Now;
                Task<DataTableCollection> task;
                if (proc!.query)
                {
                    if (proc!.proc.ToLower().Contains("delete ") || proc!.proc.ToLower().Contains("drop ") || proc!.proc.ToLower().Contains("update ") || proc!.proc.ToLower().Contains("insert ") || proc!.proc.ToLower().Contains("--"))
                    {
                        proc!.proc = Regex.Replace(proc!.proc, "delete ", " ", RegexOptions.IgnoreCase);
                        proc!.proc = Regex.Replace(proc!.proc, "drop ", " ", RegexOptions.IgnoreCase);
                        proc!.proc = Regex.Replace(proc!.proc, "update ", " ", RegexOptions.IgnoreCase);
                        proc!.proc = Regex.Replace(proc!.proc, "insert ", " ", RegexOptions.IgnoreCase);
                        proc!.proc = Regex.Replace(proc!.proc, "--", "", RegexOptions.IgnoreCase);
                    }
                    proc!.proc = Regex.Replace(proc!.proc, "drop2table", "drop table", RegexOptions.IgnoreCase);
                    proc!.proc = Regex.Replace(proc!.proc, "create2table", "create table", RegexOptions.IgnoreCase);
                    proc!.proc = Regex.Replace(proc!.proc, "insert2into", "insert into", RegexOptions.IgnoreCase);
                    task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, CommandType.Text, proc!.proc!).Tables);
                }
                else
                {
                    task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc!.proc!, arrpas).Tables);
                }
                var tables = await task;
                DateTime edate = DateTime.Now;
                string JSONresult = JsonConvert.SerializeObject(tables);
                int time = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0", proc_name = (helper.debug && proc!.query != true ? proc!.proc : ""), time });
            }
            catch (Exception e)
            {
                string StackTrace = e.StackTrace!;
                var messages = new List<string>();
                do
                {
                    messages.Add(e.Message);
                    e = e.InnerException!;
                }
                while (e != null);
                var message = string.Join("\n", messages);
                var contents = helper.ExceptionMessage(e);
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Helper.Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = message, err = "1" });
            }
        }
    }
}