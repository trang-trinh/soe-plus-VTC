using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Controllers.HRM.Declare
{
    [Authorize(Roles = "login")]
    public class hrm_review_form_usersController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_review_form_users()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
      
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string fddeclare = "";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

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
                  
                        string fdReport = "";
                        fddeclare = provider.FormData.GetValues("hrm_review_form_users").SingleOrDefault();
                        fdReport = provider.FormData.GetValues("report").SingleOrDefault();
                        List< hrm_review_form_users> liDatas = JsonConvert.DeserializeObject<List<hrm_review_form_users>>(fddeclare);
                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                        hrm_declare_review_form hrm_Declare_Review= JsonConvert.DeserializeObject<hrm_declare_review_form>(fdReport);
                        foreach (var item in liDatas)
                        {

                            var dataCk =   db.hrm_review_form_users.Where(a => a.profile_id==item.profile_id).FirstOrDefault();
                            if(dataCk == null)
                            { 
                            item.review_form_id = hrm_Declare_Review.review_form_id;
                            db.hrm_review_form_users.Add(item);
                            db.SaveChanges();
                            }
                            else
                            {
                                item.review_form_id = hrm_Declare_Review.review_form_id;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                       
 
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fddeclare, contents }), domainurl + "hrm_review_form_users/Add_declare", ip, tid, "Lỗi khi thêm mẫu phiếu lương", 0, "mẫu phiếu lương");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fddeclare, contents }), domainurl + "hrm_review_form_users/Add_declare", ip, tid, "Lỗi khi thêm mẫu phiếu lương", 0, "mẫu phiếu lương  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
  
        [HttpDelete]
        public async Task<HttpResponseMessage> delete_hrm_review_form_users([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;

            try
            {
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.hrm_review_form_users.Where(a => id.Contains(a.profile_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<hrm_review_form_users> del = new List<hrm_review_form_users>();
                            foreach (var da in das)
                            {

                                del.Add(da);

                             
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_review_form_users.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_review_form_users/Delete_declare", ip, tid, "Lỗi khi xoá tem", 0, "declare");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_review_form_users/Delete_declare", ip, tid, "Lỗi khi xoá tem", 0, "declare");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
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