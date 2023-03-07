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


namespace API.Controllers.HRM.Campaign
{
    [Authorize(Roles = "login")]
    public class hrm_candidateController : ApiController
    {

        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_candidate()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdcandidate = "";
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

                    string strPath = root + "/" + dvid + "/Candidate";
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
                        fdcandidate = provider.FormData.GetValues("hrm_candidate").SingleOrDefault();
                        hrm_candidate candidate = JsonConvert.DeserializeObject<hrm_candidate>(fdcandidate);
                        var intw = int.Parse(dvid);
                        var checkBarcode = db.hrm_candidate.Where(a => a.candidate_code == candidate.candidate_code && a.organization_id == intw).FirstOrDefault();
                        if (checkBarcode != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã ứng viên đã tồn tại! Vui lòng nhập lại", err = "1" });
                        }
                        candidate.organization_id = super ? 0 : int.Parse(dvid);
                        candidate.created_by = uid;
                        candidate.created_date = DateTime.Now;
                        candidate.created_ip = ip;
                        candidate.created_token_id = tid;
                        candidate.modified_by = uid;
                        candidate.modified_date = DateTime.Now;
                        candidate.modified_ip = ip;
                        candidate.modified_token_id = tid;
                        db.hrm_candidate.Add(candidate);
                        db.SaveChanges();
                        var hrm_candidate_family = "";
                        hrm_candidate_family = provider.FormData.GetValues("hrm_candidate_family").SingleOrDefault();
                        List<hrm_candidate_family> hrm_Users_Trainings = JsonConvert.DeserializeObject<List<hrm_candidate_family>>(hrm_candidate_family);
                        foreach (var item in hrm_Users_Trainings)
                        {
                            item.candidate_id = candidate.candidate_id;
                            item.organization_id = super ? 0 : int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;

                            db.hrm_candidate_family.Add(item);
                            db.SaveChanges();
                        }
                        var hrm_class_s = "";
                        hrm_class_s = provider.FormData.GetValues("hrm_candidate_academic").SingleOrDefault();
                        List<hrm_candidate_academic> hrm_Class_Schedules = JsonConvert.DeserializeObject<List<hrm_candidate_academic>>(hrm_class_s);
                        foreach (var item in hrm_Class_Schedules)
                        {
                            item.candidate_id = candidate.candidate_id;
                            item.organization_id = super ? 0 : int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_candidate_academic.Add(item);
                            db.SaveChanges();
                        }

                        var hrm_class_ss = "";
                        hrm_class_ss = provider.FormData.GetValues("list_work_experience").SingleOrDefault();
                        List<hrm_candidate_experience> hrm_Candidate_Experience = JsonConvert.DeserializeObject<List<hrm_candidate_experience>>(hrm_class_ss);
                        foreach (var item in hrm_Candidate_Experience)
                        {
                            item.candidate_id = candidate.candidate_id;
                            item.organization_id = super ? 0 : int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_candidate_experience.Add(item);
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Candidate", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Candidate", fileName);
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
                            hrm_File.key_id = candidate.candidate_id.ToString();
                            hrm_File.file_path = "/Portals/" + dvid + "/Candidate/" + fileName;
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
                            hrm_File.is_type = 4;
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
                            log.title = "Thêm thông tin ứng viên " + candidate.candidate_name;

