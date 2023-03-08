using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Models;
using Helper;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using static System.Web.Razor.Parser.SyntaxConstants;
using static iTextSharp.text.pdf.AcroFields;

namespace API.Controllers.Doc
{
    [Authorize(Roles = "login")]
    public class doc_codesController : ApiController
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
            public async Task<HttpResponseMessage> add_doc_codes()
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
                        int dvid = int.Parse(claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value);
                        bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                    var doc_Codes_Check = await db.doc_codes.AsNoTracking().Where(s =>s.organization_id== dvid).ToListAsync<doc_codes>();
               var doc_Codes = new doc_codes();
                    doc_Codes.is_used = false;
                    doc_Codes.separator = "/";
                    doc_Codes.auto_gen = true;
                    doc_Codes.num_by_group = false;
                    doc_Codes.organization_id = dvid;
                    doc_Codes.created_date = DateTime.Now;
                    doc_Codes.created_by = uid;
                    doc_Codes.created_token_id = tid;
                    doc_Codes.created_ip = ip;
                    doc_Codes.modified_by = uid;
                    doc_Codes.modified_date = DateTime.Now;
                    doc_Codes.modified_ip = ip;
                    doc_Codes.modified_token_id = tid;
                    if (doc_Codes_Check.Count == 0)
                    {
                        for (int i = 1; i < 4; i++)
                        {
                            doc_Codes.info_col = "Số văn bản";
                            doc_Codes.idkey = "Sovanban";
                            doc_Codes.is_order = 1;
                            doc_Codes.nav_type = i;
                            db.doc_codes.Add(doc_Codes);
                            db.SaveChanges();
                        }
                        for (int i = 1; i < 4; i++)
                        {
                            doc_Codes.info_col = "Năm";
                            doc_Codes.idkey = "Nam";
                            doc_Codes.is_order = 2;
                            doc_Codes.nav_type = i;
                            db.doc_codes.Add(doc_Codes);
                            db.SaveChanges();
                        }
                        for (int i = 1; i < 4; i++)
                        {
                            doc_Codes.info_col = "Nhóm văn bản";
                            doc_Codes.idkey = "Nhomvanban";
                            doc_Codes.is_order = 3;
                            doc_Codes.nav_type = i;
                            db.doc_codes.Add(doc_Codes);
                            db.SaveChanges();
                        }
                        for (int i = 1; i < 4; i++)
                        {
                            doc_Codes.info_col = "Mã phòng ban";
                            doc_Codes.idkey = "Maphongban";
                            doc_Codes.is_order = 4;
                            doc_Codes.nav_type = i;
                            db.doc_codes.Add(doc_Codes);
                            db.SaveChanges();
                        }
                        for (int i = 1; i < 4; i++)
                        {
                            doc_Codes.info_col = "Mã công ty";
                            doc_Codes.idkey = "Macongty";
                            doc_Codes.is_order = 5;
                            doc_Codes.nav_type = i;
                            db.doc_codes.Add(doc_Codes);
                            db.SaveChanges();
                        }
                        for (int i = 1; i < 4; i++)
                        {
                            doc_Codes.info_col = "Mã khối cơ quan";
                            doc_Codes.idkey = "Makhoi";
                            doc_Codes.is_order = 6;
                            doc_Codes.nav_type = i;
                            db.doc_codes.Add(doc_Codes);
                            db.SaveChanges();
                        }
                    }
                        await db.SaveChangesAsync();
                   
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_codes/add_doc_codes", ip, tid, "Lỗi khi thêm số hiệu văn bản", 0, "device_codes");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_codes/add_doc_codes", ip, tid, "Lỗi khi thêm số hiệu văn bản", 0, "device_codes");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        [HttpPost]
        public async Task<HttpResponseMessage> update_doc_codes()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fddoc_code = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
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
                        fddoc_code = provider.FormData.GetValues("doc_code").SingleOrDefault();
                        doc_codes_master doc_code = JsonConvert.DeserializeObject<doc_codes_master>(fddoc_code);

                        if(String.IsNullOrEmpty(doc_code.code_master_id))
                        {
                            doc_code.code_master_id = helper.GenKey();
                            var total = db.doc_codes_master.AsNoTracking().Where(x => x.organization_id == doc_code.organization_id && x.nav_type == doc_code.nav_type).ToList();
                            var stt = 1;
                            if (total.Count() > 0)
                            {
                                stt = total.Max(x => x.is_order) + 1;
                            }
                            doc_code.is_order = stt;
                            doc_code.created_by = uid;
                            doc_code.created_date = DateTime.Now;
                            doc_code.created_ip = ip;
                            doc_code.created_token_id = tid;
                            db.doc_codes_master.Add(doc_code);
                        }
                        else
                        {
                            doc_code.modified_by = uid;
                            doc_code.modified_date = DateTime.Now;
                            doc_code.modified_ip = ip;
                            doc_code.modified_token_id = tid;
                            db.Entry(doc_code).State = EntityState.Modified;
                        }

                        if (doc_code.is_default)
                        {
                            var same_default = db.doc_codes_master.FirstOrDefault(x => x.organization_id == doc_code.organization_id && x.nav_type == doc_code.nav_type && x.is_default && x.code_master_id != doc_code.code_master_id);
                            if(same_default != null)
                            {
                                same_default.is_default = false;
                            }
                        }

                        var fd_details = provider.FormData.GetValues("details").SingleOrDefault();
                        List<doc_codes> details = JsonConvert.DeserializeObject<List<doc_codes>>(fd_details);

                        var old_details = db.doc_codes.Where(x => x.code_master_id == doc_code.code_master_id && x.organization_id == doc_code.organization_id).ToList();
                        if (old_details.Count() > 0) db.doc_codes.RemoveRange(old_details);

                        foreach(var item in details)
                        {
                            item.organization_id = doc_code.organization_id;
                            item.code_master_id = doc_code.code_master_id;
                            item.auto_gen = doc_code.auto_gen;
                            item.nav_type = doc_code.nav_type;
                            item.num_by_group = doc_code.num_by_group;
                            item.created_by = uid;
                            item.created_date = DateTime.Now;
                            item.created_ip = ip;
                            item.created_token_id = tid;

                            db.doc_codes.Add(item);
                        }

                        var old_use = db.doc_codes_use.Where(x => x.code_master_id == doc_code.code_master_id && x.organization_id == doc_code.organization_id).ToList();
                        if (old_use.Count() > 0) db.doc_codes_use.RemoveRange(old_use);

                        if (provider.FormData.GetValues("groups") != null)
                        {
                            var fd_groups = provider.FormData.GetValues("groups").SingleOrDefault();
                            List<int> group_ids = JsonConvert.DeserializeObject<List<int>>(fd_groups);

                            foreach(var gr_id in group_ids)
                            {
                                var new_gr = new doc_codes_use();
                                new_gr.code_master_id = doc_code.code_master_id;
                                new_gr.organization_id = doc_code.organization_id;
                                new_gr.doc_group_id = gr_id;
                                db.doc_codes_use.Add(new_gr);

                                var same_gr = db.doc_codes_use.FirstOrDefault(x => x.organization_id == doc_code.organization_id && x.code_master_id != doc_code.code_master_id && x.doc_group_id == gr_id);
                                if (same_gr != null)
                                {
                                    db.doc_codes_use.Remove(same_gr);
                                }
                            }
                        }

                        if (provider.FormData.GetValues("dispatch_books") != null)
                        {
                            var fd_dispatch_books = provider.FormData.GetValues("dispatch_books").SingleOrDefault();
                            List<int> dispatch_book_ids = JsonConvert.DeserializeObject<List<int>>(fd_dispatch_books);

                            foreach (var dp_id in dispatch_book_ids)
                            {
                                var new_dp = new doc_codes_use();
                                new_dp.code_master_id = doc_code.code_master_id;
                                new_dp.organization_id = doc_code.organization_id;
                                new_dp.dispatch_book_id = dp_id;
                                db.doc_codes_use.Add(new_dp);

                                var same_dp = db.doc_codes_use.FirstOrDefault(x => x.organization_id == doc_code.organization_id && x.code_master_id != doc_code.code_master_id && x.dispatch_book_id == dp_id);
                                if (same_dp != null)
                                {
                                    db.doc_codes_use.Remove(same_dp);
                                }
                            }
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fddoc_code, contents }), domainurl + "doc_codes/update_doc_codes", ip, tid, "Lỗi khi cập nhật doc_codes", 0, "doc_codes");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fddoc_code, contents }), domainurl + "doc_codes/update_doc_codes", ip, tid, "Lỗi khi cập nhật doc_codes", 0, "doc_codes");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> delete_doc_codes([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> ids)
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
                        foreach(var id in ids)
                        {
                            var del = await db.doc_codes_master.FindAsync(id);
                            if (del != null) db.doc_codes_master.Remove(del);
                        }

                        await db.SaveChangesAsync();

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "doc_codes/delete_doc_codes", ip, tid, "Lỗi khi xoá cấu hình", 0, "doc_codes");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "doc_codes/delete_doc_codes", ip, tid, "Lỗi khi xoá cấu hình", 0, "doc_codes");
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
    }
    }