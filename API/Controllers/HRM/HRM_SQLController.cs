using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;




namespace API.Controllers.HRM
{
    [Authorize(Roles = "login")]
    public class HRM_SQLController : ApiController
    {
        public string getipaddress()
        {

            return HttpContext.Current.Request.UserHostAddress;
        }
        #region CallProcSmartP

        [HttpPost]
        public async Task<HttpResponseMessage> PostProc([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);

            try
            {
                string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    if (proc.query == true) {
                        DateTime sdate = DateTime.Now;
 
                      
                         var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, CommandType.Text, proc.proc).Tables);
                        var tables = await task;
                        DateTime edate = DateTime.Now;

                        #region add SQLLog
                        if (helper.wlog)
                        {
                            using (DBEntities db = new DBEntities())
                            {
                                sql_log log = new sql_log();
                                log.controller = domainurl + "Proc/CallProc";
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
                        return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                    }
                    else { 
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
                            log.controller = domainurl + "Proc/CallProc";
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
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);

                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Proc/CallProc", ip, tid, "Lỗi khi gọi proc ", 0, "Proc");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Proc/CallProc", ip, tid, "Lỗi khi gọi proc ", 0, "Proc");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
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
        #endregion

        #region CallProc

        [HttpPost]
        public async Task<HttpResponseMessage> getData([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);

            try
            {
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
                            log.controller = domainurl + "Proc/CallProc";
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
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);

                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Proc/CallProc", ip, tid, "Lỗi khi gọi proc ", 0, "Proc");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Proc/CallProc", ip, tid, "Lỗi khi gọi proc ", 0, "Proc");
                    if (!helper.debug)
                    {
                        contents = helper.logCongtent;
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
        #endregion


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_hrm_training_emps([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }


            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = "  select count(*) as totalRecords from hrm_training_emps  hcal ";
            try
            {










                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";



                sql = selectStr + " hcal.*,su.full_name,su.avatar,(select distinct '[' + STUFF(( "
  + "   SELECT ',{\"user_id\":\"' + cast(ISNULL(hcs.lecturers_id, '') as nvarchar(50)) + '\"' "
  + "  + ',\"full_name\":\"' + cast(ISNULL(hcs.lecturers_name, '') as nvarchar(50)) + '\"' "
 + "   + ',\"avatar\":\"' + cast(ISNULL(hcs.avatar, '') as nvarchar(200)) + '\"' "
  + "  + ',\"phone_number\":\"' + cast(ISNULL((hcs.phone_number), '') as nvarchar(500)) + '\"' + '}' "
 + "   FROM hrm_class_schedule hcs WHERE hcs.training_emps_id = hcal.training_emps_id  for xml path(''), type) "
 + "   .value('.', 'nvarchar(max)'), 1, 1, '') +']') as li_user_verify, "
+ " (SELECT COUNT(*) FROM hrm_users_training hut WHERE hut.training_emps_id = hcal.training_emps_id) as count_emps "
 + " , so.organization_name AS department_name, cp.position_name "
+ " from hrm_training_emps hcal "
+ " LEFT JOIN sys_users su ON su.user_id = hcal.created_by "
+ " LEFT JOIN sys_organization so ON su.department_id = so.organization_id "
+ " LEFT JOIN ca_positions cp ON su.position_id = cp.position_id ";




                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                        + "("

                        + " CONTAINS( hcal.training_emps_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') " +

                        " or ( hcal.training_emps_code like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +

                        ")";
                }


                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + "  and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords1 from hrm_training_emps hcal WHERE " + WhereSQL + " and hcal.status=1 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords2 from hrm_training_emps hcal WHERE " + WhereSQL + " and hcal.status=2 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords3 from hrm_training_emps hcal WHERE " + WhereSQL + " and hcal.status=3 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords4 from hrm_training_emps hcal WHERE " + WhereSQL + " and hcal.status=4 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords5 from hrm_training_emps hcal WHERE " + WhereSQL + " and hcal.status=5 and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE  " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords1 from hrm_training_emps hcal WHERE hcal.status=1 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords2 from hrm_training_emps hcal WHERE hcal.status=2 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords3 from hrm_training_emps hcal WHERE hcal.status=3 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords4 from hrm_training_emps hcal WHERE hcal.status=4 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords5 from hrm_training_emps hcal WHERE hcal.status=5 and " + checkOrgz;

                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_ca_SQL/Filter_hrm_ca_academic_level", ip, tid, "Lỗi khi gọi Filter_hrm_ca_academic_level", 0, "hrm_ca_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_ca_SQL/Filter_hrm_ca_academic_level", ip, tid, "Lỗi khi gọi proc Filter_hrm_ca_academic_level", 0, "hrm_ca_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_hrm_candidate([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }


            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = "  select count(*) as totalRecords from hrm_candidate  hcal ";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " hcal.*,hc.campaign_name,su.full_name,su.avatar from hrm_candidate      hcal      LEFT JOIN hrm_campaign hc ON hcal.campaign_id = hc.campaign_id LEFT JOIN sys_users su ON hcal.created_by = su.user_id ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")


                        + " CONTAINS(hcal.candidate_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') ";
                }


                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + "  and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords1 from hrm_candidate  hcal WHERE " + WhereSQL + " and hcal.status=0 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords2 from hrm_candidate  hcal WHERE " + WhereSQL + " and hcal.status=1 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords3 from hrm_candidate  hcal WHERE " + WhereSQL + " and hcal.status=2 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords4 from hrm_candidate  hcal WHERE " + WhereSQL + " and hcal.status=3 and " + checkOrgz;

                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE  " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords1 from hrm_candidate  hcal WHERE hcal.status=0 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords2 from hrm_candidate  hcal WHERE hcal.status=1 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords3 from hrm_candidate  hcal WHERE hcal.status=2 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords4 from hrm_candidate  hcal WHERE hcal.status=3 and " + checkOrgz;


                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_ca_SQL/Filter_hrm_ca_academic_level", ip, tid, "Lỗi khi gọi Filter_hrm_ca_academic_level", 0, "hrm_ca_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_ca_SQL/Filter_hrm_ca_academic_level", ip, tid, "Lỗi khi gọi proc Filter_hrm_ca_academic_level", 0, "hrm_ca_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_hrm_campaign([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }


            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = "  select count(*) as totalRecords from hrm_campaign  hcal ";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " hcal.* ,su.full_name,su.avatar,(SELECT COUNT(*) FROM hrm_candidate hc WHERE hc.campaign_id=hcal.campaign_id) AS slTuyen,hcv.work_position_name, " +
                " (SELECT COUNT(*) FROM hrm_candidate hc WHERE hc.campaign_id = hcal.campaign_id AND hc.status = 1) AS trungTuyen " +
                " from hrm_campaign hcal     LEFT JOIN hrm_ca_work_position hcv ON hcv.work_position_id = hcal.rec_vacancies   LEFT JOIN sys_users su ON hcal.created_by = su.user_id ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")


                        + " ( CONTAINS(hcal.campaign_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') or hcal.campaign_code like N'%" + filterSQL.Search.ToUpper() + "%' )";
                }


                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + "  and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords1 from hrm_campaign  hcal WHERE " + WhereSQL + " and hcal.status=1 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords2 from hrm_campaign  hcal WHERE " + WhereSQL + " and hcal.status=2 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords3 from hrm_campaign  hcal WHERE " + WhereSQL + " and hcal.status=3 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords4 from hrm_campaign  hcal WHERE " + WhereSQL + " and hcal.status=4 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords5 from hrm_campaign  hcal WHERE " + WhereSQL + " and hcal.status=5 and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE  " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords1 from hrm_campaign  hcal WHERE hcal.status=1 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords2 from hrm_campaign  hcal WHERE hcal.status=2 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords3 from hrm_campaign  hcal WHERE hcal.status=3 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords4 from hrm_campaign  hcal WHERE hcal.status=4 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords5 from hrm_campaign  hcal WHERE hcal.status=5 and " + checkOrgz;

                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_hrm_campaign", ip, tid, "Lỗi khi gọi Filter_hrm_campaign", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_hrm_campaign", ip, tid, "Lỗi khi gọi proc Filter_hrm_campaign", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_hrm_rec_calendar([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = "  select count(*) as totalRecords from hrm_rec_calendar  hcal ";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " hcal.*, (SELECT COUNT(*) FROM hrm_rec_candidate hrc WHERE hrc.rec_calendar_id = hcal.rec_calendar_id) AS countUser, su.full_name AS created_name, su.avatar AS created_avatar, " +
" (select distinct '[' + STUFF((SELECT     ',{\"full_name\":\"' + cast(ISNULL(hcs.full_name, '') as nvarchar(50)) + '\"' + ',\"avatar\":\"' + cast(ISNULL(hcs.avatar, '') as nvarchar(50)) + '\"' + '}' " +
" FROM sys_users   hcs  WHERE hcs.user_id IN(SELECT * FROM dbo.udf_PivotParameters(hcal.interviewers, ',') upp) for xml path(''), type " +
" ).value('.', 'nvarchar(max)'), 1, 1, '')  +']'  ) as listUserRecs , hc.campaign_name from hrm_rec_calendar hcal   LEFT JOIN sys_users su ON su.user_id = hcal.created_by " +
" LEFT JOIN hrm_campaign hc ON hcal.campaign_id = hc.campaign_id ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")


                        + " CONTAINS(hcal.rec_calendar_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') ";
                }


                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + "  and " + checkOrgz;

                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE  " + checkOrgz;


                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_hrm_rec_calendar", ip, tid, "Lỗi khi gọi Filter_hrm_rec_calendar", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_hrm_rec_calendar", ip, tid, "Lỗi khi gọi proc Filter_hrm_rec_calendar", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_hrm_proposal([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }


            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = "  select count(*) as totalRecords from hrm_recruitment_proposal  hcal ";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " hcal.* ,su.full_name,su.avatar ,hcv.work_position_name " +
 " from hrm_recruitment_proposal hcal LEFT JOIN hrm_ca_work_position hcv ON hcv.work_position_id = hcal.work_position_id  " +
 " LEFT JOIN sys_users su ON hcal.created_by = su.user_id";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")


                        + " ( CONTAINS(hcal.campaign_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') or hcal.campaign_code like N'%" + filterSQL.Search.ToUpper() + "%' )";
                }


                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + "  and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords1 from hrm_recruitment_proposal  hcal WHERE " + WhereSQL + " and hcal.status=1 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords2 from hrm_recruitment_proposal  hcal WHERE " + WhereSQL + " and hcal.status=2 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords3 from hrm_recruitment_proposal  hcal WHERE " + WhereSQL + " and hcal.status=3 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords4 from hrm_recruitment_proposal  hcal WHERE " + WhereSQL + " and hcal.status=4 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords5 from hrm_recruitment_proposal  hcal WHERE " + WhereSQL + " and hcal.status=5 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords6 from hrm_recruitment_proposal  hcal WHERE " + WhereSQL + " and hcal.status=6 and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE  " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords1 from hrm_recruitment_proposal  hcal WHERE hcal.status=1 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords2 from hrm_recruitment_proposal  hcal WHERE hcal.status=2 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords3 from hrm_recruitment_proposal  hcal WHERE hcal.status=3 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords4 from hrm_recruitment_proposal  hcal WHERE hcal.status=4 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords5 from hrm_recruitment_proposal  hcal WHERE hcal.status=5 and " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords6 from hrm_recruitment_proposal  hcal WHERE hcal.status=6 and " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_hrm_proposal", ip, tid, "Lỗi khi gọi Filter_hrm_proposal", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_hrm_proposal", ip, tid, "Lỗi khi gọi proc Filter_hrm_proposal", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_hrm_reward([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }


            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = "  select count(*) as totalRecords from hrm_reward  hcal ";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";



                sql =

                    " (Select FieldValue into #child from dbo.udf_PivotParameters((Select IDChild from view_sys_organization where organization_id = " + dvid + " ), ',')) " +
"Select tbn.*, cts.contract_id, (o.organization_id)department_id, (o.organization_name)department_name, ps.position_name, wp.work_position_name into #contract " +
" from(Select ct.profile_id, Max(ct.sign_date)sign_date, Max(ct.is_order)maxorder from hrm_contract ct " +

  "  where (ct.organization_id =" + dvid + " or ct.organization_id in (Select FieldValue from #child)) and ct.status = 1	group by ct.profile_id) tbn " +
"inner join hrm_contract cts on cts.profile_id = tbn.profile_id and cts.sign_date = tbn.sign_date and cts.is_order = tbn.maxorder " +
"left join sys_organization o on cts.department_id = o.organization_id " +
"left join ca_positions ps on cts.position_id = ps.position_id " +
"left join hrm_ca_work_position wp on cts.work_position_id = wp.work_position_id";

                sql += " select  hcal.*,su.full_name,su.avatar, " +
              "  CASE WHEN hcal.reward_type = 1 OR hcal.reward_type = 3 THEN(select distinct '[' + STUFF(( " +
                  "   SELECT ',{\"full_name\":\"' + cast(ISNULL(hcs.profile_user_name, '') as nvarchar(150)) + '\"' " +
                   "     + ',\"avatar\":\"' + cast(ISNULL(hcs.avatar, '') as nvarchar(250)) + '\"' " +
                    "     + ',\"profile_id\":\"' + cast(ISNULL(hcs.profile_id, '') as nvarchar(100)) + '\"'  " +
                    "      +',\"profile_code\":\"' + cast(ISNULL(hcs.profile_code, '') as nvarchar(250)) + '\"'" +
                 "  + ',\"position_name\":\"' + cast(ISNULL(sc.position_name, '') as nvarchar(250)) + '\"'" +
                 "  + ',\"department_name\":\"' + cast(ISNULL(sc.department_name, '') as nvarchar(250)) + '\"' + '}' " +
                "    FROM hrm_profile   hcs LEFT JOIN #contract sc ON hcs.profile_id = sc.profile_id  " +
                "  WHERE hcs.profile_id IN(SELECT * FROM dbo.udf_PivotParameters(hcal.reward_name, ',') upp) for xml path(''), type) " +
               "  .value('.', 'nvarchar(max)'), 1, 1, '')  +']'   ) " +

                "  WHEN hcal.reward_type = 2 THEN(select distinct '[' + STUFF((" +
                "    SELECT     ',{\"department_name\":\"' + cast(ISNULL(hcs.organization_name, '') as nvarchar(150)) + '\"' + '}' " +
                "   FROM sys_organization hcs  WHERE hcs.organization_id IN(SELECT * FROM dbo.udf_PivotParameters(hcal.reward_name, ',') upp) for xml path(''), type).value('.', 'nvarchar(max)'), 1, 1, '')  +']'   )  " +
              " END as listRewards  ,  " +
              " CASE WHEN hcal.reward_type = 1 OR hcal.reward_type = 2 THEN  " +
              " (SELECT hcrt1.reward_title_name FROM hrm_ca_reward_title hcrt1 WHERE hcrt1.reward_title_id = hcal.reward_title_id)  " +
              " WHEN hcal.reward_type = 3 THEN  " +
              "  (SELECT hcd.discipline_name FROM hrm_ca_discipline hcd   WHERE hcd.discipline_id = hcal.reward_title_id) END as reward_title_name ,  " +
              " CASE WHEN hcal.reward_type = 1 OR hcal.reward_type = 2 THEN  " +
              " (SELECT hcrl.reward_level_name FROM hrm_ca_reward_level hcrl     WHERE hcrl.reward_level_id = hcal.reward_level_id)  WHEN hcal.reward_type = 3 THEN  " +
               "    (SELECT hcd.discipline_level_name FROM hrm_ca_discipline_level hcd   WHERE hcd.discipline_level_id = hcal.reward_level_id) END as reward_level_name  " +
              " from hrm_reward hcal LEFT JOIN sys_users su ON su.user_id = hcal.created_by  ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                        break;
                                    case "arrIntersec":
                                        WhereSQLR += " ((SELECT COUNT(*) FROM( " +
                                        " SELECT upp.FieldValue AS app  from dbo.udf_PivotParameters('" + m.value + "', ',') upp " +
                                        " INTERSECT " +
                                        " SELECT upp.FieldValue AS app  from dbo.udf_PivotParameters(hcal.reward_name, ',') upp " +
                                        " ) as aas) > 0 ) ";
                                        break;
                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")


                        + " ( CONTAINS(hcal.reward_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') or hcal.reward like N'%" + filterSQL.Search.ToUpper() + "%' )";
                }


                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + "  and " + checkOrgz;

                    sqlCount += " select count(*) as totalRecords from hrm_reward  hcal WHERE " + WhereSQL + " and ( hcal.reward_type=1 or hcal.reward_type=2 ) and " + checkOrgz;


                    sqlCount += " select count(*) as totalRecords from hrm_reward  hcal WHERE " + WhereSQL + " and hcal.reward_type=3    and " + checkOrgz;


                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE  " + checkOrgz;
                    sqlCount += " select count(*) as totalRecords from hrm_reward  hcal WHERE   ( hcal.reward_type=1 or hcal.reward_type=2 ) and " + checkOrgz;

                    sqlCount += " select count(*) as totalRecords from hrm_reward  hcal WHERE hcal.reward_type=3 and " + checkOrgz;

                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_reward", ip, tid, "Lỗi khi gọi Filter_reward", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_reward", ip, tid, "Lỗi khi gọi proc Filter_reward", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }
        [HttpPost]
        public async Task<HttpResponseMessage> Filter_hrm_work_schedule([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }


            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sql1 = "";
            string sqlCount = "  select count(*) as totalRecords from hrm_work_schedule  hcal ";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";



                sql =
                    " SELECT DISTINCT hp.profile_user_name,hp.avatar,hp.profile_id,( SELECT top(1)  ps.position_name from hrm_contract ct " +
" left join ca_positions ps on ct.position_id = ps.position_id AND ct.profile_id = hcal.profile_id " +
" and ct.is_active = 1 and ct.status = 1 ) position_name, " +
" (SELECT   wp.work_position_name from hrm_contract ct left join hrm_ca_work_position wp on ct.work_position_id = wp.work_position_id " +
" where ct.profile_id = hcal.profile_id and ct.is_active = 1 and ct.status = 1 )work_position_name, " +
" (SELECT   so.organization_name from hrm_contract ct left join sys_organization so   on ct.department_id = so.organization_id " +
"     where ct.profile_id = hcal.profile_id  and ct.is_active = 1 and ct.status = 1 )department_name, " +
" (select distinct '[' + STUFF((SELECT     ',{\"work_schedule_id\":\"' + cast(ISNULL(hcs.work_schedule_id, '') as nvarchar(50)) + '\"' " +
" + ',\"declare_shift_name\":\"' + cast(ISNULL(hds.declare_shift_name, '') as nvarchar(50)) + '\"' " +
" + ',\"config_work_location_name\":\"' + cast(ISNULL(hcwl.config_work_location_name, '') as nvarchar(200)) + '\"' " +
" + ',\"work_schedule_months\":\"' + cast(ISNULL((hcs.work_schedule_months), '') as nvarchar(max)) + '\"' " +
" + ',\"work_schedule_days\":\"' + cast(ISNULL((hcs.work_schedule_days), '') as nvarchar(max)) + '\"' " +
" + ',\"is_full_time\":\"' + cast(ISNULL(hcs.is_full_time, '') as nvarchar(50)) + '\"' " +
" + ',\"status\":\"' + cast(ISNULL(hcs.status, '') as nvarchar(50)) + '\"' + '}' " +
" FROM hrm_work_schedule   hcs  LEFT JOIN  hrm_declare_shift hds ON hcs.declare_shift_id = hds.declare_shift_id " +
" LEFT JOIN hrm_config_work_location hcwl ON hcs.config_work_location_id = hcwl.config_work_location_id ";

                sql1 +=
" for xml path(''), type).value('.', 'nvarchar(max)'), 1, 1, '') +']') as datalists " +
" from hrm_work_schedule hcal " +
" LEFT JOIN hrm_profile hp ON hp.profile_id = hcal.profile_id ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = " ";
                string WhereSQLG = "";
                string checkOrgz = super == "True" ? " (   hcs.organization_id =" + dvid + " OR  hcs.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" (   hcs.organization_id =" + dvid + " ) ");
                string checkOrgzG = super == "True" ? " (   hcal.organization_id =" + dvid + " OR  hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" (   hcal.organization_id =" + dvid + " ) ");


                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        string WhereSQLRG = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                            WhereSQLRG += (WhereSQLRG != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";

                        }
                        else
                        {
                            WhereSQLRG += field.filterconstraints.Count > 1 ? "(" : "";
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " (  hcs." + field.key + " = N'" + m.value + "')";
                                        WhereSQLRG += " " + field.filteroperator + " (  hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " (   hcs." + field.key + "  <> N'" + m.value + "')";
                                        WhereSQLRG += " " + field.filteroperator + " (  hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                        break;
                                    case "arrIntersec":
                                        WhereSQLR += " ((SELECT COUNT(*) FROM( " +
                                        " SELECT upp.FieldValue AS app  from dbo.udf_PivotParameters('" + m.value + "', ',') upp " +
                                        " INTERSECT " +
                                        " SELECT upp.FieldValue AS app  from dbo.udf_PivotParameters(hcs.profile_id, ',') upp " +
                                        " ) as aas) > 0 ) ";
                                        WhereSQLRG += " ((SELECT COUNT(*) FROM( " +
                                     " SELECT upp.FieldValue AS app  from dbo.udf_PivotParameters('" + m.value + "', ',') upp " +
                                     " INTERSECT " +
                                     " SELECT upp.FieldValue AS app  from dbo.udf_PivotParameters(hcal.profile_id, ',') upp " +
                                     " ) as aasq) > 0 ) ";
                                        break;
                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                            WhereSQLRG += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;


                        if (WhereSQLRG.StartsWith("( and"))
                        {

                            WhereSQLRG = "( " + WhereSQLRG.Substring(5);
                        }
                        else if (WhereSQLRG.StartsWith("( or"))
                        {
                            WhereSQLRG = "( " + WhereSQLRG.Substring(4);
                        }
                        if (WhereSQLRG.StartsWith(" and"))
                        {

                            WhereSQLRG = WhereSQLRG.Substring(4);
                        }
                        else if (WhereSQLRG.StartsWith(" or"))
                        {
                            WhereSQLRG = WhereSQLRG.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLRG = "  and  " + WhereSQLRG;
                        }
                        WhereSQLG += WhereSQLRG;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQLG = (WhereSQLG.Trim() != "" ? (WhereSQLG + " And  ") : "")
                        + " ( CONTAINS( hcal.profile_user_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') ";
                }


                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE hcs.profile_id = hcal.profile_id  and (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY hcs.is_order";
                    sql1 += " WHERE (" + WhereSQLG + ") and " + checkOrgzG + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQLG + "  and " + checkOrgzG;
                    sql += sql1;



                }
                else
                {
                    sql += " WHERE  hcs.profile_id = hcal.profile_id and " + checkOrgz + @"
                        ORDER BY hcs.is_order ";
                    sql1 += " WHERE " + checkOrgzG + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE  " + checkOrgzG;
                    sql += sql1;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_work_schedule", ip, tid, "Lỗi khi gọi Filter_work_schedule", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_work_schedule", ip, tid, "Lỗi khi gọi proc Filter_work_schedule", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_hrm_holiday_dates([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(holiday_dates_id)  as totalRecords from hrm_ca_holiday_dates hcal";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " hcal.* ,hcht.holiday_type_name from hrm_holiday_dates   hcal LEFT JOIN hrm_ca_holiday_type hcht ON hcal.holiday_type_id = hcht.holiday_type_id ";


                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }
                        if (check == true)
                        {
                            WhereSQLR = field.filteroperator + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                        + "("
                        + " hcal.reason like N'%" + filterSQL.Search + "%'" +

                        ")";
                }


                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + " and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_ca_SQL/Filter_hrm_ca_holiday_dates", ip, tid, "Lỗi khi gọi Filter_hrm_ca_holiday_dates", 0, "hrm_ca_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_ca_SQL/Filter_hrm_ca_holiday_dates", ip, tid, "Lỗi khi gọi proc Filter_hrm_ca_holiday_dates", 0, "hrm_ca_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_hrm_declare_paycheck([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }


            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = "  select count(*) as totalRecords from hrm_declare_paycheck  hcal ";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";



                sql =

                    " (Select FieldValue into #child from dbo.udf_PivotParameters((Select IDChild from view_sys_organization where organization_id = " + dvid + " ), ',')) " +
"Select tbn.*,hp.profile_user_name,hp.avatar,hp.profile_code, cts.contract_id, (o.organization_id)department_id, (o.organization_name)department_name, ps.position_name, wp.work_position_name into #contract " +
" from(Select ct.profile_id, Max(ct.sign_date)sign_date, Max(ct.is_order)maxorder from hrm_contract ct " +

  "  where (ct.organization_id =" + dvid + " or ct.organization_id in (Select FieldValue from #child)) and ct.status = 1	group by ct.profile_id) tbn " +
"inner join hrm_contract cts on cts.profile_id = tbn.profile_id and cts.sign_date = tbn.sign_date and cts.is_order = tbn.maxorder " +
"left join sys_organization o on cts.department_id = o.organization_id " +
"left join ca_positions ps on cts.position_id = ps.position_id " +
"left join hrm_ca_work_position wp on cts.work_position_id = wp.work_position_id" +
" LEFT JOIN hrm_profile hp ON cts.profile_id = hp.profile_id ";




                sql +=
                 "   SELECT hcal.*,(SELECT DISTINCT '[' + STUFF((SELECT " +
            "    ',{\"full_name\":\"' + CAST(ISNULL(hcs.profile_user_name, '') AS NVARCHAR(150)) + '\"' " +
           "     + ',\"avatar\":\"' + CAST(ISNULL(hcs.avatar, '') AS NVARCHAR(250)) + '\"' " +
            "    + ',\"profile_id\":\"' + CAST(ISNULL(hcs.profile_id, '') AS NVARCHAR(250)) + '\"' " +
           "     + ',\"profile_code\":\"' + CAST(ISNULL(hcs.profile_code, '') AS NVARCHAR(250)) + '\"' " +
           "     + ',\"position_name\":\"' + CAST(ISNULL(hcs.position_name, '') AS NVARCHAR(250)) + '\"' " +
           "     + ',\"department_name\":\"' + CAST(ISNULL(hcs.department_name, '') AS NVARCHAR(250)) + '\"' " +
           "     + '}' FROM #contract hcs    WHERE hcs.profile_id IN (SELECT *  FROM dbo.udf_PivotParameters(hcal.list_profile_id, ',') upp) " +
          "    FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '') +']') AS listUsers " +
    "  FROM hrm_declare_paycheck hcal LEFT JOIN sys_users su ON su.user_id = hcal.created_by ";

                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                        break;
                                    case "arrIntersec":
                                        WhereSQLR += " ((SELECT COUNT(*) FROM( " +
                                        " SELECT upp.FieldValue AS app  from dbo.udf_PivotParameters('" + m.value + "', ',') upp " +
                                        " INTERSECT " +
                                        " SELECT upp.FieldValue AS app  from dbo.udf_PivotParameters(hcal.list_profile_id, ',') upp " +
                                        " ) as aas) > 0 ) ";
                                        break;
                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")


                        + " ( CONTAINS(hcal.declare_paycheck_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') )";
                }
                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + "  and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE  " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_declare_paycheck", ip, tid, "Lỗi khi gọi Filter_declare_paycheck", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_declare_paycheck", ip, tid, "Lỗi khi gọi proc Filter_declare_paycheck", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_hrm_payroll([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }


            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = "  select count(*) as totalRecords from hrm_payroll  hcal ";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";



                sql =

                    " (Select FieldValue into #child from dbo.udf_PivotParameters((Select IDChild from view_sys_organization where organization_id = " + dvid + " ), ',')) " +
"Select tbn.*,hp.profile_user_name,hp.avatar,hp.profile_code, cts.contract_id, (o.organization_id)department_id, (o.organization_name)department_name, ps.position_name, wp.work_position_name into #contract " +
" from(Select ct.profile_id, Max(ct.sign_date)sign_date, Max(ct.is_order)maxorder from hrm_contract ct " +

  "  where (ct.organization_id =" + dvid + " or ct.organization_id in (Select FieldValue from #child)) and ct.status = 1	group by ct.profile_id) tbn " +
"inner join hrm_contract cts on cts.profile_id = tbn.profile_id and cts.sign_date = tbn.sign_date and cts.is_order = tbn.maxorder " +
"left join sys_organization o on cts.department_id = o.organization_id " +
"left join ca_positions ps on cts.position_id = ps.position_id " +
"left join hrm_ca_work_position wp on cts.work_position_id = wp.work_position_id" +
" LEFT JOIN hrm_profile hp ON cts.profile_id = hp.profile_id ";




                sql +=
                 "   SELECT hcal.*,(SELECT DISTINCT '[' + STUFF((SELECT " +
            "    ',{\"full_name\":\"' + CAST(ISNULL(hcs.profile_user_name, '') AS NVARCHAR(150)) + '\"' " +
           "     + ',\"avatar\":\"' + CAST(ISNULL(hcs.avatar, '') AS NVARCHAR(250)) + '\"' " +
            "    + ',\"profile_id\":\"' + CAST(ISNULL(hcs.profile_id, '') AS NVARCHAR(250)) + '\"' " +
           "     + ',\"profile_code\":\"' + CAST(ISNULL(hcs.profile_code, '') AS NVARCHAR(250)) + '\"' " +
           "     + ',\"position_name\":\"' + CAST(ISNULL(hcs.position_name, '') AS NVARCHAR(250)) + '\"' " +
           "     + ',\"department_name\":\"' + CAST(ISNULL(hcs.department_name, '') AS NVARCHAR(250)) + '\"' " +
           "     + '}' FROM #contract hcs    WHERE hcs.profile_id IN (SELECT *  FROM dbo.udf_PivotParameters(hcal.list_profile_id, ',') upp) " +
          "    FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '') +']') AS listUsers " +
    "  FROM hrm_payroll hcal LEFT JOIN sys_users su ON su.user_id = hcal.created_by ";

                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                        break;
                                    case "arrIntersec":
                                        WhereSQLR += " ((SELECT COUNT(*) FROM( " +
                                        " SELECT upp.FieldValue AS app  from dbo.udf_PivotParameters('" + m.value + "', ',') upp " +
                                        " INTERSECT " +
                                        " SELECT upp.FieldValue AS app  from dbo.udf_PivotParameters(hcal.list_profile_id, ',') upp " +
                                        " ) as aas) > 0 ) ";
                                        break;
                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")


                        + " ( CONTAINS(hcal.payroll_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') )";
                }
                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + "  and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE  " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_payroll", ip, tid, "Lỗi khi gọi Filter_payroll", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_payroll", ip, tid, "Lỗi khi gọi proc Filter_payroll", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_smart_report([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }


            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = "  select count(*) as totalRecords from smart_report  hcal ";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " hcal.*   " +
 " from smart_report hcal   "  ;
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")


                        + " ( CONTAINS(hcal.report_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') or hcal.report_name like N'%" + filterSQL.Search.ToUpper() + "%' )";
                }


                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + "  and " + checkOrgz;
             }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE  " + checkOrgz;
             }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_smart_report", ip, tid, "Lỗi khi gọi Filter_smart_report", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_smart_report", ip, tid, "Lỗi khi gọi proc Filter_smart_report", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_smart_proc([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }


            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = "  select count(*) as totalRecords from smart_proc  hcal ";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " hcal.*   " +
 " from smart_proc hcal   ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " (  hcal.organization_id =" + dvid + " OR hcal.organization_id IN (SELECT *    FROM dbo.udf_getChildOrg(" + dvid + ") uco)  ) " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQLR += (WhereSQLR != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQLR += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQLR += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQLR += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( hcal." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) <= CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQLR += " " + field.filteroperator + " CAST(" + field.key + " as date) >= CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQLR += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                        if (WhereSQLR.StartsWith("( and"))
                        {

                            WhereSQLR = "( " + WhereSQLR.Substring(5);
                        }
                        else if (WhereSQLR.StartsWith("( or"))
                        {
                            WhereSQLR = "( " + WhereSQLR.Substring(4);
                        }
                        if (WhereSQLR.StartsWith(" and"))
                        {

                            WhereSQLR = WhereSQLR.Substring(4);
                        }
                        else if (WhereSQLR.StartsWith(" or"))
                        {
                            WhereSQLR = WhereSQLR.Substring(3);
                        }

                        if (check == true)
                        {
                            WhereSQLR = "  and  " + WhereSQLR;
                        }
                        WhereSQL += WhereSQLR;
                        check = true;
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")


                        + " ( CONTAINS(hcal.proc_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') or hcal.proc_name like N'%" + filterSQL.Search.ToUpper() + "%' )";
                }


                var offSetSQL = "";
                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }

                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE (" + WhereSQL + ") and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + "  and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE  " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }



                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });


            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_smart_proc", ip, tid, "Lỗi khi gọi Filter_smart_proc", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/hrm_SQL/Filter_smart_proc", ip, tid, "Lỗi khi gọi proc Filter_smart_proc", 0, "hrm_SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }

        }
    }
}
