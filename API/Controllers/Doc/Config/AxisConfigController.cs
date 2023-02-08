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
using Newtonsoft.Json;
using API.Helper;
namespace API.Controllers.Doc.Config
{
    public class AxisConfigController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_doc_org_connect([System.Web.Mvc.Bind(Include = "connect_token_key,created_by,created_date,created_ip,created_token_id")] doc_organization_connect doc_Organization_Connect1)
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
                    var search = db.doc_organization_connect.FirstOrDefault(x => x.organization_connect_id == doc_Organization_Connect1.organization_connect_id);
                    if (search != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã cấu hình trục liên thông đã có trong hệ thống!", err = "1" });
                    }
                    else
                    {
                        doc_Organization_Connect1.connect_token_key = doc_Organization_Connect1.connect_token_key == null ? helper.GenKey() : doc_Organization_Connect1.connect_token_key;
                        doc_Organization_Connect1.created_by = uid;
                        doc_Organization_Connect1.created_date = DateTime.Now;
                        doc_Organization_Connect1.created_ip = ip;
                        doc_Organization_Connect1.created_token_id = tid;
                        db.doc_organization_connect.Add(doc_Organization_Connect1);
                        await db.SaveChangesAsync();
                        #region add sys_logs
                        if (helper.wlog)
                        {
                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = doc_Organization_Connect1, }), domainurl + "AxisConfig/add_doc_org_connect", ip, tid, "Thêm mới cấu hình trục liên thông", 1, "Văn bản");
                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "AxisConfig/add_doc_org_connect", ip, tid, "Lỗi khi thêm cấu hình trục liên thông", 0, "Văn bản");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "AxisConfig/add_doc_org_connect", ip, tid, "Lỗi khi thêm cấu hình trục liên thông", 0, "Văn bản");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_doc_org_connect([System.Web.Mvc.Bind(Include = "connect_token_key,modified_by,modified_date,modified_ip,modified_token_id")] doc_organization_connect role_Groups)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
            IEnumerable<Claim> claims = identity.Claims;
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
                    role_Groups.connect_token_key = role_Groups.connect_token_key == null ? helper.GenKey() : role_Groups.connect_token_key;
                    role_Groups.modified_by = uid;
                    role_Groups.modified_date = DateTime.Now;
                    role_Groups.modified_ip = ip;
                    role_Groups.modified_token_id = tid;
                    db.Entry(role_Groups).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    #region add sys_logs
                    if (helper.wlog)
                    {
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = role_Groups, }), domainurl + "AxisConfig/Update_doc_org_connect", ip, tid, "Cập nhật cấu hình trục liên thông", 1, "Văn bản");
                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "AxisConfig/Update_doc_org_connect", ip, tid, "Lỗi khi cập nhật cấu hình trục liên thông", 0, "Văn bản");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "AxisConfig/Update_doc_org_connect", ip, tid, "Lỗi khi cập nhật cấu hình trục liên thông", 0, "Văn bản");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_doc_org_connect([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                    var checkDel = db.doc_connect.Where(x => id.Contains(x.organization_connect_id)).ToList();
                    if (checkDel.Count != 0)
                    {
                        string list_id = "";
                        foreach (var d in checkDel)
                        { list_id += (list_id == "" ? "" : ",") + d.organization_connect_id.ToString(); }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Cấu hình " + list_id + " đã được sử dụng.<br/> Bạn không thể xóa cấu hình này!" });
                    }
                    var das = await db.doc_organization_connect.Where(a => id.Contains(a.organization_connect_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<doc_organization_connect> del = new List<doc_organization_connect>();
                        foreach (var da in das)
                        {
                            del.Add(da);
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da }), domainurl + "AxisConfig/Delete_doc_org_connect", ip, tid, "Xóa cấu hình trục liên thông", 1, "Văn bản");
                            }
                            #endregion
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Đã xảy ra lỗi! Vui lòng thử lại." });
                        }
                        db.doc_organization_connect.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "AxisConfig/Delete_doc_org_connect", ip, tid, "Lỗi khi xoá cấu hình trục liên thông", 0, "Văn bản");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "AxisConfig/Delete_doc_org_connect", ip, tid, "Lỗi khi xóa cấu hình trục liên thông", 0, "Văn bản");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }


        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Status_Doc_org_Connect([System.Web.Mvc.Bind(Include = "TextID,BitTrangthai")] Trangthai trangthai)
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
                    var das = db.doc_organization_connect.FirstOrDefault(a => a.organization_connect_id == trangthai.TextID);
                    if (das != null)
                    {

                        das.status = !trangthai.BitTrangthai;
                        das.modified_by = uid;
                        das.modified_date = DateTime.Now;
                        das.modified_ip = ip;
                        das.modified_token_id = tid;
                        #region add cms_logs
                        if (helper.wlog)
                        {
                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = das.organization_connect_id, das.status }), domainurl + "AxisConfig/Update_Status_Doc_org_Connect", ip, tid, "Cập nhật trạng thái cấu hình trục liên thông", 1, "Văn bản");
                        }
                        #endregion
                        await db.SaveChangesAsync();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "AxisConfig/Update_Status_Doc_org_Connect", ip, tid, "Lỗi khi cập nhật trạng thái cấu hình trục liên thông", 0, "Văn bản");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "AxisConfig/Update_Status_Doc_org_Connect", ip, tid, "Lỗi khi cập nhật trạng thái cấu hình trục liên thông", 0, "Văn bản");
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
