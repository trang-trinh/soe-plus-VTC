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

namespace Controllers
{
    [Authorize(Roles = "login")]
    public class sys_organization_historyController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> add_sys_organization_history()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdOrganization_history = "";
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

                    string strPath = root + "/" + dvid + "/OrganizationHistory";
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
                        fdOrganization_history = provider.FormData.GetValues("sys_organization_history").SingleOrDefault();
                        sys_organization_history Organization_history = JsonConvert.DeserializeObject<sys_organization_history>(fdOrganization_history);

                        var intw = int.Parse(dvid);
                        var checkBarcode = db.sys_organization_history.AsNoTracking().Where(a => a.decision_number == Organization_history.decision_number && a.organization_id == intw).FirstOrDefault();
                        if (checkBarcode != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Số quyết định đã tồn tại! Vui lòng nhập lại.", err = "1" });
                        }

                        Organization_history.organization_id = int.Parse(dvid);
                        Organization_history.created_by = uid;
                        Organization_history.created_date = DateTime.Now;
                        Organization_history.created_ip = ip;
                        Organization_history.created_token_id = tid;
                        Organization_history.modified_by = uid;
                        Organization_history.modified_date = DateTime.Now;
                        Organization_history.modified_ip = ip;
                        Organization_history.modified_token_id = tid;
                        db.sys_organization_history.Add(Organization_history);
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
                            newFileName = Path.Combine(root + "/" + dvid + "/OrganizationHistory", fileName);
                            fileInfo = new FileInfo(newFileName);

