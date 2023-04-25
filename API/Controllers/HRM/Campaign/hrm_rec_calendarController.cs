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
    public class hrm_rec_calendarController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_rec_calendar()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdrec_calendar = "";
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

                    string strPath = root + "/" + dvid + "/RecCalendar";
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
                        fdrec_calendar = provider.FormData.GetValues("hrm_rec_calendar").SingleOrDefault();
                        hrm_rec_calendar rec_calendar = JsonConvert.DeserializeObject<hrm_rec_calendar>(fdrec_calendar);
                        var intw = int.Parse(dvid);
                        rec_calendar.organization_id =  int.Parse(dvid);
                        rec_calendar.created_by = uid;
                        rec_calendar.created_date = DateTime.Now;
                        rec_calendar.created_ip = ip;
                        rec_calendar.created_token_id = tid;
                        rec_calendar.modified_by = uid;
                        rec_calendar.modified_date = DateTime.Now;
                        rec_calendar.modified_ip = ip;
                        rec_calendar.modified_token_id = tid;
                        db.hrm_rec_calendar.Add(rec_calendar);
                        db.SaveChanges();
                        var hrm_rec_Candidate = "";
                        hrm_rec_Candidate = provider.FormData.GetValues("hrm_rec_candidate").SingleOrDefault();
                        List<hrm_rec_candidate> hrm_rec_Candidates = JsonConvert.DeserializeObject<List<hrm_rec_candidate>>(hrm_rec_Candidate);
                        foreach (var item in hrm_rec_Candidates)
                        {
                            item.rec_calendar_id = rec_calendar.rec_calendar_id;
                            item.organization_id =   int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_rec_candidate.Add(item);
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
                            newFileName = Path.Combine(root + "/" + dvid + "/RecCalendar", fileName);
                            fileInfo = new FileInfo(newFileName);
                            // if (fileInfo.Exists)
                            // {
                            //     fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            //     fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                            //     newFileName = Path.Combine(root + "/" + dvid + "/RecCalendar", fileName);
                            // }
                             newFileName = Path.Combine(root + "/" + dvid + "/RecCalendar",
                                helper.newFileName(fileInfo, root + "/" + dvid + "/RecCalendar", newFileName, 1, root, int.Parse(dvid)));
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
                            hrm_File.key_id = rec_calendar.rec_calendar_id.ToString();
                            hrm_File.file_path = "/Portals/" + dvid + "/RecCalendar/" + fileName;
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
                            hrm_File.is_type = 5;
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
                            log.title = "Thêm thông tin lịch phỏng vấn " + rec_calendar.rec_calendar_name;

                            log.log_module = "rec_calendar";
                            log.log_type = 0;
                            log.id_key = rec_calendar.rec_calendar_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdrec_calendar, contents }), domainurl + "hrm_rec_calendar/Add_rec_calendar", ip, tid, "Lỗi khi thêm lịch phỏng vấn", 0, "lịch phỏng vấn");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdrec_calendar, contents }), domainurl + "hrm_rec_calendar/Add_rec_calendar", ip, tid, "Lỗi khi thêm lịch phỏng vấn", 0, "lịch phỏng vấn  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_hrm_rec_calendar()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdrec_calendar = "";
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

                    string strPath = root + "/" + dvid + "/RecCalendar";
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



                        fdrec_calendar = provider.FormData.GetValues("hrm_rec_calendar").SingleOrDefault();
                        hrm_rec_calendar rec_calendar = JsonConvert.DeserializeObject<hrm_rec_calendar>(fdrec_calendar);
                         

                        rec_calendar.modified_by = uid;
                        rec_calendar.modified_date = DateTime.Now;
                        rec_calendar.modified_ip = ip;
                        rec_calendar.modified_token_id = tid;

                        db.Entry(rec_calendar).State = EntityState.Modified;

                        db.SaveChanges();
                        var hrm_rec_candidate_old = db.hrm_rec_candidate.Where(s => s.rec_calendar_id == rec_calendar.rec_calendar_id).ToArray<hrm_rec_candidate>();
                        if (hrm_rec_candidate_old.Length > 0)
                        {
                            db.hrm_rec_candidate.RemoveRange(hrm_rec_candidate_old);
                            db.SaveChanges();
                        }

                        var hrm_rec_Candidate = "";
                        hrm_rec_Candidate = provider.FormData.GetValues("hrm_rec_candidate").SingleOrDefault();
                        List<hrm_rec_candidate> hrm_rec_Candidates = JsonConvert.DeserializeObject<List<hrm_rec_candidate>>(hrm_rec_Candidate);
                        foreach (var item in hrm_rec_Candidates)
                        {
                            item.rec_calendar_id = rec_calendar.rec_calendar_id;
                            item.organization_id =  int.Parse(dvid);
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;
                            db.hrm_rec_candidate.Add(item);
                            db.SaveChanges();
                        }

                        var hrm_Files = "";
                        List<string> paths = new List<string>();
                        hrm_Files = provider.FormData.GetValues("hrm_files").SingleOrDefault();
                        List<hrm_file> hrm_File_S = JsonConvert.DeserializeObject<List<hrm_file>>(hrm_Files);
                        var id = rec_calendar.rec_calendar_id.ToString();
                        var hrmfile_Delete = new List<hrm_file>();
                        var hrm_file_Olds = db.hrm_file.Where(s => s.is_type == 5 && s.key_id == id).ToArray<hrm_file>();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/RecCalendar", fileName);
                            fileInfo = new FileInfo(newFileName);
                            // if (fileInfo.Exists)
                            // {
                            //     fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            //     fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                            //     newFileName = Path.Combine(root + "/" + dvid + "/RecCalendar", fileName);
                            // }
                               newFileName = Path.Combine(root + "/" + dvid + "/RecCalendar",
                                helper.newFileName(fileInfo, root + "/" + dvid + "/RecCalendar", newFileName, 1, root, int.Parse(dvid)));
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
                            hrm_File.key_id = rec_calendar.rec_calendar_id.ToString();
                            hrm_File.file_name = Path.GetFileName(newFileName);
                            hrm_File.file_path = "/Portals/" + dvid + "/RecCalendar/" + fileName;
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
                            hrm_File.is_type = 5;
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
                            log.title = "Sửa thông tin lịch phỏng vấn " + rec_calendar.rec_calendar_name;
                            log.log_module = "rec_calendar";
                            log.log_type = 0;
                            log.id_key = rec_calendar.rec_calendar_id.ToString();
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

                            bool exists = File.Exists(root + "/" + dvid + "/RecCalendar/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(root + "/" + dvid + "/RecCalendar/" + Path.GetFileName(strP));
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdrec_calendar, contents }), domainurl + "hrm_rec_calendar/Update_rec_calendar", ip, tid, "Lỗi khi cập nhật rec_calendar", 0, "rec_calendar");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdrec_calendar, contents }), domainurl + "hrm_rec_calendar/Update_rec_calendar", ip, tid, "Lỗi khi cập nhật rec_calendar", 0, "rec_calendar");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_hrm_rec_calendar([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.hrm_rec_calendar.Where(a => id.Contains(a.rec_calendar_id)).ToListAsync();
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<hrm_rec_calendar> del = new List<hrm_rec_calendar>();
                            foreach (var da in das)
                            {
                                del.Add(da);
 

                                var das1 = await db.hrm_rec_candidate.Where(a => id.Contains(a.rec_calendar_id)).ToListAsync();
                                db.hrm_rec_candidate.RemoveRange(das1);

                                var arr = new List<String>();
                                foreach (var item in id)
                                {
                                    arr.Add(item.ToString());
                                }
                                var das4 = await db.hrm_file.Where(a => arr.Contains(a.key_id) && a.is_type == 5).ToListAsync();


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
                                    log.title = "Xóa lịch phỏng vấn " + da.rec_calendar_name;

                                    log.log_module = "rec_calendar";
                                    log.log_type = 2;
                                    log.id_key = da.rec_calendar_id.ToString();
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

                                    bool exists = File.Exists(root + "/" + dvid + "/RecCalendar/" + Path.GetFileName(strP));
                                    if (exists)
                                        System.IO.File.Delete(root + "/" + dvid + "/RecCalendar/" + Path.GetFileName(strP));
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_rec_calendar.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_rec_calendar/Delete_rec_calendar", ip, tid, "Lỗi khi xoá lịch phỏng vấn", 0, "rec_calendar");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_rec_calendar/Delete_rec_calendar", ip, tid, "Lỗi khi xoá lịch phỏng vấn", 0, "rec_calendar");
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
        public async Task<HttpResponseMessage> update_s_hrm_rec_calendar([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                        var das = db.hrm_rec_calendar.Where(a => (a.rec_calendar_id == int_id)).FirstOrDefault<hrm_rec_calendar>();
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
                                log.title = "Sửa ứng viên " + das.rec_calendar_name;

                                log.log_module = "rec_calendar";
                                log.log_type = 1;
                                log.id_key = das.rec_calendar_id.ToString();
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_rec_calendar/Update_Trangthairec_calendar", ip, tid, "Lỗi khi cập nhật trạng thái ứng viên", 0, "hrm_rec_calendar");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_rec_calendar/Update_Trangthairec_calendar", ip, tid, "Lỗi khi cập nhật trạng thái ứng viên", 0, "hrm_rec_calendar");
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