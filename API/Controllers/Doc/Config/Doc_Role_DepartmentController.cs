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
namespace API.Controllers.Doc.Config
{
    public class Doc_Role_DepartmentController : ApiController
    {

        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_user([System.Web.Mvc.Bind(Include = "user_id,organization_id,created_by,created_date,created_ip,created_token_id,modified_by,modified_date,modified_ip,modified_token_id,department_id")] doc_ca_role_group_department role_Groups)
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
                    var extsss = db.doc_ca_role_group_department.Where(x => x.role_group_id == null && x.department_id == role_Groups.department_id).FirstOrDefault();
                    if (extsss != null)
                    {
                        int id = Convert.ToInt32(dvid);
                        extsss.user_id = role_Groups.user_id;
                        extsss.modified_by = uid;
                        extsss.modified_date = DateTime.Now;
                        extsss.modified_ip = ip;
                        extsss.modified_token_id = tid;
                        extsss.organization_id = id;
                        db.Entry(extsss).State = EntityState.Modified;
                    }
                    else
                    {
                        int id = Convert.ToInt32(dvid);
                        role_Groups.created_by = uid;
                        role_Groups.created_date = DateTime.Now;
                        role_Groups.created_ip = ip;
                        role_Groups.created_token_id = tid;
                        role_Groups.organization_id = id;
                        db.doc_ca_role_group_department.Add(role_Groups);
                    }
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = role_Groups, }), domainurl + "Doc_Role_Department/Update_user", ip, tid, "Cập nhật người duyệt", 1, "Văn bản");
                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
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
                    var das = await db.doc_ca_role_group_department.Where(a => id.Contains(a.role_group_department_id)).ToListAsync();
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
