﻿using System;
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
using GemBox.Document;
using Helper;
using HtmlAgilityPack;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Controllers.HRM.Category
{
    [Authorize(Roles = "login")]
    public class hrm_ca_type_decisionController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_ca_type_decision()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_type_decision = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

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

                    string strPath = root + "/" + dvid + "/Typedecision";
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
                        fdca_type_decision = provider.FormData.GetValues("hrm_ca_type_decision").SingleOrDefault();
                        hrm_ca_type_decision ca_type_decision = JsonConvert.DeserializeObject<hrm_ca_type_decision>(fdca_type_decision);


                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                        if (!super)
                        {
                            var sytx = ca_type_decision.report_key;
                            ca_type_decision.report_key = null;
                            ca_type_decision.is_system = true;
                            ca_type_decision.organization_id = int.Parse(dvid);
                            ca_type_decision.created_by = uid;
                            ca_type_decision.created_date = DateTime.Now;
                            ca_type_decision.created_ip = ip;
                            ca_type_decision.created_token_id = tid;
                            db.hrm_ca_type_decision.Add(ca_type_decision);
                            db.SaveChanges();
                            hrm_smartreport_link hrm_Smartreport_Link = new hrm_smartreport_link();
                            hrm_Smartreport_Link.key_id = ca_type_decision.type_decision_id;
                            hrm_Smartreport_Link.report_key = sytx;
                            hrm_Smartreport_Link.report_type = 2;
                            hrm_Smartreport_Link.organization_id = int.Parse(dvid);
                            hrm_Smartreport_Link.created_by = uid;
                            hrm_Smartreport_Link.created_date = DateTime.Now;
                            hrm_Smartreport_Link.created_ip = ip;
                            hrm_Smartreport_Link.created_token_id = tid;
                            db.hrm_smartreport_link.Add(hrm_Smartreport_Link);
                            db.SaveChanges();
                        }
                        else
                        {
                            ca_type_decision.is_system = true;
                            ca_type_decision.organization_id = int.Parse(dvid);
                            ca_type_decision.created_by = uid;
                            ca_type_decision.created_date = DateTime.Now;
                            ca_type_decision.created_ip = ip;
                            ca_type_decision.created_token_id = tid;
                            db.hrm_ca_type_decision.Add(ca_type_decision);
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Typedecision", fileName);
                            fileInfo = new FileInfo(newFileName);
                            //if (fileInfo.Exists)
                            //{
                            //    fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            //    fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                            //    newFileName = Path.Combine(root + "/" + dvid + "/Typedecision", fileName);
                            //}
                            newFileName = Path.Combine(root + "/" + dvid + "/Typedecision", helper.newFileName(fileInfo, root + "/" + dvid + "/Typedecision", newFileName, 1, root, int.Parse(dvid)));

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
                            hrm_File.key_id = ca_type_decision.type_decision_id.ToString();
                            hrm_File.file_path = newFileName;
                            hrm_File.file_type = helper.GetFileExtension(newFileName);
                            var file_info = new FileInfo(strPath + "/" + Path.GetFileName(newFileName));
                            hrm_File.file_size = file_info.Length;
                            if (helper.IsImageFileName(newFileName))
                            {
                                hrm_File.is_image = true;
                            }
                            else
                            {
                                hrm_File.is_image = false;
                            }
                            hrm_File.is_type = 8;
                            if (super == true)
                            {
                                hrm_File.is_system = true;
                            }
                            else
                            {
                                hrm_File.is_system = false;
                            }
                            hrm_File.status = true;
                            hrm_File.created_by = uid;
                            hrm_File.created_date = DateTime.Now;
                            hrm_File.organization_id = int.Parse(dvid);
                            hrm_File.created_ip = ip;
                            hrm_File.created_token_id = tid;
                            db.hrm_file.Add(hrm_File); db.SaveChanges();
                            try
                            {
                                string FilePath = newFileName;

                                var fileHtml = new FileInfo(newFileName);
                                var newFileHTML = Path.GetFileName(newFileName).Substring(0, Path.GetFileName(newFileName).LastIndexOf("."));
                                newFileHTML += ".html";

                                newFileHTML = Path.Combine(root + "/" + dvid + "/Typedecision", helper.newFileName(fileInfo, root + "/" + dvid + "/Typedecision", newFileHTML, 1, root, int.Parse(dvid)));
                                if (!File.Exists(newFileHTML))
                                {
                                    var newFilehtm = File.Create(newFileHTML);
                                    newFilehtm.Close();
                                }
                                if (Path.GetFileName(newFileName).Contains(".pdf"))
                                {
                                    //string html = GetText(FilePath).Replace("@$", "");
                                    //System.IO.File.WriteAllText(newFileHTML, html);
                                }
                                else
                                {
                                    ComponentInfo.SetLicense("DTZX-HTZ5-B7Q6-2GA6");
                                    DocumentModel.Load(FilePath).Save(newFileHTML, SaveOptions.HtmlDefault);
                                }
                                HtmlDocument doc = new HtmlDocument();
                                using (var stream = new FileStream(path: newFileHTML, mode: FileMode.Open))
                                {
                                    doc.Load(stream, System.Text.Encoding.UTF8);
                                    var html = doc.DocumentNode.OuterHtml.Trim();
                                    var text = doc.DocumentNode.InnerText;
                                    ca_type_decision.content = html;
                                    db.Entry(ca_type_decision).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                try
                                {
                                    System.IO.File.Delete(newFileHTML);
                                    //System.IO.File.Delete(htmlpath);
                                }
                                catch (Exception)
                                {

                                }
                            }
                            catch (Exception e)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = e.Message, err = "1" });

                            }
                        }



                        #region add hrm_log
                        if (helper.wlog)
                        {

                            hrm_log log = new hrm_log();
                            log.title = "Thêm loại hợp đồng " + ca_type_decision.type_decision_name;

                            log.log_module = "ca_type_decision";
                            log.log_type = 0;
                            log.id_key = ca_type_decision.type_decision_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_type_decision, contents }), domainurl + "hrm_ca_type_decision/Add_ca_type_decision", ip, tid, "Lỗi khi thêm loại hợp đồng", 0, "loại hợp đồng");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_type_decision, contents }), domainurl + "hrm_ca_type_decision/Add_ca_type_decision", ip, tid, "Lỗi khi thêm loại hợp đồng", 0, "loại hợp đồng  ");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_hrm_ca_type_decision()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_type_decision = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            int dvid = int.Parse(claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value);
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
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
                    bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/" + dvid + "/Typedecision";
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
                        fdca_type_decision = provider.FormData.GetValues("hrm_ca_type_decision").SingleOrDefault();
                        hrm_ca_type_decision ca_type_decision = JsonConvert.DeserializeObject<hrm_ca_type_decision>(fdca_type_decision);
                        if (!super)
                        {
                            var ca_type_decision_old = db.hrm_ca_type_decision.AsNoTracking().Where(s => s.type_decision_id == ca_type_decision.type_decision_id).FirstOrDefault();
                            var sytx = ca_type_decision.report_key;
                            if (ca_type_decision_old != null)
                                ca_type_decision.report_key = ca_type_decision_old.report_key;
                            else
                                ca_type_decision.report_key = null;
                            ca_type_decision.modified_by = uid;
                            ca_type_decision.modified_date = DateTime.Now;
                            ca_type_decision.modified_ip = ip;

                            ca_type_decision.modified_token_id = tid;
                            db.Entry(ca_type_decision).State = EntityState.Modified;
                            db.SaveChanges();

                            var hrm_smartreport_link_old = db.hrm_smartreport_link.AsNoTracking().Where(s => s.report_type == 2 &&
                            s.key_id == ca_type_decision.type_decision_id && s.organization_id == dvid).FirstOrDefault();
                            if (hrm_smartreport_link_old == null)
                            {
                                hrm_smartreport_link hrm_Smartreport_Link = new hrm_smartreport_link();
                                hrm_Smartreport_Link.key_id = ca_type_decision.type_decision_id;
                                hrm_Smartreport_Link.report_key = sytx;
                                hrm_Smartreport_Link.report_type = 2;
                                hrm_Smartreport_Link.organization_id = dvid;
                                hrm_Smartreport_Link.created_by = uid;
                                hrm_Smartreport_Link.created_date = DateTime.Now;
                                hrm_Smartreport_Link.created_ip = ip;
                                hrm_Smartreport_Link.created_token_id = tid;
                                db.hrm_smartreport_link.Add(hrm_Smartreport_Link);
                                db.SaveChanges();
                            }
                            else
                            {
                                hrm_smartreport_link_old.report_key = sytx;
                                hrm_smartreport_link_old.modified_by = uid;
                                hrm_smartreport_link_old.modified_date = DateTime.Now;
                                hrm_smartreport_link_old.modified_ip = ip;
                                hrm_smartreport_link_old.modified_token_id = tid;
                                db.Entry(hrm_smartreport_link_old).State = EntityState.Modified;
                                db.SaveChanges();

                            }
                        }
                        else
                        {

                            ca_type_decision.modified_by = uid;
                            ca_type_decision.modified_date = DateTime.Now;
                            ca_type_decision.modified_ip = ip;
                            ca_type_decision.modified_token_id = tid;
                            db.Entry(ca_type_decision).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        var hrm_Files = "";
                        List<string> paths = new List<string>();
                        hrm_Files = provider.FormData.GetValues("hrm_files").SingleOrDefault();
                        List<hrm_file> hrm_File_S = JsonConvert.DeserializeObject<List<hrm_file>>(hrm_Files);
                        var arc = hrm_File_S.FindAll(x => x.organization_id == dvid).ToList();
                        var id = ca_type_decision.type_decision_id.ToString();
                        var hrmfile_Delete = new List<hrm_file>();
                        var hrm_file_Olds = db.hrm_file.Where(s => s.is_type == 8 && s.key_id == id && s.organization_id == dvid).ToArray<hrm_file>();
                        foreach (var item in hrm_file_Olds)
                        {
                            var check = false;
                            foreach (var element in arc)
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
                            newFileName = Path.Combine(root + "/" + dvid + "/Typedecision", fileName);
                            fileInfo = new FileInfo(newFileName);
                            //if (fileInfo.Exists)
                            //{
                            //    fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            //    fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                            //    newFileName = Path.Combine(root + "/" + dvid + "/Typedecision", fileName);
                            //}
                            var fileNameNew = helper.newFileName(fileInfo, root + "/" + dvid + "/Typedecision", newFileName, 1, root, dvid);
                            newFileName = Path.Combine(root + "/" + dvid + "/Typedecision", fileNameNew);
                            ffileData = fileData;
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }
                            var hrmfile_Dels = new List<hrm_file>();
                            if (super == true)
                            {
                                var hrm_file_dels = db.hrm_file.Where(s => s.is_type == 8 && s.key_id == id && s.organization_id == dvid && s.is_system == true).ToArray<hrm_file>();
                                foreach (var item in hrm_file_dels)
                                {
                                    paths.Add(item.file_path);
                                    hrmfile_Dels.Add(item);
                                }
                                db.hrm_file.RemoveRange(hrmfile_Dels);
                                db.SaveChanges();
                            }
                            else
                            {
                                var hrm_file_dels = db.hrm_file.Where(s => s.is_type == 8 && s.key_id == id && s.organization_id == dvid).ToArray<hrm_file>();
                                foreach (var item in hrm_file_dels)
                                {
                                    paths.Add(item.file_path);
                                    hrmfile_Dels.Add(item);
                                }
                                db.hrm_file.RemoveRange(hrmfile_Dels);
                                db.SaveChanges();
                            }
                            hrm_file hrm_File = new hrm_file();
                            hrm_File.key_id = ca_type_decision.type_decision_id.ToString();
                            hrm_File.file_name = Path.GetFileName(newFileName);
                            hrm_File.file_path = newFileName;
                            hrm_File.file_type = helper.GetFileExtension(newFileName);
                            var file_info = new FileInfo(strPath + "/" + Path.GetFileName(newFileName));
                            hrm_File.file_size = file_info.Length;
                            if (helper.IsImageFileName(newFileName))
                            {
                                hrm_File.is_image = true;
                            }
                            else
                            {
                                hrm_File.is_image = false;
                            }
                            if (super == true)
                            {
                                hrm_File.is_system = true;
                            }
                            else
                            {
                                hrm_File.is_system = false;
                            }
                            hrm_File.is_type = 8;
                            hrm_File.status = true;
                            hrm_File.created_by = uid;
                            hrm_File.created_date = DateTime.Now;
                            hrm_File.created_ip = ip; hrm_File.organization_id = dvid;
                            hrm_File.created_token_id = tid;
                            db.hrm_file.Add(hrm_File); db.SaveChanges();
                            try
                            {
                                string FilePath = newFileName;

                                var fileHtml = new FileInfo(newFileName);
                                var newFileHTML = Path.GetFileName(newFileName).Substring(0, Path.GetFileName(newFileName).LastIndexOf("."));
                                newFileHTML += ".html";

                                newFileHTML = Path.Combine(root + "/" + dvid + "/Typedecision", helper.newFileName(fileInfo, root + "/" + dvid + "/Typedecision", newFileHTML, 1, root, dvid));
                                if (!File.Exists(newFileHTML))
                                {
                                    var newFilehtm = File.Create(newFileHTML);
                                    newFilehtm.Close();
                                }



                                if (Path.GetFileName(newFileName).Contains(".pdf"))
                                {
                                    //string html = GetText(FilePath).Replace("@$", "");
                                    //System.IO.File.WriteAllText(newFileHTML, html);
                                }
                                else
                                {
                                    ComponentInfo.SetLicense("DTZX-HTZ5-B7Q6-2GA6");
                                    DocumentModel.Load(FilePath).Save(newFileHTML, SaveOptions.HtmlDefault);
                                }
                                HtmlDocument doc = new HtmlDocument();
                                using (var stream = new FileStream(path: newFileHTML, mode: FileMode.Open))
                                {
                                    doc.Load(stream, System.Text.Encoding.UTF8);
                                    var html = doc.DocumentNode.OuterHtml.Trim();

                                    ca_type_decision.content = html;
                                    db.Entry(ca_type_decision).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                try
                                {
                                    System.IO.File.Delete(newFileHTML);
                                    //System.IO.File.Delete(htmlpath);
                                }
                                catch (Exception)
                                {

                                }
                            }
                            catch (Exception e)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = e.Message, err = "1" });

                            }
                        }



                        foreach (string strP in paths)
                        {

                            bool exists = File.Exists(root + "/" + dvid + "/Typedecision/" + Path.GetFileName(strP));
                            if (exists)
                                System.IO.File.Delete(root + "/" + dvid + "/Typedecision/" + Path.GetFileName(strP));
                        }

                        #region add hrm_log
                        if (helper.wlog)
                        {

                            hrm_log log = new hrm_log();
                            log.title = "Sửa loại hợp đồng " + ca_type_decision.type_decision_name;

                            log.log_module = "ca_type_decision";
                            log.log_type = 1;
                            log.id_key = ca_type_decision.type_decision_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_type_decision, contents }), domainurl + "hrm_ca_type_decision/Update_ca_type_decision", ip, tid, "Lỗi khi cập nhật ca_type_decision", 0, "ca_type_decision");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_type_decision, contents }), domainurl + "hrm_ca_type_decision/Update_ca_type_decision", ip, tid, "Lỗi khi cập nhật ca_type_decision", 0, "ca_type_decision");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> delete_hrm_ca_type_decision([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                        var das = await db.hrm_ca_type_decision.Where(a => id.Contains(a.type_decision_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        string root = HttpContext.Current.Server.MapPath("~/Portals");
                        if (das != null)
                        {
                            List<hrm_ca_type_decision> del = new List<hrm_ca_type_decision>();
                            foreach (var da in das)
                            {
                                del.Add(da);
                                var arr = new List<String>();
                                foreach (var item in id)
                                {
                                    arr.Add(item.ToString());
                                }

                                var das2 = await db.hrm_smartreport_link.Where(a => a.key_id == da.type_decision_id && a.report_type == 2).ToListAsync();
                                db.hrm_smartreport_link.RemoveRange(das2);
                                db.SaveChanges();
                                var das3 = await db.hrm_file.Where(a => arr.Contains(a.key_id) && a.is_type == 8).ToListAsync();


                                foreach (var item in das3)
                                {


                                    if (!string.IsNullOrWhiteSpace(item.file_path))
                                        paths.Add(item.file_path.Substring(8));

                                }
                                db.hrm_file.RemoveRange(das3);

                                foreach (string strP in paths)
                                {

                                    bool exists = File.Exists(root + "/" + dvid + "/Typedecision/" + Path.GetFileName(strP));
                                    if (exists)
                                        System.IO.File.Delete(root + "/" + dvid + "/Typedecision/" + Path.GetFileName(strP));
                                }
                                #region add hrm_log
                                if (helper.wlog)
                                {

                                    hrm_log log = new hrm_log();
                                    log.title = "Xóa loại hợp đồng " + da.type_decision_name;

                                    log.log_module = "ca_type_decision";
                                    log.log_type = 2;
                                    log.id_key = da.type_decision_id.ToString();
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    db.hrm_log.Add(log);
                                    db.SaveChanges();

                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.hrm_ca_type_decision.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_ca_type_decision/Delete_ca_type_decision", ip, tid, "Lỗi khi xoá loại hợp đồng", 0, "ca_type_decision");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "hrm_ca_type_decision/Delete_ca_type_decision", ip, tid, "Lỗi khi xoá loại hợp đồng", 0, "ca_type_decision");
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
        public async Task<HttpResponseMessage> update_s_hrm_ca_type_decision([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                        var das = db.hrm_ca_type_decision.Where(a => (a.type_decision_id == int_id)).FirstOrDefault<hrm_ca_type_decision>();
                        if (das != null)
                        {
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            das.status = !trangthai.BitTrangthai;


                            #region add hrm_log
                            if (helper.wlog)
                            {

                                hrm_log log = new hrm_log();
                                log.title = "Sửa loại hợp đồng " + das.type_decision_name;

                                log.log_module = "ca_type_decision";
                                log.log_type = 1;
                                log.id_key = das.type_decision_id.ToString();
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_ca_type_decision/Update_Trangthaica_type_decision", ip, tid, "Lỗi khi cập nhật trạng thái loại hợp đồng", 0, "hrm_ca_type_decision");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "hrm_ca_type_decision/Update_Trangthaica_type_decision", ip, tid, "Lỗi khi cập nhật trạng thái loại hợp đồng", 0, "hrm_ca_type_decision");
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