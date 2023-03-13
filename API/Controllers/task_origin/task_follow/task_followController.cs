using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Models;
using Helper;
using API.Helper;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace API.Controllers
{
    public class task_followController : ApiController
    {
        string module_key = "M4";
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> addFollows()
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
            string temp1 = "";
            string temp2 = "";
            try
            {
                using (DBEntities db = new DBEntities())
                {

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Task_Files";
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
                        temp1 = provider.FormData.GetValues("task_follow").SingleOrDefault();
                        task_follow task_Follow = JsonConvert.DeserializeObject<task_follow>(temp1);
                        task_Follow.follow_id = helper.GenKey();
                        task_Follow.created_by = uid;
                        task_Follow.created_date = DateTime.Now;
                        task_Follow.created_ip = ip;
                        task_Follow.created_token_id = tid;
                        if (task_Follow.start_date <= DateTime.Now)
                        {
                            task_Follow.status = 1;
                        }
                        temp2 = provider.FormData.GetValues("task_follow_detail").SingleOrDefault();
                        db.task_follow.Add(task_Follow);
                        if (temp2 != null && temp2 != "")
                        {
                            List<task_follow_detail> task_Follow_Details = new List<task_follow_detail>();
                            List<task_follow_detail> task_follow_deltai = JsonConvert.DeserializeObject<List<task_follow_detail>>(temp2);
                            foreach (var de in task_follow_deltai)
                            {
                                de.follow_detail_id = helper.GenKey();
                                de.follow_id = task_Follow.follow_id;
                                de.created_by = uid;
                                de.created_date = DateTime.Now;
                                de.created_token_id = tid;
                                de.created_ip = ip;
                                task_Follow_Details.Add(de);
                            }
                            db.task_follow_detail.AddRange(task_Follow_Details);
                        }
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Checklists/addChecklists", ip, tid, "Lỗi khi thêm Checklists", 0, "Checklists");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Checklists/addChecklists", ip, tid, "Lỗi khi thêm Checklists", 0, "Checklists");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateFollow()
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
            string temp1 = "";
            string temp2 = "";
            try
            {
                using (DBEntities db = new DBEntities())
                {

                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Task_Files";
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
                        temp1 = provider.FormData.GetValues("task_follow").SingleOrDefault();
                        task_follow task_Follow = JsonConvert.DeserializeObject<task_follow>(temp1);
                        //task_Follow.follow_id = helper.GenKey();
                        task_Follow.modified_by = uid;
                        task_Follow.modified_date = DateTime.Now;
                        task_Follow.modified_ip = ip;
                        task_Follow.modified_token_id = tid;
               
                        if(task_Follow.start_date>=DateTime.Now)
                        {
                            task_Follow.status = 1;
                        }    
                        db.Entry(task_Follow).State = EntityState.Modified;
                        temp2 = provider.FormData.GetValues("task_follow_detail").SingleOrDefault();
                        if (temp2 != null && temp2 != "")
                        {
                            var delDetail= db.task_follow_detail.Where(x=>x.task_id==task_Follow.task_id).ToList();
                            db.task_follow_detail.RemoveRange(delDetail);
                            List<task_follow_detail> task_Follow_Details = new List<task_follow_detail>();
                            List<task_follow_detail> task_follow_deltai = JsonConvert.DeserializeObject<List<task_follow_detail>>(temp2);
                            foreach (var de in task_follow_deltai)
                            {
                                de.follow_detail_id = helper.GenKey();
                                de.follow_id = task_Follow.follow_id;
                                de.created_by = uid;
                                de.created_date = DateTime.Now;
                                de.created_token_id = tid;
                                de.created_ip = ip;
                                task_Follow_Details.Add(de);
                            }
                            db.task_follow_detail.AddRange(task_Follow_Details);
                        }
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Checklists/addChecklists", ip, tid, "Lỗi khi thêm Checklists", 0, "Checklists");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Checklists/addChecklists", ip, tid, "Lỗi khi thêm Checklists", 0, "Checklists");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
    }
}
