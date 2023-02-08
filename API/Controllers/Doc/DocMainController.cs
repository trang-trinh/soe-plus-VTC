using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Web.Services.Description;
using API.Helper;
using API.Models;
using GleamTech.DocumentUltimate;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Web.Razor.Parser.SyntaxConstants;
//using Spire.Pdf.Exporting.XPS.Schema;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class DocMainController : ApiController
    {
        private const string const_module_key = "M3";
        public string getipaddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "localhost";
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_ViewDoc(doc_follows model)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            var follow_id = model.follow_id;
            var doc_master_id = model.doc_master_id;
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
                        if (!String.IsNullOrEmpty(follow_id))
                        {
                            var fl = await db.doc_follows.FindAsync(follow_id);
                            if (fl != null && fl.view_date == null)
                            {
                                fl.view_date = DateTime.Now;
                            }
                        }
                        var user = await db.sys_users.FindAsync(uid);
                        var doc = await db.doc_views.FirstOrDefaultAsync(x => x.doc_master_id == doc_master_id && x.user_id == user.user_key);
                        if (doc == null)
                        {
                            var new_view = new doc_views();
                            new_view.doc_master_id = doc_master_id;
                            new_view.user_id = user.user_key;
                            new_view.viewed_date = DateTime.Now;
                            new_view.is_app_viewed = false;
                            new_view.created_by = uid;
                            new_view.created_date = DateTime.Now;
                            new_view.created_ip = ip;
                            new_view.created_token_id = tid;
                            db.doc_views.Add(new_view);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = doc_master_id, contents }), domainurl + "DocMain/Update_ViewDoc", ip, tid, "Lỗi khi cập nhật trạng thái xem văn bản", 0, "DocMain");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = doc_master_id, contents }), domainurl + "DocMain/Update_ViewDoc", ip, tid, "Lỗi khi cập nhật trạng thái xem văn bản", 0, "DocMain");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }
        public string Update_detailDoc(MultipartFormDataStreamProvider provider, string created_by, string tid, string ip, string root, double count = 0)
        {
            string fddoc = "";
            using (DBEntities db = new DBEntities())
            {
                try
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == created_by);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }

                    fddoc = provider.FormData.GetValues("doc").SingleOrDefault();
                    doc_master vb = JsonConvert.DeserializeObject<doc_master>(fddoc);

                    bool is_new = true;

                    if (vb.doc_master_id == 0)
                    {
                        vb.doc_master_guid = helper.GenKey();
                        vb.created_date = DateTime.Now;
                        vb.sent_by = vb.created_by;
                        vb.handle_date = vb.created_date;
                        vb.first_doc_status_id = "sohoa";
                        vb.doc_status_id = "sohoa";

                        // Auto increase so vao so
                        if (vb.is_auto_num ?? false)
                        {
                            bool isNumeric = double.TryParse(vb.dispatch_book_code, out double n);
                            if (isNumeric)
                            {
                                n += count;
                                if (vb.doc_code == vb.dispatch_book_code) vb.doc_code = n.ToString();
                                vb.dispatch_book_code = n.ToString();
                                vb.dispatch_book_num = n;
                            }
                            else
                            {
                                var fdsokhtemp = provider.FormData.GetValues("auto_doc_code").SingleOrDefault();
                                List<DocCodeTemp> sokhtemp = JsonConvert.DeserializeObject<List<DocCodeTemp>>(fdsokhtemp);
                                if (sokhtemp != null && sokhtemp.Count() > 0)
                                {
                                    var final_sokh = "";
                                    double sovaosotemp = 0;
                                    foreach (var sokh in sokhtemp)
                                    {
                                        if (sokh.is_number)
                                        {
                                            bool ok = double.TryParse(sokh.value, out double output);
                                            if (ok)
                                            {
                                                sokh.value = (output + count).ToString();
                                                sovaosotemp = output + count;
                                            }
                                        }
                                        final_sokh += sokh.value + sokh.separator;
                                    }
                                    if (vb.doc_code == vb.dispatch_book_code) vb.doc_code = final_sokh;
                                    vb.dispatch_book_code = final_sokh;
                                    vb.dispatch_book_num = sovaosotemp;
                                }
                            }
                        }
                        else
                        {
                            bool isNumeric = double.TryParse(vb.dispatch_book_code, out double n);
                            if (isNumeric)
                            {
                                vb.dispatch_book_num = n;
                            }
                        }
                        // ---------------------------------
                        if (vb.doc_group_id != null)
                        {
                            var nvb = db.doc_ca_group_departments.FirstOrDefault(x => x.doc_group_id == vb.doc_group_id && x.department_id == vb.department_id);
                            if(nvb.current_num != null)
                            nvb.current_num = nvb.current_num + 1;
                        }
                        //----------------------------------
                        string department_id = provider.FormData.GetValues("department_id").SingleOrDefault();
                        if (department_id != null && department_id != "")
                        {
                            var qt = db.doc_workflows.FirstOrDefault(x => x.department_id == department_id && x.workflow_type == vb.nav_type && x.organization_id == vb.organization_id && x.status == true);
                            if (qt != null)
                            {
                                vb.workflow_id = qt.workflow_id;
                            }
                            else
                            {
                                var qtn = db.doc_workflows.FirstOrDefault(x => x.department_id == null && x.workflow_type == vb.nav_type && x.organization_id == vb.organization_id && x.status == true);
                                if (qtn != null)
                                {
                                    vb.workflow_id = qtn.workflow_id;
                                }
                            }
                        }
                        //----------Reservation number----------
                        string fd_is_reservation_number = provider.FormData.GetValues("is_reservation_number").SingleOrDefault();
                        if (fd_is_reservation_number != null && fd_is_reservation_number != "")
                        {
                            bool is_reservation_number = bool.Parse(fd_is_reservation_number);
                            if (is_reservation_number)
                            {
                                var reservation_number = db.doc_reservation_number.FirstOrDefault(x => x.organization_id == vb.organization_id && x.nav_type == vb.nav_type && x.reservation_code.Trim().ToLower() == vb.doc_code.Trim().ToLower() && !x.is_used);
                                if(reservation_number != null)
                                {
                                    reservation_number.is_used = true;
                                }
                            }
                        }
                        //--------------------------------
                        if (vb.is_auto_num ?? false)
                        {
                            var socongvan = db.doc_ca_dispatch_books.Find(vb.dispatch_book_id);
                            if (socongvan != null)
                            {
                                if (vb.dispatch_book_num == (socongvan.current_num + count + 1))
                                {
                                    socongvan.current_num += count + 1;
                                }
                                else if (vb.dispatch_book_num == (socongvan.current_num + 1))
                                {
                                    socongvan.current_num += 1;
                                }
                            }
                        }

                        string strPath = root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid;
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);

                        db.doc_master.Add(vb);

                        var new_view = new doc_views();
                        new_view.doc_master_id = vb.doc_master_id;
                        new_view.user_id = vb.created_by;
                        new_view.viewed_date = DateTime.Now;
                        new_view.is_app_viewed = false;
                        new_view.created_by = created_by;
                        new_view.created_date = DateTime.Now;
                        new_view.created_ip = ip;
                        new_view.created_token_id = tid;
                        db.doc_views.Add(new_view);
                    }
                    else
                    {
                        is_new = false;
                        bool isNumeric = double.TryParse(vb.dispatch_book_code, out double n);
                        if (isNumeric)
                        {
                            n += count;
                            vb.dispatch_book_code = n.ToString();
                            vb.dispatch_book_num = n;
                        }

                        var curvb = db.doc_master.AsNoTracking().FirstOrDefault(x => x.doc_master_id == vb.doc_master_id);
                        if (vb.doc_group_id != null)
                        {
                            if (curvb.doc_group_id != vb.doc_group_id)
                            {
                                var nvb = db.doc_ca_group_departments.FirstOrDefault(x => x.doc_group_id == vb.doc_group_id && x.department_id == vb.department_id);
                                if(nvb.current_num != null)
                                nvb.current_num = nvb.current_num + 1;
                            }
                        }
                        if (vb.dispatch_book_id != null)
                        {
                            if (curvb.dispatch_book_id != vb.dispatch_book_id)
                            {
                                var scv = db.doc_ca_dispatch_books.Find(vb.dispatch_book_id);
                                scv.current_num = scv.current_num + 1;
                            }
                        }

                        if ((provider.FormData.GetValues("docUploadOld") != null) && string.IsNullOrEmpty(vb.file_path))
                        {
                            string pathDel = provider.FormData.GetValues("docUploadOld").SingleOrDefault();
                            if (pathDel.Contains("/Portals/") && pathDel.Contains("/Doc/"))
                            {
                                bool existFiles = System.IO.File.Exists(root + pathDel);
                                if (existFiles)
                                    System.IO.File.Delete(root + pathDel);
                            }
                        };

                        db.Entry(vb).State = EntityState.Modified;
                    }

                    var old_relates = db.doc_related.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                    if (old_relates.Count() > 0) db.doc_related.RemoveRange(old_relates);
                    if (provider.FormData.GetValues("doc_relates") != null)
                    {
                        string doc_relates = provider.FormData.GetValues("doc_relates").SingleOrDefault();
                        List<doc_related> listRelateDoc = JsonConvert.DeserializeObject<List<doc_related>>(doc_relates);
                        if (listRelateDoc != null && listRelateDoc.Count > 0)
                        {
                            List<doc_related> listRelate = new List<doc_related>();
                            foreach (var item in listRelateDoc)
                            {
                                doc_related relate = new doc_related();
                                relate.doc_master_id = vb.doc_master_id;
                                relate.doc_related_id = item.doc_master_id;
                                relate.created_by = created_by;
                                relate.created_date = DateTime.Now;
                                relate.created_ip = ip;
                                relate.created_token_id = tid;
                                listRelate.Add(relate);
                            }
                            if (listRelate.Count > 0)
                            {
                                db.doc_related.AddRange(listRelate);
                            }
                        }
                    }

                    db.SaveChanges();

                    // File upload
                    // This illustrates how to get thefile names.
                    FileInfo fileInfo = null;
                    MultipartFileData ffileData = null;
                    string newFileName = "";

                    int numfileDoc = 1;
                    var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                    if (max_num.Count() > 0)
                    {
                        numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                    }
                    List<doc_files> listFileUp = new List<doc_files>();
                    List<string> listPathFileUp = new List<string>();
                    foreach (MultipartFileData fileData in provider.FileData)
                    {
                        numfileDoc++;
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
                            fileName = System.IO.Path.GetFileName(fileName);
                        }
                        newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                        fileInfo = new FileInfo(newFileName);
                        if (fileInfo.Exists)
                        {
                            fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                            // Convert to unsign
                            Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                            fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                        }
                        if (fileData.Headers.ContentDisposition.Name == "\"doc_file\"")
                        {
                            vb.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            vb.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            vb.file_size = new FileInfo(fileData.LocalFileName).Length;
                            vb.file_type = vb.file_name.Substring(vb.file_name.LastIndexOf(".") + 1).ToLower();
                        }
                        else
                        {
                            doc_files file_doc = new doc_files();
                            file_doc.doc_master_id = vb.doc_master_id;
                            file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                            file_doc.is_order = numfileDoc;
                            file_doc.is_drafted = false;
                            file_doc.doc_file_type = 0;
                            file_doc.message = "Thêm file văn bản " + vb.compendium;
                            file_doc.created_by = created_by;
                            file_doc.created_date = DateTime.Now;
                            file_doc.created_ip = ip;
                            file_doc.created_token_id = tid;
                            listFileUp.Add(file_doc);
                        }
                        ffileData = fileData;
                        //Add file
                        if (fileInfo != null)
                        {
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            File.Move(ffileData.LocalFileName, newFileName);
                            listPathFileUp.Add(ffileData.LocalFileName);
                        }
                    }
                    if (listFileUp.Count > 0)
                    {
                        db.doc_files.AddRange(listFileUp);
                    }

                    if (provider.FormData.GetValues("fileUploadOld") != null)
                    {
                        string doc_old_files = provider.FormData.GetValues("fileUploadOld").SingleOrDefault();
                        List<doc_files> listFileOld = JsonConvert.DeserializeObject<List<doc_files>>(doc_old_files);
                        List<string> pathFilesDel = new List<string>();
                        if (listFileOld != null && listFileOld.Count > 0)
                        {
                            List<doc_files> listFileDel = new List<doc_files>();
                            var listFileDelTemp = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                            var delItems = listFileDelTemp.Where(x => listFileOld.Count(y => y.file_id == x.file_id) > 0).ToList();
                            foreach (var item in delItems)
                            {
                                listFileDel.Add(item);
                                pathFilesDel.Add(item.file_path);
                            }
                            if (listFileDel.Count > 0)
                            {
                                db.doc_files.RemoveRange(listFileDel);
                            }
                        }
                        foreach (var pathDel in pathFilesDel)
                        {
                            if (pathDel.Contains("/Portals/") && pathDel.Contains("/Doc/"))
                            {
                                bool existFiles = System.IO.File.Exists(root + pathDel);
                                if (existFiles)
                                    System.IO.File.Delete(root + pathDel);
                            }
                        }
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
                    //---------------------
                    #region add doc_logs
                    if (helper.wlog)
                    {
                        doc_logs log = new doc_logs();
                        log.log_type = 0;
                        //log.message = JsonConvert.SerializeObject(new { data = law_main });
                        log.message = (is_new ? "Thêm mới" : "Cập nhật") + "văn bản: " + vb.compendium;
                        log.doc_name = vb.compendium;
                        log.doc_master_id = vb.doc_master_id;
                        log.organization_id = vb.organization_id;
                        log.log_type = is_new ? 0 : 1;
                        log.created_date = DateTime.Now;
                        log.created_by = created_by;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        log.is_view = false;
                        db.doc_logs.Add(log);
                        db.SaveChanges();
                    }
                    #endregion
                    return "OK";
                }
                catch (Exception e)
                {
                    if (e.InnerException.InnerException.Message.Contains("with unique index 'idx_dispatch_book_code_notnull"))
                    {
                        return "duplicatedocnum";
                    }
                    else if (e.InnerException.InnerException.Message.Contains("with unique index 'idx_doc_code_notnull"))
                    {
                        return "duplicatedoccode";
                    }
                    else
                    {
                        string contents = helper.ExceptionMessage(e);
                        return contents;
                    }
                }
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fddoc = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            int countNum = 0;
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fddoc = provider.FormData.GetValues("doc").SingleOrDefault();
                        doc_master vb = JsonConvert.DeserializeObject<doc_master>(fddoc);
                        int notunique_case = 0;
                        if (vb != null)
                        {
                            if (vb.dispatch_book_code != null)
                            {
                                string str = "";
                                do
                                {
                                    if (str.Contains("duplicatedocnum") && (vb.is_auto_num ?? false))
                                    {
                                        notunique_case = 1;
                                    }
                                    else if (str.Contains("duplicatedocnum") && (!vb.is_auto_num ?? true))
                                    {
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", notunique_case = "2" });
                                    }
                                    else if (str.Contains("duplicatedoccode"))
                                    {
                                        if (vb.doc_code == vb.dispatch_book_code && vb.nav_type != 1 && (vb.is_auto_num ?? false))
                                        {
                                            notunique_case = 1;
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", notunique_case = "3" });
                                        }
                                    }
                                    else if (str != "OK" && str != "")
                                    {
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = str });
                                    }
                                    str = Update_detailDoc(provider, uid, tid, ip, root, countNum++);
                                }
                                while (str.Contains("duplicatedocnum") || str != "OK");
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", notunique_case });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Add_Doc", ip, tid, "Lỗi khi cập nhật văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Add_Doc", ip, tid, "Lỗi khi cập nhật văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Doc([FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
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
                        var us = await db.sys_users.FindAsync(uid);
                        var das = await db.doc_master.Where(a => id.Contains(a.doc_master_id)).ToListAsync();
                        var folder_paths = new List<string>();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<doc_master> del = new List<doc_master>();
                            foreach (var da in das)
                            {
                                if (ad || da.created_by == us.user_key)
                                {
                                    if (!String.IsNullOrEmpty(da.file_path)) paths.Add(da.file_path);
                                    var organization_id_doc = da.organization_id != null ? da.organization_id.ToString() : "other";
                                    var listFileDoc = db.doc_files.Where(x => x.doc_master_id == da.doc_master_id).ToList();
                                    if (listFileDoc.Count > 0)
                                    {
                                        foreach (var item in listFileDoc)
                                        {
                                            if (item.file_path != null)
                                            {
                                                paths.Add(item.file_path);
                                            }
                                        }
                                    }
                                    del.Add(da);
                                    var path_folder = "/Portals/" + organization_id_doc + "/Doc/" + da.doc_master_guid;
                                    folder_paths.Add(path_folder);
                                }
                                #region add cms_logs
                                if (helper.wlog)
                                {
                                    doc_logs log = new doc_logs();
                                    //log.message = JsonConvert.SerializeObject(new { data = law_main });
                                    log.message = "Xoá văn bản: " + da.compendium;
                                    log.doc_name = da.compendium;
                                    log.doc_master_id = da.doc_master_id;
                                    log.organization_id = da.organization_id;
                                    log.log_type = 13;
                                    log.created_date = DateTime.Now;
                                    log.created_by = uid;
                                    log.created_token_id = tid;
                                    log.created_ip = ip;
                                    log.is_view = false;
                                    db.doc_logs.Add(log);
                                    db.SaveChanges();
                                }
                                #endregion
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.doc_master.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        foreach (string strPath in paths)
                        {
                            var pathDelFile = HttpContext.Current.Server.MapPath("~/" + strPath);
                            bool existFiles = System.IO.File.Exists(pathDelFile);
                            if (existFiles)
                                System.IO.File.Delete(pathDelFile);
                        }
                        foreach (string strPath in folder_paths)
                        {
                            var pathDelFolder = HttpContext.Current.Server.MapPath("~/" + strPath);
                            bool existFolder = Directory.Exists(pathDelFolder);
                            if (existFolder)
                                Directory.Delete(pathDelFolder, true);
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "DocMain/Delete_Doc", ip, tid, "Lỗi khi xoá văn bản", 0, "DocMain");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "DocMain/Delete_Doc", ip, tid, "Lỗi khi xoá văn bản", 0, "DocMain");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add_DocDuthao()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fddoc = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        fddoc = provider.FormData.GetValues("doc").SingleOrDefault();
                        doc_master vb = JsonConvert.DeserializeObject<doc_master>(fddoc);

                        bool is_new = true;

                        if (vb.doc_master_id == 0)
                        {
                            vb.doc_master_guid = helper.GenKey();
                            vb.created_date = DateTime.Now;
                            vb.doc_status_id = "duthao";
                            vb.first_doc_status_id = "duthao";
                            vb.sent_by = vb.created_by;
                            vb.handle_date = vb.created_date;

                            //----------------------------------
                            string department_id = provider.FormData.GetValues("department_id").SingleOrDefault();
                            if (department_id != null && department_id != "")
                            {
                                var qt = db.doc_workflows.FirstOrDefault(x => x.department_id == department_id && x.workflow_type == vb.nav_type && x.organization_id == vb.organization_id && x.status == true);
                                if (qt != null)
                                {
                                    vb.workflow_id = qt.workflow_id;
                                }
                                else
                                {
                                    var qtn = db.doc_workflows.FirstOrDefault(x => x.department_id == null && x.workflow_type == vb.nav_type && x.organization_id == vb.organization_id && x.status == true);
                                    if (qtn != null)
                                    {
                                        vb.workflow_id = qtn.workflow_id;
                                    }
                                }
                            }

                            string strPath = root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid;
                            bool exists = Directory.Exists(strPath);
                            if (!exists)
                                Directory.CreateDirectory(strPath);

                            db.doc_master.Add(vb);

                            var new_view = new doc_views();
                            new_view.doc_master_id = vb.doc_master_id;
                            new_view.user_id = vb.created_by;
                            new_view.viewed_date = DateTime.Now;
                            new_view.is_app_viewed = false;
                            new_view.created_by = uid;
                            new_view.created_date = DateTime.Now;
                            new_view.created_ip = ip;
                            new_view.created_token_id = tid;
                            db.doc_views.Add(new_view);
                        }
                        else
                        {
                            is_new = false;
                            if ((provider.FormData.GetValues("docUploadOld") != null) && string.IsNullOrEmpty(vb.file_path))
                            {
                                string pathDel = provider.FormData.GetValues("docUploadOld").SingleOrDefault();
                                if (pathDel.Contains("/Portals/") && pathDel.Contains("/Doc/"))
                                {
                                    bool existFiles = System.IO.File.Exists(root + pathDel);
                                    if (existFiles)
                                        System.IO.File.Delete(root + pathDel);
                                }
                            };
                            db.Entry(vb).State = EntityState.Modified;
                        }

                        var old_relates = db.doc_related.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                        if (old_relates.Count() > 0) db.doc_related.RemoveRange(old_relates);
                        if (provider.FormData.GetValues("doc_relates") != null)
                        {
                            string doc_relates = provider.FormData.GetValues("doc_relates").SingleOrDefault();
                            List<doc_related> listRelateDoc = JsonConvert.DeserializeObject<List<doc_related>>(doc_relates);
                            if (listRelateDoc != null && listRelateDoc.Count > 0)
                            {
                                List<doc_related> listRelate = new List<doc_related>();
                                foreach (var item in listRelateDoc)
                                {
                                    doc_related relate = new doc_related();
                                    relate.doc_master_id = vb.doc_master_id;
                                    relate.doc_related_id = item.doc_master_id;
                                    relate.created_by = uid;
                                    relate.created_date = DateTime.Now;
                                    relate.created_ip = ip;
                                    relate.created_token_id = tid;
                                    listRelate.Add(relate);
                                }
                                if (listRelate.Count > 0)
                                {
                                    db.doc_related.AddRange(listRelate);
                                }
                            }
                        }

                        // File upload
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        int numfileDoc = 1;
                        var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                        if (max_num.Count() > 0)
                        {
                            numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                        }
                        List<doc_files> listFileUp = new List<doc_files>();
                        List<string> listPathFileUp = new List<string>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            numfileDoc++;
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
                                fileName = System.IO.Path.GetFileName(fileName);
                            }
                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            }
                            if (fileData.Headers.ContentDisposition.Name == "\"doc_file\"")
                            {
                                vb.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                                vb.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                                vb.file_size = new FileInfo(fileData.LocalFileName).Length;
                                vb.file_type = vb.file_name.Substring(vb.file_name.LastIndexOf(".") + 1).ToLower();
                            }
                            else
                            {
                                doc_files file_doc = new doc_files();
                                file_doc.doc_master_id = vb.doc_master_id;
                                file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                                file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                                //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                                file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                                file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                                file_doc.is_order = numfileDoc;
                                file_doc.is_drafted = false;
                                file_doc.doc_file_type = 0;
                                file_doc.message = "Thêm file văn bản " + vb.compendium;
                                file_doc.created_by = uid;
                                file_doc.created_date = DateTime.Now;
                                file_doc.created_ip = ip;
                                file_doc.created_token_id = tid;
                                listFileUp.Add(file_doc);
                            }
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }
                        if (listFileUp.Count > 0)
                        {
                            db.doc_files.AddRange(listFileUp);
                        }

                        if (provider.FormData.GetValues("fileUploadOld") != null)
                        {
                            string doc_old_files = provider.FormData.GetValues("fileUploadOld").SingleOrDefault();
                            List<doc_files> listFileOld = JsonConvert.DeserializeObject<List<doc_files>>(doc_old_files);
                            List<string> pathFilesDel = new List<string>();
                            if (listFileOld != null && listFileOld.Count > 0)
                            {
                                List<doc_files> listFileDel = new List<doc_files>();
                                var listFileDelTemp = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                                var delItems = listFileDelTemp.Where(x => listFileOld.Count(y => y.file_id == x.file_id) > 0).ToList();
                                foreach (var item in delItems)
                                {
                                    listFileDel.Add(item);
                                    pathFilesDel.Add(item.file_path);
                                }
                                if (listFileDel.Count > 0)
                                {
                                    db.doc_files.RemoveRange(listFileDel);
                                }
                            }
                            foreach (var pathDel in pathFilesDel)
                            {
                                if (pathDel.Contains("/Portals/") && pathDel.Contains("/Doc/"))
                                {
                                    bool existFiles = System.IO.File.Exists(root + pathDel);
                                    if (existFiles)
                                        System.IO.File.Delete(root + pathDel);
                                }
                            }
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
                        //---------------------
                        #region add doc_logs
                        if (helper.wlog)
                        {
                            doc_logs log = new doc_logs();
                            log.log_type = 0;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = (is_new ? "Thêm mới" : "Cập nhật") + "văn bản dự thảo: " + vb.compendium;
                            log.doc_name = vb.compendium;
                            log.doc_master_id = vb.doc_master_id;
                            log.organization_id = vb.organization_id;
                            log.log_type = is_new ? 0 : 1;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.doc_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Add_DocDuthao", ip, tid, "Lỗi khi cập nhật văn bản dự thảo", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Add_DocDuthao", ip, tid, "Lỗi khi cập nhật văn bản dự thảo", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Publish_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string publish_item = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        DateTime send_date_now = DateTime.Now;

                        publish_item = provider.FormData.GetValues("publish_item").SingleOrDefault();
                        doc_follows model = JsonConvert.DeserializeObject<doc_follows>(publish_item);

                        string orgs = provider.FormData.GetValues("orgs").SingleOrDefault();
                        List<int> org_ids = JsonConvert.DeserializeObject<List<int>>(orgs);

                        if (org_ids.Count() > 0)
                        {
                            foreach (var org_id in org_ids)
                            {
                                var new_fl = new doc_follows();
                                new_fl.follow_id = helper.GenKey();
                                new_fl.organization_id = model.organization_id;
                                new_fl.doc_master_id = model.doc_master_id;
                                new_fl.send_by = user_now.user_key;
                                new_fl.send_by_name = user_now.full_name;
                                new_fl.send_date = send_date_now;
                                new_fl.receive_by = org_id;

                                var org = db.sys_organization.Find(org_id);
                                new_fl.receive_by_name = org.organization_name;
                                new_fl.receive_type = 2;
                                new_fl.follow_parent_id = model.follow_id;
                                new_fl.level = 1;

                                if (new_fl.follow_parent_id != null)
                                {
                                    var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                    if (par != null)
                                    {
                                        new_fl.level = par.level + 1;
                                    }
                                    new_fl.parent_doc_status_id = par.doc_status_id;
                                }
                                new_fl.doc_status_id = "phanphat";
                                new_fl.message = model.message;
                                new_fl.created_by = uid;
                                new_fl.created_date = DateTime.Now;
                                new_fl.created_ip = ip;
                                new_fl.created_token_id = tid;
                                db.doc_follows.Add(new_fl);
                            }
                        }

                        //---------------------------------------

                        string groups = provider.FormData.GetValues("groups").SingleOrDefault();
                        List<int> group_ids = JsonConvert.DeserializeObject<List<int>>(groups);

                        if (group_ids.Count() > 0)
                        {
                            foreach (var group_id in group_ids)
                            {
                                var new_fl = new doc_follows();
                                new_fl.follow_id = helper.GenKey();
                                new_fl.organization_id = model.organization_id;
                                new_fl.doc_master_id = model.doc_master_id;
                                new_fl.send_by = user_now.user_key;
                                new_fl.send_by_name = user_now.full_name;
                                new_fl.send_date = send_date_now;
                                new_fl.receive_by = group_id;

                                var gr = db.doc_ca_role_groups.Find(group_id);
                                new_fl.receive_by_name = gr.role_group_name;
                                new_fl.receive_type = 1;
                                new_fl.follow_parent_id = model.follow_id;
                                new_fl.level = 1;

                                if (new_fl.follow_parent_id != null)
                                {
                                    var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                    if (par != null)
                                    {
                                        new_fl.level = par.level + 1;
                                    }
                                    new_fl.parent_doc_status_id = par.doc_status_id;
                                }
                                new_fl.doc_status_id = "phanphat";
                                new_fl.message = model.message;
                                new_fl.created_by = uid;
                                new_fl.created_date = DateTime.Now;
                                new_fl.created_ip = ip;
                                new_fl.created_token_id = tid;
                                db.doc_follows.Add(new_fl);
                            }
                        }

                        var gr_us_ids = new List<string>();
                        foreach (var gr_id in group_ids)
                        {
                            var gr_us = db.doc_ca_role_group_users.Where(x => x.role_group_id == gr_id).ToList();
                            if (gr_us.Count() > 0)
                            {
                                foreach (var us in gr_us)
                                {
                                    if (!gr_us_ids.Contains(us.user_id))
                                    {
                                        gr_us_ids.Add(us.user_id);
                                    }
                                }
                            }
                        }

                        //---------------------------------------

                        string fd_departments = provider.FormData.GetValues("departments").SingleOrDefault();
                        List<int> departments_ids = JsonConvert.DeserializeObject<List<int>>(fd_departments);

                        List<string> department_user_ids = new List<string>();

                        if (departments_ids.Count() > 0)
                        {
                            foreach (var departments_id in departments_ids)
                            {
                                var us_department = db.doc_ca_role_group_department.FirstOrDefault(x => (x.department_id ?? 0) == departments_id && x.role_group_id == null);
                                if (us_department != null)
                                {
                                    var us = db.sys_users.Find(us_department.user_id);
                                    var department = db.sys_organization.Find(departments_id);

                                    department_user_ids.Add(us_department.user_id);

                                    var new_fl = new doc_follows();
                                    new_fl.follow_id = helper.GenKey();
                                    new_fl.organization_id = model.organization_id;
                                    new_fl.doc_master_id = model.doc_master_id;
                                    new_fl.deadline_date = model.deadline_date;
                                    new_fl.send_by = user_now.user_key;
                                    new_fl.send_by_name = user_now.full_name;
                                    new_fl.send_date = send_date_now;
                                    new_fl.receive_by = departments_id;
                                    new_fl.receive_last_group_user = us.user_key;

                                    new_fl.receive_by_name = department?.organization_name;
                                    new_fl.receive_type = 3;
                                    new_fl.follow_parent_id = model.follow_id;
                                    new_fl.level = 1;

                                    if (new_fl.follow_parent_id != null)
                                    {
                                        var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                        if (par != null)
                                        {
                                            new_fl.level = par.level + 1;
                                        }
                                        new_fl.parent_doc_status_id = par.doc_status_id;
                                    }
                                    new_fl.doc_status_id = "phanphat";
                                    new_fl.message = model.message;
                                    new_fl.created_by = uid;
                                    new_fl.created_date = DateTime.Now;
                                    new_fl.created_ip = ip;
                                    new_fl.created_token_id = tid;
                                    db.doc_follows.Add(new_fl);
                                }
                            }
                        }
                        //---------------------------------------

                        string users = provider.FormData.GetValues("users").SingleOrDefault();
                        List<string> user_ids = JsonConvert.DeserializeObject<List<string>>(users);

                        if (user_ids.Count() > 0)
                        {
                            var uss = db.sys_users.Where(x => (org_ids.Contains(x.organization_id ?? 0) || gr_us_ids.Contains(x.user_id) || department_user_ids.Contains(x.user_id)) && x.status == 1).Select(a =>
                              a.user_id);
                            foreach (var u in uss)
                            {
                                if (user_ids != null && user_ids.Count > 0)
                                {
                                    if (user_ids.Contains(u))
                                    {
                                        int idx = user_ids.IndexOf(u);
                                        user_ids.RemoveAt(idx);
                                    }
                                }
                            }
                            foreach (var user_id in user_ids)
                            {
                                var us = db.sys_users.Find(user_id);

                                var new_fl = new doc_follows();
                                new_fl.follow_id = helper.GenKey();
                                new_fl.organization_id = model.organization_id;
                                new_fl.doc_master_id = model.doc_master_id;
                                new_fl.send_by = user_now.user_key;
                                new_fl.send_by_name = user_now.full_name;
                                new_fl.send_date = send_date_now;
                                new_fl.receive_by = us.user_key;

                                new_fl.receive_by_name = us.full_name;
                                new_fl.receive_type = 0;
                                new_fl.follow_parent_id = model.follow_id;
                                new_fl.level = 1;

                                if (new_fl.follow_parent_id != null)
                                {
                                    var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                    if (par != null)
                                    {
                                        new_fl.level = par.level + 1;
                                    }
                                    new_fl.parent_doc_status_id = par.doc_status_id;
                                }
                                new_fl.doc_status_id = "phanphat";
                                new_fl.message = model.message;
                                new_fl.created_by = uid;
                                new_fl.created_date = DateTime.Now;
                                new_fl.created_ip = ip;
                                new_fl.created_token_id = tid;
                                db.doc_follows.Add(new_fl);
                            }
                        }


                        //-----------Master----------
                        var vb = db.doc_master.Find(model.doc_master_id);
                        if (vb != null)
                        {
                            vb.sent_by = user_now.user_key;
                            vb.follow_id = model.follow_id;
                            vb.handle_date = DateTime.Now;
                            vb.doc_status_id = "phanphat";
                            vb.message = model.message;
                        }

                        //---------------------
                        #region add doc_logs
                        if (helper.wlog)
                        {
                            doc_logs log = new doc_logs();
                            log.log_type = 5;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = "Phân phát văn bản: " + vb.compendium;
                            log.doc_name = vb.compendium;
                            log.doc_master_id = vb.doc_master_id;
                            log.organization_id = user_now.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.doc_logs.Add(log);
                        }
                        #endregion

                        //-----------------------
                        var ns_sh = (from nss in db.sys_users
                                     where (org_ids.FirstOrDefault(x => x == nss.organization_id) != 0 || user_ids.Contains(nss.user_id) || gr_us_ids.Contains(nss.user_id) || department_user_ids.Contains(nss.user_id)) && nss.status == 1
                                     select new { nss.user_id, nss.full_name, nss.avatar }).ToList();

                        #region export xml message
                        if (provider.FormData.GetValues("is_exported_xml") != null)
                        {
                            string fd_is_exported_xml = provider.FormData.GetValues("is_exported_xml").SingleOrDefault();
                            bool is_exported_xml = bool.Parse(fd_is_exported_xml);
                            if (is_exported_xml)
                            {
                                var content_xml = new doc_follows();
                                content_xml.doc_master_id = vb.doc_master_id;
                                content_xml.send_by_name = user_now.full_name;
                                content_xml.receive_by_name = "";
                                foreach (var ns in ns_sh)
                                {
                                    if (content_xml.receive_by_name != "") content_xml.receive_by_name += ", ";
                                    content_xml.receive_by_name += ns.full_name;
                                }
                                content_xml.message = model.message;
                                content_xml.send_date = send_date_now;
                                var path_save = helper.path_xml + "/Doc";
                                if (!Directory.Exists(path_save))
                                    Directory.CreateDirectory(path_save);
                                string res = ExportMessage_XML(content_xml, path_save);
                                if (res != "OK")
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = res });
                                }
                            }
                        }
                        #endregion

                        #region send socket
                        string res_socket = SendSocketMultiple(ns_sh.Select(x => x.user_id), "Văn bản", "Phân phát: " + vb.compendium);
                        #endregion

                        #region sendhub

                        foreach (var ns in ns_sh)
                        {
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.user_send = uid;
                            sh.module_key = const_module_key;
                            sh.receiver = ns.user_id;
                            sh.icon = ns.avatar;
                            sh.title = "Văn bản";
                            sh.contents = "Phân phát: " + vb.compendium;
                            sh.type = 3;
                            sh.is_type = 0;
                            sh.date_send = DateTime.Now;
                            sh.id_key = model.doc_master_id.ToString();
                            sh.group_id = model.follow_id;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            db.sys_sendhub.Add(sh);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Publish_Doc", ip, tid, "Lỗi khi phân phát văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Publish_Doc", ip, tid, "Lỗi khi phân phát văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> FollowPerson_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string followperson_item = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }

                    DateTime send_date_now = DateTime.Now;

                    followperson_item = provider.FormData.GetValues("followperson_item").SingleOrDefault();
                    doc_follows model = JsonConvert.DeserializeObject<doc_follows>(followperson_item);

                    string hosts = provider.FormData.GetValues("hosts").SingleOrDefault();
                    List<string> hosts_ids = JsonConvert.DeserializeObject<List<string>>(hosts);

                    if (hosts_ids.Count() > 0)
                    {
                        foreach (var hosts_id in hosts_ids)
                        {
                            var us = db.sys_users.Find(hosts_id);

                            var new_fl = new doc_follows();
                            new_fl.follow_id = helper.GenKey();
                            new_fl.organization_id = model.organization_id;
                            new_fl.doc_master_id = model.doc_master_id;
                            new_fl.deadline_date = model.deadline_date;
                            new_fl.send_by = user_now.user_key;
                            new_fl.send_by_name = user_now.full_name;
                            new_fl.send_date = send_date_now;
                            new_fl.receive_by = us.user_key;

                            new_fl.receive_by_name = us.full_name;
                            new_fl.receive_type = 0;
                            new_fl.follow_parent_id = model.follow_id;
                            new_fl.level = 1;

                            if (new_fl.follow_parent_id != null)
                            {
                                var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                if (par != null)
                                {
                                    new_fl.level = par.level + 1;
                                }
                                new_fl.parent_doc_status_id = par.doc_status_id;
                            }
                            new_fl.doc_status_id = "xulychinh";
                            new_fl.message = model.message;
                            new_fl.created_by = uid;
                            new_fl.created_date = DateTime.Now;
                            new_fl.created_ip = ip;
                            new_fl.created_token_id = tid;
                            db.doc_follows.Add(new_fl);
                        }
                    }

                    //---------------------------------------

                    string trackers = provider.FormData.GetValues("trackers").SingleOrDefault();
                    List<string> trackers_ids = JsonConvert.DeserializeObject<List<string>>(trackers);

                    if (trackers_ids.Count() > 0)
                    {
                        foreach (var trackers_id in trackers_ids)
                        {
                            var us = db.sys_users.Find(trackers_id);

                            var new_fl = new doc_follows();
                            new_fl.follow_id = helper.GenKey();
                            new_fl.organization_id = model.organization_id;
                            new_fl.doc_master_id = model.doc_master_id;
                            new_fl.send_by = user_now.user_key;
                            new_fl.send_by_name = user_now.full_name;
                            new_fl.send_date = send_date_now;
                            new_fl.receive_by = us.user_key;

                            new_fl.receive_by_name = us.full_name;
                            new_fl.receive_type = 0;
                            new_fl.follow_parent_id = model.follow_id;
                            new_fl.level = 1;

                            if (new_fl.follow_parent_id != null)
                            {
                                var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                if (par != null)
                                {
                                    new_fl.level = par.level + 1;
                                }
                                new_fl.parent_doc_status_id = par.doc_status_id;
                            }
                            new_fl.doc_status_id = "phoihop";
                            new_fl.message = model.message;
                            new_fl.created_by = uid;
                            new_fl.created_date = DateTime.Now;
                            new_fl.created_ip = ip;
                            new_fl.created_token_id = tid;
                            db.doc_follows.Add(new_fl);
                        }
                    }

                    //---------------------------------------

                    string track_groups = provider.FormData.GetValues("track_groups").SingleOrDefault();
                    List<int> track_groups_ids = JsonConvert.DeserializeObject<List<int>>(track_groups);

                    if (track_groups_ids.Count() > 0)
                    {
                        foreach (var group_id in track_groups_ids)
                        {
                            var new_fl = new doc_follows();
                            new_fl.follow_id = helper.GenKey();
                            new_fl.organization_id = model.organization_id;
                            new_fl.doc_master_id = model.doc_master_id;
                            new_fl.send_by = user_now.user_key;
                            new_fl.send_by_name = user_now.full_name;
                            new_fl.send_date = send_date_now;
                            new_fl.receive_by = group_id;

                            var gr = db.doc_ca_role_groups.Find(group_id);
                            new_fl.receive_by_name = gr.role_group_name;
                            new_fl.receive_type = 1;
                            new_fl.follow_parent_id = model.follow_id;
                            new_fl.level = 1;

                            if (new_fl.follow_parent_id != null)
                            {
                                var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                if (par != null)
                                {
                                    new_fl.level = par.level + 1;
                                }
                                new_fl.parent_doc_status_id = par.doc_status_id;
                            }
                            new_fl.doc_status_id = "phoihop";
                            new_fl.message = model.message;
                            new_fl.created_by = uid;
                            new_fl.created_date = DateTime.Now;
                            new_fl.created_ip = ip;
                            new_fl.created_token_id = tid;
                            db.doc_follows.Add(new_fl);
                        }
                    }

                    var track_gr_us_ids = new List<string>();
                    foreach (var gr_id in track_groups_ids)
                    {
                        var gr_us = db.doc_ca_role_group_users.Where(x => x.role_group_id == gr_id).ToList();
                        if (gr_us.Count() > 0)
                        {
                            foreach (var us in gr_us)
                            {
                                if (!track_gr_us_ids.Contains(us.user_id))
                                {
                                    track_gr_us_ids.Add(us.user_id);
                                }
                            }
                        }
                    }
                    //---------------------------------------

                    string seetoknow_users = provider.FormData.GetValues("seetoknow_users").SingleOrDefault();
                    List<string> seetoknow_users_ids = JsonConvert.DeserializeObject<List<string>>(seetoknow_users);

                    if (seetoknow_users_ids.Count() > 0)
                    {
                        foreach (var seetoknow_users_id in seetoknow_users_ids)
                        {
                            var us = db.sys_users.Find(seetoknow_users_id);

                            var new_fl = new doc_follows();
                            new_fl.follow_id = helper.GenKey();
                            new_fl.organization_id = model.organization_id;
                            new_fl.doc_master_id = model.doc_master_id;
                            new_fl.send_by = user_now.user_key;
                            new_fl.send_by_name = user_now.full_name;
                            new_fl.send_date = send_date_now;
                            new_fl.receive_by = us.user_key;

                            new_fl.receive_by_name = us.full_name;
                            new_fl.receive_type = 0;
                            new_fl.follow_parent_id = model.follow_id;
                            new_fl.level = 1;

                            if (new_fl.follow_parent_id != null)
                            {
                                var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                if (par != null)
                                {
                                    new_fl.level = par.level + 1;
                                }
                                new_fl.parent_doc_status_id = par.doc_status_id;
                            }
                            new_fl.doc_status_id = "xemdebiet";
                            new_fl.message = model.message;
                            new_fl.created_by = uid;
                            new_fl.created_date = DateTime.Now;
                            new_fl.created_ip = ip;
                            new_fl.created_token_id = tid;
                            db.doc_follows.Add(new_fl);
                        }
                    }

                    //---------------------------------------

                    string seetoknow_groups = provider.FormData.GetValues("seetoknow_groups").SingleOrDefault();
                    List<int> seetoknow_groups_ids = JsonConvert.DeserializeObject<List<int>>(seetoknow_groups);

                    if (seetoknow_groups_ids.Count() > 0)
                    {
                        foreach (var group_id in seetoknow_groups_ids)
                        {
                            var new_fl = new doc_follows();
                            new_fl.follow_id = helper.GenKey();
                            new_fl.organization_id = model.organization_id;
                            new_fl.doc_master_id = model.doc_master_id;
                            new_fl.send_by = user_now.user_key;
                            new_fl.send_by_name = user_now.full_name;
                            new_fl.send_date = send_date_now;
                            new_fl.receive_by = group_id;

                            var gr = db.doc_ca_role_groups.Find(group_id);
                            new_fl.receive_by_name = gr.role_group_name;
                            new_fl.receive_type = 1;
                            new_fl.follow_parent_id = model.follow_id;
                            new_fl.level = 1;

                            if (new_fl.follow_parent_id != null)
                            {
                                var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                if (par != null)
                                {
                                    new_fl.level = par.level + 1;
                                }
                                new_fl.parent_doc_status_id = par.doc_status_id;
                            }
                            new_fl.doc_status_id = "xemdebiet";
                            new_fl.message = model.message;
                            new_fl.created_by = uid;
                            new_fl.created_date = DateTime.Now;
                            new_fl.created_ip = ip;
                            new_fl.created_token_id = tid;
                            db.doc_follows.Add(new_fl);
                        }
                    }

                    var seetoknow_gr_us_ids = new List<string>();
                    foreach (var gr_id in seetoknow_groups_ids)
                    {
                        var gr_us = db.doc_ca_role_group_users.Where(x => x.role_group_id == gr_id).ToList();
                        if (gr_us.Count() > 0)
                        {
                            foreach (var us in gr_us)
                            {
                                if (!seetoknow_gr_us_ids.Contains(us.user_id))
                                {
                                    seetoknow_gr_us_ids.Add(us.user_id);
                                }
                            }
                        }
                    }

                    //---------------------------------------

                    string fd_track_departments = provider.FormData.GetValues("track_departments").SingleOrDefault();
                    List<int> track_departments_ids = JsonConvert.DeserializeObject<List<int>>(fd_track_departments);
                    List<string> department_user_ids = new List<string>();

                    if (track_departments_ids.Count() > 0)
                    {
                        foreach (var track_departments_id in track_departments_ids)
                        {
                            var us_department = db.doc_ca_role_group_department.FirstOrDefault(x => (x.department_id ?? 0) == track_departments_id && x.role_group_id == null);
                            if (us_department != null)
                            {
                                var us = db.sys_users.Find(us_department.user_id);
                                var department = db.sys_organization.Find(track_departments_id);

                                department_user_ids.Add(us_department.user_id);

                                var new_fl = new doc_follows();
                                new_fl.follow_id = helper.GenKey();
                                new_fl.organization_id = model.organization_id;
                                new_fl.doc_master_id = model.doc_master_id;
                                new_fl.deadline_date = model.deadline_date;
                                new_fl.send_by = user_now.user_key;
                                new_fl.send_by_name = user_now.full_name;
                                new_fl.send_date = send_date_now;
                                new_fl.receive_by = track_departments_id;
                                new_fl.receive_last_group_user = us.user_key;

                                new_fl.receive_by_name = department?.organization_name;
                                new_fl.receive_type = 3;
                                new_fl.follow_parent_id = model.follow_id;
                                new_fl.level = 1;

                                if (new_fl.follow_parent_id != null)
                                {
                                    var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                    if (par != null)
                                    {
                                        new_fl.level = par.level + 1;
                                    }
                                    new_fl.parent_doc_status_id = par.doc_status_id;
                                }
                                new_fl.doc_status_id = "phoihop";
                                new_fl.message = model.message;
                                new_fl.created_by = uid;
                                new_fl.created_date = DateTime.Now;
                                new_fl.created_ip = ip;
                                new_fl.created_token_id = tid;
                                db.doc_follows.Add(new_fl);
                            }
                        }
                    }

                    //---------------------------------------

                    //-----------Master----------
                    var vb = db.doc_master.Find(model.doc_master_id);
                    if (vb != null)
                    {
                        vb.sent_by = user_now.user_key;
                        vb.follow_id = model.follow_id;
                        vb.handle_date = DateTime.Now;
                        vb.doc_status_id = "xulychinh";
                        vb.message = model.message;
                    }

                    //----------------------
                    #region File upload
                    FileInfo fileInfo = null;
                    MultipartFileData ffileData = null;
                    string newFileName = "";

                    int numfileDoc = 1;
                    var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                    if (max_num.Count() > 0)
                    {
                        numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                    }
                    List<doc_files> listFileUp = new List<doc_files>();
                    List<string> listPathFileUp = new List<string>();
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
                            fileName = System.IO.Path.GetFileName(fileName);
                        }
                        newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                        fileInfo = new FileInfo(newFileName);
                        if (fileInfo.Exists)
                        {
                            fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                            // Convert to unsign
                            Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                            fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                        }
                        // defined file doc
                        doc_files file_doc = new doc_files();
                        file_doc.doc_master_id = vb.doc_master_id;
                        file_doc.follow_id = model.follow_id;
                        file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                        file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                        //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                        file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                        file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                        file_doc.is_order = numfileDoc;
                        file_doc.is_drafted = false;
                        file_doc.doc_file_type = 1;
                        file_doc.message = "Thêm file chuyển xử lý cá nhân văn bản " + vb.compendium;
                        file_doc.created_by = uid;
                        file_doc.created_date = DateTime.Now;
                        file_doc.created_ip = ip;
                        file_doc.created_token_id = tid;
                        listFileUp.Add(file_doc);
                        ffileData = fileData;
                        //Add file
                        if (fileInfo != null)
                        {
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            File.Move(ffileData.LocalFileName, newFileName);
                            listPathFileUp.Add(ffileData.LocalFileName);
                        }
                        numfileDoc++;
                    }
                    if (listFileUp.Count > 0)
                    {
                        db.doc_files.AddRange(listFileUp);
                    }

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
                    #endregion

                    //---------------------
                    #region add doc_logs
                    if (helper.wlog)
                    {
                        doc_logs log = new doc_logs();
                        log.log_type = 2;
                        //log.message = JsonConvert.SerializeObject(new { data = law_main });
                        log.message = "Chuyển xử lý cá nhân văn bản: " + vb.compendium;
                        log.doc_name = vb.compendium;
                        log.doc_master_id = vb.doc_master_id;
                        log.organization_id = user_now.organization_id;
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        log.is_view = false;
                        db.doc_logs.Add(log);
                    }
                    #endregion

                    //-----------------------
                    var ns_sh = (from nss in db.sys_users
                                 where (hosts_ids.Contains(nss.user_id) || department_user_ids.Contains(nss.user_id) || trackers_ids.Contains(nss.user_id) || seetoknow_users_ids.Contains(nss.user_id) || track_gr_us_ids.Contains(nss.user_id) || seetoknow_gr_us_ids.Contains(nss.user_id)) && nss.status == 1
                                 select new { nss.user_id, nss.full_name, nss.avatar }).ToList();

                    #region export xml message
                    if (provider.FormData.GetValues("is_exported_xml") != null)
                    {
                        string fd_is_exported_xml = provider.FormData.GetValues("is_exported_xml").SingleOrDefault();
                        bool is_exported_xml = bool.Parse(fd_is_exported_xml);
                        if (is_exported_xml)
                        {
                            var content_xml = new doc_follows();
                            content_xml.doc_master_id = vb.doc_master_id;
                            content_xml.send_by_name = user_now.full_name;
                            content_xml.receive_by_name = "";
                            foreach (var ns in ns_sh)
                            {
                                if (content_xml.receive_by_name != "") content_xml.receive_by_name += ", ";
                                content_xml.receive_by_name += ns.full_name;
                            }
                            content_xml.message = model.message;
                            content_xml.send_date = send_date_now;
                            var path_save = helper.path_xml + "/Doc";
                            if (!Directory.Exists(path_save))
                                Directory.CreateDirectory(path_save);
                            string res = ExportMessage_XML(content_xml, path_save);
                            if (res != "OK")
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = res });
                            }
                        }
                    }
                    #endregion

                    #region send socket
                    var lst_socket = new Dictionary<string, string>();
                    #endregion

                    #region sendhub

                    foreach (var ns in ns_sh)
                    {
                            string name_task = "";
                            if (hosts_ids.Contains(ns.user_id)) name_task = "Chuyển xử lý: ";
                            else if (department_user_ids.Contains(ns.user_id) || trackers_ids.Contains(ns.user_id) || track_gr_us_ids.Contains(ns.user_id)) name_task = "Phối hợp: ";
                            else if (seetoknow_users_ids.Contains(ns.user_id) || seetoknow_gr_us_ids.Contains(ns.user_id)) name_task = "Xem để biết: ";

                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.user_send = uid;
                            sh.module_key = const_module_key;
                            sh.receiver = ns.user_id;
                            sh.icon = ns.avatar;
                            sh.title = "Văn bản";
                            sh.contents = name_task + vb.compendium;
                            lst_socket.Add(ns.user_id, sh.contents);
                            sh.type = 3;
                            sh.is_type = 0;
                            sh.date_send = DateTime.Now;
                            sh.id_key = model.doc_master_id.ToString();
                            sh.group_id = model.follow_id;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            db.sys_sendhub.Add(sh);
                        }

                        string res_socket = SendSocketSingle(lst_socket, "Văn bản");

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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/FollowPerson_Doc", ip, tid, "Lỗi khi chuyển xử lý cá nhân văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/FollowPerson_Doc", ip, tid, "Lỗi khi chuyển xử lý cá nhân văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> FollowGroup_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string followgroup_item = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        DateTime send_date_now = DateTime.Now;

                        followgroup_item = provider.FormData.GetValues("followgroup_item").SingleOrDefault();
                        var user_sendhub = new List<string>();
                        doc_follows model = JsonConvert.DeserializeObject<doc_follows>(followgroup_item);

                        var role_gr = db.doc_ca_role_groups.Find(model.receive_by);

                        var new_fl = new doc_follows();
                        new_fl.follow_id = helper.GenKey();
                        new_fl.organization_id = model.organization_id;
                        new_fl.doc_master_id = model.doc_master_id;
                        new_fl.deadline_date = model.deadline_date;
                        new_fl.send_by = user_now.user_key;
                        new_fl.send_by_name = user_now.full_name;
                        new_fl.send_date = send_date_now;
                        new_fl.receive_by = model.receive_by;

                        if ((role_gr.type_approval ?? 0) == 1)
                        {
                            if (new_fl.follow_parent_id != null)
                            {
                                var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                if (par != null)
                                {
                                    if (par.receive_by == new_fl.receive_by && par.receive_type == new_fl.receive_type)
                                    {
                                        if (par.receive_last_group_user != null)
                                        {
                                            var us_id = db.sys_users.FirstOrDefault(x => x.user_key == par.receive_last_group_user);
                                            var last_gr_u = db.doc_ca_role_group_users.FirstOrDefault(x => x.role_group_id == par.receive_by && x.user_id == us_id.user_id);
                                            if (last_gr_u != null)
                                            {
                                                var next_gr_u = db.doc_ca_role_group_users.Where(x => x.role_group_id == new_fl.receive_by && x.is_order > last_gr_u.is_order).OrderBy(x => x.is_order).FirstOrDefault();
                                                if (next_gr_u != null)
                                                {
                                                    new_fl.receive_last_group_user = db.sys_users.Find(next_gr_u.user_id)?.user_key;
                                                    user_sendhub.Add(next_gr_u.user_id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var all_gr_us = db.doc_ca_role_group_users.Where(x => x.role_group_id == new_fl.receive_by);
                            if (all_gr_us.Count() > 0)
                            {
                                user_sendhub.AddRange(all_gr_us.Select(x => x.user_id));
                            }
                        }

                        if (role_gr.is_bydepartment ?? false)
                        {
                            var user_approval_department = db.doc_ca_role_group_department.FirstOrDefault(x => x.role_group_id == role_gr.role_group_id && x.department_id == user_now.department_id);
                            if (user_approval_department == null)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Chưa cấu hình người duyệt nhóm phòng ban!" });
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(user_approval_department.user_id))
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Chưa cấu hình người duyệt nhóm phòng ban!" });
                                }
                                new_fl.receive_last_group_user = db.sys_users.Find(user_approval_department.user_id)?.user_key;
                                user_sendhub.Add(user_approval_department.user_id);
                            }
                        }

                        new_fl.receive_by_name = role_gr.role_group_name;
                        new_fl.receive_type = 1;
                        new_fl.follow_parent_id = model.follow_id;
                        new_fl.level = 1;

                        if (new_fl.follow_parent_id != null)
                        {
                            var par = db.doc_follows.Find(new_fl.follow_parent_id);
                            if (par != null)
                            {
                                new_fl.level = par.level + 1;
                            }
                            new_fl.parent_doc_status_id = par.doc_status_id;
                        }
                        new_fl.doc_status_id = "xulychinh";
                        new_fl.message = model.message;
                        new_fl.created_by = uid;
                        new_fl.created_date = DateTime.Now;
                        new_fl.created_ip = ip;
                        new_fl.created_token_id = tid;
                        db.doc_follows.Add(new_fl);

                        //---------------------------------------

                        string trackers = provider.FormData.GetValues("trackers").SingleOrDefault();
                        List<string> trackers_ids = JsonConvert.DeserializeObject<List<string>>(trackers);

                        if (trackers_ids.Count() > 0)
                        {
                            foreach (var trackers_id in trackers_ids)
                            {
                                var us = db.sys_users.Find(trackers_id);

                                var new_tr = new doc_follows();
                                new_tr.follow_id = helper.GenKey();
                                new_tr.organization_id = model.organization_id;
                                new_tr.doc_master_id = model.doc_master_id;
                                new_tr.send_by = user_now.user_key;
                                new_tr.send_by_name = user_now.full_name;
                                new_tr.send_date = send_date_now;
                                new_tr.receive_by = us.user_key;

                                new_tr.receive_by_name = us.full_name;
                                new_tr.receive_type = 0;
                                new_tr.follow_parent_id = model.follow_id;
                                new_tr.level = 1;

                                if (new_tr.follow_parent_id != null)
                                {
                                    var par = db.doc_follows.Find(new_tr.follow_parent_id);
                                    if (par != null)
                                    {
                                        new_tr.level = par.level + 1;
                                    }
                                    new_tr.parent_doc_status_id = par.doc_status_id;
                                }
                                new_tr.doc_status_id = "phoihop";
                                new_tr.message = model.message;
                                new_tr.created_by = uid;
                                new_tr.created_date = DateTime.Now;
                                new_tr.created_ip = ip;
                                new_tr.created_token_id = tid;
                                db.doc_follows.Add(new_tr);
                            }
                        }

                        //---------------------------------------

                        //-----------Master----------
                        var vb = db.doc_master.Find(model.doc_master_id);
                        if (vb != null)
                        {
                            vb.sent_by = user_now.user_key;
                            vb.follow_id = model.follow_id;
                            vb.handle_date = DateTime.Now;
                            vb.handle_by = new_fl.receive_last_group_user;
                            vb.doc_status_id = "xulychinh";
                            vb.message = model.message;
                        }

                        //----------------------
                        #region File upload
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        int numfileDoc = 1;
                        var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                        if (max_num.Count() > 0)
                        {
                            numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                        }
                        List<doc_files> listFileUp = new List<doc_files>();
                        List<string> listPathFileUp = new List<string>();
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
                                fileName = System.IO.Path.GetFileName(fileName);
                            }
                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            }
                            // defined file doc
                            doc_files file_doc = new doc_files();
                            file_doc.doc_master_id = vb.doc_master_id;
                            file_doc.follow_id = model.follow_id;
                            file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                            file_doc.is_order = numfileDoc;
                            file_doc.is_drafted = false;
                            file_doc.doc_file_type = 1;
                            file_doc.message = "Thêm file chuyển xử lý nhóm văn bản " + vb.compendium;
                            file_doc.created_by = uid;
                            file_doc.created_date = DateTime.Now;
                            file_doc.created_ip = ip;
                            file_doc.created_token_id = tid;
                            listFileUp.Add(file_doc);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                            numfileDoc++;
                        }
                        if (listFileUp.Count > 0)
                        {
                            db.doc_files.AddRange(listFileUp);
                        }

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
                        #endregion

                        //---------------------
                        #region add doc_logs
                        if (helper.wlog)
                        {
                            doc_logs log = new doc_logs();
                            log.log_type = 2;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = "Chuyển xử lý nhóm văn bản: " + vb.compendium;
                            log.doc_name = vb.compendium;
                            log.doc_master_id = vb.doc_master_id;
                            log.organization_id = user_now.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.doc_logs.Add(log);
                        }
                        #endregion

                        //-----------------------

                        var ns_sh = (from nss in db.sys_users
                                     where (user_sendhub.Contains(nss.user_id) || trackers_ids.Contains(nss.user_id)) && nss.status == 1
                                     select new { nss.user_id, nss.full_name, nss.avatar }).ToList();

                        #region export xml message
                        if (provider.FormData.GetValues("is_exported_xml") != null)
                        {
                            string fd_is_exported_xml = provider.FormData.GetValues("is_exported_xml").SingleOrDefault();
                            bool is_exported_xml = bool.Parse(fd_is_exported_xml);
                            if (is_exported_xml)
                            {
                                var content_xml = new doc_follows();
                                content_xml.doc_master_id = vb.doc_master_id;
                                content_xml.send_by_name = user_now.full_name;
                                content_xml.receive_by_name = "";
                                foreach (var ns in ns_sh)
                                {
                                    if (content_xml.receive_by_name != "") content_xml.receive_by_name += ", ";
                                    content_xml.receive_by_name += ns.full_name;
                                }
                                content_xml.message = model.message;
                                content_xml.send_date = send_date_now;
                                var path_save = helper.path_xml + "/Doc";
                                if (!Directory.Exists(path_save))
                                    Directory.CreateDirectory(path_save);
                                string res = ExportMessage_XML(content_xml, path_save);
                                if (res != "OK")
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = res });
                                }
                            }
                        }
                        #endregion

                        #region send socket
                        var lst_socket = new Dictionary<string, string>();
                        #endregion

                        #region sendhub
                        foreach (var ns in ns_sh)
                        {
                            string name_task = "";
                            if (user_sendhub.Contains(ns.user_id)) name_task = "Chuyển xử lý: ";
                            else if (trackers_ids.Contains(ns.user_id)) name_task = "Phối hợp: ";

                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.user_send = uid;
                            sh.module_key = const_module_key;
                            sh.receiver = ns.user_id;
                            sh.icon = ns.avatar;
                            sh.title = "Văn bản";
                            sh.contents = name_task + vb.compendium;
                        lst_socket.Add(ns.user_id, sh.contents);
            sh.type = 3;
                            sh.is_type = 0;
                            sh.date_send = DateTime.Now;
                            sh.id_key = model.doc_master_id.ToString();
                            sh.group_id = model.follow_id;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            db.sys_sendhub.Add(sh);
                        }
        #endregion

        string res_socket = SendSocketSingle(lst_socket, "Văn bản");

        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/FollowGroup_Doc", ip, tid, "Lỗi khi chuyển xử lý nhóm văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/FollowGroup_Doc", ip, tid, "Lỗi khi chuyển xử lý nhóm văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> FollowDepartment_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string followdepartment_item = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        DateTime send_date_now = DateTime.Now;

                        followdepartment_item = provider.FormData.GetValues("followdepartment_item").SingleOrDefault();
                        var user_sendhub_main = new List<string>();
                        var user_sendhub_track = new List<string>();
                        doc_follows model = JsonConvert.DeserializeObject<doc_follows>(followdepartment_item);

                        string fd_main_departments = provider.FormData.GetValues("main_departments").SingleOrDefault();
                        List<int> main_departments_ids = JsonConvert.DeserializeObject<List<int>>(fd_main_departments);

                        if (main_departments_ids.Count() > 0)
                        {
                            foreach (var main_departments_id in main_departments_ids)
                            {
                                var us_department = db.doc_ca_role_group_department.FirstOrDefault(x => (x.department_id ?? 0) == main_departments_id && x.role_group_id == null);
                                if (us_department != null)
                                {
                                    var us = db.sys_users.Find(us_department.user_id);
                                    var department = db.sys_organization.Find(main_departments_id);

                                    user_sendhub_main.Add(us_department.user_id);

                                    var new_fl = new doc_follows();
                                    new_fl.follow_id = helper.GenKey();
                                    new_fl.organization_id = model.organization_id;
                                    new_fl.doc_master_id = model.doc_master_id;
                                    new_fl.deadline_date = model.deadline_date;
                                    new_fl.send_by = user_now.user_key;
                                    new_fl.send_by_name = user_now.full_name;
                                    new_fl.send_date = send_date_now;
                                    new_fl.receive_by = main_departments_id;
                                    new_fl.receive_last_group_user = us.user_key;

                                    new_fl.receive_by_name = department?.organization_name;
                                    new_fl.receive_type = 3;
                                    new_fl.follow_parent_id = model.follow_id;
                                    new_fl.level = 1;

                                    if (new_fl.follow_parent_id != null)
                                    {
                                        var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                        if (par != null)
                                        {
                                            new_fl.level = par.level + 1;
                                        }
                                        new_fl.parent_doc_status_id = par.doc_status_id;
                                    }
                                    new_fl.doc_status_id = "xulychinh";
                                    new_fl.message = model.message;
                                    new_fl.created_by = uid;
                                    new_fl.created_date = DateTime.Now;
                                    new_fl.created_ip = ip;
                                    new_fl.created_token_id = tid;
                                    db.doc_follows.Add(new_fl);
                                }
                            }
                        }

                        //---------------------------------------

                        string fd_track_departments = provider.FormData.GetValues("track_departments").SingleOrDefault();
                        List<int> track_departments_ids = JsonConvert.DeserializeObject<List<int>>(fd_track_departments);

                        if (track_departments_ids.Count() > 0)
                        {
                            foreach (var track_departments_id in track_departments_ids)
                            {
                                var us_department = db.doc_ca_role_group_department.FirstOrDefault(x => (x.department_id ?? 0) == track_departments_id && x.role_group_id == null);
                                if (us_department != null)
                                {
                                    var us = db.sys_users.Find(us_department.user_id);
                                    var department = db.sys_organization.Find(track_departments_id);

                                    user_sendhub_track.Add(us_department.user_id);

                                    var new_fl = new doc_follows();
                                    new_fl.follow_id = helper.GenKey();
                                    new_fl.organization_id = model.organization_id;
                                    new_fl.doc_master_id = model.doc_master_id;
                                    new_fl.deadline_date = model.deadline_date;
                                    new_fl.send_by = user_now.user_key;
                                    new_fl.send_by_name = user_now.full_name;
                                    new_fl.send_date = send_date_now;
                                    new_fl.receive_by = track_departments_id;
                                    new_fl.receive_last_group_user = us.user_key;

                                    new_fl.receive_by_name = department?.organization_name;
                                    new_fl.receive_type = 3;
                                    new_fl.follow_parent_id = model.follow_id;
                                    new_fl.level = 1;

                                    if (new_fl.follow_parent_id != null)
                                    {
                                        var par = db.doc_follows.Find(new_fl.follow_parent_id);
                                        if (par != null)
                                        {
                                            new_fl.level = par.level + 1;
                                        }
                                        new_fl.parent_doc_status_id = par.doc_status_id;
                                    }
                                    new_fl.doc_status_id = "phoihop";
                                    new_fl.message = model.message;
                                    new_fl.created_by = uid;
                                    new_fl.created_date = DateTime.Now;
                                    new_fl.created_ip = ip;
                                    new_fl.created_token_id = tid;
                                    db.doc_follows.Add(new_fl);
                                }
                            }
                        }

                        //---------------------------------------

                        string trackers = provider.FormData.GetValues("trackers").SingleOrDefault();
                        List<string> trackers_ids = JsonConvert.DeserializeObject<List<string>>(trackers);

                        if (trackers_ids.Count() > 0)
                        {
                            foreach (var trackers_id in trackers_ids)
                            {
                                var us = db.sys_users.Find(trackers_id);

                                var new_tr = new doc_follows();
                                new_tr.follow_id = helper.GenKey();
                                new_tr.organization_id = model.organization_id;
                                new_tr.doc_master_id = model.doc_master_id;
                                new_tr.send_by = user_now.user_key;
                                new_tr.send_by_name = user_now.full_name;
                                new_tr.send_date = send_date_now;
                                new_tr.receive_by = us.user_key;

                                new_tr.receive_by_name = us.full_name;
                                new_tr.receive_type = 0;
                                new_tr.follow_parent_id = model.follow_id;
                                new_tr.level = 1;

                                if (new_tr.follow_parent_id != null)
                                {
                                    var par = db.doc_follows.Find(new_tr.follow_parent_id);
                                    if (par != null)
                                    {
                                        new_tr.level = par.level + 1;
                                    }
                                    new_tr.parent_doc_status_id = par.doc_status_id;
                                }
                                new_tr.doc_status_id = "phoihop";
                                new_tr.message = model.message;
                                new_tr.created_by = uid;
                                new_tr.created_date = DateTime.Now;
                                new_tr.created_ip = ip;
                                new_tr.created_token_id = tid;
                                db.doc_follows.Add(new_tr);
                            }
                        }

                        //---------------------------------------

                        //-----------Master----------
                        var vb = db.doc_master.Find(model.doc_master_id);
                        if (vb != null)
                        {
                            vb.sent_by = user_now.user_key;
                            vb.follow_id = model.follow_id;
                            vb.handle_date = DateTime.Now;
                            vb.doc_status_id = "xulychinh";
                            vb.message = model.message;
                        }

                        //----------------------
                        #region File upload
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        int numfileDoc = 1;
                        var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                        if (max_num.Count() > 0)
                        {
                            numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                        }
                        List<doc_files> listFileUp = new List<doc_files>();
                        List<string> listPathFileUp = new List<string>();
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
                                fileName = System.IO.Path.GetFileName(fileName);
                            }
                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            }
                            // defined file doc
                            doc_files file_doc = new doc_files();
                            file_doc.doc_master_id = vb.doc_master_id;
                            file_doc.follow_id = model.follow_id;
                            file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                            file_doc.is_order = numfileDoc;
                            file_doc.is_drafted = false;
                            file_doc.doc_file_type = 1;
                            file_doc.message = "Thêm file chuyển xử lý phòng ban văn bản " + vb.compendium;
                            file_doc.created_by = uid;
                            file_doc.created_date = DateTime.Now;
                            file_doc.created_ip = ip;
                            file_doc.created_token_id = tid;
                            listFileUp.Add(file_doc);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                            numfileDoc++;
                        }
                        if (listFileUp.Count > 0)
                        {
                            db.doc_files.AddRange(listFileUp);
                        }

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
                        #endregion

                        //---------------------
                        #region add doc_logs
                        if (helper.wlog)
                        {
                            doc_logs log = new doc_logs();
                            log.log_type = 2;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = "Chuyển xử lý phòng ban văn bản: " + vb.compendium;
                            log.doc_name = vb.compendium;
                            log.doc_master_id = vb.doc_master_id;
                            log.organization_id = user_now.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.doc_logs.Add(log);
                        }
                        #endregion

                        //-----------------------
                        var ns_sh = (from nss in db.sys_users
                                     where (user_sendhub_main.Contains(nss.user_id) || user_sendhub_track.Contains(nss.user_id) || trackers_ids.Contains(nss.user_id)) && nss.status == 1
                                     select new { nss.user_id, nss.full_name, nss.avatar }).ToList();

                        #region export xml message
                        if (provider.FormData.GetValues("is_exported_xml") != null)
                        {
                            string fd_is_exported_xml = provider.FormData.GetValues("is_exported_xml").SingleOrDefault();
                            bool is_exported_xml = bool.Parse(fd_is_exported_xml);
                            if (is_exported_xml)
                            {
                                var content_xml = new doc_follows();
                                content_xml.doc_master_id = vb.doc_master_id;
                                content_xml.send_by_name = user_now.full_name;
                                content_xml.receive_by_name = "";
                                foreach (var ns in ns_sh)
                                {
                                    if (content_xml.receive_by_name != "") content_xml.receive_by_name += ", ";
                                    content_xml.receive_by_name += ns.full_name;
                                }
                                content_xml.message = model.message;
                                content_xml.send_date = send_date_now;
                                var path_save = helper.path_xml + "/Doc";
                                if (!Directory.Exists(path_save))
                                    Directory.CreateDirectory(path_save);
                                string res = ExportMessage_XML(content_xml, path_save);
                                if (res != "OK")
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = res });
                                }
                            }
                        }
                        #endregion

                        #region send socket
                        var lst_socket = new Dictionary<string, string>();
                        #endregion

                        //----------------------------
                        #region sendhub

                        foreach (var ns in ns_sh)
                        {
                            string name_task = "";
                            if (user_sendhub_main.Contains(ns.user_id)) name_task = "Chuyển xử lý: ";
                            else if (user_sendhub_track.Contains(ns.user_id) || trackers_ids.Contains(ns.user_id)) name_task = "Phối hợp: ";

                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.user_send = uid;
                            sh.module_key = const_module_key;
                            sh.receiver = ns.user_id;
                            sh.icon = ns.avatar;
                            sh.title = "Văn bản";
                            sh.contents = name_task + vb.compendium;
                        lst_socket.Add(ns.user_id, sh.contents);
                            sh.type = 3;
                            sh.is_type = 0;
                            sh.date_send = DateTime.Now;
                            sh.id_key = model.doc_master_id.ToString();
                            sh.group_id = model.follow_id;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            db.sys_sendhub.Add(sh);
                        }

        #endregion

        string res_socket = SendSocketSingle(lst_socket, "Văn bản");

        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/FollowDepartment_Doc", ip, tid, "Lỗi khi chuyển xử lý phòng ban văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/FollowDepartment_Doc", ip, tid, "Lỗi khi chuyển xử lý phòng ban văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> TransferStamp_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string stamp_item = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        DateTime send_date_now = DateTime.Now;

                        stamp_item = provider.FormData.GetValues("stamp_item").SingleOrDefault();
                        doc_follows model = JsonConvert.DeserializeObject<doc_follows>(stamp_item);

                        var us = db.sys_users.FirstOrDefault(x => x.user_key == model.receive_by);

                        var new_fl = new doc_follows();
                        new_fl.follow_id = helper.GenKey();
                        new_fl.organization_id = model.organization_id;
                        new_fl.doc_master_id = model.doc_master_id;
                        new_fl.deadline_date = model.deadline_date;
                        new_fl.send_by = user_now.user_key;
                        new_fl.send_by_name = user_now.full_name;
                        new_fl.send_date = send_date_now;
                        new_fl.receive_by = us.user_key;

                        new_fl.receive_by_name = us.full_name;
                        new_fl.receive_type = 0;
                        new_fl.follow_parent_id = model.follow_id;
                        new_fl.level = 1;

                        if (new_fl.follow_parent_id != null)
                        {
                            var par = db.doc_follows.Find(new_fl.follow_parent_id);
                            if (par != null)
                            {
                                new_fl.level = par.level + 1;
                            }
                            new_fl.parent_doc_status_id = par.doc_status_id;
                        }
                        new_fl.doc_status_id = "chodongdau";
                        new_fl.message = model.message;
                        new_fl.created_by = uid;
                        new_fl.created_date = DateTime.Now;
                        new_fl.created_ip = ip;
                        new_fl.created_token_id = tid;
                        db.doc_follows.Add(new_fl);

                        //---------------------------------------

                        //-----------Master----------
                        var vb = db.doc_master.Find(model.doc_master_id);
                        if (vb != null)
                        {
                            vb.sent_by = user_now.user_key;
                            vb.follow_id = model.follow_id;
                            vb.handle_date = DateTime.Now;
                            vb.handle_by = model.receive_by;
                            vb.doc_status_id = "chodongdau";
                            vb.message = model.message;
                        }

                        //----------------------
                        #region File upload
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        int numfileDoc = 1;
                        var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                        if (max_num.Count() > 0)
                        {
                            numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                        }
                        List<doc_files> listFileUp = new List<doc_files>();
                        List<string> listPathFileUp = new List<string>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            numfileDoc++;
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
                                fileName = System.IO.Path.GetFileName(fileName);
                            }
                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            }
                            // defined file doc
                            doc_files file_doc = new doc_files();
                            file_doc.doc_master_id = vb.doc_master_id;
                            file_doc.follow_id = model.follow_id;
                            file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                            file_doc.is_order = numfileDoc;
                            file_doc.is_drafted = false;
                            file_doc.doc_file_type = 1;
                            file_doc.message = "Thêm file chuyển đóng dấu văn bản " + vb.compendium;
                            file_doc.created_by = uid;
                            file_doc.created_date = DateTime.Now;
                            file_doc.created_ip = ip;
                            file_doc.created_token_id = tid;
                            listFileUp.Add(file_doc);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }
                        if (listFileUp.Count > 0)
                        {
                            db.doc_files.AddRange(listFileUp);
                        }

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
                        #endregion

                        //---------------------
                        #region add doc_logs
                        if (helper.wlog)
                        {
                            doc_logs log = new doc_logs();
                            log.log_type = 7;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = "Chuyển đóng dấu văn bản: " + vb.compendium;
                            log.doc_name = vb.compendium;
                            log.doc_master_id = vb.doc_master_id;
                            log.organization_id = user_now.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.doc_logs.Add(log);
                        }
                        #endregion

                        //-----------------------
                        var ns_sh = (from nss in db.sys_users
                                     where nss.user_key == model.receive_by && nss.status == 1
                                     select new { nss.user_id, nss.full_name, nss.avatar }).ToList();

                        #region export xml message
                        if (provider.FormData.GetValues("is_exported_xml") != null)
                        {
                            string fd_is_exported_xml = provider.FormData.GetValues("is_exported_xml").SingleOrDefault();
                            bool is_exported_xml = bool.Parse(fd_is_exported_xml);
                            if (is_exported_xml)
                            {
                                var content_xml = new doc_follows();
                                content_xml.doc_master_id = vb.doc_master_id;
                                content_xml.send_by_name = user_now.full_name;
                                content_xml.receive_by_name = "";
                                foreach (var ns in ns_sh)
                                {
                                    if (content_xml.receive_by_name != "") content_xml.receive_by_name += ", ";
                                    content_xml.receive_by_name += ns.full_name;
                                }
                                content_xml.message = model.message;
                                content_xml.send_date = send_date_now;
                                var path_save = helper.path_xml + "/Doc";
                                if (!Directory.Exists(path_save))
                                    Directory.CreateDirectory(path_save);
                                string res = ExportMessage_XML(content_xml, path_save);
                                if (res != "OK")
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = res });
                                }
                            }
                        }
                        #endregion

                        #region send socket
                        string res_socket = SendSocketMultiple(ns_sh.Select(x => x.user_id), "Văn bản", "Chuyển đóng dấu: " + vb.compendium);
                        #endregion

                        #region sendhub
                        foreach (var ns in ns_sh)
                        {
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.module_key = const_module_key;
                            sh.user_send = uid;
                            sh.receiver = ns.user_id;
                            sh.icon = ns.avatar;
                            sh.title = "Văn bản";
                            sh.contents = "Chuyển đóng dấu: " + vb.compendium;
                            sh.type = 3;
                            sh.is_type = 0;
                            sh.date_send = DateTime.Now;
                            sh.id_key = model.doc_master_id.ToString();
                            sh.group_id = model.follow_id;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            db.sys_sendhub.Add(sh);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/TransferStamp_Doc", ip, tid, "Lỗi khi đóng dấu văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/TransferStamp_Doc", ip, tid, "Lỗi khi đóng dấu văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Stamp_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fddoc = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            int countNum = 0;
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fddoc = provider.FormData.GetValues("doc").SingleOrDefault();
                        doc_master vb = JsonConvert.DeserializeObject<doc_master>(fddoc);
                        int notunique_case = 0;
                        if (vb != null)
                        {
                            if (vb.dispatch_book_code != null)
                            {
                                string str = "";
                                do
                                {
                                    if (str.Contains("duplicatedocnum") && (vb.is_auto_num ?? false))
                                    {
                                        notunique_case = 1;
                                    }
                                    else if (str.Contains("duplicatedocnum") && (!vb.is_auto_num ?? true))
                                    {
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", notunique_case = "2" });
                                    }
                                    else if (str.Contains("duplicatedoccode"))
                                    {
                                        if (vb.doc_code == vb.dispatch_book_code && vb.nav_type != 1 && (vb.is_auto_num ?? false))
                                        {
                                            notunique_case = 1;
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", notunique_case = "3" });
                                        }
                                    }
                                    else if (str != "OK" && str != "")
                                    {
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = str });
                                    }
                                    str = Update_Stamp_detailDoc(provider, uid, tid, ip, root, countNum++);
                                }
                                while (str.Contains("duplicatedocnum") || str != "OK");
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", notunique_case });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Stamp_Doc", ip, tid, "Lỗi khi đóng dấu văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Stamp_Doc", ip, tid, "Lỗi khi đóng dấu văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        public string Update_Stamp_detailDoc(MultipartFormDataStreamProvider provider, string created_by, string tid, string ip, string root, double count = 0)
        {
            string fddoc = "";
            using (DBEntities db = new DBEntities())
            {
                try
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == created_by);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }

                    string fdis_stamp = provider.FormData.GetValues("is_stamp").SingleOrDefault();
                    bool is_stamp = bool.Parse(fdis_stamp);

                    fddoc = provider.FormData.GetValues("doc").SingleOrDefault();
                    doc_master vb = JsonConvert.DeserializeObject<doc_master>(fddoc);
                    var follow = db.doc_follows.Find(vb.follow_id);
                    if (vb.doc_master_id != 0)
                    {
                        // Auto increase so vao so
                        if (vb.is_auto_num ?? false)
                        {
                            bool isNumeric = double.TryParse(vb.dispatch_book_code, out double n);
                            if (isNumeric)
                            {
                                n += count;
                                if (vb.doc_code == vb.dispatch_book_code) vb.doc_code = n.ToString();
                                vb.dispatch_book_code = n.ToString();
                                vb.dispatch_book_num = n;
                            }
                            else
                            {
                                var fdsokhtemp = provider.FormData.GetValues("auto_doc_code").SingleOrDefault();
                                List<DocCodeTemp> sokhtemp = JsonConvert.DeserializeObject<List<DocCodeTemp>>(fdsokhtemp);
                                if (sokhtemp != null && sokhtemp.Count() > 0)
                                {
                                    var final_sokh = "";
                                    double sovaosotemp = 0;
                                    foreach (var sokh in sokhtemp)
                                    {
                                        if (sokh.is_number)
                                        {
                                            bool ok = double.TryParse(sokh.value, out double output);
                                            if (ok)
                                            {
                                                sokh.value = (output + count).ToString();
                                                sovaosotemp = output + count;
                                            }
                                        }
                                        final_sokh += sokh.value + sokh.separator;
                                    }
                                    if (vb.doc_code == vb.dispatch_book_code) vb.doc_code = final_sokh;
                                    vb.dispatch_book_code = final_sokh;
                                    vb.dispatch_book_num = sovaosotemp;
                                }
                            }
                        }
                        else
                        {
                            bool isNumeric = double.TryParse(vb.dispatch_book_code, out double n);
                            if (isNumeric)
                            {
                                vb.dispatch_book_num = n;
                            }
                        }
                        // ---------------------------------
                        if (vb.doc_group_id != null)
                        {
                            var nvb = db.doc_ca_group_departments.FirstOrDefault(x => x.doc_group_id == vb.doc_group_id && x.department_id == vb.department_id);
                            if (nvb.current_num != null)
                            nvb.current_num = nvb.current_num + 1;
                        }
                        //----------------------------------
                        var curvb = db.doc_master.AsNoTracking().FirstOrDefault(x => x.doc_master_id == vb.doc_master_id);
                        if (vb.doc_group_id != null)
                        {
                            if (curvb.doc_group_id != vb.doc_group_id)
                            {
                                var nvb = db.doc_ca_groups.Find(vb.doc_group_id);
                                if (nvb.current_num != null)
                                    nvb.current_num = nvb.current_num + 1;
                            }
                        }
                        if (vb.dispatch_book_id != null)
                        {
                            if (curvb.dispatch_book_id != vb.dispatch_book_id)
                            {
                                var scv = db.doc_ca_dispatch_books.Find(vb.dispatch_book_id);
                                scv.current_num = scv.current_num + 1;
                            }
                        }

                        vb.doc_status_id = "dadongdau";
                        vb.is_drafted = false;
                        vb.handle_date = DateTime.Now;

                        if (follow.doc_status_id != "dadongdau")
                        {
                            var new_fl = new doc_follows();
                            new_fl.follow_id = helper.GenKey();
                            new_fl.organization_id = follow.organization_id;
                            new_fl.doc_master_id = follow.doc_master_id;
                            new_fl.send_by = user_now.user_key;
                            new_fl.send_by_name = user_now.full_name;
                            new_fl.send_date = DateTime.Now;
                            new_fl.receive_by = user_now.user_key;

                            new_fl.receive_by_name = user_now.full_name;
                            new_fl.receive_type = 0;
                            new_fl.follow_parent_id = follow.follow_id;
                            new_fl.level = follow.level + 1;
                            new_fl.parent_doc_status_id = follow.doc_status_id;
                            new_fl.doc_status_id = "dadongdau";
                            new_fl.created_by = created_by;
                            new_fl.created_date = DateTime.Now;
                            new_fl.created_ip = ip;
                            new_fl.created_token_id = tid;
                            new_fl.is_stamped = true;
                            new_fl.stamped_date = DateTime.Now;
                            db.doc_follows.Add(new_fl);
                        }
                        else
                        {
                            follow.modified_by = created_by;
                            follow.modified_date = DateTime.Now;
                            follow.modified_ip = ip;
                            follow.modified_token_id = tid;
                        }

                        //----------Reservation number----------
                        string fd_is_reservation_number = provider.FormData.GetValues("is_reservation_number").SingleOrDefault();
                        if (fd_is_reservation_number != null && fd_is_reservation_number != "")
                        {
                            bool is_reservation_number = bool.Parse(fd_is_reservation_number);
                            if (is_reservation_number)
                            {
                                var reservation_number = db.doc_reservation_number.FirstOrDefault(x => x.organization_id == vb.organization_id && x.nav_type == vb.nav_type && x.reservation_code.Trim().ToLower() == vb.doc_code.Trim().ToLower() && !x.is_used);
                                if (reservation_number != null)
                                {
                                    reservation_number.is_used = true;
                                }
                            }
                        }


                        if ((provider.FormData.GetValues("docUploadOld") != null) && string.IsNullOrEmpty(vb.file_path))
                        {
                            string pathDel = provider.FormData.GetValues("docUploadOld").SingleOrDefault();
                            if (pathDel.Contains("/Portals/") && pathDel.Contains("/Doc/"))
                            {
                                bool existFiles = System.IO.File.Exists(root + pathDel);
                                if (existFiles)
                                    System.IO.File.Delete(root + pathDel);
                            }
                        };

                        db.Entry(vb).State = EntityState.Modified;
                    }

                    var old_relates = db.doc_related.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                    if (old_relates.Count() > 0) db.doc_related.RemoveRange(old_relates);
                    if (provider.FormData.GetValues("doc_relates") != null)
                    {
                        string doc_relates = provider.FormData.GetValues("doc_relates").SingleOrDefault();
                        List<doc_related> listRelateDoc = JsonConvert.DeserializeObject<List<doc_related>>(doc_relates);
                        if (listRelateDoc != null && listRelateDoc.Count > 0)
                        {
                            List<doc_related> listRelate = new List<doc_related>();
                            foreach (var item in listRelateDoc)
                            {
                                doc_related relate = new doc_related();
                                relate.doc_master_id = vb.doc_master_id;
                                relate.doc_related_id = item.doc_master_id;
                                relate.created_by = created_by;
                                relate.created_date = DateTime.Now;
                                relate.created_ip = ip;
                                relate.created_token_id = tid;
                                listRelate.Add(relate);
                            }
                            if (listRelate.Count > 0)
                            {
                                db.doc_related.AddRange(listRelate);
                            }
                        }
                    }

                    // ky dien tu
                    List<Dictionary<string, List<WaterImage>>> WaterImages = new List<Dictionary<string, List<WaterImage>>>();
                    if (provider.FormData.GetValues("waterimages") != null)
                    {
                        string wimg = provider.FormData.GetValues("waterimages").SingleOrDefault();
                        WaterImages = JsonConvert.DeserializeObject<List<Dictionary<string, List<WaterImage>>>>(wimg);
                    }
                    List<Dictionary<string, List<WaterTxt>>> WaterTxts = new List<Dictionary<string, List<WaterTxt>>>();
                    if (provider.FormData.GetValues("watertxts") != null)
                    {
                        string wtxt = provider.FormData.GetValues("watertxts").SingleOrDefault();
                        WaterTxts = JsonConvert.DeserializeObject<List<Dictionary<string, List<WaterTxt>>>>(wtxt);
                    }
                    var fileduthao = db.doc_files.FirstOrDefault(x => x.doc_master_id == vb.doc_master_id && (x.is_drafted ?? false));
                    if (vb.is_approved != null)
                    {
                        string res = "";
                        foreach (var lst_wt in WaterTxts)
                        {
                            foreach (KeyValuePair<string, List<WaterTxt>> entry_txt in lst_wt)
                            {
                                if (fileduthao == null || (fileduthao != null && entry_txt.Key != fileduthao.file_path))
                                {
                                    res = SignDoc_Txt(root, entry_txt.Key, entry_txt.Value);
                                    if (res != "OK")
                                    {
                                        return "Chèn thông tin văn bản lỗi!";
                                    }
                                }
                            }
                        }
                        if (res == "OK")
                        {
                            if (WaterImages.Count() > 0)
                            {
                                foreach (var lst_wi in WaterImages)
                                {
                                    foreach (KeyValuePair<string, List<WaterImage>> entry_img in lst_wi)
                                    {
                                        if (entry_img.Key == vb.file_path)
                                        {
                                            string destFolder = root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid;
                                            string fromFile = root + vb.file_path;
                                            string added = " (không dấu)";

                                            bool exists = System.IO.File.Exists(fromFile);
                                            bool exists_filekhongdau = db.doc_files.AsNoTracking().Any(x => x.doc_master_id == vb.doc_master_id && x.is_drafted == false && x.doc_file_type == 5);
                                            if (exists && !exists_filekhongdau)
                                            {
                                                string name = helper.UniqueFileName(System.IO.Path.GetFileNameWithoutExtension(vb.file_name) + added + vb.file_name.GetFileExtension());
                                                string destFile = destFolder + "/" + name;
                                                string Dinhdang = helper.GetFileExtension(vb.file_name);
                                                if (destFile.Length > 260)
                                                {
                                                    name = name.Substring(0, name.LastIndexOf('.') - 1);
                                                    int le = 260 - (destFolder.Length + 1) - Dinhdang.Length;
                                                    name = name.Substring(0, le) + Dinhdang;
                                                }
                                                destFile = destFolder + "/" + name;

                                                var file_doc = new doc_files();
                                                string Duongdan = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid;

                                                System.IO.File.Copy(fromFile, destFile);

                                                var total = db.doc_files.AsNoTracking().Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                                                var stt = 0;
                                                if (total.Count() > 0)
                                                {
                                                    stt = total.Max(x => x.is_order) ?? 0;
                                                }
                                                file_doc.doc_master_id = vb.doc_master_id;
                                                file_doc.follow_id = vb.follow_id;
                                                file_doc.file_path = Duongdan;
                                                file_doc.file_name = System.IO.Path.GetFileNameWithoutExtension(vb.file_name) + added + vb.file_name.GetFileExtension();
                                                //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                                                file_doc.file_size = vb.file_size;
                                                file_doc.file_type = Dinhdang;
                                                file_doc.is_order = stt + 1;
                                                file_doc.is_drafted = false;
                                                file_doc.doc_file_type = 5;
                                                file_doc.message = "Thêm file backup không dấu văn bản " + vb.compendium;
                                                file_doc.created_by = created_by;
                                                file_doc.created_date = DateTime.Now;
                                                file_doc.created_ip = ip;
                                                file_doc.created_token_id = tid;
                                            }
                                        }

                                        if (fileduthao == null || (fileduthao != null && entry_img.Key != fileduthao.file_path))
                                        {
                                            string res1 = SignDoc(root, entry_img.Key, entry_img.Value);
                                            if (res1 != "OK")
                                            {
                                                return "Chèn dấu lỗi!";
                                            }
                                        }
                                    }
                                }
                            }
                            vb.file_size = new System.IO.FileInfo(root + vb.file_path).Length;
                        }
                    }

                    //------------- Ky CA (vgca) --------------
                    List<doc_sign_approval> ca_files = new List<doc_sign_approval>();
                    if (provider.FormData.GetValues("ca_files") != null)
                    {
                        string fd_ca_files = provider.FormData.GetValues("ca_files").SingleOrDefault();
                        ca_files = JsonConvert.DeserializeObject<List<doc_sign_approval>>(fd_ca_files);
                    }

                    foreach (var file_ca in ca_files)
                    {
                        var dir_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/";
                        if (file_ca.doc_master_id != null)
                        {
                            if (File.Exists(root + vb.file_path))
                            {
                                File.Delete(root + vb.file_path);
                            }
                            var new_FileName = System.IO.Path.GetFileName(file_ca.file_path);
                            var new_path = root + dir_path + new_FileName;
                            File.Move(root + file_ca.file_path, new_path);
                            vb.file_name = new_FileName;
                            vb.file_path = dir_path + new_FileName;
                            vb.file_size = new System.IO.FileInfo(root + vb.file_path).Length;
                        }
                        else if (file_ca.file_id != null)
                        {
                            var file_vb = db.doc_files.Find(file_ca.file_id);
                            if (File.Exists(root + file_vb.file_path))
                            {
                                File.Delete(root + file_vb.file_path);
                            }
                            var new_FileName = System.IO.Path.GetFileName(file_ca.file_path);
                            var new_path = root + dir_path + new_FileName;
                            File.Move(root + file_ca.file_path, new_path);
                            file_vb.file_name = new_FileName;
                            file_vb.file_path = dir_path + new_FileName;
                            file_vb.file_size = new System.IO.FileInfo(root + file_vb.file_path).Length;
                        }
                    }

                    // File upload
                    // This illustrates how to get thefile names.
                    FileInfo fileInfo = null;
                    MultipartFileData ffileData = null;
                    string newFileName = "";

                    int numfileDoc = 1;
                    var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                    if (max_num.Count() > 0)
                    {
                        numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                    }
                    List<doc_files> listFileUp = new List<doc_files>();
                    List<string> listPathFileUp = new List<string>();

                    if (fileduthao == null)
                    {
                        var fidt = new doc_files();
                        fidt.doc_master_id = vb.doc_master_id;
                        if (vb.is_approved ?? false)
                        {
                            string FilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(vb.file_path), System.IO.Path.GetFileNameWithoutExtension(vb.file_path)) + ".doc";
                            if (!System.IO.File.Exists(root + FilePath))
                            {
                                FilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(vb.file_path), System.IO.Path.GetFileNameWithoutExtension(vb.file_path)) + ".docx";
                                fidt.file_name = System.IO.Path.GetFileNameWithoutExtension(vb.file_name) + ".docx";
                                fidt.file_path = FilePath;
                                fidt.file_type = "docx";
                            }
                            else
                            {
                                fidt.file_name = System.IO.Path.GetFileNameWithoutExtension(vb.file_name) + ".doc";
                                fidt.file_path = FilePath;
                                fidt.file_type = "doc";
                            }
                        }
                        else
                        {
                            fidt.file_name = vb.file_name;
                            fidt.file_path = vb.file_path;
                            fidt.file_type = vb.file_type;
                        }
                        fidt.file_size = vb.file_size ?? default(double);
                        fidt.is_order = numfileDoc;
                        fidt.created_by = created_by;
                        fidt.created_date = DateTime.Now;
                        fidt.created_ip = ip;
                        fidt.created_token_id = tid;
                        fidt.is_drafted = true;
                        fidt.doc_file_type = 0;
                        fidt.follow_id = vb.follow_id;
                        db.doc_files.Add(fidt);
                    }

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
                            fileName = System.IO.Path.GetFileName(fileName);
                        }
                        newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                        fileInfo = new FileInfo(newFileName);
                        if (fileInfo.Exists)
                        {
                            fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                            fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                            // Convert to unsign
                            Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                            fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                        }

                        if (fileData.Headers.ContentDisposition.Name == "\"doc_file\"")
                        {
                            vb.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            vb.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            vb.file_size = new FileInfo(fileData.LocalFileName).Length;
                            vb.file_type = vb.file_name.Substring(vb.file_name.LastIndexOf(".") + 1).ToLower();

                            //if (Dinhdang.ToLower() == "pdf")
                            //{
                            //    string logocty = "";
                            //    var cty = await db.HT_TD_Congty.FindAsync(vb.Congty_ID);
                            //    if (cty != null && cty.Watermark)
                            //    {
                            //        logocty = cty.Img_Watermark;
                            //        if (logocty != null)
                            //        {
                            //            string file = path + "/" + name;
                            //            addWatermarkPDF(helper.rootPath + '/' + logocty, file);
                            //        }
                            //    }
                            //}
                        }
                        else
                        {
                            doc_files file_doc = new doc_files();
                            file_doc.doc_master_id = vb.doc_master_id;
                            file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                            file_doc.is_order = numfileDoc;
                            file_doc.is_drafted = false;
                            file_doc.doc_file_type = 0;
                            file_doc.message = "Thêm file văn bản đóng dấu" + vb.compendium;
                            file_doc.created_by = created_by;
                            file_doc.created_date = DateTime.Now;
                            file_doc.created_ip = ip;
                            file_doc.created_token_id = tid;
                            listFileUp.Add(file_doc);

                            //if (Dinhdang.ToLower() == "pdf")
                            //{
                            //    string logocty = "";
                            //    var cty = await db.HT_TD_Congty.FindAsync(vb.Congty_ID);
                            //    if (cty != null && cty.Watermark)
                            //    {
                            //        logocty = cty.Img_Watermark;
                            //        if (logocty != null)
                            //        {
                            //            string file = path + "/" + name;
                            //            addWatermarkPDF(helper.rootPath + '/' + logocty, file);
                            //        }
                            //    }
                            //}
                        }
                        numfileDoc++;
                        ffileData = fileData;
                        //Add file
                        if (fileInfo != null)
                        {
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            File.Move(ffileData.LocalFileName, newFileName);
                            listPathFileUp.Add(ffileData.LocalFileName);
                        }
                    }

                    //if (provider.FileData.FirstOrDefault(x => x.Headers.ContentDisposition.Name == "\"doc_file\"") == null && !(vb.is_approved ?? false))
                    //{
                    //    vb.file_name = null;
                    //    vb.file_size = null;
                    //    vb.file_path = null;
                    //    vb.file_type = null;
                    //}

                    if (listFileUp.Count > 0)
                    {
                        db.doc_files.AddRange(listFileUp);
                    }

                    if (provider.FormData.GetValues("fileUploadOld") != null)
                    {
                        string doc_old_files = provider.FormData.GetValues("fileUploadOld").SingleOrDefault();
                        List<doc_files> listFileOld = JsonConvert.DeserializeObject<List<doc_files>>(doc_old_files);
                        List<string> pathFilesDel = new List<string>();
                        if (listFileOld != null && listFileOld.Count > 0)
                        {
                            List<doc_files> listFileDel = new List<doc_files>();
                            var listFileDelTemp = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                            var delItems = listFileDelTemp.Where(x => listFileOld.Count(y => y.file_id == x.file_id) > 0).ToList();
                            foreach (var item in delItems)
                            {
                                listFileDel.Add(item);
                                pathFilesDel.Add(item.file_path);
                            }
                            if (listFileDel.Count > 0)
                            {
                                db.doc_files.RemoveRange(listFileDel);
                            }
                        }
                        foreach (var pathDel in pathFilesDel)
                        {
                            if (pathDel.Contains("/Portals/") && pathDel.Contains("/Doc/"))
                            {
                                bool existFiles = System.IO.File.Exists(root + pathDel);
                                if (existFiles)
                                    System.IO.File.Delete(root + pathDel);
                            }
                        }
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
                    //---------------------
                    #region add doc_logs
                    if (helper.wlog)
                    {
                        doc_logs log = new doc_logs();
                        log.log_type = 8;
                        //log.message = JsonConvert.SerializeObject(new { data = law_main });
                        log.message = "Đóng dấu văn bản: " + vb.compendium;
                        log.doc_name = vb.compendium;
                        log.doc_master_id = vb.doc_master_id;
                        log.organization_id = vb.organization_id;
                        log.created_date = DateTime.Now;
                        log.created_by = created_by;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        log.is_view = false;
                        db.doc_logs.Add(log);
                        db.SaveChanges();
                    }
                    #endregion
                    return "OK";
                }
                catch (Exception e)
                {
                    if (e.InnerException.InnerException.Message.Contains("with unique index 'idx_dispatch_book_code_notnull"))
                    {
                        return "duplicatedocnum";
                    }
                    else if (e.InnerException.InnerException.Message.Contains("with unique index 'idx_doc_code_notnull"))
                    {
                        return "duplicatedoccode";
                    }
                    else
                    {
                        string contents = helper.ExceptionMessage(e);
                        return contents;
                    }
                }
            }
        }
        bool main_completed = true;
        public string Recursive_CompletedConfirm(string uid, string ip, string tid, doc_follows model)
        {
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var follow = db.doc_follows.Find(model.follow_id);
                    var fl_par = new doc_follows();
                    if (follow.follow_parent_id != null)
                    {
                        fl_par = db.doc_follows.Find(follow.follow_parent_id);
                    }
                    var vb = db.doc_master.Find(follow.doc_master_id);
                    if (follow != null && follow.doc_status_id != "tralai")
                    {
                        follow.is_completed = true;
                        follow.completed_date = DateTime.Now;
                        follow.completed_message = model.message;
                        follow.modified_by = uid;
                        follow.modified_date = DateTime.Now;
                        follow.modified_ip = ip;
                        follow.modified_token_id = tid;
                        if (main_completed)
                        {
                            follow.is_main_completed = true;
                            main_completed = false;
                        }

                        #region sendhub
                        var ns_sh = (from nss in db.sys_users
                                     where nss.user_key == follow.receive_by && nss.status == 1
                                     select new { nss.user_id, nss.full_name, nss.avatar }).ToList();

                        #region send socket
                        string res_socket = SendSocketMultiple(ns_sh.Select(x => x.user_id), "Văn bản", "Xác nhận hoàn thành: " + vb.compendium);
                        #endregion

                        foreach (var ns in ns_sh)
                        {
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.user_send = uid;
                            sh.receiver = ns.user_id;
                            sh.module_key = const_module_key;
                            sh.icon = ns.avatar;
                            sh.title = "Văn bản";
                            sh.contents = "Xác nhận hoàn thành: " + vb.compendium;
                            sh.type = 3;
                            sh.is_type = 0;
                            sh.date_send = DateTime.Now;
                            sh.id_key = model.doc_master_id.ToString();
                            sh.group_id = follow.follow_id;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            db.sys_sendhub.Add(sh);
                        }
                        #endregion
                    }

                    var child_samepar = (from a in db.doc_follows
                                         where a.doc_master_id == vb.doc_master_id
                                         && !(a.is_completed ?? false) && a.doc_status_id != "tralai" && a.follow_id != follow.follow_id && a.follow_parent_id == follow.follow_parent_id && !a.is_recall
                                         select a).ToList();

                    if (child_samepar.Count() == 0)
                    {
                        if (follow.follow_parent_id != null)
                        {
                            var follow_par = db.doc_follows.Find(follow.follow_parent_id);
                            string res = Recursive_CompletedConfirm(uid, ip, tid, follow_par);
                        }
                        else
                        {
                            vb.doc_status_id = "hoanthanh";
                        }
                    }

                    db.SaveChanges();
                    return "OK";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Completed_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string stamp_item = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        DateTime send_date_now = DateTime.Now;

                        stamp_item = provider.FormData.GetValues("completed_item").SingleOrDefault();
                        doc_follows model = JsonConvert.DeserializeObject<doc_follows>(stamp_item);

                        var vb = db.doc_master.Find(model.doc_master_id);
                        string res = Recursive_CompletedConfirm(uid, ip, tid, model);
                        if (res != "OK")
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = res, err = "1" });
                        }

                        //----------------------
                        #region File upload
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        int numfileDoc = 1;
                        var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                        if (max_num.Count() > 0)
                        {
                            numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                        }
                        List<doc_files> listFileUp = new List<doc_files>();
                        List<string> listPathFileUp = new List<string>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            numfileDoc++;
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
                                fileName = System.IO.Path.GetFileName(fileName);
                            }
                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            }
                            // defined file doc
                            doc_files file_doc = new doc_files();
                            file_doc.doc_master_id = vb.doc_master_id;
                            file_doc.follow_id = model.follow_id;
                            file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                            file_doc.is_order = numfileDoc;
                            file_doc.is_drafted = false;
                            file_doc.doc_file_type = 3;
                            file_doc.message = "Thêm file xác nhận hoàn thành văn bản " + vb.compendium;
                            file_doc.created_by = uid;
                            file_doc.created_date = DateTime.Now;
                            file_doc.created_ip = ip;
                            file_doc.created_token_id = tid;
                            listFileUp.Add(file_doc);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }
                        if (listFileUp.Count > 0)
                        {
                            db.doc_files.AddRange(listFileUp);
                        }

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
                        #endregion

                        //---------------------
                        #region add doc_logs
                        if (helper.wlog)
                        {
                            doc_logs log = new doc_logs();
                            log.log_type = 17;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = "Xác nhận hoàn thành văn bản: " + vb.compendium;
                            log.doc_name = vb.compendium;
                            log.doc_master_id = vb.doc_master_id;
                            log.organization_id = user_now.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.doc_logs.Add(log);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Completed_Doc", ip, tid, "Lỗi khi xác nhận hoàn thành văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Completed_Doc", ip, tid, "Lỗi khi xác nhận hoàn thành văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Return_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string return_item = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        DateTime send_date_now = DateTime.Now;

                        return_item = provider.FormData.GetValues("return_item").SingleOrDefault();
                        doc_follows model = JsonConvert.DeserializeObject<doc_follows>(return_item);

                        var follow_cur = db.doc_follows.Find(model.follow_id);
                        follow_cur.is_return = true;


                        var vb = db.doc_master.Find(model.doc_master_id);

                        string FileVBFollow_ID = "";
                        int nguoitralai_pb_id;
                        int? Up_NhomduyetVanban_ID = null;
                        var new_fl = new doc_follows();
                        if (!String.IsNullOrEmpty(model.follow_id))
                        {
                            var cur_fl_par_id = follow_cur.follow_parent_id;
                            var fltl = db.doc_follows.FirstOrDefault(x => x.follow_id == cur_fl_par_id && !x.is_return && !x.is_recall);
                            while (cur_fl_par_id != null && fltl == null)
                            {
                                var fl_par = db.doc_follows.Find(cur_fl_par_id);
                                if (fl_par != null)
                                {
                                    cur_fl_par_id = fl_par.follow_parent_id;
                                    fltl = db.doc_follows.FirstOrDefault(x => x.follow_id == cur_fl_par_id && !x.is_return && !x.is_recall);
                                }
                            }
                            if (fltl == null)
                            {
                                fltl = new doc_follows();
                                fltl.receive_by = vb.created_by;
                                fltl.receive_type = 0;
                                fltl.receive_by_name = db.sys_users.FirstOrDefault(x => x.user_key == vb.created_by).full_name;
                                fltl.level = 1;
                                fltl.doc_status_id = vb.first_doc_status_id;
                            }
                            else
                            {
                                fltl.is_return = true;
                            }
                            new_fl.level = fltl.level;
                            new_fl.doc_status_id = fltl.doc_status_id;
                            bool is_inworkflow = false;
                            if (fltl != null) is_inworkflow = fltl.is_inworkflow ?? false;

                            var NhomQT = db.doc_workflow_groups.Find(vb.workflow_group_id);
                            if (NhomQT != null)
                            {
                                if (NhomQT.return_type == null)
                                {
                                    var Nguoitralai = fltl.receive_by;
                                    var LoaiduyetNhomtralai = db.doc_ca_role_groups.FirstOrDefault(x => x.role_group_id == fltl.receive_by)?.type_approval;
                                    var DuyettheoPB = db.doc_ca_role_groups.FirstOrDefault(x => x.role_group_id == fltl.receive_by)?.is_bydepartment;

                                    if (LoaiduyetNhomtralai == 0)
                                    {
                                        if (fltl.receive_type == 1)
                                        {
                                            new_fl.receive_by = fltl.receive_by;
                                            new_fl.receive_type = 1;
                                            new_fl.receive_by_name = db.doc_ca_role_groups.FirstOrDefault(x => x.role_group_id == new_fl.receive_by)?.role_group_name;
                                        }
                                    }
                                    else if (LoaiduyetNhomtralai == 1)
                                    {
                                        new_fl.receive_by = fltl.receive_by;
                                        new_fl.receive_last_group_user = fltl.receive_last_group_user;
                                        new_fl.receive_type = 1;
                                        new_fl.receive_by_name = db.sys_users.FirstOrDefault(x => x.user_key == new_fl.receive_last_group_user)?.full_name;
                                    }
                                    else if (LoaiduyetNhomtralai == null)
                                    {
                                        if (fltl.receive_type == 1)
                                        {
                                            new_fl.receive_by = fltl.receive_by;
                                            new_fl.receive_type = 1;
                                            new_fl.receive_by_name = db.doc_ca_role_groups.FirstOrDefault(x => x.role_group_id == new_fl.receive_by)?.role_group_name;
                                            Up_NhomduyetVanban_ID = fltl.receive_by ?? 0;
                                        }
                                        else if (fltl.receive_type == 0)
                                        {
                                            new_fl.receive_by = fltl.receive_by;
                                            new_fl.receive_type = 0;
                                            new_fl.receive_by_name = db.sys_users.FirstOrDefault(x => x.user_key == new_fl.receive_by)?.full_name;
                                            Up_NhomduyetVanban_ID = 0;
                                        }
                                    }
                                }
                                else if (NhomQT.return_type == 0)
                                {
                                    var fltl_2 = db.doc_follows.Where(x => x.doc_master_id == follow_cur.doc_master_id && x.level < follow_cur.level && !x.is_recall && !x.is_return && x.receive_type == 1 && x.receive_by == (NhomQT.return_group_id ?? 0)).OrderByDescending(x => x.level).FirstOrDefault();

                                    var Nguoitralai = fltl_2.receive_last_group_user;
                                    var Nhomtralai = NhomQT.return_group_id;
                                    Up_NhomduyetVanban_ID = Nhomtralai ?? 0;
                                    var LoaiduyetNhomtralai = db.doc_ca_role_groups.FirstOrDefault(x => x.role_group_id == NhomQT.return_group_id)?.type_approval;
                                    var DuyettheoPB = db.doc_ca_role_groups.FirstOrDefault(x => x.role_group_id == NhomQT.return_group_id)?.is_bydepartment;

                                    if (LoaiduyetNhomtralai == 0)
                                    {
                                        new_fl.receive_by = Nhomtralai;
                                        new_fl.receive_type = 1;
                                        new_fl.receive_by_name = db.doc_ca_role_groups.FirstOrDefault(x => x.role_group_id == new_fl.receive_by)?.role_group_name;
                                    }
                                    else if (LoaiduyetNhomtralai == 1)
                                    {
                                        new_fl.receive_by = Nhomtralai;
                                        new_fl.receive_last_group_user = Nguoitralai;
                                        new_fl.receive_type = 1;
                                        new_fl.receive_by_name = db.sys_users.FirstOrDefault(x => x.user_key == new_fl.receive_last_group_user)?.full_name;
                                    }
                                    else if (LoaiduyetNhomtralai == null)
                                    {
                                        new_fl.receive_by = Nhomtralai;
                                        new_fl.receive_last_group_user = Nguoitralai;
                                        new_fl.receive_type = 1;
                                        new_fl.receive_by_name = db.sys_users.FirstOrDefault(x => x.user_key == new_fl.receive_by)?.full_name;
                                        Up_NhomduyetVanban_ID = 0;
                                    }
                                }
                                else if (NhomQT.return_type == 1)
                                {
                                    var LoaiduyetNhomtralai = db.doc_ca_role_groups.FirstOrDefault(x => x.role_group_id == NhomQT.return_group_id)?.type_approval;
                                    var Nguoitralai = fltl.receive_last_group_user;

                                    if (fltl.receive_type == 1 && fltl.receive_last_group_user != 0)
                                    {
                                        new_fl.receive_by = fltl.receive_by;
                                        new_fl.receive_last_group_user = fltl.receive_last_group_user;
                                        new_fl.receive_type = 1;
                                        new_fl.receive_by_name = db.sys_users.FirstOrDefault(x => x.user_key == new_fl.receive_last_group_user)?.full_name;
                                    }
                                    else if (fltl.receive_type == 0)
                                    {
                                        new_fl.receive_by = fltl.receive_by;
                                        new_fl.receive_type = 0;
                                        new_fl.receive_by_name = db.sys_users.FirstOrDefault(x => x.user_key == new_fl.receive_by)?.full_name;
                                    }

                                    if (fltl.receive_type == 1)
                                    {
                                        Up_NhomduyetVanban_ID = fltl.receive_by ?? 0;
                                    }
                                    else if (fltl.receive_type == 0)
                                    {
                                        Up_NhomduyetVanban_ID = 0;
                                    }
                                }
                                else if (NhomQT.return_type == 2)
                                {
                                    var Nguoitralai = vb.created_by;
                                    new_fl.receive_by = Nguoitralai;
                                    new_fl.receive_type = 0;
                                    new_fl.receive_by_name = db.sys_users.FirstOrDefault(x => x.user_key == new_fl.receive_by)?.full_name;
                                    Up_NhomduyetVanban_ID = 0;
                                }
                            }
                            else
                            {
                                new_fl.receive_by = fltl.receive_by;
                                new_fl.receive_type = fltl.receive_type;
                                new_fl.receive_by_name = fltl.receive_by_name;
                                new_fl.receive_last_group_user = fltl.receive_last_group_user;
                                Up_NhomduyetVanban_ID = 0;
                            }
                        }

                        new_fl.follow_id = helper.GenKey();
                        new_fl.organization_id = model.organization_id;
                        new_fl.doc_master_id = model.doc_master_id;
                        new_fl.send_by = user_now.user_key;
                        new_fl.send_by_name = user_now.full_name;
                        new_fl.send_date = send_date_now;
                        new_fl.follow_parent_id = model.follow_id;
                        new_fl.is_inworkflow = model.is_inworkflow;
                        new_fl.doc_status_id = "tralai";
                        new_fl.message = model.message;
                        new_fl.created_by = uid;
                        new_fl.created_date = DateTime.Now;
                        new_fl.created_ip = ip;
                        new_fl.created_token_id = tid;

                        if (new_fl.receive_type == 1)
                        {
                            var nhom = db.doc_ca_role_groups.Find(new_fl.receive_by);
                            if (nhom != null)
                            {
                                if (nhom.is_bydepartment ?? false)
                                {
                                    var nguoitralai_follow = db.doc_follows.Where(x => x.doc_master_id == new_fl.doc_master_id && x.receive_type == 1 && x.receive_by == new_fl.receive_by && x.department_user_approval != null).OrderByDescending(x => x.level).FirstOrDefault();
                                    if (nguoitralai_follow != null)
                                    {
                                        new_fl.receive_by = nguoitralai_follow.department_user_approval;
                                    }
                                }
                            }
                        }

                        db.doc_follows.Add(new_fl);

                        //---------------------------------------

                        //-----------Master----------
                        if (vb != null)
                        {
                            vb.sent_by = user_now.user_key;
                            vb.follow_id = model.follow_id;
                            vb.handle_date = DateTime.Now;
                            vb.handle_by = new_fl.receive_by;
                            vb.doc_status_id = "tralai";
                            vb.message = model.message;
                            if (vb.is_inworkflow ?? false)
                            {
                                var minsttqt = db.doc_workflow_groups.Where(x => x.workflow_id == vb.workflow_id).Min(x => x.is_order);
                                var curNhomqt = db.doc_workflow_groups.FirstOrDefault(x => x.workflow_group_id == vb.workflow_group_id && x.workflow_id == vb.workflow_id);
                                if (curNhomqt != null)
                                {
                                    if (minsttqt == curNhomqt.is_order)
                                    {
                                        if (new_fl.receive_type == 0 || new_fl.receive_last_group_user != null)
                                        {
                                            int? user_key = null;
                                            string user_id = null;
                                            if (new_fl.receive_type == 0) user_key = new_fl.receive_by;
                                            else user_key = new_fl.receive_last_group_user;
                                            user_id = db.sys_users.FirstOrDefault(x => x.user_key == user_key).user_id;
                                            var findU = db.doc_ca_role_group_users.FirstOrDefault(x => x.role_group_id == curNhomqt.role_group_id && x.user_id == user_id);
                                            if (findU == null)
                                            {
                                                vb.workflow_group_id = null;
                                            }
                                        }
                                        else
                                        {
                                            vb.workflow_group_id = null;
                                        }
                                    }
                                    else
                                    {
                                        if (Up_NhomduyetVanban_ID != null)
                                        {
                                            var ndqttruoc = db.doc_workflow_groups.FirstOrDefault(x => x.role_group_id == Up_NhomduyetVanban_ID && x.workflow_id == vb.workflow_id);
                                            if (ndqttruoc != null)
                                            {
                                                vb.workflow_group_id = ndqttruoc.workflow_group_id;
                                            }
                                        }
                                        else
                                        {
                                            vb.workflow_group_id = null;
                                        }
                                    }
                                }

                                vb.workflow_last_person = user_now.user_key;
                            }
                            else
                            {

                            }
                            if (Up_NhomduyetVanban_ID != null)
                            {
                                vb.role_group_id = Up_NhomduyetVanban_ID;
                            }
                        }

                        //----------------------
                        #region File upload
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        int numfileDoc = 1;
                        var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                        if (max_num.Count() > 0)
                        {
                            numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                        }
                        List<doc_files> listFileUp = new List<doc_files>();
                        List<string> listPathFileUp = new List<string>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            numfileDoc++;
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
                                fileName = System.IO.Path.GetFileName(fileName);
                            }
                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            }
                            // defined file doc
                            doc_files file_doc = new doc_files();
                            file_doc.doc_master_id = vb.doc_master_id;
                            file_doc.follow_id = model.follow_id;
                            file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                            file_doc.is_order = numfileDoc;
                            file_doc.is_drafted = false;
                            file_doc.doc_file_type = 1;
                            file_doc.message = "Thêm file trả lại văn bản " + vb.compendium;
                            file_doc.created_by = uid;
                            file_doc.created_date = DateTime.Now;
                            file_doc.created_ip = ip;
                            file_doc.created_token_id = tid;
                            listFileUp.Add(file_doc);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }
                        if (listFileUp.Count > 0)
                        {
                            db.doc_files.AddRange(listFileUp);
                        }

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
                        #endregion

                        //---------------------
                        #region add doc_logs
                        if (helper.wlog)
                        {
                            doc_logs log = new doc_logs();
                            log.log_type = 4;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = "Trả lại văn bản: " + vb.compendium;
                            log.doc_name = vb.compendium;
                            log.doc_master_id = vb.doc_master_id;
                            log.organization_id = user_now.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.doc_logs.Add(log);
                        }
                        #endregion

                        //-----------------------
                        var ns_sh = (from nss in db.sys_users
                                     where nss.user_key == new_fl.receive_by && nss.status == 1
                                     select new { nss.user_id, nss.full_name, nss.avatar }).ToList();
                        if (ns_sh.Count() == 0)
                        {
                            if (new_fl.receive_last_group_user != null)
                            {
                                ns_sh = (from nss in db.sys_users
                                         where nss.user_key == new_fl.receive_last_group_user && nss.status == 1
                                         select new { nss.user_id, nss.full_name, nss.avatar }).ToList();
                            }
                            else
                            {
                                ns_sh = (from nss in db.sys_users
                                         join nu in db.doc_ca_role_group_users on nss.user_id equals nu.user_id
                                         where nu.role_group_id == new_fl.receive_by
                                         && nss.status == 1
                                         orderby nu.is_order
                                         select new { nss.user_id, nss.full_name, nss.avatar }).ToList();
                                if (ns_sh.Count() == 0)
                                {
                                    var pbid = db.sys_users.FirstOrDefault(x => x.user_key == new_fl.receive_by)?.department_id;
                                    ns_sh = (from nss in db.sys_users
                                             join nu in db.doc_ca_role_group_department on nss.user_id equals nu.user_id
                                             where nu.role_group_id == new_fl.receive_by && nu.department_id == pbid
                                     && nss.status == 1
                                             select new { nss.user_id, nss.full_name, nss.avatar }).ToList();
                                }
                            }
                        }

                        #region export xml message
                        if (provider.FormData.GetValues("is_exported_xml") != null)
                        {
                            string fd_is_exported_xml = provider.FormData.GetValues("is_exported_xml").SingleOrDefault();
                            bool is_exported_xml = bool.Parse(fd_is_exported_xml);
                            if (is_exported_xml)
                            {
                                var content_xml = new doc_follows();
                                content_xml.doc_master_id = vb.doc_master_id;
                                content_xml.send_by_name = user_now.full_name;
                                content_xml.receive_by_name = "";
                                foreach (var ns in ns_sh)
                                {
                                    if (content_xml.receive_by_name != "") content_xml.receive_by_name += ", ";
                                    content_xml.receive_by_name += ns.full_name;
                                }
                                content_xml.message = model.message;
                                content_xml.send_date = send_date_now;
                                var path_save = helper.path_xml + "/Doc";
                                if (!Directory.Exists(path_save))
                                    Directory.CreateDirectory(path_save);
                                string res = ExportMessage_XML(content_xml, path_save);
                                if (res != "OK")
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = res });
                                }
                            }
                        }
                        #endregion

                        #region send socket
                        string res_socket = SendSocketMultiple(ns_sh.Select(x => x.user_id), "Văn bản", "Trả lại: " + vb.compendium);
                        #endregion

                        #region sendhub

                        foreach (var ns in ns_sh)
                        {
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.module_key = const_module_key;
                            sh.user_send = uid;
                            sh.receiver = ns.user_id;
                            sh.icon = ns.avatar;
                            sh.title = "Văn bản";
                            sh.contents = "Trả lại: " + vb.compendium;
                            sh.type = 3;
                            sh.is_type = 0;
                            sh.date_send = DateTime.Now;
                            sh.id_key = model.doc_master_id.ToString();
                            sh.group_id = model.follow_id;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            db.sys_sendhub.Add(sh);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Return_Doc", ip, tid, "Lỗi khi trả lại văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Return_Doc", ip, tid, "Lỗi khi trả lại văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Recall_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string recall_item = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        DateTime send_date_now = DateTime.Now;

                        recall_item = provider.FormData.GetValues("recall_item").SingleOrDefault();
                        doc_follows model = JsonConvert.DeserializeObject<doc_follows>(recall_item);

                        var fd_follow_ids = provider.FormData.GetValues("recall_follow_ids").SingleOrDefault();
                        List<string> follow_ids = JsonConvert.DeserializeObject<List<string>>(fd_follow_ids);

                        DateTime recall_date = DateTime.Now;
                        foreach (var follow_id in follow_ids)
                        {
                            var fl = db.doc_follows.Find(follow_id);
                            fl.is_recall = true;
                            fl.recall_date = recall_date;
                            fl.recall_message = model.recall_message;
                        }
                        // Xu ly khi thu hoi het
                        var first_fl = db.doc_follows.Find(follow_ids[0]);
                        var vb = db.doc_master.Find(first_fl.doc_master_id);
                        string follow_parent_id = first_fl.follow_parent_id;
                        var count_all_child = db.doc_follows.Count(x => x.follow_parent_id == follow_parent_id && x.doc_master_id == first_fl.doc_master_id);
                        var count_recall_child = db.doc_follows.Count(x => x.follow_parent_id == follow_parent_id && x.doc_master_id == first_fl.doc_master_id && x.is_recall);
                        if (follow_ids.Count() + count_recall_child == count_all_child)
                        {
                            if (vb != null)
                            {
                                if (follow_parent_id == null)
                                {
                                    vb.sent_by = vb.created_by;
                                    vb.follow_id = null;
                                    vb.doc_status_id = vb.first_doc_status_id;
                                    vb.message = model.recall_message;
                                }
                                else
                                {
                                    var parent = db.doc_follows.Find(follow_parent_id);
                                    vb.sent_by = parent.send_by;
                                    vb.follow_id = parent.follow_parent_id;
                                    vb.doc_status_id = parent.doc_status_id;
                                    vb.message = model.recall_message;
                                }
                            }
                        }

                        //---------------------
                        #region add doc_logs
                        if (helper.wlog)
                        {
                            doc_logs log = new doc_logs();
                            log.log_type = 9;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = "Thu hồi văn bản: " + vb.compendium;
                            log.doc_name = vb.compendium;
                            log.doc_master_id = vb.doc_master_id;
                            log.organization_id = user_now.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.doc_logs.Add(log);
                        }
                        #endregion

                        //-----------------------

                        List<doc_follows> fls = db.doc_follows.Where(x => follow_ids.Contains(x.follow_id)).ToList();
                        var us_keys = new List<doc_follows>();
                        foreach (var fl in fls)
                        {
                            var re_by = fl.receive_by ?? 0;
                            if (fl.receive_type == 0)
                            {
                                us_keys.Add(new doc_follows() { receive_by = re_by, follow_id = fl.follow_id });
                            }
                            else if (fl.receive_type == 1)
                            {
                                List<doc_ca_role_group_users> gr_us_ids = db.doc_ca_role_group_users.Where(x => x.role_group_id == re_by).ToList();
                                foreach (var us_id in gr_us_ids)
                                {
                                    var us = db.sys_users.Find(us_id.user_id);
                                    if (us != null)
                                    {
                                        us_keys.Add(new doc_follows() { receive_by = us.user_key, follow_id = fl.follow_id });
                                    }
                                }
                            }
                            else if (fl.receive_type == 2)
                            {
                                List<sys_users> org_us = db.sys_users.Where(x => x.organization_id == re_by).ToList();
                                foreach (var us in org_us)
                                {
                                    us_keys.Add(new doc_follows() { receive_by = us.user_key, follow_id = fl.follow_id });
                                }
                            }
                            else if (fl.receive_type == 3)
                            {
                                us_keys.Add(new doc_follows() { receive_by = fl.receive_last_group_user, follow_id = fl.follow_id });
                            }
                        }
                        var us_key_ids = us_keys.Select(x => x.receive_by).Distinct().ToList();
                        var ns_sh = (from nss in db.sys_users
                                     where us_key_ids.Contains(nss.user_key) && nss.status == 1
                                     select new { nss.user_id, nss.full_name, nss.avatar, nss.user_key }).ToList();

                        #region export xml message
                        if (provider.FormData.GetValues("is_exported_xml") != null)
                        {
                            string fd_is_exported_xml = provider.FormData.GetValues("is_exported_xml").SingleOrDefault();
                            bool is_exported_xml = bool.Parse(fd_is_exported_xml);
                            if (is_exported_xml)
                            {
                                var content_xml = new doc_follows();
                                content_xml.doc_master_id = vb.doc_master_id;
                                content_xml.send_by_name = user_now.full_name;
                                content_xml.receive_by_name = "";
                                foreach (var ns in ns_sh)
                                {
                                    if (content_xml.receive_by_name != "") content_xml.receive_by_name += ", ";
                                    content_xml.receive_by_name += ns.full_name;
                                }
                                content_xml.message = model.message;
                                content_xml.send_date = send_date_now;
                                var path_save = helper.path_xml + "/Doc";
                                if (!Directory.Exists(path_save))
                                    Directory.CreateDirectory(path_save);
                                string res = ExportMessage_XML(content_xml, path_save);
                                if (res != "OK")
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = res });
                                }
                            }
                        }
                        #endregion

                        #region send socket
                        string res_socket = SendSocketMultiple(ns_sh.Select(x => x.user_id), "Văn bản", "Thu hồi: " + vb.compendium);
                        #endregion

                        #region sendhub

                        foreach (var ns in ns_sh)
                        {
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.module_key = const_module_key;
                            sh.user_send = uid;
                            sh.receiver = ns.user_id;
                            sh.icon = ns.avatar;
                            sh.title = "Văn bản";
                            sh.contents = "Thu hồi: " + vb.compendium;
                            sh.type = 3;
                            sh.is_type = 0;
                            sh.date_send = DateTime.Now;
                            sh.id_key = model.doc_master_id.ToString();
                            sh.group_id = us_keys.FirstOrDefault(x => x.receive_by == ns.user_key)?.follow_id;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            db.sys_sendhub.Add(sh);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Recall_Doc", ip, tid, "Lỗi khi thu hồi văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Recall_Doc", ip, tid, "Lỗi khi thu hồi văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Approval_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string approval_item = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        DateTime send_date_now = DateTime.Now;

                        approval_item = provider.FormData.GetValues("approval_item").SingleOrDefault();
                        doc_follows model = JsonConvert.DeserializeObject<doc_follows>(approval_item);

                        List<doc_sign_approval> typesigns = new List<doc_sign_approval>();
                        if (provider.FormData.GetValues("type_signs") != null)
                        {
                            string fd_typesigns = provider.FormData.GetValues("type_signs").SingleOrDefault();
                            typesigns = JsonConvert.DeserializeObject<List<doc_sign_approval>>(fd_typesigns);
                        }

                        // ky dien tu
                        List<Dictionary<string, List<WaterImage>>> WaterImages = new List<Dictionary<string, List<WaterImage>>>();
                        if (provider.FormData.GetValues("waterimages") != null)
                        {
                            string wimg = provider.FormData.GetValues("waterimages").SingleOrDefault();
                            WaterImages = JsonConvert.DeserializeObject<List<Dictionary<string, List<WaterImage>>>>(wimg);
                        }
                        List<Dictionary<string, List<WaterTxt>>> WaterTxts = new List<Dictionary<string, List<WaterTxt>>>();
                        if (provider.FormData.GetValues("watertxts") != null)
                        {
                            string wtxt = provider.FormData.GetValues("watertxts").SingleOrDefault();
                            WaterTxts = JsonConvert.DeserializeObject<List<Dictionary<string, List<WaterTxt>>>>(wtxt);
                        }

                        var us = db.sys_users.FirstOrDefault(x => x.user_key == model.receive_by);
                        bool first_approval = true;
                        string title = model.follow_id == null ? "Trình phê duyệt" : "Duyệt chuyển tiếp";

                        var vb = db.doc_master.Find(model.doc_master_id);

                        var new_fl = new doc_follows();
                        new_fl.follow_id = helper.GenKey();
                        new_fl.organization_id = model.organization_id;
                        new_fl.doc_master_id = model.doc_master_id;
                        new_fl.deadline_date = model.deadline_date;
                        new_fl.send_by = user_now.user_key;
                        new_fl.is_prioritized = model.is_prioritized;
                        new_fl.send_by_name = user_now.full_name;
                        new_fl.send_date = send_date_now;
                        new_fl.receive_by = us.user_key;

                        new_fl.receive_by_name = us.full_name;
                        new_fl.receive_type = 0;
                        new_fl.follow_parent_id = model.follow_id;
                        new_fl.level = 1;

                        if (new_fl.follow_parent_id != null)
                        {
                            var par = db.doc_follows.Find(new_fl.follow_parent_id);
                            if (par != null)
                            {
                                new_fl.level = par.level + 1;
                            }
                            new_fl.parent_doc_status_id = par.doc_status_id;
                        }
                        new_fl.doc_status_id = "chopheduyet";
                        new_fl.message = model.message;
                        new_fl.created_by = uid;
                        new_fl.created_date = DateTime.Now;
                        new_fl.created_ip = ip;
                        new_fl.created_token_id = tid;
                        db.doc_follows.Add(new_fl);

                        //---------------------------------------

                        // ---------- Ky so ---------------
                        if (model.follow_id == null)
                        {
                            // ký phê duyệt
                            if (vb != null)
                            {
                                if ((vb.file_path.GetFileExtension().Trim().ToLower() == "doc" || vb.file_path.GetFileExtension().Trim().ToLower() == "docx"))
                                {
                                    string res = "";
                                    if (WaterImages.Count() > 0)
                                    {
                                        foreach (var lst_wi in WaterImages)
                                        {
                                            foreach (KeyValuePair<string, List<WaterImage>> entry in lst_wi)
                                            {
                                                if (entry.Key == vb.file_path)
                                                {
                                                    var total = db.doc_files.AsNoTracking().Where(x => x.doc_master_id == vb.doc_master_id && x.is_drafted == false && x.doc_file_type == 0).ToList();
                                                    var stt = 0;
                                                    if (total.Count() > 0)
                                                    {
                                                        stt = total.Max(x => x.is_order) ?? 0;
                                                    }
                                                    var backup = new doc_files();
                                                    backup.doc_master_id = vb.doc_master_id;
                                                    backup.file_name = System.IO.Path.GetFileNameWithoutExtension(vb.file_name) + " (doc)" + System.IO.Path.GetExtension(vb.file_name);
                                                    backup.file_path = vb.file_path;
                                                    backup.file_size = vb.file_size ?? 0;
                                                    backup.file_type = vb.file_type;
                                                    backup.doc_file_type = 4;
                                                    backup.is_order = stt + 1; ;
                                                    backup.created_by = uid;
                                                    backup.created_date = DateTime.Now;
                                                    backup.created_ip = ip;
                                                    backup.created_token_id = tid;
                                                    db.doc_files.Add(backup);
                                                }

                                                res = SignDoc(root, entry.Key, entry.Value);
                                                if (res != "OK")
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = res, err = "1" });
                                                }
                                                else
                                                {
                                                    var typeky = typesigns.FirstOrDefault(x => x.file_path == entry.Key);
                                                    if (typeky != null)
                                                    {
                                                        if (typeky.file_id == null)
                                                        {
                                                            new_fl.flash_sign = typeky.flash_sign;
                                                        }
                                                        else
                                                        {
                                                            typeky.follow_id = new_fl.follow_id;
                                                            typeky.doc_master_id = vb.doc_master_id;
                                                            typeky.created_by = uid;
                                                            typeky.created_date = DateTime.Now;
                                                            typeky.created_ip = ip;
                                                            typeky.created_token_id = tid;
                                                            db.doc_sign_approval.Add(typeky);
                                                        }
                                                    }
                                                    if (entry.Key == vb.file_path)
                                                    {
                                                        // Save State Đã ký
                                                        new_fl.is_signed = true;
                                                        var new_FileName_pdf = System.IO.Path.GetFileNameWithoutExtension(vb.file_name) + ".pdf";
                                                        var new_duongdan = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(vb.file_path), System.IO.Path.GetFileNameWithoutExtension(vb.file_path)) + ".pdf";
                                                        var new_pdfpath = root + new_duongdan;

                                                        vb.file_path = new_duongdan;
                                                        vb.file_name = new_FileName_pdf;
                                                        vb.file_size = new System.IO.FileInfo(new_pdfpath).Length;
                                                        vb.file_type = "pdf";
                                                    }
                                                    else
                                                    {
                                                        var tl = db.doc_files.FirstOrDefault(x => x.doc_master_id == vb.doc_master_id && x.file_path == entry.Key);
                                                        if (tl != null && (tl.file_path.GetFileExtension().Trim().ToLower() == "doc" || tl.file_path.GetFileExtension().Trim().ToLower() == "docx"))
                                                        {
                                                            var new_FileName_pdf = System.IO.Path.GetFileNameWithoutExtension(tl.file_name) + ".pdf";
                                                            var new_duongdan = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(tl.file_path), System.IO.Path.GetFileNameWithoutExtension(tl.file_path)) + ".pdf";
                                                            var new_pdfpath = root + new_duongdan;

                                                            tl.file_path = new_duongdan;
                                                            tl.file_name = new_FileName_pdf;
                                                            tl.file_size = new System.IO.FileInfo(new_pdfpath).Length;
                                                            tl.file_type = "pdf";
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (vb != null)
                            {
                                if (vb.file_path.GetFileExtension().Trim().ToLower() == "pdf" || vb.file_path.GetFileExtension().Trim().ToLower() == "doc" || vb.file_path.GetFileExtension().Trim().ToLower() == "docx")
                                {
                                    if (WaterImages.Count() > 0)
                                    {
                                        string res = "";
                                        foreach (var lst_wi in WaterImages)
                                        {
                                            foreach (KeyValuePair<string, List<WaterImage>> entry in lst_wi)
                                            {
                                                res = SignDoc(root, entry.Key, entry.Value);
                                                if (res != "OK")
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = res, err = "1" });
                                                }
                                                else
                                                {
                                                    var typeky = typesigns.FirstOrDefault(x => x.file_path == entry.Key);
                                                    if (typeky != null)
                                                    {
                                                        if (typeky.file_id == null)
                                                        {
                                                            model.flash_sign = typeky.flash_sign;
                                                        }
                                                        else
                                                        {
                                                            typeky.follow_id = new_fl.follow_id;
                                                            typeky.doc_master_id = vb.doc_master_id;
                                                            typeky.created_by = uid;
                                                            typeky.created_date = DateTime.Now;
                                                            typeky.created_ip = ip;
                                                            typeky.created_token_id = tid;
                                                            db.doc_sign_approval.Add(typeky);
                                                        }
                                                    }
                                                    if (entry.Key == vb.file_path && (vb.file_path.GetFileExtension().Trim().ToLower() == "doc" || vb.file_path.GetFileExtension().Trim().ToLower() == "docx"))
                                                    {
                                                        var new_FileName_pdf = System.IO.Path.GetFileNameWithoutExtension(vb.file_name) + ".pdf";
                                                        var new_duongdan = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(vb.file_path), System.IO.Path.GetFileNameWithoutExtension(vb.file_path)) + ".pdf";
                                                        var new_pdfpath = root + new_duongdan;

                                                        vb.file_path = new_duongdan;
                                                        vb.file_name = new_FileName_pdf;
                                                        vb.file_size = new System.IO.FileInfo(new_pdfpath).Length;
                                                        vb.file_type = "pdf";
                                                    }
                                                    var tl = db.doc_files.FirstOrDefault(x => x.doc_master_id == vb.doc_master_id && x.file_path == entry.Key);
                                                    if (tl != null && (tl.file_path.GetFileExtension().Trim().ToLower() == "doc" || tl.file_path.GetFileExtension().Trim().ToLower() == "docx"))
                                                    {
                                                        var new_FileName_pdf = System.IO.Path.GetFileNameWithoutExtension(tl.file_name) + ".pdf";
                                                        var new_duongdan = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(tl.file_path), System.IO.Path.GetFileNameWithoutExtension(tl.file_path)) + ".pdf";
                                                        var new_pdfpath = root + new_duongdan;

                                                        tl.file_path = new_duongdan;
                                                        tl.file_name = new_FileName_pdf;
                                                        tl.file_size = new System.IO.FileInfo(new_pdfpath).Length;
                                                        tl.file_type = "pdf";
                                                    }
                                                }
                                            }
                                        }
                                        if (res == "OK")
                                        {
                                            // Save State Đã ký
                                            new_fl.is_signed = true;
                                        }
                                    }
                                }
                            }
                        }

                        //------------- Ky CA --------------
                        List<doc_sign_approval> ca_files = new List<doc_sign_approval>();
                        if (provider.FormData.GetValues("ca_files") != null)
                        {
                            string fd_ca_files = provider.FormData.GetValues("ca_files").SingleOrDefault();
                            ca_files = JsonConvert.DeserializeObject<List<doc_sign_approval>>(fd_ca_files);
                        }

                        foreach(var file_ca in ca_files)
                        {
                            var dir_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/";
                            if (file_ca.doc_master_id != null)
                            {
                                if (File.Exists(root + vb.file_path))
                                {
                                    File.Delete(root + vb.file_path);
                                }
                                var new_FileName = System.IO.Path.GetFileName(file_ca.file_path);
                                var new_path = root + dir_path + new_FileName;
                                File.Move(root + file_ca.file_path, new_path);
                                vb.file_name = new_FileName;
                                vb.file_path = dir_path + new_FileName;
                                vb.file_size = new System.IO.FileInfo(root + vb.file_path).Length;
                            }
                            else if(file_ca.file_id != null)
                            {
                                var file_vb = db.doc_files.Find(file_ca.file_id);
                                if (File.Exists(root + file_vb.file_path))
                                {
                                    File.Delete(root + file_vb.file_path);
                                }
                                var new_FileName = System.IO.Path.GetFileName(file_ca.file_path);
                                var new_path = root + dir_path + new_FileName;
                                File.Move(root + file_ca.file_path, new_path);
                                file_vb.file_name = new_FileName;
                                file_vb.file_path = dir_path + new_FileName;
                                file_vb.file_size = new System.IO.FileInfo(root + file_vb.file_path).Length;
                            }
                        }


                        //-----------Master----------
                        if (vb != null)
                        {
                            vb.sent_by = user_now.user_key;
                            vb.follow_id = model.follow_id;
                            vb.handle_date = DateTime.Now;
                            vb.handle_by = model.receive_by;
                            vb.doc_status_id = "chopheduyet";
                            vb.message = model.message;
                            vb.is_approved = true;
                        }

                        //----------------------
                        #region File upload
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        int numfileDoc = 1;
                        var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                        if (max_num.Count() > 0)
                        {
                            numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                        }
                        List<doc_files> listFileUp = new List<doc_files>();
                        List<string> listPathFileUp = new List<string>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            numfileDoc++;
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
                                fileName = System.IO.Path.GetFileName(fileName);
                            }
                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            }
                            // defined file doc
                            doc_files file_doc = new doc_files();
                            file_doc.doc_master_id = vb.doc_master_id;
                            file_doc.follow_id = model.follow_id;
                            file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                            file_doc.is_order = numfileDoc;
                            file_doc.is_drafted = false;
                            file_doc.doc_file_type = 1;
                            file_doc.message = "Thêm file " + title + " văn bản " + vb.compendium;
                            file_doc.created_by = uid;
                            file_doc.created_date = DateTime.Now;
                            file_doc.created_ip = ip;
                            file_doc.created_token_id = tid;
                            listFileUp.Add(file_doc);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }
                        if (listFileUp.Count > 0)
                        {
                            db.doc_files.AddRange(listFileUp);
                        }

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
                        #endregion

                        //---------------------
                        #region add doc_logs
                        if (helper.wlog)
                        {
                            doc_logs log = new doc_logs();
                            log.log_type = 15;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = title + " văn bản: " + vb.compendium;
                            log.doc_name = vb.compendium;
                            log.doc_master_id = vb.doc_master_id;
                            log.organization_id = user_now.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.doc_logs.Add(log);
                        }
                        #endregion

                        //-----------------------

                        var ns_sh = (from nss in db.sys_users
                                     where nss.user_key == model.receive_by && nss.status == 1
                                     select new { nss.user_id, nss.full_name, nss.avatar }).ToList();

                        #region export xml message
                        if (provider.FormData.GetValues("is_exported_xml") != null)
                        {
                            string fd_is_exported_xml = provider.FormData.GetValues("is_exported_xml").SingleOrDefault();
                            bool is_exported_xml = bool.Parse(fd_is_exported_xml);
                            if (is_exported_xml)
                            {
                                var content_xml = new doc_follows();
                                content_xml.doc_master_id = vb.doc_master_id;
                                content_xml.send_by_name = user_now.full_name;
                                content_xml.receive_by_name = "";
                                foreach (var ns in ns_sh)
                                {
                                    if (content_xml.receive_by_name != "") content_xml.receive_by_name += ", ";
                                    content_xml.receive_by_name += ns.full_name;
                                }
                                content_xml.message = model.message;
                                content_xml.send_date = send_date_now;
                                var path_save = helper.path_xml + "/Doc";
                                if (!Directory.Exists(path_save))
                                    Directory.CreateDirectory(path_save);
                                string res = ExportMessage_XML(content_xml, path_save);
                                if (res != "OK")
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = res });
                                }
                            }
                        }
                        #endregion

                        #region send socket
                        string res_socket = SendSocketMultiple(ns_sh.Select(x => x.user_id), "Văn bản", title + ": " + vb.compendium);
                        #endregion

                        #region sendhub
                        foreach (var ns in ns_sh)
                        {
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.module_key = const_module_key;
                            sh.user_send = uid;
                            sh.receiver = ns.user_id;
                            sh.icon = ns.avatar;
                            sh.title = "Văn bản";
                            sh.contents = title + ": " + vb.compendium;
                            sh.type = 3;
                            sh.is_type = 0;
                            sh.date_send = DateTime.Now;
                            sh.id_key = model.doc_master_id.ToString();
                            sh.group_id = model.follow_id;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            db.sys_sendhub.Add(sh);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Approval_Doc", ip, tid, "Lỗi khi chuyển phê duyệt văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Approval_Doc", ip, tid, "Lỗi khi chuyển phê duyệt văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> PublishingApproval_Doc()
        {
            var identity = User.Identity as ClaimsIdentity;
            string approval_item = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        DateTime send_date_now = DateTime.Now;

                        approval_item = provider.FormData.GetValues("approval_item").SingleOrDefault();
                        doc_follows model = JsonConvert.DeserializeObject<doc_follows>(approval_item);

                        // ky dien tu
                        List<Dictionary<string, List<WaterImage>>> WaterImages = new List<Dictionary<string, List<WaterImage>>>();
                        if (provider.FormData.GetValues("waterimages") != null)
                        {
                            string wimg = provider.FormData.GetValues("waterimages").SingleOrDefault();
                            WaterImages = JsonConvert.DeserializeObject<List<Dictionary<string, List<WaterImage>>>>(wimg);
                        }
                        List<Dictionary<string, List<WaterTxt>>> WaterTxts = new List<Dictionary<string, List<WaterTxt>>>();
                        if (provider.FormData.GetValues("watertxts") != null)
                        {
                            string wtxt = provider.FormData.GetValues("watertxts").SingleOrDefault();
                            WaterTxts = JsonConvert.DeserializeObject<List<Dictionary<string, List<WaterTxt>>>>(wtxt);
                        }

                        var us = db.sys_users.FirstOrDefault(x => x.user_key == model.receive_by);

                        var vb = db.doc_master.Find(model.doc_master_id);

                        var new_fl = new doc_follows();
                        new_fl.follow_id = helper.GenKey();
                        new_fl.organization_id = model.organization_id;
                        new_fl.doc_master_id = model.doc_master_id;
                        new_fl.deadline_date = model.deadline_date;
                        new_fl.send_by = user_now.user_key;
                        new_fl.is_prioritized = model.is_prioritized;
                        new_fl.esim_sign = model.esim_sign;
                        new_fl.send_by_name = user_now.full_name;
                        new_fl.send_date = send_date_now;
                        new_fl.receive_by = us.user_key;

                        new_fl.receive_by_name = us.full_name;
                        new_fl.receive_type = 0;
                        new_fl.follow_parent_id = model.follow_id;
                        new_fl.level = 1;

                        if (new_fl.follow_parent_id != null)
                        {
                            var par = db.doc_follows.Find(new_fl.follow_parent_id);
                            if (par != null)
                            {
                                new_fl.level = par.level + 1;
                            }
                            new_fl.parent_doc_status_id = par.doc_status_id;
                        }
                        new_fl.doc_status_id = "chodongdau";
                        new_fl.message = model.message;
                        new_fl.created_by = uid;
                        new_fl.created_date = DateTime.Now;
                        new_fl.created_ip = ip;
                        new_fl.created_token_id = tid;
                        db.doc_follows.Add(new_fl);

                        //---------------------------------------

                        // ---------- Ky so ---------------
                        if (vb != null)
                        {
                            if (vb.file_path.GetFileExtension().Trim().ToLower() == "pdf")
                            {
                                string res = "";
                                foreach (var lst_wi in WaterImages)
                                {
                                    foreach (KeyValuePair<string, List<WaterImage>> entry in lst_wi)
                                    {
                                        res = SignDoc(root, entry.Key, entry.Value);
                                        if (res != "OK")
                                        {
                                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = res, err = "1" });
                                        }
                                        else
                                        {
                                            var tl = db.doc_files.FirstOrDefault(x => x.doc_master_id == vb.doc_master_id && x.file_path == entry.Key);
                                            if (tl != null && (tl.file_path.GetFileExtension().Trim().ToLower() == "doc" || tl.file_path.GetFileExtension().Trim().ToLower() == "docx"))
                                            {
                                                var new_FileName_pdf = System.IO.Path.GetFileNameWithoutExtension(tl.file_name) + ".pdf";
                                                var new_duongdan = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(tl.file_path), System.IO.Path.GetFileNameWithoutExtension(tl.file_path)) + ".pdf";
                                                var new_pdfpath = root + new_duongdan;

                                                tl.file_path = new_duongdan;
                                                tl.file_name = new_FileName_pdf;
                                                tl.file_size = new System.IO.FileInfo(new_pdfpath).Length;
                                                tl.file_type = "pdf";
                                            }
                                        }
                                    }
                                }
                                if (res == "OK" || new_fl.is_signed == true)
                                {
                                    // Save State Đã ký
                                    new_fl.is_signed = true;
                                    // Ky dien tu CA
                                    if (model.esim_sign ?? false)
                                    {
                                        var phoneNumberUs = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                                        if (phoneNumberUs == null || String.IsNullOrEmpty(phoneNumberUs.ca_number))
                                        {
                                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Không tìm thấy số đăng ký!", err = "1" });
                                        }
                                        var strPath = root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/";
                                        var fd_esimsign = provider.FormData.GetValues("esim_sign").SingleOrDefault();
                                        EsimSign esimsign = JsonConvert.DeserializeObject<EsimSign>(fd_esimsign);
                                        ImpMobileCA signca = new ImpMobileCA();
                                        string res_simca = signca.SignMobileCA(
                                                            strPath,
                                                            System.IO.Path.GetFileName(vb.file_path),
                                                            phoneNumberUs.ca_number,
                                                            !String.IsNullOrEmpty(esimsign.reason) ? esimsign.reason : "",
                                                            !String.IsNullOrEmpty(esimsign.place) ? esimsign.place : "",
                                                            esimsign.show_signby
                                                            );
                                        if (res_simca == "OK")
                                        {
                                            var new_FileName = System.IO.Path.GetFileNameWithoutExtension(vb.file_path) + "_casigned" + System.IO.Path.GetExtension(vb.file_path);
                                            var new_path = strPath + "/" + new_FileName;
                                            var duongdan = root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/";
                                            vb.file_path = duongdan + new_FileName;
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = res_simca, err = "1" });
                                        }
                                    }
                                }
                            }
                        }

                        //------------- Ky CA (vgca) --------------
                        List<doc_sign_approval> ca_files = new List<doc_sign_approval>();
                        if (provider.FormData.GetValues("ca_files") != null)
                        {
                            string fd_ca_files = provider.FormData.GetValues("ca_files").SingleOrDefault();
                            ca_files = JsonConvert.DeserializeObject<List<doc_sign_approval>>(fd_ca_files);
                        }

                        foreach (var file_ca in ca_files)
                        {
                            var dir_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/";
                            if (file_ca.doc_master_id != null)
                            {
                                if (File.Exists(root + vb.file_path))
                                {
                                    File.Delete(root + vb.file_path);
                                }
                                var new_FileName = System.IO.Path.GetFileName(file_ca.file_path);
                                var new_path = root + dir_path + new_FileName;
                                File.Move(root + file_ca.file_path, new_path);
                                vb.file_name = new_FileName;
                                vb.file_path = dir_path + new_FileName;
                                vb.file_size = new System.IO.FileInfo(root + vb.file_path).Length;
                            }
                            else if (file_ca.file_id != null)
                            {
                                var file_vb = db.doc_files.Find(file_ca.file_id);
                                if (File.Exists(root + file_vb.file_path))
                                {
                                    File.Delete(root + file_vb.file_path);
                                }
                                var new_FileName = System.IO.Path.GetFileName(file_ca.file_path);
                                var new_path = root + dir_path + new_FileName;
                                File.Move(root + file_ca.file_path, new_path);
                                file_vb.file_name = new_FileName;
                                file_vb.file_path = dir_path + new_FileName;
                                file_vb.file_size = new System.IO.FileInfo(root + file_vb.file_path).Length;
                            }
                        }

                        //-----------Master----------
                        if (vb != null)
                        {
                            vb.sent_by = user_now.user_key;
                            vb.follow_id = model.follow_id;
                            vb.handle_date = DateTime.Now;
                            vb.handle_by = model.receive_by;
                            vb.doc_status_id = "chodongdau";
                            vb.message = model.message;
                        }

                        //----------------------
                        #region File upload
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        int numfileDoc = 1;
                        var max_num = db.doc_files.Where(x => x.doc_master_id == vb.doc_master_id).ToList();
                        if (max_num.Count() > 0)
                        {
                            numfileDoc = (int)max_num.Max(x => x.is_order) + 1;
                        }
                        List<doc_files> listFileUp = new List<doc_files>();
                        List<string> listPathFileUp = new List<string>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            numfileDoc++;
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
                                fileName = System.IO.Path.GetFileName(fileName);
                            }
                            newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + (new Random().Next(0, 10000)) + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = System.IO.Path.Combine(root + "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid, fileName);
                            }
                            // defined file doc
                            doc_files file_doc = new doc_files();
                            file_doc.doc_master_id = vb.doc_master_id;
                            file_doc.follow_id = model.follow_id;
                            file_doc.file_path = "/Portals/" + organization_id_user + "/Doc/" + vb.doc_master_guid + "/" + fileName;
                            file_doc.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            //file_doc.file_size = fileData.Headers.ContentLength != null ? fileData.Headers.ContentLength : fileData.Headers.ContentDisposition.Size;
                            file_doc.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_doc.file_type = file_doc.file_name.Substring(file_doc.file_name.LastIndexOf(".") + 1).ToLower();
                            file_doc.is_order = numfileDoc;
                            file_doc.is_drafted = false;
                            file_doc.doc_file_type = 1;
                            file_doc.message = "Thêm file duyệt phát hành văn bản " + vb.compendium;
                            file_doc.created_by = uid;
                            file_doc.created_date = DateTime.Now;
                            file_doc.created_ip = ip;
                            file_doc.created_token_id = tid;
                            listFileUp.Add(file_doc);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }
                        if (listFileUp.Count > 0)
                        {
                            db.doc_files.AddRange(listFileUp);
                        }

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
                        #endregion

                        //---------------------
                        #region add doc_logs
                        if (helper.wlog)
                        {
                            doc_logs log = new doc_logs();
                            log.log_type = 3;
                            //log.message = JsonConvert.SerializeObject(new { data = law_main });
                            log.message = "Duyệt phát hành văn bản: " + vb.compendium;
                            log.doc_name = vb.compendium;
                            log.doc_master_id = vb.doc_master_id;
                            log.organization_id = user_now.organization_id;
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            log.is_view = false;
                            db.doc_logs.Add(log);
                        }
                        #endregion

                        //-----------------------
                        var ns_sh = (from nss in db.sys_users
                                     where nss.user_key == model.receive_by && nss.status == 1
                                     select new { nss.user_id, nss.full_name, nss.avatar }).ToList();

                        #region export xml message
                        if (provider.FormData.GetValues("is_exported_xml") != null)
                        {
                            string fd_is_exported_xml = provider.FormData.GetValues("is_exported_xml").SingleOrDefault();
                            bool is_exported_xml = bool.Parse(fd_is_exported_xml);
                            if (is_exported_xml)
                            {
                                var content_xml = new doc_follows();
                                content_xml.doc_master_id = vb.doc_master_id;
                                content_xml.send_by_name = user_now.full_name;
                                content_xml.receive_by_name = "";
                                foreach (var ns in ns_sh)
                                {
                                    if (content_xml.receive_by_name != "") content_xml.receive_by_name += ", ";
                                    content_xml.receive_by_name += ns.full_name;
                                }
                                content_xml.message = model.message;
                                content_xml.send_date = send_date_now;
                                var path_save = helper.path_xml + "/Doc";
                                if (!Directory.Exists(path_save))
                                    Directory.CreateDirectory(path_save);
                                string res = ExportMessage_XML(content_xml, path_save);
                                if (res != "OK")
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = res });
                                }
                            }
                        }
                        #endregion

                        #region send socket
                        string res_socket = SendSocketMultiple(ns_sh.Select(x => x.user_id), "Văn bản", "Duyệt phát hành: " + vb.compendium);
                        #endregion

                        #region sendhub

                        foreach (var ns in ns_sh)
                        {
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.module_key = const_module_key;
                            sh.user_send = uid;
                            sh.receiver = ns.user_id;
                            sh.icon = ns.avatar;
                            sh.title = "Văn bản";
                            sh.contents = "Duyệt phát hành: " + vb.compendium;
                            sh.type = 3;
                            sh.is_type = 0;
                            sh.date_send = DateTime.Now;
                            sh.id_key = model.doc_master_id.ToString();
                            sh.group_id = model.follow_id;
                            sh.token_id = tid;
                            sh.created_date = DateTime.Now;
                            sh.created_by = uid;
                            sh.created_token_id = tid;
                            sh.created_ip = ip;
                            db.sys_sendhub.Add(sh);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/PublishingApproval_Doc", ip, tid, "Lỗi khi duyệt phát hành văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/PublishingApproval_Doc", ip, tid, "Lỗi khi duyệt phát hành văn bản", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        #region Convert to PDF
        public async Task<HttpResponseMessage> ConvertToPDF(doc_sign_approval file)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
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

                        string filePath = root + file.file_path;
                        var documentConverter = new DocumentConverter(filePath);
                        PdfOutputOptions pdf = new PdfOutputOptions();
                        pdf.FastWebViewEnabled = true;
                        var ext = System.IO.Path.GetExtension(file.file_path);
                        documentConverter.ConvertTo(filePath.Replace(ext, ".pdf"), pdf);

                        if(file.doc_master_id != null)
                        {
                            var vb = db.doc_master.Find(file.doc_master_id);
                            if(vb != null)
                            {
                                var new_FileName_pdf = System.IO.Path.GetFileNameWithoutExtension(vb.file_name) + ".pdf";
                                var new_duongdan = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(vb.file_path), System.IO.Path.GetFileNameWithoutExtension(vb.file_path)) + ".pdf";
                                var new_pdfpath = root + new_duongdan;

                                vb.file_path = new_duongdan;
                                vb.file_name = new_FileName_pdf;
                                vb.file_size = new System.IO.FileInfo(new_pdfpath).Length;
                                vb.file_type = "pdf";
                            }
                        }
                        else if(file.file_id != null)
                        {
                            var fi = db.doc_files.Find(file.file_id);
                            if(fi != null)
                            {
                                var new_FileName_pdf = System.IO.Path.GetFileNameWithoutExtension(fi.file_name) + ".pdf";
                                var new_duongdan = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fi.file_path), System.IO.Path.GetFileNameWithoutExtension(fi.file_path)) + ".pdf";
                                var new_pdfpath = root + new_duongdan;

                                fi.file_path = new_duongdan;
                                fi.file_name = new_FileName_pdf;
                                fi.file_size = new System.IO.FileInfo(new_pdfpath).Length;
                                fi.file_type = "pdf";
                            };
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/ConvertToPDF", ip, tid, "Lỗi khi convert file thành pdf", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/ConvertToPDF", ip, tid, "Lỗi khi convert file thành pdf", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion
        #region Reservation Code
        [HttpPost]
        public async Task<HttpResponseMessage> Validate_Reservation_Code(doc_reservation_number reservation_number)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        string reservation_code = reservation_number.reservation_code;
                        if (!String.IsNullOrEmpty(reservation_code))
                        {
                            var find_same = db.doc_master.FirstOrDefault(x => x.nav_type == reservation_number.nav_type && x.doc_code.Trim().ToLower() == reservation_number.reservation_code.Trim().ToLower());
                            if(find_same != null)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", is_same = true });
                            }
                            var exists = db.doc_reservation_number.FirstOrDefault(x => x.organization_id == reservation_number.organization_id && x.nav_type == reservation_number.nav_type && x.reservation_code.Trim().ToLower() == reservation_number.reservation_code.Trim().ToLower() && !x.is_used);
                            if (exists != null)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", is_same = true });
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Validate_Reservation_Code", ip, tid, "Lỗi khi kiểm tra số văn bản đặt chỗ", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Validate_Reservation_Code", ip, tid, "Lỗi khi kiểm tra số văn bản đặt chỗ", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Save_Reservation_Code(doc_reservation_number reservation_number)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (!String.IsNullOrEmpty(reservation_number.reservation_code))
                        {
                            var find_same = db.doc_master.FirstOrDefault(x => x.nav_type == reservation_number.nav_type && x.doc_code.Trim().ToLower() == reservation_number.reservation_code.Trim().ToLower());
                            if (find_same != null)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms="Số đã tồn tại! Vui lòng chọn số khác!" });
                            }
                            var exists = db.doc_reservation_number.FirstOrDefault(x => x.organization_id == reservation_number.organization_id && x.nav_type == reservation_number.nav_type && x.reservation_code.Trim().ToLower() == reservation_number.reservation_code.Trim().ToLower() && !x.is_used);
                            if (exists != null)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Số đã tồn tại! Vui lòng chọn số khác!" });
                            }
                        }

                        reservation_number.created_date = DateTime.Now;
                        reservation_number.created_by = uid;
                        reservation_number.created_token_id = tid;
                        reservation_number.created_ip = ip;
                        db.doc_reservation_number.Add(reservation_number);
                        db.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Save_Reservation_Code", ip, tid, "Lỗi khi lưu số văn bản đặt chỗ", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocMain/Save_Reservation_Code", ip, tid, "Lỗi khi lưu số văn bản đặt chỗ", 0, "DocMain");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Reservation_Code([FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
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
                        var us = await db.sys_users.FindAsync(uid);
                        var das = await db.doc_reservation_number.Where(a => id.Contains(a.reservation_number_id)).ToListAsync();
                        if (das != null)
                        {
                            List<doc_reservation_number> del = new List<doc_reservation_number>();
                            foreach (var da in das)
                            {
                                if (ad || da.created_by == us.user_id)
                                {
                                    del.Add(da);
                                }
                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.doc_reservation_number.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "DocMain/Delete_Reservation_Code", ip, tid, "Lỗi khi xoá giữ số văn bản", 0, "DocMain");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "DocMain/Delete_Reservation_Code", ip, tid, "Lỗi khi xoá giữ số văn bản", 0, "DocMain");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        #endregion
        #region Export Message Encrypted XML
        private string ExportMessage_XML(doc_follows follow, string save_dir_path)
        {
            string xml_result = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var vb = db.doc_master.Find(follow.doc_master_id);
                    xml_result += "<document><compendium>" + vb.compendium + "</compendium>";

                    var lst_ele = new Dictionary<string, object>()
                    {
                        { "send_by_name", follow.send_by_name},
                        { "receive_by_name", follow.receive_by_name},
                        { "send_date", follow.send_date},
                        { "message", follow.message}
                    };

                    foreach (KeyValuePair<string, object> col in lst_ele)
                    {
                        var column_value = col.Value;
                        DateTime dt;
                        if (DateTime.TryParse(col.Value.ToString(), out dt))
                        {
                            column_value = DateTime.Parse(col.Value.ToString()).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                        }
                        xml_result += @"<" + col.Key + @">" + col.Value + @"</" + col.Key + @">";
                    }

                    xml_result += "</document>";
                    var xml_path = save_dir_path + '/' + vb.doc_master_guid + '_' + DateTime.Now.ToString("ddMMyyyyHHmm", CultureInfo.InvariantCulture) + ".xml";
                    File.WriteAllText(xml_path, xml_result);
                    var res_encr = helper.encryptXML(xml_path, "document", vb.compendium);
                    if (res_encr != "OK") return "Không thể mã hoã file XML!";

                    return "OK";
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                if (!helper.debug)
                {
                    contents = "";
                }
                return contents;
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                if (!helper.debug)
                {
                    contents = "";
                }
                return contents;
            }
        }
        #endregion
        #region Send Socket
        private string SendSocketMultiple(IEnumerable<string> user_ids, string title, string contents)
        {
            if (helper.socketClient != null && helper.socketClient.Connected == true)
            {
                try
                {
                    var message = new Dictionary<string, dynamic>
                        {
                            { "event", "sendNotify" },
                            { "user_id", "chatbot" },
                            { "title", title },
                            { "contents", contents },
                            { "date", DateTime.Now },
                            { "uids", user_ids },
                        };
                    helper.socketClient.EmitAsync("sendData", message);
                }
                catch (Exception) { };
            }
            return "OK";
        }
        private string SendSocketSingle(Dictionary<string, string> users, string title)
        {
            if (helper.socketClient != null && helper.socketClient.Connected == true)
            {
                try
                {
                    List<Dictionary<string, dynamic>> sendsockets = new List<Dictionary<string, dynamic>>();
                    foreach (var us in users)
                    {
                        var message = new Dictionary<string, dynamic>
                            {
                                { "event", "sendNotify" },
                                { "user_id", "chatbot" },
                                { "title", title },
                                { "contents", us.Value },
                                { "date", DateTime.Now },
                                { "uids", new List<string>(){ us.Key } },
                            };
                        sendsockets.Add(message);
                    }
                   
                    if (sendsockets.Count > 0)
                    {
                        foreach (Dictionary<string, dynamic> par in sendsockets)
                        {
                            helper.socketClient.EmitAsync("sendData", par);
                        }
                    }
                }
                catch (Exception) { };
            }
            return "OK";
        }
        #endregion
        [HttpPost]
        [System.Web.Mvc.ValidateInput(false)]
        public string SignDoc(string root, string url, List<WaterImage> WaterImages)
        {
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var Watermarks = new List<Watermark>();
                    foreach (var wi in WaterImages)
                    {
                        if (!String.IsNullOrEmpty(wi.ImageFile))
                        {
                            ImageWatermark im = new ImageWatermark
                            {
                                ImageFile = root + wi.ImageFile,
                                PageRange = wi.PageRange,
                                HorizontalDistance = wi.HorizontalDistance * 0.75,
                                VerticalDistance = wi.VerticalDistance * 0.75,
                                Width = wi.Width * 0.75,
                                Height = wi.Height * 0.75,
                                Rotation = wi.Rotation,
                                Opacity = wi.Opacity
                            };
                            Watermarks.Add(im);
                        }
                    }
                    string filePath = root + url;
                    var documentConverter = new DocumentConverter(filePath);
                    PdfOutputOptions pdf = new PdfOutputOptions();
                    pdf.Watermarks = Watermarks.ToArray();
                    pdf.FastWebViewEnabled = true;
                    var ext = System.IO.Path.GetExtension(url);
                    documentConverter.ConvertTo(filePath.Replace(ext, ".pdf"), pdf);
                    return "OK";
                }
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                return contents;
            }
        }
        public string SignDoc_Txt(string root, string url, List<WaterTxt> WaterTxts)
        {
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var Watermarks = new List<Watermark>();
                    foreach (var wi in WaterTxts)
                    {
                        if (!String.IsNullOrEmpty(wi.Text))
                        {
                            var font_style = FontStyle.Regular;
                            if (wi.Italic && wi.Bold)
                            {
                                font_style = FontStyle.Italic | FontStyle.Bold;
                            }
                            else if (wi.Italic)
                            {
                                font_style = FontStyle.Italic;
                            }
                            else if (wi.Bold)
                            {
                                font_style = FontStyle.Bold;
                            }
                            GleamTech.DocumentUltimate.TextWatermark im = new GleamTech.DocumentUltimate.TextWatermark
                            {
                                Text = wi.Text,
                                PageRange = wi.PageRange,
                                HorizontalDistance = wi.HorizontalDistance * 0.75,
                                VerticalDistance = wi.VerticalDistance * 0.75,
                                Width = wi.Width * 0.75,
                                Height = wi.Height * 0.75,
                                Rotation = wi.Rotation,
                                Opacity = wi.Opacity,
                                Font = new Font("Times New Roman", (float)(17 * 0.75),  font_style),
                                UseFontSize = true
                            };
                            Watermarks.Add(im);
                        }
                    }
                    string filePath = root + url;
                    var documentConverter = new DocumentConverter(filePath);
                    PdfOutputOptions pdf = new PdfOutputOptions();
                    pdf.Watermarks = Watermarks.ToArray();
                    pdf.FastWebViewEnabled = true;
                    var ext = System.IO.Path.GetExtension(url);
                    documentConverter.ConvertTo(filePath.Replace(ext, ".pdf"), pdf);
                    return "OK";
                }
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                return contents;
            }
        }
        #region Filter
        [HttpPost]
        public async Task<HttpResponseMessage> FilterDoc_Receive([FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                int organization_id = data["organization_id"].ToObject<int>();
                int user_key = data["user_key"].ToObject<int>();
                int typeCount = data["typeCount"].ToObject<int>();
                int pageno = data["pageno"].ToObject<int>();
                int pagesize = data["pagesize"].ToObject<int>();
                string search = data["search"].ToObject<string>();
                List<FieldSQL> fields = data["fields"].ToObject<List<FieldSQL>>();
                string order_by = data["order_by"].ToObject<string>();

                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                string sql = "";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var usernow = db.sys_users.Find(uid);
                        if (usernow != null)
                        {
                            string WhereSQL = "";

                            if (usernow.is_super == true)
                            {
                                WhereSQL += "";
                            }
                            //Default
                            WhereSQL += @"
                                (" + typeCount + @" is null or " + typeCount + @" = 0 or (" + typeCount + @" is not null and " + typeCount + @"<>0 and 
	                            ((" + typeCount + @" = 1 and nav_type = 1 and type_group is null) or (" + typeCount + @" = 2 and nav_type = 2 and type_group is null) or (" + typeCount + @" = 3 and nav_type = 3 and type_group is null) 
	                            or (" + typeCount + @" = 4 and (select count(*) from task_linkdoc ld where ld.doc_master_id = do.doc_master_id) > 0 /*Bổ sung task lien quan sau*/) 
	                            or (" + typeCount + @" = 5 and ((view_date is null and status_id not in ('sohoa','duthao')) or  (select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") is null)) 
	                            or (" + typeCount + @" = 6 and stt.is_handle = 1) 
	                            or (" + typeCount + @" = 7 and date_deadline is not null and date_deadline <0 and status_id not in ('hoanthanh','phanphat','dadongdau'))
                                 or (" + typeCount + @" = 8 and gr.type_group = 0) 
                                 or (" + typeCount + @" = 9 and gr.type_group = 1) 
                                )))
                            ";
                            //serch
                            if (!string.IsNullOrWhiteSpace(search))
                            {
                                WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + "(" + @" CONTAINS(do.compendium,'""*" + search + @"*""')" + "or" + "(do.dispatch_book_num like N'%" + search + "%')" + "or" + @" CONTAINS(do.doc_code,'""*" + search + @"*""')" + "or" + @" CONTAINS(do.issue_place,'""*" + search + @"*""')" + ")";
                            }
                            //filed
                            if (fields != null && fields.Count > 0)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.filteroperator == "in")
                                    {
                                        WhereSQL += (WhereSQL != "" ? " and " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => (field.type_of == 1 ? "N" : "") + "'" + a.value + "'").ToList()) + ")";
                                    }
                                    else
                                    {
                                        foreach (var m in field.filterconstraints.Where(a => a.value != null))
                                        {
                                            switch (m.matchMode)
                                            {
                                                case "contains":
                                                    WhereSQL += " " + field.filteroperator + " (N'" + m.value + "' like N'%' + do." + field.key + " + ',%')";
                                                    break;
                                                case "containsMany":
                                                    List<string> listKey = m.value.Split(',').ToList();
                                                    WhereSQL += " " + field.filteroperator + " (";
                                                    foreach (var str in listKey)
                                                    {
                                                        if (str.Trim() != "")
                                                        {
                                                            WhereSQL += " (" + field.key + " like N'%" + str + "%')  or";
                                                        }
                                                    }
                                                    if (WhereSQL.EndsWith(" or"))
                                                    {
                                                        WhereSQL = WhereSQL.Substring(0, WhereSQL.Length - 3);
                                                    }
                                                    WhereSQL += ")";
                                                    break;
                                                case "equals":
                                                    WhereSQL += " " + field.filteroperator + " do." + field.key + " = N'" + m.value + "'";
                                                    break;
                                                case "dateBefore":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                                    break;
                                                case "dateAfter":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                                    break;

                                            }
                                        }
                                    }
                                }
                            }
                            //Cut
                            if (WhereSQL.StartsWith(" and "))
                            {
                                WhereSQL = WhereSQL.Substring(4);
                            }
                            else if (WhereSQL.StartsWith(" or "))
                            {
                                WhereSQL = WhereSQL.Substring(3);
                            }
                            //Select
                            sql = @" 
                                        ;WITH us_group AS(
		                                    select role_group_id
		                                    from doc_ca_role_groups gr
		                                    where '" + usernow.user_id + @"' in (select user_id from doc_ca_role_group_users where role_group_id = gr.role_group_id)
	                                    ),
	                                    approval_doc AS (
				                                    select fl.doc_master_id, max(send_date) as send_date
				                                    from doc_follows fl
				                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and (receive_last_group_user = " + user_key + @" or receive_by in (select role_group_id from us_group))) or (receive_type = 2 and receive_by = " + organization_id + @") or (receive_type = 3 and receive_last_group_user = " + user_key + @")) and doc_status_id in ('xulychinh','chopheduyet','chodongdau','dadongdau','tralai') and is_recall = 0 and not exists(select follow_id from doc_follows where follow_parent_id = fl.follow_id and is_recall = 0)
				                                    group by fl.doc_master_id,fl.organization_id
	                                    ),
	                                    follows as (
		                                    select send_by, ap.send_date, send_by_name, follow_id, fl.doc_master_id,doc_status_id,[message],case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end as date_deadline,
		                                    view_date, is_completed, deadline_date, is_signed, is_prioritized
		                                    from doc_follows fl join approval_doc ap on fl.doc_master_id = ap.doc_master_id and fl.send_date = ap.send_date
		                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and (receive_last_group_user = " + user_key + @" or receive_by in (select role_group_id from us_group))) or (receive_type = 2 and receive_by = " + organization_id + @") or (receive_type = 3 and receive_last_group_user = " + user_key + @")) and doc_status_id in ('xulychinh','chopheduyet','chodongdau','dadongdau','tralai') and is_recall = 0
		                                    union all
		                                    select send_by, send_date, send_by_name, follow_id, fl.doc_master_id,doc_status_id,[message],case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end as date_deadline,
		                                    view_date, is_completed,deadline_date, is_signed, is_prioritized
		                                    from doc_follows fl
		                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and (receive_last_group_user = " + user_key + @" or receive_by in (select role_group_id from us_group))) or (receive_type = 2 and receive_by = " + organization_id + @") or (receive_type = 3 and receive_last_group_user = " + user_key + @")) and doc_status_id not in ('xulychinh','chopheduyet','chodongdau','dadongdau','tralai') and is_recall = 0
		                                    union all 
		                                    select created_by, created_date, (select full_name from sys_users where user_key = doc_master.created_by), null, doc_master_id,doc_status_id,null,null,null,null,null,null,null
		                                    from doc_master
		                                    where created_by = " + user_key + @" and doc_status_id in ('duthao','sohoa')
	                                    )
	                                    select handle_date, do.doc_master_id, fl.[message],compendium, doc_code, nav_type, doc_date, fl.send_by,fl.send_by_name,fl.send_date,fl.follow_id,us.avatar,is_signed,fl.is_prioritized,do.urgency,do.is_not_send_paper,
	                                    stt.status_id, stt.status_name, stt.background_color, stt.text_color,do.first_doc_status_id,do.file_path,do.file_name,fl.is_completed,do.is_drafted,
	                                    (select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") as view_id, view_date, date_deadline, abs(date_deadline) as abs_date_deadline, fl.deadline_date, do.deadline_date as deadline_date_master,
                                        (select count(*) from task_linkdoc ld where ld.doc_master_id = do.doc_master_id) as countTask
	                                    from follows fl join doc_master do on do.doc_master_id = fl.doc_master_id
	                                    join sys_users us on fl.send_by = us.user_key
                                        left join doc_ca_groups gr on do.doc_group_id = gr.doc_group_id
	                                    join doc_ca_status stt on ((fl.is_completed is null and fl.doc_status_id = stt.status_id) or (fl.is_completed = 1 and stt.status_id = 'hoanthanh'))";

                            if (WhereSQL.Trim() != "")
                            {
                                sql += " where " + WhereSQL;
                            }
                            string OFFSET = @"(" + pageno + @") * (" + pagesize + @")";
                            sql += @" ORDER BY " + order_by
                                 + @" OFFSET " + OFFSET + " ROWS FETCH NEXT " + pagesize + " ROWS ONLY ";
                            sql = sql.Replace("--", "").Replace("\r", " ").Replace("   ", " ").Trim();
                            sql = Regex.Replace(sql, "drop", "");
                            sql = Regex.Replace(sql, "update", "");
                            sql = Regex.Replace(sql, "delete", "");
                            sql = Regex.Replace(sql, @"\s+", " ").Trim();
                            var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                            var tables = await task;
                            string JSONresult = JsonConvert.SerializeObject(tables);
                            return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { sql, err = "2", ms = "Bạn không có quyền truy cập chức năng này" });
                        }
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                        domainurl + "/DocMain/FilterDoc_Receive", ip, tid, "Lỗi khi gọi FilterDoc_Receive", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                       domainurl + "/DocMain/FilterDoc_Receive", ip, tid, "Lỗi khi gọi FilterDoc_Receive", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Count_FilterDoc_Receive([FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                int organization_id = data["organization_id"].ToObject<int>();
                int user_key = data["user_key"].ToObject<int>();
                string search = data["search"].ToObject<string>();
                List<FieldSQL> fields = data["fields"].ToObject<List<FieldSQL>>();

                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                string sql = "";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var usernow = db.sys_users.Find(uid);
                        if (usernow != null)
                        {
                            string WhereSQL = "";

                            if (usernow.is_super == true)
                            {
                                WhereSQL += "";
                            }
                            //serch
                            if (!string.IsNullOrWhiteSpace(search))
                            {
                                WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + "(" + @" CONTAINS(do.compendium,'""*" + search + @"*""')" + "or" + "(do.dispatch_book_num like N'%" + search + "%')" + "or" + @" CONTAINS(do.doc_code,'""*" + search + @"*""')" + "or" + @" CONTAINS(do.issue_place,'""*" + search + @"*""')" + ")";
                            }
                            //filed
                            if (fields != null && fields.Count > 0)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.filteroperator == "in")
                                    {
                                        WhereSQL += (WhereSQL != "" ? " and " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => (field.type_of == 1 ? "N" : "") + "'" + a.value + "'").ToList()) + ")";
                                    }
                                    else
                                    {
                                        foreach (var m in field.filterconstraints.Where(a => a.value != null))
                                        {
                                            switch (m.matchMode)
                                            {
                                                case "contains":
                                                    WhereSQL += " " + field.filteroperator + " (N'" + m.value + "' like N'%' + do." + field.key + " + ',%')";
                                                    break;
                                                case "containsMany":
                                                    List<string> listKey = m.value.Split(',').ToList();
                                                    WhereSQL += " " + field.filteroperator + " (";
                                                    foreach (var str in listKey)
                                                    {
                                                        if (str.Trim() != "")
                                                        {
                                                            WhereSQL += " (" + field.key + " like N'%" + str + "%')  or";
                                                        }
                                                    }
                                                    if (WhereSQL.EndsWith(" or"))
                                                    {
                                                        WhereSQL = WhereSQL.Substring(0, WhereSQL.Length - 3);
                                                    }
                                                    WhereSQL += ")";
                                                    break;
                                                case "equals":
                                                    WhereSQL += " " + field.filteroperator + " do." + field.key + " = N'" + m.value + "'";
                                                    break;
                                                case "dateBefore":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                                    break;
                                                case "dateAfter":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                                    break;

                                            }
                                        }
                                    }
                                }
                            }
                            //Cut
                            if (WhereSQL.StartsWith(" and "))
                            {
                                WhereSQL = WhereSQL.Substring(4);
                            }
                            else if (WhereSQL.StartsWith(" or "))
                            {
                                WhereSQL = WhereSQL.Substring(3);
                            }
                            //Select
                            sql = @" 
                                        ;WITH us_group AS(
		                                    select role_group_id
		                                    from doc_ca_role_groups gr
		                                    where '" + usernow.user_id + @"' in (select user_id from doc_ca_role_group_users where role_group_id = gr.role_group_id)
	                                    ),
	                                    approval_doc AS (
				                                    select fl.doc_master_id, max(send_date) as send_date
				                                    from doc_follows fl
				                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and (receive_last_group_user = " + user_key + @" or receive_by in (select role_group_id from us_group))) or (receive_type = 2 and receive_by = " + organization_id + @") or (receive_type = 3 and receive_last_group_user = " + user_key + @")) and doc_status_id in ('xulychinh','chopheduyet','chodongdau','dadongdau','tralai') and is_recall = 0 and not exists(select follow_id from doc_follows where follow_parent_id = fl.follow_id and is_recall = 0)
				                                    group by fl.doc_master_id,fl.organization_id
	                                    ),
	                                    follows as (
		                                    select send_by, ap.send_date, send_by_name, follow_id, fl.doc_master_id,doc_status_id,[message],deadline_date,fl.is_completed,fl.view_date
		                                    from doc_follows fl join approval_doc ap on fl.doc_master_id = ap.doc_master_id and fl.send_date = ap.send_date
		                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and (receive_last_group_user = " + user_key + @" or receive_by in (select role_group_id from us_group))) or (receive_type = 2 and receive_by = " + organization_id + @") or (receive_type = 3 and receive_last_group_user = " + user_key + @")) and doc_status_id in ('xulychinh','chopheduyet','dadongdau','chodongdau','tralai') and is_recall = 0
		                                    union all
		                                    select send_by, send_date, send_by_name, follow_id, fl.doc_master_id,doc_status_id,[message],deadline_date,fl.is_completed,fl.view_date
		                                    from doc_follows fl
		                                    where ((receive_type = 0 and receive_by = " + user_key + @") or (receive_type = 1 and (receive_last_group_user = " + user_key + @" or receive_by in (select role_group_id from us_group))) or (receive_type = 2 and receive_by = " + organization_id + @") or (receive_type = 3 and receive_last_group_user = " + user_key + @")) and doc_status_id not in ('xulychinh','chopheduyet','dadongdau','chodongdau','tralai') and is_recall = 0
		                                    union all 
		                                    select created_by, created_date, (select full_name from sys_users where user_key = doc_master.created_by), null, doc_master_id,doc_status_id,null,null,null,null
		                                    from doc_master
		                                    where created_by = " + user_key + @" and doc_status_id in ('duthao','sohoa')
	                                    )
	                                    select do.doc_master_id, fl.[message],compendium, doc_code, nav_type, doc_date, fl.send_by,fl.send_by_name,fl.send_date,fl.follow_id,us.avatar,
	                                    stt.status_id, stt.status_name, stt.background_color, stt.text_color, stt.is_handle,case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end as date_deadline,
	                                    (select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") as view_id,fl.view_date,
	                                    (select count(*) from task_linkdoc ld where ld.doc_master_id = do.doc_master_id) as countTask,gr.type_group
	                                    into #Doc
	                                    from follows fl join doc_master do on do.doc_master_id = fl.doc_master_id
	                                    join sys_users us on fl.send_by = us.user_key
                                        left join doc_ca_groups gr on do.doc_group_id = gr.doc_group_id
	                                    left join doc_ca_status stt on ((fl.is_completed is null and fl.doc_status_id = stt.status_id) or (fl.is_completed = 1 and stt.status_id = 'hoanthanh'))";

                            if (WhereSQL.Trim() != "")
                            {
                                sql += " where " + WhereSQL;
                            }
                            sql += @"
                                Declare @handle int,@ood int, @receive int, @send int, @internal int, @notseen int, @all int, @reltask int, @invite int, @insurance int
		                        set @all = (select count(*) from #Doc)
		                        set @receive = (select count(*) from #Doc where nav_type = 1 and type_group is null)
		                        set @send = (select count(*) from #Doc where nav_type = 2 and type_group is null)
		                        set @internal = (select count(*) from #Doc where nav_type = 3 and type_group is null)
		                        SET @handle=(Select count(*) from #Doc where is_handle=1)
		                        SET @notseen=(Select count(*) from #Doc where view_id is null or (view_date is null and status_id not in ('sohoa','duthao')))
		                        SET @ood=(Select count(*) from #Doc where date_deadline is not null and date_deadline <0 and status_id not in ('hoanthanh','phanphat','dadongdau'))
		                        SET @reltask = (Select count(*) from #Doc where countTask > 0)
                                SET @invite = (Select count(*) from #Doc where type_group = 0)
		                        SET @insurance = (Select count(*) from #Doc where type_group = 1)
		                        Select @all as [all], @receive as [receive], @send [send],@internal internal,@handle as handle,@notseen notseen, @ood as ood, @reltask as reltask, @invite as invite, @insurance as insurance
                            ";
                            sql = sql.Replace("--", "").Replace("\r", " ").Replace("   ", " ").Trim();
                            sql = Regex.Replace(sql, "drop", "");
                            sql = Regex.Replace(sql, "update", "");
                            sql = Regex.Replace(sql, "delete", "");
                            sql = Regex.Replace(sql, @"\s+", " ").Trim();
                            var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                            var tables = await task;
                            string JSONresult = JsonConvert.SerializeObject(tables);
                            return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { sql, err = "2", ms = "Bạn không có quyền truy cập chức năng này" });
                        }
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                        domainurl + "/DocMain/Count_FilterDoc_Receive", ip, tid, "Lỗi khi gọi Count_FilterDoc_Receive", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                       domainurl + "/DocMain/Count_FilterDoc_Receive", ip, tid, "Lỗi khi gọi Count_FilterDoc_Receive", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> FilterDoc_Send([FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                int organization_id = data["organization_id"].ToObject<int>();
                int user_key = data["user_key"].ToObject<int>();
                int typeCount = data["typeCount"].ToObject<int>();
                int pageno = data["pageno"].ToObject<int>();
                int pagesize = data["pagesize"].ToObject<int>();
                string search = data["search"].ToObject<string>();
                List<FieldSQL> fields = data["fields"].ToObject<List<FieldSQL>>();
                string order_by = data["order_by"].ToObject<string>();

                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                string sql = "";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var usernow = db.sys_users.Find(uid);
                        if (usernow != null)
                        {
                            string WhereSQL = "";

                            if (usernow.is_super == true)
                            {
                                WhereSQL += "";
                            }
                            //Default
                            WhereSQL += @"
                                (" + typeCount + @" is null or " + typeCount + @" = 0 or (" + typeCount + @" is not null and " + typeCount + @"<>0 and 
	                            ((" + typeCount + @" = 1 and nav_type = 1 and type_group is null) or (" + typeCount + @" = 2 and nav_type = 2 and type_group is null) or (" + typeCount + @" = 3 and nav_type = 3 and type_group is null) 
	                            or (" + typeCount + @" = 4 and (select count(*) from task_linkdoc ld where ld.doc_master_id = do.doc_master_id) > 0 /*Bổ sung task lien quan sau*/) 
	                            or (" + typeCount + @" = 5 and (select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") is null) 
	                            or (" + typeCount + @" = 6 and stt.is_handle = 1) 
	                            or (" + typeCount + @" = 7 and date_deadline is not null and date_deadline <0 and status_id not in ('hoanthanh','phanphat','dadongdau'))
                                or (" + typeCount + @" = 8 and gr.type_group = 0) 
                                or (" + typeCount + @" = 9 and gr.type_group = 1) 
                                )))
                            ";
                            //serch
                            if (!string.IsNullOrWhiteSpace(search))
                            {
                                WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + "(" + @" CONTAINS(do.compendium,'""*" + search + @"*""')" + "or" + "(do.dispatch_book_num like N'%" + search + "%')" + "or" + @" CONTAINS(do.doc_code,'""*" + search + @"*""')" + "or" + @" CONTAINS(do.issue_place,'""*" + search + @"*""')" + ")";
                            }
                            //filed
                            if (fields != null && fields.Count > 0)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.filteroperator == "in")
                                    {
                                        WhereSQL += (WhereSQL != "" ? " and " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => (field.type_of == 1 ? "N" : "") + "'" + a.value + "'").ToList()) + ")";
                                    }
                                    else
                                    {
                                        foreach (var m in field.filterconstraints.Where(a => a.value != null))
                                        {
                                            switch (m.matchMode)
                                            {
                                                case "contains":
                                                    WhereSQL += " " + field.filteroperator + " (N'" + m.value + "' like N'%' + do." + field.key + " + ',%')";
                                                    break;
                                                case "containsMany":
                                                    List<string> listKey = m.value.Split(',').ToList();
                                                    WhereSQL += " " + field.filteroperator + " (";
                                                    foreach (var str in listKey)
                                                    {
                                                        if (str.Trim() != "")
                                                        {
                                                            WhereSQL += " (" + field.key + " like N'%" + str + "%')  or";
                                                        }
                                                    }
                                                    if (WhereSQL.EndsWith(" or"))
                                                    {
                                                        WhereSQL = WhereSQL.Substring(0, WhereSQL.Length - 3);
                                                    }
                                                    WhereSQL += ")";
                                                    break;
                                                case "equals":
                                                    WhereSQL += " " + field.filteroperator + " do." + field.key + " = N'" + m.value + "'";
                                                    break;
                                                case "dateBefore":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                                    break;
                                                case "dateAfter":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                                    break;

                                            }
                                        }
                                    }
                                }
                            }
                            //Cut
                            if (WhereSQL.StartsWith(" and "))
                            {
                                WhereSQL = WhereSQL.Substring(4);
                            }
                            else if (WhereSQL.StartsWith(" or "))
                            {
                                WhereSQL = WhereSQL.Substring(3);
                            }
                            //Select
                            sql = @" 
                                        ;WITH same_date as(
			                select doc_master_id, follow_parent_id, send_date, min(is_order) as is_order
			                from doc_follows
			                where send_by = " + user_key + @" and is_recall = 0
			                group by doc_master_id, follow_parent_id, send_date
	                ),send_doc AS (
			                select follow_id,fl.organization_id,fl.doc_master_id,send_by,send_by_name,fl.send_date,is_completed, fl.receive_type,fl.receive_by,fl.follow_parent_id,
			                view_date,doc_status_id,[message],case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end as date_deadline,
			                (case when fl.receive_type = 0 then (select avatar from sys_users us where fl.receive_by = us.user_key)
			                when fl.receive_type = 1 then null
			                else (select logo from sys_organization og where fl.receive_by = og.organization_id) end) as avatar
			                from same_date sd join doc_follows fl on sd.doc_master_id = fl.doc_master_id and (sd.follow_parent_id is null and fl.follow_parent_id is null or (sd.follow_parent_id is not null and sd.follow_parent_id = fl.follow_parent_id)) and sd.send_date = fl.send_date and sd.is_order = fl.is_order
			                where fl.send_by = " + user_key + @" and is_recall = 0
	                )
	                select handle_date, do.doc_master_id, fl.[message],compendium, doc_code, nav_type, doc_date,fl.send_date,fl.follow_id,fl.follow_parent_id,fl.avatar,do.urgency,do.is_not_send_paper,
	                stt.status_id, stt.status_name, stt.background_color, stt.text_color,do.first_doc_status_id,do.file_path,fl.is_completed,
	                (select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") as view_id, view_date,
	                (select receive_by_name from view_doc_name_receiver_follows vi where ((fl.follow_parent_id is null and vi.follow_parent_id is null) or (fl.follow_parent_id is not null and fl.follow_parent_id = vi.follow_parent_id)) and fl.doc_master_id = vi.doc_master_id and fl.send_date = vi.send_date) as receive_by_name
	                from send_doc fl join doc_master do on do.doc_master_id = fl.doc_master_id
                    left join doc_ca_groups gr on do.doc_group_id = gr.doc_group_id
	                join doc_ca_status stt on ((fl.is_completed is null and fl.doc_status_id = stt.status_id) or (fl.is_completed = 1 and stt.status_id = 'hoanthanh'))";

                            if (WhereSQL.Trim() != "")
                            {
                                sql += " where " + WhereSQL;
                            }
                            string OFFSET = @"(" + pageno + @") * (" + pagesize + @")";
                            sql += @" ORDER BY " + order_by
                                + @" OFFSET " + OFFSET + " ROWS FETCH NEXT " + pagesize + " ROWS ONLY ";
                            sql = sql.Replace("--", "").Replace("\r", " ").Replace("   ", " ").Trim();
                            sql = Regex.Replace(sql, "drop", "");
                            sql = Regex.Replace(sql, "update", "");
                            sql = Regex.Replace(sql, "delete", "");
                            sql = Regex.Replace(sql, @"\s+", " ").Trim();
                            var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                            var tables = await task;
                            string JSONresult = JsonConvert.SerializeObject(tables);
                            return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { sql, err = "2", ms = "Bạn không có quyền truy cập chức năng này" });
                        }
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                        domainurl + "/DocMain/FilterDoc_Send", ip, tid, "Lỗi khi gọi FilterDoc_Send", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                       domainurl + "/DocMain/FilterDoc_Send", ip, tid, "Lỗi khi gọi FilterDoc_Send", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Count_FilterDoc_Send([FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                int organization_id = data["organization_id"].ToObject<int>();
                int user_key = data["user_key"].ToObject<int>();
                string search = data["search"].ToObject<string>();
                List<FieldSQL> fields = data["fields"].ToObject<List<FieldSQL>>();

                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                string sql = "";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var usernow = db.sys_users.Find(uid);
                        if (usernow != null)
                        {
                            string WhereSQL = "";

                            if (usernow.is_super == true)
                            {
                                WhereSQL += "";
                            }
                            //serch
                            if (!string.IsNullOrWhiteSpace(search))
                            {
                                WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + "(" + @" CONTAINS(do.compendium,'""*" + search + @"*""')" + "or" + "(do.dispatch_book_num like N'%" + search + "%')" + "or" + @" CONTAINS(do.doc_code,'""*" + search + @"*""')" + "or" + @" CONTAINS(do.issue_place,'""*" + search + @"*""')" + ")";
                            }
                            //filed
                            if (fields != null && fields.Count > 0)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.filteroperator == "in")
                                    {
                                        WhereSQL += (WhereSQL != "" ? " and " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => (field.type_of == 1 ? "N" : "") + "'" + a.value + "'").ToList()) + ")";
                                    }
                                    else
                                    {
                                        foreach (var m in field.filterconstraints.Where(a => a.value != null))
                                        {
                                            switch (m.matchMode)
                                            {
                                                case "contains":
                                                    WhereSQL += " " + field.filteroperator + " (N'" + m.value + "' like N'%' + do." + field.key + " + ',%')";
                                                    break;
                                                case "containsMany":
                                                    List<string> listKey = m.value.Split(',').ToList();
                                                    WhereSQL += " " + field.filteroperator + " (";
                                                    foreach (var str in listKey)
                                                    {
                                                        if (str.Trim() != "")
                                                        {
                                                            WhereSQL += " (" + field.key + " like N'%" + str + "%')  or";
                                                        }
                                                    }
                                                    if (WhereSQL.EndsWith(" or"))
                                                    {
                                                        WhereSQL = WhereSQL.Substring(0, WhereSQL.Length - 3);
                                                    }
                                                    WhereSQL += ")";
                                                    break;
                                                case "equals":
                                                    WhereSQL += " " + field.filteroperator + " do." + field.key + " = N'" + m.value + "'";
                                                    break;
                                                case "dateBefore":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                                    break;
                                                case "dateAfter":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                                    break;

                                            }
                                        }
                                    }
                                }
                            }
                            //Cut
                            if (WhereSQL.StartsWith(" and "))
                            {
                                WhereSQL = WhereSQL.Substring(4);
                            }
                            else if (WhereSQL.StartsWith(" or "))
                            {
                                WhereSQL = WhereSQL.Substring(3);
                            }
                            //Select
                            sql = @";WITH same_date as(
			                        select doc_master_id, follow_parent_id, send_date, max(is_order) as is_order
			                        from doc_follows
			                        where send_by = " + user_key + @" and is_recall = 0
			                        group by doc_master_id, follow_parent_id, send_date
	                        ),send_doc AS (
			                        select follow_id,fl.organization_id,fl.doc_master_id,send_by,send_by_name,fl.send_date,is_completed,
			                        (select nav_type from doc_master where doc_master_id = fl.doc_master_id) as nav_type,
			                        view_date,doc_status_id,[message],fl.deadline_date
			                        from same_date sd join doc_follows fl on sd.doc_master_id = fl.doc_master_id and (sd.follow_parent_id is null and fl.follow_parent_id is null or (sd.follow_parent_id is not null and sd.follow_parent_id = fl.follow_parent_id)) and sd.send_date = fl.send_date and sd.is_order = fl.is_order
			                        where fl.send_by = " + user_key + @" and is_recall = 0
	                        )
	                        select fl.doc_master_id, fl.[message], fl.send_by,fl.send_by_name,fl.send_date,fl.follow_id,fl.nav_type,
	                        stt.status_id, stt.status_name, stt.background_color, stt.text_color, stt.is_handle,case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end as date_deadline,
	                        (select count(*) from task_linkdoc ld where ld.doc_master_id = fl.doc_master_id) as countTask,gr.type_group
	                        into #Doc
	                        from send_doc fl join doc_master do on do.doc_master_id = fl.doc_master_id
                            join (select type_group,doc_master_id from doc_ca_groups gr join doc_master doc on gr.doc_group_id = doc.doc_group_id) gr on gr.doc_master_id = fl.doc_master_id
	                        join doc_ca_status stt on fl.doc_status_id = stt.status_id";

                            if (WhereSQL.Trim() != "")
                            {
                                sql += " where " + WhereSQL;
                            }
                            sql += @"
                                Declare @handle int,@ood int, @receive int, @send int, @internal int, @notseen int, @all int, @reltask int, @invite int, @insurance int
		                        set @all = (select count(*) from #Doc)
		                        set @receive = (select count(*) from #Doc where nav_type = 1 and type_group is null)
		                        set @send = (select count(*) from #Doc where nav_type = 2 and type_group is null)
		                        set @internal = (select count(*) from #Doc where nav_type = 3 and type_group is null)
		                        SET @handle=(Select count(*) from #Doc where is_handle=1)
		                        SET @notseen=(Select count(*) from #Doc where not exists(select view_id from doc_views where doc_master_id = #Doc.doc_master_id and [user_id] = " + user_key + @"))
		                        SET @ood=(Select count(*) from #Doc where date_deadline is not null and date_deadline <0 and status_id not in ('hoanthanh','phanphat','dadongdau'))
		                        SET @reltask = (Select count(*) from #Doc where countTask > 0)
                                SET @invite = (Select count(*) from #Doc where type_group = 0)
		                        SET @insurance = (Select count(*) from #Doc where type_group = 1)
		                        Select @all as [all], @receive as [receive], @send [send],@internal internal,@handle as handle,@notseen notseen, @ood as ood, @reltask as reltask, @invite as invite, @insurance as insurance
                            ";
                            sql = sql.Replace("--", "").Replace("\r", " ").Replace("   ", " ").Trim();
                            sql = Regex.Replace(sql, "drop", "");
                            sql = Regex.Replace(sql, "update", "");
                            sql = Regex.Replace(sql, "delete", "");
                            sql = Regex.Replace(sql, @"\s+", " ").Trim();
                            var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                            var tables = await task;
                            string JSONresult = JsonConvert.SerializeObject(tables);
                            return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { sql, err = "2", ms = "Bạn không có quyền truy cập chức năng này" });
                        }
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                        domainurl + "/DocMain/Count_FilterDoc_Send", ip, tid, "Lỗi khi gọi Count_FilterDoc_Send", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                       domainurl + "/DocMain/Count_FilterDoc_Send", ip, tid, "Lỗi khi gọi Count_FilterDoc_Send", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> FilterDoc_Store([FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                int organization_id = data["organization_id"].ToObject<int>();
                int user_key = data["user_key"].ToObject<int>();
                int typeCount = data["typeCount"].ToObject<int>();
                int pageno = data["pageno"].ToObject<int>();
                int pagesize = data["pagesize"].ToObject<int>();
                string search = data["search"].ToObject<string>();
                List<FieldSQL> fields = data["fields"].ToObject<List<FieldSQL>>();
                string order_by = data["order_by"].ToObject<string>();

                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                string sql = "";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var usernow = db.sys_users.Find(uid);
                        if (usernow != null)
                        {
                            string WhereSQL = "";

                            if (usernow.is_super == true)
                            {
                                WhereSQL += "";
                            }
                            //Default
                            WhereSQL += @"do.organization_id = " + organization_id + @" and do.is_drafted = 0 and 
                                (" + typeCount + @" is null or " + typeCount + @" = 0 or (" + typeCount + @" is not null and " + typeCount + @"<>0 and 
	                            ((" + typeCount + @" = 1 and nav_type = 1 and type_group is null) or (" + typeCount + @" = 2 and nav_type = 2 and type_group is null) or (" + typeCount + @" = 3 and nav_type = 3 and type_group is null) 
	                            or (" + typeCount + @" = 4 and (select count(*) from task_linkdoc ld where ld.doc_master_id = do.doc_master_id) > 0 /*Bổ sung task lien quan sau*/) 
	                            or (" + typeCount + @" = 5 and (select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") is null) 
	                            or (" + typeCount + @" = 6 and stt.is_handle = 1) 
	                            or (" + typeCount + @" = 7 and (case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end) is not null and (case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end) < 0 and status_id not in ('hoanthanh','phanphat','dadongdau'))
                                or (" + typeCount + @" = 8 and stt.is_handle = 0)
                                or (" + typeCount + @" = 9 and stt.is_handle = 1)
                                )))
                            ";
                            //serch
                            if (!string.IsNullOrWhiteSpace(search))
                            {
                                WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + "(" + @" CONTAINS(do.compendium,'""*" + search + @"*""')" + "or" + "(do.dispatch_book_num like N'%" + search + "%')" + "or" + @" CONTAINS(do.doc_code,'""*" + search + @"*""')" + "or" + @" CONTAINS(do.issue_place,'""*" + search + @"*""')" + ")";
                            }
                            //filed
                            if (fields != null && fields.Count > 0)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.filteroperator == "in")
                                    {
                                        WhereSQL += (WhereSQL != "" ? " and " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => (field.type_of == 1 ? "N" : "") + "'" + a.value + "'").ToList()) + ")";
                                    }
                                    else
                                    {
                                        foreach (var m in field.filterconstraints.Where(a => a.value != null))
                                        {
                                            switch (m.matchMode)
                                            {
                                                case "contains":
                                                    WhereSQL += " " + field.filteroperator + " (N'" + m.value + "' like N'%' + do." + field.key + " + ',%')";
                                                    break;
                                                case "containsMany":
                                                    List<string> listKey = m.value.Split(',').ToList();
                                                    WhereSQL += " " + field.filteroperator + " (";
                                                    foreach (var str in listKey)
                                                    {
                                                        if (str.Trim() != "")
                                                        {
                                                            WhereSQL += " (" + field.key + " like N'%" + str + "%')  or";
                                                        }
                                                    }
                                                    if (WhereSQL.EndsWith(" or"))
                                                    {
                                                        WhereSQL = WhereSQL.Substring(0, WhereSQL.Length - 3);
                                                    }
                                                    WhereSQL += ")";
                                                    break;
                                                case "equals":
                                                    WhereSQL += " " + field.filteroperator + " do." + field.key + " = N'" + m.value + "'";
                                                    break;
                                                case "dateBefore":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                                    break;
                                                case "dateAfter":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                                    break;

                                            }
                                        }
                                    }
                                }
                            }
                            //Cut
                            if (WhereSQL.StartsWith(" and "))
                            {
                                WhereSQL = WhereSQL.Substring(4);
                            }
                            else if (WhereSQL.StartsWith(" or "))
                            {
                                WhereSQL = WhereSQL.Substring(3);
                            }
                            //Select
                            sql = @" 
	select handle_date, do.doc_master_id, fl.[message],compendium, doc_code, nav_type, doc_date, ISNULL(fl.send_by, do.created_by) as send_by, ISNULL(fl.send_by_name, (select full_name from sys_users where user_key = do.created_by)) as send_by_name,ISNULL(fl.send_date,do.created_date) as send_date,fl.follow_id,us.avatar,is_signed,fl.is_prioritized,do.urgency,do.is_not_send_paper,
	stt.status_id, stt.status_name, stt.background_color, stt.text_color,do.first_doc_status_id,do.file_path,do.file_name,fl.is_completed,
	(select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") as view_id, view_date, case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end as date_deadline
	from doc_master do left join doc_follows fl on do.follow_id = fl.follow_id
	left join sys_users us on ISNULL(fl.send_by, do.created_by) = us.user_key
    left join doc_ca_groups gr on do.doc_group_id = gr.doc_group_id
	left join doc_ca_status stt on ((fl.is_completed is null and fl.doc_status_id = stt.status_id) or (fl.is_completed = 1 and stt.status_id = 'hoanthanh'))
	";

                            if (WhereSQL.Trim() != "")
                            {
                                sql += " where " + WhereSQL;
                            }
                            string OFFSET = @"(" + pageno + @") * (" + pagesize + @")";
                            sql += @" ORDER BY " + order_by
                                + @" OFFSET " + OFFSET + " ROWS FETCH NEXT " + pagesize + " ROWS ONLY ";
                            sql = sql.Replace("--", "").Replace("\r", " ").Replace("   ", " ").Trim();
                            sql = Regex.Replace(sql, "drop", "");
                            sql = Regex.Replace(sql, "update", "");
                            sql = Regex.Replace(sql, "delete", "");
                            sql = Regex.Replace(sql, @"\s+", " ").Trim();
                            var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                            var tables = await task;
                            string JSONresult = JsonConvert.SerializeObject(tables);
                            return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { sql, err = "2", ms = "Bạn không có quyền truy cập chức năng này" });
                        }
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                        domainurl + "/DocMain/FilterDoc_Store", ip, tid, "Lỗi khi gọi FilterDoc_Store", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                       domainurl + "/DocMain/FilterDoc_Store", ip, tid, "Lỗi khi gọi FilterDoc_Store", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Count_FilterDoc_Store([FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                int organization_id = data["organization_id"].ToObject<int>();
                int user_key = data["user_key"].ToObject<int>();
                string search = data["search"].ToObject<string>();
                List<FieldSQL> fields = data["fields"].ToObject<List<FieldSQL>>();

                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                string sql = "";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var usernow = db.sys_users.Find(uid);
                        if (usernow != null)
                        {
                            string WhereSQL = "";

                            if (usernow.is_super == true)
                            {
                                WhereSQL += "";
                            }
                            WhereSQL += @"do.organization_id = " + organization_id + @" and do.is_drafted = 0 ";
                            //serch
                            if (!string.IsNullOrWhiteSpace(search))
                            {
                                WhereSQL += (WhereSQL.Trim() != "" ? " and " : " ") + "(" + @" CONTAINS(do.compendium,'""*" + search + @"*""')" + "or" + "(do.dispatch_book_num like N'%" + search + "%')" + "or" + @" CONTAINS(do.doc_code,'""*" + search + @"*""')" + "or" + @" CONTAINS(do.issue_place,'""*" + search + @"*""')" + ")";
                            }
                            //filed
                            if (fields != null && fields.Count > 0)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.filteroperator == "in")
                                    {
                                        WhereSQL += (WhereSQL != "" ? " and " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => (field.type_of == 1 ? "N" : "") + "'" + a.value + "'").ToList()) + ")";
                                    }
                                    else
                                    {
                                        foreach (var m in field.filterconstraints.Where(a => a.value != null))
                                        {
                                            switch (m.matchMode)
                                            {
                                                case "contains":
                                                    WhereSQL += " " + field.filteroperator + " (N'" + m.value + "' like N'%' + do." + field.key + " + ',%')";
                                                    break;
                                                case "containsMany":
                                                    List<string> listKey = m.value.Split(',').ToList();
                                                    WhereSQL += " " + field.filteroperator + " (";
                                                    foreach (var str in listKey)
                                                    {
                                                        if (str.Trim() != "")
                                                        {
                                                            WhereSQL += " (" + field.key + " like N'%" + str + "%')  or";
                                                        }
                                                    }
                                                    if (WhereSQL.EndsWith(" or"))
                                                    {
                                                        WhereSQL = WhereSQL.Substring(0, WhereSQL.Length - 3);
                                                    }
                                                    WhereSQL += ")";
                                                    break;
                                                case "equals":
                                                    WhereSQL += " " + field.filteroperator + " do." + field.key + " = N'" + m.value + "'";
                                                    break;
                                                case "dateBefore":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                                    break;
                                                case "dateAfter":
                                                    WhereSQL += " " + field.filteroperator + " CAST(do." + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                                    break;

                                            }
                                        }
                                    }
                                }
                            }
                            //Cut
                            if (WhereSQL.StartsWith(" and "))
                            {
                                WhereSQL = WhereSQL.Substring(4);
                            }
                            else if (WhereSQL.StartsWith(" or "))
                            {
                                WhereSQL = WhereSQL.Substring(3);
                            }
                            //Select
                            sql = @" 
                                       select do.doc_master_id, fl.[message],compendium, doc_code, nav_type, doc_date, fl.send_by,fl.send_by_name,fl.send_date,fl.follow_id,us.avatar,
	stt.status_id, stt.status_name, stt.background_color, stt.text_color, stt.is_handle,case when fl.deadline_date is not null then DATEDIFF(DAY,getDate(),fl.deadline_date) else null end as date_deadline,
	(select top 1 view_id from doc_views where doc_master_id = do.doc_master_id and [user_id] = " + user_key + @") as view_id,
	(select count(*) from task_linkdoc ld where ld.doc_master_id = do.doc_master_id) as countTask,gr.type_group
	into #Doc
	from doc_master do left join doc_follows fl on do.follow_id = fl.follow_id
	left join sys_users us on ISNULL(fl.send_by, do.created_by) = us.user_key
    left join doc_ca_groups gr on do.doc_group_id = gr.doc_group_id
	left join doc_ca_status stt on ((fl.is_completed is null and fl.doc_status_id = stt.status_id) or (fl.is_completed = 1 and stt.status_id = 'hoanthanh'))
	";

                            if (WhereSQL.Trim() != "")
                            {
                                sql += " where " + WhereSQL;
                            }
                            sql += @"
                                Declare @handle int,@ood int, @receive int, @send int, @internal int, @notseen int, @all int, @reltask int, @invite int, @insurance int
		                        set @all = (select count(*) from #Doc)
		                        set @receive = (select count(*) from #Doc where nav_type = 1 and type_group is null)
		                        set @send = (select count(*) from #Doc where nav_type = 2 and type_group is null)
		                        set @internal = (select count(*) from #Doc where nav_type = 3 and type_group is null)
		                        SET @handle=(Select count(*) from #Doc where is_handle=1)
		                        SET @notseen=(Select count(*) from #Doc where view_id is null)
		                        SET @ood=(Select count(*) from #Doc where date_deadline is not null and date_deadline <0 and status_id not in ('hoanthanh','phanphat','dadongdau'))
		                        SET @reltask = (Select count(*) from #Doc where countTask > 0)
                                SET @invite = (Select count(*) from #Doc where type_group = 0)
		                        SET @insurance = (Select count(*) from #Doc where type_group = 1)
		                        Select @all as [all], @receive as [receive], @send [send],@internal internal,@handle as handle,@notseen notseen, @ood as ood, @reltask as reltask, @invite as invite, @insurance as insurance
                            ";
                            sql = sql.Replace("--", "").Replace("\r", " ").Replace("   ", " ").Trim();
                            sql = Regex.Replace(sql, "drop", "");
                            sql = Regex.Replace(sql, "update", "");
                            sql = Regex.Replace(sql, "delete", "");
                            sql = Regex.Replace(sql, @"\s+", " ").Trim();
                            var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                            var tables = await task;
                            string JSONresult = JsonConvert.SerializeObject(tables);
                            return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { sql, err = "2", ms = "Bạn không có quyền truy cập chức năng này" });
                        }
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                        domainurl + "/DocMain/Count_FilterDoc_Store", ip, tid, "Lỗi khi gọi Count_FilterDoc_Store", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                       domainurl + "/DocMain/Count_FilterDoc_Store", ip, tid, "Lỗi khi gọi Count_FilterDoc_Store", 0, "SQL");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
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