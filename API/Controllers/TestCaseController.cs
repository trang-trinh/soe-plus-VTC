using API.Helper;
using API.Models;
using Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Controllers
{
    [Authorize(Roles = "login")]
    public class TestCaseController : ApiController
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

        #region Case

        [HttpPost]
        public async Task<HttpResponseMessage> Add_Case([System.Web.Mvc.Bind(Include = "status,test_number,created_by,created_date,created_ip,created_token_id,modified_by,modified_date,modified_ip,modified_token_id")] test_case model)
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
                    model.status = true;
                    model.created_date = DateTime.Now;
                    model.created_by = uid;
                    model.created_token_id = tid;
                    model.created_ip = ip;
                    model.modified_date = DateTime.Now;
                    model.modified_by = uid;
                    model.modified_token_id = tid;
                    model.modified_ip = ip;
                    model.test_number = 0;
                    db.test_case.Add(model);
                    #region  add Log
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents = "" }), domainurl + "TestCase/Add_Case", ip, tid, "Thêm mới Test Case " + model.case_name, 1, "TestCase");
                    #endregion
                    await db.SaveChangesAsync();
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    //string strPath = root + "/TestCase/" + model.case_id;
                    string strPath = Path.Combine(root, "TestCase", Path.GetFileName(model.case_id.ToString()));
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", id = model.case_id });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "TestCase/Add_Case", ip, tid, "Lỗi khi thêm Test Case", 0, "TestCase");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "TestCase/Add_Cases", ip, tid, "Lỗi khi thêm Test Case", 0, "TestCase");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Case([System.Web.Mvc.Bind(Include = "modified_by,modified_date,modified_ip,modified_token_id")] test_case model)
        {
            var identity = User.Identity as ClaimsIdentity;

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
                    model.modified_ip = ip;
                    model.modified_by = uid;
                    model.modified_date = DateTime.Now;
                    model.modified_token_id = tid;
                    db.Entry(model).State = EntityState.Modified;
                    #region  add Log
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents = "" }), domainurl + "TestCase/Update_Case", ip, tid, "Chỉnh sửa Test Case " + model.case_name, 1, "TestCase");
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "TestCase/Update_Case", ip, tid, "Lỗi khi cập nhật Test Case", 0, "TestCase");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "TestCase/Update_Case", ip, tid, "Lỗi khi cập nhật Test Case", 0, "TestCase");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Del_Case([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;

            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
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
                    List<string> paths = new List<string>();
                    var das = await db.test_case.Where(a => ids.Contains(a.case_id)).ToListAsync();
                    if (das != null)
                    {
                        List<test_case> del = new List<test_case>();
                        foreach (var da in das)
                        {
                            if (da.created_by == uid || ad)
                            {
                                del.Add(da);
                                paths.Add(HttpContext.Current.Server.MapPath("~/Portals/TestCase/") + da.case_id);
                                #region  add Log
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), domainurl + "TestCase/Del_Case", ip, tid, "Xoá Test Case " + da.case_name, 1, "TestCase");
                                #endregion
                            }
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa test case này." });
                        }
                        db.test_case.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    foreach (string strPath in paths)
                    {
                        string pathDel = HttpContext.Current.Server.MapPath("~/Portals/TestCase/") + Path.GetFileName(strPath.TrimEnd('/'));
                        bool exists = Directory.Exists(pathDel);
                        if (exists)
                            Directory.Delete(pathDel, true);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "TestCase/Del_Case", ip, tid, "Lỗi khi xoá Test Case", 0, "TestCase");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "TestCase/Del_Case", ip, tid, "Lỗi khi xoá Test Case", 0, "TestCase");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_statusCase([System.Web.Mvc.Bind(Include = "ids,tts")][FromBody] JObject data)
        {
            List<int> ids = data["ids"].ToObject<List<int>>();
            List<bool> tts = data["tts"].ToObject<List<bool>>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;

            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
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
                    var das = await db.test_case.Where(a => ids.Contains(a.case_id)).ToListAsync();
                    if (das != null)
                    {
                        List<test_case> del = new List<test_case>();
                        for (int i = 0; i < das.Count; i++)
                        {
                            var da = das[i];
                            if (ad)
                            {
                                #region  add Log
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "TestCase/Update_statusCase", ip, tid, "Cập nhật trạng thái Test Case " + da.case_name, 1, "TestCase");
                                #endregion
                                da.status = tts[i];
                            }
                        }
                        await db.SaveChangesAsync();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "TestCase/Update_statusCase", ip, tid, "Lỗi khi cập nhật trạng thái Test Case", 0, "TestCase");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "TestCase/Update_statusCases", ip, tid, "Lỗi khi cập nhật trạng thái Test Case", 0, "TestCase");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }
        #endregion

        #region Lần Test

        [HttpPost]
        public async Task<HttpResponseMessage> Add_TestLan([System.Web.Mvc.Bind(Include = "created_by,created_date,created_ip,created_token_id,modified_by,modified_date,modified_ip,modified_token_id")] test_times model)
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
                    model.created_date = DateTime.Now;
                    model.created_by = uid;
                    model.created_token_id = tid;
                    model.created_ip = ip;
                    model.modified_date = DateTime.Now;
                    model.modified_by = uid;
                    model.modified_token_id = tid;
                    model.modified_ip = ip;
                    db.test_times.Add(model);
                    #region  add Log
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents = "" }), domainurl + "TestCase/Add_TestLan", ip, tid, "Thêm mới Lần Test (" + model.date_start.Value.ToString("dd/MM/yyyy") + ")", 1, "TestLan");
                    #endregion
                    await db.SaveChangesAsync();
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/TestCase/" + Path.GetFileName(model.case_id.ToString()) + "/" + Path.GetFileName(model.times_id.ToString());
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", id = model.times_id });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "TestCase/Add_TestLan", ip, tid, "Lỗi khi thêm Lần Test (" + model.date_start.Value.ToString("dd/MM/yyyy") + ")", 0, "TestLan");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "TestCase/Add_Cases", ip, tid, "Lỗi khi thêm Lần Test (" + model.date_start.Value.ToString("dd/MM/yyyy") + ")", 0, "TestLan");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_TestLan([System.Web.Mvc.Bind(Include = "modified_by,modified_date,modified_ip,modified_token_id")] test_times model)
        {
            var identity = User.Identity as ClaimsIdentity;

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
                    model.modified_ip = ip;
                    model.modified_by = uid;
                    model.modified_date = DateTime.Now;
                    model.modified_token_id = tid;
                    db.Entry(model).State = EntityState.Modified;
                    #region  add Log
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents = "" }), domainurl + "TestCase/Update_TestLan", ip, tid, "Chỉnh sửa  Lần Test (" + model.date_start.Value.ToString("dd/MM/yyyy") + ")", 1, "TestLan");
                    #endregion
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "TestCase/Update_TestLan", ip, tid, "Lỗi khi cập nhật  Lần Test (" + model.date_start.Value.ToString("dd/MM/yyyy") + ")", 0, "TestLan");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "TestCase/Update_TestLan", ip, tid, "Lỗi khi cập nhật  Lần Test (" + model.date_start.Value.ToString("dd/MM/yyyy") + ")", 0, "TestLan");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Del_TestLan([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;

            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
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
                    //List<string> paths = new List<string>();
                    List<test_times> paths = new List<test_times>();
                    var das = await db.test_times.Where(a => ids.Contains(a.case_id)).ToListAsync();
                    if (das != null)
                    {
                        List<test_times> del = new List<test_times>();
                        foreach (var da in das)
                        {
                            if (da.modified_by == uid || ad)
                            {
                                del.Add(da);
                                //paths.Add(HttpContext.Current.Server.MapPath("~/Portals/TestCase/") + da.case_id + "/" + da.times_id);
                                paths.Add(da);
                                #region  add Log
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = da, contents = "" }), domainurl + "TestCase/Del_TestLan", ip, tid, "Xoá Lần Test (" + da.date_start.Value.ToString("dd/MM/yyyy") + ")", 1, "TestLan");
                                #endregion
                            }
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa lần Test này." });
                        }
                        db.test_times.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    foreach (var strPath in paths)
                    {
                        string pathDel = HttpContext.Current.Server.MapPath("~/Portals/TestCase/") + Path.GetFileName(strPath.case_id.ToString()) + "/" + Path.GetFileName(strPath.times_id.ToString());
                        bool exists = Directory.Exists(pathDel);
                        if (exists)
                            Directory.Delete(pathDel, true);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "TestCase/Del_TestLan", ip, tid, "Lỗi khi xoá lần Test", 0, "TestLan");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "TestCase/Del_TestLan", ip, tid, "Lỗi khi xoá lần Test", 0, "TestLan");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }
        #endregion

        #region Step
        [HttpPost]
        public async Task<HttpResponseMessage> Add_TestStep()
        {
            var identity = User.Identity as ClaimsIdentity;

            IEnumerable<Claim> claims = identity.Claims;
            List<test_step> models = new List<test_step>();
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
                string root = HttpContext.Current.Server.MapPath("~/Portals");
                var provider = new MultipartFormDataStreamProvider(root);
                var task = Request.Content.ReadAsMultipartAsync(provider).
                   ContinueWith<HttpResponseMessage>(t =>
                   {
                       if (t.IsFaulted || t.IsCanceled)
                       {
                           Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                       }
                       var fdmodel = provider.FormData.GetValues("models").SingleOrDefault();
                       models = JsonConvert.DeserializeObject<List<test_step>>(fdmodel);
                       // This illustrates how to get thefile names.
                       string newFileName = "";
                       for (int i = 0; i <= provider.FileData.Count - 1; i++)
                       {
                           MultipartFileData fileData = provider.FileData[i];
                           var model = models[i];
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
                           newFileName = Path.Combine(root + "/TestCase/" + model.case_id + "/" + model.times_id, fileName);
                           var fileInfo = new FileInfo(newFileName);
                           if (fileInfo.Exists)
                           {
                               fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                               fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;

                               newFileName = Path.Combine(root + "/TestCase/" + model.case_id + "/" + model.times_id, fileName);
                           }
                           model.image = root + "/TestCase/" + model.case_id + "/" + model.times_id + "/" + fileName;
                           File.Move(fileData.LocalFileName, newFileName);
                           helper.ResizeImage(newFileName, 640, 640, 90);
                           model.created_date = DateTime.Now;
                           model.created_by = uid;
                           model.created_token_id = tid;
                           model.created_ip = ip;
                           model.modified_date = DateTime.Now;
                           model.modified_by = uid;
                           model.modified_token_id = tid;
                           model.modified_ip = ip;
                       }
                       using (DBEntities db = new DBEntities())
                       {
                           var lan = db.test_times.Find(models.FirstOrDefault().times_id);
                           var ca = db.test_case.Find(models.FirstOrDefault().case_id);
                           if (lan != null)
                           {
                               lan.is_pass = models.Count(a => a.is_pass == false) == 0;
                               lan.is_pass_step = models.Count(a => a.is_pass == true);
                           }
                           if (ca != null)
                           {
                               ca.is_pass = models.Count(a => a.is_pass == false) == 0;
                               ca.is_pass_step = models.Count(a => a.is_pass == true);
                           }
                           db.test_step.AddRange(models);
                           db.SaveChanges();
                       }
                       return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                   });
                return await task;
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = models, contents }), domainurl + "TestCase/Add_TestStep", ip, tid, "Lỗi khi thêm Bước Test", 0, "TestStep");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = models, contents }), domainurl + "TestCase/Add_TestStep", ip, tid, "Lỗi khi thêm Bước Test", 0, "TestStep");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_TestAuto()
        {
            var identity = User.Identity as ClaimsIdentity;

            IEnumerable<Claim> claims = identity.Claims;
            List<test_step> models = new List<test_step>();
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
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                string root = HttpContext.Current.Server.MapPath("~/Portals");
                var provider = new MultipartFormDataStreamProvider(root);
                var task = Request.Content.ReadAsMultipartAsync(provider).
                   ContinueWith<HttpResponseMessage>(t =>
                   {
                       if (t.IsFaulted || t.IsCanceled)
                       {
                           Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                       }
                       var fdmodel = provider.FormData.GetValues("Case").SingleOrDefault();
                       var mcase = JsonConvert.DeserializeObject<test_case>(fdmodel);
                       fdmodel = provider.FormData.GetValues("models").SingleOrDefault();
                       models = JsonConvert.DeserializeObject<List<test_step>>(fdmodel);
                       // This illustrates how to get thefile names.
                       string newFileName = "";
                       using (DBEntities db = new DBEntities())
                       {
                           var dbCase = db.test_case.FirstOrDefault(a => a.case_name == mcase.case_name && a.project_id == mcase.project_id && a.file_test == mcase.file_test && a.is_url == mcase.is_url);
                           if (dbCase == null)
                           {
                               dbCase = new test_case();
                               dbCase.case_name = mcase.case_name;
                               dbCase.project_id = mcase.project_id;
                               dbCase.work_id = mcase.work_id;
                               dbCase.keywords = mcase.keywords;
                               dbCase.des = mcase.des;
                               dbCase.status = true;
                               dbCase.is_order = db.test_case.Count() + 1;
                               dbCase.created_date = DateTime.Now;
                               dbCase.created_by = uid;
                               dbCase.created_ip = ip;
                               dbCase.created_token_id = tid;
                               dbCase.test_number = 1;
                               dbCase.file_test = mcase.file_test;
                               dbCase.code_test = mcase.code_test;
                               dbCase.date_test = DateTime.Now;
                               dbCase.is_url = mcase.is_url;
                               db.test_case.Add(dbCase);
                               db.SaveChanges();
                           }
                           test_times dbLan = new test_times();
                           dbLan.case_id = dbCase.case_id;
                           dbLan.des = mcase.des;
                           dbLan.modified_date = DateTime.Now;
                           dbLan.modified_by = uid;
                           dbLan.modified_ip = ip;
                           dbLan.modified_token_id = tid;
                           dbLan.date_start = models.First().date_start;
                           dbLan.date_end = models.Last().date_end;
                           db.test_times.Add(dbLan);
                           db.SaveChanges();
                           foreach (var model in models)
                           {
                               model.case_id = dbCase.case_id;
                               model.times_id = dbLan.times_id;
                               model.modified_date = DateTime.Now;
                               model.modified_by = uid;
                               model.modified_token_id = tid;
                               model.modified_ip = ip;
                           }
                           for (int i = 0; i <= provider.FileData.Count - 1; i++)
                           {
                               MultipartFileData fileData = provider.FileData[i];
                               var model = models.FirstOrDefault(x => fileData.LocalFileName.Contains("_" + x.is_order + ".png"));
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
                               newFileName = Path.Combine(root + "/TestCase/" + model.case_id + "/" + model.times_id, fileName);
                               var fileInfo = new FileInfo(newFileName);
                               if (fileInfo.Exists)
                               {
                                   fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                   fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;

                                   newFileName = Path.Combine(root + "/TestCase/" + model.case_id + "/" + model.times_id, fileName);
                               }
                               model.image = root + "/TestCase/" + model.case_id + "/" + model.times_id + "/" + fileName;
                               File.Move(fileData.LocalFileName, newFileName);
                               helper.ResizeImage(newFileName, 640, 640, 90);
                           }

                           dbLan.is_pass = models.Count(a => a.is_pass == false) == 0;
                           dbLan.is_pass_step = models.Count(a => a.is_pass == true);

                           dbCase.is_pass = models.Count(a => a.is_pass == false) == 0;
                           dbCase.is_pass_step = models.Count(a => a.is_pass == true);

                           db.test_step.AddRange(models);
                           db.SaveChanges();
                       }
                       return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                   });
                return await task;
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = models, contents }), domainurl + "TestCase/Add_TestAuto", ip, tid, "Lỗi khi gửi Test Auto", 0, "TestAuto");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = models, contents }), domainurl + "TestCase/Add_TestAuto", ip, tid, "Lỗi khi gửi Test Auto", 0, "TestAuto");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_TestAuto64([System.Web.Mvc.Bind(Include = "Case,models")][FromBody] JObject data)
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
                test_case Case = data["Case"].ToObject<test_case>();
                List<test_step> models = data["models"].ToObject<List<test_step>>();
                string root = HttpContext.Current.Server.MapPath("~/");
                string newFileName = "";
                using (DBEntities db = new DBEntities())
                {
                    var dbCase = db.test_case.FirstOrDefault(a => a.case_name == Case.case_name && a.project_id == Case.project_id && a.file_test == Case.file_test && a.is_url == Case.is_url);
                    if (dbCase == null)
                    {
                        dbCase = new test_case();
                        dbCase.case_name = Case.case_name;
                        dbCase.project_id = Case.project_id;
                        dbCase.work_id = Case.work_id;
                        dbCase.keywords = Case.keywords;
                        dbCase.des = Case.des;
                        dbCase.status = true;
                        dbCase.is_order = db.test_case.Count() + 1;
                        dbCase.created_date = DateTime.Now;
                        dbCase.created_by = uid;
                        dbCase.created_ip = ip;
                        dbCase.created_token_id = tid;
                        dbCase.modified_date = DateTime.Now;
                        dbCase.modified_by = uid;
                        dbCase.modified_ip = ip;
                        dbCase.modified_token_id = tid;
                        dbCase.test_number = 1;
                        dbCase.file_test = Case.file_test;
                        dbCase.date_test = DateTime.Now;
                        dbCase.is_url = Case.is_url;
                        db.test_case.Add(dbCase);
                        db.SaveChanges();
                    }
                    dbCase.date_test = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(Case.code_test))
                    {
                        File.WriteAllBytes(root + "/Portals/Python/" + Path.GetFileName(dbCase.file_test), Convert.FromBase64String(Case.code_test));
                        dbCase.code_test = File.ReadAllText(root + "/Portals/Python/" + Path.GetFileName(dbCase.file_test));
                    }
                    test_times dbLan = new test_times();
                    dbLan.case_id = dbCase.case_id;
                    dbLan.des = Case.des;
                    dbLan.modified_date = DateTime.Now;
                    dbLan.modified_by = uid;
                    dbLan.modified_ip = ip;
                    dbLan.modified_token_id = tid;
                    dbLan.date_start = models.First().date_start;
                    dbLan.date_end = models.Last().date_end;
                    db.test_times.Add(dbLan);
                    db.SaveChanges();
                    foreach (var model in models)
                    {
                        model.case_id = dbCase.case_id;
                        model.times_id = dbLan.times_id;
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_token_id = tid;
                        model.modified_ip = ip;
                        if (!string.IsNullOrWhiteSpace(model.image))
                        {
                            if (!Directory.Exists(root + "/Portals/TestCase/" + Path.GetFileName(model.case_id.ToString()) + "/" + Path.GetFileName(model.times_id.ToString())))
                            {
                                Directory.CreateDirectory(root + "/Portals/TestCase/" + Path.GetFileName(model.case_id.ToString()));
                                Directory.CreateDirectory(root + "/Portals/TestCase/" + Path.GetFileName(model.case_id.ToString()) + "/" + Path.GetFileName(model.times_id.ToString()));
                            }
                            File.WriteAllBytes(root + "/Portals/TestCase/" + Path.GetFileName(model.case_id.ToString()) + "/" + Path.GetFileName(model.times_id.ToString()) + "/" + Path.GetFileName(model.step_id.ToString()) + ".png", Convert.FromBase64String(model.image));
                            model.image = "/Portals/TestCase/" + model.case_id + "/" + model.times_id + "/" + model.step_id + ".png";
                            helper.ResizeImage(newFileName, 1920, 1080, 90);
                        }
                    }
                    dbLan.is_pass = models.Count(a => a.is_pass == false) == 0;
                    dbLan.is_pass_step = models.Count(a => a.is_pass == true);
                    if (dbLan.is_pass == true)
                        dbCase.is_pass_step = (dbCase.is_pass_step ?? 0) + 1;
                    dbCase.test_number = (dbCase.test_number ?? 0) + 1;
                    dbCase.is_pass = models.Count(a => a.is_pass == false) == 0;
                    dbCase.is_pass_step = models.Count(a => a.is_pass == true);

                    db.test_step.AddRange(models);
                    await db.SaveChangesAsync();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = data, contents }), domainurl + "TestCase/Add_TestAuto64", ip, tid, "Lỗi khi gửi Test Auto", 0, "TestAuto");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = data, contents }), domainurl + "TestCase/Add_TestAuto64", ip, tid, "Lỗi khi gửi Test Auto", 0, "TestAuto");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        #endregion
    }
}