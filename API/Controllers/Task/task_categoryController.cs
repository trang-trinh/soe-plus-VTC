using API.Helper;
using API.Models;
using Helper;
using Newtonsoft.Json;
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

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class task_categoryController : ApiController
    {
        public string getipaddress()
        {
      return  HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add_Task_Category([System.Web.Mvc.Bind(Include = "created_by,created_date,created_ip,created_token_id")] task_category tsk)
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
                using (DBEntities db = new DBEntities())
                {
                    tsk.created_by = uid;
                    tsk.created_date = DateTime.Now;
                    tsk.created_ip = ip;
                    tsk.created_token_id = tid;
                    db.task_category.Add(tsk);
                    await db.SaveChangesAsync();

                  
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Task_Category/Add_Task_Category", ip, tid, "Lỗi khi thêm nhóm công việc", 0, "Task_Category");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Task_Category/Add_Task_Category", ip, tid, "Lỗi khi thêm nhóm công việc", 0, "Task_Category");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_Task_Category([System.Web.Mvc.Bind(Include = "created_by,created_date,created_ip,created_token_id")] task_category tsk)
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
                using (DBEntities db = new DBEntities())
                {
                    tsk.created_by = uid;
                    tsk.created_date = DateTime.Now;
                    tsk.created_ip = ip;
                    tsk.created_token_id = tid;
                    db.Entry(tsk).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                   
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Task_Category/Add_Task_Category", ip, tid, "Lỗi khi cập nhật nhóm công việc", 0, "Task_Category");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Task_Category/Add_Task_Category", ip, tid, "Lỗi khi cập nhật nhóm công việc", 0, "Task_Category");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        public List<int> deleteChild(List<int> id)
        {
            List<int> del = new List<int>();
            using (DBEntities db = new DBEntities())
            {

                var das = db.task_category.Where(a => id.Contains(a.category_id)).ToArray();

                if (das != null)
                {


                    foreach (var da in das)
                    {
                        var arrC = db.task_category.Where(a => a.parent_id != null).ToArray();
                        del.Add(da.category_id);
                        var arrId = new List<int>();
                        for (int i = 0; i < id.Count; i++)
                        {
                            for (int j = 0; j < arrC.Length; j++)
                            {
                                if (id[i] == arrC[j].parent_id)
                                {

                                    arrId.Add(arrC[j].category_id);
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
        public async Task<HttpResponseMessage> Delete_Task_Category([System.Web.Mvc.Bind(Include = "")]  List<int> id)
        {
            var identity = User.Identity as ClaimsIdentity;
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

                        
                            var arrC = deleteChild(id);
                            var del = await db.task_category.Where(a => arrC.Contains(a.category_id)).ToListAsync();

                            if (del.Count == 0)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                            }

                            db.task_category.RemoveRange(del);
                        await db.SaveChangesAsync();


                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Task_Category/Delete_Task_Category", ip, tid, "Lỗi khi xoá nhóm công việc", 0, "Task_Category");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Task_Category/Delete_Task_Category", ip, tid, "Lỗi khi xoá nhóm công việc", 0, "Task_Category");
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

