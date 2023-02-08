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

namespace API.Controllers.Device
{
    [Authorize(Roles = "login")]
    public class device_groupsController : ApiController
    {
        public string getipaddress()
        {
         return  HttpContext.Current.Request.UserHostAddress;
        }
 
       
        [HttpPost]
        public async Task<HttpResponseMessage> add_device_groups([System.Web.Mvc.Bind(Include = "organization_id,created_date,created_by,created_token_id,created_ip,modified_by,modified_date,modified_ip,modified_token_id,device_groups_id")] device_groups device_Groups)
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
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
          
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                    bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                    device_Groups.organization_id = super ? 0 : int.Parse(dvid);
                    device_Groups.created_date = DateTime.Now;
                    device_Groups.created_by = uid;
                    device_Groups.created_token_id = tid;
                    device_Groups.created_ip = ip;
                    device_Groups.modified_by = uid;
                    device_Groups.modified_date = DateTime.Now;
                    device_Groups.modified_ip = ip;
                    device_Groups.modified_token_id = tid;
                    db.device_groups.Add(device_Groups);
                    await db.SaveChangesAsync();
                    #region add device_log
                    if (helper.wlog)
                    {

                        device_log log = new device_log();
                        log.title = "Thêm nhóm tài sản" + device_Groups.groups_name;

                        log.log_module = "device_groups";
                        log.log_type = 0;
                        log.id_key = device_Groups.device_groups_id.ToString();
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.device_log.Add(log);
                        db.SaveChanges();


                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Groups/add_groups", ip, tid, "Lỗi khi thêm nhóm tài sản", 0, "device_Groups");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "device_Groups/add_groups", ip, tid, "Lỗi khi thêm nhóm tài sản", 0, "device_Groups");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_device_groups([System.Web.Mvc.Bind(Include = "modified_by,modified_date,modified_ip,modified_token_id,device_groups_id,groups_name")] device_groups device_Groups)
        {
             var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
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
                using (DBEntities db = new DBEntities())
                {
                    device_Groups.modified_by = uid;
                    device_Groups.modified_date = DateTime.Now;
                    device_Groups.modified_ip = ip;
                    device_Groups.modified_token_id = tid;
                    db.Entry(device_Groups).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    #region add device_log
                    if (helper.wlog)
                    {

                        device_log log = new device_log();
                        log.title = "Sửa nhóm tài sản" + device_Groups.groups_name;

                        log.log_module = "device_groups";
                        log.id_key = device_Groups.device_groups_id.ToString();
                        log.log_type = 1;
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.created_ip = ip;
                        db.device_log.Add(log);
                        db.SaveChanges();


                    }
                    #endregion
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Groups/update_groups", ip, tid, "Lỗi khi cập nhật nhóm tài sản", 0, "device_Groups");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdlang, contents }), domainurl + "device_Groups/update_groups", ip, tid, "Lỗi khi cập nhật nhóm tài sản", 0, "device_Groups");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


        public List<int> deleteChild(  List<int> id)
        {
            List<int> del = new List<int>();
            List<int> delO = new List<int>();
            using (DBEntities db = new DBEntities())
            {
                var das = db.device_groups.Where(a => id.Contains(a.device_groups_id)).ToArray();
                if (das != null)
                {
                    foreach (var da in das)
                    {
                        var device_main = db.device_main.AsNoTracking().Where(s => s.device_groups_id == da.device_groups_id).FirstOrDefault<device_main>();
                        if (device_main != null)
                        {
                            delO.Add(-1);
                            return delO;
                           
                        }
                        var arrC = db.device_groups.Where(a => a.parent_id != null).ToArray();
                        del.Add(da.device_groups_id);
                        var arrId = new List<int>();
                        for (int i = 0; i < id.Count; i++)
                        {
                            for (int j = 0; j < arrC.Length; j++)
                            {
                                if (id[i] == arrC[j].parent_id)
                                {

                                    arrId.Add(arrC[j].device_groups_id);
                                    del.AddRange(deleteChild(arrId));
                                }
                            }
                        }
                    }
                }
            }
            return del;
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> delete_device_groups([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = await db.device_groups.Where(a => id.Contains(a.device_groups_id)).ToListAsync();
                        List<string> paths = new List<string>();
                        if (das != null)
                        {
                            List<device_groups> del = new List<device_groups>();
                            foreach (var da in das)
                            {
                                if (uid == da.created_by || (int.Parse(dvid) == da.organization_id && ad) || super)
                                {
                                    var device_main = db.device_main.AsNoTracking().Where(s => s.device_groups_id == da.device_groups_id).FirstOrDefault<device_main>();
                                    if (device_main != null)
                                    {
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Có tài sản đang dùng nhóm tài sản này!" });
                                    }
                                    var arrC = deleteChild(id);
                                    if (del.Count == 1 && arrC[0] == -1)
                                    {
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Cấp con của nhóm tài sản đang được sử dụng !" });
                                    }
                                        del.AddRange( await db.device_groups.Where(a => arrC.Contains(a.device_groups_id)).ToListAsync());

                                
                                    del.Add(da);
                                    #region add device_log
                                    if (helper.wlog)
                                    {

                                        device_log log = new device_log();
                                        log.title = "Xóa nhóm tài sản" + da.groups_name;

                                        log.log_module = "device_groups";
                                        log.id_key = da.device_groups_id.ToString();
                                        log.log_type = 2;
                                        log.created_date = DateTime.Now;
                                        log.created_by = uid;
                                        log.created_token_id = tid;
                                        log.created_ip = ip;
                                        db.device_log.Add(log);
                                        db.SaveChanges();


                                    }
                                    #endregion
                                }


                            }
                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }
                            db.device_groups.RemoveRange(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Groups/delete_groups", ip, tid, "Lỗi khi xoá nhóm tài sản", 0, "device_Groups");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "device_Groups/delete_groups", ip, tid, "Lỗi khi xoá nhóm tài sản", 0, "device_Groups");
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
        public async Task<HttpResponseMessage> update_s_device_groups([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
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
                        var das = db.device_groups.Where(a => (a.device_groups_id == int_id)).FirstOrDefault<device_groups>();
                        if (das != null)
                        {
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            das.status = !trangthai.BitTrangthai;

                            #region add device_log
                            if (helper.wlog)
                            {

                                device_log log = new device_log();
                                log.title = "Sửa nhóm tài sản" + das.groups_name;

                                log.log_module = "device_groups";
                                log.id_key = das.device_groups_id.ToString();
                                log.log_type = 1;
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.device_log.Add(log);
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_groups/update_s_device_groups", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_groups", 0, "device_groups");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "device_groups/update_s_device_groups", ip, tid, "Lỗi khi cập nhật trạng thái update_s_device_groups", 0, "device_groups");
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