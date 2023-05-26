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


namespace API.Controllers.HRM.Review
{
    [Authorize(Roles = "login")]
    public class hrm_review_userController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_review_user()
        {
           
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdhrm_Review_user = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
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

                    string strPath = root + "/" + dvid + "/Review_user";
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
                        fdhrm_Review_user = provider.FormData.GetValues("hrm_review_user").SingleOrDefault();
                        hrm_review_user hrm_Review_user = JsonConvert.DeserializeObject<hrm_review_user>(fdhrm_Review_user);
                        var intw = int.Parse(dvid);
                
                        hrm_Review_user.organization_id = int.Parse(dvid);
                        hrm_Review_user.created_by = uid;
                        hrm_Review_user.created_date = DateTime.Now;
                        hrm_Review_user.created_ip = ip;
                        hrm_Review_user.created_token_id = tid;
                        hrm_Review_user.modified_by = uid;
                        hrm_Review_user.modified_date = DateTime.Now;
                        hrm_Review_user.modified_ip = ip;
                        hrm_Review_user.modified_token_id = tid;
                        db.hrm_review_user.Add(hrm_Review_user);
                        db.SaveChanges();
                        var liReviewImp = "";

                        liReviewImp = provider.FormData.GetValues("hrm_review_imp").SingleOrDefault();
                        List<hrm_review_imp> liDatas = JsonConvert.DeserializeObject<List<hrm_review_imp>>(liReviewImp);
                        foreach (var item in liDatas)
                        {
                       
                            item.organization_id = int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            item.review_user_id = hrm_Review_user.review_user_id;
                                db.hrm_review_imp.Add(item);
                                db.SaveChanges();
                            var liReviewImpChild = "";

                            liReviewImpChild = provider.FormData.GetValues("hrm_review_imp_child").SingleOrDefault();
                            List<hrm_review_imp_child> liDatasChild = JsonConvert.DeserializeObject<List<hrm_review_imp_child>>(liReviewImpChild);
                            foreach (var elem in liDatasChild.Where(x=>x.roman_order==item.roman_order))
                            {
                                elem.organization_id = int.Parse(dvid);
                                elem.created_by = uid;
                                elem.created_date = DateTime.Now;
                                elem.created_ip = ip;
                                elem.created_token_id = tid;
                                elem.review_user_id = hrm_Review_user.review_user_id;
                                elem.review_imp_id = item.review_imp_id;
                                db.hrm_review_imp_child.Add(elem);
                                db.SaveChanges();
                            }
                        }

                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Review_user", fileName);
                            fileInfo = new FileInfo(newFileName);
                            // if (fileInfo.Exists)
                            // {
                            //     fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            //     fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                            //     newFileName = Path.Combine(root + "/" + dvid + "/Review_user", fileName);
                            // }
                            newFileName = Path.Combine(root + "/" + dvid + "/Review_user",
                              helper.newFileName(fileInfo, root + "/" + dvid + "/Review_user", newFileName, 1, root, int.Parse(dvid)));
                            ffileData = fileData;
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }
                            hrm_file hrm_File = new hrm_file();
                            hrm_File.file_name = Path.GetFileName(newFileName);
                            hrm_File.key_id = hrm_Review_user.review_user_id.ToString();
                            hrm_File.file_path = "/Portals/" + dvid + "/Review_user/" + fileName;
                            hrm_File.file_type = helper.GetFileExtension(fileName);
                            var file_info = new FileInfo(strPath + "/" + fileName);
                            hrm_File.file_size = file_info.Length;
                            if (helper.IsImageFileName(newFileName))
                            {
                                hrm_File.is_image = true;
                            }
                            else
                            {
                                hrm_File.is_image = false;
                            }
                            hrm_File.is_type = 12;
                            hrm_File.status = true;
                            hrm_File.created_by = uid;
                            hrm_File.created_date = DateTime.Now;
                            hrm_File.organization_id = int.Parse(dvid);
                            hrm_File.created_ip = ip;
                            hrm_File.created_token_id = tid;
                            
