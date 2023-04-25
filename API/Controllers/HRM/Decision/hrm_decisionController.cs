using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers.Hrm
{
    [Authorize(Roles = "login")]
    public class hrm_decisionController : ApiController
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
        public async Task<HttpResponseMessage> update_decision()
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
                    hrm_decision model = JsonConvert.DeserializeObject<hrm_decision>(md);
                    #region Model
                    if (isAdd)
                    {
                        var check = await db.hrm_decision.CountAsync(x => x.decision_code == model.decision_code) > 0;
                        if (check)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Mã quyết định đã tồn tại!" });
                        }
                        model.decision_id = helper.GenKey();
                        model.is_order = model.is_order ?? (db.hrm_decision.Count() + 1);
                        model.status = 0;
                        model.created_by = uid;
                        model.created_date = DateTime.Now;
                        model.created_ip = ip;
                        model.created_token_id = tid;
                        model.organization_id = user_now.organization_id ?? user_now.organization_parent_id;
                        db.hrm_decision.Add(model);
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
                    string path = root + "/" + model.organization_id + "/Hrm/Decision/" + model.decision_id;
                    bool exists = Directory.Exists(path);
                    if (!exists)
                        Directory.CreateDirectory(path);
                    List<hrm_file> dfs = new List<hrm_file>();
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
                        string Duongdan = "/Portals/" + model.organization_id + "/Hrm/Decision/" + model.decision_id + "/" + name_file;
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
                        var df = new hrm_file();
                        df.key_id = model.decision_id;
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
                        df.is_type = 10;
                        df.status = true;
                        df.created_by = uid;
                        df.created_date = DateTime.Now;
                        df.created_ip = ip;
                        df.created_token_id = tid;
                        df.organization_id = user_now.organization_id ?? user_now.organization_parent_id;
                        dfs.Add(df);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_decision/update_decision", ip, tid, "Lỗi khi cập nhật", 0, "hrm_decision");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_decision/update_decision", ip, tid, "Lỗi khi cập nhật", 0, "hrm_decision");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_file([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> ids)
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
                        var das = await db.hrm_file.Where(a => ids.Contains(a.file_id) && (ad || a.created_by == uid)).ToListAsync();
                        var dasUrl = await db.hrm_file.AsNoTracking().Where(a => ids.Contains(a.file_id) && (ad || a.created_by == uid) && a.file_path != null).Select(a => a.file_path).ToListAsync();
                        List<string> paths = new List<string>();
                        string root = HttpContext.Current.Server.MapPath("~/");
                        if (das != null)
                        {
                            List<hrm_file> del = new List<hrm_file>();
                            foreach (var da in das)
                            {
                                del.Add(da);
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
                                db.hrm_file.RemoveRange(del);
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "hrm_decision/delete_file", ip, tid, "Lỗi khi xoá file", 0, "hrm_decision");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "hrm_decision/delete_file", ip, tid, "Lỗi khi xoá file", 0, "hrm_decision");
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
        public async Task<HttpResponseMessage> delete_decision([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        var das = await db.hrm_decision.Where(a => ids.Contains(a.decision_id)).ToListAsync();
                        var dasUrl = await db.hrm_file.AsNoTracking().Where(a => ids.Contains(a.key_id) && (ad || a.created_by == uid) && a.file_path != null).Select(a => a.file_path).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<hrm_decision> del = new List<hrm_decision>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                            }
                            foreach (var p in dasUrl)
                            {
                                paths.Add(p);
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_decision.RemoveRange(del);
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "hrm_decision/delete_decision", ip, tid, "Lỗi khi xóa", 0, "hrm_decision");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "hrm_decision/delete_decision", ip, tid, "Lỗi khi xoá", 0, "hrm_decision");
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

    }
}
