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
                        if (old != null && old.profile_id != model.profile_id) {
                            if (db.hrm_profile.Count(a => a.profile_id == model.profile_id) > 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Đã tồn tại mã nhân sự này trong hệ thống!", err = "1" });
                            }
                        }
                    }
                    var relative = provider.FormData.GetValues("relative").SingleOrDefault();
                    var skill = provider.FormData.GetValues("skill").SingleOrDefault();
                    var clan_history = provider.FormData.GetValues("clan_history").SingleOrDefault();
                    var experience = provider.FormData.GetValues("experience").SingleOrDefault();

                    List<hrm_profile_relative> relatives = JsonConvert.DeserializeObject<List<hrm_profile_relative>>(relative);
                    List<hrm_profile_skill> skills = JsonConvert.DeserializeObject<List<hrm_profile_skill>>(skill);
                    List<hrm_profile_clan_history> clan_historys = JsonConvert.DeserializeObject<List<hrm_profile_clan_history>>(clan_history);
                    List<hrm_profile_experience> experiences = JsonConvert.DeserializeObject<List<hrm_profile_experience>>(experience);
                    #region Model
                    if (isAdd)
                    {
                        model.is_order = model.is_order ?? (db.hrm_profile.Count() + 1);
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id;
                        db.hrm_profile.Add(model);
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
                    #region add relative
                    var relatives_old = await db.hrm_profile_relative.Where(x => x.profile_id == model.profile_id).ToListAsync();
                    if (relatives_old.Count > 0)
                    {
                        db.hrm_profile_relative.RemoveRange(relatives_old);
                    }
                    if (relatives != null)
                    {
                        var count = 0;
                        foreach (var rl in relatives)
                        {
                            rl.profile_relative_id = helper.GenKey();
                            rl.profile_id = model.profile_id;
                            rl.is_order = count++;
                            rl.created_token_id = tid;
                            rl.created_date = DateTime.Now;
                            rl.created_by = uid;
                            rl.created_token_id = tid;
                            rl.created_ip = ip;
                            db.hrm_profile_relative.Add(rl);
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
                            df.created_by = uid;
                            df.created_date = DateTime.Now;
                            df.created_ip = ip;
                            df.created_token_id = tid;
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
    }
}