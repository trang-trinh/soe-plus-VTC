using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers.Request
{
    [Authorize(Roles = "login")]
    public class requestController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        //
        [HttpPost]
        public async Task<HttpResponseMessage> Add_Request()
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    //string jwtcookie = HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies["jwt"] != null ? HttpContext.Current.Request.Cookies["jwt"].Value : null;
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var modelRequest = provider.FormData.GetValues("modelRequest").SingleOrDefault();
                        request_master request = JsonConvert.DeserializeObject<request_master>(modelRequest);
                        request.request_id = helper.GenKey();
                        request.created_by = uid;
                        request.created_date = DateTime.Now;
                        request.created_ip = ip;
                        request.created_token_id = tid;
                        request.organization_id = user_now.organization_id;
                        request.status = 0;
                        request.status_processing = 0;
                        // ... coding
                        db.request_master.Add(request);
                        db.SaveChanges();

                        var formDS_Request = provider.FormData.GetValues("formDS").SingleOrDefault();
                        List<request_master_detail> formDS = JsonConvert.DeserializeObject<List<request_master_detail>>(formDS_Request);
                        List<request_master_detail> request_detail_add = new List<request_master_detail>();
                        foreach(var item in formDS)
                        {
                            item.request_detail_id = helper.GenKey();
                            item.request_id = request.request_id;
                            request_detail_add.Add(item);
                        }

                        var modelApprover = provider.FormData.GetValues("listApprover").SingleOrDefault();
                        List<request_sign_user> listApprover = JsonConvert.DeserializeObject<List<request_sign_user>>(modelApprover);
                        //var sttApprover = 1;
                        //foreach(var item in listApprover)
                        //{
                        //    request_sign_user approver = new request_sign_user();
                        //    approver.request_sign_user_id = helper.GenKey();
                        //    approver.request_id = request.request_id;
                        //    approver.status = false;
                        //    approver.is_order = sttApprover;
                        //    sttApprover++;
                        //}

                        var modelManager = provider.FormData.GetValues("listManager").SingleOrDefault();
                        List<request_follow> listManager = JsonConvert.DeserializeObject<List<request_follow>>(modelManager);

                        var modelFollower = provider.FormData.GetValues("listFollower").SingleOrDefault();
                        List<request_follow> listFollower = JsonConvert.DeserializeObject<List<request_follow>>(modelFollower);
                        List<request_follow> listFollowAdd = new List<request_follow>();
                        var sttfollower = 1;
                        foreach (var item in listFollowAdd)
                        {
                            item.request_follow_id = helper.GenKey();
                            item.request_id = request.request_id;
                            item.status_follow = true;
                            item.is_order = sttfollower;
                            item.is_notify = true;
                            item.is_type = 0;
                            sttfollower++;
                            listFollowAdd.Add(item);
                        }

                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        List<string> listPathFileUp = new List<string>();
                        List<request_master_file> listFileRequest = new List<request_master_file>();
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
                            newFileName = Path.Combine("/" + organization_id_user + "/Request/" + request.request_id, Path.GetFileName(fileName));

                            var listPathEdit_File = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                            var pathEdit_File = "";
                            foreach (var itemEdit in listPathEdit_File)
                            {
                                if (itemEdit.Trim() != "")
                                {
                                    pathEdit_File += "/" + Path.GetFileName(itemEdit);
                                }
                            }

                            fileInfo = new FileInfo(root + pathEdit_File);
                            var nameFileOrigin = fileName;
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = Path.Combine("/" + organization_id_user + "/Request/" + request.request_id, fileName);
                            }
                            string pathFile = "/Portals/" + organization_id_user + "/Request/" + request.request_id + "/" + fileName;
                            var fileR = new request_master_file();
                            fileR.request_file_id = helper.GenKey();
                            fileR.request_id = request.request_id;
                            fileR.file_type = helper.GetFileExtension(fileName).ToLower();
                            fileR.file_name = nameFileOrigin;
                            fileR.file_path = pathFile;
                            fileR.file_size = new FileInfo(fileData.LocalFileName).Length;
                            fileR.is_image = helper.IsImageFileName(nameFileOrigin);
                            fileR.is_type = 0;
                            fileR.is_delete = false;
                            fileR.created_by = uid;
                            fileR.created_date = DateTime.Now;
                            fileR.created_ip = ip;
                            fileR.created_token_id = tid;

                            if (helper.IsImageFileName(fileName))
                            {
                                helper.ResizeImage(domainurl + pathFile, 1920, 1080, 90);
                            }
                            listFileRequest.Add(fileR);

                            ffileData = fileData;
                            if (fileInfo != null)
                            {
                                var strDirectory = "/" + organization_id_user + "/Request/" + request.request_id;
                                var listPathEdit = Regex.Replace(strDirectory.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit = "";
                                foreach (var itemEdit in listPathEdit)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                if (!Directory.Exists(root + pathEdit))
                                {
                                    Directory.CreateDirectory(root + pathEdit);
                                }

                                var listPathEdit_1 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_1 = "";
                                foreach (var itemEdit in listPathEdit_1)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                File.Move(ffileData.LocalFileName, root + pathEdit_1);
                                listPathFileUp.Add(ffileData.LocalFileName);
                                //helper.UploadFileToDestination(jwtcookie, root, ffileData, pathEdit_1, 360, 360);
                                /*
                                var Portals = ConfigurationManager.AppSettings["Portals"];
                                sys_file_mapping fm = new sys_file_mapping();
                                fm.file_key_id = helper.GenKey();
                                fm.file_id = fileR.request_file_id;
                                fm.file_path = fileR.file_path;
                                fm.file_name = fileR.file_name;
                                fm.file_size = fileR.file_size;
                                fm.file_title = fileR.file_name;
                                fm.file_table = "request_master_file";
                                if (string.IsNullOrWhiteSpace(Portals))
                                {
                                    fm.type_path = 0;
                                }
                                else if (Portals.Contains("ftp"))
                                {
                                    fm.type_path = 1;
                                }
                                else if (Portals.Contains("http"))
                                {
                                    fm.type_path = 2;
                                }
                                fm.module_key = "M12";
                                fm.role_access = null;
                                fm.user_access = "";
                                //var memberChat = db.chat_member.AsNoTracking().Where(z => z.chat_group_id == chatGroupNow.chat_group_id).Select(c => c.user_join).ToList();
                                //foreach (var userID in memberChat)
                                //{
                                //    fm.user_access += (fm.user_access != "" ? "," : "") + userID;
                                //}
                                fm.deny_access = null;
                                fm.created_by = uid;
                                fm.created_date = fileR.created_date;
                                fm.created_ip = ip;
                                fm.created_token_id = tid;
                                db.sys_file_mapping.Add(fm);
                                //});
                                */
                            }
                        }

                        if (request_detail_add.Count > 0)
                        {
                            db.request_master_detail.AddRange(request_detail_add);
                        }
                        if (listFollowAdd.Count > 0)
                        {
                            db.request_follow.AddRange(listFollowAdd);
                        }
                        if (listFileRequest.Count > 0)
                        {
                            db.request_master_file.AddRange(listFileRequest);
                        }

                        db.SaveChanges();

                        if (listPathFileUp.Count > 0)
                        {
                            foreach (var path in listPathFileUp)
                            {
                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                }
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "request/Add_Request", ip, tid, "Lỗi khi thêm mới đề xuất", 0, "request");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "request/Add_Request", ip, tid, "Lỗi khi thêm mới đề xuất", 0, "request");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Request()
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    //string jwtcookie = HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies["jwt"] != null ? HttpContext.Current.Request.Cookies["jwt"].Value : null;
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var modelRequest = provider.FormData.GetValues("modelRequest").SingleOrDefault();
                        request_master request = JsonConvert.DeserializeObject<request_master>(modelRequest);
                        request.modified_by = uid;
                        request.modified_date = DateTime.Now;
                        request.modified_ip = ip;
                        request.modified_token_id = tid;
                        db.Entry(request).State = EntityState.Modified;

                        var formDS_Request = provider.FormData.GetValues("formDS").SingleOrDefault();
                        List<request_master_detail> formDS = JsonConvert.DeserializeObject<List<request_master_detail>>(formDS_Request);
                        List<request_master_detail> request_detail_add = new List<request_master_detail>();
                        var delFormDS = db.request_master_detail.Where(x => x.request_id == request.request_id).ToList();
                        foreach (var item in formDS)
                        {
                            item.request_detail_id = helper.GenKey();
                            item.request_id = request.request_id;
                            request_detail_add.Add(item);
                        }

                        var modelApprover = provider.FormData.GetValues("listApprover").SingleOrDefault();
                        List<request_sign_user> listApprover = JsonConvert.DeserializeObject<List<request_sign_user>>(modelApprover);
                        //var sttApprover = 1;
                        //foreach(var item in listApprover)
                        //{
                        //    request_sign_user approver = new request_sign_user();
                        //    approver.request_sign_user_id = helper.GenKey();
                        //    approver.request_id = request.request_id;
                        //    approver.status = false;
                        //    approver.is_order = sttApprover;
                        //    sttApprover++;
                        //}

                        var modelManager = provider.FormData.GetValues("listManager").SingleOrDefault();
                        List<request_follow> listManager = JsonConvert.DeserializeObject<List<request_follow>>(modelManager);

                        var modelFollower = provider.FormData.GetValues("listFollower").SingleOrDefault();
                        List<request_follow> listFollower = JsonConvert.DeserializeObject<List<request_follow>>(modelFollower);                        
                        List<request_follow> listFollowAdd = new List<request_follow>();
                        var userFollowAdd = listFollower.Where(x => db.request_follow.Count(y => y.user_id == x.user_id && y.request_id == x.request_id) == 0).ToList();
                        var userFollowTemp = db.request_follow.AsNoTracking().Where(x => x.request_id == request.request_id).ToList();
                        var userFollowDel = userFollowTemp.Where(x => listApprover.Count(y => y.user_id == x.user_id) == 0).ToList();
                        var sttfollower = 1;
                        foreach (var item in userFollowAdd)
                        {
                            item.request_follow_id = helper.GenKey();
                            item.request_id = request.request_id;
                            item.status_follow = true;
                            item.is_order = sttfollower;
                            item.is_notify = true;
                            item.is_type = 0;
                            sttfollower++;
                            listFollowAdd.Add(item);
                        }

                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        List<string> listPathFileUp = new List<string>();
                        List<request_master_file> listFileRequest = new List<request_master_file>();
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
                            newFileName = Path.Combine("/" + organization_id_user + "/Request/" + request.request_id, Path.GetFileName(fileName));

                            var listPathEdit_File = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                            var pathEdit_File = "";
                            foreach (var itemEdit in listPathEdit_File)
                            {
                                if (itemEdit.Trim() != "")
                                {
                                    pathEdit_File += "/" + Path.GetFileName(itemEdit);
                                }
                            }

                            fileInfo = new FileInfo(root + pathEdit_File);
                            var nameFileOrigin = fileName;
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = Path.Combine("/" + organization_id_user + "/Request/" + request.request_id, fileName);
                            }
                            string pathFile = "/Portals/" + organization_id_user + "/Request/" + request.request_id + "/" + fileName;
                            var fileR = new request_master_file();
                            fileR.request_file_id = helper.GenKey();
                            fileR.request_id = request.request_id;
                            fileR.file_type = helper.GetFileExtension(fileName).ToLower();
                            fileR.file_name = nameFileOrigin;
                            fileR.file_path = pathFile;
                            fileR.file_size = new FileInfo(fileData.LocalFileName).Length;
                            fileR.is_image = helper.IsImageFileName(nameFileOrigin);
                            fileR.is_type = 0;
                            fileR.is_delete = false;
                            fileR.created_by = uid;
                            fileR.created_date = DateTime.Now;
                            fileR.created_ip = ip;
                            fileR.created_token_id = tid;

                            if (helper.IsImageFileName(fileName))
                            {
                                helper.ResizeImage(domainurl + pathFile, 1920, 1080, 90);
                            }
                            listFileRequest.Add(fileR);

                            ffileData = fileData;
                            if (fileInfo != null)
                            {
                                var strDirectory = "/" + organization_id_user + "/Request/" + request.request_id;
                                var listPathEdit = Regex.Replace(strDirectory.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit = "";
                                foreach (var itemEdit in listPathEdit)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                if (!Directory.Exists(root + pathEdit))
                                {
                                    Directory.CreateDirectory(root + pathEdit);
                                }

                                var listPathEdit_1 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_1 = "";
                                foreach (var itemEdit in listPathEdit_1)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                File.Move(ffileData.LocalFileName, root + pathEdit_1);
                                listPathFileUp.Add(ffileData.LocalFileName);
                                //helper.UploadFileToDestination(jwtcookie, root, ffileData, pathEdit_1, 360, 360);

                                var Portals = ConfigurationManager.AppSettings["Portals"];
                                sys_file_mapping fm = new sys_file_mapping();
                                fm.file_key_id = helper.GenKey();
                                fm.file_id = fileR.request_file_id;
                                fm.file_path = fileR.file_path;
                                fm.file_name = fileR.file_name;
                                fm.file_size = fileR.file_size;
                                fm.file_title = fileR.file_name;
                                fm.file_table = "request_master_file";
                                if (string.IsNullOrWhiteSpace(Portals))
                                {
                                    fm.type_path = 0;
                                }
                                else if (Portals.Contains("ftp"))
                                {
                                    fm.type_path = 1;
                                }
                                else if (Portals.Contains("http"))
                                {
                                    fm.type_path = 2;
                                }
                                fm.module_key = "M12";
                                fm.role_access = null;
                                fm.user_access = "";
                                //var memberChat = db.chat_member.AsNoTracking().Where(z => z.chat_group_id == chatGroupNow.chat_group_id).Select(c => c.user_join).ToList();
                                //foreach (var userID in memberChat)
                                //{
                                //    fm.user_access += (fm.user_access != "" ? "," : "") + userID;
                                //}
                                fm.deny_access = null;
                                fm.created_by = uid;
                                fm.created_date = fileR.created_date;
                                fm.created_ip = ip;
                                fm.created_token_id = tid;
                                db.sys_file_mapping.Add(fm);
                                //});
                            }
                        }
                        
                        // detail request del
                        if (delFormDS.Count > 0)
                        {
                            db.request_master_detail.RemoveRange(delFormDS);
                        }
                        // detail request add
                        if (request_detail_add.Count > 0)
                        {
                            db.request_master_detail.AddRange(request_detail_add);
                        }
                        // follower
                        if (listFollowAdd.Count > 0)
                        {
                            db.request_follow.AddRange(listFollowAdd);
                        }
                        // follower del
                        if (userFollowDel.Count > 0)
                        {
                            db.request_follow.RemoveRange(userFollowDel);
                        }
                        // file add
                        if (listFileRequest.Count > 0)
                        {
                            db.request_master_file.AddRange(listFileRequest);
                        }

                        db.SaveChanges();

                        if (listPathFileUp.Count > 0)
                        {
                            foreach (var path in listPathFileUp)
                            {
                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                }
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "request/Add_Request", ip, tid, "Lỗi khi thêm mới đề xuất", 0, "request");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "request/Add_Request", ip, tid, "Lỗi khi thêm mới đề xuất", 0, "request");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Request([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
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
                    var das = await db.request_master.Where(a => id.Contains(a.request_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<request_master> del = new List<request_master>();
                        foreach (var da in das)
                        {
                            if (ad || da.created_by == uid)
                            {
                                var listFileLaw = db.request_master_file.Where(x => x.request_id == da.request_id).ToList();
                                if (listFileLaw.Count > 0)
                                {
                                    foreach (var item in listFileLaw)
                                    {
                                        var organization_id_law = db.request_master.Find(item.request_id) != null ? db.request_master.Find(item.request_id).organization_id.ToString() : "other";
                                        var pathFile = "/Portals/" + organization_id_law + "/";
                                        if (item.file_path != null && item.file_path.Contains(pathFile))
                                        {
                                            paths.Add(item.file_path);
                                        }
                                    }
                                }
                                del.Add(da);
                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    request_log log = new request_log();
                                    log.log_type = 2;
                                    log.title = "Xóa đề xuất: " + da.request_name;
                                    log.log_content = JsonConvert.SerializeObject(new { requestDel = JsonConvert.SerializeObject(da, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) });
                                    log.log_module = "request_master";
                                    log.id_key = da.request_id;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.request_log.Add(log);
                                    db.SaveChanges();

                                }
                                #endregion
                            }
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.request_master.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    foreach (string strPath in paths)
                    {
                        if (strPath.Contains("/Portals/") && strPath.Contains("/Request/") && !strPath.Contains("../"))
                        {
                            var strPathFormat = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = strPathFormat.Split('/');
                            var pathConfig = "";
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    pathConfig += "/" + Path.GetFileName(item);
                                }
                            }
                            var pathDelFile = HttpContext.Current.Server.MapPath("~/" + pathConfig);
                            bool existFiles = System.IO.Directory.Exists(pathDelFile);
                            if (existFiles)
                                System.IO.Directory.Delete(pathDelFile, true);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "request/Delete_Request", ip, tid, "Lỗi khi xoá đề xuất", 0, "request");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "request/Delete_Request", ip, tid, "Lỗi khi xoá đề xuất", 0, "request");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> UploadFileAttach()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string fname = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;

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

                    string root = HttpContext.Current.Server.MapPath("~/");

                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        string request_id_str = provider.FormData.GetValues("request_id").SingleOrDefault();
                        string request_id = JsonConvert.DeserializeObject<string>(request_id_str);
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        var request = db.request_master.AsNoTracking().Where(x => x.request_id == request_id).FirstOrDefault();
                        var file = provider.FileData;

                        if (file.Count > 0)
                        {
                            #region file
                            string path = root + "/Portals/" + request.organization_id + "/Request/" + request.request_id + "/";
                            bool exists = Directory.Exists(path);
                            if (!exists)
                            {
                                Directory.CreateDirectory(path);
                            }

                            List<request_master_file> dfs = new List<request_master_file>();
                            foreach (MultipartFileData fileData in provider.FileData)
                            {
                                string org_name_file = fileData.Headers.ContentDisposition.FileName;
                                if (org_name_file.StartsWith("\"") && org_name_file.EndsWith("\""))
                                {
                                    org_name_file = org_name_file.Trim('"');
                                }
                                if (org_name_file.Contains(@"/") || org_name_file.Contains(@"\"))
                                {
                                    org_name_file = System.IO.Path.GetFileName(org_name_file);
                                }
                                string name_file = "";
                                if (org_name_file.Length > 500)
                                {
                                    name_file = helper.UniqueFileName(org_name_file);
                                }
                                else
                                {
                                    name_file = (org_name_file);
                                }
                                string rootPath = path + "/" + name_file;
                                string Duongdan = "/Portals/" + request.organization_id + "/Request/" + request.request_id + "/" + name_file;

                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName).Replace("\"", "");
                                if (rootPath.Length > 500)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 500 - (path.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }

                                File.Move(fileData.LocalFileName, rootPath);
                                File.Delete(fileData.LocalFileName);
                                //File.Copy(fileData.LocalFileName, rootPathFile, true);
                                var df = new request_master_file();
                                df.request_file_id = helper.GenKey();
                                df.request_id = request.request_id;
                                df.file_name = name_file;
                                df.file_path = Duongdan;
                                df.file_type = Dinhdang;
                                var file_info = new FileInfo(rootPath);
                                df.file_size = file_info.Length;
                                df.is_type = 0;
                                df.is_image = helper.IsImageFileName(name_file);
                                if (df.is_image == true)
                                {
                                    //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                }
                                df.is_delete = false;
                                df.created_by = uid;
                                df.created_date = DateTime.Now;
                                df.created_ip = ip;
                                df.created_token_id = tid;
                                dfs.Add(df);
                            }
                            if (dfs.Count > 0)
                            {
                                db.request_master_file.AddRange(dfs);
                            }
                            #endregion
                        }
                        db.SaveChanges();
                        //notify
                        var listfollower = db.request_follow.Where(x => x.request_id == request.request_id).Select(x => x.user_id).Distinct().ToList();
                        var listUserSign = db.request_master_signuser.Where(x => x.request_id == request.request_id && x.status == true).Select(x => x.user_id).Distinct().ToList();
                        listfollower.Remove(uid);
                        listUserSign.Remove(uid);
                        var listuser = listfollower.Union(listUserSign);
                        foreach (var us in listuser)
                        {
                            helper.saveNotify(uid, us, null, "Đề xuất", "Thêm tệp tài liệu đính kèm đề xuất " + (request.request_name.Length > 100 ? request.request_name.Substring(0, 97) + "..." : request.request_name),
                                null, 7, 0, false, "M12", request.request_id, null, null, tid, ip);
                        }

                        #region add request_log
                        if (helper.wlog)
                        {
                            request_log log = new request_log();
                            log.id_key = request_id;
                            log.title = fname + " đã thêm tệp tài liệu";
                            log.log_type = 0;
                            log.log_content = JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(request, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) });
                            log.log_module = "request_master_file";
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.request_log.Add(log);
                            db.SaveChanges();
                        }
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "request/UploadFileAttach", ip, tid, "Lỗi khi thêm tệp tài liệu", 0, "request");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "request/UploadFileAttach", ip, tid, "Lỗi khi thêm tệp tài liệu", 0, "request");
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Send_Request()
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

                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        // type_send = 0: Chuyển theo quy trình , 1: Chuyển đến nhóm duyệt, 2: Chuyển đích danh
                        int type_send = int.Parse(provider.FormData.GetValues("type_send").SingleOrDefault());
                        var key_id = provider.FormData.GetValues("key_id").SingleOrDefault();
                        string content = provider.FormData.GetValues("content").SingleOrDefault();
                        string request_obj = provider.FormData.GetValues("request_obj").SingleOrDefault();
                        string request_form_id_send = provider.FormData.GetValues("request_form_id_send").SingleOrDefault();
                        List<request_master> listRequest = JsonConvert.DeserializeObject<List<request_master>>(request_obj);


                        List<request_ca_form_sign> signforms = new List<request_ca_form_sign>(); // Nhóm duyệt theo loại form đề xuất
                        List<request_form_sign_user> signuserforms = new List<request_form_sign_user>(); // Người duyệt trong nhóm của form đề xuất
                        sys_config_process processSystem = new sys_config_process(); // Quy trình hệ thống dùng chung
                        List<sys_approved_groups> signformsSystem = new List<sys_approved_groups>(); // Nhóm duyệt hệ thống dùng chung
                        List<sys_approved_users> signuserformsSystem = new List<sys_approved_users>(); // Người duyệt trong nhóm hệ thống dùng chung
                        #region Xử lý request
                        foreach (var item_request in listRequest)
                        {
                            #region hide code
                            //if (type_send == 0)
                            //{
                            //    if (key_id == "0")
                            //    {
                            //        signforms = db.request_ca_form_sign.AsNoTracking().Where(x => x.request_form_id == item_request.request_form_id && x.status == true).OrderBy(x => x.is_order).ToList();
                            //        var signform_ids = signforms.Select(x => x.request_form_sign_id).ToList();
                            //        signuserforms = db.request_form_sign_user.AsNoTracking().Where(x => signform_ids.Contains(x.request_form_sign_id) && x.status == true && x.user_id != uid).OrderBy(x => x.STT).ToList();
                            //    }
                            //    else
                            //    {
                            //        processSystem = db.sys_config_process.AsNoTracking().FirstOrDefault(x => x.config_process_id == int.Parse(key_id));
                            //        if (processSystem != null)
                            //        {
                            //            var listGroupLink = db.sys_process_link_approved.AsNoTracking().Where(x => x.config_process_id == processSystem.config_process_id).OrderBy(x => x.is_order).ToList();
                            //            foreach (var item in listGroupLink)
                            //            {
                            //                var groupSys = db.sys_approved_groups.AsNoTracking().FirstOrDefault(x => x.approved_groups_id == item.approved_groups_id);
                            //                if (groupSys != null)
                            //                {
                            //                    signformsSystem.Add(groupSys);
                            //                }
                            //            }
                            //            var signformsSystem_ids = signformsSystem.Select(x => x.approved_groups_id).ToList();
                            //            signuserformsSystem = db.sys_approved_users.AsNoTracking().Where(x => signformsSystem_ids.Contains(x.approved_groups_id) && x.user_id != uid).ToList();
                            //        }
                            //    }
                            //}
                            //else if (type_send == 1)
                            //{
                            //    if (request_form_id_send != "")
                            //    {
                            //        signforms = db.request_ca_form_sign.AsNoTracking().Where(x => x.request_form_sign_id == key_id).ToList();
                            //        var signform_ids = signforms.Select(x => x.request_form_sign_id).ToList();
                            //        signuserforms = db.request_form_sign_user.AsNoTracking().Where(x => signform_ids.Contains(x.request_form_sign_id) && x.status == true && x.user_id != uid).OrderBy(x => x.STT).ToList();
                            //    }
                            //    else
                            //    {
                            //        signformsSystem = db.sys_approved_groups.AsNoTracking().Where(x => x.approved_groups_id == int.Parse(key_id)).ToList();
                            //        var signformsSystem_ids = signformsSystem.Select(x => x.approved_groups_id).ToList();
                            //        signuserformsSystem = db.sys_approved_users.AsNoTracking().Where(x => signformsSystem_ids.Contains(x.approved_groups_id) && x.user_id != uid).ToList();
                            //    }
                            //}
                            //else if (type_send == 2)
                            //{
                            //    signuserforms = new List<request_form_sign_user>();
                            //}
                            #endregion
                            #region quy trinh cu

                            #endregion
                            var request = db.request_master.Find(item_request.request_id);
                            if (request != null)
                            {
                                //Send Message
                                List<string> sendUsers = new List<string>();
                                string sendTitle = "Đề xuất";
                                string sendContent = (name ?? "") + "vừa gửi đến bạn đề xuất chờ duyệt: \"" + request.request_name + "\".";
                                //Log
                                request_log log = new request_log();

                                request.is_type_send = type_send;
                                request.status = 1;
                                request.status_processing = 1;
                                request.modified_by = uid;
                                request.modified_date = DateTime.Now;
                                request.modified_ip = ip;
                                request.modified_token_id = tid;
                                request.start_send_date = DateTime.Now;
                                #region Gen quy trinh
                                if (type_send == 0) // gửi đến quy trình
                                {
                                    request_master_procedure procedure = new request_master_procedure();
                                    procedure.procedure_id = helper.GenKey();
                                    procedure.request_id = request.request_id;
                                    // Nhóm duyệt
                                    List<request_master_sign> signs = new List<request_master_sign>();
                                    List<request_master_signuser> signusers = new List<request_master_signuser>();
                                    int is_stepsign = 0;
                                    if (key_id == "0")
                                    {
                                        signforms = db.request_ca_form_sign.AsNoTracking().Where(x => x.request_form_id == item_request.request_form_id && x.status == true).OrderBy(x => x.is_order).ToList();
                                        var signform_ids = signforms.Select(x => x.request_form_sign_id).ToList();
                                        signuserforms = db.request_form_sign_user.AsNoTracking().Where(x => signform_ids.Contains(x.request_form_sign_id) && x.status == true && x.user_id != uid).OrderBy(x => x.STT).ToList();
                                        // Quy trình
                                        var name_type_request = db.request_ca_form.FirstOrDefault(x => x.request_form_id == request.request_form_id)?.request_form_name ?? "";
                                        procedure.procedure_name = "Quy trình duyệt loại đề xuất: " + name_type_request;
                                        int approved_type = int.Parse(provider.FormData.GetValues("approved_type").SingleOrDefault());
                                        procedure.is_type = approved_type;
                                        procedure.is_order = 0;
                                        procedure.organization_id = db.request_ca_form.AsNoTracking().Where(x => x.request_form_id == item_request.request_form_id).FirstOrDefault()?.organization_id;
                                        is_stepsign = 0;
                                        foreach (var sf in signforms)
                                        {
                                            is_stepsign++;
                                            request_master_sign sign = new request_master_sign();
                                            sign.sign_id = helper.GenKey();
                                            sign.request_id = request.request_id;
                                            sign.procedure_id = procedure.procedure_id;
                                            sign.sign_name = sf.group_name;
                                            sign.is_order = sf.is_order;
                                            sign.is_type = sf.type_process;
                                            sign.is_sign = false;
                                            sign.is_skip = false;
                                            switch (procedure.is_type)
                                            {
                                                case 0: // Duyệt tuần tự
                                                    sign.status = false;
                                                    if (is_stepsign == 1 || signusers.Count == 1)
                                                    {
                                                        sign.status = true;
                                                    }
                                                    break;
                                                case 1: // Duyệt một nhiều
                                                    sign.status = true;
                                                    break;
                                                case 2: // Duyệt ngẫu nhiên

                                                    break;
                                            }
                                            sign.created_by = uid;
                                            sign.created_date = DateTime.Now;
                                            sign.created_ip = ip;
                                            sign.created_token_id = tid;
                                            sign.organization_id = procedure.organization_id;
                                            signs.Add(sign);

                                            // Người lập
                                            request_master_signuser signusercreate = new request_master_signuser();
                                            if (sign.is_order == 1)
                                            {
                                                signusercreate.signuser_id = helper.GenKey();
                                                signusercreate.request_id = request.request_id;
                                                signusercreate.sign_id = sign.sign_id;
                                                signusercreate.user_id = uid;
                                                signusercreate.is_order = 0;
                                                signusercreate.is_type = 1; //Người lập
                                                signusercreate.is_sign = 2;
                                                signusercreate.sign_date = DateTime.Now;
                                                signusercreate.sign_content = content;
                                                signusercreate.read_date = DateTime.Now;
                                                signusercreate.status = true;
                                                signusercreate.created_by = uid;
                                                signusercreate.created_date = DateTime.Now;
                                                signusercreate.created_ip = ip;
                                                signusercreate.created_token_id = tid;
                                                signusers.Add(signusercreate);
                                            }
                                            //Người duyệt
                                            var signuserformfilters = signuserforms.Where(x => x.request_form_sign_id == sf.request_form_sign_id).ToList();
                                            int is_stepsignuser = 0;
                                            foreach (var signuserform in signuserformfilters)
                                            {
                                                is_stepsignuser++;
                                                request_master_signuser signuser = new request_master_signuser();
                                                signuser.signuser_id = helper.GenKey();
                                                signuser.request_id = request.request_id;
                                                signuser.sign_id = sign.sign_id;
                                                signuser.user_id = signuserform.user_id;
                                                signuser.is_order = signuserform.STT;
                                                signuser.is_type = signuserform.IsType;
                                                signuser.is_sign = 0;
                                                if (sign.status == true)
                                                {
                                                    switch (sign.is_type)
                                                    {
                                                        case 0:
                                                            signuser.status = false;
                                                            if (is_stepsignuser == 1)
                                                            {
                                                                signuser.status = true;
                                                            }
                                                            break;
                                                        case 1:
                                                            signuser.status = true;
                                                            break;
                                                        case 2:

                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    signuser.status = false;
                                                }
                                                signuser.created_by = uid;
                                                signuser.created_date = DateTime.Now;
                                                signuser.created_ip = ip;
                                                signuser.created_token_id = tid;
                                                signusers.Add(signuser);
                                            }
                                            if (signusers.Count > 0)
                                            {
                                                db.request_master_signuser.AddRange(signusers);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        processSystem = db.sys_config_process.AsNoTracking().FirstOrDefault(x => x.config_process_id == int.Parse(key_id));
                                        if (processSystem != null)
                                        {
                                            var listGroupLink = db.sys_process_link_approved.AsNoTracking().Where(x => x.config_process_id == processSystem.config_process_id).OrderBy(x => x.is_order).ToList();
                                            foreach (var item in listGroupLink)
                                            {
                                                var groupSys = db.sys_approved_groups.AsNoTracking().FirstOrDefault(x => x.approved_groups_id == item.approved_groups_id);
                                                if (groupSys != null)
                                                {
                                                    signformsSystem.Add(groupSys);
                                                }
                                            }
                                            var signformsSystem_ids = signformsSystem.Select(x => x.approved_groups_id).ToList();
                                            signuserformsSystem = db.sys_approved_users.AsNoTracking().Where(x => signformsSystem_ids.Contains(x.approved_groups_id) && x.user_id != uid).ToList();
                                            // Quy trình
                                            procedure.procedure_name = processSystem.config_process_name;
                                            procedure.is_type = processSystem.config_process_type == 1 ? 1 : (processSystem.config_process_type == 2 ? 0 : 2);
                                            procedure.is_order = processSystem.is_order;
                                            procedure.organization_id = processSystem.organization_id;
                                            is_stepsign = 0;
                                            var orderGroup = 1;
                                            foreach (var sf in signformsSystem)
                                            {
                                                is_stepsign++;
                                                request_master_sign sign = new request_master_sign();
                                                sign.sign_id = helper.GenKey();
                                                sign.request_id = request.request_id;
                                                sign.procedure_id = procedure.procedure_id;
                                                sign.sign_name = sf.approved_group_name;
                                                sign.is_order = orderGroup;
                                                sign.is_type = sf.approved_type == 1 ? 1 : (sf.approved_type == 2 ? 0 : 2);
                                                sign.is_sign = false;
                                                sign.is_skip = false;
                                                orderGroup++;
                                                switch (procedure.is_type)
                                                {
                                                    case 0: // Duyệt tuần tự
                                                        sign.status = false;
                                                        if (is_stepsign == 1 || signusers.Count == 1)
                                                        {
                                                            sign.status = true;
                                                        }
                                                        break;
                                                    case 1: // Duyệt một nhiều
                                                        sign.status = true;
                                                        break;
                                                    case 2: // Duyệt ngẫu nhiên

                                                        break;
                                                }
                                                sign.created_by = uid;
                                                sign.created_date = DateTime.Now;
                                                sign.created_ip = ip;
                                                sign.created_token_id = tid;
                                                sign.organization_id = procedure.organization_id;
                                                signs.Add(sign);

                                                // Người lập
                                                request_master_signuser signusercreate = new request_master_signuser();
                                                if (sign.is_order == 1)
                                                {
                                                    signusercreate.signuser_id = helper.GenKey();
                                                    signusercreate.request_id = request.request_id;
                                                    signusercreate.sign_id = sign.sign_id;
                                                    signusercreate.user_id = uid;
                                                    signusercreate.is_order = 0;
                                                    signusercreate.is_type = 1; //Người lập
                                                    signusercreate.is_sign = 2;
                                                    signusercreate.sign_date = DateTime.Now;
                                                    signusercreate.sign_content = content;
                                                    signusercreate.read_date = DateTime.Now;
                                                    signusercreate.status = true;
                                                    signusercreate.created_by = uid;
                                                    signusercreate.created_date = DateTime.Now;
                                                    signusercreate.created_ip = ip;
                                                    signusercreate.created_token_id = tid;
                                                    signusers.Add(signusercreate);
                                                }
                                                //Người duyệt
                                                var signuserformfilters = signuserformsSystem.Where(x => x.approved_groups_id == sf.approved_groups_id).ToList();
                                                int is_stepsignuser = 0;
                                                foreach (var signuserform in signuserformfilters)
                                                {
                                                    is_stepsignuser++;
                                                    request_master_signuser signuser = new request_master_signuser();
                                                    signuser.signuser_id = helper.GenKey();
                                                    signuser.request_id = request.request_id;
                                                    signuser.sign_id = sign.sign_id;
                                                    signuser.user_id = signuserform.user_id;
                                                    signuser.is_order = signuserform.is_order;
                                                    signuser.is_type = 0; // Người duyệt bình thường
                                                    signuser.is_sign = 0;
                                                    if (sign.status == true)
                                                    {
                                                        switch (sign.is_type)
                                                        {
                                                            case 0: // Duyệt tuần tự
                                                                signuser.status = false;
                                                                if (is_stepsignuser == 1)
                                                                {
                                                                    signuser.status = true;
                                                                }
                                                                break;
                                                            case 1: // Duyệt một nhiều
                                                                signuser.status = true;
                                                                break;
                                                            case 2: // Duyệt ngẫu nhiên

                                                                break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        signuser.status = false;
                                                    }
                                                    signuser.created_by = uid;
                                                    signuser.created_date = DateTime.Now;
                                                    signuser.created_ip = ip;
                                                    signuser.created_token_id = tid;
                                                    signusers.Add(signuser);
                                                }
                                                if (signusers.Count > 0)
                                                {
                                                    db.request_master_signuser.AddRange(signusers);
                                                }
                                            }
                                        }
                                    }
                                    procedure.status = true;
                                    procedure.created_by = uid;
                                    procedure.created_date = DateTime.Now;
                                    procedure.created_ip = ip;
                                    procedure.created_token_id = tid;
                                    db.request_master_procedure.Add(procedure);
                                    
                                    if (signs.Count > 0)
                                    {
                                        db.request_master_sign.AddRange(signs);
                                    }
                                    //send users
                                    sendUsers = signusers.Where(x => x.status == true && (x.is_sign == 0 || x.is_sign == 1)).Select(x => x.user_id).ToList();
                                }
                                else if (type_send == 1) // Gửi tới nhóm
                                {
                                    //Nhóm duyệt
                                    List<request_master_sign> signs = new List<request_master_sign>();
                                    List<request_master_signuser> signusers = new List<request_master_signuser>();
                                    if (request_form_id_send != "")
                                    {
                                        signforms = db.request_ca_form_sign.AsNoTracking().Where(x => x.request_form_sign_id == key_id).ToList();
                                        var signform_ids = signforms.Select(x => x.request_form_sign_id).ToList();
                                        signuserforms = db.request_form_sign_user.AsNoTracking().Where(x => signform_ids.Contains(x.request_form_sign_id) && x.status == true && x.user_id != uid).OrderBy(x => x.STT).ToList();
                                        
                                        foreach (var sf in signforms)
                                        {
                                            request_master_sign sign = new request_master_sign();
                                            sign.sign_id = helper.GenKey();
                                            sign.request_id = request.request_id;
                                            sign.sign_name = sf.group_name;
                                            sign.is_order = sf.is_order;
                                            sign.is_type = sf.type_process;
                                            sign.is_sign = false;
                                            sign.is_skip = false;
                                            sign.status = true;
                                            sign.created_by = uid;
                                            sign.created_date = DateTime.Now;
                                            sign.created_ip = ip;
                                            sign.created_token_id = tid;
                                            sign.organization_id = db.request_ca_form.AsNoTracking().Where(x => x.request_form_id == request.request_form_id).FirstOrDefault()?.organization_id;
                                            signs.Add(sign);

                                            //Người lập
                                            request_master_signuser signusercreate = new request_master_signuser();
                                            signusercreate.signuser_id = helper.GenKey();
                                            signusercreate.request_id = request.request_id;
                                            signusercreate.sign_id = sign.sign_id;
                                            signusercreate.user_id = uid;
                                            signusercreate.is_order = 0;
                                            signusercreate.is_type = 1;
                                            signusercreate.is_sign = 2;
                                            signusercreate.sign_date = DateTime.Now;
                                            signusercreate.sign_content = content;
                                            signusercreate.read_date = DateTime.Now;
                                            signusercreate.status = true;
                                            signusercreate.created_by = uid;
                                            signusercreate.created_date = DateTime.Now;
                                            signusercreate.created_ip = ip;
                                            signusercreate.created_token_id = tid;
                                            signusers.Add(signusercreate);
                                            //Người duyệt
                                            var signuserformfilters = signuserforms.Where(x => x.request_form_sign_id == sf.request_form_sign_id).ToList();
                                            int is_stepsignuser = 0;
                                            foreach (var signuserform in signuserformfilters)
                                            {
                                                is_stepsignuser++;
                                                request_master_signuser signuser = new request_master_signuser();
                                                signuser.signuser_id = helper.GenKey();
                                                signuser.request_id = request.request_id;
                                                signuser.sign_id = sign.sign_id;
                                                signuser.user_id = signuserform.user_id;
                                                signuser.is_order = signuserform.STT;
                                                signuser.is_type = signuserform.IsType;
                                                signuser.is_sign = 0;
                                                switch (sign.is_type)
                                                {
                                                    case 0: // Duyệt tuần tự
                                                        signuser.status = false;
                                                        if (is_stepsignuser == 1)
                                                        {
                                                            signuser.status = true;
                                                        }
                                                        break;
                                                    case 1: // Duyệt một nhiều
                                                        signuser.status = true;
                                                        break;
                                                    case 2: // Duyệt ngẫu nhiên

                                                        break;
                                                }
                                                signuser.created_by = uid;
                                                signuser.created_date = DateTime.Now;
                                                signuser.created_ip = ip;
                                                signuser.created_token_id = tid;
                                                signusers.Add(signuser);
                                            }
                                            if (signusers.Count > 0)
                                            {
                                                db.request_master_signuser.AddRange(signusers);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        signformsSystem = db.sys_approved_groups.AsNoTracking().Where(x => x.approved_groups_id == int.Parse(key_id)).ToList();
                                        var signformsSystem_ids = signformsSystem.Select(x => x.approved_groups_id).ToList();
                                        signuserformsSystem = db.sys_approved_users.AsNoTracking().Where(x => signformsSystem_ids.Contains(x.approved_groups_id) && x.user_id != uid).ToList();
                                        var sttGroup = db.request_master_sign.AsNoTracking().Count(x => x.request_id == request.request_id) > 0
                                                        ? (db.request_master_sign.AsNoTracking().Where(x => x.request_id == request.request_id).Max(x => x.is_order) + 1)
                                                        : 1;
                                        foreach (var sf in signformsSystem)
                                        {
                                            request_master_sign sign = new request_master_sign();
                                            sign.sign_id = helper.GenKey();
                                            sign.request_id = request.request_id;
                                            sign.sign_name = sf.approved_group_name;
                                            sign.is_order = sttGroup;
                                            sign.is_type = sf.approved_type == 1 ? 1 : (sf.approved_type == 2 ? 0 : 2);
                                            sign.is_sign = false;
                                            sign.is_skip = false;
                                            sign.status = true;
                                            sign.created_by = uid;
                                            sign.created_date = DateTime.Now;
                                            sign.created_ip = ip;
                                            sign.created_token_id = tid;
                                            sign.organization_id = sf.organization_id;
                                            signs.Add(sign);
                                            sttGroup++;
                                            //Người lập
                                            request_master_signuser signusercreate = new request_master_signuser();
                                            signusercreate.signuser_id = helper.GenKey();
                                            signusercreate.request_id = request.request_id;
                                            signusercreate.sign_id = sign.sign_id;
                                            signusercreate.user_id = uid;
                                            signusercreate.is_order = 0;
                                            signusercreate.is_type = 1;
                                            signusercreate.is_sign = 2;
                                            signusercreate.sign_date = DateTime.Now;
                                            signusercreate.sign_content = content;
                                            signusercreate.read_date = DateTime.Now;
                                            signusercreate.status = true;
                                            signusercreate.created_by = uid;
                                            signusercreate.created_date = DateTime.Now;
                                            signusercreate.created_ip = ip;
                                            signusercreate.created_token_id = tid;
                                            signusers.Add(signusercreate);
                                            //Người duyệt
                                            var signuserformfilters = signuserformsSystem.Where(x => x.approved_groups_id == sf.approved_groups_id).ToList();
                                            int is_stepsignuser = 0;
                                            foreach (var signuserform in signuserformfilters)
                                            {
                                                is_stepsignuser++;
                                                request_master_signuser signuser = new request_master_signuser();
                                                signuser.signuser_id = helper.GenKey();
                                                signuser.request_id = request.request_id;
                                                signuser.sign_id = sign.sign_id;
                                                signuser.user_id = signuserform.user_id;
                                                signuser.is_order = signuserform.is_order;
                                                signuser.is_type = 0;
                                                signuser.is_sign = 0;
                                                switch (sign.is_type)
                                                {
                                                    case 0: // Duyệt tuần tự
                                                        signuser.status = false;
                                                        if (is_stepsignuser == 1)
                                                        {
                                                            signuser.status = true;
                                                        }
                                                        break;
                                                    case 1: // Duyệt một nhiều
                                                        signuser.status = true;
                                                        break;
                                                    case 2: // Duyệt ngẫu nhiên

                                                        break;
                                                }
                                                signuser.created_by = uid;
                                                signuser.created_date = DateTime.Now;
                                                signuser.created_ip = ip;
                                                signuser.created_token_id = tid;
                                                signusers.Add(signuser);
                                            }
                                            if (signusers.Count > 0)
                                            {
                                                db.request_master_signuser.AddRange(signusers);
                                            }
                                        }
                                    }
                                    if (signs.Count > 0)
                                    {
                                        db.request_master_sign.AddRange(signs);
                                    }
                                    //send
                                    sendUsers = signusers.Where(x => x.status == true && (x.is_sign == 0 || x.is_sign == 1)).Select(x => x.user_id).ToList();
                                }
                                else if (type_send == 2) // Gửi đích danh
                                {
                                    signuserforms = new List<request_form_sign_user>();
                                    //Người lập
                                    List<request_master_signuser> signusers = new List<request_master_signuser>();
                                    request_master_signuser signusercreate = new request_master_signuser();
                                    signusercreate.signuser_id = helper.GenKey();
                                    signusercreate.request_id = request.request_id;
                                    signusercreate.user_id = uid;
                                    signusercreate.is_order = 0;
                                    signusercreate.is_type = 1;
                                    signusercreate.is_sign = 2;
                                    signusercreate.sign_date = DateTime.Now;
                                    signusercreate.sign_content = content;
                                    signusercreate.read_date = DateTime.Now;
                                    signusercreate.status = true;
                                    signusercreate.created_by = uid;
                                    signusercreate.created_date = DateTime.Now;
                                    signusercreate.created_ip = ip;
                                    signusercreate.created_token_id = tid;
                                    signusers.Add(signusercreate);

                                    //Người duyệt
                                    request_master_signuser signuser = new request_master_signuser();
                                    signuser.signuser_id = helper.GenKey();
                                    signuser.request_id = request.request_id;
                                    signuser.user_id = key_id;
                                    signuser.is_order = 1;
                                    signuser.is_type = 0;
                                    signuser.is_sign = 0;
                                    signuser.status = true;
                                    signuser.created_by = uid;
                                    signuser.created_date = DateTime.Now;
                                    signuser.created_ip = ip;
                                    signuser.created_token_id = tid;
                                    signusers.Add(signuser);

                                    if (signusers.Count > 0)
                                    {
                                        db.request_master_signuser.AddRange(signusers);
                                    }
                                    //send
                                    sendUsers = signusers.Where(x => x.status == true && (x.is_sign == 0 || x.is_sign == 1)).Select(x => x.user_id).ToList();
                                }
                                #endregion

                                #region file
                                string path = root + "/Portals/" + request.organization_id + "/Request/" + request.request_id;
                                // Format path
                                var pathFormatRoot = Regex.Replace(path.Replace("\\", "/"), @"\.*/+", "/");
                                var listPathRoot = pathFormatRoot.Split('/');
                                var pathConfigRoot = "";
                                var sttPartPath = 1;
                                foreach (var item in listPathRoot)
                                {
                                    if (item.Trim() != "")
                                    {
                                        if (sttPartPath == 1)
                                        {
                                            pathConfigRoot += (item);
                                        }
                                        else
                                        {
                                            pathConfigRoot += "/" + Path.GetFileName(item);
                                        }
                                    }
                                    sttPartPath++;
                                }
                                bool exists = Directory.Exists(pathConfigRoot);
                                if (!exists)
                                    Directory.CreateDirectory(pathConfigRoot);
                                List<request_master_file> dfs = new List<request_master_file>();
                                foreach (MultipartFileData fileData in provider.FileData)
                                {
                                    string org_name_file = fileData.Headers.ContentDisposition.FileName;
                                    if (org_name_file.StartsWith("\"") && org_name_file.EndsWith("\""))
                                    {
                                        org_name_file = org_name_file.Trim('"');
                                    }
                                    if (org_name_file.Contains(@"/") || org_name_file.Contains(@"\"))
                                    {
                                        org_name_file = System.IO.Path.GetFileName(org_name_file);
                                    }
                                    string name_file = org_name_file;
                                    string rootPath = pathConfigRoot + "/" + name_file;
                                    string Duongdan = "/Portals/" + request.organization_id + "/Request/" + request.request_id + "/" + name_file;
                                    string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                                    if (rootPath.Length > 350)
                                    {
                                        name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                        int le = 500 - (pathConfigRoot.Length + 1) - Dinhdang.Length;
                                        name_file = name_file.Substring(0, le) + Dinhdang;
                                    }
                                    if (File.Exists(rootPath))
                                    {
                                        File.Delete(rootPath);
                                    }
                                    File.Move(fileData.LocalFileName, rootPath);
                                    //File.Copy(fileData.LocalFileName, rootPathFile, true);
                                    var df = new request_master_file();
                                    df.request_file_id = helper.GenKey();
                                    df.request_id = request.request_id;
                                    df.file_name = name_file;
                                    df.file_path = Duongdan;
                                    df.file_type = Dinhdang;
                                    var file_info = new FileInfo(rootPath);
                                    df.file_size = file_info.Length;
                                    df.is_image = helper.IsImageFileName(name_file);
                                    if (df.is_image == true)
                                    {
                                        //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                    }
                                    df.is_type = 2;
                                    df.is_delete = false;
                                    df.created_by = uid;
                                    df.created_date = DateTime.Now;
                                    df.created_ip = ip;
                                    df.created_token_id = tid;
                                    dfs.Add(df);
                                }
                                if (dfs.Count > 0)
                                {
                                    db.request_master_file.AddRange(dfs);
                                }
                                #endregion

                                #region log
                                if (helper.wlog)
                                {
                                    log.title = "Xử lý đề xuất: " + request.request_code + " : " + request.request_name + " : " + content; 
                                    log.log_type = 3;
                                    log.log_content = JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(request, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) });
                                    log.id_key = request.request_id;
                                    log.log_module = "request_master";
                                    log.created_by = uid;
                                    log.created_date = DateTime.Now;
                                    log.created_ip = ip;
                                    log.created_token_id = tid;
                                    db.request_log.Add(log);
                                }
                                #endregion

                                #region Send Message
                                List<Notification> notifications = new List<Notification>();
                                if (sendUsers.Count > 0)
                                {
                                    send_message(uid, request.request_id, sendUsers, sendTitle, sendContent, 0);
                                    notifications.Add(new Notification()
                                    {
                                        request_id = request.request_id,
                                        uids = sendUsers,
                                        title = sendTitle,
                                        text = sendContent,
                                    });
                                }
                                #endregion
                            }
                        }
                        #endregion
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "request/Send_Request", ip, tid, "Lỗi khi gửi xử lý đề xuất", 0, "request");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "request/Send_Request", ip, tid, "Lỗi khi gửi xử lý đề xuất", 0, "request");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Approved_Request()
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
                    // Provider
                    string rootTemp = HttpContext.Current.Server.MapPath("~/");
                    bool existsTemp = Directory.Exists(rootTemp);
                    if (!existsTemp)
                        Directory.CreateDirectory(rootTemp);
                    var provider = new MultipartFormDataStreamProvider(rootTemp + "/Portals");
                    var task = await Request.Content.ReadAsMultipartAsync(provider);

                    // Params
                    List<Notification> notifications = new List<Notification>();
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    int is_type_approve = int.Parse(provider.FormData.GetValues("is_type_approve").SingleOrDefault());
                    string content = provider.FormData.GetValues("content").SingleOrDefault();
                    var rq = provider.FormData.GetValues("requests").SingleOrDefault();
                    List<string> requests = JsonConvert.DeserializeObject<List<string>>(rq);

                    foreach (var request_id in requests)
                    {
                        var request = await db.request_master.FindAsync(request_id);
                        if (request != null)
                        {
                            //Send Message
                            List<string> sendUsers = new List<string>();
                            string sendTitle = "Đề xuất";
                            string sendContent = (name ?? "") + "vừa gửi đến bạn đề xuất chờ duyệt: \"" + request.request_name + "\".";

                            request_master_procedure procedure = await db.request_master_procedure.FirstOrDefaultAsync(x => x.request_id == request_id && x.is_close != true);
                            var procedure_id = procedure?.procedure_id;
                            List<request_master_sign> signs = await db.request_master_sign.Where(x => x.request_id == request_id && x.procedure_id == procedure_id && x.is_close != true).OrderBy(x => x.is_order).ToListAsync();
                            List<request_master_signuser> signusers = await db.request_master_signuser.Where(x => x.request_id == request_id && x.is_close != true).OrderBy(x => x.is_order).ToListAsync();

                            #region next step
                            //Current step
                            var sign_current = signs.Where(x => x.is_sign != true).FirstOrDefault();
                            int stepsign_current = sign_current?.is_order ?? 0;
                            var sign_id = sign_current?.sign_id;
                            var signuser_currents = signusers.Where(x => (x.is_sign == 0 || x.is_sign == 1)).OrderBy(x => x.is_order).ToList();
                            if (procedure != null && procedure.is_type == 0) //Là quy trinh duyệt tuần tự
                            {
                                signuser_currents = signusers.Where(x => x.sign_id == sign_id && (x.is_sign == 0 || x.is_sign == 1)).OrderBy(x => x.is_order).ToList();
                            }

                            //Cập nhật nguời ký duyệt
                            var user_current = signuser_currents.FirstOrDefault(a => a.user_id == uid && a.status == true);
                            user_current.is_sign = 2;
                            user_current.sign_date = DateTime.Now;
                            user_current.sign_content = content;
                            user_current.read_date = DateTime.Now;
                            user_current.modified_by = uid;
                            user_current.modified_date = DateTime.Now;
                            user_current.modified_ip = ip;
                            user_current.modified_token_id = tid;

                            //Danh sách chưa ký
                            signuser_currents = signusers.Where(x => x.sign_id == user_current.sign_id && (x.is_sign == 0 || x.is_sign == 1)).OrderBy(x => x.is_order).ToList();
                            if (procedure != null && procedure.is_type == 0) //Là quy trinh duyệt tuần tự
                            {
                                signuser_currents = signusers.Where(x => x.sign_id == user_current.sign_id && (x.is_sign == 0 || x.is_sign == 1)).OrderBy(x => x.is_order).ToList();
                            }

                            switch (is_type_approve)
                            {
                                case 1: // Chấp thuận
                                    if (request.is_type_send == 0) // Gui theo quy trinh
                                    {
                                        if (procedure.is_type == 0) // quy trình duyệt lần lượt
                                        {
                                            var countapprove = 0;
                                            if (sign_current != null && sign_current.is_type == 1)
                                            {
                                                countapprove = signusers.Count(a => a.sign_id == sign_current.sign_id && a.is_type != 1 && (a.is_sign == 2 || a.is_sign == -1 || a.is_sign == -3));
                                            }
                                            while (sign_current != null && (signuser_currents.Count(a => a.is_sign == 0 || a.is_sign == 1) == 0 || countapprove > 0))
                                            {
                                                sign_current.is_sign = true;
                                                sign_current = signs.Where(a => a.is_sign != true && a.is_order > stepsign_current).OrderBy(a => a.is_order).FirstOrDefault();
                                                if (sign_current != null)
                                                {
                                                    stepsign_current = sign_current?.is_order ?? 0;
                                                    signuser_currents = signusers.Where(a => a.sign_id == sign_current.sign_id && (a.is_sign == 0 || a.is_sign == 1)).OrderBy(a => a.is_order).ToList();
                                                    countapprove = 0;
                                                    if (sign_current.is_type == 1)
                                                    {
                                                        countapprove = signuser_currents.Count(a => a.is_type != 1 && (a.is_sign == 2 || a.is_sign == -1 || a.is_sign == -3));
                                                    }
                                                }
                                            }
                                            if (sign_current != null && signuser_currents.Count > 0)
                                            {
                                                sign_current.status = true;
                                                switch (sign_current.is_type)
                                                {
                                                    case 0: //Nhóm duyệt lần lượt
                                                        var signuser = signuser_currents.FirstOrDefault();
                                                        signuser.status = true;
                                                        signuser.modified_by = uid;
                                                        signuser.modified_date = DateTime.Now;
                                                        signuser.modified_ip = ip;
                                                        signuser.modified_token_id = tid;
                                                        break;
                                                    case 1: //Nhóm duyệt 1 nhiều
                                                        foreach (var user in signuser_currents)
                                                        {
                                                            user.status = true;
                                                            user.modified_by = uid;
                                                            user.modified_date = DateTime.Now;
                                                            user.modified_ip = ip;
                                                            user.modified_token_id = tid;
                                                        }
                                                        break;
                                                    case 2: //Nhóm ngẫu nhiên

                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                request.status = 2; //Ban hành
                                                request.modified_by = uid;
                                                request.modified_date = DateTime.Now;
                                                request.modified_ip = ip;
                                                request.modified_token_id = tid;
                                            }
                                        }
                                        else if (procedure.is_type == 1) // quy trình duyệt 1 nhiều
                                        {
                                            sign_current = signs.FirstOrDefault(x => x.sign_id == user_current.sign_id);
                                            stepsign_current = sign_current?.is_order ?? 0;
                                            sign_current.is_sign = true;
                                            sign_current.status = true;
                                            if (sign_current != null && signuser_currents.Count > 0)
                                            {
                                                switch (sign_current.is_type)
                                                {
                                                    case 0: //Nhóm duyệt lần lượt
                                                        var signuser = signuser_currents.FirstOrDefault();
                                                        signuser.status = true;
                                                        signuser.modified_by = uid;
                                                        signuser.modified_date = DateTime.Now;
                                                        signuser.modified_ip = ip;
                                                        signuser.modified_token_id = tid;
                                                        break;
                                                    case 1: //Nhóm duyệt 1 nhiều
                                                        request.status = 2; // Hoan thanh
                                                        request.modified_by = uid;
                                                        request.modified_date = DateTime.Now;
                                                        request.modified_ip = ip;
                                                        request.modified_token_id = tid;
                                                        break;
                                                    case 2: //Nhóm ngẫu nhiên

                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                request.status = 2; // Hoan thanh
                                                request.modified_by = uid;
                                                request.modified_date = DateTime.Now;
                                                request.modified_ip = ip;
                                                request.modified_token_id = tid;
                                            }
                                        }
                                        else if (procedure.is_type == 2) // quy trình ngẫu nhiên
                                        {

                                        }
                                    }
                                    else if (request.is_type_send == 1)
                                    {
                                        if (sign_current != null && signuser_currents.Count > 0)
                                        {
                                            sign_current.status = true;
                                            switch (sign_current.is_type)
                                            {
                                                case 0: //Nhóm duyệt lần lượt
                                                    var signuser = signuser_currents.FirstOrDefault();
                                                    signuser.status = true;
                                                    signuser.modified_by = uid;
                                                    signuser.modified_date = DateTime.Now;
                                                    signuser.modified_ip = ip;
                                                    signuser.modified_token_id = tid;
                                                    break;
                                                case 1: //Nhóm duyệt 1 nhiều
                                                    foreach (var user in signuser_currents)
                                                    {
                                                        user.status = true;
                                                        user.modified_by = uid;
                                                        user.modified_date = DateTime.Now;
                                                        user.modified_ip = ip;
                                                        user.modified_token_id = tid;
                                                    }
                                                    request.status = 2; //Ban hành
                                                    request.modified_by = uid;
                                                    request.modified_date = DateTime.Now;
                                                    request.modified_ip = ip;
                                                    request.modified_token_id = tid;
                                                    break;
                                                case 2: //Nhóm ngẫu nhiên

                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            request.status = 2; //Ban hành
                                            request.modified_by = uid;
                                            request.modified_date = DateTime.Now;
                                            request.modified_ip = ip;
                                            request.modified_token_id = tid;
                                        }
                                    }
                                    else if (request.is_type_send == 2)
                                    {
                                        request.status = 2; //Hoan thanh
                                        request.modified_by = uid;
                                        request.modified_date = DateTime.Now;
                                        request.modified_ip = ip;
                                        request.modified_token_id = tid;
                                    }
                                    break;
                                case -2: // Trả lại
                                    user_current.is_sign = -1; //Trả lại
                                    request.status = -2; //Trả lại
                                    request.modified_by = uid;
                                    request.modified_date = DateTime.Now;
                                    request.modified_ip = ip;
                                    request.modified_token_id = tid;
                                    break;
                                case 2: // Hoan thanh
                                    user_current.is_sign = 2; // Ban hành
                                    request.status = 2; // Ban hành
                                    request.modified_by = uid;
                                    request.modified_date = DateTime.Now;
                                    request.modified_ip = ip;
                                    request.modified_token_id = tid;
                                    break;
                                case -1: // Hủy de xuat
                                    user_current.is_sign = -2; // Hủy de xuat
                                    request.status = -1; // Hủy de xuat
                                    request.modified_by = uid;
                                    request.modified_date = DateTime.Now;
                                    request.modified_ip = ip;
                                    request.modified_token_id = tid;
                                    break;
                                case 3: // Thu hồi
                                    user_current.is_sign = 3; // Thu hồi
                                    request.status = 3; // Thu hồi 
                                    request.modified_by = uid;
                                    request.modified_date = DateTime.Now;
                                    request.modified_ip = ip;
                                    request.modified_token_id = tid;
                                    break;
                            }

                            // Update
                            foreach (var item in signs)
                            {
                                var is_exists = db.request_master_sign.FirstOrDefault(x => x.sign_id == item.sign_id);
                                if (is_exists != null)
                                {
                                    db.Entry(item).State = EntityState.Modified;
                                }
                                else
                                {
                                    db.request_master_sign.Add(item);
                                }
                            }
                            foreach (var item in signusers)
                            {
                                var is_exists = db.request_master_signuser.FirstOrDefault(x => x.sign_id == item.sign_id);
                                if (is_exists != null)
                                {
                                    db.Entry(item).State = EntityState.Modified;
                                }
                                else
                                {
                                    db.request_master_signuser.Add(item);
                                }
                            }

                            //send
                            sendUsers = signuser_currents.Where(x => x.status == true && (x.is_sign == 0 || x.is_sign == 1)).Select(x => x.user_id).ToList();
                            #endregion

                            #region Comment
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                request_comment comment = new request_comment();
                                comment.request_comment_id = helper.GenKey();
                                comment.request_id = request_id;
                                comment.content = content;
                                comment.is_type = 1;
                                comment.type_comment = 0;
                                comment.is_delete = false;
                                comment.created_by = uid;
                                comment.created_date = DateTime.Now;
                                comment.created_ip = ip;
                                comment.created_token_id = tid;
                                db.request_comment.Add(comment);
                            }
                            #endregion

                            #region file
                            string root = HttpContext.Current.Server.MapPath("~/Portals");
                            string path = root + "/" + request.organization_id + "/Request/" + request_id;

                            // Format path
                            var pathFormatRoot = Regex.Replace(path.Replace("\\", "/"), @"\.*/+", "/");
                            var listPathRoot = pathFormatRoot.Split('/');
                            var pathConfigRoot = "";
                            var sttPartPath = 1;
                            foreach (var item in listPathRoot)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfigRoot += (item);
                                    }
                                    else
                                    {
                                        pathConfigRoot += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            bool exists = Directory.Exists(pathConfigRoot);
                            if (!exists)
                                Directory.CreateDirectory(pathConfigRoot);

                            List<request_master_file> dfs = new List<request_master_file>();
                            foreach (MultipartFileData fileData in provider.FileData)
                            {
                                string org_name_file = fileData.Headers.ContentDisposition.FileName;
                                if (org_name_file.StartsWith("\"") && org_name_file.EndsWith("\""))
                                {
                                    org_name_file = org_name_file.Trim('"');
                                }
                                if (org_name_file.Contains(@"/") || org_name_file.Contains(@"\"))
                                {
                                    org_name_file = System.IO.Path.GetFileName(org_name_file);
                                }
                                string name_file = org_name_file; //helper.UniqueFileName(org_name_file);
                                string rootPath = pathConfigRoot + "/" + name_file;
                                string Duongdan = "/Portals/" + request.organization_id + "/Request/" + request_id + "/" + name_file;
                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                                if (rootPath.Length > 350)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 350 - (pathConfigRoot.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                File.Move(fileData.LocalFileName, rootPath);
                                //File.Copy(fileData.LocalFileName, rootPathFile, true);
                                var df = new request_master_file();
                                df.request_file_id = helper.GenKey();
                                df.request_id = request.request_id;
                                df.file_name = name_file;
                                df.file_path = Duongdan;
                                df.file_type = Dinhdang;
                                var file_info = new FileInfo(rootPath);
                                df.file_size = file_info.Length;
                                df.is_image = helper.IsImageFileName(name_file);
                                if (df.is_image == true)
                                {
                                    //helper.ResizeImage(rootPathFile, 1024, 768, 90);
                                }
                                df.is_type = 2;
                                df.is_delete = false;
                                df.created_by = uid;
                                df.created_date = DateTime.Now;
                                df.created_ip = ip;
                                df.created_token_id = tid;
                                dfs.Add(df);
                            }
                            if (dfs.Count > 0)
                            {
                                db.request_master_file.AddRange(dfs);
                            }
                            #endregion

                            #region log
                            if (helper.wlog)
                            {
                                //Log
                                request_log log = new request_log();

                                switch (is_type_approve)
                                {
                                    case 0:
                                        log.title = "Duyệt lịch: " + content;
                                        break;
                                    case 1:
                                        log.title = "Trả lại: " + content;
                                        break;
                                    case 2:
                                        log.title = "Ban hành: " + content;
                                        break;
                                    case 3:
                                        log.title = "Hủy lịch: " + content;
                                        break;
                                }
                                log.log_type = 3;
                                log.log_content = JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(request, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) }); ;
                                log.id_key = request_id;
                                log.created_by = uid;
                                log.log_module = "request_master";
                                log.created_date = DateTime.Now;
                                log.created_ip = ip;
                                log.created_token_id = tid;
                                db.request_log.Add(log);
                            }
                            #endregion

                            await db.SaveChangesAsync();
                            #region Send Message
                            switch (request.status)
                            {
                                case 2:
                                    sendContent = "Vừa hoàn thành đề xuất: \"" + request.content + "\".";
                                    //Notify tất cả user tham gia lịch
                                    string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                                    var sqlpas = new List<SqlParameter>();
                                    sqlpas.Add(new SqlParameter("@" + "calendar_id", request.request_id));
                                    var arrpas = sqlpas.ToArray();
                                    var tasks = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "calendar_member_get", arrpas).Tables);
                                    var tables = await tasks;
                                    List<string> temps = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(tables[0]));
                                    List<string> users = temps.Where(x => !sendUsers.Contains(x)).ToList();
                                    send_message(uid, request.request_id, users, sendTitle, sendContent, 0);
                                    notifications.Add(new Notification()
                                    {
                                        request_id = request.request_id,
                                        uids = sendUsers,
                                        title = sendTitle,
                                        text = sendContent,
                                    });
                                    break;
                                case -2:
                                    sendContent = "Vừa trả lại đề xuất: \"" + request.content + "\".";
                                    break;
                                case -1:
                                    sendContent = "Vừa hủy đề xuất: \"" + request.content + "\".";
                                    break;
                            }
                            if (sendUsers.Count > 0)
                            {
                                send_message(uid, request.request_id, sendUsers, sendTitle, sendContent, 0);
                                notifications.Add(new Notification()
                                {
                                    request_id = request.request_id,
                                    uids = sendUsers,
                                    title = sendTitle,
                                    text = sendContent,
                                });
                            }
                            #endregion
                        }
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JsonConvert.SerializeObject(notifications) });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "request/Approved_Request", ip, tid, "Lỗi khi duyệt đề xuất", 0, "request");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "request/Approved_Request", ip, tid, "Lỗi khi duyệt đề xuất", 0, "request");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        #region Send Message
        public void send_message(string user_send, string id_key, [System.Web.Mvc.Bind(Include = "")][FromBody] List<string> users, string title, string content, int is_type)
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        #region Sendhub
                        List<sys_sendhub> sendhubs = new List<sys_sendhub>();
                        users = users.Where(x => x != user_send).ToList();
                        foreach (String user_id in users)
                        {
                            sys_sendhub sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.module_key = "M12";
                            sh.user_send = user_send;
                            sh.receiver = user_id;
                            sh.title = title;
                            sh.contents = content;
                            sh.type = 7; // Phê duyệt đề xuất
                            sh.is_type = is_type;
                            sh.seen = false;
                            sh.date_send = DateTime.Now;
                            sh.id_key = id_key;
                            sh.created_by = user_send;
                            sh.created_date = DateTime.Now;
                            sendhubs.Add(sh);
                        }
                        if (sendhubs.Count > 0)
                        {
                            db.sys_sendhub.AddRange(sendhubs);
                            await db.SaveChangesAsync();
                        }
                        #endregion
                        #region SendSocket
                        //send socket
                        var message = new Dictionary<string, dynamic>
                        {
                            { "event", "sendNotify" },
                            { "user_id", user_send },
                            { "title", title },
                            { "contents", content },
                            { "date", DateTime.Now },
                            { "uids", users },
                        };
                        if (helper.socketClient != null && helper.socketClient.Connected == true)
                        {
                            try
                            {
                                await helper.socketClient.EmitAsync("sendData", message);
                            }
                            catch { };
                        }
                        #endregion
                    }
                }
                catch { }
            });
        }

        public class Notification
        {

            public string request_id { get; set; }
            public List<string> uids { get; set; }
            public string title { get; set; }
            public string text { get; set; }
        }
        #endregion

        #region CallProc
        [HttpPost]
        public async Task<HttpResponseMessage> getData([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);

            try
            {
                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    var sqlpas = new List<SqlParameter>();
                    if (proc != null && proc.par != null)
                    {
                        foreach (sqlPar p in proc.par)
                        {
                            sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                        }
                    }
                    var arrpas = sqlpas.ToArray();
                    DateTime sdate = DateTime.Now;
                    var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables);
                    var tables = await task;
                    DateTime edate = DateTime.Now;
                    #region add SQLLog
                    if (helper.wlog)
                    {
                        using (DBEntities db = new DBEntities())
                        {
                            sql_log log = new sql_log();
                            log.controller = domainurl + "Proc/CallProc";
                            log.start_date = sdate;
                            log.end_date = edate;
                            log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                            log.user_id = uid;
                            log.token_id = tid;
                            log.created_ip = ip;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.modified_ip = ip;
                            log.modified_date = DateTime.Now;
                            log.modified_by = uid;
                            log.modified_token_id = tid;
                            log.full_name = name;
                            log.title = proc.proc;
                            log.log_content = JsonConvert.SerializeObject(new { data = proc });
                            db.sql_log.Add(log);
                            await db.SaveChangesAsync();
                        }
                    }
                    #endregion
                    string JSONresult = JsonConvert.SerializeObject(tables);
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0", proc_name = (helper.debug ? proc.proc : "") });
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);

                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "request/getData", ip, tid, "Lỗi khi gọi proc ", 0, "request");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "request/getData", ip, tid, "Lỗi khi gọi proc ", 0, "request");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
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
        #endregion
    }
}
