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

namespace API.Controllers.HRM
{
    [Authorize(Roles = "login")]
    public class hrm_training_empsController : ApiController
    {

        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_training_emps()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdtraining_emps = "";
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

                    string strPath = root + "/" + dvid + "/Training";
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
                        fdtraining_emps = provider.FormData.GetValues("hrm_training_emps").SingleOrDefault();
                        hrm_training_emps training_emps = JsonConvert.DeserializeObject<hrm_training_emps>(fdtraining_emps);
                        var intw = int.Parse(dvid);
                        var checkBarcode = db.hrm_training_emps.Where(a => a.training_emps_code == training_emps.training_emps_code && a.organization_id == intw).FirstOrDefault();
                        if (checkBarcode != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã số đã tồn tại! Vui lòng nhập lại", err = "1" });
                        }
                        training_emps.organization_id =  int.Parse(dvid);
                        training_emps.created_by = uid;
                        training_emps.created_date = DateTime.Now;
                        training_emps.created_ip = ip;
                        training_emps.created_token_id = tid;
                        training_emps.modified_by = uid;
                        training_emps.modified_date = DateTime.Now;
                        training_emps.modified_ip = ip;
                        training_emps.modified_token_id = tid;
                        db.hrm_training_emps.Add(training_emps);
                        db.SaveChanges();
                        var hrm_users_trainings = "";
                        hrm_users_trainings = provider.FormData.GetValues("hrm_students").SingleOrDefault();
                        List<hrm_users_training> hrm_Users_Trainings = JsonConvert.DeserializeObject<List<hrm_users_training>>(hrm_users_trainings);
                        foreach (var item in hrm_Users_Trainings)
                        {
                            item.training_emps_id = training_emps.training_emps_id;
                            item.organization_id = int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;

                            db.hrm_users_training.Add(item);
                            db.SaveChanges();
                        }
                        var hrm_class_s = "";
                        hrm_class_s = provider.FormData.GetValues("hrm_schedule").SingleOrDefault();
                        List<hrm_class_schedule> hrm_Class_Schedules = JsonConvert.DeserializeObject<List<hrm_class_schedule>>(hrm_class_s);
                        foreach (var item in hrm_Class_Schedules)
                        {
                            item.training_emps_id = training_emps.training_emps_id;
                            item.organization_id =  int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_class_schedule.Add(item);
                            db.SaveChanges();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Training", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Training", fileName);
                            }
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
                            hrm_File.key_id = training_emps.training_emps_id.ToString();
                            hrm_File.file_path = "/Portals/" + dvid + "/Training/" + fileName;
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
                            hrm_File.is_type = 2;
                            hrm_File.status = true;
                            hrm_File.created_by = uid;
                            hrm_File.created_date = DateTime.Now;
                            hrm_File.organization_id = int.Parse(dvid);
                            hrm_File.created_ip = ip;
                            hrm_File.created_token_id = tid;
                            db.hrm_file.Add(hrm_File);

                        }




                        #region add hrm_log
                        if (helper.wlog)
                        {
                            hrm_log log = new hrm_log();
                            log.title = "Thêm thông tin đào tạo " + training_emps.training_emps_name;

                            log.log_module = "training_emps";
                            log.log_type = 0;
                            log.id_key = training_emps.training_emps_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtraining_emps, contents }), domainurl + "hrm_training_emps/Add_training_emps", ip, tid, "Lỗi khi thêm đào tạo", 0, "đào tạo");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtraining_emps, contents }), domainurl + "hrm_training_emps/Add_training_emps", ip, tid, "Lỗi khi thêm đào tạo", 0, "đào tạo  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_hrm_training_emps()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdtraining_emps = "";
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

                    string strPath = root + "/" + dvid + "/Training";
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



                        fdtraining_emps = provider.FormData.GetValues("hrm_training_emps").SingleOrDefault();
                        hrm_training_emps training_emps = JsonConvert.DeserializeObject<hrm_training_emps>(fdtraining_emps);
                        var checkBarcode = db.hrm_training_emps.Where(a => a.training_emps_code == training_emps.training_emps_code && a.training_emps_id
                        != training_emps.training_emps_id && training_emps.organization_id== intw).FirstOrDefault();
                        if (checkBarcode != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã số đã tồn tại! Vui lòng nhập lại", err = "1" });
                        }
                      

                        training_emps.modified_by = uid;
                        training_emps.modified_date = DateTime.Now;
                        training_emps.modified_ip = ip;
                        training_emps.modified_token_id = tid;
                       
                        db.Entry(training_emps).State = EntityState.Modified;
                  
                        db.SaveChanges();
                        var hrm_users_trainings_old = db.hrm_users_training.Where(s => s.training_emps_id == training_emps.training_emps_id).ToArray<hrm_users_training>();
                        if (hrm_users_trainings_old.Length > 0)
                        {
                            db.hrm_users_training.RemoveRange(hrm_users_trainings_old);
                            db.SaveChanges();
                        }
                        
                        var hrm_class_schedules_old = db.hrm_class_schedule.Where(s => s.training_emps_id == training_emps.training_emps_id).ToArray<hrm_class_schedule>();
                        if (hrm_class_schedules_old.Length > 0)
                        {
                            db.hrm_class_schedule.RemoveRange(hrm_class_schedules_old);
                            db.SaveChanges();
                        }
                        var hrm_users_trainings = "";

                     

                        hrm_users_trainings = provider.FormData.GetValues("hrm_students").SingleOrDefault();
                        List<hrm_users_training> hrm_Users_Trainings = JsonConvert.DeserializeObject<List<hrm_users_training>>(hrm_users_trainings);
                        foreach (var item in hrm_Users_Trainings)
                        {
                            item.training_emps_id = training_emps.training_emps_id;
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_users_training.Add(item);
                            db.SaveChanges();
                        }
                        var hrm_class_s = "";
                        hrm_class_s = provider.FormData.GetValues("hrm_schedule").SingleOrDefault();
                        List<hrm_class_schedule> hrm_Class_Schedules = JsonConvert.DeserializeObject<List<hrm_class_schedule>>(hrm_class_s);
                        foreach (var item in hrm_Class_Schedules)
                        {
                            
                            item.training_emps_id = training_emps.training_emps_id;
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_class_schedule.Add(item);
                            db.SaveChanges();
                        }
                        var hrm_Files = "";
                        List<string> paths = new List<string>();
                        hrm_Files = provider.FormData.GetValues("hrm_files").SingleOrDefault();
                        List<hrm_file> hrm_File_S = JsonConvert.DeserializeObject<List<hrm_file>>(hrm_Files);
                        var id = training_emps.training_emps_id.ToString();
                        var hrmfile_Delete = new List<hrm_file>();
                        var hrm_file_Olds = db.hrm_file.Where(s =>   s.is_type==2 && s.key_id == id).ToArray<hrm_file>();
                        foreach (var item in hrm_file_Olds)
                        {
                            var check = false;
                            foreach (var element in hrm_File_S)
                            {
                                if (element.key_id == item.key_id && element.file_name==item.file_name)
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Training", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Training", fileName);
                            }
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
                            hrm_File.key_id = training_emps.training_emps_id.ToString();
                            hrm_File.file_name = Path.GetFileName(newFileName);
                            hrm_File.file_path = "/Portals/" + dvid + "/Training/" + fileName;
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
                            hrm_File.is_type = 2;
                            hrm_File.status = true;
                            hrm_File.created_by = uid;
                            hrm_File.created_date = DateTime.Now;
                            hrm_File.created_ip = ip; hrm_File.organization_id = int.Parse(dvid);
                            hrm_File.created_token_id = tid;
                            db.hrm_file.Add(hrm_File);

                        }
                     

                        #region add hrm_log
                        if (helper.wlog)
                        {
                            hrm_log log = new hrm_log();
                            log.title = "Sửa thông tin đào tạo " + training_emps.training_emps_name;

                            log.log_module = "training_emps";
                            log.log_type = 0;
                            log.id_key = training_emps.training_emps_id.ToString();
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

                            bool exists = File.Exists(root + "/" + dvid + "/Training/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(root + "/" + dvid + "/Training/" + Path.GetFileName(strP));
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtraining_emps, contents }), domainurl + "hrm_training_emps/Update_training_emps", ip, tid, "Lỗi khi cập nhật training_emps", 0, "training_emps");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdtraining_emps, contents }), domainurl + "hrm_training_emps/Update_training_emps", ip, tid, "Lỗi khi cập nhật training_emps", 0, "training_emps");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_hrm_training_emps([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.hrm_training_emps.Where(a => id.Contains(a.training_emps_id)).ToListAsync();
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<hrm_training_emps> del = new List<hrm_training_emps>();
                            foreach (var da in das)
                            {
                                del.Add(da);

                                var das1 = await db.hrm_users_training.Where(a => id.Contains(a.training_emps_id)).ToListAsync();
                                db.hrm_users_training.RemoveRange(das1);
                                var das2 = await db.hrm_class_schedule.Where(a => id.Contains(a.training_emps_id)).ToListAsync();
                                db.hrm_class_schedule.RemoveRange(das2);
                                var arr = new List<String>();
                                foreach (var item in id)
                                {
                                    arr.Add(item.ToString());
                                }
                                var das3 = await db.hrm_file.Where(a => arr.Contains(a.key_id )  && a.is_type==2 ).ToListAsync();


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
                                    log.title = "Xóa đào tạo " + da.training_emps_name;

                                    log.log_module = "training_emps";
                                    log.log_type = 2;
                                    log.id_key = da.training_emps_id.ToString();
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

                                    bool exists = File.Exists(root + "/" + dvid + "/Training/" + Path.GetFileName(strP));
                                    if (exists)
                                        System.IO.File.Delete(root + "/" + dvid + "/Training/" + Path.GetFileName(strP));
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_training_emps.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_training_emps/Delete_training_emps", ip, tid, "Lỗi khi xoá đào tạo", 0, "training_emps");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_training_emps/Delete_training_emps", ip, tid, "Lỗi khi xoá đào tạo", 0, "training_emps");
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


        [HttpPut]
        public async Task<HttpResponseMessage> update_s_hrm_training_emps([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var int_id = int.Parse(trangthai.IntID.ToString());
                        var das = db.hrm_training_emps.Where(a => (a.training_emps_id == int_id)).FirstOrDefault<hrm_training_emps>();
                        if (das != null)
                        {
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            das.status = trangthai.IntTrangthai;


                            #region add hrm_log
                            if (helper.wlog)
                            {

                                hrm_log log = new hrm_log();
                                log.title = "Sửa đào tạo " + das.training_emps_name;

                                log.log_module = "training_emps";
                                log.log_type = 1;
                                log.id_key = das.training_emps_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.hrm_log.Add(log);
                                db.SaveChanges();


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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_training_emps/Update_Trangthaitraining_emps", ip, tid, "Lỗi khi cập nhật trạng thái đào tạo", 0, "hrm_training_emps");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_training_emps/Update_Trangthaitraining_emps", ip, tid, "Lỗi khi cập nhật trạng thái đào tạo", 0, "hrm_training_emps");
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