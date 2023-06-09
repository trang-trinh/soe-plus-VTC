﻿using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Helper;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.IO;
using OfficeOpenXml;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json.Linq;
using API.Helper;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class law_doc_signersController : ApiController
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

        [HttpPost]
        public async Task<HttpResponseMessage> Add_Signer([System.Web.Mvc.Bind(Include = "signer_name,signer_id,created_by,created_date,created_ip,created_token_id,nav_type")] law_doc_signers law_signer)
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
                    law_signer.created_by = uid;
                    law_signer.created_date = DateTime.Now;
                    law_signer.created_ip = ip;
                    law_signer.created_token_id = tid;
                    law_signer.nav_type = 0;
                    db.law_doc_signers.Add(law_signer);
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {

                        cms_logs log = new cms_logs();
                        log.log_title = "Thêm người ký " + law_signer.signer_name;
                        log.log_content = JsonConvert.SerializeObject(new { data = law_signer });
                        log.log_module = "Luật - Người ký";
                        log.id_key = law_signer.signer_id.ToString();
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.cms_logs.Add(log);
                        db.SaveChanges();

                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_doc_signers/Add_Signer", ip, tid, "Lỗi khi thêm người ký", 0, "law_doc_signers");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "law_doc_signers/Add_Signer", ip, tid, "Lỗi khi thêm người ký", 0, "law_doc_signers");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_Signer([System.Web.Mvc.Bind(Include = "signer_name,signer_id,modified_by,modified_date,modified_ip,modified_token_id")] law_doc_signers law_signer)
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdlang = "";
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
                    law_signer.modified_by = uid;
                    law_signer.modified_date = DateTime.Now;
                    law_signer.modified_ip = ip;
                    law_signer.modified_token_id = tid;
                    db.Entry(law_signer).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {

                        cms_logs log = new cms_logs();
                        log.log_title = "Sửa người ký " + law_signer.signer_name;
                        log.log_content = JsonConvert.SerializeObject(new { data = law_signer });
                        log.log_module = "Luật - Người ký";
                        log.id_key = law_signer.signer_id.ToString();
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.cms_logs.Add(log);
                        db.SaveChanges();

                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "law_doc_signers/Update_Signer", ip, tid, "Lỗi khi cập nhật người ký", 0, "law_doc_signers");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "law_doc_signers/Update_Signer", ip, tid, "Lỗi khi cập nhật người ký", 0, "law_doc_signers");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Signer([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                    var das = await db.law_doc_signers.Where(a => id.Contains(a.signer_id)).ToListAsync();
                    if (das != null)
                    {
                        List<law_doc_signers> del = new List<law_doc_signers>();
                        foreach (var da in das)
                        {
                            if (ad)
                            {
                                del.Add(da);

                            }
                            #region add cms_logs
                            if (helper.wlog)
                            {

                                cms_logs log = new cms_logs();
                                log.log_title = "Xóa người ký " + da.signer_name;
                                log.log_module = "Luật - Người ký";
                                log.id_key = da.signer_id.ToString();
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.cms_logs.Add(log);
                                db.SaveChanges();

                            }
                            #endregion
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.law_doc_signers.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "law_doc_signers/Delete_Signer", ip, tid, "Lỗi khi xoá người ký", 0, "law_doc_signers");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "law_doc_signers/Delete_Signer", ip, tid, "Lỗi khi xoá người ký", 0, "law_doc_signers");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_StatusSigners([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                    var ID_Change_Status = Int32.Parse(trangthai.IntID.ToString());
                    var das = db.law_doc_signers.Where(a => a.signer_id == ID_Change_Status).FirstOrDefault<law_doc_signers>();
                    if (das != null)
                    {
                        das.modified_by = uid;
                        das.modified_date = DateTime.Now;
                        das.modified_ip = ip;
                        das.modified_token_id = tid;
                        das.status = !trangthai.BitTrangthai;

                        #region add cms_logs
                        if (helper.wlog)
                        {

                            cms_logs log = new cms_logs();
                            log.log_title = "Sửa trạng thái người ký " + das.signer_name;

                            log.log_module = "Luật - Người ký";
                            log.id_key = das.signer_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.cms_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "law_doc_signers/Update_StatusSigners", ip, tid, "Lỗi khi cập nhật trạng thái người ký", 0, "law_doc_signers");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "law_doc_signers/Update_StatusSigners", ip, tid, "Lỗi khi cập nhật trạng thái người ký", 0, "law_doc_signers");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> ImportExcel()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
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
                    string strPath = "/Portals/" + organization_id_user + "/Excel/";

                    var listPathEdit_1 = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                    var pathEdit_1 = "";
                    foreach (var itemEdit in listPathEdit_1)
                    {
                        if (itemEdit.Trim() != "")
                        {
                            pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                        }
                    }
                    strPath = pathEdit_1;
                    bool exists = Directory.Exists(root + strPath);
                    if (!exists)
                        Directory.CreateDirectory(root + strPath);
                    var provider = new MultipartFormDataStreamProvider(root + strPath);
                    var task = Request.Content.ReadAsMultipartAsync(provider).ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        List<string> fileUpLocalRoot = new List<string>();
                        List<string> fileUpLocal = new List<string>();
                        MultipartFileData ffileData = provider.FileData.First();
                        FileInfo finfo = new FileInfo(ffileData.LocalFileName);
                        fileUpLocalRoot.Add(ffileData.LocalFileName);
                        string guid = Guid.NewGuid().ToString();

                        File.Move(finfo.FullName, Path.Combine(root + strPath, guid + "_" + ffileData.Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + "/" + guid + "_" + ffileData.Headers.ContentDisposition.FileName.Replace("\"", "");
                        fileUpLocal.Add(fpath);
                        FileInfo temp = new FileInfo(root + fpath);
                        //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<law_doc_signers> dvs = new List<law_doc_signers>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                //var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                                var is_admin = user_now.organization_id;
                                if (user_now != null && user_now.is_super == true)
                                {
                                    is_admin = 0;
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    law_doc_signers dv = new law_doc_signers();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value == null)
                                        {
                                            break;
                                        }
                                        var column = ws.Cells[3, j].Value;
                                        var vl = ws.Cells[i, j].Value;
                                        switch (column)
                                        {
                                            case "signer_name":
                                                dv.signer_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "position":
                                                dv.position = vl != null ? vl.ToString() : null;
                                                break;
                                            case "department_id":
                                                if (vl != null)
                                                { dv.department_id = int.Parse(vl.ToString()); }
                                                else
                                                { dv.department_id = null; }
                                                break;
                                            case "is_order":
                                                dv.is_order = vl != null ? int.Parse(vl.ToString()) : 1;
                                                break;
                                            case "status":
                                                if (vl.ToString().ToUpper().Trim() == "CÓ" || vl.ToString().ToUpper().Trim() == "HIỂN THỊ") { dv.status = true; }
                                                else { dv.status = false; }
                                                break;
                                        }
                                    }
                                    dv.nav_type = 0;
                                    dv.organization_id = is_admin;
                                    dv.created_by = name;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.law_doc_signers.AddRange(dvs);
                                }
                                db.SaveChanges();
                                if (fileUpLocal.Count > 0)
                                {
                                    foreach (var itemPath in fileUpLocal)
                                    {
                                        var listPathEdit = Regex.Replace(itemPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                        var pathEdit = "";
                                        foreach (var itemEdit in listPathEdit)
                                        {
                                            if (itemEdit.Trim() != "")
                                            {
                                                pathEdit += "/" + Path.GetFileName(itemEdit);
                                            }
                                        }
                                        string pathDel = root + pathEdit;
                                        if (System.IO.File.Exists(pathDel))
                                        {
                                            System.IO.File.Delete(pathDel);
                                        }
                                    }
                                }
                                if (fileUpLocalRoot.Count > 0)
                                {
                                    foreach (var path in fileUpLocalRoot)
                                    {
                                        if (System.IO.File.Exists(path))
                                        {
                                            System.IO.File.Delete(path);
                                        }
                                    }
                                }
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", ms = "ok" });
                            }
                            catch (DbEntityValidationException e)
                            {
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e.Message });
                            }
                            catch (Exception e)
                            {
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e.Message });
                            }

                        }
                        //return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/law_doc_signers/ImportExcel", ip, tid, "Lỗi khi Import Excel", 0, "law_doc_signers");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/law_doc_signers/ImportExcel", ip, tid, "Lỗi khi Import Excel", 0, "law_doc_signers");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }
        // end

        #region CallProc
        [HttpPost]
        public async Task<HttpResponseMessage> GetDataProc([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);
            string nameErrProc = "Lỗi khi gọi proc";// + proc.proc + "'";

            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
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
                        log.controller = domainurl + "law_doc_signers/GetDataProc";
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "law_doc_signers/GetDataProc", ip, tid, nameErrProc, 0, "law_doc_signers");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "law_doc_signers/GetDataProc", ip, tid, nameErrProc, 0, "law_doc_signers");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }


        }
        #endregion

    }
}
