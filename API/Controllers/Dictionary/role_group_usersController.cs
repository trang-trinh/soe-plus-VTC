using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using API.Models;
using Helper;
using API.Helper;
using Newtonsoft.Json;
namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class role_group_usersController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_role_groups_user([System.Web.Mvc.Bind(Include = "created_date,created_by,created_token_id,created_ip,role_group_user_id,role_group_id")] doc_ca_role_group_users doc_Ca_Role_Group_Users)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
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
                using (DBEntities db = new DBEntities())
                {
                    doc_Ca_Role_Group_Users.created_date = DateTime.Now;
                    doc_Ca_Role_Group_Users.created_by = uid;
                    doc_Ca_Role_Group_Users.created_token_id = tid;
                    doc_Ca_Role_Group_Users.created_ip = ip;
                    db.doc_ca_role_group_users.Add(doc_Ca_Role_Group_Users);
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {

                        cms_logs log = new cms_logs();
                        log.log_title = "Thêm người dùng nhóm chức năng " + doc_Ca_Role_Group_Users.role_group_user_id;
                        log.log_content = JsonConvert.SerializeObject(new { data = doc_Ca_Role_Group_Users });
                        log.log_module = "Nhóm chức năng";
                        log.id_key = doc_Ca_Role_Group_Users.role_group_id.ToString();
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.cms_logs.Add(log);
                        db.SaveChanges();

                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "role_group_users/Add_role_groups_user", ip, tid, "Lỗi khi thêm người dùng nhóm chức năng", 0, "Nhóm chức năng");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "role_group_users/Add_role_groups_user", ip, tid, "Lỗi khi thêm người dùng nhóm chức năng", 0, "Nhóm chức năng");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Users([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                    var das = await db.doc_ca_role_group_users.Where(a => id.Contains(a.role_group_user_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<doc_ca_role_group_users> del = new List<doc_ca_role_group_users>();
                        foreach (var da in das)
                        {

                            del.Add(da);
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                cms_logs log = new cms_logs();
                                log.log_title = "Xóa người dùng - nhóm chức năng" + da.user_id;
                                log.log_module = "Nhóm người dùng";
                                log.id_key = da.role_group_user_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.cms_logs.Add(log);
                                db.SaveChanges();
                            }
                            #endregion
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.doc_ca_role_group_users.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "role_group_users/Delete_Users", ip, tid, "Lỗi khi xoá người dùng nhóm chức năng", 0, "Nhóm chức năng");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "role_group_users/Delete_Users", ip, tid, "Lỗi khi xoá người dùng nhóm chức năng", 0, "Nhóm chức năng");
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
