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
using Newtonsoft.Json.Linq;
using ImageMagick;


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
    }
}