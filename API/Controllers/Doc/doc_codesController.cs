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

namespace API.Controllers.Doc
{
    [Authorize]
    public class doc_codesController : ApiController
    {


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
        [HttpPut]
        public async Task<HttpResponseMessage> update_doc_codes(List<doc_codes> lidoc_Codes)
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
                    foreach (var item in lidoc_Codes)
                    {
                        item.modified_by = uid;
                        item.modified_date = DateTime.Now;
                        item.modified_ip = ip;
                        item.modified_token_id = tid;
                        db.Entry(item).State = EntityState.Modified;
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "doc_codes/update_doc_codes", ip, tid, "Lỗi khi cập nhật doc_codes", 0, "doc_codes");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "doc_codes/update_doc_codes", ip, tid, "Lỗi khi cập nhật doc_codes", 0, "doc_codes");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

    }
    }