using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Models;
using Helper;
using System.Web.Script.Serialization;
using HtmlAgilityPack;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class ProfileController : ApiController
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

        //[Authorize(Roles = "login")]
        //[Authorize]
        [HttpPost]
        public async Task<HttpResponseMessage> add_profile()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
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

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Hrm/Profile/Avatar/";
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
                        fdmodel = provider.FormData.GetValues("profile").SingleOrDefault();
                        hrm_profile model = JsonConvert.DeserializeObject<hrm_profile>(fdmodel);
                        if (db.hrm_profile.Count(a => a.profile_id == model.profile_id) > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Đã có mã nhân sự này trong hệ thống rồi!", err = "1" });
                        }
                        #region add avatar
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
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
                            fileName = helper.convertToUnSign3(fileName);
                            newFileName = Path.Combine(root + "/Hrm/Profile/Avatar/" + dvid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/Hrm/Profile/Avatar/" + dvid, fileName);
                            }
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            if (fileData.Headers.ContentDisposition.Name.Contains("avatar"))
                            {
                                model.avatar = "/Portals/Hrm/Profile/Avatar/" + dvid + "/" + fileName;
                            }
                            ffileData = fileData;
                            File.Move(fileData.LocalFileName, newFileName);
                            helper.ResizeImage(newFileName, 1920, 1080, 90);
                        }
                        #endregion
                        var skill = provider.FormData.GetValues("skill").SingleOrDefault();
                        var relative = provider.FormData.GetValues("relative").SingleOrDefault();
                        var experience = provider.FormData.GetValues("experience").SingleOrDefault();
                        var clan_history = provider.FormData.GetValues("clan_history").SingleOrDefault();

                        List<hrm_profile_skill> skills = JsonConvert.DeserializeObject<List<hrm_profile_skill>>(skill);
                        List<hrm_profile_relative> relatives = JsonConvert.DeserializeObject<List<hrm_profile_relative>>(relative);
                        List<hrm_profile_experience> experiences = JsonConvert.DeserializeObject<List<hrm_profile_experience>>(experience);
                        List<hrm_profile_clan_history> clan_historys = JsonConvert.DeserializeObject<List<hrm_profile_clan_history>>(clan_history);

                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        model.created_token_id = tid;
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_token_id = tid;
                        model.created_ip = ip;

                        db.hrm_profile.Add(model);
                        #region add skill
                        if (skills != null)
                        {
                            var count = db.hrm_profile_skill.Count();
                            foreach (var item in skills)
                            {
                                count++;
                                hrm_profile_skill sk = new hrm_profile_skill();
                                sk = item;
                                sk.profile_skill_id = helper.GenKey();
                                sk.profile_id = model.profile_id;
                                sk.is_order = count;
                                sk.created_token_id = tid;
                                sk.created_date = DateTime.Now;
                                sk.created_by = uid;
                                sk.created_token_id = tid;
                                sk.created_ip = ip;
                                db.hrm_profile_skill.Add(sk);
                            }
                        }
                        #endregion
                        #region add relative
                        if (relatives != null)
                        {
                            var count = db.hrm_profile_relative.Count();
                            foreach (var item in relatives)
                            {
                                count++;
                                hrm_profile_relative rl = new hrm_profile_relative();
                                rl = item;
                                rl.profile_relative_id = helper.GenKey();
                                rl.profile_id = model.profile_id;
                                rl.is_order = count;
                                rl.created_token_id = tid;
                                rl.created_date = DateTime.Now;
                                rl.created_by = uid;
                                rl.created_token_id = tid;
                                rl.created_ip = ip;
                                db.hrm_profile_relative.Add(rl);
                            }
                        }
                        #endregion
                        #region add experience
                        if (experiences != null)
                        {
                            var count = db.hrm_profile_experience.Count();
                            foreach (var item in experiences)
                            {
                                count++;
                                hrm_profile_experience ex = new hrm_profile_experience();
                                ex = item;
                                ex.profile_experience_id = helper.GenKey();
                                ex.profile_id = model.profile_id;
                                ex.is_order = count;
                                ex.created_token_id = tid;
                                ex.created_date = DateTime.Now;
                                ex.created_by = uid;
                                ex.created_token_id = tid;
                                ex.created_ip = ip;
                                db.hrm_profile_experience.Add(ex);
                            }
                        }
                        #endregion
                        #region add clan_history
                        if (clan_historys != null)
                        {
                            var count = db.hrm_profile_clan_history.Count();
                            foreach (var item in clan_historys)
                            {
                                count++;
                                hrm_profile_clan_history clan = new hrm_profile_clan_history();
                                clan = item;
                                clan.profile_clan_history_id = helper.GenKey();
                                clan.profile_id = model.profile_id;
                                clan.is_order = count;
                                clan.created_token_id = tid;
                                clan.created_date = DateTime.Now;
                                clan.created_by = uid;
                                clan.created_token_id = tid;
                                clan.created_ip = ip;
                                db.hrm_profile_clan_history.Add(clan);
                            }
                        }
                        #endregion
                        db.SaveChanges();
                        #region  add Log
                        //helper.saveLogFiles(uid, 5, null, model.profile_id, "Thêm folder " + model.folder_name, ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = model.profile_id });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_profile()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
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

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Hrm/Profile/Avatar/";
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
                        fdmodel = provider.FormData.GetValues("profile").SingleOrDefault();
                        hrm_profile model = JsonConvert.DeserializeObject<hrm_profile>(fdmodel);
                        #region add avatar
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
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
                            fileName = helper.convertToUnSign3(fileName);
                            newFileName = Path.Combine(root + "/Hrm/Profile/Avatar/" + dvid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/Hrm/Profile/Avatar/" + dvid, fileName);
                            }
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            if (fileData.Headers.ContentDisposition.Name.Contains("avatar"))
                            {
                                model.avatar = "/Portals/Hrm/Profile/Avatar/" + dvid + "/" + fileName;
                            }
                            ffileData = fileData;
                            File.Move(fileData.LocalFileName, newFileName);
                            helper.ResizeImage(newFileName, 1920, 1080, 90);
                        }
                        #endregion
                        var skill = provider.FormData.GetValues("skill").SingleOrDefault();
                        var relative = provider.FormData.GetValues("relative").SingleOrDefault();
                        var experience = provider.FormData.GetValues("experience").SingleOrDefault();
                        var clan_history = provider.FormData.GetValues("clan_history").SingleOrDefault();

                        List<hrm_profile_skill> skills = JsonConvert.DeserializeObject<List<hrm_profile_skill>>(skill);
                        List<hrm_profile_relative> relatives = JsonConvert.DeserializeObject<List<hrm_profile_relative>>(relative);
                        List<hrm_profile_experience> experiences = JsonConvert.DeserializeObject<List<hrm_profile_experience>>(experience);
                        List<hrm_profile_clan_history> clan_historys = JsonConvert.DeserializeObject<List<hrm_profile_clan_history>>(clan_history);

                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_ip = ip;
                        model.modified_token_id = tid;
                        model.created_token_id = tid;
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_token_id = tid;
                        model.created_ip = ip;
                        db.Entry(model).State = EntityState.Modified;
                        #region add skill
                        //delte all 
                        List<hrm_profile_skill> itemToRemove = db.hrm_profile_skill.Where(a => a.profile_id == model.profile_id).ToList();
                        if (itemToRemove.Count > 0)
                            db.hrm_profile_skill.RemoveRange(itemToRemove);
                        if (skills != null)
                        {
                            var count = db.hrm_profile_skill.Count();
                            foreach (var item in skills)
                            {
                                count++;
                                hrm_profile_skill sk = new hrm_profile_skill();
                                sk = item;
                                sk.profile_skill_id = helper.GenKey();
                                sk.profile_id = model.profile_id;
                                sk.is_order = count;
                                sk.created_token_id = tid;
                                sk.created_date = DateTime.Now;
                                sk.created_by = uid;
                                sk.created_token_id = tid;
                                sk.created_ip = ip;
                                db.hrm_profile_skill.Add(sk);
                            }
                        }
                        #endregion
                        #region add relative
                        List<hrm_profile_relative> itemToRemove2 = db.hrm_profile_relative.Where(a => a.profile_id == model.profile_id).ToList();
                        if (itemToRemove.Count > 0)
                            db.hrm_profile_relative.RemoveRange(itemToRemove2);
                        if (relatives != null)
                        {
                            var count = db.hrm_profile_relative.Count();
                            foreach (var item in relatives)
                            {
                                count++;
                                hrm_profile_relative rl = new hrm_profile_relative();
                                rl = item;
                                rl.profile_relative_id = helper.GenKey();
                                rl.profile_id = model.profile_id;
                                rl.is_order = count;
                                rl.created_token_id = tid;
                                rl.created_date = DateTime.Now;
                                rl.created_by = uid;
                                rl.created_token_id = tid;
                                rl.created_ip = ip;
                                db.hrm_profile_relative.Add(rl);
                            }
                        }
                        #endregion
                        #region add experience
                        List<hrm_profile_experience> itemToRemove3 = db.hrm_profile_experience.Where(a => a.profile_id == model.profile_id).ToList();
                        if (itemToRemove.Count > 0)
                            db.hrm_profile_experience.RemoveRange(itemToRemove3);
                        if (experiences != null)
                        {
                            var count = db.hrm_profile_experience.Count();
                            foreach (var item in experiences)
                            {
                                count++;
                                hrm_profile_experience ex = new hrm_profile_experience();
                                ex = item;
                                ex.profile_experience_id = helper.GenKey();
                                ex.profile_id = model.profile_id;
                                ex.is_order = count;
                                ex.created_token_id = tid;
                                ex.created_date = DateTime.Now;
                                ex.created_by = uid;
                                ex.created_token_id = tid;
                                ex.created_ip = ip;
                                db.hrm_profile_experience.Add(ex);
                            }
                        }
                        #endregion
                        #region add clan_history
                        List<hrm_profile_clan_history> itemToRemove4 = db.hrm_profile_clan_history.Where(a => a.profile_id == model.profile_id).ToList();
                        if (itemToRemove.Count > 0)
                            db.hrm_profile_clan_history.RemoveRange(itemToRemove4);
                        if (clan_historys != null)
                        {
                            var count = db.hrm_profile_clan_history.Count();
                            foreach (var item in clan_historys)
                            {
                                count++;
                                hrm_profile_clan_history clan = new hrm_profile_clan_history();
                                clan = item;
                                clan.profile_clan_history_id = helper.GenKey();
                                clan.profile_id = model.profile_id;
                                clan.is_order = count;
                                clan.created_token_id = tid;
                                clan.created_date = DateTime.Now;
                                clan.created_by = uid;
                                clan.created_token_id = tid;
                                clan.created_ip = ip;
                                db.hrm_profile_clan_history.Add(clan);
                            }
                        }
                        #endregion
                        db.SaveChanges();
                        #region  add Log
                        //helper.saveLogFiles(uid, 5, null, model.profile_id, "Thêm folder " + model.folder_name, ip, tid);
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = model.profile_id });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "FildeFolder/Add_FildeFolder", ip, tid, "Lỗi khi thêm người dùng", 0, "FildeFolder");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> del_profile([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        var das = await db.hrm_profile.Where(a => ids.Contains(a.profile_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das.Count > 0)
                        {
                            List<hrm_profile> del = new List<hrm_profile>();
                            foreach (var da in das)
                            {
                                //xoa file
                                var profiles = await db.hrm_profile.Where(a => a.profile_id == da.profile_id).ToListAsync();
                                //if (profiles.Count > 0)
                                //{
                                //    foreach (var f in profiles)
                                //    {
                                //        if (!string.IsNullOrWhiteSpace(f.is_filepath) && f.id_key == null)
                                //            paths.Add(HttpContext.Current.Server.MapPath("~/") + f.is_filepath);
                                //    }
                                //}
                                del.Add(da);
                                #region  add Log
                                #endregion
                            }
                            //foreach (string strPath in paths)
                            //{
                            //    bool exists = System.IO.File.Exists(strPath);
                            //    if (exists)
                            //        System.IO.File.Delete(strPath);
                            //}
                            db.hrm_profile.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FileFolder/Del_FildeFolder", ip, tid, "Lỗi khi xoá người dùng", 0, "FildeFolder");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "FildeFolder/Del_FildeFolder", ip, tid, "Lỗi khi xoá người dùng", 0, "FildeFolder");
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

        #region CallProc
        [HttpPost]
        public async Task<HttpResponseMessage> GetDataProc([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
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
                var sqlpas = new List<SqlParameter>();
                if (proc != null && proc.par != null)
                {
                    foreach (sqlPar p in proc.par)
                    {
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
                        log.controller = domainurl + "User/GetDataProc";
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
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "chat/GetDataProc", ip, tid, "Lỗi khi gọi proc '" + proc + "'", 0, "chat");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "chat/GetDataProc", ip, tid, "Lỗi khi gọi proc '" + proc + "'", 0, "chat");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }
        #endregion

    }
}