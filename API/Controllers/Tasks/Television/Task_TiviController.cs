using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json.Linq;
using API.Helper;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace API.Controllers.Tasks.Television
{
    [Authorize(Roles = "login")]
    public class Task_TiviController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Count_Task([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            string proc_name = "Tivi_Task_Count";
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            List<sqlPar> lst_par = JsonConvert.DeserializeObject<List<sqlPar>>(des);
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                var sqlpas = new List<SqlParameter>();
                if (lst_par != null)
                {
                    foreach (sqlPar p in lst_par)
                    {
                        if (p.par == "fromDate" || p.par == "toDate")
                        {
                            DateTime? t1 = null;
                            if (p.va == null)
                            { sqlpas.Add(new SqlParameter("@" + p.par, t1)); }
                            else
                                sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                        }
                        else
                        {
                            sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                        }
                    }
                }
                var arrpas = sqlpas.ToArray();
                DateTime sdate = DateTime.Now;
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc_name, arrpas).Tables);
                var tables = await task;
                DateTime edate = DateTime.Now;
                #region add SQLLog
                if (helper.wlog)
                {
                    using (DBEntities db = new DBEntities())
                    {
                        sql_log log = new sql_log();
                        log.controller = domainurl + "Task_Tivi";
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
                        log.title = proc_name;
                        log.log_content = JsonConvert.SerializeObject(new { data = proc_name });
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc_name, contents }), domainurl + "TaskProc/getTaskData", ip, tid, "Lỗi khi gọi proc '" + proc_name + "'", 0, "TaskProc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc_name, contents }), domainurl + "TaskProc/getTaskData", ip, tid, "Lỗi khi gọi proc '" + proc_name + "'", 0, "TaskProc");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> List_Task([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            string proc_name = "tivi_task_list";
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            List<sqlPar> lst_par = JsonConvert.DeserializeObject<List<sqlPar>>(des);
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                var sqlpas = new List<SqlParameter>();
                if (lst_par != null)
                {
                    foreach (sqlPar p in lst_par)
                    {
                        sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                    }
                }

                var arrpas = sqlpas.ToArray();
                DateTime sdate = DateTime.Now;
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc_name, arrpas).Tables);
                var tables = await task;
                DateTime edate = DateTime.Now;
                #region add SQLLog
                if (helper.wlog)
                {
                    using (DBEntities db = new DBEntities())
                    {
                        sql_log log = new sql_log();
                        log.controller = domainurl + "Task_Television/List_task";
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
                        log.title = proc_name;
                        log.log_content = JsonConvert.SerializeObject(new { data = proc_name });
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc_name, contents }), domainurl + "TaskProc/getTaskData", ip, tid, "Lỗi khi gọi proc '" + proc_name + "'", 0, "TaskProc");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc_name, contents }), domainurl + "TaskProc/getTaskData", ip, tid, "Lỗi khi gọi proc '" + proc_name + "'", 0, "TaskProc");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);

                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
    }
}
