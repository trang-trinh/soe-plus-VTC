﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
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
using API.Helper;
using Newtonsoft.Json;
using Microsoft.ApplicationBlocks.Data;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class ca_tagsController : ApiController
    {


        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;

        }


        [HttpPost]
        public async Task<HttpResponseMessage> Add_tag([System.Web.Mvc.Bind(Include = "tag_id,tag_name,created_by,created_date,created_ip,created_token_id")] doc_ca_tags Tag)
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
                    Tag.created_by = uid;
                    Tag.created_date = DateTime.Now;
                    Tag.created_ip = ip;
                    Tag.created_token_id = tid;

                    db.doc_ca_tags.Add(Tag);
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {

                        cms_logs log = new cms_logs();
                        log.log_title = "Thêm nhãn " + Tag.tag_name;
                        log.log_content = JsonConvert.SerializeObject(new { data = Tag });
                        log.log_module = "Nhãn";
                        log.id_key = Tag.tag_id.ToString();
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tag/Add_Tag", ip, tid, "Lỗi khi thêm nhãn", 0, "Tag");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tag/Add_Tag", ip, tid, "Lỗi khi thêm nhãn", 0, "Tag");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update_tag([System.Web.Mvc.Bind(Include = "tag_id,tag_name,modified_by,modified_date,modified_ip,modified_token_id")] doc_ca_tags Tag)
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
                    Tag.modified_by = uid;
                    Tag.modified_date = DateTime.Now;
                    Tag.modified_ip = ip;
                    Tag.modified_token_id = tid;

                    db.Entry(Tag).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    #region add cms_logs
                    if (helper.wlog)
                    {

                        cms_logs log = new cms_logs();
                        log.log_title = "Sửa nhãn " + Tag.tag_name;
                        log.log_content = JsonConvert.SerializeObject(new { data = Tag });
                        log.log_module = "Nhãn";
                        log.id_key = Tag.tag_id.ToString();
                        log.modified_date = DateTime.Now;
                        log.modified_by = uid;
                        log.modified_token_id = tid;
                        log.modified_ip = ip;
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "Tag/Update_tag", ip, tid, "Lỗi khi cập nhật Nhãn", 0, "Tag");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "Tag/Update_tag", ip, tid, "Lỗi khi cập nhật Nhãn", 0, "Tag");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }



        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_tag([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

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
                    var das = await db.doc_ca_tags.Where(a => id.Contains(a.tag_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<doc_ca_tags> del = new List<doc_ca_tags>();
                        foreach (var da in das)
                        {

                            del.Add(da);


                            #region add cms_logs
                            if (helper.wlog)
                            {

                                cms_logs log = new cms_logs();
                                log.log_title = "Xóa nhãn" + da.tag_name;

                                log.log_module = "Nhãn";
                                log.id_key = da.tag_id.ToString();
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
                        db.doc_ca_tags.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Tag/Delete_tag", ip, tid, "Lỗi khi xoá nhãn", 0, "Tag");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Tag/Delete_tag", ip, tid, "Lỗi khi xoá nhãn", 0, "Tag");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Tag_Status([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

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
                    var das = db.doc_ca_tags.Where(a => (a.tag_id == trangthai.IntID)).FirstOrDefault<doc_ca_tags>();
                    if (das != null)
                    {

                        das.status = !trangthai.BitTrangthai;
                        #region add cms_logs
                        if (helper.wlog)
                        {

                            cms_logs log = new cms_logs();
                            log.log_title = "Sửa nhãn nhãn" + das.tag_name;

                            log.log_module = "Nhãn";
                            log.id_key = das.tag_id.ToString();
                            log.modified_date = DateTime.Now;
                            log.modified_by = uid;
                            log.modified_token_id = tid;
                            log.modified_ip = ip;
                            db.cms_logs.Add(log);
                            db.SaveChanges();

                        }
                        #endregion
                        das.modified_by = uid;
                        das.modified_date = DateTime.Now;
                        das.modified_ip = ip;
                        das.modified_token_id = tid;
                        await db.SaveChangesAsync();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "Tag/Update_Tag_Status", ip, tid, "Lỗi khi cập nhật nhãn Nhãn", 0, "Tag");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "Tag/Update_Tag_Status", ip, tid, "Lỗi khi cập nhật nhãn Nhãn", 0, "Tag");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

    }
}