using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
                sql = selectStr + "  hcal.*, (select distinct '[' + STUFF(("
  + "    SELECT     ',{\"user_id\":\"' + cast(ISNULL(hcs.lecturers_id, '') as nvarchar(50)) + '\"'"
   + "      + ',\"full_name\":\"' + cast(ISNULL(hcs.lecturers_name, '') as nvarchar(50)) + '\"'"
  + "   + ',\"avatar\":\"' + cast(ISNULL(hcs.avatar, '') as nvarchar(200)) + '\"'"
 + "    + ',\"phone_number\":\"' + cast(ISNULL((hcs.phone_number), '') as nvarchar(500)) + '\"'"
  + "     + '}'"
 + "    FROM hrm_class_schedule hcs WHERE hcs.training_emps_id = hcal.training_emps_id"
 + "     for xml path(''), type"
+ "  ).value('.', 'nvarchar(max)'), 1, 1, '') +']') as li_user_verify,"
          + "   (SELECT COUNT(*) FROM hrm_users_training hut WHERE hut.training_emps_id = hcal.training_emps_id)as count_emps"
           + "   from hrm_training_emps hcal ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " hcal.organization_id is not null " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
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
                sql = selectStr + " hcal.*,hc.campaign_name from hrm_candidate      hcal      LEFT JOIN hrm_campaign hc ON hcal.campaign_id = hc.campaign_id" ;
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " hcal.organization_id is not null " : (" ( hcal.organization_id = 0 or hcal.organization_id =" + dvid + " ) ");
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
                     

                        + " CONTAINS(hcal.candidate_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') " ;
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
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords1 from hrm_candidate  hcal WHERE hcal.status=0 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords2 from hrm_candidate  hcal WHERE hcal.status=1 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords3 from hrm_candidate  hcal WHERE hcal.status=2 and " + checkOrgz;
                    sqlCount += " select count(hcal.training_emps_id) as totalRecords4 from hrm_candidate  hcal WHERE hcal.status=3 and " + checkOrgz;
              

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
    }
}
