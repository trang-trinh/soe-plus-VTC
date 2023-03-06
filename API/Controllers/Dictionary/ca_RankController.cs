using API.Helper;
using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers.Dictionary
{
    [Authorize(Roles = "login")]
    public class ca_RankController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;

        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_Rank([System.Web.Mvc.Bind(Include = "created_by,created_date,created_ip,created_token_id")] ca_rank ca_Rank)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
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

                    ca_Rank.created_by = uid;
                    ca_Rank.created_date = DateTime.Now;
                    ca_Rank.created_ip = ip;
                    ca_Rank.created_token_id = tid;
                    db.ca_rank.Add(ca_Rank);
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ca_Rank }), domainurl + "ca_Rank/add_Rank", ip, tid, "Thêm mới Cấp bậc", 1, "Cấp bậc");

                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "ca_Rank/add_Rank", ip, tid, "Lỗi khi thêm Cấp bậc", 0, "Cấp bậc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "ca_Rank/add_Rank", ip, tid, "Lỗi khi thêm Cấp bậc", 0, "Cấp bậc  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_ca_rank([System.Web.Mvc.Bind(Include = "modified_by,modified_date,modified_ip,modified_token_id")] ca_rank ca_Rank)
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
                    ca_Rank.modified_by = uid;
                    ca_Rank.modified_date = DateTime.Now;
                    ca_Rank.modified_ip = ip;
                    ca_Rank.modified_token_id = tid;
                    db.Entry(ca_Rank).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ca_Rank }), domainurl + "ca_Rank/Update_ca_rank", ip, tid, "Cập nhật Cấp bậc", 1, "Cấp bậc");
                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "ca_rank/Update_ca_rank", ip, tid, "Lỗi khi cập nhật Cấp bậc", 0, "Cấp bậc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "ca_rank/Update_ca_rank", ip, tid, "Lỗi khi cập nhật Cấp bậc", 0, "Cấp bậc");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_ca_rank([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                    var das = await db.ca_rank.Where(a => id.Contains(a.rank_id)).ToListAsync();

                    if (das != null)
                    {
                        List<ca_rank> del = new List<ca_rank>();
                        foreach (var da in das)
                        {
                            del.Add(da);
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.ca_rank.RemoveRange(del);
                        foreach (var da in das)
                        {
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da }), domainurl + "ca_Rank/Delete_ca_rank", ip, tid, "Xóa Cấp bậc", 1, "Cấp bậc");
                            }
                            #endregion
                        }
                    }
                    await db.SaveChangesAsync();


                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new
                {
                    data = id,
                    contents
                }), domainurl + "ca_rank/Delete_ca_rank", ip, tid, "Lỗi khi xoá Cấp bậc", 0, "Cấp bậc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "ca_rank/Delete_ca_rank", ip, tid, "Lỗi khi xoá Cấp bậc", 0, "Cấp bậc");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }


        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_Status_Ca_Rank([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                    var das = db.ca_rank.FirstOrDefault(a => (a.rank_id == trangthai.IntID));
                    if (das != null)
                    {
                        das.modified_by = uid;
                        das.modified_date = DateTime.Now;
                        das.modified_ip = ip;
                        das.modified_token_id = tid;
                        das.status = !trangthai.BitTrangthai;
                        #region add cms_logs
                        if (helper.wlog)
                        {
                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = das }), domainurl + "ca_Rank/Update_Status_Ca_Rank", ip, tid, "Cập nhật trạng thái Cấp bậc", 1, "Cấp bậc");

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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "ca_rank/Update_Status_Ca_Rank", ip, tid, "Lỗi khi cập nhật trạng thái Cấp bậc", 0, "Cấp bậc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "ca_rank/Update_Status_Ca_Rank", ip, tid, "Lỗi khi cập nhật trạng thái Cấp bậc", 0, "Cấp bậc");
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