                            newFileName = Path.Combine(root + "/" + dvid + "/OrganizationHistory",
                            helper.newFileName(fileInfo, root + "/" + dvid + "/OrganizationHistory", newFileName, 1, root, int.Parse(dvid)));
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
                            hrm_File.key_id = Organization_history.organization_history_id.ToString();
                            hrm_File.file_path = "/Portals/" + dvid + "/OrganizationHistory/" + fileName;
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
                            hrm_File.is_type = 10;
                            hrm_File.status = true;
                            hrm_File.created_by = uid;
                            hrm_File.created_date = DateTime.Now;
                            hrm_File.organization_id = int.Parse(dvid);
                            hrm_File.created_ip = ip;
                            hrm_File.created_token_id = tid;
                            db.hrm_file.Add(hrm_File);
                            db.SaveChanges();
                        }



 
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdOrganization_history, contents }), domainurl + "sys_organization_history/Add_Organization_history", ip, tid, "Lỗi khi thêm chiến dịch", 0, "chiến dịch");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdOrganization_history, contents }), domainurl + "sys_organization_history/Add_Organization_history", ip, tid, "Lỗi khi thêm chiến dịch", 0, "chiến dịch  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_sys_organization_history()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdOrganization_history = "";
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

                    string strPath = root + "/" + dvid + "/OrganizationHistory";
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



                        fdOrganization_history = provider.FormData.GetValues("sys_organization_history").SingleOrDefault();
                        sys_organization_history Organization_history = JsonConvert.DeserializeObject<sys_organization_history>(fdOrganization_history);

                        var checkBarcode = db.sys_organization_history.AsNoTracking().Where(a => a.decision_number == Organization_history.decision_number && a.organization_history_id
                  != Organization_history.organization_history_id && Organization_history.organization_id == intw).FirstOrDefault();
                        if (checkBarcode != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Số quyết định đã tồn tại! Vui lòng nhập lại", err = "1" });
                        }

                        Organization_history.modified_by = uid;
                        Organization_history.modified_date = DateTime.Now;
                        Organization_history.modified_ip = ip;
                        Organization_history.modified_token_id = tid;

                        db.Entry(Organization_history).State = EntityState.Modified;

                        db.SaveChanges();

                        var hrm_Files = "";
                        List<string> paths = new List<string>();
                        hrm_Files = provider.FormData.GetValues("hrm_files").SingleOrDefault();
                        List<hrm_file> hrm_File_S = JsonConvert.DeserializeObject<List<hrm_file>>(hrm_Files);
                        var id = Organization_history.organization_history_id.ToString();
                        var hrmfile_Delete = new List<hrm_file>();
                        var hrm_file_Olds = db.hrm_file.Where(s => s.is_type == 10 && s.key_id == id).ToArray<hrm_file>();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/OrganizationHistory", fileName);
                            fileInfo = new FileInfo(newFileName);
                            // if (fileInfo.Exists)
                            // {
                            //     fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            //     fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                            //     newFileName = Path.Combine(root + "/" + dvid + "/OrganizationHistory", fileName);
                            // }
                            newFileName = Path.Combine(root + "/" + dvid + "/OrganizationHistory",
                       helper.newFileName(fileInfo, root + "/" + dvid + "/OrganizationHistory", newFileName, 1, root, int.Parse(dvid)));
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
                            hrm_File.key_id = Organization_history.organization_history_id.ToString();
                            hrm_File.file_name = Path.GetFileName(newFileName);
                            hrm_File.file_path = "/Portals/" + dvid + "/OrganizationHistory/" + fileName;
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
                            hrm_File.is_type = 10;
                            hrm_File.status = true;
                            hrm_File.created_by = uid;
                            hrm_File.created_date = DateTime.Now;
                            hrm_File.created_ip = ip; hrm_File.organization_id = int.Parse(dvid);
                            hrm_File.created_token_id = tid;
                            db.hrm_file.Add(hrm_File);
                            db.SaveChanges();
                        }


                        
                        foreach (string strP in paths)
                        {

                            bool exists = File.Exists(root + "/" + dvid + "/OrganizationHistory/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(root + "/" + dvid + "/OrganizationHistory/" + Path.GetFileName(strP));
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdOrganization_history, contents }), domainurl + "sys_organization_history/Update_Organization_history", ip, tid, "Lỗi khi cập nhật Organization_history", 0, "Organization_history");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdOrganization_history, contents }), domainurl + "sys_organization_history/Update_Organization_history", ip, tid, "Lỗi khi cập nhật Organization_history", 0, "Organization_history");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_sys_organization_history([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.sys_organization_history.Where(a => id.Contains(a.organization_history_id)).ToListAsync();
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<sys_organization_history> del = new List<sys_organization_history>();
                            foreach (var da in das)
                            {
                                del.Add(da);


                                var arr = new List<String>();
                                foreach (var item in id)
                                {
                                    arr.Add(item.ToString());
                                }
                                var das3 = await db.hrm_file.Where(a => arr.Contains(a.key_id) && a.is_type == 10).ToListAsync();


                                foreach (var item in das3)
                                {


                                    if (!string.IsNullOrWhiteSpace(item.file_path))
                                        paths.Add(item.file_path.Substring(8));

                                }
                                db.hrm_file.RemoveRange(das3);

                                
                                foreach (string strP in paths)
                                {

                                    bool exists = File.Exists(root + "/" + dvid + "/OrganizationHistory/" + Path.GetFileName(strP));
                                    if (exists)
                                        System.IO.File.Delete(root + "/" + dvid + "/OrganizationHistory/" + Path.GetFileName(strP));
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.sys_organization_history.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "sys_organization_history/Delete_Organization_history", ip, tid, "Lỗi khi xoá chiến dịch", 0, "Organization_history");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "sys_organization_history/Delete_Organization_history", ip, tid, "Lỗi khi xoá chiến dịch", 0, "Organization_history");
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


        [HttpPost]
        public async Task<HttpResponseMessage> add_sys_organization_merger()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdOrganization_merger = "";
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

                    string strPath = root + "/" + dvid + "/OrganizationMerger";
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
                        fdOrganization_merger = provider.FormData.GetValues("sys_organization_merger").SingleOrDefault();
                        sys_organization_merger Organization_merger = JsonConvert.DeserializeObject<sys_organization_merger>(fdOrganization_merger);

                        var intw = int.Parse(dvid);
                        var checkBarcode = db.sys_organization_merger.AsNoTracking().Where(a => a.decision_number == Organization_merger.decision_number && a.organization_id == intw).FirstOrDefault();
                        if (checkBarcode != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Số quyết định đã tồn tại! Vui lòng nhập lại.", err = "1" });
                        }

                        Organization_merger.organization_id = int.Parse(dvid);
                        Organization_merger.created_by = uid;
                        Organization_merger.created_date = DateTime.Now;
                        Organization_merger.created_ip = ip;
                        Organization_merger.created_token_id = tid;
                        Organization_merger.modified_by = uid;
                        Organization_merger.modified_date = DateTime.Now;
                        Organization_merger.modified_ip = ip;
                        Organization_merger.modified_token_id = tid;
                        db.sys_organization_merger.Add(Organization_merger);
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
                            newFileName = Path.Combine(root + "/" + dvid + "/OrganizationMerger", fileName);
                            fileInfo = new FileInfo(newFileName);

                            newFileName = Path.Combine(root + "/" + dvid + "/OrganizationMerger",
                            helper.newFileName(fileInfo, root + "/" + dvid + "/OrganizationMerger", newFileName, 1, root, int.Parse(dvid)));
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
                            hrm_File.key_id = Organization_merger.organization_merger_id.ToString();
                            hrm_File.file_path = "/Portals/" + dvid + "/OrganizationMerger/" + fileName;
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
                            hrm_File.is_type = 11;
                            hrm_File.status = true;
                            hrm_File.created_by = uid;
                            hrm_File.created_date = DateTime.Now;
                            hrm_File.organization_id = int.Parse(dvid);
                            hrm_File.created_ip = ip;
                            hrm_File.created_token_id = tid;
                            db.hrm_file.Add(hrm_File);
                            db.SaveChanges();
                        }




                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdOrganization_merger, contents }), domainurl + "sys_organization_merger/Add_Organization_merger", ip, tid, "Lỗi khi thêm chiến dịch", 0, "chiến dịch");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdOrganization_merger, contents }), domainurl + "sys_organization_merger/Add_Organization_merger", ip, tid, "Lỗi khi thêm chiến dịch", 0, "chiến dịch  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_sys_organization_merger()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdOrganization_merger = "";
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

                    string strPath = root + "/" + dvid + "/OrganizationMerger";
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



                        fdOrganization_merger = provider.FormData.GetValues("sys_organization_merger").SingleOrDefault();
                        sys_organization_merger Organization_merger = JsonConvert.DeserializeObject<sys_organization_merger>(fdOrganization_merger);

                        var checkBarcode = db.sys_organization_merger.AsNoTracking().Where(a => a.decision_number == Organization_merger.decision_number && a.organization_merger_id
                  != Organization_merger.organization_merger_id && Organization_merger.organization_id == intw).FirstOrDefault();
                        if (checkBarcode != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Số quyết định đã tồn tại! Vui lòng nhập lại", err = "1" });
                        }

                        Organization_merger.modified_by = uid;
                        Organization_merger.modified_date = DateTime.Now;
                        Organization_merger.modified_ip = ip;
                        Organization_merger.modified_token_id = tid;

                        db.Entry(Organization_merger).State = EntityState.Modified;

                        db.SaveChanges();

                        var hrm_Files = "";
                        List<string> paths = new List<string>();
                        hrm_Files = provider.FormData.GetValues("hrm_files").SingleOrDefault();
                        List<hrm_file> hrm_File_S = JsonConvert.DeserializeObject<List<hrm_file>>(hrm_Files);
                        var id = Organization_merger.organization_merger_id.ToString();
                        var hrmfile_Delete = new List<hrm_file>();
                        var hrm_file_Olds = db.hrm_file.Where(s => s.is_type == 11 && s.key_id == id).ToArray<hrm_file>();
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
                            newFileName = Path.Combine(root + "/" + dvid + "/OrganizationMerger", fileName);
                            fileInfo = new FileInfo(newFileName);
                            // if (fileInfo.Exists)
                            // {
                            //     fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            //     fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                            //     newFileName = Path.Combine(root + "/" + dvid + "/OrganizationMerger", fileName);
                            // }
                            newFileName = Path.Combine(root + "/" + dvid + "/OrganizationMerger",
                       helper.newFileName(fileInfo, root + "/" + dvid + "/OrganizationMerger", newFileName, 1, root, int.Parse(dvid)));
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
                            hrm_File.key_id = Organization_merger.organization_merger_id.ToString();
                            hrm_File.file_name = Path.GetFileName(newFileName);
                            hrm_File.file_path = "/Portals/" + dvid + "/OrganizationMerger/" + fileName;
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
                            hrm_File.is_type = 11;
                            hrm_File.status = true;
                            hrm_File.created_by = uid;
                            hrm_File.created_date = DateTime.Now;
                            hrm_File.created_ip = ip; hrm_File.organization_id = int.Parse(dvid);
                            hrm_File.created_token_id = tid;
                            db.hrm_file.Add(hrm_File);
                            db.SaveChanges();
                        }



                        foreach (string strP in paths)
                        {

                            bool exists = File.Exists(root + "/" + dvid + "/OrganizationMerger/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(root + "/" + dvid + "/OrganizationMerger/" + Path.GetFileName(strP));
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdOrganization_merger, contents }), domainurl + "sys_organization_merger/Update_Organization_merger", ip, tid, "Lỗi khi cập nhật Organization_merger", 0, "Organization_merger");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdOrganization_merger, contents }), domainurl + "sys_organization_merger/Update_Organization_merger", ip, tid, "Lỗi khi cập nhật Organization_merger", 0, "Organization_merger");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_sys_organization_merger([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.sys_organization_merger.Where(a => id.Contains(a.organization_merger_id)).ToListAsync();
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<sys_organization_merger> del = new List<sys_organization_merger>();
                            foreach (var da in das)
                            {
                                del.Add(da);


                                var arr = new List<String>();
                                foreach (var item in id)
                                {
                                    arr.Add(item.ToString());
                                }
                                var das3 = await db.hrm_file.Where(a => arr.Contains(a.key_id) && a.is_type == 11).ToListAsync();


                                foreach (var item in das3)
                                {


                                    if (!string.IsNullOrWhiteSpace(item.file_path))
                                        paths.Add(item.file_path.Substring(8));

                                }
                                db.hrm_file.RemoveRange(das3);


                                foreach (string strP in paths)
                                {

                                    bool exists = File.Exists(root + "/" + dvid + "/OrganizationMerger/" + Path.GetFileName(strP));
                                    if (exists)
                                        System.IO.File.Delete(root + "/" + dvid + "/OrganizationMerger/" + Path.GetFileName(strP));
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.sys_organization_merger.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "sys_organization_merger/Delete_Organization_merger", ip, tid, "Lỗi khi xoá chiến dịch", 0, "Organization_merger");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "sys_organization_merger/Delete_Organization_merger", ip, tid, "Lỗi khi xoá chiến dịch", 0, "Organization_merger");
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