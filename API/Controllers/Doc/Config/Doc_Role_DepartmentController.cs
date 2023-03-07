using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Security.Claims;
using System.Web;
using Helper;
using API.Models;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using System.Data.Entity;
using API.Helper;
using Org.BouncyCastle.Crypto;

namespace API.Controllers.Doc.Config
{
    public class Doc_Role_DepartmentController : ApiController
    {

        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_user()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
            IEnumerable<Claim> claims = identity.Claims;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
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

                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        string tempGroup = provider.FormData.GetValues("group").SingleOrDefault();
                        List<doc_ca_role_group_department> ca_Groups = JsonConvert.DeserializeObject<List<doc_ca_role_group_department>>(tempGroup);
                        var dept_id = ca_Groups.FirstOrDefault();
                        var ex = db.doc_ca_role_group_department.Where(x => x.department_id == dept_id.department_id ).ToList();
                        db.doc_ca_role_group_department.RemoveRange(ex);
                        db.SaveChanges();
                        List<doc_ca_role_group_department> addCa = new List<doc_ca_role_group_department>();
                       
                        foreach (var da in ca_Groups)
                        {
                      
                            {  da.created_date = DateTime.Now;
                            da.created_by = uid;
                            da.created_ip = ip;
                            da.created_token_id = tid;
                                addCa.Add(da);
                            }
                           
                        }
                        db.doc_ca_role_group_department.AddRange(addCa);
                        db.SaveChanges();
                        #region add log
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "" }), domainurl + "Doc_Role_Department / Update_user", ip, tid, "Cập nhật phòng ban", 0, "Văn bản");
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "Doc_Role_Department/Update_user", ip, tid, "Lỗi khi cập nhật người duyệt", 0, "Văn bản");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "Doc_Role_Department/Update_user", ip, tid, "Lỗi khi cập nhật người duyệt", 0, "Văn bản");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteUser([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

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
                    var das = await db.doc_ca_role_group_department.Where(a => id.Any(x => x == a.department_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<doc_ca_role_group_department> del = new List<doc_ca_role_group_department>();
                        foreach (var da in das)
                        {
                            del.Add(da);
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, }), domainurl + "Doc_Role_Department/Delete_Weights", ip, tid, "Xóa người duyệt", 1, "Văn bản");
                            }
                            #endregion
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.doc_ca_role_group_department.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Doc_Role_Department/Delete_Weights", ip, tid, "Lỗi khi xoá người duyệt", 0, "Văn bản");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Doc_Role_Department/Delete_Weights", ip, tid, "Lỗi khi xoá người duyệt", 0, "Văn bản");
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