                            db.hrm_file.Add(hrm_File);
                            db.SaveChanges();
                        }




                        #region add hrm_log
                        if (helper.wlog)
                        {
                            hrm_log log = new hrm_log();
                            log.title = "Thêm biểu mẫu đánh giá " + hrm_Review_user.profile_user_name;

                            log.log_module = "hrm_Review_user";
                            log.log_type = 0;
                            log.id_key = hrm_Review_user.review_user_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.hrm_log.Add(log);
                            db.SaveChanges();

                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_Review_user, contents }), domainurl + "hrm_review_user/Add_hrm_Review_user", ip, tid, "Lỗi khi thêm chiến dịch", 0, "chiến dịch");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_Review_user, contents }), domainurl + "hrm_review_user/Add_hrm_Review_user", ip, tid, "Lỗi khi thêm chiến dịch", 0, "chiến dịch  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_hrm_review_user()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdhrm_Review_user = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");

                    string strPath = root + "/" + dvid + "/Review_user";
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




                        var intw = int.Parse(dvid);



                        fdhrm_Review_user = provider.FormData.GetValues("hrm_review_user").SingleOrDefault();
                        hrm_review_user hrm_Review_user = JsonConvert.DeserializeObject<hrm_review_user>(fdhrm_Review_user);
                        hrm_Review_user.modified_by = uid;
                        hrm_Review_user.modified_date = DateTime.Now;
                        hrm_Review_user.modified_ip = ip;
                        hrm_Review_user.modified_token_id = tid;

                        db.Entry(hrm_Review_user).State = EntityState.Modified;

                        db.SaveChanges();
                        var liReviewImp = "";
                        liReviewImp = provider.FormData.GetValues("hrm_review_imp").SingleOrDefault();
                        List<hrm_review_imp> liDatas = JsonConvert.DeserializeObject<List<hrm_review_imp>>(liReviewImp);
                        foreach (var item in liDatas)
                        {
 
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();

                        }

                        var liReviewImpChild = "";


                        liReviewImpChild = provider.FormData.GetValues("hrm_review_imp_child").SingleOrDefault();
                        List<hrm_review_imp_child> liDatasChild = JsonConvert.DeserializeObject<List<hrm_review_imp_child>>(liReviewImpChild);
                        foreach (var item in liDatasChild)
                        {
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        var hrm_Files = "";
                        List<string> paths = new List<string>();
                        hrm_Files = provider.FormData.GetValues("hrm_files").SingleOrDefault();
                        List<hrm_file> hrm_File_S = JsonConvert.DeserializeObject<List<hrm_file>>(hrm_Files);
                        var id = hrm_Review_user.review_user_id.ToString();
                        var hrmfile_Delete = new List<hrm_file>();
                        var hrm_file_Olds = db.hrm_file.Where(s => s.is_type == 12 && s.key_id == id).ToArray<hrm_file>();
                        foreach (var item in hrm_file_Olds)
                        {
                            var check = false;
                            foreach (var element in hrm_File_S)
                            {
                                if (element.key_id == item.key_id && element.file_name == item.file_name)
                                    check = true;

                            }
                            if (check == false)
                            {
                                paths.Add(item.file_path);
                                hrmfile_Delete.Add(item);

                            }

                        }
                        db.hrm_file.RemoveRange(hrmfile_Delete);
                        db.SaveChanges();
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Review_user", fileName);
                            fileInfo = new FileInfo(newFileName);
                            // if (fileInfo.Exists)
                            // {
                            //     fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            //     fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;
                            //     newFileName = Path.Combine(root + "/" + dvid + "/Review_user", fileName);
                            // }
                            newFileName = Path.Combine(root + "/" + dvid + "/Review_user",
                            helper.newFileName(fileInfo, root + "/" + dvid + "/Review_user", newFileName, 1, root, int.Parse(dvid)));
                            ffileData = fileData;
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }

                            hrm_file hrm_File = new hrm_file();
                            hrm_File.key_id = hrm_Review_user.review_user_id.ToString();
                            hrm_File.file_name = Path.GetFileName(newFileName);
                            hrm_File.file_path = "/Portals/" + dvid + "/Review_user/" + fileName;
                            hrm_File.file_type = helper.GetFileExtension(fileName);
                            var file_info = new FileInfo(strPath + "/" + fileName);
                            hrm_File.file_size = file_info.Length;
                            if (helper.IsImageFileName(newFileName))
                            {
                                hrm_File.is_image = true;
                            }
                            else
                            {
                                hrm_File.is_image = false;
                            }
                            hrm_File.is_type = 12;
                            hrm_File.status = true;
                            hrm_File.created_by = uid;
                            hrm_File.created_date = DateTime.Now;
                            hrm_File.created_ip = ip; hrm_File.organization_id = int.Parse(dvid);
                            hrm_File.created_token_id = tid;
                       
                            db.hrm_file.Add(hrm_File);
                            db.SaveChanges();
                        }


                        #region add hrm_log
                        if (helper.wlog)
                        {
                            hrm_log log = new hrm_log();
                            log.title = "Sửa biểu mẫu đánh giá " + hrm_Review_user.profile_user_name;

                            log.log_module = "hrm_Review_user";
                            log.log_type = 1;
                            log.id_key = hrm_Review_user.review_user_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.hrm_log.Add(log);
                            db.SaveChanges();

                        }
                        #endregion
                        foreach (string strP in paths)
                        {

                            bool exists = File.Exists(root + "/" + dvid + "/Review_user/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(root + "/" + dvid + "/Review_user/" + Path.GetFileName(strP));
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_Review_user, contents }), domainurl + "hrm_review_user/Update_hrm_Review_user", ip, tid, "Lỗi khi cập nhật hrm_Review_user", 0, "hrm_Review_user");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_Review_user, contents }), domainurl + "hrm_review_user/Update_hrm_Review_user", ip, tid, "Lỗi khi cập nhật hrm_Review_user", 0, "hrm_Review_user");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_hrm_review_user([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.hrm_review_user.Where(a => id.Contains(a.review_user_id)).ToListAsync();
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<hrm_review_user> del = new List<hrm_review_user>();
                            foreach (var da in das)
                            {
                                del.Add(da);


                                var arr = new List<String>();
                                foreach (var item in id)
                                {
                                    arr.Add(item.ToString());
                                }
                                var das3 = await db.hrm_file.Where(a => arr.Contains(a.key_id) && a.is_type == 12).ToListAsync();
                                foreach (var item in das3)
                                {
                                    if (!string.IsNullOrWhiteSpace(item.file_path))
                                        paths.Add(item.file_path.Substring(8));
                                }
                                db.hrm_file.RemoveRange(das3);

                                #region add hrm_log
                                if (helper.wlog)
                                {

                                    hrm_log log = new hrm_log();
                                    log.title = "Xóa mẫu biểu đánh giá " + da.profile_user_name;
                                    log.log_module = "hrm_Review_user";
                                    log.log_type = 2;
                                    log.id_key = da.review_user_id.ToString();
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.hrm_log.Add(log);
                                    db.SaveChanges();
                                }
                                #endregion
                                foreach (string strP in paths)
                                {
                                    bool exists = File.Exists(root + "/" + dvid + "/Review_user/" + Path.GetFileName(strP));
                                    if (exists)
                                        System.IO.File.Delete(root + "/" + dvid + "/Review_user/" + Path.GetFileName(strP));
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_review_user.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_review_user/Delete_hrm_Review_user", ip, tid, "Lỗi khi xoá chiến dịch", 0, "hrm_Review_user");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_review_user/Delete_hrm_Review_user", ip, tid, "Lỗi khi xoá chiến dịch", 0, "hrm_Review_user");
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