                            log.log_module = "candidate";
                            log.log_type = 0;
                            log.id_key = candidate.candidate_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdcandidate, contents }), domainurl + "hrm_candidate/Add_candidate", ip, tid, "Lỗi khi thêm ứng viên", 0, "ứng viên");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdcandidate, contents }), domainurl + "hrm_candidate/Add_candidate", ip, tid, "Lỗi khi thêm ứng viên", 0, "ứng viên  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_hrm_candidate()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdcandidate = "";
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

                    string strPath = root + "/" + dvid + "/Candidate";
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



                        fdcandidate = provider.FormData.GetValues("hrm_candidate").SingleOrDefault();
                        hrm_candidate candidate = JsonConvert.DeserializeObject<hrm_candidate>(fdcandidate);
                        var checkBarcode = db.hrm_candidate.Where(a => a.candidate_code == candidate.candidate_code && a.candidate_id
                        != candidate.candidate_id && candidate.organization_id == intw).FirstOrDefault();
                        if (checkBarcode != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Mã số đã tồn tại! Vui lòng nhập lại", err = "1" });
                        }


                        candidate.modified_by = uid;
                        candidate.modified_date = DateTime.Now;
                        candidate.modified_ip = ip;
                        candidate.modified_token_id = tid;

                        db.Entry(candidate).State = EntityState.Modified;

                        db.SaveChanges();
                        var hrm_candidate_family_old = db.hrm_candidate_family.Where(s => s.candidate_id == candidate.candidate_id).ToArray<hrm_candidate_family>();
                        if (hrm_candidate_family_old.Length > 0)
                        {
                            db.hrm_candidate_family.RemoveRange(hrm_candidate_family_old);
                            db.SaveChanges();
                        }

                        var hrm_candidate_academics_old = db.hrm_candidate_academic.Where(s => s.candidate_id == candidate.candidate_id).ToArray<hrm_candidate_academic>();
                        if (hrm_candidate_academics_old.Length > 0)
                        {
                            db.hrm_candidate_academic.RemoveRange(hrm_candidate_academics_old);
                            db.SaveChanges();
                        }

                        var hrm_candidate_Experience_old = db.hrm_candidate_experience.Where(s => s.candidate_id == candidate.candidate_id).ToArray<hrm_candidate_experience>();
                        if (hrm_candidate_Experience_old.Length > 0)
                        {
                            db.hrm_candidate_experience.RemoveRange(hrm_candidate_Experience_old);
                            db.SaveChanges();
                        }
                        var hrm_candidate_family = "";



                        hrm_candidate_family = provider.FormData.GetValues("hrm_candidate_family").SingleOrDefault();
                        List<hrm_candidate_family> hrm_Users_Trainings = JsonConvert.DeserializeObject<List<hrm_candidate_family>>(hrm_candidate_family);
                        foreach (var item in hrm_Users_Trainings)
                        {
                            item.candidate_id = candidate.candidate_id;
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_candidate_family.Add(item);
                            db.SaveChanges();
                        }
                        var hrm_class_s = "";
                        hrm_class_s = provider.FormData.GetValues("hrm_candidate_academic").SingleOrDefault();
                        List<hrm_candidate_academic> hrm_Class_Schedules = JsonConvert.DeserializeObject<List<hrm_candidate_academic>>(hrm_class_s);
                        foreach (var item in hrm_Class_Schedules)
                        {

                            item.candidate_id = candidate.candidate_id;
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_candidate_academic.Add(item);
                            db.SaveChanges();
                        }

                        var hrm_class_ss = "";
                        hrm_class_ss = provider.FormData.GetValues("list_work_experience").SingleOrDefault();
                        List<hrm_candidate_experience> hrm_Candidate_Experience = JsonConvert.DeserializeObject<List<hrm_candidate_experience>>(hrm_class_ss);
                        foreach (var item in hrm_Candidate_Experience)
                        {
                            item.candidate_id = candidate.candidate_id;
                            item.organization_id = super ? 0 : int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_candidate_experience.Add(item);
                            db.SaveChanges();
                        }
                        var hrm_Files = "";
                        List<string> paths = new List<string>();
                        hrm_Files = provider.FormData.GetValues("hrm_files").SingleOrDefault();
                        List<hrm_file> hrm_File_S = JsonConvert.DeserializeObject<List<hrm_file>>(hrm_Files);
                        var id = candidate.candidate_id.ToString();
                        var hrmfile_Delete = new List<hrm_file>();
                        var hrm_file_Olds = db.hrm_file.Where(s => s.is_type == 4 && s.key_id == id).ToArray<hrm_file>();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Candidate", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                newFileName = Path.Combine(root + "/" + dvid + "/Candidate", fileName);
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
                            hrm_File.key_id = candidate.candidate_id.ToString();
                            hrm_File.file_name = Path.GetFileName(newFileName);
                            hrm_File.file_path = "/Portals/" + dvid + "/Candidate/" + fileName;
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
                            hrm_File.is_type = 4;
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
                            log.title = "Sửa thông tin ứng viên " + candidate.candidate_name;

                            log.log_module = "candidate";
                            log.log_type = 0;
                            log.id_key = candidate.candidate_id.ToString();
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

                            bool exists = File.Exists(root + "/" + dvid + "/Candidate/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(root + "/" + dvid + "/Candidate/" + Path.GetFileName(strP));
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdcandidate, contents }), domainurl + "hrm_candidate/Update_candidate", ip, tid, "Lỗi khi cập nhật candidate", 0, "candidate");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdcandidate, contents }), domainurl + "hrm_candidate/Update_candidate", ip, tid, "Lỗi khi cập nhật candidate", 0, "candidate");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_hrm_candidate([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.hrm_candidate.Where(a => id.Contains(a.candidate_id)).ToListAsync();
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<hrm_candidate> del = new List<hrm_candidate>();
                            foreach (var da in das)
                            {
                                del.Add(da);

                                var das1 = await db.hrm_candidate_family.Where(a => id.Contains(a.candidate_id)).ToListAsync();
                                db.hrm_candidate_family.RemoveRange(das1);
                                var das2 = await db.hrm_candidate_academic.Where(a => id.Contains(a.candidate_id)).ToListAsync();
                                db.hrm_candidate_academic.RemoveRange(das2);
                                var das3 = await db.hrm_candidate_experience.Where(a => id.Contains(a.candidate_id)).ToListAsync();
                                db.hrm_candidate_experience.RemoveRange(das3);
                                var arr = new List<String>();
                                foreach (var item in id)
                                {
                                    arr.Add(item.ToString());
                                }
                                var das4 = await db.hrm_file.Where(a => arr.Contains(a.key_id) && a.is_type == 4).ToListAsync();


                                foreach (var item in das4)
                                {


                                    if (!string.IsNullOrWhiteSpace(item.file_path))
                                        paths.Add(item.file_path.Substring(8));

                                }
                                db.hrm_file.RemoveRange(das4);

                                #region add hrm_log
                                if (helper.wlog)
                                {

                                    hrm_log log = new hrm_log();
                                    log.title = "Xóa ứng viên " + da.candidate_name;

                                    log.log_module = "candidate";
                                    log.log_type = 2;
                                    log.id_key = da.candidate_id.ToString();
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

                                    bool exists = File.Exists(root + "/" + dvid + "/Candidate/" + Path.GetFileName(strP));
                                    if (exists)
                                        System.IO.File.Delete(root + "/" + dvid + "/Candidate/" + Path.GetFileName(strP));
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_candidate.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_candidate/Delete_candidate", ip, tid, "Lỗi khi xoá ứng viên", 0, "candidate");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_candidate/Delete_candidate", ip, tid, "Lỗi khi xoá ứng viên", 0, "candidate");
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


        //[HttpPut]
        //public async Task<HttpResponseMessage> update_s_hrm_candidate([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    if (identity == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
        //    }
        //    IEnumerable<Claim> claims = identity.Claims;

        //    try
        //    {
        //        string ip = getipaddress();
        //        string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
        //        string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
        //        string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
        //        bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
        //        string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
        //        try
        //        {
        //            using (DBEntities db = new DBEntities())
        //            {
        //                var int_id = int.Parse(trangthai.IntID.ToString());
        //                var das = db.hrm_candidate.Where(a => (a.candidate_id == int_id)).FirstOrDefault<hrm_candidate>();
        //                if (das != null)
        //                {
        //                    das.modified_by = uid;
        //                    das.modified_date = DateTime.Now;
        //                    das.modified_ip = ip;
        //                    das.modified_token_id = tid;
        //                    das.status = trangthai.IntTrangthai;


        //                    #region add hrm_log
        //                    if (helper.wlog)
        //                    {

        //                        hrm_log log = new hrm_log();
        //                        log.title = "Sửa ứng viên " + das.candidate_name;

        //                        log.log_module = "candidate";
        //                        log.log_type = 1;
        //                        log.id_key = das.candidate_id.ToString();
        //                        log.created_date = DateTime.Now;
        //                        log.created_by = uid;
        //                        log.created_token_id = tid;
        //                        log.created_ip = ip;
        //                        db.hrm_log.Add(log);
        //                        db.SaveChanges();


        //                    }
        //                    #endregion
        //                    await db.SaveChangesAsync();
        //                }

        //                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
        //            }
        //        }
        //        catch (DbEntityValidationException e)
        //        {
        //            string contents = helper.getCatchError(e, null);
        //            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_candidate/Update_Trangthaicandidate", ip, tid, "Lỗi khi cập nhật trạng thái ứng viên", 0, "hrm_candidate");
        //            if (!helper.debug)
        //            {
        //                contents = "";
        //            }
        //            Log.Error(contents);
        //            return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
        //        }
        //        catch (Exception e)
        //        {
        //            string contents = helper.ExceptionMessage(e);
        //            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_candidate/Update_Trangthaicandidate", ip, tid, "Lỗi khi cập nhật trạng thái ứng viên", 0, "hrm_candidate");
        //            if (!helper.debug)
        //            {
        //                contents = "";
        //            }
        //            Log.Error(contents);
        //            return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);
        //    }

        //}
    }
}