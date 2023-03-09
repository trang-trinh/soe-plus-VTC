using API.Models;
using Helper;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace API.Controllers.Doc
{
    public class DocConnectController : ApiController
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
        private void saveXML(string root, List<string> column_names, List<int> doc_master_ids, string save_dir_path)
        {
            string xml_result = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>";
            // Format root
            string rootCheck = Regex.Replace(root.Replace("\\", "/"), @"\.*/+", "/");
            var listPath = rootCheck.Split('/');
            var rootConfig = "";
            var sttPartPath = 1;
            foreach (var item in listPath)
            {
                if (item.Trim() != "")
                {
                    if (sttPartPath == 1)
                    {
                        rootConfig += (item);
                    }
                    else
                    {
                        rootConfig += "/" + Path.GetFileName(item);
                    }
                }
                sttPartPath++;
            }

            // format save_dir_path
            string pathCheck = Regex.Replace(save_dir_path.Replace("\\", "/"), @"\.*/+", "/");
            var listPathCheck = pathCheck.Split('/');
            var pathConfig = "";
            foreach (var item in listPathCheck)
            {
                if (item.Trim() != "")
                {
                    pathConfig += "/" + Path.GetFileName(item);
                }
            }

            using (DBEntities db = new DBEntities())
            {
                foreach(int id in doc_master_ids)
                {
                    var doc = db.doc_master.Find(id);
                    xml_result += "<document><document-id>" + doc.doc_master_guid + "</document-id>";
                    foreach(var col_name in column_names)
                    {
                        var column_value = string.Empty;
                        if(doc.GetType().GetProperty(col_name).GetValue(doc, null) != null)
                        {
                            column_value = doc.GetType().GetProperty(col_name).GetValue(doc, null).ToString();
                            DateTime dt;
                            if (DateTime.TryParse(column_value, out dt))
                            {
                                column_value = DateTime.Parse(column_value).ToShortDateString();
                            }
                        }
                        xml_result += @"<" + col_name + @">" + column_value + @"</" + col_name + @">";
                    }
                    xml_result += "<file-name>" + doc.file_name + "</file-name>";

                    // format doc.file_path
                    string pathCheck_1 = Regex.Replace(doc.file_path.Replace("\\", "/"), @"\.*/+", "/");
                    var listPathCheck_1 = pathCheck_1.Split('/');
                    var pathDoc_Config = "";
                    foreach (var item in listPathCheck_1)
                    {
                        if (item.Trim() != "")
                        {
                            pathDoc_Config += "/" + Path.GetFileName(item);
                        }
                    }
                    Byte[] bytes_doc = File.ReadAllBytes(rootConfig + pathDoc_Config);

                    String file_doc = Convert.ToBase64String(bytes_doc);
                    xml_result += "<file-data>" + file_doc + "</file-data>";
                    var files = db.doc_files.Where(x => x.doc_master_id == id && x.doc_file_type != 4).ToList();
                    if (files.Count() > 0)
                    {
                        xml_result += "<attach-files>";
                        foreach(var fi in files)
                        {
                            xml_result += "<attach-file>";
                            xml_result += "<file-id>" + fi.file_id + "</file-id>";
                            xml_result += "<file-name>" + fi.file_name + "</file-name>";

                            // format fi.file_path
                            string pathCheck_2 = Regex.Replace(fi.file_path.Replace("\\", "/"), @"\.*/+", "/");
                            var listPathCheck_2 = pathCheck_2.Split('/');
                            var pathDoc_Config_fi = "";
                            foreach (var item in listPathCheck_2)
                            {
                                if (item.Trim() != "")
                                {
                                    pathDoc_Config_fi += "/" + Path.GetFileName(item);
                                }
                            }
                            Byte[] bytes = File.ReadAllBytes(rootConfig + pathDoc_Config_fi);

                            String file = Convert.ToBase64String(bytes);
                            xml_result += "<file-data>" + file + "</file-data>";
                            xml_result += "</attach-file>";
                        }
                        xml_result += "</attach-files>";
                    }
                    xml_result += "</document>";

                    // format doc.doc_master_guid
                    string pathCheck_3 = Regex.Replace(doc.doc_master_guid.Replace("\\", "/"), @"\.*/+", "/");
                    var listPathCheck_3 = pathCheck_3.Split('/');
                    var pathDocMaster_Config = "";
                    foreach (var item in listPathCheck_3)
                    {
                        if (item.Trim() != "")
                        {
                            pathDocMaster_Config += "/" + Path.GetFileName(item);
                        }
                    }
                    File.WriteAllText(rootConfig + pathConfig + pathDocMaster_Config + ".xml", xml_result);
                }
            };
        }
        private void saveJSON(string root, List<string> column_names, List<int> doc_master_ids, string save_dir_path)
        {
            // Format root
            string rootCheck = Regex.Replace(root.Replace("\\", "/"), @"\.*/+", "/");
            var listPath = rootCheck.Split('/');
            var rootConfig = "";
            var sttPartPath = 1;
            foreach (var item in listPath)
            {
                if (item.Trim() != "")
                {
                    if (sttPartPath == 1)
                    {
                        rootConfig += (item);
                    }
                    else
                    {
                        rootConfig += "/" + Path.GetFileName(item);
                    }
                }
                sttPartPath++;
            }

            // format save_dir_path
            string pathCheck = Regex.Replace(save_dir_path.Replace("\\", "/"), @"\.*/+", "/");
            var listPathCheck = pathCheck.Split('/');
            var pathConfig = "";
            foreach (var item in listPathCheck)
            {
                if (item.Trim() != "")
                {
                    pathConfig += "/" + Path.GetFileName(item);
                }
            }

            string json_result = @"{";
            using (DBEntities db = new DBEntities())
            {
                foreach (int id in doc_master_ids)
                {
                    var doc = db.doc_master.Find(id);
                    json_result += @"""document"": {""document_id"":" + @"""" + doc.doc_master_guid + @""",";
                    foreach (var col_name in column_names)
                    {
                        var column_value = string.Empty;
                        if (doc.GetType().GetProperty(col_name).GetValue(doc, null) != null)
                        {
                            column_value = doc.GetType().GetProperty(col_name).GetValue(doc, null).ToString();
                            DateTime dt;
                            if (DateTime.TryParse(column_value, out dt))
                            {
                                column_value = DateTime.Parse(column_value).ToShortDateString();
                            }
                        }
                        json_result += @"""" + col_name + @""": """ + column_value + @""",";
                    }
                    json_result += @"""file_name"":""" + doc.file_name + @""",";

                    // format doc.file_path
                    string pathCheck_1 = Regex.Replace(doc.file_path.Replace("\\", "/"), @"\.*/+", "/");
                    var listPathCheck_1 = pathCheck_1.Split('/');
                    var pathDoc_Config = "";
                    foreach (var item in listPathCheck_1)
                    {
                        if (item.Trim() != "")
                        {
                            pathDoc_Config += "/" + Path.GetFileName(item);
                        }
                    }
                    Byte[] bytes_doc = File.ReadAllBytes(rootConfig + pathDoc_Config);
                    String file_doc = Convert.ToBase64String(bytes_doc);
                    json_result += @"""file_data"":""" + file_doc + @""",";
                    json_result = json_result.Remove(json_result.Length - 1);
                    json_result += "}";
                    var files = db.doc_files.Where(x => x.doc_master_id == id && x.doc_file_type != 4).ToList();
                    if (files.Count() > 0)
                    {
                        json_result += @",""attach_files"":[";
                        foreach (var fi in files)
                        {
                            json_result += "{";
                            json_result += @"""file_id"": """ + fi.file_id + @""",";
                            json_result += @"""file_name"": """ + fi.file_name + @""",";

                            // format fi.file_path
                            string pathCheck_2 = Regex.Replace(fi.file_path.Replace("\\", "/"), @"\.*/+", "/");
                            var listPathCheck_2 = pathCheck_2.Split('/');
                            var pathDoc_Config_fi = "";
                            foreach (var item in listPathCheck_2)
                            {
                                if (item.Trim() != "")
                                {
                                    pathDoc_Config_fi += "/" + Path.GetFileName(item);
                                }
                            }
                            Byte[] bytes = File.ReadAllBytes(rootConfig + pathDoc_Config_fi);
                            String file = Convert.ToBase64String(bytes);
                            json_result += @"""file_data"":""" + file + @"""";
                            json_result += "},";
                        }
                        json_result = json_result.Remove(json_result.Length - 1);
                        json_result += "]";
                    }
                    json_result += "}";

                    // format doc.doc_master_guid
                    string pathCheck_3 = Regex.Replace(doc.doc_master_guid.Replace("\\", "/"), @"\.*/+", "/");
                    var listPathCheck_3 = pathCheck_3.Split('/');
                    var pathDocMaster_Config = "";
                    foreach (var item in listPathCheck_3)
                    {
                        if (item.Trim() != "")
                        {
                            pathDocMaster_Config += "/" + Path.GetFileName(item);
                        }
                    }
                    File.WriteAllText(rootConfig + pathConfig + pathDocMaster_Config + ".json", json_result);
                }
            };
        }
        public async Task<HttpResponseMessage> Send_Doc()
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
                        var fd_connect_orgs = provider.FormData.GetValues("connect_orgs").SingleOrDefault();
                        List<string> connect_orgs_ids = JsonConvert.DeserializeObject<List<string>>(fd_connect_orgs);

                        var fd_send_docs = provider.FormData.GetValues("send_docs").SingleOrDefault();
                        List<int> doc_master_ids = JsonConvert.DeserializeObject<List<int>>(fd_send_docs);

                        string fd_type_export = provider.FormData.GetValues("type_export").SingleOrDefault();
                        int type_export = int.Parse(fd_type_export);
                        foreach (var organization_connect_id in connect_orgs_ids)
                        {
                            var checked_doc_ids = new List<int>();
                            string dir_path = "/Portals/" + organization_id_user + "/Doc/ConnectSend/" + organization_connect_id + "/";

                            // format dir_path
                            string checkPathFile = Regex.Replace(dir_path.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = checkPathFile.Split('/');
                            var config_dir_path = "";
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    config_dir_path += "/" + Path.GetFileName(item);
                                }
                            }

                            bool exists_dir = Directory.Exists(root + config_dir_path);
                            if (!exists_dir)
                                Directory.CreateDirectory(root + config_dir_path);
                            foreach (var id in doc_master_ids)
                            {
                                var exists = db.doc_connect.FirstOrDefault(x => x.doc_master_id == id && x.organization_connect_id == organization_connect_id && x.is_type == 0);
                                if(exists == null)
                                {
                                    checked_doc_ids.Add(id);
                                    var new_doc_send = new doc_connect();
                                    new_doc_send.organization_connect_id = organization_connect_id;
                                    new_doc_send.doc_master_id = id;
                                    new_doc_send.status = false;
                                    new_doc_send.is_type = 0;
                                    new_doc_send.type_export = type_export;
                                    new_doc_send.created_by = uid;
                                    new_doc_send.created_date = DateTime.Now;
                                    new_doc_send.created_ip = ip;
                                    new_doc_send.created_token_id = tid;
                                    db.doc_connect.Add(new_doc_send);
                                }
                            }
                            List<string> convert_col_names = new List<string>() { "doc_code", "doc_date", "receive_date", "urgency", "doc_group", "issue_place", "compendium", "signer", "field_name", "security" };
                            if (type_export == 0)
                            {
                                saveXML(root, convert_col_names, checked_doc_ids, dir_path);
                            }
                            else if(type_export == 1)
                            {
                                saveJSON(root, convert_col_names, checked_doc_ids, dir_path);
                            }
                        }
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0"});
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocConnect/Send_Doc", ip, tid, "Lỗi khi gửi văn bản sang trục liên thông", 0, "DocConnect");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocConnect/Send_Doc", ip, tid, "Lỗi khi gửi văn bản sang trục liên thông", 0, "DocConnect");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        public async Task<HttpResponseMessage> Update_Status_Send(string token_key, [System.Web.Mvc.Bind(Include = "")][FromBody] bool status)
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
                        var org = db.doc_organization_connect.FirstOrDefault(x => x.connect_token_key == token_key);
                        if(org != null)
                        {
                            var update_docs = db.doc_connect.Where(x => x.organization_connect_id == org.organization_connect_id && x.is_type == 0 && x.status == false);
                            update_docs.ToList().ForEach(s => s.status = status);
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocConnect/Update_Status_Send", ip, tid, "Lỗi khi cập nhật trạng thái văn bản gửi sang trục liên thông", 0, "DocConnect");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocConnect/Update_Status_Send", ip, tid, "Lỗi khi cập nhật trạng thái văn bản gửi sang trục liên thông", 0, "DocConnect");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        public async Task<HttpResponseMessage> Delete_All_Send(string token_key)
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
                        var org = db.doc_organization_connect.FirstOrDefault(x => x.connect_token_key == token_key);
                        if (org != null)
                        {
                            var delete_docs = db.doc_connect.Where(x => x.organization_connect_id == org.organization_connect_id && x.is_type == 0);
                            if (delete_docs.Count() > 0) db.doc_connect.RemoveRange(delete_docs);
                        }
                        else return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Không tìm thấy đơn vị trục liên thông!" });
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocConnect/Delete_All_Send", ip, tid, "Lỗi khi xoá văn bản gửi sang trục liên thông", 0, "DocConnect");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocConnect/Delete_All_Send", ip, tid, "Lỗi khi xoá văn bản gửi sang trục liên thông", 0, "DocConnect");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        public async Task<HttpResponseMessage> Delete_Send(string token_key, [System.Web.Mvc.Bind(Include = "")][FromBody] List<string> doc_ids)
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
                        var org = db.doc_organization_connect.FirstOrDefault(x => x.connect_token_key == token_key);
                        if (org != null)
                        {
                            var vbs = db.doc_master.Where(x => doc_ids.Contains(x.doc_master_guid));
                            if (vbs.Count() > 0)
                            {
                                var id_int_vb = vbs.Select(x => x.doc_master_id);
                                var delete_docs = db.doc_connect.Where(x => x.organization_connect_id == org.organization_connect_id && id_int_vb.Contains(x.doc_master_id ?? 0) && x.is_type == 0);
                                if (delete_docs.Count() > 0) db.doc_connect.RemoveRange(delete_docs);
                            }
                           
                        }
                        else return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms="Không tìm thấy đơn vị trục liên thông!" });
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocConnect/Delete_Send", ip, tid, "Lỗi khi xoá văn bản gửi sang trục liên thông", 0, "DocConnect");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "DocConnect/Delete_Send", ip, tid, "Lỗi khi xoá văn bản gửi sang trục liên thông", 0, "DocConnect");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

    }
}
