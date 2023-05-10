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

namespace API.Controllers.Tasks.Config
{
    [Authorize(Roles = "login")]
    public class task_review_member_configController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_config()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
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
                        ///Get data from FrontEnd
                        string tempID = provider.FormData.GetValues("config").SingleOrDefault();
                        task_review_member_config listprocess_id = JsonConvert.DeserializeObject<task_review_member_config>(tempID);
                        listprocess_id.created_by = uid;
                        listprocess_id.created_date = DateTime.Now;
                        listprocess_id.created_ip = ip; ;
                        listprocess_id.created_token_id = tid;
                        db.task_review_member_config.Add(listprocess_id);
                        db.SaveChanges();
                        #region add tasklog
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = listprocess_id }), domainurl + "/task_review_member_config/add_Config", ip, tid, "Thêm mới thiết lập đánh giá thành viên công việc", 1, "Công việc");

                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "/task_review_member_config/add_Config", ip, tid, "Lỗi khi thêm mới thiết lập đánh giá thành viên công việc", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "/task_review_member_config/add_Config", ip, tid, "Lỗi khi thêm mới thiết lập đánh giá thành viên công việc", 0, "Công việc");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_config()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
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
                        ///Get data from FrontEnd
                        string tempID = provider.FormData.GetValues("config").SingleOrDefault();
                        task_review_member_config listprocess_id = JsonConvert.DeserializeObject<task_review_member_config>(tempID);
                        listprocess_id.modified_by = uid;
                        listprocess_id.modified_date=DateTime.Now;
                        listprocess_id.modified_ip=ip; ;
                        listprocess_id.modified_token_id=tid; 
                        db.Entry(listprocess_id).State=EntityState.Modified;
                        db.SaveChanges();   
                        #region add tasklog
                        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = listprocess_id }), domainurl + "/task_review_member_config/Update_config", ip, tid, "Cập nhật thiết lập đánh giá thành viên công việc", 1, "Công việc");
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "/task_review_member_config/Update_config", ip, tid, "Lỗi khi cập nhật thiết lập đánh giá thành viên công việc", 0, "Công việc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "/task_review_member_config/Update_config", ip, tid, "Lỗi khi cập nhật thiết lập đánh giá thành viên công việc", 0, "Công việc");
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
