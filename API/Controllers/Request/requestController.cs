﻿using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Metadata.Edm;
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
using System.Web.Services.Description;

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
                    string jwtcookie = HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies["jwt"] != null ? HttpContext.Current.Request.Cookies["jwt"].Value : null;
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
                            fileR.file_type = helper.GetFileExtension(fileName);
                            fileR.file_name = nameFileOrigin;
                            fileR.file_path = pathFile;
                            fileR.file_size = new FileInfo(fileData.LocalFileName).Length;
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
                                //File.Move(ffileData.LocalFileName, root + pathEdit_1);
                                //listPathFileUp.Add(ffileData.LocalFileName);
                                helper.UploadFileToDestination(jwtcookie, root, ffileData, pathEdit_1, 360, 360);

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

                        if (request_detail_add.Count > 0)
                        {
                            db.request_master_detail.AddRange(request_detail_add);
                        }
                        if (listFileRequest.Count > 0)
                        {
                            db.request_master_file.AddRange(listFileRequest);
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
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
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
